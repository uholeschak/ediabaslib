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

        public class OptionsItem
        {
            public OptionsItem(PdszDatabase.SwiAction swiAction)
            {
                SwiAction = swiAction;
                Invalid = false;
            }

            public PdszDatabase.SwiAction SwiAction { get; private set; }

            public bool Invalid { get; set; }

            public override string ToString()
            {
                return SwiAction.EcuTranslation.GetTitle(ClientContext.GetClientContext().Language);
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

            public override string ToString()
            {
                PdszDatabase.SwiRegister swiRegister = SwiRegister;
                if (swiRegister != null)
                {
                    return swiRegister.EcuTranslation.GetTitle(ClientContext.GetClientContext().Language);
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

        private static readonly ILog log = LogManager.GetLogger(typeof(ProgrammingJobs));
        private static OptionType[] _optionTypes =
        {
            new OptionType("Coding", PdszDatabase.SwiRegisterEnum.VehicleModificationCodingConversion),
            new OptionType("Coding back", PdszDatabase.SwiRegisterEnum.VehicleModificationCodingBackConversion),
            new OptionType("Modification", PdszDatabase.SwiRegisterEnum.VehicleModificationConversion),
            new OptionType("Modification back", PdszDatabase.SwiRegisterEnum.VehicleModificationBackConversion),
            new OptionType("Retrofit", PdszDatabase.SwiRegisterEnum.VehicleModificationRetrofitting),
        };
        public static OptionType[] OptionTypes => _optionTypes;

        private bool _disposed;
        private string _dealerId;
        public PsdzContext PsdzContext { get; private set; }
        public ProgrammingService ProgrammingService { get; private set; }
        public List<ProgrammingJobs.OptionsItem> SelectedOptions { get; set; }


        public ProgrammingJobs(string dealerId)
        {
            _dealerId = dealerId;
            ProgrammingService = null;
        }

        public bool StartProgrammingService(CancellationTokenSource cts, string istaFolder)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine("Starting programming service");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "DealerId={0}", _dealerId));
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService != null && ProgrammingService.IsPsdzPsdzServiceHostInitialized())
                {
                    if (!StopProgrammingService(cts))
                    {
                        sbResult.AppendLine("Stop host failed");
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }
                }

                ProgrammingService = new ProgrammingService(istaFolder, _dealerId);
                SetupLog4Net();
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
                            string message = string.Format(CultureInfo.InvariantCulture, "{0}%, {1}s", progress, programmingEventArgs.TimeLeftSec);
                            ProgressEvent?.Invoke(progress, false, message);
                        }
                    }
                };

                sbResult.AppendLine("Generating test module data ...");
                UpdateStatus(sbResult.ToString());
                bool result = ProgrammingService.PdszDatabase.GenerateTestModuleData(progress =>
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "{0}%", progress);
                    ProgressEvent?.Invoke(progress, false, message);

                    if (cts != null)
                    {
                        return cts.Token.IsCancellationRequested;
                    }
                    return false;
                });

                ProgressEvent?.Invoke(0, true);

                if (!result)
                {
                    sbResult.AppendLine("Generating test module data failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine("Starting host ...");
                UpdateStatus(sbResult.ToString());
                if (!ProgrammingService.StartPsdzServiceHost())
                {
                    sbResult.AppendLine("Start host failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ProgrammingService.SetLogLevelToMax();
                sbResult.AppendLine("Host started");
                UpdateStatus(sbResult.ToString());

                ProgrammingService.PdszDatabase.ResetXepRules();
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
        }

        public bool StopProgrammingService(CancellationTokenSource cts)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine("Stopping host ...");
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService != null)
                {
                    ProgrammingService.Psdz.Shutdown();
                    ProgrammingService.CloseConnectionsToPsdzHost();
                    ProgrammingService.Dispose();
                    ProgrammingService = null;
                    ClearProgrammingObjects();
                }

                sbResult.AppendLine("Host stopped");
                UpdateStatus(sbResult.ToString());
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }

            return true;
        }

        public bool ConnectVehicle(CancellationTokenSource cts, string istaFolder, string ipAddress, bool icomConnection)
        {
            log.InfoFormat("ConnectVehicle Start - Ip: {0}, ICOM: {1}", ipAddress, icomConnection);
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine("Connecting vehicle ...");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ip={0}, ICOM={1}",
                    ipAddress, icomConnection));
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService == null)
                {
                    return false;
                }

                if (!InitProgrammingObjects(istaFolder))
                {
                    return false;
                }

                sbResult.AppendLine("Detecting vehicle ...");
                UpdateStatus(sbResult.ToString());

                string ecuPath = Path.Combine(istaFolder, @"Ecu");
                int diagPort = icomConnection ? 50160 : 6801;
                int controlPort = icomConnection ? 50161 : 6811;
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

                PsdzContext.DetectVehicle = new DetectVehicle(ecuPath, enetConnection);
                PsdzContext.DetectVehicle.AbortRequest += () =>
                {
                    if (cts != null)
                    {
                        return cts.Token.IsCancellationRequested;
                    }
                    return false;
                };

                bool detectResult = PsdzContext.DetectVehicle.DetectVehicleBmwFast();
                cts?.Token.ThrowIfCancellationRequested();
                if (!detectResult)
                {
                    sbResult.AppendLine("Vehicle detection failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Detected vehicle: VIN={0}, GroupFile={1}, BR={2}, Series={3}, BuildDate={4}-{5}",
                    PsdzContext.DetectVehicle.Vin ?? string.Empty, PsdzContext.DetectVehicle.GroupSgdb ?? string.Empty,
                    PsdzContext.DetectVehicle.ModelSeries ?? string.Empty, PsdzContext.DetectVehicle.Series ?? string.Empty,
                    PsdzContext.DetectVehicle.ConstructYear ?? string.Empty, PsdzContext.DetectVehicle.ConstructMonth ?? string.Empty));
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Detected ILevel: Ship={0}, Current={1}, Backup={2}",
                    PsdzContext.DetectVehicle.ILevelShip ?? string.Empty, PsdzContext.DetectVehicle.ILevelCurrent ?? string.Empty,
                    PsdzContext.DetectVehicle.ILevelBackup ?? string.Empty));

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecus: {0}", PsdzContext.DetectVehicle.EcuList.Count()));
                foreach (PdszDatabase.EcuInfo ecuInfo in PsdzContext.DetectVehicle.EcuList)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecu: Name={0}, Addr={1}, Sgdb={2}, Group={3}",
                        ecuInfo.Name, ecuInfo.Address, ecuInfo.Sgbd, ecuInfo.Grp));
                }

                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                string series = PsdzContext.DetectVehicle.Series;
                if (string.IsNullOrEmpty(series))
                {
                    sbResult.AppendLine("Vehicle series not detected");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                string mainSeries = ProgrammingService.Psdz.ConfigurationService.RequestBaureihenverbund(series);
                IEnumerable<IPsdzTargetSelector> targetSelectors =
                    ProgrammingService.Psdz.ConnectionFactoryService.GetTargetSelectors();
                PsdzContext.TargetSelectors = targetSelectors;
                TargetSelectorChooser targetSelectorChooser = new TargetSelectorChooser(PsdzContext.TargetSelectors);
                IPsdzTargetSelector psdzTargetSelectorNewest =
                    targetSelectorChooser.GetNewestTargetSelectorByMainSeries(mainSeries);
                if (psdzTargetSelectorNewest == null)
                {
                    sbResult.AppendLine("No target selector");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.ProjectName = psdzTargetSelectorNewest.Project;
                PsdzContext.VehicleInfo = psdzTargetSelectorNewest.VehicleInfo;
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Target selector: Project={0}, Vehicle={1}, Series={2}",
                    psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo,
                    psdzTargetSelectorNewest.Baureihenverbund));
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                string bauIStufe = PsdzContext.DetectVehicle.ILevelShip;
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel shipment: {0}", bauIStufe));

                string url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", ipAddress, diagPort);
                IPsdzConnection psdzConnection;
                if (icomConnection)
                {
                    psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverIcom(
                        psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, 1000, series,
                        bauIStufe, IcomConnectionType.Ip, false);
                }
                else
                {
                    psdzConnection = ProgrammingService.Psdz.ConnectionManagerService.ConnectOverEthernet(
                        psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, series,
                        bauIStufe);
                }

                Vehicle vehicle = new Vehicle();
                vehicle.VCI.VCIType = icomConnection ?
                    BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ICOM : BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ENET;
                vehicle.VCI.IPAddress = ipAddress;
                vehicle.VCI.Port = diagPort;
                vehicle.VCI.NetworkType = "LAN";
                vehicle.VCI.VIN = PsdzContext.DetectVehicle.Vin;
                PsdzContext.Vehicle = vehicle;

                ProgrammingService.CreateEcuProgrammingInfos(PsdzContext.Vehicle);
                PsdzContext.Connection = psdzConnection;

                sbResult.AppendLine("Vehicle connected");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Connection: Id={0}, Port={1}",
                    psdzConnection.Id, psdzConnection.Port));

                UpdateStatus(sbResult.ToString());

                ProgrammingService.AddListener(PsdzContext);
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
            finally
            {
                log.InfoFormat("ConnectVehicle Finish - Ip: {0}, ICOM: {1}", ipAddress, icomConnection);
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
                sbResult.AppendLine("Disconnecting vehicle ...");
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService == null)
                {
                    sbResult.AppendLine("No Host");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (PsdzContext?.Connection == null)
                {
                    sbResult.AppendLine("No connection");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                ProgrammingService.RemoveListener();
                ProgrammingService.Psdz.ConnectionManagerService.CloseConnection(PsdzContext.Connection);

                ClearProgrammingObjects();
                sbResult.AppendLine("Vehicle disconnected");
                UpdateStatus(sbResult.ToString());
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
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
            log.InfoFormat("VehicleFunctions Start - Type: {0}", operationType);
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine("Executing vehicle functions ...");
                UpdateStatus(sbResult.ToString());

                if (ProgrammingService == null)
                {
                    sbResult.AppendLine("No Host");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (PsdzContext?.Connection == null)
                {
                    sbResult.AppendLine("No connection");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzVin psdzVin = ProgrammingService.Psdz.VcmService.GetVinFromMaster(PsdzContext.Connection);
                if (string.IsNullOrEmpty(psdzVin?.Value))
                {
                    sbResult.AppendLine("Reading VIN failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Vin: {0}", psdzVin.Value));
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                if (operationType == OperationType.ExecuteTal)
                {
                    sbResult.AppendLine("Execute TAL");
                    UpdateStatus(sbResult.ToString());

                    if (PsdzContext.Tal == null)
                    {
                        sbResult.AppendLine("No TAL present");
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    DateTime calculationStartTime = DateTime.Now;
                    IEnumerable<IPsdzEcuIdentifier> psdzEcuIdentifiersPrg = ProgrammingService.Psdz.ProgrammingService.CheckProgrammingCounter(PsdzContext.Connection, PsdzContext.Tal);
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ProgCounter: {0}", psdzEcuIdentifiersPrg.Count()));
                    foreach (IPsdzEcuIdentifier ecuIdentifier in psdzEcuIdentifiersPrg)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                            ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                    }
                    UpdateStatus(sbResult.ToString());
                    cts?.Token.ThrowIfCancellationRequested();

                    PsdzSecureCodingConfigCto secureCodingConfig = SecureCodingConfigWrapper.GetSecureCodingConfig(ProgrammingService);
                    IPsdzCheckNcdResultEto psdzCheckNcdResultEto = ProgrammingService.Psdz.SecureCodingService.CheckNcdAvailabilityForGivenTal(PsdzContext.Tal, secureCodingConfig.NcdRootDirectory, psdzVin);
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ncd EachSigned: {0}", psdzCheckNcdResultEto.isEachNcdSigned));
                    foreach (IPsdzDetailedNcdInfoEto detailedNcdInfo in psdzCheckNcdResultEto.DetailedNcdStatus)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ncd: Btld={0}, Cafd={1}, Status={2}",
                            detailedNcdInfo.Btld.HexString, detailedNcdInfo.Cafd.HexString, detailedNcdInfo.NcdStatus));
                    }
                    UpdateStatus(sbResult.ToString());
                    cts?.Token.ThrowIfCancellationRequested();

                    List<IPsdzRequestNcdEto> requestNcdEtos = ProgrammingUtils.CreateRequestNcdEtos(psdzCheckNcdResultEto);
#if false
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ncd Requests: {0}", requestNcdEtos.Count));
                    foreach (IPsdzRequestNcdEto requestNcdEto in requestNcdEtos)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ncd for Cafd={0}, Btld={1}",
                            requestNcdEto.Cafd.Id, requestNcdEto.Btld.HexString));
                    }
                    UpdateStatus(sbResult.ToString());
