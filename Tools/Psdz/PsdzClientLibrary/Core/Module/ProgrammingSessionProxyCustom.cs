using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient;
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
    [PreserveSource(Hint = "Adapted from ProgrammingSessionProxy", SuppressWarning = true)]
    internal class ProgrammingSessionProxyCustom : IProgrammingSession, INotifyPropertyChanged, IDisposable
    {
        private readonly ClientContext clientContext;

        private readonly ProgrammingJobs programmingJobs;

        //private readonly IProgrammingSessionExt programmingSession;

        private readonly IProgrammingApi programmingApi;

        private readonly IAPISecurity apiSecurity;

        //internal IProgrammingSession ProgrammingSession => programmingSession;

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaCurrent => ProgrammingUtils.BuildFa(programmingJobs.PsdzContext.FaActual);

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaTarget => ProgrammingUtils.BuildFa(programmingJobs.PsdzContext.FaTarget);

        public string IntegrationLevelTarget => programmingJobs.PsdzContext.VecInfo.TargetILevel;

        public IProgrammingApi ProgrammingApi => programmingApi;

        public IAPISecurity APISecurity => apiSecurity;

        public IPsdzInfo Psdz => programmingJobs.ProgrammingService.Psdz;

        public IPsdzContext PsdzContext => programmingJobs.PsdzContext;

        public ISvt SvtCurrent => programmingJobs.PsdzContext.SvtCurrent;

        public ISvt SvtTarget => programmingJobs.PsdzContext.SvtTarget;

        public string TalAsXml => programmingJobs.PsdzContext.Tal?.AsXml;

        public string TalFilterAsXml => programmingJobs.PsdzContext.TalFilter?.AsXml;

        public double TimeLeftSec => 0;

        public bool UseReferenceSvtAsTarget
        {
            get
            {
                return false;
            }
            set
            {
                //programmingSession.UseReferenceSvtAsTarget = value;
            }
        }

        public ITherapyPlanApi TherapyPlanApi => null;

        public ISecureEcuModeService SecureEcuModeService => null;

        public ISecManagementService SecurityManagementService => null;

        public IComponentTheftProtectionService ComponentTheftProtectionService => null;

        public IValidityCondition ValidityCondition => null;

        public IFeatureSpecificField FeatureSpecificField => null;

        public IDictionary<IEcu, ProgrammingActionType> FailedProgrammingEcus => null;

        public IDictionary<IEcu, HashSet<ProgrammingActionType>> FailedProgrammingEcusActions => null;

        public ISet<ISmartActuatorEcu> FailedProgrammingSmartActuators => null;

        public ISet<ISmartActuatorMasterEcu> FailedProgrammingSmartActuatorMasters => null;

        public event PropertyChangedEventHandler PropertyChanged;

        public ProgrammingSessionProxyCustom(ClientContext clientContext, ProgrammingJobs programmingJobs)
        {
            this.clientContext = clientContext;
            this.programmingJobs = programmingJobs;
        }

        public IList<string> GetServiceProgramsForSwiAction(string swiActionName)
        {
            //IList<string> serviceProgramsForSwiAction = programmingSession.GetServiceProgramsForSwiAction(swiActionName);
            //return serviceProgramsForSwiAction;
            return null;
        }

        public bool? IsSoftwareUpToDate(string ecu)
        {
            int? diagnosisAddress = programmingJobs.RetrieveDiagnosisAddress(ecu);
            if (!diagnosisAddress.HasValue)
            {
                return null;
            }
            return programmingJobs.PsdzContext.IsSoftwareUpToDate(diagnosisAddress.Value);
        }

        public IEnumerable<ISgbmIdChange> GetDifferentSgbmIds(string ecu)
        {
            int? diagnosisAddress = programmingJobs.RetrieveDiagnosisAddress(ecu);
            if (!diagnosisAddress.HasValue)
            {
                return null;
            }
            return programmingJobs.PsdzContext.GetDifferentSgbmIds(diagnosisAddress.Value);
        }

        public bool GetProgrammingModeSwitchFromTALExecution()
        {
            //bool programmingModeSwitchFromTALExecution = programmingSession.GetProgrammingModeSwitchFromTALExecution();
            //return programmingModeSwitchFromTALExecution;
            return false;
        }

        public void SetProgrammingModeSwitchFromTALExecution(bool value)
        {
            //programmingSession.SetProgrammingModeSwitchFromTALExecution(value);
        }

        public void SetECUsNotToSwitchToProgrammingMode(IList<string> ecus)
        {
            //programmingSession.SetECUsNotToSwitchToProgrammingMode(ecus);
        }

        public void SetECUsToPreventUDSFallback(IList<string> ecus)
        {
            //programmingSession.SetECUsToPreventUDSFallback(ecus);
        }

        public bool? GetParallelFlashFromTALExecution()
        {
            //return programmingSession.GetParallelFlashFromTALExecution();
            return null;
        }

        public void SetParallelFlashFromTALExecution(bool valueToSet)
        {
            //programmingSession.SetParallelFlashFromTALExecution(valueToSet);
        }

        public void SetBackProgrammingModeSwitchFromTALExecution()
        {
            //programmingSession.SetBackProgrammingModeSwitchFromTALExecution();
        }

        public void ClearTalFilter()
        {
            IPsdzTalFilter psdzTalFilter = programmingJobs.ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
            programmingJobs.PsdzContext.SetTalFilter(psdzTalFilter);
        }

        public void RestoreDefaultTalFilter()
        {
            IPsdzTalFilter psdzTalFilter = programmingJobs.ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
            programmingJobs.PsdzContext.SetTalFilter(psdzTalFilter);
        }

        public void SetFaCurrent(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa)
        {
            string vin = programmingJobs.PsdzContext.FaActual.Vin;
            IPsdzFa psdzFa = programmingJobs.ProgrammingService.Psdz.ObjectBuilder.BuildFa(fa, vin);
            programmingJobs.PsdzContext.SetFaActual(psdzFa);
        }

        public void SetFaTarget(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa)
        {
            string vin = programmingJobs.PsdzContext.FaActual.Vin;
            IPsdzFa psdzFa = programmingJobs.ProgrammingService.Psdz.ObjectBuilder.BuildFa(fa, vin);
            programmingJobs.PsdzContext.SetFaTarget(psdzFa);
        }

        public void SetSvtCurrent(ISvt svt)
        {
            string vin = programmingJobs.PsdzContext.FaActual.Vin;
            IPsdzSvt psdzSvt = programmingJobs.ProgrammingService?.Psdz?.ObjectBuilder?.BuildSvt(svt, vin);
            programmingJobs.PsdzContext.SetSvtCurrent(programmingJobs.ProgrammingService, psdzSvt, vin);
        }

        public void SetVehicleUpdate(IVehicleUpdate vehicleUpdate)
        {
            //programmingSession.SetVehicleUpdate(vehicleUpdate);
        }

        public IVehicleUpdate SpecialPlanRequired(string swiActionName)
        {
            //IVehicleUpdate result = programmingSession.SpecialPlanRequired(swiActionName);
            //return result;
            return null;
        }

        public void UpdateTalFilterForAllEcus(TaCategories[] taCategories, TalFilterOptions talFilterOptions)
        {
            programmingJobs.UpdateTalFilterForAllEcus(taCategories, talFilterOptions);
        }

        public void UpdateSFATalFilterForAllEcus(ISfaPerEcuOptions ecuOptions)
        {
            //programmingSession.UpdateSFATalFilterForAllEcus(ecuOptions);
        }

        public void UpdateSFATalFilterForSelectedEcus(IDictionary<int, ISfaPerEcuOptions> ecuOptions)
        {
            //programmingSession.UpdateSFATalFilterForSelectedEcus(ecuOptions);
        }

        public void UpdateTalFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions)
        {
            programmingJobs.UpdateTalFilterForSelectedEcus(taCategories, diagAddress, talFilterOptions);
        }

        public void UpdateTalFilterForSelectedEcuOnSweLevel(int diagAddress, TaCategories taCategory, string processClass, TalFilterOptions talFilterOptions, IDictionary<string, TalFilterOptions> sweFilter)
        {
            //programmingSession.UpdateTalFilterForSelectedEcuOnSweLevel(diagAddress, taCategory, processClass, talFilterOptions, sweFilter);
        }

        public void UpdateTalFilterForSelectedEcuOnSweLevel(int diagAddress, TaCategories taCategory, string processClass, TalFilterOptions talFilterOptions, List<string> sgbmIds, List<TalFilterOptions> sweTalFilterOptions)
        {
            //programmingSession.UpdateTalFilterForSelectedEcuOnSweLevel(diagAddress, taCategory, processClass, talFilterOptions, sgbmIds, sweTalFilterOptions);
        }

        public void DisableCodingSelection(string da)
        {
            //programmingSession.DisableCodingSelection(da);
        }

        public void DisableProgrammingSelection(string da)
        {
            //programmingSession.DisableProgrammingSelection(da);
        }

        public void SetConnectionToDCan()
        {
            //programmingSession.SetConnectionToDCan();
        }

        public void SetConnectionPort(int port)
        {
            //programmingSession.SetConnectionPort(port);
        }

        public int GetConnectionPort()
        {
            //int connectionPort = programmingSession.GetConnectionPort();
            //return connectionPort;
            return 0;
        }

        public IHttpServerResponse RequestProgrammingHttpServer()
        {
            //IHttpServerResponse httpServerResponse = programmingSession.RequestProgrammingHttpServer();
            //return httpServerResponse;
            return null;
        }

        public bool IsHddUpdateUrlReachableByVehicle()
        {
            //bool result = programmingSession.IsHddUpdateUrlReachableByVehicle();
            //return result;
            return false;
        }

        public void SetPsdzPreferredFlashprotocolUDS(int diagAddress)
        {
            //programmingSession.SetPsdzPreferredFlashprotocolUDS(diagAddress);
        }

        public bool SetPreExchangeSelectionForEcu(int diagAddress, bool activateSelection)
        {
            //bool result = programmingSession.SetPreExchangeSelectionForEcu(diagAddress, activateSelection);
            //return result;
            return false;
        }

        public bool SetPostExchangeSelectionForEcu(int diagAddress, bool activateSelection)
        {
            //bool result = programmingSession.SetPostExchangeSelectionForEcu(diagAddress, activateSelection);
            //return result;
            return false;
        }

        public bool SetTargetToBackupILevel()
        {
            programmingJobs.PsdzContext.VecInfo.TargetILevel = programmingJobs.PsdzContext.VecInfo.ILevelBackup;
            return true;
        }

        public bool SetTargetToDefinedILevel(string targetILevel)
        {
            programmingJobs.PsdzContext.VecInfo.TargetILevel = targetILevel;
            return true;
        }

        public IBoolResultObject SetTargetContext(string newTargetILevel, BMW.Rheingold.CoreFramework.Contracts.Programming.IFa targetFa)
        {
            //IBoolResultObject boolResultObject = programmingSession.SetTargetContext(newTargetILevel, targetFa);
            //return boolResultObject;
            return null;
        }

        public IBoolResultObject PlanVehicleModifications(List<IPlannedSwiAction> plannedSwiActions)
        {
            //IBoolResultObject boolResultObject = programmingSession.PlanVehicleModifications(plannedSwiActions);
            //return boolResultObject;
            return null;
        }

        public IBoolResultObject DeselectVehicleModifications(List<string> swiActionsToDeselect)
        {
            //IBoolResultObject boolResultObject = programmingSession.DeselectVehicleModifications(swiActionsToDeselect);
            //return boolResultObject;
            return null;
        }

        public bool IsTargetILevelSetToBackIlevel()
        {
            return programmingJobs.PsdzContext.VecInfo.TargetILevel.Equals(programmingJobs.PsdzContext.VecInfo.ILevelBackup);
        }

        public IBoolResultObject AddTechnicalActionResultToProtocoll(string taNummer, string taBezeichnung, IList<string> mindestIStufens, bool abArbeitungsstatus, string diagnosisCodeTitle, string diagnoseCodes)
        {
            //IBoolResultObject boolResultObject = programmingSession.AddTechnicalActionResultToProtocoll(taNummer, taBezeichnung, mindestIStufens, abArbeitungsstatus, diagnosisCodeTitle, diagnoseCodes);
            //return boolResultObject;
            return null;
        }

        public IBoolResultObject SwtDeactivationWhiteListFill(string minDiagAdress, string maxDiagAdress, string minAppNumber, string maxAppNumber, string minUpgradeIndex, string maxUpgradeIndex)
        {
            //IBoolResultObject boolResultObject = programmingSession.SwtDeactivationWhiteListFill(minDiagAdress, maxDiagAdress, minAppNumber, maxAppNumber, minUpgradeIndex, maxUpgradeIndex);
            //return boolResultObject;
            return null;
        }

        public IBoolResultObject SwtDeactivationWhiteListClear()
        {
            //IBoolResultObject boolResultObject = programmingSession.SwtDeactivationWhiteListClear();
            //return boolResultObject;
            return null;
        }

        public IBoolResultObject AddRxSwinListToProtocol(List<IRxSwinObject> rxSwinList, bool updateActualContext)
        {
            //IBoolResultObject boolResultObject = programmingSession.AddRxSwinListToProtocol(rxSwinList, updateActualContext);
            //return boolResultObject;
            return null;
        }

        public IBoolResultObject StartVehicleOrderImport()
        {
            //IBoolResultObject result = programmingSession.StartVehicleOrderImport();
            //return result;
            return null;
        }

        public IBoolResultObject StartVehicleOrderImportOnlyOnlineOption()
        {
            //IBoolResultObject result = programmingSession.StartVehicleOrderImportOnlyOnlineOption();
            //return result;
            return null;
        }

        public IBoolResultObject StartVehicleOrderImportOnlyManualOption()
        {
            //IBoolResultObject result = programmingSession.StartVehicleOrderImportOnlyManualOption();
            //return result;
            return null;
        }

        public IBoolResultObject ImportSecureToken()
        {
            //IBoolResultObject result = programmingSession.ImportSecureToken();
            //return result;
            return null;
        }

        public IBoolResultObject<ISdpPatchResult> SdpPatchAvailable()
        {
            //IBoolResultObject<ISdpPatchResult> result = programmingSession.SdpPatchAvailable();
            //return result;
            return null;
        }

        public IList<ISdpPatchResult> GetAvailableSdpPatches()
        {
            //IList<ISdpPatchResult> availableSdpPatches = programmingSession.GetAvailableSdpPatches();
            //return availableSdpPatches;
            return null;
        }

        public IBoolResultObject<IList<ISdpPatchResult>> AvailableSdpPatches()
        {
            //IBoolResultObject<IList<ISdpPatchResult>> result = programmingSession.AvailableSdpPatches();
            //return result;
            return null;
        }

        public IBoolResultObject SdpPatchDownload(string swiDataTarget)
        {
            //IBoolResultObject result = programmingSession.SdpPatchDownload(swiDataTarget);
            //return result;
            return null;
        }

        public IBoolResultObject SdpPatchDownload(string swiDataTarget, string newTargetILevel)
        {
            //IBoolResultObject result = programmingSession.SdpPatchDownload(swiDataTarget, newTargetILevel);
            //return result;
            return null;
        }

        public IBoolResultObject CheckAvailabilityOfPsdzConnection()
        {
            //IBoolResultObject result = programmingSession.CheckAvailabilityOfPsdzConnection();
            //return result;
            return null;
        }

        public IBoolResultObject<IEcuFailureResponseSet> ResetEcus(List<string> hexEcuAddress)
        {
            //IBoolResultObject<IEcuFailureResponseSet> result = programmingSession.ResetEcus(hexEcuAddress);
            //return result;
            return null;
        }

        public IBoolResultObject CheckAvailabilityOfSdpPatchStorage()
        {
            //IBoolResultObject result = programmingSession.CheckAvailabilityOfSdpPatchStorage();
            //return result;
            return null;
        }

        public IBoolResultObject<long> GetDurationOfWenToken()
        {
            //IBoolResultObject<long> durationOfWenToken = programmingSession.GetDurationOfWenToken();
            //return durationOfWenToken;
            return null;
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
