using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseNullPropagation

namespace EdiabasLib
{
    public class EdInterfaceEnet : EdInterfaceBase
    {
#if Android
        public class ConnectParameterType
        {
            public ConnectParameterType(Android.Net.ConnectivityManager connectivityManager)
            {
                ConnectivityManager = connectivityManager;
            }

            public Android.Net.ConnectivityManager ConnectivityManager { get; }
        }
#endif

        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        protected delegate void ExecuteNetworkDelegate();

        private bool _disposed;
        protected const int TransBufferSize = 0x10010; // transmit buffer size
        protected const int TcpReadTimeoutOffset = 1000;
        protected const int TcpAckTimeout = 5000;
        protected const int UdpDetectRetries = 3;
        protected const string AutoIp = "auto";
        protected static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly byte[] ByteArray0 = new byte[0];
        protected static readonly byte[] UdpIdentReq = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 };
        protected static readonly byte[] TcpControlIgnitReq = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x10 };
        protected static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        protected static object ConnManager;
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
        protected static bool ReconnectRequired;

        protected Socket UdpSocket;
        protected byte[] UdpBuffer = new byte[0x100];
        protected volatile List<IPAddress> UdpRecIpListList = new List<IPAddress>();
        protected object UdpRecListLock = new object();
        protected int UdpMaxResponses;
        protected AutoResetEvent UdpEvent = new AutoResetEvent(false);

        protected string RemoteHostProtected = AutoIp;
        protected int TesterAddress = 0xF4;
        protected int ControlPort = 6811;
        protected int DiagnosticPort = 6801;
        protected int ConnectTimeout = 5000;

        protected byte[] RecBuffer = new byte[TransBufferSize];
        protected byte[] DataBuffer = new byte[TransBufferSize];
        protected byte[] AckBuffer = new byte[TransBufferSize];
        protected Dictionary<byte, int> Nr78Dict = new Dictionary<byte, int>();

        protected TransmitDelegate ParTransmitFunc;
        protected int ParTimeoutStd;
        protected int ParTimeoutTelEnd;
        protected int ParInterbyteTime;
        protected int ParRegenTime;
        protected int ParTimeoutNr78;
        protected int ParRetryNr78;

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
                ParTimeoutNr78 = 0;
                ParRetryNr78 = 0;
                Nr78Dict.Clear();

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
                        ParTimeoutNr78 = (int)CommParameterProtected[6];
                        ParRetryNr78 = (int)CommParameterProtected[5];
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
                        ParTimeoutNr78 = (int)CommParameterProtected[9];
                        ParRetryNr78 = (int)CommParameterProtected[10];
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
                if (ReconnectRequired)
                {
                    InterfaceDisconnect();
                    if (!InterfaceConnect())
                    {
                        ReconnectRequired = true;
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                }
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

        public override Int64 GetPort(UInt32 index)
        {
            return 0;
        }

        public override bool Connected
        {
            get
            {
                return ((TcpDiagClient != null) && (TcpDiagStream != null)) || ReconnectRequired;
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
            TcpDiagStreamSendLock = new object();
            TcpDiagStreamRecLock = new object();
            TcpControlTimer = new Timer(TcpControlTimeout, null, Timeout.Infinite, Timeout.Infinite);
            TcpControlTimerLock = new object();
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
#if Android
                if (ConnectParameter is ConnectParameterType connectParameter)
                {
                    ConnManager = connectParameter.ConnectivityManager;
                }
#endif
                TcpHostIp = null;
                if (RemoteHostProtected.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
                {
                    List<IPAddress> detectedVehicles = DetectedVehicles(RemoteHostProtected, 1, UdpDetectRetries);
                    if ((detectedVehicles == null) || (detectedVehicles.Count < 1))
                    {
                        return false;
                    }
                    TcpHostIp = detectedVehicles[0];
                    UdpSocket.Close();
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Received: IP={0}", TcpHostIp));
                }
                else
                {
                    TcpHostIp = IPAddress.Parse(RemoteHostProtected);
                }
                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                {
                    TcpDiagClient = new TcpClientWithTimeout(TcpHostIp, DiagnosticPort, ConnectTimeout).Connect();
                }, ConnManager);
                TcpDiagStream = TcpDiagClient.GetStream();
                TcpDiagRecLen = 0;
                LastTcpDiagRecTime = DateTime.MinValue.Ticks;
                lock (TcpDiagStreamRecLock)
                {
                    TcpDiagRecQueue.Clear();
                }
                StartReadTcpDiag(6);
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Connected");
                ReconnectRequired = false;
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
            if (EdiabasProtected != null)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Disconnect");
            }
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
            TcpHostIp = null;
            ReconnectRequired = false;
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
            byte[] cachedResponse;
            EdiabasNet.ErrorCodes cachedErrorCode;
            if (ReadCachedTransmission(sendData, out cachedResponse, out cachedErrorCode))
            {
                if (cachedErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected.SetError(cachedErrorCode);
                    return false;
                }
                receiveData = cachedResponse;
                return true;
            }
            if (ReconnectRequired)
            {
                InterfaceDisconnect();
                if (!InterfaceConnect())
                {
                    ReconnectRequired = true;
                    EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                    return false;
                }
            }
            int recLength;
            EdiabasNet.ErrorCodes errorCode = ObdTrans(sendData, sendData.Length, ref RecBuffer, out recLength);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
                {
                    ReconnectRequired = true;
                }
                CacheTransmission(sendData, null, errorCode);
                EdiabasProtected.SetError(errorCode);
                return false;
            }
            receiveData = new byte[recLength];
            Array.Copy(RecBuffer, receiveData, recLength);
            CacheTransmission(sendData, receiveData, EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE);
            return true;
        }

        public override bool TransmitFrequent(byte[] sendData)
        {
            EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
            return false;
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

        public override bool RawData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = ByteArray0;
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

        public List<IPAddress> DetectedVehicles(string remoteHostConfig)
        {
            return DetectedVehicles(remoteHostConfig, -1, UdpDetectRetries);
        }

        public List<IPAddress> DetectedVehicles(string remoteHostConfig, int maxVehicles, int maxRetries)
        {
            if (!remoteHostConfig.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            // ReSharper disable once UseObjectOrCollectionInitializer
            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if !WindowsCE
            UdpSocket.EnableBroadcast = true;
#endif
            IPEndPoint ipUdp = new IPEndPoint(IPAddress.Any, 0);
            UdpSocket.Bind(ipUdp);
            lock (UdpRecListLock)
            {
                UdpRecIpListList = new List<IPAddress>();
            }
            UdpMaxResponses = maxVehicles;
            StartUdpListen();

            int retryCount = 0;
            for (;;)
            {
                bool broadcastSend = false;
#if !WindowsCE
                string configData = remoteHostConfig.Remove(0, AutoIp.Length);
                if ((configData.Length > 0) && (configData[0] == ':'))
                {
                    string adapterName = configData.StartsWith(":all", StringComparison.OrdinalIgnoreCase) ? string.Empty : configData.Remove(0, 1);

#if Android
                    Java.Util.IEnumeration networkInterfaces = Java.Net.NetworkInterface.NetworkInterfaces;
                    while (networkInterfaces.HasMoreElements)
                    {
                        Java.Net.NetworkInterface netInterface = (Java.Net.NetworkInterface) networkInterfaces.NextElement();
                        if (netInterface.IsUp)
                        {
                            IList<Java.Net.InterfaceAddress> interfaceAdresses = netInterface.InterfaceAddresses;
                            foreach (Java.Net.InterfaceAddress interfaceAddress in interfaceAdresses)
                            {
                                if (string.IsNullOrEmpty(adapterName) || (netInterface.Name.StartsWith(adapterName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (interfaceAddress.Broadcast != null)
                                    {
                                        string broadcastAddressName = interfaceAddress.Broadcast.HostAddress;
                                        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                                        if (broadcastAddressName == null)
                                        {
                                            string text = interfaceAddress.Broadcast.ToString();
                                            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text, @"\d+\.\d+\.\d+\.\d+$", System.Text.RegularExpressions.RegexOptions.Singleline);
                                            if (match.Success)
                                            {
                                                broadcastAddressName = match.Value;
                                            }
                                        }
                                        if (broadcastAddressName != null && !broadcastAddressName.StartsWith("127."))
                                        {
                                            try
                                            {
                                                IPAddress broadcastAddress = IPAddress.Parse(broadcastAddressName);
                                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending: '{0}': Broadcast={1}",
                                                    netInterface.Name, broadcastAddress));
                                                IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, ControlPort);

                                                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                                {
                                                    UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                                }, ConnManager);
                                                broadcastSend = true;
                                            }
                                            catch (Exception)
                                            {
                                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Broadcast failed");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
#else
                    System.Net.NetworkInformation.NetworkInterface[] adapters = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                    foreach (System.Net.NetworkInformation.NetworkInterface adapter in adapters)
                    {
                        if (adapter.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                        {
                            System.Net.NetworkInformation.IPInterfaceProperties properties = adapter.GetIPProperties();
                            if (properties?.UnicastAddresses != null)
                            {
                                foreach (System.Net.NetworkInformation.UnicastIPAddressInformation ipAddressInfo in properties.UnicastAddresses)
                                {
                                    if (ipAddressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        if (string.IsNullOrEmpty(adapterName) || (adapter.Name.StartsWith(adapterName, StringComparison.OrdinalIgnoreCase)))
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
                                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending: '{0}': Ip={1} Mask={2} Broadcast={3}",
                                                    adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress));
                                                IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, ControlPort);
                                                UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                                broadcastSend = true;
                                            }
                                            catch (Exception)
                                            {
                                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Broadcast failed");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
#endif
                }
                else
#endif
                {
                    try
                    {
#if WindowsCE
                        IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Broadcast, ControlPort);
#else
                        IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Parse("169.254.255.255"), ControlPort);
#endif
                        if (EdiabasProtected != null)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending to: {0}", ipUdpIdent.Address));
                        }
                        TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                        {
                            UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                        }, ConnManager);
                        broadcastSend = true;
                    }
                    catch (Exception)
                    {
                        if (EdiabasProtected != null)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Broadcast failed");
                        }
                    }
                }
                if (!broadcastSend)
                {
                    if (EdiabasProtected != null)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "No broadcast send");
                    }
                    InterfaceDisconnect();
                    return null;
                }

                UdpEvent.WaitOne(1000, false);
                if (UdpRecIpListList.Count == 0)
                {
                    if (EdiabasProtected != null)
                    {
                        EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "No answer received");
                    }
                    if (retryCount < maxRetries)
                    {
                        retryCount++;
                        continue;
                    }
                    InterfaceDisconnect();
                    return null;
                }
                break;
            }
            UdpSocket.Close();
            List<IPAddress> ipList;
            lock (UdpRecListLock)
            {
                ipList = UdpRecIpListList;
                UdpRecIpListList = null;
            }
            return ipList;
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
                if ((recLen >= (6 + 38)) &&
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
                    int listCount = 0;
                    lock (UdpRecListLock)
                    {
                        if (UdpRecIpListList != null)
                        {
                            UdpRecIpListList.Add(((IPEndPoint)tempRemoteEp).Address);
                            listCount = UdpRecIpListList.Count;
                        }
                    }
                    if ((UdpMaxResponses >= 1) && (listCount >= UdpMaxResponses))
                    {
                        UdpEvent.Set();
                    }
                    else
                    {
                        StartUdpListen();
                    }
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
                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                {
                    TcpControlClient = TcpDiagClient = new TcpClientWithTimeout(TcpHostIp, ControlPort, ConnectTimeout).Connect();
                }, ConnManager);
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
                            case 0x01:  // diag data
                            case 0x02:  // ack
                            case 0xFF:  // nack
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
                    if (sendData[3] == 0)
                    {
                        dataLength = (sendData[4] << 8) | sendData[5];
                        dataOffset = 6;
                    }
                    else
                    {
                        dataLength = sendData[3];
                        dataOffset = 4;
                    }
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
                int recLen = ReceiveAck(AckBuffer, TcpAckTimeout, enableLogging);
                if (recLen < 0)
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No ack received");
                    return false;
                }
                if ((recLen == 6) && (AckBuffer[5] == 0xFF))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "nack received: resending");
                    lock (TcpDiagStreamSendLock)
                    {
                        TcpDiagStream.Write(DataBuffer, 0, sendLength);
                    }
                    recLen = ReceiveAck(AckBuffer, TcpAckTimeout, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No ack received");
                        return false;
                    }
                }
                if ((recLen < 6) || (recLen > sendLength) || (AckBuffer[5] != 0x02))
                {
                    if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack frame invalid");
                    return false;
                }
                AckBuffer[4] = DataBuffer[4];
                AckBuffer[5] = DataBuffer[5];
                for (int i = 4; i < recLen; i++)
                {
                    if (AckBuffer[i] != DataBuffer[i])
                    {
                        if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack data invalid");
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
                if ((dataLen < 1) || ((dataLen + 8) > recLen) || (dataLen > 0xFFFF))
                {
                    return false;
                }
                // create BMW-FAST telegram
                byte sourceAddr = DataBuffer[6];
                byte targetAddr = 0xF1;
                int len;
                if (dataLen > 0xFF)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = 0x00;
                    receiveData[4] = (byte)(dataLen >> 8);
                    receiveData[5] = (byte)dataLen;
                    Array.Copy(DataBuffer, 8, receiveData, 6, dataLen);
                    len = dataLen + 6;
                }
                else if (dataLen > 0x3F)
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

        protected int ReceiveAck(byte[] receiveData, int timeout, bool enableLogging)
        {
            long startTick = Stopwatch.GetTimestamp();
            for (;;)
            {
                int recLen = ReceiveTelegram(receiveData, timeout);
                if (recLen < 0)
                {
                    return recLen;
                }
                if ((recLen >= 6) &&
                    ((receiveData[5] == 0x02) || (receiveData[5] == 0xFF)))
                {   // ACK or NACK received
                    return recLen;
                }
                if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Ignore Non ack");
                if ((Stopwatch.GetTimestamp() - startTick) > timeout * TickResolMs)
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Ack timeout");
                    return -1;
                }
            }
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

        private void Nr78DictAdd(byte deviceAddr, bool enableLogging)
        {
            int retries;
            if (Nr78Dict.TryGetValue(deviceAddr, out retries))
            {
                Nr78Dict.Remove(deviceAddr);
                retries++;
                if (retries <= ParRetryNr78)
                {
                    if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) count={1}", deviceAddr, retries);
                    Nr78Dict.Add(deviceAddr, retries);
                }
                else
                {
                    if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NR78({0:X02}) exceeded", deviceAddr);
                }
            }
            else
            {
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) added", deviceAddr);
                Nr78Dict.Add(deviceAddr, 0);
            }
        }

        private void Nr78DictRemove(byte deviceAddr, bool enableLogging)
        {
            if (Nr78Dict.ContainsKey(deviceAddr))
            {
                if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) removed", deviceAddr);
                Nr78Dict.Remove(deviceAddr);
            }
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
                Nr78Dict.Clear();
                int sendLength = TelLengthBmwFast(sendData);
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendLength, "Send");

                if (!SendData(sendData, sendLength, enableLogging))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                }
            }

            for (; ; )
            {
                int timeout = (Nr78Dict.Count > 0) ? ParTimeoutNr78 : ParTimeoutStd;
                //if (enableLogging) EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Timeout: {0}", timeout);
                timeout += TcpReadTimeoutOffset;
                if (!ReceiveData(receiveData, timeout))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No data received");
                    return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                }

                int recLength = TelLengthBmwFast(receiveData);
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength + 1, "Resp");

                int dataLen = receiveData[0] & 0x3F;
                int dataStart = 3;
                if (dataLen == 0)
                {   // with length byte
                    if (receiveData[3] == 0)
                    {
                        dataLen = (receiveData[4] << 8) | receiveData[5];
                        dataStart += 3;
                    }
                    else
                    {
                        dataLen = receiveData[3];
                        dataStart++;
                    }
                }
                if ((dataLen == 3) && (receiveData[dataStart] == 0x7F) && (receiveData[dataStart + 2] == 0x78))
                {   // negative response 0x78
                    Nr78DictAdd(receiveData[2], enableLogging);
                }
                else
                {
                    Nr78DictRemove(receiveData[2], enableLogging);
                    break;
                }
                if (Nr78Dict.Count == 0)
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
                if (dataBuffer[3] == 0)
                {
                    telLength = ((dataBuffer[4] << 8) | dataBuffer[5]) + 6;
                }
                else
                {
                    telLength = dataBuffer[3] + 4;
                }
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
    }
}
