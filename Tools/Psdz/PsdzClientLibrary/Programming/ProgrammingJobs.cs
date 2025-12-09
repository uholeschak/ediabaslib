using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;
using BmwFileReader;
using EdiabasLib;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using PsdzClient.Core;
using PsdzClient.Psdz;
using PsdzClientLibrary.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "Custom code")]
    public class ProgrammingJobs : IDisposable
    {
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public const string RegKeyIsta = @"SOFTWARE\BMWGroup\ISPI\ISTA";
        public const string RegValueIstaLocation = @"InstallLocation";
        public const string ArgumentGenerateModulesDirect = "-GenerateModulesDirect";
        public const string ArgumentGenerateServiceModules = "-GenerateServiceModules";
        public const string ArgumentGenerateTestModules = "-GenerateTestModules";
        public const string GlobalMutexGenerateServiceModules = "PsdzClient_GenerateServiceModules";
        public const string GlobalMutexGenerateTestModules = "PsdzClient_GenerateTestModules";
        public static readonly long TickResolMs = Stopwatch.Frequency / 1000;

        [PreserveSource(Hint = "Custom code")]
        public enum ExecutionMode
        {
            Normal,
            GenerateModulesDirect,
            GenerateServiceModules,
            GenerateTestModules,
        }

        [PreserveSource(Hint = "Custom code")]
        public enum OperationType
        {
            CreateOptions,
            BuildTalILevel,
            BuildTalModFa,
            ExecuteTal,
        }

        [PreserveSource(Hint = "Custom code")]
        public enum CacheType
        {
            None,
            NoResponse,
            FuncAddress,
        }

        [PreserveSource(Hint = "Custom code")]
        public class OptionsItem
        {
            public OptionsItem(PsdzDatabase.SwiRegisterEnum swiRegisterEnum,PsdzDatabase.SwiAction swiAction, ClientContext clientContext)
            {
                Init();
                SwiRegisterEnum = swiRegisterEnum;
                SwiAction = swiAction;
                ClientContext = clientContext;
            }

            public OptionsItem(PsdzDatabase.SwiRegisterEnum swiRegisterEnum, PsdzDatabase.EcuInfo ecuInfo, IEcuLogisticsEntry ecuLogisticsEntry, ClientContext clientContext)
            {
                Init();
                SwiRegisterEnum = swiRegisterEnum;
                EcuInfo = ecuInfo;
                EcuLogisticsEntry = ecuLogisticsEntry;
                ClientContext = clientContext;
            }

            private void Init()
            {
                Id = Guid.NewGuid().ToString();
                Invalid = false;
            }

            public PsdzDatabase.SwiRegisterEnum SwiRegisterEnum { get; private set; }

            public PsdzDatabase.SwiAction SwiAction { get; private set; }

            public PsdzDatabase.EcuInfo EcuInfo { get; private set; }

            public IEcuLogisticsEntry EcuLogisticsEntry { get; private set; }

            public ClientContext ClientContext { get; private set; }

            public string Id { get; private set; }

            public bool Invalid { get; set; }

            public override string ToString()
            {
                if (SwiAction != null)
                {
                    return SwiAction.EcuTranslation.GetTitle(ClientContext?.Language);
                }

                if (EcuInfo != null)
                {
                    StringBuilder sb = new StringBuilder();
                    string name = EcuInfo.Name;
                    if (EcuLogisticsEntry != null)
                    {
                        if (!string.IsNullOrWhiteSpace(EcuLogisticsEntry.Name))
                        {
                            name = EcuLogisticsEntry.Name;
                        }
                    }

                    sb.Append(name.ToUpperInvariant());
                    if (EcuInfo.EcuVar != null)
                    {
                        sb.Append(" ");
                        sb.Append(EcuInfo.EcuVar.EcuTranslation.GetTitle(ClientContext?.Language));
                    }

                    return sb.ToString();
                }

                return string.Empty;
            }
        }

        [PreserveSource(Hint = "Custom code")]
        public class OptionType
        {
            public OptionType(string name, PsdzDatabase.SwiRegisterEnum swiRegisterEnum)
            {
                Name = name;
                SwiRegisterEnum = swiRegisterEnum;
                SwiRegister = null;
            }

            public string Name { get; private set; }

            public PsdzDatabase.SwiRegisterEnum SwiRegisterEnum { get; private set; }

            public PsdzDatabase.SwiRegister SwiRegister { get; set; }

            public ClientContext ClientContext { get; set; }

            public override string ToString()
            {
                PsdzDatabase.SwiRegister swiRegister = SwiRegister;
                if (swiRegister != null)
                {
                    return swiRegister.EcuTranslation.GetTitle(ClientContext?.Language);
                }
                return Name;
            }
        }

        [PreserveSource(Hint = "Custom code")]
        [XmlType("SelectedOptionData")]
        public class SelectedOptionData
        {
            public SelectedOptionData()
            {
            }

            public SelectedOptionData(string swiId, PsdzDatabase.SwiRegisterEnum swiRegister)
            {
                SwiId = swiId;
                SwiRegister = swiRegister;
            }

            [XmlElement("SwiId")] public string SwiId { get; set; }
            [XmlElement("SwiRegister")] public PsdzDatabase.SwiRegisterEnum SwiRegister { get; set; }
        }


        [PreserveSource(Hint = "Custom code")]
        [XmlInclude(typeof(SelectedOptionData)), XmlInclude(typeof(VehicleStructsBmw.VersionInfo))]
        [XmlType("OperationStateData")]
        public class OperationStateData
        {
            public const int StructVersionCurrent = 1;

            [PreserveSource(Hint = "Custom code")]
            public enum OperationEnum
            {
                Idle,
                HwDeinstall,
                HwInstall,
                Modification,
            }

            [PreserveSource(Hint = "Custom code")]
            public enum TalExecutionStateEnum
            {
                None,
                BackupTalExecuting,
                TalExecuting,
                RestoreTalExecuting,
                TslUpdateExecuting,
                WriteILevelExecuting,
                WriteILevelBackupExecuting,
                UpdatePiaMasterExecuting,
                WriteFaExecuting,
                WriteFaBackupExecuting,
                Finished,
            }

            [PreserveSource(Hint = "Custom code")]
            public enum TalExecutionResultEnum
            {
                None,
                Started,
                Skipped,
                Success,
                Failure,
                Cancelled,
            }

            public OperationStateData()
            {
            }

            public OperationStateData(OperationEnum operation, List<int> diagAddrList = null)
            {
                StructVersion = StructVersionCurrent;
                Version = null;
                Operation = operation;
                DiagAddrList = diagAddrList;
                BackupTargetFA = null;
                TalExecutionDict = null;
                SelectedOptionList = null;
            }

            [XmlElement("StructVersion")] public int StructVersion { get; set; }
            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }
            [XmlElement("Operation")] public OperationEnum Operation { get; set; }
            [XmlArray("DiagAddrList"), DefaultValue(null)] public List<int> DiagAddrList { get; set; }
            [XmlElement("TalExecutionState")] public TalExecutionStateEnum TalExecutionState { get; set; }
            [XmlElement("BackupTargetFA"), DefaultValue(null)] public string BackupTargetFA { get; set; }
            [XmlElement("TalExecutionDict"), DefaultValue(null)] public SerializableDictionary<TalExecutionStateEnum, TalExecutionResultEnum> TalExecutionDict { get; set; }
            [XmlArray("SelectedOptionList"), DefaultValue(null)] public List<SelectedOptionData> SelectedOptionList { get; set; }
        }

        public delegate void UpdateStatusDelegate(string message = null);
        public event UpdateStatusDelegate UpdateStatusEvent;

        public delegate void ProgressDelegate(int percent, bool marquee, string message = null);
        public event ProgressDelegate ProgressEvent;

        public delegate void UpdateOptionsDelegate(Dictionary<PsdzDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict);
        public event UpdateOptionsDelegate UpdateOptionsEvent;

        public delegate void UpdateOptionSelectionsDelegate(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum);
        public event UpdateOptionSelectionsDelegate UpdateOptionSelectionsEvent;

        public delegate bool ShowMessageDelegate(CancellationTokenSource cts, string message, bool okBtn, bool wait);
        public event ShowMessageDelegate ShowMessageEvent;

        public delegate int TelSendQueueSizeDelegate();
        public event TelSendQueueSizeDelegate TelSendQueueSizeEvent;

        public delegate void ServiceInitialized(ProgrammingService2 programmingService);
        public event ServiceInitialized ServiceInitializedEvent;

        private static readonly ILog log = LogManager.GetLogger(typeof(ProgrammingJobs));
        private OptionType[] _optionTypes =
        {
            new OptionType("Coding", PsdzDatabase.SwiRegisterEnum.VehicleModificationCodingConversion),
            new OptionType("Coding back", PsdzDatabase.SwiRegisterEnum.VehicleModificationCodingBackConversion),
            new OptionType("Modification", PsdzDatabase.SwiRegisterEnum.VehicleModificationConversion),
            new OptionType("Modification back", PsdzDatabase.SwiRegisterEnum.VehicleModificationBackConversion),
            new OptionType("Retrofit", PsdzDatabase.SwiRegisterEnum.VehicleModificationRetrofitting),
            new OptionType("Before replacement", PsdzDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement),
            new OptionType("After replacement", PsdzDatabase.SwiRegisterEnum.EcuReplacementAfterReplacement),
        };
        public OptionType[] OptionTypes => _optionTypes;

        public const int CodingConnectionTimeout = 10000;

        public const double MinBatteryVoltageErrorPb = 9.95;
        public const double MinBatteryVoltageErrorLfp = 10.25;

        public const double MinBatteryVoltageWarnPb = 12.55;
        public const double MinBatteryVoltageWarnLfp = 12.35;

        public const double MaxBatteryVoltageWarnPb = 14.85;
        public const double MaxBatteryVoltageWarnLfp = 14.05;

        public const double MaxBatteryVoltageErrorPb = 15.55;
        public const double MaxBatteryVoltageErrorLfp = 14.45;

        private bool _disposed;
        public ClientContext ClientContext { get; private set; }
        private string _dealerId;
        private ExecutionMode _executionMode;
        private object _contextLock = new object();
        private object _cacheLock = new object();
        private object _operationLock = new object();
        private object _optionsLock = new object();
        public ProgrammingService2 ProgrammingService { get; private set; }
        public List<OptionsItem> SelectedOptions { get; set; }
        public bool DisableTalFlash { get; set; }
        public PsdzDatabase.SwiRegisterGroup RegisterGroup { get; set; }
        public IntPtr ParentWindowHandle { get; set; }
        public bool GenServiceModules { get; set; }

        private PsdzContext _psdzContext;
        public PsdzContext PsdzContext
        {
            get
            {
                lock (_contextLock)
                {
                    return _psdzContext;
                }
            }

            private set
            {
                lock (_contextLock)
                {
                    _psdzContext = value;
                }
            }
        }

        private OperationStateData _operationState;
        public OperationStateData OperationState
        {
            get
            {
                lock (_operationLock)
                {
                    return _operationState;
                }
            }

            set
            {
                lock (_operationLock)
                {
                    _operationState = value;
                }
            }
        }

        private Dictionary<PsdzDatabase.SwiRegisterEnum, List<OptionsItem>> _optionsDict;
        public Dictionary<PsdzDatabase.SwiRegisterEnum, List<OptionsItem>> OptionsDict
        {
            get
            {
                lock (_optionsLock)
                {
                    return _optionsDict;
                }
            }

            set
            {
                lock (_optionsLock)
                {
                    _optionsDict = value;
                }
            }
        }

        private CacheType _cacheResponseType;
        public CacheType CacheResponseType
        {
            get
            {
                lock (_cacheLock)
                {
                    return _cacheResponseType;
                }
            }

            set
            {
                lock (_cacheLock)
                {
                    _cacheResponseType = value;
                }
            }
        }

        private bool _cacheClearRequired;
        public bool CacheClearRequired
        {
            get
            {
                lock (_cacheLock)
                {
                    return _cacheClearRequired;
                }
            }

            set
            {
                lock (_cacheLock)
                {
                    _cacheClearRequired = value;
                }
            }
        }

        private bool _licenseValid;
        public bool LicenseValid
        {
            get
            {
                lock (_cacheLock)
                {
                    return _licenseValid;
                }
            }

            set
            {
                lock (_cacheLock)
                {
                    _licenseValid = value;
                }
            }
        }

        public ProgrammingJobs(string dealerId, ExecutionMode executionMode = ExecutionMode.Normal)
        {
            ClientContext = new ClientContext();
            _dealerId = dealerId;
            _executionMode = executionMode;
            ProgrammingService = null;
        }

        public bool IsModuleGenerationMode()
        {
            switch (_executionMode)
            {
                case ExecutionMode.Normal:
                case ExecutionMode.GenerateModulesDirect:
                    return false;
            }

            return true;
        }

        public bool StartProgrammingService(CancellationTokenSource cts, string istaFolder)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "Start programming service DealerId={0}", _dealerId);
                if (!IsModuleGenerationMode())
                {
                    sbResult.AppendLine(Strings.HostStarting);
                    UpdateStatus(sbResult.ToString());
                }

                if (PsdzDatabase.RestartRequired)
                {
                    sbResult.AppendLine(Strings.AppRestartRequired);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (ProgrammingService != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Programming service already existing");
                }
                else
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Starting programming service MultiSession={0}", ConfigSettings.GetActivateSdpOnlinePatch());
                    ProgrammingService = new ProgrammingService2(istaFolder, _dealerId);
                    if (!EdiabasNet.IsDirectoryWritable(ProgrammingService.BackupDataPath))
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.DirectoryWriteProtected, ProgrammingService.BackupDataPath));
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    ClientContext.Database = ProgrammingService.PsdzDatabase;
                    if (ServiceInitializedEvent != null)
                    {
                        ServiceInitializedEvent.Invoke(ProgrammingService);
                    }

                    ProgrammingService.EventManager.ProgrammingEventRaised += (sender, args) =>
                    {
                        if (args is ProgrammingTaskEventArgs programmingEventArgs)
                        {
                            if (programmingEventArgs.IsTaskFinished)
                            {
                                ProgressEvent?.Invoke(100, false);
                            }
                            else
                            {
                                int progress = (int)(programmingEventArgs.Progress * 100.0);
                                string message = string.Format(CultureInfo.InvariantCulture, "{0}%, {1:0}s", progress, programmingEventArgs.TimeLeftSec);
                                ProgressEvent?.Invoke(progress, false, message);
                            }
                        }
                    };

                    bool executeDirect = _executionMode == ExecutionMode.GenerateModulesDirect;
                    if (ProgrammingService.PsdzDatabase.IsExecutable())
                    {
                        if (!ProgrammingService.PsdzDatabase.SaveVehicleSeriesInfo(ClientContext,
                                (startConvert, progress, failures) =>
                                {
                                    if (startConvert)
                                    {
                                        sbResult.AppendLine(Strings.GeneratingInfoFiles);
                                        UpdateStatus(sbResult.ToString());
                                    }

                                    if (cts != null)
                                    {
                                        return cts.Token.IsCancellationRequested;
                                    }
                                    return false;
                                }))
                        {
                            log.ErrorFormat("SaveVehicleSeriesInfo failed");
                            sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (!ProgrammingService.PsdzDatabase.SaveFaultRulesInfo(ClientContext,
                                (startConvert, progress, failures) =>
                                {
                                    if (startConvert)
                                    {
                                        sbResult.AppendLine(Strings.GeneratingInfoFiles);
                                        UpdateStatus(sbResult.ToString());
                                    }

                                    if (cts != null)
                                    {
                                        return cts.Token.IsCancellationRequested;
                                    }
                                    return false;
                                }))
                        {
                            log.ErrorFormat("SaveFaultRulesInfo failed");
                            sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (GenServiceModules || _executionMode == ExecutionMode.GenerateServiceModules)
                        {
                            bool checkOnlyService = _executionMode != ExecutionMode.GenerateServiceModules;

                            for (; ; )
                            {
                                if (!executeDirect)
                                {
                                    if (_executionMode != ExecutionMode.Normal && _executionMode != ExecutionMode.GenerateServiceModules)
                                    {
                                        break;
                                    }

                                    if (!checkOnlyService)
                                    {
                                        if (!IsMasterProcessMutexValid(GlobalMutexGenerateServiceModules))
                                        {
                                            log.ErrorFormat("IsMasterProcessMutexValid: Mutex invalid: {0}", GlobalMutexGenerateServiceModules);
                                            sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                            UpdateStatus(sbResult.ToString());
                                            return false;
                                        }
                                    }
                                }

                                int failCountService = -1;
                                int lastProgressService = 100;
                                bool resultService = ProgrammingService.PsdzDatabase.GenerateServiceModuleData((startConvert, progress, failures) =>
                                {
                                    if (startConvert)
                                    {
                                        sbResult.AppendLine(Strings.GeneratingInfoFiles);
                                        UpdateStatus(sbResult.ToString());
                                    }
                                    else
                                    {
                                        failCountService = failures;
                                        lastProgressService = progress;
                                        string message = string.Format(CultureInfo.InvariantCulture, Strings.TestModuleProgress, progress, failures);
                                        ProgressEvent?.Invoke(progress, false, message);
                                    }

                                    if (!checkOnlyService && !executeDirect)
                                    {
                                        if (!IsMasterProcessMutexValid(GlobalMutexGenerateServiceModules))
                                        {
                                            log.ErrorFormat("Aborting IsMasterProcessMutexValid: Mutex invalid: {0}", GlobalMutexGenerateServiceModules);
                                            return true;
                                        }
                                    }

                                    if (cts != null)
                                    {
                                        return cts.Token.IsCancellationRequested;
                                    }
                                    return false;
                                }, checkOnlyService && !executeDirect);

                                if (checkOnlyService)
                                {
                                    if (resultService)
                                    {
                                        break;
                                    }

                                    if (!ExecuteSubProcess(cts, ArgumentGenerateServiceModules, GlobalMutexGenerateServiceModules))
                                    {
                                        log.ErrorFormat("GenerateServiceModuleData failed");
                                        sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                        UpdateStatus(sbResult.ToString());
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (!resultService)
                                    {
                                        log.ErrorFormat("GenerateServiceModuleData failed");
                                        sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                        UpdateStatus(sbResult.ToString());
                                        return false;
                                    }
                                }

                                if (lastProgressService < 100)
                                {
                                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.TestModuleNotCompleted, lastProgressService));
                                }

                                if (failCountService >= 0)
                                {
                                    log.InfoFormat("Test module generation failures: {0}", failCountService);
                                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.TestModuleFailures, failCountService));
                                    UpdateStatus(sbResult.ToString());
                                }

                                if (executeDirect)
                                {
                                    break;
                                }

                                if (!checkOnlyService)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    if (PsdzDatabase.RestartRequired)
                    {
                        sbResult.AppendLine(Strings.AppRestartRequired);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }
#if false
                    List<PsdzDatabase.SwiDiagObj> diagObjsNodeClass = ProgrammingService.PsdzDatabase.GetInfoObjectsTreeForNodeclassName(
                        PsdzDatabase.DiagObjServiceRoot, null, new List<string> { PsdzDatabase.AblFilter });
                    if (diagObjsNodeClass != null)
                    {
                        foreach (PsdzDatabase.SwiDiagObj swiDiagObj in diagObjsNodeClass)
                        {
                            log.InfoFormat("GetInfoObjectsTreeForNodeclassName all InfoObject: {0}", swiDiagObj.InfoObjectsCount);
                            log.Info(swiDiagObj.ToString(ClientContext.Language));
                        }
                    }
#endif
                    bool checkOnlyTest = _executionMode != ExecutionMode.GenerateTestModules;
                    for (int loop = 0;; loop++)
                    {
                        if (!executeDirect)
                        {
                            if (_executionMode != ExecutionMode.Normal && _executionMode != ExecutionMode.GenerateTestModules)
                            {
                                break;
                            }

                            if (!checkOnlyTest)
                            {
                                if (!IsMasterProcessMutexValid(GlobalMutexGenerateTestModules))
                                {
                                    log.ErrorFormat("IsMasterProcessMutexValid: Mutex invalid: {0}", GlobalMutexGenerateTestModules);
                                    sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                    UpdateStatus(sbResult.ToString());
                                    return false;
                                }
                            }
                        }

                        int failCountTest = -1;
                        bool resultTest = ProgrammingService.PsdzDatabase.GenerateTestModuleData((startConvert, progress, failures) =>
                        {
                            if (startConvert)
                            {
                                sbResult.AppendLine(Strings.GeneratingInfoFiles);
                                UpdateStatus(sbResult.ToString());
                            }
                            else
                            {
                                failCountTest = failures;
                                string message = string.Format(CultureInfo.InvariantCulture, Strings.TestModuleProgress, progress, failures);
                                ProgressEvent?.Invoke(progress, false, message);
                            }

                            if (!checkOnlyTest && !executeDirect)
                            {
                                if (!IsMasterProcessMutexValid(GlobalMutexGenerateTestModules))
                                {
                                    log.ErrorFormat("Aborting IsMasterProcessMutexValid: Mutex invalid: {0}", GlobalMutexGenerateTestModules);
                                    return true;
                                }
                            }

                            if (cts != null)
                            {
                                return cts.Token.IsCancellationRequested;
                            }
                            return false;
                        }, checkOnlyTest && !executeDirect);

                        if (!resultTest && !ProgrammingService.PsdzDatabase.IsExecutable())
                        {
                            log.ErrorFormat("No test module data present");
                            sbResult.AppendLine(Strings.TestModuleDataMissing);
                            return false;
                        }

                        if (checkOnlyTest)
                        {
                            if (resultTest)
                            {
                                break;
                            }

                            if (loop > 1)
                            {
                                log.ErrorFormat("GenerateTestModuleData loop exceeded");
                                sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                UpdateStatus(sbResult.ToString());
                                return false;
                            }

                            if (!ExecuteSubProcess(cts, ArgumentGenerateTestModules, GlobalMutexGenerateTestModules))
                            {
                                log.ErrorFormat("GenerateTestModuleData failed");
                                sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                UpdateStatus(sbResult.ToString());
                                return false;
                            }
                        }
                        else
                        {
                            if (!resultTest)
                            {
                                log.ErrorFormat("GenerateTestModuleData failed");
                                sbResult.AppendLine(Strings.GenerateInfoFilesFailed);
                                UpdateStatus(sbResult.ToString());
                                return false;
                            }
                        }

                        if (failCountTest > 0)
                        {
                            log.InfoFormat("Test module generation failures: {0}", failCountTest);
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.TestModuleFailures, failCountTest));
                            UpdateStatus(sbResult.ToString());
                        }

                        if (executeDirect)
                        {
                            break;
                        }

                        if (!checkOnlyTest)
                        {
                            return true;
                        }
                    }

                    bool resultEcuCharacteristics = true;
                    if (!ProgrammingService.PsdzDatabase.GenerateEcuCharacteristicsData())
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData failed");
                        resultEcuCharacteristics = false;
                    }

                    ProgressEvent?.Invoke(0, true);

                    if (!resultEcuCharacteristics)
                    {
                        if (!ProgrammingService.PsdzDatabase.IsExecutable())
                        {
                            log.ErrorFormat("No test module data present");
                            sbResult.AppendLine(Strings.TestModuleDataMissing);
                        }
                        else
                        {
                            log.ErrorFormat("Generating test module data failed");
                            sbResult.AppendLine(Strings.HostStartFailed);
                        }
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }
                }

                if (PsdzDatabase.RestartRequired)
                {
                    sbResult.AppendLine(Strings.AppRestartRequired);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (!ProgrammingService.Psdz.IsPsdzInitialized)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Starting host");
                    if (!ProgrammingService.StartPsdzService())
                    {
                        sbResult.AppendLine(Strings.HostStartFailed);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "Host started");
                }

                EnableLogTrace(true);

                sbResult.AppendLine(Strings.HostStarted);
                UpdateStatus(sbResult.ToString());

                ProgrammingService.PsdzDatabase.ResetXepRules();
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "StartProgrammingService Exception: {0}", ex.Message);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.ExceptionMsg, ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
            finally
            {
                if (ProgrammingService != null)
                {
                    if (!ProgrammingService.Psdz.IsPsdzInitialized)
                    {
                        ProgrammingService.Dispose();
                        ProgrammingService = null;
                    }
                }
            }
        }

        public bool StopProgrammingService(CancellationTokenSource cts, string istaFolder, bool force = false)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine(Strings.HostStopping);
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService != null && ProgrammingService.Psdz.IsPsdzInitialized)
                {
                    ProgrammingService.CloseConnectionsToPsdz(force);
                    ProgrammingService.Dispose();
                    ProgrammingService = null;
                    ClientContext.Database = null;
                    ClearProgrammingObjects();
                }

                sbResult.AppendLine(Strings.HostStopped);
                UpdateStatus(sbResult.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "StopProgrammingService Exception: {0}", ex.Message);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.ExceptionMsg, ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }

            return true;
        }

        public void EnableLogTrace(bool enableTrace)
        {
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Logging.Level.Trace.Enabled"))
            {
                ClientContext.IsProblemHandlingTraceRunning = true;
                ProgrammingService.SetLogLevelToMax();
                CoreFramework.DebugLevel = ConfigSettings.getConfigint("BMW.Rheingold.Logging.Level.Trace.Ista", 5);
            }
            else
            {
                ClientContext.IsProblemHandlingTraceRunning = false;
                ProgrammingService.SetLogLevelToNormal();
            }
        }

        public bool ConnectVehicle(CancellationTokenSource cts, string istaFolder, string remoteHost, bool useIcom, int addTimeout = 1000)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "ConnectVehicle Start - IstaFolder: {0}, Ip: {1}, ICOM: {2}, Timeout: {3}", istaFolder, remoteHost, useIcom, addTimeout);
            StringBuilder sbResult = new StringBuilder();

            if (string.IsNullOrEmpty(istaFolder) || !Directory.Exists(istaFolder))
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "Invalid IstaFolder: {0}", istaFolder);
                return false;
            }

            if (ProgrammingService == null)
            {
                if (!StartProgrammingService(cts, istaFolder))
                {
                    return false;
                }

                if (IsModuleGenerationMode())
                {
                    return true;
                }
            }

            try
            {
                sbResult.AppendLine(Strings.VehicleConnecting);
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService == null)
                {
                    return false;
                }

                PsdzDatabase.DbInfo dbInfo = ProgrammingService.PsdzDatabase.GetDbInfo();
                if (dbInfo != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "DbInfo: {0}", dbInfo.ToString());

                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.DbInfo, dbInfo.Version, dbInfo.DateTime.ToShortDateString()));
                    UpdateStatus(sbResult.ToString());
                }

                string swiVersion = PsdzDatabase.GetSwiVersion();
                if (!string.IsNullOrEmpty(swiVersion))
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SWI Version: {0}", swiVersion);

                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.SwiVersion, swiVersion));
                    UpdateStatus(sbResult.ToString());
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Connecting to: Host={0}, ICOM={1}", remoteHost, useIcom);
                string[] hostParts = remoteHost.Split(':');
                if (hostParts.Length < 1)
                {
                    return false;
                }

                string ipAddress = hostParts[0];
                if (!InitProgrammingObjects(istaFolder))
                {
                    return false;
                }

                sbResult.AppendLine(Strings.VehicleDetecting);
                UpdateStatus(sbResult.ToString());

                CacheResponseType = CacheType.NoResponse;
                bool icomConnection = useIcom;
                if (hostParts.Length > 1)
                {
                    icomConnection = true;
                }

                int diagPort = icomConnection ? EdInterfaceEnet.IcomDiagPortDefault : EdInterfaceEnet.DiagPortDefault;
                int controlPort = icomConnection ? EdInterfaceEnet.IcomControlPortDefault : EdInterfaceEnet.ControlPortDefault;

                if (hostParts.Length >= 2)
                {
                    Int64 portValue = EdiabasNet.StringToValue(hostParts[1], out bool valid);
                    if (valid)
                    {
                        diagPort = (int)portValue;
                    }
                }

                if (hostParts.Length >= 3)
                {
                    Int64 portValue = EdiabasNet.StringToValue(hostParts[2], out bool valid);
                    if (valid)
                    {
                        controlPort = (int)portValue;
                    }
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "ConnectVehicle Ip: {0}, Diag: {1}, Control: {2}, ICOM: {3}, Timeout: {4}", ipAddress, diagPort, controlPort, icomConnection, addTimeout);
                EdInterfaceEnet.EnetConnection.InterfaceType interfaceType = EdInterfaceEnet.EnetConnection.InterfaceType.DirectHsfz;
                if (icomConnection)
                {
                    interfaceType = EdInterfaceEnet.EnetConnection.InterfaceType.Icom;
                }
                else
                {
                    if (ProgrammingService.PsdzDatabase.IsExecutable())
                    {
                        interfaceType = EdInterfaceEnet.EnetConnection.InterfaceType.Undefined;
                    }
                }

                EdInterfaceEnet.EnetConnection enetConnection;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (icomConnection)
                {
                    enetConnection = new EdInterfaceEnet.EnetConnection(interfaceType, IPAddress.Parse(ipAddress), diagPort, controlPort);
                }
                else
                {
                    enetConnection = new EdInterfaceEnet.EnetConnection(interfaceType, IPAddress.Parse(ipAddress));
                }

                PsdzContext.DetectVehicle = new DetectVehicle(ProgrammingService.PsdzDatabase, ClientContext, istaFolder, enetConnection, useIcom, addTimeout);
                if (!EdiabasNet.IsDirectoryWritable(PsdzContext.DetectVehicle.DoIpS29BasePath))
                {
                    sbResult.AppendLine(string.Format(Strings.DirectoryWriteProtected, PsdzContext.DetectVehicle.DoIpS29BasePath));
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (!EdiabasNet.IsDirectoryWritable(PsdzContext.DetectVehicle.DoIpS29CertPath))
                {
                    sbResult.AppendLine(string.Format(Strings.DirectoryWriteProtected, PsdzContext.DetectVehicle.DoIpS29CertPath));
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                DetectVehicle.DetectResult detectResult = PsdzContext.DetectVehicle.DetectVehicleBmwFast(() =>
                {
                    if (cts != null)
                    {
                        return cts.Token.IsCancellationRequested;
                    }
                    return false;
                }, percent =>
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Strings.VehicleDetectingProgress, percent);
                    ProgressEvent?.Invoke(percent, false, message);
                });

                ProgressEvent?.Invoke(0, true);
                cts?.Token.ThrowIfCancellationRequested();

                CacheResponseType = CacheType.FuncAddress;
                string series = PsdzContext.DetectVehicle.Series;
                if (string.IsNullOrEmpty(series))
                {
                    sbResult.AppendLine(Strings.VehicleSeriesNotDetected);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (detectResult != DetectVehicle.DetectResult.Ok)
                {
                    string bnType = PsdzContext.DetectVehicle.BnType;
                    if (!string.IsNullOrEmpty(bnType))
                    {
                        // ToDo: Check on update
                        if (bnType.IndexOf("2020", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.VehicleSeriesInvalid, series));
                        }
                    }

                    switch (detectResult)
                    {
                        case DetectVehicle.DetectResult.InvalidDatabase:
                            sbResult.AppendLine(Strings.VehicleDatabaseInvalid);
                            break;

                        default:
                            sbResult.AppendLine(Strings.VehicleDetectionFailed);
                            break;
                    }

                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Detected vehicle: VIN={0}, GroupFile={1}, BR={2}, Series={3}, BuildDate={4}-{5}",
                    PsdzContext.DetectVehicle.Vin ?? string.Empty, PsdzContext.DetectVehicle.GroupSgbd ?? string.Empty,
                    PsdzContext.DetectVehicle.BrName ?? string.Empty,
                    PsdzContext.DetectVehicle.Series ?? string.Empty,
                    PsdzContext.DetectVehicle.ConstructYear ?? string.Empty,
                    PsdzContext.DetectVehicle.ConstructMonth ?? string.Empty);

                log.InfoFormat(CultureInfo.InvariantCulture, "Detected ILevel: Ship={0}, Current={1}, Backup={2}",
                    PsdzContext.DetectVehicle.ILevelShip ?? string.Empty, PsdzContext.DetectVehicle.ILevelCurrent ?? string.Empty,
                    PsdzContext.DetectVehicle.ILevelBackup ?? string.Empty);

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.VehicleInfo,
                    PsdzContext.DetectVehicle.Vin ?? string.Empty,
                    PsdzContext.DetectVehicle.Series ?? string.Empty,
                    PsdzContext.DetectVehicle.ILevelShip ?? string.Empty,
                    PsdzContext.DetectVehicle.ILevelCurrent ?? string.Empty));

                log.InfoFormat(CultureInfo.InvariantCulture, "Ecus: {0}", PsdzContext.DetectVehicle.EcuListPsdz.Count());
                foreach (PsdzDatabase.EcuInfo ecuInfo in PsdzContext.DetectVehicle.EcuListPsdz)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, " Ecu: Name={0}, Addr={1}, Sgdb={2}, Group={3}",
                        ecuInfo.Name, ecuInfo.Address, ecuInfo.Sgbd, ecuInfo.Grp);
                }

                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                // From GetVehicleSvtUsingPsdz
                string mainSeries = ProgrammingService.Psdz.ConfigurationService.RequestBaureihenverbund(series);
                IEnumerable<IPsdzTargetSelector> targetSelectors =
                    ProgrammingService.Psdz.ConnectionFactoryService.GetTargetSelectors();
                PsdzContext.TargetSelectors = targetSelectors;
                TargetSelectorChooser targetSelectorChooser = new TargetSelectorChooser(PsdzContext.TargetSelectors);
                IPsdzTargetSelector psdzTargetSelectorNewest =
                    targetSelectorChooser.GetNewestTargetSelectorByMainSeries(mainSeries);
                if (psdzTargetSelectorNewest == null)
                {
                    sbResult.AppendLine(Strings.VehicleNoTargetSelector);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.ProjectName = psdzTargetSelectorNewest.Project;
                PsdzContext.VehicleInfo = psdzTargetSelectorNewest.VehicleInfo;
                log.InfoFormat(CultureInfo.InvariantCulture, "Target selector: Project={0}, Vehicle={1}, Series={2}",
                    psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo,
                    psdzTargetSelectorNewest.Baureihenverbund);
                cts?.Token.ThrowIfCancellationRequested();

                // From ConnectionManager.ConnectToProject
                string bauIStufe = PsdzContext.DetectVehicle.ILevelShip;
                bool isDoIp = PsdzContext.DetectVehicle.IsDoIp;

                Vehicle vehicle = new Vehicle(ClientContext);
                vehicle.VCI.VCIType = icomConnection ?
                    global::BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ICOM : global::BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ENET;
                vehicle.VCI.IPAddress = ipAddress;
                vehicle.VCI.Port = diagPort;
                vehicle.VCI.NetworkType = "LAN";
                vehicle.VCI.VIN = PsdzContext.DetectVehicle.Vin;
                vehicle.VCI.IsDoIP = isDoIp;
                vehicle.VIN17 = PsdzContext.DetectVehicle.Vin;

                IPsdzConnection psdzConnection;
                if (icomConnection)
                {
                    int useDiagPort = isDoIp ? EdInterfaceEnet.IcomSslPortDefault : diagPort;
                    string url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", ipAddress, useDiagPort);

                    if (isDoIp)
                    {
                        psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverIcom(
                            psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, addTimeout, series,
                            bauIStufe, IcomConnectionType.Ip, false, true);
                    }
                    else
                    {
                        psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverIcom(
                            psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, addTimeout, series,
                            bauIStufe, IcomConnectionType.Ip, false);
                    }
                }
                else
                {
                    int useDiagPort = isDoIp ? 13400 : diagPort;
                    string url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", ipAddress, useDiagPort);

                    if (isDoIp)
                    {
                        psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverEthernet(
                            psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, series,
                            bauIStufe, true);
                    }
                    else
                    {
                        psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverEthernet(
                            psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, series,
                            bauIStufe);
                    }
                }

                if (isDoIp)
                {
                    ISec4DiagHandler sec4DiagHandler = PsdzContext.DetectVehicle.GetSec4DiagHandler();
                    if (sec4DiagHandler == null || sec4DiagHandler.Sec4DiagCertificates == null)
                    {
                        sbResult.AppendLine(Strings.CertificatesNotPresent);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    ConnectionManager connectionManager = new ConnectionManager(ProgrammingService.Psdz, vehicle, null);
                    connectionManager.RegisterCallbackAndPassCertificatesToPsdzPublic(psdzConnection);
                    ProgrammingService.Psdz.SecureDiagnosticsService.UnlockGateway(psdzConnection);
                }

                PsdzContext.VecInfo = vehicle;
                ProgrammingService.CreateEcuProgrammingInfos(PsdzContext.VecInfo);
                if (!PsdzContext.UpdateVehicle(ProgrammingService))
                {
                    sbResult.AppendLine(Strings.UpdateVehicleDataFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                string saString = PsdzContext.GetLocalizedSaString();
                if (!string.IsNullOrEmpty(saString))
                {
                    log.Info("Localized SaLaPa:" + Environment.NewLine + saString);
                }
#if false
                for (int type = 0; type < 1; type++)
                {
                    ProgrammingService.PsdzDatabase.UseIsAtLeastOnePathToRootValid = type == 0;
                    List<PsdzDatabase.SwiDiagObj> diagObjsNodeClass = ProgrammingService.PsdzDatabase.GetInfoObjectsTreeForNodeclassName(
                        PsdzDatabase.DiagObjServiceRoot, vehicle, new List<string> { PsdzDatabase.AblFilter }, true);
                    if (diagObjsNodeClass != null)
                    {
                        foreach (PsdzDatabase.SwiDiagObj swiDiagObj in diagObjsNodeClass)
                        {
                            log.InfoFormat("GetInfoObjectsTreeForNodeclassName for vehicle InfoObject: {0}, OneRoot={1}",
                                swiDiagObj.InfoObjectsCount, ProgrammingService.PsdzDatabase.UseIsAtLeastOnePathToRootValid);
                            log.Info(swiDiagObj.ToString(ClientContext.Language));
                        }
                    }
                }

                ProgrammingService.PsdzDatabase.UseIsAtLeastOnePathToRootValid = true;
#endif
                if (!CheckVoltage(cts, sbResult, true))
                {
                    sbResult.AppendLine(Strings.BatteryVoltageReadError);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.Connection = psdzConnection;

                sbResult.AppendLine(Strings.VehicleConnected);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Connection: Id={0}, Port={1}", psdzConnection.Id, psdzConnection.Port);

                ProgrammingService.AddListener(PsdzContext);
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "ConnectVehicle Exception: {0}", ex.Message);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.ExceptionMsg, ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
            finally
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "ConnectVehicle Finish - Host: {0}, ICOM: {1}", remoteHost, useIcom);
                log.Info(Environment.NewLine + sbResult);

                if (PsdzContext != null)
                {
                    if (PsdzContext.Connection == null)
                    {
                        ClearProgrammingObjects();
                    }
                }
            }
        }

        public bool DisconnectVehicle(CancellationTokenSource cts)
        {
            log.Info("DisconnectVehicle Start");
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine(Strings.VehicleDisconnecting);
                UpdateStatus(sbResult.ToString());

                CacheResponseType = CacheType.FuncAddress;
                if (ProgrammingService == null)
                {
                    sbResult.AppendLine(Strings.VehicleNotConnected);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (PsdzContext?.Connection == null)
                {
                    sbResult.AppendLine(Strings.VehicleNotConnected);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ProgrammingService.RemoveListener();
                ProgrammingService.Psdz.ConnectionManagerService.CloseConnection(PsdzContext.Connection);
                PsdzContext?.CleanupBackupData();

                ClearProgrammingObjects();
                sbResult.AppendLine(Strings.VehicleDisconnected);
                UpdateStatus(sbResult.ToString());
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "DisconnectVehicle Exception: {0}", ex.Message);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.ExceptionMsg, ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
            finally
            {
                log.Info("DisconnectVehicle Finish");
                log.Info(Environment.NewLine + sbResult);

                if (PsdzContext != null)
                {
                    if (PsdzContext.Connection == null)
                    {
                        ClearProgrammingObjects();
                    }
                }
            }
        }

        public string ExecuteContainerXml(CancellationTokenSource cts, string configurationContainerXml, Dictionary<string, string> runOverrideDict)
        {
            string result;
            try
            {
                if (string.IsNullOrEmpty(configurationContainerXml))
                {
                    return null;
                }

                if (PsdzContext.DetectVehicle == null)
                {
                    return null;
                }

                result = PsdzContext.DetectVehicle.ExecuteContainerXml(() =>
                {
                    if (cts != null)
                    {
                        return cts.Token.IsCancellationRequested;
                    }
                    return false;
                }, configurationContainerXml, runOverrideDict);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        public bool VehicleFunctions(CancellationTokenSource cts, OperationType operationType)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "VehicleFunctions Start - Type: {0}", operationType);
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine(Strings.ExecutingVehicleFunc);
                UpdateStatus(sbResult.ToString());

                CacheResponseType = CacheType.FuncAddress;
                if (ProgrammingService == null)
                {
                    sbResult.AppendLine(Strings.VehicleNotConnected);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (PsdzContext?.Connection == null)
                {
                    sbResult.AppendLine(Strings.VehicleNotConnected);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ClientContext clientContext = ClientContext.GetClientContext(PsdzContext.VecInfo);
                if (clientContext == null)
                {
                    sbResult.AppendLine(Strings.ContextMissing);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (!CheckVoltage(cts, sbResult))
                {
                    sbResult.AppendLine(Strings.BatteryVoltageReadError);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzVin psdzVin = ProgrammingService.Psdz.VcmService.GetVinFromMaster(PsdzContext.Connection);
                if (string.IsNullOrEmpty(psdzVin?.Value))
                {
                    sbResult.AppendLine(Strings.ReadVinFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Vin: {0}", psdzVin.Value);
                cts?.Token.ThrowIfCancellationRequested();

                if (operationType == OperationType.ExecuteTal)
                {
                    bool talExecutionFailed = false;
                    if (PsdzContext.Tal == null)
                    {
                        sbResult.AppendLine(Strings.TalMissing);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    DateTime calculationStartTime = DateTime.Now;
                    log.InfoFormat(CultureInfo.InvariantCulture, "Checking programming counter");
                    IEnumerable<IPsdzEcuIdentifier> psdzEcuIdentifiersPrg = ProgrammingService.Psdz.ProgrammingService.CheckProgrammingCounter(PsdzContext.Connection, PsdzContext.Tal);
                    if (psdzEcuIdentifiersPrg == null)
                    {
                        sbResult.AppendLine(Strings.PrgCounterCheckFailed);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    int prgCounter = psdzEcuIdentifiersPrg.Count();
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.PrgCounters, prgCounter));
                    UpdateStatus(sbResult.ToString());

                    log.InfoFormat(CultureInfo.InvariantCulture, "ProgCounter: {0}", prgCounter);
                    foreach (IPsdzEcuIdentifier ecuIdentifier in psdzEcuIdentifiersPrg)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                            ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                    }
                    cts?.Token.ThrowIfCancellationRequested();

                    log.InfoFormat(CultureInfo.InvariantCulture, "Checking NCD availability");
                    PsdzSecureCodingConfigCto secureCodingConfig = SecureCodingConfigWrapper.GetSecureCodingConfig(ProgrammingService);
                    secureCodingConfig.ConnectionTimeout = CodingConnectionTimeout;

                    if (!EdiabasNet.IsDirectoryWritable(secureCodingConfig.NcdRootDirectory, true))
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.DirectoryWriteProtected, secureCodingConfig.NcdRootDirectory));
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    // From SecureCodingLogic.ReturnNcdsToBeCalculated
                    IPsdzCheckNcdResultEto psdzCheckNcdResultEto = ProgrammingService.Psdz.SecureCodingService.CheckNcdAvailabilityForGivenTal(PsdzContext.Tal, secureCodingConfig.NcdRootDirectory, psdzVin);
                    log.InfoFormat(CultureInfo.InvariantCulture, "Ncd EachSigned: {0}", psdzCheckNcdResultEto.isEachNcdSigned);
                    foreach (IPsdzDetailedNcdInfoEto detailedNcdInfo in psdzCheckNcdResultEto.DetailedNcdStatus)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " Ncd: Btld={0}, Cafd={1}, Status={2}",
                            detailedNcdInfo.Btld.HexString, detailedNcdInfo.Cafd.HexString, detailedNcdInfo.NcdStatus);
                    }
                    cts?.Token.ThrowIfCancellationRequested();

                    log.InfoFormat(CultureInfo.InvariantCulture, "Checking requested NCD ETOS");
                    List<IPsdzRequestNcdEto> requestNcdEtos = ProgrammingUtils.CreateRequestNcdEtos(psdzCheckNcdResultEto);
                    log.InfoFormat(CultureInfo.InvariantCulture, "Ncd Requests: {0}", requestNcdEtos.Count);
                    foreach (IPsdzRequestNcdEto requestNcdEto in requestNcdEtos)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " Ncd for Cafd={0}, Btld={1}",
                            requestNcdEto.Cafd.Id, requestNcdEto.Btld.HexString);
                    }

                    string secureCodingPath = SecureCodingConfigWrapper.GetSecureCodingPathWithVin(ProgrammingService, psdzVin.Value);
                    string jsonRequestFilePath = Path.Combine(secureCodingPath, string.Format(CultureInfo.InvariantCulture, "SecureCodingNCDCalculationRequest_{0}_{1}_{2}.json",
                        psdzVin.Value, _dealerId, calculationStartTime.ToString("HHmmss", CultureInfo.InvariantCulture)));
                    PsdzBackendNcdCalculationEtoEnum backendNcdCalculationEtoEnumOld = secureCodingConfig.BackendNcdCalculationEtoEnum;
                    try
                    {
                        try
                        {
                            PsdzContext.VpcFromVcm = null;
                            ProgrammingObjectBuilder programmingObjectBuilder = ProgrammingService.ProgrammingInfos?.ProgrammingObjectBuilder;
                            if (programmingObjectBuilder != null)
                            {
                                IPsdzReadVpcFromVcmCto psdzReadVpcFromVcmCto = ProgrammingService.Psdz.VcmService.RequestVpcFromVcm(PsdzContext.Connection);
                                bool bVcpSuccess = psdzReadVpcFromVcmCto != null && psdzReadVpcFromVcmCto.IsSuccessful;
                                if (!bVcpSuccess)
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "RequestVpcFromVcm failure, Is null: {0}", psdzReadVpcFromVcmCto == null);
                                }
                                else
                                {
                                    IVehicleProfileChecksum vehicleProfileChecksum = programmingObjectBuilder.Build(psdzReadVpcFromVcmCto);
                                    PsdzContext.VpcFromVcm = vehicleProfileChecksum;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat(CultureInfo.InvariantCulture, "RequestVpcFromVcm failure: {0}", ex.Message);
                        }

                        secureCodingConfig.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.ALLOW;
                        byte[] vcpCrc = PsdzContext.VpcFromVcm?.VpcCrc;
                        IList<IPsdzSecurityBackendRequestFailureCto> psdzSecurityBackendRequestFailureList =
                            ProgrammingService.Psdz.SecureCodingService.RequestCalculationNcdAndSignatureOffline(requestNcdEtos, jsonRequestFilePath, secureCodingConfig, psdzVin, PsdzContext.FaTarget, vcpCrc);

                        int failureCount = psdzSecurityBackendRequestFailureList.Count;
                        log.InfoFormat(CultureInfo.InvariantCulture, "Ncd failures: {0}", failureCount);
                        foreach (IPsdzSecurityBackendRequestFailureCto psdzSecurityBackendRequestFailure in psdzSecurityBackendRequestFailureList)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Failure: Cause={0}, Retry={1}, Url={2}",
                                psdzSecurityBackendRequestFailure.Cause, psdzSecurityBackendRequestFailure.Retry, psdzSecurityBackendRequestFailure.Url);
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        if (failureCount > 0)
                        {
                            sbResult.AppendLine(Strings.NcdFailures);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (!File.Exists(jsonRequestFilePath))
                        {
                            sbResult.AppendLine(Strings.NoNcdRequestFile);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        RequestJson requestJson = new JsonHelper().ReadRequestJson(jsonRequestFilePath);
                        if (requestJson == null || !ProgrammingUtils.CheckIfThereAreAnyNcdInTheRequest(requestJson))
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "No ecu data in the request json file. Ncd calculation not required");
                        }
                        else
                        {
                            sbResult.AppendLine(Strings.NcdOnlineCalculation);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        List<string> cafdCalculatedInSCB = ProgrammingUtils.CafdCalculatedInSCB(requestJson).ToList();
                        log.InfoFormat(CultureInfo.InvariantCulture, "Cafd in SCB: {0}", cafdCalculatedInSCB.Count);
                        foreach (string cafd in cafdCalculatedInSCB)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Cafd: {0}", cafd);
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        log.InfoFormat(CultureInfo.InvariantCulture, "Requesting SWE list");
                        List<IPsdzSgbmId> sweList = ProgrammingService.Psdz.LogicService.RequestSweList(PsdzContext.Tal, true).ToList();
                        log.InfoFormat(CultureInfo.InvariantCulture, "Swe list: {0}", sweList.Count());
                        foreach (IPsdzSgbmId psdzSgbmId in sweList)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString);
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        List<IPsdzSgbmId> sgbmIds = ProgrammingUtils.RemoveCafdsCalculatedOnSCB(cafdCalculatedInSCB, sweList).ToList();
                        List<IPsdzSgbmId> softwareEntries = ProgrammingService.Psdz.MacrosService.CheckSoftwareEntries(sgbmIds).ToList();
                        int softwareEntryCount = softwareEntries.Count();
                        log.InfoFormat(CultureInfo.InvariantCulture, "Sw entries: {0}", softwareEntryCount);
                        foreach (IPsdzSgbmId psdzSgbmId in softwareEntries)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString);
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        if (softwareEntryCount > 0)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.SoftwareFailures, softwareEntryCount));
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (!CheckVoltage(cts, sbResult))
                        {
                            sbResult.AppendLine(Strings.BatteryVoltageReadError);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (ShowMessageEvent != null)
                        {
                            if (!ShowMessageEvent.Invoke(cts, Strings.TalExecutionHint, false, true))
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecutionHint aborted");
                                return false;
                            }
                        }

                        if (!CheckVoltage(cts, sbResult))
                        {
                            sbResult.AppendLine(Strings.BatteryVoltageReadError);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }
