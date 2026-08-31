using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer;
using BMW.Rheingold.CoreFramework.Feedback;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using Microsoft.Win32;
using PsdzClient;
using PsdzClient.Contracts;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.FASTA;

#pragma warning disable CS0649, CS0618, CS0169
namespace BMW.Rheingold.RheingoldSessionController
{
    [PreserveSource(Hint = "Simplified", SuppressWarning = true)]
    public class Logic : ILogic, ISession, INotifyPropertyChanged
    {
        public enum ConnectionLossError
        {
            IcomNetworkFailure,
            IcomReleaseFailure,
            VehicleVinNotMatch
        }

        //private readonly ConnectionLogic logicConnection = new ConnectionLogic();

        private readonly bool hideBogusFaults;

        private readonly bool hideUnknownFaults;

        //private readonly FaultFilter faultFilterSettings;

        [PreserveSource(Added = true)]
        private readonly ClientContext clientContext;

        [PreserveSource(Added = true)]
        private readonly ProgrammingJobs programmingJobs;

        private readonly PsdzDatabase database;

        //protected MultidimensionalApplicationState applicationState;

        //private readonly VehicleDataConverter vdc;

        public bool fastaRunningBackground;

        private Vehicle vecInfo;

        private IOperationServices services;

        protected internal IEcuKom ecuKom;

        //protected TransactionMetaData metaData;

        //protected ModuleLoader moduleLoader;

        //protected readonly IstaOperationDataContext operationDataContext;

        //private VehicleIdent vecIdent;

        private readonly ISWTProcessor swtProcessor;

        private IFFMDynamicResolver ffmResolver;

        private IProgrammingSessionExt programmingSession;

        //private IVehicleDataLogic vehicleDataLogic;

        private IProgrammingSessionData programmingSessionDataContext;

        private IBackendCallsWatchDog backendCallsWatchDogLogic;

        private IDiagnosticsBusinessData diagnosticsBusinessData;

        private bool checkILevelAgainstLatestPossibleInKisAlreadyCarriedOut;

        //private EslProtocoller eslProtocoller;

        private bool ablGesPreptime;

        private bool ablGesDurationNuFl;

        private bool isSendFastaDataForbiddenBitsQueueFullField;

        //private List<VinNotSendDataModel> restrictedVins = new List<VinNotSendDataModel>();

        public bool operationIsBeingCreated;

        private bool operationMissmatchingVinCancelled;

        private int countAblges;

        private SpecialSecurityCases detectedSpecialSecurityCase;

        protected SpecialSecurityCases successfulSec4CnAuthentication;

        private Action calculateTestplan;

        private Dealer dealer;

        private SessionInfo sessionInfo;

        public bool IsPrintPopupOpen { get; set; }

        //public IstaOperationOwnerData OperationOwnerData { get; set; }
#if false
        public List<VinNotSendDataModel> RestrictedVins
        {
            get
            {
                return restrictedVins;
            }
            set
            {
                if (restrictedVins != value)
                {
                    restrictedVins = value;
                    OnPropertyChanged("RestrictedVins");
                    OnPropertyChanged("IsSendFastaDataForbidden");
                    OnPropertyChanged("IsSendOBFCMDataForbidden");
                }
            }
        }
#endif
        public bool IsSendFastaDataForbidden => true;

        public bool IsSendOBFCMDataForbidden
        {
            get
            {
                return false;
            }
        }

        public bool IsSendFastaDataForbiddenBitsQueueFull
        {
            get
            {
                return isSendFastaDataForbiddenBitsQueueFullField;
            }
            set
            {
                if (!isSendFastaDataForbiddenBitsQueueFullField.Equals(value))
                {
                    isSendFastaDataForbiddenBitsQueueFullField = value;
                    OnPropertyChanged("IsSendFastaDataForbidden");
                }
            }
        }

        public IFeedbackViewHeaderTitleHelper FeedbackViewHeaderTitleHelper { get; private set; }

        //public TransactionMetaData OperationContinued { get; set; }

        //public IDatabaseProvider DatabaseProvider => database;

        //public IIndustrialCustomerManager IndustrialCustomer => IndustrialCustomerManager.Instance;

        public IBackendCallsWatchDog BackendCallWatchDog => backendCallsWatchDogLogic;

        public IDiagnosticsBusinessData DiagnosticsBusinessData => diagnosticsBusinessData;

        public string SerializedProgrammingSessionData { get; private set; }

        public ISessionLogic SessionLogic { get; protected set; }

        //public PukVehicleCaseData VehicleCase { get; set; }

        public ICollection<string> PukCaseInfoGuid { get; }

        public ISet<decimal> FaultPatternImportedFromPuk { get; }

        public bool RunNativeMeasurePlan { get; set; }

        //public IInfoObjectFactory Factory { get; internal set; }

        //public VersionInformation VersionInfo { get; }

        public ISWTProcessor SWTProcessor => swtProcessor;

        public IFFMDynamicResolver FFMResolver
        {
            get
            {
                return ffmResolver;
            }
            set
            {
                ffmResolver = value;
            }
        }

        //public IGlobalSettingsObject GlobalSettings { get; }

        public bool IsVehicleConnectionTSOnline { get; set; }

        public bool ZgwRepairDetected { get; set; }

        public bool IsTherapyPlanStateExecuted { get; set; }

        public UiBrand Brand => UiBrand.BMWBMWi;

        public string IstaCaseId { get; protected set; }

        public bool IsLogArchivToUpload { get; protected set; }

        public virtual bool IsProblemHandlingTraceRunning { get; protected set; }

        public bool OperationIsBeingCreated
        {
            get
            {
                return operationIsBeingCreated;
            }
            set
            {
                operationIsBeingCreated = value;
            }
        }

        public IEcuKom EcuKom
        {
            get
            {
                return ecuKom;
            }
            protected internal set
            {
                ecuKom = value;
                if (value != null && IsProblemHandlingTraceRunning)
                {
                    ecuKom.SetLogLevelToMax();
                }
            }
        }

        public IEcuKom EcuKomInterface => EcuKom;
#if false
        public TransactionMetaData MetaData
        {
            get
            {
                return metaData;
            }
            set
            {
                if (!object.Equals(value, metaData))
                {
                    Log.Info("Logic.MetaData_set", "Replace meta data from \"{0}\" to \"{1}\".", metaData, value);
                    metaData = value;
                    OnPropertyChanged("MetaData");
                }
            }
        }
#endif
        public IOperationServices Services
        {
            get
            {
                return services;
            }
            protected set
            {
                if (services != value)
                {
                    services = value;
                    OnPropertyChanged("Services");
                }
            }
        }

        public virtual Vehicle VecInfo
        {
            get
            {
                return vecInfo;
            }
            set
            {
                if (vecInfo != value)
                {
                    if (vecInfo != null)
                    {
                        vecInfo.PropertyChanged -= VecInfo_PropertyChanged;
                    }
                    vecInfo = value;
                    OnPropertyChanged("VecInfo");
                    if (vecInfo != null)
                    {
                        vecInfo.PropertyChanged += VecInfo_PropertyChanged;
                    }
                }
            }
        }
#if false
        public virtual IApplicationState ApplicationState
        {
            get
            {
                throw new NotSupportedException("ApplicationState only available from operation.");
            }
        }

        public EnumVCIConnectionType VciConnType => GlobalSettings.AppVCIConnectionType;
#endif
        public Dealer Dealer
        {
            get
            {
                return dealer; //?? LicenseHelper.DealerInstance;
            }
            set
            {
                dealer = value;
            }
        }

        public bool IsVehicleCommunicationRunning
        {
            get
            {
                if (VecInfo != null)
                {
                    return VecInfo.Status_FunctionState == StateType.running;
                }
                return false;
            }
        }

