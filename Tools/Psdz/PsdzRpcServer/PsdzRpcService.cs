using PsdzClient;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

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
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

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
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }
        
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
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        _programmingJobs.GenServiceModules = genServiceModules;
        return Task.FromResult(true);
    }

    public Task<PsdzRpcCacheType> GetCacheResponseType()
    {
        PsdzRpcCacheType cacheResponseType = ToPsdzRpcCacheType(_programmingJobs.CacheResponseType);
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

    public Task<List<PsdzRpcOptionType>> GetOptionTypes()
    {
        List<PsdzRpcOptionType> optionTypes = new List<PsdzRpcOptionType>();
        foreach (ProgrammingJobs.OptionType optionTypeUpdate in _programmingJobs.OptionTypes)
        {
            PsdzRpcOptionType optionType = new PsdzRpcOptionType(optionTypeUpdate.SwiRegisterEnum, optionTypeUpdate.ToString());
            optionTypes.Add(optionType);
        }

        return Task.FromResult(optionTypes);
    }

    public Task<List<PsdzRpcOptionItem>> GetSelectedOptions(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
    {
        List<PsdzRpcOptionItem> options = GetSelectedOptionsInternal(swiRegisterEnum);
        return Task.FromResult(options);
    }

    public Task<bool> SelectOption(PsdzRpcOptionItem optionItem, bool select)
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        bool result = SelectOptionInternal(optionItem, select);
        return Task.FromResult(result);
    }

    public Task<bool> ClearOptionsDict()
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        _programmingJobs.OptionsDict = null;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateTargetFa(bool reset)
    {
        if (IsOperationActive())
        {
            return Task.FromResult(false);
        }

        _programmingJobs.UpdateTargetFa(reset);
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

    private List<PsdzRpcOptionItem> GetSelectedOptionsInternal(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
    {
        List<PsdzRpcOptionItem> options = new List<PsdzRpcOptionItem>();
        if (_programmingJobs.ProgrammingService == null || _programmingJobs.PsdzContext?.Connection == null)
        {
            return options;
        }

        bool replacement = false;
        if (swiRegisterEnum.HasValue)
        {
            switch (PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnum.Value))
            {
                case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                case PsdzDatabase.SwiRegisterGroup.HwInstall:
                    replacement = true;
                    break;
            }
        }

        List<PsdzDatabase.SwiAction> selectedSwiActions = GetSelectedSwiActions();
        List<PsdzDatabase.SwiAction> linkedSwiActions = _programmingJobs.ProgrammingService.PsdzDatabase.ReadLinkedSwiActions(_programmingJobs.PsdzContext?.VecInfo, selectedSwiActions, null);
        Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
        if (optionsDict != null && _programmingJobs.SelectedOptions != null && swiRegisterEnum.HasValue)
        {
            if (optionsDict.TryGetValue(swiRegisterEnum.Value, out List<ProgrammingJobs.OptionsItem> optionsItems))
            {
                foreach (ProgrammingJobs.OptionsItem optionsItem in optionsItems.OrderBy(x => x.ToString()))
                {
                    bool enabled = true;
                    bool selected = false;
                    bool addItem = true;
                    int selectIndex = _programmingJobs.SelectedOptions.IndexOf(optionsItem);
                    if (selectIndex >= 0)
                    {
                        if (replacement)
                        {
                            selected = true;
                        }
                        else
                        {
                            if (selectIndex == _programmingJobs.SelectedOptions.Count - 1)
                            {
                                selected = true;
                            }
                            else
                            {
                                enabled = false;
                            }
                        }
                    }
                    else
                    {
                        if (replacement)
                        {
                            if (optionsItem.EcuInfo == null)
                            {
                                addItem = false;
                            }
                        }
                        else
                        {
                            if (optionsItem.SwiAction != null)
                            {
                                if (linkedSwiActions != null &&
                                    linkedSwiActions.Any(x => string.Compare(x.Id, optionsItem.SwiAction.Id, StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    addItem = false;
                                }
                                else
                                {
                                    if (!_programmingJobs.ProgrammingService.PsdzDatabase.EvaluateXepRulesById(optionsItem.SwiAction.Id, _programmingJobs.PsdzContext?.VecInfo, null))
                                    {
                                        addItem = false;
                                    }
                                }
                            }
                        }
                    }

                    if (!_programmingJobs.IsOptionsItemEnabled(optionsItem))
                    {
                        if (selected)
                        {
                            enabled = false;
                        }
                        else
                        {
                            addItem = false;
                        }
                    }

                    if (addItem)
                    {
                        options.Add(new PsdzRpcOptionItem(optionsItem.SwiRegisterEnum, optionsItem.Id, optionsItem.ToString(), enabled, selected));
                    }
                }
            }
        }

        return options;
    }

    private bool SelectOptionInternal(PsdzRpcOptionItem optionItem, bool select)
    {
        if (_programmingJobs?.SelectedOptions == null)
        {
            return false;
        }

        if (optionItem == null)
        {
            _programmingJobs.SelectedOptions.Clear();
            return true;
        }

        Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
        PsdzDatabase.SwiRegisterEnum swiRegisterEnum = optionItem.SwiRegisterEnum;
        if (_programmingJobs.SelectedOptions.Count > 0)
        {
            PsdzDatabase.SwiRegisterEnum swiRegisterEnumCurrent = _programmingJobs.SelectedOptions[0].SwiRegisterEnum;
            if (PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnum) != PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnumCurrent))
            {
                _programmingJobs.SelectedOptions.Clear();
            }
        }

        if (!optionsDict.TryGetValue(swiRegisterEnum, out List<ProgrammingJobs.OptionsItem> optionsItems))
        {
            return false;
        }

        ProgrammingJobs.OptionsItem optionsItem = optionsItems.FirstOrDefault(x => string.Compare(x.Id, optionItem.Id, StringComparison.OrdinalIgnoreCase) == 0);
        if (optionsItem == null)
        {
            return false;
        }

        if (!optionItem.Enabled)
        {
            return false;
        }

        bool modified = false;
        if (_programmingJobs.SelectedOptions != null)
        {
            List<ProgrammingJobs.OptionsItem> combinedOptionsItems = _programmingJobs.GetCombinedOptionsItems(optionsItem, optionsItems);
            if (select)
            {
                if (!_programmingJobs.SelectedOptions.Contains(optionsItem))
                {
                    _programmingJobs.SelectedOptions.Add(optionsItem);
                }

                if (combinedOptionsItems != null)
                {
                    foreach (ProgrammingJobs.OptionsItem combinedItem in combinedOptionsItems)
                    {
                        if (!_programmingJobs.SelectedOptions.Contains(combinedItem))
                        {
                            _programmingJobs.SelectedOptions.Add(combinedItem);
                        }
                    }
                }
            }
            else
            {
                _programmingJobs.SelectedOptions.Remove(optionsItem);

                if (combinedOptionsItems != null)
                {
                    foreach (ProgrammingJobs.OptionsItem combinedItem in combinedOptionsItems)
                    {
                        _programmingJobs.SelectedOptions.Remove(combinedItem);
                    }
                }
            }

            modified = true;
        }

        if (modified)
        {
            PsdzContext psdzContext = _programmingJobs.PsdzContext;
            if (psdzContext?.Connection != null)
            {
                psdzContext.Tal = null;
            }

            _programmingJobs.UpdateTargetFa();
        }

        return true;
    }

    private List<PsdzDatabase.SwiAction> GetSelectedSwiActions()
    {
        if (_programmingJobs.PsdzContext?.Connection == null || _programmingJobs.SelectedOptions == null)
        {
            return null;
        }

        List<PsdzDatabase.SwiAction> selectedSwiActions = new List<PsdzDatabase.SwiAction>();
        foreach (ProgrammingJobs.OptionsItem optionsItem in _programmingJobs.SelectedOptions)
        {
            if (optionsItem.SwiAction != null)
            {
                selectedSwiActions.Add(optionsItem.SwiAction);
            }
        }

        return selectedSwiActions;
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

    private void UpdateOptions()
    {
        _programmingJobs.SelectedOptions = new List<ProgrammingJobs.OptionsItem>();
        _callback.OnUpdateOptions();
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

    private static PsdzRpcCacheType ToPsdzRpcCacheType(ProgrammingJobs.CacheType cacheType)
    {
        return cacheType switch
        {
            ProgrammingJobs.CacheType.None => PsdzRpcCacheType.None,
            ProgrammingJobs.CacheType.NoResponse => PsdzRpcCacheType.NoResponse,
            ProgrammingJobs.CacheType.FuncAddress => PsdzRpcCacheType.FuncAddress,
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, "Unknown CacheType value")
        };
    }
}
