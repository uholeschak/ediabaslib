using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;

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
            public ConnectParameterType(TcpClientWithTimeout.NetworkData networkData)
            {
                NetworkData = networkData;
            }

            public TcpClientWithTimeout.NetworkData NetworkData { get; }
        }
#endif

        public class EnetConnection : IComparable<EnetConnection>, IEquatable<EnetConnection>
        {
            public enum InterfaceType
            {
                Direct,
                Enet,
                Icom
            }

            public EnetConnection(InterfaceType connectionType, IPAddress ipAddress, int diagPort = -1, int controlPort = -1)
            {
                ConnectionType = connectionType;
                IpAddress = ipAddress;
                DiagPort = diagPort;
                ControlPort = controlPort;
                Mac = string.Empty;
                Vin = string.Empty;
            }

            public InterfaceType ConnectionType { get; }
            public IPAddress IpAddress { get;}
            public int DiagPort { get; }
            public int ControlPort { get; }
            public string Mac { get; set; }
            public string Vin { get; set; }
            private int? hashCode;

            public override string ToString()
            {
                if (IpAddress == null)
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(IpAddress);
                int skipped = 0;
                if (DiagPort >= 0)
                {
                    sb.Append(string.Format(CultureInfo.InvariantCulture, ":{0}", DiagPort));
                }
                else
                {
                    skipped++;
                }

                if (ControlPort >= 0)
                {
                    while (skipped > 0)
                    {
                        sb.Append(":");
                        skipped--;
                    }

                    sb.Append(string.Format(CultureInfo.InvariantCulture, ":{0}", ControlPort));
                }

                return sb.ToString();
            }

            public int CompareTo(EnetConnection enetConnection)
            {
                if (IpAddress == null)
                {
                    return 0;
                }

                byte[] bytesLocal = IpAddress.GetAddressBytes().Reverse().ToArray();
                UInt32 localValue = BitConverter.ToUInt32(bytesLocal, 0);
                byte[] bytesOther = enetConnection.IpAddress.GetAddressBytes().Reverse().ToArray();
                UInt32 otherValue = BitConverter.ToUInt32(bytesOther, 0);
                if (localValue < otherValue)
                {
                    return -1;
                }

                if (localValue > otherValue)
                {
                    return 1;
                }

                if (DiagPort < enetConnection.DiagPort)
                {
                    return -1;
                }

                if (DiagPort > enetConnection.DiagPort)
                {
                    return 1;
                }

                return 0;
            }

            public override bool Equals(object obj)
            {
                EnetConnection enetConnection = obj as EnetConnection;
                if ((object)enetConnection == null)
                {
                    return false;
                }

                return Equals(enetConnection);
            }

            public bool Equals(EnetConnection enetConnection)
            {
                if ((object)enetConnection == null)
                {
                    return false;
                }

                string thisString = ToString();
                string otherString = enetConnection.ToString();
                if (string.IsNullOrEmpty(thisString) || string.IsNullOrEmpty(otherString))
                {
                    return false;
                }

                if (string.Compare(otherString, thisString, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                if (!hashCode.HasValue)
                {
                    hashCode = ToString().GetHashCode();
                }

                return hashCode.Value;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }

            public static bool operator == (EnetConnection lhs, EnetConnection rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return Object.Equals(lhs, rhs);
                }

                return lhs.Equals(rhs);
            }

            public static bool operator != (EnetConnection lhs, EnetConnection rhs)
            {
                if ((object)lhs == null || (object)rhs == null)
                {
                    return !Object.Equals(lhs, rhs);
                }

                return !(lhs == rhs);
            }

        }

        protected class SharedData : IDisposable
        {
            public SharedData()
            {
                HttpAllocCancelToken = new CancellationTokenSource();
                TcpDiagStreamRecEvent = new AutoResetEvent(false);
                TransmitCancelEvent = new ManualResetEvent(false);
                TcpDiagStreamSendLock = new object();
                TcpDiagStreamRecLock = new object();
                TcpControlTimer = new Timer(TcpControlTimeout, this, Timeout.Infinite, Timeout.Infinite);
                TcpControlTimerLock = new object();
                TcpDiagBuffer = new byte[TransBufferSize];
                TcpDiagRecLen = 0;
                LastTcpDiagRecTime = DateTime.MinValue.Ticks;
                TcpDiagRecQueue = new Queue<byte[]>();
            }

            public void Dispose()
            {
                Dispose(true);
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SupressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                // Check to see if Dispose has already been called.
                if (!_disposed)
                {
                    // If disposing equals true, dispose all managed
                    // and unmanaged resources.
                    if (disposing)
                    {
                        // Dispose managed resources.
                        if (TcpDiagStreamRecEvent != null)
                        {
                            TcpDiagStreamRecEvent.Dispose();
                            TcpDiagStreamRecEvent = null;
                        }

                        if (TransmitCancelEvent != null)
                        {
                            TransmitCancelEvent.Dispose();
                            TransmitCancelEvent = null;
                        }

                        if (TcpControlTimer != null)
                        {
                            TcpControlTimer.Dispose();
                            TcpControlTimer = null;
                        }
                    }

                    // Note disposing has been done.
                    _disposed = true;
                }
            }

            private bool _disposed;
            public object NetworkData;
            public EnetConnection EnetHostConn;
            public HttpClient IcomAllocateDeviceHttpClient;
            public CancellationTokenSource HttpAllocCancelToken;
            public TcpClient TcpDiagClient;
            public NetworkStream TcpDiagStream;
            public AutoResetEvent TcpDiagStreamRecEvent;
            public ManualResetEvent TransmitCancelEvent;
            public TcpClient TcpControlClient;
            public NetworkStream TcpControlStream;
            public Timer TcpControlTimer;
            public bool TcpControlTimerEnabled;
            public object TcpDiagStreamSendLock;
            public object TcpDiagStreamRecLock;
            public object TcpControlTimerLock;
            public byte[] TcpDiagBuffer;
            public int TcpDiagRecLen;
            public long LastTcpDiagRecTime;
            public Queue<byte[]> TcpDiagRecQueue;
            public bool ReconnectRequired;
            public bool IcomAllocateActive;
        }

        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        protected delegate void ExecuteNetworkDelegate();
        public delegate void IcomAllocateDeviceDelegate(bool success, int statusCode = -1);

        private bool _disposed;
        private static Mutex _interfaceMutex;
        protected const string MutexName = "EdiabasLib_InterfaceEnet";
        protected const int TransBufferSize = 0x10010; // transmit buffer size
        protected const int TcpConnectTimeoutMin = 1000;
        protected const int TcpAckTimeout = 5000;
        protected const int TcpSendBufferSize = 1400;
        protected const int UdpDetectRetries = 3;
        protected const string AutoIp = "auto";
        protected const string IniFileSection = "XEthernet";
        protected const string IcomOwner = "DeepObd";
        protected static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        protected static readonly byte[] ByteArray0 = new byte[0];
        protected static readonly byte[] UdpIdentReq =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x11
        };
        protected static readonly byte[] UdpSvrLocReq =
        {
            0x02, 0x06, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00,
            0x00, 0x00, 0xAB, 0xCD, 0x00, 0x02, 0x65, 0x6E,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x64, 0x65,
            0x66, 0x61, 0x75, 0x6C, 0x74, 0x00, 0x00, 0x00,
            0x00
        };
        protected static readonly byte[] TcpControlIgnitReq = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x10 };
        protected static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        protected static SharedData SharedDataStatic;

        protected SharedData NonSharedData;
        protected Socket UdpSocket;
        protected byte[] UdpBuffer = new byte[1500];
        protected volatile List<EnetConnection> UdpRecIpListList = new List<EnetConnection>();
        protected object UdpRecListLock = new object();
        protected int UdpMaxResponses;
        protected AutoResetEvent UdpEvent = new AutoResetEvent(false);
        protected AutoResetEvent IcomEvent = new AutoResetEvent(false);

        protected string RemoteHostProtected = AutoIp;
        protected int TesterAddress = 0xF4;
        protected string AutoIpBroadcastAddress = @"169.254.255.255";
        protected int UdpIdentPort = 6811;
        protected int UdpSrvLocPort = 427;
        protected int ControlPort = 6811;
        protected int DiagnosticPort = 6801;
        protected int ConnectTimeout = 5000;
        protected int AddRecTimeoutProtected = 1000;
        protected int AddRecTimeoutIcomProtected = 2000;
        protected bool IcomAllocateProtected = false;

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

        protected SharedData SharedDataActive
        {
            get
            {
                return NonSharedData ?? SharedDataStatic;
            }
        }

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

                prop = EdiabasProtected.GetConfigProperty("RemoteHost");
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

                prop = EdiabasProtected.GetConfigProperty("ControlPort");
                if (prop != null)
                {
                    ControlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetDiagnosticPort");
                if (prop != null)
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("DiagnosticPort");
                if (prop != null)
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetTimeoutConnect");
                if (prop != null)
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("TimeoutConnect");
                if (prop != null)
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetAddRecTimeout");
                if (prop != null)
                {
                    AddRecTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected.GetConfigProperty("EnetAddRecTimeoutIcom");
                if (prop != null)
                {
                    AddRecTimeoutIcom = (int)EdiabasNet.StringToValue(prop);
                }
#if Android
                IcomAllocate = true;
#else
                IcomAllocate = false;
#endif
                prop = EdiabasProtected.GetConfigProperty("EnetIcomAllocate");
                if (prop != null)
                {
                    IcomAllocate = EdiabasNet.StringToValue(prop) != 0;
                }

                if (!IsIpv4Address(RemoteHostProtected))
                {
                    string iniFile = EdiabasProtected.IniFileName;
                    if (!string.IsNullOrEmpty(iniFile))
                    {
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using ENET ini file at: {0}", iniFile);
                        IniFile ediabasIni = new IniFile(iniFile);
                        string iniRemoteHost = ediabasIni.GetValue(IniFileSection, "RemoteHost", string.Empty);
                        bool hostValid = false;
                        if (IsIpv4Address(iniRemoteHost))
                        {
                            hostValid = true;
                            RemoteHostProtected = iniRemoteHost;
                            EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using remote host from ini file: {0}", RemoteHostProtected);
                        }

                        if (hostValid)
                        {
                            string iniControlPort = ediabasIni.GetValue(IniFileSection, "ControlPort", string.Empty);
                            if (!string.IsNullOrEmpty(iniControlPort))
                            {
                                ControlPort = (int)EdiabasNet.StringToValue(iniControlPort);
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using control port from ini file: {0}", ControlPort);
                            }

                            string iniDiagnosticPort = ediabasIni.GetValue(IniFileSection, "DiagnosticPort", string.Empty);
                            if (!string.IsNullOrEmpty(iniDiagnosticPort))
                            {
                                DiagnosticPort = (int)EdiabasNet.StringToValue(iniDiagnosticPort);
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using diagnostic port from ini file: {0}", DiagnosticPort);
                            }
                        }
                    }
                }

                if (ConnectTimeout < TcpConnectTimeoutMin)
                {
                    ConnectTimeout = TcpConnectTimeoutMin;
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

        public override bool BmwFastProtocol
        {
            get
            {
                return true;
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
                if (SharedDataActive.ReconnectRequired)
                {
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(true))
                    {
                        SharedDataActive.ReconnectRequired = true;
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
                    lock (SharedDataActive.TcpControlTimerLock)
                    {
                        TcpControlTimerStop(SharedDataActive);
                    }
                    if (!TcpControlConnect())
                    {
                        EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    WriteNetworkStream(SharedDataActive.TcpControlStream, TcpControlIgnitReq, 0, TcpControlIgnitReq.Length);
                    SharedDataActive.TcpControlStream.ReadTimeout = 1000;
                    int recLen = SharedDataActive.TcpControlStream.Read(RecBuffer, 0, 7);
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
                    TcpControlTimerStart(SharedDataActive);
                }
                return 0;
            }
        }

        public override int? AdapterVersion
        {
            get
            {
                return null;
            }
        }

        public override byte[] AdapterSerial
        {
            get
            {
                return null;
            }
        }

        public override double? AdapterVoltage
        {
            get
            {
                return null;
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
                return ((SharedDataActive.TcpDiagClient != null) && (SharedDataActive.TcpDiagStream != null)) || SharedDataActive.ReconnectRequired;
            }
        }

        protected override Mutex InterfaceMutex
        {
            get { return _interfaceMutex; }
            set { _interfaceMutex = value; }
        }

        protected override string InterfaceMutexName
        {
            get { return MutexName; }
        }

        static EdInterfaceEnet()
        {
#if WindowsCE || Android
            _interfaceMutex = new Mutex(false);
#else
            _interfaceMutex = new Mutex(false, MutexName);
#endif
            SharedDataStatic = new SharedData();
        }

        ~EdInterfaceEnet()
        {
            Dispose(false);
        }

        public EdInterfaceEnet(bool shared = true)
        {
            if (!shared)
            {
                NonSharedData = new SharedData();
            }
        }

        public override bool IsValidInterfaceName(string name)
        {
            return IsValidInterfaceNameStatic(name);
        }

        public static bool IsValidInterfaceNameStatic(string name)
        {
            string[] nameParts = name.Split(':');
            if (nameParts.Length > 0 && string.Compare(nameParts[0], "ENET", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        public override bool InterfaceConnect()
        {
            return InterfaceConnect(false);
        }

        public bool InterfaceConnect(bool reconnect)
        {
            if (SharedDataActive.TcpDiagClient != null)
            {
                return true;
            }
            try
            {
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connect to: {0}", RemoteHostProtected);
                SharedDataActive.NetworkData = null;
#if Android
                if (ConnectParameter is ConnectParameterType connectParameter)
                {
                    SharedDataActive.NetworkData = connectParameter.NetworkData;
                }
#endif
                SharedDataActive.EnetHostConn = null;
                if (RemoteHostProtected.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
                {
                    List<EnetConnection> detectedVehicles = DetectedVehicles(RemoteHostProtected, 1, UdpDetectRetries);
                    if ((detectedVehicles == null) || (detectedVehicles.Count < 1))
                    {
                        return false;
                    }
                    SharedDataActive.EnetHostConn = detectedVehicles[0];
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Received: IP={0}:{1}", SharedDataActive.EnetHostConn.IpAddress, SharedDataActive.EnetHostConn.DiagPort));
                }
                else
                {
                    string[] hostParts = RemoteHostProtected.Split(':');
                    if (hostParts.Length < 1)
                    {
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Host name invalid: {0}", RemoteHostProtected);
                        return false;
                    }

                    string hostIp = hostParts[0];
                    EnetConnection.InterfaceType connectionType = EnetConnection.InterfaceType.Direct;
                    int hostDiagPort = -1;
                    int hostControlPort = -1;
                    if (hostParts.Length >= 2)
                    {
                        Int64 portValue = EdiabasNet.StringToValue(hostParts[1], out bool valid);
                        if (valid)
                        {
                            hostDiagPort = (int)portValue;
                            connectionType = EnetConnection.InterfaceType.Icom;
                        }
                    }

                    if (hostParts.Length >= 3)
                    {
                        Int64 portValue = EdiabasNet.StringToValue(hostParts[2], out bool valid);
                        if (valid)
                        {
                            hostControlPort = (int)portValue;
                            connectionType = EnetConnection.InterfaceType.Icom;
                        }
                    }

                    SharedDataActive.EnetHostConn = new EnetConnection(connectionType, IPAddress.Parse(hostIp), hostDiagPort, hostControlPort);
                }

                int diagPort = DiagnosticPort;
                if (SharedDataActive.EnetHostConn.DiagPort >= 0)
                {
                    diagPort = SharedDataActive.EnetHostConn.DiagPort;
                }

                if (IcomAllocate && !reconnect && SharedDataActive.EnetHostConn.DiagPort >= 0)
                {
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM at: {0}", SharedDataActive.EnetHostConn.IpAddress);
                    IcomEvent.Reset();
                    using (CancellationTokenSource cts = new CancellationTokenSource())
                    {
                        if (!IcomAllocateDevice(SharedDataActive.EnetHostConn.IpAddress.ToString(), true, cts, (success, code) =>
                        {
                            if (success && code == 0)
                            {
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM ok: Code={0}", code);
                            }
                            else
                            {
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM rejected: Code={0}", code);
                            }

                            IcomEvent.Set();
                        }))
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM error");
                        }

                        if (!IcomEvent.WaitOne(2000, false))
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM timeout");
                            cts.Cancel();
                            IcomEvent.WaitOne(1000, false);
                        }
                    }
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM finished");
                }

                SharedDataActive.TransmitCancelEvent.Reset();
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting to: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress, diagPort);
                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                {
                    SharedDataActive.TcpDiagClient = new TcpClientWithTimeout(SharedDataActive.EnetHostConn.IpAddress, diagPort, ConnectTimeout, true).
                        Connect(() => SharedDataActive.TransmitCancelEvent.WaitOne(0, false));
                }, SharedDataActive.EnetHostConn.IpAddress, SharedDataActive.NetworkData);

                SharedDataActive.TcpDiagClient.SendBufferSize = TcpSendBufferSize;
                SharedDataActive.TcpDiagClient.NoDelay = true;
                SharedDataActive.TcpDiagStream = SharedDataActive.TcpDiagClient.GetStream();
                SharedDataActive.TcpDiagRecLen = 0;
                SharedDataActive.LastTcpDiagRecTime = DateTime.MinValue.Ticks;
                lock (SharedDataActive.TcpDiagStreamRecLock)
                {
                    SharedDataActive.TcpDiagRecQueue.Clear();
                }
                StartReadTcpDiag(6);
                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connected to: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress.ToString(), diagPort);
                SharedDataActive.ReconnectRequired = false;
            }
            catch (Exception ex)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "InterfaceConnect exception: " + EdiabasNet.GetExceptionText(ex));
                InterfaceDisconnect(reconnect);
                return false;
            }
            return true;
        }

        public override bool InterfaceDisconnect()
        {
            return InterfaceDisconnect(false);
        }

        public bool InterfaceDisconnect(bool reconnect)
        {
            if (EdiabasProtected != null)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Disconnect");
            }
            bool result = true;

            try
            {
                if (SharedDataActive.TcpDiagStream != null)
                {
                    SharedDataActive.TcpDiagStream.Close();
                    SharedDataActive.TcpDiagStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (SharedDataActive.TcpDiagClient != null)
                {
                    SharedDataActive.TcpDiagClient.Close();
                    SharedDataActive.TcpDiagClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            if (!TcpControlDisconnect(SharedDataActive))
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

            if (IcomAllocate && !reconnect && SharedDataActive.EnetHostConn != null && SharedDataActive.EnetHostConn.DiagPort >= 0)
            {
                if (EdiabasProtected != null)
                {
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM at: {0}", SharedDataActive.EnetHostConn.IpAddress);
                }

                IcomEvent.Reset();
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    if (!IcomAllocateDevice(SharedDataActive.EnetHostConn.IpAddress.ToString(), false, cts, (success, code) =>
                    {
                        if (success)
                        {
                            if (EdiabasProtected != null)
                            {
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM ok: Code={0}", code);
                            }
                        }
                        else
                        {
                            if (EdiabasProtected != null)
                            {
                                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM rejected: Code={0}", code);
                            }
                        }

                        IcomEvent.Set();
                    }))
                    {
                        if (EdiabasProtected != null)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM error");
                        }
                    }

                    if (!IcomEvent.WaitOne(2000, false))
                    {
                        if (EdiabasProtected != null)
                        {
                            EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM timeout");
                        }
                        cts.Cancel();
                        IcomEvent.WaitOne(1000, false);
                    }
                }

                if (EdiabasProtected != null)
                {
                    EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM finished");
                }
            }

            SharedDataActive.EnetHostConn = null;
            SharedDataActive.ReconnectRequired = false;
            return result;
        }

        public override bool InterfaceReset()
        {
            CommParameter = null;
            return true;
        }

        public override bool InterfaceBoot()
        {
            CommParameter = null;
            return true;
        }

        public override bool TransmitData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = null;
            if (CommParameterProtected == null)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Info, "TransmitData with default CommParameter");
                SetDefaultCommParameter();
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
            if (SharedDataActive.ReconnectRequired)
            {
                InterfaceDisconnect(true);
                if (!InterfaceConnect(true))
                {
                    SharedDataActive.ReconnectRequired = true;
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
                    SharedDataActive.ReconnectRequired = true;
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

        public override bool TransmitCancel(bool cancel)
        {
            if (cancel)
            {
                SharedDataActive.TransmitCancelEvent.Set();
            }
            else
            {
                SharedDataActive.TransmitCancelEvent.Reset();
            }
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

        public int AddRecTimeout
        {
            get
            {
                return AddRecTimeoutProtected;
            }
            set
            {
                AddRecTimeoutProtected = value;
            }
        }

        public int AddRecTimeoutIcom
        {
            get
            {
                return AddRecTimeoutIcomProtected;
            }
            set
            {
                AddRecTimeoutIcomProtected = value;
            }
        }

        public bool IcomAllocate
        {
            get
            {
                return IcomAllocateProtected;
            }
            set
            {
                IcomAllocateProtected = value;
            }
        }

        public List<EnetConnection> DetectedVehicles(string remoteHostConfig)
        {
            return DetectedVehicles(remoteHostConfig, -1, UdpDetectRetries);
        }

        public List<EnetConnection> DetectedVehicles(string remoteHostConfig, int maxVehicles, int maxRetries)
        {
            if (!remoteHostConfig.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
// ReSharper disable once UseObjectOrCollectionInitializer
                UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if !WindowsCE
                UdpSocket.EnableBroadcast = true;
#endif
                IPEndPoint ipUdp = new IPEndPoint(IPAddress.Any, 0);
                UdpSocket.Bind(ipUdp);
                lock (UdpRecListLock)
                {
                    UdpRecIpListList = new List<EnetConnection>();
                }
                UdpMaxResponses = maxVehicles;
                StartUdpListen();

                int retryCount = 0;
                for (;;)
                {
                    UdpEvent.Reset();
                    bool broadcastSend = false;
#if !WindowsCE
                    string configData = remoteHostConfig.Remove(0, AutoIp.Length);
                    if ((configData.Length > 0) && (configData[0] == ':'))
                    {
                        string adapterName = configData.StartsWith(":all", StringComparison.OrdinalIgnoreCase) ? string.Empty : configData.Remove(0, 1);

#if Android
                        Java.Util.IEnumeration networkInterfaces = Java.Net.NetworkInterface.NetworkInterfaces;
                        while (networkInterfaces != null && networkInterfaces.HasMoreElements)
                        {
                            Java.Net.NetworkInterface netInterface = (Java.Net.NetworkInterface) networkInterfaces.NextElement();
                            if (netInterface == null)
                            {
                                continue;
                            }

                            if (netInterface.IsUp)
                            {
                                IList<Java.Net.InterfaceAddress> interfaceAdresses = netInterface.InterfaceAddresses;
                                if (interfaceAdresses == null)
                                {
                                    continue;
                                }

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
                                                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending: '{0}': Ident broadcast={1} Port={2}",
                                                        netInterface.Name, broadcastAddress, UdpIdentPort));
                                                    IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, UdpIdentPort);

                                                    TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                                    {
                                                        UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                                    }, ipUdpIdent.Address, SharedDataActive.NetworkData);

                                                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending: '{0}': SrvLoc broadcast={1} Port={2}",
                                                        netInterface.Name, broadcastAddress, UdpSrvLocPort));
                                                    IPEndPoint ipUdpSvrLoc = new IPEndPoint(broadcastAddress, UdpSrvLocPort);

                                                    TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                                    {
                                                        UdpSocket.SendTo(UdpSvrLocReq, ipUdpSvrLoc);
                                                    }, ipUdpSvrLoc.Address, SharedDataActive.NetworkData);

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

                                                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending ident: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                        adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, UdpIdentPort));
                                                    IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, UdpIdentPort);
                                                    UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);

                                                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending SvrLoc: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                        adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, UdpSrvLocPort));
                                                    IPEndPoint ipUdpSvrLoc = new IPEndPoint(broadcastAddress, UdpSrvLocPort);
                                                    UdpSocket.SendTo(UdpSvrLocReq, ipUdpSvrLoc);
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
                            IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Broadcast, UdpIdentPort);
                            IPEndPoint ipUdpSvrLoc = new IPEndPoint(IPAddress.Broadcast, UdpSrvLocPort);