        //public FaultFilter FaultFilterSettings => faultFilterSettings;

        public IProgrammingService ProgrammingService => ServiceLocator.Current.GetService<IProgrammingService>();

        public IList<ISdpPatchResult> SdpPatchResults { get; set; }

        //public IList<ISdpPatchBomContent> SdpPatchBomContents { get; set; }

        public virtual IProgrammingSessionData ProgrammingSessionDataContext
        {
            get
            {
                return programmingSessionDataContext;
            }
            set
            {
                if (programmingSessionDataContext != value)
                {
                    programmingSessionDataContext = value;
                    OnPropertyChanged("ProgrammingSessionDataContext");
                }
            }
        }

        public IProgrammingSessionExt ProgrammingSession
        {
            get
            {
                return programmingSession;
            }
            set
            {
                programmingSession = value;
                OnPropertyChanged("ProgrammingSession");
            }
        }

        [PreserveSource(Added = true)]
        public ClientContext ClientContext => clientContext;

        [PreserveSource(Added = true)]
        public ProgrammingJobs ProgrammingJobs => programmingJobs;
#if false
        public IKmmService KmmService => ServiceLocator.Current.GetService<IKmmService>();
#endif
        public IFasta2Service Fasta2Service { get; private set; }

        internal bool SimulationFileWasSent { get; set; }
#if false
        public IVehicleDataLogic VehicleDataLogic
        {
            get
            {
                if (vehicleDataLogic == null || vehicleDataLogic.Brand != ConfigSettings.SelectedBrand)
                {
                    vehicleDataLogic = VehicleDataLogicFactory.Create();
                }
                return vehicleDataLogic;
            }
        }

        protected bool IsFastaAndTransactionEnabledForSimulation
        {
            get
            {
                EnumFASTATransferMode enumFASTATransferMode = EnumFASTATransferMode.BITSUpload;
                if (ReadBooleanRegistryKey("FASTAEnabledForSimulation") && EnumFASTATransferMode.None.Equals(enumFASTATransferMode) && !ReadBooleanRegistryKey("TransactionDataFileUploadEnabled"))
                {
                    return !ReadBooleanRegistryKey("TransactionBitsUploadEnabled");
                }
                return false;
            }
        }
#endif
        public bool NewsDisclaimerDone { get; set; }

        public DateTime OperationStartTime { get; set; }

        public IList<string> Lang { get; }

        public bool IsCarbDataRelevant
        {
            get
            {
                return false;
            }
        }

        public bool CanCleanErrorMemory
        {
            get
            {
                return false;
            }
        }

        public string IstaGuiLogFullName { get; set; }

        public bool ShouldSetPsdzConnectionToDcan { get; set; }

        public int ConnectionPort { get; set; }

        public int ZgwRepairEscalation { get; set; }

        public bool HasInputBeenDetectedInModule { get; set; }

        public bool IsInputListenerActive { get; set; }

        public object QDMInfoList { get; set; }

        public int OpenedRepairManualsCount { get; set; }

        public Task EslTask { get; set; }

        public bool IsVehicleIdentifyedAndVinNotXxxxxxx
        {
            get
            {
                return false;
            }
        }

        public bool IsFaultMemoryExistent
        {
            get
            {
                return false;
            }
        }

        public bool AbortHddUpdate { get; set; }

        public SessionInfo SessionInfo => null;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetTestPlanAction(Action calculateTestplan)
        {
            this.calculateTestplan = calculateTestplan;
        }

        public string GetSgbmIdNavD()
        {
            return string.Empty;
        }

        public Logic()
            : this(null, null)
        {
        }

        public Logic(ClientContext clientContext, ProgrammingJobs programmingJobs)
        {
            Lang = new List<string>();
            Lang.Add("de-DE");
            this.clientContext = clientContext;
            this.programmingJobs = programmingJobs;
            this.database = clientContext.Database;
            FaultPatternImportedFromPuk = new HashSet<decimal>();
            PukCaseInfoGuid = new List<string>();
            Services = null;
            //LogicAssemblyVersionInfo logicAssemblyVersionInfo = new LogicAssemblyVersionInfo();
            //VersionInfo = new VersionInformation(logicAssemblyVersionInfo.GetInfo(), ConfigSettings.GetIstaConfigString("ProductVersion", null), ConfigSettings.GetIstaConfigString("DataVersion", null), ConfigSettings.GetIstaConfigString("MainProductVersion", null), ConfigSettings.GetIstaConfigString("SWIData", null), this.database, ConfigSettings.GetIstaConfigString("SWIVersionQueue", null));
            SetLauncherLangVersion(ConfigSettings.CurrentUICulture);
            LogVersionInfo();
            SessionLogic = null;
            //Factory = InfoObjectFactory.Instance;
            //Fasta2Service = fasta2;
            if (Fasta2Service == null)
            {
                Fasta2Service = new Fasta2ServiceNop();
            }
            //applicationState = new MultidimensionalApplicationState(this);
            //FeedbackViewHeaderTitleHelper = ServiceLocator.Current.GetService<IFeedbackViewHeaderTitleHelper>();
            //if (FeedbackViewHeaderTitleHelper == null)
            //{
                //ServiceLocator.Current.RemoveService<IFeedbackViewHeaderTitleHelper>();
                //FeedbackViewHeaderTitleHelper = new FeedbackViewHeaderTitleHelper();
                //ServiceLocator.Current.AddService(FeedbackViewHeaderTitleHelper);
            //}
            Log.LocalIP();
            Log.Threads();
            //GlobalSettings = GlobalSettingsObject.TryLoadFromRegistry();
            //GlobalSettings.PropertyChanged += GlobalSettingsPropertyChanged;
            //SetupVehicle(new VCIDevice(VCIDeviceType.UNKNOWN, "none", "none"));
            //faultFilterSettings = new FaultFilter();
            backendCallsWatchDogLogic = ServiceLocator.Current.GetService<IBackendCallsWatchDog>();
            if (ConfigSettings.OperationalMode != OperationalMode.ISTA && ConfigSettings.OperationalMode != OperationalMode.OPAPI)
            {
                hideBogusFaults = ReadBooleanRegistryKey("TesterGUI.HideBogusFaults", defaultValue: true);
                hideUnknownFaults = ReadBooleanRegistryKey("TesterGUI.HideUnknownFaults");
            }
            else
            {
                hideBogusFaults = true;
                hideUnknownFaults = true;
            }
            //vehicleDataLogic = null;
            //ProgrammingSession = CreateProgrammingSession();
            InitProgrammingSessionData();
            if (ReadBooleanRegistryKey("BMW.Rheingold.RheingoldSessionController.Logic.SetupSWTProcessorV1"))
            {
                //swtProcessor = new SwtProcessorService(new SWTProcessorImpl(this));
            }
            else
            {
                //swtProcessor = new SwtProcessorV3Service(new SWTProcessorV3Impl(this));
            }
            //vdc = new VehicleDataConverter(this.database);
            ConnectionPort = -1;
            ZgwRepairEscalation = 0;
            diagnosticsBusinessData = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            SetCaseId();
        }

        protected virtual void SetCaseId()
        {
            //IstaCaseId = SessionInfo?.IstaCaseId;
            Log.Info("Logic.Logic()", "IstaCaseId: {0}", IstaCaseId);
            //InfoProvider.IstaCaseId = IstaCaseId;
        }

        private void LogVersionInfo()
        {
            Log.Info("Logic.Logic()", "*************** *************** *************** Version Information  *************** *************** ***************");
        }

