using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

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

        public delegate List<X509CertificateStructure> GenS29CertDelegate(AsymmetricKeyParameter machinePublicKey, List<X509CertificateStructure> trustedCaCerts, string trustedKeyPath, string vin);

        public delegate void VehicleConnectedDelegate(bool connected, bool reconnect, string vin, bool isDoIp);

        public class ConnectParameterType
        {
#if ANDROID
            public ConnectParameterType(TcpClientWithTimeout.NetworkData networkData, GenS29CertDelegate genS29CertHandler, VehicleConnectedDelegate vehicleConnectedHandler = null)
            {
                NetworkData = networkData;
                GenS29CertHandler = genS29CertHandler;
                VehicleConnectedHandler = vehicleConnectedHandler;
            }
#else
            public ConnectParameterType(GenS29CertDelegate genS29CertHandler, VehicleConnectedDelegate vehicleConnectedHandler = null)
            {
                GenS29CertHandler = genS29CertHandler;
                VehicleConnectedHandler = vehicleConnectedHandler;
            }
#endif
#if ANDROID
            public TcpClientWithTimeout.NetworkData NetworkData { get; }
#endif
            public GenS29CertDelegate GenS29CertHandler { get; }

            public VehicleConnectedDelegate VehicleConnectedHandler { get; }
        }

        public class EnetConnection : IComparable<EnetConnection>, IEquatable<EnetConnection>
        {
            public enum InterfaceType
            {
                DirectHsfz,
                DirectDoIp,
                Enet,
                Icom,
                Undefined
            }

            public EnetConnection(InterfaceType connectionType, IPAddress ipAddress, int diagPort = -1, int controlPort = -1, int doIpPort = -1, int sslPort = -1)
            {
                ConnectionType = connectionType;
                IpAddress = ipAddress;
                DiagPort = diagPort;
                ControlPort = controlPort;
                DoIpPort = doIpPort;
                SslPort = sslPort;
                Mac = string.Empty;
                Vin = string.Empty;
            }

            public InterfaceType ConnectionType { get; }
            public IPAddress IpAddress { get;}
            public int DiagPort { get; }
            public int ControlPort { get; }
            public int DoIpPort { get; }
            public int SslPort { get; }
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

                bool isRplus = ConnectionType == InterfaceType.Icom && DiagPort == DiagPortRplusDefault && ControlPort < 0;
                bool isDoIp = ConnectionType == InterfaceType.DirectDoIp || DoIpPort >= 0 || SslPort >= 0;
                if (isRplus)
                {
                    sb.Append(":");
                    sb.Append(ProtocolIcomP);
                }
                else if (isDoIp)
                {
                    sb.Append(":");
                    sb.Append(ProtocolDoIp);

                    int skipped = 0;
                    if (DoIpPort >= 0)
                    {
                        sb.Append(string.Format(CultureInfo.InvariantCulture, ":{0}", DoIpPort));
                    }
                    else
                    {
                        skipped++;
                    }

                    if (SslPort >= 0)
                    {
                        while (skipped > 0)
                        {
                            sb.Append(":");
                            skipped--;
                        }
                        sb.Append(string.Format(CultureInfo.InvariantCulture, ":{0}", SslPort));
                    }
                }
                else if (ConnectionType != InterfaceType.Undefined)
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

                if (ConnectionType < enetConnection.ConnectionType)
                {
                    return -1;
                }

                if (ConnectionType > enetConnection.ConnectionType)
                {
                    return 1;
                }

                if (ConnectionType == InterfaceType.DirectHsfz)
                {
                    if (DiagPort < enetConnection.DiagPort)
                    {
                        return -1;
                    }

                    if (DiagPort > enetConnection.DiagPort)
                    {
                        return 1;
                    }

                    if (ControlPort < enetConnection.ControlPort)
                    {
                        return -1;
                    }

                    if (ControlPort > enetConnection.ControlPort)
                    {
                        return 1;
                    }

                    return 0;
                }

                if (DoIpPort < enetConnection.DoIpPort)
                {
                    return -1;
                }

                if (DoIpPort > enetConnection.DoIpPort)
                {
                    return 1;
                }

                if (SslPort < enetConnection.SslPort)
                {
                    return -1;
                }

                if (SslPort > enetConnection.SslPort)
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

        public class NmpParameter
        {
            public enum DataTypes
            {
                None = 0,
                Integer = 4,
                String = 7,
                Binary = 8,
                Structure = 9,
            }

            public enum DataSubTypes
            {
                None = 0,
                ConfigParameter = 1,
            }

            public NmpParameter(byte[] telegram, int bufferSize, int offset)
            {
                if (telegram == null || bufferSize - offset < 8)
                {
                    throw new ArgumentException("Invalid NMP parameter data");
                }

                DataType = (DataTypes)((telegram[1 + offset] << 8) | telegram[0 + offset]);

                SubType = (DataSubTypes)((telegram[3 + offset] << 8) | telegram[2 + offset]);

                int dataLen = (telegram[7 + offset] << 24) | (telegram[6 + offset] << 16) | (telegram[5 + offset] << 8) | telegram[4 + offset];
                if (dataLen < 0 || dataLen > bufferSize - offset - 8)
                {
                    throw new ArgumentException("Invalid NMP parameter data length");
                }

                DataArray = new byte[dataLen];
                Array.Copy(telegram, 8 + offset, DataArray, 0, dataLen);
            }

            public NmpParameter(int value)
            {
                DataType = DataTypes.Integer;
                SubType = DataSubTypes.None;
                DataArray = new byte[] { (byte)value, (byte)(value >> 8) };
            }

            public NmpParameter(string text)
            {
                string contentString = text ?? string.Empty;
                contentString += "\0";

                DataType = DataTypes.String;
                SubType = DataSubTypes.None;
                DataArray = Encoding.ASCII.GetBytes(contentString);
            }

            public NmpParameter(byte[] data)
            {
                byte[] contentData = data ?? Array.Empty<byte>();

                DataType = DataTypes.Binary;
                SubType = DataSubTypes.None;
                DataArray = new byte[contentData.Length];
                Array.Copy(contentData, DataArray, contentData.Length);
            }

            public NmpParameter(int parameterId, EdiabasNet.IfhParameterType parameterType, int value = 0xFFFF)
            {
                DataType = DataTypes.Structure;
                SubType = DataSubTypes.ConfigParameter;
                DataArray = new byte[6];

                int parameterTypeInt = (int)parameterType;
                DataArray[0] = (byte)parameterId;
                DataArray[1] = (byte)(parameterId >> 8);
                DataArray[2] = (byte)parameterTypeInt;
                DataArray[3] = (byte)(parameterTypeInt >> 8);
                DataArray[4] = (byte)value;
                DataArray[5] = (byte)(value >> 8);
            }

            public int? GetInteger()
            {
                if (DataType != DataTypes.Integer)
                {
                    return null;
                }

                if (DataArray.Length != 2)
                {
                    return null;
                }

                return (DataArray[1] << 8) | DataArray[0];
            }

            public EdiabasNet.ErrorCodes? GetErrorCode()
            {
                int? error = GetInteger();
                if (error == null)
                {
                    return null;
                }

                return (EdiabasNet.ErrorCodes)error.Value;
            }

            public EdiabasNet.IfhStatusCodes? GetStatusCode()
            {
                int? status = GetInteger();
                if (status == null)
                {
                    return null;
                }

                return (EdiabasNet.IfhStatusCodes)status.Value;
            }

            public string GetString()
            {
                if (DataType != DataTypes.String)
                {
                    return null;
                }

                int strLen = DataArray.Length;
                if (strLen > 0 && DataArray[strLen - 1] == 0)
                {
                    strLen--;
                }

                return Encoding.ASCII.GetString(DataArray, 0, strLen);
            }

            public byte[] GetBinary()
            {
                if (DataType != DataTypes.Binary)
                {
                    return null;
                }

                return DataArray;
            }

            public int? GetIfhParameter(out int parameterId, out EdiabasNet.IfhParameterType parameterType)
            {
                parameterId = -1;
                parameterType = EdiabasNet.IfhParameterType.CFGTYPE_NONE;

                if (DataType != DataTypes.Structure ||  SubType != DataSubTypes.ConfigParameter ||
                    DataArray.Length < 6)
                {
                    return null;
                }

                parameterId = (DataArray[0] << 8) | DataArray[1];
                parameterType = (EdiabasNet.IfhParameterType) ((DataArray[2] << 8) | DataArray[3]);
                int value = (DataArray[5] << 8) | DataArray[4];
                return value;
            }

            public List<byte> GetTelegram()
            {
                List<byte> telegram = new List<byte>();

                telegram.Add((byte)DataType);
                telegram.Add((byte)((ushort)DataType >> 8));

                telegram.Add((byte)SubType);
                telegram.Add((byte)((ushort)SubType >> 8));

                int dataLen = DataArray.Length;
                telegram.Add((byte)dataLen);
                telegram.Add((byte)(dataLen >> 8));
                telegram.Add((byte)(dataLen >> 16));
                telegram.Add((byte)(dataLen >> 24));

                telegram.AddRange(DataArray);
                return telegram;
            }

            public int GetTelegramLength()
            {
                return 8 + DataArray.Length;
            }
            public DataTypes DataType { get; set; }
            public DataSubTypes SubType { get; set; }
            public byte[] DataArray { get; set; }
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
                TrustedCAs = null;
                TrustedCaStructs = null;
                S29Certs = null;
                S29CertFiles = null;
                MachineKeyPair = null;
                S29SelectCert = null;
                TcpControlTimerLock = new object();
                TcpDiagBuffer = new byte[TransBufferSize];
                TcpDiagRecLen = 0;
                LastTcpDiagRecTime = DateTime.MinValue.Ticks;
                TcpDiagRecQueue = new Queue<byte[]>();
                NmpCounter = 0;
                NmpChannel = 0;
            }

            public void DisposeCAs()
            {
                if (TrustedCAs != null)
                {
                    foreach (X509Certificate2 certificate in TrustedCAs)
                    {
                        certificate.Dispose();
                    }

                    TrustedCAs.Clear();
                    TrustedCAs = null;
                }

                if (TrustedCaStructs != null)
                {
                    TrustedCaStructs.Clear();
                    TrustedCaStructs = null;
                }
            }

            public void DisposeS29Certs()
            {
                if (S29Certs != null)
                {
                    foreach (X509Certificate2 certificate in S29Certs)
                    {
                        certificate.Dispose();
                    }

                    S29Certs.Clear();
                    S29Certs = null;
                }

                if (S29CertFiles != null)
                {
                    foreach (EdBcTlsClient.CertInfo certInfo in S29CertFiles)
                    {
                        if (certInfo.TempFile)
                        {
                            try
                            {
                                if (File.Exists(certInfo.PrivateCert))
                                {
                                    File.Delete(certInfo.PrivateCert);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            try
                            {
                                if (File.Exists(certInfo.PublicCert))
                                {
                                    File.Delete(certInfo.PublicCert);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    S29CertFiles.Clear();
                    S29CertFiles = null;
                }

                S29SelectCert = null;
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

                        DisposeCAs();
                        DisposeS29Certs();
                    }

                    // Note disposing has been done.
                    _disposed = true;
                }
            }

            private bool _disposed;
            public object NetworkData;
            public GenS29CertDelegate GenS29CertHandler;
            public VehicleConnectedDelegate VehicleConnectedHandler;
            public EnetConnection EnetHostConn;
            public TcpClient TcpDiagClient;
            public Stream TcpDiagStream;
            public TlsClientProtocol BcTlsClientProtocol;
            public bool DiagDoIp;
            public bool DiagDoIpSsl;
            public bool DiagRplus;
            public AutoResetEvent TcpDiagStreamRecEvent;
            public ManualResetEvent TransmitCancelEvent;
            public TcpClient TcpControlClient;
            public Stream TcpControlStream;
            public Timer TcpControlTimer;
            public List<X509Certificate2> TrustedCAs;
            public List<X509CertificateStructure> TrustedCaStructs;
            public List<X509Certificate2> S29Certs;
            public List<EdBcTlsClient.CertInfo> S29CertFiles;
            public AsymmetricCipherKeyPair MachineKeyPair;
            public List<X509CertificateStructure> S29SelectCert;
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
            public int NmpCounter;
            public int NmpChannel;
        }

        protected delegate EdiabasNet.ErrorCodes TransmitDelegate(byte[] sendData, int sendDataLength, ref byte[] receiveData, out int receiveLength);
        public delegate void IcomAllocateDeviceDelegate(bool success, int statusCode = -1);

        private bool _disposed;
        private static Mutex _interfaceMutex;
        public const int MaxAckLength = 13;
        public const int MaxDoIpAckLength = 5;
        public const int DoIpProtoVer = 0x03;
        public const int DoIpGwAddrDefault = 0x0010;
        public const int DiagPortDefault = 6801;
        public const int DiagPortRplusDefault = 6801;
        public const int ControlPortDefault = 6811;
        public const int DoIpPortDefault = 13400;
        public const int DoIpSslPortDefault = 3496;
        public const int IcomDiagPortDefault = 50160;
        public const int IcomControlPortDefault = 50161;
        public const int IcomDoIpPortDefault = 50162;
        public const int IcomSslPortDefault = 50163;
        public const string NetworkProtocolTcp = "TCP";
        public const string NetworkProtocolSsl = "SSL";
        public const string AutoIp = "auto";
        public const string AutoIpAll = ":all";
        public const string ProtocolHsfz = "HSFZ";
        public const string ProtocolDoIp = "DoIP";
        public const string ProtocolIcomP = "ICOM_P";
        public const string DoIpSecurityDir = "Security";
        public const string DoIpS29Dir = "S29";
        public const string DoIpCertificatesDir = "Certificates";
        public const string DoIpSslTrustDir = "SSL_Truststore";
        protected const string MutexName = "EdiabasLib_InterfaceEnet";
        protected const int TransBufferSize = 0x10010; // transmit buffer size
        protected const int TcpConnectTimeoutMin = 1000;
        protected const int TcpEnetAckTimeout = 5000;
        protected const int SslAuthTimeout = 5000;
        protected const int TcpDoIpMaxRetries = 2;
        protected const int TcpSendBufferSize = 1400;
        protected const int UdpDetectRetries = 3;
        protected const string IniFileEnetSection = "XEthernet";
        protected const string IniFileSslSection = "SSL";
        protected const string IcomOwner = "EXPERT";
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
        protected IPAddress UdpIpFilter;
        protected int? UdpDiagPortFilter;
        protected int? UdpDoIpPortFilter;
        protected int? UdpDoIpSslFilter;
        protected string UdpIcomOwnerFilter;
        protected AutoResetEvent UdpEvent;
        protected AutoResetEvent IcomEvent;

        protected string NetworkProtocolProtected = NetworkProtocolTcp;
        protected string RemoteHostProtected = AutoIp;
        protected string VehicleProtocolProtected = ProtocolHsfz + "," + ProtocolDoIp;
        protected int TesterAddress = 0xF4;
        protected int DoIpTesterAddress = 0x0EF3;
        protected int DoIpGatewayAddress = DoIpGwAddrDefault;
        protected string HostIdentServiceProtected = "255.255.255.255";
        protected int UdpIdentPort = ControlPortDefault;
        protected int UdpSrvLocPort = 427;
        protected int DiagnosticPort = DiagPortDefault;
        protected int ControlPort = ControlPortDefault;
        protected int DoIpPort = DoIpPortDefault;
        protected int DoIpSslPort = DoIpSslPortDefault;
        protected string RplusSectionProtected = null;
        protected int RplusPort = DiagPortRplusDefault;
        protected bool DoIpBcSsl = true;
        protected string DoIpSslSecurityPathProtected = string.Empty;
        protected string DoIpS29PathProtected = string.Empty;
        protected string DoIpS29SelectCert = string.Empty;
        protected string DoIpS29JsonRequestPath = string.Empty;
        protected string DoIpS29JsonResponsePath = string.Empty;
        protected int ConnectTimeout = 5000;
        protected int BatteryVoltageValue = 12000;
        protected int IgnitionVoltageValue = 12000;
        protected int DoIpTimeoutAcknowledge = 2000;
        protected int RplusFunctionTimeout = 15000;
        protected int AddRecTimeoutProtected = 1000;
        protected int AddRecTimeoutIcomProtected = 2000;
        protected bool IcomAllocateProtected = false;
        protected bool RplusIcomEnetRedirectProtected = true;
        protected HttpClient IcomAllocateDeviceHttpClient;

        protected byte[] RecBuffer = new byte[TransBufferSize];
        protected byte[] DataBuffer = new byte[TransBufferSize];
        protected byte[] AckBuffer = new byte[TransBufferSize];
        protected byte[] AuthBuffer = new byte[TransBufferSize];
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

                string prop = EdiabasProtected?.GetConfigProperty("EnetNetworkProtocol");
                if (!string.IsNullOrEmpty(prop))
                {
                    NetworkProtocolProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("NetworkProtocol");
                if (!string.IsNullOrEmpty(prop))
                {
                    NetworkProtocolProtected = prop;
                }

                if (!string.IsNullOrEmpty(RplusSectionProtected) && string.IsNullOrEmpty(RemoteHostProtected))
                {
                    prop = EdiabasProtected?.GetConfigProperty("EnetRemoteHost");
                    if (!string.IsNullOrEmpty(prop))
                    {
                        RemoteHostProtected = prop;
                    }

                    prop = EdiabasProtected?.GetConfigProperty("RemoteHost");
                    if (!string.IsNullOrEmpty(prop))
                    {
                        RemoteHostProtected = prop;
                    }
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetVehicleProtocol");
                if (!string.IsNullOrEmpty(prop))
                {
                    VehicleProtocolProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("VehicleProtocol");
                if (!string.IsNullOrEmpty(prop))
                {
                    VehicleProtocolProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetHostIdentService");
                if (!string.IsNullOrEmpty(prop))
                {
                    HostIdentServiceProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("HostIdentService");
                if (!string.IsNullOrEmpty(prop))
                {
                    HostIdentServiceProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetTesterAddress");
                if (!string.IsNullOrEmpty(prop))
                {
                    TesterAddress = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDoIPTesterAddress");
                if (!string.IsNullOrEmpty(prop))
                {
                    string propTrim = prop.Trim();
                    if (!propTrim.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        propTrim = "0x" + propTrim;
                    }
                    DoIpTesterAddress = (int)EdiabasNet.StringToValue(propTrim);
                }

                prop = EdiabasProtected?.GetConfigProperty("DoIPTesterAddress");
                if (!string.IsNullOrEmpty(prop))
                {
                    string propTrim = prop.Trim();
                    if (!propTrim.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        propTrim = "0x" + propTrim;
                    }
                    DoIpTesterAddress = (int)EdiabasNet.StringToValue(propTrim);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDoipGatewayAddress");
                if (!string.IsNullOrEmpty(prop))
                {
                    string propTrim = prop.Trim();
                    if (!propTrim.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        propTrim = "0x" + propTrim;
                    }
                    DoIpGatewayAddress = (int)EdiabasNet.StringToValue(propTrim);
                }

                prop = EdiabasProtected?.GetConfigProperty("DoipGatewayAddress");
                if (!string.IsNullOrEmpty(prop))
                {
                    string propTrim = prop.Trim();
                    if (!propTrim.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        propTrim = "0x" + propTrim;
                    }
                    DoIpGatewayAddress = (int)EdiabasNet.StringToValue(propTrim);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDiagnosticPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("DiagnosticPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    DiagnosticPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetControlPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    ControlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("ControlPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    ControlPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetDoIPPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("PortDoIP");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetSslPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpSslPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("SslPort");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpSslPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("SslBC");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpBcSsl = (int)EdiabasNet.StringToValue(prop) != 0;
                }

                prop = EdiabasProtected?.GetConfigProperty("SslSecurityPath");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpSslSecurityPathProtected = prop;

                    if (Directory.Exists(DoIpSslSecurityPathProtected))
                    {
                        string parentDir = Directory.GetParent(DoIpSslSecurityPathProtected)?.FullName;
                        if (!string.IsNullOrEmpty(parentDir))
                        {
                            string s29BasePath = Path.Combine(parentDir, DoIpS29Dir);
                            string certificatesPath = Path.Combine(s29BasePath, DoIpCertificatesDir);
                            if (Directory.Exists(certificatesPath))
                            {
                                DoIpS29PathProtected = certificatesPath;
                            }
#if false
                        string s29JsonRequestPath = Path.Combine(s29BasePath, "JsonRequests");
                        if (Directory.Exists(s29JsonRequestPath))
                        {
                            DoIpS29JsonRequestPath = s29JsonRequestPath;
                        }

                        string s29JsonResponsePath = Path.Combine(s29BasePath, "JsonResponses");
                        if (Directory.Exists(s29JsonResponsePath))
                        {
                            DoIpS29JsonResponsePath = s29JsonResponsePath;
                        }
#endif
                        }
                    }
                }

                prop = EdiabasProtected?.GetConfigProperty("S29Path");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpS29PathProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("selectCertificate");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpS29SelectCert = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("JSONRequestPath");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpS29JsonRequestPath = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("JSONResponsePath");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpS29JsonResponsePath = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetTimeoutConnect");
                if (!string.IsNullOrEmpty(prop))
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("TimeoutConnect");
                if (!string.IsNullOrEmpty(prop))
                {
                    ConnectTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetBatteryVoltage");
                if (!string.IsNullOrEmpty(prop))
                {
                    BatteryVoltageValue = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetIgnitionVoltage");
                if (!string.IsNullOrEmpty(prop))
                {
                    IgnitionVoltageValue = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetTimeoutAcknowledge");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpTimeoutAcknowledge = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("TimeoutAcknowledge");
                if (!string.IsNullOrEmpty(prop))
                {
                    DoIpTimeoutAcknowledge = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("RplusTimeoutFunction");
                if (!string.IsNullOrEmpty(prop))
                {
                    RplusFunctionTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("TimeoutFunction");
                if (!string.IsNullOrEmpty(prop))
                {
                    RplusFunctionTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetAddRecTimeout");
                if (!string.IsNullOrEmpty(prop))
                {
                    AddRecTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("EnetAddRecTimeoutIcom");
                if (!string.IsNullOrEmpty(prop))
                {
                    AddRecTimeoutIcom = (int)EdiabasNet.StringToValue(prop);
                }
#if ANDROID
                IcomAllocate = true;
#else
                IcomAllocate = false;
#endif
                prop = EdiabasProtected?.GetConfigProperty("EnetIcomAllocate");
                if (!string.IsNullOrEmpty(prop))
                {
                    IcomAllocate = EdiabasNet.StringToValue(prop) != 0;
                }

                prop = EdiabasProtected?.GetConfigProperty("RplusIcomEnetRedirect");
                if (!string.IsNullOrEmpty(prop))
                {
                    RplusIcomEnetRedirect = EdiabasNet.StringToValue(prop) != 0;
                }

                if (string.IsNullOrEmpty(RplusSectionProtected) && !IsIpv4Address(RemoteHostProtected))
                {
                    string iniFile = EdiabasProtected?.IniFileName;
                    if (!string.IsNullOrEmpty(iniFile))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using ENET ini file at: {0}", iniFile);
                        IniFile ediabasIni = new IniFile(iniFile);
                        string iniRemoteHost = ediabasIni.GetValue(IniFileEnetSection, "RemoteHost", string.Empty);
                        bool hostValid = false;
                        if (IsIpv4Address(iniRemoteHost))
                        {
                            hostValid = true;
                            RemoteHostProtected = iniRemoteHost;
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using remote host from ini file: {0}", RemoteHostProtected);
                        }

                        if (hostValid)
                        {
                            string iniDiagnosticPort = ediabasIni.GetValue(IniFileEnetSection, "DiagnosticPort", string.Empty);
                            if (!string.IsNullOrEmpty(iniDiagnosticPort))
                            {
                                DiagnosticPort = (int)EdiabasNet.StringToValue(iniDiagnosticPort);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using diagnostic port from ini file: {0}", DiagnosticPort);
                            }

                            string iniControlPort = ediabasIni.GetValue(IniFileEnetSection, "ControlPort", string.Empty);
                            if (!string.IsNullOrEmpty(iniControlPort))
                            {
                                ControlPort = (int)EdiabasNet.StringToValue(iniControlPort);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using control port from ini file: {0}", ControlPort);
                            }

                            string iniPortDoIP = ediabasIni.GetValue(IniFileEnetSection, "PortDoIP", string.Empty);
                            if (!string.IsNullOrEmpty(iniPortDoIP))
                            {
                                DoIpPort = (int)EdiabasNet.StringToValue(iniPortDoIP);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using DoIp port from ini file: {0}", DoIpPort);
                            }

                            string iniSslPort = ediabasIni.GetValue(IniFileSslSection, "SSLPORT", string.Empty);
                            if (!string.IsNullOrEmpty(iniSslPort))
                            {
                                DoIpSslPort = (int)EdiabasNet.StringToValue(iniSslPort);
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using DoIpSslPort port from ini file: {0}", DoIpSslPort);
                            }

                            string iniSslSecPath = ediabasIni.GetValue(IniFileSslSection, "SecurityPath", string.Empty);
                            if (!string.IsNullOrEmpty(iniSslSecPath))
                            {
                                DoIpSslSecurityPathProtected = iniSslSecPath;
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using DoIpSslSecurityPath from ini file: {0}", DoIpSslSecurityPathProtected);
                            }

                            string iniVehicleProtocol = ediabasIni.GetValue(IniFileEnetSection, "VehicleProtocol", string.Empty);
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

                if (string.Compare(NetworkProtocolProtected, NetworkProtocolSsl, StringComparison.OrdinalIgnoreCase) == 0 &&
                    string.IsNullOrEmpty(DoIpS29SelectCert))
                {   // always create cert in ssl mode
                    if (!CreateS29Certs(null, DoIpS29PathProtected))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "S29 certificate generation failed path: {0}", DoIpS29PathProtected);
                    }
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
                    string.Format(Culture, "{0} CommParameter Host={1}, Tester=0x{2:X02}, DiagPort={3}, ControlPort={4}",
                            InterfaceName, RemoteHostProtected, TesterAddress, DiagnosticPort, ControlPort));

                if (SharedDataActive.DiagRplus)
                {
                    EdiabasNet.ErrorCodes errorCode = NmtSetParameter(UInt32ByteArrayToLe(CommParameterProtected));
                    if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        EdiabasProtected?.SetError(errorCode);
                    }
                    return;
                }

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

        public override Int16[] CommAnswerLen
        {
            get
            {
                return base.CommAnswerLen;
            }
            set
            {
                base.CommAnswerLen = value;
                if (SharedDataActive.DiagRplus)
                {
                    EdiabasNet.ErrorCodes errorCode = NmtSetPreface(Int16ByteArrayToLe(base.CommAnswerLen));
                    if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        EdiabasProtected?.SetError(errorCode);
                    }
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
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read battery voltage");
                if (IsSimulationMode())
                {
                    return UbatVoltageSimulation;
                }

                if (!Connected)
                {
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0056);
                    return Int64.MinValue;
                }
                return BatteryVoltageValue;
            }
        }

        public override Int64 IgnitionVoltage
        {
            get
            {
                EdiabasProtected.LogString(EdiabasNet.EdLogLevel.Ifh, "Read ignition voltage");
                if (IsSimulationMode())
                {
                    return IgnitionVoltageSimulation;
                }

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
                        return IgnitionVoltageValue;
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
                if (IsSimulationMode())
                {
                    return SimulationConnected;
                }

                return ((SharedDataActive.TcpDiagClient != null) && (SharedDataActive.TcpDiagStream != null)) || SharedDataActive.ReconnectRequired;
            }
        }

        public override bool IsEcuConnected
        {
            get
            {
                return true;
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

        protected virtual string RplusSection
        {
            get
            {
                return RplusSectionProtected;
            }
        }

        static EdInterfaceEnet()
        {
#if ANDROID
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
            if (!base.InterfaceConnect())
            {
                EdiabasProtected.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0018);
                return false;
            }

            if (IsSimulationMode())
            {
                return true;
            }

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
                SharedDataActive.GenS29CertHandler = null;
                SharedDataActive.VehicleConnectedHandler = null;
                SharedDataActive.TransmitCancelEvent.Reset();
                if (ConnectParameter is ConnectParameterType connectParameter)
                {
#if ANDROID
                    SharedDataActive.NetworkData = connectParameter.NetworkData;
#endif
                    SharedDataActive.GenS29CertHandler = connectParameter.GenS29CertHandler;
                    SharedDataActive.VehicleConnectedHandler = connectParameter.VehicleConnectedHandler;
                }

                string[] protocolParts = VehicleProtocolProtected.Split(',');
                if (protocolParts.Length < 1)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle protocol: {0}", VehicleProtocolProtected);
                    return false;
                }

                bool diagRplusMode = !string.IsNullOrEmpty(RplusSectionProtected);
                List<CommunicationMode> communicationModes = new List<CommunicationMode>();
                if (reconnect)
                {
                    // reuse last host connection
                    if (SharedDataActive.DiagRplus)
                    {
                        diagRplusMode = true;
                        communicationModes.Add(CommunicationMode.Hsfz);
                    }
                    else if (SharedDataActive.DiagDoIp)
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
                    SharedDataActive.EnetHostConn = null;
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

                bool ignoreIcomOwner = !IcomAllocate;
                if (SharedDataActive.EnetHostConn == null)
                {
                    if (RemoteHostProtected.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
                    {
                        List<EnetConnection> detectedVehicles = DetectedVehicles(RemoteHostProtected, 1, UdpDetectRetries, communicationModes, ignoreIcomOwner);
                        if ((detectedVehicles == null) || (detectedVehicles.Count < 1))
                        {
                            return false;
                        }
                        SharedDataActive.EnetHostConn = detectedVehicles[0];
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Received: IP={0}:{1}, Type={2}",
                            SharedDataActive.EnetHostConn.IpAddress, SharedDataActive.EnetHostConn.DiagPort, SharedDataActive.EnetHostConn.ConnectionType));

                        if (SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.Icom &&
                                 SharedDataActive.EnetHostConn.DiagPort == DiagPortRplusDefault && SharedDataActive.EnetHostConn.ControlPort < 0)
                        {
                            diagRplusMode = true;
                        }
                        else if (SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.DirectDoIp ||
                            SharedDataActive.EnetHostConn.DoIpPort >= 0 || SharedDataActive.EnetHostConn.SslPort >= 0)
                        {
                            communicationModes.Clear();
                            communicationModes.Add(CommunicationMode.DoIp);
                        }
                        else if(SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.DirectHsfz ||
                                SharedDataActive.EnetHostConn.DiagPort >= 0 || SharedDataActive.EnetHostConn.ControlPort >= 0)
                        {
                            communicationModes.Clear();
                            communicationModes.Add(CommunicationMode.Hsfz);
                        }
                    }
                    else
                    {
                        string[] hostParts = RemoteHostProtected.Split(':');
                        if (hostParts.Length < 1)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Host name invalid: {0}", RemoteHostProtected);
                            return false;
                        }

                        string hostIp = hostParts[0];
                        int hostPos = 1;
                        int hostDiagPort = -1;
                        int hostControlPort = -1;
                        int hostDoIpPort = -1;
                        int hostSslPort = -1;
                        EnetConnection.InterfaceType connectionType = EnetConnection.InterfaceType.DirectHsfz;
                        bool protocolSpecified = false;

                        if (hostParts.Length >= hostPos + 1)
                        {
                            if (string.Compare(hostParts[hostPos], ProtocolHsfz, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                protocolSpecified = true;
                                hostPos++;
                                connectionType = EnetConnection.InterfaceType.DirectHsfz;
                            }
                            else if (string.Compare(hostParts[hostPos], ProtocolDoIp, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                protocolSpecified = true;
                                hostPos++;
                                connectionType = EnetConnection.InterfaceType.DirectDoIp;
                            }
                            else if (string.Compare(hostParts[hostPos], ProtocolIcomP, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                protocolSpecified = true;
                                hostPos++;
                                connectionType = EnetConnection.InterfaceType.Icom;
                                diagRplusMode = true;
                            }
                        }

                        if (diagRplusMode)
                        {
                            hostDiagPort = DiagPortRplusDefault;
                            hostControlPort = -1;
                            communicationModes.Clear();
                            communicationModes.Add(CommunicationMode.Hsfz);
                            if (RplusIcomEnetRedirect)
                            {
                                List<EnetConnection> detectedVehicles = DetectedVehicles(hostIp, 1, UdpDetectRetries, new List<CommunicationMode> { CommunicationMode.Hsfz }, ignoreIcomOwner);
                                if ((detectedVehicles == null) || (detectedVehicles.Count < 1))
                                {
                                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No RPLUS UDP response for host: {0}", hostIp);
                                    return false;
                                }

                                if (detectedVehicles[0].ConnectionType == EnetConnection.InterfaceType.Icom &&
                                    detectedVehicles[0].DiagPort == IcomDiagPortDefault && detectedVehicles[0].ControlPort == IcomControlPortDefault)
                                {
                                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Switching from RPLUS to HSFZ connection for host: {0}", hostIp);
                                    diagRplusMode = false;
                                    hostDiagPort = detectedVehicles[0].DiagPort;
                                    hostControlPort = detectedVehicles[0].ControlPort;
                                }
                            }
                        }
                        else if (connectionType == EnetConnection.InterfaceType.DirectHsfz)
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

                            if (hostParts.Length >= hostPos + 1)
                            {
                                Int64 portValue = EdiabasNet.StringToValue(hostParts[hostPos], out bool valid);
                                hostPos++;

                                if (valid)
                                {
                                    hostSslPort = (int)portValue;
                                }
                            }
                        }

                        SharedDataActive.EnetHostConn = new EnetConnection(connectionType, IPAddress.Parse(hostIp), hostDiagPort, hostControlPort, hostDoIpPort, hostSslPort);
                    }
                }

                int diagPort;
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

                    bool diagDoIpSsl = false;

                    if (!diagRplusMode && SharedDataActive.DiagDoIp)
                    {
                        string hostIp = SharedDataActive.EnetHostConn.IpAddress.ToString();
                        List<EnetConnection> detectedVehicles = DetectedVehicles(hostIp, 1, UdpDetectRetries, new List<CommunicationMode> { CommunicationMode.DoIp }, ignoreIcomOwner);
                        if ((detectedVehicles == null) || (detectedVehicles.Count < 1))
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No DoIp UDP response for host: {0}", hostIp);
                            continue;
                        }

                        SharedDataActive.EnetHostConn = detectedVehicles[0];
                        diagDoIpSsl = string.Compare(NetworkProtocolProtected, NetworkProtocolSsl, StringComparison.OrdinalIgnoreCase) == 0;
                        if (diagDoIpSsl)
                        {
                            if (!GetTrustedCAs(SharedDataActive, DoIpSslSecurityPathProtected))
                            {
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No trusted certificates found in path: {0}", DoIpSslSecurityPathProtected);
                                continue;
                            }

                            if (!CreateS29Certs(SharedDataActive, DoIpS29PathProtected))
                            {
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No S29 certificates found in path: {0}", DoIpS29PathProtected);
                                continue;
                            }

                            string selectCert = DoIpS29SelectCert;
                            SharedDataActive.S29SelectCert = null;
                            if (string.IsNullOrEmpty(selectCert) && SharedDataActive.GenS29CertHandler != null)
                            {
                                string vin = SharedDataActive.EnetHostConn?.Vin;
                                List<X509CertificateStructure> certList = SharedDataActive.GenS29CertHandler(SharedDataActive.MachineKeyPair.Public, SharedDataActive.TrustedCaStructs, DoIpSslSecurityPathProtected, vin);
                                if (certList != null && certList.Count > 1)
                                {
                                    SharedDataActive.S29SelectCert = certList;
                                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "S29 certificates generated: {0}", certList.Count);
                                }
                                else
                                {
                                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "S29 certificate generation failed");
                                }
                            }

                            if (SharedDataActive.S29SelectCert == null)
                            {
                                if (string.IsNullOrEmpty(selectCert))
                                {
                                    if (!string.IsNullOrEmpty(DoIpS29JsonRequestPath))
                                    {
                                        if (!CreateRequestJson(SharedDataActive, DoIpS29JsonRequestPath, EdSec4Diag.CertReqProfile.EnumType.crp_M2M_3rdParty_4_CUST_ReadWriteControl))
                                        {
                                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "External S29 certificate request generation failed");
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(DoIpS29JsonResponsePath))
                                    {
                                        if (StoreResponseJsonCerts(SharedDataActive, DoIpS29JsonResponsePath, DoIpS29PathProtected))
                                        {
                                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "External S29 response certificate stored in: {0}", DoIpS29JsonResponsePath);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!LoadS29Cert(SharedDataActive, DoIpS29PathProtected, selectCert))
                                    {
                                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Selected S29 certificate load failed: {0}", selectCert);
                                        EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_SEC_0036);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    SharedDataActive.DiagDoIpSsl = diagDoIpSsl;
                    SharedDataActive.DiagRplus = diagRplusMode;

                    if (SharedDataActive.DiagRplus)
                    {
                        diagPort = RplusPort;
                    }
                    else
                    {
                        int doIpPort = SharedDataActive.DiagDoIpSsl ? DoIpSslPort : DoIpPort;
                        if (SharedDataActive.DiagDoIp)
                        {
                            diagPort = doIpPort;
                            if (SharedDataActive.DiagDoIpSsl)
                            {
                                if (SharedDataActive.EnetHostConn.SslPort >= 0)
                                {
                                    diagPort = SharedDataActive.EnetHostConn.SslPort;
                                }
                            }
                            else
                            {
                                if (SharedDataActive.EnetHostConn.DoIpPort >= 0)
                                {
                                    diagPort = SharedDataActive.EnetHostConn.DoIpPort;
                                }
                            }
                        }
                        else
                        {
                            diagPort = DiagnosticPort;
                            if (SharedDataActive.EnetHostConn.DiagPort >= 0)
                            {
                                diagPort = SharedDataActive.EnetHostConn.DiagPort;
                            }
                        }
                    }

                    if (IcomAllocate && !reconnect && SharedDataActive.EnetHostConn != null &&
                        (SharedDataActive.DiagRplus || SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.Icom))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM at: {0}", SharedDataActive.EnetHostConn.IpAddress);
                        IcomEvent?.Reset();
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

                                IcomEvent?.Set();
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
                                IcomEvent?.WaitOne(1000);
                                // reset allocate active after cancel
                                SharedDataActive.IcomAllocateActive = false;
                            }
                        }
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Allocate ICOM finished");
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
                        if (SharedDataActive.DiagRplus)
                        {
                            SharedDataActive.TcpDiagClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        }

                        if (SharedDataActive.DiagDoIpSsl)
                        {
                            if (DoIpBcSsl)
                            {
                                SharedDataActive.TcpDiagStream = CreateBcSslStream(SharedDataActive);
                            }
                            else
                            {
                                SharedDataActive.TcpDiagStream = CreateSslStream(SharedDataActive);
                            }
                        }
                        else
                        {
                            SharedDataActive.TcpDiagStream = SharedDataActive.TcpDiagClient.GetStream();
                        }

                        SharedDataActive.TcpDiagRecLen = 0;
                        SharedDataActive.LastTcpDiagRecTime = DateTime.MinValue.Ticks;
                        lock (SharedDataActive.TcpDiagStreamRecLock)
                        {
                            SharedDataActive.TcpDiagRecQueue.Clear();
                        }

                        int readLen = 20;
                        if (!SharedDataActive.DiagRplus)
                        {
                            readLen = SharedDataActive.DiagDoIp ? 8 : 6;
                        }

                        StartReadTcpDiag(readLen);
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Connected to: {0}:{1}", SharedDataActive.EnetHostConn.IpAddress, diagPort);
                        SharedDataActive.ReconnectRequired = false;
                        SharedDataActive.DoIpRoutingState = DoIpRoutingState.None;
                        SharedDataActive.NmpCounter = 0;
                        SharedDataActive.NmpChannel = 0;

                        if (SharedDataActive.DiagRplus)
                        {
                            EdiabasNet.ErrorCodes openResult = NmtOpenConnection();
                            if (openResult != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                            {
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT open failed: {0}", openResult);
                                EdiabasProtected?.SetError(openResult);
                                break;
                            }
                        }
                        else if (SharedDataActive.DiagDoIp)
                        {
                            if (!DoIpRoutingActivation(true))
                            {
                                InterfaceDisconnect(reconnect);
                                continue;
                            }

                            if (SharedDataActive.DiagDoIpSsl && !reconnect)
                            {
                                EdiabasNet.ErrorCodes authResult = DoIpAuthenticate(SharedDataActive);
                                if (authResult != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                                {
                                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** DoIp authentication failed: {0}", authResult);
                                    EdiabasProtected?.SetError(authResult);
                                    break;
                                }
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
                        if (ex is EdiabasNet.EdiabasNetException)
                        {
                            throw;
                        }
                    }
                }

                if (SharedDataActive.VehicleConnectedHandler != null)
                {
                    SharedDataActive.VehicleConnectedHandler(Connected, reconnect, SharedDataActive.EnetHostConn?.Vin, SharedDataActive.DiagDoIp);
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
                if (ex is EdiabasNet.EdiabasNetException)
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        public override bool InterfaceDisconnect()
        {
            if (!base.InterfaceDisconnect())
            {
                return false;
            }

            if (IsSimulationMode())
            {
                return true;
            }

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
                    if (SharedDataActive.DiagRplus)
                    {
                        NmtCloseConnection();
                    }

                    SharedDataActive.TcpDiagStream.Dispose();
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
                    SharedDataActive.TcpDiagClient.Dispose();
                    SharedDataActive.TcpDiagClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            SharedDataActive.BcTlsClientProtocol = null;
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

            if (!reconnect)
            {
                SharedDataActive.DisposeCAs();
                SharedDataActive.DisposeS29Certs();
            }

            if (IcomAllocate && !reconnect && SharedDataActive.EnetHostConn != null &&
                (SharedDataActive.DiagRplus || SharedDataActive.EnetHostConn.ConnectionType == EnetConnection.InterfaceType.Icom))
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM at: {0}", SharedDataActive.EnetHostConn.IpAddress);

                IcomEvent?.Reset();
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

                            IcomEvent?.Set();
                        }))
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM error");
                    }

                    // don't use cancell event here, because this could be set.
                    int waitResult = WaitHandle.WaitAny(new WaitHandle[] { IcomEvent }, 2000);
                    if (waitResult != 0)
                    {
                        if (waitResult == WaitHandle.WaitTimeout)
                        {
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM timeout");
                        }
                        else
                        {
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM error");
                        }

                        cts.Cancel();
                        IcomEvent?.WaitOne(1000);
                        // reset allocate active after cancel
                        SharedDataActive.IcomAllocateActive = false;
                    }
                }

                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Deallocate ICOM finished");
            }

            if (!reconnect)
            {
                SharedDataActive.EnetHostConn = null;
            }

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

            if (SharedDataActive.DiagRplus)
            {
                EdiabasNet.ErrorCodes errorCodeNmt = NmtSendTelegram(sendData, out receiveData);
                if (errorCodeNmt != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected?.SetError(errorCodeNmt);
                    return false;
                }
                return true;
            }

            if (IsSimulationMode())
            {
                byte[] simResponse;
                if (!TransmitSimulationData(sendData, out simResponse, null, null, ParTransmitFunc == TransBmwFast))
                {
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0009);
                    return false;
                }

                receiveData = simResponse;
                return true;
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
            if (SharedDataActive.DiagRplus)
            {
                EdiabasNet.ErrorCodes errorCodeNmt = NmtSendTelegramFreq(sendData);
                if (errorCodeNmt != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected?.SetError(errorCodeNmt);
                    return false;
                }
                return true;
            }

            EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0006);
            return false;
        }

        public override bool ReceiveFrequent(out byte[] receiveData)
        {
            if (SharedDataActive.DiagRplus)
            {
                EdiabasNet.ErrorCodes errorCodeNmt = NmtReqTelegramFreq(out receiveData);
                if (errorCodeNmt != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected?.SetError(errorCodeNmt);
                    receiveData = null;
                    return false;
                }
                return true;
            }

            receiveData = ByteArray0;
            return true;
        }

        public override bool StopFrequent()
        {
            if (SharedDataActive.DiagRplus)
            {
                EdiabasNet.ErrorCodes errorCodeNmt = NmtStopFreqTelegram();
                if (errorCodeNmt != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    EdiabasProtected?.SetError(errorCodeNmt);
                    return false;
                }
                return true;
            }

            return true;
        }

        public override bool RawData(byte[] sendData, out byte[] receiveData)
        {
            receiveData = ByteArray0;
            return true;
        }

        public override bool TransmitCancel(bool cancel)
        {
            if (IsSimulationMode())
            {
                return true;
            }

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

        public string NetworkProtocol
        {
            get
            {
                return NetworkProtocolProtected;
            }
            set
            {
                NetworkProtocolProtected = value;
            }
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

        public string DoIpSslSecurityPath
        {
            get
            {
                return DoIpSslSecurityPathProtected;
            }
            set
            {
                DoIpSslSecurityPathProtected = value;
            }
        }

        public string DoIpS29Path
        {
            get
            {
                return DoIpS29PathProtected;
            }
            set
            {
                DoIpS29PathProtected = value;
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

        public bool RplusIcomEnetRedirect
        {
            get
            {
                return RplusIcomEnetRedirectProtected;
            }
            set
            {
                RplusIcomEnetRedirectProtected = value;
            }
        }


        public List<EnetConnection> DetectedVehicles(string remoteHostConfig, List<CommunicationMode> communicationModes = null)
        {
            if (IsSimulationMode())
            {
                return null;
            }

#if ANDROID
            if (ConnectParameter is ConnectParameterType connectParameter)
            {
                SharedDataActive.NetworkData = connectParameter.NetworkData;
            }
#endif

            return DetectedVehicles(remoteHostConfig, -1, UdpDetectRetries, communicationModes);
        }

        public List<EnetConnection> DetectedVehicles(string remoteHostConfig, int maxVehicles, int maxRetries, List<CommunicationMode> communicationModes, bool ignoreIcomOwner = false)
        {
            if (IsSimulationMode())
            {
                return null;
            }

            IPAddress hostIpAddress = null;
            if (!remoteHostConfig.StartsWith(AutoIp, StringComparison.OrdinalIgnoreCase))
            {
                string[] hostParts = remoteHostConfig.Split(':');
                if (hostParts.Length < 1)
                {
                    return null;
                }

                if (!IPAddress.TryParse(hostParts[0], out hostIpAddress))
                {
                    return null;
                }
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
                string configData = string.Empty;
                if (hostIpAddress == null)
                {
                    configData = remoteHostConfig.Remove(0, AutoIp.Length);

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
                }

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
                UdpIpFilter = hostIpAddress;

                UdpDiagPortFilter = null;
                UdpDoIpPortFilter = null;
                UdpDoIpSslFilter = null;
                UdpIcomOwnerFilter = null;

                if (DiagnosticPort != DiagPortDefault)
                {
                    UdpDiagPortFilter = DiagnosticPort;
                }

                if (DoIpPort != DoIpPortDefault)
                {
                    UdpDoIpPortFilter = DoIpPort;
                }

                if (DoIpSslPort != DoIpSslPortDefault)
                {
                    UdpDoIpSslFilter = DoIpSslPort;
                }

                if (!ignoreIcomOwner)
                {
                    UdpIcomOwnerFilter = IcomOwner;
                }

                if (UdpIpFilter != null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DetectedVehicles: UDP IP filter: {0}", UdpIpFilter);
                }

                if (UdpDiagPortFilter != null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DetectedVehicles: UDP diag port filter: {0}", UdpDiagPortFilter.Value);
                }

                if (UdpDoIpPortFilter != null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DetectedVehicles: UDP DoIp port filter: {0}", UdpDoIpPortFilter.Value);
                }

                if (UdpDoIpSslFilter != null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DetectedVehicles: UDP DoIp SSL port filter: {0}", UdpDoIpSslFilter.Value);
                }

                if (UdpIcomOwnerFilter != null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DetectedVehicles: UDP ICOM owner filter: {0}", UdpIcomOwnerFilter);
                }

                StartUdpListen();

                int retryCount = 0;
                for (;;)
                {
                    UdpEvent.Reset();
                    bool broadcastSend = false;

                    if ((configData.Length > 0) && (configData[0] == ':'))
                    {
                        string adapterName = configData.StartsWith(AutoIpAll, StringComparison.OrdinalIgnoreCase) ? string.Empty : configData.Remove(0, 1);

#if ANDROID
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
                                                    if (protocolHsfz)
                                                    {
                                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending ident: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                            adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, UdpIdentPort));

                                                        IPEndPoint ipUdpIdent = new IPEndPoint(broadcastAddress, UdpIdentPort);
                                                        UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                                    }

                                                    if (protocolDoIp)
                                                    {
                                                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending DoIp ident: '{0}': Ip={1} Mask={2} Broadcast={3} Port={4}",
                                                            adapter.Name, ipAddressInfo.Address, ipAddressInfo.IPv4Mask, broadcastAddress, DoIpPort));

                                                        IPEndPoint ipUdpDoIpIdent = new IPEndPoint(broadcastAddress, DoIpPort);
                                                        UdpSocket.SendTo(UdpDoIpIdentReq, ipUdpDoIpIdent);
                                                    }

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
                    {
                        try
                        {
                            if (protocolHsfz)
                            {
                                IPEndPoint ipUdpIdent = new IPEndPoint(hostIpAddress ?? IPAddress.Parse(HostIdentServiceProtected), UdpIdentPort);
                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending Ident broadcast to: {0}:{1}", ipUdpIdent.Address, UdpIdentPort));
                                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                {
                                    UdpSocket.SendTo(UdpIdentReq, ipUdpIdent);
                                }, ipUdpIdent.Address, SharedDataActive.NetworkData);
                            }

                            if (protocolDoIp)
                            {
                                IPEndPoint ipUdpDoIpIdent = new IPEndPoint(hostIpAddress ?? IPAddress.Parse(HostIdentServiceProtected), DoIpPort);
                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending DoIp broadcast to: {0}:{1}", ipUdpDoIpIdent.Address, DoIpPort));
                                TcpClientWithTimeout.ExecuteNetworkCommand(() =>
                                {
                                    UdpSocket.SendTo(UdpDoIpIdentReq, ipUdpDoIpIdent);
                                }, ipUdpDoIpIdent.Address, SharedDataActive.NetworkData);
                            }

                            IPEndPoint ipUdpSvrLoc = new IPEndPoint(hostIpAddress ?? IPAddress.Parse(HostIdentServiceProtected), UdpSrvLocPort);
                            EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, string.Format("Sending SvrLoc broadcast to: {0}:{1}", ipUdpSvrLoc.Address, UdpSrvLocPort));
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
                List<EnetConnection> addListConnList = new List<EnetConnection>();
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
                        addListConnList.Add(new EnetConnection(EnetConnection.InterfaceType.DirectHsfz, recIp));
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
                        if (payloadLen >= minPayloadLength && (gwAddr == DoIpGatewayAddress || DoIpGatewayAddress == 0xFFFF))
                        {
                            addListConnList.Add(new EnetConnection(EnetConnection.InterfaceType.DirectDoIp, recIp));
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
                            if (devType.StartsWith("ENET", StringComparison.OrdinalIgnoreCase))
                            {
                                isEnet = true;
                            }
                            else if (devType.StartsWith("ICOM", StringComparison.OrdinalIgnoreCase))
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
                            addListConnList.Add(new EnetConnection(EnetConnection.InterfaceType.Enet, ipAddressHost));
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

                            bool klineChannel = false;
                            bool dcanChannel = false;
                            bool enetChannel = false;
                            if (attrDict.TryGetValue("VCICHANNELS", out string vciChannels))
                            {
                                vciChannels = vciChannels.TrimStart('[');
                                vciChannels = vciChannels.TrimEnd(']');
                                string[] channelList = vciChannels.Split(';');

                                if (channelList.Contains("0+") || channelList.Contains("0*"))
                                {
                                    klineChannel = true;
                                }

                                if (channelList.Contains("1+") || channelList.Contains("1*"))
                                {
                                    dcanChannel = true;
                                }

                                if (channelList.Contains("3+") || channelList.Contains("3*"))
                                {
                                    enetChannel = true;
                                }
                            }

                            bool isFree = true;
                            if (attrDict.TryGetValue("OWNER", out string ownerString))
                            {
                                ownerString = ownerString.Trim();
                                if (!string.IsNullOrEmpty(ownerString) && !string.IsNullOrEmpty(UdpIcomOwnerFilter))
                                {
                                    if (string.Compare(ownerString, UdpIcomOwnerFilter, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        isFree = false;
                                    }
                                }
                            }

                            bool isDoIp = false;
                            if (attrDict.TryGetValue("DOIP", out string doIpString))
                            {
                                doIpString = doIpString.Trim();
                                if (!string.IsNullOrEmpty(doIpString))
                                {
                                    if (string.Compare(doIpString, "Yes", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        isDoIp = true;
                                    }
                                }
                            }

                            if (isFree)
                            {
                                if (enetChannel && gatewayAddr >= 0)
                                {
                                    if (isDoIp)
                                    {
                                        addListConnList.Add(new EnetConnection(EnetConnection.InterfaceType.Icom, ipAddressHost, -1, -1, IcomDoIpPortDefault, IcomSslPortDefault));
                                    }
                                    else
                                    {
                                        addListConnList.Add(new EnetConnection(EnetConnection.InterfaceType.Icom, ipAddressHost, IcomDiagPortDefault, IcomControlPortDefault));
                                    }
                                }
                                if (klineChannel || dcanChannel)
                                {
                                    addListConnList.Add(new EnetConnection(EnetConnection.InterfaceType.Icom, ipAddressHost, DiagPortRplusDefault));
                                }
                            }
                        }
                    }
                }

                foreach (EnetConnection addListConn in addListConnList)
                {
                    EnetConnection addConn = addListConn;
                    if (addConn != null)
                    {
                        if (UdpIpFilter != null && !UdpIpFilter.Equals(IPAddress.Any))
                        {
                            if (!UdpIpFilter.Equals(addConn.IpAddress))
                            {
                                addConn = null;
                            }
                        }
                    }

                    if (addConn != null)
                    {
                        if (UdpDiagPortFilter != null && addConn.DiagPort >= 0 && UdpDiagPortFilter != addConn.DiagPort)
                        {
                            addConn = null;
                        }
                    }

                    if (addConn != null)
                    {
                        if (UdpDoIpPortFilter != null && addConn.DoIpPort >= 0 && UdpDoIpPortFilter != addConn.DoIpPort)
                        {
                            addConn = null;
                        }
                    }

                    if (addConn != null)
                    {
                        if (UdpDoIpSslFilter != null && addConn.SslPort >= 0 && UdpDoIpSslFilter != addConn.SslPort)
                        {
                            addConn = null;
                        }
                    }

                    if (addConn != null)
                    {
                        addConn.Mac = vehicleMac;
                        addConn.Vin = vehicleVin;

                        int listCount = 0;
                        lock (UdpRecListLock)
                        {
                            if (UdpRecIpListList != null)
                            {
                                if (UdpRecIpListList.All(x => x != addConn))
                                {
                                    UdpRecIpListList.Add(addConn);
                                }

                                listCount = UdpRecIpListList.Count;
                            }
                        }

                        if ((UdpMaxResponses >= 1) && (listCount >= UdpMaxResponses))
                        {
                            UdpEvent.Set();
                            continueRec = false;
                            break;
                        }
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
                    IcomAllocateDeviceHttpClient.Timeout = TimeSpan.FromSeconds(5);
                }

                // ISTA: IVMUtils.ReserveVCIDeviceIcom, IVMUtils.ReleaseVCIDeviceIcom
                // The code here is base on iToolRadar and assigns only a device owner
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
                    // ISTA: IVMUtils.CreateRemoteClient
                    // The code here is base on iToolRadar and assigns only a device owner
                    IcomAllocateDeviceHttpClient.DefaultRequestHeaders.Authorization = null;
                    IcomAllocateDeviceHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Jakarta Commons-HttpClient/3.1");

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
                    SharedDataActive.TcpControlClient = new TcpClientWithTimeout(SharedDataActive.EnetHostConn.IpAddress, controlPort, ConnectTimeout, true)
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
                    sharedData.TcpControlStream.Dispose();
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
                    sharedData.TcpControlClient.Dispose();
                    sharedData.TcpControlClient = null;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        protected SslStream CreateSslStream(SharedData sharedData)
        {
            if (sharedData == null)
            {
                return null;
            }

            SslStream sslStream = new SslStream(sharedData.TcpDiagClient.GetStream(), false,
                (sender, certificate, chain, errors) =>
                {
                    try
                    {
                        if (errors == SslPolicyErrors.None)
                        {
                            return true;
                        }

                        foreach (X509Certificate2 trustedCertificate in sharedData.TrustedCAs)
                        {
                            try
                            {
                                using (X509Chain chain2 = new X509Chain())
                                {
                                    chain2.ChainPolicy.ExtraStore.Add(trustedCertificate);
                                    chain2.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreInvalidName;
                                    chain2.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                                    chain2.Build(new X509Certificate2(certificate));
                                    if (chain2.ChainStatus.Length == 0)
                                    {
                                        return true;
                                    }

                                    X509ChainStatusFlags status = chain2.ChainStatus.First().Status;
                                    switch (status)
                                    {
                                        case X509ChainStatusFlags.NoError:
                                            return true;

                                        case X509ChainStatusFlags.UntrustedRoot:
                                            if (chain2.ChainStatus.Length == 1 &&
                                                chain2.ChainPolicy.ExtraStore.Contains(chain2.ChainElements[chain2.ChainElements.Count - 1].Certificate))
                                            {
                                                return true;
                                            }
                                            break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "CreateSslStream exception: " + EdiabasNet.GetExceptionText(ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "CreateSslStream exception: " + EdiabasNet.GetExceptionText(ex));
                        return false;
                    }

                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** CreateSslStream Certificate error: {0}", errors);
                    return false;
                },
                (sender, host, certificates, certificate, issuers) =>
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Client certificate request Host={0}", host);
                    if (issuers != null && issuers.Length > 0 && certificates != null && certificates.Count > 0)
                    {
                        // Use the first certificate that is from an acceptable issuer.
                        foreach (System.Security.Cryptography.X509Certificates.X509Certificate cert in certificates)
                        {
                            string issuer = cert.Issuer;
                            if (Array.IndexOf(issuers, issuer) != -1)
                            {
                                return cert;
                            }
                        }
                    }

                    if (certificates != null && certificates.Count > 0)
                    {
                        return certificates[0];
                    }

                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Client certificate request not found Host={0}", host);
                    return null;
                });
            try
            {
                // Authenticate the server but don't require the client to authenticate.
                X509CertificateCollection clientCertificates = null;
                if (sharedData.S29Certs != null)
                {
                    clientCertificates = new X509CertificateCollection();
                    foreach (X509Certificate2 cert in sharedData.S29Certs)
                    {
                        clientCertificates.Add(cert);
                    }
                }
#if NET
                Thread abortThread = null;
                AutoResetEvent threadFinishEvent = null;
                ManualResetEvent cancelEvent = sharedData.TransmitCancelEvent;
                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource())
                    {
                        threadFinishEvent = new AutoResetEvent(false);
                        SslClientAuthenticationOptions authenticationOptions = new SslClientAuthenticationOptions();
                        authenticationOptions.TargetHost = string.Empty;
                        authenticationOptions.ClientCertificates = clientCertificates;
                        authenticationOptions.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;
                        System.Threading.Tasks.Task authTask = sslStream.AuthenticateAsClientAsync(authenticationOptions, cts.Token);
                        if (cancelEvent != null)
                        {
                            WaitHandle[] waitHandles = { threadFinishEvent, cancelEvent };
                            abortThread = new Thread(() =>
                            {
                                if (WaitHandle.WaitAny(waitHandles, SslAuthTimeout) == 1)
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    cts.Cancel();
                                }
                            });
                            abortThread.Start();
                        }

                        if (!authTask.Wait(SslAuthTimeout))
                        {
                            cts.Cancel();
                        }

                        if (authTask.Status != System.Threading.Tasks.TaskStatus.RanToCompletion || cts.IsCancellationRequested)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** CreateSslStream auth timeout");
                            sslStream.Close();
                            return null;  // aborted
                        }

                        if (cancelEvent != null)
                        {
                            if (cancelEvent.WaitOne(0))
                            {
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** CreateSslStream auth cancelled");
                                sslStream.Close();
                                return null;
                            }
                        }
                    }
                }
                finally
                {
                    if (abortThread != null)
                    {
                        threadFinishEvent.Set();
                        abortThread.Join();
                    }

                    threadFinishEvent?.Dispose();
                }
#else
                sslStream.ReadTimeout = SslAuthTimeout;
                sslStream.AuthenticateAsClient(string.Empty, clientCertificates, false);
#endif
                if (!sslStream.IsEncrypted || !sslStream.IsSigned)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** CreateSslStream not encrypted: Encrypted={0}, Signed={1}",
                        sslStream.IsEncrypted, sslStream.IsSigned);
                    sslStream.Close();
                    return null;
                }

                if (clientCertificates != null && clientCertificates.Count > 0)
                {
                    if (!sslStream.IsMutuallyAuthenticated)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** CreateSslStream not mutually authenticated");
                        return null;
                    }
                }
                return sslStream;
            }
            catch (AuthenticationException ex)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** CreateSslStream exception: " + EdiabasNet.GetExceptionText(ex));
                sslStream.Close();
                EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_SEC_0002);
                throw;
            }
        }

        protected Stream CreateBcSslStream(SharedData sharedData)
        {
            if (sharedData == null)
            {
                return null;
            }

            try
            {
                NetworkStream serverStream = sharedData.TcpDiagClient.GetStream();
                serverStream.ReadTimeout = SslAuthTimeout;
                TlsClientProtocol clientProtocol = new TlsClientProtocol(serverStream);
                clientProtocol.IsResumableHandshake = true;
                EdBcTlsClient tlsClient = new EdBcTlsClient(EdiabasProtected, sharedData.S29CertFiles, sharedData.TrustedCaStructs);
                tlsClient.HandshakeTimeout = SslAuthTimeout;
                clientProtocol.Connect(tlsClient);
                Stream sslStream = clientProtocol.Stream;
                sharedData.BcTlsClientProtocol = clientProtocol;
                return sslStream;
            }
            catch (Exception ex)
            {
                sharedData.BcTlsClientProtocol = null;
                if (ex is TlsException tlsException)
                {
                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** CreateBcSslStream TLS exception: " + EdiabasNet.GetExceptionText(tlsException));
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_SEC_0002);
                }
                else
                {
                    EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** CreateBcSslStream exception: " + EdiabasNet.GetExceptionText(ex));
                    EdiabasProtected?.SetError(EdiabasNet.ErrorCodes.EDIABAS_IFH_0003);
                }

                throw;
            }
        }

        protected bool GetTrustedCAs(SharedData sharedData, string certPath)
        {
            try
            {
                if (sharedData == null)
                {
                    return false;
                }

                sharedData.DisposeCAs();
                if (string.IsNullOrEmpty(certPath))
                {
                    return false;
                }

                if (!Directory.Exists(certPath))
                {
                    return false;
                }

                List<X509Certificate2> caList = new List<X509Certificate2>();
                List<X509CertificateStructure> caStructList = new List<X509CertificateStructure>();
                IEnumerable<string> certFiles = Directory.EnumerateFiles(certPath, "*.*", SearchOption.AllDirectories);
                foreach (string certFile in certFiles)
                {
                    try
                    {
                        string fileExt = Path.GetExtension(certFile);
                        if (string.IsNullOrEmpty(fileExt) || fileExt.Length < 2)
                        {
                            continue;
                        }

                        if (!fileExt.Skip(1).All(char.IsDigit))
                        {
                            continue;
                        }
#if NET9_0_OR_GREATER
                        X509Certificate2 cert = X509CertificateLoader.LoadCertificateFromFile(certFile);
#else
                        X509Certificate2 cert = new X509Certificate2(certFile);
#endif
                        caList.Add(cert);

                        X509CertificateStructure certStruct = EdBcTlsUtilities.LoadBcCertificateResource(certFile);
                        if (certStruct != null)
                        {
                            caStructList.Add(certStruct);
                        }
                    }
                    catch (Exception ex)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetTrustedCAs File {0}, Exception: {1}", certFile, EdiabasNet.GetExceptionText(ex));
                    }
                }

                sharedData.TrustedCAs = caList;
                sharedData.TrustedCaStructs = caStructList;
                return caList.Count > 0 | caStructList.Count > 0;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetTrustedCAs exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        protected bool CreateS29Certs(SharedData sharedData, string certPath)
        {
            try
            {
                if (sharedData != null)
                {
                    sharedData.DisposeS29Certs();
                }

                if (string.IsNullOrEmpty(certPath))
                {
                    return false;
                }

                if (!Directory.Exists(certPath))
                {
                    return false;
                }

                string machineName = EdSec4Diag.GetMachineName();
                string machinePrivateFile = Path.Combine(certPath, machineName + ".p12");
                string machinePublicFile = Path.Combine(certPath, machineName + EdSec4Diag.S29MachinePublicName);

                string p12Password;
                using (SHA256 algorithm = SHA256.Create())
                {
                    p12Password = Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(machineName.ToUpperInvariant())));
                }

                AsymmetricKeyParameter machineAsymmetricKeyPar = null;
                X509CertificateEntry[] machinePublicChain = null;
                AsymmetricCipherKeyPair machineKeyPair = null;

                for (int retry = 0; retry < 2; retry++)
                {
                    machineAsymmetricKeyPar = null;
                    machinePublicChain = null;
                    machineKeyPair = null;

                    if (File.Exists(machinePrivateFile))
                    {
                        try
                        {
                            AsymmetricKeyParameter asymmetricKeyPar = EdBcTlsUtilities.LoadPkcs12Key(machinePrivateFile, p12Password, out X509CertificateEntry[] publicChain);
                            if (asymmetricKeyPar == null || publicChain == null)
                            {
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs Load private cert failed: {0}", machinePrivateFile);
                            }
                            else
                            {
                                machineAsymmetricKeyPar = asymmetricKeyPar;
                                machinePublicChain = publicChain;
                                if (machinePublicChain.Length > 0)
                                {
                                    machineKeyPair = new AsymmetricCipherKeyPair(machinePublicChain[0].Certificate.GetPublicKey(), machineAsymmetricKeyPar);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs File {0}, Exception: {1}", machinePrivateFile, EdiabasNet.GetExceptionText(ex));
                        }
                    }

                    if (File.Exists(machinePublicFile) && machinePublicChain != null)
                    {
                        try
                        {
                            AsymmetricKeyParameter asymmetricKeyPar = EdBcTlsUtilities.LoadPemObject(machinePublicFile) as AsymmetricKeyParameter;
                            if (asymmetricKeyPar == null)
                            {
                                machineAsymmetricKeyPar = null;
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs Load public cert failed: {0}", machinePublicFile);
                            }
                            else
                            {
                                if (machinePublicChain.Length < 1 ||
                                    !machinePublicChain[0].Certificate.GetPublicKey().Equals(asymmetricKeyPar))
                                {
                                    machineAsymmetricKeyPar = null;
                                    machineKeyPair = null;
                                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs Load public cert different: {0}", machinePublicFile);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs File {0}, Exception: {1}", machinePublicFile, EdiabasNet.GetExceptionText(ex));
                        }
                    }

                    if (machineAsymmetricKeyPar != null && machinePublicChain != null)
                    {
                        break;
                    }

                    if (!EdBcTlsUtilities.GenerateEcKeyPair(machinePrivateFile, machinePublicFile, SecObjectIdentifiers.SecP384r1, p12Password))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs Generate private key file failed: {0}", machinePrivateFile);
                        break;
                    }
                }

                if (sharedData != null)
                {
                    List<X509Certificate2> certList = null;
                    List<EdBcTlsClient.CertInfo> certKeyList = null;
#if false
                    if (machineAsymmetricKeyPar != null && machinePublicChain != null)
                    {
                        string tempPath = Path.GetTempPath();
                        string privateTempFile = Path.Combine(tempPath, Path.GetTempFileName());
                        string publicTempFile = Path.Combine(tempPath, Path.GetTempFileName());

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (certList == null)
                        {
                            certList = new List<X509Certificate2>();
                        }

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (certKeyList == null)
                        {
                            certKeyList = new List<EdBcTlsClient.CertInfo>();
                        }

                        if (!EdBcTlsUtilities.ExtractPkcs12Key(machinePrivateFile, p12Password, privateTempFile, publicTempFile))
                        {
                            try
                            {
                                if (File.Exists(privateTempFile))
                                {
                                    File.Delete(privateTempFile);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            try
                            {
                                if (File.Exists(publicTempFile))
                                {
                                    File.Delete(publicTempFile);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        else
                        {
                            certKeyList.Add(new EdBcTlsClient.CertInfo(privateTempFile, publicTempFile, true));
                        }

                        try
                        {
#if NET9_0_OR_GREATER
                            X509Certificate2 cert = X509CertificateLoader.LoadPkcs12FromFile(machinePrivateFile, p12Password);
#else
                            X509Certificate2 cert = new X509Certificate2(machinePrivateFile, p12Password);
#endif
                            certList.Add(cert);
                        }
                        catch (Exception ex)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs Private key file {0}, Exception: {1}", machinePrivateFile, EdiabasNet.GetExceptionText(ex));
                        }
                    }
#endif
#if false
                    IEnumerable<string> certFiles = Directory.EnumerateFiles(certPath, "*.*", SearchOption.AllDirectories);
                    foreach (string certFile in certFiles)
                    {
                        string certExtension = Path.GetExtension(certFile);
                        if (string.IsNullOrEmpty(certExtension))
                        {
                            continue;
                        }

                        if (string.Compare(certExtension, ".key", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string publicCert = Path.ChangeExtension(certFile, ".pem");
                            if (File.Exists(publicCert))
                            {
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                if (certList == null)
                                {
                                    certList = new List<X509Certificate2>();
                                }

                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                if (certKeyList == null)
                                {
                                    certKeyList = new List<EdBcTlsClient.CertInfo>();
                                }

                                certKeyList.Add(new EdBcTlsClient.CertInfo(certFile, publicCert));
                                byte[] pkcs12Data = EdBcTlsUtilities.CreatePkcs12Data(publicCert, certFile);
                                if (pkcs12Data != null)
                                {
                                    try
                                    {
#if NET9_0_OR_GREATER
                                        X509Certificate2 cert = X509CertificateLoader.LoadCertificate(pkcs12Data);
#else
                                        X509Certificate2 cert = new X509Certificate2(pkcs12Data);
#endif
                                        certList.Add(cert);
                                    }
                                    catch (Exception ex)
                                    {
                                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs File {0}, Exception: {1}", certFile, EdiabasNet.GetExceptionText(ex));
                                    }
                                }
                            }
                        }
                    }
#endif
                    sharedData.S29Certs = certList;
                    sharedData.S29CertFiles = certKeyList;
                    sharedData.MachineKeyPair = machineKeyPair;
                }

                return true;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetS29Certs exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        protected bool LoadS29Cert(SharedData sharedData, string certPath, string selectFile)
        {
            try
            {
                if (sharedData == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(certPath) || !Directory.Exists(certPath))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "LoadS29Cert path not found: {0}", certPath);
                    return false;
                }

                string fileName = Path.Combine(certPath, selectFile + ".pem");
                if (!File.Exists(fileName))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "LoadS29Cert file not found: {0}", fileName);
                    return false;
                }

                List<X509CertificateStructure> certList = EdBcTlsUtilities.LoadBcCertificateResources(fileName);
                if (certList == null || certList.Count < 2)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "LoadS29Cert no certificates found in file: {0}", fileName);
                    return false;
                }

                List<Org.BouncyCastle.X509.X509Certificate> x509CertList =  EdBcTlsUtilities.ConvertToX509CertList(certList);
                List<Org.BouncyCastle.X509.X509Certificate> rootCerts = EdBcTlsUtilities.ConvertToX509CertList(sharedData.TrustedCaStructs);
                if (!EdBcTlsUtilities.ValidateCertChain(x509CertList, rootCerts))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "LoadS29Cert certificate chain validation failed: {0}", fileName);
                    return false;
                }

                sharedData.S29SelectCert = certList;
                return true;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "LoadS29Cert exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        protected bool CreateRequestJson(SharedData sharedData, string jsonRequestPath, EdSec4Diag.CertReqProfile.EnumType? certReqProfileType = null)
        {
            try
            {
                if (sharedData == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(jsonRequestPath) || !Directory.Exists(jsonRequestPath))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson path not found: {0}", jsonRequestPath);
                    return false;
                }

                string vin = sharedData.EnetHostConn?.Vin;
                if (string.IsNullOrWhiteSpace(vin))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson VIN is empty");
                    return false;
                }

                if (sharedData.MachineKeyPair == null || sharedData.MachineKeyPair.Public == null || sharedData.MachineKeyPair.Private == null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson machine key pair not available");
                    return false;
                }

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                serializer.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

                EdSec4Diag.Sec4DiagRequestData requestData;
                if (certReqProfileType == null)
                {
                    string templateJson = Path.Combine(jsonRequestPath, "template.json");
                    if (!File.Exists(templateJson))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson template file not found: {0}", templateJson);
                        return false;
                    }

                    using (StreamReader file = File.OpenText(templateJson))
                    {
                        requestData = serializer.Deserialize(file, typeof(EdSec4Diag.Sec4DiagRequestData)) as EdSec4Diag.Sec4DiagRequestData;
                    }

                    if (requestData == null)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson template file invalid: {0}", templateJson);
                        return false;
                    }
                }
                else
                {
                    requestData = new EdSec4Diag.Sec4DiagRequestData
                    {
                        CertReqProfile = certReqProfileType.Value.ToString()
                    };
                }

                string publicKey = EdBcTlsUtilities.ConvertPublicKeyToPEM(sharedData.MachineKeyPair.Public);
                if (string.IsNullOrEmpty(publicKey))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson Convert public key failed");
                    return false;
                }

                ECPrivateKeyParameters privateKey = sharedData.MachineKeyPair.Private as ECPrivateKeyParameters;
                if (privateKey == null)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson Private key is not ECPrivateKeyParameters");
                    return false;
                }

                string vin17 = vin.ToUpperInvariant().Trim();
                requestData.Vin17 = vin17;
                requestData.PublicKey = publicKey;
                requestData.ProofOfPossession = new EdSec4Diag.ProofOfPossession
                {
                    SignatureType = "SHA512withECDSA"
                };

                string signMessage = vin17 + requestData.CertReqProfile;
                string signature = EdBcTlsUtilities.SignData(signMessage, privateKey, requestData.ProofOfPossession.SignatureType);
                requestData.ProofOfPossession.Signature = signature;

                string requestFileName = "RequestContainer_service-29-" + requestData.CertReqProfile + "-" + vin17 + ".json";
                string requestJson = Path.Combine(jsonRequestPath, requestFileName);
                using (StreamWriter sw = new StreamWriter(requestJson))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, requestData);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "CreateRequestJson exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        protected string StoreResponseJsonCert(SharedData sharedData, string jsonResponseFile, string certPath)
        {
            try
            {
                if (sharedData == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(jsonResponseFile) || !File.Exists(jsonResponseFile))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert file not found: {0}", jsonResponseFile);
                    return null;
                }

                bool responseFileValid = false;
                try
                {
                    EdSec4Diag.Sec4DiagResponseData responseData;
                    using (StreamReader file = File.OpenText(jsonResponseFile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        responseData = serializer.Deserialize(file, typeof(EdSec4Diag.Sec4DiagResponseData)) as EdSec4Diag.Sec4DiagResponseData;
                    }

                    if (responseData == null)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert file invalid: {0}", jsonResponseFile);
                        return null;
                    }

                    if (responseData.Certificate == null || responseData.CertificateChain == null)
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert no certificates found in file: {0}", jsonResponseFile);
                        return null;
                    }

                    if (string.IsNullOrEmpty(responseData.Vin17))
                    {
                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert no VIN found in file: {0}", jsonResponseFile);
                        return null;
                    }

                    List<Org.BouncyCastle.X509.X509Certificate> fileCertChain = new List<Org.BouncyCastle.X509.X509Certificate>();
                    fileCertChain.Add(EdBcTlsUtilities.CreateCertificateFromBase64(responseData.Certificate));
                    foreach (string chainCert in responseData.CertificateChain)
                    {
                        fileCertChain.Add(EdBcTlsUtilities.CreateCertificateFromBase64(chainCert));
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (Org.BouncyCastle.X509.X509Certificate fileCert in fileCertChain)
                    {
                        if (!fileCert.IsValid(DateTime.UtcNow.AddHours(1.0)))
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh,
                                "StoreResponseJsonCert certificate not valid: {0}", fileCert.SubjectDN);
                            return null;
                        }

                        stringBuilder.AppendLine(EdBcTlsUtilities.BeginCertificate);
                        stringBuilder.AppendLine(Convert.ToBase64String(fileCert.GetEncoded()));
                        stringBuilder.AppendLine(EdBcTlsUtilities.EndCertificate);
                    }

                    string vin17 = responseData.Vin17.ToUpperInvariant().Trim();
                    string outputCertFileName = $"S29-{vin17}.pem";
                    string outputCertFile = Path.Combine(certPath, outputCertFileName);
                    string certContent = stringBuilder.ToString();
                    File.WriteAllText(outputCertFile, certContent);
                    responseFileValid = true;

                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert stored certificate: {0}", outputCertFile);
                    return vin17;
                }
                finally
                {
                    if (!responseFileValid)
                    {
                        try
                        {
                            File.Delete(jsonResponseFile);
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert Old file deleted: {0}", jsonResponseFile);
                        }
                        catch (Exception ex)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert Delete json file exception: {0}", EdiabasNet.GetExceptionText(ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCert exception: {0}", EdiabasNet.GetExceptionText(ex));
                return null;
            }
        }

        protected bool StoreResponseJsonCerts(SharedData sharedData, string jsonResponsePath, string certPath)
        {
            try
            {
                if (sharedData == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(jsonResponsePath) || !Directory.Exists(jsonResponsePath))
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCerts path not found: {0}", jsonResponsePath);
                    return false;
                }

                bool foundCerts = false;
                string vin = sharedData.EnetHostConn.Vin;
                DirectoryInfo directoryInfo = new DirectoryInfo(jsonResponsePath);
                FileInfo[] files = directoryInfo.GetFiles().OrderBy(p => p.LastWriteTime).ToArray();
                foreach (FileInfo fileInfo in files)
                {
                    string jsonFile = fileInfo.FullName;
                    string baseFileName = Path.GetFileName(jsonFile);
                    if (!baseFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string vin17 = StoreResponseJsonCert(sharedData, jsonFile, certPath);
                    if (!string.IsNullOrEmpty(vin17))
                    {
                        if (string.Compare(vin, vin17, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            foundCerts = true;
                        }
                    }
                }

                return foundCerts;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "StoreResponseJsonCerts exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        protected bool StartReadTcpDiag(int telLength)
        {
            Stream localStream = SharedDataActive.TcpDiagStream;
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
                Stream networkStream = SharedDataActive.TcpDiagStream;
                if (networkStream == null)
                {
                    return;
                }

                if (SharedDataActive.TcpDiagRecLen > 0 && !SharedDataActive.DiagRplus)
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

                int nextReadLength = 0;
                if (SharedDataActive.DiagRplus)
                {
                    nextReadLength = TcpDiagNmpReceiver(networkStream);
                }
                else
                {
                    nextReadLength = SharedDataActive.DiagDoIp ? TcpDiagDoIpReceiver(networkStream) : TcpDiagEnetReceiver(networkStream);
                }

                if (nextReadLength > 0)
                {
                    StartReadTcpDiag(nextReadLength);
                }
            }
            catch (Exception)
            {
                SharedDataActive.TcpDiagRecLen = 0;
            }
        }

        protected int TcpDiagEnetReceiver(Stream networkStream)
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
                        ClearNetworkStream(SharedDataActive.TcpDiagStream);
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

        protected int TcpDiagDoIpReceiver(Stream networkStream)
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
                        ClearNetworkStream(SharedDataActive.TcpDiagStream);
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

        protected int TcpDiagNmpReceiver(Stream networkStream)
        {
            int nextReadLength = 20;
            try
            {
                if (SharedDataActive.TcpDiagRecLen >= 20)
                {   // header received
                    int telLen = ((int)SharedDataActive.TcpDiagBuffer[5] << 8) | SharedDataActive.TcpDiagBuffer[4];
                    if (SharedDataActive.TcpDiagRecLen == telLen)
                    {   // telegram received
                        if (SharedDataActive.TcpDiagBuffer[0] != 0x4E || SharedDataActive.TcpDiagBuffer[1] != 0x4D ||
                            SharedDataActive.TcpDiagBuffer[2] != 0x50 || SharedDataActive.TcpDiagBuffer[3] != 0x40 ||
                            SharedDataActive.TcpDiagBuffer[6] != 0x14 || SharedDataActive.TcpDiagBuffer[7] != 0x00)     // NMP header length
                        {
                            EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                "*** NMP header invalid");
                            InterfaceDisconnect(true);
                            SharedDataActive.ReconnectRequired = true;
                            return nextReadLength;
                        }

                        if (SharedDataActive.TcpDiagBuffer[6] != 0x14 || SharedDataActive.TcpDiagBuffer[7] != 0x00)
                        {
                            EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                "*** NMP length duplicate invalid");
                            InterfaceDisconnect(true);
                            SharedDataActive.ReconnectRequired = true;
                            return nextReadLength;
                        }

                        bool isCompatible = true;
                        int dataOffset = 0x14;
                        int actionBlocks = (SharedDataActive.TcpDiagBuffer[11] << 8) | SharedDataActive.TcpDiagBuffer[10];
                        if (actionBlocks > 0)
                        {   // action block
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NMP action blocks: {0}", actionBlocks);
                            for (int block = 0; block < actionBlocks; block++)
                            {
                                if (dataOffset + 4 > telLen)
                                {
                                    EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                        "*** NMP invalid data block header length");
                                    InterfaceDisconnect(true);
                                    SharedDataActive.ReconnectRequired = true;
                                    return nextReadLength;
                                }

                                int blockLen = ((int)SharedDataActive.TcpDiagBuffer[dataOffset + 1] << 8) | SharedDataActive.TcpDiagBuffer[dataOffset];
                                int blockType = ((int)SharedDataActive.TcpDiagBuffer[dataOffset + 3] << 8) | SharedDataActive.TcpDiagBuffer[dataOffset + 2];
                                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NMP action block[{0}]: Len={1}, Type={2}", block, blockLen, blockType);
                                if (dataOffset + blockLen > telLen)
                                {
                                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP invalid block[{0}] size: {1}", block, blockLen);
                                    InterfaceDisconnect(true);
                                    SharedDataActive.ReconnectRequired = true;
                                    return nextReadLength;
                                }

                                switch (blockType)
                                {
                                    case 3:
                                    {
                                        int compatibility = ((int)SharedDataActive.TcpDiagBuffer[dataOffset + 5] << 8) | SharedDataActive.TcpDiagBuffer[dataOffset + 4];
                                        if (blockLen != 6 || compatibility != 1)
                                        {
                                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP compatibility invalid: Len={0}, Compat={1}", blockLen, compatibility);
                                            isCompatible = false;
                                        }
                                        break;
                                    }
                                }

                                dataOffset += blockLen;
                            }
                        }

                        EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "NMP data offset: {0}", dataOffset);
                        if (dataOffset + 6 > telLen ||
                            SharedDataActive.TcpDiagBuffer[dataOffset] != 0x54 || SharedDataActive.TcpDiagBuffer[dataOffset + 1] != 0x4D)
                        {
                            EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                "*** NMP invalid data block type");
                            InterfaceDisconnect(true);
                            SharedDataActive.ReconnectRequired = true;
                            return nextReadLength;
                        }

                        long dataLen = ((long)SharedDataActive.TcpDiagBuffer[dataOffset + 5] << 24) | ((long)SharedDataActive.TcpDiagBuffer[dataOffset + 4] << 16) | 
                                        ((long)SharedDataActive.TcpDiagBuffer[dataOffset + 3] << 8) | SharedDataActive.TcpDiagBuffer[dataOffset + 2];
                        if (dataLen < 6 || dataLen + dataOffset > telLen)
                        {
                            EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, SharedDataActive.TcpDiagBuffer, 0, SharedDataActive.TcpDiagRecLen,
                                "*** NMP data length invalid");
                            InterfaceDisconnect(true);
                            SharedDataActive.ReconnectRequired = true;
                            return nextReadLength;
                        }

                        lock (SharedDataActive.TcpDiagStreamRecLock)
                        {
                            if (SharedDataActive.TcpDiagRecQueue.Count > 256)
                            {
                                SharedDataActive.TcpDiagRecQueue.Dequeue();
                            }

                            byte[] recDataTel = Array.Empty<byte>();
                            if (isCompatible)
                            {
                                recDataTel = new byte[dataLen];
                                Array.Copy(SharedDataActive.TcpDiagBuffer, dataOffset + 6, recDataTel, 0, dataLen - 6);
                            }

                            SharedDataActive.TcpDiagRecQueue.Enqueue(recDataTel);
                            SharedDataActive.TcpDiagStreamRecEvent.Set();
                        }

                        SharedDataActive.TcpDiagRecLen = 0;
                    }
                    else if (SharedDataActive.TcpDiagRecLen > telLen)
                    {
                        SharedDataActive.TcpDiagRecLen = 0;
                    }
                    else if (telLen > SharedDataActive.TcpDiagBuffer.Length)
                    {   // telegram too large -> remove all
                        ClearNetworkStream(SharedDataActive.TcpDiagStream);
                        SharedDataActive.TcpDiagRecLen = 0;
                    }
                    else
                    {
                        nextReadLength = (int)telLen;
                    }
                }
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP Exception: {0}", EdiabasNet.GetExceptionText(ex));
                InterfaceDisconnect(true);
                SharedDataActive.ReconnectRequired = true;
                return nextReadLength;
            }

            return nextReadLength;
        }

        protected bool SendEnetData(byte[] sendData, int length, bool enableLogging)
        {
            for (int retries = 0; retries < 3; retries++)
            {
                try
                {
                    if (SharedDataActive.TcpDiagStream == null)
                    {
                        return false;
                    }

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

                    return true;
                }
                catch (Exception ex)
                {
                    if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "SendEnetData exception: {0}", EdiabasNet.GetExceptionText(ex));
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(true))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        SharedDataActive.ReconnectRequired = true;
                        return false;
                    }

                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected: retrying");
                }
            }

            return false;
        }

        protected bool SendDoIpData(byte[] sendData, int length, bool enableLogging)
        {
            for (int retries = 0; retries < 3; retries++)
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

                    return true;
                }
                catch (Exception ex)
                {
                    if (enableLogging) EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "SendDoIpData exception: {0}", EdiabasNet.GetExceptionText(ex));
                    InterfaceDisconnect(true);
                    if (!InterfaceConnect(true))
                    {
                        if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Reconnect failed");
                        SharedDataActive.ReconnectRequired = true;
                        return false;
                    }

                    if (enableLogging) EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "Reconnected: retrying");
                }
            }

            return false;
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

        protected List<NmpParameter> ReceiveNmpParameters(int timeout, int? channelCompare,int? nmpCounterCompare, EdiabasNet.IfhCommands? ifhCommandCompare)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return null;
            }

            try
            {
                int recLen = ReceiveTelegram(DataBuffer, timeout);
                if (recLen < 18)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** Invalid NMP message length: {0}", recLen);
                    return null;
                }

                if (DataBuffer[0] != 0x10 || DataBuffer[1] != 0x02 ||
                    DataBuffer[8] != 0x00 || DataBuffer[9] != 0x01)
                {
                    EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, DataBuffer, 0, recLen, "*** NMP message identifiers invalid");
                    return null;
                }

                int channel = (DataBuffer[5] << 8) | DataBuffer[4];
                int nmpCounter = (DataBuffer[7] << 8) | DataBuffer[6];
                int ifhCommandValue = (DataBuffer[13] << 8) | DataBuffer[12];

                if (channelCompare != null && channel != channelCompare.Value)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP message channel mismatch: {0} != {1}", channel, channelCompare.Value);
                    return null;
                }

                if (nmpCounterCompare != null && nmpCounter != nmpCounterCompare.Value)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP message counter mismatch: {0} != {1}", nmpCounter, nmpCounterCompare.Value);
                    return null;
                }

                EdiabasNet.IfhCommands ifhCommand = (EdiabasNet.IfhCommands)ifhCommandValue;
                if (ifhCommandCompare != null && ifhCommand != ifhCommandCompare.Value)
                {
                    EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP message IFH command mismatch: {0} != {1}", ifhCommand, ifhCommandCompare.Value);
                    return null;
                }

                int blockCount = (DataBuffer[17] << 8) | DataBuffer[16];
                int dataOffset = 18;
                List<NmpParameter> paramList = new List<NmpParameter>();

                for (int block = 0; block < blockCount; block++)
                {
                    NmpParameter nmpParameter = new NmpParameter(DataBuffer, recLen, dataOffset);
                    paramList.Add(nmpParameter);
                    dataOffset += nmpParameter.GetTelegramLength();
                }

                return paramList;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMP message exception: {0}", EdiabasNet.GetExceptionText(ex));
                return null;
            }
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

        protected EdiabasNet.ErrorCodes DoIpAuthenticate(SharedData sharedData)
        {
            byte[] authConfRequest = { 0x82, DoIpGwAddrDefault, 0xF1, 0x29, 0x08 };
            if (TransBmwFast(authConfRequest, authConfRequest.Length, ref AuthBuffer, out int receiveLength) != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending DoIp auth conf request failed");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            int dataLength = DataLengthBmwFast(AuthBuffer, out int dataOffset);
            if (dataLength < 3 || AuthBuffer[dataOffset + 0] != (0x29 | 0x40))
            {
                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AuthBuffer, 0, receiveLength, "*** DoIp auth conf response invalid");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            byte authConfig = AuthBuffer[dataOffset + 2];
            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "DoIp auth configuration: {0:X02}", authConfig);

            if (sharedData.S29SelectCert == null)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** No S29 certificate selected for DoIp authentication");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            List<byte> certRequest = new List<byte> { 0x80, DoIpGwAddrDefault, 0xF1,
                0x00, 0x00, 0x03,       // 16 bit length
                0x29, 0x01, 0x00 };
            List<byte> certBlock = new List<byte>();
            foreach (X509CertificateStructure selectCert in sharedData.S29SelectCert)
            {
                byte[] certBytes = selectCert.GetEncoded();
                AppendS29DataBlock(ref certBlock, certBytes);
            }

            AppendS29DataBlock(ref certRequest, certBlock.ToArray());
            AppendS29DataBlock(ref certRequest, Array.Empty<byte>());   // Ephemeral Public Key

            int certRequestLength = certRequest.Count - 6;
            certRequest[4] = (byte)(certRequestLength >> 8);
            certRequest[5] = (byte)(certRequestLength & 0xFF);
            if (TransBmwFast(certRequest.ToArray(), certRequest.Count, ref AuthBuffer, out receiveLength) != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending DoIp auth cert request failed");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            dataLength = DataLengthBmwFast(AuthBuffer, out dataOffset);
            if (dataLength < 3 + 2 + 16 || AuthBuffer[dataOffset + 0] != (0x29 | 0x40))
            {
                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AuthBuffer, 0, receiveLength, "*** DoIp auth cert response invalid");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            byte certCheckResponseType = AuthBuffer[dataOffset + 2];
            if (certCheckResponseType != 0x11)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** DoIp auth cert response type invalid: {0:X02}", certCheckResponseType);
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            List<byte[]> challengeList = GetS29ParameterList(AuthBuffer, dataLength + dataOffset, dataOffset + 3);
            if (challengeList == null || challengeList.Count < 1)
            {
                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AuthBuffer, 0, receiveLength, "*** DoIp auth challenge parameter invalid");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            byte[] serverChallenge = challengeList[0];
            if (serverChallenge.Length < 16)
            {
                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AuthBuffer, 0, receiveLength, "*** DoIp auth challenge too short");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            ECPrivateKeyParameters privateKey = sharedData.MachineKeyPair?.Private as ECPrivateKeyParameters;
            ECPublicKeyParameters publicKey = sharedData.MachineKeyPair?.Public as ECPublicKeyParameters;
            byte[] proofData = EdSec4Diag.CalculateProofOfOwnership(serverChallenge, privateKey);
            if (proofData == null)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** DoIp auth proof of ownership calculation failed");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            if (!EdSec4Diag.VerifyProofOfOwnership(proofData, serverChallenge, publicKey))
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** DoIp auth proof of ownership verification failed");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            List<byte> proofRequest = new List<byte> { 0x80, DoIpGwAddrDefault, 0xF1,
                0x00, 0x00, 0x03,       // 16 bit length
                0x29, 0x03 };
            AppendS29DataBlock(ref proofRequest, proofData);
            AppendS29DataBlock(ref proofRequest, Array.Empty<byte>());   // Ephemeral Public Key

            int proofRequestLength = proofRequest.Count - 6;
            proofRequest[4] = (byte)(proofRequestLength >> 8);
            proofRequest[5] = (byte)(proofRequestLength & 0xFF);
            if (TransBmwFast(proofRequest.ToArray(), proofRequest.Count, ref AuthBuffer, out receiveLength) != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** Sending DoIp auth proof request failed");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            dataLength = DataLengthBmwFast(AuthBuffer, out dataOffset);
            if (dataLength < 3 || AuthBuffer[dataOffset + 0] != (0x29 | 0x40))
            {
                EdiabasProtected?.LogData(EdiabasNet.EdLogLevel.Ifh, AuthBuffer, 0, receiveLength, "*** DoIp auth cert response invalid");
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            byte proofResponseType = AuthBuffer[dataOffset + 2];
            if (proofResponseType != 0x12)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** DoIp auth proof response type invalid: {0:X02}", proofResponseType);
                return EdiabasNet.ErrorCodes.EDIABAS_SEC_0036;
            }

            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        public static void ClearNetworkStream(Stream tcpClientStream)
        {
            if (tcpClientStream == null)
            {
                return;
            }


            NetworkStream diagNetworkStream = tcpClientStream as NetworkStream;
            SslStream diagSslStream = tcpClientStream as SslStream;
            if (diagNetworkStream != null)
            {
                while (diagNetworkStream.DataAvailable)
                {
                    diagNetworkStream.ReadByte();
                }
            }

            if (diagSslStream != null)
            {
                diagSslStream.ReadTimeout = 1;
                while (diagSslStream.ReadByte() >= 0)
                {
                }
                diagSslStream.ReadTimeout = SslAuthTimeout;
            }
        }

        public static void AppendS29DataBlock(ref List<byte> buffer, byte[] dataBlock)
        {
            int length = dataBlock.Length;
            buffer.Add((byte) (length >> 8));
            buffer.Add((byte) (length & 0xFF));
            if (length > 0)
            {
                buffer.AddRange(dataBlock);
            }
        }

        public static byte[] GetS29DataBlock(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + 2)
            {
                return null;
            }

            int length = (buffer[offset] << 8) + buffer[offset + 1];
            if (buffer.Length < offset + 2 + length)
            {
                return null;
            }

            byte[] parameter = new byte[length];
            Array.Copy(buffer, offset + 2, parameter, 0, length);
            return parameter;
        }

        public static List<byte[]> GetS29ParameterList(byte[] buffer, int bufferLength, int offset, int maxEntries = int.MaxValue)
        {
            List<byte[]> parameters = new List<byte[]>();
            while (offset < bufferLength)
            {
                byte[] parameter = GetS29DataBlock(buffer, offset);
                if (parameter == null)
                {
                    return null;
                }

                parameters.Add(parameter);
                offset += 2 + parameter.Length; // move to next parameter

                if (parameters.Count >= maxEntries)
                {
                    break;
                }
            }

            return parameters;
        }

        public static List<byte> GetNmpFrameTelegram(int channel, int nmpCounter, EdiabasNet.IfhCommands ifhCommand, List<NmpParameter> nmpParamList = null, List<byte[]> actionBlocks = null)
        {
            List<byte> nmpFrame = new List<byte>();
            nmpFrame.Add(0x4E);
            nmpFrame.Add(0x4D);
            nmpFrame.Add(0x50);
            nmpFrame.Add(0x40);

            // NMP telegram length
            nmpFrame.Add(0x00);
            nmpFrame.Add(0x00);

            // NMP header length
            nmpFrame.Add(0x14);
            nmpFrame.Add(0x00);

            nmpFrame.Add(0x00);
            nmpFrame.Add(0x02);

            int actionBlockCount = actionBlocks?.Count ?? 0;
            nmpFrame.Add((byte)actionBlockCount);
            nmpFrame.Add((byte)(actionBlockCount >> 8));

            nmpFrame.Add((byte)nmpCounter);
            nmpFrame.Add((byte)(nmpCounter >> 8));

            nmpFrame.Add(0x00);
            nmpFrame.Add(0x00);

            // NMP telegram length (copy)
            nmpFrame.Add(0x00);
            nmpFrame.Add(0x00);

            nmpFrame.Add(0x00);
            nmpFrame.Add(0x00);

            if (actionBlocks != null)
            {
                foreach (byte[] actionBlock in actionBlocks)
                {
                    int actionBlockSize = actionBlock.Length + 2;
                    nmpFrame.Add((byte)actionBlockSize);
                    nmpFrame.Add((byte)(actionBlockSize >> 8));
                    nmpFrame.AddRange(actionBlock);
                }
            }

            List<byte> nmpMessage = GetNmpMessageTelegram(channel, nmpCounter, ifhCommand, nmpParamList);
            nmpFrame.AddRange(nmpMessage);

            int nmpFrameSize = nmpFrame.Count;
            nmpFrame[4] = (byte)nmpFrameSize;
            nmpFrame[5] = (byte)(nmpFrameSize >> 8);

            nmpFrame[16] = (byte)nmpFrameSize;
            nmpFrame[17] = (byte)(nmpFrameSize >> 8);

            return nmpFrame;
        }

        public static List<byte> GetNmpMessageTelegram(int channel, int nmpCounter, EdiabasNet.IfhCommands ifhCommand, List<NmpParameter> nmpParamList = null)
        {
            List<byte> content = new List<byte>();
            content.Add(0x10);
            content.Add(0x02);
            content.Add(0x00);
            content.Add(0x00);

            content.Add((byte)channel);
            content.Add((byte)(channel >> 8));

            content.Add((byte)nmpCounter);
            content.Add((byte)(nmpCounter >> 8));

            content.Add(0x00);
            content.Add(0x01);
            content.Add(0x00);
            content.Add(0x00);

            int ifhCommandValue = (int)ifhCommand;
            content.Add((byte)ifhCommandValue);
            content.Add((byte)(ifhCommandValue >> 8));

            content.Add(0x00);
            content.Add(0x00);

            int parameterCount = nmpParamList?.Count ?? 0;
            content.Add((byte)parameterCount);
            content.Add((byte)(parameterCount >> 8));

            if (nmpParamList != null)
            {
                foreach (NmpParameter nmpParameter in nmpParamList)
                {
                    content.AddRange(nmpParameter.GetTelegram());
                }
            }

            List<byte> nmpMessage = new List<byte>();
            nmpMessage.Add(0x54);
            nmpMessage.Add(0x4D);

            int dataLength = content.Count + 6;
            nmpMessage.Add((byte)(dataLength));
            nmpMessage.Add((byte)(dataLength >> 8));
            nmpMessage.Add((byte)(dataLength >> 16));
            nmpMessage.Add((byte)(dataLength >> 24));

            nmpMessage.AddRange(content);
            return nmpMessage;
        }

        private List<NmpParameter> TransNmpParameters(int timeout, int channel, EdiabasNet.IfhCommands ifhCommand, List<NmpParameter> nmpParamList = null, List<byte[]> actionBlocks = null)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return null;
            }

            try
            {
                int paramCount = nmpParamList?.Count ?? 0;
                int actionCount = actionBlocks?.Count ?? 0;
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "TransNmpParameters Timeout={0}, Channel={1}, Command={2}, Params={3}, Actions={4}", timeout, channel, ifhCommand, paramCount, actionCount);

                SharedDataActive.NmpCounter++;
                int nmpCounter = SharedDataActive.NmpCounter;
                List<byte> nmpFrame = GetNmpFrameTelegram(channel, nmpCounter, ifhCommand, nmpParamList, actionBlocks);
                lock (SharedDataActive.TcpDiagStreamSendLock)
                {
                    WriteNetworkStream(SharedDataActive.TcpDiagStream, nmpFrame.ToArray(), 0, nmpFrame.Count);
                }

                List<NmpParameter> paramListRec = ReceiveNmpParameters(timeout, channel, nmpCounter, ifhCommand);
                if (paramListRec == null)
                {
                    return null;
                }

                return paramListRec;
            }
            catch (Exception ex)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "TransNmpParameters exception: " + EdiabasNet.GetExceptionText(ex));
                return null;
            }
        }

        protected EdiabasNet.ErrorCodes NmtInit(string unit, string application)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            string unitPar = unit ?? string.Empty;
            if (unitPar.Length > 1)
            {
                unitPar = unitPar.Substring(0, 1);
            }

            string appPar = application ?? string.Empty;
            if (appPar.Length > 8)
            {
                appPar = appPar.Substring(0, 8);
            }

            int timeout = ConnectTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(1),
                new NmpParameter(unitPar),
                new NmpParameter(appPar)
            };

            List<byte[]> actionBlocks = new List<byte[]>()
            {
                new byte[] {0x01, 0x00, 0x01, 0x00, 0x00},          // identification, len 1, empty
                new byte[] {0x02, 0x00, 0x00, 0x01, 0x00, 0x00}     // version 0x100
            };

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, 0, EdiabasNet.IfhCommands.IfhInit, paramListSend, actionBlocks);
            if (paramListRec == null || paramListRec.Count < 1)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT init failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[0].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT init invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtEnd()
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = ConnectTimeout;
            List<NmpParameter> paramListRec = TransNmpParameters(timeout, 0, EdiabasNet.IfhCommands.IfhEnd);
            if (paramListRec == null || paramListRec.Count < 1)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT end failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[0].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT end invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtConnect(string sgbd)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = ConnectTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(sgbd),
            };

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, 0, EdiabasNet.IfhCommands.IfhConnect, paramListSend);
            if (paramListRec == null || paramListRec.Count < 1)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT connect failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[0].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT connect invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtOpenChannel()
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListRec = TransNmpParameters(timeout, 0, EdiabasNet.IfhCommands.IfhOpenChannel);
            if (paramListRec == null || paramListRec.Count < 2)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT open channel failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[0].GetErrorCode();
            int? channel = paramListRec[1].GetInteger();
            if (errorCode == null || channel == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT open channel invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                SharedDataActive.NmpChannel = channel.Value;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtCloseChannel()
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhCloseChannel);
            SharedDataActive.NmpChannel = 0;

            if (paramListRec == null || paramListRec.Count < 1)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT close channel failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[0].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT close channel invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtNotifyConfig(string name, int parameterId, EdiabasNet.IfhParameterType parameterType, string parameter)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int parameterValue = 0;
            string parameterString = null;

            switch (parameterType)
            {
                case EdiabasNet.IfhParameterType.CFGTYPE_PATH:
                case EdiabasNet.IfhParameterType.CFGTYPE_STRING:
                    parameterValue = 0xFFFF;
                    parameterString = parameter ?? string.Empty;
                    break;

                case EdiabasNet.IfhParameterType.CFGTYPE_INT:
                case EdiabasNet.IfhParameterType.CFGTYPE_BOOL:
                {
                    long stringValue = EdiabasNet.StringToValue(parameter ?? string.Empty, out bool valid);
                    if (valid)
                    {
                        if (parameterType == EdiabasNet.IfhParameterType.CFGTYPE_BOOL)
                        {
                            parameterValue = stringValue != 0 ? 1 : 0;
                            break;
                        }

                        parameterValue = (int) stringValue;
                    }
                    break;
                }
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(name),
                new NmpParameter(parameterId, parameterType, parameterValue),
            };

            if (parameterString != null)
            {
                paramListSend.Add(new NmpParameter(parameterString));
            }

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhNotifyConfig, paramListSend);
            if (paramListRec == null || paramListRec.Count < 1)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT notify config failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[0].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT notify config invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtSetParameter(byte[] parameter)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(parameter),
            };

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhSetParameter, paramListSend);
            if (paramListRec == null || paramListRec.Count < 4)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT set parameter failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[2].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT set parameter invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtSetPreface(byte[] preface)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(preface),
            };

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhSetTelPreface, paramListSend);
            if (paramListRec == null || paramListRec.Count < 4)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT set preface failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[2].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT set preface invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtSendTelegram(byte[] requestData, out byte[] responseData)
        {
            responseData = null;
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(requestData),
            };

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhSendTelegram, paramListSend);
            if (paramListRec == null || paramListRec.Count < 4)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT send telegram failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[2].GetErrorCode();
            responseData = paramListRec[3].GetBinary();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT send telegram invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtSendTelegramFreq(byte[] requestData)
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListSend = new List<NmpParameter>()
            {
                new NmpParameter(requestData),
            };

            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhSendTelegramFreq, paramListSend);
            if (paramListRec == null || paramListRec.Count < 4)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT send telegram freq failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[2].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT send telegram freq invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtReqTelegramFreq(out byte[] responseData)
        {
            responseData = null;
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhSendTelegram);
            if (paramListRec == null || paramListRec.Count < 4)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT req telegram freq failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[2].GetErrorCode();
            responseData = paramListRec[3].GetBinary();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT req telegram freq invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }

        protected EdiabasNet.ErrorCodes NmtStopFreqTelegram()
        {
            if (SharedDataActive.TcpDiagStream == null)
            {
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            int timeout = RplusFunctionTimeout;
            List<NmpParameter> paramListRec = TransNmpParameters(timeout, SharedDataActive.NmpChannel, EdiabasNet.IfhCommands.IfhStopFreqTelegram);
            if (paramListRec == null || paramListRec.Count < 4)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NMT send telegram failed");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            EdiabasNet.ErrorCodes? errorCode = paramListRec[2].GetErrorCode();
            if (errorCode == null)
            {
                EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NMT send telegram invalid parameters");
                return EdiabasNet.ErrorCodes.EDIABAS_IFH_0019;
            }

            return errorCode.Value;
        }


        protected EdiabasNet.ErrorCodes NmtOpenConnection()
        {
            EdiabasNet.ErrorCodes errorCode = NmtInit(UnitName, ApplicationName);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NmtInit failed");
                return errorCode;
            }

            if (EdiabasProtected != null)
            {
                List<Tuple<string, int, EdiabasNet.IfhParameterType>> configProperties = new List<Tuple<string, int, EdiabasNet.IfhParameterType>>()
                {
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("UBattHandling", 3, EdiabasNet.IfhParameterType.CFGTYPE_BOOL),
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("IgnitionHandling", 4, EdiabasNet.IfhParameterType.CFGTYPE_BOOL),
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("TracePath", 19, EdiabasNet.IfhParameterType.CFGTYPE_PATH),
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("SystemTraceIfh", 8, EdiabasNet.IfhParameterType.CFGTYPE_INT),
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("IfhTrace", 10, EdiabasNet.IfhParameterType.CFGTYPE_INT),
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("SimulationPath", 11, EdiabasNet.IfhParameterType.CFGTYPE_PATH),
                    new Tuple<string, int, EdiabasNet.IfhParameterType>("Simulation", 12, EdiabasNet.IfhParameterType.CFGTYPE_BOOL),
                };

                foreach (Tuple<string, int, EdiabasNet.IfhParameterType> configProperty in configProperties)
                {
                    string configValue = EdiabasProtected.GetConfigProperty(configProperty.Item1);
                    if (configValue != null)
                    {
                        errorCode = NmtNotifyConfig(configProperty.Item1, configProperty.Item2, configProperty.Item3, configValue);
                        if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                        {
                            EdiabasProtected?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** NmtNotifyConfig {0} failed", configProperty.Item1);
                            return errorCode;
                        }
                    }
                }
            }

            string sgbd = EdiabasProtected?.SgbdFileName ?? string.Empty;
            if (!string.IsNullOrEmpty(sgbd))
            {
                sgbd = Path.GetFileNameWithoutExtension(sgbd);
            }

            errorCode = NmtConnect(sgbd);
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NmtConnect failed");
                return errorCode;
            }

            errorCode = NmtOpenChannel();
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NmtOpenChannel failed");
                return errorCode;
            }

            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
        }

        protected EdiabasNet.ErrorCodes NmtCloseConnection()
        {
            EdiabasNet.ErrorCodes errorCode = NmtCloseChannel();
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NmtCloseChannel failed");
                return errorCode;
            }

            errorCode = NmtEnd();
            if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
            {
                EdiabasProtected?.LogString(EdiabasNet.EdLogLevel.Ifh, "*** NmtEnd failed");
                return errorCode;
            }

            return EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
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

        private void WriteNetworkStream(Stream networkStream, byte[] buffer, int offset, int size, int packetSize = TcpSendBufferSize)
        {
            if (networkStream == null)
            {
#if !ANDROID
                Debug.WriteLine("WriteNetworkStream No stream");
#endif
                throw new IOException("No network stream");
            }

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
#if !ANDROID
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
                            IcomAllocateDeviceHttpClient.CancelPendingRequests();
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
