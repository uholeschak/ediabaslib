#if DEBUG && ANDROID
#define DEBUG_ANDROID
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EdiabasLib
{
    public class EdCustomWiFiInterface
    {
#if ANDROID
        public class ConnectParameterType
        {
            public ConnectParameterType(TcpClientWithTimeout.NetworkData networkData, Android.Net.Wifi.WifiManager wifiManager)
            {
                NetworkData = networkData;
                WifiManager = wifiManager;
            }

            public TcpClientWithTimeout.NetworkData NetworkData { get; }
            public Android.Net.Wifi.WifiManager WifiManager { get; }
        }
#endif

#if DEBUG_ANDROID
        private static readonly string Tag = typeof(EdCustomWiFiInterface).FullName;
#endif
        public const string PortId = "DEEPOBDWIFI";
        public const string RawTag = "RAW";
        public static string AdapterIp = "192.168.0.10";
        public static string AdapterIpEspLink = "192.168.4.1";
        public static int AdapterPort = 35000;
        public static int AdapterPortEspLink = 23;
        protected const int TcpReadTimeoutOffset = 1000;
        protected const int EchoTimeout = 1000;
        protected static int ConnectTimeout = 5000;
        private static readonly EdCustomAdapterCommon CustomAdapter =
            new EdCustomAdapterCommon(SendData, ReceiveData, DiscardInBuffer, ReadInBuffer, TcpReadTimeoutOffset, -1, EchoTimeout, true);
        // ReSharper disable once UnusedMember.Global
        protected static Stopwatch StopWatch = new Stopwatch();
        protected static TcpClient TcpClient;
        protected static NetworkStream TcpStream;
        protected static EscapeStreamWriter WriteStream;
        protected static readonly ManualResetEvent TransmitCancelEvent = new ManualResetEvent(false);
        protected static string ConnectPort;
        protected static object ConnectParameter;
        protected static object NetworkData;
        protected static object WifiManager;
        protected static bool WriteEscapeRequired;

        public static Stream NetworkReadStream => TcpStream;
        public static Stream NetworkWriteStream => WriteStream;

        public static EdiabasNet Ediabas
        {
            get => CustomAdapter.Ediabas;
            set => CustomAdapter.Ediabas = value;
        }

        static EdCustomWiFiInterface()
        {
        }

        public static bool InterfaceConnect(string port, object parameter)
        {
            return InterfaceConnect(port, parameter, false);
        }

        public static bool InterfaceConnect(string port, object parameter, bool reconnect)
        {
            if (TcpClient != null)
            {
                if (ConnectPort == port)
                {
                    return true;
                }
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Wifi port {0} different, disconnect", port);
                InterfaceDisconnect(true);
                return true;
            }

            if (!port.StartsWith(PortId, StringComparison.OrdinalIgnoreCase))
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Invalid port id: {0}", port);
                InterfaceDisconnect(true);
                return false;
            }

            CustomAdapter.Init();
            try
            {
                TransmitCancelEvent.Reset();
                ConnectPort = port;
                ConnectParameter = parameter;
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "WiFi connect: {0}", port);
                string adapterIp = AdapterIp;
                int adapterPort = AdapterPort;
                NetworkData = null;
                WifiManager = null;
                if (!reconnect)
                {
                    WriteEscapeRequired = false;
                }
#if ANDROID
                if (ConnectParameter is ConnectParameterType connectParameter)
                {
                    NetworkData = connectParameter.NetworkData;
                    WifiManager = connectParameter.WifiManager;
                }

                if (IsWifiApMode())
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "AP mode, using default ESP-Link port");
                    adapterPort = AdapterPortEspLink;
                }
#endif
                bool ipSpecified = false;
                string portData = port.Remove(0, PortId.Length);
                if (portData.Length > 0 && portData[0] == ':')
                {
                    // special ip
                    string addr = portData.Remove(0, 1);
                    string[] stringList = addr.Split(':');
                    int listLength = stringList.Length;
                    if (listLength == 0)
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Missing port parameters: {0}", port);
                        InterfaceDisconnect();
                        return false;
                    }

                    if (string.Compare(stringList[listLength - 1], RawTag, StringComparison.Ordinal) == 0)
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Raw mode enabled");
                        CustomAdapter.RawMode = true;
                        listLength--;
                    }

                    if (listLength > 0)
                    {
                        ipSpecified = true;
                        adapterIp = stringList[0].Trim();
                        if (string.Compare(adapterIp, AdapterIpEspLink, StringComparison.Ordinal) == 0)
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Using ESP-Link port");
                            adapterPort = AdapterPortEspLink;
                        }

                        if (listLength > 1)
                        {
                            if (!int.TryParse(stringList[1].Trim(), out adapterPort))
                            {
                                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting: Invalid port parameters: {0}", port);
                                InterfaceDisconnect();
                                return false;
                            }
                        }
                    }
                }