        private void GlobalSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
#if false
        public virtual void AcceptOperation(TransactionMetaData metaDataAccepted, VCIDevice infoSession = null)
        {
            throw new NotImplementedException("AcceptOperation is only in IstaOperationLogic available.");
        }
#endif
        public void SetLauncherLangVersion(string lang)
        {
        }

        public virtual void SetBitsQueueFull(bool bitsFull)
        {
        }

        public virtual void VecInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            if (!(propertyName == "VIN17"))
            {
                if (propertyName == "BrandName" && VecInfo.BrandName.HasValue && SessionInfo != null)
                {
                    SessionInfo.BuNo = Dealer.DealerData.GetDealerNumber(BMW.Rheingold.CoreFramework.Utility.EnumConverter.ConvertBrandNameToContractsBrandName(VecInfo.BrandName));
                }
            }
            else if (!string.IsNullOrWhiteSpace(VecInfo.VIN17))
            {
                HandleDataProtectionSettings();
            }
        }

#if false
        public void ChangeApplicationState(IApplicationState stateNew)
        {
            applicationState.SetState(stateNew);
        }
#endif
        public virtual bool IsModuleExecutionMinimized()
        {
            return false;
        }

        public virtual bool IsModuleExecutionRunning()
        {
            return false;
        }

        public virtual bool IsInSearchResults(string xepId)
        {
            throw new NotImplementedException();
        }

#if false
        public virtual void AddSuspiciousItem(IModule module, InfoObject infoObject, XEP_DIAGNOSISOBJECTSEX diagObj)
        {
        }

        private void AddItemCalcPriority(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, IModule m, InfoObject infoObject, XEP_DIAGNOSISOBJECTSEX diagObj)
        {
        }

        public virtual void AddToHitList(InfoObject infoObject, InfoBrowserTreeType browserTreeType)
        {
            throw new NotImplementedException();
        }

        public virtual void AddFaultPattern(XEP_PERCEIVEDSYMPTOMSEX add)
        {
        }

        public virtual void RemoveFaultPattern(XEP_PERCEIVEDSYMPTOMSEX perceivedSymptom)
        {
        }
#endif
        public virtual bool IsInTestplan(decimal infoObjId)
        {
            return true;
        }

        public virtual void ProtocolTestplanIfNecessary()
        {
        }

        public void ClearTestplan()
        {
        }

        public bool IsDiagnosticFeedbackAdded()
        {
            return Fasta2Service.HasFeedback;
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool CheckVehicleVIN(IProgressMonitor monitor)
        {
            return true;
        }

        public void DoVehicleIdentAfterZgwRepair()
        {
            if (ZgwRepairDetected && IsTherapyPlanStateExecuted)
            {
                string errorText = string.Empty;
                ZgwRepairDetected = false;
                IsTherapyPlanStateExecuted = false;
                //ProgressMonitor monitor = new ProgressMonitor();
                Log.Info("Logic.DoVehicleIdentAfterZgwRepair", "ZgwRepair is done successfully, next step is do vehicle test");
                //StartVehicleIdentification(monitor, ref errorText);
            }
            else
            {
                Log.Info("Logic.DoVehicleIdentAfterZgwRepair", "Vehicle Identification is not triggered, IsTherapyPlanStateExecuted = {0}", IsTherapyPlanStateExecuted.ToString());
            }
        }

        public bool StartVehicleIdentification(IProgressMonitor monitor, ref string errorText, bool forceOldVehicleIdentificationProcess = false, bool comingFromInfosession = false)
        {
            Log.Info(Log.CurrentMethod(), "started.");
            if (vecInfo == null || vecInfo.VCI == null || vecInfo.VCI.VCIType == VCIDeviceType.UNKNOWN)
            {
                return false;
            }
            if (comingFromInfosession)
            {
                VecInfo.ECU.Clear();
                VecInfo.FA = new FA();
            }
            return true;
        }

        private void FinalizeECUConfiguration()
        {
            VehicleLogistics.CalculateECUConfiguration(VecInfo);
            FillVehicleEcusFromDatabaseIfAllowed();
        }

        private bool HasConnectedVehicle()
        {
            if (vecInfo.VCI.VCIType != VCIDeviceType.INFOSESSION && VecInfo.VIN17 != null)
            {
                return !VecInfo.VIN17.Contains("XXXX");
            }
            return false;
        }

        private bool CanStartVehicleIdentWithVehicle()
        {
            if ((vecInfo.ECU.Count == 0 || vecInfo.HasUnidentifiedECU()) && vecInfo.VCI != null)
            {
                return vecInfo.VCI.VCIType != VCIDeviceType.INFOSESSION;
            }
            return false;
        }

        private bool IsVehicleIgnition()
        {
            return false;
        }

        private void CheckForSpezificOldEReihe(IProgressMonitor monitor)
        {
            if ("E30 E32 E34 E36 E38 E39 E46 E52 E53".Contains(VecInfo.Ereihe))
            {
                if (vecInfo.FA != null && vecInfo.FA.SA != null && vecInfo.FA.SA.Count != 0)
                {
                    vecInfo.FA.AlreadyDone = true;
                }
            }
            else
            {
                UpdateVehicleIdentLevelOnline();
            }
        }

        public bool ValidateSecurityVehicleAndCheckReactorEnable()
        {
            return true;
        }

        private bool OldInfosession(IProgressMonitor monitor)
        {
            VecInfo.VehicleIdentLevel = IdentificationLevel.VINBasedFeatures;
            return true;
        }

        public void UpdateAlpinaCharacteristics()
        {
        }

        private bool StartVehicleIdentificationWithVehicle(IProgressMonitor monitor, bool comingFromInfosession)
        {
            return true;
        }

        private bool SpecialIndustrialCustomerTreatment()
        {
            return true;
        }
#if false
        public IBoolResultObject CheckIfNVIIsEnabled(IProgressMonitor monitor, VCIDevice device, bool continueVecInfo, IdentificationLevel idLev, ObjectCalculationObjectType? fastaNode)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }
#endif
        public IBoolResultObject PerformNugetIdent(IProgressMonitor monitor)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }

        public bool CalculateKIS()
        {
            return true;
        }

        public IBoolResultObject PerformTypeKeyIdent(IProgressMonitor monitor)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }

        public void SpecialTreatmentToyota()
        {
        }

        public void AskForGWSZ(IProgressMonitor monitor)
        {
            if (monitor != null && VecInfo.BNType == BNType.BNK01X_MOTORBIKE && !VecInfo.GWSZReadoutSuccess)
            {
                monitor.RequestConfirmation(ProgressRequestConfirmationType.GWSZInput, VecInfo);
            }
        }

        private void CalculateECUConfiguration(IProgressMonitor monitor)
        {
        }

        public void HandleBN2000()
        {
        }

        public void CheckAndSetFA()
        {
            if (vecInfo.FA != null && vecInfo.FA.SA != null && vecInfo.FA.SA.Count != 0 && VecInfo.BNType != BNType.BNK01X_MOTORBIKE && VecInfo.Ereihe != "R13" && VecInfo.Ereihe != "C01" && VecInfo.Ereihe != "K14" && VecInfo.Ereihe != "K17")
            {
                vecInfo.FA.AlreadyDone = true;
            }
        }

        public void CheckAndSetBrandMotorrad()
        {
            if (vecInfo.BNType == BNType.BN2000_MOTORBIKE || vecInfo.BNType == BNType.BN2020_MOTORBIKE || vecInfo.BNType == BNType.BNK01X_MOTORBIKE)
            {
                vecInfo.BrandName = BrandName.BMWMOTORRAD;
            }
        }

        public void DoInitialElectricalChecks()
        {
        }

        public bool IsVehicleConnectionOnlineUpdated()
        {
            if (VecInfo.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated || VecInfo.VehicleIdentLevel == IdentificationLevel.VINVehicleReadoutOnlineUpdated)
            {
                return true;
            }
            return false;
        }

        private void CheckConstructionDateInfo()
        {
        }

        public void CreatePukVehicleCase(string vin)
        {
        }
