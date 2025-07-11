//#define MAP_ISOTP_ECU
#if DEBUG
#define CAN_DEBUG
#endif
#define CAN_DYN_LEN
#define VCDS
using BmwFileReader;
using EdiabasLib;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.X509;
using Peak.Can.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

// ReSharper disable RedundantAssignment
// ReSharper disable RedundantCast
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable IdentifierTypo

namespace CarSimulator
{
    public class CommThread : IDisposable
    {
        public class ConfigData
        {
            public ConfigData()
            {
                ConfigList = new List<byte[]>();
                ResponseOnlyList = new List<ResponseEntry>();
                ResponseList = new List<ResponseEntry>();
            }
            public List<byte[]> ConfigList { get; }
            public List<ResponseEntry> ResponseOnlyList { get; }
            public List<ResponseEntry> ResponseList { get; }
        }

        public class ResponseEntry
        {
            private int _responseIndex;

            public ResponseEntry(byte[] request, byte[] response, byte[] config)
            {
                Request = request;
                ResponseMultiList = new List<byte[]>();
                ResponseList = new List<byte[]> {response};
                Config = config;
                _responseIndex = 0;
            }

            public void Reset()
            {
                _responseIndex = 0;
            }

            public byte[] Request { get; }

            public byte[] ResponseDyn
            {
                get
                {
                    if (_responseIndex >= ResponseList.Count)
                    {
                        _responseIndex = 0;
                    }
                    return ResponseList[_responseIndex++];
                }
            }

            public List<byte[]> ResponseList { get; }

            public List<byte[]> ResponseMultiList { get; }

            public byte[] Config { get; }
        }

        private class DynamicUdsEntry
        {
            public DynamicUdsEntry(int ecuAddr, int dataId, List<DynamicUdsValue> udsValueList)
            {
                EcuAddr = ecuAddr;
                DataId = dataId;
                UdsValueList = udsValueList;
            }

            public int EcuAddr { get; }

            public int DataId { get; }

            public List<DynamicUdsValue> UdsValueList { get; }
        }

        private class DynamicUdsValue
        {
            public DynamicUdsValue(int dataId, int dataPos, int dataLength, ResponseEntry responseEntry)
            {
                DataId = dataId;
                DataPos = dataPos;
                DataLength = dataLength;
                ResponseEntry = responseEntry;
            }

            public int DataId { get; }

            public int DataPos { get; }

            public int DataLength { get; }

            public ResponseEntry ResponseEntry { get; }
        }

        private class BmwTcpClientData
        {
            public BmwTcpClientData(BmwTcpChannel bmwTcpChannel, int index, bool isDoIp = false, bool doIpSsl = false)
            {
                BmwTcpChannel = bmwTcpChannel;
                Index = index;
                IsDoIp = isDoIp;
                DoIpSsl = doIpSsl;
                TcpClientConnection = null;
                TcpClientStream = null;
                LastTcpRecTick = DateTime.MinValue.Ticks;
                LastTcpSendTick = DateTime.MinValue.Ticks;
                LastTesterAddress = null;
                ClientPublicKey = null;
                ServerChallenge = null;
            }

            public int UsedDoIpPort => DoIpSsl ? BmwTcpChannel.DoIpSslPort : BmwTcpChannel.DoIpPort;

            public readonly BmwTcpChannel BmwTcpChannel;
            public readonly int Index;
            public readonly bool IsDoIp;
            public readonly bool DoIpSsl;
            public TcpClient TcpClientConnection;
            public Stream TcpClientStream;
            public long LastTcpRecTick;
            public long LastTcpSendTick;
            public uint? LastTesterAddress;
            public int TcpNackIndex;
            public int TcpDataIndex;
            public byte[] TcpLastResponse;
            public ECPublicKeyParameters ClientPublicKey;
            public byte[] ServerChallenge;
        }

        private class BmwTcpChannel
        {
            public BmwTcpChannel(int diagPort, int controPort, int doIpPort = -1, int doIpSslPort = -1)
            {
                DiagPort = diagPort;
                ControlPort = controPort;
                DoIpPort = doIpPort;
                DoIpSslPort = doIpSslPort;
                TcpClientDiagList = new List<BmwTcpClientData>();
                TcpClientControlList = new List<BmwTcpClientData>();
                TcpClientDoIpList = new List<BmwTcpClientData>();
                TcpClientDoIpSslList = new List<BmwTcpClientData>();
                for (int i = 0; i < 10; i++)
                {
                    TcpClientDiagList.Add(new BmwTcpClientData(this, i));
                    TcpClientControlList.Add(new BmwTcpClientData(this, i));
                    TcpClientDoIpList.Add(new BmwTcpClientData(this, i, true));
                    TcpClientDoIpSslList.Add(new BmwTcpClientData(this, i, true, true));
                }
            }

            public readonly int DiagPort;
            public readonly int ControlPort;
            public readonly int DoIpPort;
            public readonly int DoIpSslPort;
            public TcpListener TcpServerDiag;
            public readonly List<BmwTcpClientData> TcpClientDiagList;
            public TcpListener TcpServerControl;
            public readonly List<BmwTcpClientData> TcpClientControlList;
            public TcpListener TcpServerDoIp;
            public readonly List<BmwTcpClientData> TcpClientDoIpList;
            public TcpListener TcpServerDoIpSsl;
            public readonly List<BmwTcpClientData> TcpClientDoIpSslList;
        }

        private class Tp20Channel
        {
            public Tp20Channel()
            {
                BlockSize = 0x0F;
                T1 = 0x8A;
                T3 = 0x32;
                RecSeq = 0;
                SendSeq = 0;
                SendData = new List<byte[]>();
            }
            private byte _t1;
            private byte _t3;

            public long LastAccessTick { get; set; }
            public long LastKeepAliveTick { get; set; }
            public byte EcuAddress { get; set; }
            public byte TelAddress { get; set; }
            public byte AppId { get; set; }
            public int TxId { get; set; }
            public int RxId { get; set; }
            public byte BlockSize { get; set; }
            public byte T1
            {
                // ReSharper disable once UnusedMember.Local
                get => _t1;
                set
                {
                    _t1 = value;
                    T1Time = ConvertTp20Time(value);
                }
            }
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public double T1Time { get; private set; }
            public byte T3
            {
                // ReSharper disable once UnusedMember.Local
                get => _t3;
                set
                {
                    _t3 = value;
                    T3Time = ConvertTp20Time(value);
                }
            }
            public double T3Time { get; private set; }

            private static double ConvertTp20Time(byte time)
            {
                double scale = time & 0x3F;
                double unit = 1.0;
                switch ((time >> 6) & 0x03)
                {
                    case 0x00:
                        unit = 0.1;
                        break;

                    case 0x01:
                        unit = 1.0;
                        break;

                    case 0x02:
                        unit = 10.0;
                        break;

                    case 0x03:
                        unit = 100.0;
                        break;
                }
                return scale * unit;
            }

            public byte RecSeq { get; set; }
            public byte SendSeq { get; set; }
            public int RecLen { get; set; }
            public int SendPos { get; set; }
            public int SendBlock { get; set; }
            public bool WaitForAck { get; set; }
            public long AckWaitStartTick { get; set; }
            public bool WaitForKeepAliveResp { get; set; }
            public bool SendDelay { get; set; }
            public long SendDelayStartTick { get; set; }
            public List<byte> RecData { get; set; }
            public List<byte[]> SendData { get; }
        }

        public enum ConceptType
        {
            ConceptBwmFast,
            ConceptKwp2000Bmw,
            ConceptKwp2000S,
            ConceptDs2,
            Concept1,
            ConceptKwp1281,     // Concept2 (ISO9141)
            Concept3,
            ConceptKwp2000,     // VW
            ConceptTp20,        // VW CAN
        };

        public enum ResponseType
        {
            Standard,
            E61,
            E90,
            G31,
            SMG2,
        };

        [Flags]
        public enum EnetCommType
        {
            None = 0x00,
            Hsfz = 0x01,
            DoIp = 0x02,
        };

        private static readonly long TickResolMs = Stopwatch.Frequency/1000;
        private const byte TcpTesterAddr = 0xF4;
        private const int DoIpTesterAddrMin = 0x0EF3;
        private const int DoIpTesterAddrMax = 0x0EF4;
        private const int EnetDiagPort = 6801;
        private const int EnetControlPort = 6811;
        private const int EnetDiagPrgPort = 51560;
        private const int EnetControlPrgPort = 51561;
        private const int DoIpDiagPort = 13400;
        private const int DoIpProtoVer = EdInterfaceEnet.DoIpProtoVer;
        private const int SrvLocPort = 427;
        // Make sure that on the OBD interface side of the ICOM only the IP4 protocol is enabled in the interface settings!
        // Otherwise, there is packet loss in the ICOM internally!
        // Use PC connection via WiFi or 100Mbit network, this also reduces the traffic.
        private const int MaxBufferLength = 0x10000;
        private const int TcpSendBufferSize = 1400;
        private const int TcpSendTimeout = 5000;
        private const int SslAuthTimeout = 5000;
        private const int Kwp2000Nr2123Retries = 3;
        private const int ResetAdaptionChannel = 0;
        private const int DefaultAdaptionChannelValue = 0x1234;
        private const int FunctOrgAddr = 0x7DF;
        private const int FunctSubstAddr = 0x7E0;   // motor
        private bool _disposed;
        private readonly MainForm _form;
        private volatile bool _stopThread;
        private bool _threadRunning;
        private Thread _workerThread;
        private string _comPort;
        private ConceptType _conceptType;
        private ConceptType _responseConcept;
        private bool _adsAdapter;
        private bool _klineResponder;
        private ResponseType _responseType;
        private ConfigData _configData;
        private EnetCommType _enetCommType;
        private bool _testMode;
        private bool _isoTpMode;
        private ushort _pcanHandle;
        private long _lastCanSendTick;
#if CAN_DEBUG
        private long _lastCanReceiveTick;
#endif
        private long _lastCanStatusTick;
#if MAP_ISOTP_ECU
        private uint _originalEcuId;
        private uint _originalTesterId;
#endif
        private readonly List<Tp20Channel> _tp20Channels;
        private readonly List<DynamicUdsEntry> _dynamicUdsEntries;
        private readonly Dictionary<byte, byte[]> _codingStampDict;
        private readonly List<BmwTcpChannel> _bmwTcpChannels;
        private X509Certificate2 _serverCertificate;
        private List<X509CertificateStructure> _serverCAs;
        private UdpClient _udpClient;
        private bool _udpError;
        private UdpClient _udpDoIpClient;
        private bool _udpDoIpError;
        private string _lastDoIpIdentAddr;
        private UdpClient _srvLocClient;
        private IcomDhcpServer _icomDhcpServer;
        private bool _srvLocError;
        private bool _icomUp;
        private int _icomIdentBroadcastCount;
        private readonly Stopwatch _timeIcomIdentBroadcast;
        private readonly SerialPort _serialPort;
        private readonly AutoResetEvent _serialReceiveEvent;
        private readonly AutoResetEvent _pcanReceiveEvent;
        private Random _random = new Random();
        private readonly Object _networkChangeLock = new Object();
        private readonly byte[] _sendData;
        private readonly byte[] _receiveData;
        private readonly byte[] _receiveDataMotorBackup;
        private int _doIpGwAddr;
        private int _noResponseCount;
        private int _nr2123SendCount;
#pragma warning disable 414
        private int _kwp1281InvRespIndex;
        private int _kwp1281InvEchoIndex;
        private int _kwp1281InvBlockEndIndex;
#pragma warning restore 414
        private byte _kwp2000AdaptionStatus;
        private int _kwp2000AdaptionChannel;
        private int _kwp2000AdaptionValue;
        private readonly Stopwatch[] _timeValveWrite = new Stopwatch[10];
        private byte _mode; // 2: conveyor, 4: transport
        private int _outputs; // 0:left, 1:right, 2:down, 3:comp
        private readonly Stopwatch _outputsActiveTime = new Stopwatch();
        private int _axisPosPrescaler;
        private int _axisPosRaw;
        private double _axisPosFilt;
        private int _batteryVoltage;
        private int _speed;
        private int _compressorRunningTime;
        private int _idleSpeedControl;
        private readonly List<byte> _ecuErrorResetList;
        private readonly Stopwatch _timeIdleSpeedControlWrite;
        private readonly Stopwatch _receiveStopWatch;

        private const string TestMac = "D8182B890A8B";
        private const string TestVin = "WBAJM71000B055940";
        private const string IcomVehicleAddress = "192.168.11.1";
        private const string IcomDhcpAddress = "192.168.201.1";

        private const double FilterConst = 0.95;
        private const int IsoTimeout = 2000;
        private const int IsoAckTimeout = 100;
        private const int Tp20T1 = 100;
        private const int Tp20T3 =
#if VCDS
                                    10;
#else
                                    0;
#endif
        // ReSharper disable InconsistentNaming
        // 0x38 EHC
        private readonly byte[] _response381802FFFF = {
            0x85, 0xF1, 0x38, 0x58, 0x01, 0x5F, 0xB4, 0x60};

        private readonly byte[] _response38175FB4 = {
            0x8F, 0xF1, 0x38, 0x57, 0x01, 0x5F, 0xB4, 0x60, 0x01, 0x28,
            0x44, 0x53, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        private readonly byte[] _response381A80 = {
            0x9F, 0xF1, 0x38, 0x5A, 0x80, 0x00, 0x00, 0x06, 0x78, 0x43,
            0x14, 0x04, 0x11, 0x02, 0xB0, 0x4E, 0x4C, 0x20, 0x07, 0x04,
            0x23, 0x66, 0x00, 0x10, 0x72, 0x06, 0x3F, 0x01, 0x03, 0x01,
            0x04, 0x00, 0x00, 0x00};

        private readonly byte[] _response3821C2 = {
            0x90, 0xF1, 0x38, 0x61, 0xC2, 0x03, 0xA1, 0x04, 0x01, 0x01,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x00, 0x00, 0x8A};

        private readonly byte[] _response382230 = {
            0x80, 0xF1, 0x38, 0xA7, 0x62, 0x30, 0x00, 0x07, 0x11, 0x00,
            0x01, 0xF4, 0x01, 0xF4, 0x01, 0xF4, 0x01, 0xAC, 0x03, 0x72,
            0x06, 0xC2, 0x01, 0x26, 0x02, 0x04, 0xFF, 0xBC, 0x00, 0xBC,
            0x00, 0x52, 0x02, 0xAE, 0x01, 0xFF, 0xFF, 0x28, 0x00, 0xB8,
            0x03, 0xFF, 0xFF, 0xFF, 0xAA, 0xE0, 0x10, 0xE0, 0x0B, 0x32,
            0x04, 0x11, 0x13, 0x84, 0x03, 0xB0, 0x04, 0x10, 0x0E, 0x84,
            0x03, 0x78, 0x05, 0x0E, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0x06, 0x00, 0x0A, 0x00, 0xD0, 0x20, 0x1A, 0x23, 0x32,
            0xFF, 0x3C, 0x05, 0x32, 0xFF, 0x19, 0x03, 0x18, 0x1C, 0x1E,
            0xFF, 0x40, 0x9C, 0xF7, 0x09, 0xF9, 0x07, 0xD9, 0x00, 0xD0,
            0x07, 0xFA, 0x06, 0xFC, 0x04, 0xFF, 0xFF, 0xF7, 0x09, 0xF9,
            0x07, 0xF7, 0x09, 0xF9, 0x07, 0x00, 0x00, 0x1E, 0x1E, 0xFF,
            0xFF, 0xF1, 0xFF, 0x0F, 0x00, 0x91, 0xE6, 0x6F, 0x19, 0x91,
            0xE6, 0x6F, 0x19, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0A, 0xC8,
            0x00, 0x64, 0x00, 0xFD, 0xFF, 0x64, 0x00, 0x0A, 0x01, 0x12,
            0x05, 0x12, 0x0A, 0x03, 0x64, 0x08, 0x64, 0x08, 0x64, 0x01,
            0xB4, 0x00, 0xFC, 0x04, 0xFC, 0x04, 0x01, 0xFF, 0x64, 0xDE,
            0x42};

        private readonly byte[] _response121A80 = {
            0xBC, 0xF1, 0x12, 0x5A, 0x80, 0x00, 0x00, 0x07, 0x80, 0x81,
            0x25, 0x00, 0x00, 0x00, 0x12, 0x4C, 0x50, 0x20, 0x08, 0x02,
            0x15, 0x08, 0x08, 0x02, 0x30, 0x39, 0x34, 0x37, 0x03, 0x03,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x79, 0x51, 0x46,
            0x31, 0x65, 0x57, 0x28, 0x30, 0x30, 0x38, 0x39, 0x51, 0x39,
            0x30, 0x30, 0x30, 0x38, 0x39, 0x51, 0x39, 0x30, 0x41, 0x39,
            0x34, 0x37, 0x42};

        private readonly byte[] _response121A94 = {
            0x8C, 0xF1, 0x12, 0x5A, 0x94, 0x31, 0x30, 0x33, 0x37, 0x33,
            0x38, 0x39, 0x38, 0x38, 0x32};

        private readonly byte[] _response122120 = {
            0x8C, 0xF1, 0x12, 0x61, 0x20, 0x4F, 0x5F, 0x46, 0x31, 0x52,
            0x39, 0x34, 0x37, 0x20, 0x20};

        private readonly byte[] _response12224021 = {
            0x97, 0xF1, 0x12, 0x62, 0x40, 0x21, 0x39, 0x31, 0x33, 0x32,
            0x32, 0x35, 0x30, 0x06, 0x39, 0xB9, 0x20, 0x04, 0x3C, 0x39,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        private readonly byte[] _response12224022 = {
            0x80, 0xF1, 0x12, 0x4D, 0x62, 0x40, 0x22, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x36, 0x02, 0x35, 0xF7, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0x36, 0x02, 0x33, 0x47, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF,
            0xFF, 0x00, 0x88, 0x4B, 0xFF, 0x00, 0x0E, 0x00, 0xF5, 0x00,
            0x00};

        private readonly byte[] _response12224023 = {
            0xA0, 0xF1, 0x12, 0x62, 0x40, 0x23, 0x5B, 0x00, 0xAA, 0x00,
            0x00, 0x01, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x4D, 0x4F, 0xFF,
            0xFF, 0xFF, 0xFF, 0xAA, 0xB3, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x15, 0x00, 0x00, 0x00};

        private readonly byte[] _response12230000000740 = {
            0x80, 0xF1, 0x12, 0x41, 0x63, 0x40, 0x43, 0x5A, 0x34, 0x38,
            0x39, 0x36, 0x38, 0x20, 0x09, 0x01, 0x12, 0x00, 0x00, 0x07,
            0x81, 0x17, 0x42, 0x00, 0x00, 0x07, 0x81, 0x17, 0x48, 0x00,
            0x00, 0x07, 0x79, 0x67, 0x25, 0x01, 0x11, 0x11, 0x31, 0x32,
            0x33, 0x34, 0x35, 0x4C, 0x30, 0x30, 0x38, 0x39, 0x51, 0x39,
            0x30, 0x41, 0x39, 0x34, 0x37, 0x42, 0x57, 0x42, 0x41, 0x50,
            0x58, 0x31, 0x31, 0x30, 0x35, 0x30, 0xFF, 0xFF, 0xFF};

        private readonly byte[] _response12230000400740 = {
            0x80, 0xF1, 0x12, 0x41, 0x63, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};

        private readonly byte[] _response121802FFFF = {
            0x85, 0xF1, 0x12, 0x58, 0x01, 0x42, 0x32, 0x24};

        private readonly byte[] _response12174232 = {
            0xA2, 0xF1, 0x12, 0x57, 0x01, 0x42, 0x32, 0x24, 0x06, 0x00,
            0x00, 0x21, 0x28, 0x42, 0x3F, 0x1F, 0x43, 0x36, 0x2F, 0x51,
            0x64, 0x00, 0x58, 0x00, 0x93, 0x43, 0xD0, 0x1F, 0x43, 0x37,
            0x31, 0x51, 0x64, 0x00, 0x57, 0x00, 0x93};

            // ReSharper disable once UnusedMember.Local
        private readonly byte[] _response12Nr = {
            0x83, 0xF1, 0x12, 0x7F, 0x1A, 0x78 };

        // Device A0 CCCA
        private readonly byte[] _responseA01802FFFF = {
            0x82, 0xF1, 0xA0, 0x58, 0x00};

        private readonly byte[] _responseA01A80p1 = {
            0x83, 0xF1, 0xA0, 0x7F, 0x1A, 0x78};

        private readonly byte[] _responseA01A80p2 = {
            0x9F, 0xF1, 0xA0, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x19, 0x38,
            0x71, 0xC4, 0x0C, 0x09, 0x30, 0x4B, 0x49, 0x20, 0x07, 0x05,
            0x28, 0x10, 0x00, 0x0A, 0x94, 0x08, 0x6A, 0x01, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00};

        private readonly byte[] _responseA0222000 = {
            0x84, 0xF1, 0xA0, 0x62, 0x20, 0x00, 0x00};

        private readonly byte[] _responseA022F121 = {
            0x89, 0xF1, 0xA0, 0x62, 0xF1, 0x21, 0x00, 0x01, 0x00, 0x00, 0x07, 0xF3};

        private readonly byte[] _responseA022F120p1 = {
            0x83, 0xF1, 0xA0, 0x7F, 0x22, 0x78};

        private readonly byte[] _responseA022F120p2 = {
            0x86, 0xF1, 0xA0, 0x62, 0xF1, 0x20, 0x00, 0x03, 0x01};

        private readonly byte[] _responseA022F122p1 = {
            0x83, 0xF1, 0xA0, 0x7F, 0x22, 0x78};

        private readonly byte[] _responseA022F122p2 = {
            0x85, 0xF1, 0xA0, 0x62, 0xF1, 0x22, 0x00, 0x00};

        private readonly byte[] _responseA022F123 = {
            0x97, 0xF1, 0xA0, 0x62, 0xF1, 0x23, 0x00, 0x23, 0x29, 0xC3,
            0x3F, 0x06, 0x12, 0x7E, 0xFE, 0x00, 0x00, 0x00, 0x66, 0x01,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x01};

        private readonly byte[] _responseA022F124 = {
            0x97, 0xF1, 0xA0, 0x62, 0xF1, 0x24, 0x00, 0x23, 0x29, 0xFA,
            0x97, 0x06, 0x0F, 0xC8, 0x3B, 0x00, 0x00, 0x00, 0x5C, 0x01,
            0x00, 0x00, 0x01, 0x7E, 0xFF, 0x01};

        private readonly byte[] _responseA022F125 = {
            0x8A, 0xF1, 0xA0, 0x62, 0xF1, 0x25, 0x00, 0x00, 0xD6, 0x00,
            0x84, 0x00, 0xFB};

        private readonly byte[] _responseA022F127 = {
            0x8D, 0xF1, 0xA0, 0x62, 0xF1, 0x27, 0x00, 0xC6, 0xA4, 0x06,
            0x17, 0x20, 0x56, 0x46, 0x01, 0x01};

        private readonly byte[] _responseA022F128 = {
            0x80, 0xF1, 0xA0, 0x7E, 0x62, 0xF1, 0x28, 0x00, 0x0A, 0x0B,
            0x01, 0x11, 0x58, 0xBE, 0x93, 0x10, 0x5B, 0x01, 0x01, 0x01,
            0x03, 0x10, 0xF4, 0x84, 0x44, 0x31, 0x11, 0x01, 0x01, 0x01,
            0x0B, 0x12, 0x34, 0xC7, 0x1C, 0x1D, 0xDD, 0x01, 0x01, 0x01,
            0x0E, 0x0E, 0xD8, 0x4F, 0xA4, 0x11, 0x11, 0x01, 0x01, 0x01,
            0x12, 0x0F, 0x8C, 0x23, 0x8E, 0x0C, 0xCC, 0x01, 0x01, 0x01,
            0x13, 0x10, 0xF4, 0xA6, 0x66, 0x3B, 0x05, 0x01, 0x01, 0x01,
            0x16, 0x11, 0xBC, 0x30, 0x5B, 0x25, 0xB0, 0x01, 0x01, 0x01,
            0x1B, 0x11, 0x08, 0x67, 0xD2, 0x27, 0x1C, 0x01, 0x01, 0x01,
            0x00, 0x11, 0xF8, 0x91, 0x11, 0x12, 0x7D, 0x01, 0x01, 0x01,
            0x10, 0x00, 0x00, 0x85, 0xB0, 0x04, 0xFA, 0x01, 0x00, 0x01,
            0x1C, 0x0F, 0x8C, 0xE7, 0xD2, 0x09, 0xF4, 0x01, 0x01, 0x01,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        // device 0x40 (CAS car access system)
        private readonly byte[] _response401A90 = {
            0x89, 0xF1, 0x40, 0x5A, 0x90, 0x43, 0x5A, 0x34, 0x38, 0x39,
            0x36, 0x38};

        private readonly byte[] _response401802FFFF = {
            0x85, 0xF1, 0x40, 0x58, 0x01, 0xA1, 0x17, 0x21};

        // device 0x60
        private readonly byte[] _response601A80 = {
            0x9F, 0xF1, 0x60, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x19, 0x61,
            0x08, 0x03, 0x07, 0x06, 0xE0, 0x53, 0x59, 0x20, 0x07, 0x05,
            0x31, 0x10, 0x00, 0x15, 0xB1, 0x89, 0x51, 0x00, 0x03, 0x10,
            0x01, 0x00, 0x00, 0x00};

        private readonly byte[] _response60210B = {
            0x86, 0xF1, 0x60, 0x61, 0x0B, 0x00, 0x02, 0x1F, 0x7E};

        private readonly byte[] _response602117 = {
            0x83, 0xF1, 0x60, 0x61, 0x17, 0x0C};

        // device 0x70
        private readonly byte[] _response70221000 = {
            0x86, 0xF1, 0x70, 0x62, 0x10, 0x00, 0xAD, 0xE8, 0xD2};

        private readonly byte[] _response701A80 = {
            0xBC, 0xF1, 0x70, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x20, 0x30,
            0x82, 0x08, 0x35, 0x0D, 0x60, 0x53, 0x52, 0x20, 0x07, 0x05,
            0x29, 0x09, 0x00, 0x10, 0x70, 0x04, 0x3C, 0x00, 0x04, 0x00,
            0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x12, 0x94, 0x78,
            0x00, 0x05, 0x21, 0x67, 0x30, 0x30, 0x39, 0x31, 0x42, 0x35,
            0x30, 0x30, 0x30, 0x39, 0x31, 0x42, 0x35, 0x30, 0x46, 0x34,
            0x35, 0x30, 0x41};

        private readonly byte[] _response701A90 = {
            0x89, 0xF1, 0x70, 0x5A, 0x90, 0x43, 0x5A, 0x34, 0x38, 0x39,
            0x36, 0x38};

        private readonly byte[] _response70230000000712 = {
            0x93, 0xF1, 0x70, 0x63, 0x12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0x20, 0x07, 0x05, 0x29, 0x00, 0x00, 0x09, 0x15,
            0x32, 0x73};

        private readonly byte[] _response70230000120712 = {
            0x93, 0xF1, 0x70, 0x63, 0x01, 0x43, 0x5A, 0x34, 0x38, 0x39,
            0x36, 0x38, 0x20, 0x13, 0x03, 0x05, 0x00, 0x00, 0x09, 0x20,
            0x30, 0x82};

        // device 0x73 CID
        private readonly byte[] _response731A80 = {
            0x9F, 0xF1, 0x73, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x15, 0x19,
            0x79, 0x17, 0x02, 0x0A, 0x30, 0x49, 0x41, 0x20, 0x07, 0x05,
            0x25, 0x17, 0x00, 0x0B, 0xF5, 0x06, 0x09, 0x00, 0x03, 0x03,
            0x00, 0x00, 0x00, 0x00};

        private readonly byte[] _response731802FFFF = {
            0x82, 0xF1, 0x73, 0x58, 0x00};

        // device 0x78 IHK2
        private readonly byte[] _response781A80 = {
            0x9F, 0xF1, 0x78, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x24, 0x87,
            0x02, 0x15, 0x0D, 0x07, 0x92, 0x47, 0x4C, 0x20, 0x07, 0x05,
            0x25, 0x21, 0x00, 0x15, 0x06, 0x05, 0x3D, 0xFF, 0x03, 0x03,
            0x3C, 0x00, 0x00, 0x00};

        private readonly byte[] _response781802FFFF = {
            0x82, 0xF1, 0x78, 0x58, 0x00};

        // device 0x64 PDC
        private readonly byte[] _response641A80 = {
            0x9F, 0xF1, 0x64, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x18, 0x51,
            0x39, 0x01, 0x02, 0x04, 0x21, 0x4C, 0x57, 0x20, 0x07, 0x05,
            0x31, 0x11, 0x00, 0x0C, 0x5F, 0x09, 0x0F, 0x00, 0x03, 0x03,
            0x1E, 0x00, 0x00, 0x00, 0x59};

        private readonly byte[] _response641802FFFF = {
            0x85, 0xF1, 0x64, 0x58, 0x01, 0xE2, 0x05, 0x24};

        private readonly byte[] _response6417E205 = {
            0x8C, 0xF1, 0x64, 0x57, 0x01, 0xE2, 0x05, 0x24, 0x01, 0x44,
            0x4A, 0x7B, 0x00, 0x00, 0x00};

        // device 0x65 SZM
        private readonly byte[] _response651A80 = {
            0x9F, 0xF1, 0x65, 0x5A, 0x80, 0x00, 0x00, 0x09, 0x18, 0x32,
            0x33, 0x03, 0x04, 0x07, 0x18, 0x44, 0x55, 0x20, 0x06, 0x08,
            0x15, 0x05, 0x00, 0x15, 0x1A, 0x02, 0x05, 0x01, 0x03, 0x03,
            0x3C, 0x00, 0x00, 0x00};

        private readonly byte[] _response651802FFFF = {
            0x85, 0xF1, 0x65, 0x58, 0x01, 0x9F, 0xF1, 0x24};

        private readonly byte[] _response65179FF1 = {
            0x89, 0xF1, 0x65, 0x57, 0x01, 0x9F, 0xF1, 0x24, 0x00, 0x00,
            0x44, 0x0E};

        private readonly byte[] _response6521F907 = {
            0x85, 0xF1, 0x65, 0x61, 0xF9, 0x07, 0x00, 0x00};

        // device 0x78 IHK
        private readonly byte[] _response78300201 = {
            0x9F, 0xF1, 0x78, 0x70, 0x02, 0x01, 0xC3, 0x28, 0x50, 0x64,
            0x69, 0x65, 0x3F, 0xFF, 0xFF, 0x0E, 0x10, 0x0E, 0x10, 0x59,
            0x53, 0x00, 0xAA, 0xAA, 0xC8, 0x00, 0x00, 0x00, 0xFF, 0xFB,
            0x00, 0x00, 0x00, 0x0F};

        private readonly byte[] _response78300601 = {
            0x86, 0xF1, 0x78, 0x70, 0x06, 0x01, 0x00, 0x00, 0x00};
        // ReSharper restore InconsistentNaming

        private readonly byte[] _zgwCert = {
            0x30, 0x82, 0x02, 0x98, 0x30, 0x82, 0x01, 0xC1, 0x02, 0x02, 0x4E, 0x86,
            0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01,
            0x05, 0x05, 0x00, 0x30, 0x67, 0x31, 0x13, 0x30, 0x11, 0x06, 0x0A, 0x09,
            0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x19, 0x16, 0x03, 0x63,
            0x6F, 0x6D, 0x31, 0x18, 0x30, 0x16, 0x06, 0x0A, 0x09, 0x92, 0x26, 0x89,
            0x93, 0xF2, 0x2C, 0x64, 0x01, 0x19, 0x16, 0x08, 0x62, 0x6D, 0x77, 0x67,
            0x72, 0x6F, 0x75, 0x70, 0x31, 0x0C, 0x30, 0x0A, 0x06, 0x03, 0x55, 0x04,
            0x0A, 0x13, 0x03, 0x70, 0x6B, 0x69, 0x31, 0x14, 0x30, 0x12, 0x06, 0x03,
            0x55, 0x04, 0x0B, 0x13, 0x0B, 0x62, 0x6D, 0x77, 0x2D, 0x66, 0x7A, 0x67,
            0x2D, 0x70, 0x6B, 0x69, 0x31, 0x12, 0x30, 0x10, 0x06, 0x03, 0x55, 0x04,
            0x03, 0x13, 0x09, 0x46, 0x5A, 0x47, 0x53, 0x65, 0x63, 0x2D, 0x43, 0x41,
            0x30, 0x1E, 0x17, 0x0D, 0x30, 0x37, 0x30, 0x38, 0x31, 0x34, 0x30, 0x39,
            0x30, 0x31, 0x35, 0x39, 0x5A, 0x17, 0x0D, 0x33, 0x32, 0x30, 0x37, 0x33,
            0x30, 0x31, 0x32, 0x34, 0x36, 0x33, 0x38, 0x5A, 0x30, 0x81, 0x82, 0x31,
            0x13, 0x30, 0x11, 0x06, 0x0A, 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C,
            0x64, 0x01, 0x19, 0x16, 0x03, 0x63, 0x6F, 0x6D, 0x31, 0x18, 0x30, 0x16,
            0x06, 0x0A, 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x19,
            0x16, 0x08, 0x62, 0x6D, 0x77, 0x67, 0x72, 0x6F, 0x75, 0x70, 0x31, 0x0C,
            0x30, 0x0A, 0x06, 0x03, 0x55, 0x04, 0x0A, 0x13, 0x03, 0x70, 0x6B, 0x69,
            0x31, 0x14, 0x30, 0x12, 0x06, 0x03, 0x55, 0x04, 0x0B, 0x13, 0x0B, 0x62,
            0x6D, 0x77, 0x2D, 0x66, 0x7A, 0x67, 0x2D, 0x70, 0x6B, 0x69, 0x31, 0x14,
            0x30, 0x12, 0x06, 0x03, 0x55, 0x04, 0x03, 0x13, 0x0B, 0x43, 0x6F, 0x64,
            0x69, 0x65, 0x72, 0x2D, 0x53, 0x69, 0x67, 0x6E, 0x31, 0x17, 0x30, 0x15,
            0x06, 0x0A, 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x01,
            0x13, 0x07, 0x44, 0x45, 0x56, 0x45, 0x4C, 0x4F, 0x50, 0x30, 0x81, 0x9D,
            0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01,
            0x01, 0x05, 0x00, 0x03, 0x81, 0x8B, 0x00, 0x30, 0x81, 0x87, 0x02, 0x81,
            0x81, 0x00, 0xBA, 0x86, 0x1F, 0x46, 0x75, 0x04, 0x5E, 0xA9, 0xCB, 0x99,
            0x26, 0x91, 0x42, 0x9E, 0x35, 0x5B, 0xDE, 0x8C, 0xA1, 0xD9, 0x8B, 0xF3,
            0x80, 0x83, 0x79, 0xBC, 0xCC, 0xCB, 0xED, 0xB7, 0x92, 0x6B, 0xB8, 0x25,
            0x3D, 0x0C, 0xE8, 0xA6, 0xFB, 0x9D, 0x79, 0x65, 0xA5, 0x2B, 0xAB, 0x7C,
            0x21, 0x23, 0xB3, 0x27, 0x97, 0x16, 0x4C, 0x00, 0x17, 0x72, 0xFF, 0xC7,
            0x4B, 0xCF, 0xC7, 0x74, 0xAE, 0x72, 0xD4, 0x7E, 0xCB, 0x2A, 0x93, 0x13,
            0x41, 0xA0, 0x6B, 0xF3, 0x8F, 0x7E, 0xFD, 0x4E, 0x9A, 0x75, 0x61, 0x15,
            0xA5, 0x57, 0x24, 0xAA, 0xEB, 0x38, 0x18, 0xBB, 0xA6, 0x18, 0x10, 0x3F,
            0x64, 0x95, 0x76, 0xF2, 0xCA, 0x05, 0x98, 0xAB, 0xF7, 0x40, 0x69, 0x0C,
            0xEF, 0xD9, 0x10, 0xCB, 0xF5, 0xC0, 0x5C, 0xBF, 0xDD, 0x00, 0xEF, 0xC3,
            0x7B, 0xD1, 0x78, 0xC9, 0x8E, 0x64, 0x14, 0x89, 0x22, 0xFD, 0x02, 0x01,
            0x03, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01,
            0x01, 0x05, 0x05, 0x00, 0x03, 0x81, 0xC1, 0x00, 0x07, 0x6E, 0x22, 0x3E,
            0x8F, 0x69, 0x80, 0x0C, 0xE0, 0xB9, 0x68, 0xEF, 0x0F, 0x1D, 0x4A, 0x5E,
            0xC2, 0x30, 0x41, 0xA1, 0x76, 0x68, 0xD5, 0xEC, 0x6F, 0x17, 0x9B, 0xF8,
            0x54, 0x89, 0xEC, 0x25, 0x5D, 0x14, 0x87, 0x79, 0x52, 0xF6, 0x2F, 0x52,
            0x1F, 0x84, 0xFD, 0xD1, 0x94, 0x75, 0x53, 0xAA, 0x06, 0x6E, 0x88, 0xAB,
            0x30, 0x37, 0x73, 0xDD, 0x05, 0x93, 0x58, 0xC8, 0x89, 0xDA, 0x6A, 0x6C,
            0xF0, 0xD0, 0xFE, 0x77, 0xD0, 0x8D, 0x40, 0x5D, 0xB0, 0xC4, 0x1B, 0x99,
            0x7A, 0x6F, 0xC0, 0x27, 0x23, 0x8F, 0xD9, 0x39, 0xD8, 0x86, 0x26, 0xF6,
            0xE2, 0xBB, 0x39, 0x91, 0x1D, 0x05, 0xAD, 0x27, 0x36, 0x5E, 0x1B, 0x09,
            0xA5, 0xA3, 0x32, 0x0D, 0x5C, 0x81, 0x84, 0x79, 0x19, 0x5F, 0xCD, 0x4C,
            0x3B, 0xF9, 0xF8, 0x36, 0x52, 0xB5, 0xDE, 0x62, 0x9B, 0x03, 0xB9, 0x75,
            0x27, 0x9B, 0x4E, 0xBE, 0x51, 0x5A, 0x83, 0x1F, 0x75, 0xC1, 0x26, 0x39,
            0xCF, 0xD1, 0x26, 0x36, 0x89, 0xDD, 0x16, 0x9A, 0x46, 0x46, 0xA7, 0x60,
            0x65, 0xA4, 0x16, 0xB6, 0xF0, 0xD5, 0x86, 0x18, 0x21, 0x5F, 0xA2, 0x83,
            0xC8, 0x89, 0x69, 0xEA, 0x2C, 0x0B, 0x94, 0x19, 0x13, 0x51, 0xFB, 0x91,
            0x8F, 0x71, 0x89, 0x31, 0x9F, 0xD4, 0xFF, 0x33, 0x1E, 0xE2, 0xCD, 0xF1,
            0xED, 0xC9, 0x7B, 0xCE, 0x55, 0x42, 0x31, 0x1A
        };

        public bool Moving
        {
            get;
            set;
        }

        public bool VariableValues
        {
            get;
            set;
        }

        public bool IgnitionOk
        {
            get;
            set;
        }

        public bool ErrorDefault
        {
            get;
            set;
        }

        public string ServerCertFile
        {
            get;
            set;
        }

        public string ServerCertPwd
        {
            get;
            set;
        }

        public int ServerSslPort
        {
            get;
            set;
        }

        public bool ServerUseBcSsl
        {
            get;
            set;
        }

        public bool Connected
        {
            get;
            private set;
        }

        public CommThread(MainForm form)
        {
            _form = form;
            _stopThread = false;
            _threadRunning = false;
            _workerThread = null;
            _pcanHandle = PCANBasic.PCAN_NONEBUS;
            _lastCanSendTick = 0;
#if CAN_DEBUG
            _lastCanReceiveTick = Stopwatch.GetTimestamp();
#endif
            _lastCanStatusTick = 0;
            _tp20Channels = new List<Tp20Channel>();
            _dynamicUdsEntries = new List<DynamicUdsEntry>();
            _codingStampDict = new Dictionary<byte, byte[]>();
            _bmwTcpChannels = new List<BmwTcpChannel>();
            _udpClient = null;
            _udpError = false;
            _udpDoIpClient = null;
            _udpDoIpError = false;
            _lastDoIpIdentAddr = null;
            _srvLocClient = null;
            _srvLocError = false;
            _icomUp = false;
            _icomIdentBroadcastCount = 0;
            _timeIcomIdentBroadcast = new Stopwatch();
            _serialPort = new SerialPort();
            _serialReceiveEvent = new AutoResetEvent(false);
            _pcanReceiveEvent = new AutoResetEvent(false);
            _sendData = new byte[MaxBufferLength];
            _receiveData = new byte[MaxBufferLength];
            _receiveDataMotorBackup = new byte[_receiveData.Length];
            _doIpGwAddr = EdInterfaceEnet.DoIpGwAddrDefault + 0;
            _noResponseCount = 0;
            _nr2123SendCount = 0;
            _kwp1281InvRespIndex = 0;
            _kwp1281InvEchoIndex = 0;
            _kwp1281InvBlockEndIndex = 0;

            _kwp2000AdaptionStatus = 0x01;
            _kwp2000AdaptionChannel = -1;
            _kwp2000AdaptionValue = DefaultAdaptionChannelValue;

            for (int i = 0; i < _timeValveWrite.Length; i++)
            {
                _timeValveWrite[i] = new Stopwatch();
            }
            _mode = 0x00;
            _outputs = 0x00;
            _outputsActiveTime.Stop();
            _axisPosPrescaler = 0;
            _axisPosRaw = 0;
            _axisPosFilt = _axisPosRaw;
            _batteryVoltage = 1445;
            _speed = 0;
            _compressorRunningTime = 0;
            _idleSpeedControl = 0;
            _ecuErrorResetList = new List<byte>();
            _timeIdleSpeedControlWrite = new Stopwatch();
            _receiveStopWatch = new Stopwatch();
            Moving = false;
            VariableValues = false;
            IgnitionOk = false;

            System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += (sender, args) =>
            {
                UpdateIcomStatus();
            };
        }

        public bool StartThread(string comPort, ConceptType conceptType, bool adsAdapter, bool klineResponder, ResponseType responseType, ConfigData configData, EnetCommType enetCommType, bool testMode = false)
        {
            try
            {
                StopThread();
                _stopThread = false;
                _comPort = comPort;
                _conceptType = conceptType;
                _responseConcept = conceptType;
                _adsAdapter = adsAdapter;
                _klineResponder = klineResponder;
                _responseType = responseType;
                _configData = configData;
                _enetCommType = enetCommType;
                _testMode = testMode;
                foreach (ResponseEntry responseEntry in _configData.ResponseList)
                {
                    responseEntry.Reset();
                }
                _isoTpMode = false;
                _tp20Channels.Clear();
                _dynamicUdsEntries.Clear();
                _codingStampDict.Clear();
                _workerThread = new Thread(ThreadFunc);
                _threadRunning = true;
                _workerThread.Priority = ThreadPriority.Highest;
                _workerThread.Start();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void StopThread()
        {
            if (_workerThread != null)
            {
                _stopThread = true;
                _workerThread.Join();
                _workerThread = null;
            }
        }

        public bool ThreadRunning()
        {
            if (_workerThread == null)
            {
                return false;
            }

            return _threadRunning;
        }

        private void ThreadFunc()
        {
            if (Connect())
            {
                foreach (Stopwatch watch in _timeValveWrite)
                {
                    watch.Stop();
                }
                _outputs = 0x00;
                _outputsActiveTime.Stop();
                _noResponseCount = 0;
                _nr2123SendCount = 0;
                _kwp1281InvRespIndex = 0;
                _kwp1281InvEchoIndex = 0;
                _kwp1281InvBlockEndIndex = 0;
                _ecuErrorResetList.Clear();
                ErrorDefault = false;
                while (!_stopThread)
                {
                    SendIcomMessages();

                    if (ErrorDefault)
                    {
                        ErrorDefault = false;
                        _ecuErrorResetList.Clear();
                    }
                    try
                    {
                        switch (_conceptType)
                        {
                            case ConceptType.Concept1:
                                SerialConcept1Transmission();
                                break;

                            case ConceptType.ConceptKwp1281:
                                SerialKwp1281Transmission();
                                break;

                            case ConceptType.Concept3:
                                SerialConcept3Transmission();
                                break;

                            case ConceptType.ConceptKwp2000:
                            case ConceptType.ConceptTp20:
                                SerialKwp2000Transmission();
                                break;

                            default:
                                SerialTransmission();
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                Disconnect();
            }
            _threadRunning = false;
            _form.RefreshDisplay();
        }

        private bool Connect()
        {
            Disconnect();
            if (_comPort.StartsWith("ENET", StringComparison.OrdinalIgnoreCase))
            {
                int? exceptionPort = null;
                try
                {
                    switch (_conceptType)
                    {
                        case ConceptType.ConceptBwmFast:
                            break;

                        default:
                            return false;
                    }

                    if (File.Exists(ServerCertFile))
                    {
                        try
                        {
                            // RSA (not working with EDIABAS)
                            // generate CA:
                            // openssl genrsa -out rootCA.key 4096
                            // openssl req -x509 -new -nodes -key rootCA.key -sha256 -days 36500 -out rootCA.crt
                            // openssl x509 -inform pem -noout -text -in rootCA.crt
                            // generate hash name:
                            // openssl x509 -hash -noout -in rootCA.crt
                            // cp rootCA.crt <hash>.0
                            // copy files to EDIABAS.ini [SSL] SecurityPath property location.

                            // vehicle key:
                            // openssl req -new -nodes -out vin1.csr -newkey rsa:4096 -keyout vin1.key
                            // openssl x509 -req -in vin1.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out vin1.crt -days 36500 -sha256
                            // openssl x509 -inform pem -noout -text -in vin1.crt
                            // cat vin1.crt rootCA.crt > vin1.pem
                            // optional: openssl pkcs12 -export -out vin1.pfx -inkey vin1.key -in vin1.pem -passout pass:

                            // client key:
                            // openssl req -new -nodes -out client.csr -newkey rsa:4096 -keyout client.key
                            // openssl x509 -req -in client.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out client.crt -days 36500 -sha256
                            // openssl x509 -inform pem -noout -text -in client.crt
                            // cat client.crt rootCA.crt > client.pem
                            // optional: openssl pkcs12 -export -out client.pfx -inkey client.key -in client.pem -passout pass:
                            // copy files to EDIABAS.ini [SSL] S29Path property location.

                            // ECDSA (working with EDIABAS):
                            // generate CA:
                            // openssl ecparam -out rootCA_EC.key -name secp384r1 -genkey
                            // openssl req -x509 -new -nodes -key rootCA_EC.key -sha512 -days 36500 -outform pem -out rootCA_EC.crt -subj '//DUMMY=/CN=Sec4Diag-Root-CA-2025/ST=Production/OU=Sec4Diag-PKI-Root-CA/O=BMW Group/C=DE' -set_serial 1000
                            // openssl x509 -inform pem -noout -text -in rootCA_EC.crt
                            // generate hash name:
                            // openssl x509 -hash -noout -in rootCA_EC.crt
                            // cp rootCA_EC.crt <hash>.0
                            // create PKCS12 file:
                            // openssl pkcs12 -export -out rootCA_EC.pfx -inkey rootCA_EC.key -in rootCA_EC.crt -passout pass:
                            // copy files to EDIABAS.ini [SSL] SecurityPath property location.

                            // vehicle key ECDSA (not working):
                            // openssl ecparam -out vin_EC1.key -name secp384r1 -genkey
                            // openssl req -new -nodes -key vin_EC1.key -outform pem -out vin_EC1.csr -sha512 -subj '//DUMMY=/ST=Production/O=BMW Group/CN=WBAJM71000B055940'
                            // openssl x509 -req -in vin_EC1.csr -CA rootCA_EC.crt -CAkey rootCA_EC.key -CAcreateserial -out vin_EC1.crt -days 36500 -sha256
                            // openssl x509 -inform pem -noout -text -in vin_EC1.crt
                            // cat vin_EC1.crt rootCA_EC.crt > vin_EC1.pem

                            // vehicle key RSA with ECDSA CA (ok):
                            // openssl req -new -nodes -out vin_RSA1.csr -newkey rsa:4096 -keyout vin_RSA1.key -subj '//DUMMY=/ST=Production/O=BMW Group/CN=WBAJM71000B055940'
                            // openssl x509 -req -in vin_RSA1.csr -CA rootCA_EC.crt -CAkey rootCA_EC.key -CAcreateserial -out vin_RSA1.crt -days 36500 -sha256
                            // openssl x509 -inform pem -noout -text -in vin_RSA1.crt
                            // cat vin_RSA1.crt rootCA_EC.crt > vin_RSA1.pem

                            // view PSdZ cacerts:
                            // keytool -list -storepass changeit -keystore "C:\Program Files\BMW\ISPI\TRIC\ISTA\PSdZ\Security"
                            // import into PSdZ cacerts
                            // openssl x509 -outform der -in rootCA_EC.crt -out rootCA_EC.der
                            // keytool -import -alias "EdiabasLib root ca" -storepass changeit -keystore "C:\Program Files\BMW\ISPI\TRIC\ISTA\PSdZ\Security" -file rootCA_EC.der

                            // set EDIABAS.ini [SSL] SSLPORT property to DoIpDiagSslPort value.
                            string publicCert = Path.ChangeExtension(ServerCertFile, ".pem");
                            string privateCert = Path.ChangeExtension(ServerCertFile, ".key");
                            if (File.Exists(publicCert) && File.Exists(privateCert))
                            {
                                byte[] pkcs12Data = EdBcTlsUtilities.CreatePkcs12Data(publicCert, privateCert, ServerCertPwd);
                                if (pkcs12Data != null)
                                {
#if NET9_0_OR_GREATER
                                    _serverCertificate = X509CertificateLoader.LoadCertificate(pkcs12Data);
#else
                                    _serverCertificate = new X509Certificate2(pkcs12Data);
#endif
                                }
                            }

                            string certDir = Path.GetDirectoryName(ServerCertFile);
                            if (certDir != null)
                            {
                                _serverCAs = new List<X509CertificateStructure>();
                                string[] trustedFiles = Directory.GetFiles(certDir, "*.crt", SearchOption.TopDirectoryOnly);
                                foreach (string trustedFile in trustedFiles)
                                {
                                    string certFileName = Path.GetFileName(trustedFile);
                                    if (string.IsNullOrEmpty(certFileName))
                                    {
                                        continue;
                                    }

                                    if (certFileName.StartsWith("rootCA", StringComparison.OrdinalIgnoreCase))
                                    {
                                        X509CertificateStructure trustedCert = EdBcTlsUtilities.LoadBcCertificateResource(trustedFile);
                                        if (trustedCert != null)
                                        {
                                            _serverCAs.Add(trustedCert);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Fail to load Server cert: {0}", e.Message);
                        }
                    }

                    _bmwTcpChannels.Clear();
                    _bmwTcpChannels.Add(new BmwTcpChannel(EnetDiagPort, EnetControlPort, DoIpDiagPort, ServerSslPort));
                    _bmwTcpChannels.Add(new BmwTcpChannel(EnetDiagPrgPort, EnetControlPrgPort));

                    bool isHszfz = ((_enetCommType & EnetCommType.Hsfz) == EnetCommType.Hsfz);
                    bool isDoIp = ((_enetCommType & EnetCommType.DoIp) == EnetCommType.DoIp);
                    foreach (BmwTcpChannel bmwTcpChannel in _bmwTcpChannels)
                    {
                        if (isHszfz)
                        {
                            try
                            {
                                bmwTcpChannel.TcpServerDiag = new TcpListener(IPAddress.Any, bmwTcpChannel.DiagPort);
                                bmwTcpChannel.TcpServerDiag.Start();
                            }
                            catch (Exception)
                            {
                                exceptionPort = bmwTcpChannel.DiagPort;
                                throw;
                            }
                        }

                        try
                        {
                            bmwTcpChannel.TcpServerControl = new TcpListener(IPAddress.Any, bmwTcpChannel.ControlPort);
                            bmwTcpChannel.TcpServerControl.Start();
                        }
                        catch (Exception)
                        {
                            exceptionPort = bmwTcpChannel.ControlPort;
                            throw;
                        }

                        if (bmwTcpChannel.DoIpPort > 0)
                        {
                            if (isDoIp)
                            {
                                try
                                {
                                    bmwTcpChannel.TcpServerDoIp = new TcpListener(IPAddress.Any, bmwTcpChannel.DoIpPort);
                                    bmwTcpChannel.TcpServerDoIp.Start();
                                }
                                catch (Exception)
                                {
                                    exceptionPort = bmwTcpChannel.DoIpPort;
                                    throw;
                                }
                            }
                        }

                        if (bmwTcpChannel.DoIpSslPort > 0 && _serverCertificate != null)
                        {
                            if (isDoIp)
                            {
                                try
                                {
                                    bmwTcpChannel.TcpServerDoIpSsl = new TcpListener(IPAddress.Any, bmwTcpChannel.DoIpSslPort);
                                    bmwTcpChannel.TcpServerDoIpSsl.Start();
                                }
                                catch (Exception)
                                {
                                    exceptionPort = bmwTcpChannel.DoIpSslPort;
                                    throw;
                                }
                            }
                        }
                    }

                    UpdateIcomStatus(true);
                    UdpConnect();
                    UdpDoIpConnect();
                    SrvLocConnect();
                }
                catch (Exception ex)
                {
                    Disconnect();

                    Debug.WriteLine("Connect Exception: {0}", (object)ex.Message);
                    if (exceptionPort != null)
                    {
                        Debug.WriteLine("Exception port: {0}", (object)exceptionPort.Value);
                        Debug.WriteLine("Print excluded system ports:");
                        Debug.WriteLine("netsh interface ipv4 show excludedportrange protocol=tcp");
                        Debug.WriteLine($"If excluded, reboot in safe mode (with shift pressed) and exclude the port {exceptionPort.Value}:");
                        Debug.WriteLine($"netsh int ipv4 add excludedportrange protocol=tcp startport={exceptionPort.Value} numberofports=1");
                    }
                    return false;
                }

                Connected = true;
                return true;
            }

            if (_comPort.StartsWith("CAN", StringComparison.OrdinalIgnoreCase))
            {
                TPCANBaudrate baudRate;
                switch (_conceptType)
                {
                    case ConceptType.ConceptBwmFast:
                        baudRate = TPCANBaudrate.PCAN_BAUD_500K;
                        //baudRate = TPCANBaudrate.PCAN_BAUD_100K;
                        break;

                    case ConceptType.ConceptTp20:
                        baudRate = TPCANBaudrate.PCAN_BAUD_500K;
                        break;

                    default:
                        return false;
                }
                TPCANStatus stsResult = PCANBasic.Initialize(PCANBasic.PCAN_USBBUS1, baudRate, TPCANType.PCAN_TYPE_DNG, 0, 0);
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    return false;
                }
                _pcanHandle = PCANBasic.PCAN_USBBUS1;
                UInt32 iEventBuffer = Convert.ToUInt32(_pcanReceiveEvent.SafeWaitHandle.DangerousGetHandle().ToInt32());
                stsResult = PCANBasic.SetValue(_pcanHandle, TPCANParameter.PCAN_RECEIVE_EVENT, ref iEventBuffer, sizeof(UInt32));
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Disconnect();
                    return false;
                }

                UInt32 autoReset = 1;
                stsResult = PCANBasic.SetValue(_pcanHandle, TPCANParameter.PCAN_BUSOFF_AUTORESET, ref autoReset, sizeof(UInt32));
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Disconnect();
                    return false;
                }

                Connected = true;
                return true;
            }
            try
            {
                int baudRate = 9600;
                Parity parity = Parity.Even;
                switch (_conceptType)
                {
                    case ConceptType.ConceptBwmFast:
                        baudRate = 115200;
                        parity = Parity.None;
                        break;

                    case ConceptType.ConceptKwp2000Bmw:
                        baudRate = 10400;
                        parity = Parity.None;
                        break;

                    case ConceptType.ConceptKwp2000S:
                    case ConceptType.ConceptDs2:
                    case ConceptType.Concept1:
                        baudRate = 9600;
                        parity = Parity.Even;
                        break;

                    case ConceptType.ConceptKwp1281:
                        baudRate = 10400;
                        //baudRate = 9600;
                        //baudRate = 1200;
                        //baudRate = 1000;
                        parity = Parity.None;
                        break;

                    case ConceptType.Concept3:
                        baudRate = 9600;
                        parity = Parity.Even;
                        break;

                    case ConceptType.ConceptKwp2000:
                        baudRate = 10400;
                        //baudRate = 9600;
                        //baudRate = 4800;
                        //baudRate = 4000;
                        //baudRate = 2000;
                        //baudRate = 1200;
                        //baudRate = 1000;
                        parity = Parity.None;
                        break;
                }

                _serialPort.PortName = _comPort;
                _serialPort.BaudRate = baudRate;
                _serialPort.DataBits = 8;
                _serialPort.Parity = parity;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReadTimeout = 0;
                _serialPort.DtrEnable = false;
                _serialPort.RtsEnable = false;
                _serialPort.Open();
                if (_serialPort.IsOpen)
                {
                    _serialPort.DataReceived += SerialDataReceived;
                }
            }
            catch (Exception)
            {
                return false;
            }

            Connected = true;
            return true;
        }

        private void Disconnect()
        {
            Connected = false;

            UdpDisconnect();
            UdpDoIpDisconnect();
            SrvLocDisconnect();

            foreach (BmwTcpChannel bmwTcpChannel in _bmwTcpChannels)
            {
                // diag port
                foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDiagList)
                {
                    EnetDiagClose(bmwTcpClientData);
                }

                try
                {
                    if (bmwTcpChannel.TcpServerDiag != null)
                    {
                        bmwTcpChannel.TcpServerDiag.Stop();
                        bmwTcpChannel.TcpServerDiag = null;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                // control port
                foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientControlList)
                {
                    EnetControlClose(bmwTcpClientData);
                }

                try
                {
                    if (bmwTcpChannel.TcpServerControl != null)
                    {
                        bmwTcpChannel.TcpServerControl.Stop();
                        bmwTcpChannel.TcpServerControl = null;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                // DoIp
                foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDoIpList)
                {
                    DoIpClose(bmwTcpClientData);
                }

                try
                {
                    if (bmwTcpChannel.TcpServerDoIp != null)
                    {
                        bmwTcpChannel.TcpServerDoIp.Stop();
                        bmwTcpChannel.TcpServerDoIp = null;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                // DoIpSsl
                foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDoIpSslList)
                {
                    DoIpClose(bmwTcpClientData);
                }

                try
                {
                    if (bmwTcpChannel.TcpServerDoIpSsl != null)
                    {
                        bmwTcpChannel.TcpServerDoIpSsl.Stop();
                        bmwTcpChannel.TcpServerDoIpSsl = null;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _bmwTcpChannels.Clear();

            if (_serverCertificate != null)
            {
                _serverCertificate.Dispose();
                _serverCertificate = null;
            }

            if (_serverCAs != null)
            {
                _serverCAs.Clear();
                _serverCAs = null;
            }

            if (_pcanHandle != PCANBasic.PCAN_NONEBUS)
            {
                UInt32 iEventBuffer = (UInt32)new IntPtr(-1);   // invalid handle
                TPCANStatus stsResult = PCANBasic.SetValue(_pcanHandle, TPCANParameter.PCAN_RECEIVE_EVENT, ref iEventBuffer, sizeof(UInt32));
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    Debug.WriteLine("Removing CAN handle failed");
                }
                PCANBasic.Uninitialize(_pcanHandle);
                _pcanHandle = PCANBasic.PCAN_NONEBUS;
            }
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DataReceived -= SerialDataReceived;
                    _serialPort.Close();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UdpConnect()
        {
            if ((_enetCommType & EnetCommType.Hsfz) != EnetCommType.Hsfz)
            {
                return;
            }
            // a virtual network adapter with an auto ip address
            // is required tp receive the UPD broadcasts
            _udpError = false;
            _udpClient = new UdpClient(EnetControlPort);
            StartUdpListen();
        }

        private void UdpDisconnect()
        {
            try
            {
                if (_udpClient != null)
                {
                    _udpClient.Close();
                    _udpClient.Dispose();
                    _udpClient = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UdpRecover()
        {
            try
            {
                if (_udpError)
                {
                    UdpDisconnect();
                    UdpConnect();
                }
            }
            catch (Exception)
            {
                _udpError = true;
            }
        }

        private void UdpDoIpConnect()
        {
            if ((_enetCommType & EnetCommType.DoIp) != EnetCommType.DoIp)
            {
                return;
            }

            _udpDoIpError = false;
            _lastDoIpIdentAddr = null;
            _udpDoIpClient = new UdpClient(DoIpDiagPort);
            StartUdpDoIpListen();
        }

        private void UdpDoIpDisconnect()
        {
            try
            {
                if (_udpDoIpClient != null)
                {
                    _udpDoIpClient.Close();
                    _udpDoIpClient.Dispose();
                    _udpDoIpClient = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UdpDoIpRecover()
        {
            try
            {
                if (_udpDoIpError)
                {
                    UdpDoIpDisconnect();
                    UdpDoIpConnect();
                }
            }
            catch (Exception)
            {
                _udpDoIpError = true;
            }
        }

        private void SrvLocConnect()
        {
#if false
            // a virtual network adapter with an auto ip address
            // is required tp receive the UPD broadcasts
            _srvLocError = false;
            _srvLocClient = new UdpClient(SrvLocPort);
            StartSrvLocListen();
#endif
        }

        private void SrvLocDisconnect()
        {
            try
            {
                if (_srvLocClient != null)
                {
                    _srvLocClient.Close();
                    _srvLocClient.Dispose();
                    _srvLocClient = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void SrvLocRecover()
        {
            try
            {
                if (_srvLocError)
                {
                    SrvLocDisconnect();
                    SrvLocConnect();
                }
            }
            catch (Exception)
            {
                _srvLocError = true;
            }
        }

        private void UpdateOutState()
        {
            try
            {
                if (_serialPort.DtrEnable != IgnitionOk)
                {
                    _serialPort.DtrEnable = IgnitionOk;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool ReceiveWakeUp(out byte address)
        {
            address = 0;
            try
            {
                while (!_stopThread)
                {
                    UpdateOutState();
                    if (_klineResponder)
                    {
                        if (_serialPort.CtsHolding)
                        {   // start bit
                            break;
                        }
                    }
                    else
                    {
                        if (_serialPort.DsrHolding)
                        {   // start bit
                            break;
                        }
                    }
                    Thread.Sleep(10);
                }
                if (_stopThread) return false;
                Thread.Sleep(100);
                int recValue = 0x00;
                for (int i = 0; i < 9; i++)
                {
                    Thread.Sleep(200);
                    if (_klineResponder)
                    {
                        if (!_serialPort.CtsHolding)
                        {
                            recValue |= (1 << i);
                        }
                    }
                    else
                    {
                        if (!_serialPort.DsrHolding)
                        {
                            recValue |= (1 << i);
                        }
                    }
                    if (_stopThread) return false;
                }
                if ((recValue & 0x100) == 0)
                {   // invalid stop bit
                    return false;
                }
                Thread.Sleep(100);
                address = (byte)recValue;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _serialReceiveEvent.Set();
        }

        private bool SendData(byte[] sendData, int length)
        {   // no try catch to allow loop exit
            //_serialPort.DiscardInBuffer();
            _serialPort.Write(sendData, 0, length);
            if (!RemoveEcho(sendData, 0, length))
            {
                return false;
            }
            return true;
        }

        private bool SendData(byte[] sendData, int offset, int length)
        {   // no try catch to allow loop exit
            _serialPort.DiscardInBuffer();
            _serialPort.Write(sendData, offset, length);
            if (!RemoveEcho(sendData, offset, length))
            {
                return false;
            }
            return true;
        }

        private bool RemoveEcho(byte[] sendData, int offset, int length)
        {
            if (_klineResponder)
            {   // remove echo
                byte[] buffer = new byte[length];
                if (!ReceiveData(buffer, 0, length))
                {
                    return false;
                }
                for (int i = 0; i < length; i++)
                {
                    if (buffer[i] != sendData[i + offset])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, bool discard = false)
        {
            if (length < 0)
            {
                return false;
            }
            try
            {
                UpdateOutState();
                // wait for first byte
                // for stable switching we always need 10ms, but then are problems with win CE client
                int interByteTimeout = 10;
                switch (_conceptType)
                {
                    case ConceptType.ConceptBwmFast:
                        interByteTimeout = 100;     // this is more reliable
                        break;

                    case ConceptType.ConceptKwp2000:
                        interByteTimeout = 30;
                        break;
                }

                if (_serialPort.BaudRate < 4000 && interByteTimeout < 30)
                {
                    interByteTimeout = 30;
                }

                int lastBytesToRead = 0;
                int recLen = 0;
                _receiveStopWatch.Reset();
                _receiveStopWatch.Start();
                for (; ; )
                {
                    int bytesToRead = _serialPort.BytesToRead;
                    if (bytesToRead >= length)
                    {
                        recLen = _serialPort.Read(receiveData, offset + recLen, length - recLen);
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if (lastBytesToRead != bytesToRead)
                    {   // bytes received
                        _receiveStopWatch.Reset();
                        _receiveStopWatch.Start();
                        lastBytesToRead = bytesToRead;
                    }
                    else
                    {
                        if (_receiveStopWatch.ElapsedMilliseconds > interByteTimeout)
                        {
                            if (offset > 0)
                            {
                                //Debug.WriteLine("Interbyte timeout: O={0}, L={1}, R={2}", offset, length, recLen);
                            }
                            break;
                        }
                    }
                    // no _serialReceiveEvent.WaitOne(1, false); allowed here!
                }
                _receiveStopWatch.Stop();
                if (recLen < length)
                {
                    if (discard && lastBytesToRead > 0)
                    {
                        recLen = _serialPort.Read(receiveData, offset + recLen, lastBytesToRead);
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ReceiveData(byte[] receiveData, int offset, int length, int timeout, int timeoutTelEnd)
        {
            try
            {
                // wait for first byte
                int lastBytesToRead;
                _receiveStopWatch.Reset();
                _receiveStopWatch.Start();
                for (; ; )
                {
                    lastBytesToRead = _serialPort.BytesToRead;
                    if (lastBytesToRead > 0)
                    {
                        break;
                    }
                    if (_receiveStopWatch.ElapsedMilliseconds > timeout)
                    {
                        _receiveStopWatch.Stop();
                        return false;
                    }
                    _serialReceiveEvent.WaitOne(1, false);
                }

                int recLen = 0;
                _receiveStopWatch.Reset();
                _receiveStopWatch.Start();
                for (; ; )
                {
                    int bytesToRead = _serialPort.BytesToRead;
                    if (bytesToRead >= length)
                    {
                        recLen += _serialPort.Read(receiveData, offset + recLen, length - recLen);
                    }
                    if (recLen >= length)
                    {
                        break;
                    }
                    if (lastBytesToRead != bytesToRead)
                    {   // bytes received
                        _receiveStopWatch.Reset();
                        _receiveStopWatch.Start();
                        lastBytesToRead = bytesToRead;
                    }
                    else
                    {
                        if (_receiveStopWatch.ElapsedMilliseconds > timeoutTelEnd)
                        {
                            break;
                        }
                    }
                    _serialReceiveEvent.WaitOne(1, false);
                }
                _receiveStopWatch.Stop();
                if (recLen < length)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ObdSend(byte[] sendData, BmwTcpClientData bmwTcpClientData = null)
        {
#if false
            DebugLogData("Response: ", sendData, sendData.Length);
#endif
            switch (_responseConcept)
            {
                case ConceptType.ConceptBwmFast:
                case ConceptType.ConceptKwp2000Bmw:
#if false
                    if (bmwTcpClientData != null)
                    {
                        Debug.WriteLine("Time[{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.BmpBmwTcpChannel.DiagPort, DateTime.Now.ToString("hh:mm:ss.fff"));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("Time: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
                    }
#endif
                    DebugLogData("Response: ", sendData, TelLengthBmwFast(sendData));

                    if (bmwTcpClientData != null)
                    {
                        if (bmwTcpClientData.IsDoIp)
                        {
                            return SendDoIp(sendData, bmwTcpClientData);
                        }
                        return SendEnet(sendData, bmwTcpClientData);
                    }
                    if (_pcanHandle != PCANBasic.PCAN_NONEBUS)
                    {
                        return SendCan(sendData);
                    }
                    return SendBmwfast(sendData);

                case ConceptType.ConceptKwp2000S:
                    {
                        DebugLogData("Response KWP2000*: ", sendData, TelLengthBmwFast(sendData));

                        byte[] tempArray = new byte[260];
                        // convert to KWP2000*
                        int dataLength = sendData[0] & 0x3F;
                        if (dataLength == 0)
                        {   // with length byte
                            dataLength = sendData[3];
                            Array.Copy(sendData, 0, tempArray, 0, dataLength + 4);
                            tempArray[0] = 0xB8;
                        }
                        else
                        {   // without length byte
                            Array.Copy(sendData, 0, tempArray, 0, 3);
                            Array.Copy(sendData, 3, tempArray, 4, dataLength);
                            tempArray[0] = 0xB8;
                            tempArray[3] = (byte)dataLength;
                        }
                        return SendKwp2000S(tempArray);
                    }

                case ConceptType.ConceptDs2:
                    {
                        DebugLogData("Response DS2: ", sendData, TelLengthBmwFast(sendData));

                        byte[] tempArray = new byte[260];
                        // convert to DS2
                        int dataLength = sendData[0] & 0x3F;
                        byte ecuAddr = sendData[2];
                        if (ecuAddr == 0xF1)
                        {   // for echo response
                            ecuAddr = sendData[1];
                        }
                        if (dataLength == 0)
                        {   // with length byte
                            dataLength = sendData[3];
                            Array.Copy(sendData, 4, tempArray, 2, dataLength);
                            tempArray[0] = ecuAddr;
                            tempArray[1] = (byte)(dataLength + 3);
                        }
                        else
                        {   // without length byte
                            Array.Copy(sendData, 3, tempArray, 2, dataLength);
                            tempArray[0] = ecuAddr;
                            tempArray[1] = (byte)(dataLength + 3);
                        }
                        return SendDs2(tempArray);
                    }

                case ConceptType.ConceptKwp2000:
                    return SendBmwfast(sendData);

                case ConceptType.ConceptTp20:
                    if (_pcanHandle != PCANBasic.PCAN_NONEBUS)
                    {
                        if (_isoTpMode)
                        {
                            return SendCanVw(sendData);
                        }
                        return SendCanTp20(sendData);
                    }
                    return false;
            }
            return false;
        }

        private bool ObdReceive(byte[] receiveData, BmwTcpClientData bmwTcpClientData = null)
        {
            _responseConcept = _conceptType;
            _isoTpMode = false;
            switch (_conceptType)
            {
                case ConceptType.ConceptBwmFast:
                case ConceptType.ConceptKwp2000Bmw:
                    if (bmwTcpClientData != null)
                    {
                        UdpRecover();
                        SrvLocRecover();

                        foreach (BmwTcpClientData bmwTcpClientDataControl in bmwTcpClientData.BmwTcpChannel.TcpClientControlList)
                        {
                            if (bmwTcpClientDataControl.BmwTcpChannel != null ||
                                bmwTcpClientData.BmwTcpChannel.TcpServerControl.Pending())
                            {
                                ReceiveEnetControl(bmwTcpClientDataControl);
                            }
                        }

                        if (bmwTcpClientData.IsDoIp)
                        {
                            return ReceiveDoIp(receiveData, bmwTcpClientData);
                        }

                        return ReceiveEnet(receiveData, bmwTcpClientData);
                    }

                    if (_pcanHandle != PCANBasic.PCAN_NONEBUS)
                    {
                        return ReceiveCan(receiveData);
                    }
                    return ReceiveBmwFast(receiveData);

                case ConceptType.ConceptKwp2000S:
                    {
                        if (!ReceiveKwp2000S(receiveData))
                        {
                            return false;
                        }
                        // convert to BMW-FAST
                        int dataLength = receiveData[3];
                        if (dataLength > 0x3F)
                        {   // with length byte
                            receiveData[0] = 0x80;
                            receiveData[dataLength + 4] = CalcChecksumBmwFast(receiveData, dataLength + 4);
                        }
                        else
                        {   // without length byte
                            byte[] tempArray = new byte[260];
                            Array.Copy(receiveData, 4, tempArray, 0, dataLength);
                            Array.Copy(tempArray, 0, receiveData, 3, dataLength);
                            receiveData[0] = (byte)(0x80 | dataLength);
                            receiveData[dataLength + 3] = CalcChecksumBmwFast(receiveData, dataLength + 3);
                        }
                        return true;
                    }

                case ConceptType.ConceptDs2:
                    {
                        if (!ReceiveDs2(receiveData, out bool kwp2000S))
                        {
                            return false;
                        }
                        // convert to BMW-FAST
                        int dataLength;
                        if (kwp2000S)
                        {   // KWP2000*
                            Debug.WriteLine("Detected KWP2000* request");
                            _responseConcept = ConceptType.ConceptKwp2000S;
                            dataLength = receiveData[3];
                            if (dataLength > 0x3F)
                            {   // with length byte
                                receiveData[0] = 0x80;
                                receiveData[dataLength + 4] = CalcChecksumBmwFast(receiveData, dataLength + 4);
                            }
                            else
                            {   // without length byte
                                byte[] tempArray = new byte[260];
                                Array.Copy(receiveData, 4, tempArray, 0, dataLength);
                                Array.Copy(tempArray, 0, receiveData, 3, dataLength);
                                receiveData[0] = (byte)(0x80 | dataLength);
                                receiveData[dataLength + 3] = CalcChecksumBmwFast(receiveData, dataLength + 3);
                            }
                            return true;
                        }
                        // DS2
                        dataLength = receiveData[1] - 3;
                        if (dataLength > 0x3F)
                        {   // with length byte
                            byte[] tempArray = new byte[260];
                            Array.Copy(receiveData, 2, tempArray, 4, dataLength);
                            tempArray[0] = 0x80;
                            tempArray[1] = receiveData[0];
                            tempArray[2] = 0xF1;
                            tempArray[3] = (byte)dataLength;
                            Array.Copy(tempArray, 0, receiveData, 0, dataLength + 4);
                            receiveData[dataLength + 4] = CalcChecksumBmwFast(receiveData, dataLength + 4);
                        }
                        else
                        {   // without length byte
                            byte[] tempArray = new byte[260];
                            Array.Copy(receiveData, 2, tempArray, 3, dataLength);
                            tempArray[0] = (byte)(dataLength | 0x80);
                            tempArray[1] = receiveData[0];
                            tempArray[2] = 0xF1;
                            Array.Copy(tempArray, 0, receiveData, 0, dataLength + 3);
                            receiveData[dataLength + 3] = CalcChecksumBmwFast(receiveData, dataLength + 3);
                        }
                        return true;
                    }

                case ConceptType.ConceptKwp2000:
                    return ReceiveBmwFast(receiveData);

                case ConceptType.ConceptTp20:
                    if (_pcanHandle != PCANBasic.PCAN_NONEBUS)
                    {
                        if (_tp20Channels.Count > 0)
                        {
                            return ReceiveCanTp20(receiveData);
                        }
                        TPCANStatus stsResult = PCANBasic.Read(_pcanHandle, out TPCANMsg canMsg, out TPCANTimestamp _);
                        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                        {
                            return false;
                        }
                        if (canMsg.MSGTYPE != TPCANMessageType.PCAN_MESSAGE_STANDARD)
                        {
                            return false;
                        }

                        if (GetIsoTpConfigData(canMsg.ID) != null)
                        {
                            _isoTpMode = true;
                            return ReceiveCanVw(receiveData, canMsg);
                        }
                        return ReceiveCanTp20(receiveData, canMsg);
                    }
                    return false;
            }
            return false;
        }

        private int ReadNetworkStream(Stream tcpClientStream, byte[] buffer, int offset, int count, int timeout = 2000)
        {
            if (tcpClientStream == null || count <= 0 || buffer == null || offset < 0 || offset + count > buffer.Length)
            {
                return 0;
            }

            int recLen = 0;
            long startTick = Stopwatch.GetTimestamp();
            for (; ; )
            {
                int readBytes = tcpClientStream.Read(buffer, offset + recLen, count);
                if (readBytes > 0)
                {
                    recLen += readBytes;
                    if (recLen >= count)
                    {
                        break;
                    }

                    startTick = Stopwatch.GetTimestamp();
                }

                if ((Stopwatch.GetTimestamp() - startTick) > timeout * TickResolMs)
                {
                    break;
                }

                Thread.Sleep(10);
            }

            return recLen;
        }

        private void WriteNetworkStream(BmwTcpClientData bmwTcpClientData, byte[] buffer, int offset, int size)
        {
            if (buffer == null || size <= 0 || offset < 0 || offset + size > buffer.Length)
            {
                return;
            }

            if (bmwTcpClientData?.TcpClientConnection == null)
            {
                return;
            }

            if (bmwTcpClientData.TcpClientStream == null)
            {
                return;
            }
#if false
            while ((Stopwatch.GetTimestamp() - bmwTcpClientData.LastTcpSendTick) < 10 * TickResolMs)
            {
                Thread.Sleep(10);
            }
#endif
            int packetSize = bmwTcpClientData.TcpClientConnection.SendBufferSize;
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
                    bmwTcpClientData.TcpClientStream.Write(buffer, offset + pos, length);
                    pos += length;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("WriteNetworkStream exception: {0}", (object)ex.Message);
                    throw;
                }
            }

            bmwTcpClientData.LastTcpSendTick = Stopwatch.GetTimestamp();
        }

        void ClearNetworkStream(Stream tcpClientStream)
        {
            if (tcpClientStream == null)
            {
                return;
            }

            string streamName = tcpClientStream.GetType().Name;
            NetworkStream networkStream = tcpClientStream as NetworkStream;
            SslStream sslStream = tcpClientStream as SslStream;
            Stream tlsStream = streamName == EdBcTlsUtilities.TlsStreamName ? tcpClientStream : null;

            if (networkStream != null)
            {
                while (networkStream.DataAvailable)
                {
                    tcpClientStream.ReadByte();
                }
                return;
            }

            if (sslStream != null)
            {
                sslStream.ReadTimeout = 1;
                while (sslStream.ReadByte() >= 0)
                {
                }
                sslStream.ReadTimeout = SslAuthTimeout;
                return;
            }

            if (tlsStream != null)
            {
                while (tlsStream.ReadByte() >= 0)
                {
                }
            }
        }

        private void StartUdpListen()
        {
            _udpClient?.BeginReceive(UdpReceiver, new Object());
        }

        private void UdpReceiver(IAsyncResult ar)
        {
            try
            {
                UdpClient udpClientLocal = _udpClient;
                if (udpClientLocal == null)
                {
                    return;
                }
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udpClientLocal.EndReceive(ar, ref ip);
#if false
                if (bytes != null)
                {
                    DebugLogData("Udp: ", bytes, bytes.Length);
                }
#endif
                if (bytes != null && bytes.Length == 6 && bytes[5] == 0x11)
                {
                    Debug.WriteLine("Ident request from: {0}:{1}", ip.Address, ip.Port);
                    SendIdentMessage(ip, EnetControlPort);
                }
                StartUdpListen();
            }
            catch (Exception)
            {
                _udpError = true;
            }
        }

        private bool SendIdentMessage(IPEndPoint ipEnd, int localPort)
        {
            Debug.WriteLine("Sending ident message to: {0}:{1}", ipEnd.Address, ipEnd.Port);

            byte[] identMessage = new byte[6 + 50];
            int idx = 0;
            identMessage[idx++] = 0x00;
            identMessage[idx++] = 0x00;
            identMessage[idx++] = 0x00;
            identMessage[idx++] = (byte)(identMessage.Length - 6);
            identMessage[idx++] = 0x00;
            identMessage[idx++] = 0x11; // ENET
            // TESTER ID
            identMessage[idx++] = (byte)'D';
            identMessage[idx++] = (byte)'I';
            identMessage[idx++] = (byte)'A';
            identMessage[idx++] = (byte)'G';
            identMessage[idx++] = (byte)'A';
            identMessage[idx++] = (byte)'D';
            identMessage[idx++] = (byte)'R';
            identMessage[idx++] = (byte)'1';
            identMessage[idx++] = (byte)'0';
            // MAC
            identMessage[idx++] = (byte)'B';
            identMessage[idx++] = (byte)'M';
            identMessage[idx++] = (byte)'W';
            identMessage[idx++] = (byte)'M';
            identMessage[idx++] = (byte)'A';
            identMessage[idx++] = (byte)'C';

            byte[] macBytes = Encoding.ASCII.GetBytes(TestMac);
            int macLen = macBytes.Length;
            Array.Copy(macBytes, 0, identMessage, idx, macLen);
            idx += macLen;
            // VIN
            identMessage[idx++] = (byte)'B';
            identMessage[idx++] = (byte)'M';
            identMessage[idx++] = (byte)'W';
            identMessage[idx++] = (byte)'V';
            identMessage[idx++] = (byte)'I';
            identMessage[idx++] = (byte)'N';

            byte[] vinBytes = Encoding.ASCII.GetBytes(TestVin);
            int vinLen = vinBytes.Length;
            Array.Copy(vinBytes, 0, identMessage, idx, vinLen);
            idx += vinLen;

            return SendUdpPacketTo(identMessage, ipEnd, localPort);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendUdpPacketTo(byte[] data, EndPoint endPoint, int port)
        {
            bool result = false;
            if (endPoint is IPEndPoint ipEnd)
            {
                IPAddress localIp = GetLocalIpAddress(ipEnd.Address, false, out _, out _);
                if (localIp != null)
                {
                    Debug.WriteLine("Sending to: {0} with: {1}", ipEnd.Address, localIp);
                    try
                    {
                        using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                        {
                            IPEndPoint ipUdp = new IPEndPoint(localIp, port);
                            sock.Bind(ipUdp);
                            sock.SendTo(data, endPoint);
                            result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Send exception: {0}", (object)ex.Message);
                    }
                }
            }

            return result;
        }

        private void StartUdpDoIpListen()
        {
            _udpDoIpClient?.BeginReceive(UdpDoIpReceiver, new Object());
        }

        private void UdpDoIpReceiver(IAsyncResult ar)
        {
            try
            {
                UdpClient udpClientLocal = _udpDoIpClient;
                if (udpClientLocal == null)
                {
                    return;
                }
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udpClientLocal.EndReceive(ar, ref ip);
#if true
                if (bytes != null)
                {
                    DebugLogData("DoIp Udp: ", bytes, bytes.Length);
                }
#endif
                if (bytes != null && bytes.Length >= 8)
                {
                    byte protoVersion = bytes[0];
                    byte protoVersionInv = bytes[1];
                    bool valid = true;
                    Debug.WriteLine("DoIp Udp request from: {0}:{1}", ip.Address, ip.Port);

                    if (protoVersion != (byte)~protoVersionInv)
                    {
                        Debug.WriteLine("Invalid version: {0}:{1}", protoVersion, protoVersionInv);
                        valid = false;
                    }

                    if (valid)
                    {
                        SendDoIpUdpMessage(bytes, ip, DoIpDiagPort);
                    }
                }

                StartUdpDoIpListen();
            }
            catch (Exception)
            {
                _udpDoIpError = true;
            }
        }

        private bool SendDoIpUdpMessage(byte[] request, IPEndPoint ipEnd, int localPort)
        {
            uint payloadType = (((uint)request[2] << 8) | request[3]);
            uint payloadLength = (((uint)request[4] << 24) | ((uint)request[5] << 16) | ((uint)request[6] << 8) | request[7]);
            uint resPayloadType = 0x0000;
            List<byte> resData = new List<byte>();
            switch (payloadType)
            {
                case 0x0001: // ident request
                {
                    // ident response
                    resPayloadType = 0x0004;

                    byte[] vinBytes = Encoding.ASCII.GetBytes(TestVin);
                    resData.AddRange(vinBytes);
                    // log address
                    resData.Add((byte)(_doIpGwAddr >> 8));
                    resData.Add((byte)(_doIpGwAddr & 0xFF));
                    // MAC
                    byte[] macBytes = new byte[6];
                    resData.AddRange(macBytes);
                    // GID
                    byte[] gidBytes = new byte[6];
                    resData.AddRange(gidBytes);
                    // further action required
                    resData.Add(0x00);
                    // VIN sync status
                    resData.Add(0x00);
                    break;
                }
            }

            if (resData.Count == 0)
            {
                return false;
            }

            List<byte> responseList = new List<byte>();
            uint resPayloadLength = (uint)resData.Count;
            responseList.Add(DoIpProtoVer);
            responseList.Add(~DoIpProtoVer & 0xFF);
            responseList.Add((byte)(resPayloadType >> 8));
            responseList.Add((byte)resPayloadType);
            responseList.Add((byte)(resPayloadLength >> 24));
            responseList.Add((byte)(resPayloadLength >> 16));
            responseList.Add((byte)(resPayloadLength >> 8));
            responseList.Add((byte)resPayloadLength);
            responseList.AddRange(resData);

            byte[] resBytes = responseList.ToArray();
            DebugLogData("DoIp Res: ", resBytes, resBytes.Length);

            bool sendPacket = true;
            string ipEndString = ipEnd.Address.ToString();
            if (!string.IsNullOrEmpty(_lastDoIpIdentAddr))
            {
                if (string.Compare(ipEndString, _lastDoIpIdentAddr, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    sendPacket = false;
                }
            }

            if (!sendPacket)
            {
                Debug.WriteLine("DoIp Udp: Not sending to: {0}", (object)ipEndString);
                return true;
            }

            _lastDoIpIdentAddr = ipEndString;
            return SendUdpPacketTo(resBytes, ipEnd, localPort);
        }

        private IPAddress GetLocalIpAddress(IPAddress remoteIp, bool broadcast,
            out System.Net.NetworkInformation.NetworkInterface networkAdapter, out byte[] networkMask)
        {
            networkAdapter = null;
            networkMask = null;
            if (remoteIp == null)
            {
                return null;
            }

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
                                byte[] ipBytesRemote = remoteIp.GetAddressBytes();
                                byte[] ipBytesLocal = ipAddressInfo.Address.GetAddressBytes();
                                byte[] maskBytes = ipAddressInfo.IPv4Mask.GetAddressBytes();

                                for (int i = 0; i < ipBytesRemote.Length; i++)
                                {
                                    ipBytesRemote[i] &= maskBytes[i];
                                }

                                for (int i = 0; i < ipBytesLocal.Length; i++)
                                {
                                    ipBytesLocal[i] &= maskBytes[i];
                                }

                                IPAddress ipRemoteMask = new IPAddress(ipBytesRemote);
                                IPAddress ipLocalMask = new IPAddress(ipBytesLocal);
                                if (ipRemoteMask.Equals(ipLocalMask))
                                {
                                    networkAdapter = adapter;
                                    networkMask = maskBytes;

                                    if (broadcast)
                                    {
                                        byte[] ipBytes = ipAddressInfo.Address.GetAddressBytes();
                                        for (int i = 0; i < ipBytes.Length; i++)
                                        {
                                            ipBytes[i] |= (byte)(~maskBytes[i]);
                                        }

                                        IPAddress broadcastAddress = new IPAddress(ipBytes);
                                        return broadcastAddress;
                                    }

                                    return ipAddressInfo.Address;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void SendIcomMessages()
        {
            lock (_networkChangeLock)
            {
                if (_icomIdentBroadcastCount > 0)
                {
                    if (_timeIcomIdentBroadcast.ElapsedMilliseconds > 500)
                    {
                        IPAddress ipIcomBroadcast = GetLocalIpAddress(IPAddress.Parse(IcomVehicleAddress), true, out _, out _);
                        if (ipIcomBroadcast == null ||
                            !SendIdentMessage(new IPEndPoint(ipIcomBroadcast, 7811), EnetControlPort))
                        {
                            _icomIdentBroadcastCount = 0;
                        }
                        else
                        {
                            _timeIcomIdentBroadcast.Reset();
                            _timeIcomIdentBroadcast.Start();
                            _icomIdentBroadcastCount--;
                        }
                    }
                }
            }
        }

        private void UpdateIcomStatus(bool init = false)
        {
            lock(_networkChangeLock)
            {
                if (_bmwTcpChannels.Count == 0)
                {
                    return;
                }

                try
                {
                    IPAddress ipIcomDhcp = IPAddress.Parse(IcomDhcpAddress);
                    IPAddress ipIcomDhcpLocal = GetLocalIpAddress(ipIcomDhcp, false, out _, out byte[] networkMaskIcomDhcp);
                    bool isDhcpUp = ipIcomDhcpLocal != null && networkMaskIcomDhcp != null;
                    if (isDhcpUp)
                    {
                        IPAddress ipNetMaskDhcp = new IPAddress(networkMaskIcomDhcp);
                        Debug.WriteLine("ICOM DHCP server is up");
                        Debug.WriteLine("ICOM DHCP server IP:{0} / {1}", ipIcomDhcp.ToString(), ipNetMaskDhcp.ToString());

                        if (_icomDhcpServer == null)
                        {
                            _icomDhcpServer = new IcomDhcpServer(ipIcomDhcpLocal, ipNetMaskDhcp, () =>
                            {
                                Debug.WriteLine("Disconnected, ICOM DHCP server stopped");
                            });
                        }
                        if (!_icomDhcpServer.IsRunning)
                        {
                            _icomDhcpServer.Start();
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ICOM DHCP server is down, adapter must be configured with IP: {0} / 24", (object)ipIcomDhcp.ToString());
                        Debug.WriteLine("Allow private and public firewall access for this app.");
                        if (_icomDhcpServer != null && _icomDhcpServer.IsRunning)
                        {
                            _icomDhcpServer.Stop();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DHCP Server exception: {0}", (object)ex.Message);
                }

                IPAddress ipIcom = IPAddress.Parse(IcomVehicleAddress);
                IPAddress ipIcomLocal = GetLocalIpAddress(ipIcom, false, out _, out _);
                IPAddress ipIcomBroadcast = GetLocalIpAddress(ipIcom, true, out _, out _);
                bool isUp = false;
                if (ipIcomLocal != null && ipIcomBroadcast != null)
                {
                    isUp = true;
                    Debug.WriteLine("ICOM is up at: {0}, broadcast={1}", ipIcomLocal, ipIcomBroadcast);
                }
                else
                {
                    Debug.WriteLine("ICOM is down");
                }

                if (!init)
                {
                    if (!_icomUp && isUp)
                    {
                        Debug.WriteLine("ICOM changed to up");
                        _timeIcomIdentBroadcast.Reset();
                        _timeIcomIdentBroadcast.Start();
                        _icomIdentBroadcastCount = 3;
                    }
                }

                _icomUp = isUp;
            }
        }

        private void SendIcomDhcpRequest(IPAddress ipIcomLocal, IPAddress ipIcomBroadcast, bool discover, System.Net.NetworkInformation.NetworkInterface networkAdapter)
        {
            try
            {
                if (ipIcomLocal == null || ipIcomBroadcast == null || networkAdapter == null)
                {
                    Debug.WriteLine("Sending ICOM Dhcp request failed");
                    return;
                }

                Debug.WriteLine("Sending ICOM Dhcp request, discover={0}", (object) discover);
                byte[] macId = networkAdapter.GetPhysicalAddress().GetAddressBytes();
                byte[] localIpBytes = ipIcomLocal.GetAddressBytes();

                List<byte> response = new List<byte>();
                response.Add(0x01);     // Message Type Boot
                response.Add(0x01);     // Ethernet
                response.Add(0x06);     // Address length
                response.Add(0x00);     // Hops

                byte[] transId = new byte[4];
                _random.NextBytes(transId);
                response.AddRange(transId); // Transaction ID

                response.Add(0x00);     // Seconds
                response.Add(0x00);
                                        // Bootp flags
                uint bootpFlags = (uint) (discover ? 0x8000 : 0x0000);     // broadcast / unicast
                response.Add((byte) ((bootpFlags >> 8) & 0xFF));
                response.Add((byte) (bootpFlags & 0xFF));
                response.AddRange(localIpBytes);     // Client IP
                response.AddRange(localIpBytes);     // Own IP
                response.Add(0x00);     // Next server IP
                response.Add(0x00);
                response.Add(0x00);
                response.Add(0x00);
                response.Add(0x00);     // Relay agent IP
                response.Add(0x00);
                response.Add(0x00);
                response.Add(0x00);
                response.AddRange(macId);   // Client MAC
                response.AddRange(new byte[10]);        // padding
                response.AddRange(new byte[0x40]);      // server host name
                response.AddRange(new byte[0x80]);      // boot file name
                byte[] cookieBytes = new byte[] { 0x63, 0x82, 0x53, 0x63 };
                response.AddRange(cookieBytes);

                response.Add(53);   // Option DHCP message type
                response.Add(1);    // Length
                byte messageType = (byte) (discover ? 1 : 3);    // DHCP discover / request
                response.Add(messageType);    // DHCP discover

                response.Add(61);   // Option Client id
                response.Add(7);    // Length
                response.Add(1);    // Ethernet
                response.AddRange(macId);   // Client MAC

                if (!discover)
                {
                    response.Add(50);   // Option Requested IP
                    response.Add((byte)localIpBytes.Length);    // Length
                    response.AddRange(localIpBytes);     // Local ip
                }

                string hostName = "DIAGADR10BMWMAC" + BitConverter.ToString(macId).Replace("-", "");
                byte[] hostNameBytes = Encoding.ASCII.GetBytes(hostName);
                response.Add(12);   // Option Host name
                response.Add((byte)hostNameBytes.Length);   // Length
                response.AddRange(hostNameBytes);     // Host Name

                byte[] parameterList = new byte[] { 1, 6, 12, 15, 3, 28 };
                response.Add(55);   // Option Parameter request list
                response.Add((byte)parameterList.Length);    // Length
                response.AddRange(parameterList);            // Parameter list

                response.Add(255);   // Option End

                SendUdpPacketTo(response.ToArray(), new IPEndPoint(ipIcomBroadcast, 67), 68);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Sending ICOM Dhcp request failed: {0}", (object)ex.Message);
            }
        }

        private void StartSrvLocListen()
        {
            _srvLocClient?.BeginReceive(SrvLocReceiver, new Object());
        }

        private void SrvLocReceiver(IAsyncResult ar)
        {
            try
            {
                UdpClient srvLocClientLocal = _srvLocClient;
                if (srvLocClientLocal == null)
                {
                    return;
                }
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = srvLocClientLocal.EndReceive(ar, ref ip);
                IPAddress localIp = GetLocalIpAddress(ip.Address, false, out _, out _);
#if true
                if (bytes != null)
                {
                    DebugLogData("srvLoc: ", bytes, bytes.Length);
                }
#endif
                if (localIp != null &&
                    bytes != null && bytes.Length >= 12 &&
                    bytes[0] == 0x02 &&     // Version 2
                    bytes[1] == 0x06)       // Attribute request
                {
                    Debug.WriteLine("SrvLoc request from: {0}:{1}, at {2}", ip.Address, ip.Port, localIp);
                    int packetlength = (bytes[2] << 16) | (bytes[3] << 8) | bytes[4];
                    int flags = (bytes[5] << 8) | bytes[6];
                    int nextExtOffset = (bytes[7] << 16) | (bytes[8] << 8) | bytes[9];
                    int xId = (bytes[10] << 8) | bytes[11];

                    Debug.WriteLine("SrvLoc Len={0}, Flags={1:X04}, Offs={2}, XID={3}", packetlength, flags, nextExtOffset, xId);

                    int resExtOffset = 0;
                    List<byte> response = new List<byte>();
                    response.Add(0x02);     // Version
                    response.Add(0x07);     // Attribute reply
                    response.Add(0x00);     // Length (3 byte)
                    response.Add(0x00);
                    response.Add(0x00);
                    response.Add(0x00);     // Flags (2 Byte)
                    response.Add(0x00);
                    response.Add((byte)(resExtOffset >> 16));
                    response.Add((byte)(resExtOffset >> 8));
                    response.Add((byte)(resExtOffset & 0xFF));  // Extension Offset
                    response.Add((byte)(xId >> 8));             // XID
                    response.Add((byte)(xId & 0xFF));

                    byte[] langBytes = Encoding.ASCII.GetBytes(@"en");
                    int langLen = langBytes.Length;
                    response.Add((byte)(langLen >> 8));      // Lang len
                    response.Add((byte)(langLen & 0xFF));
                    response.AddRange(langBytes);

                    response.Add(0x00);         // Error code (2 Byte)
                    response.Add(0x00);

                    bool doIpMode = (_enetCommType & EnetCommType.DoIp) == EnetCommType.DoIp;
                    string doIpSupport = doIpMode ? "Yes" : "No";
                    string attributs =
                        string.Format("(DevId=G31),(Serial={2}{0}),(MacAddress={1}),(DevType=ENET),(Color=#00ff00),(State=4),(Kl15Voltage=12000),(Kl30Voltage=12000),(VIN={2}),(PowerSupply=12000),(VciChannels=[0?;1?;2?;3+]),(IPAddress={3}),(DoIP={4})",
                            BitConverter.ToString(localIp.GetAddressBytes()).Replace("-", ""), TestMac, TestVin, localIp, doIpSupport);
                    byte[] attrBytes = Encoding.ASCII.GetBytes(attributs);
                    int attrLen = attrBytes.Length;
                    response.Add((byte)(attrLen >> 8));      // List length
                    response.Add((byte)(attrLen & 0xFF));
                    response.AddRange(attrBytes);
                    response.Add(0x00);         // Auth

                    int resLength = response.Count;
                    response[2] = (byte)(resLength >> 16);     // Length
                    response[3] = (byte)(resLength >> 8);
                    response[4] = (byte)(resLength & 0xFF);

                    SendUdpPacketTo(response.ToArray(), ip, SrvLocPort);
                }
                StartSrvLocListen();
            }
            catch (Exception)
            {
                _srvLocError = true;
            }
        }

        private void EnetControlClose(BmwTcpClientData bmwTcpClientData)
        {
            bool changed = false;
            try
            {
                if (bmwTcpClientData.TcpClientStream != null)
                {
                    bmwTcpClientData.TcpClientStream.Close();
                    bmwTcpClientData.TcpClientStream = null;
                    changed = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (bmwTcpClientData.TcpClientConnection != null)
                {
                    Debug.WriteLine("Control Closed [{0}]: {1}", bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.ControlPort);
                    bmwTcpClientData.TcpClientConnection.Close();
                    bmwTcpClientData.TcpClientConnection = null;
                    changed = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (changed)
            {
                GetClientConnections();
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ReceiveEnetControl(BmwTcpClientData bmwTcpClientData)
        {
            try
            {
                if (!IsTcpClientConnected(bmwTcpClientData.TcpClientConnection))
                {
                    EnetControlClose(bmwTcpClientData);
                    if (!bmwTcpClientData.BmwTcpChannel.TcpServerControl.Pending())
                    {
                        return false;
                    }
                    bmwTcpClientData.TcpClientConnection = bmwTcpClientData.BmwTcpChannel.TcpServerControl.AcceptTcpClient();
                    bmwTcpClientData.TcpClientConnection.SendBufferSize = TcpSendBufferSize;
                    bmwTcpClientData.TcpClientConnection.SendTimeout = TcpSendTimeout;
                    bmwTcpClientData.TcpClientConnection.NoDelay = true;
                    bmwTcpClientData.TcpClientStream = bmwTcpClientData.TcpClientConnection.GetStream();
                    bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    bmwTcpClientData.LastTcpSendTick = DateTime.MinValue.Ticks;
                    Debug.WriteLine("Control connected [{0}], Port={1}, Local={2}, Remote={3}",
                        bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.ControlPort,
                        bmwTcpClientData.TcpClientConnection.Client.LocalEndPoint.ToString(),
                        bmwTcpClientData.TcpClientConnection.Client.RemoteEndPoint.ToString());

                    GetClientConnections();
                }
            }
            catch (Exception)
            {
                EnetControlClose(bmwTcpClientData);
            }

            try
            {
                if (bmwTcpClientData.TcpClientStream is NetworkStream tcpClientStream && tcpClientStream.DataAvailable)
                {
                    byte[] dataBuffer = new byte[MaxBufferLength];
                    int recLen = tcpClientStream.Read(dataBuffer, 0, dataBuffer.Length);
#if true
                    DebugLogData("Ctrl Rec: ", dataBuffer, recLen);
#endif
                    byte[] responseBuffer = GetControlResponse(dataBuffer, recLen);
                    if (responseBuffer != null)
                    {
                        WriteNetworkStream(bmwTcpClientData, responseBuffer, 0, responseBuffer.Length);
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        private byte[] GetControlResponse(byte[] recData, int recLen)
        {
            if (recLen >= 6 && recData[5] == 0x10)
            {   // ignition state
                // send response
                byte[] responseBuffer = new byte[6 + 1];
                responseBuffer[2] = (byte)((responseBuffer.Length - 6) >> 8);
                responseBuffer[3] = (byte)(responseBuffer.Length - 6);
                responseBuffer[4] = 0x00;
                responseBuffer[5] = 0x10;   // ignition state
                responseBuffer[6] = (byte)(IgnitionOk ? 0x05 : 0x00);   // Clamp state, Bit3,4 = 1 -> ignition on
                return responseBuffer;
            }

            return null;
        }

        private void EnetDiagClose(BmwTcpClientData bmwTcpClientData)
        {
            bool changed = false;
            try
            {
                if (bmwTcpClientData.TcpClientStream != null)
                {
                    bmwTcpClientData.TcpClientStream.Close();
                    bmwTcpClientData.TcpClientStream = null;
                    changed = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (bmwTcpClientData.TcpClientConnection != null)
                {
                    Debug.WriteLine("Diag Closed[{0}]: {1}", bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.DiagPort);
                    bmwTcpClientData.TcpClientConnection.Close();
                    bmwTcpClientData.TcpClientConnection = null;
                    changed = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (changed)
            {
                GetClientConnections();
            }
        }

        private bool ReceiveEnet(byte[] receiveData, BmwTcpClientData bmwTcpClientData)
        {
            if (bmwTcpClientData == null)
            {
                return false;
            }

            if (bmwTcpClientData.BmwTcpChannel?.TcpServerDiag == null)
            {
                return false;
            }

            try
            {
                if (!IsTcpClientConnected(bmwTcpClientData.TcpClientConnection))
                {
                    EnetDiagClose(bmwTcpClientData);
                    if (!bmwTcpClientData.BmwTcpChannel.TcpServerDiag.Pending())
                    {
                        return false;
                    }

                    Debug.WriteLine("Diag connect request [{0}], Port={1}", bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.DiagPort);
                    bmwTcpClientData.TcpClientConnection = bmwTcpClientData.BmwTcpChannel.TcpServerDiag.AcceptTcpClient();
                    bmwTcpClientData.TcpClientConnection.SendBufferSize = TcpSendBufferSize;
                    bmwTcpClientData.TcpClientConnection.SendTimeout = TcpSendTimeout;
                    bmwTcpClientData.TcpClientConnection.NoDelay = true;
                    bmwTcpClientData.TcpClientStream = bmwTcpClientData.TcpClientConnection.GetStream();
                    bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    bmwTcpClientData.LastTcpSendTick = DateTime.MinValue.Ticks;
                    bmwTcpClientData.TcpNackIndex = 0;
                    bmwTcpClientData.TcpDataIndex = 0;
                    Debug.WriteLine("Diag connected [{0}], Port={1}, Local={2}, Remote={3}",
                        bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.DiagPort,
                        bmwTcpClientData.TcpClientConnection.Client.LocalEndPoint.ToString(),
                        bmwTcpClientData.TcpClientConnection.Client.RemoteEndPoint.ToString());

                    GetClientConnections();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Diag exception [{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.DiagPort, ex.Message);
                EnetDiagClose(bmwTcpClientData);
            }

            try
            {
                if (bmwTcpClientData.TcpClientStream != null)
                {
                    if ((Stopwatch.GetTimestamp() - bmwTcpClientData.LastTcpRecTick) > 2000 * TickResolMs)
                    {
                        bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                        byte[] dataBuffer = new byte[6 + 2];
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;
                        dataBuffer[2] = 0x00;
                        dataBuffer[3] = 0x02;
                        dataBuffer[4] = 0x00;
                        dataBuffer[5] = 0x12;   // Payoad type: alive check
                        dataBuffer[6] = 0xF4;
                        dataBuffer[7] = 0x00;
                        WriteNetworkStream(bmwTcpClientData, dataBuffer, 0, dataBuffer.Length);
                        Debug.WriteLine("Alive Check [{0}], Port={1}", bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.DiagPort);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Keep alive exception [{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.BmwTcpChannel.DiagPort, ex.Message);
                // ignored
            }

            try
            {
                if (bmwTcpClientData.TcpClientStream is NetworkStream tcpClientStream && tcpClientStream.DataAvailable)
                {
                    bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    byte[] dataBuffer = new byte[MaxBufferLength];
                    int recLen = tcpClientStream.Read(dataBuffer, 0, 6);
                    if (recLen < 6)
                    {
                        return false;
                    }
                    int payloadLength = (((int)dataBuffer[0] << 24) | ((int)dataBuffer[1] << 16) | ((int)dataBuffer[2] << 8) | dataBuffer[3]);
                    if (payloadLength > dataBuffer.Length - 6)
                    {
                        ClearNetworkStream(bmwTcpClientData.TcpClientStream);
                        Debug.WriteLine("Payload length too long: {0} > {1}", payloadLength, dataBuffer.Length - 6);
                        return false;
                    }
                    if (payloadLength > 0)
                    {
                        recLen += tcpClientStream.Read(dataBuffer, 6, payloadLength);
                    }
                    if (recLen < payloadLength + 6)
                    {
                        return false;
                    }
#if false
                    DebugLogData("Diag Rec: ", dataBuffer, recLen);
#endif
                    int dataLen = payloadLength - 2;
                    if ((dataLen < 1) || ((dataLen + 8) > recLen))
                    {
                        return false;
                    }
                    int payloadType = dataBuffer[5];
                    if (payloadType != 0x01)
                    {
                        return false;
                    }
                    // send ack
#if false
                    if (bmwTcpClientData.TcpNackIndex >= 5)
                    {
                        Debug.WriteLine("Send NAck");
                        bmwTcpClientData.TcpNackIndex = 0;
                        byte[] nack = new byte[6];
                        nack[5] = 0xFF;     // nack
                        WriteNetworkStream(bmwTcpClientData, nack, 0, nack.Length);
                        return false;
                    }
                    bmwTcpClientData.TcpNackIndex++;
#endif
#if false
                    if (bmwTcpClientData.TcpLastResponse != null)
                    {
                        Debug.WriteLine("Inject old response");
                        WriteNetworkStream(bmwTcpClientData, bmwTcpClientData.TcpLastResponse, 0, bmwTcpClientData.TcpLastResponse.Length);
                    }
#endif

                    int ackLen = recLen;
                    if (ackLen > EdInterfaceEnet.MaxAckLength)
                    {
                        ackLen = EdInterfaceEnet.MaxAckLength;
                        Debug.WriteLine("Ack length limited");
                    }

                    byte[] ack = new byte[ackLen];
                    Array.Copy(dataBuffer, ack, ack.Length);

                    int ackPayloadLength = ackLen - 6;
                    ack[0] = (byte)((ackPayloadLength >> 24) & 0xFF);
                    ack[1] = (byte)((ackPayloadLength >> 16) & 0xFF);
                    ack[2] = (byte)((ackPayloadLength >> 8) & 0xFF);
                    ack[3] = (byte)(ackPayloadLength & 0xFF);
                    ack[4] = 0x00;
                    ack[5] = 0x02;
#if false
                    DebugLogData("Send Ack: ", ack, ack.Length);
#endif
                    WriteNetworkStream(bmwTcpClientData, ack, 0, ackLen);

                    // create BMW-FAST telegram
                    byte sourceAddr = dataBuffer[6];
                    byte targetAddr = dataBuffer[7];
                    int len;
                    if (sourceAddr == TcpTesterAddr) sourceAddr = 0xF1;
                    if (dataLen > 0x3F)
                    {
                        if (dataLen > 0xFF)
                        {
                            receiveData[0] = 0x80;
                            receiveData[1] = targetAddr;
                            receiveData[2] = sourceAddr;
                            receiveData[3] = 0;
                            receiveData[4] = (byte)(dataLen >> 8);
                            receiveData[5] = (byte)(dataLen & 0xFF);
                            Array.Copy(dataBuffer, 8, receiveData, 6, dataLen);
                            len = dataLen + 6;
                        }
                        else
                        {
                            receiveData[0] = 0x80;
                            receiveData[1] = targetAddr;
                            receiveData[2] = sourceAddr;
                            receiveData[3] = (byte)dataLen;
                            Array.Copy(dataBuffer, 8, receiveData, 4, dataLen);
                            len = dataLen + 4;
                        }
                    }
                    else
                    {
                        receiveData[0] = (byte)(0x80 | dataLen);
                        receiveData[1] = targetAddr;
                        receiveData[2] = sourceAddr;
                        Array.Copy(dataBuffer, 8, receiveData, 3, dataLen);
                        len = dataLen + 3;
                    }

                    if (IsFunctionalAddress(targetAddr))
                    {   // functional address
                        receiveData[0] |= 0xC0;
                    }
                    receiveData[len] = CalcChecksumBmwFast(receiveData, len);
                    return true;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            catch (Exception)
            {
                Thread.Sleep(10);
                return false;
            }
            return false;
        }

        private bool SendEnet(byte[] sendData, BmwTcpClientData bmwTcpClientData)
        {
            if (bmwTcpClientData?.TcpClientStream == null)
            {
                return false;
            }
            try
            {
                byte targetAddr = sendData[1];
                byte sourceAddr = sendData[2];
                if (targetAddr == 0xF1) targetAddr = TcpTesterAddr;
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
                byte[] dataBuffer = new byte[dataLength + 8];
                int payloadLength = dataLength + 2;
                dataBuffer[0] = (byte)((payloadLength >> 24) & 0xFF);
                dataBuffer[1] = (byte)((payloadLength >> 16) & 0xFF);
                dataBuffer[2] = (byte)((payloadLength >> 8) & 0xFF);
                dataBuffer[3] = (byte)(payloadLength & 0xFF);
                dataBuffer[4] = 0x00;
                dataBuffer[5] = 0x01;   // Payoad type: Diag message
                dataBuffer[6] = sourceAddr;
                dataBuffer[7] = targetAddr;
                Array.Copy(sendData, dataOffset, dataBuffer, 8, dataLength);
#if false
                DebugLogData("Send: ", dataBuffer, dataBuffer.Length);
#endif
                WriteNetworkStream(bmwTcpClientData, dataBuffer, 0, dataBuffer.Length);
                bmwTcpClientData.TcpLastResponse = dataBuffer;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DoIpClose(BmwTcpClientData bmwTcpClientData)
        {
            bool changed = false;
            try
            {
                if (bmwTcpClientData.TcpClientStream != null)
                {
                    bmwTcpClientData.TcpClientStream.Close();
                    bmwTcpClientData.TcpClientStream = null;
                    changed = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                if (bmwTcpClientData.TcpClientConnection != null)
                {
                    Debug.WriteLine("DoIp Closed[{0}]: {1}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort);
                    bmwTcpClientData.TcpClientConnection.Close();
                    bmwTcpClientData.TcpClientConnection = null;
                    changed = true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (changed)
            {
                GetClientConnections();
            }
        }

        private bool ReceiveDoIp(byte[] receiveData, BmwTcpClientData bmwTcpClientData)
        {
            if (bmwTcpClientData == null)
            {
                return false;
            }

            if (bmwTcpClientData.BmwTcpChannel?.TcpServerDoIp == null)
            {
                return false;
            }

            try
            {
                if (!IsTcpClientConnected(bmwTcpClientData.TcpClientConnection))
                {
                    DoIpClose(bmwTcpClientData);
                    TcpListener tcpServer = bmwTcpClientData.DoIpSsl ? bmwTcpClientData.BmwTcpChannel.TcpServerDoIpSsl : bmwTcpClientData.BmwTcpChannel.TcpServerDoIp;
                    if (!tcpServer.Pending())
                    {
                        return false;
                    }

                    Debug.WriteLine("DoIp connect request [{0}], Port={1}, SSL={2}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort, bmwTcpClientData.DoIpSsl);
                    bmwTcpClientData.TcpClientConnection = tcpServer.AcceptTcpClient();
                    bmwTcpClientData.TcpClientConnection.SendBufferSize = TcpSendBufferSize;
                    bmwTcpClientData.TcpClientConnection.SendTimeout = TcpSendTimeout;
                    bmwTcpClientData.TcpClientConnection.NoDelay = true;
                    if (bmwTcpClientData.DoIpSsl)
                    {
                        if (ServerUseBcSsl)
                        {
                            bmwTcpClientData.TcpClientStream = CreateBcSslStream(bmwTcpClientData.TcpClientConnection, ServerCertFile, _serverCAs);
                        }
                        else
                        {
                            bmwTcpClientData.TcpClientStream = CreateSslStream(bmwTcpClientData.TcpClientConnection, _serverCertificate);
                        }
                    }
                    else
                    {
                        bmwTcpClientData.TcpClientStream = bmwTcpClientData.TcpClientConnection.GetStream();
                    }
                    bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    bmwTcpClientData.LastTcpSendTick = DateTime.MinValue.Ticks;
                    bmwTcpClientData.TcpNackIndex = 0;
                    bmwTcpClientData.TcpDataIndex = 0;
                    Debug.WriteLine("DoIp connected [{0}], Port={1}, Local={2}, Remote={3}",
                        bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort,
                        bmwTcpClientData.TcpClientConnection.Client.LocalEndPoint.ToString(),
                        bmwTcpClientData.TcpClientConnection.Client.RemoteEndPoint.ToString());

                    GetClientConnections();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoIp exception [{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort, ex.Message);
                SendDoIpError(0x00, bmwTcpClientData);  // incorrect pattern format
                DoIpClose(bmwTcpClientData);
            }

            try
            {
                if (bmwTcpClientData.TcpClientStream != null)
                {
                    if ((Stopwatch.GetTimestamp() - bmwTcpClientData.LastTcpRecTick) > 2000 * TickResolMs)
                    {
                        bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();

                        // the message is acknowledged by Ediabas, but there is no timeout processing
                        List<byte> responseList = new List<byte>();
                        uint resPayloadType = 0x0007;   // keep alive check
                        uint resPayloadLength = 0;
                        responseList.Add(DoIpProtoVer);
                        responseList.Add(~DoIpProtoVer & 0xFF);
                        responseList.Add((byte)(resPayloadType >> 8));
                        responseList.Add((byte)resPayloadType);
                        responseList.Add((byte)(resPayloadLength >> 24));
                        responseList.Add((byte)(resPayloadLength >> 16));
                        responseList.Add((byte)(resPayloadLength >> 8));
                        responseList.Add((byte)resPayloadLength);

                        byte[] resBytes = responseList.ToArray();
                        WriteNetworkStream(bmwTcpClientData, resBytes, 0, resBytes.Length);
                        Debug.WriteLine("DoIp Alive Check [{0}], Port={1}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DoIp Keep alive exception [{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort, ex.Message);
                // ignored
            }

            try
            {
                if (bmwTcpClientData.TcpClientStream != null)
                {
                    NetworkStream networkStream = bmwTcpClientData.TcpClientStream as NetworkStream;

                    if (networkStream != null && !networkStream.DataAvailable)
                    {
                        Debug.WriteLine("DoIp No data available [{0}], Port={1}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort);
                        return false;
                    }

                    bmwTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    byte[] dataBuffer = new byte[MaxBufferLength];
                    int recLen = ReadNetworkStream(bmwTcpClientData.TcpClientStream, dataBuffer, 0, 8);
                    if (recLen < 8)
                    {
                        Debug.WriteLine("DoIp header too short [{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort, recLen);
                        return false;
                    }
                    int payloadLength = (((int)dataBuffer[4] << 24) | ((int)dataBuffer[5] << 16) | ((int)dataBuffer[6] << 8) | dataBuffer[7]);
                    if (payloadLength > dataBuffer.Length - 8)
                    {
                        ClearNetworkStream(bmwTcpClientData.TcpClientStream);
                        Debug.WriteLine("DoIp Rec data buffer overflow [{0}], Port={1}: {2}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort, payloadLength);
                        return false;
                    }

                    if (payloadLength > 0)
                    {
                        recLen += ReadNetworkStream(bmwTcpClientData.TcpClientStream, dataBuffer, recLen, payloadLength);
                    }

                    if (recLen < payloadLength + 8)
                    {
                        Debug.WriteLine("DoIp rec data length too short [{0}], Port={1}: {2} < {3}", bmwTcpClientData.Index, bmwTcpClientData.UsedDoIpPort, recLen, payloadLength + 8);
                        return false;
                    }
#if true
                    DebugLogData("DoIp Rec: ", dataBuffer, recLen);
#endif
                    byte protoVersion = dataBuffer[0];
                    byte protoVersionInv = dataBuffer[1];
                    if (protoVersion != (byte)~protoVersionInv)
                    {
                        Debug.WriteLine("DoIp protocol version invalid [{0}], {1}: {2}", bmwTcpClientData.Index, protoVersion, protoVersionInv);
                        SendDoIpError(0x00, bmwTcpClientData);  // incorrect pattern format
                        DoIpClose(bmwTcpClientData);
                        return false;
                    }
                    uint payloadType = (((uint)dataBuffer[2] << 8) | dataBuffer[3]);

                    int dataLen = 0;
                    uint resPayloadType = 0x0000;
                    List<byte> resData = new List<byte>();
                    switch (payloadType)
                    {
                        case 0x0005: // routing activation request
                        {
                            Debug.WriteLine("DoIp routing activation");
                            if (payloadLength < 11)
                            {
                                Debug.WriteLine("DoIp routing activate length too short: {0}", (object)payloadLength);
                                break;
                            }

                            // enable vehicle full searching
                            _lastDoIpIdentAddr = null;

                            uint srcAddr = (((uint)dataBuffer[8] << 8) | dataBuffer[9]);
                            resPayloadType = 0x0006;    // routing activation response
                            // tester address
                            resData.Add((byte)(srcAddr >> 8));
                            resData.Add((byte)srcAddr);
                            // log address
                            resData.Add((byte)(_doIpGwAddr >> 8));
                            resData.Add((byte)(_doIpGwAddr & 0xFF));
                            // response code
                            resData.Add(0x10);
                            // reserved
                            resData.AddRange(new byte[4]);
                            //Thread.Sleep(1900);   // valid max delay
                            break;
                        }

                        case 0x8001: // diagostic message
                        {
                            if (payloadLength < 5)
                            {
                                Debug.WriteLine("DoIp diag msg length too short: {0}", (object)payloadLength);
                                break;
                            }

                            int resDataLen = payloadLength - 4;
                            int previousDataLen = resDataLen;
                            if (previousDataLen > EdInterfaceEnet.MaxDoIpAckLength)
                            {
                                previousDataLen = EdInterfaceEnet.MaxDoIpAckLength;
                                Debug.WriteLine("Ack length limited");
                            }
#if false
                            if (bmwTcpClientData.TcpNackIndex >= 5)
                            {
                                Debug.WriteLine("Send NAck");
                                bmwTcpClientData.TcpNackIndex = 0;
                                resPayloadType = 0x8003;        // diagnostic message nack
                                resData.Add(dataBuffer[10]);    // source address
                                resData.Add(dataBuffer[11]);
                                resData.Add(dataBuffer[8]);     // target address
                                resData.Add(dataBuffer[9]);
                                resData.Add(0x00);         // retry
                                resData.AddRange(dataBuffer.Skip(12).Take(previousDataLen));
                                break;
                            }
                            bmwTcpClientData.TcpNackIndex++;
#endif
#if false
                            if (bmwTcpClientData.TcpNackIndex >= 5)
                            {
                                Debug.WriteLine("Send Ack error");
                                bmwTcpClientData.TcpNackIndex = 0;
                                resPayloadType = 0x8002;        // diagnostic message ack
                                resData.Add(dataBuffer[10]);    // source address
                                resData.Add(dataBuffer[11]);
                                resData.Add(dataBuffer[8]);     // target address
                                resData.Add(dataBuffer[9]);
                                resData.Add(0x01);         // invalid
                                break;
                            }
                            bmwTcpClientData.TcpNackIndex++;
#endif
#if false
                            if (bmwTcpClientData.TcpNackIndex >= 2)
                            {
                                Debug.WriteLine("Send No Ack");
                                bmwTcpClientData.TcpNackIndex = 0;
                                break;
                            }
                            bmwTcpClientData.TcpNackIndex++;
#endif
                            dataLen = resDataLen;

                            resPayloadType = 0x8002;        // diagnostic message ack
                            resData.Add(dataBuffer[10]);    // source address
                            resData.Add(dataBuffer[11]);
                            resData.Add(dataBuffer[8]);     // target address
                            resData.Add(dataBuffer[9]);
                            resData.Add(0x00);          // ACK
                            resData.AddRange(dataBuffer.Skip(12).Take(previousDataLen));
#if false
                            if (bmwTcpClientData.TcpDataIndex == 0)
                            {
                                Debug.WriteLine("Ignoring diag message");
                                dataLen = 0;
                                bmwTcpClientData.TcpDataIndex++;
                            }
                            else
                            {
                                bmwTcpClientData.TcpDataIndex = 0;
                            }
#endif
                            break;
                        }
                    }

                    if (resData.Count > 0)
                    {
                        List<byte> responseList = new List<byte>();
                        uint resPayloadLength = (uint)resData.Count;
                        responseList.Add(DoIpProtoVer);
                        responseList.Add(~DoIpProtoVer & 0xFF);
                        responseList.Add((byte)(resPayloadType >> 8));
                        responseList.Add((byte)resPayloadType);
                        responseList.Add((byte)(resPayloadLength >> 24));
                        responseList.Add((byte)(resPayloadLength >> 16));
                        responseList.Add((byte)(resPayloadLength >> 8));
                        responseList.Add((byte)resPayloadLength);
                        responseList.AddRange(resData);

                        byte[] resBytes = responseList.ToArray();
                        DebugLogData("DoIp Res: ", resBytes, resBytes.Length);

                        WriteNetworkStream(bmwTcpClientData, resBytes, 0, resBytes.Length);
                    }

                    if (dataLen > 0)
                    {
                        // create BMW-FAST telegram
                        int len;
                        uint srcAddr = (((uint)dataBuffer[8] << 8) | dataBuffer[9]);
                        uint targAddr = (((uint)dataBuffer[10] << 8) | dataBuffer[11]);
                        byte sourceAddr = (byte)srcAddr;
                        byte targetAddr = (byte)targAddr;
                        bmwTcpClientData.LastTesterAddress = null;
                        if (srcAddr >= DoIpTesterAddrMin && srcAddr <= DoIpTesterAddrMax)
                        {
                            bmwTcpClientData.LastTesterAddress = srcAddr;
                            sourceAddr = 0xF1;
                        }

                        if (dataLen > 0x3F)
                        {
                            if (dataLen > 0xFF)
                            {
                                receiveData[0] = 0x80;
                                receiveData[1] = targetAddr;
                                receiveData[2] = sourceAddr;
                                receiveData[3] = 0;
                                receiveData[4] = (byte)(dataLen >> 8);
                                receiveData[5] = (byte)(dataLen & 0xFF);
                                Array.Copy(dataBuffer, 12, receiveData, 6, dataLen);
                                len = dataLen + 6;
                            }
                            else
                            {
                                receiveData[0] = 0x80;
                                receiveData[1] = targetAddr;
                                receiveData[2] = sourceAddr;
                                receiveData[3] = (byte)dataLen;
                                Array.Copy(dataBuffer, 12, receiveData, 4, dataLen);
                                len = dataLen + 4;
                            }
                        }
                        else
                        {
                            receiveData[0] = (byte)(0x80 | dataLen);
                            receiveData[1] = targetAddr;
                            receiveData[2] = sourceAddr;
                            Array.Copy(dataBuffer, 12, receiveData, 3, dataLen);
                            len = dataLen + 3;
                        }

                        if (IsFunctionalAddress(targetAddr))
                        {   // functional address
                            receiveData[0] |= 0xC0;
                        }
                        receiveData[len] = CalcChecksumBmwFast(receiveData, len);
                        return true;
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception)
            {
                Thread.Sleep(10);
                return false;
            }
            return false;
        }

        private bool SendDoIp(byte[] sendData, BmwTcpClientData bmwTcpClientData)
        {
            if (bmwTcpClientData?.TcpClientStream == null)
            {
                return false;
            }
            try
            {
                uint targetAddr = sendData[1];
                uint sourceAddr = sendData[2];
                if (targetAddr == 0xF1)
                {
                    if (bmwTcpClientData.LastTesterAddress != null)
                    {
                        targetAddr = bmwTcpClientData.LastTesterAddress.Value;
                    }
                    else
                    {
                        targetAddr = DoIpTesterAddrMin;
                    }
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

                int resPayloadLength = dataLength + 4;
                List<byte> responseList = new List<byte>();
                uint resPayloadType = 0x8001;    // Diagnostic message
                responseList.Add(DoIpProtoVer);
                responseList.Add(~DoIpProtoVer & 0xFF);
                responseList.Add((byte)(resPayloadType >> 8));
                responseList.Add((byte)(resPayloadType & 0xFF));
                responseList.Add((byte)(resPayloadLength >> 24));
                responseList.Add((byte)(resPayloadLength >> 16));
                responseList.Add((byte)(resPayloadLength >> 8));
                responseList.Add((byte)resPayloadLength);
                responseList.Add((byte)(sourceAddr >> 8));
                responseList.Add((byte)sourceAddr);
                responseList.Add((byte)(targetAddr >> 8));
                responseList.Add((byte)targetAddr);
                responseList.AddRange(sendData.Skip(dataOffset).Take(dataLength));
                byte[] resBytes = responseList.ToArray();
#if true
                DebugLogData("DoIp Send: ", resBytes, resBytes.Length);
#endif
                WriteNetworkStream(bmwTcpClientData, resBytes, 0, resBytes.Length);
                bmwTcpClientData.TcpLastResponse = resBytes;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool SendDoIpError(byte errorCode, BmwTcpClientData bmwTcpClientData)
        {
            try
            {
                uint resPayloadType = 0x0000;   // error response
                List<byte> responseList = new List<byte>();
                uint resPayloadLength = 1;
                responseList.Add(DoIpProtoVer);
                responseList.Add(~DoIpProtoVer & 0xFF);
                responseList.Add((byte)(resPayloadType >> 8));
                responseList.Add((byte)resPayloadType);
                responseList.Add((byte)(resPayloadLength >> 24));
                responseList.Add((byte)(resPayloadLength >> 16));
                responseList.Add((byte)(resPayloadLength >> 8));
                responseList.Add((byte)resPayloadLength);
                responseList.Add(errorCode);

                byte[] resBytes = responseList.ToArray();
                DebugLogData("DoIp Send Error: ", resBytes, resBytes.Length);

                WriteNetworkStream(bmwTcpClientData, resBytes, 0, resBytes.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool IsTcpClientConnected(TcpClient tcpClient)
        {
            try
            {
                if (tcpClient?.Client != null && tcpClient.Client.Connected)
                {
                    // Detect if client disconnected
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            // Client disconnected
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private SslStream CreateSslStream(TcpClient client, X509Certificate2 serverCertificate)
        {
            if (serverCertificate == null)
            {
                return null;
            }

            SslStream sslStream = new SslStream(client.GetStream(), false,
                (sender, certificate, chain, errors) =>
                {
                    switch (errors)
                    {
                        case SslPolicyErrors.None:
                            return true;

                        case SslPolicyErrors.RemoteCertificateNotAvailable:
                            Debug.WriteLine("CreateSslStream no remote certificate");
                            return true;

                        case SslPolicyErrors.RemoteCertificateChainErrors:
                            Debug.WriteLine("CreateSslStream Ignoring chain error");
                            return true;
                    }

                    Debug.WriteLine("CreateSslStream Certificate error: {0}", errors);
                    return false;
                });
            try
            {
                // Authenticate the server but don't require the client to authenticate.
                sslStream.ReadTimeout = SslAuthTimeout;
                sslStream.AuthenticateAsServer(serverCertificate, true, false);
                return sslStream;
            }
            catch (AuthenticationException e)
            {
                Debug.WriteLine("CreateSslStream Exception: {0}", (object)EdiabasNet.GetExceptionText(e));
                sslStream.Close();
                throw;
            }
        }

        private Stream CreateBcSslStream(TcpClient client, string serverCertFile, List<X509CertificateStructure> certificateAuthorities)
        {
            if (string.IsNullOrEmpty(serverCertFile))
            {
                return null;
            }

            Stream sslStream = null;
            try
            {
                NetworkStream clientStream = client.GetStream();
                clientStream.ReadTimeout = SslAuthTimeout;
                TlsServerProtocol tlsProtocol = new TlsServerProtocol(client.GetStream());
                tlsProtocol.IsResumableHandshake = true;
                BcTlsServer tlsServer = new BcTlsServer(serverCertFile, ServerCertPwd, certificateAuthorities);
                tlsServer.HandshakeTimeout = SslAuthTimeout;
                tlsProtocol.Accept(tlsServer);
                sslStream = tlsProtocol.Stream;
                return sslStream;
            }
            catch (Exception e)
            {
                Debug.WriteLine("CreateBcSslStream Exception: {0}", e.Message);
                sslStream?.Close();
                throw;
            }
        }

        private bool ReceiveCan(byte[] receiveData)
        {
#if CAN_DEBUG
            long lastReceiveTime = Stopwatch.GetTimestamp();
#endif
            const byte blocksize = 0;
            const byte sepTime = 0;
            const byte waitCount = 0;
            byte fcCount = 0;
            int len;
            byte blockCount = 0;
            byte sourceAddr = 0;
            byte targetAddr = 0;
            byte[] dataBuffer = null;

            int recLen = 0;
            _receiveStopWatch.Reset();
            _receiveStopWatch.Start();
            for (; ; )
            {
                for (; ; )
                {
                    TPCANStatus stsResult;
                    if (!_testMode)
                    {
                        long currentTime = Stopwatch.GetTimestamp();
                        if ((currentTime - _lastCanStatusTick) > 100 * TickResolMs)
                        {
                            _lastCanStatusTick = currentTime;
                            TPCANMsg sendMsg = new TPCANMsg
                            {
                                DATA = new byte[8],
                                ID = 0x130,
                                LEN = 5,
                                MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
                            };
                            sendMsg.DATA[0] = (byte)(0xC0 | (IgnitionOk ? 0x05 : 0x00));
                            sendMsg.DATA[1] = 0x40;
                            sendMsg.DATA[2] = 0xFF;
                            sendMsg.DATA[3] = 0xFF;
                            sendMsg.DATA[4] = 0xFF;
#if CAN_DEBUG
                            //Debug.WriteLine("Send Status: {0:X02}", sendMsg.DATA[0]);
#endif
                            stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                            {
#if CAN_DEBUG
                                Debug.WriteLine("Send Status failed: {0}", (object)stsResult);
#endif
                            }
                        }
                    }

                    stsResult = PCANBasic.Read(_pcanHandle, out TPCANMsg canMsg, out TPCANTimestamp _);
                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    {
                        break;
                    }
                    if ((canMsg.LEN < 2) || (canMsg.MSGTYPE != TPCANMessageType.PCAN_MESSAGE_STANDARD) ||
                        ((canMsg.ID & 0xFF00) != 0x0600))
                    {
                        continue;
                    }
                    byte frameType = (byte)((canMsg.DATA[1] >> 4) & 0x0F);
#if CAN_DEBUG
                    long receiveTime = Stopwatch.GetTimestamp();
                    long timeDiff = (receiveTime - lastReceiveTime) / TickResolMs;
                    lastReceiveTime = receiveTime;
                    //Debug.WriteLine(string.Format("CAN rec: {0:X03} {1}", canMsg.ID, BitConverter.ToString(canMsg.DATA, 0, canMsg.LEN).Replace("-", " ")));
                    Debug.WriteLine("Rec({0}): {1}", frameType, timeDiff);
#endif
                    if (recLen == 0)
                    {   // first telegram
                        sourceAddr = (byte)(canMsg.ID & 0xFF);
                        targetAddr = canMsg.DATA[0];
                        switch (frameType)
                        {
                            case 0: // single frame
                                len = canMsg.DATA[1] & 0x0F;
                                if (len > canMsg.LEN - 2)
                                {
                                    continue;
                                }
                                dataBuffer = new byte[len];
                                Array.Copy(canMsg.DATA, 2, dataBuffer, 0, len);
                                recLen = len;
                                _receiveStopWatch.Reset();
                                _receiveStopWatch.Start();
                                break;

                            case 1: // first frame
                                if (canMsg.LEN < 8)
                                {
                                    continue;
                                }
                                len = (((int)canMsg.DATA[1] & 0x0F) << 8) + canMsg.DATA[2];
                                dataBuffer = new byte[len];
                                Array.Copy(canMsg.DATA, 3, dataBuffer, 0, 5);
                                recLen = 5;
                                blockCount = 1;
                                {
                                    TPCANMsg sendMsg = new TPCANMsg
                                    {
                                        DATA = new byte[8],
                                        ID = (uint) (0x600 + targetAddr),
#if CAN_DYN_LEN
                                        LEN = 4,
#else
                                        LEN = 8,
#endif
                                        MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
                                    };
                                    sendMsg.DATA[0] = sourceAddr;
                                    sendMsg.DATA[1] = 0x30;  // FC
                                    sendMsg.DATA[2] = blocksize;    // Block size
                                    sendMsg.DATA[3] = sepTime;      // Min sep. Time
                                    fcCount = blocksize;
                                    stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                    {
#if CAN_DEBUG
                                        Debug.WriteLine("Send FC failed: {0}", (object)stsResult);
#endif
                                        _receiveStopWatch.Stop();
                                        return false;
                                    }
                                }
                                _receiveStopWatch.Reset();
                                _receiveStopWatch.Start();
                                break;

                            default:
                                continue;
                        }
                    }
                    else
                    {
                        if (frameType == 1)
                        {
                            continue;
                        }
                        if (frameType != 2)
                        {   // consecutive frame
                            continue;
                        }
                        if ((sourceAddr != (canMsg.ID & 0xFF)) || (targetAddr != canMsg.DATA[0]))
                        {
                            continue;
                        }
                        if ((canMsg.DATA[1] & 0x0F) != (blockCount & 0x0F))
                        {
                            continue;
                        }
                        if (dataBuffer == null)
                        {
                            return false;
                        }
                        len = dataBuffer.Length - recLen;
                        if (len > 6)
                        {
                            len = 6;
                        }
                        if (len > canMsg.LEN - 2)
                        {
                            continue;
                        }
                        Array.Copy(canMsg.DATA, 2, dataBuffer, recLen, len);
                        recLen += len;
                        blockCount++;
                        _receiveStopWatch.Reset();
                        _receiveStopWatch.Start();

                        if (fcCount > 0 && recLen < dataBuffer.Length)
                        {
                            fcCount--;
                            if (fcCount == 0)
                            {
#if CAN_DEBUG
                                Debug.WriteLine("Send FC");
#endif
                                TPCANMsg sendMsg = new TPCANMsg
                                {
                                    DATA = new byte[8],
                                    ID = (uint) (0x600 + targetAddr),
#if CAN_DYN_LEN
                                    LEN = 4,
#else
                                    LEN = 8,
#endif
                                    MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
                                };

                                // sleep test
                                for (int i = 0; i < waitCount; i++)
                                {
                                    Thread.Sleep(500);
#if CAN_DEBUG
                                    Debug.WriteLine("Send FC wait");
#endif
                                    sendMsg.DATA[0] = sourceAddr;
                                    sendMsg.DATA[1] = 0x31;         // FC, wait
                                    sendMsg.DATA[2] = blocksize;    // Block size
                                    sendMsg.DATA[3] = sepTime;      // Min sep. Time
                                    stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                    {
#if CAN_DEBUG
                                        Debug.WriteLine("Send FC failed: {0}", stsResult);
#endif
                                        _receiveStopWatch.Stop();
                                        return false;
                                    }
                                }
                                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                                if (waitCount > 0)
                                {
                                    Thread.Sleep(500);
                                }
#pragma warning restore 162
                                // ReSharper restore HeuristicUnreachableCode
                                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                _receiveStopWatch.Reset();
                                _receiveStopWatch.Start();

                                sendMsg.DATA[0] = sourceAddr;
                                sendMsg.DATA[1] = 0x30;         // FC
                                sendMsg.DATA[2] = blocksize;    // Block size
                                sendMsg.DATA[3] = sepTime;      // Min sep. Time
                                fcCount = blocksize;
                                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                {
#if CAN_DEBUG
                                    Debug.WriteLine("Send FC failed: {0}", (object)stsResult);
#endif
                                    _receiveStopWatch.Stop();
                                    return false;
                                }
                            }
                        }
                    }
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (dataBuffer != null && recLen >= dataBuffer.Length)
                    {
                        break;
                    }
                }
                if (dataBuffer != null && recLen >= dataBuffer.Length)
                {
                    break;
                }
                if (_receiveStopWatch.ElapsedMilliseconds > 1000)
                {
#if CAN_DEBUG
                    Debug.WriteLine("Rec Timeout");
#endif
                    _receiveStopWatch.Stop();
                    return false;
                }
                _pcanReceiveEvent.WaitOne(10);
                if (recLen == 0)
                {   // nothing received
                    _receiveStopWatch.Stop();
                    return false;
                }
            }
            _receiveStopWatch.Stop();
#if CAN_DEBUG
            Debug.WriteLine("Rec OK");
#endif
            if (dataBuffer.Length == 0)
            {
#if CAN_DEBUG
                Debug.WriteLine("Empty telegram");
#endif
                return false;
            }
            // create BMW-FAST telegram
            if (dataBuffer.Length > 0x3F)
            {
                if (dataBuffer.Length > 0xFF)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = 0;
                    receiveData[4] = (byte)(dataBuffer.Length >> 8);
                    receiveData[5] = (byte)dataBuffer.Length;
                    Array.Copy(dataBuffer, 0, receiveData, 6, dataBuffer.Length);
                    len = dataBuffer.Length + 6;
                }
                else
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = targetAddr;
                    receiveData[2] = sourceAddr;
                    receiveData[3] = (byte)dataBuffer.Length;
                    Array.Copy(dataBuffer, 0, receiveData, 4, dataBuffer.Length);
                    len = dataBuffer.Length + 4;
                }
            }
            else
            {
                receiveData[0] = (byte)(0x80 | dataBuffer.Length);
                receiveData[1] = targetAddr;
                receiveData[2] = sourceAddr;
                Array.Copy(dataBuffer, 0, receiveData, 3, dataBuffer.Length);
                len = dataBuffer.Length + 3;
            }

            if (IsFunctionalAddress(targetAddr))
            {   // functional address
                receiveData[0] |= 0xC0;
            }
            receiveData[len] = CalcChecksumBmwFast(receiveData, len);
            return true;
        }

        private bool SendCan(byte[] sendData)
        {
            TPCANMsg canMsg;
            TPCANStatus stsResult;
            TPCANMsg sendMsg = new TPCANMsg
            {
                DATA = new byte[8]
            };
            byte blockSize = 0;
            byte sepTime = 0;

            byte targetAddr = sendData[1];
            byte sourceAddr = sendData[2];
            int dataOffset = 3;
            int dataLength = sendData[0] & 0x3F;
            if (dataLength == 0)
            {   // with length byte
                if (sendData[3] == 0)
                {
                    dataLength = (sendData[4] << 8) + sendData[5];
                    dataOffset = 6;
                }
                else
                {
                    dataLength = sendData[3];
                    dataOffset = 4;
                }
            }

            if ((Stopwatch.GetTimestamp() - _lastCanSendTick) < 10 * TickResolMs)
            {
                Thread.Sleep(10);   // required for multiple telegrams
            }
            // clear input buffer
            while (PCANBasic.Read(_pcanHandle, out canMsg, out TPCANTimestamp _) == TPCANStatus.PCAN_ERROR_OK)
            {
            }

            if (dataLength <= 6)
            {   // single frame
                sendMsg.ID = (uint)(0x600 + sourceAddr);
#if CAN_DYN_LEN
                sendMsg.LEN = (byte)(2 + dataLength);
#else
                sendMsg.LEN = 8;
#endif
                sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                Array.Clear(sendMsg.DATA, 0, sendMsg.DATA.Length);
                sendMsg.DATA[0] = targetAddr;
                sendMsg.DATA[1] = (byte)(0x00 | dataLength);  // SF
                Array.Copy(sendData, dataOffset, sendMsg.DATA, 2, dataLength);
#if CAN_DEBUG
                //Debug.WriteLine(string.Format("CAN send SF: {0:X03} {1}", sendMsg.ID, BitConverter.ToString(sendMsg.DATA, 0, sendMsg.LEN).Replace("-", " ")));
#endif
                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
#if CAN_DEBUG
                    Debug.WriteLine("Send SF failed: {0}", (object)stsResult);
#endif
                    return false;
                }
                _lastCanSendTick = Stopwatch.GetTimestamp();
                return true;
            }
            // first frame
            sendMsg.ID = (uint)(0x600 + sourceAddr);
            sendMsg.LEN = 8;
            sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            Array.Clear(sendMsg.DATA, 0, sendMsg.DATA.Length);
            sendMsg.DATA[0] = targetAddr;
            sendMsg.DATA[1] = (byte)(0x10 | ((dataLength >> 8) & 0x0F));  // FF
            sendMsg.DATA[2] = (byte)dataLength;
            int len = 5;
            Array.Copy(sendData, dataOffset, sendMsg.DATA, 3, len);
            dataLength -= len;
            dataOffset += len;
#if CAN_DEBUG
            //Debug.WriteLine(string.Format("CAN send FF: {0:X03} {1}", sendMsg.ID, BitConverter.ToString(sendMsg.DATA, 0, sendMsg.LEN).Replace("-", " ")));
#endif
            stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
            {
#if CAN_DEBUG
                Debug.WriteLine("Send FF failed: {0}", (object)stsResult);
#endif
                return false;
            }
            bool waitForFc = true;
            byte blockCount = 1;

            for (; ; )
            {
                if (waitForFc)
                {
                    bool wait;
                    do
                    {
                        _receiveStopWatch.Reset();
                        _receiveStopWatch.Start();
                        for (; ; )
                        {
                            stsResult = PCANBasic.Read(_pcanHandle, out canMsg, out TPCANTimestamp _);
                            if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                            {
#if CAN_DEBUG
                                //Debug.WriteLine(string.Format("CAN rec FC: {0:X03} {1}", canMsg.ID, BitConverter.ToString(canMsg.DATA, 0, canMsg.LEN).Replace("-", " ")));
#endif
                                byte sourceRec = canMsg.DATA[0];
                                byte frameType = (byte)((canMsg.DATA[1] >> 4) & 0x0F);
                                bool sourceValid = sourceRec == sourceAddr;
                                if ((canMsg.LEN >= 4) && (canMsg.MSGTYPE == TPCANMessageType.PCAN_MESSAGE_STANDARD) &&
                                    ((canMsg.ID & 0xFF00) == 0x0600) &&
                                    ((canMsg.ID & 0xFF) == targetAddr) && sourceValid)
                                {
                                    if (frameType == 0x3)
                                    {
                                        break;
                                    }
#if CAN_DEBUG
                                    Debug.WriteLine("Unexpected frame type: {0}, aborting", (object)frameType);
#endif
                                    return false;
                                }
#if CAN_DEBUG
                                Debug.WriteLine(string.Format("CAN ignored: {0:X03} {1}", canMsg.ID, BitConverter.ToString(canMsg.DATA, 0, canMsg.LEN).Replace("-", " ")));
                                if (!sourceValid)
                                {
                                    Debug.WriteLine("Source address mismatch: received={0:X02}, expected={1:X02}", sourceRec, sourceAddr);
                                }
#endif
                            }
                            if (_receiveStopWatch.ElapsedMilliseconds > 1000)
                            {
                                _receiveStopWatch.Stop();
                                return false;
                            }
                        }
                        _receiveStopWatch.Stop();
                        switch (canMsg.DATA[1] & 0x0F)
                        {
                            case 0: // CTS
                                wait = false;
                                break;

                            case 1: // Wait
                                wait = true;
                                break;

                            default:
                                return false;
                        }
                        blockSize = canMsg.DATA[2];
                        sepTime = canMsg.DATA[3];
                    } while (wait);
#if CAN_DEBUG
                    Debug.WriteLine("FC: BS={0} ST={1}", blockSize, sepTime);
#endif
                }
#if false
                if (blockCount == 2)    // test telegram injection
                {
                    sendMsg.ID = (uint)(0x600 + sourceAddr + 1);
                    sendMsg.LEN = 8;
                    sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    sendMsg.DATA[0] = targetAddr;
                    sendMsg.DATA[1] = (byte)(0x00 | 6);  // SF
                    sendMsg.DATA[2] = 0xFF;
                    sendMsg.DATA[3] = 0xFF;
                    sendMsg.DATA[4] = 0xFF;
                    sendMsg.DATA[5] = 0xFF;
                    sendMsg.DATA[6] = 0xFF;
                    sendMsg.DATA[7] = 0xFF;
                    stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    {
#if CAN_DEBUG
                        Debug.WriteLine("Send SF failed: {0}", (object) stsResult);
#endif
                        return false;
                    }
                }
#endif
#if false
                if (blockCount == 2) // test telegram injection
                {   // first frame
                    sendMsg.ID = (uint) (0x600 + sourceAddr + 1);
                    sendMsg.LEN = 8;
                    sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    sendMsg.DATA[0] = targetAddr;
                    sendMsg.DATA[1] = (byte) (0x10 | 0); // FF
                    sendMsg.DATA[2] = (byte) 10;    // length
                    sendMsg.DATA[3] = 0xFF;
                    sendMsg.DATA[4] = 0xFF;
                    sendMsg.DATA[5] = 0xFF;
                    sendMsg.DATA[6] = 0xFF;
                    sendMsg.DATA[7] = 0xFF;
                }
                if (blockCount == 3) // test!
                {   // consecutive frame
                    sendMsg.ID = (uint)(0x600 + sourceAddr + 1);
                    sendMsg.LEN = 8;
                    sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    sendMsg.DATA[0] = targetAddr;
                    sendMsg.DATA[1] = (byte)(0x20 | 1);  // CF, block count
                    sendMsg.DATA[2] = 0xFF;
                    sendMsg.DATA[3] = 0xFF;
                    sendMsg.DATA[4] = 0xFF;
                    sendMsg.DATA[5] = 0xFF;
                    sendMsg.DATA[6] = 0xFF;
                    sendMsg.DATA[7] = 0xFF;
                }
#endif
                // consecutive frame
                len = dataLength;
                if (len > 6)
                {
                    len = 6;
                }
#if CAN_DEBUG
                //Debug.WriteLine("Send CC: Source={0:X02} Target={1:X02} Len={2}", sourceAddr, targetAddr, len);
#endif
                sendMsg.ID = (uint)(0x600 + sourceAddr);
#if CAN_DYN_LEN
                sendMsg.LEN = (byte)(2 + len);
#else
                sendMsg.LEN = 8;
#endif
                sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                Array.Clear(sendMsg.DATA, 0, sendMsg.DATA.Length);
                sendMsg.DATA[0] = targetAddr;
                sendMsg.DATA[1] = (byte)(0x20 | (blockCount & 0x0F));  // CF
                Array.Copy(sendData, dataOffset, sendMsg.DATA, 2, len);
                dataLength -= len;
                dataOffset += len;
                blockCount++;
                //Thread.Sleep(900);    // timeout test
#if CAN_DEBUG
                //Debug.WriteLine(string.Format("CAN send CC: {0:X03} {1}", sendMsg.ID, BitConverter.ToString(sendMsg.DATA, 0, sendMsg.LEN).Replace("-", " ")));
#endif
                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
#if CAN_DEBUG
                    Debug.WriteLine("Send CC failed: {0}", (object) stsResult);
#endif
                    return false;
                }
                if (dataLength <= 0)
                {
                    break;
                }

                waitForFc = false;
                if (blockSize > 0)
                {
                    if (blockSize == 1)
                    {
                        waitForFc = true;
                    }
                    blockSize--;
                }
                if (!waitForFc && sepTime > 0)
                {
                    Thread.Sleep(sepTime);
                }
            }
            _lastCanSendTick = Stopwatch.GetTimestamp();
            return true;
        }

        private bool ReceiveCanVw(byte[] receiveData, TPCANMsg? canMsgLast = null)
        {
#if CAN_DEBUG
            long lastReceiveTime = Stopwatch.GetTimestamp();
#endif
            const byte blocksize = 0;
            const byte sepTime = 0;
            const byte waitCount = 0;
            byte fcCount = 0;
            int len;
            byte blockCount = 0;
            uint targetAddr = 0;
            uint testerAddr = 0;
            byte[] dataBuffer = null;

            int recLen = 0;
            _receiveStopWatch.Reset();
            _receiveStopWatch.Start();
            for (;;)
            {
                for (;;)
                {
                    TPCANMsg canMsg;
                    TPCANStatus stsResult;
                    if (canMsgLast.HasValue)
                    {
                        canMsg = canMsgLast.Value;
                        canMsgLast = null;
                    }
                    else
                    {
                        stsResult = PCANBasic.Read(_pcanHandle, out canMsg, out TPCANTimestamp _);
                        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                        {
                            break;
                        }
                    }
                    if ((canMsg.LEN < 1) || (canMsg.MSGTYPE != TPCANMessageType.PCAN_MESSAGE_STANDARD))
                    {
                        continue;
                    }
                    byte frameType = (byte)((canMsg.DATA[0] >> 4) & 0x0F);
#if CAN_DEBUG
                    long receiveTime = Stopwatch.GetTimestamp();
                    long timeDiff = (receiveTime - lastReceiveTime) / TickResolMs;
                    lastReceiveTime = receiveTime;
                    Debug.WriteLine("Rec({0}): {1}", frameType, timeDiff);
#endif
                    if (recLen == 0)
                    {   // first telegram
                        targetAddr = canMsg.ID;
                        int testerId = GetIsoTpTesterId(canMsg.ID);
                        if (testerId < 0)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("No tester ID found!");
#endif
                            return false;
                        }
                        testerAddr = (uint) testerId;
#if CAN_DEBUG
                        // Debug.WriteLine("Tester ID {0:X03}", (object) testerAddr);
#endif
                        switch (frameType)
                        {
                            case 0: // single frame
                                len = canMsg.DATA[0] & 0x0F;
                                if (len > canMsg.LEN - 1)
                                {
                                    continue;
                                }
                                dataBuffer = new byte[len];
                                Array.Copy(canMsg.DATA, 1, dataBuffer, 0, len);
                                recLen = len;
                                _receiveStopWatch.Reset();
                                _receiveStopWatch.Start();
                                break;

                            case 1: // first frame
                                if (canMsg.LEN < 8)
                                {
                                    continue;
                                }
                                len = (((int)canMsg.DATA[0] & 0x0F) << 8) + canMsg.DATA[1];
                                dataBuffer = new byte[len];
                                Array.Copy(canMsg.DATA, 2, dataBuffer, 0, 6);
                                recLen = 6;
                                blockCount = 1;
                                {
                                    TPCANMsg sendMsg = new TPCANMsg
                                    {
                                        DATA = new byte[8],
                                        ID = testerAddr,
#if CAN_DYN_LEN
                                        LEN = 4,
#else
                                        LEN = 8,
#endif
                                        MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
                                    };
                                    sendMsg.DATA[0] = 0x30;  // FC
                                    sendMsg.DATA[1] = blocksize;    // Block size
                                    sendMsg.DATA[2] = sepTime;      // Min sep. Time
                                    fcCount = blocksize;
                                    stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                    {
                                        _receiveStopWatch.Stop();
                                        return false;
                                    }
                                }
                                _receiveStopWatch.Reset();
                                _receiveStopWatch.Start();
                                break;

                            default:
                                continue;
                        }
                    }
                    else
                    {
                        if (frameType == 1)
                        {
                            continue;
                        }
                        if (frameType != 2)
                        {   // consecutive frame
                            continue;
                        }
                        if (targetAddr != canMsg.ID)
                        {
                            continue;
                        }
                        if ((canMsg.DATA[0] & 0x0F) != (blockCount & 0x0F))
                        {
                            continue;
                        }
                        if (dataBuffer == null)
                        {
                            return false;
                        }
                        len = dataBuffer.Length - recLen;
                        if (len > 7)
                        {
                            len = 7;
                        }
                        if (len > canMsg.LEN - 1)
                        {
                            continue;
                        }
                        Array.Copy(canMsg.DATA, 1, dataBuffer, recLen, len);
                        recLen += len;
                        blockCount++;
                        _receiveStopWatch.Reset();
                        _receiveStopWatch.Start();

                        if (fcCount > 0 && recLen < dataBuffer.Length)
                        {
                            fcCount--;
                            if (fcCount == 0)
                            {
#if CAN_DEBUG
                                Debug.WriteLine("Send FC");
#endif
                                TPCANMsg sendMsg = new TPCANMsg
                                {
                                    DATA = new byte[8],
                                    ID = testerAddr,
#if CAN_DYN_LEN
                                    LEN = 4,
#else
                                    LEN = 8,
#endif
                                    MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
                                };

                                // sleep test
                                for (int i = 0; i < waitCount; i++)
                                {
                                    Thread.Sleep(500);
#if CAN_DEBUG
                                    Debug.WriteLine("Send FC wait");
#endif
                                    sendMsg.DATA[0] = 0x31;         // FC, wait
                                    sendMsg.DATA[1] = blocksize;    // Block size
                                    sendMsg.DATA[2] = sepTime;      // Min sep. Time
                                    stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                    {
                                        _receiveStopWatch.Stop();
                                        return false;
                                    }
                                }
                                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                                if (waitCount > 0)
                                {
                                    Thread.Sleep(500);
                                }
#pragma warning restore 162
                                // ReSharper restore HeuristicUnreachableCode
                                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                _receiveStopWatch.Reset();
                                _receiveStopWatch.Start();

                                sendMsg.DATA[0] = 0x30;         // FC
                                sendMsg.DATA[1] = blocksize;    // Block size
                                sendMsg.DATA[2] = sepTime;      // Min sep. Time
                                fcCount = blocksize;
                                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                {
                                    _receiveStopWatch.Stop();
                                    return false;
                                }
                            }
                        }
                    }
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (dataBuffer != null && recLen >= dataBuffer.Length)
                    {
                        break;
                    }
                }
                if (dataBuffer != null && recLen >= dataBuffer.Length)
                {
                    break;
                }
                if (_receiveStopWatch.ElapsedMilliseconds > 1000)
                {
#if CAN_DEBUG
                    Debug.WriteLine("Rec Timeout");
#endif
                    _receiveStopWatch.Stop();
                    return false;
                }
                _pcanReceiveEvent.WaitOne(10);
                if (recLen == 0)
                {   // nothing received
                    _receiveStopWatch.Stop();
                    return false;
                }
            }
            _receiveStopWatch.Stop();
#if CAN_DEBUG
            Debug.WriteLine("Rec OK");
#endif
            if (dataBuffer.Length == 0)
            {
#if CAN_DEBUG
                Debug.WriteLine("Empty telegram");
#endif
                return false;
            }
            // create BMW-FAST telegram
#if MAP_ISOTP_ECU
            _originalEcuId = targetAddr;
            _originalTesterId = testerAddr;
            if (targetAddr != 0x710)
            {   // no did
                targetAddr = 0x7E0; // mot
            }
#endif
            if (targetAddr == FunctOrgAddr)
            {
                targetAddr = FunctSubstAddr;
                Debug.WriteLine("Mapped functional CAN ID: {0:X03} {1:X03}", FunctOrgAddr, FunctSubstAddr);
            }

            if (dataBuffer.Length > 0x3F)
            {
                if (dataBuffer.Length > 0xFF)
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = (byte)(targetAddr >> 8);
                    receiveData[2] = (byte)targetAddr;
                    receiveData[3] = 0x00;
                    receiveData[4] = (byte)(dataBuffer.Length >> 8);
                    receiveData[5] = (byte)dataBuffer.Length;
                    Array.Copy(dataBuffer, 0, receiveData, 6, dataBuffer.Length);
                    len = dataBuffer.Length + 6;
                }
                else
                {
                    receiveData[0] = 0x80;
                    receiveData[1] = (byte)(targetAddr >> 8);
                    receiveData[2] = (byte)targetAddr;
                    receiveData[3] = (byte)dataBuffer.Length;
                    Array.Copy(dataBuffer, 0, receiveData, 4, dataBuffer.Length);
                    len = dataBuffer.Length + 4;
                }
            }
            else
            {
                receiveData[0] = (byte)(0x80 | dataBuffer.Length);
                receiveData[1] = (byte)(targetAddr >> 8);
                receiveData[2] = (byte)targetAddr;
                Array.Copy(dataBuffer, 0, receiveData, 3, dataBuffer.Length);
                len = dataBuffer.Length + 3;
            }
            receiveData[len] = CalcChecksumBmwFast(receiveData, len);
            return true;
        }

        private bool SendCanVw(byte[] sendData)
        {
            TPCANMsg canMsg;
            TPCANStatus stsResult;
            TPCANMsg sendMsg = new TPCANMsg
            {
                DATA = new byte[8]
            };
            byte blockSize = 0;
            byte sepTime = 0;

            uint sourceAddr = (uint) (sendData[1] << 8) + sendData[2];
            int testerId = GetIsoTpTesterId(sourceAddr);
#if MAP_ISOTP_ECU
            sourceAddr = _originalEcuId;
            testerId = (int)_originalTesterId;
#endif
            if (testerId < 0)
            {
#if CAN_DEBUG
                Debug.WriteLine("No tester ID found!");
#endif
                return false;
            }
            uint testerAddr = (uint)testerId;
#if CAN_DEBUG
            // Debug.WriteLine("Tester ID {0:X03}", (object) testerAddr);
#endif
            int dataOffset = 3;
            int dataLength = sendData[0] & 0x3F;
            if (dataLength == 0)
            {   // with length byte
                if (sendData[3] == 0)
                {   // two length bytes
                    dataLength = (sendData[4] << 8) + sendData[5];
                    dataOffset = 6;
                }
                else
                {
                    dataLength = sendData[3];
                    dataOffset = 4;
                }
            }

            if ((Stopwatch.GetTimestamp() - _lastCanSendTick) < 10 * TickResolMs)
            {
                Thread.Sleep(10);   // required for multiple telegrams
            }
            // clear input buffer
            while (PCANBasic.Read(_pcanHandle, out canMsg, out TPCANTimestamp _) == TPCANStatus.PCAN_ERROR_OK)
            {
            }

            if (dataLength <= 7)
            {   // single frame
                sendMsg.ID = testerAddr;
#if CAN_DYN_LEN
                sendMsg.LEN = (byte)(1 + dataLength);
#else
                sendMsg.LEN = 8;
#endif
                sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                sendMsg.DATA[0] = (byte)(0x00 | dataLength);  // SF
                Array.Copy(sendData, dataOffset, sendMsg.DATA, 1, dataLength);
                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    return false;
                }
                _lastCanSendTick = Stopwatch.GetTimestamp();
                return true;
            }
            // first frame
            sendMsg.ID = testerAddr;
            sendMsg.LEN = 8;
            sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            sendMsg.DATA[0] = (byte)(0x10 | ((dataLength >> 8) & 0xFF));  // FF
            sendMsg.DATA[1] = (byte)dataLength;
            int len = 6;
            Array.Copy(sendData, dataOffset, sendMsg.DATA, 2, len);
            dataLength -= len;
            dataOffset += len;
            stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
            {
                return false;
            }
            bool waitForFc = true;
            byte blockCount = 1;

            for (;;)
            {
                if (waitForFc)
                {
                    bool wait;
                    do
                    {
                        _receiveStopWatch.Reset();
                        _receiveStopWatch.Start();
                        for (;;)
                        {
                            stsResult = PCANBasic.Read(_pcanHandle, out canMsg, out TPCANTimestamp _);
                            if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                            {
                                if ((canMsg.LEN >= 3) && (canMsg.MSGTYPE == TPCANMessageType.PCAN_MESSAGE_STANDARD) &&
                                    (canMsg.ID == sourceAddr) &&
                                    ((canMsg.DATA[0] & 0xF0) == 0x30))
                                {
                                    break;
                                }
                            }
                            if (_receiveStopWatch.ElapsedMilliseconds > 1000)
                            {
                                _receiveStopWatch.Stop();
                                return false;
                            }
                        }
                        _receiveStopWatch.Stop();
                        switch (canMsg.DATA[0] & 0x0F)
                        {
                            case 0: // CTS
                                wait = false;
                                break;

                            case 1: // Wait
                                wait = true;
                                break;

                            default:
                                return false;
                        }
                        blockSize = canMsg.DATA[1];
                        sepTime = canMsg.DATA[2];
                    } while (wait);
#if CAN_DEBUG
                    Debug.WriteLine("FC: BS={0} ST={1}", blockSize, sepTime);
#endif
                }
                // consecutive frame
                len = dataLength;
                if (len > 7)
                {
                    len = 7;
                }
                sendMsg.ID = testerAddr;
#if CAN_DYN_LEN
                sendMsg.LEN = (byte)(1 + len);
#else
                sendMsg.LEN = 8;
#endif
                sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                sendMsg.DATA[0] = (byte)(0x20 | (blockCount & 0x0F));  // CF
                Array.Copy(sendData, dataOffset, sendMsg.DATA, 1, len);
                dataLength -= len;
                dataOffset += len;
                blockCount++;
                //Thread.Sleep(900);    // timeout test
                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                {
                    return false;
                }
                if (dataLength <= 0)
                {
                    break;
                }

                waitForFc = false;
                if (blockSize > 0)
                {
                    if (blockSize == 1)
                    {
                        waitForFc = true;
                    }
                    blockSize--;
                }
                if (!waitForFc && sepTime > 0)
                {
                    Thread.Sleep(sepTime);
                }
            }
            _lastCanSendTick = Stopwatch.GetTimestamp();
            return true;
        }

        private bool ReceiveCanTp20(byte[] receiveData, TPCANMsg? canMsgLast = null)
        {
            TPCANMsg sendMsg = new TPCANMsg
            {
                DATA = new byte[8]
            };

            for (;;)
            {
                TPCANStatus stsResult;
                // clean up old channels
                for (int i = 0; i < _tp20Channels.Count; i++)
                {
                    Tp20Channel channel = _tp20Channels[i];
                    bool removeChannel = false;
                    if (channel.SendData.Count == 0)
                    {
                        if ((Stopwatch.GetTimestamp() - channel.LastAccessTick) > 5000 * TickResolMs)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("Timeout channel {0:X04}", (object)channel.TxId);
#endif
                            removeChannel = true;
                        }
                    }
                    if (channel.WaitForKeepAliveResp)
                    {
                        if ((Stopwatch.GetTimestamp() - channel.LastKeepAliveTick) > 3000 * TickResolMs)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("Keep alive timeout channel {0:X04}", (object)channel.TxId);
#endif
                            removeChannel = true;
                        }
                    }
                    if (removeChannel)
                    {
                        _tp20Channels.Remove(channel);
                        // send disconnect
                        sendMsg.ID = (uint)(channel.RxId);
                        sendMsg.LEN = 1;
                        sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                        sendMsg.DATA[0] = 0xA8;
                        stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                        {
                            return false;
                        }
                    }
                }

                // check for send data
                foreach (Tp20Channel channel in _tp20Channels)
                {
#if !VCDS
                    // VCDS is not able to handle keep alives from ECU correctly
                    if (!channel.WaitForKeepAliveResp && ((Stopwatch.GetTimestamp() - channel.LastKeepAliveTick) > 1000 * TickResolMs))
                    {
#if CAN_DEBUG
                        Debug.WriteLine("Send keep alive channel {0:X04}", (object) channel.TxId);
#endif
                        sendMsg.ID = (uint)(channel.RxId);
                        sendMsg.LEN = 1;
                        sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                        sendMsg.DATA[0] = 0xA3;

                        stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                        {
                            return false;
                        }
                        channel.LastKeepAliveTick = Stopwatch.GetTimestamp();
                        channel.WaitForKeepAliveResp = true;
                    }
#endif
                    if (channel.WaitForAck)
                    {
                        if ((Stopwatch.GetTimestamp() - channel.AckWaitStartTick) > Tp20T1 * TickResolMs)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("ACK timeout channel {0:X04}", (object)channel.TxId);
#endif
                            channel.SendData.Clear();
                            channel.WaitForAck = false;
                        }
                        continue;
                    }
                    if (channel.SendData.Count > 0)
                    {
                        if (channel.SendDelay)
                        {
                            if ((Stopwatch.GetTimestamp() - channel.SendDelayStartTick) > 500*TickResolMs)
                            {
                                channel.SendDelay = false;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        byte[] sendData = channel.SendData[0];
                        sendMsg.ID = (uint)(channel.RxId);
                        sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                        int len = sendData.Length - channel.SendPos;
                        int offset;
                        if (channel.SendPos == 0)
                        {   // first part
                            sendMsg.DATA[1] = (byte)(sendData.Length >> 8);
                            sendMsg.DATA[2] = (byte)(sendData.Length);
                            if (channel.SendData.Count > 1)
                            {
                                sendMsg.DATA[1] |= 0x80;
#if CAN_DEBUG
                                Debug.WriteLine("More telegrams follow");
#endif
                            }
                            offset = 3;
                        }
                        else
                        {
                            offset = 1;
                        }
                        if (len > 8 - offset)
                        {
                            len = 8 - offset;
                        }
                        Array.Copy(sendData, channel.SendPos, sendMsg.DATA, offset, len);
                        sendMsg.LEN = (byte)(len + offset);
                        channel.SendPos += len;

                        byte op;
                        if (channel.SendPos >= sendData.Length)
                        {
                            // all send
                            op = 0x1;   // wait for ACK, last packet
                            // start with next telegram
                            channel.SendPos = 0;
                            channel.SendBlock = 0;
                            channel.SendDelay = false;
                            channel.SendDelayStartTick = Stopwatch.GetTimestamp();
#if true
                            if (sendData.Length == 3 && sendData[0] == 0x7F && sendData[2] == 0x78)
                            {
#if CAN_DEBUG
                                Debug.WriteLine("Delay NR78");
#endif
                                channel.SendDelay = true;
                            }
#endif
                            channel.SendData.RemoveAt(0);
                            channel.WaitForAck = true;
                            channel.AckWaitStartTick = Stopwatch.GetTimestamp();
                        }
                        else
                        {
                            channel.SendBlock++;
                            if (channel.BlockSize != 0x00 && channel.SendBlock >= channel.BlockSize)
                            {
                                op = 0x0;   // wait for ACK, block size reached
                                channel.SendBlock = 0;
                                channel.WaitForAck = true;
                                channel.AckWaitStartTick = Stopwatch.GetTimestamp();
                            }
                            else
                            {
                                op = 0x2;   // no wait for ACK, more packets follow
                                Thread.Sleep((int)channel.T3Time);
                            }
                        }

                        sendMsg.DATA[0] = (byte)((op << 4) | (channel.SendSeq & 0x0F));
                        channel.SendSeq = (byte)((channel.SendSeq + 1) & 0x0F);
#if CAN_DEBUG
                        Debug.WriteLine("Send channel {0:X04} Op={1:X01} Seq={2}", channel.TxId, op, channel.SendSeq);
#endif
                        stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                        {
                            return false;
                        }
                    }
                }

                TPCANMsg canMsg;
                if (canMsgLast.HasValue)
                {
                    canMsg = canMsgLast.Value;
                    canMsgLast = null;
                }
                else
                {
                    stsResult = PCANBasic.Read(_pcanHandle, out canMsg, out TPCANTimestamp _);
                    if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                    {
                        break;
                    }
                }
                if (canMsg.MSGTYPE != TPCANMessageType.PCAN_MESSAGE_STANDARD)
                {
                    break;
                }
#if CAN_DEBUG
                long receiveTime = Stopwatch.GetTimestamp();
                long timeDiff = (receiveTime - _lastCanReceiveTick) / TickResolMs;
                _lastCanReceiveTick = receiveTime;

                string dataString = string.Empty;
                for (int i = 0; i < canMsg.LEN; i++)
                {
                    dataString += string.Format("{0:X02} ", canMsg.DATA[i]);
                }
                Debug.WriteLine("CAN rec: T={0,-5} {1:X04} {2}", timeDiff, canMsg.ID, dataString);
#endif
                if (canMsg.ID == 0x0200 && canMsg.LEN == 7)
                {
                    // channel setup request
                    if (canMsg.DATA[1] == 0xC0)
                    {
                        byte[] configData = GetConfigData(canMsg.DATA[0]);
#if MAP_ISOTP_ECU
                        if (configData != null && configData.Length == 2 && canMsg.DATA[0] != 0x1F)
                        {
                            // don't ignore did
                            Debug.WriteLine("Ignore Tp20 ECU: {0:X02}", (object) canMsg.DATA[0]);
                            configData = null;
                        }
#endif
                        if ((configData == null) || (configData.Length != 2))
                        {
#if CAN_DEBUG
                            Debug.WriteLine("Invalid ECU address");
#endif
                            break;
                        }
                        for (int i = 0; i < _tp20Channels.Count; i++)
                        {
                            Tp20Channel channel = _tp20Channels[i];
                            if (channel.EcuAddress == canMsg.DATA[0])
                            {
                                _tp20Channels.Remove(channel);
#if CAN_DEBUG
                                Debug.WriteLine("Removed multiple ECU {0:X02} channel {1:X04}", channel.EcuAddress, channel.TxId);
#endif
                            }
                        }

                        Tp20Channel newChannel = new Tp20Channel
                        {
                            EcuAddress = canMsg.DATA[0],
                            TelAddress = configData[1],
                            AppId = canMsg.DATA[6],
                            RxId = (canMsg.DATA[5] << 8) | canMsg.DATA[4],
                            TxId = 0x600 + canMsg.DATA[0],   // no real id
                            LastKeepAliveTick = Stopwatch.GetTimestamp()
                        };
                        _tp20Channels.Add(newChannel);
#if CAN_DEBUG
                        Debug.WriteLine("Added channel {0:X04}:{1:X04}", newChannel.TxId, newChannel.RxId);
#endif

                        newChannel.LastAccessTick = Stopwatch.GetTimestamp();

                        sendMsg.ID = (uint) (0x200 + newChannel.EcuAddress);
                        sendMsg.LEN = 7;
                        sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;

                        sendMsg.DATA[0] = 0x00;
                        sendMsg.DATA[1] = 0xD0; // pos. response
                        sendMsg.DATA[2] = (byte) newChannel.RxId;
                        sendMsg.DATA[3] = (byte) (newChannel.RxId >> 8);
                        sendMsg.DATA[4] = (byte) newChannel.TxId;
                        sendMsg.DATA[5] = (byte) (newChannel.TxId >> 8);
                        sendMsg.DATA[6] = newChannel.AppId;

                        stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                        if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                        {
                            return false;
                        }
                        newChannel.LastAccessTick = Stopwatch.GetTimestamp();
                    }
                    break;
                }
                Tp20Channel currChannel = _tp20Channels.FirstOrDefault(channel => channel.TxId == canMsg.ID);
                if (currChannel == null)
                {
                    break;
                }
                currChannel.LastAccessTick = Stopwatch.GetTimestamp();
                if (canMsg.LEN < 1)
                {
                    break;
                }
                byte sequence = (byte)(canMsg.DATA[0] & 0x0F);
                byte opcode = (byte)(canMsg.DATA[0] >> 4);
                switch (opcode)
                {
                    case 0x00:  // wait for ACK, block size reached
                    case 0x01:  // wait for ACK, last packet
                    case 0x02:  // no wait for ACK, more packets follow
                    case 0x03:  // no wait for ACK, last packet
                        if (currChannel.RecSeq != sequence)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("Invalid rec sequence");
#endif
                            break;
                        }
                        currChannel.RecSeq = (byte)((sequence + 1) & 0x0F);
                        if (currChannel.RecData == null)
                        {
                            // start of telegram
                            if (canMsg.LEN < 3)
                            {
                                break;
                            }
                            currChannel.RecLen = (canMsg.DATA[1] << 8) | canMsg.DATA[2];
                            currChannel.RecData = new List<byte>();
                            for (int i = 0; i < canMsg.LEN - 3; i++)
                            {
                                currChannel.RecData.Add(canMsg.DATA[i + 3]);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < canMsg.LEN - 1; i++)
                            {
                                currChannel.RecData.Add(canMsg.DATA[i + 1]);
                            }
                        }
                        if ((opcode == 0x01) || (opcode == 0x03))
                        {   // last packet, length too short
                            if (currChannel.RecData.Count < currChannel.RecLen)
                            {
                                currChannel.RecData = null;
                            }
                        }
                        if ((opcode == 0x00) || (opcode == 0x01))
                        {   // wait for ack
#if CAN_DEBUG
                            Debug.WriteLine("Send ACK");
#endif
                            sendMsg.ID = (uint)(currChannel.RxId);
                            sendMsg.LEN = 1;
                            sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                            sendMsg.DATA[0] = (byte)(0xB0 | (currChannel.RecSeq & 0x0F));

                            stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                            if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                            {
                                return false;
                            }
                        }
                        break;

                    case 0x09:  // ACK, not ready for next packet
#if CAN_DEBUG
                        Debug.WriteLine("Rec ACK not ready");
#endif
                        if (currChannel.SendSeq != sequence)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("Invalid send sequence");
#endif
                            break;
                        }
                        currChannel.AckWaitStartTick = Stopwatch.GetTimestamp();
                        break;

                    case 0x0B:  // ACK, ready for next packet
#if CAN_DEBUG
                        Debug.WriteLine("Rec ACK");
#endif
                        if (currChannel.SendSeq != sequence)
                        {
#if CAN_DEBUG
                            Debug.WriteLine("Invalid send sequence");
#endif
                            break;
                        }
                        currChannel.WaitForAck = false;
                        break;

                    case 0x0A:  // parameter
                        switch (canMsg.DATA[0])
                        {
                            case 0xA0: // parameter request
                            case 0xA3: // channel test
                                if (canMsg.DATA[0] == 0xA0 && canMsg.LEN == 6)
                                {
#if VCDS
                                    currChannel.BlockSize = 1;
#else
                                    currChannel.BlockSize = canMsg.DATA[1];
#endif
                                    currChannel.T1 = canMsg.DATA[2];
                                    currChannel.T3 = canMsg.DATA[4];
#if CAN_DEBUG
                                    Debug.WriteLine("Parameter Block:{0:X02} T1:{1} T3:{2}", currChannel.BlockSize, currChannel.T1Time, currChannel.T3Time);
#endif
                                }
                                if (canMsg.DATA[0] == 0xA3)
                                {
#if CAN_DEBUG
                                    Debug.WriteLine("Keep alive");
#endif
                                }
                                sendMsg.ID = (uint)(currChannel.RxId);
                                sendMsg.LEN = 6;
                                sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                                sendMsg.DATA[0] = 0xA1; // parameter response
                                sendMsg.DATA[1] = 0x0F; // block size
                                sendMsg.DATA[2] = 0x80 | (Tp20T1 / 10); // T1 100ms
                                sendMsg.DATA[3] = 0xFF;
                                sendMsg.DATA[4] = 0x40 | Tp20T3;        // T3 10ms
                                sendMsg.DATA[5] = 0xFF;
                                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                {
                                    return false;
                                }
                                break;

                            case 0xA1: // parameter response (from channel test)
                                if (canMsg.LEN != 6)
                                {
#if CAN_DEBUG
                                    Debug.WriteLine("Parameter response length invalid");
#endif
                                    break;
                                }
                                currChannel.WaitForKeepAliveResp = false;
                                break;

                            case 0xA4: // break;
                                currChannel.RecData = null;
                                break;

                            case 0xA8: // disconnect
#if CAN_DEBUG
                                Debug.WriteLine("Disconnect channel {0:X04}", currChannel.TxId);
#endif
                                _tp20Channels.Remove(currChannel);
                                sendMsg.ID = (uint)(currChannel.RxId);
                                sendMsg.LEN = 1;
                                sendMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                                sendMsg.DATA[0] = 0xA8;
                                stsResult = PCANBasic.Write(_pcanHandle, ref sendMsg);
                                if (stsResult != TPCANStatus.PCAN_ERROR_OK)
                                {
                                    return false;
                                }
                                break;
                        }
                        break;
                }
                if (currChannel.RecData != null && currChannel.RecData.Count >= currChannel.RecLen)
                {
#if CAN_DEBUG
                    Debug.WriteLine("Rec OK");
#endif
                    // create BMW-FAST telegram
                    byte[] dataBuffer = currChannel.RecData.ToArray();
                    int len;
                    if (currChannel.RecLen > 0x3F)
                    {
                        receiveData[0] = 0x80;
                        receiveData[1] = currChannel.TelAddress;
                        receiveData[2] = 0xF1;
                        receiveData[3] = (byte)currChannel.RecLen;
                        Array.Copy(dataBuffer, 0, receiveData, 4, currChannel.RecLen);
                        len = dataBuffer.Length + 4;
                    }
                    else
                    {
                        receiveData[0] = (byte)(0x80 | dataBuffer.Length);
                        receiveData[1] = currChannel.TelAddress;
                        receiveData[2] = 0xF1;
                        Array.Copy(dataBuffer, 0, receiveData, 3, dataBuffer.Length);
                        len = dataBuffer.Length + 3;
                    }
                    receiveData[len] = CalcChecksumBmwFast(receiveData, len);
                    currChannel.RecData = null;
                    return true;
                }
            }
            return false;
        }

        private bool SendCanTp20(byte[] sendData)
        {
            byte sourceAddr = sendData[2];

            Tp20Channel currChannel = _tp20Channels.FirstOrDefault(channel => channel.TelAddress == sourceAddr);
            if (currChannel == null)
            {
#if CAN_DEBUG
                Debug.WriteLine("Send channel not found");
#endif
                return false;
            }

            int dataOffset = 3;
            int dataLength = sendData[0] & 0x3F;
            if (dataLength == 0)
            {   // with length byte
                dataLength = sendData[3];
                dataOffset = 4;
            }
#if false
            DebugLogData("Add send: ", sendData, sendData.Length);
#endif
            if (currChannel.SendData.Count == 0)
            {
                currChannel.SendPos = 0;
                currChannel.SendBlock = 0;
            }
            byte[] sendArray = new byte[dataLength];
            Array.Copy(sendData, dataOffset, sendArray, 0, dataLength);
            currChannel.SendData.Add(sendArray);
            return true;
        }

        private bool SendBmwfast(byte[] sendData)
        {
            int sendLength = TelLengthBmwFast(sendData);
            sendData[sendLength] = CalcChecksumBmwFast(sendData, sendLength);
            sendLength++;
            if (!SendData(sendData, sendLength))
            {
                return false;
            }
            return true;
        }

        private bool ReceiveBmwFast(byte[] receiveData)
        {
            // header byte
            if (!ReceiveData(receiveData, 0, 4, true))
            {
                return false;
            }
            if ((receiveData[0] & 0x80) != 0x80)
            {   // 0xC0: Broadcast
                Thread.Sleep(1000);
                _serialPort.DiscardInBuffer();
                return false;
            }

            int recLength = TelLengthBmwFast(receiveData);
            if (!ReceiveData(receiveData, 4, recLength - 3))
            {
                _serialPort.DiscardInBuffer();
                return false;
            }
            if (CalcChecksumBmwFast(receiveData, recLength) != receiveData[recLength])
            {
                _serialPort.DiscardInBuffer();
                return false;
            }
            return true;
        }

        public static bool IsFunctionalAddress(byte address)
        {
            if (address == 0xDF)
            {
                return true;
            }

            if (address >= 0xE6 && address <= 0xEF)
            {
                return true;
            }

            return false;
        }

        public static int TelLengthBmwFast(byte[] dataBuffer)
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

        public static byte CalcChecksumBmwFast(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i];
            }
            return sum;
        }

        private bool SendKwp2000S(byte[] sendData)
        {
            int sendLength = sendData[3] + 4;
            sendData[sendLength] = CalcChecksumXor(sendData, sendLength);
            sendLength++;
#if false
            DebugLogData("Send KWP2000*: ", sendData, sendLength);
#endif
            if (!SendData(sendData, sendLength))
            {
                return false;
            }
            return true;
        }

        private bool ReceiveKwp2000S(byte[] receiveData)
        {
            // header byte
            if (!ReceiveData(receiveData, 0, 4, true))
            {
                return false;
            }
            int recLength = receiveData[3] + 4;
            if (!ReceiveData(receiveData, 4, recLength - 3))
            {
                _serialPort.DiscardInBuffer();
#if false
                DebugLogData("No data: ", _receiveData, 4);
#endif
                return false;
            }
            if (CalcChecksumXor(receiveData, recLength) != receiveData[recLength])
            {
                _serialPort.DiscardInBuffer();
#if false
                DebugLogData("Checksum: ", _receiveData, recLength + 1);
#endif
                return false;
            }
            return true;
        }

        private bool SendDs2(byte[] sendData)
        {
            int sendLength = sendData[1] - 1;
            sendData[sendLength] = CalcChecksumXor(sendData, sendLength);
            sendLength++;
#if false
            DebugLogData("Send DS2: ", sendData, sendLength);
#endif
            if (!SendData(sendData, sendLength))
            {
                return false;
            }
            return true;
        }

        private bool ReceiveDs2(byte[] receiveData, out bool kwp2000S)
        {
            kwp2000S = false;
            // header byte
            if (!ReceiveData(receiveData, 0, 4, true))
            {
                return false;
            }
            int recLength;
            if (receiveData[0] == 0xB8 && receiveData[2] == 0xF1)
            {   // KPW2000*
                recLength = receiveData[3] + 4;
                kwp2000S = true;
            }
            else
            {
                recLength = receiveData[1] - 1;
            }
            if (recLength < 3)
            {
                _serialPort.DiscardInBuffer();
                return false;
            }
            if (!ReceiveData(receiveData, 4, recLength - 3))
            {
                _serialPort.DiscardInBuffer();
                return false;
            }
            if (CalcChecksumXor(receiveData, recLength) != receiveData[recLength])
            {
                _serialPort.DiscardInBuffer();
                return false;
            }
            return true;
        }

        public static byte CalcChecksumXor(byte[] data, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum ^= data[i];
            }
            return sum;
        }

        private byte[] GetConfigData(byte wakeAddress)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (byte[] configData in _configData.ConfigList)
            {
                if (configData.Length > 0 && configData[0] == wakeAddress)
                {
                    return configData;
                }
            }
            return null;
        }

        private byte[] GetIsoTpConfigData(uint canId)
        {
            uint checkId = canId;
            if (canId == FunctOrgAddr)
            {
                checkId = FunctSubstAddr;
                Debug.WriteLine("Mapped functional ISOTP CAN ID: {0:X03} {1:X03}", canId, checkId);
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (byte[] configData in _configData.ConfigList)
            {
                if (configData.Length == 5 && ((configData[1] << 8) | configData[2]) == checkId)
                {
                    return configData;
                }
            }
#if MAP_ISOTP_ECU
            // map to mot ecu
            foreach (UdsFileReader.VehicleInfoVag.EcuAddressEntry ecuAddressEntry in UdsFileReader.VehicleInfoVag.EcuAddressArray)
            {
                if (ecuAddressEntry.IsoTpEcuCanId == canId)
                {
                    Debug.WriteLine("Mapped ISOTP CAN ID: {0:X03} {1:X03}", canId, ecuAddressEntry.IsoTpTesterCanId);
                    return new byte[] { 0x01, (byte)(canId >> 8), (byte)canId, (byte)(ecuAddressEntry.IsoTpTesterCanId >> 8), (byte)(ecuAddressEntry.IsoTpTesterCanId) };
                }
            }
#endif
            return null;
        }

        private int GetIsoTpTesterId(uint canId)
        {
            byte[] configData = GetIsoTpConfigData(canId);
            if (configData == null)
            {
                return -1;
            }
            return ((configData[3] << 8) | configData[4]);
        }

        private bool SendKwp1281Block(byte[] sendData)
        {
            int retries = 0;
            int blockLen = sendData[0];
            byte[] buffer = new byte[1];
            for (;;)
            {
                bool restart = false;
                for (int i = 0; i < blockLen; i++)
                {
                    if (_stopThread)
                    {
                        return false;
                    }
                    Debug.WriteLine("Send {0:X02}", sendData[i]);
                    if (!SendData(sendData, i, 1))
                    {
                        return false;
                    }
                    if (!ReceiveData(buffer, 0, 1, IsoTimeout, IsoTimeout))
                    {
                        return false;
                    }
#if false
                    _kwp1281InvRespIndex++;
                    if (_kwp1281InvRespIndex > 50)
                    {
                        Debug.WriteLine("Simulate invalid response");
                        buffer[0]++;
                        _kwp1281InvRespIndex = 0;
                        Thread.Sleep(150);  // min 70
                    }
#endif
                    if ((byte)(~buffer[0]) != sendData[i])
                    {
                        retries++;
                        Debug.WriteLine("Echo incorrect {0:X02}, Retry {1}", (byte)(~buffer[0]), retries);
                        if (retries > 3)
                        {
                            return false;
                        }
                        restart = true;
                        break;
                    }
                }
                if (restart)
                {
                    continue;
                }
                buffer[0] = 0x03;   // block end
#if false
                _kwp1281InvBlockEndIndex++;
                if (_kwp1281InvBlockEndIndex > 3)
                {
                    Debug.WriteLine("Simulate invalid block end");
                    buffer[0]++;
                    _kwp1281InvBlockEndIndex = 0;
                }
#endif
                Debug.WriteLine("Send {0:X02}", (object)buffer[0]);
                if (!SendData(buffer, 0, 1))
                {
                    return false;
                }
                return true;
            }
        }

        private bool ReceiveKwp1281Block(byte[] recData)
        {
            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                // block length
                if (!ReceiveData(recData, 0, 1, IsoTimeout, IsoTimeout))
                {
                    Debug.WriteLine("Nothing received");
                    return false;
                }
                long timeDiff = (Stopwatch.GetTimestamp() - startTime) / TickResolMs;
                Debug.WriteLine("Rec {0:X02} t={1}", recData[0], timeDiff);

                bool restart = false;
                int blockLen = recData[0];
                byte[] buffer = new byte[1];
                for (int i = 0; i < blockLen; i++)
                {
                    if (_stopThread)
                    {
                        return false;
                    }
                    buffer[0] = (byte)~recData[i];
#if false
                    _kwp1281InvEchoIndex++;
                    if (_kwp1281InvEchoIndex > 40)
                    {
                        Debug.WriteLine("Inject invalid echo");
                        buffer[0]++;
                        _kwp1281InvEchoIndex = 0;
                    }
#endif
                    if (!SendData(buffer, 0, 1))
                    {
                        return false;
                    }
                    startTime = Stopwatch.GetTimestamp();
                    if (!ReceiveData(recData, i + 1, 1, IsoAckTimeout, IsoAckTimeout))
                    {
                        Debug.WriteLine("ACK timeout");
                        restart = true;
                        break;
                    }
                    timeDiff = (Stopwatch.GetTimestamp() - startTime) / TickResolMs;
                    Debug.WriteLine("Rec {0:X02} t={1}", recData[i + 1], timeDiff);
                }
                if (restart)
                {
                    continue;
                }
                if (recData[blockLen] != 0x03)
                {
                    Debug.WriteLine("Block end invalid {0:X02}", (object)recData[blockLen]);
                    return false;
                }
                return true;
            }
        }

        private byte IntToBcd(int value)
        {
            byte result = (byte) ((value % 10) + ((value / 10) << 4));
            return result;
        }

        private int GetClientConnections()
        {
            int enetConnections = 0;
            int controlConnections = 0;
            int doIpConnections = 0;
            int sslConnections = 0;

            if (_bmwTcpChannels.Count > 0)
            {
                foreach (BmwTcpChannel bmwTcpChannel in _bmwTcpChannels)
                {
                    if (bmwTcpChannel.TcpServerDiag != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDiagList)
                        {
                            if (bmwTcpClientData.TcpClientStream != null)
                            {
                                enetConnections++;
                            }
                        }
                    }

                    if (bmwTcpChannel.TcpServerDiag != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientControlList)
                        {
                            if (bmwTcpClientData.TcpClientStream != null)
                            {
                                controlConnections++;
                            }
                        }
                    }

                    if (bmwTcpChannel.TcpServerDoIp != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDoIpList)
                        {
                            if (bmwTcpClientData.TcpClientStream != null)
                            {
                                doIpConnections++;
                            }
                        }
                    }

                    if (bmwTcpChannel.TcpServerDoIpSsl != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDoIpSslList)
                        {
                            if (bmwTcpClientData.TcpClientStream != null)
                            {
                                sslConnections++;
                            }
                        }
                    }
                }
            }

            Debug.WriteLine("Client connections ENET={0}, Control={1}, DoIp={2}, SSL={3}",
                (object)enetConnections, (object)controlConnections, (object)doIpConnections, (object)sslConnections);
            return enetConnections + doIpConnections + sslConnections;
        }

        private void SerialTransmission()
        {
            if (_bmwTcpChannels.Count > 0)
            {
                bool transmitted = false;
                foreach (BmwTcpChannel bmwTcpChannel in _bmwTcpChannels)
                {
                    if (bmwTcpChannel.TcpServerDiag != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDiagList)
                        {
                            if (bmwTcpClientData.TcpClientConnection != null ||
                                bmwTcpChannel.TcpServerDiag.Pending())
                            {
                                SerialTransmission(bmwTcpClientData);
                                transmitted = true;
                            }
                        }
                    }

                    if (bmwTcpChannel.TcpServerDoIp != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDoIpList)
                        {
                            if (bmwTcpClientData.TcpClientConnection != null ||
                                (bmwTcpChannel.TcpServerDoIp != null && bmwTcpChannel.TcpServerDoIp.Pending()))
                            {
                                SerialTransmission(bmwTcpClientData);
                                transmitted = true;
                            }
                        }
                    }

                    if (bmwTcpChannel.TcpServerDoIpSsl != null)
                    {
                        foreach (BmwTcpClientData bmwTcpClientData in bmwTcpChannel.TcpClientDoIpSslList)
                        {
                            if (bmwTcpClientData.TcpClientConnection != null ||
                                (bmwTcpChannel.TcpServerDoIpSsl != null && bmwTcpChannel.TcpServerDoIpSsl.Pending()))
                            {
                                SerialTransmission(bmwTcpClientData);
                                transmitted = true;
                            }
                        }
                    }
                }

                if (!transmitted)
                {
                    Thread.Sleep(10);
                }
            }
            else
            {
                SerialTransmission(null);
            }
        }

        private void SerialTransmission(BmwTcpClientData bmwTcpClientData)
        {
            bool manualMode = false;
            for (int i = 0; i < _timeValveWrite.Length; i++)
            {
                if (_timeValveWrite[i].IsRunning)
                {
                    manualMode = true;
                    if (_timeValveWrite[i].ElapsedMilliseconds > 1000)
                    {
                        _outputs &= ~(1 << i);
                        _timeValveWrite[i].Stop();
                    }
                }
            }

            if (_outputs == 0x00)
            {
                _outputsActiveTime.Stop();
            }
            else
            {
                if (!_outputsActiveTime.IsRunning)
                {
                    _outputsActiveTime.Reset();
                    _outputsActiveTime.Start();
                }
            }

            if (_timeIdleSpeedControlWrite.IsRunning)
            {
                if (_timeIdleSpeedControlWrite.ElapsedMilliseconds > 500)
                {
                    _timeIdleSpeedControlWrite.Stop();
                }
            }

            _axisPosPrescaler++;
            if (_axisPosPrescaler > 5)
            {
                int outputsDown = 0x07;
                int outputsUp = 0x0B;
                if (_responseType == ResponseType.G31)
                {
                    outputsDown = 0x103;
                    outputsUp = 0x43;
                }

                _axisPosPrescaler = 0;
                if (!manualMode && _mode == 0x00)
                {
                    if (_axisPosRaw > 0) _axisPosRaw--;
                    if (_axisPosRaw < 0) _axisPosRaw++;
                }
                if (_outputs == outputsDown)
                {
                    if (_axisPosRaw > -80) _axisPosRaw--;
                }
                if (_outputs == outputsUp)
                {
                    if (_axisPosRaw < 80) _axisPosRaw++;
                }
                _axisPosFilt = (_axisPosFilt * FilterConst) + ((double)_axisPosRaw * (1 - FilterConst));
            }

            if (VariableValues)
            {
                if (_batteryVoltage > 800)
                {
                    _batteryVoltage--;
                }
                else
                {
                    _batteryVoltage = 1600;
                }
            }
            else
            {
                _batteryVoltage = 1445;
            }

            if (Moving && _speed < 250)
            {
                _speed++;
            }
            else
            {
                _speed = 0;
            }

            if (_compressorRunningTime < 4000)
            {
                _compressorRunningTime++;
            }
            else
            {
                _compressorRunningTime = 0;
            }

            if (!ObdReceive(_receiveData, bmwTcpClientData))
            {
                return;
            }

            int recLength = TelLengthBmwFast(_receiveData);
            recLength += 1; // checksum
#if true
            if (bmwTcpClientData != null)
            {
                Debug.WriteLine("Time[{0}]: {1}", bmwTcpClientData.Index, DateTime.Now.ToString("hh:mm:ss.fff"));
            }
            else
            {
                Debug.WriteLine(string.Format("Time: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
            }
            DebugLogData("Request: ", _receiveData, recLength);
#endif
            if (!_adsAdapter && !_klineResponder && (bmwTcpClientData == null) && (_pcanHandle == PCANBasic.PCAN_NONEBUS))
            {
                // send echo
                ObdSend(_receiveData, bmwTcpClientData);
            }
            if (_noResponseCount > 0)
            {   // no response requested
                _noResponseCount--;
                return;
            }

            byte errorResetData = (byte) (_conceptType == ConceptType.ConceptDs2 ? 0x00 : 0xFF);
            bool standardResponse = false;
            if (
                _receiveData[0] == 0x81 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x81)
            {   // start communication service
                int i = 0;
                _sendData[i++] = 0x83;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0xC1;
                _sendData[i++] = 0xDF;  // key low
                _sendData[i++] = 0x8F;  // key high

                ObdSend(_sendData, bmwTcpClientData);
                Debug.WriteLine("Start communication");
                standardResponse = true;
            }
            else if (
                _receiveData[0] == 0x81 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x3E)
            {   // tester present
                int i = 0;
                _sendData[i++] = 0x81;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x7E;

                ObdSend(_sendData, bmwTcpClientData);
                Debug.WriteLine("Tester present short");
                standardResponse = true;
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x3E)
            {
                if ((_receiveData[4] & 0x80) == 0x00)
                {   // with response
                    int i = 0;
                    _sendData[i++] = _receiveData[0];
                    _sendData[i++] = _receiveData[1];
                    _sendData[i++] = _receiveData[2];
                    _sendData[i++] = (byte)(_receiveData[3] | 0x40);
                    _sendData[i++] = _receiveData[2];

                    ObdSend(_sendData, bmwTcpClientData);
                }
                Debug.WriteLine("Tester present");
                standardResponse = true;
            }
            else if (
                _receiveData[0] == 0x81 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x20)
            {   // stop diag
                int i = 0;
                _sendData[i++] = 0x83;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x60;

                ObdSend(_sendData, bmwTcpClientData);
                Debug.WriteLine("Stop diag");
                standardResponse = true;
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x11)
            {   // ECU reset
                int i = 0;
                _sendData[i++] = 0x82;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x51;
                _sendData[i++] = _receiveData[4];

                ObdSend(_sendData, bmwTcpClientData);

                Debug.WriteLine("ECU reset");
                if (_responseType == ResponseType.G31)
                {
                    if (_receiveData[1] == 0x76)
                    {
                        Debug.WriteLine("Clearing outputs");
                        _outputs = 0;
                        _outputsActiveTime.Stop();
                    }
                }

                _noResponseCount = 1;
                standardResponse = true;
            }
            else if (
                (_receiveData[0] == 0x83 || _receiveData[0] == 0x84) &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x14 &&
                _receiveData[4] == errorResetData &&
                _receiveData[5] == errorResetData)
            {   // error reset
                _sendData[0] = 0x83;
                _sendData[1] = 0xF1;
                _sendData[2] = _receiveData[1];
                _sendData[3] = 0x54;
                _sendData[4] = 0xFF;
                _sendData[5] = 0xFF;

                Debug.WriteLine("Error reset");
                if (!_ecuErrorResetList.Contains(_receiveData[1]))
                {
                    _ecuErrorResetList.Add(_receiveData[1]);
                }
                ObdSend(_sendData, bmwTcpClientData);
                standardResponse = true;
            }
            else if (
                _receiveData[0] == 0xC3 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x14 &&
                _receiveData[4] == 0xFF &&
                _receiveData[5] == 0xFF)
            {   // global error reset
                Debug.WriteLine("Global error reset");
                HashSet<byte> addrHash = new HashSet<byte>();
                foreach (ResponseEntry responseEntry in _configData.ResponseList)
                {
                    foreach (byte[] responseTel in responseEntry.ResponseList)
                    {
                        if (responseTel.Length >= 3 && (responseTel[0] & 0xC0) == 0x80)
                        {
                            addrHash.Add(responseTel[2]);
                        }
                    }
                    foreach (byte[] responseTel in responseEntry.ResponseMultiList)
                    {
                        if (responseTel.Length >= 3 && (responseTel[0] & 0xC0) == 0x80)
                        {
                            addrHash.Add(responseTel[2]);
                        }
                    }
                }

                DebugLogData("Reset addr: ", addrHash.ToArray(), addrHash.Count);
                foreach (byte addr in addrHash)
                {
                    _sendData[0] = 0x83;
                    _sendData[1] = 0xF1;
                    _sendData[2] = addr;
                    _sendData[3] = 0x54;
                    _sendData[4] = 0xFF;
                    _sendData[5] = 0xFF;

                    if (!_ecuErrorResetList.Contains(addr))
                    {
                        _ecuErrorResetList.Add(addr);
                    }
                    ObdSend(_sendData, bmwTcpClientData);
                }

                standardResponse = true;
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {   // error request
                if (_ecuErrorResetList.Contains(_receiveData[1]))
                {   // disable error response -> send dummy
                    _sendData[0] = 0x82;
                    _sendData[1] = 0xF1;
                    _sendData[2] = _receiveData[1];
                    _sendData[3] = 0x58;
                    _sendData[4] = 0x00;

                    ObdSend(_sendData, bmwTcpClientData);
                    standardResponse = true;
                }
            }
            else if (bmwTcpClientData != null &&
                _receiveData[0] == 0xC3 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x17 &&
                _receiveData[5] == 0x2A)
            {   // get IP configuration
                Debug.WriteLine("Get IP configuration");

                IPAddress ipIcom = IPAddress.Parse(IcomVehicleAddress);
                IPAddress ipIcomLocal = GetLocalIpAddress(ipIcom, false, out _, out byte[] networkMask);
                byte[] ipLocalBytes = ipIcomLocal?.GetAddressBytes();

                if (ipLocalBytes != null && ipLocalBytes.Length == 4 && networkMask != null && networkMask.Length == 4)
                {
                    byte[] ecuList = { 0x10
                        , 0x21, 0x5D, 0x60, 0x61, 0x63
                        };

                    int index = 0;
                    foreach (byte ecuAddr in ecuList)
                    {
                        int i = 0;
                        _sendData[i++] = 0x90;
                        _sendData[i++] = 0xF1;
                        _sendData[i++] = ecuAddr;
                        _sendData[i++] = 0x62;
                        _sendData[i++] = 0x17;
                        _sendData[i++] = 0x2A;
                        _sendData[i++] = 0x00;

                        // IP
                        _sendData[i++] = ipLocalBytes[0];
                        _sendData[i++] = ipLocalBytes[1];
                        _sendData[i++] = ipLocalBytes[2];
                        _sendData[i++] = (byte)(ipLocalBytes[3] /*+ index*/);

                        // mask
                        _sendData[i++] = networkMask[0];
                        _sendData[i++] = networkMask[1];
                        _sendData[i++] = networkMask[2];
                        _sendData[i++] = networkMask[3];

                        // gateway
                        _sendData[i++] = 0;
                        _sendData[i++] = 0;
                        _sendData[i++] = 0;
                        _sendData[i++] = 0;

                        ObdSend(_sendData, bmwTcpClientData);
                        index++;
                    }
                    standardResponse = true;
                }
            }
            else if (!_klineResponder &&
                _receiveData[0] == 0x81 &&
                _receiveData[1] == 0x00 &&
                _receiveData[2] == 0x00)
            {   // program CAN adapter
                int i = 0;
                _sendData[i++] = 0x81;
                _sendData[i++] = 0x00;
                _sendData[i++] = 0x00;
                _sendData[i++] = (byte)(~_receiveData[3]);

                ObdSend(_sendData, bmwTcpClientData);
                Debug.WriteLine("Program CAN adapter");
                standardResponse = true;
            }

            bool useResponseList = false;
            if (standardResponse)
            {
                useResponseList = false;
            }
            else
            {
                switch (_responseType)
                {
                   case ResponseType.E61:
                        if (!ResponseE61(bmwTcpClientData))
                        {
                            useResponseList = true;
                        }
                        break;

                    case ResponseType.E90:
                        if (!ResponseE90(bmwTcpClientData))
                        {
                            useResponseList = true;
                        }
                        break;

                   case ResponseType.G31:
                       if (!ResponseG31(bmwTcpClientData))
                       {
                           useResponseList = true;
                       }
                       break;

                   case ResponseType.SMG2:
                       if (!ResponseSmg2(bmwTcpClientData))
                       {
                           useResponseList = true;
                       }
                       break;

                    default:
                        if (!HandleDynamicUdsIds(bmwTcpClientData))
                        {
                            useResponseList = true;
                        }
                        break;
                }
            }

            if (useResponseList)
            {
                bool found = false;
                foreach (ResponseEntry responseEntry in _configData.ResponseList)
                {
                    if (recLength != responseEntry.Request.Length) continue;
                    bool equal = true;
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                    for (int i = 0; i < recLength - 1; i++)
                    {   // don't compare checksum
                        if (_receiveData[i] != responseEntry.Request[i])
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                    {       // entry found
                        found = true;
#if false
                        SendData(responseEntry.Response, responseEntry.Response.Length);
#else
                        if (responseEntry.ResponseMultiList.Count > 1)
                        {
                            foreach (byte[] responseTel in responseEntry.ResponseMultiList)
                            {
                                ObdSend(responseTel, bmwTcpClientData);
                            }
                        }
                        else
                        {
                            ObdSend(responseEntry.ResponseDyn, bmwTcpClientData);
                        }
#endif
                        break;
                    }
                }

                if (!found)
                {
                    switch (_responseType)
                    {
                        case ResponseType.G31:
                            if (ResponseG31Generic(bmwTcpClientData))
                            {
                                Debug.WriteLine("Generic G31 response");
                                found = true;
                            }
                            break;
                    }
                }

                if (!found)
                {
                    if (
                        _receiveData[0] == 0x82 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x10)
                    {
                        // session control
                        if (_receiveData[4] == 0x01)
                        {
                            Debug.WriteLine("Dummy service 10 default session");
                            _sendData[0] = 0x82;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x50;
                            _sendData[4] = _receiveData[4];
                        }
                        else
                        {
                            Debug.WriteLine("Dummy service 10 other session");
                            _sendData[0] = 0x86;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x50;
                            _sendData[4] = _receiveData[4];
                            _sendData[5] = 0x01;
                            _sendData[6] = 0x2B;
                            _sendData[7] = 0x01;
                            _sendData[8] = 0xF4;
                        }
                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        _receiveData[0] == 0x84 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x18 &&
                        _receiveData[4] == 0x02 &&
                        _receiveData[5] == 0xFF &&
                        _receiveData[6] == 0xFF)
                    {
                        // dummy error response for all devices
                        Debug.WriteLine("Dummy service 18");
                        _sendData[0] = 0x82;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x58;
                        _sendData[4] = 0x00;

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        _receiveData[0] == 0x83 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x19 &&
                        _receiveData[4] == 0x02)
                    {
                        // dummy error response for all devices
                        Debug.WriteLine("Dummy service 19 02");
                        _sendData[0] = 0x83;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x59;
                        _sendData[4] = 0x02;
                        _sendData[5] = 0xFF;

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        _receiveData[0] == 0x86 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x19 &&
                        _receiveData[4] == 0x06)
                    {
                        // dummy error response for all devices
                        Debug.WriteLine("Dummy service 19 06");
                        _sendData[0] = 0x83;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x59;
                        _sendData[4] = 0x06;
                        _sendData[5] = 0xFF;

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x22)
                    {
                        // dummy error response for service 22
                        Debug.WriteLine("Dummy service 22: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);
                        bool responseFound = false;
                        if (_receiveData[4] == 0x17 && _receiveData[5] == 0x1F)
                        {
                            Debug.WriteLine("RDBI_CERT ZGW Zertifikat");
                            int telLength = _zgwCert.Length + 3;

                            _sendData[0] = 0x80;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x00;
                            _sendData[4] = (byte)(telLength >> 8);
                            _sendData[5] = (byte)(telLength & 0xFF);
                            _sendData[6] = 0x62;
                            _sendData[7] = _receiveData[4];
                            _sendData[8] = _receiveData[5];
                            Array.Copy(_zgwCert, 0, _sendData, 9, _zgwCert.Length);
                            responseFound = true;
                        }
                        else if (_receiveData[4] == 0x37 && _receiveData[5] == 0xFE)
                        {
                            Debug.WriteLine("RDBI_CPS Codierpruefstempel");

                            if (_codingStampDict.TryGetValue(_receiveData[1], out byte[] responseData))
                            {
                                Array.Copy(responseData, _sendData, responseData.Length);
                                _sendData[1] = 0xF1;
                                _sendData[2] = _receiveData[1];
                                _sendData[3] = 0x62;
                                responseFound = true;
                            }
                            else
                            {
                                Debug.WriteLine("No response found");
                            }
                        }
                        else if (_receiveData[4] == 0x17 && _receiveData[5] == 0x80)
                        {
                            Debug.WriteLine("SEC4DIAG_READ_AUTH_MODE");
                            Debug.WriteLine("Expected role mask: {0:X08}", EdSec4Diag.RoleMaskAsInt);

                            int telLength = 6 + 3;
                            _sendData[0] = 0x80;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x00;
                            _sendData[4] = (byte)(telLength >> 8);
                            _sendData[5] = (byte)(telLength & 0xFF);
                            _sendData[6] = 0x62;
                            _sendData[7] = _receiveData[4];
                            _sendData[8] = _receiveData[5];
                            _sendData[9] = 0x03;    // ACR with asymetric crypto
                            Array.Copy(EdSec4Diag.RoleMask, 0, _sendData, 10, EdSec4Diag.RoleMask.Length);
                            _sendData[14] = 0x01;   // Whitelist active
                            responseFound = true;
                        }
                        else if (_receiveData[4] == 0xF1 && _receiveData[5] == 0x00)
                        {
                            Debug.WriteLine("ReadActiveSessionState: defaultSession, FlashMode deactivated, EnergyModeDeactivated, SROE_Activated");

                            _sendData[0] = 0x87;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x62;
                            _sendData[4] = 0xF1;
                            _sendData[5] = 0x00;
                            _sendData[6] = 0x01;
                            _sendData[7] = 0x81;
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x01;
                            responseFound = true;
                        }

                        if (!responseFound)
                        {
                            _sendData[0] = 0x83;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x7F;
                            _sendData[4] = _receiveData[3];
                            _sendData[5] = 0x31;
                        }

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x23)
                    {
                        // dummy error response for service 23
                        Debug.WriteLine("Dummy service 23: {0:X02}", (object)_receiveData[4]);
                        _sendData[0] = 0x83;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x7F;
                        _sendData[4] = _receiveData[3];
                        _sendData[5] = 0x31;

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    int offset = _receiveData[0] == 0x80 ? 1 : 0;
                    if (offset > 0 && _receiveData[3] == 0x00)
                    {
                        offset += 2;
                    }
                    if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3 + offset] == 0x27)
                    {   // service 27 (security access)
                        found = true;

                        byte subFunction = _receiveData[4 + offset];
                        bool requestSeed = false;
                        switch (subFunction)
                        {
                            case 0x01:
                            case 0x03:
                            case 0x05:
                            case var n when (n >= 0x07 && n <= 0x41):
                                requestSeed = true;
                                break;
                        }

                        if (requestSeed)
                        {
                            if (_receiveData[0] == 0x86 &&
                                _receiveData[5 + offset] == 0xFF && _receiveData[6 + offset] == 0xFF && _receiveData[7 + offset] == 0xFF && _receiveData[8 + offset] == 0xFF)
                            {
                                if (subFunction == 0x01)
                                {
                                    Debug.WriteLine("Request seed 8 SubFunc: {0:X02}", (object)subFunction);
                                    byte[] dummyResponse = { 0x8A, _receiveData[2], _receiveData[1], 0x67, _receiveData[4 + offset], 0x12, 0x34, 0x56, 0x78, 0x12, 0x34, 0x56, 0x78, 0x00 };   // send seed
                                    ObdSend(dummyResponse, bmwTcpClientData);
                                }
                                else
                                {
                                    Debug.WriteLine("Request seed 20 SubFunc: {0:X02}", (object)subFunction);
                                    byte[] dummyResponse = { 0x96, _receiveData[2], _receiveData[1], 0x67, _receiveData[4 + offset],
                                        0x12, 0x34, 0x56, 0x78, 0x12, 0x34, 0x56, 0x78,
                                        0x12, 0x34, 0x56, 0x78, 0x12, 0x34, 0x56, 0x78,
                                        0x12, 0x34, 0x56, 0x78, 0x00 };   // send seed
                                    ObdSend(dummyResponse, bmwTcpClientData);
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Request seed 4 SubFunc: {0:X02}", (object)subFunction);
                                byte[] dummyResponse = { 0x86, _receiveData[2], _receiveData[1], 0x67, _receiveData[4 + offset], 0x12, 0x34, 0x56, 0x78, 0x00 };   // send seed
                                ObdSend(dummyResponse, bmwTcpClientData);
                            }
                        }
                        else
                        {
                            if (_receiveData[0] == 0x86)
                            {
                                Debug.WriteLine("Receive key SubFunc: {0:X02}, key: {1:X02} {2:X02} {3:X02} {4:X02}",
                                    subFunction, _receiveData[5 + offset], _receiveData[6 + offset], _receiveData[7 + offset], _receiveData[8 + offset]);
                            }
                            else
                            {
                                Debug.WriteLine("Dummy service 27: SubFunc: {0:X02}", (object)subFunction);
                            }
                            byte[] dummyResponse = { 0x82, _receiveData[2], _receiveData[1], 0x67, subFunction, 0x00 };   // positive ACK
                            ObdSend(dummyResponse, bmwTcpClientData);
                        }
                    }
                }

                if (!found)
                {
                    int offset = _receiveData[0] == 0x80 ? 1 : 0;
                    if (offset > 0 && _receiveData[3] == 0x00)
                    {
                        offset += 2;
                    }
                    if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3 + offset] == 0x29)
                    {   // service 29 (authentication)
                        found = true;

                        byte subFunction = _receiveData[4 + offset];
                        bool verifyCertUni = false;
                        bool proofOfOwnership = false;
                        bool authConf = false;
                        switch (subFunction)
                        {
                            case 0x01:
                                verifyCertUni = true;
                                break;

                            case 0x03:
                                proofOfOwnership = true;
                                break;

                            case 0x08:
                                authConf = true;
                                break;
                        }

                        if (verifyCertUni)
                        {
                            bool certValid = false;
                            Debug.WriteLine("Verify certificate unidirectional");
                            List<byte[]> parameterList = EdInterfaceEnet.GetS29ParameterList(_receiveData, offset + 6, 2);
                            if (parameterList == null || parameterList.Count < 2)
                            {
                                Debug.WriteLine("Invalid S29 parameters");
                            }
                            else
                            {
                                DebugLogData("Cert: ", parameterList[0], parameterList[0].Length);
                                DebugLogData("Ephemeral PublicKey: ", parameterList[1], parameterList[1].Length);

                                byte[] certBlock = parameterList[0];
                                List<byte[]> certList = EdInterfaceEnet.GetS29ParameterList(certBlock, 0);
                                if (certList == null || certList.Count < 2)
                                {
                                    Debug.WriteLine("Invalid certificate data");
                                }
                                else
                                {
                                    List<Org.BouncyCastle.X509.X509Certificate> x509CertList = new List<Org.BouncyCastle.X509.X509Certificate>();
                                    foreach (byte[] certData in certList)
                                    {
                                        try
                                        {
                                            Org.BouncyCastle.X509.X509Certificate x509Cert = new X509CertificateParser().ReadCertificate(certData);
                                            if (x509Cert == null)
                                            {
                                                Debug.WriteLine("Invalid X509 certificate data");
                                                break;
                                            }

                                            x509CertList.Add(x509Cert);
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("Error parsing X509 certificate: {0}", e.Message);
                                            x509CertList.Clear();
                                            break;
                                        }
                                    }

                                    if (x509CertList.Count > 0)
                                    {
                                        Debug.WriteLine("Cert chain length: {0}", x509CertList.Count);
                                        List<Org.BouncyCastle.X509.X509Certificate> rootCerts = EdBcTlsUtilities.ConvertToX509CertList(_serverCAs);
                                        if (EdBcTlsUtilities.ValidateCertChain(x509CertList, rootCerts))
                                        {
                                            Debug.WriteLine("Certificate chain is valid");
                                            try
                                            {
                                                bmwTcpClientData.ClientPublicKey = x509CertList[0].GetPublicKey() as ECPublicKeyParameters;
                                                if (bmwTcpClientData.ClientPublicKey == null)
                                                {
                                                    Debug.WriteLine("Invalid public key in certificate");
                                                }
                                                else
                                                {
                                                    certValid = true;
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Error getting public key from certificate: {0}", e.Message);
                                                certValid = false;
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Certificate chain is invalid");
                                        }
                                    }
                                }
                            }

                            if (certValid)
                            {
                                Debug.WriteLine("Certificate send challenge");
                                byte[] challenge = new byte[16];
                                RandomNumberGenerator.Create().GetBytes(challenge);
                                List<byte> challengeResponse = new List<byte> { (byte) (0x80 + 5 + challenge.Length), _receiveData[2], _receiveData[1], 0x69, subFunction, 0x11 };
                                EdInterfaceEnet.AppendS29DataBlock(ref challengeResponse, challenge); // challenge block
                                byte[] prefixData = Encoding.ASCII.GetBytes(EdSec4Diag.S29ProofOfOwnershipData);
                                EdInterfaceEnet.AppendS29DataBlock(ref challengeResponse, prefixData); // prefix block
                                challengeResponse.Add(0x00); // checksum
                                bmwTcpClientData.ServerChallenge = challenge;
                                ObdSend(challengeResponse.ToArray(), bmwTcpClientData);
                            }
                            else
                            {
                                Debug.WriteLine("Certificate is invalid");
                                byte[] errorResponse = { 0x83, _receiveData[2], _receiveData[1], 0x7F, subFunction, 0x31, 0x00 };   // negative ACK
                                ObdSend(errorResponse, bmwTcpClientData);
                            }
                        }
                        else if (proofOfOwnership)
                        {
                            Debug.WriteLine("Proof of ownership");
                            bool proofValid = false;
                            List<byte[]> parameterList = EdInterfaceEnet.GetS29ParameterList(_receiveData, offset + 5, 2);
                            if (parameterList == null || parameterList.Count < 2)
                            {
                                Debug.WriteLine("Invalid S29 parameters");
                            }
                            else
                            {
                                DebugLogData("Proof: ", parameterList[0], parameterList[0].Length);
                                DebugLogData("Ephemeral PublicKey: ", parameterList[1], parameterList[1].Length);
                                if (!EdSec4Diag.VerifyProofOfOwnership(parameterList[0], bmwTcpClientData.ServerChallenge, bmwTcpClientData.ClientPublicKey))
                                {
                                    Debug.WriteLine("Proof of ownership is invalid");
                                }
                                else
                                {
                                    Debug.WriteLine("Proof of ownership is valid");
                                    proofValid = true;
                                }
                            }

                            bmwTcpClientData.ServerChallenge = null; // reset challenge after use
                            if (proofValid)
                            {
                                byte[] validResponse = { 0x83, _receiveData[2], _receiveData[1], 0x69, subFunction, 0x12, 0x00 };   // positive ACK
                                ObdSend(validResponse, bmwTcpClientData);
                            }
                            else
                            {
                                byte[] errorResponse = { 0x83, _receiveData[2], _receiveData[1], 0x7F, subFunction, 0x31, 0x00 };   // negative ACK
                                ObdSend(errorResponse, bmwTcpClientData);
                            }
                        }
                        else if (authConf)
                        {
                            Debug.WriteLine("Authentication Configuration -> PKI certificate");
                            byte[] dummyResponse = { 0x83, _receiveData[2], _receiveData[1], 0x69, subFunction, 0x02, 0x00 };   // PKI certificate
                            bmwTcpClientData.ClientPublicKey = null;
                            bmwTcpClientData.ServerChallenge = null;
                            ObdSend(dummyResponse, bmwTcpClientData);
                        }
                        else
                        {
                            Debug.WriteLine("Dummy service 29: SubFunc: {0:X02}", (object)subFunction);
                            byte[] dummyResponse = { 0x82, _receiveData[2], _receiveData[1], 0x69, subFunction, 0x00 };   // positive ACK
                            ObdSend(dummyResponse, bmwTcpClientData);
                        }
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 3 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x2E)
                    {
                        // dummy ok response for service 2E WriteDataByLocalIdentification
                        Debug.WriteLine("Dummy service 2E: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);

                        if (_receiveData[4] == 0x37 && _receiveData[5] == 0xFE)
                        {
                            Debug.WriteLine("Store RDBI_CPS Codierpruefstempel");
                            byte[] codeStamp = new byte[recLength];
                            Array.Copy(_receiveData, codeStamp, codeStamp.Length);
                            _codingStampDict[_receiveData[1]] = codeStamp;
                        }

                        _sendData[0] = 0x83;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x6E;
                        _sendData[4] = _receiveData[4];
                        _sendData[5] = _receiveData[5];

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) == 0 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] >= 3 &&
                        _receiveData[4] == 0x2E)
                    {
                        // dummy ok response for service 2E WriteDataByLocalIdentification
                        Debug.WriteLine("Dummy service 2E long: {0:X02}{1:X02}", _receiveData[5], _receiveData[6]);

                        _sendData[0] = 0x83;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x6E;
                        _sendData[4] = _receiveData[5];
                        _sendData[5] = _receiveData[6];

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 3 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x30)
                    {
                        // dummy ok response for service 30 (actuator)
                        Debug.WriteLine("Dummy service 30: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);
                        _sendData[0] = 0x83;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x70;
                        _sendData[4] = _receiveData[4];
                        _sendData[5] = _receiveData[5];

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    int offset = _receiveData[0] == 0x80 ? 1 : 0;
                    if (offset > 0 && _receiveData[3] == 0x00)
                    {
                        offset += 2;
                    }
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3 + offset] == 0x31)
                    {
                        // dummy ok response for service 31 (routine control)
                        byte state = _receiveData[4 + offset];
                        Debug.WriteLine("Dummy service 31: {0:X02}: {1:X02}{2:X02}", state, _receiveData[5 + offset], _receiveData[6 + offset]);
                        if (_receiveData[4 + offset] == 0x01 && _receiveData[5 + offset] == 0x10 && _receiveData[6 + offset] == 0x02)
                        {
                            byte idType = _receiveData[7 + offset];
                            switch (idType)
                            {
                                case 0x01:
                                {
                                    int index = (_receiveData[10 + offset] << 8) | _receiveData[11 + offset];
                                    Debug.WriteLine("RC_RLEBI_IDR Type1 Index:{0}", (object)index);
                                    byte[] response =
                                    {
                                        0x9F, 0xF1, 0x63, 0x71, 0x01, 0x10, 0x02, 0x01,
                                        0x00, 0x02, 0x58, 0x0E, 0x10, 0x0E, 0x10, 0xFF,
                                        0xFF, 0x00, 0x46, 0x10, 0x02, 0x00, 0x00, 0x00,
                                        (byte)(index + 1), 0x00, 0x00, 0x00, 0x00, 0x04, 0xFE, 0x0E,
                                        0x00, 0x01
                                    };

                                    if (index >= 5)
                                    {
                                        Debug.WriteLine("RC_RLEBI_IDR Final entry");
                                        response[8] = 0x02; // final (0x01=not valid and not final)
                                    }

                                    Array.Clear(_sendData, 0, _sendData.Length);
                                    Array.Copy(response, _sendData, response.Length);
                                    found = true;
                                    break;
                                }

                                case 0x02:
                                {
                                    int index = (_receiveData[13 + offset] << 8) | _receiveData[14 + offset];
                                    Debug.WriteLine("RC_RLEBI_IDR Type2 Index:{0}", (object)index);
                                    byte[] response =
                                    {
                                        0x9F, 0xF1, 0x63, 0x71, 0x01, 0x10, 0x02, 0x02,
                                        0x00, 0x02, 0x58, 0x0E, 0x10, 0x0E, 0x10, 0xFF,
                                        0xFF, 0x00, 0x46, 0x10, 0x02, 0x00, 0x00, (byte)(index >> 8),
                                        (byte)index, 0x00, 0x00, 0x0F, 0x98, 0x04, 0xFE, 0x0E,
                                        0x00, 0x01
                                    };

                                    Array.Clear(_sendData, 0, _sendData.Length);
                                    Array.Copy(response, _sendData, response.Length);
                                    found = true;
                                    break;
                                }

                                default:
                                    Debug.WriteLine("Ignoring RC_RLEBI_IDR Type:{0}", (object)idType);
                                    break;

                            }
                        }

                        if (!found && _receiveData[4 + offset] == 0x01 && _receiveData[5 + offset] == 0x0F && _receiveData[6 + offset] == 0x01)
                        {
                            Debug.WriteLine("MCD3_FinalizeECUCoding {0}", (object)state);
                            _sendData[0] = 0x87;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x00;

                            found = true;
                        }

                        if (!found && _receiveData[4 + offset] == 0x01 && _receiveData[5 + offset] == 0x0F && _receiveData[6 + offset] == 0x0B)
                        {
                            Debug.WriteLine("executeDiagnosticService {0}", (object)state);
                            _sendData[0] = 0x8B;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = _receiveData[7 + offset];
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x04;
                            _sendData[10] = 0xC6;
                            _sendData[11] = _receiveData[11 + offset];
                            _sendData[12] = 0x00;
                            _sendData[13] = 0x02;

                            found = true;
                        }

                        if (!found && _receiveData[4 + offset] == 0x01 && _receiveData[5 + offset] == 0x70 && _receiveData[6 + offset] == 0x00)
                        {
                            Debug.WriteLine("RC_PAD processingApplicationData {0}", (object)state);
                            _sendData[0] = 0x85;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x01;

                            found = true;
                        }

                        if (!found && _receiveData[4 + offset] == 0x01 && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x33)
                        {
                            Debug.WriteLine("RC_GET_PARAM_N11_CSM {0}", (object)state);
                            int parLen = 16 * 2;
                            _sendData[0] = 0x80;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = (byte)(parLen + 3);
                            _sendData[4] = 0x71;
                            _sendData[5] = _receiveData[4 + offset];
                            _sendData[6] = _receiveData[5 + offset];
                            _sendData[7] = _receiveData[6 + offset];
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x00;
                            _sendData[10] = (byte)parLen;

                            int idx = 11;
                            _sendData[idx++] = 0x01;
                            _sendData[idx++] = 0x0B;

                            for (int i = 0; i < 3; i++)
                            {
                                _sendData[idx++] = (byte)(i + 0x20);
                                _sendData[idx++] = 0x00;
                            }

                            for (int i = 0; i < 12; i++)
                            {
                                _sendData[idx++] = (byte)(i + 0x40);
                                _sendData[idx++] = 0x00;
                            }

                            found = true;
                        }

                        if (!found && _receiveData[4 + offset] == 0x01 && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x12)
                        {
                            Debug.WriteLine("RC_STKL StoreTransportKeyList {0}", (object)state);
                            _sendData[0] = 0x87;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x00;

                            found = true;
                        }

                        if (!found && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x0C)
                        {
                            Debug.WriteLine("RC_SKE StartKeyExchange {0}", (object)state);
                            _sendData[0] = 0x87;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x00;

                            found = true;
                        }

                        if (!found && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x13)
                        {
                            Debug.WriteLine("InitSignalKeyDeployment {0}", (object)state);
                            _sendData[0] = 0x8A;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x01;
                            _sendData[10] = 0x10;
                            _sendData[11] = 0x00;
                            _sendData[12] = 0x04;

                            found = true;
                        }

                        if (!found && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x32)
                        {
                            Debug.WriteLine("RC_RESET_STATE_CSM Reset State F11 CSM {0}", (object)state);
                            _sendData[0] = 0x87;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;
                            _sendData[9] = 0x01;

                            found = true;
                        }

                        if (!found && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x34)
                        {
                            Debug.WriteLine("RC_EXT_INIT, Externer Init F25 {0}", (object)state);
                            _sendData[0] = 0x86;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;

                            found = true;
                        }

                        if (!found && _receiveData[5 + offset] == 0x02 && _receiveData[6 + offset] == 0x11)
                        {
                            Debug.WriteLine("RC_EA, ExternalAuthentication {0}", (object)state);
                            _sendData[0] = 0x86;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                            _sendData[7] = 0x00;
                            _sendData[8] = 0x00;

                            found = true;
                        }

                        if (!found)
                        {
                            Debug.WriteLine("Default response {0}", (object)state);
                            _sendData[0] = 0x84;
                            _sendData[1] = 0xF1;
                            _sendData[2] = _receiveData[1];
                            _sendData[3] = 0x71;
                            _sendData[4] = _receiveData[4 + offset];
                            _sendData[5] = _receiveData[5 + offset];
                            _sendData[6] = _receiveData[6 + offset];
                        }

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 2 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x34)
                    {
                        // dummy ok response for service 34 (request download, RD_IDR)
                        Debug.WriteLine("Dummy service 34: Format={0:X02}: AFID={1}, LFID={2}", _receiveData[4], _receiveData[5], _receiveData[5] >> 4);
                        _sendData[0] = 0x86;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x74;
                        _sendData[4] = 0x40;    // 4 bytes block length
                        _sendData[5] = 0x00;
                        _sendData[6] = 0x00;
                        _sendData[7] = 0x00;
                        _sendData[8] = 0x30;    // length 0x0030

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 3 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x35)
                    {
                        // dummy ok response for service 35 (request upload, RU_IDR)
                        Debug.WriteLine("Dummy service 35: Format={0:X02}: AFID={1}, LFID={2}", _receiveData[4], _receiveData[5], _receiveData[5] >> 4);
                        _sendData[0] = 0x86;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x75;
                        _sendData[4] = 0x40;    // 4 bytes block length
                        _sendData[5] = 0x00;
                        _sendData[6] = 0x00;
                        _sendData[7] = 0x0F;
                        _sendData[8] = 0x58;    // length 0x0F58

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 2 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x36)
                    {
                        byte sequence = _receiveData[4];
                        // dummy ok response for service 36 (transfer data, TD)
                        Debug.WriteLine("Dummy service 36: Seq={0}", (object)sequence);
                        _sendData[0] = 0x82 + 0x30;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x76;
                        _sendData[4] = sequence; // block sequence
                        for (int idx = 0; idx < 0x30; idx++)
                        {
                            _sendData[5+idx] = (byte) (idx + sequence);
                        }

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 1 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x37)
                    {
                        // dummy ok response for service 37 (transfer exit, RTE)
                        Debug.WriteLine("Dummy service 37");
                        _sendData[0] = 0x81;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x77;

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    if (
                        (_receiveData[0] & 0xC0) == 0x80 &&
                        (_receiveData[0] & 0x3F) >= 2 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x3B)
                    {
                        // dummy ok response for service 3B WriteDataByLocalIdentification
                        Debug.WriteLine("Dummy service 3B: {0:X02}", _receiveData[4]);
                        _sendData[0] = 0x82;
                        _sendData[1] = 0xF1;
                        _sendData[2] = _receiveData[1];
                        _sendData[3] = 0x7B;
                        _sendData[4] = _receiveData[4];

                        ObdSend(_sendData, bmwTcpClientData);
                        found = true;
                    }
                }

                if (!found)
                {
                    DebugLogData("Not found: ", _receiveData, recLength);
                }
            }
        }

        private bool HandleDynamicUdsIds(BmwTcpClientData bmwTcpClientData)
        {
            int ecuAddr = _receiveData[1];
            int dataId = -1;
            List<DynamicUdsValue> udsValueList = null;
            if (
                _receiveData[0] == 0x84 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x2C &&
                _receiveData[4] == 0x03)
            {
                // clear ID
                dataId = (_receiveData[5] << 8) | _receiveData[6];
            }
            else if (
                (_receiveData[0] & 0xC0) == 0x80 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x2C &&
                _receiveData[4] == 0x01)
            {
                // define IDs
                dataId = (_receiveData[5] << 8) | _receiveData[6];
                int items = ((_receiveData[0] & 0x3F) - 4) / 4;
                if (items > 0)
                {
                    udsValueList = new List<DynamicUdsValue>();
                    for (int i = 0; i < items; i++)
                    {
                        int itemId = (_receiveData[7 + i * 4] << 8) + _receiveData[8 + i * 4];
                        int pos = _receiveData[9 + i * 4];
                        int length = _receiveData[10 + i * 4];
                        // search response entry
                        ResponseEntry addEntry = null;
                        foreach (ResponseEntry responseEntry in _configData.ResponseList)
                        {
                            if (responseEntry.Request.Length >= 6)
                            {
                                if (responseEntry.Request[0] == 0x83 &&
                                    responseEntry.Request[1] == ecuAddr &&
                                    responseEntry.Request[2] == 0xF1 &&
                                    responseEntry.Request[3] == 0x22 &&
                                    responseEntry.Request[4] == (byte) (itemId >> 8) &&
                                    responseEntry.Request[5] == (byte) itemId
                                    )
                                {
                                    addEntry = responseEntry;
                                    break;
                                }
                            }
                        }
                        udsValueList.Add(new DynamicUdsValue(itemId, pos, length, addEntry));
                    }
                }
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22)
            {
                // read IDs
                int readId = (_receiveData[4] << 8) | _receiveData[5];
                foreach (DynamicUdsEntry dynamicUdsEntry in _dynamicUdsEntries)
                {
                    if (dynamicUdsEntry.EcuAddr == ecuAddr && dynamicUdsEntry.DataId == readId)
                    {
                        Debug.WriteLine("Found dynamic UDS ID: ECU={0}, ID={1:X04}", ecuAddr, readId);
                        int i = 0;
                        _sendData[i++] = 0x82;
                        _sendData[i++] = _receiveData[2];
                        _sendData[i++] = _receiveData[1];
                        _sendData[i++] = 0x62;
                        _sendData[i++] = _receiveData[4];   // ID
                        _sendData[i++] = _receiveData[5];

                        foreach (DynamicUdsValue dynamicUdsValue in dynamicUdsEntry.UdsValueList)
                        {
                            //Debug.WriteLine("UDS Data: ID={0:X04} P={1} L={2}", dynamicUdsValue.DataId, dynamicUdsValue.DataPos, dynamicUdsValue.DataLength);
                            for (int pos = 0; pos < dynamicUdsValue.DataLength; pos++)
                            {
                                byte value = 0x00;
                                if (dynamicUdsValue.ResponseEntry != null)
                                {
                                    int offset = dynamicUdsValue.DataPos + pos + 6;
                                    if (dynamicUdsValue.ResponseEntry.ResponseDyn.Length > offset)
                                    {
                                        value = dynamicUdsValue.ResponseEntry.ResponseDyn[offset];
                                    }
                                }
                                _sendData[i++] = value;
                            }
                        }
                        _sendData[0] = (byte)(0x80 | (i - 3));

                        ObdSend(_sendData, bmwTcpClientData);
                        return true;
                    }
                }
            }

            if (dataId >= 0)
            {
                for (int idx = 0; idx < _dynamicUdsEntries.Count;)
                {
                    DynamicUdsEntry dynamicUdsEntry = _dynamicUdsEntries[idx];
                    if (dynamicUdsEntry.EcuAddr == ecuAddr && dynamicUdsEntry.DataId == dataId)
                    {
                        Debug.WriteLine("Delete dynamic UDS ID: ECU={0}, ID={1:X04}", ecuAddr, dataId);
                        _dynamicUdsEntries.Remove(dynamicUdsEntry);
                    }
                    else
                    {
                        idx++;
                    }
                }

                if (udsValueList != null)
                {
                    StringBuilder sr = new StringBuilder();
                    sr.Append(string.Format("Add dynamic UDS ID: ECU={0}, ID={1:X04}, IDs=", ecuAddr, dataId));
                    foreach (DynamicUdsValue dynamicUdsValue in udsValueList)
                    {
                        sr.Append(string.Format("{0:X04},P={1},L={2},R={3} ",
                            dynamicUdsValue.DataId, dynamicUdsValue.DataPos, dynamicUdsValue.DataLength, dynamicUdsValue.ResponseEntry != null));
                    }
                    Debug.WriteLine(sr.ToString());
                    DynamicUdsEntry newEntry = new DynamicUdsEntry(ecuAddr, dataId, udsValueList);
                    _dynamicUdsEntries.Add(newEntry);
                }

                int i = 0;
                _sendData[i++] = 0x84;
                _sendData[i++] = _receiveData[2];
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = (byte) (_receiveData[3] | 0x40);
                _sendData[i++] = _receiveData[4];   // service
                _sendData[i++] = _receiveData[5];   // ID
                _sendData[i++] = _receiveData[6];

                ObdSend(_sendData, bmwTcpClientData);
                return true;
            }

            return false;
        }

        private bool ResponseE61(BmwTcpClientData bmwTcpClientData)
        {
            // axis unit
            if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0xC1)
            {   // get axis position
                int i = 0;
                int posFilt = (int)Math.Round(_axisPosFilt);
                _sendData[i++] = 0x8A;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x61;
                _sendData[i++] = 0xC1;
                _sendData[i++] = (byte)_axisPosRaw;         // left fast
                _sendData[i++] = (byte)(_axisPosRaw + 2);   // lest slow
                _sendData[i++] = (byte)posFilt;             // right fast
                _sendData[i++] = (byte)(posFilt + 2);       // rght slow
                _sendData[i++] = 0x00;
                _sendData[i++] = 0x00;
                _sendData[i++] = 0x00;
                _sendData[i++] = 0x00;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0xC0)
            {   // get voltage values
                int i = 0;
                _sendData[i++] = 0x8D;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x61;
                _sendData[i++] = 0xC0;
                _sendData[i++] = 0x0E;
                _sendData[i++] = 0x00;
                _sendData[i++] = 0x17;
                _sendData[i++] = 0x00;
                // battery voltage *100
                _sendData[i++] = (byte)_batteryVoltage;
                _sendData[i++] = (byte)(_batteryVoltage >> 8);

                _sendData[i++] = 0xF9;  // sensor voltage *100
                _sendData[i++] = 0x01;

                _sendData[i++] = 0xF7;  // sensor voltage *100
                _sendData[i++] = 0x01;

                _sendData[i++] = 0x4E;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0xC2)
            {   // digital status
                Array.Copy(_response3821C2, _sendData, _response3821C2.Length);
                _sendData[11] = (byte)((_speed < 10) ? 0x00 : 0x01);   // door contact
                // speed km/h
                _sendData[12] = (byte)_speed;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0xAC)
            {   // get compressor running time
                int i = 0;
                _sendData[i++] = 0x85;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x61;
                _sendData[i++] = 0xAC;

                _sendData[i++] = (byte)_compressorRunningTime;
                _sendData[i++] = (byte)(_compressorRunningTime >> 8);
                _sendData[i++] = 0x00;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x86 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x30 &&
                _receiveData[4] == 0x41 &&
                _receiveData[5] == 0x01)
            {   // get mode
                int i = 0;
                _sendData[i++] = 0x85;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x70;
                _sendData[i++] = 0x41;
                _sendData[i++] = 0x01;
                _sendData[i++] = 0x00;
                _sendData[i++] = _mode;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] >= 0x83 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x30 &&
                _receiveData[4] >= 0x11 && _receiveData[4] <= 0x14 &&
                _receiveData[5] == 0x01)
            {   // get valve state
                int channel = _receiveData[4] - 0x11;
                int i = 0;
                _sendData[i++] = 0x85;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x70;
                _sendData[i++] = _receiveData[4];
                _sendData[i++] = 0x01;
                _sendData[i++] = 0x00;
                _sendData[i++] = (byte)(((_outputs & (1 << channel)) != 0x00) ? 0x01 : 0x00);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x86 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x30 &&
                _receiveData[4] >= 0x11 && _receiveData[4] <= 0x14 &&
                _receiveData[5] == 0x07)
            {   // set valve state
                int channel = _receiveData[4] - 0x11;
                int i = 0;
                _sendData[i++] = 0x86;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x70;
                _sendData[i++] = _receiveData[4];
                _sendData[i++] = 0x07;
                _sendData[i++] = 0x00;
                _sendData[i++] = _receiveData[7];
                _sendData[i++] = _receiveData[8];

                ObdSend(_sendData, bmwTcpClientData);

                if (channel < _timeValveWrite.Length)
                {
                    _timeValveWrite[channel].Reset();
                    _timeValveWrite[channel].Start();
                    if ((_receiveData[7] & 0x01) != 0x00)
                    {
                        _outputs |= 1 << channel;
                    }
                    else
                    {
                        _outputs &= ~(1 << channel);
                    }
                }
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x31 &&
                _receiveData[4] == 0x0C)
            {   // set mode
                int i = 0;
                _sendData[i++] = 0x83;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x38;
                _sendData[i++] = 0x71;
                _sendData[i++] = 0x0C;
                _sendData[i++] = _receiveData[5];

                ObdSend(_sendData, bmwTcpClientData);
                switch (_receiveData[5])
                {
                    case 0x00:  // normal
                        if (_mode != 0x00)
                        {
                            _noResponseCount = 1;
                        }
                        _mode = 0x00;
                        break;

                    case 0x01:  // conveyor mode
                        if (_mode != 0x02)
                        {
                            _noResponseCount = 1;
                        }
                        _mode = 0x02;
                        break;

                    case 0x02:  // transport mode
                        if (_mode != 0x04)
                        {
                            _noResponseCount = 1;
                        }
                        _mode = 0x04;
                        break;

                    case 0x04:  // garage mode
                        if (_mode != 0x40)
                        {
                            _noResponseCount = 1;
                        }
                        _mode = 0x40;
                        break;
                }
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {   // read error memory for DIS
                Array.Copy(_response381802FFFF, _sendData, _response381802FFFF.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x17 &&
                _receiveData[4] == 0x5F &&
                _receiveData[5] == 0xB4)
            {
                Array.Copy(_response38175FB4, _sendData, _response38175FB4.Length);

                // Fehlerhäufigkeit
                _sendData[8] = 3;

                // Logistikzähler
                _sendData[9] = 20;

                // Kilometerstand
                // 0xFFFF: 524280 = 0x7FFF8 (Left shift 3)
                int intValue = (int)(123456 >> 3);
                _sendData[10] = (byte)(intValue >> 8);
                _sendData[11] = (byte)(intValue);

                // Unbenutzte UW
                intValue = 0x1234;
                _sendData[12] = (byte)(intValue >> 8);
                _sendData[13] = (byte)(intValue);

                intValue = 0x2345;
                _sendData[14] = (byte)(intValue >> 8);
                _sendData[15] = (byte)(intValue);

                intValue = 0x3456;
                _sendData[16] = (byte)(intValue >> 8);
                _sendData[17] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {   // standard response 1A80 for INPA
                Array.Copy(_response381A80, _sendData, _response381A80.Length);
#if false
                // production date
                _sendData[17] = 0x20;
                _sendData[18] = 0x10;
                _sendData[19] = 0x05;
                _sendData[20] = 0x11;
#endif
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x38 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x30)
            {   // standard response 2230 for INPA
                Array.Copy(_response382230, _sendData, _response382230.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            // motor unit DDE6.0 for M47 TÜ2
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x31 &&
                _receiveData[4] == 0x85)
            {   // set LL controller
                _idleSpeedControl = _receiveData[5];
                _timeIdleSpeedControlWrite.Reset();
                _timeIdleSpeedControlWrite.Start();

                int i = 0;
                _sendData[i++] = 0x83;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x12;
                _sendData[i++] = 0x71;
                _sendData[i++] = 0x85;
                _sendData[i++] = (byte)_idleSpeedControl;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                (_receiveData[0] & 0xC0) == 0x80 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x2C &&
                _receiveData[4] == 0x10)
            {   // request list
                int i = 0;
                _sendData[i++] = 0x82;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x12;
                _sendData[i++] = 0x6C;
                _sendData[i++] = 0x10;

                int items = ((_receiveData[0] & 0x3F) - 2) / 2;
                if (items == 0)
                {   // use last request data
                    if (_receiveDataMotorBackup[1] == 0x12)
                    {
                        _receiveDataMotorBackup.CopyTo(_receiveData, 0);
                        items = ((_receiveData[0] & 0x3F) - 2) / 2;
                    }
                }
                else
                {
                    _receiveData.CopyTo(_receiveDataMotorBackup, 0);
                }
                for (int j = 0; j < items; j++)
                {
                    int itemAddr = ((int)_receiveData[5 + j * 2] << 8) + _receiveData[6 + j * 2];
                    long itemValue = 0x000;
                    int resultBytes = 2;
                    switch (itemAddr)
                    {
                        case 0x0005:    // motor / refrigerant temp
                            // temp [C] + 41.08
                            itemValue = (long)(50 + 41.08);
                            break;

                        case 0x0080:    // Luftmasse
                            // 0x0000 == -1600
                            // 0x7FFF == 0
                            // 0xFFFF == +1600
                            // (air * 10 * 0xFFFF / 3200) + 0x7FFF;
                            itemValue = (350 * 0xFFFF / 3200) + 0x7FFF;
                            break;

                        case 0x0081:    // Luftmasse ist
                            // 0x0000 == -1600
                            // 0x8000 == 0
                            // 0xFFFF == 1600
                            // (lm [mg] + 1600) * 0xFFFF / 3200
                            itemValue = (527 + 1600) * 0xFFFF / 3200;
                            break;

                        case 0x0089:    // PWG 1 (Pedalwertgeber)
                            // 0x0000 == 0 V
                            // 0x1FFF == 5 V
                            // pwg [V] * 0x1FFF / 5
                            itemValue = (long)(3.5 * 0x1FFF / 5);
                            break;

                        case 0x008A:    // PWG 2 (Pedalwertgeber)
                            // 0x0000 == 0 V
                            // 0x1FFF == 5 V
                            // pwg [V] * 0x1FFF / 5
                            itemValue = (long)(1.3 * 0x1FFF / 5);
                            break;

                        case 0x008B:    // Umgebungsdruck
                            // 0x0000 == 0 mbar
                            // 0x8000 == 4096 mbar
                            // Druck [mbar] * 0x8000 / 4096
                            itemValue = (long)(935 * 0x8000 / 4096);
                            break;

                        case 0x008D:    // Luftmasse soll
                            // 0x0000 == -1600
                            // 0x8000 == 0
                            // 0xFFFF == 1600
                            // (lm [mg] + 1600) * 0xFFFF / 3200
                            itemValue = (523 + 1600) * 0xFFFF / 3200;
                            break;

                        case 0x0091:    // Ladedruck ist
                            // 0x0000 == 0
                            // 0x8000 == 4096
                            // Druck [mbar] * 0x8000 / 4096
                            itemValue = (long)(1935 * 0x8000 / 4096);
                            break;

                        case 0x0093:    // battery voltage
                            // 0x7F3C = 80.00 V
                            // bat * 100 * 0x7F3C / 8000
                            itemValue = _batteryVoltage * 0x7F3C / 8000;
                            break;

                        case 0x0095:    // refrigerant temp
                            // temp [C] + 41.08
                            // 0x0000 == -50.1
                            // 0x8000 == 499.9
                            // (temp [C] + 50.1) * 0x8000 / 550.0
                            itemValue = (long)((50.0 + 50.1) * 0x8000 / 550.0);
                            break;

                        case 0x009B:    // Partikelfilter Status Regeneration
                            // 0,1 == inaktiv
                            // >=2 == aktiv
                            itemValue = 0x0002;
                            break;

                        case 0x009E:    // motor rpm
                            // rpm * 8
                            itemValue = 400 * 8;
                            break;

                        case 0x00A0:    // Kraftstofftemperatur
                            // 0x0000 == -50.1
                            // 0x8000 == 499.9
                            // (temp [C] + 50.1) * 0x8000 / 550.0
                            itemValue = (long)((40.3 + 50.1) * 0x8000 / 550.0);
                            break;

                        case 0x00AD:    // intake air temp
                            // 0x0000 == -50.1
                            // 0x8000 == 499.9
                            // (temp [C] + 50.1) * 0x8000 / 550.0
                            itemValue = (long)((80 + 50.1) * 0x8000 / 550.0);
                            break;

                        case 0x00AE:    // Ladelufttemp
                            // 0x0000 == -50.1
                            // 0x8000 == 499.9
                            // (temp [C] + 50.1) * 0x8000 / 550.0
                            itemValue = (long)((60.1 + 50.1) * 0x8000 / 550.0);
                            break;

                        case 0x00C6:    // Ladedruck soll
                            // 0x0000 == 0
                            // 0x8000 == 4096
                            // Druck [mbar] * 0x8000 / 4096
                            itemValue = (long)(1938 * 0x8000 / 4096);
                            break;

                        case 0x00BF:    // Öldruckschalter
                            itemValue = 0x0001;
                            break;

                        case 0x00C2:    // Abgastemp vor Kat
                            // 0x0000 == -51.1
                            // 0x8000 == 32724.9
                            // (temp [C] + 51.1) * 0x8000 / 32776.0
                            itemValue = (long)((175.3 + 51.1) * 0x8000 / 32776);
                            break;

                        case 0x00CA:    // Abgastemp vor Partikelfilter
                            // 0x0000 == -51.1
                            // 0x8000 == 32724.9
                            // (temp [C] + 51.1) * 0x8000 / 32776.0
                            itemValue = (long)((165.3 + 51.1) * 0x8000 / 32776);
                            break;

                        case 0x00D1:    // Partikelfilter Strecke seit Regeneration
                            // 0x00 == 0m
                            // 0xFF == 32640m
                            // Strecke [m] >> 7
                            itemValue = 145678 >> 7;
                            break;

                        case 0x00D8:    // Abgasgegendruck
                            // 0 == 0m
                            // 0x8000 == 4096 mbar
                            // Druck [mbar] * 0x8000 / 4096
                            itemValue = (long)(2943 * 0x8000 / 4096);
                            break;

                        case 0x00DD:    // Partikelfilter Freigabe Regeneration
                            // 0 == freigegeben
                            // 1 == gesperrt
                            itemValue = 0x0000;
                            break;

                        case 0x00DF:    // Raildruck ist
                            // 0x0000 == 0
                            // 0x8000 == 1000
                            // Druck [mbar] * 0x8000 / 1000
                            itemValue = (long)(1027 * 0x8000 / 1000);
                            break;

                        case 0x00E1:    // Raildruck soll
                            // 0x0000 == 0
                            // 0x8000 == 1000
                            // Druck [mbar] * 0x8000 / 1000
                            itemValue = (long)(1024 * 0x8000 / 1000);
                            break;

                        case 0x13EC:    // Bremslichtschalter
                            itemValue = 0x0001;
                            break;

                        case 0x13ED:    // Bremslichttestschalter
                            itemValue = 0x0001;
                            break;

                        case 0x146E:    // Kupplungsschalter
                            itemValue = 0x0001;
                            break;

                        case 0x1482:    // FGR Bedienteil
                            itemValue = 0x006F; // 0x01: +, 0x02: -, 0x04: Wieder, 0x08: Aus, 0x20: +über, 0x40: -über
                            break;

                        case 0x15E4:    // Klimaschalter
                            itemValue = 0x0001;
                            break;

                        case 0x15E5:    // Umgebungstemperatur
                            // 0x0000 == -50.1
                            // 0x8000 == 499.9
                            // (temp [C] + 50.1) * 0x8000 / 550.0
                            itemValue = (long)((35.4 + 50.1) * 0x8000 / 550.0);
                            break;

                        case 0x1645:    // Getriebeart
                            itemValue = 0x0000; // (0=Hand, 1=Auto)
                            break;

                        case 0x1770:    // Drehung Zylinder 1
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x01))
                            {
                                break;
                            }
                            // 0x0000 = 0
                            // 0xFFFF = 8192
                            // (speed [rpm] -100) * 0xFFFF / 8192
                            itemValue = (long)(123.4 * 0xFFFF / 8192);
                            break;

                        case 0x1771:    // Drehung Zylinder 2
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x01))
                            {
                                break;
                            }
                            // 0x0000 = 0
                            // 0xFFFF = 8192
                            // (speed [rpm] -100) * 0xFFFF / 8192
                            itemValue = (long)(234.5 * 0xFFFF / 8192);
                            break;

                        case 0x1772:    // Drehung Zylinder 3
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x01))
                            {
                                break;
                            }
                            // 0x0000 = 0
                            // 0xFFFF = 8192
                            // (speed [rpm] -100) * 0xFFFF / 8192
                            itemValue = (long)(345.6 * 0xFFFF / 8192);
                            break;

                        case 0x1773:    // Drehung Zylinder 4
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x01))
                            {
                                break;
                            }
                            // 0x0000 = 0
                            // 0xFFFF = 8192
                            // (speed [rpm] -100) * 0xFFFF / 8192
                            itemValue = (long)(456.7 * 0xFFFF / 8192);
                            break;

                        case 0x177A:    // Mengenkorrektur Zylinder 1
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x00))
                            {
                                break;
                            }
                            // 0x0000 = -100
                            // 0xFFFF = 100
                            // (Mkorr [mg/Hub] + 100) * 0xFFFF / 200
                            itemValue = (long)((3.45 + 100) * 0xFFFF / 200);
                            break;

                        case 0x177B:    // Mengenkorrektur Zylinder 2
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x00))
                            {
                                break;
                            }
                            // 0x0000 = -100
                            // 0xFFFF = 100
                            // (Mkorr [mg/Hub] + 100) * 0xFFFF / 200
                            itemValue = (long)((1.23 + 100) * 0xFFFF / 200);
                            break;

                        case 0x177C:    // Mengenkorrektur Zylinder 3
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x00))
                            {
                                break;
                            }
                            // 0x0000 = -100
                            // 0xFFFF = 100
                            // (Mkorr [mg/Hub] + 100) * 0xFFFF / 200
                            itemValue = (long)((-4.56 + 100) * 0xFFFF / 200);
                            break;

                        case 0x177D:    // Mengenkorrektur Zylinder 4
                            if (!_timeIdleSpeedControlWrite.IsRunning || (_idleSpeedControl != 0x00))
                            {
                                break;
                            }
                            // 0x0000 = -100
                            // 0xFFFF = 100
                            // (Mkorr [mg/Hub] + 100) * 0xFFFF / 200
                            itemValue = (long)((-1.45 + 100) * 0xFFFF / 200);
                            break;

                        case 0x1952:    // Partikelfilter Anforderung Regeneration
                            // 0 == angefordert
                            // 1 == nicht angefordert
                            itemValue = 0x0000;
                            break;
                    }
                    if (resultBytes >= 4) _sendData[i++] = (byte)(itemValue >> 24);
                    if (resultBytes >= 3) _sendData[i++] = (byte)(itemValue >> 16);
                    if (resultBytes >= 2) _sendData[i++] = (byte)(itemValue >> 8);
                    if (resultBytes >= 1) _sendData[i++] = (byte)(itemValue);
                }
                _sendData[0] = (byte)(0x80 | (i - 3));

                //Thread.Sleep(2000);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {   // motor error log summary
                Array.Copy(_response121802FFFF, _sendData, _response121802FFFF.Length);

                _sendData[0] = 0x88;
                _sendData[4] = 0x02;
                _sendData[8] = 0x42;
                _sendData[9] = 0x22;
                _sendData[10] = 0x24;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x17 &&
                _receiveData[4] == 0x42 &&
                _receiveData[5] == 0x32)
            {   // motor error log detail for error 4232
                // digit 3 = zylinder numer
                // digit 4 = error type (2=disrupted)
                Array.Copy(_response12174232, _sendData, _response12174232.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x17 &&
                _receiveData[4] == 0x42 &&
                _receiveData[5] == 0x22)
            {   // motor error log detail for error 4222
                // digit 3 = zylinder numer
                // digit 4 = error type (2=disrupted)
                Array.Copy(_response12174232, _sendData, _response12174232.Length);

                _sendData[6] = 0x22;

                // Fehlerart
                // Bit 0-3:
                // 0: Kein passendes Fehlersymptom
                // 1: Signal oder Wert oberhalb der Schwelle
                // 2: Signal oder Wert unterhalb der Schwelle
                // 3: Unbekannte Fehlerart
                // 4: Kein Signal oder Wert
                // 5-7: Unbekannte Fehlerart
                // 8: Unplausibles Signal oder Wert
                // 9-15: Unbekannte Fehlerart
                //
                // Bit 4: 1=Testbedingung noch nicht erfüllt
                //
                // Bit 5-6:
                // 0: Fehler bisher nicht aufgetreten
                // 1: Fehler momentan nicht vorhanden, aber bereits gespeichert
                // 2: Fehler momentan vorhanden, aber noch nicht gespeichert
                // 3: Fehler momentan vorhanden, und bereits gespeichert
                //
                // Bit 7: 1=Fehler würde das Aufleuchten einer Warnlampe verursachen
                _sendData[7] = 0x24;

                // Status
                // Bit 0: 1=Fehler in Shadow eingetragen
                // Bit 1: 1=Block 1 gültig
                // Bit 2: 1=Block 1 gültig
                _sendData[8] = 0x07;

                // Fehlerdetail
                int intValue = 0x1234;
                _sendData[9] = (byte)(intValue >> 8);
                _sendData[10] = (byte)(intValue);

                // Fehlerhäufigkeit
                _sendData[11] = 10;

                // Logistikzähler
                _sendData[12] = 50;

                // Block1
                // Kilometerstand
                // 0xFFFF: 524280 = 0x7FFF8 (Left shift 3)
                intValue = (int)(123456 >> 3);
                _sendData[13] = (byte)(intValue >> 8);
                _sendData[14] = (byte)(intValue);

                // Motordrehzahl
                // 0xFF: 7033.54
                intValue = (int)(1000.0 * 0xFF / 7033.54);
                _sendData[15] = (byte)intValue;

                // Kühlmitteltemperatur
                // 0x00: -50.27
                // 0xFF: 205.73
                // (temp [C] + 50.27)
                intValue = (int)(100.0 + 50.27);
                _sendData[16] = (byte)intValue;

                // Raildruck ist
                // 0x00: 0
                // 0xFF: 2008.62
                intValue = (int)(1500.0 * 0xFF / 2008.62);
                _sendData[17] = (byte)intValue;

                // Einspritzmenge
                // 0x00: 0
                // 0xFF: 100.43
                intValue = (int)(50.0 * 0xFF / 100.43);
                _sendData[18] = (byte)intValue;

                // Luftmasse pro Zylinder
                // 0x00: 0
                // 0xFF: 1606.89
                intValue = (int)(1000.0 * 0xFF / 1606.89);
                _sendData[19] = (byte)intValue;

                // Ladedruck Istwert
                // 0x00: 0
                // 0xFF: 2510.77
                intValue = (int)(2000.0 * 0xFF / 2510.77);
                _sendData[20] = (byte)intValue;

                // Pedalwertgeber
                // 0x00: 0
                // 0xFF: 200.79
                intValue = (int)(80.0 * 0xFF / 200.79);
                _sendData[21] = (byte)intValue;

                // Batteriespannung
                // 0xFF: 41546.17
                intValue = (int)(12000.0 * 0xFF / 41546.17);
                _sendData[22] = (byte)intValue;

                // Geschwindigkeit
                // 0xFF: 251.68
                intValue = (int)(100.0 * 0xFF / 251.68);
                _sendData[23] = (byte)intValue;

                // Zustand Glühanzeige
                _sendData[24] = 210;

                // Block 2
                // Kilometerstand
                // 0xFFFF: 524280 = 0x7FFF8 (Left shift 3)
                intValue = (int)(234567 >> 3);
                _sendData[25] = (byte)(intValue >> 8);
                _sendData[26] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {   // standard response 1A80 for INPA
                Array.Copy(_response121A80, _sendData, _response121A80.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x94)
            {   // standard response 1A94 for DIS
                Array.Copy(_response121A94, _sendData, _response121A94.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0x20)
            {   // standard response 2120 for DIS
                Array.Copy(_response122120, _sendData, _response122120.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x86 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x23 &&
                _receiveData[4] == 0x00 &&
                _receiveData[5] == 0x00 &&
                _receiveData[6] == 0x00 &&
                _receiveData[7] == 0x07 &&
                _receiveData[8] == 0x40)
            {   // standard response 230000000740 for DIS
                Array.Copy(_response12230000000740, _sendData, _response12230000000740.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x86 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x23 &&
                _receiveData[4] == 0x00 &&
                _receiveData[5] == 0x00 &&
                _receiveData[6] == 0x40 &&
                _receiveData[7] == 0x07 &&
                _receiveData[8] == 0x40)
            {   // standard response 230000000740 for DIS
                Array.Copy(_response12230000400740, _sendData, _response12230000400740.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x40 &&
                _receiveData[5] == 0x21)
            {   // PM ident
                Array.Copy(_response12224021, _sendData, _response12224021.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x40 &&
                _receiveData[5] == 0x22)
            {   // PM info 1/3
#if false
                Array.Copy(_response12Nr, _sendData, _response12Nr.Length);
                ObdSend(_sendData);
                Thread.Sleep(1000);
#endif
                Array.Copy(_response12224022, _sendData, _response12224022.Length);

                // Batterieentladung gesamt Ah
                // 0x0000 = 0
                // 0xFFFF = 19088.16
                // Bat [Ah] * 0xFFFF / 19088.16
                int intValue = (int)(1345.6 * 0xFFFF / 19088.16);
                _sendData[7] = (byte)(intValue >> 8);
                _sendData[8] = (byte)(intValue);

                // Batterieladung gesamt Ah
                // 0x0000 = 0
                // 0xFFFF = 19088.16
                // Bat [Ah] * 0xFFFF / 19088.16
                intValue = (int)(1456.7 * 0xFFFF / 19088.16);
                _sendData[9] = (byte)(intValue >> 8);
                _sendData[10] = (byte)(intValue);

                // Stunden im Ladungsbereich
                // 0-20
                intValue = 4567;
                _sendData[11] = (byte)(intValue >> 8);
                _sendData[12] = (byte)(intValue);
                // 20-40
                intValue = 5678;
                _sendData[13] = (byte)(intValue >> 8);
                _sendData[14] = (byte)(intValue);
                // 40-60
                intValue = 6789;
                _sendData[15] = (byte)(intValue >> 8);
                _sendData[16] = (byte)(intValue);
                // 60-80
                intValue = 7890;
                _sendData[17] = (byte)(intValue >> 8);
                _sendData[18] = (byte)(intValue);
                // 80-100
                intValue = 8901;
                _sendData[19] = (byte)(intValue >> 8);
                _sendData[20] = (byte)(intValue);

                // Minuten bei Temp
                // 0x0000 = 0
                // 0xFFFF = 327675
                // Bat [Ah] * 0xFFFF / 19088.16
                // < 0
                intValue = (int)(1485 * 0xFFFF / 327675);
                _sendData[21] = (byte)(intValue >> 8);
                _sendData[22] = (byte)(intValue);
                // 0-20
                intValue = (int)(1357 * 0xFFFF / 327675);
                _sendData[23] = (byte)(intValue >> 8);
                _sendData[24] = (byte)(intValue);
                // 20-40
                intValue = (int)(3579 * 0xFFFF / 327675);
                _sendData[25] = (byte)(intValue >> 8);
                _sendData[26] = (byte)(intValue);
                // 40-60
                intValue = (int)(5791 * 0xFFFF / 327675);
                _sendData[27] = (byte)(intValue >> 8);
                _sendData[28] = (byte)(intValue);
                // > 60
                intValue = (int)(7913 * 0xFFFF / 327675);
                _sendData[29] = (byte)(intValue >> 8);
                _sendData[30] = (byte)(intValue);

                // Km Stand
                // Heute
                intValue = 123;
                _sendData[31] = (byte)(intValue >> 8);
                _sendData[32] = (byte)(intValue);
                // Vor 1 Tag
                intValue = 1234;
                _sendData[33] = (byte)(intValue >> 8);
                _sendData[34] = (byte)(intValue);
                // Vor 2 Tagen
                intValue = 12345;
                _sendData[35] = (byte)(intValue >> 8);
                _sendData[36] = (byte)(intValue);
                // Vor 3 Tagen
                intValue = 234;
                _sendData[37] = (byte)(intValue >> 8);
                _sendData[38] = (byte)(intValue);
                // Vor 4 Tagen
                intValue = 2345;
                _sendData[39] = (byte)(intValue >> 8);
                _sendData[40] = (byte)(intValue);
                // Vor 5 Tagen
                intValue = 23456;
                _sendData[41] = (byte)(intValue >> 8);
                _sendData[42] = (byte)(intValue);

                // Letzter Batterietausch
                intValue = 18346;
                _sendData[43] = (byte)(intValue >> 8);
                _sendData[44] = (byte)(intValue);
                intValue = 17346;
                _sendData[45] = (byte)(intValue >> 8);
                _sendData[46] = (byte)(intValue);
                intValue = 16346;
                _sendData[47] = (byte)(intValue >> 8);
                _sendData[48] = (byte)(intValue);
                intValue = 15346;
                _sendData[49] = (byte)(intValue >> 8);
                _sendData[50] = (byte)(intValue);

                // Batterieentladung während Motorlauf
                // 0x0000 = 0
                // 0xFFFF = 19088.16
                // Bat [Ah] * 0xFFFF / 19088.16
                intValue = (int)(4796.5 * 0xFFFF / 19088.16);
                _sendData[51] = (byte)(intValue >> 8);
                _sendData[52] = (byte)(intValue);

                // Ruhestromverletzung
                // 0x0 = kein Ruhestrom
                // 0x1 = 80-200mA
                // 0x2 = 200mA-1000mA
                // 0x3 = > 1000mA
                // 0x04 : Licht
                // 0x08 : Standheizung
                // 0x0C : Sonstige
                _sendData[54] = 0x01;
                _sendData[53] = 0x23;
                _sendData[56] = 0x48 | 0x11;
                _sendData[55] = 0xC0 | 0x11;
                _sendData[58] = 0x48 | 0x22;
                _sendData[57] = 0xC0 | 0x22;

                _sendData[66] = 0x48 | 0x33;
                _sendData[65] = 0xC0 | 0x33;

                // IBS Fehler Parity
                intValue = 10045;
                _sendData[69] = (byte)(intValue >> 8);
                _sendData[70] = (byte)(intValue);

                // IBS Fehler Watchdog Reset
                intValue = 10046;
                _sendData[71] = (byte)(intValue >> 8);
                _sendData[72] = (byte)(intValue);

                // IBS Fehler Power on Reset
                intValue = 10047;
                _sendData[73] = (byte)(intValue >> 8);
                _sendData[74] = (byte)(intValue);

                // KTBS Fehler BSD erweitert
                intValue = 20031;
                _sendData[75] = (byte)(intValue >> 8);
                _sendData[76] = (byte)(intValue);

                // KTIBS Fehler BSD
                intValue = 20032;
                _sendData[77] = (byte)(intValue >> 8);
                _sendData[78] = (byte)(intValue);

                // KTIBS Fehler EBSD Checksumme
                intValue = 20033;
                _sendData[79] = (byte)(intValue >> 8);
                _sendData[80] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x40 &&
                _receiveData[5] == 0x23)
            {   // PM info 2
                Array.Copy(_response12224023, _sendData, _response12224023.Length);
#if true

                // Battery capacity
                // 0x00 = 0
                // 0xFF = 255
                // Bat [Ah]
                _sendData[6] = 95;

                // SOH State of Health
                // 0x00 = 0
                // 0x7F = +50
                // 0xFF = -50
                // bit 7 : 1= neg
                // (SOH * 0x7F / 50.0) + ((SOH < 0) ? 0x80 : 0x00)
                double value = -45.0;
                _sendData[7] = (byte)((Math.Abs(value) * 0x7F / 50.0) + ((value < 0) ? 0x80 : 0x00));

                // SOC Fit
                // 0x00 = 0
                // 0xFF = 100.0
                // fit [%] * 0xFF / 100.0
                _sendData[8] = (byte)(90.0 * 0xFF / 100.0);

                // Saison temp
                // 0x00 = 0
                // 0x7F = +127.0
                // 0xFF = -127.0
                // bit 7 : 1= neg
                // (temp [°C] * 0x7F / 127.0) + ((temp < 0) ? 0x80 : 0x00)
                value = +23.0;
                _sendData[9] = (byte)((Math.Abs(value) * 0x7F / 127.0) + ((value < 0) ? 0x80 : 0x00));

                // Kalibrierereignisse
                _sendData[10] = 5;

                // Ah Q SOC
                // 0x00 = 0
                // 0xFF = 1188.3
                // Q [Ah] * 0xFF / 1188.3
                _sendData[11] = (byte)(300.0 * 0xFF / 1188.3);
                _sendData[12] = (byte)(400.0 * 0xFF / 1188.3);
                _sendData[13] = (byte)(500.0 * 0xFF / 1188.3);
                _sendData[14] = (byte)(600.0 * 0xFF / 1188.3);
                _sendData[15] = (byte)(700.0 * 0xFF / 1188.3);
                _sendData[16] = (byte)(800.0 * 0xFF / 1188.3);

                // Startfähigkeit
                // 0x00 = 0
                // 0xFF = 100.0
                // start [%] * 0xFF / 100.0
                _sendData[17] = (byte)(10.0 * 0xFF / 100.0);
                _sendData[18] = (byte)(20.0 * 0xFF / 100.0);
                _sendData[19] = (byte)(30.0 * 0xFF / 100.0);
                _sendData[20] = (byte)(40.0 * 0xFF / 100.0);
                _sendData[21] = (byte)(50.0 * 0xFF / 100.0);
                _sendData[22] = (byte)(60.0 * 0xFF / 100.0);

                // Ladungszustand
                // 0x00 = 0
                // 0xFF = 100.0
                // start [%] * 0xFF / 100.0
                _sendData[23] = (byte)(20.0 * 0xFF / 100.0);
                _sendData[24] = (byte)(30.0 * 0xFF / 100.0);
                _sendData[25] = (byte)(40.0 * 0xFF / 100.0);
                _sendData[26] = (byte)(50.0 * 0xFF / 100.0);
                _sendData[27] = (byte)(60.0 * 0xFF / 100.0);
                _sendData[28] = (byte)(70.0 * 0xFF / 100.0);

                // IBS Intelligent Battery sensor
                // IBS error download checksum
                _sendData[29] = 35;
                // IBS error EEPROM diag
                _sendData[30] = 8;
                // IBS error RAM diag
                _sendData[31] = 9;
                // IBS error PROM diag
                _sendData[32] = 16;
                // IBS error I2C NAC
                _sendData[33] = 1;
                // IBS error Bus Coll
                _sendData[34] = 2;
#endif
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {   // read error memory
                Array.Copy(_responseA01802FFFF, _sendData, _responseA01802FFFF.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {   // CCC nav
                Array.Copy(_responseA01A80p1, _sendData, _responseA01A80p1.Length);
                ObdSend(_sendData, bmwTcpClientData);

                Array.Copy(_responseA01A80p2, _sendData, _responseA01A80p2.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x20 &&
                _receiveData[5] == 0x00)
            {   // CCC nav
                Array.Copy(_responseA0222000, _sendData, _responseA0222000.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x21)
            {   // CCC nav tacho pulses
                Array.Copy(_responseA022F121, _sendData, _responseA022F121.Length);

                // GYRO status
                // 0=OK
                _sendData[6] = 0;

                // tacho pulses
                int intValue = 312;
                _sendData[8] = (byte)(intValue >> 8);
                _sendData[9] = (byte)(intValue);

                // gear rate
                // 0x4000 : 20V
                // 0x0000 : 0V
                intValue = (int)(7.854 * 0x4000 / 20.0);
                _sendData[10] = (byte)(intValue >> 8);
                _sendData[11] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x20)
            {   // CCC nav GPS status
                Array.Copy(_responseA022F120p1, _sendData, _responseA022F120p1.Length);
                ObdSend(_sendData, bmwTcpClientData);

                Array.Copy(_responseA022F120p2, _sendData, _responseA022F120p2.Length);

                // HIP driver
                // 0=OK
                // 1=No data
                _sendData[6] = 0;

                // GPS status
                // 0=search
                // 1=tracking
                // 2=2D
                // 3=3D
                _sendData[7] = 3;

                // Almanach
                // 0=Not OK
                // 1=OK
                _sendData[8] = 1;

                // 4000 delay are accepted by INPA
                //Thread.Sleep(4000);
                // test for CarControl
                //Thread.Sleep(750);
                Thread.Sleep(300);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x22)
            {   // CCC nav, Self test GPS
                Array.Copy(_responseA022F122p1, _sendData, _responseA022F122p1.Length);
                ObdSend(_sendData, bmwTcpClientData);

                Array.Copy(_responseA022F122p2, _sendData, _responseA022F122p2.Length);

                // Self test GPS
                // 0=OK
                // 1=Not connected
                // 2=Short circuit
                int intValue = 0;
                _sendData[6] = (byte)(intValue >> 8);
                _sendData[7] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x23)
            {   // CCC nav position
                Array.Copy(_responseA022F123, _sendData, _responseA022F123.Length);

                // GPS data valid
                // 0=OK
                _sendData[6] = 0;

                // Position latitude
                // max = +0x7FFFFFFF;
                // min = -0x7FFFFFFF;
                // 180 Grad, 60 min, 60 sec, Rest ms (180 * 60 * 60 * 1000)
                long longValue = 45 * 60 * 60 * 1000 + 32 * 60 * 1000 + 56 * 1000 + 764;
                int intValue = (int)(longValue * 0x7FFFFFFF / (180 * 60 * 60 * 1000));
                _sendData[7] = (byte)(intValue >> 24);
                _sendData[8] = (byte)(intValue >> 16);
                _sendData[9] = (byte)(intValue >> 8);
                _sendData[10] = (byte)(intValue);

                // Position longitude
                // max = +0x7FFFFFFF;
                // min = -0x7FFFFFFF;
                // 180 Grad, 60 min, 60 sec, Rest ms (180 * 60 * 60 * 1000)
                longValue = -(42 * 60 * 60 * 1000 + 24 * 60 * 1000 + 53 * 1000 + 876);
                intValue = (int)(longValue * 0x7FFFFFFF / (180 * 60 * 60 * 1000));
                _sendData[11] = (byte)(intValue >> 24);
                _sendData[12] = (byte)(intValue >> 16);
                _sendData[13] = (byte)(intValue >> 8);
                _sendData[14] = (byte)(intValue);

                // Height NN
                intValue = 350;
                _sendData[15] = (byte)(intValue >> 24);
                _sendData[16] = (byte)(intValue >> 16);
                _sendData[17] = (byte)(intValue >> 8);
                _sendData[18] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x24)
            {   // CCC nav dr position
                Array.Copy(_responseA022F124, _sendData, _responseA022F124.Length);

                // Position latitude
                // max = +0x7FFFFFFF;
                // min = -0x7FFFFFFF;
                // 180 Grad, 60 min, 60 sec, Rest ms (180 * 60 * 60 * 1000)
                long longValue = 45 * 60 * 60 * 1000 + 32 * 60 * 1000 + 56 * 1000 + 764;
                int intValue = (int)(longValue * 0x7FFFFFFF / (180 * 60 * 60 * 1000));
                _sendData[7] = (byte)(intValue >> 24);
                _sendData[8] = (byte)(intValue >> 16);
                _sendData[9] = (byte)(intValue >> 8);
                _sendData[10] = (byte)(intValue);

                // Position longitude
                // max = +0x7FFFFFFF;
                // min = -0x7FFFFFFF;
                // 180 Grad, 60 min, 60 sec, Rest ms (180 * 60 * 60 * 1000)
                longValue = -(42 * 60 * 60 * 1000 + 24 * 60 * 1000 + 53 * 1000 + 876);
                intValue = (int)(longValue * 0x7FFFFFFF / (180 * 60 * 60 * 1000));
                _sendData[11] = (byte)(intValue >> 24);
                _sendData[12] = (byte)(intValue >> 16);
                _sendData[13] = (byte)(intValue >> 8);
                _sendData[14] = (byte)(intValue);

                // Height NN
                intValue = 350;
                _sendData[15] = (byte)(intValue >> 24);
                _sendData[16] = (byte)(intValue >> 16);
                _sendData[17] = (byte)(intValue >> 8);
                _sendData[18] = (byte)(intValue);

                // Status position 1 = OK
                _sendData[19] = 1;

                // Geschwindigkeit
                // 0xFFFF: 2359.2123360 km/h
                // 0x0000: 0 km/h
                intValue = (int) (100 * 0xFFFF / 2359.2123360);
                _sendData[20] = (byte)(intValue >> 8);
                _sendData[21] = (byte)(intValue);

                // Status Geschwindigkeit 1 = OK
                _sendData[22] = 1;

                // Richtung
                // 0x7FF8: 180 Grad
                // 0x0000: 0 km/h
                longValue = (57 * 60 * 60 + 48 * 60 + 53);
                intValue = (int)(longValue * 0x7FF8 / (180 * 60 * 60));
                _sendData[23] = (byte)(intValue >> 8);
                _sendData[24] = (byte)(intValue);

                // Status Richtung 1 = OK
                _sendData[25] = 1;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x25)
            {   // CCC nav resolution
                Array.Copy(_responseA022F125, _sendData, _responseA022F125.Length);

                // Vertikale Auflösung
                int intValue = 1234;
                _sendData[7] = (byte)(intValue >> 8);
                _sendData[8] = (byte)(intValue);

                // Horizontale Auflösung
                intValue = 2345;
                _sendData[9] = (byte)(intValue >> 8);
                _sendData[10] = (byte)(intValue);

                // Position Auflösung
                intValue = 3456;
                _sendData[11] = (byte)(intValue >> 8);
                _sendData[12] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x27)
            {   // CCC nav gps date/time
                Array.Copy(_responseA022F127, _sendData, _responseA022F127.Length);

                if (VariableValues)
                {
                    DateTime dateTime = DateTime.Now.ToUniversalTime();
                    // year (bcd), real coding seems to be different
                    _sendData[7] = IntToBcd(dateTime.Year / 100);
                    _sendData[8] = IntToBcd(dateTime.Year % 100);    // real code is 0xA4

                    _sendData[9] = IntToBcd(dateTime.Month);    // month
                    _sendData[10] = IntToBcd(dateTime.Day);     // day
                    // time (bcd)
                    _sendData[11] = IntToBcd(dateTime.Hour);    // hour
                    _sendData[12] = IntToBcd(dateTime.Minute);  // min
                    _sendData[13] = IntToBcd(dateTime.Second);  // sec
                }

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0xA0 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xF1 &&
                _receiveData[5] == 0x28)
            {   // CCC nav gps satellites
                Array.Copy(_responseA022F128, _sendData, _responseA022F128.Length);

                _sendData[8] = 20;   // verfolgbare satelliten
                _sendData[9] = 22;   // empfangbare satelliten

                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x040
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x40 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x90)
            {
                Array.Copy(_response401A90, _sendData, _response401A90.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x40 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {
                Array.Copy(_response401802FFFF, _sendData, _response401802FFFF.Length);

                _sendData[0] = 0x82;
                _sendData[4] = 0x00;

                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x60
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x60 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {
                Array.Copy(_response601A80, _sendData, _response601A80.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x60 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0x0B)
            {
                Array.Copy(_response60210B, _sendData, _response60210B.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x60 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0x17)
            {
                Array.Copy(_response602117, _sendData, _response602117.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x70
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x70 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x10 &&
                _receiveData[5] == 0x00)
            {
                Array.Copy(_response70221000, _sendData, _response70221000.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x70 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {
                Array.Copy(_response701A80, _sendData, _response701A80.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x70 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x90)
            {
                Array.Copy(_response701A90, _sendData, _response701A90.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x86 &&
                _receiveData[1] == 0x70 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x23 &&
                _receiveData[4] == 0x00 &&
                _receiveData[5] == 0x00 &&
                _receiveData[6] == 0x00 &&
                _receiveData[7] == 0x07 &&
                _receiveData[8] == 0x12)
            {
                Array.Copy(_response70230000000712, _sendData, _response70230000000712.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x86 &&
                _receiveData[1] == 0x70 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x23 &&
                _receiveData[4] == 0x00 &&
                _receiveData[5] == 0x00 &&
                _receiveData[6] == 0x12 &&
                _receiveData[7] == 0x07 &&
                _receiveData[8] == 0x12)
            {
                Array.Copy(_response70230000120712, _sendData, _response70230000120712.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x73 CID
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x73 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {
                Array.Copy(_response731A80, _sendData, _response731A80.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x73 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {
                Array.Copy(_response731802FFFF, _sendData, _response731802FFFF.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x78 IHK2
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x78 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {
                Array.Copy(_response781A80, _sendData, _response781A80.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x78 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {
                Array.Copy(_response781802FFFF, _sendData, _response781802FFFF.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x64 PDC
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x64 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {
                Array.Copy(_response641A80, _sendData, _response641A80.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x64 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {
                Array.Copy(_response641802FFFF, _sendData, _response641802FFFF.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x64 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x17 &&
                _receiveData[4] == 0xE2 &&
                _receiveData[5] == 0x05)
            {
                Array.Copy(_response6417E205, _sendData, _response6417E205.Length);

                // Fehlerhäufigkeit
                _sendData[8] = 5;

                // Kilometerstand
                // 0xFFFF: 524280 = 0x7FFF8 (Left shift 3)
                int intValue = (int)(123456 >> 3);
                _sendData[9] = (byte)(intValue >> 8);
                _sendData[10] = (byte)(intValue);

                // 0x0000 == -50.1
                // 0x8000 == 499.9
                // (temp [C] + 50.1) * 0x8000 / 550.0

                // Temperatur
                // 0x00: -40.00
                // 0xFF: 87.5
                // (temp [C] + 40.00) * 0xFF / 127.5
                intValue = (int)((30.0 + 40.00) * 0xFF / 127.5);
                _sendData[11] = (byte)intValue;

                // Kilometerstand
                // 0xFFFF: 524280 = 0x7FFF8 (Left shift 3)
                intValue = (int)(234567 >> 3);
                _sendData[12] = (byte)(intValue >> 8);
                _sendData[13] = (byte)(intValue);

                // Temperatur
                // 0x00: -40.00
                // 0xFF: 87.5
                // (temp [C] + 40.00) * 0xFF / 127.5
                intValue = (int)((80.0 + 40.00) * 0xFF / 127.5);
                _sendData[14] = (byte)intValue;

                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x65 SZM
            else if (
                _receiveData[0] == 0x82 &&
                _receiveData[1] == 0x65 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x1A &&
                _receiveData[4] == 0x80)
            {
                Array.Copy(_response651A80, _sendData, _response651A80.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x84 &&
                _receiveData[1] == 0x65 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x18 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0xFF &&
                _receiveData[6] == 0xFF)
            {
                Array.Copy(_response651802FFFF, _sendData, _response651802FFFF.Length);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x65 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x17 &&
                _receiveData[4] == 0x9F &&
                _receiveData[5] == 0xF1)
            {
                Array.Copy(_response65179FF1, _sendData, _response65179FF1.Length);

                // Fehlerart
                // Bit 0-3:
                // 0: Kein passendes Fehlersymptom
                // 1: Signal oder Wert oberhalb der Schwelle
                // 2: Signal oder Wert unterhalb der Schwelle
                // 3: Unbekannte Fehlerart
                // 4: Kein Signal oder Wert
                // 5-7: Unbekannte Fehlerart
                // 8: Unplausibles Signal oder Wert
                // 9-15: Unbekannte Fehlerart
                //
                // Bit 4: 1=Testbedingung noch nicht erfüllt
                //
                // Bit 5-6:
                // 0: Fehler bisher nicht aufgetreten
                // 1: Fehler momentan nicht vorhanden, aber bereits gespeichert
                // 2: Fehler momentan vorhanden, aber noch nicht gespeichert
                // 3: Fehler momentan vorhanden, und bereits gespeichert
                //
                // Bit 7: 1=Fehler würde das Aufleuchten einer Warnlampe verursachen
                _sendData[7] = 0xA4;

                // Kilometerstand
                // 0xFFFF: 524280 = 0x7FFF8 (Left shift 3)
                int intValue = (int)(123456 >> 3);
                _sendData[8] = (byte)(intValue >> 8);
                _sendData[9] = (byte)(intValue);

                intValue = (int)(234567 >> 3);
                _sendData[10] = (byte)(intValue >> 8);
                _sendData[11] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x65 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x21 &&
                _receiveData[4] == 0xF9 &&
                _receiveData[5] == 0x07)
            {   // Key status
                Array.Copy(_response6521F907, _sendData, _response6521F907.Length);

                // Bit 2: PDC
                // Bit 4: FDC
                _sendData[6] = 0x04;

                // Bit 0: Heizung links
                // Bit 1: Klima links
                // Bit 2: Aktivsitz links
                // Bit 4: Heizung rechts
                // Bit 5: Klima rechts
                // Bit 6: Aktivsitz rechts
                _sendData[7] = 0x00;

                ObdSend(_sendData, bmwTcpClientData);
            }
            // device 0x78 IHK
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x78 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x30 &&
                _receiveData[4] == 0x02 &&
                _receiveData[5] == 0x01)
            {   // Status Regler
                Array.Copy(_response78300201, _sendData, _response78300201.Length);

                // Sollwert Basis
                // 0x00: 0 °C
                // 0xFF: 127.5 °C
                // (temp [C]) * 0xFF / 127.5
                int intValue = (int)(15.0 * 0xFF / 127.5);
                _sendData[7] = (byte)(intValue);

                // Aussenwert
                // 0x00: -40.00
                // 0xFF: 87.5
                // (temp [C] + 40.00) * 0xFF / 127.5
                intValue = (int)((35.0 + 40.00) * 0xFF / 127.5);
                _sendData[8] = (byte)(intValue);

                // Waermetauscher Istwert (rechts)
                // 0x00: 5°
                // 0xFF: 132.5°
                // (Istwert [C] - 5) * 0xFF / 127.5
                intValue = (int)((90.0 - 5) * 0xFF / 127.5);
                _sendData[10] = (byte)(intValue);

                // Innenwert
                // 0x00: 10.0
                // 0xFF: 52.5
                // (temp [C] - 10.0) * 0xFF / 42.5
                intValue = (int)((20.0 - 10.0) * 0xFF / 42.5);
                _sendData[11] = (byte)(intValue);

                // Luftleistung
                // 0x00: 0%
                // 0xFF: 255%
                intValue = 45;
                _sendData[12] = (byte)(intValue);

                // Hauptstellgroesse (rechts)
                // 0x00: -27%
                // 0xFF: 100%
                // (Stell [%] + 27.0) * 0xFF / 127
                intValue = (int)((90.0 + 27.0) * 0xFF / 127);
                _sendData[14] = (byte)(intValue);

                // Wasserventiloeffnungszeit (rechts)
                // 0x0000: 0 ms
                // 0xFFFF: -1 ms
                intValue = 1234;
                _sendData[17] = (byte)(intValue >> 8);
                _sendData[18] = (byte)(intValue);

                // Innenwert verzoegert
                // 0x00: 10.0
                // 0xFF: 52.5
                // (temp [C] - 10.0) * 0xFF / 42.5
                intValue = (int)((22.0 - 10.0) * 0xFF / 42.5);
                _sendData[19] = (byte)(intValue);

                // Sollwert (links)
                // 0x00: 10°
                // 0xFF: 52.5°
                // (temp [C] - 10.0) * 0xFF / 42.5
                intValue = (int)((30.0 - 10.0) * 0xFF / 42.5);
                _sendData[20] = (byte)(intValue);

                // Waermetauschersollwert (rechts)
                // 0x00: 5°
                // 0xFF: 132.5°
                // (Sollwert [C] - 5) * 0xFF / 127.5
                intValue = (int)((100.0 - 5) * 0xFF / 127.5);
                _sendData[23] = (byte)(intValue);

                // Waermetauscherstellgroesse (rechts)
                // 0x00: 0%
                // 0xFF: 127%
                // (Stell [%]) * 0xFF / 127
                intValue = (int)((50.0) * 0xFF / 127);
                _sendData[25] = (byte)(intValue);

                // Führungsgroesse (links)
                // 0x0000: 0%
                // 0xFFFF: -1%
                intValue = 30;
                _sendData[28] = (byte)(intValue >> 8);
                _sendData[29] = (byte)(intValue);

                // Geschwindigkeit
                // 0x00: 0 km/h
                // 0xFF: 255 km/h
                intValue = 150;
                _sendData[32] = (byte)(intValue);

                // Motordrehzahl
                // 0x00: 0 1/min
                // 0xFF: 12750 1/min
                intValue = (int)((1000.0) * 0xFF / 12750);
                _sendData[33] = (byte)(intValue);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x78 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x30 &&
                _receiveData[4] == 0x06 &&
                _receiveData[5] == 0x01)
            {   // Status Digital
                Array.Copy(_response78300601, _sendData, _response78300601.Length);

                ObdSend(_sendData, bmwTcpClientData);
            }
            else
            {   // nothing matched, check response list
                return false;
            }
            return true;
        }

        private bool ResponseE90(BmwTcpClientData bmwTcpClientData)
        {
            if (
                (_receiveData[0] & 0xC0) == 0x80 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x2C &&
                _receiveData[4] == 0x10)
            {   // request list
                int i = 0;
                _sendData[i++] = 0x82;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x12;
                _sendData[i++] = 0x6C;
                _sendData[i++] = 0x10;

                int items = ((_receiveData[0] & 0x3F) - 2) / 2;
                if (items == 0)
                {   // use last request data
                    if (_receiveDataMotorBackup[1] == 0x12)
                    {
                        _receiveDataMotorBackup.CopyTo(_receiveData, 0);
                        items = ((_receiveData[0] & 0x3F) - 2) / 2;
                    }
                }
                else
                {
                    _receiveData.CopyTo(_receiveDataMotorBackup, 0);
                }
                for (int j = 0; j < items; j++)
                {
                    int itemAddr = ((int)_receiveData[5 + j * 2] << 8) + _receiveData[6 + j * 2];
                    long itemValue = 0x000;
                    int resultBytes = 2;
                    switch (itemAddr)
                    {
                        case 0x0005:    // refrigerant temp (sensor)
                            // temp [C] * 1.000000 -40.000000
                            itemValue = (long)(50.0 + 40.000000);
                            resultBytes = 1;
                            break;

                        case 0x0010:    // Luftmasse von HFM (OBD_PID10_AFS_dmSens)
                            // (air * 0.010000);
                            itemValue = (long)(355.0 / 0.010000);
                            break;

                        case 0x0042:    // battery voltage
                            // bat [V] * 0.001000
                            itemValue = (long)(_batteryVoltage / 100.0 / 0.001000);
                            break;

                        case 0x012C: // Batteriespannung korrigiert
                            // bat [mV] * 0.389105
                            itemValue = (long)(_batteryVoltage * 10.0 / 0.389105);
                            break;

                        case 0x01F4:    // Ladedruck soll
                            // Druck [mbar] * 0.091554
                            itemValue = (long)(1938.0 / 0.091554);
                            break;

                        case 0x0385:    // Kraftstofftemperatur
                            // (temp [C] * 0.010000) -50.000000
                            itemValue = (long)((40.3 + 50.000000) / 0.010000);
                            break;

                        case 0x03EB:    // Partikelfilter Strecke seit Regeneration (IDSLRE)
                            // Strecke [m]
                            itemValue = 145678;
                            resultBytes = 4;
                            break;

                        case 0x03EE:    // Partikelfilter Freigabe Regeneration (ISRBF)
                            // 0 == freigegeben
                            // 1 == gesperrt
                            itemValue = 0;
                            resultBytes = 1;
                            break;

                        case 0x0404:    // Partikelfilter Anforderung Regeneration (PFltRgn_numRgn)
                            // 4 - 6 == angefordert
                            // other: nicht angefordert
                            itemValue = 4;
                            resultBytes = 1;
                            break;

                        case 0x041B:    // Abgastemp vor Partikelfilter
                            // (temp [C] * 0.031281) -50.000000
                            itemValue = (long)((165.3 + 50.000000) / 0.031281);
                            break;

                        case 0x041E:    // Abgastemp vor Kat
                            // (temp [C] * 0.031281) -50.000000
                            itemValue = (long)((175.3 + 50.000000) / 0.031281);
                            break;

                        case 0x0424:    // Abgasgegendruck
                            // Druck [mbar]
                            itemValue = (long)(2943);
                            break;

                        case 0x0458:    // oil temp
                            // temp [C] * 0.010000 -100.000000
                            itemValue = (long)((60.0 + 100.000000) / 0.010000);
                            break;

                        case 0x0547:    // refrigerant temp
                            // temp [C] * 0.010000 -100.000000
                            itemValue = (long)((50.0 + 100.000000) / 0.010000);
                            break;

                        case 0x05AA:    // Partikelfilter Status Regeneration (CoEOM_stOpModeAct)
                            // bit1 set == aktiv
                            // other == inaktiv
                            itemValue = 0x02;
                            resultBytes = 4;
                            break;

                        case 0x0641:    // Raildruck soll
                            // Druck [mbar] * 0.045777
                            itemValue = (long)(1024.0 / 0.045777);
                            break;

                        case 0x0672:    // Raildruck ist
                            // Druck [mbar] * 0.045777
                            itemValue = (long)(1027.0 / 0.045777);
                            break;

                        case 0x0708:    // Luftmasse (ILMKG)
                            // (air * 0.100000);
                            itemValue = (long)(350.0 / 0.100000);
                            break;

                        case 0x0709:    // Luftmasse ist
                            // (lm [mg] * 0.024414)
                            itemValue = (long)(527.0 / 0.024414 );
                            break;

                        case 0x076D:    // Ladedruck ist
                            // Druck [mbar] * 0.091554
                            itemValue = (long)(1935.0 / 0.091554);
                            break;

                        case 0x076F:    // Ladelufttemp
                            // (temp [C] * 0.010000) -100.000000
                            itemValue = (long)((60.1 + 100.000000) / 0.010000);
                            break;

                        case 0x0772:    // intake air temp
                            // (temp [C] * 0.100000) -273.140000
                            itemValue = (long)((80.0 + 273.140000) / 0.100000);
                            break;

                        case 0x079E:    // Luftmasse soll
                            // (lm [mg] * 0.030518)
                            itemValue = (long)(523.0 / 0.030518);
                            break;

                        case 0x0ABE:    // Öldruckschalter
                            itemValue = 0x0001;
                            break;

                        case 0x0AF1:    // motor temp
                            // temp [C] * 0.100000 -273.140000
                            itemValue = (long)((50.0 + 273.140000) / 0.100000);
                            break;

                        case 0x0BA4:    // Partikelfilter Restlaufstrecke
                            // dist * 10
                            itemValue = (long)(100000d / 10);
                            break;

                        case 0x0C1C:    // Umgebungsdruck
                            // Druck [mbar] * 0.030518
                            itemValue = (long)(935.0 / 0.030518);
                            break;

                        case 0x0FD2:    // Umgebungstemperatur
                            // (temp [C] * 0.100000) -273.140000
                            itemValue = (long)((35.4 + 273.140000) / 0.100000);
                            break;

                        case 0x1881:    // motor rpm
                            // rpm * 0.500000
                            itemValue = (long)(400.0 / 0.500000);
                            break;
                    }
                    if (resultBytes >= 4) _sendData[i++] = (byte)(itemValue >> 24);
                    if (resultBytes >= 3) _sendData[i++] = (byte)(itemValue >> 16);
                    if (resultBytes >= 2) _sendData[i++] = (byte)(itemValue >> 8);
                    if (resultBytes >= 1) _sendData[i++] = (byte)(itemValue);
                }
                _sendData[0] = (byte)(0x80 | (i - 3));

                ObdSend(_sendData, bmwTcpClientData);
            }
            else
            {   // nothing matched, check response list
                return false;
            }
            return true;
        }

        private bool ResponseG31(BmwTcpClientData bmwTcpClientData)
        {
            // Axis
            if (HandleDynamicUdsIds(bmwTcpClientData))
            {
                return true;
            }

            if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xDC &&
                _receiveData[5] == 0x05)
            {
                // read position
                float value1 = 1.0f / 1000;
                float value2 = -1.0f / 1000;
                float value3 = (float)((_axisPosFilt + 1) / 1000);
                float value4 = (float)((_axisPosFilt - 1) / 1000);
                byte[] dataArray1 = BitConverter.GetBytes(value1);
                byte[] dataArray2 = BitConverter.GetBytes(value2);
                byte[] dataArray3 = BitConverter.GetBytes(value3);
                byte[] dataArray4 = BitConverter.GetBytes(value4);

                int i = 0;
                _sendData[i++] = 0x93;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x76;
                _sendData[i++] = 0x62;
                _sendData[i++] = 0xDC;
                _sendData[i++] = 0x05;
                _sendData[i++] = dataArray1[3];
                _sendData[i++] = dataArray1[2];
                _sendData[i++] = dataArray1[1];
                _sendData[i++] = dataArray1[0];
                _sendData[i++] = dataArray2[3];
                _sendData[i++] = dataArray2[2];
                _sendData[i++] = dataArray2[1];
                _sendData[i++] = dataArray2[0];
                _sendData[i++] = dataArray3[3];
                _sendData[i++] = dataArray3[2];
                _sendData[i++] = dataArray3[1];
                _sendData[i++] = dataArray3[0];
                _sendData[i++] = dataArray4[3];
                _sendData[i++] = dataArray4[2];
                _sendData[i++] = dataArray4[1];
                _sendData[i++] = dataArray4[0];

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x85 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x31 &&
                _receiveData[4] == 0x01 &&
                _receiveData[5] == 0x0F &&
                _receiveData[6] == 0x0C)
            {
                // set mode
                int i = 0;
                _sendData[i++] = 0x84;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x76;
                _sendData[i++] = 0x71;
                _sendData[i++] = 0x01;
                _sendData[i++] = 0x0F;
                _sendData[i++] = 0x0C;

                ObdSend(_sendData, bmwTcpClientData);

                byte mode = _receiveData[7];
                switch (mode)
                {
                    case 0x00: // normal
                    case 0x01: // production mode
                    case 0x02: // transport mode
                    case 0x03: // flash
                        if (_mode != mode)
                        {
                            _noResponseCount = 1;
                        }

                        _mode = mode;
                        break;
                }
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x10 &&
                _receiveData[5] == 0x0A)
            {
                // get mode
                int i = 0;
                _sendData[i++] = 0x84;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x76;
                _sendData[i++] = 0x62;
                _sendData[i++] = 0x10;
                _sendData[i++] = 0x0A;
                _sendData[i++] = _mode;

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0xDA &&
                _receiveData[5] == 0xD8)
            {   // read voltage
                int i = 0;
                _sendData[i++] = 0x85;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x62;
                _sendData[i++] = _receiveData[4];
                _sendData[i++] = _receiveData[5];

                int value = _batteryVoltage / 10;
                _sendData[i++] = (byte)(value >> 8);
                _sendData[i++] = (byte)value;
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] >= 0x86 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x2F &&
                _receiveData[4] == 0xDB &&
                _receiveData[5] == 0x67 &&
                _receiveData[6] == 0x03)
            {
                // get valve state
                int channel = _receiveData[8] - 0x11;
                int i = 0;
                _sendData[i++] = 0x86;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x76;
                _sendData[i++] = 0x6F;
                _sendData[i++] = 0xDB;
                _sendData[i++] = 0x67;
                _sendData[i++] = 0x03;
                _sendData[i++] = 0x00;
                if (_outputsActiveTime.IsRunning && _outputsActiveTime.ElapsedMilliseconds > 5000)
                {
                    _sendData[i++] = 0x00;
                }
                else
                {
                    _sendData[i++] = (byte)(((_outputs & (1 << channel)) != 0x00) ? 0x01 : 0x00);
                }

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x89 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x2E &&
                _receiveData[4] == 0xDB &&
                _receiveData[5] == 0x77)
            {
                // set valve state
                int channel = _receiveData[11] - 0x11;
                int i = 0;
                _sendData[i++] = 0x83;
                _sendData[i++] = 0xF1;
                _sendData[i++] = 0x76;
                _sendData[i++] = 0x6E;
                _sendData[i++] = 0xDB;
                _sendData[i++] = 0x77;

                ObdSend(_sendData, bmwTcpClientData);

                if (channel == 0xB2)
                {
                    _timeValveWrite[0].Reset();
                    _timeValveWrite[0].Start();
                    _timeValveWrite[1].Reset();
                    _timeValveWrite[1].Start();
                    _timeValveWrite[8].Reset();
                    _timeValveWrite[8].Start();
                    _outputs = 0x103;
                }
                else
                {
                    if (channel < _timeValveWrite.Length)
                    {
                        _timeValveWrite[channel].Reset();
                        _timeValveWrite[channel].Start();
                        if ((_receiveData[7] & 0x01) != 0x00)
                        {
                            _outputs |= 1 << channel;
                        }
                        else
                        {
                            _outputs &= ~(1 << channel);
                        }
                    }
                }
            }
            else if (
                (_receiveData[0] & 0xC0) == 0x80 &&
                (_receiveData[0] & 0x3F) >= 3 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x31)
            {
                // dummy ok response for service 31 (routine control)
                Debug.WriteLine("Dummy service 31 (G31): {0:X02}: {1:X02}{2:X02}", _receiveData[4], _receiveData[5], _receiveData[6]);

                if (_receiveData[4] == 0x01 && _receiveData[5] == 0x10 && _receiveData[6] == 0x02)
                {
                    Debug.WriteLine("Using default handler");
                    return false;
                }

                if (_receiveData[4] == 0x01 && _receiveData[5] == 0x0F && _receiveData[6] == 0x01)
                {
                    Debug.WriteLine("Using default handler");
                    return false;
                }

                _sendData[0] = 0x84;
                _sendData[1] = 0xF1;
                _sendData[2] = _receiveData[1];
                _sendData[3] = 0x71;
                _sendData[4] = _receiveData[4];
                _sendData[5] = _receiveData[5];
                _sendData[6] = _receiveData[6];

                bool withResponse = false;
                switch (_receiveData[4])
                {
                    case 0x01:
                    case 0x03:
                        withResponse = true;
                        break;
                }

                if (withResponse)
                {
                    switch (_receiveData[5])
                    {
                        case 0xF0:
                            _sendData[0]++;
                            _sendData[7] = 0x01;    // active
                            break;

                        case 0xAB:
                            _sendData[0] += 8;

                            _sendData[7] = 0x01;
                            _sendData[8] = 0x02;

                            _sendData[9] = 0x03;
                            _sendData[10] = 0x04;

                            _sendData[11] = 0x05;
                            _sendData[12] = 0x06;

                            _sendData[13] = 0x07;
                            _sendData[14] = 0x08;
                            break;
                    }
                }

                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                _receiveData[0] == 0x83 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22 &&
                _receiveData[4] == 0x40 &&
                _receiveData[5] == 0xC6)
            {   // read voltage
                int i = 0;
                _sendData[i++] = 0x89;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x62;
                _sendData[i++] = _receiveData[4];
                _sendData[i++] = _receiveData[5];

                int value = _batteryVoltage * 10;
                _sendData[i++] = (byte)(value >> 8);
                _sendData[i++] = (byte)value;
                _sendData[i++] = 0xFF;
                _sendData[i++] = 0xD8;
                _sendData[i++] = 0x14;
                _sendData[i++] = 0x01;
                ObdSend(_sendData, bmwTcpClientData);
            }
            else
            {
                // nothing matched, check response list
                return false;
            }
            return true;
        }

        private bool ResponseG31Generic(BmwTcpClientData bmwTcpClientData)
        {
            if (
                (_receiveData[0] & 0xC0) == 0x80 &&
                _receiveData[1] == 0x76 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22)
            {
                int length = (_receiveData[0] & 0x3F) - 1;

                int i = 0;
                _sendData[i++] = 0x80;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x00;  // 16 bit length
                _sendData[i++] = 0x00;  // length h
                _sendData[i++] = 0x01;  // length l
                _sendData[i++] = (byte)(_receiveData[3] | 0x40);

                for (int offset = 0; offset < length; offset += 2)
                {
                    int serviceId = (_receiveData[4 + offset] << 8) + _receiveData[5 + offset];
                    int responseLength = GetResposeLength("vdp_g11.prg", serviceId);
                    Debug.WriteLine("Response length: {0}", responseLength);

                    if (responseLength <= 0)
                    {   // no data, check response list
                        return false;
                    }

                    _sendData[i++] = (byte)(serviceId >> 8);
                    _sendData[i++] = (byte)(serviceId & 0xFF);
                    for (int j = 0; j < responseLength; j++)
                    {
                        _sendData[i++] = 0;
                    }
                }

                int sendLength = i - 6;
                _sendData[4] = (byte)(sendLength >> 8);
                _sendData[5] = (byte)(sendLength & 0xFF);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else if (
                (_receiveData[0] & 0xC0) == 0x80 &&
                _receiveData[1] == 0x12 &&
                _receiveData[2] == 0xF1 &&
                _receiveData[3] == 0x22)
            {
                int length = (_receiveData[0] & 0x3F) - 1;

                int i = 0;
                _sendData[i++] = 0x80;
                _sendData[i++] = 0xF1;
                _sendData[i++] = _receiveData[1];
                _sendData[i++] = 0x00;  // 16 bit length
                _sendData[i++] = 0x00;  // length h
                _sendData[i++] = 0x01;  // length l
                _sendData[i++] = (byte)(_receiveData[3] | 0x40);

                for (int offset = 0; offset < length; offset += 2)
                {
                    int serviceId = (_receiveData[4 + offset] << 8) + _receiveData[5 + offset];
                    int responseLength = GetResposeLength("d83bx7c0.prg", serviceId);
                    Debug.WriteLine("Response length: {0}", responseLength);

                    if (responseLength <= 0)
                    {   // no data, check response list
                        return false;
                    }

                    _sendData[i++] = (byte)(serviceId >> 8);
                    _sendData[i++] = (byte)(serviceId & 0xFF);
                    for (int j = 0; j < responseLength; j++)
                    {
                        _sendData[i++] = 0;
                    }
                }

                int sendLength = i - 6;
                _sendData[4] = (byte)(sendLength >> 8);
                _sendData[5] = (byte)(sendLength & 0xFF);
                ObdSend(_sendData, bmwTcpClientData);
            }
            else
            {
                // nothing matched
                return false;
            }

            return true;
        }
        private bool ResponseSmg2(BmwTcpClientData bmwTcpClientData)
        {
            if (
                _receiveData[0] == 0x85 &&
                _receiveData[1] == 0x32 &&
                _receiveData[2] == 0xF1)
            {
                if (
                    _receiveData[3] == 0x90 &&
                    _receiveData[4] == 0x42 &&
                    _receiveData[5] == 0x4D &&
                    _receiveData[6] == 0x57)
                {
                    // seed_key request
                    // 85 32 F1 90 42 4D 57 XX CRC : AB 32 F1 A0 37 38 34 33 32 36 30 32 46 30 30 FF FF FF FF 31 35 30 35 30 30 30 30 30 30 30 30 30 30 31 30 30 35 32 35 31 31 35 30 33 31 31 E4
                    byte[] response =
                    {
                        0xAB, 0x32, 0xF1, 0xA0, 0x37, 0x38, 0x34, 0x33,
                        0x32, 0x36, 0x30, 0x32, 0x46, 0x30, 0x30, 0xFF,
                        0xFF, 0xFF, 0xFF, 0x31, 0x35, 0x30, 0x35, 0x30,
                        0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                        0x30, 0x31, 0x30, 0x30, 0x35, 0x32, 0x35, 0x31,
                        0x31, 0x35, 0x30, 0x33, 0x31, 0x31, 0xE4
                    };

                    Debug.WriteLine("Seed Key request");
                    Array.Copy(response, _sendData, response.Length);
                    ObdSend(_sendData, bmwTcpClientData);
                }
                else
                {
                    // seed_key response
                    // 85 32 F1 90 91 98 91 97 89 : 82 32 F1 A0 00 45
                    byte[] response =
                    {
                        0x82, 0x32, 0xF1, 0xA0, 0x00, 0x45
                    };

                    Debug.WriteLine("Seed Key response");
                    Array.Copy(response, _sendData, response.Length);
                    ObdSend(_sendData, bmwTcpClientData);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private void SerialConcept1Transmission()
        {
            int recLength = 0;
            for (;;)
            {
                if (!ReceiveData(_receiveData, recLength, 1))
                {   // complete tel received
                    break;
                }
                recLength++;
            }
            if (recLength == 0)
            {
                return;
            }

#if true
            Debug.WriteLine(string.Format("Time: {0}", DateTime.Now.ToString("hh:mm:ss.fff")));
            DebugLogData("Request: ", _receiveData, recLength);
#endif
            if (!_adsAdapter && !_klineResponder)
            {
                // send echo
                SendData(_receiveData, recLength);
            }

            bool found = false;
            foreach (ResponseEntry responseEntry in _configData.ResponseList)
            {
                if (recLength != responseEntry.Request.Length) continue;
                bool equal = true;
                for (int i = 0; i < recLength - 1; i++)
                {   // don't compare checksum
                    if (_receiveData[i] != responseEntry.Request[i])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal)
                {       // entry found
                    found = true;
                    byte[] response = responseEntry.ResponseList[0];
                    int responseLen = response.Length;
                    if (responseLen > 0)
                    {
                        Array.Copy(response, _sendData, responseLen);
                        _sendData[responseLen - 1] = CalcChecksumXor(_sendData, responseLen - 1);
                        SendData(_sendData, responseLen);
                    }
                    break;
                }
            }
            if (!found)
            {
                DebugLogData("Not found: ", _receiveData, recLength);
            }
        }

        private void SerialKwp1281Transmission()
        {
            byte wakeAddress;
            bool initOk;
            do
            {
                initOk = false;
                if (!ReceiveWakeUp(out wakeAddress))
                {
                    break;
                }
                Debug.WriteLine("Wake Address: {0:X02}", wakeAddress);
                byte[] configData = GetConfigData(wakeAddress);
                if (configData == null)
                {
                    Debug.WriteLine("Invalid wake address");
                    continue;
                }

                Thread.Sleep(60);  // maximum is 2000ms
                _sendData[0] = 0x55;
                SendData(_sendData, 0, 1);

                Thread.Sleep(5);   // maximum 400ms
                int sendLen;
                if (configData.Length > 1)
                {
                    sendLen = configData.Length - 1;
                    Array.Copy(configData, 1, _sendData, 0, sendLen);
                }
                else
                {
                    sendLen = 2;
                    _sendData[0] = 0x08;
                    _sendData[1] = 0x08;
                }

                SendData(_sendData, 0, sendLen);

                if (ReceiveData(_receiveData, 0, 1, 50, 50))  // too fast for ELM
                //if (ReceiveData(_receiveData, 0, 1, 80, 80))
                {
                    if ((byte) (~_receiveData[0]) == _sendData[1])
                    {
                        initOk = true;
                    }
                    else
                    {
                        Debug.WriteLine("Invalid init response {0}", (byte)(~_receiveData[0]));
                    }
                }
                else
                {
                    Debug.WriteLine("No init response");
                }
            } while (!initOk);
            _kwp1281InvEchoIndex = 0;

            Debug.WriteLine("Init done");

            ResponseEntry responseOnlyEntry = null;
            foreach (ResponseEntry responseEntry in _configData.ResponseOnlyList)
            {
                if ((responseEntry.Config == null) || (responseEntry.Config.Length < 1) ||
                    (responseEntry.Config[0] != wakeAddress))
                {
                    continue;
                }
                responseOnlyEntry = responseEntry;
                break;
            }

            bool requestInvalid = false;
            byte blockCount = 1;
            int telBlockIndex = 0;
            int initSequenceCount = 0;
            ResponseEntry identityResponse = responseOnlyEntry;
            ResponseEntry activeResponse = null;
            byte[] lastCoding = null;
            int lastAdaption = DefaultAdaptionChannelValue;
            for (; ; )
            {
                if (_stopThread)
                {
                    break;
                }

                byte[] responseOnlyData = null;
                if (responseOnlyEntry != null && initSequenceCount < responseOnlyEntry.ResponseMultiList.Count)
                {
                    responseOnlyData = responseOnlyEntry.ResponseMultiList[initSequenceCount++];
                }
                if (responseOnlyData != null)
                {
                    Array.Copy(responseOnlyData, _sendData, responseOnlyData.Length);
                }
                else
                {
                    _sendData[0] = 0x03;    // block length
                    _sendData[2] = (byte) (requestInvalid ? 0x0A : 0x09);    // NACK, ACK
                }
                requestInvalid = false;

                if (activeResponse != null)
                {
                    if (telBlockIndex < activeResponse.ResponseMultiList.Count)
                    {
                        byte[] responseTel = activeResponse.ResponseMultiList[telBlockIndex];
                        if (lastCoding != null && responseTel.Length == 8 && responseTel[2] == 0xF6 && responseTel[3] == 0x00)
                        {
                            Debug.WriteLine("updating coding values");
                            Array.Copy(lastCoding, 0, responseTel, 4, lastCoding.Length);
                        }
                        Array.Copy(responseTel, _sendData, responseTel.Length);
                        telBlockIndex++;
                    }
                    if (telBlockIndex >= activeResponse.ResponseMultiList.Count)
                    {
                        activeResponse = null;
                    }
                }
                resend:
                _sendData[1] = blockCount++;    // block counter

                if (_stopThread)
                {
                    break;
                }

                if (!SendKwp1281Block(_sendData))
                {
                    Debug.WriteLine("Send block failed");
                    break;
                }

                if (!ReceiveKwp1281Block(_receiveData))
                {
                    Debug.WriteLine("Receive block failed");
                    break;
                }
                if (blockCount != _receiveData[1])
                {
                    Debug.WriteLine("Block count invalid");
                    //break;
                }
                blockCount++;
                byte command = _receiveData[2];
                int recLength = _receiveData[0];
                if (command == 0x06)
                {   // end output
                    Debug.WriteLine("Disconnect");
                    break;
                }
                if (command == 0x0A)
                {   // NACK, resend last telegram
                    Debug.WriteLine("NACK, resend telegram");
                    goto resend;
                }
                if (command != 0x09)
                {   // no ack
                    DebugLogData("Request: ", _receiveData, recLength);

                    bool found = false;
                    foreach (ResponseEntry responseEntry in _configData.ResponseList)
                    {
                        if ((responseEntry.Config == null) || (responseEntry.Config.Length < 1) ||
                            responseEntry.Config[0] != wakeAddress)
                        {
                            continue;
                        }
                        if (recLength != responseEntry.Request.Length) continue;
                        bool equal = true;
                        for (int i = 0; i < recLength; i++)
                        {
                            if (i == 1)
                            {   // don't compare block count
                                continue;
                            }
                            if (_receiveData[i] != responseEntry.Request[i])
                            {
                                equal = false;
                                break;
                            }
                        }
                        if (equal)
                        {       // entry found
                            found = true;
                            activeResponse = responseEntry;
                            telBlockIndex = 0;
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (_receiveData.Length >= 4 && _receiveData[0] == 0x04 && _receiveData[2] == 0x29)
                        {   // read mwblock
                            found = true;
                            Debug.WriteLine("Dummy mwblock: {0:X02}", _receiveData[3]);
                            byte[] dummyResponse = { 0x0F, 0x00, 0xE7, 0x10, 0x00, 0x00, 0x10, 0x00, 0x00, 0x10, 0x00, 0x00, 0x10, 0x00, 0x00 };
                            //byte[] dummyResponse = { 0x04, 0x00, 0x0A, 0x0F };
                            activeResponse = new ResponseEntry(_receiveData, dummyResponse, null);
                            activeResponse.ResponseMultiList.Add(dummyResponse);
                            telBlockIndex = 0;
                        }
                        else if (_receiveData.Length >= 4 && _receiveData[0] >= 0x07 && _receiveData[2] == 0x10)
                        {   // parameter coding
                            found = true;
                            Debug.WriteLine("Parameter coding");
                            lastCoding = new byte[4];
                            Array.Copy(_receiveData, 3, lastCoding, 0, lastCoding.Length);
                            activeResponse = identityResponse;
                            telBlockIndex = 0;
                        }
                        else if (_receiveData.Length >= 4 && _receiveData[0] >= 0x04 && (_receiveData[2] == 0x21 || _receiveData[2] == 0x22 || _receiveData[2] == 0x2A))
                        {   // adaption
                            found = true;
                            bool addValues = true;
                            if (_receiveData.Length >= 9 && _receiveData[0] >= 0x09 && _receiveData[2] == 0x2A)
                            {
                                byte adaptionChannel = _receiveData[3];
                                Debug.WriteLine("Adaption save channel: {0}", adaptionChannel);
                                if (adaptionChannel == ResetAdaptionChannel)
                                {
                                    lastAdaption = DefaultAdaptionChannelValue;
                                    Debug.WriteLine("Adaption reset value: {0:X04}", lastAdaption);
                                }
                                else
                                {
                                    lastAdaption = (_receiveData[4] << 8) + _receiveData[5];
                                    Debug.WriteLine("Adaption save value: {0:X04}", lastAdaption);
                                }
                                addValues = false;
                            }
                            else if (_receiveData.Length >= 6 && _receiveData[0] >= 0x06 && _receiveData[2] == 0x22)
                            {
                                byte adaptionChannel = _receiveData[3];
                                Debug.WriteLine("Adaption transfer channel: {0}", adaptionChannel);
                                if (adaptionChannel != ResetAdaptionChannel)
                                {
                                    lastAdaption = (_receiveData[4] << 8) + _receiveData[5];
                                    Debug.WriteLine("Adaption transfer value: {0:X04}", lastAdaption);
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Adaption read: {0:X04}", lastAdaption);
                            }
                            List<byte> dummyResponseList = new List<byte>() { 0x06, 0x00, 0xE6, _receiveData[3], (byte)(lastAdaption >> 8), (byte)lastAdaption };
                            if (addValues)
                            {
                                dummyResponseList.AddRange(new List<byte>() { 0xE7, 0x01, 0xC8, 0x00, 0x05, 0x0A, 0x8A, 0x14, 0x64, 0x80, 0x14, 0x64, 0x80 });
                                dummyResponseList[0] = (byte)dummyResponseList.Count;
                            }
                            byte[] dummyResponse = dummyResponseList.ToArray();
                            activeResponse = new ResponseEntry(_receiveData, dummyResponse, null);
                            activeResponse.ResponseMultiList.Add(dummyResponse);
                            telBlockIndex = 0;
                        }
                        else if (_receiveData.Length >= 4 && _receiveData[0] >= 0x04 && _receiveData[2] == 0x2B)
                        {   // login
                            found = true;
                            Debug.WriteLine("Login");
                            byte[] dummyResponse = { 0x04, 0x00, 0x09, 0x0F };
                            activeResponse = new ResponseEntry(_receiveData, dummyResponse, null);
                            activeResponse.ResponseMultiList.Add(dummyResponse);
                            telBlockIndex = 0;
                        }
                        else if (_receiveData.Length >= 3 && _receiveData[0] == 0x03 && _receiveData[2] == 0x00)
                        {   // ECU identification
                            found = true;
                            Debug.WriteLine("Dummy identification");
                            activeResponse = identityResponse;
                            telBlockIndex = 0;
                        }
                    }
                    if (!found)
                    {
                        DebugLogData("Not found: ", _receiveData, recLength);
                        requestInvalid = true;
                    }
                }
            }
        }

        private void SerialConcept3Transmission()
        {
            byte wakeAddress;
            bool initOk;
            do
            {
                initOk = false;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                if (!ReceiveWakeUp(out wakeAddress))
                {
                    break;
                }
                Debug.WriteLine("Wake Address: {0:X02}", wakeAddress);
                byte[] configData = GetConfigData(wakeAddress);
                if (configData == null)
                {
                    Debug.WriteLine("Invalid wake address");
                    continue;
                }

                Thread.Sleep(60);  // maximum is 2200ms
                _sendData[0] = 0x55;
                SendData(_sendData, 0, 1);

                Thread.Sleep(5);   // maximum 200ms
                int sendLen = 0;
                if (configData.Length > 1)
                {
                    sendLen = configData.Length - 1;
                    Array.Copy(configData, 1, _sendData, 0, sendLen);
                }

                if (sendLen > 1)
                {
                    SendData(_sendData, 1, sendLen);
                    Thread.Sleep(10);
                    _serialPort.DataBits = 8;
                    _serialPort.Parity = Parity.Even;
                    //Thread.Sleep(10);     // max sum of both timeouts 2500ms
                    Thread.Sleep(200);
                }
                initOk = true;
            } while (!initOk);

            Debug.WriteLine("Init done");

            bool stopSend = false;
            for (;;)
            {
                if (stopSend)
                {
                    break;
                }

                foreach (ResponseEntry responseOnlyEntry in _configData.ResponseOnlyList)
                {
                    if (_stopThread)
                    {
                        stopSend = true;
                        break;
                    }
                    if (_serialPort.BytesToRead > 0)
                    {
                        Debug.WriteLine("Abort comm");
                        Thread.Sleep(100);
                        stopSend = true;
                        break;
                    }
                    if ((responseOnlyEntry.Config == null) || (responseOnlyEntry.Config.Length < 1) ||
                        (responseOnlyEntry.Config[0] != wakeAddress))
                    {
                        continue;
                    }
                    byte[] responseOnly = responseOnlyEntry.ResponseList[0];
                    int responseLen = responseOnly.Length;
                    if (responseLen > 0)
                    {
                        Array.Copy(responseOnly, _sendData, responseLen);
                        _sendData[responseLen - 1] = CalcChecksumXor(_sendData, responseLen - 1);
                        SendData(_sendData, responseLen);
                        // max interbyte timeout is 10ms
                        Thread.Sleep(200);  // min 150ms, max 2500ms (this time includes the send time of 50ms!)
                    }
                }
            }
        }

        private void SerialKwp2000Transmission()
        {
            byte wakeAddress = 0x00;
            byte[] keyBytes = { 0xEF, 0x8F };
            if (_conceptType == ConceptType.ConceptKwp2000)
            {
                bool initOk;
                do
                {
                    initOk = false;
                    if (!ReceiveWakeUp(out wakeAddress))
                    {
                        break;
                    }
                    Debug.WriteLine("Wake Address: {0:X02}", wakeAddress);
                    byte[] configData = GetConfigData(wakeAddress);
                    if (configData == null)
                    {
                        Debug.WriteLine("Invalid wake address");
                        continue;
                    }

                    Thread.Sleep(60); // W1: 60-300ms
                    _sendData[0] = 0x55;
                    SendData(_sendData, 0, 1);

                    Thread.Sleep(5); // W2: 5-20ms
                    if (configData.Length > 1)
                    {
                        if (configData.Length >= 3)
                        {
                            keyBytes[0] = configData[1];
                            keyBytes[1] = configData[2];
                        }
                    }
                    SendData(keyBytes, 0, keyBytes.Length);

                    if (ReceiveData(_receiveData, 0, 1, 50, 50)) // too fast for ELM
                    //if (ReceiveData(_receiveData, 0, 1, 200, 200))
                    {
                        if ((byte) (~_receiveData[0]) == keyBytes[1])
                        {
                            initOk = true;
                        }
                        else
                        {
                            Debug.WriteLine("Invalid init response {0}", (byte) (~_receiveData[0]));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("No init response");
                    }

                    if (initOk)
                    {
                        Thread.Sleep(25); // W4: 25-50ms
                        _sendData[0] = (byte) (~configData[0]);
                        SendData(_sendData, 0, 1);
                    }
                } while (!initOk);
                _nr2123SendCount = 0;

                Debug.WriteLine("Init done");
            }

            long lastRecTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                if (_stopThread)
                {
                    break;
                }
                if (!ObdReceive(_receiveData))
                {
                    if (_conceptType == ConceptType.ConceptKwp2000)
                    {
                        if ((Stopwatch.GetTimestamp() - lastRecTime) > 3000*TickResolMs)
                        {
                            Debug.WriteLine("Receive timeout");
                            break;
                        }
                        continue;
                    }
                    break;
                }
                lastRecTime = Stopwatch.GetTimestamp();
                int dataLength = _receiveData[0] & 0x3F;
                int dataOffset = 3;
                int recLength = dataLength;
                if (recLength == 0)
                {
                    // with length byte
                    if (_receiveData[3] == 0)
                    {
                        dataLength = (_receiveData[4] << 8) + _receiveData[5];
                        dataOffset = 6;
                        recLength = dataLength + 6;
                    }
                    else
                    {
                        dataLength = _receiveData[3];
                        dataOffset = 4;
                        recLength = dataLength + 4;
                    }
                }
                else
                {
                    recLength += 3;
                }
                recLength += 1; // checksum
#if true
                DebugLogData("Request: ", _receiveData, recLength);
#endif
                if (!_adsAdapter && !_klineResponder)
                {
                    // send echo
                    ObdSend(_receiveData);
                }

                bool standardResponse = false;
                if (_isoTpMode)
                {
                    if (
                        _receiveData[0] == 0x82 &&
                        _receiveData[3] == 0x3E)
                    {
                        Debug.WriteLine("Tester present");
                        if ((_receiveData[4] & 0x80) == 0x00)
                        {   // with response
                            int i = 0;
                            _sendData[i++] = _receiveData[0];
                            _sendData[i++] = _receiveData[1];
                            _sendData[i++] = _receiveData[2];
                            _sendData[i++] = (byte) (_receiveData[3] | 0x40);
                            _sendData[i++] = _receiveData[2];

                            ObdSend(_sendData);
                        }
                        standardResponse = true;
                    }
                }
                else
                {
                    if (
                        _receiveData[0] == 0x81 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x81)
                    {
                        // start communication service
                        int i = 0;
                        _sendData[i++] = 0x83;
                        _sendData[i++] = 0xF1;
                        _sendData[i++] = _receiveData[1];
                        _sendData[i++] = 0xC1;
                        _sendData[i++] = keyBytes[0]; // key low
                        _sendData[i++] = keyBytes[1]; // key high

                        ObdSend(_sendData);
                        Debug.WriteLine("Start communication");
                        standardResponse = true;
                    }
                    else if (
                        _receiveData[0] == 0x81 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x82)
                    {
                        // stop communication service
                        int i = 0;
                        _sendData[i++] = 0x81;
                        _sendData[i++] = 0xF1;
                        _sendData[i++] = _receiveData[1];
                        _sendData[i++] = 0xC2;

                        ObdSend(_sendData);
                        Debug.WriteLine("Stop communication");
                        standardResponse = true;
                        break;
                    }
                    else if (
                        _receiveData[0] == 0x81 &&
                        _receiveData[2] == 0xF1 &&
                        _receiveData[3] == 0x3E)
                    {
                        // tester present
                        int i = 0;
                        _sendData[i++] = 0x81;
                        _sendData[i++] = 0xF1;
                        _sendData[i++] = _receiveData[1];
                        _sendData[i++] = 0x7E;

                        ObdSend(_sendData);
                        Debug.WriteLine("Tester present");
                        standardResponse = true;
                    }
                }

                if (!standardResponse)
                {
                    bool found = false;
                    foreach (ResponseEntry responseEntry in _configData.ResponseList)
                    {
                        if (_conceptType == ConceptType.ConceptKwp2000)
                        {
                            if ((responseEntry.Config == null) || (responseEntry.Config.Length < 1) ||
                                (responseEntry.Config[0] != wakeAddress))
                            {
                                continue;
                            }
                        }
                        if (recLength != responseEntry.Request.Length) continue;
                        bool equal = true;
                        // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                        for (int i = 0; i < recLength - 1; i++)
                        {
                            // don't compare checksum
                            if (_receiveData[i] != responseEntry.Request[i])
                            {
                                equal = false;
                                break;
                            }
                        }
                        if (equal)
                        {
                            // entry found
                            found = true;
                            if (responseEntry.ResponseMultiList.Count > 1)
                            {
                                foreach (byte[] responseTel in responseEntry.ResponseMultiList)
                                {
                                    bool nr2123 = responseTel.Length == 7 && responseTel[3] == 0x7F && ((responseTel[5] == 0x21) || (responseTel[5] == 0x23));
                                    if (!nr2123 || (_nr2123SendCount < Kwp2000Nr2123Retries))
                                    {
#if false
                                        DebugLogData("Response: ", responseTel, responseTel.Length);
#endif
                                        ObdSend(responseTel);
                                        if (nr2123)
                                        {
                                            Debug.WriteLine("Send NR21/23: {0}", _nr2123SendCount);
                                            _nr2123SendCount++;
                                            break;
                                        }
                                    }
                                    _nr2123SendCount = 0;
#if false
                                    if (responseTel.Length == 7 && responseTel[3] == 0x7F && responseTel[5] == 0x78)
                                    {
                                        Debug.WriteLine("Delay NR78");
                                        Thread.Sleep(4000);
                                    }
#endif
                                }
                            }
                            else
                            {
                                byte[] responseTel = responseEntry.ResponseDyn;
#if false
                                DebugLogData("Response: ", responseTel, responseTel.Length);
#endif
                                ObdSend(responseTel);
                                _nr2123SendCount = 0;
                            }
                            lastRecTime = Stopwatch.GetTimestamp();
                            break;
                        }
                    }
                    if (!found)
                    {
                        if ((_conceptType == ConceptType.ConceptKwp2000) ||
                            (_conceptType == ConceptType.ConceptTp20))
                        {
                            if (_isoTpMode)
                            {
                                if (_receiveData.Length >= 4 && _receiveData[0] == 0x81 && _receiveData[3] == 0x04)
                                {   // Clear DTC ISO 15031-5
                                    found = true;
                                    Debug.WriteLine("Dummy service 04: Clear DTC");
                                    byte[] dummyResponse = { 0x81, _receiveData[1], _receiveData[2], 0x44 };
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 7 && _receiveData[0] == 0x84 && _receiveData[3] == 0x14 &&
                                         _receiveData[4] == 0xFF && _receiveData[5] == 0xFF && _receiveData[6] == 0xFF)
                                {   // Clear DTC
                                    found = true;
                                    Debug.WriteLine("Dummy service 14: Clear DTC");
                                    byte[] dummyResponse = { 0x81, _receiveData[1], _receiveData[2], 0x54 };
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && _receiveData[0] == 0x83 && _receiveData[3] == 0x22)
                                {   // service 22
                                    found = true;
                                    Debug.WriteLine("Dummy service 22: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);

                                    //bool simulateData = false;
                                    bool simulateData = true;
                                    int identifer = (_receiveData[4] << 8) + _receiveData[5];
                                    if (identifer >= 0x0600 && identifer <= 0x06FF)
                                    {
                                        simulateData = false;
                                    }
                                    if (identifer >= 0x6000 && identifer <= 0x6FFF)
                                    {
                                        simulateData = false;
                                    }
                                    if (identifer >= 0xF100 && identifer <= 0xF1FF)
                                    {
                                        simulateData = false;
                                    }

                                    byte[] dummyResponse;
                                    if (simulateData)
                                    {
                                        dummyResponse = new byte[] { 0x80, _receiveData[1], _receiveData[2], 0x26, 0x62, _receiveData[4], _receiveData[5],
                                            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x00 };   // dummy string response
                                    }
                                    else
                                    {
                                        dummyResponse = new byte[] { 0x83, _receiveData[1], _receiveData[2], 0x7F, _receiveData[3], 0x31, 0x00 };   // request out of range
                                    }
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x2E)
                                {   // service 2E
                                    found = true;
                                    Debug.WriteLine("Dummy service 2E: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);
                                    byte[] dummyResponse = { 0x83, _receiveData[1], _receiveData[2], 0x6E, _receiveData[4], _receiveData[5], 0x00 };   // positive write ACK

                                    if (recLength > 3)
                                    {
                                        if (_receiveData[4] == 0xF1 && _receiveData[5] == 0x98)
                                        {
                                            Debug.WriteLine("Map RepairShopCode 0xF198 -> 0xF1A5");
                                            _receiveData[4] = 0xF1;
                                            _receiveData[5] = 0xA5;
                                        }

                                        for (int retry = 0; retry < 2; retry++)
                                        {
                                            bool entryFound = false;
                                            foreach (ResponseEntry responseEntry in _configData.ResponseList)
                                            {
                                                if (responseEntry.Request.Length == 7 && responseEntry.Request[1] == _receiveData[1] && responseEntry.Request[2] == _receiveData[2] &&
                                                    responseEntry.Request[3] == 0x22 && responseEntry.Request[4] == _receiveData[4] && responseEntry.Request[5] == _receiveData[5])
                                                {
                                                    Debug.WriteLine("Found service 22 read entry");
                                                    entryFound = true;
                                                    foreach (byte[] responseTel in responseEntry.ResponseList)
                                                    {
                                                        if (responseTel[0] == _receiveData[0] && responseTel[3] == 0x62 &&
                                                            responseTel[4] == _receiveData[4] && responseTel[5] == _receiveData[5] && recLength == responseTel.Length)
                                                        {
                                                            Debug.WriteLine("Updating read entry dyn");
                                                            Array.Copy(_receiveData, dataOffset + 3, responseTel, dataOffset + 3, dataLength - 3);
                                                        }
                                                    }

                                                    foreach (byte[] responseTel in responseEntry.ResponseMultiList)
                                                    {
                                                        if (responseTel[0] == _receiveData[0] && responseTel[3] == 0x62 &&
                                                            responseTel[4] == _receiveData[4] && responseTel[5] == _receiveData[5] && recLength == responseTel.Length)
                                                        {
                                                            Debug.WriteLine("Updating read response multi");
                                                            Array.Copy(_receiveData, dataOffset + 3, responseTel, dataOffset + 3, dataLength - 3);
                                                        }
                                                    }
                                                }
                                            }

                                            if (entryFound)
                                            {
                                                break;
                                            }

                                            Debug.WriteLine("Generating service 22 read entry");
                                            byte[] requestData = { 0x83, _receiveData[1], _receiveData[2], 0x22, _receiveData[4], _receiveData[5], 0x00 };
                                            byte[] responseData = new byte[recLength];
                                            Array.Copy(_receiveData, responseData, responseData.Length);
                                            responseData[3] = 0x62;

                                            byte[] configData = null;
                                            foreach (ResponseEntry responseEntry in _configData.ResponseList)
                                            {
                                                if (responseEntry.Request[1] == _receiveData[1] && responseEntry.Request[2] == _receiveData[2])
                                                {
                                                    Debug.WriteLine("Found config data");
                                                    configData = responseEntry.Config;
                                                    break;
                                                }
                                            }

                                            _configData.ResponseList.Add(new ResponseEntry(requestData, responseData, configData));
                                        }
                                    }

                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x3B)
                                {   // service 3B
                                    found = true;
                                    Debug.WriteLine("Dummy service 3B: {0:X02}", _receiveData[4]);
#if true
                                    byte[] dummyResponse = { 0x82, _receiveData[1], _receiveData[2], 0x7B, _receiveData[4], 0x00 };   // positive write ACK
#else
                                    byte[] dummyResponse = { 0x83, _receiveData[1], _receiveData[2], 0x7F, _receiveData[3], 0x33, 0x00 };   // access denied
#endif
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && _receiveData[0] == 0x83 && _receiveData[3] == 0x09)
                                {   // service 09
                                    found = true;
                                    Debug.WriteLine("Dummy service 09: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);
                                    byte[] dummyResponse = { 0x83, _receiveData[1], _receiveData[2], 0x7F, _receiveData[3], 0x12, 0x00 };   // sub function not supported
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && _receiveData[0] == 0x82 && _receiveData[3] == 0x11)
                                {   // service 11
                                    found = true;
                                    Debug.WriteLine("Request reset: {0:X02}", _receiveData[4]);
                                    byte[] dummyResponse = { 0x82, _receiveData[1], _receiveData[2], 0x51, _receiveData[4], 0x00 };   // positive ACK
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x27)
                                {   // service 27 (security access)
                                    found = true;
                                    if (_receiveData[0] == 0x82)
                                    {
                                        Debug.WriteLine("Request seed accmode: {0:X02}", _receiveData[4]);
                                        byte[] dummyResponse = { 0x86, _receiveData[1], _receiveData[2], 0x67, _receiveData[4], 0x12, 0x34, 0x56, 0x78, 0x00 };   // send seed
                                        ObdSend(dummyResponse);
                                    }
                                    else
                                    {
                                        if (_receiveData[0] == 0x86)
                                        {
                                            Debug.WriteLine("Receive key accmode: {0:X02}, key: {1:X02} {2:X02} {3:X02} {4:X02}",
                                                _receiveData[4], _receiveData[5], _receiveData[6], _receiveData[7], _receiveData[8]);
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Dummy service 27: {0:X02}", _receiveData[4]);
                                        }
                                        byte[] dummyResponse = { 0x82, _receiveData[1], _receiveData[2], 0x67, _receiveData[4], 0x00 };   // positive ACK
                                        ObdSend(dummyResponse);
                                    }
                                }
                            }
                            else
                            {   // No ISO TP mode
                                if (_receiveData.Length >= 5 && _receiveData[0] == 0x82 && _receiveData[3] == 0x21)
                                {   // read mwblock
                                    found = true;
                                    Debug.WriteLine("Dummy mwblock: {0:X02}", _receiveData[4]);
                                    byte[] dummyResponse = { 0x9A, _receiveData[2], _receiveData[1], 0x61, _receiveData[4],
                                        0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x25, 0x00, 0x00, 0x00 };
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && _receiveData[0] == 0x83 && _receiveData[3] == 0x22)
                                {   // service 22
                                    found = true;
                                    Debug.WriteLine("Dummy service 22: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);
                                    byte[] dummyResponse = { 0x83, _receiveData[2], _receiveData[1], 0x7F, _receiveData[3], 0x31, 0x00 };   // request out of range
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x2E)
                                {   // service 2E
                                    found = true;
                                    Debug.WriteLine("Dummy service 2E: {0:X02}{1:X02}", _receiveData[4], _receiveData[5]);
#if true
                                    byte[] dummyResponse = { 0x83, _receiveData[2], _receiveData[1], 0x6E, _receiveData[4], _receiveData[5], 0x00 };   // positive write ACK
#else
                                    byte[] dummyResponse = { 0x83, _receiveData[2], _receiveData[1], 0x7F, _receiveData[3], 0x33, 0x00 };   // access denied
#endif
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x3B)
                                {   // service 3B
                                    found = true;
                                    if (_receiveData[4] == 0x9A && _receiveData[0] == 0x90)
                                    {
                                        if ((_receiveData[16] & 0x80) == 0x00)
                                        {
                                            Debug.WriteLine("Parameter coding");
                                            byte[] lastCoding = new byte[3];
                                            Array.Copy(_receiveData, 16, lastCoding, 0, lastCoding.Length);
                                            byte[] lastRepairShop = new byte[6];
                                            Array.Copy(_receiveData, 5, lastRepairShop, 0, lastRepairShop.Length);

                                            foreach (ResponseEntry responseEntry in _configData.ResponseList)
                                            {
                                                if (responseEntry.Request.Length == 6 && responseEntry.Request[1] == _receiveData[1] && responseEntry.Request[2] == _receiveData[2] &&
                                                    responseEntry.Request[3] == 0x1A && responseEntry.Request[4] == 0x9B)
                                                {
                                                    Debug.WriteLine("Found read coding");
                                                    foreach (byte[] responseTel in responseEntry.ResponseList)
                                                    {
                                                        if (responseTel[3] == 0x5A && responseTel[4] == 0x9B && responseTel.Length > 22 + 3)
                                                        {
                                                            Debug.WriteLine("Updating coding response dyn");
                                                            Array.Copy(lastCoding, 0, responseTel, 22, lastCoding.Length);
                                                            Array.Copy(lastRepairShop, 0, responseTel, 25, lastRepairShop.Length);
                                                        }
                                                    }

                                                    foreach (byte[] responseTel in responseEntry.ResponseMultiList)
                                                    {
                                                        if (responseTel[3] == 0x5A && responseTel[4] == 0x9B && responseTel.Length > 22 + 3)
                                                        {
                                                            Debug.WriteLine("Updating coding response multi");
                                                            Array.Copy(lastCoding, 0, responseTel, 22, lastCoding.Length);
                                                            Array.Copy(lastRepairShop, 0, responseTel, 25, lastRepairShop.Length);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Parameter coding 2");
                                        }
                                    }
                                    else
                                    {
                                        if (_receiveData[4] == 0x9A && recLength > 17 + 2)
                                        {
                                            Debug.WriteLine("Parameter long coding");
                                            foreach (ResponseEntry responseEntry in _configData.ResponseList)
                                            {
                                                if (responseEntry.Request.Length == 6 && responseEntry.Request[1] == _receiveData[1] && responseEntry.Request[2] == _receiveData[2] &&
                                                    responseEntry.Request[3] == 0x1A && responseEntry.Request[4] == 0x9A)
                                                {
                                                    Debug.WriteLine("Found read long coding");
                                                    foreach (byte[] responseTel in responseEntry.ResponseList)
                                                    {
                                                        if (responseTel[3] == 0x5A && responseTel[4] == 0x9A && responseTel.Length == recLength)
                                                        {
                                                            Debug.WriteLine("Updating long coding response dyn");
                                                            Array.Copy(_receiveData, 5, responseTel, 5, recLength - 6);
                                                        }
                                                    }

                                                    foreach (byte[] responseTel in responseEntry.ResponseMultiList)
                                                    {
                                                        if (responseTel[3] == 0x5A && responseTel[4] == 0x9A && responseTel.Length == recLength)
                                                        {
                                                            Debug.WriteLine("Updating long coding response multi");
                                                            Array.Copy(_receiveData, 5, responseTel, 5, recLength - 6);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Dummy service 3B: {0:X02}", _receiveData[4]);
                                        }
                                    }
#if true
                                    byte[] dummyResponse = { 0x82, _receiveData[2], _receiveData[1], 0x7B, _receiveData[4], 0x00 };   // positive write ACK
#else
                                    byte[] dummyResponse = { 0x83, _receiveData[2], _receiveData[1], 0x7F, _receiveData[3], 0x33, 0x00 };   // access denied
#endif
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x1A)
                                {   // service 1A
                                    found = true;
                                    Debug.WriteLine("Dummy service 1A: {0:X02}", _receiveData[4]);
                                    byte[] dummyResponse = { 0x83, _receiveData[2], _receiveData[1], 0x7F, _receiveData[3], 0x11, 0x00 };   // not supported
                                    ObdSend(dummyResponse);
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && _receiveData[3] == 0x27)
                                {   // service 27 (security access)
                                    found = true;
                                    if (_receiveData[0] == 0x82)
                                    {
                                        Debug.WriteLine("Request seed accmode: {0:X02}", _receiveData[4]);
                                        byte[] dummyResponse = { 0x86, _receiveData[2], _receiveData[1], 0x67, _receiveData[4], 0x12, 0x34, 0x56, 0x78, 0x00 };   // send seed
                                        ObdSend(dummyResponse);
                                    }
                                    else
                                    {
                                        if (_receiveData[0] == 0x86)
                                        {
                                            Debug.WriteLine("Receive key accmode: {0:X02}, key: {1:X02} {2:X02} {3:X02} {4:X02}",
                                                _receiveData[4], _receiveData[5], _receiveData[6], _receiveData[7], _receiveData[8]);
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Dummy service 27: {0:X02}", _receiveData[4]);
                                        }
                                        byte[] dummyResponse = { 0x82, _receiveData[2], _receiveData[1], 0x67, _receiveData[4], 0x00 };   // positive ACK
                                        ObdSend(dummyResponse);
                                    }
                                }
                                else if (_receiveData.Length >= 6 && (_receiveData[0] & 0x80) == 0x80 && (_receiveData[3] == 0x31 || _receiveData[3] == 0x32))
                                {   // service 31 (StartRoutineByLocalIdentifier), service 32 (StopRoutineByLocalIdentifier)
                                    // 0x01 ANP_NICHT_EINGREIFEN
                                    // 0x02 ANP_WERT_BESTAETIGEN
                                    // 0x04 ANP_ABBRUCH
                                    // 0x05-0x7F ANP_ENDE
                                    // 0x8X ANP_WERT_EINGEBEN
                                    if (_receiveData[5] == 0x01 && _receiveData[6] == 0x03)
                                    {
                                        found = true;
                                        byte[] dummyResponse = { 0x8E, _receiveData[2], _receiveData[1], (byte)(_receiveData[3] | 0x40), _receiveData[4],
                                            0x01, 0x03, 0x01, 0x04, (byte)(_kwp2000AdaptionValue >> 8), (byte)_kwp2000AdaptionValue, 0x01, 0x14, 0x01, 0x06, 0x01, 0x08, 0x00 };
                                        if (_receiveData[3] == 0x31)
                                        {
                                            Debug.WriteLine("Start routine short");
                                            if (_receiveData[4] == 0xB8)
                                            {
                                                Debug.WriteLine("Reset adaption channel");
                                                _kwp2000AdaptionChannel = -1;
                                                _kwp2000AdaptionStatus = 0x81;  // ANP_WERT_EINGEBEN
                                            }
                                            if (_receiveData[4] == 0xB9)
                                            {
                                                if (recLength == 9)
                                                {
                                                    _kwp2000AdaptionChannel = _receiveData[7];
                                                    Debug.WriteLine("Set adaption channel: {0:X02}", _kwp2000AdaptionChannel);
                                                    _kwp2000AdaptionStatus = 0x82;  // ANP_WERT_EINGEBEN
                                                }
                                                else if (recLength > 9)
                                                {
                                                    if (_kwp2000AdaptionChannel != ResetAdaptionChannel)
                                                    {
                                                        _kwp2000AdaptionValue = (_receiveData[7] << 8) + _receiveData[8];
                                                    }
                                                    Debug.WriteLine("Write adaption channel value: {0:X04}", _kwp2000AdaptionValue);
                                                    _kwp2000AdaptionStatus = 0x82;  // ANP_WERT_EINGEBEN
                                                }
                                            }
                                            else if (_receiveData[4] == 0xBA)
                                            {
                                                Debug.WriteLine("Read adaption channel status: {0:X02}", _kwp2000AdaptionStatus);
                                                dummyResponse[7] = _kwp2000AdaptionStatus;
                                            }
                                            else if (_receiveData[4] == 0xBB)
                                            {
                                                if (recLength > 9)
                                                {
                                                    if (_kwp2000AdaptionChannel == ResetAdaptionChannel)
                                                    {
                                                        _kwp2000AdaptionValue = DefaultAdaptionChannelValue;
                                                        Debug.WriteLine("Store adaption channel value default");
                                                    }
                                                    else
                                                    {
                                                        Debug.WriteLine("Store adaption channel value: {0:X02}{1:X02}", _receiveData[7], _receiveData[8]);
                                                    }
                                                    _kwp2000AdaptionStatus = 0x05;  // ANP_ENDE
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Stop routine short");
                                            if (_receiveData[4] == 0xB8)
                                            {
                                                Debug.WriteLine("Close adaption channel");
                                                _kwp2000AdaptionStatus = 0x01;  // ANP_NICHT_EINGREIFEN
                                            }
                                        }
                                        ObdSend(dummyResponse);
                                    }
                                    else if (_receiveData[5] == 0x01 && (_receiveData[6] == 0x0A || _receiveData[6] == 0x13))
                                    {
                                        found = true;
                                        List<byte> dummyResponseList = new List<byte>
                                        {
                                            0x8D, _receiveData[2], _receiveData[1], (byte)(_receiveData[3] | 0x40), _receiveData[4],
                                            0x01, 0x04, 0x01, 0x06, 0x01, 0x01, 0x14, 0x01, 0x06, 0x01, 0x08, 0x00
                                        };
                                        if (_receiveData[3] == 0x31)
                                        {
                                            Debug.WriteLine("Start routine long");
                                            if (_receiveData[4] == 0xB8)
                                            {
                                                Debug.WriteLine("Reset adaption channel");
                                                _kwp2000AdaptionChannel = -1;
                                                _kwp2000AdaptionStatus = 0x81;  // ANP_WERT_EINGEBEN
                                            }
                                            if (_receiveData[4] == 0xB9)
                                            {
                                                if (recLength == 9)
                                                {
                                                    _kwp2000AdaptionChannel = _receiveData[7];
                                                    Debug.WriteLine("Set adaption channel: {0:X02}", _kwp2000AdaptionChannel);
                                                    _kwp2000AdaptionStatus = 0x82;  // ANP_WERT_EINGEBEN
                                                }
                                                else if (recLength > 9)
                                                {
                                                    if (_kwp2000AdaptionChannel != ResetAdaptionChannel)
                                                    {
                                                        _kwp2000AdaptionValue = (_receiveData[7] << 8) + _receiveData[8];
                                                    }
                                                    Debug.WriteLine("Write adaption channel value: {0:X04}", _kwp2000AdaptionValue);
                                                    _kwp2000AdaptionStatus = 0x02;  // ANP_WERT_BESTAETIGEN
                                                }
                                            }
                                            else if (_receiveData[4] == 0xBA)
                                            {
                                                Debug.WriteLine("Read adaption channel status: {0:X02}, channel: {1}", _kwp2000AdaptionStatus, _kwp2000AdaptionChannel);
                                                if (_kwp2000AdaptionChannel < 0)
                                                {
                                                    dummyResponseList[7] = _kwp2000AdaptionStatus;
                                                }
                                                else
                                                {
                                                    dummyResponseList[7] = (byte) _kwp2000AdaptionChannel;
                                                    dummyResponseList[8] = _kwp2000AdaptionStatus;

                                                    Debug.WriteLine("Inserting adaption value: {0:X04}", _kwp2000AdaptionValue);
                                                    dummyResponseList[0] += 2;
                                                    dummyResponseList[9] = 0x03;
                                                    dummyResponseList.Insert(10, (byte)(_kwp2000AdaptionValue >> 8));
                                                    dummyResponseList.Insert(11, (byte)_kwp2000AdaptionValue);
                                                }
                                            }
                                            else if (_receiveData[4] == 0xBB)
                                            {
                                                if (recLength > 9)
                                                {
                                                    if (_kwp2000AdaptionChannel == ResetAdaptionChannel)
                                                    {
                                                        _kwp2000AdaptionValue = DefaultAdaptionChannelValue;
                                                        Debug.WriteLine("Store adaption channel value default");
                                                    }
                                                    else
                                                    {
                                                        Debug.WriteLine("Store adaption channel value: {0:X02}{1:X02}", _receiveData[7], _receiveData[8]);
                                                    }
                                                    _kwp2000AdaptionStatus = 0x05;  // ANP_ENDE
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Stop routine long");
                                            if (_receiveData[4] == 0xB8)
                                            {
                                                Debug.WriteLine("Close adaption channel");
                                                _kwp2000AdaptionChannel = -1;
                                                _kwp2000AdaptionStatus = 0x01;  // ANP_NICHT_EINGREIFEN
                                            }
                                        }
                                        ObdSend(dummyResponseList.ToArray());
                                    }
                                }
                            }
                        }
                    }
                    if (!found)
                    {
                        DebugLogData("Not found: ", _receiveData, recLength);
                    }
                }
            }
        }

        private int GetResposeLength(string sgdbFile, int serviceId)
        {
            SgFunctions sgFunctions = _form.sgFunctions;
            if (!string.IsNullOrEmpty(sgdbFile))
            {
                try
                {
                    string newSgdb = Path.GetFileNameWithoutExtension(sgdbFile);
                    string currentSgdb = Path.GetFileNameWithoutExtension(_form.ediabas.SgbdFileName);
                    if (string.Compare(currentSgdb, newSgdb, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        _form.ediabas.ResolveSgbdFile(sgdbFile);
                        sgFunctions.ResetCache();
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            int resLength = 0;
            List<SgFunctions.SgFuncInfo> sgFuncInfoList = sgFunctions.ReadSgFuncTable();
            if (sgFuncInfoList != null)
            {
                foreach (SgFunctions.SgFuncInfo funcInfo in sgFuncInfoList)
                {
                    Int64 idValue = EdiabasNet.StringToValue(funcInfo.Id, out bool valid);
                    if (!valid || idValue != serviceId)
                    {
                        continue;
                    }

                    if (funcInfo.ServiceList != null &&
                        funcInfo.ServiceList.Contains((int) SgFunctions.UdsServiceId.ReadDataById))
                    {
                        if (funcInfo.ResInfoList != null)
                        {
                            //argFuncInfo.Id
                            foreach (SgFunctions.SgFuncNameInfo funcNameInfo in funcInfo.ResInfoList)
                            {
                                if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                                {
                                    if (funcBitFieldInfo.TableDataType == SgFunctions.TableDataType.Bit &&
                                        funcBitFieldInfo.NameInfoList != null)
                                    {
                                        foreach (SgFunctions.SgFuncNameInfo nameInfo in funcBitFieldInfo.NameInfoList)
                                        {
                                            if (nameInfo is SgFunctions.SgFuncBitFieldInfo nameInfoBitField)
                                            {
                                                if (!string.IsNullOrEmpty(nameInfoBitField.ResultName))
                                                {
                                                    if (nameInfoBitField.Length.HasValue)
                                                    {
                                                        resLength += nameInfoBitField.Length.Value;
                                                        // add only once, fields overlap
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(funcBitFieldInfo.ResultName))
                                        {
                                            if (funcBitFieldInfo.Length.HasValue)
                                            {
                                                resLength += funcBitFieldInfo.Length.Value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(funcInfo.Result))
                            {
                                if (funcInfo.Length.HasValue)
                                {
                                    resLength += funcInfo.Length.Value;
                                }
                            }
                        }
                    }
                }
            }

            return resLength;
        }

        private void DebugLogData(string message, byte[] data, int length)
        {
            StringBuilder sr = new StringBuilder();
            sr.Append(message);
            if (length <= 0)
            {
                sr.Append("No data");
            }

            if (data != null)
            {
                for (int i = 0; i < length; i++)
                {
                    sr.Append(string.Format("{0:X02} ", data[i]));
                }
                Debug.WriteLine(sr.ToString());
            }
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
                    StopThread();
                    Disconnect();
                    _serialReceiveEvent?.Dispose();
                    _pcanReceiveEvent?.Dispose();
                    _serialPort?.Dispose();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}
