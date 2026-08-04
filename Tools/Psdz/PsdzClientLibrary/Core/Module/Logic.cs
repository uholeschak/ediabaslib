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

        //private IProgrammingSessionExt programmingSession;

        //private IVehicleDataLogic vehicleDataLogic;

        //private IProgrammingSessionData programmingSessionDataContext;

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

        //public ISessionLogic SessionLogic { get; protected set; }

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
#if false
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

        public Logic(PsdzDatabase database, IFasta2Service fasta2)
        {
            Lang = new List<string>();
            Lang.Add("de-DE");
            this.database = database;
            FaultPatternImportedFromPuk = new HashSet<decimal>();
            PukCaseInfoGuid = new List<string>();
            Services = null;
            //LogicAssemblyVersionInfo logicAssemblyVersionInfo = new LogicAssemblyVersionInfo();
            //VersionInfo = new VersionInformation(logicAssemblyVersionInfo.GetInfo(), ConfigSettings.GetIstaConfigString("ProductVersion", null), ConfigSettings.GetIstaConfigString("DataVersion", null), ConfigSettings.GetIstaConfigString("MainProductVersion", null), ConfigSettings.GetIstaConfigString("SWIData", null), this.database, ConfigSettings.GetIstaConfigString("SWIVersionQueue", null));
            SetLauncherLangVersion(ConfigSettings.CurrentUICulture);
            LogVersionInfo();
            //SessionLogic = null;
            //Factory = InfoObjectFactory.Instance;
            Fasta2Service = fasta2;
            if (ConfigSettings.OperationalMode == OperationalMode.RITA)
            {
                //Fasta2Service = new Fasta2ServiceNop();
            }
            //applicationState = new MultidimensionalApplicationState(this);
            FeedbackViewHeaderTitleHelper = ServiceLocator.Current.GetService<IFeedbackViewHeaderTitleHelper>();
            if (FeedbackViewHeaderTitleHelper == null)
            {
                ServiceLocator.Current.RemoveService<IFeedbackViewHeaderTitleHelper>();
                //FeedbackViewHeaderTitleHelper = new FeedbackViewHeaderTitleHelper();
                ServiceLocator.Current.AddService(FeedbackViewHeaderTitleHelper);
            }
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
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion"))
                {
                    object value = registryKey.GetValue("UBR");
                    object value2 = registryKey.GetValue("CurrentBuild");
                    Log.Info("Logic.Logic()", "*******  OS build version:          {0,-20} OS build revision        {1,-20}", value2, value);
                }
            }
            catch (Exception arg)
            {
                Log.Warning(Log.CurrentMethod(), $"Unable to read OS version {Environment.NewLine}{arg}");
            }
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
            if (infoObject == null)
            {
                Log.Warning("IstaOperationDataContext.AddSuspiciousItem()", "Info object must not be null.");
                return;
            }
            if (module == null)
            {
                Log.Warning("IstaOperationDataContext.AddSuspiciousItem()", "Info object \"{0}\" is not added, because module is null.", infoObject.Id);
                return;
            }
            BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle = VecInfo;
            if (vehicle == null)
            {
                Log.Warning("IstaOperationDataContext.AddSuspiciousItem()", "Vehicle must not be null.", infoObject.Id);
                return;
            }
            switch (module.ExecutedFrom)
            {
                case ModuleExecutionOrigin.TestPlan:
                    {
                        XepInfoObject xepInfoObject = (XepInfoObject)infoObject.XepInfoObject;
                        if (xepInfoObject != null)
                        {
                            xepInfoObject.IsSuspicious = true;
                        }
                        AddItemCalcPriority(vehicle, module, infoObject, diagObj);
                        break;
                    }
                case ModuleExecutionOrigin.HitList:
                    operationDataContext.TreeSearchResults.GetSearchResult(InfoBrowserTreeType.NONE).Add(infoObject);
                    AddItemCalcPriority(vehicle, module, infoObject, diagObj);
                    break;
                case ModuleExecutionOrigin.HitListFunctionStructure:
                    operationDataContext.TreeSearchResults.GetSearchResult(InfoBrowserTreeType.FUNCTIONAL).Add(infoObject);
                    AddItemCalcPriority(vehicle, module, infoObject, diagObj);
                    break;
                case ModuleExecutionOrigin.HitListServicefunction:
                    operationDataContext.TreeSearchResults.GetSearchResult(InfoBrowserTreeType.SERVICEFUNCTIONS).Add(infoObject);
                    AddItemCalcPriority(vehicle, module, infoObject, diagObj);
                    break;
                case ModuleExecutionOrigin.TherapyPlan:
                    ProgrammingSession.TherapyPlan.AddSuspiciousInfoObject(infoObject);
                    break;
                default:
                    Log.Warning("IstaOperationDataContext.AddSuspiciousItem()", "Info object \"{0}\" is not added, because module was executed from \"{1}\".", infoObject.Id, module.ExecutedFrom);
                    break;
            }
        }

        private void AddItemCalcPriority(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, IModule m, InfoObject infoObject, XEP_DIAGNOSISOBJECTSEX diagObj)
        {
            vehicle.Testplan.AddItem(infoObject, diagObj, new List<string>(), new List<double>(), FFMResolver);
            MainPriorityCalculator mainPriorityCalculator = new MainPriorityCalculator(vehicle.Testplan);
            mainPriorityCalculator.AssignPriorityWeightsToDiagObj(diagObj, m.InfoObj.ParentDiagnosisObject);
            mainPriorityCalculator.Calculate();
        }

        public virtual void AddToHitList(InfoObject infoObject, InfoBrowserTreeType browserTreeType)
        {
            throw new NotImplementedException();
        }

        public virtual void AddFaultPattern(XEP_PERCEIVEDSYMPTOMSEX add)
        {
            if (add != null)
            {
                VecInfo.PerceivedSymptoms.AddIfNotContains(add);
            }
        }

        public virtual void RemoveFaultPattern(XEP_PERCEIVEDSYMPTOMSEX perceivedSymptom)
        {
            if (perceivedSymptom != null)
            {
                VecInfo.PerceivedSymptoms.Remove(perceivedSymptom);
            }
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
            foreach (ECU item in VecInfo.ECU)
            {
                //VehicleIdent.SetECUColor(item, vecInfo.VehicleIdentLevel);
            }
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
            string empty = string.Empty;
            BoolResultObject boolResultObject = new BoolResultObject();
            StartOperation(idLev);
            BoolResultObject boolResultObject2 = HandleVCI(ref device, continueVecInfo);
            boolResultObject.ErrorMessage = empty;
            Fasta2Service?.UpdateSessionHeader(IstaCaseId);
            if (boolResultObject2.ErrorCodeInt == 0)
            {
                vecIdent = new VehicleIdent(vecInfo, FFMResolver, ecuKom, VehicleDataLogic, Fasta2Service, Lang, Services, BackendCallWatchDog);
                boolResultObject.Result = vecIdent.PrepareNVIInfosession(monitor, DateTime.Now);
            }
            else if (boolResultObject2.ErrorCodeInt == -2)
            {
                boolResultObject.Result = false;
                boolResultObject.ErrorCode = ConnectToVehicleErrorCodes.IcomNetworkFailure.ToString();
                boolResultObject.ErrorCodeInt = -2;
                Log.Warning(Log.CurrentMethod(), "failed to identify vehicle : " + boolResultObject.ErrorCode);
            }
            return boolResultObject;
        }