#if false
        public virtual IIstaPukService CreatePukServiceClient()
        {
        }

        public virtual IIstaMetaDataService CreateMetaDataServiceClient()
        {
            return new IstaMetaDataServiceClient();
        }
#endif
        public void FillVehicleEcusFromDatabaseIfAllowed()
        {
            FillVehicleEcusFromDatabase();
        }

        private void FillVehicleEcusFromDatabase()
        {
        }
#if false
        public virtual void ClearErrorMemory(IJobServices services)
        {
        }
#endif
        private void CheckILevelAgainstLatestPossibleInKisIfNotAlreadyDone()
        {
            if (checkILevelAgainstLatestPossibleInKisAlreadyCarriedOut)
            {
                Log.Info(Log.CurrentMethod(), "I-level has already been checked against the kis, no need to do it again.");
                return;
            }
        }

        public bool PerformVehicleTest(IProgressMonitor monitor, string fastaNode, bool jumpToMeasurePlanAfterVehicleTest)
        {
            Log.Info("Logic.PerformVehicleTest", "Method entered.");
            return true;
        }
#if false
        internal void WriteFastaAfterVehicleTest(VehicleTestResult vehicleTestResult, string fastaNodeName)
        {
        }

        private VehicleTestResult DoVehicleTest(IProgressMonitor monitor, bool jumpToMeasurePlanAfterVehicleTest)
        {
            throw new NotImplementedException();
        }
#endif
        private string FormatServiceCodeDetails(IList<IEcuJob> jobList, TimeSpan? diff, TimeSpan? meanValueDtc, TimeSpan? identDuration, TimeSpan? fullDuration)
        {
            List<string> values = new List<string>
        {
            "ICOM Connection: '" + (vecInfo.VCI?.LocalAdapterNetworkType.ToString() ?? "-") + "'",
            "E-series: '" + (vecInfo.Ereihe ?? "-") + "'",
            "Service Pack: '" + GetServicePackInfo() + "'",
            "Backend Availability (number of Timeouts): '" + (BackendCallWatchDog.LatestBackendResponse?.Count((KeyValuePair<BackendServiceType, HttpStatusCode> x) => x.Value == HttpStatusCode.RequestTimeout).ToString() ?? "-") + "'",
            "Number of ECUs: '" + (VecInfo.ECU?.Count.ToString() ?? "-") + "'",
            "Number of ECUs with status OK: '" + (VecInfo.ECU?.Count((ECU x) => x.IDENT_SUCCESSFULLY).ToString() ?? "-") + "'",
            "Number of ECUs with status not OK: '" + (VecInfo.ECU?.Count((ECU x) => !x.IDENT_SUCCESSFULLY).ToString() ?? "-") + "'",
            "Number of ABLGES: '" + (countAblges.ToString() ?? "-") + "'",
            "Number of DTCs (FS_LESEN_Detail): '" + (vecInfo.ECU?.Count((ECU x) => x.F_ANZ != 0).ToString() ?? "-") + "'",
            "Number of FASTA jobs: '" + (jobList.Count.ToString() ?? "-") + "'",
            "Duration full Identification: '" + FormatDuration(fullDuration) + "'",
            "Duration Identification: '" + FormatDuration(identDuration) + "'",
            "Duration of shown Popups: '" + TimeMetricsUtility.Instance.GetPopupDuration().ToString("hh\\:mm\\:ss\\.fff") + "'",
            "Duration of vehicle test: '" + FormatDuration(diff) + "'",
            "Duration of ABLGES: '" + FormatDuration(ImportantLoggingItem.DurationAblges) + "'",
            "Duration of FS_LESEN_DETAIL for all DTCs: '" + FormatDuration(ImportantLoggingItem.DurationFSLesen) + "'",
            "Mean value of Duration per DTC: '" + FormatDuration(meanValueDtc) + "'",
            "Duration of FASTA readout: '" + FormatDuration(ImportantLoggingItem.DurationfastaReadout) + "'"
        };
            return string.Join("; ", values);
        }

        private string FormatDuration(TimeSpan? duration)
        {
            return duration?.ToString() ?? "-";
        }

        private string GetServicePackInfo()
        {
            if (VecInfo.BNType == BNType.BN2020)
            {
                if (diagnosticsBusinessData.IsEES25Vehicle(VecInfo))
                {
                    return "NCAR";
                }
                if (!VecInfo.Classification.IsSp2021)
                {
                    if (!VecInfo.Classification.IsSp2025)
                    {
                        return "-";
                    }
                    return "SP2025";
                }
                return "SP2021";
            }
            return "-";
        }
#if false
        private VehicleTestResult DoZgwRepair(IProgressMonitor monitor, bool jumpToMeasurePlanAfterVehicleTest)
        {
            throw new NotImplementedException();
        }
#endif
        public void SendSpeedlinkDataInBackground()
        {
        }

        private void ReadFSFromAdditionalECUsKey(IProgressMonitor monitor, List<long> ecuListAfterDiagnose)
        {
        }

        private void InquireHsfzGateway()
        {
        }

        private bool VehicleCommunicationInterfaceIsPTT()
        {
            if (VecInfo.VCI != null)
            {
                return VecInfo.VCI.VCIType == VCIDeviceType.PTT;
            }
            return false;
        }

        private bool FastaDataShouldBeReadInBackground(bool jumpToMeasurePlanAfterVehicleTest)
        {
            return false;
        }

        protected virtual bool ReadBooleanRegistryKey(string registryKeyName, bool defaultValue = false)
        {
            return ConfigSettings.getConfigStringAsBoolean(registryKeyName, defaultValue);
        }

        private IList<IEcuJob> ReadFastaFromVehicleInBackGround(IProgressMonitor monitor)
        {
            Task<IList<IEcuJob>> task = new Task<IList<IEcuJob>>(() => ReadFastaFromVehicle(monitor));
            fastaRunningBackground = true;
            task.Start();
            _ = monitor.ProcessDescription;
            monitor.ProcessDescription = null;
            monitor.IsRunningInBackground = true;
            task.Wait();
            IList<IEcuJob> result = task.Result;
            monitor.TaskDescription = null;
            return result;
        }

        private void InitProgrammingSessionData()
        {
        }
#if false
        private ProgrammingNote CreateBeforeVehicleTestProgrammingNote()
        {
            throw new NotImplementedException();
        }
#endif
        internal virtual bool FastaDataReadConditionComplied(bool readConditionEnabled = true)
        {
            if (ConfigSettings.IsISTAModeRITA)
            {
                Log.Info("Logic.FastaDataReadConditionComplied()", "Return false for operational mode \"{0}\".", ConfigSettings.OperationalMode);
                return false;
            }
            if (!readConditionEnabled)
            {
                Log.Info("Logic.FastaDataReadConditionComplied()", "Return true because FastaData.ReadCondition.Enabled \"{0}\".", readConditionEnabled);
                return true;
            }
            int configint = ConfigSettings.getConfigint("BMW.Rheingold.FastaData.ReadCondition.ReadAgainAfter.KilometerTraveled", 1);
            int configint2 = ConfigSettings.getConfigint("BMW.Rheingold.FastaData.ReadCondition.ReadAgainAfter.DaysPast", 1);
            if (configint2 <= 0 || configint <= 0)
            {
                Log.Info("Logic.FastaDataReadConditionComplied()", "Return true, because of configuration: KilometerTraveled {0}, DaysPast {1}.", configint2, configint);
                return true;
            }
            return false;
        }
