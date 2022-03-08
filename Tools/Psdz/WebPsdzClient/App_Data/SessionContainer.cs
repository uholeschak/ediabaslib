//#define EDIABAS_CONNECTION
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using EdiabasLib;
using log4net;
using Microsoft.AspNet.SignalR;
using PsdzClient;
using PsdzClient.Programing;

namespace WebPsdzClient.App_Data
{
    public class SessionContainer : IDisposable
    {
        private class Nr78Data
        {
            public Nr78Data(byte addr, byte[] nr78Tel, long firstDelay = Nr78Delay)
            {
                Addr = addr;
                Nr78Tel = nr78Tel;
                Count = 0;
                LastTcpSendTick = Stopwatch.GetTimestamp();
                FirstDelay = firstDelay;
            }

            public long GetDelay()
            {
                return Count == 0 ? FirstDelay : Nr78Delay;
            }

            public byte Addr;
            public byte[] Nr78Tel;
            public int Count;
            public long LastTcpSendTick;
            public long FirstDelay;
        }

        private class EnetTcpClientData
        {
            public EnetTcpClientData(EnetTcpChannel enetTcpChannel, int index)
            {
                EnetTcpChannel = enetTcpChannel;
                Index = index;
                TcpClientConnection = null;
                TcpClientStream = null;
                ConnectFailure = false;
                DataBuffer = new byte[0x200];
                RecQueue = new Queue<byte>();
                SendQueue = new Queue<byte>();
                RecPacketQueue = new Queue<byte[]>();
                Nr78Dict = new Dictionary<byte, Nr78Data>();
                LastTcpRecTick = DateTime.MinValue.Ticks;
            }

            public readonly EnetTcpChannel EnetTcpChannel;
            public readonly int Index;
            public TcpClient TcpClientConnection;
            public NetworkStream TcpClientStream;
            public bool ConnectFailure;
            public byte[] DataBuffer;
            public Queue<byte> RecQueue;
            public Queue<byte> SendQueue;
            public Queue<byte[]> RecPacketQueue;
            public Dictionary<byte, Nr78Data> Nr78Dict;
            public long LastTcpRecTick;
        }

        private class EnetTcpChannel
        {
            public EnetTcpChannel(bool control)
            {
                Control = control;
                ServerPort = 0;
                TcpClientList = new List<EnetTcpClientData>();
                RecEvent = new AutoResetEvent(false);
                SendEvent = new AutoResetEvent(false);
                for (int i = 0; i < 10; i++)
                {
                    TcpClientList.Add(new EnetTcpClientData(this, i));
                }
            }

            public bool Control;
            public int ServerPort;
            public TcpListener TcpServer;
            public readonly List<EnetTcpClientData> TcpClientList;
            public AutoResetEvent RecEvent;
            public AutoResetEvent SendEvent;
        }

        public delegate bool VehicleResponseDelegate();
        public string SessionId { get; }
        public ProgrammingJobs ProgrammingJobs { get; private set; }
        public bool RefreshOptions { get; set; }
        public AutoResetEvent MessageWaitEvent { get; private set; } = new AutoResetEvent(false);

        private bool _taskActive;
        public bool TaskActive
        {
            get
            {
                lock (_lockObject)
                {
                    return _taskActive;
                }
            }
            private set
            {
                lock (_lockObject)
                {
                    _taskActive = value;
                }

                if (value)
                {
                    UpdateProgress(0, true);
                }
                else
                {
                    UpdateProgress(0, false);
                }
            }
        }

        private string _statusText;
        public string StatusText
        {
            get
            {
                lock (_lockObject)
                {
                    return _statusText;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _statusText = value;
                }
            }
        }

        public void AppendStatusTextLine(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                lock (_lockObject)
                {
                    _statusText += line + Environment.NewLine;
                }
            }
        }

        private string _progressText;
        public string ProgressText
        {
            get
            {
                lock (_lockObject)
                {
                    return _progressText;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _progressText = value;
                }
            }
        }

        private CancellationTokenSource _cts;
        public CancellationTokenSource Cts
        {
            get
            {
                lock (_lockObject)
                {
                    return _cts;
                }
            }

            private set
            {
                lock (_lockObject)
                {
                    if (_cts != null)
                    {
                        _cts.Dispose();
                    }
                    _cts = value;
                }
            }

        }

        private Dictionary<PdszDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> _optionsDict;
        public Dictionary<PdszDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> OptionsDict
        {
            get
            {
                lock (_lockObject)
                {
                    return _optionsDict;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _optionsDict = value;
                }
            }
        }

        private PdszDatabase.SwiRegisterEnum? _selectedSwiRegister;
        public PdszDatabase.SwiRegisterEnum? SelectedSwiRegister
        {
            get
            {
                lock (_lockObject)
                {
                    return _selectedSwiRegister;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _selectedSwiRegister = value;
                }
            }
        }

        private string _showMessageModal;
        public string ShowMessageModal
        {
            get
            {
                lock (_lockObject)
                {
                    return _showMessageModal;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _showMessageModal = value;
                }
            }
        }

        private int _showMessageModalCount;
        public int ShowMessageModalCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _showMessageModalCount;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _showMessageModalCount = value;
                }
            }
        }

        private bool _showMessageModalOkBtn;
        public bool ShowMessageModalOkBtn
        {
            get
            {
                lock (_lockObject)
                {
                    return _showMessageModalOkBtn;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _showMessageModalOkBtn = value;
                }
            }
        }

        private bool _showMessageModalWait;
        public bool ShowMessageModalWait
        {
            get
            {
                lock (_lockObject)
                {
                    return _showMessageModalWait;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _showMessageModalWait = value;
                }
            }
        }

        private bool _showMessageModalResult;
        public bool ShowMessageModalResult
        {
            get
            {
                lock (_lockObject)
                {
                    return _showMessageModalResult;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _showMessageModalResult = value;
                }
            }
        }

        private Int64 _deepObdVersion;
        public Int64 DeepObdVersion
        {
            get
            {
                lock (_lockObject)
                {
                    return _deepObdVersion;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _deepObdVersion = value;
                }
            }
        }

        private string _appId;
        public string AppId
        {
            get
            {
                lock (_lockObject)
                {
                    return _appId;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _appId = value;
                }
            }
        }

        private string _adapterSerial;
        public string AdapterSerial
        {
            get
            {
                lock (_lockObject)
                {
                    return _adapterSerial;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _adapterSerial = value;
                }
            }
        }

        private bool _adapterSerialValid;
        public bool AdapterSerialValid
        {
            get
            {
                lock (_lockObject)
                {
                    return _adapterSerialValid;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _adapterSerialValid = value;
                }
            }
        }

        public void SetLanguage(string language)
        {
            List<string> langList = PdszDatabase.EcuTranslation.GetLanguages();
            bool matched = false;

            foreach (string lang in langList)
            {
                if (language.StartsWith(lang, StringComparison.OrdinalIgnoreCase))
                {
                    ProgrammingJobs.ClientContext.Language = lang;
                    matched = true;
                    log.InfoFormat("SetLanguage matched: {0}", lang);
                    break;
                }
            }

            if (!matched)
            {
                log.ErrorFormat("SetLanguage Language not found: {0}", language);
            }
        }

        public string GetLanguage()
        {
            return ProgrammingJobs.ClientContext.Language;
        }

        public string GetActiveVin()
        {
            string vin = null;
            lock (_lockObject)
            {
                if (_taskActive)
                {
                    vin = ProgrammingJobs?.PsdzContext?.DetectVehicle?.Vin;
                }
            }

            return vin;
        }

        private UInt64 _packetId;
        private void ResetPacketId()
        {
            lock (_lockObject)
            {
                _packetId = 0;
            }
        }

        private string GetPacketId()
        {
            lock (_lockObject)
            {
                return _packetId.ToString(CultureInfo.InvariantCulture);
            }
        }

        private string GetNextPacketId()
        {
            lock (_lockObject)
            {
                _packetId++;
                return _packetId.ToString(CultureInfo.InvariantCulture);
            }
        }

        private List<EnetTcpChannel> _enetTcpChannels = new List<EnetTcpChannel>();
        private Thread _tcpThread;
        private Thread _vehicleThread;
        private bool _stopThread;
        private AutoResetEvent _tcpThreadWakeEvent = new AutoResetEvent(false);
        private AutoResetEvent _vehicleThreadWakeEvent = new AutoResetEvent(false);
        private Queue<PsdzVehicleHub.VehicleResponse> _vehicleResponses = new Queue<PsdzVehicleHub.VehicleResponse>();
        private Dictionary<string, List<string>> _vehicleResponseDict = new Dictionary<string, List<string>>();
