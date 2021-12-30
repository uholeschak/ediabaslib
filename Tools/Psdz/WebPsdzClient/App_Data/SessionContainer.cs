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
        private class BmwTcpClientData
        {
            public BmwTcpClientData(BmwTcpChannel bmwTcpChannel, int index)
            {
                BmpBmwTcpChannel = bmwTcpChannel;
                Index = index;
                TcpClientConnection = null;
                TcpClientStream = null;
                LastTcpRecTick = DateTime.MinValue.Ticks;
                LastTcpSendTick = DateTime.MinValue.Ticks;
                ConnectFailure = false;
                DataBuffer = new byte[0x200];
            }

            public readonly BmwTcpChannel BmpBmwTcpChannel;
            public readonly int Index;
            public TcpClient TcpClientConnection;
            public NetworkStream TcpClientStream;
            public long LastTcpRecTick;
            public long LastTcpSendTick;
            public bool ConnectFailure;
            public byte[] DataBuffer;
        }

        private class BmwTcpChannel
        {
            public BmwTcpChannel()
            {
                DiagPort = 0;
                ControlPort = 0;
                TcpClientDiagList = new List<BmwTcpClientData>();
                TcpClientControlList = new List<BmwTcpClientData>();
                for (int i = 0; i < 10; i++)
                {
                    TcpClientDiagList.Add(new BmwTcpClientData(this, i));
                    TcpClientControlList.Add(new BmwTcpClientData(this, i));
                }
            }

            public int DiagPort;
            public int ControlPort;
            public TcpListener TcpServerDiag;
            public readonly List<BmwTcpClientData> TcpClientDiagList;
            public TcpListener TcpServerControl;
            public readonly List<BmwTcpClientData> TcpClientControlList;
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

        private BmwTcpChannel _bmwTcpChannel;
        private Thread _tcpThread;
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
                if (_bmwTcpChannel == null)
                {
                    _bmwTcpChannel = new BmwTcpChannel();
                }

                if (_bmwTcpChannel.TcpServerDiag == null)
                {
                    _bmwTcpChannel.DiagPort = 0;
                    _bmwTcpChannel.TcpServerDiag = new TcpListener(IPAddress.Loopback, 0);
                    _bmwTcpChannel.TcpServerDiag.Start();
                    IPEndPoint ipEndPoint = _bmwTcpChannel.TcpServerDiag.LocalEndpoint as IPEndPoint;
                    if (ipEndPoint != null)
                    {
                        _bmwTcpChannel.DiagPort = ipEndPoint.Port;
                    }

                    log.InfoFormat("StartTcpListener Diag Port: {0}", _bmwTcpChannel.DiagPort);
                }

                if (_bmwTcpChannel.TcpServerControl == null)
                {
                    _bmwTcpChannel.ControlPort = 0;
                    _bmwTcpChannel.TcpServerControl = new TcpListener(IPAddress.Loopback, 0);
                    _bmwTcpChannel.TcpServerControl.Start();
                    IPEndPoint ipEndPoint = _bmwTcpChannel.TcpServerControl.LocalEndpoint as IPEndPoint;
                    if (ipEndPoint != null)
                    {
                        _bmwTcpChannel.ControlPort = ipEndPoint.Port;
                    }

                    log.InfoFormat("StartTcpListener Control Port: {0}", _bmwTcpChannel.ControlPort);
                }

                if (_tcpThread == null)
                {
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
                if (_bmwTcpChannel == null)
                {
                    return true;
                }

                TcpClientDiagDisconnect();
                TcpClientControlDisconnect();

                if (_bmwTcpChannel.TcpServerDiag != null)
                {
                    log.ErrorFormat("StopTcpListener Stopping diag port: {0}", _bmwTcpChannel.DiagPort);
                    _bmwTcpChannel.TcpServerDiag.Stop();
                    _bmwTcpChannel.TcpServerDiag = null;
                    _bmwTcpChannel.DiagPort = 0;
                }

                if (_bmwTcpChannel.TcpServerControl != null)
                {
                    log.ErrorFormat("StopTcpListener Stopping diag port: {0}", _bmwTcpChannel.ControlPort);
                    _bmwTcpChannel.TcpServerControl.Stop();
                    _bmwTcpChannel.TcpServerControl = null;
                    _bmwTcpChannel.ControlPort = 0;
                }

                if (_tcpThread != null)
                {
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

        private void TcpClientDiagDisconnect()
        {
            if (_bmwTcpChannel == null)
            {
                return;
            }

            foreach (BmwTcpClientData bmwTcpClientData in _bmwTcpChannel.TcpClientDiagList)
            {
                TcpClientDisconnect(bmwTcpClientData, _bmwTcpChannel.DiagPort);
            }
        }

        private void TcpClientControlDisconnect()
        {
            if (_bmwTcpChannel == null)
            {
                return;
            }

            foreach (BmwTcpClientData bmwTcpClientData in _bmwTcpChannel.TcpClientControlList)
            {
                TcpClientDisconnect(bmwTcpClientData, _bmwTcpChannel.ControlPort);
            }
        }

        private bool TcpClientConnect(TcpListener bmwTcpListener, BmwTcpClientData bmwTcpClientData, int port)
        {
            try
            {
                if (bmwTcpClientData == null)
                {
                    return true;
                }

                if (bmwTcpClientData.ConnectFailure)
                {
                    TcpClientDisconnect(bmwTcpClientData, port);
                }

                if (!IsTcpClientConnected(bmwTcpClientData.TcpClientConnection))
                {
                    TcpClientDisconnect(bmwTcpClientData, port);
                    if (!bmwTcpListener.Pending())
                    {
                        return true;
                    }

                    bmwTcpClientData.ConnectFailure = false;
                    bmwTcpClientData.TcpClientConnection = bmwTcpListener.AcceptTcpClient();
                    bmwTcpClientData.TcpClientConnection.SendBufferSize = TcpSendBufferSize;
                    bmwTcpClientData.TcpClientConnection.SendTimeout = TcpSendTimeout;
                    bmwTcpClientData.TcpClientConnection.NoDelay = true;
                    bmwTcpClientData.TcpClientStream = bmwTcpClientData.TcpClientConnection.GetStream();
                    TcpReceive(bmwTcpClientData);
                    log.InfoFormat("TcpThread Accept port: {0}", port);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpClientDiagDisconnect Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool TcpClientDisconnect(BmwTcpClientData bmwTcpClientData, int port)
        {
            try
            {
                if (bmwTcpClientData == null)
                {
                    return true;
                }

                if (bmwTcpClientData.TcpClientStream != null)
                {
                    bmwTcpClientData.TcpClientStream.Close();
                    bmwTcpClientData.TcpClientStream = null;
                }

                if (bmwTcpClientData.TcpClientConnection != null)
                {
                    bmwTcpClientData.TcpClientConnection.Close();
                    bmwTcpClientData.TcpClientConnection = null;
                    log.ErrorFormat("TcpClientDiagDisconnect Client closed port: {0}", port);
                }

                bmwTcpClientData.ConnectFailure = false;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpClientDiagDisconnect Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool TcpReceive(BmwTcpClientData bmwTcpClientData)
        {
            try
            {
                bmwTcpClientData.TcpClientStream.BeginRead(bmwTcpClientData.DataBuffer, 0, bmwTcpClientData.DataBuffer.Length, TcpReceiver, bmwTcpClientData);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpReceive Exception: {0}", ex.Message);
                bmwTcpClientData.ConnectFailure = true;
                return false;
            }
        }

        private void TcpReceiver(IAsyncResult ar)
        {
            try
            {
                if (ar.AsyncState is BmwTcpClientData bmwTcpClientData)
                {
                    int length = bmwTcpClientData.TcpClientStream.EndRead(ar);
                    if (length > 0)
                    {
                        byte[] data = new byte[length];
                        Array.Copy(bmwTcpClientData.DataBuffer, data, length);
                    }

                    TcpReceive(bmwTcpClientData);
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

        private void TcpThread()
        {
            log.InfoFormat("TcpThread started");
            for (;;)
            {
                if (_tcpThreadStopEvent.WaitOne(100))
                {
                    break;
                }

                if (_bmwTcpChannel != null)
                {
                    try
                    {
                        if (_bmwTcpChannel.TcpServerDiag.Pending())
                        {
                            foreach (BmwTcpClientData bmwTcpClientData in _bmwTcpChannel.TcpClientDiagList)
                            {
                                TcpClientConnect(_bmwTcpChannel.TcpServerDiag, bmwTcpClientData, _bmwTcpChannel.DiagPort);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("TcpThread Accept Exception: {0}", ex.Message);
                    }

                    try
                    {
                        if (_bmwTcpChannel.TcpServerControl.Pending())
                        {
                            foreach (BmwTcpClientData bmwTcpClientData in _bmwTcpChannel.TcpClientControlList)
                            {
                                TcpClientConnect(_bmwTcpChannel.TcpServerDiag, bmwTcpClientData, _bmwTcpChannel.ControlPort);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("TcpThread Accept Exception: {0}", ex.Message);
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