#if false
        private TransactionMetaData FindNewest(string vin17, IEnumerable<TransactionMetaData> metaDataList)
        {
            throw new NotImplementedException();
        }
#endif
        private void WaitWhileFunctionEqualRunning(IProgressMonitor monitor)
        {
        }

        public virtual void UpdateVehicle()
        {
        }
#if false
        public virtual void DoVehicleTest(IProgressMonitor monitor, VehicleTestMode testMode)
        {
        }
#endif
        private void EvaluateOverallTestModules(IProgressMonitor monitor)
        {
        }

        public IEnumerable<IXepInfoObject> FilterToyotaObfcmIdentificator(IEnumerable<IXepInfoObject> xepInfoObjects)
        {
            return null;
        }

        public IList<IEcuJob> ReadFastaFromVehicle(IProgressMonitor monitor)
        {
            IList<IEcuJob> list = new List<IEcuJob>();
            return list;
        }

        public bool AddFileToVehicleCase(string filename)
        {
            return false;
        }

        public IBoolResultObject SendBatteryData(Dictionary<string, string> batteryDataDictionary)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }
#if false
        public PDIRequest CreatePDIRequest(string vin17, long km)
        {
            throw new NotImplementedException();
        }
#endif
        public void SendObfcmDataToBackend(int statusMessage, bool privacyConsentOBFCM, double fuelSystem_Overall_Fuel, double fuelSystem_Overall_ReferenceDistance, double fuelSystem_InChargeDepleting_Fuel, double fuelSystem_InChargeDepleting_EngineOff_ReferenceDistance, double fuelSystem_InChargeDepleting_EngineOn_ReferenceDistance, double fuelSystem_InChargeIncreasing_Fuel, double fuelSystem_InChargeIncreasing_ReferenceDistance, double electricEngine_Overall_GridEnergy, double electricEngine_Overall_ReferenceDistance, double electricEngine_EngineOff_GridEnergy, double electricEngine_EngineOff_ReferenceDistance, double eletricEngine_EngineOn_GridEnergy, double electricEngine_EngineOn_ReferenceDistance, string produceTimestamp)
        {
        }

        private string CreateOBFCMFileNameWithPath()
        {
            string text = DateTime.Now.ToString("yyyyMMdd");
            string text2 = DateTime.Now.ToString("HHmmss");
            string path = "OBFCM_" + vecInfo.VIN17 + "_" + text + "_" + text2 + ".xml";
            return Path.Combine(ConfigSettings.getConfigString("BMW.Rheingold.Logging.Directory", "..\\..\\..\\logs"), path);
        }

        public string SendFastaDataToFBM(string filename, bool forceSend)
        {
            //Discarded unreachable code: IL_0085, IL_00b9, IL_0163
            return filename;
        }
#if false
        private typeTransferState SendFastaDataToAssistant(string filePath, string fileName)
        {
            throw new NotImplementedException();
        }

        private LauncherMessageType GetMessageTypeForAssistant(string path)
        {
            throw new NotImplementedException();
        }

        private typeTransferState HandleOssMode(string filename, string fileNoPath)
        {
            throw new NotImplementedException();
        }

        private void UpdateTransferState(string fileNoPath, typeTransferState transferState)
        {
        }

        public virtual void WriteMetaData()
        {
        }

        private void SetTransferstateToDone(typeTransferUnit unit)
        {
        }

        private void SetTransferstateToDoneIfFastaAndTransactionEnabledForSimulation()
        {
        }
#endif
        public string GetTransactionFileName(string vin17, DateTime startTime)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, "RG_TRANS_{0}{1}{2}", vin17, startTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture), startTime.ToString("HHmmss", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Log.WarningException("FASTA.getTransactionFileName()", exception);
            }
            return null;
        }

        public string GetTherapyPlanFileName(string vin17, DateTime startTime)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, "RG_THERAPYPLAN_{0}{1}{2}", vin17, startTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture), startTime.ToString("HHmmss", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Log.WarningException("FASTA.GetTherapyPlanFileName()", exception);
            }
            return null;
        }

        public string GetTalFileName(string vin17, DateTime startTime)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, "RG_TAL_{0}{1}{2}", vin17, startTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture), startTime.ToString("HHmmss", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Log.WarningException("FASTA.GetTalName()", exception);
            }
            return null;
        }

        public string GetIdentResultFileName(string vin17, DateTime startTime)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, "RG_IDENTVEHICLE_{0}{1}{2}", vin17, startTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture), startTime.ToString("HHmmss", CultureInfo.InvariantCulture));
            }
            catch (Exception exception)
            {
                Log.WarningException("FASTA.GetIdentResultFileName()", exception);
            }
            return null;
        }

        public string WriteTransactionData()
        {
            return null;
        }

        public void WriteTherapyPlanData()
        {
        }

        public void WriteTalData()
        {
        }

        public void WriteIdentResultData()
        {
        }

        public virtual void ParseSimulationFile(string simFile)
        {
        }

        protected internal virtual void SetupVehicle(VCIDevice vciDevice)
        {
        }

        public virtual void ResourceCleanup()
        {
        }

        public string SetupDatabase(bool doDbCacheInit)
        {
            return null;
        }

        public ObservableCollectionEx<VCIDevice> FindConnections()
        {
            return new ObservableCollectionEx<VCIDevice>();
        }

        public string ResetLang()
        {
            string text = Lang.ToStringItems();
            Lang.Clear();
            string text2;
            using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
            {
                if (istaIcsServiceClient.IsAvailable())
                {
                    text2 = istaIcsServiceClient.GetMarketLanguage();
                }
                else
                {
                    text2 = "en-GB";
                    Log.Info("Logic.ResetLang()", "Set the MarktLanguage to \"{0}\", because iLean is not available.", text2);
                }
            }
            text2 = ((text2 == null || text2.Length != 5) ? ConfigSettings.GetCulture(text2) : text2);
            Lang.Add(ConfigSettings.CurrentUICulture);
            Lang.AddIfNotContains(text2);
            Lang.AddIfNotContains("en-GB");
            Log.Info("Logic.ResetLang()", "Change from \"{0}\" to \"{1}\".", text, Lang.ToStringItems());
            return text2;
        }
#if false
        protected virtual IProgrammingSessionExt CreateProgrammingSession()
        {
            return null;
        }
#endif
        public virtual void StartOperation(IdentificationLevel? identification)
        {
        }

#if false
        public void ReadContinuedOperationProgrammingData(TransactionMetaData continuedOperation)
        {
        }
#endif
        public void UpdatePatchDataFromOnlineServices()
        {
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OnlinePatch.Serviceprogram.IsActive", defaultValue: true))
            {
                UpdatePatchServicePrograms();
            }
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OnlinePatch.Validity.IsActive", defaultValue: true))
            {
                UpdateValidityPatches();
            }
            if (ConfigSettings.GetActivateSdpOnlinePatch())
            {
                EvaluateRulesFromSdpPatch();
            }
        }

        private void AddServiceCodeRegardingOsBitness()
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                string value = CreateValueForServiceCodeOsBitness();
                Fasta2Service.AddServiceCode("ENV01_RunningBitWindows", value, LayoutGroup.D);
            }
        }

        private string CreateValueForServiceCodeOsBitness()
        {
            string text = Dealer.DealerData?.DistributionPartnerNumber ?? string.Empty;
            string text2 = Dealer.DealerData?.OutletNumber;
            string machineName = Environment.MachineName;
            string text3 = "Bit - Version(32)";
            return text + ", " + text2 + ", " + machineName + ", " + text3;
        }

        private void UpdateValidityPatches()
        {
        }

        public virtual void EvaluateRulesFromExecutionBreak()
        {
        }

        public virtual void EvaluateRulesFromSdpPatch()
        {
        }

        public void DownloadSdpPatches(string sdpPatchesPath)
        {
        }

        private string GetSdpPatchDirectory(string swiDataTarget, IPsdz psdz)
        {
            string rootDirectory = psdz.ConfigurationService.GetRootDirectory();
            Log.Info(Log.CurrentMethod(), "Read out current PSdZ Data path: {0}", rootDirectory);
            string fullName = Directory.GetParent(rootDirectory).Parent.FullName;
            if (fullName != null && fullName.EndsWith("SDP-Patch"))
            {
                return Path.Combine(fullName, swiDataTarget.Replace(".", "-"));
            }
            return Path.Combine(fullName, "SDP-Patch", swiDataTarget.Replace(".", "-"));
        }

        private void UpdatePatchServicePrograms()
        {
        }
