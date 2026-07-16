using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework
{
    public interface ILogic : ISession, INotifyPropertyChanged
    {
        //IList<ISdpPatchBomContent> SdpPatchBomContents { get; set; }

        //IstaOperationOwnerData OperationOwnerData { get; set; }

        //List<VinNotSendDataModel> RestrictedVins { get; set; }

        bool IsSendFastaDataForbidden { get; }

        bool IsSendFastaDataForbiddenBitsQueueFull { get; set; }

        bool IsSendOBFCMDataForbidden { get; }

        bool IsPrintPopupOpen { get; set; }

        bool AbortHddUpdate { get; set; }

        bool IsTherapyPlanStateExecuted { get; set; }

        bool ZgwRepairDetected { get; set; }

        //IGlobalSettingsObject GlobalSettings { get; }

        IDiagnosticsBusinessData DiagnosticsBusinessData { get; }

        //VersionInformation VersionInfo { get; }

        DateTime OperationStartTime { get; }

        //Dealer Dealer { get; }

        SessionInfo SessionInfo { get; }

        UiBrand Brand { get; }

        string IstaCaseId { get; }

        IOperationServices Services { get; }

        new Vehicle VecInfo { get; }

        IBackendCallsWatchDog BackendCallWatchDog { get; }

        new IFFMDynamicResolver FFMResolver { get; }

        //IVehicleDataLogic VehicleDataLogic { get; }

        new IFasta2Service Fasta2Service { get; }

        //EnumVCIConnectionType VciConnType { get; }

        IEcuKom EcuKomInterface { get; }

        //FaultFilter FaultFilterSettings { get; }

        //IProgrammingSessionData ProgrammingSessionDataContext { get; }

        //IProgrammingSessionExt ProgrammingSession { get; set; }

        IProgrammingService ProgrammingService { get; }

        //IKmmService KmmService { get; }

        //IApplicationState ApplicationState { get; }

        bool IsVehicleCommunicationRunning { get; }

        bool IsVehicleIdentifyedAndVinNotXxxxxxx { get; }

        bool IsFaultMemoryExistent { get; }

        //IInfoObjectFactory Factory { get; }

        bool NewsDisclaimerDone { get; set; }

        //ISWTProcessor SWTProcessor { get; }

        IList<string> Lang { get; }

        bool IsCarbDataRelevant { get; }

        bool ShouldSetPsdzConnectionToDcan { get; set; }

        int ConnectionPort { get; set; }

        int ZgwRepairEscalation { get; set; }

        ICollection<string> PukCaseInfoGuid { get; }

        //IIndustrialCustomerManager IndustrialCustomer { get; }

        //TransactionMetaData OperationContinued { get; }

        int OpenedRepairManualsCount { get; set; }

        IEcuKom EcuKom { get; }

        //void ExportDocument(IProgressMonitor monitor, IXepInfoObject infoObject);

        void FillVehicleEcusFromDatabaseIfAllowed();

        void ClearTestplan();

        //void ClearErrorMemory(IJobServices services);

        //void ChangeApplicationState(IApplicationState stateNew);

        bool IsModuleExecutionRunning();

        void EvaluateRulesFromSdpPatch();

        void DownloadSdpPatches(string sdpPatchesPath);

        IBoolResultObject PerformTypeKeyIdent(IProgressMonitor monitor);

        IBoolResultObject PerformNugetIdent(IProgressMonitor monitor);

        bool ActivateKL15();

        //bool FilterDTCRelevance(DTC dtc, ICollection<ZFSResult> zfs);

        //bool FilterDTCRelevance(DTC dtc, ICollection<ZFSResult> zfs, FaultFilter faultFilter);

        void SwitchToInfoSession();

        void DisconnectVCI();

        string GetSgbmIdNavD();

        void ResourceCleanup();

        void DisconnectVCI(VCIDevice device);

        void DisconnectEcuKom();

        BoolResultObject HandleVCI(ref VCIDevice device, bool continueVecInfo);

        void OnPropertyChanged(string info);

        IBoolResultObject IdentifyVehicle(IProgressMonitor monitor, VCIDevice device, bool continueVecInfo, IdentificationLevel idLev);

        bool DeactivateKL15(IProgressMonitor monitor);

        //bool IsInfoObjectExecutable(InfoObject infoObj);

        //void AddFaultPattern(XEP_PERCEIVEDSYMPTOMSEX add);

        //void RemoveFaultPattern(XEP_PERCEIVEDSYMPTOMSEX perceivedSymptom);

        bool IsInTestplan(decimal infoObjId);

        void ProtocolTestplanIfNecessary();

        void UpdateVehicle();

        //void DoVehicleTest(IProgressMonitor monitor, VehicleTestMode testMode);

        ObservableCollectionEx<VCIDevice> FindConnections();

        string SendFastaDataToFBM(string filename, bool forceSend);

        void CheckAlternativePowerComponents();

        bool StartTherapyPlanCalculation(IProgressMonitor progressMonitor);

        void ResetPowerSafeMode();

        void StartOperation(IdentificationLevel? identification);

        //TransferStateType UploadFilesToPUK(IEnumerable<PukFile> files);

        //void SaveAndSendFstdat(VehicleTestResult vehicleTest);

        void EvaluateRulesFromExecutionBreak();

        bool HasEcuKom();

        bool SpecialCaseOfGatewayIssueDetected();

        LayoutGroup FindLayoutGroupVehicleTest();

        bool AddFileToVehicleCase(string filename);

        void SetBitsQueueFull(bool bitsFull);

        bool IsVehicleConnectionOnlineUpdated();

        void DoVehicleIdentAfterZgwRepair();

        IBoolResultObject CheckVinOverConnectionLossPopup(IProgressMonitor monitor, VCIDevice device);

        //IEnumerable<IXepInfoObject> FilterToyotaObfcmIdentificator(IEnumerable<IXepInfoObject> xepInfoObjects);

        IBoolResultObject ReleaseReservedIcom(VCIDevice selectedDevice);

        IBoolResultObject CheckVinAndConnectOverConnectionManager(IProgressMonitor progressMonitor, VCIDevice vciDevice);

        //IBoolResultObject CheckIfNVIIsEnabled(IProgressMonitor monitor, VCIDevice device, bool continueVecInfo, IdentificationLevel idLev, ObjectCalculationObjectType? fastaNode);

        IEnumerable<IEcuJob> ClearErrorInfoMemory();

        IEnumerable<IEcuJob> ReadErrorInfoMemory();

        void UpdatePatchDataFromOnlineServices();

        bool IsKnownConnectionType(VCIDeviceType VCIType);

        bool StartVehicleIdentification(IProgressMonitor monitor, ref string errorText, bool forceOldVehicleIdentificationProcess = false, bool comingFromInfosession = false);

        void UpdateVehicleInfoParallel();

        void CheckAndSetBrandMotorrad();

        void CheckAndSetFA();

        void DoInitialElectricalChecks();

        void UpdateAlpinaCharacteristics();

        void UpdateVehicleInfosViaServiceHistory();

        void UpdateVehicleInfosViaTechnicalCampaigns();

        void HandleBN2000();

        void AskForGWSZ(IProgressMonitor monitor);

        bool CalculateKIS();

        void CreatePukVehicleCase(string vin);

        void SpecialTreatmentToyota();

        //void AddSuspiciousItem(IModule module, InfoObject infoObject, XEP_DIAGNOSISOBJECTSEX diagObj);
    }
}