#if ANDROID
                if (!ipSpecified && WifiManager is Android.Net.Wifi.WifiManager wifiManager)
                {
                    string serverIp = null;
                    if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.S)
                    {
#pragma warning disable 618
#pragma warning disable CA1422
                        if (wifiManager.ConnectionInfo != null && wifiManager.DhcpInfo != null)
                        {
                            serverIp = TcpClientWithTimeout.ConvertIpAddress(wifiManager.DhcpInfo.ServerAddress);
                        }
#pragma warning restore CA1422
#pragma warning restore 618
                    }
                    else
                    {
                        if (NetworkData is TcpClientWithTimeout.NetworkData networkData)
                        {
                            Android.Net.ConnectivityManager connectivityManager = networkData.ConnectivityManager;
                            lock (networkData.LockObject)
                            {
                                foreach (Android.Net.Network network in networkData.ActiveWifiNetworks)
                                {
                                    Android.Net.NetworkCapabilities networkCapabilities = connectivityManager.GetNetworkCapabilities(network);
                                    Android.Net.LinkProperties linkProperties = connectivityManager.GetLinkProperties(network);
#pragma warning disable CA1416
                                    if (networkCapabilities != null && linkProperties != null && linkProperties.DhcpServerAddress != null)
                                    {
                                        if (networkCapabilities.TransportInfo is Android.Net.Wifi.WifiInfo)
                                        {
                                            string serverAddress = TcpClientWithTimeout.ConvertIpAddress(linkProperties.DhcpServerAddress);
                                            if (!string.IsNullOrEmpty(serverAddress))
                                            {
                                                serverIp = serverAddress;
                                                break;
                                            }
                                        }
                                    }
#pragma warning restore CA1416
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(serverIp))
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DHCP server IP: {0}", serverIp);
                        if (string.Compare(serverIp, AdapterIpEspLink, StringComparison.Ordinal) == 0)
                        {
                            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ESP-Link detected");
                            adapterIp = AdapterIpEspLink;
                            adapterPort = AdapterPortEspLink;
                        }
                    }
                }
#else
                if (!ipSpecified)
                {
                    System.Net.NetworkInformation.NetworkInterface[] adapters = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                    foreach (System.Net.NetworkInformation.NetworkInterface adapter in adapters)
                    {
                        if (adapter.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                        {
                            if (adapter.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
                            {
                                System.Net.NetworkInformation.IPInterfaceProperties properties = adapter.GetIPProperties();
                                if (properties?.DhcpServerAddresses != null)
                                {
                                    foreach (IPAddress dhcpServerAddress in properties.DhcpServerAddresses)
                                    {
                                        if (dhcpServerAddress.AddressFamily == AddressFamily.InterNetwork)
                                        {
                                            string serverIp = dhcpServerAddress.ToString();
                                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DHCP server IP: {0}", serverIp);
                                            if (string.Compare(serverIp, AdapterIpEspLink, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "ESP-Link detected");
                                                adapterIp = AdapterIpEspLink;
                                                adapterPort = AdapterPortEspLink;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
#endif
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting to: {0}:{1}", adapterIp, adapterPort);
                IPAddress hostIpAddress = IPAddress.Parse(adapterIp);
                TcpClientWithTimeout.ExecuteNetworkCommand((string bindIpAddress, string bindBroadcastIpAddress) =>
                {
                    TcpClient = new TcpClientWithTimeout(hostIpAddress, adapterPort, ConnectTimeout, true).Connect(TransmitCancelEvent);
                }, hostIpAddress, NetworkData);
                TcpStream = TcpClient.GetStream();
                WriteStream = new EscapeStreamWriter(TcpStream);

                if (!CustomAdapter.RawMode)
                {
                    if (!reconnect)
                    {
                        if (!UpdateWriteEscapeRequired())
                        {
                            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Update write escape failed");
                            InterfaceDisconnect(true);
                            return false;
                        }
                    }

                    CustomAdapter.EscapeModeWrite = WriteEscapeRequired;
                    if (!CustomAdapter.UpdateAdapterInfo(true))
                    {
                        Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Update adapter info failed");
                        InterfaceDisconnect(true);
                        return false;
                    }

#if DEBUG_ANDROID
                    Android.Util.Log.Info(Tag, string.Format("InterfaceConnect WriteEscape={0}", CustomAdapter.EscapeModeWrite));
#endif
                    WriteStream.SetEscapeMode(CustomAdapter.EscapeModeWrite);
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Connect failure: {0}", EdiabasNet.GetExceptionText(ex));
                InterfaceDisconnect(true);
                return false;
            }
            return true;
        }

        public static bool InterfaceDisconnect()
        {
            return InterfaceDisconnect(false);
        }

        public static bool InterfaceDisconnect(bool forceClose)
        {
            if (!forceClose && Ediabas != null && !Ediabas.Unloading)
            {
                int keepConnectionOpen = 0;
                string prop = Ediabas.GetConfigProperty("ObdKeepConnectionOpen");
                if (prop != null)
                {
                    keepConnectionOpen = (int)EdiabasNet.StringToValue(prop);
                }

                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ObdKeepConnectionOpen: {0}", keepConnectionOpen);
                if (keepConnectionOpen != 0)
                {
                    return true;
                }
            }

            Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "WiFi disconnect");
            bool result = true;
            try
            {
                if (WriteStream != null)
                {
                    WriteStream.Dispose();
                    WriteStream = null;
                }

                if (TcpStream != null)
                {
                    TcpStream.Dispose();
                    TcpStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (TcpClient != null)
                {
                    TcpClient.Dispose();
                    TcpClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static bool InterfaceTransmitCancel(bool cancel)
        {
            if (cancel)
            {
                TransmitCancelEvent.Set();
            }
            else
            {
                TransmitCancelEvent.Reset();
            }
            return true;
        }

        public static EdInterfaceObd.InterfaceErrorResult InterfaceSetConfig(EdInterfaceObd.Protocol protocol, int baudRate, int dataBits, EdInterfaceObd.SerialParity parity, bool allowBitBang)
        {
            if (TcpStream == null)
            {
                return EdInterfaceObd.InterfaceErrorResult.ConfigError;
            }

            return CustomAdapter.InterfaceSetConfig(protocol, baudRate, dataBits, parity, allowBitBang);
        }

        public static bool InterfaceSetDtr(bool dtr)
        {
            if (TcpStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetRts(bool rts)
        {
            if (TcpStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceGetDsr(out bool dsr)
        {
            dsr = true;
            if (TcpStream == null)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceSetBreak(bool enable)
        {
            return false;
        }

        public static bool InterfaceSetInterByteTime(int time)
        {
            return CustomAdapter.InterfaceSetInterByteTime(time);
        }

        public static bool InterfaceSetCanIds(int canTxId, int canRxId, EdInterfaceObd.CanFlags canFlags)
        {
            return CustomAdapter.InterfaceSetCanIds(canTxId, canRxId, canFlags);
        }

        public static bool InterfacePurgeInBuffer()
        {
            if (TcpStream == null)
            {
                return false;
            }
            try
            {
                DiscardInBuffer();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool InterfaceAdapterEcho()
        {
            return false;
        }

        public static bool InterfaceHasPreciseTimeout()
        {
            return false;
        }

        public static bool InterfaceHasAutoBaudRate()
        {
            return true;
        }

        public static bool InterfaceHasAutoKwp1281()
        {
            return CustomAdapter.InterfaceHasAutoKwp1281();
        }

        public static int? InterfaceAdapterVersion()
        {
            return CustomAdapter.InterfaceAdapterVersion();
        }

        public static byte[] InterfaceAdapterSerial()
        {
            return CustomAdapter.InterfaceAdapterSerial();
        }

        public static double? InterfaceAdapterVoltage()
        {
            return CustomAdapter.InterfaceAdapterVoltage();
        }

        public static bool InterfaceHasIgnitionStatus()
        {
            return true;
        }

        public static bool InterfaceSendData(byte[] sendData, int length, bool setDtr, double dtrTimeCorr)
        {
            if (TcpStream == null)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Port closed");
                return false;
            }

            for (int retry = 0; retry < 2; retry++)
            {
                if (CustomAdapter.ReconnectRequired)
                {
                    Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(ConnectPort, ConnectParameter, true))
                    {
                        CustomAdapter.ReconnectRequired = true;
                        return false;
                    }
                    CustomAdapter.ReconnectRequired = false;
                }

                if (CustomAdapter.InterfaceSendData(sendData, length, setDtr, dtrTimeCorr))
                {
                    return true;
                }

                if (!CustomAdapter.ReconnectRequired)
                {
                    return false;
                }
            }

            return false;
        }

        public static bool InterfaceReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog)
        {
            if (TcpStream == null)
            {
                return false;
            }

            return CustomAdapter.InterfaceReceiveData(receiveData, offset, length, timeout, timeoutTelEnd, ediabasLog);
        }

        public static bool InterfaceSendPulse(UInt64 dataBits, int length, int pulseWidth, bool setDtr, bool bothLines, int autoKeyByteDelay)
        {
            if (TcpStream == null)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Port closed");
                return false;
            }
            if (CustomAdapter.ReconnectRequired)
            {
                Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnecting");
                InterfaceDisconnect(true);
                if (!InterfaceConnect(ConnectPort, ConnectParameter, true))
                {
                    CustomAdapter.ReconnectRequired = true;
                    return false;
                }
                CustomAdapter.ReconnectRequired = false;
            }

            return CustomAdapter.InterfaceSendPulse(dataBits, length, pulseWidth, setDtr, bothLines, autoKeyByteDelay);
        }

        private static void SendData(byte[] buffer, int length)
        {
#if DEBUG_ANDROID
            List<byte> sendList = buffer.ToList().GetRange(0, length);
            Android.Util.Log.Info(Tag, string.Format("Send: {0}", BitConverter.ToString(sendList.ToArray()).Replace("-", " ")));
#endif
            if (WriteStream != null)
            {
                if (WriteStream.EscapeMode != CustomAdapter.EscapeModeWrite)
                {
#if DEBUG_ANDROID
                    Android.Util.Log.Info(Tag, string.Format("SendData Update WriteEscape={0}", CustomAdapter.EscapeModeWrite));
#endif
                    WriteStream.SetEscapeMode(CustomAdapter.EscapeModeWrite);
                }
                WriteStream.Write(buffer, 0, length);
            }
        }

        private static bool ReceiveData(byte[] buffer, int offset, int length, int timeout, int timeoutTelEnd, EdiabasNet ediabasLog = null)
        {
            int recLen = 0;
            int data;
            try
            {
                data = TcpStream.ReadByteAsync(TransmitCancelEvent, timeout);
            }
            catch (Exception)
            {
                data = -1;
            }
            if (data < 0)
            {
                return false;
            }
            buffer[offset + recLen] = (byte)data;
            recLen++;

            if (length > 1)
            {
                try
                {
                    int byteCount = TcpStream.ReadBytesAsync(buffer, offset + recLen, length - recLen, TransmitCancelEvent, timeoutTelEnd);
                    if (byteCount < 0)
                    {
                        ediabasLog?.LogData(EdiabasNet.EdLogLevel.Ifh, buffer, offset, recLen, "Rec ");
                        return false;
                    }

                    recLen += byteCount;
                }
                catch (Exception)
                {
                    return false;
                }
            }

#if DEBUG_ANDROID
            List<byte> recList = buffer.ToList().GetRange(offset, recLen);
            Android.Util.Log.Info(Tag, string.Format("Rec: {0}", BitConverter.ToString(recList.ToArray()).Replace("-", " ")));
#endif
            if (recLen < length)
            {
#if DEBUG_ANDROID
                Android.Util.Log.Info(Tag, string.Format("Rec len={0}, expected={1}", recLen, length));
#endif
                ediabasLog?.LogData(EdiabasNet.EdLogLevel.Ifh, buffer, offset, recLen, "Rec ");
                return false;
            }
            return true;
        }

        private static void DiscardInBuffer()
        {
            TcpStream.Flush();
            while (TcpStream.DataAvailable)
            {
                if (TcpStream.ReadByteAsync(TransmitCancelEvent) < 0)
                {
                    break;
                }
            }
        }

        private static List<byte> ReadInBuffer()
        {
            List<byte> responseList = new List<byte>();
            while (TcpStream.DataAvailable)
            {
                int data;
                try
                {
                    data = TcpStream.ReadByteAsync(TransmitCancelEvent);
                }
                catch (Exception)
                {
                    data = -1;
                }

                if (data >= 0)
                {
                    CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();
                    responseList.Add((byte)data);
                }
                else
                {
                    break;
                }
            }
#if DEBUG_ANDROID
            if (responseList.Count > 0)
            {
                Android.Util.Log.Info(Tag, string.Format("Rec: {0}", BitConverter.ToString(responseList.ToArray()).Replace("-", " ")));
            }
#endif
            return responseList;
        }

        private static bool UpdateWriteEscapeRequired()
        {
            Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateWriteEscapeRequired");

            WriteEscapeRequired = false;
            try
            {
                for (int telType = 0; telType < 2; telType++)
                {
                    int respLen = 0;
                    byte[] testTel = null;
                    switch (telType)
                    {
                        case 0:
                            // CAN mode (non zero request)
                            respLen = 6;
                            testTel = new byte[] { 0x82, 0xF1, 0xF1, 0x82, 0x01, 0x00 };
                            break;

                        case 1:
                            // CAN mode (zero request)
                            respLen = 6;
                            testTel = new byte[] { 0x82, 0xF1, 0xF1, 0x82, 0x00, 0x00 };
                            break;
                    }

                    if (testTel == null)
                    {
                        break;
                    }

                    testTel[testTel.Length - 1] = EdCustomAdapterCommon.CalcChecksumBmwFast(testTel, 0, testTel.Length - 1);
                    if (Ediabas != null)
                    {
                        Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, testTel, 0, testTel.Length, "AdSend");
                    }

                    DiscardInBuffer();
                    TcpStream.Write(testTel, 0, testTel.Length);
                    CustomAdapter.LastCommTick = Stopwatch.GetTimestamp();

                    List<byte> responseList = new List<byte>();
                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        List<byte> newList = ReadInBuffer();
                        if (newList.Count > 0)
                        {
                            responseList.AddRange(newList);
                            startTime = Stopwatch.GetTimestamp();
                        }
                        if (responseList.Count >= testTel.Length + respLen)
                        {
                            if (Ediabas != null)
                            {
                                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, responseList.ToArray(), 0, responseList.Count, "AdResp");
                            }

                            bool validEcho = !testTel.Where((t, i) => responseList[i] != t).Any();
                            if (!validEcho)
                            {
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateWriteEscapeRequired Type={0}: Echo invalid", telType);
                                }

                                return false;
                            }

                            if (EdCustomAdapterCommon.CalcChecksumBmwFast(responseList.ToArray(), testTel.Length, respLen - 1) !=
                                responseList[testTel.Length + respLen - 1])
                            {
                                if (Ediabas != null)
                                {
                                    Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateWriteEscapeRequired Type={0}: Checksum invalid", telType);
                                }
                                return false;
                            }
                            break;
                        }
                        if (Stopwatch.GetTimestamp() - startTime > TcpReadTimeoutOffset * EdCustomAdapterCommon.TickResolMs)
                        {
                            if (Ediabas != null)
                            {
                                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, responseList.ToArray(), 0, responseList.Count, "AdResp");
                                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateWriteEscapeRequired Type={0}: Response timeout", telType);
                            }

                            if (telType == 0)
                            {
                                return false;
                            }

                            WriteEscapeRequired = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Stream failure: {0}", EdiabasNet.GetExceptionText(ex));
                CustomAdapter.ReconnectRequired = true;
                return false;
            }

            if (Ediabas != null)
            {
                Ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "UpdateWriteEscapeRequired: {0}", WriteEscapeRequired);
            }

#if DEBUG_ANDROID
            Android.Util.Log.Info(Tag, string.Format("UpdateWriteEscapeRequired WriteEscape={0}", WriteEscapeRequired));
#endif
            return true;
        }

#if ANDROID
        private static bool IsWifiApMode()
        {
            try
            {
                if (!(WifiManager is Android.Net.Wifi.WifiManager wifiManager))
                {
                    return false;
                }

                if (wifiManager.IsWifiEnabled)
                {
                    return false;
                }

                Java.Lang.Reflect.Method methodIsWifiApEnabled = wifiManager.Class.GetDeclaredMethod(@"isWifiApEnabled");
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (methodIsWifiApEnabled != null)
                {
                    methodIsWifiApEnabled.Accessible = true;
                    Java.Lang.Object wifiApEnabledResult = methodIsWifiApEnabled.Invoke(wifiManager);
                    Java.Lang.Boolean wifiApEnabled = Android.Runtime.Extensions.JavaCast<Java.Lang.Boolean>(wifiApEnabledResult);
                    return wifiApEnabled != Java.Lang.Boolean.False;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
#endif
    }
}