#endif
                    string secureCodingPath = SecureCodingConfigWrapper.GetSecureCodingPathWithVin(ProgrammingService, psdzVin.Value);
                    string jsonRequestFilePath = Path.Combine(secureCodingPath, string.Format(CultureInfo.InvariantCulture, "SecureCodingNCDCalculationRequest_{0}_{1}_{2}.json",
                        psdzVin.Value, _dealerId, calculationStartTime.ToString("HHmmss", CultureInfo.InvariantCulture)));
                    PsdzBackendNcdCalculationEtoEnum backendNcdCalculationEtoEnumOld = secureCodingConfig.BackendNcdCalculationEtoEnum;
                    try
                    {
                        secureCodingConfig.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.ALLOW;
                        IList<IPsdzSecurityBackendRequestFailureCto> psdzSecurityBackendRequestFailureList = ProgrammingService.Psdz.SecureCodingService.RequestCalculationNcdAndSignatureOffline(requestNcdEtos, jsonRequestFilePath, secureCodingConfig, psdzVin, PsdzContext.FaTarget);
                        int failureCount = psdzSecurityBackendRequestFailureList.Count;
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ncd failures: {0}", failureCount));
                        foreach (IPsdzSecurityBackendRequestFailureCto psdzSecurityBackendRequestFailure in psdzSecurityBackendRequestFailureList)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Failure: Cause={0}, Retry={1}, Url={2}",
                                psdzSecurityBackendRequestFailure.Cause, psdzSecurityBackendRequestFailure.Retry, psdzSecurityBackendRequestFailure.Url));
                        }
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        if (failureCount > 0)
                        {
                            sbResult.AppendLine("Ncd failures present");
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        if (!File.Exists(jsonRequestFilePath))
                        {
                            sbResult.AppendLine("Ncd request file not generated");
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        RequestJson requestJson = new JsonHelper().ReadRequestJson(jsonRequestFilePath);
                        if (requestJson == null || !ProgrammingUtils.CheckIfThereAreAnyNcdInTheRequest(requestJson))
                        {
                            sbResult.AppendLine("No ecu data in the request json file. Ncd calculation not required");
                        }
                        else
                        {
                            sbResult.AppendLine("Ncd online calculation required, aborting");
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        IEnumerable<string> cafdCalculatedInSCB = ProgrammingUtils.CafdCalculatedInSCB(requestJson);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Cafd in SCB: {0}", cafdCalculatedInSCB.Count()));
                        foreach (string cafd in cafdCalculatedInSCB)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Cafd: {0}", cafd));
                        }
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        IEnumerable<IPsdzSgbmId> sweList = ProgrammingService.Psdz.LogicService.RequestSweList(PsdzContext.Tal, true);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Swe list: {0}", sweList.Count()));
                        foreach (IPsdzSgbmId psdzSgbmId in sweList)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString));
                        }
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        IEnumerable<IPsdzSgbmId> sgbmIds = ProgrammingUtils.RemoveCafdsCalculatedOnSCB(cafdCalculatedInSCB, sweList);
                        IEnumerable<IPsdzSgbmId> softwareEntries = ProgrammingService.Psdz.MacrosService.CheckSoftwareEntries(sgbmIds);
                        int softwareEntryCount = softwareEntries.Count();
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Sw entries: {0}", softwareEntryCount));
                        foreach (IPsdzSgbmId psdzSgbmId in softwareEntries)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString));
                        }
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        if (softwareEntryCount > 0)
                        {
                            sbResult.AppendLine("Software failures present");
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        sbResult.AppendLine("Executing Backup Tal ...");
                        UpdateStatus(sbResult.ToString());
                        TalExecutionSettings talExecutionSettings = ProgrammingUtils.GetTalExecutionSettings(ProgrammingService);
                        IPsdzTal backupTalResult = ProgrammingService.Psdz.IndividualDataRestoreService.ExecuteAsyncBackupTal(
                            PsdzContext.Connection, PsdzContext.IndividualDataBackupTal, null, PsdzContext.FaTarget, psdzVin, talExecutionSettings, PsdzContext.PathToBackupData);
                        sbResult.AppendLine("Backup Tal result:");
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", backupTalResult.AsXml.Length));
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " State: {0}", backupTalResult.TalExecutionState));
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecus: {0}", backupTalResult.AffectedEcus.Count()));
                        foreach (IPsdzEcuIdentifier ecuIdentifier in backupTalResult.AffectedEcus)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                        }
                        if (backupTalResult.TalExecutionState != PsdzTalExecutionState.Finished)
                        {
                            sbResult.AppendLine(backupTalResult.AsXml);
                        }
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        sbResult.AppendLine("Executing Tal ...");
                        UpdateStatus(sbResult.ToString());
                        IPsdzTal executeTalResult = ProgrammingService.Psdz.TalExecutionService.ExecuteTal(PsdzContext.Connection, PsdzContext.Tal,
                            null, psdzVin, PsdzContext.FaTarget, talExecutionSettings, PsdzContext.PathToBackupData, cts.Token);
                        sbResult.AppendLine("Exceute Tal result:");
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", executeTalResult.AsXml.Length));
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " State: {0}", executeTalResult.TalExecutionState));
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecus: {0}", executeTalResult.AffectedEcus.Count()));
                        foreach (IPsdzEcuIdentifier ecuIdentifier in executeTalResult.AffectedEcus)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                                ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                        }
                        if (executeTalResult.TalExecutionState != PsdzTalExecutionState.Finished)
                        {
                            sbResult.AppendLine(executeTalResult.AsXml);
                        }
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            ProgrammingService.Psdz.ProgrammingService.TslUpdate(PsdzContext.Connection, true, PsdzContext.SvtActual, PsdzContext.Sollverbauung.Svt);
                            sbResult.AppendLine("Tsl updated");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Tsl update failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            ProgrammingService.Psdz.VcmService.WriteIStufen(PsdzContext.Connection, PsdzContext.IstufeShipment, PsdzContext.IstufeLast, PsdzContext.IstufeCurrent);
                            sbResult.AppendLine("ILevel updated");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Write ILevel failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            ProgrammingService.Psdz.VcmService.WriteIStufenToBackup(PsdzContext.Connection, PsdzContext.IstufeShipment, PsdzContext.IstufeLast, PsdzContext.IstufeCurrent);
                            sbResult.AppendLine("ILevel backup updated");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Write ILevel backup failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        IPsdzResponse piaResponse = ProgrammingService.Psdz.EcuService.UpdatePiaPortierungsmaster(PsdzContext.Connection, PsdzContext.SvtActual);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "PIA master update Success={0}, Cause={1}",
                            piaResponse.IsSuccessful, piaResponse.Cause));
                        UpdateStatus(sbResult.ToString());
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            ProgrammingService.Psdz.VcmService.WriteFa(PsdzContext.Connection, PsdzContext.FaTarget);
                            sbResult.AppendLine("FA written");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "FA write failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            ProgrammingService.Psdz.VcmService.WriteFaToBackup(PsdzContext.Connection, PsdzContext.FaTarget);
                            sbResult.AppendLine("FA backup written");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "FA backup write failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        cts?.Token.ThrowIfCancellationRequested();
                    }
                    finally
                    {
                        secureCodingConfig.BackendNcdCalculationEtoEnum = backendNcdCalculationEtoEnumOld;
                        if (Directory.Exists(secureCodingPath))
                        {
                            try
                            {
                                Directory.Delete(secureCodingPath, true);
                            }
                            catch (Exception ex)
                            {
                                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Directory exception: {0}", ex.Message));
                                UpdateStatus(sbResult.ToString());
                            }
                        }
                    }

                    return true;
                }

                bool bModifyFa = operationType == OperationType.BuildTalModFa;
                IPsdzTalFilter psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
                // disable backup
                psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.FscBackup }, TalFilterOptions.MustNot, psdzTalFilter);
                if (bModifyFa)
                {   // enable deploy
                    psdzTalFilter = ProgrammingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.CdDeploy }, TalFilterOptions.Must, psdzTalFilter);
                }
                PsdzContext.SetTalFilter(psdzTalFilter);

                IPsdzTalFilter psdzTalFilterEmpty = ProgrammingService.Psdz.ObjectBuilder.BuildTalFilter();
                PsdzContext.SetTalFilterForIndividualDataTal(psdzTalFilterEmpty);

                if (PsdzContext.TalFilter != null)
                {
                    sbResult.AppendLine("TalFilter:");
                    sbResult.Append(PsdzContext.TalFilter.AsXml);
                }

                PsdzContext.CleanupBackupData();
                IPsdzIstufenTriple iStufenTriple = ProgrammingService.Psdz.VcmService.GetIStufenTripleActual(PsdzContext.Connection);
                if (iStufenTriple == null)
                {
                    sbResult.AppendLine("Reading ILevel failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                PsdzContext.SetIstufen(iStufenTriple);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel: Current={0}, Last={1}, Shipment={2}",
                    iStufenTriple.Current, iStufenTriple.Last, iStufenTriple.Shipment));

                if (!PsdzContext.SetPathToBackupData(psdzVin.Value))
                {
                    sbResult.AppendLine("Create backup path failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzStandardFa standardFa = ProgrammingService.Psdz.VcmService.GetStandardFaActual(PsdzContext.Connection);
                IPsdzFa psdzFa = ProgrammingService.Psdz.ObjectBuilder.BuildFa(standardFa, psdzVin.Value);
                PsdzContext.SetFaActual(psdzFa);
                sbResult.AppendLine("FA current:");
                sbResult.Append(psdzFa.AsXml);
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                if (bModifyFa)
                {
                    IFa ifaActual = ProgrammingUtils.BuildFa(PsdzContext.FaActual);
                    IFa ifaTarget = ProgrammingUtils.BuildFa(PsdzContext.FaTarget);
                    string compareFa = ProgrammingUtils.CompareFa(ifaActual, ifaTarget);
                    if (!string.IsNullOrEmpty(compareFa))
                    {
                        log.InfoFormat("Compare FA: {0}", compareFa);
                        UpdateStatus(sbResult.ToString());
                    }
                }
                else
                {   // reset target fa
                    PsdzContext.SetFaTarget(psdzFa);
                }

                ProgrammingService.PdszDatabase.ResetXepRules();

                IEnumerable<IPsdzIstufe> psdzIstufes = ProgrammingService.Psdz.LogicService.GetPossibleIntegrationLevel(PsdzContext.FaTarget);
                PsdzContext.SetPossibleIstufenTarget(psdzIstufes);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevels: {0}", psdzIstufes.Count()));
                foreach (IPsdzIstufe iStufe in psdzIstufes.OrderBy(x => x))
                {
                    if (iStufe.IsValid)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " ILevel: {0}", iStufe.Value));
                    }
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                string latestIstufeTarget = PsdzContext.LatestPossibleIstufeTarget;
                if (string.IsNullOrEmpty(latestIstufeTarget))
                {
                    sbResult.AppendLine("No target ILevels");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel Latest: {0}", latestIstufeTarget));

                IPsdzIstufe psdzIstufeShip = ProgrammingService.Psdz.ObjectBuilder.BuildIstufe(PsdzContext.IstufeShipment);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel Ship: {0}", psdzIstufeShip.Value));

                IPsdzIstufe psdzIstufeTarget = ProgrammingService.Psdz.ObjectBuilder.BuildIstufe(bModifyFa ? PsdzContext.IstufeCurrent : latestIstufeTarget);
                PsdzContext.Vehicle.TargetILevel = psdzIstufeTarget.Value;
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel Target: {0}", psdzIstufeTarget.Value));
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IEnumerable<IPsdzEcuIdentifier> psdzEcuIdentifiers = ProgrammingService.Psdz.MacrosService.GetInstalledEcuList(PsdzContext.FaActual, psdzIstufeShip);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "EcuIds: {0}", psdzEcuIdentifiers.Count()));
                foreach (IPsdzEcuIdentifier ecuIdentifier in psdzEcuIdentifiers)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                        ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzStandardSvt psdzStandardSvt = ProgrammingService.Psdz.EcuService.RequestSvt(PsdzContext.Connection, psdzEcuIdentifiers);
                IPsdzStandardSvt psdzStandardSvtNames = ProgrammingService.Psdz.LogicService.FillBntnNamesForMainSeries(PsdzContext.Connection.TargetSelector.Baureihenverbund, psdzStandardSvt);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Svt Ecus: {0}", psdzStandardSvtNames.Ecus.Count()));
                foreach (IPsdzEcu ecu in psdzStandardSvtNames.Ecus)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Variant: BaseVar={0}, Var={1}, Name={2}",
                        ecu.BaseVariant, ecu.EcuVariant, ecu.BnTnName));
                }

                IPsdzSvt psdzSvt = ProgrammingService.Psdz.ObjectBuilder.BuildSvt(psdzStandardSvtNames, psdzVin.Value);
                PsdzContext.SetSvtActual(psdzSvt);
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                ProgrammingService.PdszDatabase.LinkSvtEcus(PsdzContext.DetectVehicle.EcuList, psdzSvt);
                ProgrammingService.PdszDatabase.GetEcuVariants(PsdzContext.DetectVehicle.EcuList);
                if (!PsdzContext.UpdateVehicle(ProgrammingService, psdzStandardSvtNames))
                {
                    sbResult.AppendLine("UpdateVehicle failed");
                    UpdateStatus(sbResult.ToString());
                    return true;
                }

                ProgrammingService.PdszDatabase.ResetXepRules();
                ProgrammingService.PdszDatabase.GetEcuVariants(PsdzContext.DetectVehicle.EcuList, PsdzContext.Vehicle);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecus: {0}", PsdzContext.DetectVehicle.EcuList.Count()));
                foreach (PdszDatabase.EcuInfo ecuInfo in PsdzContext.DetectVehicle.EcuList)
                {
                    sbResult.AppendLine(ecuInfo.ToString(ClientContext.GetClientContext(PsdzContext.Vehicle).Language));
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();
                if (operationType == OperationType.CreateOptions)
                {
                    ProgrammingService.PdszDatabase.ReadSwiRegister(PsdzContext.Vehicle);
                    if (ProgrammingService.PdszDatabase.SwiRegisterTree != null)
                    {
                        string treeText = ProgrammingService.PdszDatabase.SwiRegisterTree.ToString(ClientContext.GetClientContext(PsdzContext.Vehicle).Language);
                        if (!string.IsNullOrEmpty(treeText))
                        {
                            log.Info(Environment.NewLine + "Swi tree:" + Environment.NewLine + treeText);
                        }

                        Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict = new Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>>();
                        foreach (OptionType optionType in _optionTypes)
                        {
                            optionType.SwiRegister = ProgrammingService.PdszDatabase.FindNodeForRegister(optionType.SwiRegisterEnum);
                            List<PdszDatabase.SwiAction> swiActions = ProgrammingService.PdszDatabase.GetSwiActionsForRegister(optionType.SwiRegisterEnum, true);
                            if (swiActions != null)
                            {
                                sbResult.AppendLine();
                                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Swi actions: {0}", optionType.Name));
                                List<OptionsItem> optionsItems = new List<OptionsItem>();
                                foreach (PdszDatabase.SwiAction swiAction in swiActions)
                                {
                                    sbResult.AppendLine(swiAction.ToString(ClientContext.GetClientContext(PsdzContext.Vehicle).Language));
                                    optionsItems.Add(new OptionsItem(swiAction));
                                }

                                optionsDict.Add(optionType.SwiRegisterEnum, optionsItems);
                            }
                        }

                        UpdateOptions(optionsDict);
                    }

                    UpdateStatus(sbResult.ToString());
                    return true;
                }

                IPsdzReadEcuUidResultCto psdzReadEcuUid = ProgrammingService.Psdz.SecurityManagementService.readEcuUid(PsdzContext.Connection, psdzEcuIdentifiers, PsdzContext.SvtActual);

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "EcuUids: {0}", psdzReadEcuUid.EcuUids.Count));
                foreach (KeyValuePair<IPsdzEcuIdentifier, IPsdzEcuUidCto> ecuUid in psdzReadEcuUid.EcuUids)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Uid={3}",
                        ecuUid.Key.BaseVariant, ecuUid.Key.DiagAddrAsInt, ecuUid.Key.DiagnosisAddress.Offset, ecuUid.Value.EcuUid));
                }