#endif
        public IBoolResultObject PerformNugetIdent(IProgressMonitor monitor)
        {
            ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OnlineMode", defaultValue: true);
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
            bool flag = false;
            CheckAlternativePowerComponents();
            if (!diagnosticsBusinessData.CheckForSpecificModelPopUpForElectricalChecks(VecInfo.Ereihe))
            {
                return;
            }
            flag = IsVehicleConnectionOnlineUpdated();
            if (VecInfo.VehicleIdentLevel == IdentificationLevel.ReopenedOperation || VecInfo.VCI.VCIType == VCIDeviceType.INFOSESSION)
            {
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleIdentification.InformUserAbout48VCheckIssueNoVehicle", defaultValue: false) && !vecInfo.FA.SA.Any())
                {
                    if (flag)
                    {
                        services.InteractionService.RegisterMessage(FormatedData.Localize("#Warning"), FormatedData.Localize("#EmptyVehicleOrderMessageNoVehicleOnline"));
                    }
                    else
                    {
                        services.InteractionService.RegisterMessage(FormatedData.Localize("#Warning"), FormatedData.Localize("#EmptyVehicleOrderMessageNoVehicleOffline"));
                    }
                }
            }
            else if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleIdentification.InformUserAbout48VCheckIssue", defaultValue: false) && !vecInfo.FA.SA.Any())
            {
                if (flag)
                {
                    services.InteractionService.RegisterMessage(FormatedData.Localize("#Warning"), FormatedData.Localize("#EmptyVehicleOrderMessageOnline"));
                }
                else
                {
                    services.InteractionService.RegisterMessage(FormatedData.Localize("#Warning"), FormatedData.Localize("#EmptyVehicleOrderMessageOffline"));
                }
            }
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
            try
            {
                Log.Info("CreatePukServiceClient()", "Creating PUK Service Client");
                return new IstaPukServiceClient();
            }
            catch (Exception ex)
            {
                Log.Error("CreatePukServiceClient()", ex.Message);
                return null;
            }
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
            Log.Info("Logic.ClearErrorMemory()", "started.");
            try
            {
                bool flag = vecIdent.CheckVehicleIgnition();
                Log.Info("Logic.ClearErrorMemory()", "Ignition check result was: {0} Connection type is: {1}", flag, VciConnType);
                if (flag || VciConnType != EnumVCIConnectionType.ivm)
                {
                    vecInfo.UpdateStatus("#ClearErrorMemory", StateType.running, null);
                    TestModuleManager testModuleManager = new TestModuleManager((IIstaOperationLogic)this, delegate (TestModuleManagerConfiguration configuration)
                    {
                        configuration.Lang = Lang;
                        configuration.ModuleName = TestModuleName.ClampSwitch;
                        configuration.ParameterConfigurator = delegate (ModuleParameter parameters)
                        {
                            parameters.setParameter(ModuleParameter.ParameterName.IN_konfig, "KLwechsel");
                            parameters.setParameter(ModuleParameter.ParameterName.IN_pause, 15000);
                            parameters.setParameter(ModuleParameter.ParameterName.IN_automode, true);
                            parameters.setParameter(ModuleParameter.ParameterName.IN_automaticRun, true);
                        };
                    });
                    TestModuleManager testModuleManagerCheckPwf = new TestModuleManager((IIstaOperationLogic)this, delegate (TestModuleManagerConfiguration configuration)
                    {
                        configuration.Lang = Lang;
                        configuration.ModuleName = TestModuleName.CheckPwfState;
                        configuration.ParameterConfigurator = delegate
                        {
                        };
                    });
                    TestModuleManager testModuleManagerSwitchPwf = new TestModuleManager((IIstaOperationLogic)this, delegate (TestModuleManagerConfiguration configuration)
                    {
                        configuration.Lang = Lang;
                        configuration.ModuleName = TestModuleName.SwitchPwfState;
                        configuration.ParameterConfigurator = delegate
                        {
                        };
                    });
                    vecIdent.ClearAndReadErrorInfoMemory(services, () => testModuleManager.Execute(), () => testModuleManagerCheckPwf.Execute(), () => testModuleManagerSwitchPwf.Execute());
                    vecInfo.UpdateStatus("#ClearErrorMemory", StateType.finished, null);
                    SessionInfoAccessor.SessionInfo.LastChangeDate = DateTime.Now;
                }
                else
                {
                    Log.Warning("Logic.ClearErrorMemory()", "Precondition not fitting; no ClearErrorMemory startable.");
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Logic.ClearErrorMemory()", exception);
                vecInfo.UpdateStatus("#ClearErrorMemory", StateType.error, null);
                SessionInfoAccessor.SessionInfo.LastChangeDate = DateTime.Now;
                return;
            }
            Log.Info("Logic.ClearErrorMemory()", "ended.");
            VecInfo.CalculateFaultProperties(FFMResolver);
            ImportantLoggingItem.AddItemToList("DTC Entries after Clear Error Memory: " + SessionInfoAccessor.SessionInfo.FaultCodeSum, TYPES.DTC_ENTRIES);
            vecIdent.SaeContextBuilder();
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
            if (Fasta2Service == null)
            {
                Log.Error("Logic.WriteFastaAfterVehicleTest()", "Failed to write FASTA2, because service is null.");
                return;
            }
            if (vehicleTestResult == null)
            {
                Log.Error("Logic.WriteFastaAfterVehicleTest()", "Failed to write FASTA2, because vehicleTestResult is null.");
                return;
            }
            Fasta2Service.UpdateSessionHeader(VecInfo);
            Fasta2Service.UpdateSessionHeader(vehicleTestResult);
            if (ReadBooleanRegistryKey("BMW.Rheingold.FASTA.UseMapperForFunctionalJobs", defaultValue: true))
            {
                vehicleTestResult = ((!IsFunktionalMapperForFstDatEnabled()) ? FunctionalToPhysicalMapper(vehicleTestResult) : FunktionalMapperFstDat(vehicleTestResult));
            }
            IEnumerable<IEcuObj> psdzEcus = ((ProgrammingSessionDataContext != null && ProgrammingSession != null && ProgrammingSessionDataContext.IsValid && ProgrammingSession.SvtCurrent != null) ? ProgrammingSession.SvtCurrent.Ecus : null);
            IAction<IObjectCalculation> action = Fasta2Service.CreateAndAddObjectCalculation(ObjectCalculationObjectType.VehicleIdentificationComplete, LayoutGroup.F);
            action.StartTime = vehicleTestResult.TimeStarted;
            action.EndTime = DateTime.Now;
            action.FillCurrentContext(vehicleTestResult.TimeStarted, VecInfo, vehicleTestResult.JobList, psdzEcus, FFMResolver, null);
            if (ProgrammingSessionDataContext != null && ProgrammingSession != null && ProgrammingSessionDataContext.IsValid)
            {
                ProgrammingSession.TherapyPlan.CurrentContextId = action.SpecialAction.ObjectId;
                ProgrammingSession.FastaCurrentContext = action;
            }
            ProgrammingSession.HandleSecureFeatureData();
            if (vehicleTestResult.SendFstdat)
            {
                SaveAndSendFstdat(vehicleTestResult);
            }
            else
            {
                Fasta2Service.AddVehicleTest(vehicleTestResult.TimeStarted, vehicleTestResult.TimeFinished, KindOfVehicleTestType.gesamt, VecInfo, filterRelevantOnly: true, LayoutGroup.F);
            }
        }

        private VehicleTestResult DoVehicleTest(IProgressMonitor monitor, bool jumpToMeasurePlanAfterVehicleTest)
        {
            Log.Info("Logic.DoVehicleTest", "Method entered");
            try
            {
                if (VehicleCommunicationInterfaceIsPTT())
                {
                    VecInfo.VCI.PwfState = PassThruD.GetPwfState(EcuKom);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Logic.DoVehicleTest()", exception);
            }
            DateTime now = DateTime.Now;
            DateTime? dateTime = null;
            bool flag = false;
            bool firstVehicleTest = !SessionInfoAccessor.SessionInfo.IsVehicleTestDone;
            IList<IEcuJob> jobList = new List<IEcuJob>();
            try
            {
                fastaRunningBackground = false;
                IDictionary<IEcu, IEnumerable<IEcuJob>> missingEcuToEcuJob = new Dictionary<IEcu, IEnumerable<IEcuJob>>();
                DoVehicleTest(monitor, (!firstVehicleTest) ? VehicleTestMode.ShortTest : VehicleTestMode.FullTest);
                CheckConstructionDateInfo();
                bool flag2 = false;
                if (vecInfo.IsProgrammingSupported(considerLogisticBase: true) && SessionInfoAccessor.SessionInfo.IsProgrammingSessionStartable && !ProgrammingSessionDataContext.IsValid)
                {
                    flag2 = true;
                    List<long> ecuListAfterDiagnose = vecInfo.ECU.Select((ECU x) => x.ID_SG_ADR).ToList();
                    if (vecInfo.BNType == BNType.BN2020 && !VecInfo.Classification.IsNCar)
                    {
                        InquireHsfzGateway();
                    }
                    FormatedData taskDescription = new FormatedData("#ReadVehicleContext");
                    monitor.TaskDescription = taskDescription;
                    monitor.EndTime = -1L;
                    try
                    {
                        if (!IndustrialCustomer.IsProgrammingWithOldGui)
                        {
                            StartTherapyPlanCalculation(monitor);
                            BMW.Rheingold.CoreFramework.LicenseManager.ChangeProgrammingStatus();
                            Stopwatch stopwatch = Stopwatch.StartNew();
                            WaitWhileFunctionEqualRunning(monitor);
                            stopwatch.Stop();
                            Log.Info("Logic.DoVehicleTest()", "Time for reading vehicle context: {0}s.", stopwatch.ElapsedMilliseconds / 1000);
                        }
                    }
                    catch (AppException ex)
                    {
                        Log.Error("Logic.DoVehicleTest()", "Therapyplan calculation start failed because of AppException [MessageId: {0} - Message: {1}]", ex.MessageId, ex.Message);
                        services?.InteractionService.RegisterMessage(ex.TitleLocalized, ex.MessageLocalized);
                        BMW.Rheingold.CoreFramework.LicenseManager.ChangeProgrammingStatus();
                    }
                    catch (Exception ex2)
                    {
                        Log.Error("Logic.DoVehicleTest()", "Therapyplan calculation start failed because of: {0}", ex2);
                        BMW.Rheingold.CoreFramework.LicenseManager.ChangeProgrammingStatus();
                    }
                    ReadFSFromAdditionalECUsKey(monitor, ecuListAfterDiagnose);
                }
                else
                {
                    Log.Warning("Logic.DoVehicleTest", "Therapyplan calculation and Current context determination was skipped, because Programming is not enabled!");
                }
                if (Services.NavigationService.CurrentTab != TabName.VehicleInformation_ControlUnitTree)
                {
                    Services.NavigationService.NavigateTo(TabName.VehicleInformation_ControlUnitTree);
                }
                diagnosticsBusinessData.CheckEcusFor14DigitSerialNumber(ecuKom, vecInfo.ECU);
                bool sendFst = false;
                VecInfo.CalculateFaultProperties(FFMResolver);
                TimeMetricsUtility.Instance.FastaStart();
                if (firstVehicleTest && FastaDataReadConditionComplied(ReadBooleanRegistryKey("BMW.Rheingold.FastaData.ReadCondition.Enabled")))
                {
                    calculateTestplan?.Invoke();
                    sendFst = true;
                    if (ConfigSettings.IsVehicleTestReadFastaDataActive() && ReadBooleanRegistryKey("BMW.Rheingold.RheingoldSessionController.Logic.EnableSendingSpeedlinkData", defaultValue: true) && (ConfigSettings.IsISTAModeHO || ConfigSettings.OperationalMode == OperationalMode.ISTA_PLUS || ConfigSettings.IsOssModeActive))
                    {
                        SendSpeedlinkDataInBackground();
                    }
                    if (FastaDataShouldBeReadInBackground(jumpToMeasurePlanAfterVehicleTest))
                    {
                        jobList = ReadFastaFromVehicleInBackGround(monitor);
                    }
                    else
                    {
                        jobList = ReadFastaFromVehicle(monitor);
                    }
                }
                TimeMetricsUtility.Instance.FastaEnd();
                monitor.TaskDescription = new FormatedData("#VehicleTest", true, "#VehicleTestFinished");
                fastaRunningBackground = false;
                if (flag2)
                {
                    new ProgrammingErrorHandler(this).Handle();
                }
                flag = monitor.IsRunningInBackground;
                if (flag)
                {
                    VecInfo.UpdateStatus("#VehicleTestFinishing", StateType.running, null);
                }
                diagnosticsBusinessData.Add14DigitFakeSerialNumberToFstdat(vecInfo, jobList);
                dateTime = DateTime.Now;
                return new VehicleTestResult(now, dateTime.Value, jobList, missingEcuToEcuJob, IstaCaseId, sendFst);
            }
            finally
            {
                PopUpBackendProblems.ShowBackendProblemPopUp(WebCallUtility.CheckForInternetConnection(), VecInfo, Services.InteractionService, isConnectedVehicle: true);
                DateTime? dateTime2 = dateTime;
                if (!dateTime2.HasValue)
                {
                    _ = DateTime.Now;
                }
                else
                {
                    dateTime2.GetValueOrDefault();
                }
                SessionInfoAccessor.SessionInfo.IsVehicleTestDone = true;
                if (flag)
                {
                    VecInfo.UpdateStatus("#VehicleTestFinishing", StateType.finished, null);
                }
                List<BackendServiceType> notAvailableBackends = BackendCallWatchDog.NotAvailableBackends;
                if (notAvailableBackends.Count > 0)
                {
                    string text = string.Join(", ", notAvailableBackends.Select((BackendServiceType r) => r.ToString()));
                    if (!IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))
                    {
                        Fasta2Service.AddServiceCode("SUC03_BackendNAError_nu_LF", text + ", n/a", LayoutGroup.D);
                    }
                }
                TimeMetricsUtility.Instance.Stop();
                TimeSpan vehicleTestDuration = TimeMetricsUtility.Instance.GetVehicleIdentTestDuration(TimeMetricsStage.VehicleTest);
                TimeSpan vehicleIdentDuration = TimeMetricsUtility.Instance.GetVehicleIdentTestDuration(TimeMetricsStage.VehicleIdent);
                TimeSpan fullIdentDuration = vehicleIdentDuration + vehicleTestDuration;
                Log.Info("Logic.DoVehicleTest()", "Vehicle Test Time Elapsed: {0}", vehicleTestDuration.ToString());
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OperationsReport.Long.Enabled") && ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OperationsReport.Enable.ANA.Servicecodes"))
                {
                    Fasta2Service.AddServiceCode(ServiceCodes.ANA02_TimespanVehicleTest_t_LF, "TimeSpan: '" + vehicleTestDuration.ToString() + "' Baureihe: '" + vecInfo.Baureihe + "'", LayoutGroup.D);
                }
                TimeMetricsUtility.Instance.Stop();
                EslTask?.ContinueWith(delegate
                {
                    try
                    {
                        if (ConfigSettings.SelectedBrand != UiBrand.BMWMotorrad && firstVehicleTest && !IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))
                        {
                            int num = VecInfo.ECU.SelectMany((ECU y) => y.FEHLER).Count((DTC dtc) => !dtc.IsVirtual);
                            TimeSpan value = ((num > 0) ? TimeSpan.FromTicks(ImportantLoggingItem.DurationFSLesen.Ticks / num) : TimeSpan.Zero);
                            Fasta2Service.AddServiceCode(ServiceCodes.SYS01_VehTestDuration_nu_LF, FormatServiceCodeDetails(jobList, vehicleTestDuration, value, vehicleIdentDuration, fullIdentDuration), LayoutGroup.D);
                        }
                    }
                    catch (Exception exception2)
                    {
                        Log.ErrorException(Log.CurrentMethod(), exception2);
                    }
                });
            }
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
            Log.Info(Log.CurrentMethod(), "Method entered");
            try
            {
                if (VehicleCommunicationInterfaceIsPTT())
                {
                    VecInfo.VCI.PwfState = PassThruD.GetPwfState(EcuKom);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
            DateTime now = DateTime.Now;
            DateTime? dateTime = null;
            bool flag = false;
            try
            {
                IList<IEcuJob> jobList = new List<IEcuJob>();
                fastaRunningBackground = false;
                IDictionary<IEcu, IEnumerable<IEcuJob>> missingEcuToEcuJob = new Dictionary<IEcu, IEnumerable<IEcuJob>>();
                bool flag2 = !SessionInfoAccessor.SessionInfo.IsVehicleTestDone;
                SessionInfoAccessor.SessionInfo.IsProgrammingSessionStartable = true;
                CheckConstructionDateInfo();
                bool flag3 = true;
                if (vecInfo.IsProgrammingSupported(considerLogisticBase: true) && SessionInfoAccessor.SessionInfo.IsProgrammingSessionStartable && !ProgrammingSessionDataContext.IsValid)
                {
                    List<long> ecuListAfterDiagnose = vecInfo.ECU.Select((ECU x) => x.ID_SG_ADR).ToList();
                    if (vecInfo.BNType == BNType.BN2020 && !VecInfo.Classification.IsNCar)
                    {
                        InquireHsfzGateway();
                    }
                    FormatedData taskDescription = new FormatedData("#ReadVehicleContext");
                    monitor.TaskDescription = taskDescription;
                    monitor.EndTime = -1L;
                    try
                    {
                        if (!IndustrialCustomer.IsProgrammingWithOldGui)
                        {
                            StartTherapyPlanCalculation(monitor);
                            BMW.Rheingold.CoreFramework.LicenseManager.ChangeProgrammingStatus();
                            Stopwatch stopwatch = Stopwatch.StartNew();
                            WaitWhileFunctionEqualRunning(monitor);
                            stopwatch.Stop();
                            Log.Info("Logic.DoVehicleTest()", "Time for reading vehicle context: {0}s.", stopwatch.ElapsedMilliseconds / 1000);
                        }
                    }
                    catch (AppException ex)
                    {
                        Log.Error("Logic.DoVehicleTest()", "Therapyplan calculation start failed because of AppException [MessageId: {0} - Message: {1}]", ex.MessageId, ex.Message);
                        services?.InteractionService.RegisterMessage(ex.TitleLocalized, ex.MessageLocalized);
                        BMW.Rheingold.CoreFramework.LicenseManager.ChangeProgrammingStatus();
                    }
                    catch (Exception ex2)
                    {
                        Log.Error("Logic.DoVehicleTest()", "Therapyplan calculation start failed because of: {0}", ex2);
                        BMW.Rheingold.CoreFramework.LicenseManager.ChangeProgrammingStatus();
                    }
                    ReadFSFromAdditionalECUsKey(monitor, ecuListAfterDiagnose);
                }
                else
                {
                    Log.Warning("Logic.DoVehicleTest", "Therapyplan calculation and Current context determination was skipped, because Programming is not enabled!");
                }
                if (Services.NavigationService.CurrentTab != TabName.VehicleInformation_ControlUnitTree)
                {
                    Services.NavigationService.NavigateTo(TabName.VehicleInformation_ControlUnitTree);
                }
                diagnosticsBusinessData.CheckEcusFor14DigitSerialNumber(ecuKom, vecInfo.ECU);
                bool sendFst = false;
                VecInfo.CalculateFaultProperties(FFMResolver);
                if (flag2 && FastaDataReadConditionComplied(ReadBooleanRegistryKey("BMW.Rheingold.FastaData.ReadCondition.Enabled")))
                {
                    sendFst = true;
                    if (ConfigSettings.IsVehicleTestReadFastaDataActive() && ReadBooleanRegistryKey("BMW.Rheingold.RheingoldSessionController.Logic.EnableSendingSpeedlinkData", defaultValue: true) && (ConfigSettings.IsISTAModeHO || ConfigSettings.OperationalMode == OperationalMode.ISTA_PLUS || ConfigSettings.IsOssModeActive))
                    {
                        SendSpeedlinkDataInBackground();
                    }
                    jobList = ((!FastaDataShouldBeReadInBackground(jumpToMeasurePlanAfterVehicleTest)) ? ReadFastaFromVehicle(monitor) : ReadFastaFromVehicleInBackGround(monitor));
                }
                monitor.TaskDescription = new FormatedData("#VehicleTest", true, "#VehicleTestFinished");
                fastaRunningBackground = false;
                if (flag3)
                {
                    new ProgrammingErrorHandler(this).Handle();
                }
                flag = monitor.IsRunningInBackground;
                if (flag)
                {
                    VecInfo.UpdateStatus("#VehicleTestFinishing", StateType.running, null);
                }
                diagnosticsBusinessData.Add14DigitFakeSerialNumberToFstdat(vecInfo, jobList);
                dateTime = DateTime.Now;
                return new VehicleTestResult(now, dateTime.Value, jobList, missingEcuToEcuJob, IstaCaseId, sendFst);
            }
            finally
            {
                PopUpBackendProblems.ShowBackendProblemPopUp(WebCallUtility.CheckForInternetConnection(), VecInfo, Services.InteractionService, isConnectedVehicle: true);
                DateTime obj = dateTime ?? DateTime.Now;
                SessionInfoAccessor.SessionInfo.IsVehicleTestDone = true;
                if (flag)
                {
                    VecInfo.UpdateStatus("#VehicleTestFinishing", StateType.finished, null);
                }
                TimeSpan timeSpan = obj - now;
                Log.Info("Logic.DoVehicleTest()", "Vehicle Test Time Elapsed: {0}", timeSpan.ToString());
                if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OperationsReport.Long.Enabled") && ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OperationsReport.Enable.ANA.Servicecodes"))
                {
                    Fasta2Service.AddServiceCode(ServiceCodes.ANA02_TimespanVehicleTest_t_LF, "TimeSpan: '" + timeSpan.ToString() + "' Baureihe: '" + vecInfo.Baureihe + "'", LayoutGroup.D);
                }
            }
        }
