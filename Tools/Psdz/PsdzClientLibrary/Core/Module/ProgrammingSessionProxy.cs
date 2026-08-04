using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Contracts;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

#pragma warning disable CS0067, CS0618, CS0649
namespace BMW.Rheingold.Module.ISTA
{
    internal class ProgrammingSessionProxy : IProgrammingSession, INotifyPropertyChanged, IDisposable
    {
        private readonly IProgrammingSessionExt programmingSession;

        private readonly IProtocolBasic fasta;

        private readonly IProgrammingApi programmingApi;

        private readonly IAPISecurity apiSecurity;

        internal IProgrammingSession ProgrammingSession => programmingSession;

        internal IProtocolBasic Fasta => fasta;

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaCurrent => programmingSession.FaCurrent;

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaTarget => programmingSession.FaTarget;

        public string IntegrationLevelTarget => programmingSession.IntegrationLevelTarget;

        public IProgrammingApi ProgrammingApi => programmingApi;

        public IAPISecurity APISecurity => apiSecurity;

        public IPsdzInfo Psdz => programmingSession.Psdz;

        public IPsdzContext PsdzContext => programmingSession.PsdzContext;

        public ISvt SvtCurrent => programmingSession.SvtCurrent;

        public ISvt SvtTarget => programmingSession.SvtTarget;

        public string TalAsXml => programmingSession.TalAsXml;

        public string TalFilterAsXml => programmingSession.TalFilterAsXml;

        public double TimeLeftSec => programmingSession.TimeLeftSec;

        public bool UseReferenceSvtAsTarget
        {
            get
            {
                return programmingSession.UseReferenceSvtAsTarget;
            }
            set
            {
                programmingSession.UseReferenceSvtAsTarget = value;
            }
        }

        public ITherapyPlanApi TherapyPlanApi => programmingSession.TherapyPlanApi;

        public ISecureEcuModeService SecureEcuModeService => programmingSession.SecureEcuModeService;

        public ISecManagementService SecurityManagementService => programmingSession.SecurityManagementService;

        public IComponentTheftProtectionService ComponentTheftProtectionService => programmingSession.ComponentTheftProtectionService;

        public IValidityCondition ValidityCondition => programmingSession.ValidityCondition;

        public IFeatureSpecificField FeatureSpecificField => programmingSession.FeatureSpecificField;

        public IDictionary<IEcu, ProgrammingActionType> FailedProgrammingEcus => programmingSession.FailedProgrammingEcus;

        public IDictionary<IEcu, HashSet<ProgrammingActionType>> FailedProgrammingEcusActions => programmingSession.FailedProgrammingEcusActions;

        public ISet<ISmartActuatorEcu> FailedProgrammingSmartActuators => programmingSession.FailedProgrammingSmartActuators;

        public ISet<ISmartActuatorMasterEcu> FailedProgrammingSmartActuatorMasters => programmingSession.FailedProgrammingSmartActuatorMasters;

        public event PropertyChangedEventHandler PropertyChanged;

        public ProgrammingSessionProxy(IProgrammingSessionExt programmingSession, IProtocolBasic fasta)
        {
            this.programmingSession = programmingSession;
            this.fasta = fasta;
            if (programmingSession == null)
            {
                throw new ArgumentNullException("programmingSession");
            }
            if (fasta == null)
            {
                throw new ArgumentNullException("fasta");
            }
            if (programmingSession.ProgrammingApi != null)
            {
               //[-] programmingApi = new ProgrammingApiProxy(programmingSession.ProgrammingApi, fasta);
            }
            if (programmingSession.APISecurity != null)
            {
                //[-] apiSecurity = new APISecurityProxy(programmingSession.APISecurity, fasta);
            }
        }

        public IList<string> GetServiceProgramsForSwiAction(string swiActionName)
        {
            IMethodCall methodCall = fasta.AddMethodCall("GetServiceProgramsForSwiAction");
            IList<string> serviceProgramsForSwiAction = programmingSession.GetServiceProgramsForSwiAction(swiActionName);
            methodCall.EndTime = DateTime.Now;
            return serviceProgramsForSwiAction;
        }

