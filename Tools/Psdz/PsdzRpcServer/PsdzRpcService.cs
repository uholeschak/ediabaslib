using PsdzClient;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcServer;

public class PsdzRpcService : IPsdzRpcService
{
    private readonly IPsdzRpcServiceCallback _callback;
    private readonly ProgrammingJobs _programmingJobs;
    private CancellationTokenSource _cts;
    private readonly object _ctsLock = new object();

    public PsdzRpcService(IPsdzRpcServiceCallback callback)
    {
        _callback = callback;
        _programmingJobs = new ProgrammingJobs(PsdzRpcServiceConstants.DealerId);
        _programmingJobs.UpdateStatusEvent += UpdateStatus;
        _programmingJobs.ProgressEvent += UpdateProgress;
        _programmingJobs.UpdateOptionsEvent += UpdateOptions;
        _programmingJobs.UpdateOptionSelectionsEvent += UpdateOptionSelections;
        _programmingJobs.ShowMessageEvent += ShowMessageEvent;
        _programmingJobs.TelSendQueueSizeEvent += TelSendQueueSizeEvent;
        _programmingJobs.ServiceInitializedEvent += ServiceInitializedEvent;
        _programmingJobs.GenServiceModules = false;
    }

    public async Task<bool> Connect(string parameter)
    {
        for (int i = 0; i <= 100; i += 20)
        {
            await _callback.OnUpdateStatus($"Connecting... {i}%");
            await _callback.OnProgressChanged(i, $"Connecting... {i}%");
            await Task.Delay(500);
        }
        await _callback.OnOperationCompleted(true);
        return true;
    }

    public async Task<bool> Disconnect(string parameter)
    {
        // Implement disconnection logic here
        for (int i = 0; i <= 100; i += 20)
        {
            await _callback.OnUpdateStatus($"Disconnecting... {i}%");
            await _callback.OnProgressChanged(i, $"Disconnecting... {i}%");
            await Task.Delay(500);
        }
        await _callback.OnOperationCompleted(true);
        return true;
    }

    public Task<bool> OperationActive()
    {
        bool isActive = IsOperationActive();
        return Task.FromResult(isActive);
    }

    public Task CancelOperation()
    {
        lock (_ctsLock)
        {
            _cts?.Cancel();
        }

        return Task.CompletedTask;
    }

    public Task<bool> SetupLog4Net(string logFile)
    {
        bool result = ProgrammingJobs.SetupLog4Net(logFile);
        return Task.FromResult(result);
    }

    public Task<string> GetIstaInstallLocation()
    {
        string istaFolder = ProgrammingJobs.GetIstaInstallLocation();
        return Task.FromResult(istaFolder);
    }

    public Task<bool> StartProgrammingService(string istaFolder)
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        CancellationTokenSource cts = CreateCancellationToken();
        StartProgrammingServiceTask(istaFolder).ContinueWith(task =>
        {
            bool result = task.IsCompletedSuccessfully && task.Result;
            _callback.OnOperationCompleted(result).GetAwaiter().GetResult();
            DisposeCancellationToken(cts);
        }, cts.Token);