#if false
        public virtual void SaveAndSendFstdat(VehicleTestResult vehicleTest)
        {
        }
#endif
        public virtual bool ActivateKL15()
        {
            return true;
        }

        public virtual bool DeactivateKL15(IProgressMonitor monitor)
        {
            Log.Info("Logic.DeactivateKL15()", "started.");
            return true;
        }

        public virtual IBoolResultObject IdentifyVehicle(IProgressMonitor monitor, VCIDevice device, bool continueVecInfo, IdentificationLevel idLev)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }

        public IBoolResultObject CheckVinOverConnectionLossPopup(IProgressMonitor monitor, VCIDevice device)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }

        private BoolResultObject ContinueVecInfo(BoolResultObject resultObject, IProgressMonitor monitor, IVciDevice device, ref string errorText)
        {
            resultObject.Result = CheckVehicleVIN(monitor);
            return resultObject;
        }

        private BoolResultObject PrepareVehicleIdentification(BoolResultObject resultObject, IProgressMonitor monitor, IdentificationLevel idLev, VCIDevice device, ref string errorText)
        {
            resultObject.Result = StartVehicleIdentification(monitor, ref errorText);
            resultObject.ErrorMessage = errorText;
            return resultObject;
        }

        public bool IsKnownConnectionType(VCIDeviceType VCIType)
        {
            switch (VCIType)
            {
                case VCIDeviceType.ENET:
                case VCIDeviceType.ICOM:
                case VCIDeviceType.EDIABAS:
                case VCIDeviceType.SIM:
                case VCIDeviceType.INFOSESSION:
                case VCIDeviceType.PTT:
                    return true;
                default:
                    return false;
            }
        }

        public virtual void SendFBMPing(IdentificationLevel idLev)
        {
        }

        public virtual BoolResultObject HandleVCI(ref VCIDevice device, bool continueVecInfo)
        {
            return HandleVCI(ref device, continueVecInfo, firstVciInitialisation: false);
        }

        public virtual BoolResultObject HandleVCI(ref VCIDevice device, bool continueVecInfo, bool firstVciInitialisation)
        {
            BoolResultObject boolResultObject = HandleVCIDevice(ref device, continueVecInfo, firstVciInitialisation);
            return boolResultObject;
        }

        private BoolResultObject HandleVCIDevice(ref VCIDevice device, bool continueVecInfo, bool firstVciInitialisation)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }

        private bool SpecialCaseNeedsContinuation(IBoolResultObject resultObject, bool firstVciInitialisation)
        {
            string method = Log.CurrentMethod();
            bool flag = SpecialCaseOfGatewayIssueDetected();
            if (EcuKom != null)
            {
                detectedSpecialSecurityCase = EcuKom.DetectedSpecialSecurityCase();
            }
            Log.Info(method, "Data from resultObject - Result: '" + resultObject.Result + "', " + $"ErrorCodeInt: '{resultObject.ErrorCodeInt}', " + "ErrorCode: '" + resultObject.ErrorCode + "', ErrorMessage: '" + resultObject.ErrorMessage + "'.");
            Log.Info(method, "detectedSpecialSecurityCase - '" + detectedSpecialSecurityCase.ToString() + "'");
            switch (detectedSpecialSecurityCase)
            {
                case SpecialSecurityCases.IpbCertificatesRequired:
                    return firstVciInitialisation;
                case SpecialSecurityCases.Sec4CnTokenRequiredForSp21:
                case SpecialSecurityCases.Sec4CnTokenRequiredForSp18:
                    return firstVciInitialisation || flag;
                default:
                    return detectedSpecialSecurityCase != SpecialSecurityCases.None;
            }
        }

        private void ResetTemporaryInstancesAndDisconnectVCI()
        {
            EcuKom?.End();
            EcuKom = null;
            DisconnectVCI();
        }

        private void SetupVehicleAndCheckVINLength(VCIDevice device)
        {
            SetupVehicle(device);
            if (!string.IsNullOrEmpty(device.VIN))
            {
                if (device.VIN.Length == 17)
                {
                    vecInfo.VIN17 = device.VIN;
                }
                if (device.VIN.Length == 7)
                {
                    vecInfo.VIN17 = "XXXXXXXXXX" + device.VIN;
                }
            }
        }

        private void SetEcuErrorCodeAndText(string logWarningMessage, string errorCode, string errorText)
        {
            Log.Warning("Logic.HandleVCI()", logWarningMessage + " with error: " + errorCode + "/" + errorText);
            ecuKom = null;
        }

        public void CheckContinueVecInfo(VCIDevice device, bool continueVecInfo)
        {
            if (continueVecInfo)
            {
                vecInfo.VCI = device;
                return;
            }
            SetupVehicle(device);
        }

        private static void UpdateDeviceInfo(VCIDevice device, VCIDevice vciDevice)
        {
            device.Owner = vciDevice.Owner;
            device.Description = vciDevice.Description;
            device.ReserveHandle = vciDevice.ReserveHandle;
            device.VCIReservation = vciDevice.VCIReservation;
            device.Serial = vciDevice.Serial;
            device.Kl15Voltage = vciDevice.Kl15Voltage;
            device.Kl30Voltage = vciDevice.Kl30Voltage;
            device.Kl30Trigger = vciDevice.Kl30Trigger;
            device.Kl15Trigger = vciDevice.Kl15Trigger;
            device.VciChannels = vciDevice.VciChannels;
            device.LocalAdapterNetworkType = vciDevice.LocalAdapterNetworkType;
            device.PwfState = vciDevice.PwfState;
            device.IsDoIP = vciDevice.IsDoIP;
        }
#if false
        private void SetVehicleCommunication(TransactionMetaData meta, VCIDeviceType vci)
        {
        }

        public void SetVehicleCommunication(TransactionMetaData meta, VCIDeviceType vci, bool enableSim)
        {
        }
#endif
        public bool ReadVehicleTags()
        {
            return true;
        }

        public void ShowVehicleBreakdownPopupIfNotShowedAlready()
        {
        }

        public virtual void UpdateVehicleInfosViaDOM()
        {
        }

        public virtual void UpdateVehicleInfosViaSSL2()
        {
        }

        public virtual void UpdateVehicleInfosViaSSL2OldIdent()
        {
        }
