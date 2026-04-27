using EdiabasLib;
using log4net;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer;

public class PsdzVehicleProxy : IDisposable
{
    private class Nr78Data
    {
        public Nr78Data(byte addr, byte[] nr78Tel, long firstDelay)
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
    public delegate bool VehicleConnectDelegate(ulong id);
    public delegate bool VehicleDisconnectDelegate(ulong id);
    public delegate bool VehicleSendDelegate(ulong id, byte[] data);
    public delegate bool ReportErrorDelegate(string msg);

    public event VehicleConnectDelegate VehicleConnectEvent;
    public event VehicleDisconnectDelegate VehicleDisconnectEvent;
    public event VehicleSendDelegate VehicleSendEvent;
    public event ReportErrorDelegate ReportErrorEvent;

    private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
    private const int TcpSendBufferSize = 1400;
    private const int TcpSendTimeout = 5000;
    private const int TcpTesterAddr = 0xF4;
    private const int AdditionalConnectTimeout = 3000;
    private const int VehicleReceiveTimeout = 25000;
    private const long Nr78Delay = 1000;
    private const long Nr78FirstDelay = 4000;
    private const long Nr78RetryMax = VehicleReceiveTimeout / Nr78Delay;
    private const int EnetTcpMutexTimeout = 30000;
    private const int ThreadFinishTimeout = VehicleReceiveTimeout + 5000;
    private static readonly ILog log = LogManager.GetLogger(typeof(PsdzVehicleProxy));

    private bool _disposed;
    private readonly ProgrammingJobs _programmingJobs;
    private int _contextId;
    private readonly object _lockObject = new object();
    private readonly object _vehicleLogLockObject = new object();
    private Mutex _enetTcpMutex = new Mutex(false);
    private StreamWriter _swVehicleLog;
    private readonly List<EnetTcpChannel> _enetTcpChannels = new List<EnetTcpChannel>();
    private Thread _tcpThread;
    private Thread _vehicleThread;
    private bool _stopThread;
    private readonly AutoResetEvent _tcpThreadWakeEvent = new AutoResetEvent(false);
    private readonly AutoResetEvent _vehicleThreadWakeEvent = new AutoResetEvent(false);
    private readonly Queue<PsdzVehicleResponse> _vehicleResponses = new Queue<PsdzVehicleResponse>();
    private readonly Dictionary<byte[], List<byte[]>> _vehicleResponseDict = new Dictionary<byte[], List<byte[]>>();

    public PsdzVehicleProxy(ProgrammingJobs programmingJobs)
    {
        _programmingJobs = programmingJobs;
        _contextId = programmingJobs.ThreadContextId;
    }

    private UInt64 _packetId;
    private void ResetPacketId()
    {
        lock (_lockObject)
        {
            _packetId = 0;
        }
    }

    private ulong GetPacketId()
    {
        lock (_lockObject)
        {
            return _packetId;
        }
    }

