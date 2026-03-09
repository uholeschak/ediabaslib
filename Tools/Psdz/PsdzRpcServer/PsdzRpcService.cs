using PsdzClient;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static PsdzClient.Programming.ProgrammingJobs;

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
    }

    public async Task<bool> Connect(string parameter)
    {
        for (int i = 0; i <= 100; i += 20)
        {
            await _callback.OnProgressChangedAsync(i, $"Connecting... {i}%");
            await Task.Delay(500);
        }
        await _callback.OnOperationCompletedAsync(true);
        return true;
    }

    public async Task<bool> Disconnect(string parameter)
    {
        // Implement disconnection logic here
        for (int i = 0; i <= 100; i += 20)
        {
            await _callback.OnProgressChangedAsync(i, $"Disconnecting... {i}%");
            await Task.Delay(500);
        }
        await _callback.OnOperationCompletedAsync(true);
        return true;
    }

    public async Task CancelOperation()
    {
        await _callback.OnUpdateStatus("Cancelling operation...");

        lock (_ctsLock)
        {
            _cts?.Cancel();
        }
    }

    public async Task<bool> ConnectVehicle(string istaFolder, string remoteHost, bool useIcom, int addTimeout = 1000)
    {
        CancellationTokenSource cts = CreateCancellationToken();

        try
        {
            bool result = await Task.Run(() => _programmingJobs.ConnectVehicle(cts, istaFolder, remoteHost, useIcom, addTimeout));
            return result;
        }
        finally
        {
            DisposeCancellationToken(cts);
        }
    }

    public async Task<bool> DisconnectVehicle()
    {
        CancellationTokenSource cts = CreateCancellationToken();

        try
        {
            bool result = await Task.Run(() => _programmingJobs.DisconnectVehicle(cts));
            return result;
        }
        finally
        {
            DisposeCancellationToken(cts);
        }
    }

    public async Task<bool> VehicleFunctions(OperationType operationType)
    {
        CancellationTokenSource cts = CreateCancellationToken();

        try
        {
            bool result = await Task.Run(() => _programmingJobs.VehicleFunctions(cts, operationType));
            return result;
        }
        finally
        {
            DisposeCancellationToken(cts);
        }
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

    public Task<List<OptionsItem>> GetSelectedOptions()
    {
        List<OptionsItem> options = _programmingJobs.SelectedOptions;
        return Task.FromResult(options);
    }

    private CancellationTokenSource CreateCancellationToken()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        lock (_ctsLock)
        {
            cts.Cancel();
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
        _callback.OnUpdateOptions(optionsDict);
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
