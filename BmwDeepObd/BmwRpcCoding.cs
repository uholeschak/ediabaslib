using Android.Content;
using EdiabasLib;
using PsdzRpcClient;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
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
            Vin = string.Empty;
            LicenseValid = false;
            Url = string.Empty;
            IstaFolder = string.Empty;
            CommErrorsOccurred = false;
            SelectedSwiRegister = null;
            StatusInfo = null;
            StatusOptionTypes = null;
            RpcListItems = null;
            StatusMessage = string.Empty;
            ShowMessage = null;
            ShowMessageWait = null;
            StatusUpdateTime = null;
            RpcClientConnected = false;
            ProgressIndeterminate = false;
            ProgressPercent = 0;
        }

        public StatusData Clone()
        {
            StatusData clone = (StatusData)MemberwiseClone();
            clone.StatusOptionTypes = StatusOptionTypes != null ? new List<PsdzRpcOptionType>(StatusOptionTypes) : null;
            clone.RpcListItems = RpcListItems != null ? new List<PsdzRpcOptionItem>(RpcListItems) : null;
            return clone;
        }

        public bool TaskActive { get; set; }
        public string Vin { get; set; }
        public bool LicenseValid { get; set; }
        public string Url { get; set; }
        public string IstaFolder { get; set; }
        public bool CommErrorsOccurred { get; set; }
        public PsdzRpcSwiRegisterEnum? SelectedSwiRegister { get; set; }
        public PsdzRpcStatusInfo StatusInfo { get; set; }
        public List<PsdzRpcOptionType> StatusOptionTypes { get; set; }
        public List<PsdzRpcOptionItem> RpcListItems { get; set; }
        public string StatusMessage { get; set; }
        public string ShowMessage { get; set; }
        public ShowMessageEventArgs ShowMessageWait { get; set; }
        public DateTime? StatusUpdateTime { get; set; }
        public bool RpcClientConnected { get; set; }
        public bool ProgressIndeterminate { get; set; }
        public int ProgressPercent { get; set; }
    }

    public delegate void UpdateDisplayDelegate(StatusData statusData);
    public delegate void UpdateProgressDelegate(int percent, bool indeterminate);
    public delegate void UpdateTimeDelegate(DateTime? pingDateTime);
    public event UpdateDisplayDelegate UpdateDisplayEvent;
    public event UpdateProgressDelegate UpdateProgressEvent;
    public event UpdateTimeDelegate UpdateTimeEvent;

#if DEBUG
    private static readonly string Tag = typeof(BmwRpcCoding).FullName;