#if false
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "EcuUid failures: {0}", psdzReadEcuUid.FailureResponse.Count()));
                foreach (IPsdzEcuFailureResponseCto failureResponse in psdzReadEcuUid.FailureResponse)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Fail: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Cause={3}",
                        failureResponse.EcuIdentifierCto.BaseVariant, failureResponse.EcuIdentifierCto.DiagAddrAsInt, failureResponse.EcuIdentifierCto.DiagnosisAddress.Offset,
                        failureResponse.Cause.Description));
                }
#endif
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzReadStatusResultCto psdzReadStatusResult = ProgrammingService.Psdz.SecureFeatureActivationService.ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum.ALL_FEATURES, PsdzContext.Connection, PsdzContext.SvtActual, psdzEcuIdentifiers, true, 3, 100);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Status failures: {0}", psdzReadStatusResult.Failures.Count()));
#if false
                foreach (IPsdzEcuFailureResponseCto failureResponse in psdzReadStatusResult.Failures)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Fail: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Cause={3}",
                        failureResponse.EcuIdentifierCto.BaseVariant, failureResponse.EcuIdentifierCto.DiagAddrAsInt, failureResponse.EcuIdentifierCto.DiagnosisAddress.Offset,
                        failureResponse.Cause.Description));
                }
#endif
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Status features: {0}", psdzReadStatusResult.FeatureStatusSet.Count()));
                foreach (IPsdzFeatureLongStatusCto featureLongStatus in psdzReadStatusResult.FeatureStatusSet)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Feature: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Status={3}, Token={4}",
                        featureLongStatus.EcuIdentifierCto.BaseVariant, featureLongStatus.EcuIdentifierCto.DiagAddrAsInt, featureLongStatus.EcuIdentifierCto.DiagnosisAddress.Offset,
                        featureLongStatus.FeatureStatusEto, featureLongStatus.TokenId));
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzTalFilter talFilterFlash = new PsdzTalFilter();
                IPsdzSollverbauung psdzSollverbauung = ProgrammingService.Psdz.LogicService.GenerateSollverbauungGesamtFlash(PsdzContext.Connection, psdzIstufeTarget, psdzIstufeShip, PsdzContext.SvtActual, PsdzContext.FaTarget, talFilterFlash);
                PsdzContext.SetSollverbauung(psdzSollverbauung);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Target construction: Count={0}, Units={1}",
                    psdzSollverbauung.PsdzOrderList.BntnVariantInstances.Length, psdzSollverbauung.PsdzOrderList.NumberOfUnits));
                foreach (IPsdzEcuVariantInstance bntnVariant in psdzSollverbauung.PsdzOrderList.BntnVariantInstances)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Variant: BaseVar={0}, Var={1}, Name={2}",
                        bntnVariant.Ecu.BaseVariant, bntnVariant.Ecu.EcuVariant, bntnVariant.Ecu.BnTnName));
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IEnumerable<IPsdzEcuContextInfo> psdzEcuContextInfos = ProgrammingService.Psdz.EcuService.RequestEcuContextInfos(PsdzContext.Connection, psdzEcuIdentifiers);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecu contexts: {0}", psdzEcuContextInfos.Count()));
                foreach (IPsdzEcuContextInfo ecuContextInfo in psdzEcuContextInfos)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecu context: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, ManuDate={3}, PrgDate={4}, PrgCnt={5}, FlashCnt={6}, FlashRemain={7}",
                        ecuContextInfo.EcuId.BaseVariant, ecuContextInfo.EcuId.DiagAddrAsInt, ecuContextInfo.EcuId.DiagnosisAddress.Offset,
                        ecuContextInfo.ManufacturingDate, ecuContextInfo.LastProgrammingDate, ecuContextInfo.ProgramCounter, ecuContextInfo.PerformedFlashCycles, ecuContextInfo.RemainingFlashCycles));
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzSwtAction psdzSwtAction = ProgrammingService.Psdz.ProgrammingService.RequestSwtAction(PsdzContext.Connection, true);
                PsdzContext.SwtAction = psdzSwtAction;
                if (psdzSwtAction?.SwtEcus != null)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Swt Ecus: {0}", psdzSwtAction.SwtEcus.Count()));
                    foreach (IPsdzSwtEcu psdzSwtEcu in psdzSwtAction.SwtEcus)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecu: Id={0}, Vin={1}, CertState={2}, SwSig={3}",
                            psdzSwtEcu.EcuIdentifier, psdzSwtEcu.Vin, psdzSwtEcu.RootCertState, psdzSwtEcu.SoftwareSigState));
                        foreach (IPsdzSwtApplication swtApplication in psdzSwtEcu.SwtApplications)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Fsc: Type={0}, State={1}, Length={2}",
                                swtApplication.SwtType, swtApplication.FscState, swtApplication.Fsc.Length));
                        }
                    }
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzTal psdzTal = ProgrammingService.Psdz.LogicService.GenerateTal(PsdzContext.Connection, PsdzContext.SvtActual, psdzSollverbauung, PsdzContext.SwtAction, PsdzContext.TalFilter, PsdzContext.FaActual.Vin);
                PsdzContext.Tal = psdzTal;
                sbResult.AppendLine("Tal:");
                //sbResult.AppendLine(psdzTal.AsXml);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", psdzTal.AsXml.Length));
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " State: {0}", psdzTal.TalExecutionState));
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecus: {0}", psdzTal.AffectedEcus.Count()));
                foreach (IPsdzEcuIdentifier ecuIdentifier in psdzTal.AffectedEcus)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "  Affected Ecu: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                        ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                }

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Lines: {0}", psdzTal.TalLines.Count()));
                foreach (IPsdzTalLine talLine in psdzTal.TalLines)
                {
                    sbResult.Append(string.Format(CultureInfo.InvariantCulture, "  Tal line: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                        talLine.EcuIdentifier.BaseVariant, talLine.EcuIdentifier.DiagAddrAsInt, talLine.EcuIdentifier.DiagnosisAddress.Offset));
                    sbResult.Append(string.Format(CultureInfo.InvariantCulture, " FscDeploy={0}, BlFlash={1}, IbaDeploy={2}, SwDeploy={3}, IdRestore={4}, SfaDeploy={5}, Cat={6}",
                        talLine.FscDeploy.Tas.Count(), talLine.BlFlash.Tas.Count(), talLine.IbaDeploy.Tas.Count(),
                        talLine.SwDeploy.Tas.Count(), talLine.IdRestore.Tas.Count(), talLine.SFADeploy.Tas.Count(),
                        talLine.TaCategories));
                    sbResult.AppendLine();
                    foreach (IPsdzTa psdzTa in talLine.TaCategory.Tas)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "   SgbmId={0}, State={1}",
                            psdzTa.SgbmId.HexString, psdzTa.ExecutionState));
                    }
                }

                if (psdzTal.HasFailureCauses)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Failures: {0}", psdzTal.FailureCauses.Count()));
                    foreach (IPsdzFailureCause failureCause in psdzTal.FailureCauses)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "  Failure cause: {0}", failureCause.Message));
                    }
                }
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzTal psdzBackupTal = ProgrammingService.Psdz.IndividualDataRestoreService.GenerateBackupTal(PsdzContext.Connection, PsdzContext.PathToBackupData, PsdzContext.Tal, PsdzContext.TalFilter);
                PsdzContext.IndividualDataBackupTal = psdzBackupTal;
                sbResult.AppendLine("Backup Tal:");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", psdzBackupTal.AsXml.Length));
                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();

                IPsdzTal psdzRestorePrognosisTal = ProgrammingService.Psdz.IndividualDataRestoreService.GenerateRestorePrognosisTal(PsdzContext.Connection, PsdzContext.PathToBackupData, PsdzContext.Tal, PsdzContext.IndividualDataBackupTal, PsdzContext.TalFilterForIndividualDataTal);
                PsdzContext.IndividualDataRestorePrognosisTal = psdzRestorePrognosisTal;
                sbResult.AppendLine("Restore prognosis Tal:");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", psdzRestorePrognosisTal.AsXml.Length));

                UpdateStatus(sbResult.ToString());
                cts?.Token.ThrowIfCancellationRequested();
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                if (operationType != OperationType.ExecuteTal)
                {
                    PsdzContext.Tal = null;
                }
                return false;
            }
            finally
            {
                log.InfoFormat("VehicleFunctions Finish - Type: {0}", operationType);
                log.Info(Environment.NewLine + sbResult);
            }
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

            foreach (ProgrammingJobs.OptionsItem optionsItem in SelectedOptions)
            {
                if (optionsItem.SwiAction.SwiInfoObjs != null)
                {
                    foreach (PdszDatabase.SwiInfoObj infoInfoObj in optionsItem.SwiAction.SwiInfoObjs)
                    {
                        if (infoInfoObj.LinkType == PdszDatabase.SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                        {
                            string moduleName = infoInfoObj.ModuleName;
                            PdszDatabase.TestModuleData testModuleData = ProgrammingService.PdszDatabase.GetTestModuleData(moduleName);
                            if (testModuleData == null)
                            {
                                log.ErrorFormat("UpdateTargetFa GetTestModuleData failed for: {0}", moduleName);
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
                                        log.ErrorFormat("UpdateTargetFa No info object: {0}", testModuleData.ModuleRef);
                                    }
                                    else
                                    {
                                        log.InfoFormat("UpdateTargetFa Info object: {0}", swiInfoObj.ToString(ClientContext.GetClientContext(PsdzContext.Vehicle).Language));
                                    }
                                }

                                IFa ifaTarget = ProgrammingUtils.BuildFa(PsdzContext.FaTarget);
                                if (testModuleData.RefDict.TryGetValue("faElementsToRem", out List<string> remList))
                                {
                                    if (!ProgrammingUtils.ModifyFa(ifaTarget, remList, false))
                                    {
                                        log.ErrorFormat("UpdateTargetFa Rem failed: {0}", remList.ToStringItems());
                                    }
                                }
                                if (testModuleData.RefDict.TryGetValue("faElementsToAdd", out List<string> addList))
                                {
                                    if (!ProgrammingUtils.ModifyFa(ifaTarget, addList, true))
                                    {
                                        log.ErrorFormat("UpdateTargetFa Add failed: {0}", addList.ToStringItems());
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
                log.InfoFormat("UpdateTargetFa FaTarget: {0}", PsdzContext.FaTarget.AsString);

                IFa ifaTarget = ProgrammingUtils.BuildFa(PsdzContext.FaTarget);
                IFa ifaActual = ProgrammingUtils.BuildFa(PsdzContext.FaActual);
                string compareFa = ProgrammingUtils.CompareFa(ifaActual, ifaTarget);
                if (!string.IsNullOrEmpty(compareFa))
                {
                    log.InfoFormat("UpdateTargetFa Compare FA: {0}", compareFa);
                }
            }
        }

        public bool InitProgrammingObjects(string istaFolder)
        {
            try
            {
                PsdzContext = new PsdzContext(istaFolder);
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

        public void SetupLog4Net()
        {
            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(appDir))
            {
                string log4NetConfig = Path.Combine(appDir, "log4net.xml");
                if (File.Exists(log4NetConfig))
                {
                    string logFile = Path.Combine(ProgrammingService.GetPsdzServiceHostLogDir(), "PsdzClient.log");
                    log4net.GlobalContext.Properties["LogFileName"] = logFile;
                    XmlConfigurator.Configure(new FileInfo(log4NetConfig));
                }
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
