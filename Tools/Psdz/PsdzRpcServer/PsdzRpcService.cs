using BMW.Rheingold.Psdz.Client;
using PsdzClient;
using PsdzClient.Programming;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdiabasLib;

namespace PsdzRpcServer
{
    public class PsdzRpcService : IPsdzRpcService
    {
        private readonly IPsdzRpcServiceCallback _callback;
        private readonly ProgrammingJobs _programmingJobs;
        private PsdzVehicleProxy _vehicleProxy;
        private CancellationTokenSource _cts;
        private readonly object _ctsLock = new object();

        public PsdzRpcService(IPsdzRpcServiceCallback callback, string dealerId)
        {
            _callback = callback;
            _programmingJobs = new ProgrammingJobs(dealerId);
            _programmingJobs.UpdateStatusEvent += UpdateStatus;
            _programmingJobs.ProgressEvent += UpdateProgress;
            _programmingJobs.UpdateOptionsEvent += UpdateOptions;
            _programmingJobs.UpdateOptionSelectionsEvent += UpdateOptionSelections;
            _programmingJobs.ShowMessageEvent += ShowMessageEvent;
            _programmingJobs.TelSendQueueSizeEvent += TelSendQueueSizeEvent;
            _programmingJobs.ServiceInitializedEvent += ServiceInitializedEvent;
            _programmingJobs.GenServiceModules = false;
        }

        public Task<string> GetInterfaceSignature()
        {
            return Task.FromResult(PsdzRpcServiceConstants.ServiceInterfaceSignature);
        }

        public Task<string> GetCallbackInterfaceSignature()
        {
            return Task.FromResult(PsdzRpcServiceConstants.CallbackInterfaceSignature);
        }

        public Task<bool> OperationActive()
        {
            bool isActive = IsOperationActive();
            return Task.FromResult(isActive);
        }

