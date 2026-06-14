using Android.Content;
using EdiabasLib;
using PsdzRpcClient;
using PsdzRpcServer.Shared;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BmwDeepObd;

public class BmwRpcCoding : IDisposable
{
    public class StatusData
    {
        public StatusData()
        {
            TaskActive = false;
            CodingRpcUrl = string.Empty;
            CodingRpcUrlTest = string.Empty;
            CodingRpcEnableIpv6 = false;
            DayString = string.Empty;
            ValidSerial = string.Empty;
            Vin = string.Empty;
            LicenseValid = false;
            Url = string.Empty;
            IstaFolder = string.Empty;
            TraceDir = string.Empty;
            TraceActive = true;
            TraceAppend = false;
            CommErrorsOccurred = false;
            StatusMessage = string.Empty;
        }

        public bool TaskActive { get; set; }
        public string CodingRpcUrl { get; set; }
        public string CodingRpcUrlTest { get; set; }
        public bool CodingRpcEnableIpv6 { get; set; }
        public string DayString { get; set; }
        public string ValidSerial { get; set; }
        public string Vin { get; set; }
        public bool LicenseValid { get; set; }
        public string Url { get; set; }
        public string IstaFolder { get; set; }
        public string TraceDir { get; set; }
        public bool TraceActive { get; set; }
        public bool TraceAppend { get; set; }
        public bool CommErrorsOccurred { get; set; }
        public string StatusMessage { get; set; }
    }

#if DEBUG
    private static readonly string Tag = typeof(BmwRpcCoding).FullName;
#endif

    private bool _disposed;
    private ActivityCommon _activityCommon;
    private PsdzRpcClient.PsdzRpcClient _psdzRpcClient;
    private EdiabasProxyClient _ediabasProxyClient;
    private StatusData _statusData;
    private Task<bool> _startTask;
    private CancellationTokenSource _startCts;
    private object _startLock = new object();
    public object StatusLock { get; private set; } = new object();

    public PsdzRpcClient.PsdzRpcClient PsdzRpcClient
    {
        get
        {
            lock (StatusLock)
            {
                return _psdzRpcClient;
            }
        }
        private set
        {
            lock (StatusLock)
            {
                _psdzRpcClient = value;
            }
        }
    }

    public BmwRpcCoding(Context context)
    {
        _statusData = new StatusData();
        _activityCommon = new ActivityCommon(context);
    }

    private bool CreateRpcClient(ActivityCommon activityCommon, EdiabasNet ediabas)
    {
        try
        {
            AndroidLogWriter logWriter = null;
#if DEBUG
            logWriter = new AndroidLogWriter(Tag);
#endif
            _psdzRpcClient = new PsdzRpcClient.PsdzRpcClient(logWriter,
                PsdzRpcServiceConstants.CaCertFile, PsdzRpcServiceConstants.ClientPfxFile, Assembly.GetExecutingAssembly());
            _psdzRpcClient.ClientConnected += async (sender, connected) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.PingUpdated += (sender, pingDateTime) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.StartProgrammingCompleted += async (s, success) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.StopProgrammingCompleted += async (s, success) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.ConnectVehicleCompleted += async (s, connectArgs) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.DisconnectVehicleCompleted += async (s, success) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.VehicleFunctionsCompleted += async (s, vehicleArgs) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateStatus += async (s, message) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateProgress += (s, progressArgs) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateOptions += async (sender, optionArgs) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateOptionSelections += async (sender, swiRegisterEnum) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.ShowMessage += (sender, msgArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                lock (StatusLock)
                {
                    _statusData.StatusMessage = msgArgs.Message;
                }
                msgArgs.Result = true;
            };

            _psdzRpcClient.CallbackHandler.ShowMessageWait += (sender, msgArgs) =>
            {
                if (_disposed)
                {
                    return;
                }
            };

            _psdzRpcClient.CallbackHandler.TelSendQueueSize += (sender, queueArgs) =>
            {
                queueArgs.Result = -1; // Simulate no queue
            };

            _psdzRpcClient.CallbackHandler.ServiceInitialized += async (sender, serviceArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    if (_psdzRpcClient.RpcService != null && !serviceArgs.LoggingInitialized)
                    {
                        string logFile = Path.Combine(serviceArgs.HostLogDir, "PsdzAppClient.log");

                        bool result = await _psdzRpcClient.RpcService.SetupLog4Net(logFile).ConfigureAwait(false);
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "Setup log4net result: {0}", result);

                        bool resetResult = await _psdzRpcClient.RpcService.ResetStarterGuard().ConfigureAwait(false);
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ResetStarterGuard result: {0}", resetResult);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            };


            _psdzRpcClient.CallbackHandler.GetAppInfo += (sender, infoArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                string adapterSerial = ActivityCommon.LastAdapterSerial ?? string.Empty;
                string validSerial;
                lock (StatusLock)
                {
                    validSerial = _statusData.ValidSerial ?? string.Empty;
                }

                bool adapterSerialValid = false;
                if (!string.IsNullOrEmpty(validSerial) && string.Compare(validSerial, adapterSerial, StringComparison.Ordinal) == 0)
                {
                    adapterSerialValid = true;
                }

                infoArgs.AppId = ActivityCommon.AppId;
                infoArgs.AdapterSerial = adapterSerial;
                infoArgs.AdapterSerialValid = adapterSerialValid;
            };

            _ediabasProxyClient = new EdiabasProxyClient(ediabas);
            _ediabasProxyClient.VehicleResponseEvent += (vehicleResponse) =>
            {
                return Task.Run(() => _psdzRpcClient.RpcService.SetVehicleResponse(vehicleResponse)).GetAwaiter().GetResult();
            };

            _ediabasProxyClient.MessageEvent += (messageType, message) =>
            {
                if (messageType == EdiabasProxyClient.MessageType.Error)
                {
                    lock (StatusLock)
                    {
                        _statusData.CommErrorsOccurred = true;
                    }
                }

                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "EdiabasProxyClient: Type={0}, Message={1}", messageType.ToString(), message);
            };

            _psdzRpcClient.CallbackHandler.VehicleConnect += (s, id) =>
            {
                EdiabasProxyClient proxy = _ediabasProxyClient;
                if (proxy == null || proxy.IsDisposed)
                {
                    return;
                }
                proxy.VehicleConnect(id);
            };

            _psdzRpcClient.CallbackHandler.VehicleDisconnect += (s, id) =>
            {
                EdiabasProxyClient proxy = _ediabasProxyClient;
                if (proxy == null || proxy.IsDisposed)
                {
                    return;
                }
                proxy.VehicleDisconnect(id);
            };

            _psdzRpcClient.CallbackHandler.VehicleSend += (s, sendArgs) =>
            {
                EdiabasProxyClient proxy = _ediabasProxyClient;
                if (proxy == null || proxy.IsDisposed)
                {
                    return;
                }
                proxy.VehicleSend(sendArgs.Id, sendArgs.Data);
            };

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public class AndroidLogWriter : TextWriter
    {
        private readonly string _tag;

        public AndroidLogWriter(string tag)
        {
            _tag = tag;
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void WriteLine(string value)
        {
            Android.Util.Log.Info(_tag, value ?? string.Empty);
        }

        public override void Write(string value)
        {
            Android.Util.Log.Info(_tag, value ?? string.Empty);
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
                _activityCommon.Dispose();
            }
        }

        // Note disposing has been done.
        _disposed = true;
    }
}