        return Task.FromResult(true);
    }

    public Task<bool> StopProgrammingService(string istaFolder, bool force = false)
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        CancellationTokenSource cts = CreateCancellationToken();
        StopProgrammingServiceTask(istaFolder, force).ContinueWith(task =>
        {
            bool result = task.IsCompletedSuccessfully && task.Result;
            _callback.OnOperationCompleted(result).GetAwaiter().GetResult();
            DisposeCancellationToken(cts);
        }, cts.Token);

        return Task.FromResult(true);
    }

    public Task<bool> ConnectVehicle(string istaFolder, string remoteHost, bool useIcom, int addTimeout = 1000)
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        CancellationTokenSource cts = CreateCancellationToken();
        ConnectVehicleTask(istaFolder, remoteHost, useIcom, addTimeout).ContinueWith(task =>
        {
            bool result = task.IsCompletedSuccessfully && task.Result;
            _callback.OnOperationCompleted(result).GetAwaiter().GetResult();
            DisposeCancellationToken(cts);
        }, cts.Token);

        return Task.FromResult(true);
    }

    public Task<bool> DisconnectVehicle()
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false); 
        }

        CancellationTokenSource cts = CreateCancellationToken();
        DisconnectVehicleTask().ContinueWith(task =>
        {
            bool result = task.IsCompletedSuccessfully && task.Result;
            _callback.OnOperationCompleted(result).GetAwaiter().GetResult();
            DisposeCancellationToken(cts);
        }, cts.Token);

        return Task.FromResult(true);
    }

    public Task<bool> VehicleFunctions(ProgrammingJobs.OperationType operationType)
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        CancellationTokenSource cts = CreateCancellationToken();
        VehicleFunctionsTask(operationType).ContinueWith(task =>
        {
            bool result = task.IsCompletedSuccessfully && task.Result;
            _callback.OnOperationCompleted(result).GetAwaiter().GetResult();
            DisposeCancellationToken(cts);
        }, cts.Token);

        return Task.FromResult(true);
    }

    public Task<string> GetLanguage()
    {
        string language = _programmingJobs.ClientContext.Language;
        return Task.FromResult(language);
    }

    public Task<bool> SetLanguage(string language)
    {
        _programmingJobs.ClientContext.Language = language;
        return Task.FromResult(true);
    }

    public Task<bool> GetLicenseValid()
    {
        bool licenseValid = _programmingJobs.LicenseValid;
        return Task.FromResult(licenseValid);
    }

    public Task<bool> SetLicenseValid(bool licenseValid)
    {
        _programmingJobs.LicenseValid = licenseValid;
        return Task.FromResult(true);
    }

    public Task<bool> GetCacheClearRequired()
    {
        bool cacheClearRequired = _programmingJobs.CacheClearRequired;
        return Task.FromResult(cacheClearRequired);
    }

    public Task<bool> SetCacheClearRequired(bool cacheClearRequired)
    {
        _programmingJobs.CacheClearRequired = cacheClearRequired;
        return Task.FromResult(true);
    }

    public Task<bool> GetGenServiceModules()
    {
        bool genServiceModules = _programmingJobs.GenServiceModules;
        return Task.FromResult(genServiceModules);
    }
    public Task<bool> SetGenServiceModules(bool genServiceModules)
    {
        _programmingJobs.GenServiceModules = genServiceModules;
        return Task.FromResult(true);
    }

    public Task<ProgrammingJobs.CacheType> GetCacheResponseType()
    {
        ProgrammingJobs.CacheType cacheResponseType = _programmingJobs.CacheResponseType;
        return Task.FromResult(cacheResponseType);
    }

    public Task<bool> IsPsdzInitialized()
    {
        bool isInitialized = _programmingJobs.ProgrammingService?.Psdz?.IsPsdzInitialized ?? false;
        return Task.FromResult(isInitialized);
    }

    public Task<bool> IsVehicleConnected()
    {
        bool isConnected = _programmingJobs.PsdzContext?.Connection != null;
        return Task.FromResult(isConnected);
    }

    public Task<bool> IsTalPresent()
    {
        bool hasTal = _programmingJobs.PsdzContext?.Tal != null;
        return Task.FromResult(hasTal);
    }

    public Task<string> GetVehicleVin()
    {
        string vin = _programmingJobs.PsdzContext?.DetectVehicle?.Vin;
        return Task.FromResult(vin);
    }

    public Task<string> GetPsdzServiceHostLogDir()
    {
        string hostLogDir = _programmingJobs.ProgrammingService?.GetPsdzServiceHostLogDir();
        return Task.FromResult(hostLogDir);
    }

    public Task<List<ProgrammingJobs.OptionsItem>> GetSelectedOptions()
    {
        List<ProgrammingJobs.OptionsItem> options = _programmingJobs.SelectedOptions;
        return Task.FromResult(options);
    }

    public Task<Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>>> GetOptionsDict()
    {
        Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
        return Task.FromResult(optionsDict);
    }

    public Task<bool> ClearOptionsDict()
    {
        _programmingJobs.OptionsDict = null;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateTargetFa()
    {
        _programmingJobs.UpdateTargetFa();
        return Task.FromResult(true);
    }

    private CancellationTokenSource CreateCancellationToken()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        lock (_ctsLock)
        {
            _cts?.Dispose();
            _cts = cts;
        }
        return cts;
    }

    private void DisposeCancellationToken(CancellationTokenSource cts)
    {
        lock (_ctsLock)
        {
            if (_cts == cts)
            {
                _cts = null;
            }
        }
        cts.Dispose();
    }

    private bool IsOperationActive()
    {
        bool isActive;
        lock (_ctsLock)
        {
            isActive = _cts != null;
        }
        return isActive;
    }

    private async Task<bool> StartProgrammingServiceTask(string istaFolder)
    {
        return await Task.Run(() => _programmingJobs.StartProgrammingService(_cts, istaFolder)).ConfigureAwait(false);
    }

    public async Task<bool> StopProgrammingServiceTask(string istaFolder, bool force)
    {
        return await Task.Run(() => _programmingJobs.StopProgrammingService(_cts, istaFolder, force)).ConfigureAwait(false);
    }

    public async Task<bool> ConnectVehicleTask(string istaFolder, string remoteHost, bool useIcom, int addTimeout)
    {
        return await Task.Run(() => _programmingJobs.ConnectVehicle(_cts, istaFolder, remoteHost, useIcom, addTimeout)).ConfigureAwait(false);
    }

    public async Task<bool> DisconnectVehicleTask()
    {
        return await Task.Run(() => _programmingJobs.DisconnectVehicle(_cts)).ConfigureAwait(false);
    }

    public async Task<bool> VehicleFunctionsTask(ProgrammingJobs.OperationType operationType)
    {
        return await Task.Run(() => _programmingJobs.VehicleFunctions(_cts, operationType)).ConfigureAwait(false);
    }

    private void UpdateStatus(string message = null)
    {
        _callback.OnUpdateStatus(message);
    }

    private void UpdateProgress(int percent, bool marquee, string message = null)
    {
        _callback.OnUpdateProgress(percent, marquee, message);
    }

    private void UpdateOptions(Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict)
    {
        UpdateStatus("UpdateOptions called");
        //_callback.OnUpdateOptions(optionsDict);
    }

    private void UpdateOptionSelections(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
    {
        _callback.OnUpdateOptionSelections(swiRegisterEnum);
    }

    private bool ShowMessageEvent(CancellationTokenSource cts, string message, bool okBtn, bool wait)
    {
        return _callback.OnShowMessage(message, okBtn, wait).GetAwaiter().GetResult();
    }

    private int TelSendQueueSizeEvent()
    {
        return _callback.OnTelSendQueueSize().GetAwaiter().GetResult();
    }

    private void ServiceInitializedEvent(string hostLogDir)
    {
        _callback.OnServiceInitialized(hostLogDir);
    }

    public void Dispose()
    {
        lock (_ctsLock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        _programmingJobs.UpdateStatusEvent -= UpdateStatus;
        _programmingJobs.ProgressEvent -= UpdateProgress;
        _programmingJobs.UpdateOptionsEvent -= UpdateOptions;
        _programmingJobs.UpdateOptionSelectionsEvent -= UpdateOptionSelections;
        _programmingJobs.ShowMessageEvent -= ShowMessageEvent;
        _programmingJobs.TelSendQueueSizeEvent -= TelSendQueueSizeEvent;
        _programmingJobs.Dispose();
    }
}
