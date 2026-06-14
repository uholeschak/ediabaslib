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
    private bool _disposed;
    private ActivityCommon _activityCommon;
    private PsdzRpcClient.PsdzRpcClient _psdzRpcClient;
    private EdiabasProxyClient _ediabasProxyClient;
    private Task<bool> _startTask;
    private CancellationTokenSource _startCts;
    private object _startLock = new object();
    private object _statusLock = new object();
    private object _dataLock = new object();

#if DEBUG
    private static readonly string Tag = typeof(BmwRpcCoding).FullName;
#endif

    private bool _taskActive;
    public bool TaskActive
    {
        get
        {
            lock (_statusLock)
            {
                return _taskActive;
            }
        }
        private set
        {
            lock (_statusLock)
            {
                _taskActive = value;
            }
        }
    }

    public PsdzRpcClient.PsdzRpcClient PsdzRpcClient
    {
        get
        {
            lock (_dataLock)
            {
                return _psdzRpcClient;
            }
        }
        private set
        {
            lock (_dataLock)
            {
                _psdzRpcClient = value;
            }
        }
    }

    string _adapterSerialValid;
    public string AdapterSerialValid
    {
        get
        {
            lock (_dataLock)
            {
                return _adapterSerialValid;
            }
        }
        private set
        {
            lock (_dataLock)
            {
                _adapterSerialValid = value;
            }
        }
    }

    public BmwRpcCoding(Context context)
    {
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
            };


            _psdzRpcClient.CallbackHandler.GetAppInfo += (sender, infoArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                string adapterSerial = ActivityCommon.LastAdapterSerial ?? string.Empty;
                string validSerial;
                lock (_dataLock)
                {
                    validSerial = AdapterSerialValid ?? string.Empty;
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
