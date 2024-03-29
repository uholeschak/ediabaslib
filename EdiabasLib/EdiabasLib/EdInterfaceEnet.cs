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
        public enum CommunicationMode
        {
            Hsfz,
            DoIp,
        }

        protected enum DoIpRoutingState
        {
            None,
            Requested,
            Accepted,
            Rejected
        }

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
                DirectHsfz,
                DirectDoIp,
                Enet,
                Icom
            }

            public EnetConnection(InterfaceType connectionType, IPAddress ipAddress, int diagPort = -1, int controlPort = -1, int doIpPort = -1)
            {
                ConnectionType = connectionType;
                IpAddress = ipAddress;
                DiagPort = diagPort;
                ControlPort = controlPort;
                DoIpPort = doIpPort;
                Mac = string.Empty;
                Vin = string.Empty;
            }

            public InterfaceType ConnectionType { get; }
            public IPAddress IpAddress { get;}
            public int DiagPort { get; }
            public int ControlPort { get; }
            public int DoIpPort { get; }
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
                if (ConnectionType == InterfaceType.DirectDoIp)
                {
                    sb.Append(":");
                    sb.Append(ProtocolDoIp);

                    if (DoIpPort >= 0)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, ":{0}", DoIpPort));
                    }
                }
                else
                {
                    sb.Append(":");
                    sb.Append(ProtocolHsfz);

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
            public TcpClient TcpDiagClient;
            public NetworkStream TcpDiagStream;
            public bool DiagDoIp;
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
            public DoIpRoutingState DoIpRoutingState;
        }

        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        protected delegate void ExecuteNetworkDelegate();
        public delegate void IcomAllocateDeviceDelegate(bool success, int statusCode = -1);

        private bool _disposed;
        private static Mutex _interfaceMutex;
        public const int MaxAckLength = 13;
        public const int MaxDoIpAckLength = 5;
        public const int DoIpProtoVer = 0x03;
        public const int DoIpGwAddr = 0x0010;
        public const string AutoIp = "auto";
        public const string AutoIpAll = ":all";
        public const string ProtocolHsfz = "HSFZ";
        public const string ProtocolDoIp = "DoIP";
        protected const string MutexName = "EdiabasLib_InterfaceEnet";
        protected const int TransBufferSize = 0x10010; // transmit buffer size
        protected const int TcpConnectTimeoutMin = 1000;
        protected const int TcpEnetAckTimeout = 5000;
        protected const int TcpDoIpMaxRetries = 2;
        protected const int TcpSendBufferSize = 1400;
        protected const int UdpDetectRetries = 3;
        protected const string IniFileSection = "XEthernet";
        protected const string IcomOwner = "DeepObd";
        protected static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
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
        protected static readonly byte[] UdpDoIpIdentReq =
        {
            DoIpProtoVer, ~DoIpProtoVer & 0xFF,
            0x00, 0x01,                     // ident request
            0x00, 0x00, 0x00, 0x00          // payload length
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
        protected AutoResetEvent UdpEvent;
        protected AutoResetEvent IcomEvent;

        protected string RemoteHostProtected = AutoIp;
        protected string VehicleProtocolProtected = ProtocolHsfz + "," + ProtocolDoIp;
        protected int TesterAddress = 0xF4;
        protected int DoIpTesterAddress = 0x0EF3;
        protected string HostIdentServiceProtected = "255.255.255.255";
        protected int UdpIdentPort = 6811;
        protected int UdpSrvLocPort = 427;
        protected int ControlPort = 6811;
        protected int DiagnosticPort = 6801;
        protected int DoIpPort = 13400;
        protected int ConnectTimeout = 5000;
        protected int DoIpTimeoutAcknowledge = 25000;
        protected int AddRecTimeoutProtected = 1000;
        protected int AddRecTimeoutIcomProtected = 2000;
        protected bool IcomAllocateProtected = false;
        protected HttpClient IcomAllocateDeviceHttpClient;

        protected byte[] RecBuffer = new byte[TransBufferSize];
        protected byte[] DataBuffer = new byte[TransBufferSize];
        protected byte[] AckBuffer = new byte[TransBufferSize];
        protected byte[] RoutingBuffer = new byte[8 + 11];
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

                string prop = EdiabasProtected?.GetConfigProperty("EnetRemoteHost");
                if (prop != null)
                {
                    RemoteHostProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("RemoteHost");
                if (prop != null)
                {
                    RemoteHostProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetVehicleProtocol");
                if (prop != null)
                {
                    VehicleProtocolProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("VehicleProtocol");
                if (prop != null)
                {
                    VehicleProtocolProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetHostIdentService");
                if (prop != null)
                {
                    HostIdentServiceProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("HostIdentService");
                if (prop != null)
                {
                    HostIdentServiceProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetTesterAddress");
                if (prop != null)
                {
                    TesterAddress = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDoIPTesterAddress");
                if (prop != null)
                {
                    DoIpTesterAddress = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetControlPort");
                if (prop != null)
                {
                    ControlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("ControlPort");
                if (prop != null)
                {
                    ControlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDiagnosticPort");
                if (prop != null)
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("DiagnosticPort");
                if (prop != null)
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDoIPPort");
                if (prop != null)
                {
                    DoIpPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("PortDoIP");
                if (prop != null)
                {
                    DoIpPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetTimeoutConnect");
                if (prop != null)
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("TimeoutConnect");
                if (prop != null)
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetTimeoutAcknowledge");
                if (prop != null)
                {
                    DoIpTimeoutAcknowledge = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("TimeoutAcknowledge");
                if (prop != null)
                {
                    DoIpTimeoutAcknowledge = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetAddRecTimeout");
                if (prop != null)
                {
                    AddRecTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetAddRecTimeoutIcom");
                if (prop != null)
                {
                    AddRecTimeoutIcom = (int)EdiabasNet.StringToValue(prop);
                }
#if Android
                IcomAllocate = true;
#else
                IcomAllocate = false;
#endif
                prop = EdiabasProtected?.GetConfigProperty("EnetIcomAllocate");
                if (prop != null)
                {
                    IcomAllocate = EdiabasNet.StringToValue(prop) != 0;
                }

                if (!IsIpv4Address(RemoteHostProtected))
                {
                    string iniFile = EdiabasProtected?.IniFileName;
                    if (!string.IsNullOrEmpty(iniFile))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using ENET ini file at: {0}", iniFile);
                        IniFile ediabasIni = new IniFile(iniFile);
                        string iniRemoteHost = ediabasIni.GetValue(IniFileSection, "RemoteHost", string.Empty);
                        bool hostValid = false;
                        if (IsIpv4Address(iniRemoteHost))
                        {
                            hostValid = true;
                            RemoteHostProtected = iniRemoteHost;
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using remote host from ini file: {0}", RemoteHostProtected);
                        }

                        if (hostValid)
                        {
                            string iniControlPort = ediabasIni.GetValue(IniFileSection, "ControlPort", string.Empty);
                            if (!string.IsNullOrEmpty(iniControlPort))
                            {
                                ControlPort = (int)EdiabasNet.StringToValue(iniControlPort);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using control port from ini file: {0}", ControlPort);
                            }

                            string iniDiagnosticPort = ediabasIni.GetValue(IniFileSection, "DiagnosticPort", string.Empty);
                            if (!string.IsNullOrEmpty(iniDiagnosticPort))
                            {
                                DiagnosticPort = (int)EdiabasNet.StringToValue(iniDiagnosticPort);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using diagnostic port from ini file: {0}", DiagnosticPort);
                            }

                            string iniPortDoIP = ediabasIni.GetValue(IniFileSection, "PortDoIP", string.Empty);
                            if (!string.IsNullOrEmpty(iniPortDoIP))
                            {
                                DoIpPort = (int)EdiabasNet.StringToValue(iniPortDoIP);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using DoIp port from ini file: {0}", DoIpPort);
                            }

                            string iniVehicleProtocol = ediabasIni.GetValue(IniFileSection, "VehicleProtocol", string.Empty);
                            if (!string.IsNullOrEmpty(iniVehicleProtocol))
                            {
                                VehicleProtocol = iniVehicleProtocol;
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using vehicle protocol from ini file: {0}", VehicleProtocol);
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
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
                    return;
                }

                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, CommParameterProtected, 0, CommParameterProtected.Length,
                    string.Format(Culture, "{0} CommParameter Host={1}, Tester=0x{2:X02}, ControlPort={3}, DiagPort={4}",
                            InterfaceName, RemoteHostProtected, TesterAddress, ControlPort, DiagnosticPort));

                uint concept = CommParameterProtected[0];
                switch (concept)
                {
                    case 0x010F:    // BMW-FAST
                        if (CommParameterProtected.Length < 7)
                        {
                            EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
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
                            EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0041);
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
                        EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0014);
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
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
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
                        EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                }
                if (!Connected)
                {
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
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
                        EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    WriteNetworkStream(SharedDataActive.TcpControlStream, TcpControlIgnitReq, 0, TcpControlIgnitReq.Length);
                    int recLen = SharedDataActive.TcpControlStream.ReadBytesAsync(RecBuffer, 0, 7, SharedDataActive.TransmitCancelEvent, 1000);
                    if (recLen < 7)
                    {
                        EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    if (RecBuffer[5] != 0x10)
                    {   // no clamp state response
                        EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                        return Int64.MinValue;
                    }
                    if ((RecBuffer[6] & 0x0C) == 0x04)
                    {   // ignition on
                        return 12000;
                    }
                }
                catch (Exception)
                {
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
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
#if Android
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

            UdpEvent = new AutoResetEvent(false);
            IcomEvent = new AutoResetEvent(false);
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
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connect to: {0}", RemoteHostProtected);
                SharedDataActive.NetworkData = null;
                SharedDataActive.TransmitCancelEvent.Reset();
#if Android
                if (ConnectParameter is ConnectParameterType connectParameter)
                {
                    SharedDataActive.NetworkData = connectParameter.NetworkData;
                }
#endif
                string[] protocolParts = VehicleProtocolProtected.Split(',');
                if (protocolParts.Length < 1)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle protocol: {0}", VehicleProtocolProtected);
                    return false;
                }

                List<CommunicationMode> communicationModes = new List<CommunicationMode>();
                if (reconnect)
                {
                    if (SharedDataActive.DiagDoIp)
                    {
                        communicationModes.Add(CommunicationMode.DoIp);
                    }
                    else
                    {
                        communicationModes.Add(CommunicationMode.Hsfz);
                    }
                }
                else
                {
                    foreach (string protocolPart in protocolParts)
                    {
                        string protocolPartTrim = protocolPart.Trim();
                        if (string.Compare(protocolPartTrim, ProtocolHsfz, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            communicationModes.Add(CommunicationMode.Hsfz);
                        }
                        if (string.Compare(protocolPartTrim, ProtocolDoIp, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            communicationModes.Add(CommunicationMode.DoIp);
                        }
                    }
                }

                SharedDataActive.EnetHostConn = null;
                if (RemoteHostProtected.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
                {
                    List<EnetConnection> detectedVehicles = DetectedVehicles(RemoteHostProtected, 1, UdpDetectRetries, communicationModes);
                    if ((detectedVehicles == null) || (detectedVehicles.Count < 1))
                    {
                        return false;
                    }
                    SharedDataActive.EnetHostConn = detectedVehicles[0];
                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Received: IP={0}:{1}, Type={2}",
                        SharedDataActive.EnetHostConn.IpAddress, SharedDataActive.EnetHostConn.DiagPort, SharedDataActive.EnetHostConn.ConnectionType));
                }
                else
                {
                    string[] hostParts = RemoteHostProtected.Split(':');
                    if (hostParts.Length < 1)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Host name invalid: {0}", RemoteHostProtected);
                        return false;
                    }

                    string hostIp = hostParts[0];
                    int hostPos = 1;
                    int hostDiagPort = -1;
                    int hostControlPort = -1;
                    int hostDoIpPort = -1;
                    EnetConnection.InterfaceType connectionType = EnetConnection.InterfaceType.DirectHsfz;
                    bool protocolSpecified = false;

                    if (hostParts.Length >= hostPos + 1)
                    {
                        protocolSpecified = true;
                        if (string.Compare(hostParts[hostPos], ProtocolHsfz, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            hostPos++;
                            connectionType = EnetConnection.InterfaceType.DirectHsfz;
                        }
                        else if (string.Compare(hostParts[hostPos], ProtocolDoIp, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            hostPos++;
                            connectionType = EnetConnection.InterfaceType.DirectDoIp;
                        }
                    }

                    if (connectionType == EnetConnection.InterfaceType.DirectHsfz)
                    {
                        if (protocolSpecified && !reconnect)
                        {   // protocol explicit specified
                            communicationModes.Clear();
                            communicationModes.Add(CommunicationMode.Hsfz);
                        }

                        if (hostParts.Length >= hostPos + 1)
                        {
                            Int64 portValue = EdiabasNet.StringToValue(hostParts[hostPos], out bool valid);
                            hostPos++;

                            if (valid)
                            {
                                hostDiagPort = (int)portValue;
                                connectionType = EnetConnection.InterfaceType.Icom;
                            }
                        }

                        if (hostParts.Length >= hostPos + 1)
                        {
                            Int64 portValue = EdiabasNet.StringToValue(hostParts[hostPos], out bool valid);
                            hostPos++;

                            if (valid)
                            {
                                hostControlPort = (int)portValue;
                                connectionType = EnetConnection.InterfaceType.Icom;
                            }
                        }
                    }
                    else
                    {
                        if (protocolSpecified && !reconnect)
                        {   // protocol explicit specified
                            communicationModes.Clear();
                            communicationModes.Add(CommunicationMode.DoIp);
                        }

                        if (hostParts.Length >= hostPos + 1)
                        {
                            Int64 portValue = EdiabasNet.StringToValue(hostParts[hostPos], out bool valid);
                            hostPos++;

                            if (valid)
                            {
                                hostDoIpPort = (int)portValue;
                            }
                        }
                    }

                    SharedDataActive.EnetHostConn = new EnetConnection(connectionType, IPAddress.Parse(hostIp), hostDiagPort, hostControlPort, hostDoIpPort);
                }

                int diagPort;
                if (communicationModes.Contains(CommunicationMode.DoIp))
                {
                    if (SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.Icom)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Disable DoIp for ICOM");
                        communicationModes.Remove(CommunicationMode.DoIp);
                    }
                }

                if (communicationModes.Count == 0)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No valid vehicle protocol specified: {0}", VehicleProtocolProtected);
                    return false;
                }

                EnetConnection enetHostConn = SharedDataActive.EnetHostConn;
                foreach (CommunicationMode communicationMode in communicationModes)
                {
                    SharedDataActive.EnetHostConn = enetHostConn;
                    SharedDataActive.DiagDoIp = communicationMode == CommunicationMode.DoIp;

                    if (SharedDataActive.DiagDoIp)
                    {
                        diagPort = DoIpPort;
                        if (SharedDataActive.EnetHostConn.DoIpPort >= 0)
                        {
                            diagPort = SharedDataActive.EnetHostConn.DoIpPort;
                        }
                    }
                    else
                    {
                        diagPort = SharedDataActive.DiagDoIp ? DoIpPort : DiagnosticPort;
                        if (SharedDataActive.EnetHostConn.DiagPort >= 0)
                        {
                            diagPort = SharedDataActive.EnetHostConn.DiagPort;
                        }

                        if (IcomAllocate && !reconnect && SharedDataActive.EnetHostConn.DiagPort >= 0)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM at: {0}", SharedDataActive.EnetHostConn.IpAddress);
                            IcomEvent.Reset();
                            using (CancellationTokenSource cts = new CancellationTokenSource())
                            {
                                if (!IcomAllocateDevice(SharedDataActive.EnetHostConn.IpAddress.ToString(), true, cts, (success, code) =>
                                {
                                    if (success && code == 0)
                                    {
                                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM ok: Code={0}", code);
                                    }
                                    else
                                    {
                                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM rejected: Code={0}", code);
                                    }

                                    IcomEvent.Set();
                                }))
                                {
                                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM error");
                                }

                                int waitResult = WaitHandle.WaitAny(new WaitHandle[] { IcomEvent, SharedDataActive.TransmitCancelEvent }, 2000);
                                if (waitResult != 0)
                                {
                                    if (waitResult == WaitHandle.WaitTimeout)
                                    {
                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM timeout");
                                    }
                                    else
                                    {
                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM cancelled");
                                    }
                                    cts.Cancel();
                                    if (!IcomEvent.WaitOne(1000))
                                    {
                                        // reset allocate active after timeout
                                        SharedDataActive.IcomAllocateActive = false;
                                    }
                                }
                            }
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM finished");
                        }
                    }

                    try
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connecting to: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress, diagPort);
                        TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                        {
                            SharedDataActive.TcpDiagClient = new TcpClientWithTimeout(SharedDataActive.EnetHostConn.IpAddress, diagPort, ConnectTimeout, true)
                                .Connect(SharedDataActive.TransmitCancelEvent);
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

                        int readLen = SharedDataActive.DiagDoIp ? 8 : 6;
                        StartReadTcpDiag(readLen);
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connected to: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress.ToString(), diagPort);
                        SharedDataActive.ReconnectRequired = false;
                        SharedDataActive.DoIpRoutingState = DoIpRoutingState.None;

                        if (SharedDataActive.DiagDoIp)
                        {
                            if (!DoIpRoutingActivation(true))
                            {
                                InterfaceDisconnect(reconnect);
                                continue;
                            }
                        }

                        if (Connected)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "InterfaceConnect exception: " + EdiabasNet.GetExceptionText(ex));
                        InterfaceDisconnect(reconnect);
                    }
                }

                if (!Connected)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "InterfaceConnect exception: " + EdiabasNet.GetExceptionText(ex));
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
            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Disconnect");
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
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM at: {0}", SharedDataActive.EnetHostConn.IpAddress);

                IcomEvent.Reset();
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    if (!IcomAllocateDevice(SharedDataActive.EnetHostConn.IpAddress.ToString(), false, cts, (success, code) =>
                    {
                        if (success)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM ok: Code={0}", code);
                        }
                        else
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM rejected: Code={0}", code);
                        }

                        IcomEvent.Set();
                    }))
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM error");
                    }

                    int waitResult = WaitHandle.WaitAny(new WaitHandle[] { IcomEvent, SharedDataActive.TransmitCancelEvent }, 2000);
                    if (waitResult != 0)
                    {
                        if (waitResult == WaitHandle.WaitTimeout)
                        {
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM timeout");
                        }
                        else
                        {
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM cancelled");
                        }

                        cts.Cancel();
                        if (!IcomEvent.WaitOne(1000))
                        {
                            // reset allocate active after timeout
                            SharedDataActive.IcomAllocateActive = false;
                        }
                    }
                }

                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM finished");
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
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Info, "TransmitData with default CommParameter");
                SetDefaultCommParameter();
            }

            byte[] cachedResponse;
            EdiabasNet.ErrorCodes cachedErrorCode;
            if (ReadCachedTransmission(sendData, out cachedResponse, out cachedErrorCode))
            {
                if (cachedErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected?.SetError(cachedErrorCode);
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
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
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
                EdiabasProtected?.SetError(errorCode);
                return false;
            }
            receiveData = new byte[recLength];
            Array.Copy(RecBuffer, receiveData, recLength);
            CacheTransmission(sendData, receiveData, EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE);
            return true;
        }

        public override bool TransmitFrequent(byte[] sendData)
        {
            EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
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

        public string VehicleProtocol
        {
            get
            {
                return VehicleProtocolProtected;
            }
            set
            {
                VehicleProtocolProtected = value;
            }
        }

        public string HostIdentService
        {
            get
            {
                return HostIdentServiceProtected;
            }
            set
            {
                HostIdentServiceProtected = value;
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

        public List<EnetConnection> DetectedVehicles(string remoteHostConfig, List<CommunicationMode> communicationModes = null)
        {
            return DetectedVehicles(remoteHostConfig, -1, UdpDetectRetries, communicationModes);
        }

        public List<EnetConnection> DetectedVehicles(string remoteHostConfig, int maxVehicles, int maxRetries, List<CommunicationMode> communicationModes)
        {
            if (!remoteHostConfig.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            bool protocolHsfz = true;
            bool protocolDoIp = true;
            if (communicationModes != null)
            {
                protocolHsfz = communicationModes.Contains(CommunicationMode.Hsfz);
                protocolDoIp = communicationModes.Contains(CommunicationMode.DoIp);
            }

            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("DetectedVehicles: HSFZ={0}, DoIp={1}",
                protocolHsfz, protocolDoIp));

            if (!protocolHsfz && !protocolDoIp)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "DetectedVehicles: No protocol specified");
                return null;
            }

            try
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UdpSocket.EnableBroadcast = true;
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
                    string configData = remoteHostConfig.Remove(0, AutoIp.Length);

                    if (!((configData.Length > 0) && (configData[0] == ':')))
                    {
                        if (IsIpv4Address(HostIdentServiceProtected))
                        {
                            if (IPAddress.TryParse(HostIdentServiceProtected, out IPAddress ipAddressHostIdent))
                            {
                                if (ipAddressHostIdent.Equals(IPAddress.Broadcast))
                                {
                                    configData = AutoIpAll;
                                }
                            }
                        }
                    }

                    if ((configData.Length > 0) && (configData[0] == ':'))
                    {
                        string adapterName = configData.StartsWith(AutoIpAll, StringComparison.OrdinalIgnoreCase) ? string.Empty : configData.Remove(0, 1);

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
                                                    if (protocolHsfz)
                                                    {
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
                                                    }

                                                    if (protocolDoIp)
                                                    {
                                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending: '{0}': DoIp ident broadcast={1} Port={2}",
                                                            netInterface.Name, broadcastAddress, DoIpPort));

                                                        IPEndPoint ipUdpDoIpIdent = new IPEndPoint(broadcastAddress, DoIpPort);
                                                        TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                                        {
                                                            UdpSocket.SendTo(UdpDoIpIdentReq, ipUdpDoIpIdent);
                                                        }, ipUdpDoIpIdent.Address, SharedDataActive.NetworkData);
                                                    }

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
                                                    if (protocolHsfz)
                                                    {
                                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending ident: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                            adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, UdpIdentPort));

                                                        IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, UdpIdentPort);
                                                        UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);

                                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending SvrLoc: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                            adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, UdpSrvLocPort));

                                                        IPEndPoint ipUdpSvrLoc = new IPEndPoint(broadcastAddress, UdpSrvLocPort);
                                                        UdpSocket.SendTo(UdpSvrLocReq, ipUdpSvrLoc);
                                                    }

                                                    if (protocolDoIp)
                                                    {
                                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending DoIp ident: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                            adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, DoIpPort));

                                                        IPEndPoint ipUdpDoIpIdent = new IPEndPoint(broadcastAddress, DoIpPort);
                                                        UdpSocket.SendTo(UdpDoIpIdentReq, ipUdpDoIpIdent);
                                                    }

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
                    {
                        try
                        {
                            if (protocolHsfz)
                            {
                                IPEndPoint ipUdpIdent = new IPEndPoint(IPAddress.Parse(HostIdentServiceProtected), UdpIdentPort);
                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending Ident broadcast to: {0}:{1}", ipUdpIdent.Address, UdpIdentPort));
                                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                {
                                    UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                }, ipUdpIdent.Address, SharedDataActive.NetworkData);

                                IPEndPoint ipUdpSvrLoc = new IPEndPoint(IPAddress.Parse(HostIdentServiceProtected), UdpSrvLocPort);
                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending SvrLoc broadcast to: {0}:{1}", ipUdpSvrLoc.Address, UdpSrvLocPort));
                                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                {
                                    UdpSocket.SendTo(UdpSvrLocReq, ipUdpSvrLoc);
                                }, ipUdpSvrLoc.Address, SharedDataActive.NetworkData);
                            }

                            if (protocolDoIp)
                            {
                                IPEndPoint ipUdpDoIpIdent = new IPEndPoint(IPAddress.Parse(HostIdentServiceProtected), DoIpPort);
                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending DoIp broadcast to: {0}:{1}", ipUdpDoIpIdent.Address, DoIpPort));
                                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                {
                                    UdpSocket.SendTo(UdpDoIpIdentReq, ipUdpDoIpIdent);
                                }, ipUdpDoIpIdent.Address, SharedDataActive.NetworkData);
                            }

                            broadcastSend = true;
                        }
                        catch (Exception)
                        {
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Broadcast failed");
                        }
                    }
                    if (!broadcastSend)
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "No broadcast send");
                        return null;
                    }

                    // DoIP has 500ms timeout (TimeoutConnect)
                    int waitResult = WaitHandle.WaitAny(new WaitHandle[] { UdpEvent, SharedDataActive.TransmitCancelEvent }, 1000);
                    if (waitResult == 1)
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "No broadcast cancelled");
                        return null;
                    }

                    if (UdpRecIpListList.Count == 0)
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "No answer received");
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
                        addListConn = new EnetConnection(EnetConnection.InterfaceType.DirectHsfz, recIp);
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
                else if (recPort == DoIpPort)
                {
                    int minPayloadLength = 17 + 2 + 6 + 6 + 2;
                    if ((recLen >= (8 + minPayloadLength)) &&
                        (UdpBuffer[0] == DoIpProtoVer) &&
                        (UdpBuffer[1] == (~DoIpProtoVer & 0xFF)) &&
                        (UdpBuffer[2] == 0x00) &&
                        (UdpBuffer[3] == 0x04))
                    {
                        long payloadLen = (((long)UdpBuffer[4] << 24) | ((long)UdpBuffer[5] << 16) | ((long)UdpBuffer[6] << 8) | UdpBuffer[7]);
                        uint gwAddr = (uint)((UdpBuffer[8 + 17 + 0] << 8) | UdpBuffer[8 + 17 + 1]);
                        if (payloadLen >= minPayloadLength && gwAddr == DoIpGwAddr)
                        {
                            addListConn = new EnetConnection(EnetConnection.InterfaceType.DirectDoIp, recIp);
                            try
                            {
                                vehicleVin = Encoding.ASCII.GetString(UdpBuffer, 8, 17);
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

                if (recLen <= 0)
                {
                    continueRec = false;
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

                if (IcomAllocateDeviceHttpClient == null)
                {
                    IcomAllocateDeviceHttpClient = new HttpClient(new HttpClientHandler()
                    {
                        UseProxy = false
                    });
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
                    System.Threading.Tasks.Task<HttpResponseMessage> taskAllocate = IcomAllocateDeviceHttpClient.PostAsync(deviceUrl, formAllocate, cts.Token);
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

                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TcpControlConnect: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress.ToString(), controlPort);
                lock (SharedDataActive.TcpControlTimerLock)
                {
                    TcpControlTimerStop(SharedDataActive);
                }
                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                {
                    SharedDataActive.TcpControlClient = SharedDataActive.TcpDiagClient = new TcpClientWithTimeout(SharedDataActive.EnetHostConn.IpAddress, controlPort, ConnectTimeout, true)
                        .Connect(SharedDataActive.TransmitCancelEvent);
                }, SharedDataActive.EnetHostConn.IpAddress, SharedDataActive.NetworkData);

                SharedDataActive.TcpControlClient.SendBufferSize = TcpSendBufferSize;
                SharedDataActive.TcpControlClient.NoDelay = true;
                SharedDataActive.TcpControlStream = SharedDataActive.TcpControlClient.GetStream();
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "TcpControlConnect exception: " + EdiabasNet.GetExceptionText(ex));
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

                int nextReadLength = SharedDataActive.DiagDoIp ? TcpDiagDoIpReceiver(networkStream) : TcpDiagEnetReceiver(networkStream);
                if (recLen > 0)
                {
                    StartReadTcpDiag(nextReadLength);
                }
            }
            catch (Exception)
            {
                SharedDataActive.TcpDiagRecLen = 0;
            }
        }

        protected int TcpDiagEnetReceiver(NetworkStream networkStream)
        {
            int nextReadLength = 6;
            try
            {
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
                                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                    "*** Ignoring unknown telegram type");
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
            }
            catch (Exception)
            {
                SharedDataActive.TcpDiagRecLen = 0;
            }

            return nextReadLength;
        }

        protected int TcpDiagDoIpReceiver(NetworkStream networkStream)
        {
            int nextReadLength = 8;
            try
            {
                if (SharedDataActive.TcpDiagRecLen >= 8)
                {   // header received
                    long telLen = (((long)SharedDataActive.TcpDiagBuffer[4] << 24) | ((long)SharedDataActive.TcpDiagBuffer[5] << 16) | ((long)SharedDataActive.TcpDiagBuffer[6] << 8) | SharedDataActive.TcpDiagBuffer[7]) + 8;
                    if (SharedDataActive.TcpDiagRecLen == telLen)
                    {   // telegram received
                        byte protoVersion = SharedDataActive.TcpDiagBuffer[0];
                        byte protoVersionInv = SharedDataActive.TcpDiagBuffer[1];
                        if (protoVersion != (byte)~protoVersionInv)
                        {
                            EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                "*** Protocol version invalid");
                            InterfaceDisconnect(true);
                            SharedDataActive.ReconnectRequired = true;
                            return nextReadLength;
                        }

                        uint payloadType = (((uint)SharedDataActive.TcpDiagBuffer[2] << 8) | SharedDataActive.TcpDiagBuffer[3]);
                        switch (payloadType)
                        {
                            case 0x0000:    // error response
                                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                    "*** Error response");
                                InterfaceDisconnect(true);
                                SharedDataActive.ReconnectRequired = true;
                                return nextReadLength;

                            case 0x0006: // routing activation response
                                if (telLen < 11)
                                {
                                    EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                        "*** DoIp routing response invalid");
                                    InterfaceDisconnect(true);
                                    SharedDataActive.ReconnectRequired = true;
                                    return nextReadLength;
                                }

                                if (SharedDataActive.TcpDiagBuffer[8] != (DoIpTesterAddress >> 8) ||
                                    SharedDataActive.TcpDiagBuffer[9] != (DoIpTesterAddress & 0xFF) ||
                                    SharedDataActive.TcpDiagBuffer[12] != 0x10) // ACK
                                {
                                    EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                        "*** DoIp routing rejected");
                                    lock (SharedDataActive.TcpDiagStreamRecLock)
                                    {
                                        SharedDataActive.DoIpRoutingState = DoIpRoutingState.Rejected;
                                        SharedDataActive.TcpDiagStreamRecEvent.Set();
                                    }
                                    break;
                                }

                                lock (SharedDataActive.TcpDiagStreamRecLock)
                                {
                                    SharedDataActive.DoIpRoutingState = DoIpRoutingState.Accepted;
                                    SharedDataActive.TcpDiagStreamRecEvent.Set();
                                }
                                break;

                            case 0x8001:    // diagostic message
                            case 0x8002:    // diagnostic message ack
                            case 0x8003:    // diagnostic message nack
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

                            case 0x0007:   // keep alive check
                                SharedDataActive.TcpDiagBuffer[0] = DoIpProtoVer;
                                SharedDataActive.TcpDiagBuffer[1] = ~DoIpProtoVer & 0xFF;
                                SharedDataActive.TcpDiagBuffer[2] = 0x00;    // alive check response
                                SharedDataActive.TcpDiagBuffer[3] = 0x08;
                                SharedDataActive.TcpDiagBuffer[4] = 0x00;    // payload length
                                SharedDataActive.TcpDiagBuffer[5] = 0x00;
                                SharedDataActive.TcpDiagBuffer[6] = 0x00;
                                SharedDataActive.TcpDiagBuffer[7] = 0x02;
                                SharedDataActive.TcpDiagBuffer[8] = (byte)(DoIpTesterAddress >> 8);
                                SharedDataActive.TcpDiagBuffer[9] = (byte) (DoIpTesterAddress & 0xFF);
                                lock (SharedDataActive.TcpDiagStreamSendLock)
                                {
                                    networkStream.Write(SharedDataActive.TcpDiagBuffer, 0, 10);
                                }
                                break;

                            default:
                                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen, 
                                    "*** Ignoring unknown telegram type");
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
            }
            catch (Exception)
            {
                SharedDataActive.TcpDiagRecLen = 0;
            }

            return nextReadLength;
        }

        protected bool SendEnetData(byte[] sendData, int length, bool enableLogging)
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
                if (sourceAddr == 0xF1)
                {
                    sourceAddr = (byte)TesterAddress;
                }

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
                int recLen = ReceiveEnetAck(AckBuffer, ConnectTimeout + TcpEnetAckTimeout, enableLogging);
                if (recLen < 0)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No ack received");
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(true))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        SharedDataActive.ReconnectRequired = true;
                        return false;
                    }

                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected: resending");
                    lock (SharedDataActive.TcpDiagStreamSendLock)
                    {
                        WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                    }
                    recLen = ReceiveEnetAck(AckBuffer, ConnectTimeout + TcpEnetAckTimeout, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No resend ack received");
                        return false;
                    }
                }

                if ((recLen == 6) && (AckBuffer[5] == 0xFF))
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Nack received: resending");
                    lock (SharedDataActive.TcpDiagStreamSendLock)
                    {
                        WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                    }
                    recLen = ReceiveEnetAck(AckBuffer, ConnectTimeout + TcpEnetAckTimeout, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No resend ack received");
                        return false;
                    }
                }

                if ((recLen < 6) || (recLen > sendLength) || (recLen > MaxAckLength) || (AckBuffer[5] != 0x02))
                {
                    if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack frame invalid");
                    return false;
                }

                AckBuffer[4] = DataBuffer[4];
                AckBuffer[5] = DataBuffer[5];
                for (int i = 4; i < recLen; i++)
                {
                    if (AckBuffer[i] != DataBuffer[i])
                    {
                        if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack data not matching");
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

        protected bool SendDoIpData(byte[] sendData, int length, bool enableLogging)
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

                if (SharedDataActive.DoIpRoutingState == DoIpRoutingState.None)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Routing activation required");
                    if (!DoIpRoutingActivation(enableLogging))
                    {
                        InterfaceDisconnect(true);
                        if (!InterfaceConnect(true))
                        {
                            if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                            SharedDataActive.ReconnectRequired = true;
                            return false;
                        }
                    }
                }

                uint targetAddr = sendData[1];
                uint sourceAddr = sendData[2];
                if (sourceAddr == 0xF1)
                {
                    sourceAddr = (uint)DoIpTesterAddress;
                }

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

                int payloadLength = dataLength + 4;
                DataBuffer[0] = DoIpProtoVer;
                DataBuffer[1] = ~DoIpProtoVer & 0xFF;
                DataBuffer[2] = 0x80;   // diagostic message
                DataBuffer[3] = 0x01;
                DataBuffer[4] = (byte)((payloadLength >> 24) & 0xFF);
                DataBuffer[5] = (byte)((payloadLength >> 16) & 0xFF);
                DataBuffer[6] = (byte)((payloadLength >> 8) & 0xFF);
                DataBuffer[7] = (byte)(payloadLength & 0xFF);
                DataBuffer[8] = (byte)(sourceAddr >> 8);
                DataBuffer[9] = (byte)sourceAddr;
                DataBuffer[10] = (byte)(targetAddr >> 8);
                DataBuffer[11] = (byte)targetAddr;
                Array.Copy(sendData, dataOffset, DataBuffer, 12, dataLength);
                int sendLength = dataLength + 12;
                lock (SharedDataActive.TcpDiagStreamSendLock)
                {
                    WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                }

                // wait for ack
                int recLen = ReceiveDoIpAck(AckBuffer, ConnectTimeout + DoIpTimeoutAcknowledge, enableLogging);
                if (recLen < 0)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No ack received");
                    if (!DoIpRoutingActivation(enableLogging))
                    {
                        InterfaceDisconnect(true);
                        if (!InterfaceConnect(true))
                        {
                            if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                            SharedDataActive.ReconnectRequired = true;
                            return false;
                        }
                    }

                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected: resending");
                    lock (SharedDataActive.TcpDiagStreamSendLock)
                    {
                        WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                    }
                    recLen = ReceiveDoIpAck(AckBuffer, ConnectTimeout + DoIpTimeoutAcknowledge, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No resend ack received");
                        return false;
                    }
                }

                uint payloadType = 0x0000;
                if (recLen >= 8)
                {
                    payloadType = (((uint)AckBuffer[2] << 8) | AckBuffer[3]);
                }

                if (payloadType == 0x8003)
                {   // NACK
                    if ((recLen < 13) || (AckBuffer[12] != 0x00))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Nack received: aborting");
                        InterfaceDisconnect(true);
                        SharedDataActive.ReconnectRequired = true;
                        return false;
                    }

                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Nack received: resending");
                    if (!DoIpRoutingActivation(enableLogging))
                    {
                        InterfaceDisconnect(true);
                        if (!InterfaceConnect(true))
                        {
                            if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                            SharedDataActive.ReconnectRequired = true;
                            return false;
                        }
                    }

                    lock (SharedDataActive.TcpDiagStreamSendLock)
                    {
                        WriteNetworkStream(SharedDataActive.TcpDiagStream, DataBuffer, 0, sendLength);
                    }

                    recLen = ReceiveDoIpAck(AckBuffer, ConnectTimeout + DoIpTimeoutAcknowledge, enableLogging);
                    if (recLen < 0)
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No resend ack received");
                        return false;
                    }

                    payloadType = 0x0000;
                    if (recLen >= 8)
                    {
                        payloadType = (((uint)AckBuffer[2] << 8) | AckBuffer[3]);
                    }
                }

                if (payloadType != 0x8002)
                {   // No Ack
                    if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** No Ack received");
                    return false;
                }

                if ((recLen < 13) || (recLen - 1 > sendLength) || (recLen - 13 > MaxDoIpAckLength) || (AckBuffer[12] != 0x00))
                {
                    if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack frame invalid");
                    return false;
                }

                if (AckBuffer[8] != DataBuffer[10] ||
                    AckBuffer[9] != DataBuffer[11] ||
                    AckBuffer[10] != DataBuffer[8] ||
                    AckBuffer[11] != DataBuffer[9])
                {
                    if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack address not matching");
                    return false;
                }

                for (int i = 13; i < recLen; i++)
                {
                    if (AckBuffer[i] != DataBuffer[i - 1])
                    {
                        if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AckBuffer, 0, recLen, "*** Ack data not matching");
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

        protected bool ReceiveEnetData(byte[] receiveData, int timeout)
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
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid data telegram type: {0:X02}", DataBuffer[5]);
                    return false;
                }
                // ReSharper disable RedundantCast
                int dataLen = (((int)DataBuffer[0] << 24) | ((int)DataBuffer[1] << 16) | ((int)DataBuffer[2] << 8) | DataBuffer[3]) - 2;
                // ReSharper restore RedundantCast
                if ((dataLen < 1) || ((dataLen + 8) > recLen) || (dataLen > 0xFFFF))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid data length: {0}", dataLen);
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
                    receiveData[5] = (byte)(dataLen & 0xFF);
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

        protected bool ReceiveDoIpData(byte[] receiveData, int timeout)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return false;
            }
            try
            {
                int recLen = ReceiveTelegram(DataBuffer, timeout);
                if (recLen < 8)
                {
                    return false;
                }

                uint payloadType = (((uint)DataBuffer[2] << 8) | DataBuffer[3]);
                if (payloadType != 0x8001)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid data payload type: {0:X04}", payloadType);
                    return false;
                }

                int payloadLen = (((int)DataBuffer[4] << 24) | ((int)DataBuffer[5] << 16) | ((int)DataBuffer[6] << 8) | DataBuffer[7]);
                int dataLen = payloadLen - 4;
                if ((dataLen < 1) || ((dataLen + 12) > recLen) || (dataLen > 0xFFFF))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid data length: {0}", dataLen);
                    return false;
                }
                // create BMW-FAST telegram
                byte sourceAddr = DataBuffer[9];
                byte targetAddr = 0xF1;
                int len;
                if (dataLen > 0xFF)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = 0x00;
                    receiveData[4] = (byte)(dataLen >> 8);
                    receiveData[5] = (byte)(dataLen & 0xFF);
                    Array.Copy(DataBuffer, 12, receiveData, 6, dataLen);
                    len = dataLen + 6;
                }
                else if (dataLen > 0x3F)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = (byte)dataLen;
                    Array.Copy(DataBuffer, 12, receiveData, 4, dataLen);
                    len = dataLen + 4;
                }
                else
                {
                    receiveData[0] = (byte)(0x80 | dataLen);
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    Array.Copy(DataBuffer, 12, receiveData, 3, dataLen);
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

            if (SharedDataActive.TransmitCancelEvent.WaitOne(0))
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
                    if (WaitHandle.WaitAny(new WaitHandle[] { SharedDataActive.TcpDiagStreamRecEvent, SharedDataActive.TransmitCancelEvent }, timeout) != 0)
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

        protected int ReceiveEnetAck(byte[] receiveData, int timeout, bool enableLogging)
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
                if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLen, "*** Ack or nack expected");
                if ((Stopwatch.GetTimestamp() - startTick) > timeout * TickResolMs)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Ack timeout");
                    return -1;
                }
            }
        }

        protected int ReceiveDoIpAck(byte[] receiveData, int timeout, bool enableLogging)
        {
            long startTick = Stopwatch.GetTimestamp();
            for (; ; )
            {
                int recLen = ReceiveTelegram(receiveData, timeout);
                if (recLen < 0)
                {
                    return recLen;
                }
                if (recLen >= 8)
                {
                    uint payloadType = (((uint)receiveData[2] << 8) | receiveData[3]);
                    if (payloadType == 0x8002 || payloadType == 0x8003)
                    {   // ACK or NACK received
                        return recLen;
                    }
                }
                if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLen, "*** Ack expected");
                if ((Stopwatch.GetTimestamp() - startTick) > timeout * TickResolMs)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Ack timeout");
                    return -1;
                }
            }
        }

        protected bool DoIpRoutingActivation(bool enableLogging)
        {
            for (int retry = 0; retry < TcpDoIpMaxRetries; retry++)
            {
                if (!SendDoIpRoutingRequest())
                {
                    if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Sending DoIp routing request failed");
                    return false;
                }

                if (WaitForDoIpRoutingResponse(ConnectTimeout + DoIpTimeoutAcknowledge, enableLogging))
                {
                    return true;
                }

                if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Receiving DoIp routing response failed");
            }

            return false;
        }

        protected bool SendDoIpRoutingRequest()
        {
            try
            {
                SharedDataActive.DoIpRoutingState = DoIpRoutingState.Requested;

                int payloadLength = 11;
                RoutingBuffer[0] = DoIpProtoVer;
                RoutingBuffer[1] = ~DoIpProtoVer & 0xFF;
                RoutingBuffer[2] = 0x00;   // routing activation request
                RoutingBuffer[3] = 0x05;
                RoutingBuffer[4] = (byte)((payloadLength >> 24) & 0xFF);
                RoutingBuffer[5] = (byte)((payloadLength >> 16) & 0xFF);
                RoutingBuffer[6] = (byte)((payloadLength >> 8) & 0xFF);
                RoutingBuffer[7] = (byte)(payloadLength & 0xFF);
                RoutingBuffer[8] = (byte)(DoIpTesterAddress >> 8);
                RoutingBuffer[9] = (byte)(DoIpTesterAddress & 0xFF);
                RoutingBuffer[10] = 0x00;  // activation type default
                Array.Clear(RoutingBuffer, 11, 8); // ISO and OEM reserved

                int sendLength = payloadLength + 8;
                lock (SharedDataActive.TcpDiagStreamSendLock)
                {
                    WriteNetworkStream(SharedDataActive.TcpDiagStream, RoutingBuffer, 0, sendLength);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected bool WaitForDoIpRoutingResponse(int timeout, bool enableLogging)
        {
            long startTick = Stopwatch.GetTimestamp();
            for (; ; )
            {
                if (SharedDataActive.TcpDiagStream == null)
                {
                    return false;
                }

                if (SharedDataActive.TransmitCancelEvent.WaitOne(0))
                {
                    return false;
                }

                DoIpRoutingState routingState;
                lock (SharedDataActive.TcpDiagStreamRecLock)
                {
                    routingState = SharedDataActive.DoIpRoutingState;
                }

                switch (routingState)
                {
                    case DoIpRoutingState.Requested:
                        break;

                    case DoIpRoutingState.Rejected:
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "DoIp routing response: Rejected");
                        return false;

                    case DoIpRoutingState.Accepted:
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "DoIp routing response: Accepted");
                        return true;

                    default:
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "DoIp routing state invalid");
                        return false;
                }

                if (WaitHandle.WaitAny(new WaitHandle[] { SharedDataActive.TcpDiagStreamRecEvent, SharedDataActive.TransmitCancelEvent }, timeout) != 0)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** DoIp routing event response timeout");
                    return false;
                }

                if ((Stopwatch.GetTimestamp() - startTick) > timeout * TickResolMs)
                {
                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** DoIp routing response timeout");
                    return false;
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
            string retryComm = EdiabasProtected?.GetConfigProperty("RetryComm");
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
                if (sendDataLength <= 0)
                {   // no data to send
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
                    if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) count={1}", deviceAddr, retries);
                    Nr78Dict.Add(deviceAddr, retries);
                }
                else
                {
                    if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NR78({0:X02}) exceeded", deviceAddr);
                }
            }
            else
            {
                if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) added", deviceAddr);
                Nr78Dict.Add(deviceAddr, 0);
            }
        }

        private void Nr78DictRemove(byte deviceAddr, bool enableLogging)
        {
            if (Nr78Dict.ContainsKey(deviceAddr))
            {
                if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NR78({0:X02}) removed", deviceAddr);
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
                if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, sendData, 0, sendLength, "Send");

                if (SharedDataActive.DiagDoIp)
                {
                    if (!SendDoIpData(sendData, sendLength, enableLogging))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                }
                else
                {
                    if (!SendEnetData(sendData, sendLength, enableLogging))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending failed");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0003;
                    }
                }
            }

            for (; ; )
            {
                int timeout = (Nr78Dict.Count > 0) ? ParTimeoutNr78 : ParTimeoutStd;
                //if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Timeout: {0}", timeout);

                int addRectTimeout;
                if (SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.Icom)
                {
                    addRectTimeout = AddRecTimeoutIcom;
                }
                else
                {
                    addRectTimeout = AddRecTimeout;
                }

                timeout += addRectTimeout;
                if (SharedDataActive.DiagDoIp)
                {
                    if (!ReceiveDoIpData(receiveData, timeout))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No data received");
                        // request new routing activation
                        SharedDataActive.DoIpRoutingState = DoIpRoutingState.None;
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }
                else
                {
                    if (!ReceiveEnetData(receiveData, timeout))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No data received");
                        return EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
                    }
                }

                int recLength = TelLengthBmwFast(receiveData);
                if (enableLogging) EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, receiveData, 0, recLength + 1, "Resp");

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
            // the simulated CRC is appended for EDIABAS compatibility
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
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                {
#if !Android
                    Debug.WriteLine("WriteNetworkStream exception: {0}", EdiabasNet.GetExceptionText(ex));
#endif
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

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (IcomAllocateDeviceHttpClient != null)
                    {
                        try
                        {
                            IcomAllocateDeviceHttpClient.Dispose();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        IcomAllocateDeviceHttpClient = null;
                    }

                    if (NonSharedData != null)
                    {
                        NonSharedData.Dispose();
                        NonSharedData = null;
                    }

                    if (UdpEvent != null)
                    {
                        UdpEvent.Dispose();
                        UdpEvent = null;
                    }

                    if (IcomEvent != null)
                    {
                        IcomEvent.Dispose();
                        IcomEvent = null;
                    }
                }

                InterfaceUnlock();
                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