#endif
        public void SendSpeedlinkDataInBackground()
        {
        }

        private void ReadFSFromAdditionalECUsKey(IProgressMonitor monitor, List<long> ecuListAfterDiagnose)
        {
            if (ReadBooleanRegistryKey("BMW.Rheingold.VehicleIdent.ReadFsFromAdditionalEcus", defaultValue: true))
            {
                List<ECU> list = vecInfo.ECU.Where((ECU x) => !ecuListAfterDiagnose.Contains(x.ID_SG_ADR)).ToList();
                if (list.Any())
                {
                    Log.Info("Logic.DoVehicleTest()", $"{list.Count} additional ecu(s) found. Start reading error memory.");
                    //vecIdent.ReadErrorMemoryForAdditionalEcus(list, monitor, 3);
                }
            }
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
            if (ReadBooleanRegistryKey("BMW.Rheingold.FastaData.Background.Enabled", defaultValue: true))
            {
                return !jumpToMeasurePlanAfterVehicleTest;
            }
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
            return new ProgrammingNote
            {
                Hidden = string.Empty,
                IsVisible = true,
                Message = new TextContent(new FormatedData("#Programming.NoteBeforeVehicleTest", null), Lang).GetTextForUI(Lang)[0].TextItem
            };
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
            TransactionMetaData transactionMetaData = null;
            if (metaDataList != null && vin17 != null)
            {
                foreach (TransactionMetaData metaData in metaDataList)
                {
                    if (vin17.Equals(metaData.VIN17) && metaData.IsVehicleCommunicationDone && (transactionMetaData == null || transactionMetaData.StartDate < metaData.StartDate))
                    {
                        transactionMetaData = metaData;
                    }
                }
            }
            return transactionMetaData;
        }
#endif
        private void WaitWhileFunctionEqualRunning(IProgressMonitor monitor)
        {
            Log.Info("Logic.WaitWhileFunctionEqualRunning()", "Enter method with Status_FunctionState \"{0}\", .", VecInfo.Status_FunctionState);
        }

        public virtual void UpdateVehicle()
        {
        }
