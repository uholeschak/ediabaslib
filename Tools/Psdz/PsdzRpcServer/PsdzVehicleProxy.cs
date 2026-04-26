using log4net;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using EdiabasLib;

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
    private readonly Dictionary<string, List<string>> _vehicleResponseDict = new Dictionary<string, List<string>>();

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