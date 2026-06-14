using Android.Nfc;
using EdiabasLib;
using PsdzRpcClient;
using PsdzRpcServer.Shared;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BmwDeepObd;

public class BmwRpcCoding
{
    private PsdzRpcClient.PsdzRpcClient _psdzRpcClient;
    private EdiabasProxyClient _ediabasProxyClient;
    private Task<bool> _startTask;
    private CancellationTokenSource _startCts;
    private object _startLock = new object();
    private object _statusLock = new object();

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
            lock (_statusLock)
            {
                return _psdzRpcClient;
            }
        }
        private set
        {
            lock (_statusLock)
            {
                _psdzRpcClient = value;
            }
        }
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
            };

            _psdzRpcClient.PingUpdated += (sender, pingDateTime) =>
            {
            };

            _psdzRpcClient.CallbackHandler.StartProgrammingCompleted += async (s, success) =>
            {
            };

            _psdzRpcClient.CallbackHandler.StopProgrammingCompleted += async (s, success) =>
            {
            };

            _psdzRpcClient.CallbackHandler.ConnectVehicleCompleted += async (s, connectArgs) =>
            {
            };

            _psdzRpcClient.CallbackHandler.DisconnectVehicleCompleted += async (s, success) =>
            {
            };

            _psdzRpcClient.CallbackHandler.VehicleFunctionsCompleted += async (s, vehicleArgs) =>
            {
            };

            _psdzRpcClient.CallbackHandler.UpdateStatus += async (s, message) =>
            {
            };

            _psdzRpcClient.CallbackHandler.UpdateProgress += (s, progressArgs) =>
            {
            };

            _psdzRpcClient.CallbackHandler.UpdateOptions += async (sender, optionArgs) =>
            {
            };

            _psdzRpcClient.CallbackHandler.UpdateOptionSelections += async (sender, swiRegisterEnum) =>
            {
            };

            _psdzRpcClient.CallbackHandler.ShowMessage += (sender, msgArgs) =>
            {
            };

            _psdzRpcClient.CallbackHandler.ShowMessageWait += (sender, msgArgs) =>
            {
            };

            _psdzRpcClient.CallbackHandler.TelSendQueueSize += (sender, queueArgs) =>
            {
                queueArgs.Result = -1; // Simulate no queue
            };

            _psdzRpcClient.CallbackHandler.ServiceInitialized += async (sender, serviceArgs) =>
            {
            };


            _psdzRpcClient.CallbackHandler.GetAppInfo += (sender, infoArgs) =>
            {
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
}