#if false
        private void UpdateEcusViaSsl2ForVINBasedReadout(List<EcuDto> ssl2Ecus, IDatabaseProvider dbProvider)
        {
        }

        private void UpdateEcusViaSsl2ForVehicleReadout(List<EcuDto> ssl2Ecus, IDatabaseProvider dbProvider)
        {
        }

        private ECU CheckForAlreadyIdentifiedEcus(EcuDto sslEcu, IEcu ecu)
        {
            return null;
        }

        private ECU GetEcuByEcuGroupForPreE65Vehicle(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, EcuDto sslEcu, IDatabaseProvider dbProvider)
        {
            return null;
        }

        private void SetEcuPropertiesBasedOnSsl2DataForVehicleReadout(ECU ecu, EcuDto sslEcu)
        {
        }

        private void SetMissingVariantInformationForContextEcu(ECU contextEcu, ECU ecuBasedOnCvs, BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, VehicleDataConverter vehicleDataConverter, IFFMDynamicResolver ffmResolver)
        {
        }

        internal void UpdateEcusViaSsl2(EcuDataDto ssl2EcuData, IDatabaseProvider dbProvider)
        {
        }
#endif
        public virtual void UpdateVehicleInfosViaTechnicalCampaigns()
        {
        }

        public virtual void UpdateVehicleInfosViaServiceHistory()
        {
        }

        public virtual void DisconnectVCI()
        {
            DisconnectVCI(VecInfo.VCI);
        }

        public virtual void DisconnectVCI(VCIDevice device)
        {
            Log.Info("Logic.DisconnectVCI", "called");
        }

        public virtual void DisconnectEcuKom()
        {
            DisconnectEcuKom(ConfigSettings.getPathString("SimFileDirectory", "..\\..\\..\\Testdaten"));
        }

        internal void DisconnectEcuKom(string simPath)
        {
        }

        public virtual void SetPannenfall(bool isBreakdown)
        {
            Log.Info("Logic.SetPannenfall()", "called with parameter {0}", isBreakdown.ToString());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ExportDocument(IProgressMonitor monitor, IXepInfoObject infoObject)
        {
        }
#if false
        public IEnumerable<typeServiceHistoryDetailEntry> GetRepairHistoryDetail(string serviceHistoryOrderHeaderId)
        {
            throw new NotImplementedException();
        }

        private void SendFBMPingBackgroundWorker(object useVCI)
        {
        }

        private string ReadMachineGuid()
        {
            return string.Empty;
        }

        private string ReadComputerUuid()
        {
            return string.Empty;
        }

        private string ReadComputerManufacturer()
        {
            return string.Empty;
        }
#endif
        protected void ResetSearchCache()
        {
            Log.Info("Logic.ResetSearchCache()", "reset search cache");
        }

        public virtual bool HasEcuKom()
        {
            return EcuKom != null;
        }

        public bool SpecialCaseOfGatewayIssueDetected()
        {
            return detectedSpecialSecurityCase == SpecialSecurityCases.Sec4CnGetewayIssue;
        }

        public void UpdateVehicleInfoParallel()
        {
        }

        private void UpdateVehicleIdentLevelOnline()
        {
            if (vecInfo != null)
            {
                switch (vecInfo.VehicleIdentLevel)
                {
                    case IdentificationLevel.VINOnly:
                    case IdentificationLevel.VINBasedFeatures:
                        Log.Info("Logic.UpdateVehicleIdentLevelOnline()", "changed IdentificationLevel from {0} to VINBasedOnlineUpdated", vecInfo.VehicleIdentLevel);
                        vecInfo.VehicleIdentLevel = IdentificationLevel.VINBasedOnlineUpdated;
                        break;
                    case IdentificationLevel.VINVehicleReadout:
                        vecInfo.VehicleIdentLevel = IdentificationLevel.VINVehicleReadoutOnlineUpdated;
                        Log.Info("Logic.UpdateVehicleIdentLevelOnline()", "changed IdentificationLevel from VINVehicleReadout to VINVehicleReadoutOnlineUpdated");
                        break;
                    case IdentificationLevel.None:
                    case IdentificationLevel.BasicFeatures:
                        Log.Warning("Logic.UpdateVehicleIdentLevelOnline()", "called with no valid VIN / state IdentificationLevel.None oder IdentificationLevel.BasicFeatures");
                        break;
                    case IdentificationLevel.ReopenedOperation:
                    case IdentificationLevel.VINBasedOnlineUpdated:
                        break;
                }
            }
        }

        private bool ExistsOutletsForDealer()
        {
            if (Dealer != null)
            {
                return Dealer.HasOutlet();
            }
            return false;
        }

        private bool ExistsContractsForDealer()
        {
            if (ExistsOutletsForDealer() && Dealer.FirstOutlet.contract != null)
            {
                return Dealer.FirstOutlet.contract.Count > 0;
            }
            return false;
        }
#if false
        private IEnumerable<typeServiceHistoryDetailEntry> GetServiceHistoryOrderPositions(OrderHistoryTypeDto completeOrder)
        {
            throw new NotImplementedException();
        }

        private typeServiceHistoryDetailEntry CreateOrderDetailWithPartPositon(OrderTypePositionDto position, OrderTypePositionPartListPartDto part)
        {
            throw new NotImplementedException();
        }

        private string SendObfcmDataToToyotaBackend(string filename)
        {
            throw new NotImplementedException();
        }

        private void SendObfcmDataToVehicleShadowBackend(OBFCMData obfcmData, string produceTimeStamp)
        {
        }
#endif
        public virtual void SwitchToInfoSession()
        {
            if (vecInfo != null && vecInfo.VCI != null && vecInfo.VCI.VCIType != VCIDeviceType.INFOSESSION && vecInfo.VCI.VCIType != VCIDeviceType.UNKNOWN)
            {
                Log.Info("Logic.SwitchToInfoSession()", "trying to switch to infosession");
                DisconnectEcuKom();
                DisconnectVCI(VecInfo.VCI);
                VecInfo.VCI = new VCIDevice(VCIDeviceType.INFOSESSION, "InfoSession", "127.0.0.1");
                Log.Info("Logic.SwitchToInfoSession()", "switched to infosession");
            }
            else
            {
                Log.Warning("Logic.SwitchToInfoSession()", "switch to infosession impossible due to session conditions");
            }
        }

        public bool IsInfoObjectExecutable(InfoObject infoObj)
        {
            return true;
        }
#if false
        public bool FilterDTCRelevance(DTC dtc, ICollection<ZFSResult> zfs)
        {
            return FilterDTCRelevance(dtc, zfs, FaultFilterSettings);
        }

        public bool FilterDTCRelevance(DTC dtc, ICollection<ZFSResult> zfs, FaultFilter faultFilter)
        {
            return false;
        }
#endif
        public bool CheckVinNotXXX(VCIDeviceType deviceType, string vin)
        {
            return false;
        }

        public bool StartTherapyPlanCalculation(IProgressMonitor progressMonitor)
        {
            return false;
        }

        public void InitModuleLoader()
        {
        }

        protected bool InitProgrammingSession(IProgressMonitor progressMonitor)
        {
            return InitProgrammingSession(progressMonitor, null, avoidTlsConnection: false, avoidPsdzInitialization: false);
        }

        internal bool InitProgrammingSession(IProgressMonitor progressMonitor, string mainsreies, bool avoidTlsConnection, bool avoidPsdzInitialization)
        {
            return true;
        }

        public void StartIndustrialCustomerProgramming(IEnumerable<IProgrammingTask> programmingTasks)
        {
        }

        public virtual IEnumerable<IEcuJob> ClearErrorInfoMemory()
        {
            return Enumerable.Empty<IEcuJob>();
        }

        public virtual IEnumerable<IEcuJob> ReadErrorInfoMemory()
        {
            return Enumerable.Empty<IEcuJob>();
        }

        private bool IsVehicleBrandBMWPKW()
        {
            if (ConfigSettings.SelectedBrand == UiBrand.BMWMotorrad)
            {
                return false;
            }
            return true;
        }

        public virtual void CheckAlternativePowerComponents()
        {
            if (vecInfo.BrandName != BrandName.BMWMOTORRAD && ((!string.IsNullOrEmpty(vecInfo.Kraftstoffart) && vecInfo.Kraftstoffart.Equals("H")) || (!string.IsNullOrEmpty(vecInfo.Hybridkennzeichen) && ((vecInfo.Hybridkennzeichen.Equals("HYBR") && !vecInfo.HasSA("1CE")) || vecInfo.Hybridkennzeichen.Equals("PHEV") || vecInfo.Hybridkennzeichen.Equals("BEVE"))) || (HasHighVoltagePowerComponents() && !vecInfo.HasSA("1CE"))))
            {
                if (vecInfo.Ereihe == "E72")
                {
                    ShowDialogAndWriteFasta("#MessageAlternativeDriveE72");
                    Log.Info("Confirmation dialog shown", "E72 Vehicle contains alternative power components");
                }
                else
                {
                    ShowDialogAndWriteFasta("#MessageAlternativeDrive");
                }
            }
        }

        private void ShowDialogAndWriteFasta(string localization)
        {
            services.InteractionService.RegisterMessageAsync(FormatedData.Localize("#Note"), FormatedData.Localize(localization));
        }

        private bool HasHighVoltagePowerComponents()
        {
            if (string.IsNullOrEmpty(vecInfo.EMotor.EMOTBaureihe) || vecInfo.EMotor.EMOTBaureihe.Equals("-"))
            {
                return !string.IsNullOrEmpty(vecInfo.EMotor.EMOTBezeichnung);
            }
            return true;
        }

        public virtual void ResetPowerSafeMode()
        {
            Log.Info("Logic.ResetPowerSafeMode()", "Reset the vehicle power safe mode");
        }
#if false
        public virtual CalibrationValuesResult GetRotorOffsetValue(string serial)
        {
            throw new NotImplementedException();
        }

        public TransactionMetaData CreateMetaData(TransactionMetaData metaDataAccepted = null)
        {
            throw new NotImplementedException();
        }

        public PukVehicleData CreateVehicleData()
        {
            throw new NotImplementedException();
        }

        public ISet<PukDtc> FilterVehicleDtcsForPuk()
        {
            throw new NotImplementedException();
        }

        private ISet<DTC> FilterVehicleDtcs()
        {
            throw new NotImplementedException();
        }

        internal ICollection<XEP_PERCEIVEDSYMPTOMSEX> Fill(List<FastaPukVfc> allVfcs, HashSet<string> jobIdsWithRepairVfcs, ICollection<XEP_PERCEIVEDSYMPTOMSEX> symptomsR, PukData fromData)
        {
            throw new NotImplementedException();
        }

        public virtual ICollection<ServiceConsultingModel> ImportRelatedVfcsAndServiceOperationsAndDoAllTheStuffDoneFormerlyInThePukVfcManager()
        {
            throw new NotImplementedException();
        }
#endif
        public LayoutGroup FindLayoutGroupVehicleTest()
        {
            return LayoutGroup.F;
        }

        public virtual void StoreVfcsToPuk()
        {
            DoStoreVfcsToPuk();
        }

        private void DoStoreVfcsToPuk()
        {
        }
#if false
        private VehicleTestResult FunktionalMapperFstDat(VehicleTestResult vehicleTestResult)
        {
            throw new NotImplementedException();
        }

        private VehicleTestResult FunctionalToPhysicalMapper(VehicleTestResult vehicleTestResult)
        {
            throw new NotImplementedException();
        }

        private void AssignResultSetToMappedJob(ECUJob job, IEnumerable<IEcuResult> castEcuResult)
        {
            throw new NotImplementedException();
        }

        private void AssignResultSetToMappedJobFSTDAT(ECUJob job, IEnumerable<IEcuResult> castEcuResult, int idx)
        {
            throw new NotImplementedException();
        }

        private bool AssignEcuNameToMappedJobUsingProperResultValue(ECUJob job, IEcuResult result)
        {
            return false;
        }

        public virtual TransferStateType UploadFilesToPUK(IEnumerable<PukFile> files)
        {
            throw new NotImplementedException();
        }

        protected internal IEnumerable<FastaPukVfc> ConvertPerceivedSymptoms(IEnumerable<XEP_PERCEIVEDSYMPTOMSEX> perceivedSymptoms)
        {
            throw new NotImplementedException();
        }

        private bool CheckIfIdentIsVehicleReadoutOrOnlineAndNotBNK01XMotorbike()
        {
            return false;
        }

        private IBoolResultObject CompareSessionVinToEcuJobVin(IProgressMonitor monitor)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            return boolResultObject;
        }

        private void ProtocolIsIstaRunningOnVm()
        {
        }