        public bool? IsSoftwareUpToDate(string ecu)
        {
            IMethodCall methodCall = fasta.AddMethodCall("IsSoftwareUpToDate");
            bool? result = programmingSession.IsSoftwareUpToDate(ecu);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IEnumerable<ISgbmIdChange> GetDifferentSgbmIds(string ecu)
        {
            IMethodCall methodCall = fasta.AddMethodCall("GetDifferentSgbmIds");
            IEnumerable<ISgbmIdChange> differentSgbmIds = programmingSession.GetDifferentSgbmIds(ecu);
            methodCall.EndTime = DateTime.Now;
            return differentSgbmIds;
        }

        public bool GetProgrammingModeSwitchFromTALExecution()
        {
            IMethodCall methodCall = fasta.AddMethodCall("GetProgrammingModeSwitchFromTALExecution");
            bool programmingModeSwitchFromTALExecution = programmingSession.GetProgrammingModeSwitchFromTALExecution();
            methodCall.EndTime = DateTime.Now;
            return programmingModeSwitchFromTALExecution;
        }

        public void SetProgrammingModeSwitchFromTALExecution(bool value)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetProgrammingModeSwitchFromTALExecution");
            programmingSession.SetProgrammingModeSwitchFromTALExecution(value);
            methodCall.EndTime = DateTime.Now;
        }

        public void SetECUsNotToSwitchToProgrammingMode(IList<string> ecus)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetECUsNotToSwitchToProgrammingMode");
            programmingSession.SetECUsNotToSwitchToProgrammingMode(ecus);
            methodCall.EndTime = DateTime.Now;
        }