    private ulong GetNextPacketId()
    {
        lock (_lockObject)
        {
            _packetId++;
            return _packetId;
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

    private int? _connectTimeouts;
    public int? ConnectTimeouts
    {
        get
        {
            lock (_lockObject)
            {
                return _connectTimeouts;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _connectTimeouts = value;
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

    private string _detectedVin;
    public string DetectedVin
    {
        get
        {
            lock (_lockObject)
            {
                return _detectedVin;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _detectedVin = value;
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

    private bool StartTcpListener()
    {
        try
        {
            StopTcpListener();

            if (_enetTcpMutex != null && !_enetTcpMutex.WaitOne(EnetTcpMutexTimeout))
            {
                log.ErrorFormat("StartTcpListener Aquire mutex failed");
                return false;
            }

            try
            {
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

                        log.InfoFormat("StartTcpListener Port: {0}, Control: {1}", enetTcpChannel.ServerPort,
                            enetTcpChannel.Control);
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
            }
            finally
            {
                _enetTcpMutex?.ReleaseMutex();
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
            if (_enetTcpMutex != null && !_enetTcpMutex.WaitOne(EnetTcpMutexTimeout))
            {
                log.ErrorFormat("StopTcpListener Aquire mutex failed");
                return false;
            }

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
            }
            finally
            {
                _enetTcpMutex?.ReleaseMutex();
            }

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

            try
            {
                if (enetTcpChannel.TcpServer != null)
                {
                    log.ErrorFormat("StopTcpListener Stopping Port: {0}, Control: {1}", enetTcpChannel.ServerPort, enetTcpChannel.Control);
                    enetTcpChannel.TcpServer.Stop();
                    enetTcpChannel.TcpServer = null;
                    enetTcpChannel.ServerPort = 0;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("StopTcpServers Exception: {0}", ex.Message);
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

                        TcpReceive(enetTcpClientData);
                    }
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

    private int CalculateDataOffset(byte[] bmwFastTel)
    {
        if (bmwFastTel == null || bmwFastTel.Length < 3)
        {
            return -1;
        }

        int dataOffset = 3;
        int dataLength = bmwFastTel[0] & 0x3F;
        if (dataLength == 0)
        {   // with length byte
            if (bmwFastTel[3] == 0)
            {
                if (bmwFastTel.Length < 6)
                {
                    return -1;
                }

                dataOffset = 6;
            }
            else
            {
                if (bmwFastTel.Length < 4)
                {
                    return -1;
                }

                dataOffset = 4;
            }
        }

        return dataOffset;
    }

    private byte[] CreateNr78Tel(byte[] bmwFastTel)
    {
        if (bmwFastTel == null || bmwFastTel.Length < 3)
        {
            return null;
        }

        byte sourceAddr = bmwFastTel[2];
        byte targetAddr = bmwFastTel[1];
        int dataOffset = CalculateDataOffset(bmwFastTel);
        if (dataOffset < 0)
        {
            return null;
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

    private bool SendEnetResponses(EnetTcpClientData enetTcpClientData, List<byte[]> responseList)
    {
        if (responseList == null || responseList.Count == 0)
        {
            log.ErrorFormat("SendEnetResponses Empty response list, ignoring");
            return false;
        }

        bool result = true;
        log.InfoFormat("SendEnetResponses Sending back Responses:{0}", responseList.Count);
        foreach (byte[] receiveData in responseList)
        {
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
            foreach (KeyValuePair<byte, Nr78Data> keyValuePair in enetTcpClientData.Nr78Dict)
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

    public static bool IsFunctionalAddress(byte[] bmwFastTel)
    {
        if (bmwFastTel == null || bmwFastTel.Length < 3)
        {
            return false;
        }

        return (bmwFastTel[0] & 0xC0) == 0xC0;
    }

    private void TcpThread()
    {
        log.InfoFormat("TcpThread started");

        for (; ; )
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
                                        bool funcAddress = IsFunctionalAddress(bmwFastTel);
                                        List<byte[]> cachedResponseList = null;
                                        if (funcAddress)
                                        {
                                            ProgrammingJobs.CacheType cacheType = _programmingJobs.CacheResponseType;
                                            if (cacheType == ProgrammingJobs.CacheType.None)
                                            {
                                                log.InfoFormat("TcpThread Caching disabled");
                                            }
                                            else
                                            {
                                                lock (_lockObject)
                                                {
                                                    _vehicleResponseDict.TryGetValue(bmwFastTel, out cachedResponseList);
                                                }

                                                if (cachedResponseList != null &&
                                                    cacheType == ProgrammingJobs.CacheType.NoResponse)
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
                                                if (!funcAddress)
                                                {
                                                    int nr78DictSize;
                                                    bool keyExists;
                                                    lock (enetTcpClientData.Nr78Dict)
                                                    {
                                                        keyExists = enetTcpClientData.Nr78Dict.ContainsKey(sourceAddr);
                                                        enetTcpClientData.Nr78Dict[sourceAddr] = new Nr78Data(sourceAddr, nr78Tel, Nr78FirstDelay);
                                                        nr78DictSize = enetTcpClientData.Nr78Dict.Count;
                                                    }

                                                    string nr78String = BitConverter.ToString(nr78Tel).Replace("-", " ");
                                                    log.InfoFormat("TcpThread Added NR78 Overwrite={0}, Nr78Size={1}, Data={2}", keyExists, nr78DictSize, nr78String);
                                                }
                                                else
                                                {
                                                    log.InfoFormat("TcpThread Not added NR78 functional Addr={0:X02}", sourceAddr);
                                                }

                                                log.InfoFormat("TcpThread Enqueued QueueSize={0}, Data={1}", queueSize, recString);
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

    public void VehicleResponseReceived(PsdzVehicleResponse vehicleResponse)
    {
        lock (_lockObject)
        {
            _vehicleResponses.Enqueue(vehicleResponse);
        }

        _vehicleThreadWakeEvent.Set();
    }

    public PsdzVehicleResponse VehicleResponseGet()
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

    public bool VehicleConnect()
    {
        ConnectTimeouts = null;

        if (VehicleConnectEvent == null)
        {
            log.ErrorFormat("VehicleConnectEvent is null");
            return false;
        }

        VehicleConnectEvent.Invoke(GetNextPacketId());

        PsdzVehicleResponse vehicleResponse = WaitForVehicleResponse();
        if (vehicleResponse != null)
        {
            if (vehicleResponse.ConnectTimeouts.HasValue)
            {
                ConnectTimeouts = vehicleResponse.ConnectTimeouts;
            }

            AppId = vehicleResponse.AppId;
            AdapterSerial = vehicleResponse.AdapterSerial;
            AdapterSerialValid = vehicleResponse.SerialValid;
            log.InfoFormat("VehicleConnect AppId={0}, AdapterSerial={1}, Valid={2}", AppId ?? string.Empty, AdapterSerial ?? string.Empty, AdapterSerialValid);
        }

        if (vehicleResponse == null || vehicleResponse.Error || !vehicleResponse.Valid || !vehicleResponse.Connected)
        {
            log.ErrorFormat("VehicleConnect Vehicle connect failed");
            return false;
        }

        return true;
    }

    public bool VehicleDisconnect()
    {
        if (VehicleDisconnectEvent == null)
        {
            log.ErrorFormat("VehicleDisconnect VehicleDisconnectEvent is null");
            return false;
        }

        VehicleDisconnectEvent.Invoke(GetNextPacketId());
        PsdzVehicleResponse vehicleResponse = WaitForVehicleResponse();
        if (vehicleResponse == null || vehicleResponse.Error || !vehicleResponse.Valid)
        {
            log.ErrorFormat("VehicleDisconnect Vehicle disconnect failed");
            return false;
        }

        return true;
    }

    public PsdzVehicleResponse WaitForVehicleResponse(VehicleResponseDelegate vehicleResponseDelegate = null, int timeout = VehicleReceiveTimeout)
    {
        log.InfoFormat("WaitForVehicleResponse Timeout={0}", timeout);
        ulong packetId = GetPacketId();
        long startTime = Stopwatch.GetTimestamp();
        for (; ; )
        {
            PsdzVehicleResponse vehicleResponse = VehicleResponseGet();
            if (vehicleResponse != null)
            {
                if (vehicleResponse.Id == packetId)
                {
                    if (vehicleResponse.Valid || vehicleResponse.Error)
                    {
                        log.InfoFormat("WaitForVehicleResponse Valid={0}, Error={1}", vehicleResponse.Valid, vehicleResponse.Error);
                        return vehicleResponse;
                    }
                }
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

    private void LogVehicleResponse(PsdzVehicleResponse vehicleResponse)
    {
        if (vehicleResponse == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(vehicleResponse.Request) || vehicleResponse.ResponseList == null || vehicleResponse.ResponseList.Count == 0)
        {
            log.ErrorFormat("LogVehicleResponse No Data");
            return;
        }

        lock (_vehicleLogLockObject)
        {
            try
            {
                if (_swVehicleLog == null)
                {
                    string dateString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                    string fileName = string.Format(CultureInfo.InvariantCulture, "Vehicle-{0}-[{1}].txt", dateString, _contextId);
                    string hostLogDir = null;
                    hostLogDir = _programmingJobs?.ProgrammingService?.GetPsdzServiceHostLogDir();
                    if (!string.IsNullOrEmpty(hostLogDir))
                    {
                        string logFile = Path.Combine(hostLogDir, fileName);
                        _swVehicleLog = new StreamWriter(logFile, true, Encoding.ASCII);
                    }
                }

                StringBuilder sb = new StringBuilder();
                byte[] requestData = EdiabasNet.HexToByteArray(vehicleResponse.Request);
                string requestString = BitConverter.ToString(requestData).Replace("-", " ");
                sb.Append(requestString);

                byte requestChecksum = EdCustomAdapterCommon.CalcChecksumBmwFast(requestData, 0, requestData.Length);
                sb.Append(string.Format(CultureInfo.InvariantCulture, " {0:X2}", requestChecksum));
                sb.Append(" : ");

                int index = 0;
                foreach (byte[] responseData in vehicleResponse.ResponseList)
                {
                    string responseString = BitConverter.ToString(responseData).Replace("-", " ");
                    if (index > 0)
                    {
                        sb.Append("  ");
                    }

                    sb.Append(responseString);

                    byte responseChecksum = EdCustomAdapterCommon.CalcChecksumBmwFast(responseData, 0, responseData.Length);
                    sb.Append(string.Format(CultureInfo.InvariantCulture, " {0:X2}", responseChecksum));
                    index++;
                }

                _swVehicleLog?.WriteLine(sb.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("LogVehicleResponse Exception: {0}", ex.Message);
            }
        }
    }

    private void PatchVehicleResponse(PsdzVehicleResponse vehicleResponse)
    {
        if (vehicleResponse == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(vehicleResponse.Request) || vehicleResponse.ResponseList == null || vehicleResponse.ResponseList.Count == 0)
        {
            log.ErrorFormat("PatchVehicleResponse No Data");
            return;
        }

        byte[] requestData = EdiabasNet.HexToByteArray(vehicleResponse.Request);
        int dataOffsetRequest = CalculateDataOffset(requestData);
        if (dataOffsetRequest >= 0)
        {
            byte serviceRequest = requestData[dataOffsetRequest];
            if (serviceRequest == 0x10 && vehicleResponse.ResponseList.Count == 1)
            {
                log.InfoFormat("PatchVehicleResponse Service={0:X02}", serviceRequest);

                byte[] responseData = vehicleResponse.ResponseList[0];
                int dataOffsetResponse = CalculateDataOffset(responseData);
                if (dataOffsetResponse >= 0)
                {
                    byte serviceResponse = responseData[dataOffsetResponse];
                    if (serviceResponse == (0x10 | 0x40) && responseData.Length == dataOffsetResponse + 6)
                    {
                        uint p2Max = 5000;
                        uint p2Start = 10000;
                        responseData[dataOffsetResponse + 2] = (byte)((p2Max >> 8) & 0xFF);
                        responseData[dataOffsetResponse + 3] = (byte)(p2Max & 0xFF);
                        responseData[dataOffsetResponse + 4] = (byte)((p2Start >> 8) & 0xFF);
                        responseData[dataOffsetResponse + 5] = (byte)(p2Start & 0xFF);

                        string responseOrgString = BitConverter.ToString(responseData).Replace("-", " ");
                        string responseNewString = BitConverter.ToString(responseData).Replace("-", " ");
                        log.InfoFormat("PatchVehicleResponse Patching From={0} To={1}", responseOrgString, responseNewString);
                        vehicleResponse.ResponseList[0] = responseData;
                    }
                }
            }
        }
    }

    private void CloseVehicleLog()
    {
        lock (_vehicleLogLockObject)
        {
            try
            {
                if (_swVehicleLog != null)
                {
                    _swVehicleLog.Close();
                    _swVehicleLog.Dispose();
                    _swVehicleLog = null;
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
        log.InfoFormat("VehicleThread started");

        VehicleResponseDictClear();
        VehicleResponseClear();
        ResetPacketId();

        if (!VehicleConnect())
        {
            log.ErrorFormat("VehicleThread Vehicle connect failed");
        }

        for (; ; )
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
                            bool funcAddress = IsFunctionalAddress(bmwFastTel);

                            string sendString = BitConverter.ToString(sendData).Replace("-", " ");
                            log.InfoFormat("VehicleThread Transmit Data={0}", sendString);

                            if (_programmingJobs.CacheClearRequired)
                            {
                                log.InfoFormat("VehicleThread Clearing response cache");
                                VehicleResponseDictClear();
                                _programmingJobs.CacheClearRequired = false;
                            }

                            if (VehicleSendEvent != null)
                            {
                                for (int retry = 0; retry < 3; retry++)
                                {
                                    if (!VehicleSendEvent.Invoke(GetNextPacketId(), bmwFastTel))
                                    {
                                        log.ErrorFormat("VehicleThread Vehicle send failed");
                                    }

                                    PsdzVehicleResponse vehicleResponse = WaitForVehicleResponse(() =>
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
                                            if (!VehicleConnect())
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
                                            string sendDataString = BitConverter.ToString(bmwFastTel).Replace("-", "");
                                            log.ErrorFormat("VehicleThread Vehicle transmit no response for Request={0}", sendDataString);

                                            if (funcAddress)
                                            {
                                                log.InfoFormat("VehicleThread Cache disable Request={0}", sendDataString);
                                                lock (_lockObject)
                                                {
                                                    _vehicleResponseDict[bmwFastTel] = new List<byte[]>();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LogVehicleResponse(vehicleResponse);
                                            PatchVehicleResponse(vehicleResponse);
                                            if (vehicleResponse.ConnectTimeouts.HasValue)
                                            {
                                                ConnectTimeouts = vehicleResponse.ConnectTimeouts;
                                            }

                                            if (funcAddress)
                                            {
                                                string sendDataString = BitConverter.ToString(bmwFastTel).Replace("-", "");
                                                log.InfoFormat("VehicleThread Caching Request={0}", sendDataString);
                                                lock (_lockObject)
                                                {
                                                    _vehicleResponseDict[bmwFastTel] = vehicleResponse.ResponseList;
                                                }
                                            }

                                            SendEnetResponses(enetTcpClientData, vehicleResponse.ResponseList);
                                        }
                                    }

                                    break;
                                }
                            }
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

        if (!VehicleDisconnect())
        {
            log.ErrorFormat("VehicleThread Vehicle disconnect failed");
        }

        ConnectTimeouts = null;
        VehicleResponseDictClear();
        CloseVehicleLog();
        log.InfoFormat("VehicleThread stopped");
    }

    public void ConnectVehicle(string istaFolder)
    {
        if (Cts != null)
        {
            return;
        }

        if (_programmingJobs.PsdzContext?.Connection != null)
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
        ConnectVehicleTask(istaFolder, remoteHost, false, AdditionalConnectTimeout).ContinueWith(task =>
        {
            if (!task.Result)
            {
                ReportError("ConnectVehicle failed");
                StopTcpListener();
            }
            else
            {
                PsdzContext psdzContext = _programmingJobs?.PsdzContext;
                if (psdzContext?.Connection != null)
                {
                    DetectedVin = psdzContext?.DetectVehicle?.Vin;
                }
            }

            Cts = null;
            return true;
        });
    }

    public async Task<bool> ConnectVehicleTask(string istaFolder, string remoteHost, bool useIcom, int addTimeout)
    {
        return await Task.Run(() => _programmingJobs.ConnectVehicle(Cts, istaFolder, remoteHost, useIcom, addTimeout)).ConfigureAwait(false);
    }

    public void DisconnectVehicle()
    {
        if (Cts != null)
        {
            return;
        }

        if (_programmingJobs.PsdzContext?.Connection == null)
        {
            return;
        }

        Cts = new CancellationTokenSource();
        DisconnectVehicleTask().ContinueWith(task =>
        {
            if (!task.Result)
            {
                ReportError("DisconnectVehicle failed");
            }

            Cts = null;
            StopTcpListener();
        });
    }

    public async Task<bool> DisconnectVehicleTask()
    {
        // ReSharper disable once ConvertClosureToMethodGroup
        return await Task.Run(() => _programmingJobs.DisconnectVehicle(Cts)).ConfigureAwait(false);
    }

    public int GetTelSendQueueSize()
    {
        if (_enetTcpChannels.Count == 0)
        {
            return -1;
        }

        int queueSize = 0;
        try
        {
            foreach (EnetTcpChannel enetTcpChannel in _enetTcpChannels)
            {
                if (enetTcpChannel.Control)
                {
                    continue;
                }

                foreach (EnetTcpClientData enetTcpClientData in enetTcpChannel.TcpClientList)
                {
                    if (enetTcpClientData.TcpClientStream == null)
                    {
                        continue;
                    }

                    lock (enetTcpClientData.RecPacketQueue)
                    {
                        queueSize += enetTcpClientData.RecPacketQueue.Count;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.ErrorFormat("TelSendQueueSizeEvent Exception: {0}", ex.Message);
            return -1;
        }

        return queueSize;
    }

    public void ReportError(string msg)
    {
        try
        {
            if (ReportErrorEvent == null)
            {
                log.ErrorFormat("ReportError Event is null");
                return;
            }

            ReportErrorEvent.Invoke(msg);
        }
        catch (Exception ex)
        {
            log.ErrorFormat("ReportError Exception: {0}", ex.Message);
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

    protected void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            log.InfoFormat("Disposing PsdzVehicleProxy");

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                CloseVehicleLog();
                StopTcpListener();

                if (_enetTcpMutex != null)
                {
                    _enetTcpMutex.Dispose();
                    _enetTcpMutex = null;
                }
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }
}