#if EDIABAS_CONNECTION
        private EdiabasNet _ediabas;
#endif
        private bool _disposed;
        private readonly object _lockObject = new object();
        private readonly object _vehicleLogObject = new object();
        private FileStream _fsVehicleLog;
        private static readonly ILog log = LogManager.GetLogger(typeof(_Default));
        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private static readonly List<SessionContainer> SessionContainers = new List<SessionContainer>();

        private const int TcpSendBufferSize = 1400;
        private const int TcpSendTimeout = 5000;
        private const int TcpTesterAddr = 0xF4;
        private const int VehicleReceiveTimeout = 25000;
        private const long Nr78Delay = 1000;
        private const long Nr78RetryMax = VehicleReceiveTimeout / Nr78Delay;
        private const int ThreadFinishTimeout = VehicleReceiveTimeout + 5000;

        public SessionContainer(string sessionId, string dealerId)
        {
            SessionId = sessionId;
            ProgrammingJobs = new ProgrammingJobs(dealerId);
            ProgrammingJobs.UpdateStatusEvent += UpdateStatus;
            ProgrammingJobs.ProgressEvent += UpdateProgress;
            ProgrammingJobs.UpdateOptionsEvent += UpdateOptions;
            ProgrammingJobs.ShowMessageEvent += ShowMessageEvent;
            StatusText = string.Empty;
            ProgressText = string.Empty;

#if EDIABAS_CONNECTION
            EdInterfaceEnet edInterfaceEnet = new EdInterfaceEnet(false);
            _ediabas = new EdiabasNet
            {
                EdInterfaceClass = edInterfaceEnet,
                AbortJobFunc = AbortEdiabasJob
            };
            edInterfaceEnet.RemoteHost = "127.0.0.1";
            edInterfaceEnet.IcomAllocate = false;
#endif
            lock (SessionContainers)
            {
                SessionContainers.Add(this);
            }
        }

        public static SessionContainer GetSessionContainer(string sessionId)
        {
            lock (SessionContainers)
            {
                foreach (SessionContainer sessionContainer in SessionContainers)
                {
                    if (string.Compare(sessionId, sessionContainer.SessionId, StringComparison.Ordinal) == 0)
                    {
                        return sessionContainer;
                    }
                }
            }

            return null;
        }

        public static int GetSessionContainerCount()
        {
            lock (SessionContainers)
            {
                return SessionContainers.Count;
            }
        }

        public static bool IsVinActive(string vin, SessionContainer excludeContainer = null)
        {
            lock (SessionContainers)
            {
                foreach (SessionContainer sessionContainer in SessionContainers)
                {
                    if (excludeContainer != null && sessionContainer == excludeContainer)
                    {
                        continue;
                    }

                    string activeVin = sessionContainer.GetActiveVin();
                    if (!string.IsNullOrEmpty(activeVin))
                    {
                        if (string.Compare(vin, activeVin, StringComparison.Ordinal) == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void SetLogInfo(string sessionId)
        {
            LogicalThreadContext.Properties["session"] = sessionId;
        }

        private bool StartTcpListener()
        {
            try
            {
                StopTcpListener();

                if (_enetTcpChannels.Count == 0)
                {
                    _enetTcpChannels.Add(new EnetTcpChannel(false));
                    _enetTcpChannels.Add(new EnetTcpChannel(true));
                }

                foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
                {
                    if (enetTcpChannel.TcpServer == null)
                    {
                        enetTcpChannel.ServerPort = 0;
                        enetTcpChannel.TcpServer = new TcpListener(IPAddress.Loopback, 0);
                        enetTcpChannel.TcpServer.Start();
                        IPEndPoint ipEndPoint = enetTcpChannel.TcpServer.LocalEndpoint as IPEndPoint;
                        if (ipEndPoint != null)
                        {
                            enetTcpChannel.ServerPort = ipEndPoint.Port;
                        }

                        log.InfoFormat("StartTcpListener Port: {0}, Control: {1}", enetTcpChannel.ServerPort, enetTcpChannel.Control);
                    }
                }

                if (_vehicleThread != null)
                {
                    if (!_vehicleThread.IsAlive)
                    {
                        _vehicleThread = null;
                    }
                }

                if (_vehicleThread == null)
                {
                    _stopThread = false;
                    _vehicleThreadWakeEvent.Reset();
                    _vehicleThread = new Thread(VehicleThread);
                    _vehicleThread.Priority = ThreadPriority.Normal;
                    _vehicleThread.Start();
                }

                if (_tcpThread != null)
                {
                    if (!_tcpThread.IsAlive)
                    {
                        _tcpThread = null;
                    }
                }

                if (_tcpThread == null)
                {
                    _stopThread = false;
                    _tcpThreadWakeEvent.Reset();
                    _tcpThread = new Thread(TcpThread);
                    _tcpThread.Priority = ThreadPriority.Normal;
                    _tcpThread.Start();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("StartTcpListener Exception: {0}", ex.Message);
            }

            return false;
        }

        private bool StopTcpListener()
        {
            try
            {
                if (_tcpThread != null)
                {
                    _stopThread = true;
                    _tcpThreadWakeEvent.Set();
                    if (!_tcpThread.Join(ThreadFinishTimeout))
                    {
                        log.ErrorFormat("StopTcpListener Stopping thread failed");
                    }

                    _tcpThread = null;
                }

                if (_vehicleThread != null)
                {
                    _stopThread = true;
                    _vehicleThreadWakeEvent.Set();
                    if (!_vehicleThread.Join(ThreadFinishTimeout))
                    {
                        log.ErrorFormat("StopTcpListener Stopping vehicle thread failed");
                    }

                    _vehicleThread = null;
                }

                StopTcpServers();
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("StopTcpListener Exception: {0}", ex.Message);
            }

            return false;
        }

        private void StopTcpServers()
        {
            if (_enetTcpChannels.Count == 0)
            {
                return;
            }

            foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
            {
                TcpClientsDisconnect(enetTcpChannel);

                if (enetTcpChannel.TcpServer != null)
                {
                    log.ErrorFormat("StopTcpListener Stopping Port: {0}, Control: {1}", enetTcpChannel.ServerPort, enetTcpChannel.Control);
                    enetTcpChannel.TcpServer.Stop();
                    enetTcpChannel.TcpServer = null;
                    enetTcpChannel.ServerPort = 0;
                }
            }

            _enetTcpChannels.Clear();
        }

        private void TcpClientsDisconnect(EnetTcpChannel enetTcpChannel)
        {
            if (enetTcpChannel == null)
            {
                return;
            }

            foreach (EnetTcpClientData enetTcpClientData in enetTcpChannel.TcpClientList)
            {
                TcpClientDisconnect(enetTcpClientData);
            }
        }

        private bool TcpClientConnect(TcpListener tcpListener, EnetTcpClientData enetTcpClientData)
        {
            try
            {
                if (enetTcpClientData == null)
                {
                    return true;
                }

                if (!IsTcpClientConnected(enetTcpClientData.TcpClientConnection))
                {
                    TcpClientDisconnect(enetTcpClientData);
                    if (!tcpListener.Pending())
                    {
                        return true;
                    }

                    enetTcpClientData.ConnectFailure = false;
                    lock (enetTcpClientData.RecQueue)
                    {
                        enetTcpClientData.RecQueue.Clear();
                    }
                    lock (enetTcpClientData.SendQueue)
                    {
                        enetTcpClientData.SendQueue.Clear();
                    }
                    enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();

                    enetTcpClientData.TcpClientConnection = tcpListener.AcceptTcpClient();
                    enetTcpClientData.TcpClientConnection.SendBufferSize = TcpSendBufferSize;
                    enetTcpClientData.TcpClientConnection.SendTimeout = TcpSendTimeout;
                    enetTcpClientData.TcpClientConnection.NoDelay = true;
                    enetTcpClientData.TcpClientStream = enetTcpClientData.TcpClientConnection.GetStream();
                    TcpReceive(enetTcpClientData);
                    log.InfoFormat("TcpThread Accept Port: {0}, Control: {1}", enetTcpClientData.EnetTcpChannel.ServerPort, enetTcpClientData.EnetTcpChannel.Control);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpClientDiagDisconnect Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool TcpClientDisconnect(EnetTcpClientData enetTcpClientData)
        {
            try
            {
                if (enetTcpClientData == null)
                {
                    return true;
                }

                if (enetTcpClientData.TcpClientStream != null)
                {
                    enetTcpClientData.TcpClientStream.Close();
                    enetTcpClientData.TcpClientStream = null;
                    log.ErrorFormat("TcpClientDiagDisconnect Client stream closed Port: {0}, Control: {1}", enetTcpClientData.EnetTcpChannel.ServerPort, enetTcpClientData.EnetTcpChannel.Control);
                }

                if (enetTcpClientData.TcpClientConnection != null)
                {
                    enetTcpClientData.TcpClientConnection.Close();
                    enetTcpClientData.TcpClientConnection = null;
                    log.ErrorFormat("TcpClientDiagDisconnect Client connection closed Port: {0}, Control: {1}", enetTcpClientData.EnetTcpChannel.ServerPort, enetTcpClientData.EnetTcpChannel.Control);
                }

                lock (enetTcpClientData.RecQueue)
                {
                    enetTcpClientData.RecQueue.Clear();
                }
                lock (enetTcpClientData.SendQueue)
                {
                    enetTcpClientData.SendQueue.Clear();
                }
                enetTcpClientData.ConnectFailure = false;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpClientDiagDisconnect Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool TcpReceive(EnetTcpClientData enetTcpClientData)
        {
            try
            {
                if (enetTcpClientData.TcpClientStream == null)
                {
                    return false;
                }

                enetTcpClientData.TcpClientStream.BeginRead(enetTcpClientData.DataBuffer, 0, enetTcpClientData.DataBuffer.Length, TcpReceiver, enetTcpClientData);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpReceive Exception: {0}", ex.Message);
                enetTcpClientData.ConnectFailure = true;
                return false;
            }
        }

        private void TcpReceiver(IAsyncResult ar)
        {
            if (ar.AsyncState is EnetTcpClientData enetTcpClientData)
            {
                try
                {
                    if (!enetTcpClientData.ConnectFailure && enetTcpClientData.TcpClientStream != null)
                    {
                        int length = enetTcpClientData.TcpClientStream.EndRead(ar);
                        if (length > 0)
                        {
                            string recString = BitConverter.ToString(enetTcpClientData.DataBuffer, 0, length).Replace("-", " ");
                            log.InfoFormat("TcpReceiver Received TCP Data={0}", recString);
                            lock (enetTcpClientData.RecQueue)
                            {
                                for (int i = 0; i < length; i++)
                                {
                                    enetTcpClientData.RecQueue.Enqueue(enetTcpClientData.DataBuffer[i]);
                                }
                            }

                            enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                            enetTcpClientData.EnetTcpChannel.RecEvent.Set();
                        }

                        TcpReceive(enetTcpClientData);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("TcpReceiver Exception: {0}", ex.Message);
                    enetTcpClientData.ConnectFailure = true;
                }
            }
        }

        private bool IsTcpClientConnected(TcpClient tcpClient)
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

        private void WriteNetworkStream(EnetTcpClientData enetTcpClientData, byte[] buffer, int offset, int size)
        {
            if (size == 0)
            {
                return;
            }

            int packetSize = enetTcpClientData.TcpClientConnection.SendBufferSize;
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
                    enetTcpClientData.TcpClientStream.Write(buffer, offset + pos, length);
                    pos += length;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("WriteNetworkStream Exception: {0}", ex.Message);
                    throw;
                }
            }
        }

        private byte[] GetQueuePacket(Queue<byte> queue)
        {
            if (queue.Count < 6)
            {
                return null;
            }

            byte[] data = queue.ToArray();
            UInt32 payloadLength = (((UInt32)data[0] << 24) | ((UInt32)data[1] << 16) | ((UInt32)data[2] << 8) | data[3]);
            if (payloadLength < 1 || payloadLength > 0x00FFFFFF)
            {
                log.ErrorFormat("GetQueuePacket: Invalid payload length: {0}", payloadLength);
                throw new Exception("Invalid payload length");
            }

            UInt32 packetLength = payloadLength + 6;
            if (data.Length < packetLength)
            {
                log.InfoFormat("GetQueuePacket: More data required: {0} < {1}", data.Length, packetLength);
                return null;
            }

            byte[] packetBytes = new byte[packetLength];
            Array.Copy(data, 0, packetBytes, 0, packetLength);
            for (int i = 0; i < packetLength; i++)
            {
                queue.Dequeue();
            }

            return packetBytes;
        }

        private void SendAckPacket(EnetTcpClientData enetTcpClientData, byte[] recPacket)
        {
            if (recPacket.Length < 6)
            {
                return;
            }

            byte[] ackPacket = (byte[])recPacket.Clone();
            ackPacket[5] = 0x02;    // ack

            string ackString = BitConverter.ToString(ackPacket).Replace("-", " ");
            log.InfoFormat("SendAckPacket Sending Ack Data={0}", ackString);
            enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
            lock (enetTcpClientData.SendQueue)
            {
                foreach (byte ackData in ackPacket)
                {
                    enetTcpClientData.SendQueue.Enqueue(ackData);
                }
            }

            enetTcpClientData.EnetTcpChannel.SendEvent.Set();
        }

        private byte[] CreateBmwFastTelegram(byte[] dataPacket)
        {
            if (dataPacket.Length < 8)
            {
                return null;
            }

            IEnumerable<byte> dataContent = dataPacket.Skip(8);
            int dataLen = dataPacket.Length - 8;
            byte sourceAddr = dataPacket[6];
            byte targetAddr = dataPacket[7];
            List<byte> bmwFastTel = new List<byte>();

            if (sourceAddr == TcpTesterAddr)
            {
                sourceAddr = 0xF1;
            }

            if (dataLen > 0x3F)
            {
                if (dataLen > 0xFF)
                {
                    bmwFastTel.Add(0x80);
                    bmwFastTel.Add(targetAddr);
                    bmwFastTel.Add(sourceAddr);
                    bmwFastTel.Add(0x00);
                    bmwFastTel.Add((byte)(dataLen >> 8));
                    bmwFastTel.Add((byte)(dataLen & 0xFF));
                    bmwFastTel.AddRange(dataContent);
                }
                else
                {
                    bmwFastTel.Add(0x80);
                    bmwFastTel.Add(targetAddr);
                    bmwFastTel.Add(sourceAddr);
                    bmwFastTel.Add((byte)dataLen);
                    bmwFastTel.AddRange(dataContent);
                }
            }
            else
            {
                bmwFastTel.Add((byte)(0x80 | dataLen));
                bmwFastTel.Add(targetAddr);
                bmwFastTel.Add(sourceAddr);
                bmwFastTel.AddRange(dataContent);
            }

            if (IsFunctionalAddress(targetAddr))
            {   // functional address
                bmwFastTel[0] |= 0xC0;
            }

            return bmwFastTel.ToArray();
        }

        private byte[] CreateNr78Tel(byte[] bmwFastTel)
        {
            if (bmwFastTel == null || bmwFastTel.Length < 3)
            {
                return null;
            }

            byte sourceAddr = bmwFastTel[2];
            byte targetAddr = bmwFastTel[1];
            int dataOffset = 3;
            int dataLength = bmwFastTel[0] & 0x3F;
            if (dataLength == 0)
            {   // with length byte
                if (bmwFastTel[3] == 0)
                {
                    if (bmwFastTel.Length < 6)
                    {
                        return null;
                    }

                    dataOffset = 6;
                }
                else
                {
                    if (bmwFastTel.Length < 4)
                    {
                        return null;
                    }

                    dataOffset = 4;
                }
            }

            List<byte> nr78Tel = new List<byte>();
            nr78Tel.Add(0x83);
            nr78Tel.Add(sourceAddr);
            nr78Tel.Add(targetAddr);
            nr78Tel.Add(0x7F);
            nr78Tel.Add(bmwFastTel[dataOffset]);
            nr78Tel.Add(0x78);
            return nr78Tel.ToArray();
        }

        private bool SendEnetResponses(EnetTcpClientData enetTcpClientData, List<string> responseList)
        {
            if (responseList == null || responseList.Count == 0)
            {
                log.ErrorFormat("SendEnetResponses Empty response list, ignoring");
                return false;
            }

            bool result = true;
            log.InfoFormat("SendEnetResponses Sending back Responses:{0}", responseList.Count);
            foreach (string responseData in responseList)
            {
                string responseString = responseData.Replace(" ", "");
                byte[] receiveData = EdiabasNet.HexToByteArray(responseString);
                byte[] enetTel = CreateEnetTelegram(receiveData);
                if (enetTel == null)
                {
                    log.ErrorFormat("SendEnetResponses EnetTel invalid");
                    result = false;
                }
                else
                {
                    string recString = BitConverter.ToString(enetTel).Replace("-", " ");
                    log.InfoFormat("SendEnetResponses Sending ENET Data={0}", recString);
                    enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    lock (enetTcpClientData.SendQueue)
                    {
                        foreach (byte enetData in enetTel)
                        {
                            enetTcpClientData.SendQueue.Enqueue(enetData);
                        }
                    }

                    enetTcpClientData.EnetTcpChannel.SendEvent.Set();
                }
            }

            return result;
        }

        private void SendNr78Tels(EnetTcpClientData enetTcpClientData)
        {
            List<Nr78Data> nr78SendList = new List<Nr78Data>();
            lock (enetTcpClientData.Nr78Dict)
            {
                foreach (KeyValuePair<byte,Nr78Data> keyValuePair in enetTcpClientData.Nr78Dict)
                {
                    Nr78Data nr78Data = keyValuePair.Value;
                    if ((Stopwatch.GetTimestamp() - nr78Data.LastTcpSendTick) > nr78Data.GetDelay() * TickResolMs)
                    {
                        nr78Data.LastTcpSendTick = Stopwatch.GetTimestamp();
                        nr78SendList.Add(nr78Data);
                    }
                }
            }

            foreach (Nr78Data nr78Data in nr78SendList)
            {
                bool removeTel = false;
                string sendString = BitConverter.ToString(nr78Data.Nr78Tel).Replace("-", " ");
                byte[] enetTel = CreateEnetTelegram(nr78Data.Nr78Tel);
                if (enetTel == null)
                {
                    log.ErrorFormat("SendNr78Tels EnetTel invalid, Data={0}", sendString);
                    removeTel = true;
                }
                else
                {
                    log.InfoFormat("SendNr78Tels Sending Count={0}, Data={1}", nr78Data.Count, sendString);
                    enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                    lock (enetTcpClientData.SendQueue)
                    {
                        foreach (byte enetData in enetTel)
                        {
                            enetTcpClientData.SendQueue.Enqueue(enetData);
                        }
                    }

                    enetTcpClientData.EnetTcpChannel.SendEvent.Set();
                    nr78Data.Count++;
                    if (nr78Data.Count > Nr78RetryMax)
                    {
                        removeTel = true;
                    }
                }

                if (removeTel)
                {
                    log.InfoFormat("SendNr78Tels Removing Data={0}", sendString);
                    lock (enetTcpClientData.Nr78Dict)
                    {
                        enetTcpClientData.Nr78Dict.Remove(nr78Data.Addr);
                    }
                }
            }
        }

        private byte[] CreateEnetTelegram(byte[] bmwFastTel)
        {
            if (bmwFastTel.Length < 3)
            {
                return null;
            }

            byte targetAddr = bmwFastTel[1];
            byte sourceAddr = bmwFastTel[2];
            if (targetAddr == 0xF1)
            {
                targetAddr = TcpTesterAddr;
            }
            int dataOffset = 3;
            int dataLength = bmwFastTel[0] & 0x3F;
            if (dataLength == 0)
            {   // with length byte
                if (bmwFastTel[3] == 0)
                {
                    if (bmwFastTel.Length < 6)
                    {
                        return null;
                    }

                    dataLength = (bmwFastTel[4] << 8) | bmwFastTel[5];
                    dataOffset = 6;
                }
                else
                {
                    if (bmwFastTel.Length < 4)
                    {
                        return null;
                    }

                    dataLength = bmwFastTel[3];
                    dataOffset = 4;
                }
            }

            if (bmwFastTel.Length < dataOffset + dataLength)
            {
                return null;
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
            Array.Copy(bmwFastTel, dataOffset, dataBuffer, 8, dataLength);

            return dataBuffer;
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

#if EDIABAS_CONNECTION
        private bool AbortEdiabasJob()
        {
            if (_stopThread)
            {
                return true;
            }
            return false;
        }

        public bool EdiabasConnect()
        {
            try
            {
                if (_ediabas.EdInterfaceClass.InterfaceConnect())
                {
                    log.InfoFormat("EdiabasConnect Connection ok");
                    return true;
                }

                log.ErrorFormat("EdiabasConnect Connection failed");
                return false;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("EdiabasConnect Exception: {0}", ex.Message);
                return false;
            }
        }

        public bool EdiabasDisconnect()
        {
            try
            {
                log.InfoFormat("EdiabasDisconnect");
                return _ediabas.EdInterfaceClass.InterfaceDisconnect();
            }
            catch (Exception)
            {
                return false;
            }
        }
#endif
        private void TcpThread()
        {
            SetLogInfo(SessionId);
            log.InfoFormat("TcpThread started");

            for (;;)
            {
                WaitHandle[] waitHandles = new WaitHandle[1 + _enetTcpChannels.Count * 2];
                int index = 0;

                waitHandles[index++] = _tcpThreadWakeEvent;
                foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
                {
                    waitHandles[index++] = enetTcpChannel.RecEvent;
                    waitHandles[index++] = enetTcpChannel.SendEvent;
                }

                WaitHandle.WaitAny(waitHandles, 100, false);

                if (_stopThread)
                {
                    break;
                }

                foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
                {
                    foreach (EnetTcpClientData enetTcpClientData in enetTcpChannel.TcpClientList)
                    {
                        try
                        {
                            if (enetTcpClientData.ConnectFailure)
                            {
                                TcpClientDisconnect(enetTcpClientData);
                            }

                            if (enetTcpChannel.TcpServer.Pending())
                            {
                                TcpClientConnect(enetTcpChannel.TcpServer, enetTcpClientData);
                            }

                            if (!_vehicleThread.IsAlive)
                            {
                                TcpClientDisconnect(enetTcpClientData);
                            }

                            if (enetTcpChannel.Control)
                            {
                                byte[] recPacket;
                                lock (enetTcpClientData.RecQueue)
                                {
                                    recPacket = GetQueuePacket(enetTcpClientData.RecQueue);
                                }

                                if (recPacket != null && recPacket.Length >= 6)
                                {
                                    UInt32 payloadType = ((UInt32)recPacket[4] << 8) | recPacket[5];
                                    if (payloadType == 0x0010)
                                    {   // ignition state
                                        byte[] statePacket = new byte[6 + 1];
                                        statePacket[3] = 0x06;    // length
                                        statePacket[5] = 0x10;    // state
                                        statePacket[6] = 0x05;    // ignition on
                                        log.InfoFormat("VehicleThread Send ignition state");

                                        lock (enetTcpClientData.SendQueue)
                                        {
                                            foreach (byte stateData in statePacket)
                                            {
                                                enetTcpClientData.SendQueue.Enqueue(stateData);
                                            }
                                        }

                                        enetTcpChannel.SendEvent.Set();
                                    }
                                }
                            }
                            else
                            {
                                byte[] recPacket;
                                lock (enetTcpClientData.RecQueue)
                                {
                                    recPacket = GetQueuePacket(enetTcpClientData.RecQueue);
                                }

                                if (recPacket != null && recPacket.Length >= 6)
                                {
                                    UInt32 payloadType = ((UInt32)recPacket[4] << 8) | recPacket[5];
                                    if (payloadType == 0x0001)
                                    {
                                        SendAckPacket(enetTcpClientData, recPacket);

                                        byte[] bmwFastTel = CreateBmwFastTelegram(recPacket);
                                        byte[] nr78Tel = CreateNr78Tel(bmwFastTel);
                                        if (bmwFastTel == null || nr78Tel == null)
                                        {
                                            log.ErrorFormat("TcpThread BmwFastTel invalid");
                                        }
                                        else
                                        {
                                            bool funcAddress = (bmwFastTel[0] & 0xC0) == 0xC0; // functional address
                                            List<string> cachedResponseList = null;
                                            string sendDataString = BitConverter.ToString(bmwFastTel).Replace("-", "");
                                            if (funcAddress)
                                            {
                                                ProgrammingJobs.CacheType cacheType = ProgrammingJobs.CacheResponseType;
                                                if (cacheType == ProgrammingJobs.CacheType.None)
                                                {
                                                    log.InfoFormat("TcpThread Caching disabled");
                                                }
                                                else
                                                {
                                                    lock (_lockObject)
                                                    {
                                                        _vehicleResponseDict.TryGetValue(sendDataString, out cachedResponseList);
                                                    }

                                                    if (cachedResponseList != null && cacheType == ProgrammingJobs.CacheType.NoResponse)
                                                    {
                                                        if (cachedResponseList.Count > 0)
                                                        {
                                                            log.InfoFormat("TcpThread Only none response caches are allowed");
                                                            cachedResponseList = null;
                                                        }
                                                    }
                                                }
                                            }

                                            string recString = BitConverter.ToString(bmwFastTel).Replace("-", " ");
                                            if (cachedResponseList != null)
                                            {
                                                log.InfoFormat("TcpThread Cache entry found for Data={0}", recString);
                                                SendEnetResponses(enetTcpClientData, cachedResponseList);
                                            }
                                            else
                                            {
                                                bool enqueued = false;
                                                int queueSize;
                                                lock (enetTcpClientData.RecPacketQueue)
                                                {
                                                    bool found = false;
                                                    foreach (byte[] queueData in enetTcpClientData.RecPacketQueue)
                                                    {
                                                        if (queueData.Length == bmwFastTel.Length && queueData.SequenceEqual(bmwFastTel))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                    }

                                                    if (!found)
                                                    {
                                                        enetTcpClientData.RecPacketQueue.Enqueue(bmwFastTel);
                                                        enqueued = true;
                                                    }

                                                    queueSize = enetTcpClientData.RecPacketQueue.Count;
                                                }

                                                if (enqueued)
                                                {
                                                    byte sourceAddr = bmwFastTel[1];
                                                    int nr78DictSize;
                                                    lock (enetTcpClientData.Nr78Dict)
                                                    {
                                                        enetTcpClientData.Nr78Dict[sourceAddr] = new Nr78Data(sourceAddr, nr78Tel, funcAddress ? Nr78Delay : 5000);
                                                        nr78DictSize = enetTcpClientData.Nr78Dict.Count;
                                                    }

                                                    log.InfoFormat("TcpThread Enqueued QueueSize={0}, Nr78Size={1}, Data={2}", queueSize, nr78DictSize, recString);
                                                    enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                                                    _vehicleThreadWakeEvent.Set();
                                                }
                                                else
                                                {
                                                    log.InfoFormat("TcpThread Already in queue QueueSize={0}, Data={1}", queueSize, recString);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (enetTcpClientData.TcpClientStream != null)
                                    {
                                        if ((Stopwatch.GetTimestamp() - enetTcpClientData.LastTcpRecTick) > 10000 * TickResolMs)
                                        {
                                            byte[] keepAlivePacket = new byte[6 + 2];
                                            keepAlivePacket[3] = 0x02;
                                            keepAlivePacket[5] = 0x12;   // Payoad type: alive check
                                            keepAlivePacket[6] = 0xF4;
                                            keepAlivePacket[7] = 0x00;

                                            log.InfoFormat("VehicleThread Send keep alive");
                                            enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                                            lock (enetTcpClientData.SendQueue)
                                            {
                                                foreach (byte stateData in keepAlivePacket)
                                                {
                                                    enetTcpClientData.SendQueue.Enqueue(stateData);
                                                }
                                            }

                                            enetTcpChannel.SendEvent.Set();
                                        }
                                    }
                                }

                                SendNr78Tels(enetTcpClientData);
                            }

                            if (enetTcpClientData.TcpClientStream != null)
                            {
                                byte[] sendData;
                                lock (enetTcpClientData.SendQueue)
                                {
                                    sendData = enetTcpClientData.SendQueue.ToArray();
                                    enetTcpClientData.SendQueue.Clear();
                                }

                                if (sendData.Length > 0)
                                {
                                    string sendString = BitConverter.ToString(sendData).Replace("-", " ");
                                    log.InfoFormat("TcpThread Sending TCP Data={0}", sendString);
                                    WriteNetworkStream(enetTcpClientData, sendData, 0, sendData.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("TcpThread Exception: {0}", ex.Message);
                            enetTcpClientData.ConnectFailure = true;
                        }
                    }
                }
            }

            log.InfoFormat("TcpThread stopped");
        }

        private string GetVehicleUrl()
        {
            if (DeepObdVersion > 0)
            {
                return string.Empty;
            }

            return string.Format(CultureInfo.InvariantCulture, "http://127.0.0.1:8080");
        }

        public void VehicleResponseDictClear()
        {
            lock (_lockObject)
            {
                _vehicleResponseDict.Clear();
            }
        }

        public void VehicleResponseClear()
        {
            lock (_lockObject)
            {
                _vehicleResponses.Clear();
            }
        }

        public void VehicleResponseReceived(PsdzVehicleHub.VehicleResponse vehicleResponse)
        {
            lock (_lockObject)
            {
                _vehicleResponses.Enqueue(vehicleResponse);
            }

            _vehicleThreadWakeEvent.Set();
        }

        public PsdzVehicleHub.VehicleResponse VehicleResponseGet()
        {
            lock (_lockObject)
            {
                if (_vehicleResponses.Count == 0)
                {
                    return null;
                }
                return _vehicleResponses.Dequeue();
            }
        }

        public bool HubVehicleConnect(IHubContext<IPsdzClient> hubContext)
        {
            if (hubContext == null)
            {
                log.ErrorFormat("HubVehicleConnect No hub context");
                return false;
            }

            List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
            foreach (string connectionId in connectionIds)
            {
                hubContext.Clients.Client(connectionId)?.VehicleConnect(GetVehicleUrl(), GetNextPacketId());
            }

            PsdzVehicleHub.VehicleResponse vehicleResponse = WaitForVehicleResponse();
            if (vehicleResponse != null)
            {
                AppId = vehicleResponse.AppId;
                AdapterSerial = vehicleResponse.AdapterSerial;
                AdapterSerialValid = vehicleResponse.SerialValid;
                log.InfoFormat("HubVehicleConnect AppId={0}, AdapterSerial={1}, Valid={2}", AppId ?? string.Empty, AdapterSerial ?? string.Empty, AdapterSerialValid);
            }

            if (vehicleResponse == null || vehicleResponse.Error || !vehicleResponse.Valid || !vehicleResponse.Connected)
            {
                log.ErrorFormat("HubVehicleConnect Vehicle connect failed");
                return false;
            }

            return true;
        }

        public PsdzVehicleHub.VehicleResponse WaitForVehicleResponse(VehicleResponseDelegate vehicleResponseDelegate = null, int timeout = VehicleReceiveTimeout)
        {
            log.InfoFormat("WaitForVehicleResponse Timeout={0}", timeout);
            string packetId = GetPacketId();
            long startTime = Stopwatch.GetTimestamp();
            for (;;)
            {
                PsdzVehicleHub.VehicleResponse vehicleResponse = VehicleResponseGet();
                if (vehicleResponse != null)
                {
                    GetPacketId();
                    if (string.Compare(vehicleResponse.Id, packetId, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (vehicleResponse.Valid || vehicleResponse.Error)
                        {
                            log.InfoFormat("WaitForVehicleResponse Valid={0}, Error={1}", vehicleResponse.Valid, vehicleResponse.Error);
                            return vehicleResponse;
                        }
                    }
                }

                List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                if (connectionIds == null || connectionIds.Count == 0)
                {
                    log.InfoFormat("WaitForVehicleResponse Connection broken");
                    return null;
                }

                if (vehicleResponseDelegate != null)
                {
                    if (vehicleResponseDelegate.Invoke())
                    {
                        log.InfoFormat("WaitForVehicleResponse Wait aborted");
                        return null;
                    }
                }

                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    log.InfoFormat("WaitForVehicleResponse No Response");
                    return null;
                }

                _vehicleThreadWakeEvent.WaitOne(100);
            }
        }

        private void LogVehicleResponse(PsdzVehicleHub.VehicleResponse vehicleResponse)
        {
            lock (_vehicleLogObject)
            {
                try
                {
                    if (_fsVehicleLog == null)
                    {
                        string dateString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                        string fileName = string.Format(CultureInfo.InvariantCulture, "Vehicle-{0}-{1}.txt", dateString, SessionId);
                        string logFile = Path.Combine(ProgrammingJobs.ProgrammingService.GetPsdzServiceHostLogDir(), fileName);
                        _fsVehicleLog = File.Create(logFile);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("LogVehicleResponse Exception: {0}", ex.Message);
                }
            }
        }

        private void CloseVehicleLog()
        {
            lock (_vehicleLogObject)
            {
                try
                {
                    if (_fsVehicleLog != null)
                    {
                        _fsVehicleLog.Close();
                        _fsVehicleLog.Dispose();
                        _fsVehicleLog = null;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CloseVehicleLog Exception: {0}", ex.Message);
                }
            }
        }

        private void VehicleThread()
        {
            SetLogInfo(SessionId);
            log.InfoFormat("VehicleThread started");
#if EDIABAS_CONNECTION
            EdiabasConnect();
#else
            IHubContext<IPsdzClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<PsdzVehicleHub, IPsdzClient>();
            if (hubContext == null)
            {
                log.ErrorFormat("VehicleThread No hub context");
            }

            VehicleResponseDictClear();
            if (hubContext != null)
            {
                VehicleResponseClear();
                ResetPacketId();

                if (!HubVehicleConnect(hubContext))
                {
                    log.ErrorFormat("VehicleThread Vehicle connect failed");
                }
            }
#endif
            for (;;)
            {
                _vehicleThreadWakeEvent.WaitOne(100, false);
                if (_stopThread)
                {
                    break;
                }

                foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
                {
                    if (enetTcpChannel.Control)
                    {
                        continue;
                    }

                    foreach (EnetTcpClientData enetTcpClientData in enetTcpChannel.TcpClientList)
                    {
                        try
                        {
                            if (enetTcpClientData.TcpClientStream == null)
                            {
                                continue;
                            }

                            byte[] bmwFastTel = null;
                            lock (enetTcpClientData.RecPacketQueue)
                            {
                                if (enetTcpClientData.RecPacketQueue.Count > 0)
                                {
                                    // keep the packet until processed
                                    bmwFastTel = enetTcpClientData.RecPacketQueue.Peek();
                                }
                            }

                            if (bmwFastTel != null)
                            {
                                byte[] sendData = bmwFastTel;
                                bool funcAddress = (sendData[0] & 0xC0) == 0xC0; // functional address

                                string sendString = BitConverter.ToString(sendData).Replace("-", " ");
                                log.InfoFormat("VehicleThread Transmit Data={0}", sendString);

                                if (ProgrammingJobs.CacheClearRequired)
                                {
                                    log.InfoFormat("VehicleThread Clearing response cache");
                                    VehicleResponseDictClear();
                                    ProgrammingJobs.CacheClearRequired = false;
                                }
#if !EDIABAS_CONNECTION
                                if (hubContext != null)
                                {
                                    string sendDataString = BitConverter.ToString(bmwFastTel).Replace("-", "");
                                    for (int retry = 0; retry < 3; retry++)
                                    {
                                        List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                                        foreach (string connectionId in connectionIds)
                                        {
                                            hubContext.Clients.Client(connectionId)?.VehicleSend(GetVehicleUrl(), GetNextPacketId(), sendDataString);
                                        }

                                        PsdzVehicleHub.VehicleResponse vehicleResponse = WaitForVehicleResponse(() =>
                                        {
                                            if (_stopThread)
                                            {
                                                return true;
                                            }

                                            return false;
                                        });

                                        byte sourceAddr = bmwFastTel[1];
                                        bool nr78Removed;
                                        lock (enetTcpClientData.Nr78Dict)
                                        {
                                            nr78Removed = enetTcpClientData.Nr78Dict.Remove(sourceAddr);
                                        }

                                        if (nr78Removed)
                                        {
                                            log.InfoFormat("VehicleThread NR78 removed: Addr={0:X02}", sourceAddr);
                                        }

                                        if (vehicleResponse == null || vehicleResponse.Error || !vehicleResponse.Valid)
                                        {
                                            log.ErrorFormat("VehicleThread Vehicle transmit failed");
                                        }
                                        else
                                        {
                                            if (!vehicleResponse.Connected)
                                            {
                                                log.ErrorFormat("VehicleThread Vehicle disconnected, reconnecting");
                                                if (!HubVehicleConnect(hubContext))
                                                {
                                                    log.ErrorFormat("VehicleThread Vehicle reconnect failed");
                                                }
                                                else
                                                {
                                                    log.ErrorFormat("VehicleThread Vehicle reconnected, retrying: {0}", retry);
                                                    continue;
                                                }
                                            }
                                            else if (vehicleResponse.ResponseList == null || vehicleResponse.ResponseList.Count == 0)
                                            {
                                                log.ErrorFormat("VehicleThread Vehicle transmit no response for Request={0}", sendDataString);

                                                if (funcAddress)
                                                {
                                                    log.InfoFormat("VehicleThread Cache disable Request={0}", sendDataString);
                                                    lock (_lockObject)
                                                    {
                                                        _vehicleResponseDict[sendDataString] = new List<string>();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                LogVehicleResponse(vehicleResponse);
                                                if (funcAddress)
                                                {
                                                    log.InfoFormat("VehicleThread Caching Request={0}", sendDataString);
                                                    lock (_lockObject)
                                                    {
                                                        _vehicleResponseDict[sendDataString] = vehicleResponse.ResponseList;
                                                    }
                                                }

                                                SendEnetResponses(enetTcpClientData, vehicleResponse.ResponseList);
                                            }
                                        }

                                        break;
                                    }
                                }
#else
                                for (; ; )
                                {
                                    bool dataReceived = false;
                                    try
                                    {
                                        if (_ediabas.EdInterfaceClass.TransmitData(sendData, out byte[] receiveData))
                                        {
                                            dataReceived = true;
                                            byte[] enetTel = CreateEnetTelegram(receiveData);
                                            if (enetTel == null)
                                            {
                                                log.ErrorFormat("VehicleThread EnetTel invalid");
                                            }
                                            else
                                            {
                                                string recString = BitConverter.ToString(enetTel).Replace("-", " ");
                                                log.InfoFormat("VehicleThread Receive Data={0}", recString);
                                                enetTcpClientData.LastTcpRecTick = Stopwatch.GetTimestamp();
                                                lock (enetTcpClientData.SendQueue)
                                                {
                                                    foreach (byte enetData in enetTel)
                                                    {
                                                        enetTcpClientData.SendQueue.Enqueue(enetData);
                                                    }
                                                }

                                                enetTcpChannel.SendEvent.Set();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.ErrorFormat("VehicleThread TransmitData Exception: {0}", ex.Message);
                                    }

                                    if (!funcAddress || !dataReceived)
                                    {
                                        break;
                                    }

                                    int recLen = enetTcpClientData.RecQueue.Count;
                                    if (recLen > 0)
                                    {
                                        log.ErrorFormat("VehicleThread Next request present");
                                        break;
                                    }

                                    if (AbortEdiabasJob())
                                    {
                                        break;
                                    }

                                    sendData = Array.Empty<byte>();
                                }
#endif
                                lock (enetTcpClientData.RecPacketQueue)
                                {
                                    if (enetTcpClientData.RecPacketQueue.Count > 0)
                                    {
                                        enetTcpClientData.RecPacketQueue.Dequeue();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("VehicleThread Exception: {0}", ex.Message);
                        }
                    }
                }
            }

#if EDIABAS_CONNECTION
            EdiabasDisconnect();
#else
            if (hubContext != null)
            {
                List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                foreach (string connectionId in connectionIds)
                {
                    hubContext.Clients.Client(connectionId)?.VehicleDisconnect(GetVehicleUrl(), GetNextPacketId());
                }

                PsdzVehicleHub.VehicleResponse vehicleResponse = WaitForVehicleResponse();
                if (vehicleResponse == null || vehicleResponse.Error || !vehicleResponse.Valid)
                {
                    log.ErrorFormat("VehicleThread Vehicle disconnect failed");
                }
            }
#endif
            VehicleResponseDictClear();
            CloseVehicleLog();
            log.InfoFormat("VehicleThread stopped");
        }

        public void UpdateStatus(string message = null)
        {
            string text = message ?? string.Empty;
            if (StatusText != text)
            {
                StatusText = text;
                UpdateDisplay();
            }
        }

        public void ReportError(string msg)
        {
            try
            {
                IHubContext<IPsdzClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<PsdzVehicleHub, IPsdzClient>();
                if (hubContext == null)
                {
                    log.ErrorFormat("ReportError No hub context");
                    return;
                }

                List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                foreach (string connectionId in connectionIds)
                {
                    hubContext.Clients.Client(connectionId)?.ReportError(msg);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ReportError Exception: {0}", ex.Message);
            }
        }

        public void UpdateDisplay(bool updatePanel = true)
        {
            try
            {
                IHubContext<IPsdzClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<PsdzVehicleHub, IPsdzClient>();
                if (hubContext == null)
                {
                    log.ErrorFormat("UpdateDisplay No hub context");
                    return;
                }

                List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                foreach (string connectionId in connectionIds)
                {
                    hubContext.Clients.Client(connectionId)?.UpdatePanels(updatePanel);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateDisplay Exception: {0}", ex.Message);
            }
        }

        public void ShowModalPopup(bool show = true)
        {
            try
            {
                IHubContext<IPsdzClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<PsdzVehicleHub, IPsdzClient>();
                if (hubContext == null)
                {
                    log.ErrorFormat("ShowModalPopup No hub context");
                    return;
                }

                List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                foreach (string connectionId in connectionIds)
                {
                    hubContext.Clients.Client(connectionId)?.ShowModalPopup(show);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ShowModalPopup Exception: {0}", ex.Message);
            }
        }

        public void ReloadPage()
        {
            log.InfoFormat("ReloadPage");

            try
            {
                IHubContext<IPsdzClient> hubContext = GlobalHost.ConnectionManager.GetHubContext<PsdzVehicleHub, IPsdzClient>();
                if (hubContext == null)
                {
                    log.ErrorFormat("ReloadPage No hub context");
                    return;
                }

                List<string> connectionIds = PsdzVehicleHub.GetConnectionIds(SessionId);
                foreach (string connectionId in connectionIds)
                {
                    hubContext.Clients.Client(connectionId)?.ReloadPage();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ReloadPage Exception: {0}", ex.Message);
            }
        }

        public void UpdateOptions(Dictionary<PdszDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict)
        {
            OptionsDict = optionsDict;
            ProgrammingJobs.SelectedOptions = new List<ProgrammingJobs.OptionsItem>();
            UpdateCurrentOptions();
        }

        private bool ShowMessageEvent(CancellationTokenSource cts, string message, bool okBtn, bool wait)
        {
            log.InfoFormat("ShowMessageEvent OKButton={0}, Wait={1}, Message='{2}'", okBtn, wait, message);

            ShowMessageModalCount++;
            ShowMessageModalResult = true;
            ShowMessageModalWait = wait;
            ShowMessageModal = message;
            ShowMessageModalOkBtn = okBtn;
            UpdateDisplay();

            if (!wait || cts == null)
            {
                return true;
            }

            for (;;)
            {
                //ShowModalPopup();
                int waitResult = WaitHandle.WaitAny(new WaitHandle[]
                {
                    MessageWaitEvent, cts.Token.WaitHandle
                }, 500);

                if (waitResult != WaitHandle.WaitTimeout)
                {
                    break;
                }
            }

            ShowMessageModal = null;

            if (cts.IsCancellationRequested)
            {
                return false;
            }

            return ShowMessageModalResult;
        }

        private void UpdateCurrentOptions()
        {
            try
            {
                bool vehicleConnected = ProgrammingJobs?.PsdzContext?.Connection != null;
                if (!vehicleConnected)
                {
                    OptionsDict = null;
                    SelectedSwiRegister = null;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateCurrentOptions Exception: {0}", ex.Message);
            }

            RefreshOptions = true;
            UpdateDisplay();
        }

        private void UpdateProgress(int percent, bool marquee, string message = null)
        {
            string text = string.Empty;

            if (message != null)
            {
                text = message;
            }
            else
            {
                if (marquee)
                {
                    text = HttpContext.GetGlobalResourceObject("Global", "Processing") as string ?? string.Empty;
                }
            }

            if (ProgressText != text)
            {
                ProgressText = text;
                UpdateDisplay();
            }
        }

        public string GetAdapterLicenseText()
        {
            string licenseText = string.Empty;
            if (DeepObdVersion > 0)
            {
                if (!string.IsNullOrEmpty(AdapterSerial) && AdapterSerialValid)
                {
                    licenseText = HttpContext.GetGlobalResourceObject("Global", "AdapterLicensed") as string ?? string.Empty;
                }
                else
                {
                    licenseText = HttpContext.GetGlobalResourceObject("Global", "AdapterNotLicensed") as string ?? string.Empty;
                }
            }

            log.InfoFormat("GetAdapterLicenseText: '{0}'", licenseText);
            return licenseText;
        }

        public bool Cancel()
        {
            try
            {
                CancellationTokenSource cts = Cts;
                if (cts != null)
                {
                    cts.Cancel();
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public void StartProgrammingService(string istaFolder)
        {
            if (TaskActive)
            {
                return;
            }

            Cts = new CancellationTokenSource();
            StartProgrammingServiceTask(istaFolder).ContinueWith(task =>
            {
                TaskActive = false;
                Cts = null;
                UpdateCurrentOptions();
                UpdateDisplay();
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> StartProgrammingServiceTask(string istaFolder)
        {
            return await Task.Run(() => ProgrammingJobs.StartProgrammingService(Cts, istaFolder)).ConfigureAwait(false);
        }

        public void StopProgrammingService(string istaFolder)
        {
            if (TaskActive)
            {
                return;
            }

            StopProgrammingServiceTask(istaFolder).ContinueWith(task =>
            {
                TaskActive = false;
                StopTcpListener();
                UpdateCurrentOptions();
                UpdateDisplay();
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> StopProgrammingServiceTask(string istaFolder)
        {
            return await Task.Run(() => ProgrammingJobs.StopProgrammingService(Cts, istaFolder)).ConfigureAwait(false);
        }

        public void ConnectVehicle(string istaFolder)
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection != null)
            {
                return;
            }

            if (!StartTcpListener())
            {
                return;
            }

            int diagPort = 0;
            int controlPort = 0;
            foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
            {
                if (enetTcpChannel.Control)
                {
                    controlPort = enetTcpChannel.ServerPort;
                }
                else
                {
                    diagPort = enetTcpChannel.ServerPort;
                }
            }

            string remoteHost = string.Format(CultureInfo.InvariantCulture, "127.0.0.1:{0}:{1}", diagPort, controlPort);
            Cts = new CancellationTokenSource();
            ConnectVehicleTask(istaFolder, remoteHost, false).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    ReportError("ConnectVehicle failed");
                    StopTcpListener();
                }
                else
                {
                    AppendStatusTextLine(GetAdapterLicenseText());
                }

                TaskActive = false;
                Cts = null;
                UpdateCurrentOptions();
                UpdateDisplay();
                return true;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> ConnectVehicleTask(string istaFolder, string remoteHost, bool useIcom)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ProgrammingJobs.ConnectVehicle(Cts, istaFolder, remoteHost, useIcom)).ConfigureAwait(false);
        }

        public void DisconnectVehicle()
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection == null)
            {
                return;
            }

            DisconnectVehicleTask().ContinueWith(task =>
            {
                if (!task.Result)
                {
                    ReportError("DisconnectVehicle failed");
                }

                TaskActive = false;
                StopTcpListener();
                UpdateCurrentOptions();
                UpdateDisplay();
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> DisconnectVehicleTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ProgrammingJobs.DisconnectVehicle(Cts)).ConfigureAwait(false);
        }

        public void VehicleFunctions(ProgrammingJobs.OperationType operationType)
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection == null)
            {
                return;
            }

            string vin = ProgrammingJobs?.PsdzContext?.DetectVehicle?.Vin;
            if (!string.IsNullOrEmpty(vin))
            {
                if (IsVinActive(vin, this))
                {
                    StatusText = HttpContext.GetGlobalResourceObject("Global", "VinInstanceActive") as string ?? string.Empty;
                    UpdateDisplay();
                    return;
                }
            }

            Cts = new CancellationTokenSource();
            VehicleFunctionsTask(operationType).ContinueWith(task =>
            {
                if (!task.Result)
                {
                    ReportError(string.Format(CultureInfo.InvariantCulture, "VehicleFunctions: {0} failed", operationType));
                }

                TaskActive = false;
                Cts = null;
                UpdateCurrentOptions();
                UpdateDisplay();
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> VehicleFunctionsTask(ProgrammingJobs.OperationType operationType)
        {
            return await Task.Run(() => ProgrammingJobs.VehicleFunctions(Cts, operationType)).ConfigureAwait(false);
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

        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                while (TaskActive)
                {
                    Thread.Sleep(100);
                }

                CloseVehicleLog();

                if (ProgrammingJobs != null)
                {
                    ProgrammingJobs.Dispose();
                    ProgrammingJobs = null;
                }

                StopTcpListener();

#if EDIABAS_CONNECTION
                if (_ediabas != null)
                {
                    _ediabas.Dispose();
                    _ediabas = null;
                }
#endif
                lock (SessionContainers)
                {
                    SessionContainers.Remove(this);
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
