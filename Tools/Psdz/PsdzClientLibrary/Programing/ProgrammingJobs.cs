using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;
using EdiabasLib;
using log4net;
using log4net.Config;
using PsdzClient.Core;
using PsdzClient.Programming;
using PsdzClientLibrary.Resources;

namespace PsdzClient.Programing
{
    public class ProgrammingJobs : IDisposable
    {
        public enum OperationType
        {
            CreateOptions,
            BuildTalILevel,
            BuildTalModFa,
            ExecuteTal,
        }

        public enum CacheType
        {
            None,
            NoResponse,
            FuncAddress,
        }

        public class OptionsItem
        {
            public OptionsItem(PdszDatabase.SwiRegisterEnum swiRegisterEnum,PdszDatabase.SwiAction swiAction, ClientContext clientContext)
            {
                Init();
                SwiRegisterEnum = swiRegisterEnum;
                SwiAction = swiAction;
                ClientContext = clientContext;
            }

            public OptionsItem(PdszDatabase.SwiRegisterEnum swiRegisterEnum, PdszDatabase.EcuInfo ecuInfo, ClientContext clientContext)
            {
                Init();
                SwiRegisterEnum = swiRegisterEnum;
                EcuInfo = ecuInfo;
                ClientContext = clientContext;
            }

            private void Init()
            {
                Id = Guid.NewGuid().ToString();
                Invalid = false;
            }

            public PdszDatabase.SwiRegisterEnum SwiRegisterEnum { get; private set; }

            public PdszDatabase.SwiAction SwiAction { get; private set; }

            public PdszDatabase.EcuInfo EcuInfo { get; private set; }

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
                    sb.Append(EcuInfo.Name);
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

        public class OptionType
        {
            public OptionType(string name, PdszDatabase.SwiRegisterEnum swiRegisterEnum)
            {
                Name = name;
                SwiRegisterEnum = swiRegisterEnum;
                SwiRegister = null;
            }

            public string Name { get; private set; }

            public PdszDatabase.SwiRegisterEnum SwiRegisterEnum { get; private set; }

            public PdszDatabase.SwiRegister SwiRegister { get; set; }

            public ClientContext ClientContext { get; set; }

            public override string ToString()
            {
                PdszDatabase.SwiRegister swiRegister = SwiRegister;
                if (swiRegister != null)
                {
                    return swiRegister.EcuTranslation.GetTitle(ClientContext?.Language);
                }
                return Name;
            }
        }

        public delegate void UpdateStatusDelegate(string message = null);
        public event UpdateStatusDelegate UpdateStatusEvent;

        public delegate void ProgressDelegate(int percent, bool marquee, string message = null);
        public event ProgressDelegate ProgressEvent;

        public delegate void UpdateOptionsDelegate(Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict);
        public event UpdateOptionsDelegate UpdateOptionsEvent;

        public delegate bool ShowMessageDelegate(CancellationTokenSource cts, string message, bool okBtn, bool wait);
        public event ShowMessageDelegate ShowMessageEvent;

        public delegate void ServiceInitialized(ProgrammingService programmingService);
        public event ServiceInitialized ServiceInitializedEvent;

        private static readonly ILog log = LogManager.GetLogger(typeof(ProgrammingJobs));
        private OptionType[] _optionTypes =
        {
            new OptionType("Coding", PdszDatabase.SwiRegisterEnum.VehicleModificationCodingConversion),
            new OptionType("Coding back", PdszDatabase.SwiRegisterEnum.VehicleModificationCodingBackConversion),
            new OptionType("Modification", PdszDatabase.SwiRegisterEnum.VehicleModificationConversion),
            new OptionType("Modification back", PdszDatabase.SwiRegisterEnum.VehicleModificationBackConversion),
            new OptionType("Retrofit", PdszDatabase.SwiRegisterEnum.VehicleModificationRetrofitting),
            new OptionType("Before replacement", PdszDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement),
            new OptionType("After replacement", PdszDatabase.SwiRegisterEnum.EcuReplacementAfterReplacement),
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
        private object _cacheLock = new object();
        public PsdzContext PsdzContext { get; private set; }
        public ProgrammingService ProgrammingService { get; private set; }
        public List<OptionsItem> SelectedOptions { get; set; }
        public bool DisableTalFlash { get; set; }

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

        public ProgrammingJobs(string dealerId)
        {
            ClientContext = new ClientContext();
            _dealerId = dealerId;
            ProgrammingService = null;
        }

        public bool StartProgrammingService(CancellationTokenSource cts, string istaFolder)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "Start programming service DealerId={0}", _dealerId);
                sbResult.AppendLine(Strings.HostStarting);
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Programming service already existing");
                }
                else
                {
                    ProgrammingService = new ProgrammingService(istaFolder, _dealerId);
                    ClientContext.Database = ProgrammingService.PdszDatabase;
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

                    int failCount = -1;
                    bool result = ProgrammingService.PdszDatabase.GenerateTestModuleData((progress, failures) =>
                    {
                        failCount = failures;
                        string message = string.Format(CultureInfo.InvariantCulture, Strings.TestModuleProgress, progress, failures);
                        ProgressEvent?.Invoke(progress, false, message);

                        if (cts != null)
                        {
                            return cts.Token.IsCancellationRequested;
                        }
                        return false;
                    });

                    if (!result)
                    {
                        log.ErrorFormat("GenerateTestModuleData failed");
                    }

                    if (!ProgrammingService.PdszDatabase.GenerateEcuCharacteristicsData())
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData failed");
                        result = false;
                    }

