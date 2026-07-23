using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IProgrammingSession : INotifyPropertyChanged, IDisposable
    {
        BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaCurrent { get; }

        BMW.Rheingold.CoreFramework.Contracts.Programming.IFa FaTarget { get; }

        string IntegrationLevelTarget { get; }

        //IProgrammingApi ProgrammingApi { get; }

        IAPISecurity APISecurity { get; }

        //ISecureEcuModeService SecureEcuModeService { get; }

        //ISecManagementService SecurityManagementService { get; }

        IValidityCondition ValidityCondition { get; }

        IFeatureSpecificField FeatureSpecificField { get; }

        //IComponentTheftProtectionService ComponentTheftProtectionService { get; }

        IPsdzInfo Psdz { get; }

        IPsdzContext PsdzContext { get; }

        ISvt SvtCurrent { get; }

        ISvt SvtTarget { get; }

        string TalAsXml { get; }

        string TalFilterAsXml { get; }

        ITherapyPlanApi TherapyPlanApi { get; }

        double TimeLeftSec { get; }

        bool UseReferenceSvtAsTarget { get; set; }

        [Obsolete("Use the FailedProgrammingEcusActions. This Property is not filled anymore.")]
        IDictionary<IEcu, ProgrammingActionType> FailedProgrammingEcus { get; }

        IDictionary<IEcu, HashSet<ProgrammingActionType>> FailedProgrammingEcusActions { get; }

        ISet<ISmartActuatorEcu> FailedProgrammingSmartActuators { get; }

        ISet<ISmartActuatorMasterEcu> FailedProgrammingSmartActuatorMasters { get; }

        IList<string> GetServiceProgramsForSwiAction(string swiActionName);

        IBoolResultObject ImportSecureToken();

        bool GetProgrammingModeSwitchFromTALExecution();

        void SetProgrammingModeSwitchFromTALExecution(bool value);

        void SetECUsNotToSwitchToProgrammingMode(IList<string> ecus);

        void SetECUsToPreventUDSFallback(IList<string> ecus);

        bool? GetParallelFlashFromTALExecution();

        void SetParallelFlashFromTALExecution(bool valueToSet);

        void SetBackProgrammingModeSwitchFromTALExecution();

        void ClearTalFilter();

        void RestoreDefaultTalFilter();

        void SetFaCurrent(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa);

        void SetFaTarget(BMW.Rheingold.CoreFramework.Contracts.Programming.IFa fa);

        void SetSvtCurrent(ISvt svt);

        void SetVehicleUpdate(IVehicleUpdate vehicleUpdate);

        IVehicleUpdate SpecialPlanRequired(string swiActionName);

        void UpdateSFATalFilterForAllEcus(ISfaPerEcuOptions ecuOptions);

        void UpdateTalFilterForAllEcus(TaCategories[] taCategories, TalFilterOptions talFilterOptions);

        void UpdateSFATalFilterForSelectedEcus(IDictionary<int, ISfaPerEcuOptions> ecuOptions);

        void UpdateTalFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions);

        [Obsolete("This API-Function is deprecated.")]
        void UpdateTalFilterForSelectedEcuOnSweLevel(int diagAddress, TaCategories taCategory, string processClass, TalFilterOptions talFilterOptions, IDictionary<string, TalFilterOptions> sweFilter);

        void UpdateTalFilterForSelectedEcuOnSweLevel(int diagAddress, TaCategories taCategory, string processClass, TalFilterOptions talFilterOptions, List<string> sgbmIds, List<TalFilterOptions> sweTalFilterOptions);

        void DisableCodingSelection(string da);

        void DisableProgrammingSelection(string da);

        void SetConnectionToDCan();

        void SetConnectionPort(int port);

        int GetConnectionPort();

        //IHttpServerResponse RequestProgrammingHttpServer();

        bool IsHddUpdateUrlReachableByVehicle();

        bool SetTargetToBackupILevel();

        bool SetTargetToDefinedILevel(string targetILevel);

        IBoolResultObject SetTargetContext(string newTargetILevel, BMW.Rheingold.CoreFramework.Contracts.Programming.IFa targetFa);

        IBoolResultObject PlanVehicleModifications(List<IPlannedSwiAction> plannedSwiActions);

        IBoolResultObject DeselectVehicleModifications(List<string> swiActionsToDeselect);

        IBoolResultObject SwtDeactivationWhiteListClear();

        IBoolResultObject SwtDeactivationWhiteListFill(string minDiagAdress, string maxDiagAdress, string minAppNumber, string maxAppNumber, string minUpgradeIndex, string maxUpgradeIndex);

        bool IsTargetILevelSetToBackIlevel();

        void SetPsdzPreferredFlashprotocolUDS(int diagAddress);

        bool SetPreExchangeSelectionForEcu(int diagAddress, bool activateSelection);

        bool SetPostExchangeSelectionForEcu(int diagAddress, bool activateSelection);

        IBoolResultObject AddTechnicalActionResultToProtocoll(string taNummer, string taBezeichnung, IList<string> mindestIStufens, bool abArbeitungsstatus, string diagnosisCodeTitle, string diagnoseCodes);

        IBoolResultObject AddRxSwinListToProtocol(List<IRxSwinObject> rxSwinList, bool updateActualContext);

        IBoolResultObject StartVehicleOrderImport();

        IBoolResultObject StartVehicleOrderImportOnlyOnlineOption();

        IBoolResultObject StartVehicleOrderImportOnlyManualOption();

        IBoolResultObject<ISdpPatchResult> SdpPatchAvailable();

        IList<ISdpPatchResult> GetAvailableSdpPatches();

        [Obsolete("This API-Function is deprecated, please use 'GetAvailableSdpPatches()' instead.")]
        IBoolResultObject<IList<ISdpPatchResult>> AvailableSdpPatches();

        IBoolResultObject SdpPatchDownload(string swiDataTarget);

        IBoolResultObject SdpPatchDownload(string swiDataTarget, string newTargetILevel);

        IBoolResultObject CheckAvailabilityOfPsdzConnection();

        IBoolResultObject<IEcuFailureResponseSet> ResetEcus(List<string> hexEcuAddress);

        IBoolResultObject CheckAvailabilityOfSdpPatchStorage();

        IBoolResultObject<long> GetDurationOfWenToken();

        bool? IsSoftwareUpToDate(string ecu);

        IEnumerable<ISgbmIdChange> GetDifferentSgbmIds(string ecu);
    }
}