        public Task<bool> IsCancelPossible()
        {
            bool cancelPossible = false;
            lock (_ctsLock)
            {
                cancelPossible = _cts != null;
            }

            return Task.FromResult(cancelPossible);
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

        public Task<bool> ResetStarterGuard()
        {
            PsdzStarterGuard.Instance.ResetInitialization();
            PsdzServiceStarter.ClearIstaPIDsFile();
            return Task.FromResult(true);
        }

        public Task<string> GetIstaInstallLocation()
        {
            string istaFolder = EdSec4Diag.GetIstaInstallLocation();
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
                bool result = TaskCompletedSuccessfully(task) && task.Result;
                Task.Run(() => _callback.OnStartProgrammingCompleted(result)).GetAwaiter().GetResult();
                DisposeCancellationToken(cts);
            });

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
                bool result = TaskCompletedSuccessfully(task) && task.Result;
                Task.Run(() => _callback.OnStopProgrammingCompleted(result)).GetAwaiter().GetResult();
                DisposeCancellationToken(cts);
            });

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
                bool result = TaskCompletedSuccessfully(task) && task.Result;
                string vin = _programmingJobs.PsdzContext?.DetectVehicle?.Vin;
                Task.Run(() => _callback.OnConnectVehicleCompleted(result, vin)).GetAwaiter().GetResult();
                DisposeCancellationToken(cts);
            });

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
                bool result = TaskCompletedSuccessfully(task) && task.Result;
                Task.Run(() => _callback.OnDisconnectVehicleCompleted(result)).GetAwaiter().GetResult();
                DisposeCancellationToken(cts);
            });

            return Task.FromResult(true);
        }

        public Task<bool> VehicleFunctions(PsdzOperationType operationType)
        {
            if (IsOperationActive())
            {
                return Task.FromResult(false);
            }

            ProgrammingJobs.OperationType OperationTypeValue = MapOperationType(operationType);
            CancellationTokenSource cts = CreateCancellationToken();
            VehicleFunctionsTask(OperationTypeValue).ContinueWith(task =>
            {
                bool result = TaskCompletedSuccessfully(task) && task.Result;
                Task.Run(() => _callback.OnVehicleFunctionsCompleted(result, operationType)).GetAwaiter().GetResult();
                DisposeCancellationToken(cts);
            });

            return Task.FromResult(true);
        }

        public Task<List<string>> GetLanguages()
        {
            List<string> langList = PsdzDatabase.EcuTranslation.GetLanguages();
            return Task.FromResult(langList);
        }

        public Task<string> GetLanguage()
        {
            string language = _programmingJobs.ClientContext.Language;
            return Task.FromResult(language);
        }

        public Task<bool> SetLanguage(string language, bool matchLanguage)
        {
            bool matched = false;
            if (matchLanguage)
            {
                List<string> langList = PsdzDatabase.EcuTranslation.GetLanguages();
                foreach (string lang in langList)
                {
                    if (language.StartsWith(lang, StringComparison.OrdinalIgnoreCase))
                    {
                        _programmingJobs.ClientContext.Language = lang;
                        matched = true;
                        break;
                    }
                }
            }
            else
            {
                _programmingJobs.ClientContext.Language = language;
                matched = true;
            }

            return Task.FromResult(matched);
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

        public Task<PsdzRpcCacheType> GetCacheResponseType()
        {
            PsdzRpcCacheType cacheResponseType = MapCacheType(_programmingJobs.CacheResponseType);
            return Task.FromResult(cacheResponseType);
        }

        public Task<bool> IsPsdzInitialized()
        {
            bool isInitialized = _programmingJobs.ProgrammingService?.Psdz?.IsPsdzInitialized ?? false;
            return Task.FromResult(isInitialized);
        }

        public Task<bool> IsVehicleConnected()
        {
            bool isConnected = IsVehicleConnectedInternal();
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
                PsdzDatabase.SwiRegisterGroup swiRegisterGroup = PsdzDatabase.GetSwiRegisterGroup(optionTypeUpdate.SwiRegisterEnum);
                PsdzRpcOptionType optionType = new PsdzRpcOptionType(MapSwiRegisterEnum(optionTypeUpdate.SwiRegisterEnum),
                    MapSwiRegisterGroupEnum(swiRegisterGroup), optionTypeUpdate.ToString());
                optionTypes.Add(optionType);
            }

            return Task.FromResult(optionTypes);
        }

        public Task<List<PsdzRpcOptionItem>> GetSelectedOptions(PsdzRpcSwiRegisterEnum? swiRegisterEnum)
        {
            PsdzDatabase.SwiRegisterEnum? swiRegisterEnumValue = swiRegisterEnum.HasValue ? MapSwiRegisterEnum(swiRegisterEnum.Value) : null;
            List<PsdzRpcOptionItem> options = GetSelectedOptionsInternal(swiRegisterEnumValue);
            return Task.FromResult(options);
        }

        public Task<PsdzSwiRegisterGroupEnum> GetSwiRegisterGroup(PsdzRpcSwiRegisterEnum swiRegisterEnum)
        {
            PsdzDatabase.SwiRegisterEnum swiRegisterEnumValue = MapSwiRegisterEnum(swiRegisterEnum);
            PsdzDatabase.SwiRegisterGroup swiRegisterGroup = PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnumValue);
            PsdzSwiRegisterGroupEnum rpcSwiRegisterGroup = MapSwiRegisterGroupEnum(swiRegisterGroup);
            return Task.FromResult(rpcSwiRegisterGroup);
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
            _programmingJobs.OptionsDict = null;
            return Task.FromResult(true);
        }

        public Task<bool> HasOptionsDict()
        {
            bool hasOptionsDict = _programmingJobs.OptionsDict != null;
            return Task.FromResult(hasOptionsDict);
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

        public Task<bool> EnableVehicleProxy()
        {
            if (_vehicleProxy == null)
            {
                if (IsOperationActive())
                {
                    return Task.FromResult(false);
                }

                if (IsVehicleConnectedInternal())
                {
                    return Task.FromResult(false);
                }

                _vehicleProxy = new PsdzVehicleProxy(_programmingJobs);
                _vehicleProxy.VehicleConnectEvent += (id) =>
                {
                    try
                    {
                        Task.Run(() => _callback.OnVehicleConnect(id)).GetAwaiter().GetResult();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                };

                _vehicleProxy.VehicleDisconnectEvent += (id) =>
                {
                    try
                    {
                        Task.Run(() => _callback.OnVehicleDisconnect(id)).GetAwaiter().GetResult();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                };

                _vehicleProxy.VehicleSendEvent += (id, data) =>
                {
                    try
                    {
                        Task.Run(() => _callback.OnVehicleSend(id, data)).GetAwaiter().GetResult();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                };

                _vehicleProxy.ReportErrorEvent += (message) =>
                {
                    try
                    {
                        Task.Run(() => _callback.OnReportError(message)).GetAwaiter().GetResult();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                };
            }

            return Task.FromResult(true);
        }

        public Task<bool> SetVehicleResponse(PsdzVehicleResponse response)
        {
            if (_vehicleProxy == null)
            {
                return Task.FromResult(false);
            }

            _vehicleProxy.VehicleResponseReceived(response);
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

        private bool IsVehicleConnectedInternal()
        {
            bool isConnected = _programmingJobs.PsdzContext?.Connection != null;
            return isConnected;
        }

        private List<PsdzRpcOptionItem> GetSelectedOptionsInternal(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
        {
            try
            {
                List<PsdzRpcOptionItem> options = new List<PsdzRpcOptionItem>();
                if (_programmingJobs.ProgrammingService == null || !IsVehicleConnectedInternal())
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
                                options.Add(new PsdzRpcOptionItem(MapSwiRegisterEnum(optionsItem.SwiRegisterEnum), optionsItem.Id, optionsItem.ToString(), enabled, selected));
                            }
                        }
                    }
                }

                return options;
            }
            catch (Exception)
            {
                return new List<PsdzRpcOptionItem>();
            }
        }

        private bool SelectOptionInternal(PsdzRpcOptionItem optionItem, bool select)
        {
            try
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
                PsdzDatabase.SwiRegisterEnum swiRegisterEnum = MapSwiRegisterEnum(optionItem.SwiRegisterEnum);
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
            catch (Exception)
            {
                return false;
            }
        }

        private List<PsdzDatabase.SwiAction> GetSelectedSwiActions()
        {
            if (!IsVehicleConnectedInternal() || _programmingJobs.SelectedOptions == null)
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
            if (_vehicleProxy != null)
            {
                return await _vehicleProxy.ConnectVehicleTask(_cts, istaFolder).ConfigureAwait(false);
            }

            return await Task.Run(() => _programmingJobs.ConnectVehicle(_cts, istaFolder, remoteHost, useIcom, addTimeout)).ConfigureAwait(false);
        }

        public async Task<bool> DisconnectVehicleTask()
        {
            if (_vehicleProxy != null)
            {
                return await _vehicleProxy.DisconnectVehicleTask(_cts).ConfigureAwait(false);
            }

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
            PsdzRpcSwiRegisterEnum? swiRegisterEnumValue = swiRegisterEnum.HasValue ? MapSwiRegisterEnum(swiRegisterEnum.Value) : null;
            _callback.OnUpdateOptionSelections(swiRegisterEnumValue);
        }

        private bool ShowMessageEvent(CancellationTokenSource cts, string message, bool okBtn, bool wait)
        {
            return Task.Run(() => _callback.OnShowMessage(message, okBtn, wait)).GetAwaiter().GetResult();
        }

        private int TelSendQueueSizeEvent()
        {
            if (_vehicleProxy != null)
            {
                return _vehicleProxy.GetTelSendQueueSize();
            }

            return Task.Run(() => _callback.OnTelSendQueueSize()).GetAwaiter().GetResult();
        }

        private void ServiceInitializedEvent(string hostLogDir, bool loggingInitialized)
        {
            _callback.OnServiceInitialized(hostLogDir, loggingInitialized);
        }

        public void Dispose()
        {
            lock (_ctsLock)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }

            if (_vehicleProxy != null)
            {
                _vehicleProxy.Dispose();
                _vehicleProxy = null;
            }

            _programmingJobs.UpdateStatusEvent -= UpdateStatus;
            _programmingJobs.ProgressEvent -= UpdateProgress;
            _programmingJobs.UpdateOptionsEvent -= UpdateOptions;
            _programmingJobs.UpdateOptionSelectionsEvent -= UpdateOptionSelections;
            _programmingJobs.ShowMessageEvent -= ShowMessageEvent;
            _programmingJobs.TelSendQueueSizeEvent -= TelSendQueueSizeEvent;
            _programmingJobs.Dispose();
        }

        private static bool TaskCompletedSuccessfully(Task task)
        {
#if NET
            return task.IsCompletedSuccessfully;
#else
            return task.Status == TaskStatus.RanToCompletion;
#endif
        }

        private static PsdzRpcCacheType MapCacheType(ProgrammingJobs.CacheType cacheType)
        {
            if (Enum.TryParse(cacheType.ToString(), out PsdzRpcCacheType result))
            {
                return result;
            }

            throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, "Unknown CacheType");
        }

        private static PsdzOperationType MapOperationType(ProgrammingJobs.OperationType operationType)
        {
            if (Enum.TryParse(operationType.ToString(), out PsdzOperationType result))
            {
                return result;
            }
            throw new ArgumentOutOfRangeException(nameof(operationType), operationType, "Unknown OperationType");
        }

        private static ProgrammingJobs.OperationType MapOperationType(PsdzOperationType operationType)
        {
            if (Enum.TryParse(operationType.ToString(), out ProgrammingJobs.OperationType result))
            {
                return result;
            }
            throw new ArgumentOutOfRangeException(nameof(operationType), operationType, "Unknown OperationType");
        }

        private static PsdzRpcSwiRegisterEnum MapSwiRegisterEnum(PsdzDatabase.SwiRegisterEnum swiRegisterEnum)
        {
            if (Enum.TryParse(swiRegisterEnum.ToString(), out PsdzRpcSwiRegisterEnum result))
            {
                return result;
            }
            throw new ArgumentOutOfRangeException(nameof(swiRegisterEnum), swiRegisterEnum, "Unknown SwiRegisterEnum");
        }

        private static PsdzDatabase.SwiRegisterEnum MapSwiRegisterEnum(PsdzRpcSwiRegisterEnum swiRegisterEnum)
        {
            if (Enum.TryParse(swiRegisterEnum.ToString(), out PsdzDatabase.SwiRegisterEnum result))
            {
                return result;
            }
            throw new ArgumentOutOfRangeException(nameof(swiRegisterEnum), swiRegisterEnum, "Unknown SwiRegisterEnum");
        }

        private static PsdzSwiRegisterGroupEnum MapSwiRegisterGroupEnum(PsdzDatabase.SwiRegisterGroup swiRegisterGroup)
        {
            if (Enum.TryParse(swiRegisterGroup.ToString(), out PsdzSwiRegisterGroupEnum result))
            {
                return result;
            }
            throw new ArgumentOutOfRangeException(nameof(swiRegisterGroup), swiRegisterGroup, "Unknown SwiRegisterGroup");
        }

        private static PsdzDatabase.SwiRegisterGroup MapSwiRegisterGroupEnum(PsdzSwiRegisterGroupEnum swiRegisterGroupEnum)
        {
            if (Enum.TryParse(swiRegisterGroupEnum.ToString(), out PsdzDatabase.SwiRegisterGroup result))
            {
                return result;
            }
            throw new ArgumentOutOfRangeException(nameof(swiRegisterGroupEnum), swiRegisterGroupEnum, "Unknown SwiRegisterGroupEnum");
        }
    }
}
