using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PsdzClient;
using PsdzClient.Programing;

namespace WebPsdzClient.App_Data
{
    public class SessionContainer : IDisposable
    {
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
            }

            public readonly EnetTcpChannel EnetTcpChannel;
            public readonly int Index;
            public TcpClient TcpClientConnection;
            public NetworkStream TcpClientStream;
            public bool ConnectFailure;
            public byte[] DataBuffer;
            public Queue<byte> RecQueue;
            public Queue<byte> SendQueue;
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

        public delegate void UpdateDisplayDelegate();
        public delegate void UpdateOptionsDelegate();
        public ProgrammingJobs ProgrammingJobs { get; private set; }
        public bool RefreshOptions { get; set; }

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

        private UpdateDisplayDelegate _updateDisplay;
        public UpdateDisplayDelegate UpdateDisplayFunc
        {
            get
            {
                lock (_lockObject)
                {
                    return _updateDisplay;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _updateDisplay = value;
                }
            }
        }

        private UpdateOptionsDelegate _updateOptions;
        public UpdateOptionsDelegate UpdateOptionsFunc
        {
            get
            {
                lock (_lockObject)
                {
                    return _updateOptions;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _updateOptions = value;
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

        private List<EnetTcpChannel> _enetTcpChannels = new List<EnetTcpChannel>();
        private Thread _tcpThread;
        private bool _stopThread;
        private AutoResetEvent _tcpThreadStopEvent = new AutoResetEvent(false);
        private bool _disposed;
        private readonly object _lockObject = new object();
        private static readonly ILog log = LogManager.GetLogger(typeof(_Default));

        private const int TcpSendBufferSize = 1400;
        private const int TcpSendTimeout = 5000;

        public SessionContainer(string dealerId)
        {
            ProgrammingJobs = new ProgrammingJobs(dealerId);
            ProgrammingJobs.UpdateStatusEvent += UpdateStatus;
            ProgrammingJobs.UpdateOptionsEvent += UpdateOptions;
            ProgrammingJobs.ProgressEvent += UpdateProgress;
            StatusText = string.Empty;
            ProgressText = string.Empty;
        }

        private bool StartTcpListener()
        {
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

                        log.InfoFormat("StartTcpListener Port: {0}, Control: {1}", enetTcpChannel.ServerPort, enetTcpChannel.Control);
                    }
                }

                if (_tcpThread == null)
                {
                    _stopThread = false;
                    _tcpThreadStopEvent.Reset();
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
                if (_enetTcpChannels.Count == 0)
                {
                    return true;
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

                if (_tcpThread != null)
                {
                    _stopThread = true;
                    _tcpThreadStopEvent.Set();
                    if (!_tcpThread.Join(5000))
                    {
                        log.ErrorFormat("StopTcpListener Stopping thread failed");
                    }

                    _tcpThread = null;
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("StopTcpListener Exception: {0}", ex.Message);
            }

            return false;
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

                if (enetTcpClientData.ConnectFailure)
                {
                    TcpClientDisconnect(enetTcpClientData);
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
                }

                if (enetTcpClientData.TcpClientConnection != null)
                {
                    enetTcpClientData.TcpClientConnection.Close();
                    enetTcpClientData.TcpClientConnection = null;
                    log.ErrorFormat("TcpClientDiagDisconnect Client closed Port: {0}, Control: {1}", enetTcpClientData.EnetTcpChannel.ServerPort, enetTcpClientData.EnetTcpChannel.Control);
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
            try
            {
                if (ar.AsyncState is EnetTcpClientData enetTcpClientData)
                {
                    int length = enetTcpClientData.TcpClientStream.EndRead(ar);
                    if (length > 0)
                    {
                        lock (enetTcpClientData.RecQueue)
                        {
                            for (int i = 0; i < length; i++)
                            {
                                enetTcpClientData.RecQueue.Enqueue(enetTcpClientData.DataBuffer[i]);
                            }
                        }

                        enetTcpClientData.EnetTcpChannel.RecEvent.Set();
                    }

                    TcpReceive(enetTcpClientData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpReceiver Exception: {0}", ex.Message);
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

        private void TcpThread()
        {
            log.InfoFormat("TcpThread started");
            for (;;)
            {
                WaitHandle[] waitHandles = new WaitHandle[1 + _enetTcpChannels.Count * 2];
                int index = 0;

                waitHandles[index++] = _tcpThreadStopEvent;
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
                            if (enetTcpChannel.TcpServer.Pending())
                            {
                                TcpClientConnect(enetTcpChannel.TcpServer, enetTcpClientData);
                            }

                            if (enetTcpClientData.TcpClientStream != null)
                            {
                                byte[] data;
                                lock (enetTcpClientData.SendQueue)
                                {
                                    data = enetTcpClientData.SendQueue.ToArray();
                                    enetTcpClientData.SendQueue.Clear();
                                }

                                if (data.Length > 0)
                                {
                                    WriteNetworkStream(enetTcpClientData, data, 0, data.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("TcpThread WriteNetworkStream Exception: {0}", ex.Message);
                            enetTcpClientData.ConnectFailure = true;
                        }
                    }
                }
            }
            log.InfoFormat("TcpThread stopped");
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

        public void UpdateDisplay()
        {
            UpdateDisplayFunc?.Invoke();
        }

        public void UpdateOptions(Dictionary<PdszDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict)
        {
            OptionsDict = optionsDict;
            ProgrammingJobs.SelectedOptions = new List<ProgrammingJobs.OptionsItem>();
            UpdateCurrentOptions();
        }

        private void UpdateCurrentOptions()
        {
            bool vehicleConnected = ProgrammingJobs.PsdzContext?.Connection != null;
            if (!vehicleConnected)
            {
                OptionsDict = null;
                SelectedSwiRegister = null;
            }

            UpdateOptionsFunc?.Invoke();
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
                    text = "Processing ...";
                }
            }

            if (ProgressText != text)
            {
                ProgressText = text;
                UpdateDisplayFunc.Invoke();
            }
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

        public void StopProgrammingService()
        {
            if (TaskActive)
            {
                return;
            }

            StopProgrammingServiceTask().ContinueWith(task =>
            {
                TaskActive = false;
                UpdateCurrentOptions();
                UpdateDisplay();
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> StopProgrammingServiceTask()
        {
            return await Task.Run(() => ProgrammingJobs.StopProgrammingService(Cts)).ConfigureAwait(false);
        }

        public void ConnectVehicle(string istaFolder, string ipAddress, bool icomConnection)
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

            Cts = new CancellationTokenSource();
            ConnectVehicleTask(istaFolder, ipAddress, icomConnection).ContinueWith(task =>
            {
                TaskActive = false;
                Cts = null;
                UpdateCurrentOptions();
                UpdateDisplay();
            });

            TaskActive = true;
            UpdateDisplay();
        }

        public async Task<bool> ConnectVehicleTask(string istaFolder, string ipAddress, bool icomConnection)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ProgrammingJobs.ConnectVehicle(Cts, istaFolder, ipAddress, icomConnection)).ConfigureAwait(false);
        }

        public void DisconnectVehicle(UpdateDisplayDelegate updateHandler)
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

        public void VehicleFunctions(UpdateDisplayDelegate updateHandler, ProgrammingJobs.OperationType operationType)
        {
            if (TaskActive)
            {
                return;
            }

            if (ProgrammingJobs.PsdzContext?.Connection == null)
            {
                return;
            }

            Cts = new CancellationTokenSource();
            VehicleFunctionsTask(operationType).ContinueWith(task =>
            {
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

                StopProgrammingService();

                if (ProgrammingJobs != null)
                {
                    ProgrammingJobs.Dispose();
                    ProgrammingJobs = null;
                }

                StopTcpListener();

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