#else
                            IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Parse(AutoIpBroadcastAddress), UdpIdentPort);
                            IPEndPoint ipUdpSvrLoc = new IPEndPoint(IPAddress.Parse(AutoIpBroadcastAddress), UdpSrvLocPort);
#endif
                            if (EdiabasProtected != null)
                            {
                                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending Ident broadcast to: {0}:{1}", ipUdpIdent.Address, UdpIdentPort));
                            }
                            TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                            {
                                UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                            }, ipUdpIdent.Address, SharedDataActive.NetworkData);

                            if (EdiabasProtected != null)
                            {
                                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending SvrLoc broadcast to: {0}:{1}", ipUdpSvrLoc.Address , UdpSrvLocPort));
                            }
                            TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                            {
                                UdpSocket.SendTo(UdpSvrLocReq, ipUdpSvrLoc);
                            }, ipUdpSvrLoc.Address, SharedDataActive.NetworkData);

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
                        return null;
                    }
                    break;
                }
            }
            finally
            {
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
                    // ignored
                }
            }

            List<EnetConnection> ipList;
            lock (UdpRecListLock)
            {
                ipList = UdpRecIpListList.OrderBy(x => x).ToList();
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
                int recPort = ((IPEndPoint) tempRemoteEp).Port;
                IPAddress recIp = ((IPEndPoint)tempRemoteEp).Address;
                bool continueRec = true;
                EnetConnection addListConn = null;
                string vehicleVin = string.Empty;
                string vehicleMac = string.Empty;

                if (recPort == UdpIdentPort)
                {
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
                        addListConn = new EnetConnection(EnetConnection.InterfaceType.Direct, recIp);
                        try
                        {
                            vehicleMac = Encoding.ASCII.GetString(UdpBuffer, 15 + 6, 12);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        if (recLen >= 15 + 6 + 12 + 6 + 17)
                        {
                            try
                            {
                                vehicleVin = Encoding.ASCII.GetString(UdpBuffer, 15 + 6 + 12 + 6, 17);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
                else if (recPort == UdpSrvLocPort)
                {
                    Dictionary<string, string> attrDict = ExtractSvrLocItems(UdpBuffer, recLen, 0xABCD);
                    if (attrDict != null)
                    {
                        bool isEnet = false;
                        bool isIcom = false;
                        IPAddress ipAddressHost = recIp;

                        if (attrDict.TryGetValue("DEVTYPE", out string devType))
                        {
                            if (string.Compare(devType, "ENET", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                isEnet = true;
                            }
                            else if (string.Compare(devType, "ICOM", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                isIcom = true;
                            }
                        }

                        if (attrDict.TryGetValue("IPADDRESS", out string ipString))
                        {
                            if (IPAddress.TryParse(ipString, out IPAddress vehicleIp))
                            {
                                ipAddressHost = vehicleIp;
                            }
                        }

                        if (attrDict.TryGetValue("MACADDRESS", out string macString))
                        {
                            vehicleMac = macString;
                        }

                        if (attrDict.TryGetValue("VIN", out string vinString))
                        {
                            vehicleVin = vinString;
                        }

                        if (isEnet)
                        {
                            addListConn = new EnetConnection(EnetConnection.InterfaceType.Enet, ipAddressHost);
                        }
                        else if (isIcom)
                        {
                            int gatewayAddr = -1;
                            if (attrDict.TryGetValue("GATEWAY", out string gatewayString))
                            {
                                Int64 gatewayValue = EdiabasNet.StringToValue(gatewayString, out bool valid);
                                if (valid)
                                {
                                    gatewayAddr = (int) gatewayValue;
                                }
                            }

                            bool enetChannel = false;
                            if (attrDict.TryGetValue("VCICHANNELS", out string vciChannels))
                            {
                                vciChannels = vciChannels.TrimStart('[');
                                vciChannels = vciChannels.TrimEnd(']');
                                string[] channelList = vciChannels.Split(';');
                                if (channelList.Contains("3+") || channelList.Contains("3*"))
                                {
                                    enetChannel = true;
                                }
                            }

                            bool isFree = true;
                            if (attrDict.TryGetValue("OWNER", out string ownerString))
                            {
                                ownerString = ownerString.Trim();
                                if (!string.IsNullOrEmpty(ownerString))
                                {
                                    if (string.Compare(ownerString, IcomOwner, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        isFree = false;
                                    }
                                }
                            }

                            if (gatewayAddr >= 0 && enetChannel && isFree)
                            {
                                addListConn = new EnetConnection(EnetConnection.InterfaceType.Icom, ipAddressHost, 50000 + gatewayAddr * 10, 50001 + gatewayAddr * 10);
                            }
                        }
                    }
                }

                if (addListConn != null)
                {
                    addListConn.Mac = vehicleMac;
                    addListConn.Vin = vehicleVin;

                    int listCount = 0;
                    lock (UdpRecListLock)
                    {
                        if (UdpRecIpListList != null)
                        {
                            if (UdpRecIpListList.All(x => x != addListConn))
                            {
                                UdpRecIpListList.Add(addListConn);
                            }

                            listCount = UdpRecIpListList.Count;
                        }
                    }

                    if ((UdpMaxResponses >= 1) && (listCount >= UdpMaxResponses))
                    {
                        UdpEvent.Set();
                        continueRec = false;
                    }
                }

                if (continueRec)
                {
                    StartUdpListen();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public bool IcomAllocateDevice(string deviceIp, bool allocate, CancellationTokenSource cts, IcomAllocateDeviceDelegate handler)
        {
            try
            {
                if (SharedDataActive.IcomAllocateActive)
                {
                    return false;
                }

                if (handler == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(deviceIp))
                {
                    return false;
                }

                string[] ipParts = deviceIp.Split(':');
                if (ipParts.Length == 0)
                {
                    return false;
                }

                if (!IPAddress.TryParse(ipParts[0], out IPAddress clientIp))
                {
                    return false;
                }

                if (SharedDataActive.IcomAllocateDeviceHttpClient == null)
                {
                    SharedDataActive.IcomAllocateDeviceHttpClient = new HttpClient(new HttpClientHandler());
                }

                MultipartFormDataContent formAllocate = new MultipartFormDataContent();
                string xmlHeader =
                    "<?xml version='1.0'?><!DOCTYPE wddxPacket SYSTEM 'http://www.openwddx.org/downloads/dtd/wddx_dtd_10.txt'>" +
                    "<wddxPacket version='1.0'><header/><data><struct><var name='DeviceOwner'><string>" + IcomOwner + "</string></var>";
                string xmlFooter =
                    "</struct></data></wddxPacket>";
                if (allocate)
                {
                    StringContent actionContent = new StringContent("nvmAllocateDevice", Encoding.ASCII, "text/plain");
                    formAllocate.Add(actionContent, "FunctionName");

                    string xmlString = xmlHeader +
                                        "<var name='IfhClientIpAddr'><string>ANY_HOST</string></var>" +
                                        "<var name='IfhClientTcpPorts'><string>IP_PORT_ANY</string></var>" +
                                        xmlFooter;
                    StringContent xmlContent = new StringContent(xmlString, Encoding.GetEncoding("ISO-8859-1"), "application/octet-stream");
                    formAllocate.Add(xmlContent, "com.nubix.nvm.commands.Allocate", "com.nubix.nvm.commands.Allocate");
                }
                else
                {
                    StringContent actionContent = new StringContent("nvmReleaseDevice", Encoding.ASCII, "text/plain");
                    formAllocate.Add(actionContent, "FunctionName");

                    string xmlString = xmlHeader + xmlFooter;
                    StringContent xmlContent = new StringContent(xmlString, Encoding.GetEncoding("ISO-8859-1"), "application/octet-stream");
                    formAllocate.Add(xmlContent, "com.nubix.nvm.commands.Release", "com.nubix.nvm.commands.Release");
                }

                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                {
                    string deviceUrl = "http://" + ipParts[0] + ":5302/nVm";
                    System.Threading.Tasks.Task<HttpResponseMessage> taskAllocate = SharedDataActive.IcomAllocateDeviceHttpClient.PostAsync(deviceUrl, formAllocate, cts.Token);
                    SharedDataActive.IcomAllocateActive = true;
                    taskAllocate.ContinueWith((task) =>
                    {
                        SharedDataActive.IcomAllocateActive = false;
                        try
                        {
                            HttpResponseMessage responseAllocate = taskAllocate.Result;
                            bool success = responseAllocate.IsSuccessStatusCode;
                            responseAllocate.Content.Headers.ContentType.CharSet = "ISO-8859-1";
                            string allocateResult = responseAllocate.Content.ReadAsStringAsync().Result;

                            int statusCode = -1;
                            if (success)
                            {
                                if (!GetIcomAllocateStatus(allocateResult, out statusCode))
                                {
                                    success = false;
                                }
                            }

                            handler.Invoke(success, statusCode);
                        }
                        catch (Exception)
                        {
                            handler.Invoke(false);
                        }
                    }, cts.Token, System.Threading.Tasks.TaskContinuationOptions.None, System.Threading.Tasks.TaskScheduler.Default);
                }, clientIp, SharedDataActive.NetworkData);
            }
            catch (Exception)
            {
                SharedDataActive.IcomAllocateActive = false;
                return false;
            }

            return true;
        }

        private bool GetIcomAllocateStatus(string allocResultXml, out int statusCode)
        {
            statusCode = -1;

            try
            {
                if (string.IsNullOrEmpty(allocResultXml))
                {
                    return false;
                }

                XmlReaderSettings readerSettings = new XmlReaderSettings { XmlResolver = null, DtdProcessing = DtdProcessing.Ignore };
                XmlReader xmlReader = XmlReader.Create(new StringReader(allocResultXml), readerSettings);
                XDocument xmlDoc = XDocument.Load(xmlReader);
                if (xmlDoc.Root == null)
                {
                    return false;
                }

                XElement dataNode = xmlDoc.Root.Element("data");
                if (dataNode == null)
                {
                    return false;
                }

                foreach (XElement structNode1 in dataNode.Elements("struct"))
                {
                    foreach (XElement varNode1 in structNode1.Elements("var"))
                    {
                        XAttribute nameAttr1 = varNode1.Attribute("name");
                        string name1 = nameAttr1?.Value;
                        bool isStatus = false;
                        if (!string.IsNullOrEmpty(name1))
                        {
                            name1 = name1.Trim();
                            if (string.Compare(name1, "Status", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                isStatus = true;
                            }
                        }

                        if (isStatus)
                        {
                            foreach (XElement structNode2 in varNode1.Elements("struct"))
                            {
                                foreach (XElement varNode2 in structNode2.Elements("var"))
                                {
                                    XAttribute nameAttr2 = varNode2.Attribute("name");
                                    string name2 = nameAttr2?.Value;
                                    bool isCode = false;
                                    if (!string.IsNullOrEmpty(name2))
                                    {
                                        name2 = name2.Trim();
                                        if (string.Compare(name2, "code", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            isCode = true;
                                        }
                                    }

                                    if (isCode)
                                    {
                                        foreach (XElement numberNode1 in varNode2.Elements("number"))
                                        {
                                            if (Int32.TryParse(numberNode1.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 codeValue))
                                            {
                                                statusCode = codeValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static Dictionary<string, string> ExtractSvrLocItems(byte[] dataBuffer, int dataLength, int expectedId)
        {
            if ((dataLength >= 14) &&
                (dataBuffer[0] == 0x02) &&   // Version 2
                (dataBuffer[1] == 0x07))     // Attribute reply
            {
                int packetlength = (dataBuffer[2] << 16) | (dataBuffer[3] << 8) | dataBuffer[4];
                int flags = (dataBuffer[5] << 8) | dataBuffer[6];
                int nextExtOffset = (dataBuffer[7] << 16) | (dataBuffer[8] << 8) | dataBuffer[9];
                int xId = (dataBuffer[10] << 8) | dataBuffer[11];
                int langLen = (dataBuffer[12] << 8) | dataBuffer[13];
                if (packetlength == dataLength && flags == 0 && nextExtOffset == 0 && xId == expectedId)
                {
                    try
                    {
                        int attrOffset = 14 + langLen + 2;  // lang + error code
                        int attrLen = (dataBuffer[attrOffset] << 8) | dataBuffer[attrOffset + 1];
                        if (attrOffset + 2 + attrLen < dataLength)
                        {
                            byte[] attrBytes = new byte[attrLen];
                            Array.Copy(dataBuffer, attrOffset + 2, attrBytes, 0, attrLen);
                            string attrText = Encoding.ASCII.GetString(attrBytes);
                            string[] attrList = attrText.Split(',');
                            Dictionary<string, string> attrDict = new Dictionary<string, string>();
                            foreach (string attrib in attrList)
                            {
                                string trimmed = attrib.Trim();
                                if (trimmed.StartsWith("(") && trimmed.EndsWith(")"))
                                {
                                    string attrRaw = trimmed.Substring(1, trimmed.Length - 2);
                                    string[] attrPair = attrRaw.Split('=');
                                    if (attrPair.Length == 2)
                                    {
                                        string key = attrPair[0].ToUpperInvariant();
                                        if (!attrDict.ContainsKey(key))
                                        {
                                            attrDict.Add(key, attrPair[1]);
                                        }
                                    }
                                }
                            }

                            return attrDict;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return null;
        }

        protected static void TcpControlTimerStop(SharedData sharedData)
        {
            sharedData.TcpControlTimerEnabled = false;
            sharedData.TcpControlTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected static void TcpControlTimerStart(SharedData sharedData)
        {
            sharedData.TcpControlTimerEnabled = true;
            sharedData.TcpControlTimer.Change(2000, Timeout.Infinite);
        }

        protected static void TcpControlTimeout(Object stateInfo)
        {
            if (stateInfo is SharedData sharedData)
            {
                lock (sharedData.TcpControlTimerLock)
                {
                    if (sharedData.TcpControlTimerEnabled)
                    {
                        TcpControlDisconnect(sharedData);
                    }
                }
            }
        }

        protected bool TcpControlConnect()
        {
            if (SharedDataActive.TcpControlClient != null)
            {
                return true;
            }
            if (SharedDataActive.EnetHostConn == null)
            {
                return false;
            }
            try
            {
                int controlPort = ControlPort;
                if (SharedDataActive.EnetHostConn.ControlPort >= 0)
                {
                    controlPort = SharedDataActive.EnetHostConn.ControlPort;
                }

                EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TcpControlConnect: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress.ToString(), controlPort);
                lock (SharedDataActive.TcpControlTimerLock)
                {
                    TcpControlTimerStop(SharedDataActive);
                }
                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                {
                    SharedDataActive.TcpControlClient = SharedDataActive.TcpDiagClient = new TcpClientWithTimeout(SharedDataActive.EnetHostConn.IpAddress, controlPort, ConnectTimeout, true).
                        Connect(() => SharedDataActive.TransmitCancelEvent.WaitOne(0, false));
                }, SharedDataActive.EnetHostConn.IpAddress, SharedDataActive.NetworkData);

                SharedDataActive.TcpControlClient.SendBufferSize = TcpSendBufferSize;
                SharedDataActive.TcpControlClient.NoDelay = true;
                SharedDataActive.TcpControlStream = SharedDataActive.TcpControlClient.GetStream();
            }
            catch (Exception ex)
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "TcpControlConnect exception: " + EdiabasNet.GetExceptionText(ex));
                TcpControlDisconnect(SharedDataActive);
                return false;
            }
            return true;
        }

        protected static bool TcpControlDisconnect(SharedData sharedData)
        {
            bool result = true;
            TcpControlTimerStop(sharedData);
            try
            {
                if (sharedData.TcpControlStream != null)
                {
                    sharedData.TcpControlStream.Close();
                    sharedData.TcpControlStream = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            try
            {
                if (sharedData.TcpControlClient != null)
                {
                    sharedData.TcpControlClient.Close();
                    sharedData.TcpControlClient = null;
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
            NetworkStream localStream = SharedDataActive.TcpDiagStream;
            if (localStream == null)
            {
                return false;
            }
            try
            {
                localStream.BeginRead(SharedDataActive.TcpDiagBuffer, SharedDataActive.TcpDiagRecLen, telLength - SharedDataActive.TcpDiagRecLen, TcpDiagReceiver, SharedDataActive.TcpDiagStream);
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
                NetworkStream networkStream = SharedDataActive.TcpDiagStream;
                if (networkStream == null)
                {
                    return;
                }
                if (SharedDataActive.TcpDiagRecLen > 0)
                {
                    if ((Stopwatch.GetTimestamp() - SharedDataActive.LastTcpDiagRecTime) > 300 * TickResolMs)
                    {   // pending telegram parts too late
                        SharedDataActive.TcpDiagRecLen = 0;
                    }
                }
                int recLen = networkStream.EndRead(ar);
                if (recLen > 0)
                {
                    SharedDataActive.LastTcpDiagRecTime = Stopwatch.GetTimestamp();
                    SharedDataActive.TcpDiagRecLen += recLen;
                }
                int nextReadLength = 6;
                if (SharedDataActive.TcpDiagRecLen >= 6)
                {   // header received
                    long telLen = (((long)SharedDataActive.TcpDiagBuffer[0] << 24) | ((long)SharedDataActive.TcpDiagBuffer[1] << 16) | ((long)SharedDataActive.TcpDiagBuffer[2] << 8) | SharedDataActive.TcpDiagBuffer[3]) + 6;
                    if (SharedDataActive.TcpDiagRecLen == telLen)
                    {   // telegram received
                        switch (SharedDataActive.TcpDiagBuffer[5])
                        {
                            case 0x01:  // diag data
                            case 0x02:  // ack
                            case 0xFF:  // nack
                                lock (SharedDataActive.TcpDiagStreamRecLock)
                                {
                                    if (SharedDataActive.TcpDiagRecQueue.Count > 256)
                                    {
                                        SharedDataActive.TcpDiagRecQueue.Dequeue();
                                    }
                                    byte[] recTelTemp = new byte[telLen];
                                    Array.Copy(SharedDataActive.TcpDiagBuffer, recTelTemp, SharedDataActive.TcpDiagRecLen);
                                    SharedDataActive.TcpDiagRecQueue.Enqueue(recTelTemp);
                                    SharedDataActive.TcpDiagStreamRecEvent.Set();
                                }
                                break;

                            case 0x12:  // alive check
                                SharedDataActive.TcpDiagBuffer[0] = 0x00;
                                SharedDataActive.TcpDiagBuffer[1] = 0x00;
                                SharedDataActive.TcpDiagBuffer[2] = 0x00;
                                SharedDataActive.TcpDiagBuffer[3] = 0x02;
                                SharedDataActive.TcpDiagBuffer[4] = 0x00;
                                SharedDataActive.TcpDiagBuffer[5] = 0x13;    // alive check response
                                SharedDataActive.TcpDiagBuffer[6] = 0x00;
                                SharedDataActive.TcpDiagBuffer[7] = (byte)TesterAddress;
                                lock (SharedDataActive.TcpDiagStreamSendLock)
                                {
                                    networkStream.Write(SharedDataActive.TcpDiagBuffer, 0, 8);
                                }
                                break;

                            default:
                                EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen, "*** Ignoring unknown telegram type");
                                break;
                        }
                        SharedDataActive.TcpDiagRecLen = 0;
                    }
                    else if (SharedDataActive.TcpDiagRecLen > telLen)
                    {
                        SharedDataActive.TcpDiagRecLen = 0;
                    }
                    else if (telLen > SharedDataActive.TcpDiagBuffer.Length)
                    {   // telegram too large -> remove all
                        while (SharedDataActive.TcpDiagStream.DataAvailable)
                        {
                            SharedDataActive.TcpDiagStream.ReadByte();
                        }
                        SharedDataActive.TcpDiagRecLen = 0;
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
                SharedDataActive.TcpDiagRecLen = 0;
                StartReadTcpDiag(6);
            }
        }

        protected bool SendData(byte[] sendData, int length, bool enableLogging)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return false;
            }
            try
            {
                lock (SharedDataActive.TcpDiagStreamRecLock)
                {
                    SharedDataActive.TcpDiagStreamRecEvent.Reset();
                    SharedDataActive.TcpDiagRecQueue.Clear();
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
                lock (SharedDataActive.TcpDiagStreamSendLock)
                {
                    WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                }

                // wait for ack
                int recLen = ReceiveAck(AckBuffer, ConnectTimeout + TcpAckTimeout, enableLogging);
                if (recLen < 0)
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No ack received");
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(true))
                    {
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        SharedDataActive.ReconnectRequired = true;
                        return false;
                    }

                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected: resending");
                    lock (SharedDataActive.TcpDiagStreamSendLock)
                    {
                        WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                    }
                    recLen = ReceiveAck(AckBuffer, ConnectTimeout + TcpAckTimeout, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No resend ack received");
                        return false;
                    }
                }

                if ((recLen == 6) && (AckBuffer[5] == 0xFF))
                {
                    if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Nack received: resending");
                    lock (SharedDataActive.TcpDiagStreamSendLock)
                    {
                        WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                    }
                    recLen = ReceiveAck(AckBuffer, ConnectTimeout + TcpAckTimeout, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No resend ack received");
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
                        if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack data not matching");
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
            if (SharedDataActive.TcpDiagStream == null)
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
                if (DataBuffer[5] != 0x01)
                {
                    if (EdiabasProtected != null)
                    {
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid data telegram type: {0:X02}", DataBuffer[5]);
                    }
                    return false;
                }
                // ReSharper disable RedundantCast
                int dataLen = (((int)DataBuffer[0] << 24) | ((int)DataBuffer[1] << 16) | ((int)DataBuffer[2] << 8) | DataBuffer[3]) - 2;
                // ReSharper restore RedundantCast
                if ((dataLen < 1) || ((dataLen + 8) > recLen) || (dataLen > 0xFFFF))
                {
                    if (EdiabasProtected != null)
                    {
                        EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid data length: {0}", dataLen);
                    }
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

            IncResponseCount(1);
            return true;
        }

        protected int ReceiveTelegram(byte[] receiveData, int timeout)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return -1;
            }

            if (SharedDataActive.TransmitCancelEvent.WaitOne(0, false))
            {
                return -1;
            }

            int recLen;
            try
            {
                int recTels;
                lock (SharedDataActive.TcpDiagStreamRecLock)
                {
                    recTels = SharedDataActive.TcpDiagRecQueue.Count;
                    if (recTels == 0)
                    {
                        SharedDataActive.TcpDiagStreamRecEvent.Reset();
                    }
                }

                if (recTels == 0)
                {
                    if (WaitHandle.WaitAny(new WaitHandle[] { SharedDataActive.TcpDiagStreamRecEvent, SharedDataActive.TransmitCancelEvent }, timeout, false) != 0)
                    {
                        return -1;
                    }
                }

                lock (SharedDataActive.TcpDiagStreamRecLock)
                {
                    if (SharedDataActive.TcpDiagRecQueue.Count > 0)
                    {
                        byte[] recTelFirst = SharedDataActive.TcpDiagRecQueue.Dequeue();
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
                if (enableLogging) EdiabasProtected.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLen, "*** Ack or nack expected");
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
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            if (ParTransmitFunc == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
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
                if (SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.Icom)
                {
                    timeout += AddRecTimeoutIcom;
                }
                else
                {
                    timeout += AddRecTimeout;
                }

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
            receiveData[receiveLength - 1] = 0;
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

            if (telLength > dataBuffer.Length)
            {
                telLength = dataBuffer.Length;
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

        private void SetDefaultCommParameter()
        {
            ParTransmitFunc = TransBmwFast;
            ParTimeoutStd = 1200;
            ParTimeoutTelEnd = 10;
            ParInterbyteTime = 0;
            ParRegenTime = 0;
            ParTimeoutNr78 = 5000;
            ParRetryNr78 = 2;
        }

        private void WriteNetworkStream(NetworkStream networkStream, byte[] buffer, int offset, int size, int packetSize = TcpSendBufferSize)
        {
            int pos = 0;
            while (pos < size)
            {
                int length = size;
                if (packetSize > 0)
                {
                    length = packetSize;
                }

                if (length > size - pos)
                {
                    length = size - pos;
                }

                try
                {
                    networkStream.Write(buffer, offset + pos, length);
                    pos += length;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("WriteNetworkStream exception: {0}", EdiabasNet.GetExceptionText(ex));
                    throw;
                }
            }
        }

        private bool IsIpv4Address(string address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                string[] ipParts = address.Split('.');
                if (ipParts.Length == 4)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                InterfaceDisconnect();
                if (SharedDataActive.IcomAllocateDeviceHttpClient != null)
                {
                    try
                    {
                        SharedDataActive.IcomAllocateDeviceHttpClient.Dispose();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    SharedDataActive.IcomAllocateDeviceHttpClient = null;
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (SharedDataActive != null)
                    {
                        NonSharedData.Dispose();
                        NonSharedData = null;
                    }
                }

                InterfaceUnlock();
                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
