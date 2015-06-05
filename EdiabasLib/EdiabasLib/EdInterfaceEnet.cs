using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace EdiabasLib
{
    public class EdInterfaceEnet : EdInterfaceBase
    {
        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);

        private bool _disposed;
        protected const int TransBufferSize = 512; // transmit buffer size
        protected const string AutoIp = "auto";
        protected static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly byte[] ByteArray0 = new byte[0];
        protected static readonly byte[] UdpIdentReq = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 };
        protected static readonly byte[] TcpControlIgnitReq = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x10 };
        protected static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        protected static IPAddress TcpHostIp;
        protected static TcpClient TcpDiagClient;
        protected static NetworkStream TcpDiagStream;
        protected static AutoResetEvent TcpDiagStreamRecEvent;
        protected static TcpClient TcpControlClient;
        protected static NetworkStream TcpControlStream;
        protected static Timer TcpControlTimer;
        protected static bool TcpControlTimerEnabled;
        protected static object TcpDiagStreamSendLock;
        protected static object TcpDiagStreamRecLock;
        protected static object TcpControlTimerLock;
        protected static byte[] TcpDiagBuffer;
        protected static int TcpDiagRecLen;
        protected static long LastTcpDiagRecTime;
        protected static Queue<byte[]> TcpDiagRecQueue;

        protected Socket UdpSocket;
        protected byte[] UdpBuffer = new byte[0x100];
        protected volatile IPEndPoint UdpRecEndPoint;
        protected AutoResetEvent UdpEvent = new AutoResetEvent(false);

        protected string RemoteHostProtected = AutoIp;
        protected int TesterAddress = 0xF4;
        protected int ControlPort = 6811;
        protected int DiagnosticPort = 6801;
        protected int ConnectTimeout = 5000;

        protected byte[] RecBuffer = new byte[TransBufferSize];
        protected byte[] DataBuffer = new byte[TransBufferSize];
        protected byte[] AckBuffer = new byte[TransBufferSize];

        protected TransmitDelegate ParTransmitFunc;
        protected int ParTimeoutStd;
        protected int ParTimeoutTelEnd;
        protected int ParInterbyteTime;
        protected int ParRegenTime;
        protected int ParTimeoutNr;
        protected int ParRetryNr;

        public override EdiabasNet Ediabas
        {
            get
            {
                return base.Ediabas;
            }
            set
            {
                base.Ediabas = value;

                string prop = EdiabasProtected.GetConfigProperty("EnetRemoteHost");
                if (prop != null)
                {
                    RemoteHostProtected = prop;
                }

                prop = EdiabasProtected.GetConfigProperty("EnetTesterAddress");
                if (prop != null)
                {
                    TesterAddress = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetControlPort");
                if (prop != null)
                {
                    ControlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetDiagnosticPort");
                if (prop != null)
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetConnectTimeout");
                if (prop != null)
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }
            }
        }

        public override UInt32[] CommParameter
        {
            get
            {
                return base.CommParameter;
            }
            set
            {
                CommParameterProtected = value;
                CommAnswerLenProtected[0] = 0;
                CommAnswerLenProtected[1] = 0;

                ParTransmitFunc = null;
                ParTimeoutStd = 0;
                ParTimeoutTelEnd = 0;
                ParInterbyteTime = 0;
                ParRegenTime = 0;
                ParTimeoutNr = 0;
                ParRetryNr = 0;

                if (CommParameterProtected == null)
                {   // clear parameter
                    return;
                }
                if (CommParameterProtected.Length < 1)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    return;
                }

                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, CommParameterProtected, 0, CommParameterProtected.Length,
                    string.Format(Culture, "{0} CommParameter Host={1}, Tester=0x{2:X02}, ControlPort={3}, DiagPort={4}",
                            InterfaceName, RemoteHostProtected, TesterAddress, ControlPort, DiagnosticPort));

                uint concept = CommParameterProtected[0];
                switch (concept)
                {
                    case 0x010F:    // BMW-FAST
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        ParTransmitFunc = TransBmwFast;
                        ParTimeoutStd = (int)CommParameterProtected[2];
                        ParRegenTime = (int)CommParameterProtected[3];
                        ParTimeoutTelEnd = (int)CommParameterProtected[4];
                        ParTimeoutNr = (int)CommParameterProtected[6];
                        ParRetryNr = (int)CommParameterProtected[5];
                        break;

                    case 0x0110:    // D-CAN
                        if (CommParameterProtected.Length < 30)
                        {
                            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                            return;
                        }
                        ParTransmitFunc = TransBmwFast;
                        ParTimeoutStd = (int)CommParameterProtected[7];
                        ParTimeoutTelEnd = 10;
                        ParRegenTime = (int)CommParameterProtected[8];
                        ParTimeoutNr = (int)CommParameterProtected[9];
                        ParRetryNr = (int)CommParameterProtected[10];
                        break;

                    default:
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
                        return;
                }
            }
        }

        public override string InterfaceType
        {
            get
            {
                return "ENET";
            }
        }

        public override UInt32 InterfaceVersion
        {
            get
            {
                return 1795;
            }
        }

        public override string InterfaceName
        {
            get
            {
                return "ENET";
            }
        }

        public override byte[] KeyBytes
        {
            get
            {
                return ByteArray0;
            }
        }

        public override byte[] State
        {
            get
            {
                return ByteArray0;
            }
        }

        public override Int64 BatteryVoltage
        {
            get
            {
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                return 12000;
            }
        }

        public override Int64 IgnitionVoltage
        {
            get
            {
                if (!Connected)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                try
                {
                    lock (TcpControlTimerLock)
                    {
                        TcpControlTimerStop();
                    }
                    if (!TcpControlConnect())
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    TcpControlStream.Write(TcpControlIgnitReq, 0, TcpControlIgnitReq.Length);
                    TcpControlStream.ReadTimeout = 1000;
                    int recLen = TcpControlStream.Read(RecBuffer, 0, 7);
                    if (recLen < 7)
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    if (RecBuffer[5] != 0x10)
                    {   // no clamp state response
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    if ((RecBuffer[6] & 0x0C) == 0x04)
                    {   // ignition on
                        return 12000;
                    }
                }
                catch (Exception)
                {
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                    return Int64.MinValue;
                }
                finally
                {
                    TcpControlTimerStart();
                }
                return 0;
            }
        }

        public override bool Connected
        {
            get
            {
                return (TcpDiagClient != null) && (TcpDiagStream != null);
            }
        }

        static EdInterfaceEnet()
        {
#if WindowsCE || Android
            InterfaceMutex = new Mutex(false);
#else
            InterfaceMutex = new Mutex(false, "EdiabasLib_InterfaceEnet");
#endif
            TcpDiagStreamRecEvent = new AutoResetEvent(false);
            TcpDiagStreamSendLock = new Object();
            TcpDiagStreamRecLock = new Object();
            TcpControlTimer = new Timer(TcpControlTimeout, null, Timeout.Infinite, Timeout.Infinite);
            TcpControlTimerLock = new Object();
            TcpDiagBuffer = new byte[TransBufferSize];
            TcpDiagRecLen = 0;
            LastTcpDiagRecTime = DateTime.MinValue.Ticks;
            TcpDiagRecQueue = new Queue<byte[]>();
        }

        ~EdInterfaceEnet()
        {
            Dispose(false);
        }

        public override bool IsValidInterfaceName(string name)
        {
            return IsValidInterfaceNameStatic(name);
        }

        public static bool IsValidInterfaceNameStatic(string name)
        {
            if (string.Compare(name, "ENET", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        public override bool InterfaceConnect()
        {
            if (TcpDiagClient != null)
            {
                return true;
            }
            try
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Connect");
                TcpHostIp = null;
                if (RemoteHostProtected.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
                {
                    // ReSharper disable once UseObjectOrCollectionInitializer
                    UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if !WindowsCE
                    UdpSocket.EnableBroadcast = true;
#endif
                    IPEndPoint ipUdp = new IPEndPoint(IPAddress.Any, 0);
                    UdpSocket.Bind(ipUdp);
                    UdpRecEndPoint = null;
                    StartUdpListen();

                    bool broadcastSend = false;
#if !WindowsCE
                    string configData = RemoteHostProtected.Remove(0, AutoIp.Length);
                    if ((configData.Length > 0) && (configData[0] == ':'))
                    {
                        string adapterName = configData.StartsWith(":all", StringComparison.OrdinalIgnoreCase) ? string.Empty : configData.Remove(0, 1);

                        System.Net.NetworkInformation.NetworkInterface[] adapters = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                        foreach (System.Net.NetworkInformation.NetworkInterface adapter in adapters)
                        {
                            if (adapter.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                            {
                                System.Net.NetworkInformation.IPInterfaceProperties properties = adapter.GetIPProperties();
                                if (properties.UnicastAddresses != null)
                                {
                                    foreach (System.Net.NetworkInformation.UnicastIPAddressInformation ipAddressInfo in properties.UnicastAddresses)
                                    {
                                        if (ipAddressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                                        {
                                            if ((adapterName.Length == 0) || (adapter.Name.StartsWith(adapterName, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                try
                                                {
                                                    byte[] ipBytes = ipAddressInfo.Address.GetAddressBytes();
                                                    byte[] maskBytes = ipAddressInfo.IPv4Mask.GetAddressBytes();
                                                    for (int i = 0; i < ipBytes.Length; i++)
                                                    {
                                                        ipBytes[i] |= (byte)(~maskBytes[i]);
                                                    }
                                                    IPAddress broadcastAddress = new IPAddress(ipBytes);
                                                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending: '{0}': Ip={1} Mask={2} Broadcast={3}",
                                                        adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress));
                                                    IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, ControlPort);
                                                    UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                                    broadcastSend = true;
                                                }
                                                catch (Exception)
                                                {
                                                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Broadcast failed");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
#endif
                    {
                        try
                        {
#if Android || WindowsCE
                            IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Broadcast, ControlPort);
#else
                            IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Parse("169.254.255.255"), ControlPort);
#endif
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending to: {0}", ipUdpIdent.Address));
                            UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                            broadcastSend = true;
                        }
                        catch (Exception)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Broadcast failed");
                        }
                    }
                    if (!broadcastSend)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "No broadcast send");
                        InterfaceDisconnect();
                        return false;
                    }

                    if (!UdpEvent.WaitOne(500, false) || (UdpRecEndPoint == null))
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "No answer received");
                        InterfaceDisconnect();
                        return false;
                    }
                    TcpHostIp = UdpRecEndPoint.Address;
                    UdpSocket.Close();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Received: IP={0}", TcpHostIp));
                }
                else
                {
                    TcpHostIp = IPAddress.Parse(RemoteHostProtected);
                }

                TcpDiagClient = new TcpClientWithTimeout(TcpHostIp, DiagnosticPort, ConnectTimeout).Connect();
                TcpDiagStream = TcpDiagClient.GetStream();
                TcpDiagRecLen = 0;
                LastTcpDiagRecTime = DateTime.MinValue.Ticks;
                TcpDiagRecQueue.Clear();
                StartReadTcpDiag(6);
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Connected");
            }
            catch (Exception ex)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "InterfaceConnect exception: " + EdiabasNet.GetExceptionText(ex));
                InterfaceDisconnect();
                return false;
            }
            return true;
        }

        public override bool InterfaceDisconnect()
        {
            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Disconnect");
            bool result = true;

            try
            {
                if (TcpDiagStream != null)
                {
                    TcpDiagStream.Close();
                    TcpDiagStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (TcpDiagClient != null)
                {
                    TcpDiagClient.Close();
                    TcpDiagClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            if (!TcpControlDisconnect())
            {
                result = false;
            }

            try
            {
                if (UdpSocket != null)
                {
                    UdpSocket.Close();
                    UdpSocket = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            UdpRecEndPoint = null;
            TcpHostIp = null;
            return result;
        }

        public override bool InterfaceReset()
        {
            CommParameter = null;
            return true;
        }

        public override bool TransmitData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = null;
            if (CommParameterProtected == null)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            int recLength;
            EdiabasNet.ErrorCodes errorCode = ObdTrans(sendData, sendData.Length, ref RecBuffer, out recLength);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected.SetError(errorCode);
                return false;
            }
            receiveData = new byte[recLength];
            Array.Copy(RecBuffer, receiveData, recLength);
            return true;
        }

        public override bool ReceiveFrequent(out byte[] receiveData)
        {
            receiveData = null;
            if (CommParameterProtected == null)
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
                return false;
            }
            receiveData = ByteArray0;
            return true;
        }

        public override bool StopFrequent()
        {
            return true;
        }

        public string RemoteHost
        {
            get
            {
                return RemoteHostProtected;
            }
            set
            {
                RemoteHostProtected = value;
            }
        }

        protected void StartUdpListen()
        {
            Socket udpSocketLocal = UdpSocket;
            if (udpSocketLocal == null)
            {
                return;
            }
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            EndPoint tempRemoteEp = ip;
            udpSocketLocal.BeginReceiveFrom(UdpBuffer, 0, UdpBuffer.Length, SocketFlags.None, ref tempRemoteEp, UdpReceiver, udpSocketLocal);
        }

        protected void UdpReceiver(IAsyncResult ar)
        {
            try
            {
                Socket udpSocketLocal = UdpSocket;
                if (udpSocketLocal == null)
                {
                    return;
                }
                IPEndPoint ipUdp = new IPEndPoint(IPAddress.Any, 0);
                EndPoint tempRemoteEp = ipUdp;
                int recLen = udpSocketLocal.EndReceiveFrom(ar, ref tempRemoteEp);
                if ((UdpRecEndPoint == null) &&
                    (recLen >= (6 + 38)) &&
                    (UdpBuffer[6] == 'D') &&
                    (UdpBuffer[7] == 'I') &&
                    (UdpBuffer[8] == 'A') &&
                    (UdpBuffer[9] == 'G') &&
                    (UdpBuffer[10] == 'A') &&
                    (UdpBuffer[11] == 'D') &&
                    (UdpBuffer[12] == 'R') &&
                    (UdpBuffer[13] == '1') &&
                    (UdpBuffer[14] == '0'))
                {
                    UdpRecEndPoint = (IPEndPoint)tempRemoteEp;
                    UdpEvent.Set();
                }
                else
                {
                    StartUdpListen();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected static void TcpControlTimerStop()
        {
            TcpControlTimerEnabled = false;
            TcpControlTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected static void TcpControlTimerStart()
        {
            TcpControlTimerEnabled = true;
            TcpControlTimer.Change(2000, Timeout.Infinite);
        }

        protected static void TcpControlTimeout(Object stateInfo)
        {
            lock (TcpControlTimerLock)
            {
                if (TcpControlTimerEnabled)
                {
                    TcpControlDisconnect();
                }
            }
        }

        protected bool TcpControlConnect()
        {
            if (TcpControlClient != null)
            {
                return true;
            }
            if (TcpHostIp == null)
            {
                return false;
            }
            try
            {
                lock (TcpControlTimerLock)
                {
                    TcpControlTimerStop();
                }
                TcpControlClient = TcpDiagClient = new TcpClientWithTimeout(TcpHostIp, ControlPort, ConnectTimeout).Connect();
                TcpControlStream = TcpControlClient.GetStream();
            }
            catch (Exception)
            {
                TcpControlDisconnect();
                return false;
            }
            return true;
        }

        protected static bool TcpControlDisconnect()
        {
            bool result = true;
            TcpControlTimerStop();
            try
            {
                if (TcpControlStream != null)
                {
                    TcpControlStream.Close();
                    TcpControlStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (TcpControlClient != null)
                {
                    TcpControlClient.Close();
                    TcpControlClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        protected bool StartReadTcpDiag(int telLength)
        {
            NetworkStream localStream = TcpDiagStream;
            if (localStream == null)
            {
                return false;
            }
            try
            {
                localStream.BeginRead(TcpDiagBuffer, TcpDiagRecLen, telLength - TcpDiagRecLen, TcpDiagReceiver, TcpDiagStream);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected void TcpDiagReceiver(IAsyncResult ar)
        {
            try
            {
                NetworkStream networkStream = TcpDiagStream;
                if (networkStream == null)
                {
                    return;
                }
                if (TcpDiagRecLen > 0)
                {
                    if ((Stopwatch.GetTimestamp() - LastTcpDiagRecTime) > 300 * TickResolMs)
                    {   // pending telegram parts too late
                        TcpDiagRecLen = 0;
                    }
                }
                int recLen = networkStream.EndRead(ar);
                if (recLen > 0)
                {
                    LastTcpDiagRecTime = Stopwatch.GetTimestamp();
                    TcpDiagRecLen += recLen;
                }
                int nextReadLength = 6;
                if (TcpDiagRecLen >= 6)
                {   // header received
                    long telLen = (((long)TcpDiagBuffer[0] << 24) | ((long)TcpDiagBuffer[1] << 16) | ((long)TcpDiagBuffer[2] << 8) | TcpDiagBuffer[3]) + 6;
                    if (TcpDiagRecLen == telLen)
                    {   // telegram received
                        switch (TcpDiagBuffer[5])
                        {
                            case 0x01: // diag data
                            case 0x02: // ack
                                lock (TcpDiagStreamRecLock)
                                {
                                    if (TcpDiagRecQueue.Count > 256)
                                    {
                                        TcpDiagRecQueue.Dequeue();
                                    }
                                    byte[] recTelTemp = new byte[telLen];
                                    Array.Copy(TcpDiagBuffer, recTelTemp, TcpDiagRecLen);
                                    TcpDiagRecQueue.Enqueue(recTelTemp);
                                    TcpDiagStreamRecEvent.Set();
                                }
                                break;

                            case 0x12:  // alive check
                                TcpDiagBuffer[0] = 0x00;
                                TcpDiagBuffer[1] = 0x00;
                                TcpDiagBuffer[2] = 0x00;
                                TcpDiagBuffer[3] = 0x02;
                                TcpDiagBuffer[4] = 0x00;
                                TcpDiagBuffer[5] = 0x13;    // alive check response
                                TcpDiagBuffer[6] = 0x00;
                                TcpDiagBuffer[7] = (byte)TesterAddress;
                                lock (TcpDiagStreamSendLock)
                                {
                                    networkStream.Write(TcpDiagBuffer, 0, 8);
                                }
                                break;
                        }
                        TcpDiagRecLen = 0;
                    }
                    else if (TcpDiagRecLen > telLen)
                    {
                        TcpDiagRecLen = 0;
                    }
                    else if (telLen > TcpDiagBuffer.Length)
                    {   // telegram too large -> remove all
                        while (TcpDiagStream.DataAvailable)
                        {
                            TcpDiagStream.ReadByte();
                        }
                        TcpDiagRecLen = 0;
                    }
                    else
                    {
                        nextReadLength = (int)telLen;
                    }
                }
                StartReadTcpDiag(nextReadLength);
            }
            catch (Exception)
            {
                TcpDiagRecLen = 0;
                StartReadTcpDiag(6);
            }
        }

        protected bool SendData(byte[] sendData, int length, bool enableLogging)
        {
            if (TcpDiagStream == null)
            {
                return false;
            }
            try
            {
                lock (TcpDiagStreamRecLock)
                {
                    TcpDiagStreamRecEvent.Reset();
                    TcpDiagRecQueue.Clear();
                }

                byte targetAddr = sendData[1];
                byte sourceAddr = sendData[2];
                if (sourceAddr == 0xF1) sourceAddr = (byte)TesterAddress;
                int dataOffset = 3;
                int dataLength = sendData[0] & 0x3F;
                if (dataLength == 0)
                {   // with length byte
                    dataLength = sendData[3];
                    dataOffset = 4;
                }
                int payloadLength = dataLength + 2;
                DataBuffer[0] = (byte)((payloadLength >> 24) & 0xFF);
                DataBuffer[1] = (byte)((payloadLength >> 16) & 0xFF);
                DataBuffer[2] = (byte)((payloadLength >> 8) & 0xFF);
                DataBuffer[3] = (byte)(payloadLength & 0xFF);
                DataBuffer[4] = 0x00;   // Payoad type: Diag message
                DataBuffer[5] = 0x01;
                DataBuffer[6] = sourceAddr;
                DataBuffer[7] = targetAddr;
                Array.Copy(sendData, dataOffset, DataBuffer, 8, dataLength);
                int sendLength = dataLength + 8;
                lock (TcpDiagStreamSendLock)
                {
                    TcpDiagStream.Write(DataBuffer, 0, sendLength);
                }

                // wait for ack
                int recLen = ReceiveTelegram(AckBuffer, 5000);
                if (recLen < 0)
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No ack received");
                    return false;
                }
                if ((recLen != sendLength) || (AckBuffer[5] != 0x02))
                {
                    if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, RecBuffer, 0, recLen, "*** Ack frame invalid");
                    return false;
                }
                AckBuffer[4] = DataBuffer[4];
                AckBuffer[5] = DataBuffer[5];
                for (int i = 0; i < recLen; i++)
                {
                    if (AckBuffer[i] != DataBuffer[i])
                    {
                        if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, RecBuffer, 0, recLen, "*** Ack data invalid");
                        return false;
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected bool ReceiveData(byte[] receiveData, int timeout)
        {
            if (TcpDiagStream == null)
            {
                return false;
            }
            try
            {
                int recLen = ReceiveTelegram(DataBuffer, timeout);
                if (recLen < 4)
                {
                    return false;
                }
                // ReSharper disable RedundantCast
                int dataLen = (((int)DataBuffer[0] << 24) | ((int)DataBuffer[1] << 16) | ((int)DataBuffer[2] << 8) | DataBuffer[3]) - 2;
                // ReSharper restore RedundantCast
                if ((dataLen < 1) || ((dataLen + 8) > recLen))
                {
                    return false;
                }
                // create BMW-FAST telegram
                byte sourceAddr = DataBuffer[6];
                byte targetAddr = 0xF1;
                int len;
                if (dataLen > 0x3F)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = (byte)dataLen;
                    Array.Copy(DataBuffer, 8, receiveData, 4, dataLen);
                    len = dataLen + 4;
                }
                else
                {
                    receiveData[0] = (byte)(0x80 | dataLen);
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    Array.Copy(DataBuffer, 8, receiveData, 3, dataLen);
                    len = dataLen + 3;
                }
                receiveData[len] = CalcChecksumBmwFast(receiveData, len);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        protected int ReceiveTelegram(byte[] receiveData, int timeout)
        {
            if (TcpDiagStream == null)
            {
                return -1;
            }
            int recLen;
            try
            {
                int recTels;
                lock (TcpDiagStreamRecLock)
                {
                    recTels = TcpDiagRecQueue.Count;
                    if (recTels == 0)
                    {
                        TcpDiagStreamRecEvent.Reset();
                    }
                }
                if (recTels == 0)
                {
                    if (!TcpDiagStreamRecEvent.WaitOne(timeout, false))
                    {
                        return -1;
                    }
                }
                lock (TcpDiagStreamRecLock)
                {
                    if (TcpDiagRecQueue.Count > 0)
                    {
                        byte[] recTelFirst = TcpDiagRecQueue.Dequeue();
                        recLen = recTelFirst.Length;
                        Array.Copy(recTelFirst, receiveData, recLen);
                    }
                    else
                    {
                        recLen = 0;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return recLen;
        }

        protected EdiabasNet.ErrorCodes ObdTrans(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            receiveLength = 0;
            if (TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
            UInt32 retries = CommRepeatsProtected;
            string retryComm = EdiabasProtected.GetConfigProperty("RetryComm");
            if (retryComm != null)
            {
                if (EdiabasNet.StringToValue(retryComm) == 0)
                {
                    retries = 0;
                }
            }
            for (int i = 0; i < retries + 1; i++)
            {
                errorCode = ParTransmitFunc(sendData, sendDataLength, ref receiveData, out receiveLength);
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    return errorCode;
                }
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
                {   // interface error
                    break;
                }
            }
            return errorCode;
        }

        private EdiabasNet.ErrorCodes TransBmwFast(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength)
        {
            return TransKwp2000(sendData, sendDataLength, ref receiveData, out receiveLength, true);
        }

        private EdiabasNet.ErrorCodes TransKwp2000(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength, bool enableLogging)
        {
            receiveLength = 0;

            if (sendDataLength > 0)
            {
                int sendLength = TelLengthBmwFast(sendData);
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendLength, "Send");

                if (!SendData(sendData, sendLength, enableLogging))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
            }

            int timeout = ParTimeoutStd;
            for (int retry = 0; retry <= ParRetryNr; retry++)
            {
                if (!ReceiveData(receiveData, timeout))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No data received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthBmwFast(receiveData);
                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength + 1, "Resp");

                int dataLen = receiveData[0] & 0x3F;
                int dataStart = 3;
                if (dataLen == 0)
                {   // with length byte
                    dataLen = receiveData[3];
                    dataStart++;
                }
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NR 0x78");
                    timeout = ParTimeoutNr;
                }
                else
                {
                    break;
                }
            }

            receiveLength = TelLengthBmwFast(receiveData) + 1;
            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        // telegram length without checksum
        private int TelLengthBmwFast(byte[] dataBuffer)
        {
            int telLength = dataBuffer[0] & 0x3F;
            if (telLength == 0)
            {   // with length byte
                telLength = dataBuffer[3] + 4;
            }
            else
            {
                telLength += 3;
            }
            return telLength;
        }

        private byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                InterfaceDisconnect();
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }
                InterfaceUnlock();

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// TcpClientWithTimeout is used to open a TcpClient connection, with a
        /// user definable connection timeout in milliseconds (1000=1second)
        /// Use it like this:
        /// TcpClient connection = new TcpClientWithTimeout(host,80,1000).Connect();
        /// </summary>
        private class TcpClientWithTimeout
        {
            private readonly IPAddress _host;
            private readonly int _port;
            private readonly int _timeoutMilliseconds;
            private TcpClient _connection;
            private bool _connected;
            private Exception _exception;

            public TcpClientWithTimeout(IPAddress host, int port, int timeoutMilliseconds)
            {
                _host = host;
                _port = port;
                _timeoutMilliseconds = timeoutMilliseconds;
            }

            public TcpClient Connect()
            {
                // kick off the thread that tries to connect
                _connected = false;
                _exception = null;
                Thread thread = new Thread(BeginConnect)
                {
                    IsBackground = true
                };
                // So that a failed connection attempt
                // wont prevent the process from terminating while it does the long timeout
                thread.Start();

                // wait for either the timeout or the thread to finish
                thread.Join(_timeoutMilliseconds);

                if (_connected)
                {
                    // it succeeded, so return the connection
                    thread.Abort();
                    return _connection;
                }
                if (_exception != null)
                {
                    // it crashed, so return the exception to the caller
                    thread.Abort();
                    throw _exception;
                }
                else
                {
                    // if it gets here, it timed out, so abort the thread and throw an exception
                    thread.Abort();
                    throw new TimeoutException("Connect timeout");
                }
            }

            private void BeginConnect()
            {
                try
                {
                    _connection = new TcpClient();
                    IPEndPoint ipTcp = new IPEndPoint(_host, _port);
                    _connection.Connect(ipTcp);
                    // record that it succeeded, for the main thread to return to the caller
                    _connected = true;
                }
                catch (Exception ex)
                {
                    // record the exception for the main thread to re-throw back to the calling code
                    _exception = ex;
                }
            }
        }
    }
}