        public void SetECUsToPreventUDSFallback(IList<string> ecus)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetECUsToPreventUDSFallback");
            programmingSession.SetECUsToPreventUDSFallback(ecus);
            methodCall.EndTime = DateTime.Now;
        }

        public bool? GetParallelFlashFromTALExecution()
        {
            fasta.AddMethodCall("GetParallelFlashFromTALExecution").EndTime = DateTime.Now;
            return programmingSession.GetParallelFlashFromTALExecution();
        }

        public void SetParallelFlashFromTALExecution(bool valueToSet)
        {
            fasta.AddMethodCall("SetParallelFlashFromTALExecution").EndTime = DateTime.Now;
            programmingSession.SetParallelFlashFromTALExecution(valueToSet);
        }

        public void SetBackProgrammingModeSwitchFromTALExecution()
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetBackProgrammingModeSwitchFromTALExecution");
            programmingSession.SetBackProgrammingModeSwitchFromTALExecution();
            methodCall.EndTime = DateTime.Now;
        }

        public void ClearTalFilter()
        {
            IMethodCall methodCall = fasta.AddMethodCall("ClearTalFilter");
            programmingSession.ClearTalFilter();
            methodCall.EndTime = DateTime.Now;
        }

        public void RestoreDefaultTalFilter()
        {
            IMethodCall methodCall = fasta.AddMethodCall("RestoreDefaultTalFilter");
            programmingSession.RestoreDefaultTalFilter();
            methodCall.EndTime = DateTime.Now;
        }

        public void SetFaCurrent(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetFaCurrent");
            programmingSession.SetFaCurrent(fa);
            methodCall.EndTime = DateTime.Now;
        }

        public void SetFaTarget(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetFaTarget");
            programmingSession.SetFaTarget(fa, fasta);
            methodCall.EndTime = DateTime.Now;
        }

        public void SetSvtCurrent(ISvt svt)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetSvtCurrent");
            programmingSession.SetSvtCurrent(svt);
            methodCall.EndTime = DateTime.Now;
        }

        public void SetVehicleUpdate(IVehicleUpdate vehicleUpdate)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetVehicleUpdate", new Dictionary<string, string> {
        {
            "vehicleUpdate",
            (vehicleUpdate == null) ? "null" : vehicleUpdate.ToString()
        } });
            programmingSession.SetVehicleUpdate(vehicleUpdate);
            methodCall.EndTime = DateTime.Now;
        }

        public IVehicleUpdate SpecialPlanRequired(string swiActionName)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SpecialPlanRequired", new Dictionary<string, string> { { "swiActionName", swiActionName } });
            IVehicleUpdate result = programmingSession.SpecialPlanRequired(swiActionName);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public void UpdateTalFilterForAllEcus(TaCategories[] taCategories, TalFilterOptions talFilterOptions)
        {
            IMethodCall methodCall = fasta.AddMethodCall("UpdateTalFilterForAllEcus", new Dictionary<string, string>
        {
            {
                "taCategories",
                LogArray(taCategories)
            },
            {
                "talFilterOptions",
                talFilterOptions.ToString()
            }
        });
            programmingSession.UpdateTalFilterForAllEcus(taCategories, talFilterOptions);
            methodCall.EndTime = DateTime.Now;
        }

        public void UpdateSFATalFilterForAllEcus(ISfaPerEcuOptions ecuOptions)
        {
            IMethodCall methodCall = fasta.AddMethodCall("UpdateSFATalFilterForAllEcus", new Dictionary<string, string>
        {
            {
                "categoryAction",
                ecuOptions.CategoryAction.ToString()
            },
            {
                "sfaWriteAction",
                ecuOptions.SfaWriteAction.ToString()
            },
            {
                "sfaDeleteAction",
                ecuOptions.SfaDeleteAction.ToString()
            }
        });
            programmingSession.UpdateSFATalFilterForAllEcus(ecuOptions);
            methodCall.EndTime = DateTime.Now;
        }

        public void UpdateSFATalFilterForSelectedEcus(IDictionary<int, ISfaPerEcuOptions> ecuOptions)
        {
            IMethodCall methodCall = fasta.AddMethodCall("UpdateSFATalFilterForSelectedEcus");
            programmingSession.UpdateSFATalFilterForSelectedEcus(ecuOptions);
            methodCall.EndTime = DateTime.Now;
        }

        public void UpdateTalFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions)
        {
            IMethodCall methodCall = fasta.AddMethodCall("UpdateTalFilterForSelectedEcus", new Dictionary<string, string>
        {
            {
                "taCategories",
                LogArray(taCategories)
            },
            {
                "diagAddress",
                LogArray(diagAddress)
            },
            {
                "talFilterOptions",
                talFilterOptions.ToString()
            }
        });
            programmingSession.UpdateTalFilterForSelectedEcus(taCategories, diagAddress, talFilterOptions);
            methodCall.EndTime = DateTime.Now;
        }

        public void UpdateTalFilterForSelectedEcuOnSweLevel(int diagAddress, TaCategories taCategory, string processClass, TalFilterOptions talFilterOptions, IDictionary<string, TalFilterOptions> sweFilter)
        {
            IMethodCall methodCall = fasta.AddMethodCall("UpdateTalFilterForSelectedEcuOnSweLevel", new Dictionary<string, string>
        {
            {
                "taCategory",
                taCategory.ToString()
            },
            {
                "diagAddress",
                diagAddress.ToString()
            },
            {
                "talFilterOptions",
                talFilterOptions.ToString()
            },
            { "processClass", processClass },
            {
                "sweFilter",
                LogArray(sweFilter.Select((KeyValuePair<string, TalFilterOptions> x) => $"{x.Key}-{x.Value}").ToArray())
            }
        });
            programmingSession.UpdateTalFilterForSelectedEcuOnSweLevel(diagAddress, taCategory, processClass, talFilterOptions, sweFilter);
            methodCall.EndTime = DateTime.Now;
        }

        public void UpdateTalFilterForSelectedEcuOnSweLevel(int diagAddress, TaCategories taCategory, string processClass, TalFilterOptions talFilterOptions, List<string> sgbmIds, List<TalFilterOptions> sweTalFilterOptions)
        {
            IMethodCall methodCall = fasta.AddMethodCall("UpdateTalFilterForSelectedEcuOnSweLevel", new Dictionary<string, string>
        {
            {
                "taCategory",
                taCategory.ToString()
            },
            {
                "diagAddress",
                diagAddress.ToString()
            },
            {
                "talFilterOptions",
                talFilterOptions.ToString()
            },
            { "processClass", processClass },
            {
                "sweFilter",
                LogArray(sgbmIds.ToArray())
            },
            {
                "sweTalFilterOptions",
                LogArray(sweTalFilterOptions.ToArray())
            }
        });
            programmingSession.UpdateTalFilterForSelectedEcuOnSweLevel(diagAddress, taCategory, processClass, talFilterOptions, sgbmIds, sweTalFilterOptions);
            methodCall.EndTime = DateTime.Now;
        }

        public void DisableCodingSelection(string da)
        {
            IMethodCall methodCall = fasta.AddMethodCall("DisableCodingSelection", new Dictionary<string, string> { { "da", da } });
            programmingSession.DisableCodingSelection(da);
            methodCall.EndTime = DateTime.Now;
        }

        public void DisableProgrammingSelection(string da)
        {
            IMethodCall methodCall = fasta.AddMethodCall("DisableProgrammingSelection", new Dictionary<string, string> { { "da", da } });
            programmingSession.DisableProgrammingSelection(da);
            methodCall.EndTime = DateTime.Now;
        }

        public void SetConnectionToDCan()
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetConnectionToDCan");
            programmingSession.SetConnectionToDCan();
            methodCall.EndTime = DateTime.Now;
        }

        public void SetConnectionPort(int port)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetConnectionPort", new Dictionary<string, string> {
        {
            "port",
            port.ToString() ?? ""
        } });
            programmingSession.SetConnectionPort(port);
            methodCall.EndTime = DateTime.Now;
        }

        public int GetConnectionPort()
        {
            IMethodCall methodCall = fasta.AddMethodCall("GetConnectionPort");
            int connectionPort = programmingSession.GetConnectionPort();
            methodCall.EndTime = DateTime.Now;
            methodCall.ReturnValue = connectionPort.ToString() ?? "";
            return connectionPort;
        }

        public IHttpServerResponse RequestProgrammingHttpServer()
        {
            IMethodCall methodCall = fasta.AddMethodCall("ProgrammingHttpServerRequest");
            IHttpServerResponse httpServerResponse = programmingSession.RequestProgrammingHttpServer();
            methodCall.EndTime = DateTime.Now;
            methodCall.ReturnValue = httpServerResponse?.ToString() ?? "";
            return httpServerResponse;
        }

        public bool IsHddUpdateUrlReachableByVehicle()
        {
            IMethodCall methodCall = fasta.AddMethodCall("HddUpdateServerRequest");
            bool result = programmingSession.IsHddUpdateUrlReachableByVehicle();
            methodCall.EndTime = DateTime.Now;
            methodCall.ReturnValue = result.ToString() ?? "";
            return result;
        }

        public void SetPsdzPreferredFlashprotocolUDS(int diagAddress)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetPsdzPreferredFlashprotocolUDS");
            programmingSession.SetPsdzPreferredFlashprotocolUDS(diagAddress);
            methodCall.EndTime = DateTime.Now;
        }

        public bool SetPreExchangeSelectionForEcu(int diagAddress, bool activateSelection)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetPreExchangeSelectionForEcu");
            bool result = programmingSession.SetPreExchangeSelectionForEcu(diagAddress, activateSelection);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public bool SetPostExchangeSelectionForEcu(int diagAddress, bool activateSelection)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetPostExchangeSelectionForEcu");
            bool result = programmingSession.SetPostExchangeSelectionForEcu(diagAddress, activateSelection);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public bool SetTargetToBackupILevel()
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetTargetToBackupILevel");
            bool result = programmingSession.SetTargetToBackupILevel(fasta);
            methodCall.ReturnValue = result.ToString() ?? "";
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public bool SetTargetToDefinedILevel(string targetILevel)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetTargetToDefinedILevel");
            bool result = programmingSession.SetTargetToDefinedILevel(fasta, targetILevel);
            methodCall.ReturnValue = result.ToString() ?? "";
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject SetTargetContext(string newTargetILevel, BMW.Rheingold.CoreFramework.Contracts.Programming.IFa targetFa)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SetTargetToDefinedILevel");
            IBoolResultObject boolResultObject = programmingSession.SetTargetContext(newTargetILevel, targetFa);
            methodCall.ReturnValue = boolResultObject?.ToString() ?? "";
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public IBoolResultObject PlanVehicleModifications(List<IPlannedSwiAction> plannedSwiActions)
        {
            IMethodCall methodCall = fasta.AddMethodCall("PlanVehicleModifications");
            IBoolResultObject boolResultObject = programmingSession.PlanVehicleModifications(plannedSwiActions);
            methodCall.ReturnValue = boolResultObject?.ToString() ?? "";
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public IBoolResultObject DeselectVehicleModifications(List<string> swiActionsToDeselect)
        {
            IMethodCall methodCall = fasta.AddMethodCall("DeselectVehicleModifications");
            IBoolResultObject boolResultObject = programmingSession.DeselectVehicleModifications(swiActionsToDeselect);
            methodCall.ReturnValue = boolResultObject?.ToString() ?? "";
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public bool IsTargetILevelSetToBackIlevel()
        {
            IMethodCall methodCall = fasta.AddMethodCall("IsTargetILevelSetToBackIlevel");
            bool result = programmingSession.IsTargetILevelSetToBackIlevel(fasta);
            methodCall.ReturnValue = result.ToString() ?? "";
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject AddTechnicalActionResultToProtocoll(string taNummer, string taBezeichnung, IList<string> mindestIStufens, bool abArbeitungsstatus, string diagnosisCodeTitle, string diagnoseCodes)
        {
            IMethodCall methodCall = fasta.AddMethodCall("AddTechnicalActionResultToProtocoll");
            IBoolResultObject boolResultObject = programmingSession.AddTechnicalActionResultToProtocoll(taNummer, taBezeichnung, mindestIStufens, abArbeitungsstatus, diagnosisCodeTitle, diagnoseCodes);
            methodCall.ReturnValue = boolResultObject.ToString();
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public IBoolResultObject SwtDeactivationWhiteListFill(string minDiagAdress, string maxDiagAdress, string minAppNumber, string maxAppNumber, string minUpgradeIndex, string maxUpgradeIndex)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SwtDeactivationWhiteListFill");
            IBoolResultObject boolResultObject = programmingSession.SwtDeactivationWhiteListFill(minDiagAdress, maxDiagAdress, minAppNumber, maxAppNumber, minUpgradeIndex, maxUpgradeIndex);
            methodCall.ReturnValue = boolResultObject.ToString();
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public IBoolResultObject SwtDeactivationWhiteListClear()
        {
            IMethodCall methodCall = fasta.AddMethodCall("SwtDeactivationWhiteListClear");
            IBoolResultObject boolResultObject = programmingSession.SwtDeactivationWhiteListClear();
            methodCall.ReturnValue = boolResultObject.ToString();
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public IBoolResultObject AddRxSwinListToProtocol(List<IRxSwinObject> rxSwinList, bool updateActualContext)
        {
            IMethodCall methodCall = fasta.AddMethodCall("AddRxSwinListToProtocol");
            IBoolResultObject boolResultObject = programmingSession.AddRxSwinListToProtocol(rxSwinList, updateActualContext);
            methodCall.ReturnValue = boolResultObject.ToString();
            methodCall.EndTime = DateTime.Now;
            return boolResultObject;
        }

        public IBoolResultObject StartVehicleOrderImport()
        {
            IMethodCall methodCall = fasta.AddMethodCall("StartVehicleOrderImport");
            IBoolResultObject result = programmingSession.StartVehicleOrderImport();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject StartVehicleOrderImportOnlyOnlineOption()
        {
            IMethodCall methodCall = fasta.AddMethodCall("StartVehicleOrderImport");
            IBoolResultObject result = programmingSession.StartVehicleOrderImportOnlyOnlineOption();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject StartVehicleOrderImportOnlyManualOption()
        {
            IMethodCall methodCall = fasta.AddMethodCall("StartVehicleOrderImport");
            IBoolResultObject result = programmingSession.StartVehicleOrderImportOnlyManualOption();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject ImportSecureToken()
        {
            IMethodCall methodCall = fasta.AddMethodCall("ImportSecureToken");
            IBoolResultObject result = programmingSession.ImportSecureToken();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject<ISdpPatchResult> SdpPatchAvailable()
        {
            IMethodCall methodCall = fasta.AddMethodCall("SdpPatchAvailable");
            IBoolResultObject<ISdpPatchResult> result = programmingSession.SdpPatchAvailable();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IList<ISdpPatchResult> GetAvailableSdpPatches()
        {
            IMethodCall methodCall = fasta.AddMethodCall("GetAvailableSdpPatches");
            IList<ISdpPatchResult> availableSdpPatches = programmingSession.GetAvailableSdpPatches();
            methodCall.EndTime = DateTime.Now;
            return availableSdpPatches;
        }

        public IBoolResultObject<IList<ISdpPatchResult>> AvailableSdpPatches()
        {
            IMethodCall methodCall = fasta.AddMethodCall("AvailableSdpPatches");
            IBoolResultObject<IList<ISdpPatchResult>> result = programmingSession.AvailableSdpPatches();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject SdpPatchDownload(string swiDataTarget)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SdpPatchDownload");
            IBoolResultObject result = programmingSession.SdpPatchDownload(swiDataTarget);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject SdpPatchDownload(string swiDataTarget, string newTargetILevel)
        {
            IMethodCall methodCall = fasta.AddMethodCall("SdpPatchDownload");
            IBoolResultObject result = programmingSession.SdpPatchDownload(swiDataTarget, newTargetILevel);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject CheckAvailabilityOfPsdzConnection()
        {
            IMethodCall methodCall = fasta.AddMethodCall("CheckAvailabilityOfPsdzConnection");
            IBoolResultObject result = programmingSession.CheckAvailabilityOfPsdzConnection();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject<IEcuFailureResponseSet> ResetEcus(List<string> hexEcuAddress)
        {
            IMethodCall methodCall = fasta.AddMethodCall("ResetEcus");
            IBoolResultObject<IEcuFailureResponseSet> result = programmingSession.ResetEcus(hexEcuAddress);
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject CheckAvailabilityOfSdpPatchStorage()
        {
            IMethodCall methodCall = fasta.AddMethodCall("CheckAvailabilityOfSdpPatchStorage");
            IBoolResultObject result = programmingSession.CheckAvailabilityOfSdpPatchStorage();
            methodCall.EndTime = DateTime.Now;
            return result;
        }

        public IBoolResultObject<long> GetDurationOfWenToken()
        {
            IMethodCall methodCall = fasta.AddMethodCall("GetDurationOfWenToken");
            IBoolResultObject<long> durationOfWenToken = programmingSession.GetDurationOfWenToken();
            methodCall.EndTime = DateTime.Now;
            return durationOfWenToken;
        }

        private string LogArray<T>(T[] array)
        {
            if (array == null)
            {
                return "null";
            }
            StringBuilder stringBuilder = new StringBuilder("[");
            foreach (object obj in array)
            {
                if (obj == null)
                {
                    stringBuilder.Append("null");
                }
                else
                {
                    stringBuilder.Append(obj.ToString());
                }
                stringBuilder.Append(",");
            }
            stringBuilder.Replace(',', ']', stringBuilder.Length - 1, 1);
            if (stringBuilder[stringBuilder.Length - 1] != ']')
            {
                stringBuilder.Append("]");
            }
            return stringBuilder.ToString();
        }

        public void Dispose()
        {
        }
    }
}