#if false
                        for (int i = 0; i < 0xFF; i++)
                        {
                            bool invalidSeed = false;
                            try
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, "Updating TSL");
                                ProgrammingService.Psdz.ProgrammingService.TslUpdate(PsdzContext.Connection, true, PsdzContext.SvtActual, PsdzContext.Sollverbauung.Svt);
                                sbResult.AppendLine(Strings.TslUpdated);
                                UpdateStatus(sbResult.ToString());
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "Tsl update failure: {0}", ex.Message);
                                sbResult.AppendLine(Strings.TslUpdateFailed);
                                UpdateStatus(sbResult.ToString());

                                if (ex.Message.Contains("Invalid parameter SEED"))
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "Invalid seed detected: Count={0}", i);
                                    invalidSeed = true;
                                }
                            }
                            cts?.Token.ThrowIfCancellationRequested();

                            if (!invalidSeed)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, "Seed Ok!: Count={0}", i);
                                sbResult.AppendLine("Seed Ok!");
                                UpdateStatus(sbResult.ToString());
                                return true;
                            }
                        }
#endif
                        TalExecutionSettings talExecutionSettings = ProgrammingUtils.GetTalExecutionSettings(ProgrammingService);
                        talExecutionSettings.Parallel = false;
                        talExecutionSettings.TaMaxRepeat = 3;
                        ((PsdzSecureCodingConfigCto)talExecutionSettings.SecureCodingConfig).ConnectionTimeout = CodingConnectionTimeout;
                        ProgrammingUtils.LogTalExecutionSettings(Log.CurrentMethod(), talExecutionSettings);

                        bool backupFailed = false;
                        bool executeBackupTal = true;
                        BackupTalResult backupTalState = PsdzContext.CheckBackupTal();
                        log.InfoFormat(CultureInfo.InvariantCulture, "Backup TAL: State={0}", backupTalState);
                        switch (backupTalState)
                        {
                            case BackupTalResult.Success:
                                executeBackupTal = false;
                                sbResult.AppendLine(Strings.TalExecuteOk);
                                UpdateStatus(sbResult.ToString());
                                break;

                            case BackupTalResult.Error:
                                executeBackupTal = false;
                                backupFailed = true;
                                sbResult.AppendLine(Strings.TalExecuteError);
                                UpdateStatus(sbResult.ToString());
                                break;
                        }

                        bool hwChange = false;
                        switch (RegisterGroup)
                        {
                            case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                            case PsdzDatabase.SwiRegisterGroup.HwInstall:
                                hwChange = true;
                                break;
                        }

                        bool backupValid = !hwChange && OperationState != null && !string.IsNullOrEmpty(OperationState.BackupTargetFA) && PsdzContext.HasBackupData();
                        bool keepBackupData = backupValid;
                        if (backupValid && executeBackupTal)
                        {
                            string currentTargetFA = GetFaString(PsdzContext.FaTarget);
                            bool faChanged = string.CompareOrdinal(currentTargetFA, OperationState.BackupTargetFA) != 0;

                            if (faChanged)
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalBackupFaChanged");

                                if (ShowMessageEvent != null)
                                {
                                    if (ShowMessageEvent.Invoke(cts, Strings.TalBackupFaChanged, false, true))
                                    {
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalBackupFaChanged: Restore settings");
                                        RestoreTalOperationState();
                                        sbResult.AppendLine(Strings.ExecutingVehicleFuncFinished);
                                        UpdateStatus(sbResult.ToString());
                                        return false;
                                    }
                                    else
                                    {
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalBackupFaChanged: Keep settings");
                                        keepBackupData = false;
                                    }
                                }
                            }
                        }

                        log.InfoFormat(CultureInfo.InvariantCulture, "Backup TAL: Execute={0}, KeepBackup={1}", executeBackupTal, keepBackupData);
                        OperationStateData.TalExecutionResultEnum lastTalExecutionResult = OperationStateData.TalExecutionResultEnum.None;

                        if (keepBackupData)
                        {
                            if (!OperationState.TalExecutionDict.TryGetValue(OperationStateData.TalExecutionStateEnum.TalExecuting, out lastTalExecutionResult))
                            {
                                lastTalExecutionResult = OperationStateData.TalExecutionResultEnum.None;
                            }

                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.BackupTalExecuting, true);
                        }
                        else
                        {
                            PsdzContext.RemoveBackupData();
                            if (executeBackupTal)
                            {
                                sbResult.AppendLine(Strings.ExecutingBackupTal);
                                UpdateStatus(sbResult.ToString());

                                CacheResponseType = CacheType.NoResponse;
                                log.InfoFormat(CultureInfo.InvariantCulture, "Executing backup TAL");

                                StartTalExecutionState(OperationStateData.TalExecutionStateEnum.BackupTalExecuting);
                                IPsdzTal backupTalResult = ProgrammingService.Psdz.IndividualDataRestoreService.ExecuteAsyncBackupTal(
                                    PsdzContext.Connection, PsdzContext.IndividualDataBackupTal, null, PsdzContext.FaTarget, psdzVin, talExecutionSettings, PsdzContext.PathToBackupData);
                                if (backupTalResult == null)
                                {
                                    FinishTalExecutionState(cts,true);
                                    log.ErrorFormat("Execute backup TAL failed");
                                    sbResult.AppendLine(Strings.TalExecuteError);
                                    UpdateStatus(sbResult.ToString());
                                    return false;
                                }

                                log.Info("Backup Tal result:");
                                log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", backupTalResult.AsXml.Length);
                                log.InfoFormat(CultureInfo.InvariantCulture, " State: {0}", backupTalResult.TalExecutionState);
                                log.InfoFormat(CultureInfo.InvariantCulture, " Ecus: {0}", backupTalResult.AffectedEcus.Count());
                                foreach (IPsdzEcuIdentifier ecuIdentifier in backupTalResult.AffectedEcus)
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                        ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                                }

                                if (!IsTalExecutionStateOk(backupTalResult.TalExecutionState, true))
                                {
                                    FinishTalExecutionState(cts, true);
                                    talExecutionFailed = true;
                                    backupFailed = true;
                                    log.Error(backupTalResult.AsXml);
                                    sbResult.AppendLine(Strings.TalExecuteError);
                                    UpdateStatus(sbResult.ToString());
                                }
                                else
                                {
                                    FinishTalExecutionState(cts);
                                    if (!IsTalExecutionStateOk(backupTalResult.TalExecutionState))
                                    {
                                        log.Info(backupTalResult.AsXml);
                                        sbResult.AppendLine(Strings.TalExecuteWarning);
                                    }
                                    else
                                    {
                                        sbResult.AppendLine(Strings.TalExecuteOk);
                                    }

                                    UpdateStatus(sbResult.ToString());
                                }

                                CacheClearRequired = true;
                                cts?.Token.ThrowIfCancellationRequested();
                            }
                        }

                        if (!LicenseValid)
                        {
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.None);
                            log.ErrorFormat(CultureInfo.InvariantCulture, "No valid license for TAL execution");
                            sbResult.AppendLine(Strings.NoVehiceLicense);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (talExecutionFailed)
                        {
                            if (ShowMessageEvent != null)
                            {
                                if (!ShowMessageEvent.Invoke(cts, Strings.TalExecuteErrorContinue, false, true))
                                {
                                    StartTalExecutionState(OperationStateData.TalExecutionStateEnum.None);
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecuteContinue aborted");
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (!CheckVoltage(cts, sbResult))
                            {
                                sbResult.AppendLine(Strings.BatteryVoltageReadError);
                                UpdateStatus(sbResult.ToString());
                                return false;
                            }

                            if (executeBackupTal)
                            {
                                if (ShowMessageEvent != null)
                                {
                                    if (!ShowMessageEvent.Invoke(cts, Strings.TalExecuteOkContinue, false, true))
                                    {
                                        StartTalExecutionState(OperationStateData.TalExecutionStateEnum.None);
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecuteContinue aborted");
                                        return false;
                                    }
                                }
                            }
                        }
#if false
                        sbResult.AppendLine("Test mode, abort execution");
                        UpdateStatus(sbResult.ToString());
                        return false;
#endif
                        if (!CheckVoltage(cts, sbResult))
                        {
                            sbResult.AppendLine(Strings.BatteryVoltageReadError);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        bool executeTal = true;
                        if (lastTalExecutionResult == OperationStateData.TalExecutionResultEnum.Success ||
                            lastTalExecutionResult == OperationStateData.TalExecutionResultEnum.Skipped)
                        {
                            if (ShowMessageEvent != null)
                            {
                                StringBuilder sbMessage = new StringBuilder();
                                sbMessage.Append(Strings.TalExecuteAgain);
                                sbMessage.Append(Environment.NewLine);
                                sbMessage.Append(lastTalExecutionResult == OperationStateData.TalExecutionResultEnum.Success ?
                                    Strings.TalExecuteLastSuccess : Strings.TalExecuteLastSkipped);

                                if (!ShowMessageEvent.Invoke(cts, sbMessage.ToString(), false, true))
                                {
                                    executeTal = false;
                                }
                            }
                        }

                        if (executeTal)
                        {
                            sbResult.AppendLine(Strings.ExecutingTal);
                            UpdateStatus(sbResult.ToString());
                            log.InfoFormat(CultureInfo.InvariantCulture, "Executing TAL");
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.TalExecuting);
                            CancellationToken token = cts?.Token ?? CancellationToken.None;
                            IPsdzTal executeTalResult = ProgrammingService.Psdz.TalExecutionService.ExecuteTal(PsdzContext.Connection, PsdzContext.Tal,
                                null, psdzVin, PsdzContext.FaTarget, talExecutionSettings, PsdzContext.PathToBackupData, token);
                            log.Info("Execute Tal result:");
                            log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", executeTalResult.AsXml.Length);
                            log.InfoFormat(CultureInfo.InvariantCulture, " State: {0}", executeTalResult.TalExecutionState);
                            log.InfoFormat(CultureInfo.InvariantCulture, " Ecus: {0}", executeTalResult.AffectedEcus.Count());
                            foreach (IPsdzEcuIdentifier ecuIdentifier in executeTalResult.AffectedEcus)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                    ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                            }
                            if (!IsTalExecutionStateOk(executeTalResult.TalExecutionState, true))
                            {
                                FinishTalExecutionState(cts, true);
                                talExecutionFailed = true;
                                log.Error(executeTalResult.AsXml);
                                sbResult.AppendLine(Strings.TalExecuteError);
                                if (executeTalResult.FailureCauses != null)
                                {
                                    foreach (IPsdzFailureCause failureCause in executeTalResult.FailureCauses)
                                    {
                                        if (!string.IsNullOrEmpty(failureCause.Message))
                                        {
                                            sbResult.AppendLine(failureCause.Message);
                                        }
                                    }
                                }

                                UpdateStatus(sbResult.ToString());
                            }
                            else
                            {
                                FinishTalExecutionState(cts);
                                if (!IsTalExecutionStateOk(executeTalResult.TalExecutionState))
                                {
                                    log.Info(executeTalResult.AsXml);
                                    sbResult.AppendLine(Strings.TalExecuteWarning);
                                }
                                else
                                {
                                    sbResult.AppendLine(Strings.TalExecuteOk);
                                }

                                UpdateStatus(sbResult.ToString());
                            }
                        }
                        else
                        {
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.TalExecuting, true);
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        if (RegisterGroup == PsdzDatabase.SwiRegisterGroup.HwDeinstall)
                        {
                            PsdzContext.Tal = null;
                            if (!backupFailed)
                            {
                                PsdzContext.AddIdividualDataFilesToPuk();
                            }

                            RegisterGroup = PsdzDatabase.SwiRegisterGroup.HwInstall;
                            UpdateReplaceOperationState();
                            RestoreReplaceOperationState();
                            SaveOperationState();

                            if (ShowMessageEvent != null)
                            {
                                if (!ShowMessageEvent.Invoke(cts, Strings.TalHwDeinstallFinished, true, true))
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecuteContinue aborted");
                                    return false;
                                }
                            }

                            sbResult.AppendLine(Strings.ExecutingVehicleFuncFinished);
                            UpdateStatus(sbResult.ToString());
                            return true;
                        }

                        for (int talIdx = 0; talIdx < 2; talIdx++)
                        {
                            if (talIdx == 0)
                            {
                                if (backupFailed)
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "Backup failed, not restoring");
                                    continue;
                                }
                            }
                            else
                            {
                                if (RegisterGroup != PsdzDatabase.SwiRegisterGroup.HwInstall)
                                {
                                    break;
                                }

                                log.InfoFormat(CultureInfo.InvariantCulture, "Checking IDR in PUK");
                                if (!PsdzContext.HasIndividualDataFilesInPuk())
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "No IDR in PUK present");
                                    break;
                                }

                                PsdzContext.RemoveBackupData();
                                if (!PsdzContext.DownloadIndividualDataFromPuk())
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "Getting IDR from PUK failed");
                                    break;
                                }

                                log.InfoFormat(CultureInfo.InvariantCulture, "IDR in PUK found");
                                sbResult.AppendLine(Strings.BackupDataRestored);
                                UpdateStatus(sbResult.ToString());
                            }

                            sbResult.AppendLine(Strings.ExecutingRestoreTal);
                            UpdateStatus(sbResult.ToString());

                            if (!CheckVoltage(cts, sbResult))
                            {
                                sbResult.AppendLine(Strings.BatteryVoltageReadError);
                                UpdateStatus(sbResult.ToString());
                                return false;
                            }

                            log.InfoFormat(CultureInfo.InvariantCulture, "Generating restore TAL");
                            IPsdzTal psdzRestoreTal = ProgrammingService.Psdz.IndividualDataRestoreService.GenerateRestoreTal(PsdzContext.Connection, PsdzContext.PathToBackupData, PsdzContext.Tal, PsdzContext.TalFilter);
                            if (psdzRestoreTal == null)
                            {
                                sbResult.AppendLine(Strings.TalGenerationFailed);
                                UpdateStatus(sbResult.ToString());
                            }
                            else
                            {
                                PsdzContext.IndividualDataRestoreTal = psdzRestoreTal;
                                log.InfoFormat(CultureInfo.InvariantCulture, "Executing restore TAL");
                                StartTalExecutionState(OperationStateData.TalExecutionStateEnum.RestoreTalExecuting);
                                IPsdzTal restoreTalResult = ProgrammingService.Psdz.IndividualDataRestoreService.ExecuteAsyncRestoreTal(
                                    PsdzContext.Connection, PsdzContext.IndividualDataRestoreTal, null, PsdzContext.FaTarget, psdzVin, talExecutionSettings);
                                if (restoreTalResult == null)
                                {
                                    log.ErrorFormat("Execute restore TAL failed");
                                    sbResult.AppendLine(Strings.TalExecuteError);
                                    UpdateStatus(sbResult.ToString());
                                    return false;
                                }

                                log.Info("Restore Tal result:");
                                log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", restoreTalResult.AsXml.Length);
                                log.InfoFormat(CultureInfo.InvariantCulture, " State: {0}", restoreTalResult.TalExecutionState);
                                log.InfoFormat(CultureInfo.InvariantCulture, " Ecus: {0}", restoreTalResult.AffectedEcus.Count());
                                foreach (IPsdzEcuIdentifier ecuIdentifier in restoreTalResult.AffectedEcus)
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                        ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                                }

                                if (!IsTalExecutionStateOk(restoreTalResult.TalExecutionState, true))
                                {
                                    FinishTalExecutionState(cts, true);
                                    talExecutionFailed = true;
                                    log.Error(restoreTalResult.AsXml);
                                    sbResult.AppendLine(Strings.TalExecuteError);
                                    UpdateStatus(sbResult.ToString());
                                }
                                else
                                {
                                    FinishTalExecutionState(cts);
                                    if (!IsTalExecutionStateOk(restoreTalResult.TalExecutionState))
                                    {
                                        log.Info(restoreTalResult.AsXml);
                                        sbResult.AppendLine(Strings.TalExecuteWarning);
                                    }
                                    else
                                    {
                                        sbResult.AppendLine(Strings.TalExecuteOk);
                                    }

                                    UpdateStatus(sbResult.ToString());
                                }

                                if (!CheckVoltage(cts, sbResult))
                                {
                                    sbResult.AppendLine(Strings.BatteryVoltageReadError);
                                    UpdateStatus(sbResult.ToString());
                                    return false;
                                }

                                CacheClearRequired = true;
                                cts?.Token.ThrowIfCancellationRequested();
                            }
                        }

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Updating TSL");
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.TslUpdateExecuting);
                            ProgrammingService.Psdz.ProgrammingService.TslUpdate(PsdzContext.Connection, true, PsdzContext.SvtActual, PsdzContext.Sollverbauung.Svt);
                            FinishTalExecutionState(cts);
                            sbResult.AppendLine(Strings.TslUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            FinishTalExecutionState(cts, true);
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "Tsl update failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.TslUpdateFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Writing ILevels");
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.WriteILevelExecuting);
                            ProgrammingService.Psdz.VcmService.WriteIStufen(PsdzContext.Connection, PsdzContext.IstufeShipment, PsdzContext.IstufeLast, PsdzContext.IstufeCurrent);
                            FinishTalExecutionState(cts);
                            sbResult.AppendLine(Strings.ILevelUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            FinishTalExecutionState(cts, true);
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "Write ILevel failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.ILevelUpdateFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Writing ILevels backup");
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.WriteILevelBackupExecuting);
                            ProgrammingService.Psdz.VcmService.WriteIStufenToBackup(PsdzContext.Connection, PsdzContext.IstufeShipment, PsdzContext.IstufeLast, PsdzContext.IstufeCurrent);
                            FinishTalExecutionState(cts);
                            sbResult.AppendLine(Strings.ILevelBackupUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            FinishTalExecutionState(cts, true);
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "Write ILevel backup failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.ILevelBackupFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        log.InfoFormat(CultureInfo.InvariantCulture, "Updating PIA master");
                        StartTalExecutionState(OperationStateData.TalExecutionStateEnum.UpdatePiaMasterExecuting);
                        IPsdzResponse piaResponse = ProgrammingService.Psdz.EcuService.UpdatePiaPortierungsmaster(PsdzContext.Connection, PsdzContext.SvtActual);
                        log.ErrorFormat(CultureInfo.InvariantCulture, "PIA master update Success={0}, Cause={1}",
                            piaResponse.IsSuccessful, piaResponse.Cause);

                        if (piaResponse.IsSuccessful)
                        {
                            FinishTalExecutionState(cts);
                            sbResult.AppendLine(Strings.PiaMasterUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        else
                        {
                            FinishTalExecutionState(cts, true);
                            talExecutionFailed = true;
                            sbResult.AppendLine(Strings.PiaMasterUpdateFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Writing FA");
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.WriteFaExecuting);
                            ProgrammingService.Psdz.VcmService.WriteFa(PsdzContext.Connection, PsdzContext.FaTarget);
                            FinishTalExecutionState(cts);
                            sbResult.AppendLine(Strings.FaWritten);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            FinishTalExecutionState(cts, true);
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "FA write failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.FaWriteFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Writing FA backup");
                            StartTalExecutionState(OperationStateData.TalExecutionStateEnum.WriteFaBackupExecuting);
                            ProgrammingService.Psdz.VcmService.WriteFaToBackup(PsdzContext.Connection, PsdzContext.FaTarget);
                            FinishTalExecutionState(cts);
                            sbResult.AppendLine(Strings.FaBackupWritten);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            FinishTalExecutionState(cts, true);
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "FA backup write failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.FaBackupWriteFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        if (!talExecutionFailed)
                        {
                            if (RegisterGroup == PsdzDatabase.SwiRegisterGroup.HwInstall)
                            {
                                ResetOperationState();
                            }
                            else
                            {
                                if (ShowMessageEvent != null)
                                {
                                    if (!ShowMessageEvent.Invoke(cts, Strings.TalExecutionOkMessage, false, true))
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecutionOkMessage Delete backup");
                                        StartTalExecutionState(OperationStateData.TalExecutionStateEnum.None);
                                        PsdzContext.RemoveBackupData();
                                    }
                                    else
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecutionOkMessage Keep backup");
                                        StartTalExecutionState(OperationStateData.TalExecutionStateEnum.Finished);
                                    }
                                }
                            }

                            // finally reset TAL
                            PsdzContext.Tal = null;
                            RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                            UpdateOptions(null);
                        }
                        else
                        {
                            if (ShowMessageEvent != null)
                            {
                                if (!ShowMessageEvent.Invoke(cts, Strings.TalExecutionFailMessage, false, true))
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecutionFailMessage No retry");
                                    StartTalExecutionState(OperationStateData.TalExecutionStateEnum.None);
                                    PsdzContext.RemoveBackupData();
                                }
                                else
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecutionFailMessage Retry ");
                                    StartTalExecutionState(OperationStateData.TalExecutionStateEnum.Finished);
                                }
                            }
                        }
                    }
                    finally
                    {
                        CacheClearRequired = true;
                        CacheResponseType = CacheType.FuncAddress;
                        secureCodingConfig.BackendNcdCalculationEtoEnum = backendNcdCalculationEtoEnumOld;
                        if (Directory.Exists(secureCodingPath))
                        {
                            try
                            {
                                Directory.Delete(secureCodingPath, true);
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "Directory exception: {0}", ex.Message);
                            }
                        }
                    }

                    sbResult.AppendLine(Strings.ExecutingVehicleFuncFinished);
                    UpdateStatus(sbResult.ToString());
                    return true;
                }

                restartBuildTal:
                if (!CheckVoltage(cts, sbResult))
                {
                    sbResult.AppendLine(Strings.BatteryVoltageReadError);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                bool bModifyFa = operationType == OperationType.BuildTalModFa;
                List<int> diagAddrList = new List<int>();
                RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                if (bModifyFa)
                {
                    foreach (OptionsItem optionsItem in SelectedOptions)
                    {
                        PsdzDatabase.SwiRegisterGroup swiRegisterGroup = PsdzDatabase.GetSwiRegisterGroup(optionsItem.SwiRegisterEnum);
                        switch (swiRegisterGroup)
                        {
                            case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                            case PsdzDatabase.SwiRegisterGroup.HwInstall:
                            {
                                if (optionsItem.EcuInfo == null)
                                {
                                    break;
                                }

                                if (RegisterGroup == PsdzDatabase.SwiRegisterGroup.Modification)
                                {
                                    RegisterGroup = swiRegisterGroup;
                                }

                                if (RegisterGroup == swiRegisterGroup)
                                {
                                    int diagAddress = (int)optionsItem.EcuInfo.Address;
                                    if (!diagAddrList.Contains(diagAddress))
                                    {
                                        diagAddrList.Add(diagAddress);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                IPsdzTalFilter psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
                // disable backup
                psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.FscBackup }, TalFilterOptions.MustNot, psdzTalFilter);

                switch (RegisterGroup)
                {
                    case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                        psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(ProgrammingUtils.EnabledTaCategories, TalFilterOptions.MustNot, psdzTalFilter);
                        psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.IdBackup }, TalFilterOptions.Allowed, psdzTalFilter);
                        break;

                    default:
                        if (bModifyFa)
                        {   // enable deploy
                            psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.CdDeploy }, TalFilterOptions.Must, psdzTalFilter);
                            log.InfoFormat(CultureInfo.InvariantCulture, "TAL flashing disabled: {0}", DisableTalFlash);
                            if (DisableTalFlash)
                            {
                                psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.BlFlash, TaCategories.GatewayTableDeploy, TaCategories.SwDeploy }, TalFilterOptions.MustNot, psdzTalFilter);
                            }
                        }
                        break;
                }

                PsdzContext.SetTalFilter(psdzTalFilter);

                IPsdzTalFilter psdzTalFilterIndividual = ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
                PsdzContext.SetTalFilterForIndividualDataTal(psdzTalFilterIndividual);

                switch (RegisterGroup)
                {
                    case PsdzDatabase.SwiRegisterGroup.HwInstall:
                        UpdateTalFilterForSelectedEcus(new[] { TaCategories.CdDeploy }, diagAddrList.ToArray(), TalFilterOptions.Must);
                        if (bModifyFa)
                        {
                            UpdateTalFilterForSelectedEcus(new[] { TaCategories.IdBackup, TaCategories.IdRestore }, diagAddrList.ToArray(), TalFilterOptions.MustNot);
                        }

                        UpdateTalFilterTalForECUsWithIDRClassicState(diagAddrList.ToArray());
                        break;

                    case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                        UpdateTalFilterForSelectedEcus(new[] { TaCategories.HwDeinstall }, diagAddrList.ToArray(), TalFilterOptions.Must);
                        break;

                }

                if (PsdzContext.TalFilter != null)
                {
                    log.Info("TalFilter:");
                    log.Info(PsdzContext.TalFilter.AsXml);
                }

                if (PsdzContext.TalFilterForIndividualDataTal != null)
                {
                    log.Info("TalFilterIndividaul:");
                    log.Info(PsdzContext.TalFilterForIndividualDataTal.AsXml);
                }

                if (PsdzContext.TalFilterForECUWithIDRClassicState != null)
                {
                    log.Info("TalFilterIDRClassic:");
                    log.Info(PsdzContext.TalFilterForECUWithIDRClassicState.AsXml);
                }

                IPsdzIstufenTriple iStufenTriple = ProgrammingService.Psdz.VcmService.GetIStufenTripleActual(PsdzContext.Connection);
                if (iStufenTriple == null)
                {
                    sbResult.AppendLine(Strings.ReadILevelFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetIstufen(iStufenTriple);
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel: Current={0}, Last={1}, Shipment={2}",
                    iStufenTriple.Current, iStufenTriple.Last, iStufenTriple.Shipment);

                if (!PsdzContext.SetPathToBackupData(psdzVin.Value))
                {
                    sbResult.AppendLine(Strings.CreateBackupPathFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzStandardFa standardFa = ProgrammingService.Psdz.VcmService.GetStandardFaActual(PsdzContext.Connection);
                if (standardFa == null)
                {
                    sbResult.AppendLine(Strings.ReadFaFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzFa psdzFa = ProgrammingService.Psdz.ObjectBuilder.BuildFa(standardFa, psdzVin.Value);
                if (psdzFa == null)
                {
                    sbResult.AppendLine(Strings.ReadFaFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetFaActual(psdzFa);
                log.Info("FA current:");
                log.Info(psdzFa.AsXml);
                cts?.Token.ThrowIfCancellationRequested();

                if (bModifyFa)
                {
                    IFa ifaActual = ProgrammingUtils.BuildFa(PsdzContext.FaActual);
                    IFa ifaTarget = ProgrammingUtils.BuildFa(PsdzContext.FaTarget);
                    string compareFa = ProgrammingUtils.CompareFa(ifaActual, ifaTarget);
                    if (!string.IsNullOrEmpty(compareFa))
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "Compare FA: {0}", compareFa);
                    }
                }
                else
                {   // reset target fa
                    PsdzContext.SetFaTarget(psdzFa);
                }

                ProgrammingService.PsdzDatabase.ResetXepRules();

                IEnumerable<IPsdzIstufe> psdzIstufes = ProgrammingService.Psdz.LogicService.GetPossibleIntegrationLevel(PsdzContext.FaTarget);
                if (psdzIstufes == null)
                {
                    sbResult.AppendLine(Strings.ReadILevelFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetPossibleIstufenTarget(psdzIstufes);
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevels: {0}", psdzIstufes.Count());
                foreach (IPsdzIstufe iStufe in psdzIstufes.OrderBy(x => x))
                {
                    if (iStufe.IsValid)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " ILevel: {0}", iStufe.Value);
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                string latestIstufeTarget = PsdzContext.LatestPossibleIstufeTarget;
                if (string.IsNullOrEmpty(latestIstufeTarget))
                {
                    sbResult.AppendLine(Strings.NoTargetILevel);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel Latest: {0}", latestIstufeTarget);

                IPsdzIstufe psdzIstufeShip = ProgrammingService.Psdz.ObjectBuilder.BuildIstufe(PsdzContext.IstufeShipment);
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel Ship: {0}", psdzIstufeShip.Value);

                IPsdzIstufe psdzIstufeTarget = ProgrammingService.Psdz.ObjectBuilder.BuildIstufe(bModifyFa ? PsdzContext.IstufeCurrent : latestIstufeTarget);
                PsdzContext.VecInfo.TargetILevel = psdzIstufeTarget.Value;
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel Target: {0}", psdzIstufeTarget.Value);

                IPsdzIstufe psdzIstufeLatest = ProgrammingService.Psdz.ObjectBuilder.BuildIstufe(latestIstufeTarget);
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel Latest: {0}", psdzIstufeLatest.Value);
                cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(Strings.DetectingInstalledEcus);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting ECU list");
                IEnumerable<IPsdzEcuIdentifier> psdzEcuIdentifiers = ProgrammingService.Psdz.MacrosService.GetInstalledEcuList(PsdzContext.FaActual, psdzIstufeShip);
                if (psdzEcuIdentifiers == null)
                {
                    sbResult.AppendLine(Strings.DetectInstalledEcusFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "EcuIds: {0}", psdzEcuIdentifiers.Count());
                foreach (IPsdzEcuIdentifier ecuIdentifier in psdzEcuIdentifiers)
                {
                    if (ecuIdentifier == null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                            ecuIdentifier.BaseVariant ?? string.Empty, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress?.Offset);
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                // From GetVehicleSvtUsingPsdz
                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting Svt");
                IPsdzStandardSvt psdzStandardSvt = ProgrammingService.Psdz.EcuService.RequestSvt(PsdzContext.Connection, psdzEcuIdentifiers);
                if (psdzStandardSvt == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Requesting SVT failed");
                    sbResult.AppendLine(Strings.DetectInstalledEcusFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Svt Ecus: {0}", psdzStandardSvt.Ecus?.Count());

                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting names");
                IPsdzStandardSvt psdzStandardSvtNames = ProgrammingService.Psdz.LogicService.FillBntnNamesForMainSeries(PsdzContext.Connection.TargetSelector.Baureihenverbund, psdzStandardSvt);
                if (psdzStandardSvtNames == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Requesting names failed");
                    sbResult.AppendLine(Strings.DetectInstalledEcusFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Svt Ecus names: {0}", psdzStandardSvtNames.Ecus.Count());
                foreach (IPsdzEcu ecu in psdzStandardSvtNames.Ecus)
                {
                    if (ecu != null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " Variant: BaseVar={0}, Var={1}, Name={2}, Serial={3}",
                            ecu.BaseVariant ?? string.Empty, ecu.EcuVariant ?? string.Empty, ecu.BnTnName ?? string.Empty, ecu.SerialNumber ?? string.Empty);
                        if (ecu.EcuStatusInfo != null)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "  Status: Individual={0}",
                                ecu.EcuStatusInfo.HasIndividualData);
                        }
                    }
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Building SVT");
                PsdzContext.SetSvtCurrent(ProgrammingService, psdzStandardSvtNames, psdzVin.Value);
                if (PsdzContext.SvtActual == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Building SVT failed");
                    sbResult.AppendLine(Strings.DetectInstalledEcusFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                cts?.Token.ThrowIfCancellationRequested();

                ProgrammingService.PsdzDatabase.LinkSvtEcus(PsdzContext.DetectVehicle.EcuListPsdz, PsdzContext.SvtActual);
                List<PsdzDatabase.EcuInfo> individualEcus = PsdzContext.GetEcuList(true);
                if (individualEcus != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Individual Ecus: {0}", individualEcus.Count());
                    foreach (PsdzDatabase.EcuInfo ecuInfo in individualEcus)
                    {
                        if (ecuInfo != null)
                        {
                            log.Info(ecuInfo.ToString(clientContext.Language));
                        }
                    }
                }

                ProgrammingService.PsdzDatabase.GetEcuVariants(PsdzContext.DetectVehicle.EcuListPsdz);
                if (!PsdzContext.UpdateVehicle(ProgrammingService))
                {
                    sbResult.AppendLine(Strings.UpdateVehicleDataFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ProgrammingService.PsdzDatabase.ResetXepRules();
                log.InfoFormat(CultureInfo.InvariantCulture, "Getting ECU variants");
                ProgrammingService.PsdzDatabase.GetEcuVariants(PsdzContext.DetectVehicle.EcuListPsdz, PsdzContext.VecInfo);
                log.InfoFormat(CultureInfo.InvariantCulture, "Ecu variants: {0}", PsdzContext.DetectVehicle.EcuListPsdz.Count());
                foreach (PsdzDatabase.EcuInfo ecuInfo in PsdzContext.DetectVehicle.EcuListPsdz)
                {
                    if (ecuInfo != null)
                    {
                        log.Info(ecuInfo.ToString(clientContext.Language));
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                if (operationType == OperationType.CreateOptions)
                {
                    CheckVoltage(cts, sbResult, false, true);

                    ProgrammingService.PsdzDatabase.ReadSwiRegister(PsdzContext.VecInfo);
                    if (ProgrammingService.PsdzDatabase.SwiRegisterTree != null)
                    {
                        string treeText = ProgrammingService.PsdzDatabase.SwiRegisterTree.ToString(clientContext.Language);
                        if (!string.IsNullOrEmpty(treeText))
                        {
                            log.Info(Environment.NewLine + "Swi tree:" + Environment.NewLine + treeText);
                        }

                        Dictionary<PsdzDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict = new Dictionary<PsdzDatabase.SwiRegisterEnum, List<OptionsItem>>();
                        foreach (OptionType optionType in _optionTypes)
                        {
                            PsdzDatabase.SwiRegisterEnum swiRegisterEnum = optionType.SwiRegisterEnum;
                            optionType.ClientContext = clientContext;
                            optionType.SwiRegister = ProgrammingService.PsdzDatabase.FindNodeForRegister(swiRegisterEnum, PsdzContext.VecInfo);

                            switch (swiRegisterEnum)
                            {
                                case PsdzDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement:
                                case PsdzDatabase.SwiRegisterEnum.EcuReplacementAfterReplacement:
                                {
                                    bool individualOnly = swiRegisterEnum == PsdzDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement;
                                    List<PsdzDatabase.EcuInfo> ecuList = PsdzContext.GetEcuList(individualOnly);
                                    if (ecuList != null)
                                    {
                                        List<OptionsItem> optionsItems = new List<OptionsItem>();
                                        foreach (PsdzDatabase.EcuInfo ecuInfo in ecuList)
                                        {
                                            IEcuLogisticsEntry ecuLogisticsEntry = PsdzContext.GetEcuLogisticsEntry((int) ecuInfo.Address);
                                            optionsItems.Add(new OptionsItem(swiRegisterEnum, ecuInfo, ecuLogisticsEntry, clientContext));
                                        }
                                        optionsDict.Add(swiRegisterEnum, optionsItems);
                                    }
                                    break;
                                }

                                default:
                                {
                                    List<PsdzDatabase.SwiAction> swiActions = ProgrammingService.PsdzDatabase.GetSwiActionsForRegister(swiRegisterEnum, true, PsdzContext.VecInfo);
                                    if (swiActions != null)
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "Swi actions: {0}", optionType.Name ?? string.Empty);
                                        List<OptionsItem> optionsItems = new List<OptionsItem>();
                                        foreach (PsdzDatabase.SwiAction swiAction in swiActions)
                                        {
                                            bool testModuleValid = false;
                                            if (swiAction.SwiInfoObjs != null)
                                            {
                                                foreach (PsdzDatabase.SwiInfoObj infoInfoObj in swiAction.SwiInfoObjs)
                                                {
                                                    if (infoInfoObj.LinkType == PsdzDatabase.SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                                                    {
                                                        string moduleName = infoInfoObj.ModuleName;
                                                        PsdzDatabase.TestModuleData testModuleData = ProgrammingService.PsdzDatabase.GetTestModuleData(moduleName);
                                                        if (testModuleData != null)
                                                        {
                                                            testModuleValid = true;
                                                            break;
                                                        }

                                                        log.ErrorFormat(CultureInfo.InvariantCulture, "Ignoring invalid test module: {0}", moduleName ?? string.Empty);
                                                    }
                                                }
                                            }

                                            if (testModuleValid)
                                            {
                                                log.Info(swiAction.ToString(clientContext.Language));
                                                optionsItems.Add(new OptionsItem(swiRegisterEnum, swiAction, clientContext));
                                            }
                                        }

                                        optionsDict.Add(swiRegisterEnum, optionsItems);
                                    }
                                    break;
                                }
                            }
                        }

                        UpdateOptions(optionsDict);
                        LoadOperationState();

                        bool restoreReplaceOperation = false;
                        bool restoreTalOperation = false;
                        bool removeBackupData = true;

                        switch (OperationState.Operation)
                        {
                            case OperationStateData.OperationEnum.HwInstall:
                                log.InfoFormat(CultureInfo.InvariantCulture, "Hw replace operation active");
                                if (OperationState.DiagAddrList == null || OperationState.DiagAddrList.Count == 0)
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "No replace addresses selected");
                                    break;
                                }

                                if (ShowMessageEvent != null)
                                {
                                    if (!ShowMessageEvent.Invoke(cts, Strings.HwReplaceContinue, false, true))
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "ShowMessageEvent HwReplaceContinue aborted");
                                    }
                                    else
                                    {
                                        restoreReplaceOperation = true;
                                    }
                                }
                                break;

                            case OperationStateData.OperationEnum.Modification:
                                log.InfoFormat(CultureInfo.InvariantCulture, "Modification operation active");
                                if (OperationState.SelectedOptionList == null || OperationState.SelectedOptionList.Count == 0)
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "No modification options selected");
                                    break;
                                }

                                if (string.IsNullOrEmpty(OperationState.BackupTargetFA))
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "No backup TAL created");
                                    break;
                                }

                                if (ShowMessageEvent != null)
                                {
                                    if (!ShowMessageEvent.Invoke(cts, Strings.TalOperationContinue, false, true))
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalOperationContinue aborted");
                                    }
                                    else
                                    {
                                        restoreTalOperation = true;
                                    }
                                }
                                break;
                        }

                        if (restoreReplaceOperation)
                        {
                            RestoreReplaceOperationState();
                        }
                        else if (restoreTalOperation)
                        {
                            if (RestoreTalOperationState())
                            {
                                removeBackupData = false;
                            }
                        }
                        else
                        {
                            ResetOperationState();
                            UpdateOptions(optionsDict);
                        }

                        if (removeBackupData)
                        {
                            PsdzContext.RemoveBackupData();
                        }
                    }

                    sbResult.AppendLine(Strings.ExecutingVehicleFuncFinished);
                    UpdateStatus(sbResult.ToString());
                    return true;
                }

                PsdzContext.Tal = null;
                log.InfoFormat(CultureInfo.InvariantCulture, "Reading ECU Ids");
                IPsdzReadEcuUidResultCto psdzReadEcuUid = ProgrammingService.Psdz.SecurityManagementService.readEcuUid(PsdzContext.Connection, psdzEcuIdentifiers, PsdzContext.SvtActual);
                if (psdzReadEcuUid != null)
                {
                    if (psdzReadEcuUid.EcuUids != null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "EcuUids: {0}", psdzReadEcuUid.EcuUids.Count);
                        foreach (KeyValuePair<IPsdzEcuIdentifier, IPsdzEcuUidCto> ecuUid in psdzReadEcuUid.EcuUids)
                        {
                            if (ecuUid.Value != null)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Uid={3}",
                                    ecuUid.Key?.BaseVariant ?? string.Empty, ecuUid.Key?.DiagAddrAsInt, ecuUid.Key?.DiagnosisAddress?.Offset, ecuUid.Value?.EcuUid ?? string.Empty);
                            }
                        }
                    }

                    if (psdzReadEcuUid.FailureResponse != null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "EcuUid failures: {0}", psdzReadEcuUid.FailureResponse.Count());
                        foreach (IPsdzEcuFailureResponseCto failureResponse in psdzReadEcuUid.FailureResponse)
                        {
                            if (failureResponse != null)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, " Fail: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Cause={3}",
                                    failureResponse.EcuIdentifierCto?.BaseVariant ?? string.Empty, failureResponse.EcuIdentifierCto?.DiagAddrAsInt,
                                    failureResponse.EcuIdentifierCto?.DiagnosisAddress?.Offset, failureResponse.Cause?.Description ?? string.Empty);
                            }
                        }
                    }
                }

                cts?.Token.ThrowIfCancellationRequested();

                log.InfoFormat(CultureInfo.InvariantCulture, "Reading status");
                IPsdzReadStatusResultCto psdzReadStatusResult = ProgrammingService.Psdz.SecureFeatureActivationService.ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum.ALL_FEATURES, PsdzContext.Connection, PsdzContext.SvtActual, psdzEcuIdentifiers, true, 3, 100);
                if (psdzReadStatusResult != null)
                {
                    if (psdzReadStatusResult.Failures != null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "Status failures: {0}", psdzReadStatusResult.Failures.Count());
                        foreach (IPsdzEcuFailureResponseCto failureResponse in psdzReadStatusResult.Failures)
                        {
                            if (failureResponse != null)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, " Fail: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Cause={3}",
                                    failureResponse.EcuIdentifierCto?.BaseVariant ?? string.Empty, failureResponse.EcuIdentifierCto?.DiagAddrAsInt, failureResponse.EcuIdentifierCto?.DiagnosisAddress?.Offset,
                                    failureResponse.Cause?.Description ?? string.Empty);
                            }
                        }
                    }

                    if (psdzReadStatusResult.FeatureStatusSet != null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "Status features: {0}", psdzReadStatusResult.FeatureStatusSet.Count());
                        foreach (IPsdzFeatureLongStatusCto featureLongStatus in psdzReadStatusResult.FeatureStatusSet)
                        {
                            if (featureLongStatus != null)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, " Feature: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Status={3}, Token={4}",
                                    featureLongStatus.EcuIdentifierCto?.BaseVariant ?? string.Empty, featureLongStatus.EcuIdentifierCto?.DiagAddrAsInt, featureLongStatus.EcuIdentifierCto?.DiagnosisAddress?.Offset,
                                    featureLongStatus.FeatureStatusEto, featureLongStatus.TokenId ?? string.Empty);
                            }
                        }
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzTalFilter talFilterFlash = new PsdzTalFilter();
                switch (RegisterGroup)
                {
                    case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                        talFilterFlash = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForSelectedEcus(new[] { TaCategories.HwInstall, TaCategories.HwDeinstall }, diagAddrList.ToArray(), TalFilterOptions.Must, talFilterFlash);
                        break;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting planned construction");
                IPsdzSollverbauung psdzSollverbauung = null;
                try
                {
                    try
                    {
                        psdzSollverbauung = ProgrammingService.Psdz.LogicService.GenerateSollverbauungGesamtFlash(PsdzContext.Connection, psdzIstufeTarget, psdzIstufeShip, PsdzContext.SvtActual, PsdzContext.FaTarget, talFilterFlash);
                    }
                    catch (PsdzRuntimeException ex)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "VehicleFunctions Planned construction runtime Exception: {0}", ex.Message);
                        if (ex.Message.Contains("KIS"))
                        {
                            if (psdzIstufeLatest != psdzIstufeTarget)
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, "VehicleFunctions Retrying planned construction with: {0}", latestIstufeTarget);
                                psdzSollverbauung = ProgrammingService.Psdz.LogicService.GenerateSollverbauungGesamtFlash(PsdzContext.Connection, psdzIstufeLatest, psdzIstufeShip, PsdzContext.SvtActual, PsdzContext.FaTarget, talFilterFlash);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "VehicleFunctions Planned construction general Exception: {0}", ex.Message);
                }

                if (psdzSollverbauung == null)
                {
                    sbResult.AppendLine(Strings.RequestedPlannedConstructionFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetSollverbauung(ProgrammingService, psdzSollverbauung);
                if (psdzSollverbauung.PsdzOrderList != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Target construction: Count={0}, Units={1}",
                        psdzSollverbauung.PsdzOrderList.BntnVariantInstances?.Length, psdzSollverbauung.PsdzOrderList.NumberOfUnits);
                    foreach (IPsdzEcuVariantInstance bntnVariant in psdzSollverbauung.PsdzOrderList.BntnVariantInstances)
                    {
                        if (bntnVariant != null && bntnVariant.Ecu != null)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Variant: BaseVar={0}, Var={1}, Name={2}",
                                bntnVariant.Ecu.BaseVariant ?? string.Empty, bntnVariant.Ecu.EcuVariant ?? string.Empty, bntnVariant.Ecu.BnTnName ?? string.Empty);
                        }
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(Strings.RequestingEcuContext);
                UpdateStatus(sbResult.ToString());
                IEnumerable<IPsdzEcuContextInfo> psdzEcuContextInfos = null;
                for (int retry = 0; retry < 3; retry++)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Requesting Ecu context retry: {0}", retry);
                    psdzEcuContextInfos = ProgrammingService.Psdz.EcuService.RequestEcuContextInfos(PsdzContext.Connection, psdzEcuIdentifiers);
                    if (psdzEcuContextInfos != null)
                    {
                        break;
                    }

                    log.ErrorFormat(CultureInfo.InvariantCulture, "Requesting Ecu context failed");

                    if (!WaitForEmptyVehicleQueue())
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "Requesting Ecu context failed, aborting");
                        break;
                    }

                    log.WarnFormat(CultureInfo.InvariantCulture, "Requesting Ecu context failed, retry: {0}", retry);
                }

                if (psdzEcuContextInfos == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Requesting Ecu context failed");
                    sbResult.AppendLine(Strings.RequestEcuContextFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Ecu contexts: {0}", psdzEcuContextInfos.Count());
                foreach (IPsdzEcuContextInfo ecuContextInfo in psdzEcuContextInfos)
                {
                    if (ecuContextInfo != null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, " Ecu context: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, ManuDate={3}, PrgDate={4}, PrgCnt={5}, FlashCnt={6}, FlashRemain={7}",
                            ecuContextInfo.EcuId?.BaseVariant ?? string.Empty, ecuContextInfo.EcuId?.DiagAddrAsInt, ecuContextInfo.EcuId?.DiagnosisAddress?.Offset,
                            ecuContextInfo?.ManufacturingDate ?? DateTime.MinValue, ecuContextInfo?.LastProgrammingDate ?? DateTime.MinValue,
                            ecuContextInfo.ProgramCounter, ecuContextInfo.PerformedFlashCycles, ecuContextInfo.RemainingFlashCycles);
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(Strings.SwtAction);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting Swt action");
                IPsdzSwtAction psdzSwtAction = ProgrammingService.Psdz.ProgrammingService.RequestSwtAction(PsdzContext.Connection, true);
                if (psdzSwtAction == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Requesting Swt action failed");
                    sbResult.AppendLine(Strings.SwtActionFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SwtAction = psdzSwtAction;
                if (psdzSwtAction?.SwtEcus != null)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.SwtActionCount, psdzSwtAction.SwtEcus.Count()));
                    UpdateStatus(sbResult.ToString());

                    log.InfoFormat(CultureInfo.InvariantCulture, "Swt Ecus: {0}", psdzSwtAction.SwtEcus.Count());
                    foreach (IPsdzSwtEcu psdzSwtEcu in psdzSwtAction.SwtEcus)
                    {
                        if (psdzSwtEcu != null)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Ecu: Id={0}, Vin={1}, CertState={2}, SwSig={3}",
                                psdzSwtEcu.EcuIdentifier?.BaseVariant ?? string.Empty, psdzSwtEcu.Vin ?? string.Empty, psdzSwtEcu.RootCertState, psdzSwtEcu.SoftwareSigState);
                            if (psdzSwtEcu.SwtApplications != null)
                            {
                                foreach (IPsdzSwtApplication swtApplication in psdzSwtEcu.SwtApplications)
                                {
                                    if (swtApplication != null)
                                    {
                                        int fscLength = 0;
                                        if (swtApplication.Fsc != null)
                                        {
                                            fscLength = swtApplication.Fsc.Length;
                                        }
                                        log.InfoFormat(CultureInfo.InvariantCulture, " Fsc: Type={0}, State={1}, Length={2}",
                                            swtApplication.SwtType, swtApplication.FscState, fscLength);
                                    }
                                }
                            }
                        }
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(Strings.TalGenerating);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Generating TAL");
                IPsdzTal psdzTal = ProgrammingService.Psdz.LogicService.GenerateTal(PsdzContext.Connection, PsdzContext.SvtActual, psdzSollverbauung, PsdzContext.SwtAction, PsdzContext.TalFilter, PsdzContext.FaActual.Vin);
                if (psdzTal == null)
                {
                    sbResult.AppendLine(Strings.TalGenerationFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.Tal = psdzTal;
                log.Info("Tal:");
                log.Info(psdzTal.AsXml);
                log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", psdzTal.AsXml.Length);
                log.InfoFormat(CultureInfo.InvariantCulture, " State: {0}", psdzTal.TalExecutionState);
                if (psdzTal.AffectedEcus != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, " Ecus: {0}", psdzTal.AffectedEcus.Count());
                    foreach (IPsdzEcuIdentifier ecuIdentifier in psdzTal.AffectedEcus)
                    {
                        if (ecuIdentifier != null)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                ecuIdentifier.BaseVariant ?? string.Empty, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                        }
                    }
                }

                ISet<ProgrammingActionType> programmingActionsSum = new HashSet<ProgrammingActionType>();
                log.InfoFormat(CultureInfo.InvariantCulture, " Lines: {0}", psdzTal.TalLines.Count());
                foreach (IPsdzTalLine talLine in psdzTal.TalLines)
                {
                    if (talLine != null)
                    {
                        ISet<ProgrammingActionType> programmingActions = EcuProgrammingInfo.MapProgrammingActionType(talLine);
                        programmingActionsSum.AddRange(programmingActions);
                        log.InfoFormat(CultureInfo.InvariantCulture, "  Tal line: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, PrgActions={3}",
                            talLine.EcuIdentifier.BaseVariant, talLine.EcuIdentifier.DiagAddrAsInt, talLine.EcuIdentifier.DiagnosisAddress.Offset, programmingActions.ToStringItems());
                        log.InfoFormat(CultureInfo.InvariantCulture, " FscDeploy={0}, BlFlash={1}, IbaDeploy={2}, SwDeploy={3}, IdRestore={4}, SfaDeploy={5}, Cat={6}",
                            talLine.FscDeploy?.Tas?.Count(), talLine.BlFlash?.Tas?.Count(), talLine.IbaDeploy?.Tas?.Count(),
                            talLine.SwDeploy?.Tas?.Count(), talLine.IdRestore?.Tas?.Count(), talLine.SFADeploy?.Tas?.Count(),
                            talLine.TaCategories);
                        if (talLine.TaCategory != null && talLine.TaCategory.Tas != null)
                        {
                            foreach (IPsdzTa psdzTa in talLine.TaCategory.Tas)
                            {
                                if (psdzTa != null)
                                {
                                    log.InfoFormat(CultureInfo.InvariantCulture, "   SgbmId={0}, State={1}",
                                        psdzTa.SgbmId?.HexString ?? string.Empty, psdzTa.ExecutionState);
                                }
                            }
                        }
                    }
                }

                if (psdzTal.HasFailureCauses)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, " Failures: {0}", psdzTal.FailureCauses.Count());
                    foreach (IPsdzFailureCause failureCause in psdzTal.FailureCauses)
                    {
                        if (failureCause != null)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "  Failure cause: {0}", failureCause.Message ?? string.Empty);
                        }
                    }
                }
                cts?.Token.ThrowIfCancellationRequested();

                if (bModifyFa && RegisterGroup != PsdzDatabase.SwiRegisterGroup.HwDeinstall && programmingActionsSum.Contains(ProgrammingActionType.Programming))
                {
                    PsdzContext.Tal = null;
                    RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Modify FA TAL contains programming actions, TAL flash disabled: {0}", DisableTalFlash);
                    if (ShowMessageEvent != null && !DisableTalFlash)
                    {
                        string message = Strings.TalFlashOperation + Environment.NewLine + Strings.TalDisableFlash;
                        if (!ShowMessageEvent.Invoke(cts, message, false, true))
                        {
                            log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalFlashOperation aborted");
                            return false;
                        }

                        log.InfoFormat(CultureInfo.InvariantCulture, "TAL flash disabled");
                        DisableTalFlash = true;
                        goto restartBuildTal;
                    }

                    sbResult.AppendLine(Strings.TalFlashOperation);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine(Strings.TalBackupGenerating);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Generating backup TAL");
                IPsdzTal psdzBackupTal = ProgrammingService.Psdz.IndividualDataRestoreService.GenerateBackupTal(PsdzContext.Connection, PsdzContext.PathToBackupData, PsdzContext.Tal, PsdzContext.TalFilter);
                if (psdzBackupTal == null)
                {
                    PsdzContext.Tal = null;
                    RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                    sbResult.AppendLine(Strings.TalGenerationFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.IndividualDataBackupTal = psdzBackupTal;
                log.Info("Backup TAL:");
                log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", psdzBackupTal.AsXml.Length);
                cts?.Token.ThrowIfCancellationRequested();

                if (PsdzContext.TalFilterForECUWithIDRClassicState != null)
                {
                    sbResult.AppendLine(Strings.TalIdrGenerating);
                    UpdateStatus(sbResult.ToString());

                    log.InfoFormat(CultureInfo.InvariantCulture, "Generating IDR classic TAL");
                    IPsdzTal psdzTalIdrClassic = ProgrammingService.Psdz.LogicService.GenerateTal(PsdzContext.Connection, PsdzContext.SvtActual, psdzSollverbauung, PsdzContext.SwtAction, PsdzContext.TalFilterForECUWithIDRClassicState, PsdzContext.FaActual.Vin);
                    if (psdzTalIdrClassic == null)
                    {
                        PsdzContext.Tal = null;
                        sbResult.AppendLine(Strings.TalGenerationFailed);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    PsdzContext.TalForECUWithIDRClassicState = psdzTalIdrClassic;

                    log.Info("IDR classic TAL:");
                    log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", psdzTalIdrClassic.AsXml.Length);
                    cts?.Token.ThrowIfCancellationRequested();
                }
                else
                {
                    PsdzContext.TalForECUWithIDRClassicState = null;
                }

                sbResult.AppendLine(Strings.TalRestoreGenrating);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Generating restore TAL");
                IPsdzTal psdzTalRestore = PsdzContext.Tal;
                if (PsdzContext.TalForECUWithIDRClassicState != null)
                {
                    psdzTalRestore = PsdzContext.TalForECUWithIDRClassicState;
                }

                IPsdzTal psdzRestorePrognosisTal = ProgrammingService.Psdz.IndividualDataRestoreService.GenerateRestorePrognosisTal(PsdzContext.Connection, PsdzContext.PathToBackupData, psdzTalRestore, PsdzContext.IndividualDataBackupTal, PsdzContext.TalFilterForIndividualDataTal);
                if (psdzRestorePrognosisTal == null)
                {
                    PsdzContext.Tal = null;
                    RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                    sbResult.AppendLine(Strings.TalRestoreGenerationFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.IndividualDataRestorePrognosisTal = psdzRestorePrognosisTal;
                log.Info("Restore prognosis Tal:");
                log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", psdzRestorePrognosisTal.AsXml.Length);
                cts?.Token.ThrowIfCancellationRequested();

                CheckVoltage(cts, sbResult, false, true);
                sbResult.AppendLine(Strings.ExecutingVehicleFuncFinished);
                UpdateStatus(sbResult.ToString());
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "VehicleFunctions Exception: {0}", ex.Message);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.ExceptionMsg, ex.Message));
                UpdateStatus(sbResult.ToString());
                if (operationType != OperationType.ExecuteTal)
                {
                    PsdzContext.Tal = null;
                    RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                }
                return false;
            }
            finally
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "VehicleFunctions Finish - Type: {0}", operationType);
                log.Info(Environment.NewLine + sbResult);
            }
        }

        public List<OptionsItem> GetCombinedOptionsItems(OptionsItem optionsItemSelect, List<OptionsItem> optionItemsList)
        {
            if (PsdzContext == null || optionsItemSelect == null || optionsItemSelect.EcuInfo == null || optionItemsList == null)
            {
                return null;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "GetCombinedOptionsItems Ecu: {0}", optionsItemSelect.EcuInfo.Name);
            ICombinedEcuHousingEntry combinedEcuHousingEntry = PsdzContext.GetEcuHousingEntry((int) optionsItemSelect.EcuInfo.Address);
            if (combinedEcuHousingEntry == null || combinedEcuHousingEntry.RequiredEcuAddresses == null)
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "GetCombinedOptionsItems No housing entry for address: {0}", optionsItemSelect.EcuInfo.Address);
                return null;
            }

            List<OptionsItem> combinedItems = new List<OptionsItem>();
            foreach (int ecuAddress in combinedEcuHousingEntry.RequiredEcuAddresses)
            {
                foreach (OptionsItem optionsItem in optionItemsList)
                {
                    if (optionsItem == optionsItemSelect)
                    {
                        continue;
                    }

                    if (optionsItem.EcuInfo != null)
                    {
                        if (optionsItem.EcuInfo.Address == ecuAddress)
                        {
                            if (!combinedItems.Contains(optionsItem))
                            {
                                log.InfoFormat(CultureInfo.InvariantCulture, "GetCombinedOptionsItems Adding combined Ecu: {0}", optionsItem.EcuInfo.Name);
                                combinedItems.Add(optionsItem);
                            }
                        }
                    }
                }
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "GetCombinedOptionsItems Found items: {0}", combinedItems.Count);
            return combinedItems;
        }

        public void UpdateTargetFa(bool reset = false)
        {
            if (PsdzContext == null || SelectedOptions == null)
            {
                return;
            }

            if (reset)
            {
                SelectedOptions.Clear();
            }

            PsdzContext.SetFaTarget(PsdzContext.FaActual);
            ProgrammingService.PsdzDatabase.ResetXepRules();

            foreach (OptionsItem optionsItem in SelectedOptions)
            {
                if (optionsItem.SwiAction?.SwiInfoObjs != null)
                {
                    foreach (PsdzDatabase.SwiInfoObj infoInfoObj in optionsItem.SwiAction.SwiInfoObjs)
                    {
                        if (infoInfoObj.LinkType == PsdzDatabase.SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                        {
                            string moduleName = infoInfoObj.ModuleName;
                            PsdzDatabase.TestModuleData testModuleData = ProgrammingService.PsdzDatabase.GetTestModuleData(moduleName);
                            if (testModuleData == null)
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "UpdateTargetFa GetTestModuleData failed for: {0}", moduleName);
                                optionsItem.Invalid = true;
                            }
                            else
                            {
                                optionsItem.Invalid = false;
                                if (!string.IsNullOrEmpty(testModuleData.ModuleRef))
                                {
                                    PsdzDatabase.SwiInfoObj swiInfoObj = ProgrammingService.PsdzDatabase.GetInfoObjectByControlId(testModuleData.ModuleRef, infoInfoObj.LinkType);
                                    if (swiInfoObj == null)
                                    {
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "UpdateTargetFa No info object: {0}", testModuleData.ModuleRef);
                                    }
                                    else
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "UpdateTargetFa Info object: {0}", swiInfoObj.ToString(ClientContext.GetLanguage(PsdzContext.VecInfo)));
                                    }
                                }

                                IFa ifaTarget = ProgrammingUtils.BuildFa(PsdzContext.FaTarget);
                                if (testModuleData.RefDict.TryGetValue("faElementsToRem", out List<string> remList))
                                {
                                    if (!ProgrammingUtils.ModifyFa(ifaTarget, remList, false))
                                    {
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "UpdateTargetFa Rem failed: {0}", remList.ToStringItems());
                                    }
                                }
                                if (testModuleData.RefDict.TryGetValue("faElementsToAdd", out List<string> addList))
                                {
                                    if (!ProgrammingUtils.ModifyFa(ifaTarget, addList, true))
                                    {
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "UpdateTargetFa Add failed: {0}", addList.ToStringItems());
                                    }
                                }

                                IPsdzFa psdzFaTarget = ProgrammingService.Psdz.ObjectBuilder.BuildFa(ifaTarget, PsdzContext.FaActual.Vin);
                                PsdzContext.SetFaTarget(psdzFaTarget);
                                ProgrammingService.PsdzDatabase.ResetXepRules();
                            }
                        }
                    }
                }
            }

            SelectedOptions.RemoveAll(x => x.Invalid);

            {
                log.InfoFormat(CultureInfo.InvariantCulture, "UpdateTargetFa FaTarget: {0}", PsdzContext.FaTarget.AsString);

                IFa ifaTarget = ProgrammingUtils.BuildFa(PsdzContext.FaTarget);
                IFa ifaActual = ProgrammingUtils.BuildFa(PsdzContext.FaActual);
                string compareFa = ProgrammingUtils.CompareFa(ifaActual, ifaTarget);
                if (!string.IsNullOrEmpty(compareFa))
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "UpdateTargetFa Compare FA: {0}", compareFa);
                }
            }
        }

        private void UpdateTalFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions)
        {
            PsdzContext.SetTalFilter(ProgrammingService.Psdz.ObjectBuilder.DefineFilterForSelectedEcus(taCategories, diagAddress, talFilterOptions, PsdzContext.TalFilter));
            PsdzContext.SetTalFilterForIndividualDataTal(ProgrammingService.Psdz.ObjectBuilder.DefineFilterForSelectedEcus(taCategories, diagAddress, talFilterOptions, PsdzContext.TalFilterForIndividualDataTal));
        }

        private void UpdateTalFilterTalForECUsWithIDRClassicState(int[] diagAddress)
        {
            if (diagAddress.Length == 0)
            {
                PsdzContext.SetTalFilterForECUWithIDRClassicState(null);
            }
            else
            {
                PsdzContext.SetTalFilterForECUWithIDRClassicState(ProgrammingService.Psdz.ObjectBuilder.DefineFilterForSelectedEcus(new[] { TaCategories.HwInstall, TaCategories.HwDeinstall }, diagAddress, TalFilterOptions.Must, PsdzContext.TalFilter));
            }
        }

        private bool IsTalExecutionStateOk(PsdzTalExecutionState talExecutionState, bool acceptWarning = false)
        {
            switch (talExecutionState)
            {
                case PsdzTalExecutionState.Finished:
                case PsdzTalExecutionState.FinishedForHardwareTransactions:
                    return true;

                case PsdzTalExecutionState.FinishedWithWarnings:
                case PsdzTalExecutionState.FinishedForHardwareTransactionsWithWarnings:
                    if (!acceptWarning)
                    {
                        break;
                    }
                    return true;
            }

            return false;
        }

        private string GetFaString(IPsdzFa ifa)
        {
            if (ifa != null)
            {
                if (ProgrammingUtils.BuildFa(ifa) is VehicleOrder faTarget)
                {
                    return faTarget.ToString();
                }
            }

            return null;
        }

        private bool CheckVoltage(CancellationTokenSource cts, StringBuilder sbResult, bool showInfo = false, bool addMessage = false)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage vehicle: Show info={0}", showInfo);
            if (PsdzContext.VecInfo == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "CheckVoltage No vehicle");
                return false;
            }

            if (PsdzContext.DetectVehicle.IsDoIp)
            {   // Parallel connections in DoIp mode are unstable.
                log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage Disabled voltage check in DoIP mode");
                return true;
            }

            CacheType cacheTypeOld = CacheResponseType;
            bool icomAllocated = PsdzContext.DetectVehicle.IsIcomAllocated();

            try
            {
                CacheResponseType = CacheType.NoResponse;
                for (; ; )
                {
                    double? voltage = PsdzContext.DetectVehicle.ReadBatteryVoltage(() =>
                    {
                        if (cts != null)
                        {
                            return cts.Token.IsCancellationRequested;
                        }
                        return false;
                    });

                    if (voltage == null)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "CheckVoltage read voltage error");
                        return false;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage: Battery voltage={0}", voltage);

                    bool lfpBattery = PsdzContext.VecInfo.WithLfpBattery;
                    bool lfpNCarBattery = PsdzContext.VecInfo.WithLfpNCarBattery;
                    bool isLpf = lfpBattery || lfpNCarBattery;
                    double minVoltageError = isLpf ? MinBatteryVoltageErrorLfp : MinBatteryVoltageErrorPb;
                    double minVoltageWarn = isLpf ? MinBatteryVoltageWarnLfp : MinBatteryVoltageWarnPb;
                    double maxVoltageWarn = isLpf ? MaxBatteryVoltageWarnLfp : MaxBatteryVoltageWarnPb;
                    double maxVoltageError = isLpf ? MaxBatteryVoltageErrorLfp : MaxBatteryVoltageErrorPb;
                    log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage: LFP={0}, MinErr={1}, MinWarn={2}, MaxWarn={3}, MaxErr={4}",
                        lfpBattery, minVoltageError, minVoltageWarn, maxVoltageWarn, maxVoltageError);

                    bool error = voltage < minVoltageError || voltage > maxVoltageError;
                    bool warn = voltage < minVoltageWarn || voltage > maxVoltageWarn;
                    if (!warn && !error)
                    {
                        if (ShowMessageEvent != null && showInfo)
                        {
                            string message = string.Format(CultureInfo.InvariantCulture, Strings.BatteryVoltageInfo,
                                voltage, minVoltageWarn, maxVoltageWarn);
                            if (!ShowMessageEvent.Invoke(cts, message, true, true))
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "CheckVoltage BatteryVoltageValid aborted");
                                return false;
                            }
                        }
                        else
                        {
                            string statusMessage = string.Format(CultureInfo.InvariantCulture, Strings.BatteryVoltage, voltage);
                            if (addMessage)
                            {
                                sbResult.AppendLine(statusMessage);
                            }
                            else
                            {
                                // temporary message only
                                UpdateStatus(sbResult + statusMessage);
                            }
                        }
                        break;
                    }

                    if (voltage < 0 || ShowMessageEvent == null)
                    {
                        break;
                    }

                    if (error)
                    {
                        string message = string.Format(CultureInfo.InvariantCulture, Strings.BatteryVoltageError,
                            voltage, minVoltageError, maxVoltageError);
                        if (!ShowMessageEvent.Invoke(cts, message, false, true))
                        {
                            log.ErrorFormat(CultureInfo.InvariantCulture, "CheckVoltage BatteryVoltageOutOfRange aborted");
                            return false;
                        }
                    }
                    else
                    {
                        string message = string.Format(CultureInfo.InvariantCulture, Strings.BatteryVoltageWarn,
                            voltage, minVoltageWarn, maxVoltageWarn);
                        if (!ShowMessageEvent.Invoke(cts, message, true, true))
                        {
                            log.ErrorFormat(CultureInfo.InvariantCulture, "CheckVoltage BatteryVoltageOutOfRange aborted");
                            return false;
                        }
                        break;
                    }

                    cts?.Token.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                if (!icomAllocated)
                {
                    PsdzContext.DetectVehicle.Disconnect();
                }

                CacheResponseType = cacheTypeOld;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage OK");
            return true;
        }

        public bool IsOptionsItemEnabled(OptionsItem optionsItem)
        {
            if (optionsItem == null)
            {
                return true;
            }

            if (OperationState == null || OperationState.Operation != OperationStateData.OperationEnum.HwInstall ||
                OperationState.DiagAddrList == null || OperationState.DiagAddrList.Count == 0)
            {
                return true;
            }

            if (optionsItem.SwiRegisterEnum == PsdzDatabase.SwiRegisterEnum.EcuReplacementAfterReplacement)
            {
                if (optionsItem.EcuInfo == null)
                {
                    return true;
                }

                if (!OperationState.DiagAddrList.Contains((int)optionsItem.EcuInfo.Address))
                {
                    return true;
                }
            }

            return false;
        }

        public bool SaveOperationState()
        {
            try
            {
                string backupPath = PsdzContext.PathToBackupData;
                if (string.IsNullOrEmpty(backupPath))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveOperationState No backup path");
                    return false;
                }

                bool deleteFiles = true;
                if (OperationState != null)
                {
                    switch (OperationState.Operation)
                    {
                        case OperationStateData.OperationEnum.HwInstall:
                        case OperationStateData.OperationEnum.HwDeinstall:
                            deleteFiles = false;
                            break;

                        case OperationStateData.OperationEnum.Modification:
                            if (OperationState.TalExecutionState != OperationStateData.TalExecutionStateEnum.None)
                            {
                                deleteFiles = false;
                            }
                            break;
                    }

                    OperationState.StructVersion = OperationStateData.StructVersionCurrent;
                    PsdzDatabase.DbInfo dbInfo = ProgrammingService.PsdzDatabase.GetDbInfo();
                    OperationState.Version = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                }

                string fileName = backupPath.TrimEnd('\\') + ".xml";
                if (deleteFiles)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveOperationState Deleting: {0}", fileName);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                else
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveOperationState Saving: {0}", fileName);
                    XmlSerializer serializer = new XmlSerializer(typeof(OperationStateData));
                    using (FileStream fileStream = File.Create(fileName))
                    {
                        serializer.Serialize(fileStream, OperationState);
                    }

                    if (!Utility.Encryption.SetFileFullAccessControl(fileName))
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "SaveOperationState SetFileFullAccessControl failed: {0}", fileName);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveOperationState Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        public bool LoadOperationState()
        {
            try
            {
                string backupPath = PsdzContext.PathToBackupData;
                if (string.IsNullOrEmpty(backupPath))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "LoadOperationState No backup path");
                    return false;
                }

                string fileName = backupPath.TrimEnd('\\') + ".xml";
                if (!File.Exists(fileName))
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "LoadOperationState File not found: {0}", fileName);
                    OperationState = new OperationStateData();
                }
                else
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "LoadOperationState Loading: {0}", fileName);
                    XmlSerializer serializer = new XmlSerializer(typeof(OperationStateData));
                    using (StreamReader streamReader = new StreamReader(fileName))
                    {
                        OperationState = serializer.Deserialize(streamReader) as OperationStateData;
                    }

                    if (OperationState != null)
                    {
                        bool dataValid = true;
                        if (OperationState.StructVersion != OperationStateData.StructVersionCurrent)
                        {
                            log.ErrorFormat(CultureInfo.InvariantCulture, "LoadOperationState StructVersion mismatch");
                            dataValid = false;
                        }

                        if (dataValid)
                        {
                            PsdzDatabase.DbInfo dbInfo = ProgrammingService.PsdzDatabase.GetDbInfo();
                            if (OperationState.Version == null || !OperationState.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "LoadOperationState Version mismatch");
                                dataValid = false;
                            }
                        }

                        if (!dataValid)
                        {
                            OperationState = new OperationStateData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "LoadOperationState Exception: {0}", ex.Message);
                OperationState = new OperationStateData();
                return false;
            }

            return true;
        }

        public void UpdateReplaceOperationState()
        {
            OperationStateData.OperationEnum operation = OperationStateData.OperationEnum.Idle;
            List<int> diagAddrList = null;
            switch (RegisterGroup)
            {
                case PsdzDatabase.SwiRegisterGroup.HwInstall:
                    operation = OperationStateData.OperationEnum.HwInstall;
                    break;

                case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                    operation = OperationStateData.OperationEnum.HwDeinstall;
                    break;
            }

            if (operation != OperationStateData.OperationEnum.Idle)
            {
                diagAddrList = new List<int>();
                foreach (OptionsItem optionItem in SelectedOptions)
                {
                    if (optionItem.EcuInfo != null)
                    {
                        int address = (int)optionItem.EcuInfo.Address;
                        if (!diagAddrList.Contains(address))
                        {
                            diagAddrList.Add(address);
                        }
                    }
                }
                OperationState.DiagAddrList = diagAddrList;
            }

            OperationState = new OperationStateData(operation, diagAddrList);
        }

        public bool StartTalExecutionState(OperationStateData.TalExecutionStateEnum talExecutionState, bool skipped = false)
        {
            if (OperationState == null)
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "StartTalExecutionState No data");
                OperationState = new OperationStateData();
            }

            OperationStateData.OperationEnum operation = OperationStateData.OperationEnum.Idle;
            switch (RegisterGroup)
            {
                case PsdzDatabase.SwiRegisterGroup.Modification:
                    operation = OperationStateData.OperationEnum.Modification;
                    break;
            }

            OperationState.Operation = operation;
            if (operation != OperationStateData.OperationEnum.Idle)
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "StartTalExecutionState: State={0}, Skipped={1}", talExecutionState.ToString(), skipped);
                if (talExecutionState == OperationStateData.TalExecutionStateEnum.None)
                {
                    OperationState.TalExecutionState = OperationStateData.TalExecutionStateEnum.None;
                    OperationState.BackupTargetFA = null;
                    OperationState.TalExecutionDict = null;
                }
                else
                {
                    OperationState.TalExecutionState = talExecutionState;

                    if (OperationState.TalExecutionDict == null)
                    {
                        OperationState.TalExecutionDict = new SerializableDictionary<OperationStateData.TalExecutionStateEnum, OperationStateData.TalExecutionResultEnum>();
                    }

                    if (talExecutionState == OperationStateData.TalExecutionStateEnum.BackupTalExecuting)
                    {
                        OperationState.TalExecutionDict.Clear();
                    }

                    OperationStateData.TalExecutionResultEnum talExecutionResult = OperationStateData.TalExecutionResultEnum.None;
                    if (talExecutionState != OperationStateData.TalExecutionStateEnum.Finished)
                    {
                        talExecutionResult = skipped ? OperationStateData.TalExecutionResultEnum.Skipped : OperationStateData.TalExecutionResultEnum.Started;
                    }

                    OperationState.TalExecutionDict[OperationState.TalExecutionState] = talExecutionResult;
                }

                List<SelectedOptionData> selectedOptionList = new List<SelectedOptionData>();
                foreach (OptionsItem optionsItem in SelectedOptions)
                {
                    string swiActionId = optionsItem.SwiAction.Id;
                    if (string.IsNullOrEmpty(swiActionId))
                    {
                        continue;
                    }

                    selectedOptionList.Add(new SelectedOptionData(swiActionId, optionsItem.SwiRegisterEnum));
                }

                OperationState.SelectedOptionList = selectedOptionList;
            }

            return SaveOperationState();
        }

        public bool FinishTalExecutionState(CancellationTokenSource cts, bool failure = false)
        {
            if (OperationState == null)
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "FinishTalExecutionState No data");
                return false;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "FinishTalExecutionState: Failure={0}", failure);

            if (OperationState.TalExecutionState != OperationStateData.TalExecutionStateEnum.None)
            {
                if (OperationState.TalExecutionState == OperationStateData.TalExecutionStateEnum.BackupTalExecuting)
                {
                    if (failure)
                    {
                        OperationState.BackupTargetFA = null;
                    }
                    else
                    {
                        string backupTargetFA = null;
                        if (PsdzContext != null)
                        {
                            backupTargetFA = GetFaString(PsdzContext.FaTarget);
                        }

                        OperationState.BackupTargetFA = backupTargetFA;
                    }
                }

                if (OperationState.TalExecutionDict != null)
                {
                    OperationStateData.TalExecutionResultEnum talExecutionResult = OperationStateData.TalExecutionResultEnum.None;
                    if (failure)
                    {
                        talExecutionResult = OperationStateData.TalExecutionResultEnum.Failure;
                    }
                    else if (cts != null && cts.Token.IsCancellationRequested)
                    {
                        talExecutionResult = OperationStateData.TalExecutionResultEnum.Cancelled;
                    }

                    OperationState.TalExecutionDict[OperationState.TalExecutionState] = talExecutionResult;
                }
            }

            return SaveOperationState();
        }

        public bool RestoreTalOperationState()
        {
            if (OperationState == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreTalOperationState No data");
                return false;
            }

            if (OptionsDict == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreTalOperationState No options dict");
                return false;
            }

            if (OperationState.Operation != OperationStateData.OperationEnum.Modification)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreTalOperationState Invalid operation: {0}", OperationState.Operation);
                return false;
            }

            PsdzDatabase.SwiRegisterEnum? swiRegisterEnum = null;
            SelectedOptions.Clear();
            if (OptionsDict != null && OperationState.SelectedOptionList != null)
            {
                foreach (SelectedOptionData optionData in OperationState.SelectedOptionList)
                {
                    if (!OptionsDict.TryGetValue(optionData.SwiRegister, out List<OptionsItem> optionsSwi))
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreTalOperationState Options for {0} not found", optionData.SwiRegister);
                    }

                    if (optionsSwi == null)
                    {
                        continue;
                    }

                    bool itemFound = false;
                    foreach (OptionsItem optionsItem in optionsSwi)
                    {
                        string swiActionId = optionsItem.SwiAction.Id;
                        if (string.IsNullOrEmpty(swiActionId))
                        {
                            continue;
                        }

                        if (string.Compare(swiActionId, optionData.SwiId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            itemFound = true;
                            swiRegisterEnum = optionData.SwiRegister;
                            SelectedOptions.Add(optionsItem);
                            break;
                        }
                    }

                    if (!itemFound)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreTalOperationState Item for id not found: {0}", optionData.SwiId);
                    }
                }
            }

            if (swiRegisterEnum == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreTalOperationState SwiRegister missing");
                return false;
            }

            PsdzContext.Tal = null;
            UpdateTargetFa();
            UpdateOptionSelections(swiRegisterEnum);
            return true;
        }

        public bool RestoreReplaceOperationState()
        {
            if (OperationState == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreReplaceOperationState No data");
                return false;
            }

            PsdzDatabase.SwiRegisterEnum? swiRegisterEnum = null;
            switch (OperationState.Operation)
            {
                case OperationStateData.OperationEnum.HwInstall:
                    swiRegisterEnum = PsdzDatabase.SwiRegisterEnum.EcuReplacementAfterReplacement;
                    break;

                case OperationStateData.OperationEnum.HwDeinstall:
                    swiRegisterEnum = PsdzDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement;
                    break;
            }

            if (swiRegisterEnum == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreReplaceOperationState Nothing to restore");
                return false;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "RestoreReplaceOperationState Restoring: {0}", swiRegisterEnum.Value);
            List<OptionsItem> optionsReplacement = null;
            if (OptionsDict != null)
            {
                if (!OptionsDict.TryGetValue(swiRegisterEnum.Value, out optionsReplacement))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreReplaceOperationState Options for {0} not found", swiRegisterEnum);
                }
            }

            SelectedOptions.Clear();
            if (optionsReplacement != null && OperationState.DiagAddrList != null)
            {
                foreach (int address in OperationState.DiagAddrList)
                {
                    bool itemFound = false;
                    foreach (OptionsItem optionsItem in optionsReplacement)
                    {
                        if (optionsItem.EcuInfo != null && optionsItem.EcuInfo.Address == address)
                        {
                            itemFound = true;
                            SelectedOptions.Add(optionsItem);
                            break;
                        }
                    }

                    if (!itemFound)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "RestoreReplaceOperationState Item for address not found: {0}", address);
                    }
                }
            }

            PsdzContext.Tal = null;
            UpdateOptionSelections(swiRegisterEnum);
            return true;
        }

        public void ResetOperationState()
        {
            OperationState = new OperationStateData();
            SaveOperationState();
            PsdzContext.DeleteIndividualDataFromPuk();
        }

        public bool InitProgrammingObjects(string istaFolder)
        {
            try
            {
                PsdzContext = new PsdzContext(istaFolder);
                DisableTalFlash = false;
                RegisterGroup = PsdzDatabase.SwiRegisterGroup.Modification;
                OptionsDict = null;
                OperationState = new OperationStateData();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void ClearProgrammingObjects()
        {
            PsdzContext psdzContext = PsdzContext;
            if (psdzContext != null)
            {
                psdzContext.Dispose();
                PsdzContext = null;
            }
        }

        public void UpdateStatus(string message = null)
        {
            UpdateStatusEvent?.Invoke(message);
        }

        private void UpdateOptions(Dictionary<PsdzDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict)
        {
            OptionsDict = optionsDict;
            UpdateOptionsEvent?.Invoke(optionsDict);
        }

        private void UpdateOptionSelections(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum = null)
        {
            if (swiRegisterEnum != null)
            {
                RegisterGroup = PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnum.Value);
            }

            UpdateOptionSelectionsEvent?.Invoke(swiRegisterEnum);
        }

        private bool WaitForEmptyVehicleQueue(int timeout = 20000)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "WaitForEmptyVehicleQueue Timeout: {0}", timeout);

            long startTime = Stopwatch.GetTimestamp();
            int queueSize;
            for (; ; )
            {
                queueSize = TelSendQueueSizeEvent?.Invoke() ?? -1;
                log.InfoFormat(CultureInfo.InvariantCulture, "WaitForEmptyVehicleQueue Queue size: {0}", queueSize);
                if (queueSize < 1)
                {
                    break;
                }

                if ((Stopwatch.GetTimestamp() - startTime) > timeout * TickResolMs)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "WaitForEmptyVehicleQueue Timeout, continuing");
                    break;
                }

                Thread.Sleep(1000);
            }

            long queueWaitTime = (Stopwatch.GetTimestamp() - startTime) / TickResolMs;
            log.InfoFormat(CultureInfo.InvariantCulture, "WaitForEmptyVehicleQueue Queue wait time: {0}s", queueWaitTime / 1000);

            if (queueSize < 0)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "WaitForEmptyVehicleQueue No queue");
                return false;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "WaitForEmptyVehicleQueue Final queue: {0}", queueSize);
            return true;
        }

        public static void SetupLog4Net(string logFile)
        {
            try
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                if (!string.IsNullOrEmpty(codeBase))
                {
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    string appDir = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(appDir))
                    {
                        string log4NetConfig = Path.Combine(appDir, "log4net_config.xml");
                        if (File.Exists(log4NetConfig))
                        {
                            log4net.GlobalContext.Properties["LogFileName"] = logFile;
                            XmlConfigurator.Configure(new FileInfo(log4NetConfig));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static string GetIstaInstallLocation()
        {
            using (RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey key = localMachine64.OpenSubKey(RegKeyIsta))
                {
                    string path = key?.GetValue(RegValueIstaLocation, null) as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (Directory.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (RegistryKey key = localMachine32.OpenSubKey(RegKeyIsta))
                {
                    string path = key?.GetValue(RegValueIstaLocation, null) as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (Directory.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            return null;
        }


        private bool ExecuteSubProcess(CancellationTokenSource cts, string arguments, string mutexName)
        {
            try
            {
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess No EntryAssembly");
                    return false;
                }

                string fileName = entryAssembly.Location;
                if (string.IsNullOrEmpty(fileName))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess No Location");
                    return false;
                }

                Process currentProcess = Process.GetCurrentProcess();
                Process[] sameProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
                int otherProcessesCount = 0;
                foreach (Process otherProcess in sameProcesses)
                {
                    if (otherProcess.Id != currentProcess.Id)
                    {
                        otherProcessesCount++;
                    }
                }

                if (otherProcessesCount != 0)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess Other processes running: {0}", otherProcessesCount);
                    return false;
                }

                Mutex processMutex = null;
                try
                {
                    try
                    {
                        processMutex = new Mutex(true, mutexName);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess Unable to open mutex Exception: {0}", ex.Message);
                        return false;
                    }

                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = fileName;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.RedirectStandardInput = false;
                    processStartInfo.RedirectStandardOutput = false;
                    processStartInfo.RedirectStandardError = false;
                    processStartInfo.CreateNoWindow = true;
                    processStartInfo.Arguments = arguments;

                    Process process = Process.Start(processStartInfo);
                    if (process == null)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess Process start failed: '{0}'",
                            fileName);
                        return false;
                    }

                    bool parentSet = false;
                    while (!process.WaitForExit(1000))
                    {
                        if (!parentSet && ParentWindowHandle != IntPtr.Zero)
                        {
                            IntPtr mainWindowHandle = process.MainWindowHandle;
                            if (mainWindowHandle != IntPtr.Zero)
                            {
                                SetWindowLongPtr(mainWindowHandle, -8, ParentWindowHandle);
                                parentSet = true;
                            }
                        }

                        if (cts != null)
                        {
                            if (cts.IsCancellationRequested)
                            {
                                log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess Cancelled");
                                return false;
                            }
                        }
                    }

                    int exitCode = process.ExitCode;
                    log.InfoFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess Process exit code: {0}", exitCode);
                    if (exitCode != 0)
                    {
                        return false;
                    }
                }
                finally
                {
                    if (processMutex != null)
                    {
                        processMutex.ReleaseMutex();
                        processMutex.Dispose();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteSubProcess Exception: {0}", ex.Message);
                return false;
            }
        }

        private bool IsMasterProcessMutexValid(string mutexName)
        {
            try
            {
                if (!Mutex.TryOpenExisting(mutexName, out Mutex processMutex))
                {
                    log.ErrorFormat("IsMasterProcessMutexValid Open mutex failed: {0}", mutexName);
                    return false;
                }
                processMutex.Dispose();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("IsMasterProcessMutexValid Open mutex Exception: {0}", ex.Message);
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (ProgrammingService != null)
                    {
                        ProgrammingService.Dispose();
                        ProgrammingService = null;
                    }

                    ClearProgrammingObjects();

                    if (ClientContext != null)
                    {
                        ClientContext.Dispose();
                        ClientContext = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