#if false
        public virtual void DoVehicleTest(IProgressMonitor monitor, VehicleTestMode testMode)
        {
            Stopwatch stopwatch = new Stopwatch();
            Log.Info("Logic.DoVehicleTest()", $"called with testmode {testMode}");
            if (vecIdent == null)
            {
                throw new NotSupportedException("Please call \"StartVehicleTest()\" first to initialize vecIdent.");
            }
            if ((vecIdent != null && vecIdent.CheckVehicleIgnition()) || VciConnType != EnumVCIConnectionType.ivm || IndustrialCustomer.WithoutVehicle)
            {
                if ((vecInfo.VCI != null && vecInfo.VCI.VCIType == VCIDeviceType.INFOSESSION) || vecInfo.Classification.IsPreDS2Vehicle())
                {
                    return;
                }
                string name = "#VehicleTest";
                bool flag = true;
                if (vecIdent.EcuKom == null)
                {
                    vecIdent.EcuKom = ecuKom;
                }
                try
                {
                    switch (testMode)
                    {
                        case VehicleTestMode.KeyReaderDataTest:
                            name = "#KeyReaderDataTest";
                            vecInfo.UpdateStatus(name, StateType.running, null);
                            flag = vecIdent.DoVehicleTestKeyData(monitor);
                            break;
                        case VehicleTestMode.ShortTest:
                            name = "#VehicleShortTest";
                            vecInfo.UpdateStatus(name, StateType.running, null);
                            stopwatch.Start();
                            ImportantLoggingItem.AddItemToList("Short Vehicle test started at: " + DateTime.Now.ToString(), TYPES.VEHICLE_TEST);
                            flag = vecIdent.DoVehicleShortTest(monitor);
                            stopwatch.Stop();
                            ImportantLoggingItem.AddItemToList("Short Vehicle test done at: " + DateTime.Now.ToString() + ". Time elapsed: " + stopwatch.Elapsed.ToString(), TYPES.VEHICLE_TEST);
                            stopwatch.Reset();
                            EvaluateOverallTestModules(monitor);
                            break;
                        default:
                            name = "#VehicleTest";
                            vecInfo.UpdateStatus(name, StateType.running, null);
                            stopwatch.Start();
                            ImportantLoggingItem.AddItemToList("Regular Vehicle test started at: " + DateTime.Now.ToString(), TYPES.VEHICLE_TEST);
                            flag = vecIdent.DoVehicleTest(monitor);
                            stopwatch.Stop();
                            ImportantLoggingItem.AddItemToList("Regular Vehicle test done at: " + DateTime.Now.ToString() + ". Time elapsed: " + stopwatch.Elapsed.ToString(), TYPES.VEHICLE_TEST);
                            ImportantLoggingItem.DurationFullTest = stopwatch.Elapsed;
                            stopwatch.Reset();
                            EvaluateOverallTestModules(monitor);
                            break;
                    }
                    FillVehicleEcusFromDatabase();
                    SessionInfoAccessor.SessionInfo.IsProgrammingSessionStartable = true;
                }
                catch (Exception exception)
                {
                    Log.WarningException("Logic.doVehicleTest()", exception);
                    vecInfo.UpdateStatus(name, StateType.error, null);
                }
                if (flag)
                {
                    vecInfo.UpdateStatus(name, StateType.finished, null);
                }
                else
                {
                    vecInfo.UpdateStatus(name, StateType.error, null);
                }
                SessionInfoAccessor.SessionInfo.LastChangeDate = DateTime.Now;
            }
            else
            {
                Log.Warning("Logic.doVehicleTest()", "can not start vehicle test  {0}", testMode.ToString());
            }
            Log.Info("Logic.doVehicleTest()", "ended.");
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
            PDIData pDIData = new PDIData();
            pDIData.Data.Vehicle.Status.PreDeliveryInspection.Mileage = km;
            pDIData.Data.Vehicle.Status.PreDeliveryInspection.Date = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            return IstaEdgeUtility.CreatePDIRequest(new Dictionary<string, string>(), vin17, IstaCaseId, Dealer.DealerData.DistributionPartnerNumber + "/" + Dealer.DealerData.OutletNumber, DateTime.UtcNow.ToString(PDIRequest.DateTimeFormat), pDIData);
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
            string fullPath = Path.GetFullPath(filePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found", fullPath);
            }
            LauncherCommunicationClient obj = LauncherCommunication.GetClient(ClientType.ISTAASSISTANT) ?? throw new Exception("Could not get LauncherCommunicationClient for ISTA Assistant.");
            if (!Guid.TryParse(IstaCaseId, out var result))
            {
                throw new Exception("IstaCaseId " + IstaCaseId + " is not a valid GUID.");
            }
            if (!obj.SendFastaData(type: GetMessageTypeForAssistant(fullPath), istaCaseId: result, fileName: fileName, filePath: fullPath).GetAwaiter().GetResult())
            {
                return typeTransferState.Failed;
            }
            return typeTransferState.SuccessfullyDone;
        }

        private LauncherMessageType GetMessageTypeForAssistant(string path)
        {
            string extension = Path.GetExtension(path);
            if (!(extension == ".behdat"))
            {
                if (extension == ".fstdat")
                {
                    return LauncherMessageType.Fstdat;
                }
                throw new Exception("Unsupported file type " + extension + " for sending to ISTA Assistant.");
            }
            return LauncherMessageType.Behdat;
        }

        private typeTransferState HandleOssMode(string filename, string fileNoPath)
        {
            if (!FBMUploadFactory.Create(BackendCallWatchDog).SendDataToBackend(fileNoPath, filename))
            {
                return typeTransferState.Failed;
            }
            return typeTransferState.SuccessfullyDone;
        }

        private void UpdateTransferState(string fileNoPath, typeTransferState transferState)
        {
            if (metaData.FASTA != null)
            {
                metaData.FASTA.TransferState = transferState;
            }
            if (metaData.TransferList == null)
            {
                metaData.TransferList = new ObservableCollection<TransferType>();
            }
            TransferType transferType = metaData.TransferList.FirstOrDefault((TransferType x) => x.Name.Equals(fileNoPath));
            TransferStateType state = metaData.ConvertTransferState(transferState);
            if (transferType != null)
            {
                transferType.State = state;
                Log.Info("Logic.SendFastaDataToFBM()", "File State: {0}", state.ToString());
                return;
            }
            metaData.TransferList.Add(new TransferType
            {
                Name = fileNoPath,
                LastTransferDate = DateTime.Now,
                State = state,
                TypedTarget = MetaFileTarget.BMW
            });
            Log.Info("Logic.SendFastaDataToFBM()", "FastaFile not yet in TransferList");
        }

        public virtual void WriteMetaData()
        {
            Log.Info("Logic.WriteMetaData()", "called");
            try
            {
                if (metaData == null)
                {
                    Log.Error("Logic.WriteMetaData()", "Meta data is null, creating new one for saving.");
                    metaData = new TransactionMetaData();
                }
                metaData.IstaCaseId = IstaCaseId;
                metaData.distributionPartnerNumber = Dealer.DealerData?.DistributionPartnerNumber;
                metaData.DealerNumber = SessionInfo.BuNo;
                metaData.ComputerName = Environment.MachineName;
                metaData.UserName = Environment.UserName;
                if (ExistsOutletsForDealer())
                {
                    metaData.outletNumber = Dealer.FirstOutlet?.outletNumber;
                }
                metaData.StartDate = OperationStartTime;
                metaData.VIN17 = vecInfo.VIN17;
                metaData.VIN17_OEM = vecInfo.VIN17_OEM;
                metaData.BasicFeatures = new typeBasicFeatures();
                metaData.BasicFeatures.Baureihe = vecInfo.Baureihe;
                metaData.BasicFeatures.Ereihe = vecInfo.Ereihe;
                metaData.BasicFeatures.Getriebe = vecInfo.Getriebe;
                metaData.BasicFeatures.CountryOfAssembly = vecInfo.CountryOfAssembly;
                metaData.BasicFeatures.BaseVersion = vecInfo.BaseVersion;
                metaData.BasicFeatures.Karosserie = vecInfo.Karosserie;
                metaData.BasicFeatures.Land = vecInfo.Land;
                metaData.BasicFeatures.Lenkung = vecInfo.Lenkung;
                metaData.BasicFeatures.Modelljahr = vecInfo.Modelljahr;
                metaData.BasicFeatures.Modellmonat = vecInfo.Modellmonat;
                metaData.BasicFeatures.Motor = vecInfo.GenericMotor?.Engine1;
                metaData.BasicFeatures.EMotBaureihe = vecInfo.GenericMotor?.Engine2;
                metaData.BasicFeatures.VerkaufsBezeichnung = vecInfo.SalesDesignationBadgeUIText;
                metaData.BasicFeatures.Marke = vecInfo.Marke;
                metaData.BasicFeatures.Prodart = vecInfo.Prodart;
                metaData.BasicFeatures.TypeCode = vecInfo.Typ;
                string text = Path.Combine(ConfigIAPHelper.GetMetaFilePath(), metaData.GetMetaFilename());
                SetTransferstateToDoneIfFastaAndTransactionEnabledForSimulation();
                TransactionMetaData transactionMetaData = CreatePukServiceClient().LoadPukVehicleCasesWithId(metaData.IstaCaseId);
                foreach (TransferType transfer in metaData.TransferList)
                {
                    if (transactionMetaData?.TransferList == null)
                    {
                        continue;
                    }
                    foreach (TransferType transfer2 in transactionMetaData.TransferList)
                    {
                        if (transfer.Name.Equals(transfer2.Name))
                        {
                            transfer.State = transfer2.State;
                        }
                    }
                }
                if (SafeXmlWriter.SafeWrite(text, metaData, Encoding.UTF8))
                {
                    string fileName = Path.GetFileName(text);
                    if (ReadBooleanRegistryKey("TransactionDataFileUploadEnabled"))
                    {
                        string destFileName = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", ConfigSettings.getPathString("TransactionDataFileUploadPath", "\\\\nvgm013.muc\\fastadata\\new\\lipuma3"), fileName);
                        try
                        {
                            File.Copy(text, destFileName);
                        }
                        catch (Exception exception)
                        {
                            Log.WarningException("Logic.WriteMetaData()", exception);
                        }
                    }
                }
                else
                {
                    Log.Warning("Logic.WriteMetaData()", "failed when writing meta data");
                }
            }
            catch (Exception exception2)
            {
                Log.WarningException("Logic.WriteMetadata()", exception2);
            }
            Log.Info("Logic.WriteMetadata()", "done");
        }

        private void SetTransferstateToDone(typeTransferUnit unit)
        {
            if (unit != null)
            {
                unit.TransferState = typeTransferState.SuccessfullyDone;
            }
        }

        private void SetTransferstateToDoneIfFastaAndTransactionEnabledForSimulation()
        {
            if (IsFastaAndTransactionEnabledForSimulation)
            {
                SetTransferstateToDone(metaData.ECUKom);
                SetTransferstateToDone(metaData.FASTA);
                if (MetaData.TransferList == null)
                {
                    MetaData.TransferList = new ObservableCollection<TransferType>();
                }
                TransferType transferType = metaData.TransferList.FirstOrDefault((TransferType x) => x.TypedTarget == MetaFileTarget.BMW);
                if (transferType != null)
                {
                    transferType.State = TransferStateType.Successful;
                }
                else
                {
                    Log.Warning("SetTransferstateToDoneIfFastaAnd...", "Fasta file not in TransferList");
                }
                SetTransferstateToDone(metaData.MIBKom);
                SetTransferstateToDone(metaData.Transaction);
            }
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
            if (continuedOperation == null || continuedOperation.ProgrammingData?.Uri == null)
            {
                return;
            }
            string text = Path.Combine(ConfigIAPHelper.GetTransactionPath(), continuedOperation.ProgrammingData.Uri);
            if (!File.Exists(text))
            {
                using (IIstaPukService istaPukService = CreatePukServiceClient())
                {
                    if (istaPukService.IsAvailable())
                    {
                        istaPukService.DownloadPrivateDataFiles(continuedOperation.IstaCaseId, ConfigIAPHelper.GetTransactionPath(), "RG_PRG");
                    }
                }
            }
            if (File.Exists(text))
            {
                SerializedProgrammingSessionData = File.ReadAllText(text);
                Log.Info(Log.CurrentMethod(), "Read programming data \"{0}\".", text);
            }
            else
            {
                Log.Error(Log.CurrentMethod(), "Failed to read programming data \"{0}\", file does not exist.", text);
            }
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
            if (!SessionInfoAccessor.SessionInfo.SimulatedParts || IsFastaAndTransactionEnabledForSimulation)
            {
                string text = Fasta2Service.SaveFstdat(KindOfVehicleTestType.gesamt, vehicleTest.TimeStarted, vehicleTest.TimeFinished, VecInfo, vehicleTest.JobList, vehicleTest.AdditionalEcuToEcuJob, filterRelevantOnly: true, vehicleTest.IstaCaseId);
                if (!string.IsNullOrEmpty(text) && File.Exists(text))
                {
                    SendFastaDataToFBM(text, forceSend: false);
                    return;
                }
                Log.Error("Logic.SaveAndSendFstdat()", "No FASTA fstdat could be transfered. File \"{0}\" does not exist or Fasta2Migration is notYet.", text);
            }
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
            string method = Log.CurrentMethod();
            if (device == null)
            {
                Log.Warning(method, "device was null");
                boolResultObject.ErrorMessage = "device was null";
                return boolResultObject;
            }
            Log.Info(method, $"device.VCIType: {device.VCIType}");
            if (IsKnownConnectionType(device.VCIType))
            {
                boolResultObject = HandleVCI(ref device, continueVecInfo: true);
                if (boolResultObject.ErrorCodeInt == 0)
                {
                    //boolResultObject = CompareSessionVinToEcuJobVin(monitor) as BoolResultObject;
                }
                else if (boolResultObject.ErrorCodeInt == -2)
                {
                    boolResultObject.Result = false;
                    boolResultObject.ErrorCode = "IcomNetworkFailure";
                    Log.Warning(method, $"Failed to identify vehicle {ConnectionLossError.IcomNetworkFailure}: {boolResultObject.ErrorCode}");
                }
            }
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
            bool flag = SpecialCaseOfGatewayIssueDetected();
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
            string text = vci.ToString();
            if (meta.VehicleCommunication == null)
            {
                meta.VehicleCommunication = text;
            }
            else if (!meta.VehicleCommunication.Contains(text))
            {
                meta.VehicleCommunication = meta.VehicleCommunication + "-" + text;
            }
        }

        public void SetVehicleCommunication(TransactionMetaData meta, VCIDeviceType vci, bool enableSim)
        {
            switch (vci)
            {
                case VCIDeviceType.ENET:
                case VCIDeviceType.ICOM:
                case VCIDeviceType.EDIABAS:
                case VCIDeviceType.PTT:
                    SetVehicleCommunication(meta, vci);
                    break;
                case VCIDeviceType.SIM:
                    if (enableSim)
                    {
                        SetVehicleCommunication(meta, vci);
                    }
                    break;
                default:
                    throw new Exception(string.Format(CultureInfo.InvariantCulture, "Unsupported VCIDeviceType \"{0}\".", vci));
                case VCIDeviceType.IMIB:
                case VCIDeviceType.INFOSESSION:
                case VCIDeviceType.UNKNOWN:
                    break;
            }
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
            foreach (EcuDto ssl2Ecu in ssl2Ecus)
            {
                try
                {
                    Log.Info(Log.CurrentMethod(), "Found ecu variant: " + ssl2Ecu.SgbdVar + " via SCORE (SSL2) update at diag address: \"" + ssl2Ecu.EcuAddressDecimal + "\" (dec.).");
                    ECU eCU = new ECU();
                    if (ssl2Ecu.EcuAddressDecimal != null)
                    {
                        eCU.ID_SG_ADR = Convert.ToInt64(ssl2Ecu.EcuAddressDecimal, CultureInfo.InvariantCulture);
                    }
                    else if (!string.IsNullOrEmpty(ssl2Ecu.SgbdVar) && VecInfo.Classification.IsPreE65Vehicle())
                    {
                        XEP_ECUGROUPS ecuGroupByEcuVariantName = dbProvider.GetEcuGroupByEcuVariantName(ssl2Ecu.SgbdVar);
                        if (ecuGroupByEcuVariantName == null)
                        {
                            Log.Info(Log.CurrentMethod(), "No ecuGroup for variante: \"" + ssl2Ecu.SgbdVar + "\"");
                        }
                        else if (ecuGroupByEcuVariantName.DiagnosticAddress != -1m)
                        {
                            eCU.ID_SG_ADR = Convert.ToInt64(ecuGroupByEcuVariantName.DiagnosticAddress, CultureInfo.InvariantCulture);
                            Log.Info(Log.CurrentMethod(), $"Diagnostic address of group: \"{ecuGroupByEcuVariantName.Name}\" in database is: \"{eCU.ID_SG_ADR}\" (dec.).");
                        }
                        else
                        {
                            XEP_ECUGROUPS ecuGroupByName = database.GetEcuGroupByName(ecuGroupByEcuVariantName.Name);
                            if (ecuGroupByName != null && ecuGroupByName.DiagnosticAddress != -1m)
                            {
                                eCU.ID_SG_ADR = Convert.ToInt64((int)ecuGroupByName.DiagnosticAddress, CultureInfo.InvariantCulture);
                                Log.Info(Log.CurrentMethod(), $"Diagnostic address of group: \"{ecuGroupByEcuVariantName.Name}\" in characteristics is: \"{eCU.ID_SG_ADR}\" (dec.).");
                            }
                        }
                    }
                    eCU.SERIENNUMMER = ssl2Ecu.SerialNumber;
                    eCU.VARIANTE = ssl2Ecu.SgbdVar;
                    eCU.ECU_SGBD = ssl2Ecu.SgbdVar;
                    eCU.ECU_ADR = FormatConverterBase.Dec2Hex(eCU.ID_SG_ADR);
                    eCU.IDENT_SUCCESSFULLY = false;
                    if (string.IsNullOrEmpty(eCU.ECU_GRUPPE) || (eCU.ECU_GRUPPE.Contains("|") && !string.IsNullOrEmpty(eCU.VARIANTE)))
                    {
                        XEP_ECUGROUPS ecuGroupByEcuVariantName2 = database.GetEcuGroupByEcuVariantName(eCU.VARIANTE);
                        if (ecuGroupByEcuVariantName2 != null && !string.IsNullOrEmpty(ecuGroupByEcuVariantName2.Name))
                        {
                            eCU.ECU_GRUPPE = ecuGroupByEcuVariantName2.Name;
                            Log.Info(Log.CurrentMethod(), "Set ECU group " + eCU.ECU_GRUPPE + " for ECU " + eCU.ECU_NAME + " determined by ecu variant " + eCU.VARIANTE + ".");
                        }
                    }
                    vdc.FillEcuNames(eCU, VecInfo, FFMResolver);
                    VehicleIdent.SetECUColor(eCU, VecInfo.VehicleIdentLevel);
                    VecInfo.AddOrUpdateECU(eCU);
                }
                catch (Exception exception)
                {
                    Log.WarningException(Log.CurrentMethod(), exception);
                }
            }
            foreach (ECU item in VecInfo.ECU)
            {
                item.ECU_GROBNAME = VehicleLogistics.getECU_GROBNAME(VecInfo, item.ID_SG_ADR);
                item.BUS = (BMW.Rheingold.CoreFramework.DatabaseProvider.BusType)VehicleLogistics.getECUBus(VecInfo, item.ID_SG_ADR, item.ECU_GRUPPE);
            }
        }

        private void UpdateEcusViaSsl2ForVehicleReadout(List<EcuDto> ssl2Ecus, IDatabaseProvider dbProvider)
        {
            foreach (EcuDto ssl2Ecu in ssl2Ecus)
            {
                if (ssl2Ecu == null)
                {
                    continue;
                }
                Log.Info(Log.CurrentMethod(), "found SCORE CVS data from diagnosis readout: " + ssl2Ecu.DiagnosticsDate.ToString(CultureInfo.CurrentUICulture));
                try
                {
                    ECU eCU = new ECU();
                    SetEcuPropertiesBasedOnSsl2DataForVehicleReadout(eCU, ssl2Ecu);
                    ECU eCU2 = CheckForAlreadyIdentifiedEcus(ssl2Ecu, eCU) ?? GetEcuByEcuGroupForPreE65Vehicle(VecInfo, ssl2Ecu, dbProvider);
                    if (eCU2 != null && string.IsNullOrEmpty(eCU2.VARIANTE))
                    {
                        SetMissingVariantInformationForContextEcu(eCU2, eCU, VecInfo, vdc, FFMResolver);
                        VehicleIdent.SetECUColor(eCU2, VecInfo.VehicleIdentLevel);
                    }
                    if (!SessionInfoAccessor.SessionInfo.IsEcuIdentSuccessfull && VecInfo.BNType != BNType.IBUS)
                    {
                        ECU eCU3 = new ECU();
                        SetEcuPropertiesBasedOnSsl2DataForVehicleReadout(eCU3, ssl2Ecu);
                        SetMissingVariantInformationForContextEcu(eCU3, eCU, VecInfo, vdc, FFMResolver);
                        VehicleIdent.SetECUColor(eCU3, VecInfo.VehicleIdentLevel);
                        VecInfo.AddOrUpdateECU(eCU3);
                        eCU3.ECU_GRUPPE = dbProvider.GetEcuGroupByEcuVariantName(eCU3.VARIANTE)?.Name.ToUpper();
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException(Log.CurrentMethod(), exception);
                }
            }
        }

        private ECU CheckForAlreadyIdentifiedEcus(EcuDto sslEcu, IEcu ecu)
        {
            if (!string.IsNullOrEmpty(sslEcu.EcuAddressDecimal))
            {
                return VecInfo.getECU(ecu.ID_SG_ADR);
            }
            return null;
        }

        private ECU GetEcuByEcuGroupForPreE65Vehicle(BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, EcuDto sslEcu, IDatabaseProvider dbProvider)
        {
            ECU result = null;
            if (vehicle != null && vehicle.Classification.IsPreE65Vehicle())
            {
                XEP_ECUGROUPS ecuGroupByEcuVariantName = dbProvider.GetEcuGroupByEcuVariantName(sslEcu.SgbdVar);
                if (ecuGroupByEcuVariantName != null)
                {
                    string name = ecuGroupByEcuVariantName.Name;
                    result = vehicle.getECUbyECU_GRUPPE(name);
                }
            }
            return result;
        }

        private void SetEcuPropertiesBasedOnSsl2DataForVehicleReadout(ECU ecu, EcuDto sslEcu)
        {
            if (sslEcu.EcuAddressDecimal != null)
            {
                ecu.ID_SG_ADR = Convert.ToInt64(sslEcu.EcuAddressDecimal, CultureInfo.InvariantCulture);
            }
            ecu.SERIENNUMMER = sslEcu.SerialNumber;
            ecu.VARIANTE = sslEcu.SgbdVar;
            ecu.ECU_SGBD = sslEcu.SgbdVar;
            ecu.ECU_ADR = FormatConverterBase.Dec2Hex(ecu.ID_SG_ADR);
            ecu.ECU_GRUPPE = database.GetEcuGroupByEcuVariantName(sslEcu.SgbdVar)?.Name ?? string.Empty;
        }

        private void SetMissingVariantInformationForContextEcu(ECU contextEcu, ECU ecuBasedOnCvs, BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle, VehicleDataConverter vehicleDataConverter, IFFMDynamicResolver ffmResolver)
        {
            Log.Info(Log.CurrentMethod(), $"found ecu variant: \"{ecuBasedOnCvs.VARIANTE}\" via CVS update at diag address: \"{ecuBasedOnCvs.ID_SG_ADR}\" or at ecu group: \"{ecuBasedOnCvs.ECU_GRUPPE}\"");
            contextEcu.VARIANTE = ecuBasedOnCvs.VARIANTE;
            contextEcu.ECU_SGBD = ecuBasedOnCvs.VARIANTE;
            contextEcu.IDENT_SUCCESSFULLY = false;
            vehicleDataConverter.FillEcuNames(contextEcu, vehicle, ffmResolver);
            if (!string.IsNullOrEmpty(ecuBasedOnCvs.TITLE_ECUTREE))
            {
                contextEcu.TITLE_ECUTREE = contextEcu.TITLE_ECUTREE;
            }
        }

        internal void UpdateEcusViaSsl2(EcuDataDto ssl2EcuData, IDatabaseProvider dbProvider)
        {
            List<EcuDto> list = ssl2EcuData?.EcusWithStatusInUse?.OrderByDescending((EcuDto x) => x.DiagnosticsDate)?.ToList();
            if (list == null || !list.Any())
            {
                Log.Info(Log.CurrentMethod(), "No ecus were retrieved from SSL2.");
                return;
            }
            if (vecInfo.Prodart == "M")
            {
                Log.Info(Log.CurrentMethod(), "No ecus are used from ServiceStateLayer2 for motorcycles.");
                return;
            }
            if (VecInfo.ECU == null)
            {
                VecInfo.ECU = new ObservableCollection<ECU>();
            }
            if (CheckIfIdentIsVehicleReadoutOrOnlineAndNotBNK01XMotorbike())
            {
                UpdateEcusViaSsl2ForVehicleReadout(list, dbProvider);
            }
            if (ssl2EcuData != null && ssl2EcuData.EcusWithStatusNotInUse?.Any() == true && !IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA") && ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OperationsReport.Long.Enabled") && ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OperationsReport.Enable.ANA.Servicecodes"))
            {
                string baureihenverbund = VecInfo.Baureihenverbund;
                string ereihe = VecInfo.Ereihe;
                Fasta2Service.AddServiceCode(ServiceCodes.ANA11_FdlGateEcuStateAorO_nu_LF, "BRV: " + baureihenverbund + ", E-Reihe: " + ereihe, LayoutGroup.D, allowMultipleEntries: true);
            }
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
            OrderHistoryTypeDto completeOrder = new ServiceHistoryProcessorService(new ServiceHistoryProcessorImpl(BackendCallWatchDog)).RequestHistoryDetail(vecInfo, serviceHistoryOrderHeaderId, ConfigSettings.CurrentUICulture);
            return GetServiceHistoryOrderPositions(completeOrder);
        }

        private void SendFBMPingBackgroundWorker(object useVCI)
        {
            string text = useVCI as string;
            string applicationID = string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3};{4};{5}", VersionInfo.FastaSystemVersion, Environment.MachineName, text, ReadMachineGuid(), ReadComputerUuid(), ReadComputerManufacturer());
            if (VecInfo != null && !string.IsNullOrEmpty(VecInfo.VIN17) && VecInfo.VIN17.Length == 17 && !VecInfo.VIN17.Contains("XXXX") && ExistsContractsForDealer())
            {
                FbmPingData pingData = Fasta2Service.PingData;
                if (!pingData.IsEmpty)
                {
                    new BrokerMonitoringProcessorImpl(BackendCallWatchDog).requestBrokerMonitoringEntry(VecInfo.VIN17, applicationID, "ISTA.Next", pingData.FastaSessionStartTime, pingData.DistributionPartnerNumber, pingData.OutletNumber, pingData.DealerNumber, IsSendFastaDataForbidden);
                }
                else
                {
                    Log.Warning("Logic.SendFBMPingBackgroundWorker()", "conditions incorrect, no FBM ping send: pingData == null.");
                }
            }
            else
            {
                Log.Warning("Logic.SendFBMPingBackgroundWorker()", "conditions incorrect, no FBM ping send");
            }
        }

        private string ReadMachineGuid()
        {
            try
            {
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography"))
                {
                    if (registryKey != null)
                    {
                        object value = registryKey.GetValue("MachineGuid");
                        if (value != null)
                        {
                            return value.ToString();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Logic.ReadMachineGuid()", exception);
            }
            return string.Empty;
        }

        private string ReadComputerUuid()
        {
            try
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct"))
                {
                    using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                    {
                        foreach (ManagementBaseObject item in managementObjectCollection)
                        {
                            object obj = item["UUID"];
                            if (obj != null)
                            {
                                return obj.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Logic.ReadComputerUuid()", exception);
            }
            return string.Empty;
        }

        private string ReadComputerManufacturer()
        {
            try
            {
                using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                    {
                        foreach (ManagementBaseObject item in managementObjectCollection)
                        {
                            object obj = item["Manufacturer"];
                            if (obj != null)
                            {
                                return obj.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Logic.ReadComputerManufacturer()", exception);
            }
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
            List<typeServiceHistoryDetailEntry> list = new List<typeServiceHistoryDetailEntry>();
            if (completeOrder == null)
            {
                throw new AppException(new FormatedData("#030", null));
            }
            if (completeOrder.order != null)
            {
                if (completeOrder.order.partList != null && completeOrder.order.partList.part != null)
                {
                    OrderTypePositionPartListPartDto[] part = completeOrder.order.partList.part;
                    foreach (OrderTypePositionPartListPartDto part2 in part)
                    {
                        list.Add(CreateOrderDetailWithPartPositon(new OrderTypePositionDto(), part2));
                    }
                }
                if (completeOrder.order.positionList != null)
                {
                    OrderTypePositionDto[] positionList = completeOrder.order.positionList;
                    foreach (OrderTypePositionDto orderTypePositionDto in positionList)
                    {
                        if (orderTypePositionDto.partList != null && orderTypePositionDto.partList.part != null)
                        {
                            OrderTypePositionPartListPartDto[] part = orderTypePositionDto.partList.part;
                            foreach (OrderTypePositionPartListPartDto part3 in part)
                            {
                                list.Add(CreateOrderDetailWithPartPositon(orderTypePositionDto, part3));
                            }
                        }
                        if (orderTypePositionDto.flatRateUnitList != null && orderTypePositionDto.flatRateUnitList.flatRateUnit != null)
                        {
                            int num = 0;
                            OrderTypePositionFlatRateUnitListFlatRateUnitDto[] flatRateUnit = orderTypePositionDto.flatRateUnitList.flatRateUnit;
                            foreach (OrderTypePositionFlatRateUnitListFlatRateUnitDto orderTypePositionFlatRateUnitListFlatRateUnitDto in flatRateUnit)
                            {
                                typeServiceHistoryDetailEntry typeServiceHistoryDetailEntry = new typeServiceHistoryDetailEntry();
                                typeServiceHistoryDetailEntry.PositionNumber = orderTypePositionDto.number;
                                typeServiceHistoryDetailEntry.FlatRateText = orderTypePositionFlatRateUnitListFlatRateUnitDto.description.Value;
                                typeServiceHistoryDetailEntry.SetFlatRate(orderTypePositionFlatRateUnitListFlatRateUnitDto.isLocal, orderTypePositionFlatRateUnitListFlatRateUnitDto.number);
                                typeServiceHistoryDetailEntry.FlatRateValue = orderTypePositionDto.flatRateUnitList.quantity[num];
                                num++;
                                list.Add(typeServiceHistoryDetailEntry);
                            }
                        }
                        if (orderTypePositionDto.packageList == null)
                        {
                            continue;
                        }
                        OrderTypePositionPackageDto[] packageList = orderTypePositionDto.packageList;
                        foreach (OrderTypePositionPackageDto orderTypePositionPackageDto in packageList)
                        {
                            if (orderTypePositionPackageDto.partList != null && orderTypePositionPackageDto.partList.part != null)
                            {
                                OrderTypePositionPartListPartDto[] part = orderTypePositionPackageDto.partList.part;
                                foreach (OrderTypePositionPartListPartDto orderTypePositionPartListPartDto in part)
                                {
                                    typeServiceHistoryDetailEntry typeServiceHistoryDetailEntry2 = new typeServiceHistoryDetailEntry();
                                    typeServiceHistoryDetailEntry2.PositionNumber = orderTypePositionPackageDto.number;
                                    typeServiceHistoryDetailEntry2.PartText = orderTypePositionPartListPartDto.description.Value;
                                    typeServiceHistoryDetailEntry2.SetPart(orderTypePositionPartListPartDto.isLocal, orderTypePositionPartListPartDto.number);
                                    list.Add(typeServiceHistoryDetailEntry2);
                                }
                            }
                            if (orderTypePositionPackageDto.flatRateUnitList != null && orderTypePositionPackageDto.flatRateUnitList.flatRateUnit != null)
                            {
                                int num2 = 0;
                                OrderTypePositionFlatRateUnitListFlatRateUnitDto[] flatRateUnit = orderTypePositionPackageDto.flatRateUnitList.flatRateUnit;
                                foreach (OrderTypePositionFlatRateUnitListFlatRateUnitDto orderTypePositionFlatRateUnitListFlatRateUnitDto2 in flatRateUnit)
                                {
                                    typeServiceHistoryDetailEntry typeServiceHistoryDetailEntry3 = new typeServiceHistoryDetailEntry();
                                    typeServiceHistoryDetailEntry3.PositionNumber = orderTypePositionPackageDto.number;
                                    typeServiceHistoryDetailEntry3.FlatRateText = orderTypePositionFlatRateUnitListFlatRateUnitDto2.description.Value;
                                    typeServiceHistoryDetailEntry3.SetFlatRate(orderTypePositionFlatRateUnitListFlatRateUnitDto2.isLocal, orderTypePositionFlatRateUnitListFlatRateUnitDto2.number);
                                    typeServiceHistoryDetailEntry3.FlatRateValue = orderTypePositionPackageDto.flatRateUnitList.quantity[num2];
                                    num2++;
                                    list.Add(typeServiceHistoryDetailEntry3);
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        private typeServiceHistoryDetailEntry CreateOrderDetailWithPartPositon(OrderTypePositionDto position, OrderTypePositionPartListPartDto part)
        {
            typeServiceHistoryDetailEntry typeServiceHistoryDetailEntry = new typeServiceHistoryDetailEntry();
            if (position != null)
            {
                typeServiceHistoryDetailEntry.PositionNumber = position.number;
            }
            typeServiceHistoryDetailEntry.PartText = part.description.Value;
            typeServiceHistoryDetailEntry.SetPart(part.isLocal, part.number);
            return typeServiceHistoryDetailEntry;
        }

        private string SendObfcmDataToToyotaBackend(string filename)
        {
            Log.Info("Logic.SendObfcmDataToToyotaBackend()", "called OBFCM File \"{0}\"", filename);
            try
            {
                if (!File.Exists(filename))
                {
                    Log.Error("Login.SendObfcmDataToToyotaBackend()", "OBFCM file \"{0}\" does not exist.", filename);
                }
                else
                {
                    string fileName = Path.GetFileName(filename);
                    metaData.VIN17 = VecInfo.VIN17;
                    if (!IsSendFastaDataForbiddenBitsQueueFull)
                    {
                        using (IstaFbmServiceClient istaFbmServiceClient = new IstaFbmServiceClient())
                        {
                            typeTransferState transferState = istaFbmServiceClient.SendObfcmData(filename, fileName, metaData.VIN17, metaData.StartDate);
                            if (metaData.OBFCM != null)
                            {
                                metaData.OBFCM.TransferState = transferState;
                            }
                        }
                    }
                    else
                    {
                        Log.Info("Logic.SendObfcmDataToToyotaBackend()", "OBFCM data will not be sent or added to the TransacationList because the Send OBFCM Data is forbidden for this Vehicle");
                    }
                }
                return filename;
            }
            catch (Exception exception)
            {
                Log.WarningException("Logic.SendObfcmDataToToyotaBackend()", exception);
            }
            return null;
        }

        private void SendObfcmDataToVehicleShadowBackend(OBFCMData obfcmData, string produceTimeStamp)
        {
            try
            {
                Task.Run(delegate
                {
                    OBFCMRequest oBFCMRequest = IstaEdgeUtility.CreateOBFCMRequest(obfcmData, VecInfo.VIN17, IstaCaseId, Dealer.DealerData.DistributionPartnerNumber + "/" + Dealer.DealerData.OutletNumber, produceTimeStamp);
                    if (oBFCMRequest == null)
                    {
                        Log.Error(Log.CurrentMethod(), "OBFCMRequest model could not be send to the Vehicle Shadow Backend.");
                    }
                    else
                    {
                        Log.Info(Log.CurrentMethod(), "Sending OBFCMData to the VehicleShadow backend.");
                        HttpStatusCode httpStatusCode = EDGEProcessorFactory.Create(BackendCallWatchDog).SendDataToBackend(VecInfo.VIN17, oBFCMRequest, BackendServiceType.EDGEObfcm);
                        switch (httpStatusCode)
                        {
                            case HttpStatusCode.Created:
                                Fasta2Service.AddServiceCode(ServiceCodes.FCM_80_TransmissionStatus_nu_CV, "success", LayoutGroup.R);
                                break;
                            case HttpStatusCode.BadRequest:
                                Fasta2Service.AddServiceCode(ServiceCodes.FCM_80_TransmissionStatus_nu_CV, "fail", LayoutGroup.R);
                                break;
                            case HttpStatusCode.InternalServerError:
                                Fasta2Service.AddServiceCode(ServiceCodes.FCM_80_TransmissionStatus_nu_CV, "unknown", LayoutGroup.R);
                                break;
                            case HttpStatusCode.Forbidden:
                                Fasta2Service.AddServiceCode(ServiceCodes.FCM_80_TransmissionStatus_nu_CV, "forbidden", LayoutGroup.R);
                                break;
                            case HttpStatusCode.ServiceUnavailable:
                                Fasta2Service.AddServiceCode(ServiceCodes.FCM_80_TransmissionStatus_nu_CV, "service not available", LayoutGroup.R);
                                break;
                            default:
                                Fasta2Service.AddServiceCode(ServiceCodes.FCM_80_TransmissionStatus_nu_CV, httpStatusCode.ToString(), LayoutGroup.R);
                                break;
                        }
                        if (httpStatusCode > (HttpStatusCode)299)
                        {
                            Log.Warning(Log.CurrentMethod(), $"Vehicle shadow returned non-success status code: {httpStatusCode}");
                        }
                    }
                });
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
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
            try
            {
                if (dtc == null)
                {
                    return false;
                }
                bool? relevance = dtc.Relevance;
                if (relevance.HasValue)
                {
                    if (relevance != true && hideBogusFaults)
                    {
                        return false;
                    }
                }
                else if (hideUnknownFaults)
                {
                    return false;
                }
                if (dtc.Current != null)
                {
                    long num = (dtc.Current.F_UW_KM.HasValue ? dtc.Current.F_UW_KM.Value : (-1));
                    long num2 = ((dtc.First != null && dtc.First.F_UW_KM.HasValue) ? dtc.First.F_UW_KM.Value : num);
                    if (faultFilter.LowerKMBound.HasValue && faultFilter.LowerKMBound > num && faultFilter.LowerKMBound > num2)
                    {
                        return false;
                    }
                    if (faultFilter.UpperKMBound.HasValue && faultFilter.UpperKMBound < num && faultFilter.UpperKMBound < num2)
                    {
                        return false;
                    }
                }
                else if (faultFilter.UpperKMBound.HasValue || faultFilter.LowerKMBound.HasValue)
                {
                    return false;
                }
                if (faultFilter.FaultClassHidden != null && faultFilter.FaultClassHidden.Contains(FaultCodeConverters.GetFaultClass(dtc, zfs)))
                {
                    return false;
                }
                if (faultFilter.FaultGroupNumbers != null)
                {
                    Fault fault = vecInfo.FaultList.FirstOrDefault((Fault p) => p.DTC.FortAsHexString == dtc.FortAsHexString);
                    if (fault == null || (fault.FaultGroupNumber != 0 && !faultFilter.FaultGroupNumbers.Contains(fault.FaultGroupNumber)))
                    {
                        return false;
                    }
                }
                if (ConfigSettings.getConfigStringAsBoolean("EnableRelevanceFaultCode", defaultValue: true) && dtc.RelevanceFaultCode != null && dtc.F_VORHANDEN_NR.HasValue && !dtc.RelevanceFaultCode.Contains(dtc.F_VORHANDEN_NR.Value))
                {
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.ErrorException("Logic.FilterDTCRelevance()", exception);
            }
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
            return new CalibrationValueProcessorImpl(BackendCallWatchDog).RequestCalibrationValue(serial);
        }

        public TransactionMetaData CreateMetaData(TransactionMetaData metaDataAccepted = null)
        {
            OperationContinued = metaDataAccepted;
            TransactionMetaData transactionMetaData = new TransactionMetaData();
            _ = OperationStartTime;
            transactionMetaData.StartDate = OperationStartTime;
            transactionMetaData.IstaCaseId = IstaCaseId;
            if (metaDataAccepted != null)
            {
                transactionMetaData.DateOfFastaRead = metaDataAccepted.DateOfFastaRead;
                transactionMetaData.DistanceOfFastaRead = metaDataAccepted.DistanceOfFastaRead;
            }
            Log.Info("Logic.CreateMetaData()", "New meta data. IstaCaseId \"{0}\", start date \"{1}\".", transactionMetaData.IstaCaseId, transactionMetaData.StartDate);
            return transactionMetaData;
        }

        public PukVehicleData CreateVehicleData()
        {
            BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle = VecInfo;
            PukVehicleData pukVehicleData = new PukVehicleData();
            pukVehicleData.DiagCodes.AddRange(vehicle.DiagCodes.Select((typeDiagCode x) => new PukDiagnosisCode
            {
                Code = x.DiagnoseCode
            }));
            pukVehicleData.FirstRegistration = vehicle.FirstRegistration;
            pukVehicleData.Gwsz = vehicle.Gwsz;
            pukVehicleData.GwszUnit = vehicle.GwszUnit.ToString().ParseEnum<PukMileageUnit>();
            pukVehicleData.SerialGearBox7 = vehicle.SerialGearBox7;
            return pukVehicleData;
        }

        public ISet<PukDtc> FilterVehicleDtcsForPuk()
        {
            ISet<DTC> source = FilterVehicleDtcs();
            HashSet<PukDtc> hashSet = new HashSet<PukDtc>();
            hashSet.AddRange(source.Select((DTC x) => new PukDtc
            {
                Id = x.Id,
                IsCombined = x.IsCombined,
                IsDisabledByUser = x.IsDisabledByUser,
                IsRelevant = x.Relevance,
                IsVirtual = x.IsVirtual,
                No = x.F_ORT
            }));
            return hashSet;
        }

        private ISet<DTC> FilterVehicleDtcs()
        {
            ISet<DTC> set = new HashSet<DTC>();
            if (VecInfo != null)
            {
                if (VecInfo.ECU != null)
                {
                    foreach (ECU item in VecInfo.ECU)
                    {
                        if (item.FEHLER != null)
                        {
                            foreach (DTC item2 in item.FEHLER.Where((DTC x) => x.Id.HasValue && !"I".Equals(x.EcuDTCType, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (!set.Add(item2))
                                {
                                    Log.Warning("Logic.FilterVehicleDtcsForPuk()", "DTC with id {0} already exists in FEHLER.", item2.Id);
                                }
                            }
                        }
                        if (item.INFO == null)
                        {
                            continue;
                        }
                        foreach (DTC item3 in item.INFO.Where((DTC x) => x.Id.HasValue && x.Relevance == true))
                        {
                            if (!set.Add(item3))
                            {
                                Log.Warning("Logic.FilterVehicleDtcsForPuk()", "DTC with id {0} already existsin INFO.", item3.Id);
                            }
                        }
                    }
                }
                if (VecInfo.CombinedFaults != null)
                {
                    set.AddRange(VecInfo.CombinedFaults.Where((DTC x) => x.Id.HasValue));
                }
            }
            set.ForEach(delegate (DTC dtc)
            {
                dtc.IsDisabledByUser = FilterDTCRelevance(dtc, VecInfo.ZFS) && dtc.Relevance.HasValue && dtc.Relevance.Value;
            });
            return set;
        }

        internal ICollection<XEP_PERCEIVEDSYMPTOMSEX> Fill(List<FastaPukVfc> allVfcs, HashSet<string> jobIdsWithRepairVfcs, ICollection<XEP_PERCEIVEDSYMPTOMSEX> symptomsR, PukData fromData)
        {
            PukVfcLogic pukVfcLogic = new PukVfcLogic(this, database);
            List<FastaPukVfc> list = fromData.FastaPukVfc.Where((FastaPukVfc x) => x.JobId != null && x.JobId.StartsWith("R")).ToList();
            foreach (FastaPukVfc item in list)
            {
                ICollection<XEP_PERCEIVEDSYMPTOMSEX> collection = pukVfcLogic.MapPukVfcsToIstaVfcs(new List<FastaPukVfc> { item });
                if (collection != null && collection.Count > 0)
                {
                    jobIdsWithRepairVfcs.Add(item.JobId);
                    symptomsR.AddRange(collection);
                }
            }
            List<FastaPukVfc> list2 = fromData.FastaPukVfc.Where((FastaPukVfc x) => x.JobId != null && x.JobId.StartsWith("C")).ToList();
            ICollection<XEP_PERCEIVEDSYMPTOMSEX> result = pukVfcLogic.MapPukVfcsToIstaVfcs(list2);
            allVfcs.AddRange(list);
            allVfcs.AddRange(list2);
            return result;
        }

        public virtual ICollection<ServiceConsultingModel> ImportRelatedVfcsAndServiceOperationsAndDoAllTheStuffDoneFormerlyInThePukVfcManager()
        {
            using (IIstaPukService istaPukService = CreatePukServiceClient())
            {
                if (!istaPukService.IsAvailable())
                {
                    return null;
                }
                try
                {
                    PukData pukData = istaPukService.ImportRelatedVfcs(VecInfo.VIN17);
                    HashSet<string> hashSet = new HashSet<string>();
                    List<FastaPukVfc> list = new List<FastaPukVfc>();
                    ICollection<XEP_PERCEIVEDSYMPTOMSEX> collection = new List<XEP_PERCEIVEDSYMPTOMSEX>();
                    ICollection<XEP_PERCEIVEDSYMPTOMSEX> enumerable = Fill(list, hashSet, collection, pukData);
                    List<ServiceConsultingModel> result = istaPukService.ImportServiceConsulting(VecInfo.VIN17, list, hashSet);
                    collection.ForEach(delegate (XEP_PERCEIVEDSYMPTOMSEX x)
                    {
                        AddFaultPattern(x);
                        FaultPatternImportedFromPuk.AddIfNotContains(x.Id);
                    });
                    enumerable.ForEach(delegate (XEP_PERCEIVEDSYMPTOMSEX x)
                    {
                        AddFaultPattern(x);
                        FaultPatternImportedFromPuk.AddIfNotContains(x.Id);
                    });
                    PukCaseInfoGuid.Clear();
                    PukCaseInfoGuid.AddRange(pukData.PukCaseInfoGuid);
                    ICollection<FastaPukVfc> fastaPukVfc = pukData.FastaPukVfc;
                    if (Fasta2Service != null && fastaPukVfc.Any())
                    {
                        Fasta2Service.CreateAndAddObjectCalculation(ObjectCalculationObjectType.IPSImport, ProgrammingSession?.FindLayoutGroupVehicleTest() ?? LayoutGroup.F).AddFaultPatternsFromPUK(fastaPukVfc);
                    }
                    return result;
                }
                catch (Exception exception)
                {
                    Log.ErrorException("Logic.ImportRelatedVfcsAndServiceOperationsAndDoAllTheStuffDoneFormerlyInThePukVfcManager()", exception);
                }
            }
            return null;
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
            List<ECUJob> list = new List<ECUJob>();
            foreach (string fstDatFunctionalDiagJobs in FstDatConstants.FstDatFunctionalDiagJobsList)
            {
                List<ECUJob> list2 = new List<ECUJob>();
                foreach (IEcuJob job in vehicleTestResult.JobList)
                {
                    if (job.JobName.Contains(fstDatFunctionalDiagJobs))
                    {
                        for (ushort num = 0; num < job.JobResultSets; num++)
                        {
                            ECUJob eCUJob = new ECUJob();
                            eCUJob.JobName = job.JobName;
                            eCUJob.EcuName = job.EcuName;
                            eCUJob.ExecutionStartTime = job.ExecutionStartTime;
                            eCUJob.ExecutionEndTime = job.ExecutionEndTime;
                            eCUJob.FASTARelevant = job.FASTARelevant;
                            eCUJob.JobErrorCode = job.JobErrorCode;
                            eCUJob.JobErrorText = job.JobErrorText;
                            eCUJob.JobParam = job.JobParam;
                            eCUJob.JobResultFilter = job.JobResultFilter;
                            eCUJob.JobResult = new List<IEcuResult>();
                            eCUJob.JobResultSets = job.JobResultSets;
                            list2.Add(eCUJob);
                            AssignResultSetToMappedJobFSTDAT(eCUJob, job.getResultSet(num), job.JobResultSets);
                        }
                    }
                }
                if (list2 != null && list2.Any())
                {
                    list2.ForEach(delegate (ECUJob x)
                    {
                        x.EcuName = "F01";
                    });
                    ECUJob eCUJob2 = list2.FirstOrDefault();
                    ECUJob eCUJob3 = new ECUJob
                    {
                        JobName = eCUJob2.JobName,
                        EcuName = eCUJob2.EcuName,
                        ExecutionStartTime = eCUJob2.ExecutionStartTime,
                        ExecutionEndTime = eCUJob2.ExecutionEndTime,
                        FASTARelevant = eCUJob2.FASTARelevant,
                        JobErrorCode = eCUJob2.JobErrorCode,
                        JobErrorText = eCUJob2.JobErrorText,
                        JobParam = eCUJob2.JobParam,
                        JobResultFilter = eCUJob2.JobResultFilter,
                        JobResultSets = eCUJob2.JobResultSets,
                        JobResult = new List<IEcuResult>()
                    };
                    eCUJob3.JobResult = list2.SelectMany((ECUJob job) => job.JobResult).ToList();
                    list.Add(eCUJob3);
                }
            }
            List<IEcuJob> jobList = vehicleTestResult.JobList.Concat(list).ToList();
            return new VehicleTestResult(vehicleTestResult.TimeStarted, vehicleTestResult.TimeFinished, jobList, vehicleTestResult.AdditionalEcuToEcuJob, vehicleTestResult.IstaCaseId, vehicleTestResult.SendFstdat);
        }

        private VehicleTestResult FunctionalToPhysicalMapper(VehicleTestResult vehicleTestResult)
        {
            List<ECUJob> list = new List<ECUJob>();
            foreach (IEcuJob job in vehicleTestResult.JobList)
            {
                string functionalToPhysicalMapperPattern = FstDatConstants.FunctionalToPhysicalMapperPattern;
                if (job.JobName.Contains(functionalToPhysicalMapperPattern))
                {
                    for (ushort num = 1; num < job.JobResultSets; num++)
                    {
                        ECUJob eCUJob = new ECUJob();
                        eCUJob.JobName = job.JobName;
                        eCUJob.EcuName = job.EcuName;
                        eCUJob.ExecutionStartTime = job.ExecutionStartTime;
                        eCUJob.ExecutionEndTime = job.ExecutionEndTime;
                        eCUJob.FASTARelevant = job.FASTARelevant;
                        eCUJob.JobErrorCode = job.JobErrorCode;
                        eCUJob.JobErrorText = job.JobErrorText;
                        eCUJob.JobParam = job.JobParam;
                        eCUJob.JobResultFilter = job.JobResultFilter;
                        eCUJob.JobResult = new List<IEcuResult>();
                        eCUJob.JobResultSets = 0;
                        list.Add(eCUJob);
                        AssignResultSetToMappedJob(eCUJob, job.getResultSet(num));
                    }
                }
            }
            List<IEcuJob> jobList = vehicleTestResult.JobList.Concat(list).ToList();
            return new VehicleTestResult(vehicleTestResult.TimeStarted, vehicleTestResult.TimeFinished, jobList, vehicleTestResult.AdditionalEcuToEcuJob, vehicleTestResult.IstaCaseId, vehicleTestResult.SendFstdat);
        }

        private void AssignResultSetToMappedJob(ECUJob job, IEnumerable<IEcuResult> castEcuResult)
        {
            job.JobResultSets = 1;
            bool flag = false;
            foreach (IEcuResult item in castEcuResult)
            {
                ECUResult eCUResult = new ECUResult();
                eCUResult.FASTARelevant = item.FASTARelevant;
                eCUResult.Format = item.Format;
                eCUResult.Length = item.Length;
                eCUResult.LengthSpecified = item.LengthSpecified;
                eCUResult.Set = 0;
                eCUResult.SetSpecified = item.SetSpecified;
                eCUResult.Value = item.Value;
                eCUResult.Name = item.Name;
                job.JobResult.Add(eCUResult);
                if (!flag)
                {
                    flag = AssignEcuNameToMappedJobUsingProperResultValue(job, item);
                }
            }
        }

        private void AssignResultSetToMappedJobFSTDAT(ECUJob job, IEnumerable<IEcuResult> castEcuResult, int idx)
        {
            job.JobResultSets = idx;
            bool flag = false;
            foreach (IEcuResult item in castEcuResult)
            {
                ECUResult eCUResult = new ECUResult();
                eCUResult.FASTARelevant = item.FASTARelevant;
                eCUResult.Format = item.Format;
                eCUResult.Length = item.Length;
                eCUResult.LengthSpecified = item.LengthSpecified;
                eCUResult.Set = item.Set;
                eCUResult.SetSpecified = item.SetSpecified;
                eCUResult.Value = item.Value;
                eCUResult.Name = item.Name;
                job.JobResult.Add(eCUResult);
                if (!flag)
                {
                    flag = AssignEcuNameToMappedJobUsingProperResultValue(job, item);
                }
            }
        }

        private bool AssignEcuNameToMappedJobUsingProperResultValue(ECUJob job, IEcuResult result)
        {
            if (result.Value == null)
            {
                Log.Warning("AssignEcuNameToMappedJobUsingProperResultValue", "Job result {0} value was null!", result.Name);
                return false;
            }
            if (result.Name == "ECU_SGBD")
            {
                job.EcuName = result.Value.ToString();
                return true;
            }
            if (result.Name == "ECU_GROBNAME")
            {
                ECU eCU = VecInfo.ECU.FirstOrDefault((ECU obj) => obj.ECU_GROBNAME?.Contains(result.Value.ToString()) ?? false);
                if (eCU != null)
                {
                    job.EcuName = eCU.ECU_SGBD?.ToString();
                }
                else
                {
                    job.EcuName = result.Value.ToString();
                }
                return true;
            }
            return false;
        }

        public virtual TransferStateType UploadFilesToPUK(IEnumerable<PukFile> files)
        {
            throw new NotImplementedException();
        }

        protected internal IEnumerable<FastaPukVfc> ConvertPerceivedSymptoms(IEnumerable<XEP_PERCEIVEDSYMPTOMSEX> perceivedSymptoms)
        {
            BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle vehicle = VecInfo;
            IFFMDynamicResolver fFMResolver = FFMResolver;
            IDatabaseProvider databaseProvider = database;
            List<FastaPukVfc> list = new List<FastaPukVfc>();
            if (perceivedSymptoms == null)
            {
                return list;
            }
            foreach (XEP_PERCEIVEDSYMPTOMSEX perceivedSymptom in perceivedSymptoms)
            {
                FastaPukVfc fastaPukVfc = new FastaPukVfc();
                switch (perceivedSymptom.VfcType)
                {
                    case "FO_F":
                        fastaPukVfc.FaultLocationFunctional = perceivedSymptom.VfcNr;
                        list.Add(fastaPukVfc);
                        break;
                    case "FO_K":
                        fastaPukVfc.FaultLocationComponent = perceivedSymptom.VfcNr;
                        list.Add(fastaPukVfc);
                        break;
                    case "FA":
                        fastaPukVfc.FaultType = perceivedSymptom.VfcNr;
                        if (perceivedSymptom.ParentId.HasValue)
                        {
                            XEP_PERCEIVEDSYMPTOMSEX perceivedSymptomById2 = databaseProvider.GetPerceivedSymptomById(perceivedSymptom.ParentId.Value, vehicle, fFMResolver);
                            if ("FO_K".Equals(perceivedSymptomById2.VfcType))
                            {
                                fastaPukVfc.FaultLocationComponent = perceivedSymptomById2.VfcNr;
                            }
                            else if ("FO_F".Equals(perceivedSymptomById2.VfcType))
                            {
                                fastaPukVfc.FaultLocationFunctional = perceivedSymptomById2.VfcNr;
                            }
                        }
                        list.Add(fastaPukVfc);
                        break;
                    case "FL_F":
                        fastaPukVfc.FaultPositionVehicle = perceivedSymptom.VfcNr;
                        if (perceivedSymptom.ParentId.HasValue)
                        {
                            XEP_PERCEIVEDSYMPTOMSEX perceivedSymptomById = databaseProvider.GetPerceivedSymptomById(perceivedSymptom.ParentId.Value, vehicle, fFMResolver);
                            if ("FA".Equals(perceivedSymptomById.VfcType))
                            {
                                fastaPukVfc.FaultType = perceivedSymptomById.VfcNr;
                            }
                        }
                        list.Add(fastaPukVfc);
                        break;
                    case "FL_B":
                        fastaPukVfc.FaultPositionComponentList.Add(perceivedSymptom.VfcNr);
                        list.Add(fastaPukVfc);
                        break;
                    case "FB":
                        fastaPukVfc.FaultConditionList.Add(perceivedSymptom.VfcNr);
                        list.Add(fastaPukVfc);
                        break;
                    default:
                        Log.Warning("PukVfcManager.Convert()", "For vfc-type '{0}' is not recognised.", perceivedSymptom.VfcType);
                        break;
                }
            }
            return list;
        }

        private bool CheckIfIdentIsVehicleReadoutOrOnlineAndNotBNK01XMotorbike()
        {
            if (VecInfo.VehicleIdentLevel == IdentificationLevel.VINVehicleReadout || VecInfo.VehicleIdentLevel == IdentificationLevel.VINVehicleReadoutOnlineUpdated)
            {
                return VecInfo.BNType != BNType.BNK01X_MOTORBIKE;
            }
            return false;
        }

        private IBoolResultObject CompareSessionVinToEcuJobVin(IProgressMonitor monitor)
        {
            BoolResultObject boolResultObject = new BoolResultObject();
            string vin = new VehicleIdent(vecInfo, FFMResolver, EcuKom, VehicleDataLogic, Fasta2Service, Lang, Services, BackendCallWatchDog).GetVinFromVehicleWithoutICOMAndUserInput(monitor);
            if (!string.IsNullOrEmpty(vin))
            {
                if (vin.Length == 7)
                {
                    new SVMDProcessorImpl(BackendCallWatchDog).ResolveVIN7ToVIN17(vin, ref vin, Services);
                }
                if (vin.Equals(vecInfo.VIN17))
                {
                    boolResultObject.Result = true;
                }
                else
                {
                    boolResultObject.ErrorCode = "VehicleVinNotMatch";
                }
            }
            return boolResultObject;
        }

        private void ProtocolIsIstaRunningOnVm()
        {
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
                {
                    try
                    {
                        foreach (ManagementBaseObject item in managementObjectCollection)
                        {
                            string text = item["Manufacturer"].ToString().ToLower();
                            if (!string.IsNullOrEmpty(text) && ((text == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL")) || text.Contains("vmware") || item["Model"].ToString().ToUpperInvariant().Contains("VirtualBox")))
                            {
                                Log.Info(Log.CurrentMethod(), "Ista is Running on a VirtualMachine DistributionPartnerNumber:" + Dealer.DealerData.DistributionPartnerNumber + ",OutletNumber:" + Dealer.DealerData.OutletNumber + ",Device:" + Environment.MachineName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(Log.CurrentMethod(), "Ista cannot establish if is running on a virtual machine ex :{0}", ex.ToString());
                    }
                }
            }
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
            Log.Info(Log.CurrentMethod(), "Start early gateway repair check using Psdz for {0}", vecInfo.Ereihe);
        }
    }
}