                    ProgressEvent?.Invoke(0, true);

                    if (failCount >= 0)
                    {
                        log.InfoFormat("Test module generation failures: {0}", failCount);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.TestModuleFailures, failCount));
                        UpdateStatus(sbResult.ToString());
                    }

                    if (!result)
                    {
                        if (!ProgrammingService.PdszDatabase.IsExecutable())
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

                if (!PsdzServiceStarter.IsServerInstanceRunning())
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Starting host");
                    if (!ProgrammingService.StartPsdzServiceHost())
                    {
                        sbResult.AppendLine(Strings.HostStartFailed);
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    ProgrammingService.SetLogLevelToMax();
                    log.InfoFormat(CultureInfo.InvariantCulture, "Host started");
                }

                sbResult.AppendLine(Strings.HostStarted);
                UpdateStatus(sbResult.ToString());

                ProgrammingService.PdszDatabase.ResetXepRules();
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "StartProgrammingService Exception: {0}", ex.Message);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.ExceptionMsg, ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
        }

        public bool StopProgrammingService(CancellationTokenSource cts, string istaFolder)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine(Strings.HostStopping);
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService == null && PsdzServiceStarter.IsServerInstanceRunning())
                {
                    ProgrammingService = new ProgrammingService(istaFolder, _dealerId);
                }

                if (ProgrammingService != null)
                {
                    ProgrammingService.Psdz.Shutdown();
                    ProgrammingService.CloseConnectionsToPsdzHost();
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

        public bool ConnectVehicle(CancellationTokenSource cts, string istaFolder, string remoteHost, bool useIcom, int addTimeout = 1000)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "ConnectVehicle Start - Ip: {0}, ICOM: {1}", remoteHost, useIcom);
            StringBuilder sbResult = new StringBuilder();

            if (ProgrammingService == null)
            {
                if (!StartProgrammingService(cts, istaFolder))
                {
                    return false;
                }
            }

            try
            {
                sbResult.AppendLine(Strings.VehicleConnecting);
                UpdateStatus(sbResult.ToString());

                PdszDatabase.DbInfo dbInfo = ProgrammingService.PdszDatabase.GetDbInfo();
                if (dbInfo != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "DbInfo: {0}", dbInfo.ToString());

                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.DbInfo, dbInfo.Version, dbInfo.DateTime.ToShortDateString()));
                    UpdateStatus(sbResult.ToString());
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Connecting to: Host={0}, ICOM={1}", remoteHost, useIcom);
                string[] hostParts = remoteHost.Split(':');
                if (hostParts.Length < 1)
                {
                    return false;
                }

                string ipAddress = hostParts[0];
                if (ProgrammingService == null)
                {
                    return false;
                }

                if (!InitProgrammingObjects(istaFolder))
                {
                    return false;
                }

                sbResult.AppendLine(Strings.VehicleDetecting);
                UpdateStatus(sbResult.ToString());

                CacheResponseType = CacheType.FuncAddress;
                string ecuPath = Path.Combine(istaFolder, @"Ecu");
                bool icomConnection = useIcom;
                if (hostParts.Length > 1)
                {
                    icomConnection = true;
                }

                int diagPort = icomConnection ? 50160 : 6801;
                int controlPort = icomConnection ? 50161 : 6811;

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

                log.InfoFormat(CultureInfo.InvariantCulture, "ConnectVehicle Ip: {0}, Diag: {1}, Control: {2}, ICOM: {3}", ipAddress, diagPort, controlPort, icomConnection);
                EdInterfaceEnet.EnetConnection.InterfaceType interfaceType =
                    icomConnection ? EdInterfaceEnet.EnetConnection.InterfaceType.Icom : EdInterfaceEnet.EnetConnection.InterfaceType.Direct;
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

                PsdzContext.DetectVehicle = new DetectVehicle(ecuPath, enetConnection, useIcom, addTimeout);
                bool detectResult = PsdzContext.DetectVehicle.DetectVehicleBmwFast(() =>
                {
                    if (cts != null)
                    {
                        return cts.Token.IsCancellationRequested;
                    }
                    return false;
                });
                cts?.Token.ThrowIfCancellationRequested();

                string series = PsdzContext.DetectVehicle.Series;
                if (string.IsNullOrEmpty(series))
                {
                    sbResult.AppendLine(Strings.VehicleSeriesNotDetected);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (!detectResult)
                {
                    if (series.Length > 0)
                    {
                        char seriesChar = char.ToUpperInvariant(series[0]);
                        if (char.IsLetter(seriesChar) && seriesChar <= 'E')
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, Strings.VehicleSeriesInvalid, series));
                        }
                    }

                    sbResult.AppendLine(Strings.VehicleDetectionFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Detected vehicle: VIN={0}, GroupFile={1}, BR={2}, Series={3}, BuildDate={4}-{5}",
                    PsdzContext.DetectVehicle.Vin ?? string.Empty, PsdzContext.DetectVehicle.GroupSgdb ?? string.Empty,
                    PsdzContext.DetectVehicle.ModelSeries ?? string.Empty,
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

                log.InfoFormat(CultureInfo.InvariantCulture, "Ecus: {0}", PsdzContext.DetectVehicle.EcuList.Count());
                foreach (PdszDatabase.EcuInfo ecuInfo in PsdzContext.DetectVehicle.EcuList)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, " Ecu: Name={0}, Addr={1}, Sgdb={2}, Group={3}",
                        ecuInfo.Name, ecuInfo.Address, ecuInfo.Sgbd, ecuInfo.Grp);
                }

                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

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

                string bauIStufe = PsdzContext.DetectVehicle.ILevelShip;
                string url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", ipAddress, diagPort);
                IPsdzConnection psdzConnection;
                if (icomConnection)
                {
                    psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverIcom(
                        psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, addTimeout, series,
                        bauIStufe, IcomConnectionType.Ip, false);
                }
                else
                {
                    psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverEthernet(
                        psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, series,
                        bauIStufe);
                }

                Vehicle vehicle = new Vehicle(ClientContext);
                vehicle.VCI.VCIType = icomConnection ?
                    BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ICOM : BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ENET;
                vehicle.VCI.IPAddress = ipAddress;
                vehicle.VCI.Port = diagPort;
                vehicle.VCI.NetworkType = "LAN";
                vehicle.VCI.VIN = PsdzContext.DetectVehicle.Vin;
                PsdzContext.Vehicle = vehicle;

                ProgrammingService.CreateEcuProgrammingInfos(PsdzContext.Vehicle);
                if (!PsdzContext.UpdateVehicle(ProgrammingService))
                {
                    sbResult.AppendLine(Strings.UpdateVehicleDataFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (!CheckVoltage(cts, sbResult, true))
                {
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

                ClientContext clientContext = ClientContext.GetClientContext(PsdzContext.Vehicle);
                if (clientContext == null)
                {
                    sbResult.AppendLine(Strings.ContextMissing);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (!CheckVoltage(cts, sbResult))
                {
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
                        secureCodingConfig.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.ALLOW;
                        IList<IPsdzSecurityBackendRequestFailureCto> psdzSecurityBackendRequestFailureList = ProgrammingService.Psdz.SecureCodingService.RequestCalculationNcdAndSignatureOffline(requestNcdEtos, jsonRequestFilePath, secureCodingConfig, psdzVin, PsdzContext.FaTarget);

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

                        IEnumerable<string> cafdCalculatedInSCB = ProgrammingUtils.CafdCalculatedInSCB(requestJson);
                        log.InfoFormat(CultureInfo.InvariantCulture, "Cafd in SCB: {0}", cafdCalculatedInSCB.Count());
                        foreach (string cafd in cafdCalculatedInSCB)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Cafd: {0}", cafd);
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        log.InfoFormat(CultureInfo.InvariantCulture, "Requesting SWE list");
                        IEnumerable<IPsdzSgbmId> sweList = ProgrammingService.Psdz.LogicService.RequestSweList(PsdzContext.Tal, true);
                        log.InfoFormat(CultureInfo.InvariantCulture, "Swe list: {0}", sweList.Count());
                        foreach (IPsdzSgbmId psdzSgbmId in sweList)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString);
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        IEnumerable<IPsdzSgbmId> sgbmIds = ProgrammingUtils.RemoveCafdsCalculatedOnSCB(cafdCalculatedInSCB, sweList);
                        IEnumerable<IPsdzSgbmId> softwareEntries = ProgrammingService.Psdz.MacrosService.CheckSoftwareEntries(sgbmIds);
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

                        sbResult.AppendLine(Strings.ExecutingBackupTal);
                        UpdateStatus(sbResult.ToString());

                        CacheResponseType = CacheType.NoResponse;
                        log.InfoFormat(CultureInfo.InvariantCulture, "Executing backup TAL");
                        TalExecutionSettings talExecutionSettings = ProgrammingUtils.GetTalExecutionSettings(ProgrammingService);
                        talExecutionSettings.Parallel = false;
                        talExecutionSettings.TaMaxRepeat = 3;
                        ((PsdzSecureCodingConfigCto)talExecutionSettings.SecureCodingConfig).ConnectionTimeout = CodingConnectionTimeout;

                        IPsdzTal backupTalResult = ProgrammingService.Psdz.IndividualDataRestoreService.ExecuteAsyncBackupTal(
                            PsdzContext.Connection, PsdzContext.IndividualDataBackupTal, null, PsdzContext.FaTarget, psdzVin, talExecutionSettings, PsdzContext.PathToBackupData);
                        if (backupTalResult == null)
                        {
                            log.ErrorFormat("Execute backup TAL failed");
                            sbResult.AppendLine(Strings.TalExecuteError);
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        bool backupFailed = false;
                        log.Info("Backup Tal result:");
                        log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", backupTalResult.AsXml.Length);
                        log.InfoFormat(CultureInfo.InvariantCulture, " State: {0}", backupTalResult.TalExecutionState);
                        log.InfoFormat(CultureInfo.InvariantCulture, " Ecus: {0}", backupTalResult.AffectedEcus.Count());
                        foreach (IPsdzEcuIdentifier ecuIdentifier in backupTalResult.AffectedEcus)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                        }
                        if (backupTalResult.TalExecutionState != PsdzTalExecutionState.Finished &&
                            backupTalResult.TalExecutionState != PsdzTalExecutionState.FinishedWithWarnings)
                        {
                            talExecutionFailed = true;
                            backupFailed = true;
                            log.Error(backupTalResult.AsXml);
                            sbResult.AppendLine(Strings.TalExecuteError);
                            UpdateStatus(sbResult.ToString());
                        }
                        else
                        {
                            if (backupTalResult.TalExecutionState != PsdzTalExecutionState.Finished)
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

                        if (!LicenseValid)
                        {
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
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecuteContinue aborted");
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (!CheckVoltage(cts, sbResult))
                            {
                                return false;
                            }

                            if (ShowMessageEvent != null)
                            {
                                if (!ShowMessageEvent.Invoke(cts, Strings.TalExecuteOkContinue, false, true))
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecuteContinue aborted");
                                    return false;
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
                            return false;
                        }

                        sbResult.AppendLine(Strings.ExecutingTal);
                        UpdateStatus(sbResult.ToString());
                        log.InfoFormat(CultureInfo.InvariantCulture, "Executing TAL");
                        IPsdzTal executeTalResult = ProgrammingService.Psdz.TalExecutionService.ExecuteTal(PsdzContext.Connection, PsdzContext.Tal,
                            null, psdzVin, PsdzContext.FaTarget, talExecutionSettings, PsdzContext.PathToBackupData, cts.Token);
                        log.Info("Execute Tal result:");
                        log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", executeTalResult.AsXml.Length);
                        log.InfoFormat(CultureInfo.InvariantCulture, " State: {0}", executeTalResult.TalExecutionState);
                        log.InfoFormat(CultureInfo.InvariantCulture, " Ecus: {0}", executeTalResult.AffectedEcus.Count());
                        foreach (IPsdzEcuIdentifier ecuIdentifier in executeTalResult.AffectedEcus)
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset);
                        }
                        if (executeTalResult.TalExecutionState != PsdzTalExecutionState.Finished &&
                            executeTalResult.TalExecutionState != PsdzTalExecutionState.FinishedWithWarnings)
                        {
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
                            if (executeTalResult.TalExecutionState != PsdzTalExecutionState.Finished)
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

                        if (!backupFailed)
                        {
                            sbResult.AppendLine(Strings.ExecutingRestoreTal);
                            UpdateStatus(sbResult.ToString());

                            if (!CheckVoltage(cts, sbResult))
                            {
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

                                if (restoreTalResult.TalExecutionState != PsdzTalExecutionState.Finished &&
                                    restoreTalResult.TalExecutionState != PsdzTalExecutionState.FinishedWithWarnings)
                                {
                                    talExecutionFailed = true;
                                    log.Error(restoreTalResult.AsXml);
                                    sbResult.AppendLine(Strings.TalExecuteError);
                                    UpdateStatus(sbResult.ToString());
                                }
                                else
                                {
                                    if (restoreTalResult.TalExecutionState != PsdzTalExecutionState.Finished)
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
                                    return false;
                                }

                                CacheClearRequired = true;
                                cts?.Token.ThrowIfCancellationRequested();
                            }
                        }

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Updating TSL");
                            ProgrammingService.Psdz.ProgrammingService.TslUpdate(PsdzContext.Connection, true, PsdzContext.SvtActual, PsdzContext.Sollverbauung.Svt);
                            sbResult.AppendLine(Strings.TslUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
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
                            ProgrammingService.Psdz.VcmService.WriteIStufen(PsdzContext.Connection, PsdzContext.IstufeShipment, PsdzContext.IstufeLast, PsdzContext.IstufeCurrent);
                            sbResult.AppendLine(Strings.ILevelUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
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
                            ProgrammingService.Psdz.VcmService.WriteIStufenToBackup(PsdzContext.Connection, PsdzContext.IstufeShipment, PsdzContext.IstufeLast, PsdzContext.IstufeCurrent);
                            sbResult.AppendLine(Strings.ILevelBackupUpdated);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "Write ILevel backup failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.ILevelBackupFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        log.InfoFormat(CultureInfo.InvariantCulture, "Updating PIA master");
                        IPsdzResponse piaResponse = ProgrammingService.Psdz.EcuService.UpdatePiaPortierungsmaster(PsdzContext.Connection, PsdzContext.SvtActual);
                        log.ErrorFormat(CultureInfo.InvariantCulture, "PIA master update Success={0}, Cause={1}",
                            piaResponse.IsSuccessful, piaResponse.Cause);

                        sbResult.AppendLine(piaResponse.IsSuccessful ? Strings.PiaMasterUpdated : Strings.PiaMasterUpdateFailed);
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Writing FA");
                            ProgrammingService.Psdz.VcmService.WriteFa(PsdzContext.Connection, PsdzContext.FaTarget);
                            sbResult.AppendLine(Strings.FaWritten);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
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
                            ProgrammingService.Psdz.VcmService.WriteFaToBackup(PsdzContext.Connection, PsdzContext.FaTarget);
                            sbResult.AppendLine(Strings.FaBackupWritten);
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            talExecutionFailed = true;
                            log.ErrorFormat(CultureInfo.InvariantCulture, "FA backup write failure: {0}", ex.Message);
                            sbResult.AppendLine(Strings.FaBackupWriteFailed);
                            UpdateStatus(sbResult.ToString());
                        }

                        CacheClearRequired = true;
                        cts?.Token.ThrowIfCancellationRequested();

                        if (!talExecutionFailed)
                        {
                            // finally reset TAL
                            PsdzContext.Tal = null;
                            UpdateOptions(null);
                        }
                        else
                        {
                            if (ShowMessageEvent != null)
                            {
                                if (!ShowMessageEvent.Invoke(cts, Strings.TalExecutionFailMessage, true, true))
                                {
                                    log.ErrorFormat(CultureInfo.InvariantCulture, "ShowMessageEvent TalExecutionFailMessage aborted");
                                    return false;
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
                    return false;
                }

                bool bModifyFa = operationType == OperationType.BuildTalModFa;
                List<int> diagAddrList = new List<int>();
                PdszDatabase.SwiRegisterGroup swiRegisterGroupSelect = PdszDatabase.SwiRegisterGroup.Modification;
                foreach (OptionsItem optionsItem in SelectedOptions)
                {
                    PdszDatabase.SwiRegisterGroup swiRegisterGroup = PdszDatabase.GetSwiRegisterGroup(optionsItem.SwiRegisterEnum);
                    switch (swiRegisterGroup)
                    {
                        case PdszDatabase.SwiRegisterGroup.HwDeinstall:
                        case PdszDatabase.SwiRegisterGroup.HwInstall:
                            if (swiRegisterGroupSelect == PdszDatabase.SwiRegisterGroup.Modification)
                            {
                                swiRegisterGroupSelect = swiRegisterGroup;
                            }
                            diagAddrList.Add((int) optionsItem.EcuInfo.Address);
                            break;
                    }
                }

                IPsdzTalFilter psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
                if (swiRegisterGroupSelect == PdszDatabase.SwiRegisterGroup.Modification)
                {
                    // disable backup
                    psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.FscBackup }, TalFilterOptions.MustNot, psdzTalFilter);
                    if (bModifyFa)
                    {   // enable deploy
                        psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.CdDeploy }, TalFilterOptions.Must, psdzTalFilter);
                        log.InfoFormat(CultureInfo.InvariantCulture, "TAL flashing disabled: {0}", DisableTalFlash);
                        if (DisableTalFlash)
                        {
                            psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.BlFlash, TaCategories.GatewayTableDeploy, TaCategories.SwDeploy }, TalFilterOptions.MustNot, psdzTalFilter);
                        }
                    }
                }
                else
                {
                    bModifyFa = true;
                    switch (swiRegisterGroupSelect)
                    {
                        case PdszDatabase.SwiRegisterGroup.HwDeinstall:
                            psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForSelectedEcus(new[] { TaCategories.HwDeinstall }, diagAddrList.ToArray(), TalFilterOptions.Must, psdzTalFilter);
                            bModifyFa = true;
                            break;

                        case PdszDatabase.SwiRegisterGroup.HwInstall:
                            psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForSelectedEcus(new[] { TaCategories.HwInstall }, diagAddrList.ToArray(), TalFilterOptions.Must, psdzTalFilter);
                            bModifyFa = true;
                            break;
                    }
                }
                PsdzContext.SetTalFilter(psdzTalFilter);

                IPsdzTalFilter psdzTalFilterEmpty = ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
                PsdzContext.SetTalFilterForIndividualDataTal(psdzTalFilterEmpty);

                if (PsdzContext.TalFilter != null)
                {
                    log.Info("TalFilter:");
                    log.Info(PsdzContext.TalFilter.AsXml);
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

                PsdzContext.RemoveBackupData();
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

                ProgrammingService.PdszDatabase.ResetXepRules();

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
                PsdzContext.Vehicle.TargetILevel = psdzIstufeTarget.Value;
                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel Target: {0}", psdzIstufeTarget.Value);
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
                IPsdzSvt psdzSvt = ProgrammingService.Psdz.ObjectBuilder.BuildSvt(psdzStandardSvtNames, psdzVin.Value);
                if (psdzSvt == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Building SVT failed");
                    sbResult.AppendLine(Strings.DetectInstalledEcusFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetSvtActual(psdzSvt);
                cts?.Token.ThrowIfCancellationRequested();

                ProgrammingService.PdszDatabase.LinkSvtEcus(PsdzContext.DetectVehicle.EcuList, psdzSvt);
                List<PdszDatabase.EcuInfo> individualEcus = PsdzContext.GetEcuList(true);
                if (individualEcus != null)
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "Individual Ecus: {0}", individualEcus.Count());
                    foreach (PdszDatabase.EcuInfo ecuInfo in individualEcus)
                    {
                        if (ecuInfo != null)
                        {
                            log.Info(ecuInfo.ToString(clientContext.Language));
                        }
                    }
                }

                ProgrammingService.PdszDatabase.GetEcuVariants(PsdzContext.DetectVehicle.EcuList);
                if (!PsdzContext.UpdateVehicle(ProgrammingService))
                {
                    sbResult.AppendLine(Strings.UpdateVehicleDataFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ProgrammingService.PdszDatabase.ResetXepRules();
                log.InfoFormat(CultureInfo.InvariantCulture, "Getting ECU variants");
                ProgrammingService.PdszDatabase.GetEcuVariants(PsdzContext.DetectVehicle.EcuList, PsdzContext.Vehicle);
                log.InfoFormat(CultureInfo.InvariantCulture, "Ecu variants: {0}", PsdzContext.DetectVehicle.EcuList.Count());
                foreach (PdszDatabase.EcuInfo ecuInfo in PsdzContext.DetectVehicle.EcuList)
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

                    ProgrammingService.PdszDatabase.ReadSwiRegister(PsdzContext.Vehicle);
                    if (ProgrammingService.PdszDatabase.SwiRegisterTree != null)
                    {
                        string treeText = ProgrammingService.PdszDatabase.SwiRegisterTree.ToString(clientContext.Language);
                        if (!string.IsNullOrEmpty(treeText))
                        {
                            log.Info(Environment.NewLine + "Swi tree:" + Environment.NewLine + treeText);
                        }

                        Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict = new Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>>();
                        foreach (OptionType optionType in _optionTypes)
                        {
                            PdszDatabase.SwiRegisterEnum swiRegisterEnum = optionType.SwiRegisterEnum;
                            optionType.ClientContext = clientContext;
                            optionType.SwiRegister = ProgrammingService.PdszDatabase.FindNodeForRegister(swiRegisterEnum);

                            switch (swiRegisterEnum)
                            {
                                case PdszDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement:
                                case PdszDatabase.SwiRegisterEnum.EcuReplacementAfterReplacement:
                                {
                                    bool individualOnly = swiRegisterEnum == PdszDatabase.SwiRegisterEnum.EcuReplacementBeforeReplacement;
                                    List<PdszDatabase.EcuInfo> ecuList = PsdzContext.GetEcuList(individualOnly);
                                    if (ecuList != null)
                                    {
                                        List<OptionsItem> optionsItems = new List<OptionsItem>();
                                        foreach (PdszDatabase.EcuInfo ecuInfo in ecuList)
                                        {
                                            optionsItems.Add(new OptionsItem(swiRegisterEnum, ecuInfo, clientContext));
                                        }
                                        optionsDict.Add(swiRegisterEnum, optionsItems);
                                    }
                                    break;
                                }

                                default:
                                {
                                    List<PdszDatabase.SwiAction> swiActions = ProgrammingService.PdszDatabase.GetSwiActionsForRegister(swiRegisterEnum, true);
                                    if (swiActions != null)
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "Swi actions: {0}", optionType.Name ?? string.Empty);
                                        List<OptionsItem> optionsItems = new List<OptionsItem>();
                                        foreach (PdszDatabase.SwiAction swiAction in swiActions)
                                        {
                                            bool testModuleValid = false;
                                            if (swiAction.SwiInfoObjs != null)
                                            {
                                                foreach (PdszDatabase.SwiInfoObj infoInfoObj in swiAction.SwiInfoObjs)
                                                {
                                                    if (infoInfoObj.LinkType == PdszDatabase.SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                                                    {
                                                        string moduleName = infoInfoObj.ModuleName;
                                                        PdszDatabase.TestModuleData testModuleData = ProgrammingService.PdszDatabase.GetTestModuleData(moduleName);
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
                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting planned construction");
                IPsdzSollverbauung psdzSollverbauung = ProgrammingService.Psdz.LogicService.GenerateSollverbauungGesamtFlash(PsdzContext.Connection, psdzIstufeTarget, psdzIstufeShip, PsdzContext.SvtActual, PsdzContext.FaTarget, talFilterFlash);
                if (psdzSollverbauung == null)
                {
                    sbResult.AppendLine(Strings.RequestedPlannedConstructionFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetSollverbauung(psdzSollverbauung);
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
                log.InfoFormat(CultureInfo.InvariantCulture, "Requesting Ecu context");
                IEnumerable<IPsdzEcuContextInfo> psdzEcuContextInfos = ProgrammingService.Psdz.EcuService.RequestEcuContextInfos(PsdzContext.Connection, psdzEcuIdentifiers);
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

                sbResult.AppendLine(Strings.TalGenrating);
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

                if (bModifyFa && programmingActionsSum.Contains(ProgrammingActionType.Programming))
                {
                    PsdzContext.Tal = null;
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
                    sbResult.AppendLine(Strings.TalGenerationFailed);
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.IndividualDataBackupTal = psdzBackupTal;
                log.Info("Backup Tal:");
                log.InfoFormat(CultureInfo.InvariantCulture, " Size: {0}", psdzBackupTal.AsXml.Length);
                cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(Strings.TalRestoreGenrating);
                UpdateStatus(sbResult.ToString());
                log.InfoFormat(CultureInfo.InvariantCulture, "Generating restore TAL");
                IPsdzTal psdzRestorePrognosisTal = ProgrammingService.Psdz.IndividualDataRestoreService.GenerateRestorePrognosisTal(PsdzContext.Connection, PsdzContext.PathToBackupData, PsdzContext.Tal, PsdzContext.IndividualDataBackupTal, PsdzContext.TalFilterForIndividualDataTal);
                if (psdzRestorePrognosisTal == null)
                {
                    PsdzContext.Tal = null;
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
            ProgrammingService.PdszDatabase.ResetXepRules();

            foreach (OptionsItem optionsItem in SelectedOptions)
            {
                if (optionsItem.SwiAction?.SwiInfoObjs != null)
                {
                    foreach (PdszDatabase.SwiInfoObj infoInfoObj in optionsItem.SwiAction.SwiInfoObjs)
                    {
                        if (infoInfoObj.LinkType == PdszDatabase.SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                        {
                            string moduleName = infoInfoObj.ModuleName;
                            PdszDatabase.TestModuleData testModuleData = ProgrammingService.PdszDatabase.GetTestModuleData(moduleName);
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
                                    PdszDatabase.SwiInfoObj swiInfoObj = ProgrammingService.PdszDatabase.GetInfoObjectByControlId(testModuleData.ModuleRef, infoInfoObj.LinkType);
                                    if (swiInfoObj == null)
                                    {
                                        log.ErrorFormat(CultureInfo.InvariantCulture, "UpdateTargetFa No info object: {0}", testModuleData.ModuleRef);
                                    }
                                    else
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "UpdateTargetFa Info object: {0}", swiInfoObj.ToString(ClientContext.GetLanguage(PsdzContext.Vehicle)));
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
                                ProgrammingService.PdszDatabase.ResetXepRules();
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

        private bool CheckVoltage(CancellationTokenSource cts, StringBuilder sbResult, bool showInfo = false, bool addMessage = false)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage vehicle: Show info={0}", showInfo);
            if (PsdzContext.Vehicle == null)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "CheckVoltage No vehicle");
                return false;
            }

            CacheType cacheTypeOld = CacheResponseType;
            try
            {
                CacheResponseType = CacheType.NoResponse;
                for (; ; )
                {
                    double voltage = PsdzContext.DetectVehicle.ReadBatteryVoltage(() =>
                    {
                        if (cts != null)
                        {
                            return cts.Token.IsCancellationRequested;
                        }
                        return false;
                    });

                    log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage: Battery voltage={0}", voltage);

                    bool lfpBattery = PsdzContext.Vehicle.WithLfpBattery;
                    double minVoltageError = lfpBattery ? MinBatteryVoltageErrorLfp : MinBatteryVoltageErrorPb;
                    double minVoltageWarn = lfpBattery ? MinBatteryVoltageWarnLfp : MinBatteryVoltageWarnPb;
                    double maxVoltageWarn = lfpBattery ? MaxBatteryVoltageWarnLfp : MaxBatteryVoltageWarnPb;
                    double maxVoltageError = lfpBattery ? MaxBatteryVoltageErrorLfp : MaxBatteryVoltageErrorPb;
                    log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage: LFP={0}, MinErr={1}, MinWarn={2}, MaxWarn={3}, MaxErr={4}",
                        lfpBattery, minVoltageError, minVoltageWarn, maxVoltageWarn, maxVoltageError);

                    bool warn = voltage < minVoltageError || voltage > maxVoltageError;
                    bool error = voltage < minVoltageWarn || voltage > maxVoltageWarn;
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
                PsdzContext.DetectVehicle.Disconnect();
                CacheResponseType = cacheTypeOld;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "CheckVoltage OK");
            return true;
        }

        public bool InitProgrammingObjects(string istaFolder)
        {
            try
            {
                PsdzContext = new PsdzContext(istaFolder);
                DisableTalFlash = false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void ClearProgrammingObjects()
        {
            if (PsdzContext != null)
            {
                PsdzContext.Dispose();
                PsdzContext = null;
            }
        }

        public void UpdateStatus(string message = null)
        {
            UpdateStatusEvent?.Invoke(message);
        }

        private void UpdateOptions(Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict)
        {
            UpdateOptionsEvent?.Invoke(optionsDict);
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

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
