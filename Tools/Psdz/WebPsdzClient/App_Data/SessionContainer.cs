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

        private TcpListener _tcpListenerDiag;
        private int _tcpListenerDiagPort;
        private TcpClient _tcpClientDiag;
        private NetworkStream _tcpClientDiagStream;
        private TcpListener _tcpListenerControl;
        private int _tcpListenerControlPort;
        private TcpClient _tcpClientControl;
        private NetworkStream _tcpClientControlStream;
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
                if (_tcpListenerDiag == null)
                {
                    _tcpListenerDiagPort = 0;
                    _tcpListenerDiag = new TcpListener(IPAddress.Loopback, 0);
                    _tcpListenerDiag.Start();
                    IPEndPoint ipEndPoint = _tcpListenerDiag.LocalEndpoint as IPEndPoint;
                    if (ipEndPoint != null)
                    {
                        _tcpListenerDiagPort = ipEndPoint.Port;
                    }

                    log.InfoFormat("StartTcpListener Diag Port: {0}", _tcpListenerDiagPort);
                }

                if (_tcpListenerControl == null)
                {
                    _tcpListenerControlPort = 0;
                    _tcpListenerControl = new TcpListener(IPAddress.Loopback, 0);
                    _tcpListenerControl.Start();
                    IPEndPoint ipEndPoint = _tcpListenerControl.LocalEndpoint as IPEndPoint;
                    if (ipEndPoint != null)
                    {
                        _tcpListenerControlPort = ipEndPoint.Port;
                    }

                    log.InfoFormat("StartTcpListener Control Port: {0}", _tcpListenerControlPort);
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
                TcpClientDiagDisconnect();
                TcpClientControlDisconnect();

                if (_tcpListenerDiag != null)
                {
                    log.ErrorFormat("StopTcpListener Stopping Diag Port: {0}", _tcpListenerDiagPort);
                    _tcpListenerDiag.Stop();
                    _tcpListenerDiag = null;
                    _tcpListenerDiagPort = 0;
                }

                if (_tcpListenerControl != null)
                {
                    log.ErrorFormat("StopTcpListener Stopping Diag Port: {0}", _tcpListenerControlPort);
                    _tcpListenerControl.Stop();
                    _tcpListenerControl = null;
                    _tcpListenerControlPort = 0;
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

        private bool TcpClientDiagDisconnect()
        {
            try
            {
                if (_tcpClientDiagStream != null)
                {
                    _tcpClientDiagStream.Close();
                    _tcpClientDiagStream = null;
                }

                if (_tcpClientDiag != null)
                {
                    _tcpClientDiag.Close();
                    _tcpClientDiag = null;
                    log.InfoFormat("TcpClientDiagDisconnect port: {0}", _tcpListenerDiagPort);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpClientDiagDisconnect Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool TcpClientControlDisconnect()
        {
            try
            {
                if (_tcpClientControlStream != null)
                {
                    _tcpClientControlStream.Close();
                    _tcpClientControlStream = null;
                }

                if (_tcpClientControl != null)
                {
                    _tcpClientControl.Close();
                    _tcpClientControl = null;
                    log.InfoFormat("TcpClientControlDisconnect port: {0}", _tcpListenerControlPort);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("TcpClientControlDisconnect Exception: {0}", ex.Message);
                return false;
            }

            return true;
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

                try
                {
                    if (_tcpClientDiag != null)
                    {
                        if (!_tcpClientDiag.Connected)
                        {
                            TcpClientDiagDisconnect();
                        }
                    }

                    if (_tcpClientDiag == null && _tcpListenerDiag.Pending())
                    {
                        _tcpClientDiag = _tcpListenerDiag.AcceptTcpClient();
                        _tcpClientDiag.SendBufferSize = TcpSendBufferSize;
                        _tcpClientDiag.SendTimeout = TcpSendTimeout;
                        _tcpClientDiag.NoDelay = true;
                        _tcpClientDiagStream = _tcpClientDiag.GetStream();
                        log.InfoFormat("TcpThread Accept diag port: {0}", _tcpListenerDiagPort);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("TcpThread Accept Exception: {0}", ex.Message);
                }

                try
                {
                    if (_tcpClientControl != null)
                    {
                        if (!_tcpClientControl.Connected)
                        {
                            TcpClientControlDisconnect();
                        }
                    }

                    if (_tcpClientControl == null && _tcpListenerControl.Pending())
                    {
                        _tcpClientControl = _tcpListenerControl.AcceptTcpClient();
                        _tcpClientControl.SendBufferSize = TcpSendBufferSize;
                        _tcpClientControl.SendTimeout = TcpSendTimeout;
                        _tcpClientControl.NoDelay = true;
                        _tcpClientControlStream = _tcpClientControl.GetStream();
                        log.InfoFormat("TcpThread Accept control port: {0}", _tcpListenerControlPort);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("TcpThread Accept Exception: {0}", ex.Message);
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