#endif

    private bool _disposed;
    private ActivityCommon _activityCommon;
    private PsdzRpcClient.PsdzRpcClient _psdzRpcClient;
    private EdiabasProxyClient _ediabasProxyClient;
    private StatusData _statusData;
    private string _validSerial;

    public object StatusLock { get; } = new object();
    public PsdzRpcClient.PsdzRpcClient PsdzRpcClient { get => _psdzRpcClient; }
    public EdiabasProxyClient EdiabasProxyClient { get => _ediabasProxyClient; }

    public BmwRpcCoding(Context appContext)
    {
        lock (StatusLock)
        {
            _statusData = new StatusData();
        }
        _activityCommon = new ActivityCommon(appContext);
    }

    public bool CreateRpcClient(EdiabasNet ediabas)
    {
        try
        {
            if (_psdzRpcClient != null)
            {
                ediabas.Dispose();
                return true;
            }

            AndroidLogWriter logWriter = null;
#if DEBUG
            logWriter = new AndroidLogWriter(Tag);
#endif
            lock (StatusLock)
            {
                _statusData = new StatusData();
            }

            _psdzRpcClient = new PsdzRpcClient.PsdzRpcClient(logWriter,
                PsdzRpcServiceConstants.CaCertFile, PsdzRpcServiceConstants.ClientPfxFile, Assembly.GetExecutingAssembly());
            _psdzRpcClient.ClientConnected += async (sender, connected) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _ediabasProxyClient.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ClientConnected: Connected={0}",
                        connected);

                    lock (StatusLock)
                    {
                        _statusData.TaskActive = false;
                        _statusData.RpcClientConnected = connected;
                    }

                    if (connected)
                    {
                        _activityCommon.SetLock(ActivityCommon.LockType.ScreenDim);
                        lock (StatusLock)
                        {
                            _statusData.StatusMessage = string.Empty;
                        }
                        await RpcClientUpdateDisplay().ConfigureAwait(false);
                    }
                    else
                    {
                        _activityCommon.SetLock(ActivityCommon.LockType.None);
                        lock (StatusLock)
                        {
                            _statusData.StatusInfo = null;
                            _statusData.StatusOptionTypes = null;
                            _statusData.RpcListItems = null;
                            _statusData.StatusUpdateTime = null;
                            _statusData.CommErrorsOccurred = true;
                        }
                    }

                    lock (StatusLock)
                    {
                        _statusData.ShowMessage = null;
                        if (_statusData.ShowMessageWait != null)
                        {
                            _statusData.ShowMessageWait.Dispose();
                            _statusData.ShowMessageWait = null;
                        }
                        _statusData.ProgressIndeterminate = false;
                        _statusData.ProgressPercent = 0;
                    }

                    UpdateProgress();
                    await RpcClientUpdateDisplay().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.PingUpdated += (sender, pingDateTime) =>
            {
                if (_disposed)
                {
                    return;
                }

                lock (StatusLock)
                {
                    _statusData.StatusUpdateTime = pingDateTime;
                }

                UpdateTime();
            };

            _psdzRpcClient.CallbackHandler.StartProgrammingCompleted += async (s, success) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "StartProgrammingCompleted: Success={0}",
                        success);
                    await RpcClientTaskCompleted().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.StopProgrammingCompleted += async (s, success) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "StopProgrammingCompleted: Success={0}",
                        success);
                    await RpcClientTaskCompleted().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.ConnectVehicleCompleted += async (s, connectArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ConnectVehicleCompleted: Success={0}, Vin={1}, LicenseValid={2}",
                        connectArgs.Success, connectArgs.Vin, connectArgs.LicenseValid);

                    if (connectArgs.Success)
                    {
                        lock (StatusLock)
                        {
                            _statusData.Vin = connectArgs.Vin;
                            _statusData.LicenseValid = connectArgs.LicenseValid;
                        }
                    }
                    else
                    {
                        lock (StatusLock)
                        {
                            _statusData.Vin = null;
                            _statusData.LicenseValid = false;
                        }
                    }

                    await RpcClientTaskCompleted().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.DisconnectVehicleCompleted += async (s, success) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "DisconnectVehicleCompleted: Success={0}",
                        success);
                    lock (StatusLock)
                    {
                        _statusData.Vin = null;
                        _statusData.LicenseValid = false;
                        _statusData.ShowMessage = null;
                        _statusData.ShowMessageWait = null;
                    }

                    await RpcClientTaskCompleted().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.VehicleFunctionsCompleted += async (s, vehicleArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctionsCompleted: Success={0}, Type={1}",
                        vehicleArgs.Success, vehicleArgs.OperationType);

                    await RpcClientTaskCompleted().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateStatus += async (s, message) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    lock (StatusLock)
                    {
                        _statusData.StatusMessage = message;
                    }

                    await RpcClientUpdateDisplay().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateProgress += (s, progressArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    lock (StatusLock)
                    {
                        _statusData.ProgressIndeterminate = progressArgs.Marquee;
                        _statusData.ProgressPercent = progressArgs.Percent;
                    }

                    UpdateProgress();
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateOptions += async (sender, optionArgs) =>
            {
                if (_disposed)
                {
                    return;
                }

                try
                {
                    await RpcClientUpdateDisplay().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.UpdateOptionSelections += async (sender, swiRegisterEnum) =>
            {
                if (_disposed)
                {
                    return;
                }
                try

                {
                    if (swiRegisterEnum != null)
                    {
                        lock (StatusLock)
                        {
                            _statusData.SelectedSwiRegister = swiRegisterEnum;
                        }
                    }

                    await RpcClientUpdateDisplay().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ignored
                }
            };

            _psdzRpcClient.CallbackHandler.ShowMessage += (sender, msgArgs) =>
            {
                if (_disposed)
                {
                    msgArgs.Result = false;
                    return;
                }

                try
                {
                    lock (StatusLock)
                    {
                        _statusData.ShowMessage = msgArgs.Message;
                    }

                    UpdateDisplay();
                    msgArgs.Result = true;
                }
                catch (Exception)
                {
                    msgArgs.Result = false;
                }
            };

            _psdzRpcClient.CallbackHandler.ShowMessageWait += (sender, msgArgs) =>
            {
                if (_disposed)
                {
                    msgArgs.SetResult(false);
                    return;
                }

                try
                {
                    lock (StatusLock)
                    {
                        if (_statusData.ShowMessageWait != null)
                        {
                            _statusData.ShowMessageWait.Dispose();
                            _statusData.ShowMessageWait = null;
                        }
                        _statusData.ShowMessageWait = msgArgs;
                    }

                    UpdateDisplay();
                }
                catch (Exception)
                {
                    msgArgs.SetResult(false);
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
                string validSerial = _validSerial ?? string.Empty;
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
                try
                {
                    return Task.Run(() => _psdzRpcClient.RpcService.SetVehicleResponse(vehicleResponse)).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "SetVehicleResponse: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                    return false;
                }
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

    public async Task<bool> RpcClientConnect(string loadUrl, bool enableIpV6, string validSerial, CancellationTokenSource startCts)
    {
        try
        {
            if (_ediabasProxyClient == null)
            {
                return false;
            }

            _validSerial = validSerial;
            string normalizedUrl = loadUrl;
            if (!normalizedUrl.Contains("://"))
            {
                normalizedUrl = "http://" + normalizedUrl;
            }

            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri loadUri) || string.IsNullOrEmpty(loadUri.Host))
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Invalid loadUrl={0}", loadUrl);
                return false;
            }

            lock (StatusLock)
            {
                _statusData.TaskActive = false;
            }

            string remoteHost = loadUri.Host;
            int remotePort = loadUri.Port > 0 ? loadUri.Port : PsdzRpcServiceConstants.DefaultTcpPort;
            bool connected = await _psdzRpcClient.ConnectTcpAsync(remoteHost, remotePort, enableIpV6, null, startCts.Token).ConfigureAwait(false);
            if (!connected)
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: ConnectTcpAsync failed");
                return false;
            }

            int localVersion = PsdzRpcServiceConstants.InterfaceVersion;
            int remoteVersion = await _psdzRpcClient.RpcService.GetInterfaceVersion().ConfigureAwait(false);
            if (remoteVersion < localVersion)
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Interface version mismatch");
                return false;
            }

            string istaFolder = await _psdzRpcClient.RpcService.GetIstaInstallLocation().ConfigureAwait(false);
            if (string.IsNullOrEmpty(istaFolder))
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Failed to get ISTA install location");
                return false;
            }

            lock (StatusLock)
            {
                _statusData.IstaFolder = istaFolder;
            }

            bool licenseResult = await _psdzRpcClient.RpcService.SetLicenseValid(true).ConfigureAwait(false);
            if (!licenseResult)
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: SetLicenseValid failed");
                return false;
            }

            string language = _activityCommon.GetCurrentLanguage();
            bool matched = await _psdzRpcClient.RpcService.SetLanguage(language, true).ConfigureAwait(false);
            if (matched)
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: SetLanguage matched: {0}", language);
            }
            else
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: SetLanguage mismatch: {0}", language);
            }

            if (!_ediabasProxyClient.StartEdiabasThread())
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: StartEdiabasThread failed");
                return false;
            }

            bool proxyResult = await _psdzRpcClient.RpcService.EnableVehicleProxy().ConfigureAwait(false);
            if (!proxyResult)
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: EnableVehicleProxy failed");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Exception={0}",
                EdiabasNet.GetExceptionText(ex, false, false));
            return false;
        }
    }

    public async Task<bool> ConnectVehicle()
    {
        try
        {
            string istaFolder;
            lock (StatusLock)
            {
                istaFolder = _statusData.IstaFolder;
            }

            bool result = await _psdzRpcClient.RpcService.ConnectVehicle(istaFolder, string.Empty, false).ConfigureAwait(false);
            if (result)
            {
                await RpcClientTaskStarted().ConfigureAwait(false);
            }
            else
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ConnectVehicle failed");
            }
            return result;
        }
        catch (Exception ex)
        {
            _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ConnectVehicle: Exception={0}",
                EdiabasNet.GetExceptionText(ex, false, false));
            return false;
        }
    }

    public async Task<bool> DisconnectVehicle()
    {
        try
        {
            bool result = await _psdzRpcClient.RpcService.DisconnectVehicle().ConfigureAwait(false);
            if (result)
            {
                await RpcClientTaskStarted().ConfigureAwait(false);
            }
            else
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "DisconnectVehicle failed");
            }
            return result;
        }
        catch (Exception ex)
        {
            _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "DisconnectVehicle: Exception={0}",
                EdiabasNet.GetExceptionText(ex, false, false));
            return false;
        }
    }

    public async Task<bool> VehicleFunctions(PsdzOperationType operationType)
    {
        try
        {
            bool result = await _psdzRpcClient.RpcService.VehicleFunctions(operationType).ConfigureAwait(false);
            if (result)
            {
                await RpcClientTaskStarted().ConfigureAwait(false);
            }
            else
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions {0} failed", operationType);
            }
            return result;
        }
        catch (Exception ex)
        {
            _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions {0}: Exception={1}", operationType,
                EdiabasNet.GetExceptionText(ex, false, false));
            return false;
        }
    }

    public async Task<bool> CancelOperation()
    {
        try
        {
            await _psdzRpcClient.RpcService.CancelOperation().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "CancelOperation: Exception={0}",
                EdiabasNet.GetExceptionText(ex, false, false));
            return false;
        }
    }

    public async Task<bool> SelectOptionIdAsync(string optionId, bool select)
    {
        try
        {
            if (string.IsNullOrEmpty(optionId))
            {
                return false;
            }

            PsdzRpcSwiRegisterEnum? selectedSwiRegister;
            lock (StatusLock)
            {
                selectedSwiRegister = _statusData.SelectedSwiRegister;
            }

            if (selectedSwiRegister == null)
            {
                return false;
            }

            List<PsdzRpcOptionItem> rpcListItems = await _psdzRpcClient.RpcService.GetSelectedOptions(selectedSwiRegister).ConfigureAwait(false);
            bool modified = false;
            foreach (PsdzRpcOptionItem rpcListItem in rpcListItems)
            {
                if (string.Compare(rpcListItem.Id, optionId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    bool result = await _psdzRpcClient.RpcService.SelectOption(rpcListItem, select).ConfigureAwait(false);
                    if (result)
                    {
                        modified = true;
                    }
                    break;
                }
            }

            if (modified)
            {
                await RpcClientUpdateDisplay().ConfigureAwait(false);
            }

            return modified;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SelectSwiRegister(PsdzRpcSwiRegisterEnum? selectedSwiRegister)
    {
        try
        {
            lock (StatusLock)
            {
                _statusData.SelectedSwiRegister = selectedSwiRegister;
            }

            await RpcClientUpdateDisplay().ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void AckShowMessage()
    {
        lock (StatusLock)
        {
            _statusData.ShowMessage = null;
        }

        UpdateDisplay();
    }

    public void AckShowMessageWait(bool result)
    {
        lock (StatusLock)
        {
            if (_statusData.ShowMessageWait != null)
            {
                _statusData.ShowMessageWait.SetResult(result);
                _statusData.ShowMessageWait = null;
            }
        }

        UpdateDisplay();
    }

    public async Task<bool> DisposeRpcClient()
    {
        try
        {
            if (_psdzRpcClient != null)
            {
                _psdzRpcClient.Dispose();
                _psdzRpcClient = null;
            }

            if (_ediabasProxyClient != null)
            {
                await _ediabasProxyClient.StopEdiabasThread().ConfigureAwait(false);
                await _ediabasProxyClient.DisposeAsync().ConfigureAwait(false);
                _ediabasProxyClient = null;
            }

            lock (StatusLock)
            {
                _statusData.ShowMessage = null;
                if (_statusData.ShowMessageWait != null)
                {
                    _statusData.ShowMessageWait.Dispose();
                    _statusData.ShowMessageWait = null;
                }
                _statusData.RpcClientConnected = false;
            }
            _activityCommon.SetLock(ActivityCommon.LockType.None);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task RpcClientTaskStarted()
    {
        lock (StatusLock)
        {
            _statusData.TaskActive = true;
        }

        try
        {
            await RpcClientUpdateDisplay().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task RpcClientTaskCompleted()
    {
        lock (StatusLock)
        {
            _statusData.TaskActive = false;
        }

        try
        {
            await RpcClientUpdateDisplay().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void RpcClientDisconnected()
    {
        lock (StatusLock)
        {
            _statusData.ShowMessage = null;

            if (_statusData.ShowMessageWait != null)
            {
                _statusData.ShowMessageWait.Dispose();
                _statusData.ShowMessageWait = null;
            }

            _statusData.RpcClientConnected = false;
        }
    }

    public bool UpdateDisplay()
    {
        try
        {
            StatusData statusData;
            lock (StatusLock)
            {
                statusData = _statusData.Clone();
            }

            UpdateDisplayEvent?.Invoke(statusData);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool UpdateProgress()
    {
        try
        {
            int progressPercent;
            bool progressIndeterminate;
            lock (StatusLock)
            {
                progressPercent = _statusData.ProgressPercent;
                progressIndeterminate = _statusData.ProgressIndeterminate;
            }

            UpdateProgressEvent?.Invoke(progressPercent, progressIndeterminate);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool UpdateTime()
    {
        try
        {
            DateTime? updateTime;
            lock (StatusLock)
            {
                updateTime = _statusData.StatusUpdateTime;
            }

            UpdateTimeEvent?.Invoke(updateTime);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task RpcClientUpdateDisplay()
    {
        try
        {
            await GetRemoteStatusAsync().ConfigureAwait(false);
            UpdateDisplay();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task<bool> GetRemoteStatusAsync()
    {
        try
        {
            if (_psdzRpcClient.RpcService == null)
            {
                return false;
            }

            for (int retry = 0; retry < 2; retry++)
            {
                PsdzRpcStatusInfo statusInfo = await _psdzRpcClient.RpcService.GetStatusInfo().ConfigureAwait(false);
                List<PsdzRpcOptionType> statusOptionTypes = await _psdzRpcClient.RpcService.GetOptionTypes(true).ConfigureAwait(false);

                PsdzRpcSwiRegisterEnum? selectedSwiRegister;
                lock (StatusLock)
                {
                    _statusData.StatusInfo = statusInfo;
                    _statusData.StatusOptionTypes = statusOptionTypes;
                    _statusData.StatusUpdateTime = statusInfo.LastUpdated;

                    if (statusInfo.HasOptionsDict)
                    {
                        if (_statusData.SelectedSwiRegister == null)
                        {
                            _statusData.SelectedSwiRegister = PsdzRpcSwiRegisterEnum.VehicleModificationCodingConversion;
                        }
                    }
                    else
                    {
                        _statusData.SelectedSwiRegister = null;
                    }

                    selectedSwiRegister = _statusData.SelectedSwiRegister;
                }

                List<PsdzRpcOptionItem> rpcListItems = null;
                if (selectedSwiRegister != null)
                {
                    rpcListItems = await _psdzRpcClient.RpcService.GetSelectedOptions(selectedSwiRegister).ConfigureAwait(false);
                }

                lock (StatusLock)
                {
                    _statusData.RpcListItems = rpcListItems;
                }

                if (!statusInfo.VehicleConnected && statusInfo.HasOptionsDict)
                {
                    bool cleared = await _psdzRpcClient.RpcService.ClearOptionsDict().ConfigureAwait(false);
                    if (cleared)
                    {
                        // update status again
                        continue;
                    }
                }

                break;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
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
                Task.Run(DisposeRpcClient).GetAwaiter().GetResult();

                if (_activityCommon != null)
                {
                    _activityCommon.Dispose();
                    _activityCommon = null;
                }
            }
        }

        // Note disposing has been done.
        _disposed = true;
    }
}