#endif
        public IBoolResultObject ReleaseReservedIcom(VCIDevice selectedDevice)
        {
            IBoolResultObject boolResultObject = FreeReservedIcom(selectedDevice);
            if (!boolResultObject.Result && boolResultObject.ErrorCode.Equals("IcomReleaseFailure"))
            {
                services.InteractionService.RegisterMessage(FormatedData.Localize("#Error"), FormatedData.Localize("#ConnectionLossUnableToReleaseIcom"));
            }
            return boolResultObject;
        }

        private IBoolResultObject FreeReservedIcom(VCIDevice device)
        {
            BoolResultObject boolResultObject = new BoolResultObject
            {
                Result = true
            };
            return boolResultObject;
        }

        public IBoolResultObject CheckVinAndConnectOverConnectionManager(IProgressMonitor progressMonitor, VCIDevice vciDevice)
        {
            IBoolResultObject boolResultObject = CheckVinOverConnectionLossPopup(progressMonitor, vciDevice);
            if (!boolResultObject.Result)
            {
                DisconnectVciOverConnectionManagerIfNotIcomNetworkFailure(vciDevice, boolResultObject);
                Services.InteractionService.RegisterMessage(FormatedData.Localize("#Error"), FormatedData.Localize("#VCILoss.UnableToConnectFromServicePrograms"));
            }
            return boolResultObject;
        }

        private void DisconnectVciOverConnectionManagerIfNotIcomNetworkFailure(VCIDevice vciDevice, IBoolResultObject result)
        {
            if (result != null && (string.IsNullOrEmpty(result.ErrorCode) || !result.ErrorCode.Equals("IcomNetworkFailure")))
            {
                DisconnectVCI(vciDevice);
            }
        }

        private PsdzFa BuildFaFromVecInfo()
        {
            return new PsdzFa
            {
                Vin = vecInfo.VIN17,
                FaVersion = int.Parse(vecInfo.FA.VERSION ?? "3"),
                Entwicklungsbaureihe = diagnosticsBusinessData.GetFourCharEreihe(vecInfo.Ereihe),
                Lackcode = vecInfo.FA.LACK,
                Polstercode = vecInfo.FA.POLSTER,
                Type = vecInfo.GMType,
                Zeitkriterium = vecInfo.FA.C_DATE,
                EWords = vecInfo.FA.E_WORT,
                HOWords = vecInfo.FA.HO_WORT,
                Salapas = diagnosticsBusinessData.RemoveFirstDigitOfSalapaIfLengthIs4(VecInfo.FA.SA.ToList())
            };
        }

        protected void HandleDataProtectionSettings()
        {
        }

        private bool IsFunktionalMapperForFstDatEnabled()
        {
            using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
            {
                if (istaIcsServiceClient.IsAvailable())
                {
                    return istaIcsServiceClient.GetFeatureEnabledStatus("FillFuncJobInFstdat").IsActive;
                }
            }
            return false;
        }

        private void CheckIfZgwRepairIsNeededUsingPsdz(IProgressMonitor monitor)
        {
        }
    }
}
