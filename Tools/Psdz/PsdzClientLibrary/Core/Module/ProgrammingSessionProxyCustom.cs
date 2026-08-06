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

        private readonly IProgrammingSessionExt programmingSession;

        private readonly IProgrammingApi programmingApi;

        private readonly IAPISecurity apiSecurity;

        internal IProgrammingSession ProgrammingSession => programmingSession;

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaCurrent => ProgrammingUtils.BuildFa(programmingJobs.PsdzContext.FaActual);

        public BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaTarget => ProgrammingUtils.BuildFa(programmingJobs.PsdzContext.FaTarget);

        public string IntegrationLevelTarget => programmingSession.IntegrationLevelTarget;

        public IProgrammingApi ProgrammingApi => programmingApi;

        public IAPISecurity APISecurity => apiSecurity;

        public IPsdzInfo Psdz => programmingSession.Psdz;

        public IPsdzContext PsdzContext => programmingJobs.PsdzContext;

        public ISvt SvtCurrent => programmingJobs.PsdzContext.SvtCurrent;

        public ISvt SvtTarget => programmingJobs.PsdzContext.SvtTarget;

        public string TalAsXml => programmingJobs.PsdzContext.Tal?.AsXml;

        public string TalFilterAsXml => programmingJobs.PsdzContext.TalFilter?.AsXml;

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

        public ProgrammingSessionProxyCustom(ClientContext clientContext, ProgrammingJobs programmingJobs)
        {
            this.clientContext = clientContext;
            this.programmingJobs = programmingJobs;
        }

        public IList<string> GetServiceProgramsForSwiAction(string swiActionName)
        {
            IList<string> serviceProgramsForSwiAction = programmingSession.GetServiceProgramsForSwiAction(swiActionName);
            return serviceProgramsForSwiAction;
        }

        public bool? IsSoftwareUpToDate(string ecu)
        {
            bool? result = programmingSession.IsSoftwareUpToDate(ecu);
            return result;
        }

        public IEnumerable<ISgbmIdChange> GetDifferentSgbmIds(string ecu)
        {
            IEnumerable<ISgbmIdChange> differentSgbmIds = programmingSession.GetDifferentSgbmIds(ecu);
            return differentSgbmIds;
        }

        public bool GetProgrammingModeSwitchFromTALExecution()
        {
            bool programmingModeSwitchFromTALExecution = programmingSession.GetProgrammingModeSwitchFromTALExecution();
            return programmingModeSwitchFromTALExecution;
        }

        public void SetProgrammingModeSwitchFromTALExecution(bool value)
        {
            programmingSession.SetProgrammingModeSwitchFromTALExecution(value);
        }

        public void SetECUsNotToSwitchToProgrammingMode(IList<string> ecus)
        {
            programmingSession.SetECUsNotToSwitchToProgrammingMode(ecus);
        }

        public void SetECUsToPreventUDSFallback(IList<string> ecus)
        {
            programmingSession.SetECUsToPreventUDSFallback(ecus);
        }

        public bool? GetParallelFlashFromTALExecution()
        {
            return programmingSession.GetParallelFlashFromTALExecution();
        }

        public void SetParallelFlashFromTALExecution(bool valueToSet)
        {
            programmingSession.SetParallelFlashFromTALExecution(valueToSet);
        }

        public void SetBackProgrammingModeSwitchFromTALExecution()
        {
            programmingSession.SetBackProgrammingModeSwitchFromTALExecution();
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
            programmingSession.SetVehicleUpdate(vehicleUpdate);
        }

        public IVehicleUpdate SpecialPlanRequired(string swiActionName)
        {
            IVehicleUpdate result = programmingSession.SpecialPlanRequired(swiActionName);
            return result;
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
            programmingSession.SetPsdzPreferredFlashprotocolUDS(diagAddress);
        }

        public bool SetPreExchangeSelectionForEcu(int diagAddress, bool activateSelection)
        {
            bool result = programmingSession.SetPreExchangeSelectionForEcu(diagAddress, activateSelection);
            return result;
        }

        public bool SetPostExchangeSelectionForEcu(int diagAddress, bool activateSelection)
        {
            bool result = programmingSession.SetPostExchangeSelectionForEcu(diagAddress, activateSelection);
            return result;
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
            IBoolResultObject boolResultObject = programmingSession.SetTargetContext(newTargetILevel, targetFa);
            return boolResultObject;
        }

        public IBoolResultObject PlanVehicleModifications(List<IPlannedSwiAction> plannedSwiActions)
        {
            IBoolResultObject boolResultObject = programmingSession.PlanVehicleModifications(plannedSwiActions);
            return boolResultObject;
        }

        public IBoolResultObject DeselectVehicleModifications(List<string> swiActionsToDeselect)
        {
            IBoolResultObject boolResultObject = programmingSession.DeselectVehicleModifications(swiActionsToDeselect);
            return boolResultObject;
        }

        public bool IsTargetILevelSetToBackIlevel()
        {
            programmingJobs.PsdzContext.VecInfo.TargetILevel = programmingJobs.PsdzContext.VecInfo.ILevel;
            return true;
        }

        public IBoolResultObject AddTechnicalActionResultToProtocoll(string taNummer, string taBezeichnung, IList<string> mindestIStufens, bool abArbeitungsstatus, string diagnosisCodeTitle, string diagnoseCodes)
        {
            IBoolResultObject boolResultObject = programmingSession.AddTechnicalActionResultToProtocoll(taNummer, taBezeichnung, mindestIStufens, abArbeitungsstatus, diagnosisCodeTitle, diagnoseCodes);
            return boolResultObject;
        }

        public IBoolResultObject SwtDeactivationWhiteListFill(string minDiagAdress, string maxDiagAdress, string minAppNumber, string maxAppNumber, string minUpgradeIndex, string maxUpgradeIndex)
        {
            IBoolResultObject boolResultObject = programmingSession.SwtDeactivationWhiteListFill(minDiagAdress, maxDiagAdress, minAppNumber, maxAppNumber, minUpgradeIndex, maxUpgradeIndex);
            return boolResultObject;
        }

        public IBoolResultObject SwtDeactivationWhiteListClear()
        {
            IBoolResultObject boolResultObject = programmingSession.SwtDeactivationWhiteListClear();
            return boolResultObject;
        }

        public IBoolResultObject AddRxSwinListToProtocol(List<IRxSwinObject> rxSwinList, bool updateActualContext)
        {
            IBoolResultObject boolResultObject = programmingSession.AddRxSwinListToProtocol(rxSwinList, updateActualContext);
            return boolResultObject;
        }

        public IBoolResultObject StartVehicleOrderImport()
        {
            IBoolResultObject result = programmingSession.StartVehicleOrderImport();
            return result;
        }

        public IBoolResultObject StartVehicleOrderImportOnlyOnlineOption()
        {
            IBoolResultObject result = programmingSession.StartVehicleOrderImportOnlyOnlineOption();
            return result;
        }

        public IBoolResultObject StartVehicleOrderImportOnlyManualOption()
        {
            IBoolResultObject result = programmingSession.StartVehicleOrderImportOnlyManualOption();
            return result;
        }

        public IBoolResultObject ImportSecureToken()
        {
            IBoolResultObject result = programmingSession.ImportSecureToken();
            return result;
        }

        public IBoolResultObject<ISdpPatchResult> SdpPatchAvailable()
        {
            IBoolResultObject<ISdpPatchResult> result = programmingSession.SdpPatchAvailable();
            return result;
        }

        public IList<ISdpPatchResult> GetAvailableSdpPatches()
        {
            IList<ISdpPatchResult> availableSdpPatches = programmingSession.GetAvailableSdpPatches();
            return availableSdpPatches;
        }

        public IBoolResultObject<IList<ISdpPatchResult>> AvailableSdpPatches()
        {
            IBoolResultObject<IList<ISdpPatchResult>> result = programmingSession.AvailableSdpPatches();
            return result;
        }

        public IBoolResultObject SdpPatchDownload(string swiDataTarget)
        {
            IBoolResultObject result = programmingSession.SdpPatchDownload(swiDataTarget);
            return result;
        }

        public IBoolResultObject SdpPatchDownload(string swiDataTarget, string newTargetILevel)
        {
            IBoolResultObject result = programmingSession.SdpPatchDownload(swiDataTarget, newTargetILevel);
            return result;
        }

        public IBoolResultObject CheckAvailabilityOfPsdzConnection()
        {
            IBoolResultObject result = programmingSession.CheckAvailabilityOfPsdzConnection();
            return result;
        }

        public IBoolResultObject<IEcuFailureResponseSet> ResetEcus(List<string> hexEcuAddress)
        {
            IBoolResultObject<IEcuFailureResponseSet> result = programmingSession.ResetEcus(hexEcuAddress);
            return result;
        }

        public IBoolResultObject CheckAvailabilityOfSdpPatchStorage()
        {
            IBoolResultObject result = programmingSession.CheckAvailabilityOfSdpPatchStorage();
            return result;
        }

        public IBoolResultObject<long> GetDurationOfWenToken()
        {
            IBoolResultObject<long> durationOfWenToken = programmingSession.GetDurationOfWenToken();
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
