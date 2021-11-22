using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

namespace PsdzClient
{
    public partial class FormMain : Form
    {
        private enum OperationType
        {
            CreateOptions,
            BuildTalILevel,
            BuildTalModFa,
            ExecuteTal,
        }

        private class OptionsItem
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
                return SwiAction.EcuTranslation.GetTitle(ClientContext.Language);
            }
        }

        private class OptionType
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
                    return swiRegister.EcuTranslation.GetTitle(ClientContext.Language);
                }
                return Name;
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(FormMain));

        private const string DealerId = "32395";
        private const string DefaultIp = @"127.0.0.1";
        private ProgrammingService programmingService;
        private bool _taskActive;
        private bool TaskActive
        {
            get { return _taskActive;}
            set
            {
                _taskActive = value;
                if (_taskActive)
                {
                    BeginInvoke((Action)(() =>
                    {
                        progressBarEvent.Style = ProgressBarStyle.Marquee;
                        labelProgressEvent.Text = string.Empty;
                    }));
                }
                else
                {
                    BeginInvoke((Action)(() =>
                    {
                        progressBarEvent.Style = ProgressBarStyle.Blocks;
                        progressBarEvent.Value = progressBarEvent.Minimum;
                        labelProgressEvent.Text = string.Empty;
                    }));
                }
            }
        }

        private bool _ignoreCheck = false;
        private bool _ignoreChange = false;
        private PsdzContext _psdzContext;
        private CancellationTokenSource _cts;
        private Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> _optionsDict;
        private List<OptionsItem> _selectedOptions;
        private OptionType[] _optionTypes =
        {
            new OptionType("Coding", PdszDatabase.SwiRegisterEnum.VehicleModificationCodingConversion),
            new OptionType("Coding back", PdszDatabase.SwiRegisterEnum.VehicleModificationCodingBackConversion),
            new OptionType("Modification", PdszDatabase.SwiRegisterEnum.VehicleModificationConversion),
            new OptionType("Modification back", PdszDatabase.SwiRegisterEnum.VehicleModificationBackConversion),
            new OptionType("Retrofit", PdszDatabase.SwiRegisterEnum.VehicleModificationRetrofitting),
        };

        public FormMain()
        {
            InitializeComponent();
        }

        private void UpdateDisplay()
        {
            bool active = TaskActive;
            bool abortPossible = _cts != null;
            bool hostRunning = false;
            bool vehicleConnected = false;
            bool talPresent = false;
            if (!active)
            {
                hostRunning = programmingService != null && programmingService.IsPsdzPsdzServiceHostInitialized();
            }

            if (_psdzContext?.Connection != null)
            {
                vehicleConnected = true;
                talPresent = _psdzContext?.Tal != null;
            }

            bool ipEnabled = !active && !vehicleConnected;

            textBoxIstaFolder.Enabled = !active && !hostRunning;
            comboBoxLanguage.Enabled = !active;
            ipAddressControlVehicleIp.Enabled = ipEnabled;
            checkBoxIcom.Enabled = ipEnabled;
            buttonVehicleSearch.Enabled = ipEnabled;
            buttonStartHost.Enabled = !active && !hostRunning;
            buttonStopHost.Enabled = !active && hostRunning;
            buttonConnect.Enabled = !active && hostRunning && !vehicleConnected;
            buttonDisconnect.Enabled = !active && hostRunning && vehicleConnected;
            buttonCreateOptions.Enabled = !active && hostRunning && vehicleConnected && _optionsDict == null;
            buttonModILevel.Enabled = !active && hostRunning && vehicleConnected && _optionsDict != null;
            buttonModFa.Enabled = buttonModILevel.Enabled;
            buttonExecuteTal.Enabled = buttonModILevel.Enabled && talPresent;
            buttonClose.Enabled = !active;
            buttonAbort.Enabled = active && abortPossible;
            checkedListBoxOptions.Enabled = !active && hostRunning && vehicleConnected;

            if (!vehicleConnected)
            {
                UpdateOptions(null);
            }
            comboBoxOptionType.Enabled = _optionsDict != null && _optionsDict.Count > 0;
        }

        private bool LoadSettings()
        {
            try
            {
                _ignoreChange = true;
                textBoxIstaFolder.Text = Properties.Settings.Default.IstaFolder;
                comboBoxLanguage.SelectedIndex = Properties.Settings.Default.LanguageIndex;
                ipAddressControlVehicleIp.Text = Properties.Settings.Default.VehicleIp;
                checkBoxIcom.Checked = Properties.Settings.Default.IcomConnection;
                if (string.IsNullOrWhiteSpace(ipAddressControlVehicleIp.Text.Trim('.')))
                {
                    ipAddressControlVehicleIp.Text = DefaultIp;
                    checkBoxIcom.Checked = false;
                }

                ClientContext.Language = comboBoxLanguage.SelectedItem.ToString();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _ignoreChange = false;
            }

            return true;
        }

        private bool StoreSettings()
        {
            try
            {
                Properties.Settings.Default.IstaFolder = textBoxIstaFolder.Text;
                Properties.Settings.Default.LanguageIndex = comboBoxLanguage.SelectedIndex;
                Properties.Settings.Default.VehicleIp = ipAddressControlVehicleIp.Text;
                Properties.Settings.Default.IcomConnection = checkBoxIcom.Checked;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void UpdateStatus(string message = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateStatus(message);
                }));
                return;
            }

            textBoxStatus.Text = message ?? string.Empty;
            textBoxStatus.SelectionStart = textBoxStatus.TextLength;
            textBoxStatus.Update();
            textBoxStatus.ScrollToCaret();

            UpdateDisplay();
        }

        private void UpdateCurrentOptions()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateCurrentOptions();
                }));
                return;
            }

            try
            {
                _ignoreChange = true;
                int selectedIndex = comboBoxOptionType.SelectedIndex;
                comboBoxOptionType.BeginUpdate();
                comboBoxOptionType.Items.Clear();
                if (_optionsDict != null)
                {
                    foreach (OptionType optionTypeUpdate in _optionTypes)
                    {
                        comboBoxOptionType.Items.Add(optionTypeUpdate);
                    }

                    if (selectedIndex < 0 && comboBoxOptionType.Items.Count >= 1)
                    {
                        selectedIndex = 0;
                    }

                    if (selectedIndex < comboBoxOptionType.Items.Count)
                    {
                        comboBoxOptionType.SelectedIndex = selectedIndex;
                    }
                }
            }
            finally
            {
                comboBoxOptionType.EndUpdate();
                _ignoreChange = false;
            }

            if (comboBoxOptionType.Items.Count > 0)
            {
                if (comboBoxOptionType.SelectedItem is OptionType optionType)
                {
                    SelectOptions(optionType.SwiRegisterEnum);
                }
                else
                {
                    SelectOptions(null);
                }
            }
        }

        private void UpdateOptions(Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateOptions(optionsDict);
                }));
                return;
            }

            _optionsDict = optionsDict;
            _selectedOptions = new List<OptionsItem>();
            UpdateCurrentOptions();
        }

        private void SelectOptions(PdszDatabase.SwiRegisterEnum? swiRegisterEnum)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    SelectOptions(swiRegisterEnum);
                }));
                return;
            }

            try
            {
                List<PdszDatabase.SwiAction> selectedSwiActions = GetSelectedSwiActions();
                List<PdszDatabase.SwiAction> linkedSwiActions = programmingService.PdszDatabase.ReadLinkedSwiActions(selectedSwiActions, _psdzContext.Vehicle, null);
                OptionsItem topItemCurrent = null;
                int topIndexCurrent = checkedListBoxOptions.TopIndex;
                if (topIndexCurrent >= 0 && topIndexCurrent < checkedListBoxOptions.Items.Count)
                {
                    topItemCurrent = checkedListBoxOptions.Items[topIndexCurrent] as OptionsItem;
                }

                _ignoreCheck = true;
                checkedListBoxOptions.BeginUpdate();
                checkedListBoxOptions.Items.Clear();
                if (_optionsDict != null && _selectedOptions != null && swiRegisterEnum.HasValue)
                {
                    if (_optionsDict.TryGetValue(swiRegisterEnum.Value, out List<OptionsItem> optionsItems))
                    {
                        foreach (OptionsItem optionsItem in optionsItems)
                        {
                            CheckState checkState = CheckState.Unchecked;
                            bool addItem = true;
                            int selectIndex = _selectedOptions.IndexOf(optionsItem);
                            if (selectIndex >= 0)
                            {
                                if (selectIndex == _selectedOptions.Count - 1)
                                {
                                    checkState = CheckState.Checked;
                                }
                                else
                                {
                                    checkState = CheckState.Indeterminate;
                                }
                            }
                            else
                            {
                                if (linkedSwiActions != null &&
                                    linkedSwiActions.Any(x => string.Compare(x.Id, optionsItem.SwiAction.Id, StringComparison.OrdinalIgnoreCase) == 0))
                                {
                                    addItem = false;
                                }
                                else
                                {
                                    if (!programmingService.PdszDatabase.EvaluateXepRulesById(optionsItem.SwiAction.Id, _psdzContext.Vehicle, null))
                                    {
                                        addItem = false;
                                    }
                                }
                            }

                            if (addItem)
                            {
                                checkedListBoxOptions.Items.Add(optionsItem, checkState);
                            }
                        }
                    }
                }

                if (topItemCurrent != null)
                {
                    int topIndexNew = checkedListBoxOptions.Items.IndexOf(topItemCurrent);
                    if (topIndexNew >= 0 && topIndexNew < checkedListBoxOptions.Items.Count)
                    {
                        checkedListBoxOptions.TopIndex = topIndexNew;
                    }
                }
            }
            finally
            {
                checkedListBoxOptions.EndUpdate();
                _ignoreCheck = false;
            }
        }

        private List<PdszDatabase.SwiAction> GetSelectedSwiActions()
        {
            if (_psdzContext == null || _selectedOptions == null)
            {
                return null;
            }

            List<PdszDatabase.SwiAction> selectedSwiActions = new List<PdszDatabase.SwiAction>();
            foreach (OptionsItem optionsItem in _selectedOptions)
            {
                if (optionsItem.SwiAction != null)
                {
                    log.InfoFormat("GetSelectedSwiActions Selected: {0}", optionsItem.SwiAction);
                    selectedSwiActions.Add(optionsItem.SwiAction);
                }
            }

            log.InfoFormat("GetSelectedSwiActions Count: {0}", selectedSwiActions.Count);

            return selectedSwiActions;
        }

        private void UpdateTargetFa(bool reset = false)
        {
            if (_psdzContext == null || _selectedOptions == null)
            {
                return;
            }

            if (reset)
            {
                _selectedOptions.Clear();
            }

            _psdzContext.SetFaTarget(_psdzContext.FaActual);
            programmingService.PdszDatabase.ResetXepRules();

            foreach (OptionsItem optionsItem in _selectedOptions)
            {
                if (optionsItem.SwiAction.SwiInfoObjs != null)
                {
                    foreach (PdszDatabase.SwiInfoObj infoInfoObj in optionsItem.SwiAction.SwiInfoObjs)
                    {
                        if (infoInfoObj.LinkType == PdszDatabase.SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                        {
                            string moduleName = infoInfoObj.ModuleName;
                            PdszDatabase.TestModuleData testModuleData = programmingService.PdszDatabase.ReadTestModule(moduleName);
                            if (testModuleData == null)
                            {
                                log.ErrorFormat("UpdateTargetFa ReadTestModule failed for: {0}", moduleName);
                                optionsItem.Invalid = true;
                            }
                            else
                            {
                                optionsItem.Invalid = false;
                                if (!string.IsNullOrEmpty(testModuleData.ModuleRef))
                                {
                                    PdszDatabase.SwiInfoObj swiInfoObj = programmingService.PdszDatabase.GetInfoObjectByControlId(testModuleData.ModuleRef, infoInfoObj.LinkType);
                                    if (swiInfoObj == null)
                                    {
                                        log.ErrorFormat("UpdateTargetFa No info object: {0}", testModuleData.ModuleRef);
                                    }
                                    else
                                    {
                                        log.InfoFormat("UpdateTargetFa Info object: {0}", swiInfoObj.ToString(ClientContext.Language));
                                    }
                                }

                                IFa ifaTarget = ProgrammingUtils.BuildFa(_psdzContext.FaTarget);
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

                                IPsdzFa psdzFaTarget = programmingService.Psdz.ObjectBuilder.BuildFa(ifaTarget, _psdzContext.FaActual.Vin);
                                _psdzContext.SetFaTarget(psdzFaTarget);
                                programmingService.PdszDatabase.ResetXepRules();
                            }
                        }
                    }
                }
            }

            _selectedOptions.RemoveAll(x => x.Invalid);

            log.InfoFormat("UpdateTargetFa FaTarget: {0}", _psdzContext.FaTarget.AsString);

            UpdateCurrentOptions();
        }

        private void SetupLog4Net()
        {
            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(appDir))
            {
                string log4NetConfig = Path.Combine(appDir, "log4net.xml");
                if (File.Exists(log4NetConfig))
                {
                    string logFile = Path.Combine(programmingService.GetPsdzServiceHostLogDir(), "PsdzClient.log");
                    log4net.GlobalContext.Properties["LogFileName"] = logFile;
                    XmlConfigurator.Configure(new FileInfo(log4NetConfig));
                }
            }
        }

        private async Task<bool> StartProgrammingServiceTask(string dealerId)
        {
            return await Task.Run(() => StartProgrammingService(dealerId)).ConfigureAwait(false);
        }

        private bool StartProgrammingService(string dealerId)
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine("Starting host ...");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "DealerId={0}", dealerId));
                UpdateStatus(sbResult.ToString());

                if (programmingService != null && programmingService.IsPsdzPsdzServiceHostInitialized())
                {
                    if (!StopProgrammingService())
                    {
                        sbResult.AppendLine("Stop host failed");
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }
                }

                programmingService = new ProgrammingService(textBoxIstaFolder.Text, dealerId);
                SetupLog4Net();
                programmingService.EventManager.ProgrammingEventRaised += (sender, args) =>
                {
                    if (args is ProgrammingTaskEventArgs programmingEventArgs)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            progressBarEvent.Style = ProgressBarStyle.Blocks;
                            if (programmingEventArgs.IsTaskFinished)
                            {
                                labelProgressEvent.Text = string.Empty;
                                progressBarEvent.Value = progressBarEvent.Maximum;
                            }
                            else
                            {
                                int progress = (int) (programmingEventArgs.Progress * 100.0);
                                labelProgressEvent.Text = string.Format(CultureInfo.InvariantCulture, "{0}%, {1}s", progress, programmingEventArgs.TimeLeftSec);
                                progressBarEvent.Value = progress;
                            }
                        }));
                    }
                };

                if (!programmingService.StartPsdzServiceHost())
                {
                    sbResult.AppendLine("Start host failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                programmingService.SetLogLevelToMax();
                sbResult.AppendLine("Host started");
                UpdateStatus(sbResult.ToString());

                programmingService.PdszDatabase.ResetXepRules();
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                return false;
            }
        }

        private async Task<bool> StopProgrammingServiceTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => StopProgrammingService()).ConfigureAwait(false);
        }

        private bool StopProgrammingService()
        {
            StringBuilder sbResult = new StringBuilder();
            try
            {
                sbResult.AppendLine("Stopping host ...");
                UpdateStatus(sbResult.ToString());

                if (programmingService != null)
                {
                    programmingService.Psdz.Shutdown();
                    programmingService.CloseConnectionsToPsdzHost();
                    programmingService.Dispose();
                    programmingService = null;
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

        private async Task<List<EdInterfaceEnet.EnetConnection>> SearchVehiclesTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => SearchVehicles()).ConfigureAwait(false);
        }

        private List<EdInterfaceEnet.EnetConnection> SearchVehicles()
        {
            List<EdInterfaceEnet.EnetConnection> detectedVehicles;
            using (EdInterfaceEnet edInterface = new EdInterfaceEnet())
            {
                detectedVehicles = edInterface.DetectedVehicles("auto:all");
            }

            return detectedVehicles;
        }

        private async Task<bool> ConnectVehicleTask(string istaFolder, string ipAddress, bool icomConnection)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ConnectVehicle(istaFolder, ipAddress, icomConnection)).ConfigureAwait(false);
        }

        private bool ConnectVehicle(string istaFolder, string ipAddress, bool icomConnection)
        {
            log.InfoFormat("ConnectVehicle Start - Ip: {0}, ICOM: {1}", ipAddress, icomConnection);
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine("Connecting vehicle ...");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ip={0}, ICOM={1}",
                    ipAddress, icomConnection));
                UpdateStatus(sbResult.ToString());

                if (programmingService == null)
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

                _psdzContext.DetectVehicle = new DetectVehicle(ecuPath, enetConnection);
                _psdzContext.DetectVehicle.AbortRequest += () =>
                {
                    if (_cts != null)
                    {
                        return _cts.Token.IsCancellationRequested;
                    }
                    return false;
                };

                bool detectResult = _psdzContext.DetectVehicle.DetectVehicleBmwFast();
                _cts?.Token.ThrowIfCancellationRequested();
                if (!detectResult)
                {
                    sbResult.AppendLine("Vehicle detection failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Detected vehicle: VIN={0}, GroupFile={1}, BR={2}, Series={3}, BuildDate={4}-{5}",
                    _psdzContext.DetectVehicle.Vin ?? string.Empty, _psdzContext.DetectVehicle.GroupSgdb ?? string.Empty,
                    _psdzContext.DetectVehicle.ModelSeries ?? string.Empty, _psdzContext.DetectVehicle.Series ?? string.Empty,
                    _psdzContext.DetectVehicle.ConstructYear ?? string.Empty, _psdzContext.DetectVehicle.ConstructMonth ?? string.Empty));
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Detected ILevel: Ship={0}, Current={1}, Backup={2}",
                    _psdzContext.DetectVehicle.ILevelShip ?? string.Empty, _psdzContext.DetectVehicle.ILevelCurrent ?? string.Empty,
                    _psdzContext.DetectVehicle.ILevelBackup ?? string.Empty));

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecus: {0}", _psdzContext.DetectVehicle.EcuList.Count()));
                foreach (PdszDatabase.EcuInfo ecuInfo in _psdzContext.DetectVehicle.EcuList)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecu: Name={0}, Addr={1}, Sgdb={2}, Group={3}",
                        ecuInfo.Name, ecuInfo.Address, ecuInfo.Sgbd, ecuInfo.Grp));
                }

                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                string series = _psdzContext.DetectVehicle.Series;
                if (string.IsNullOrEmpty(series))
                {
                    sbResult.AppendLine("Vehicle series not detected");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                string mainSeries = programmingService.Psdz.ConfigurationService.RequestBaureihenverbund(series);
                IEnumerable<IPsdzTargetSelector> targetSelectors =
                    programmingService.Psdz.ConnectionFactoryService.GetTargetSelectors();
                _psdzContext.TargetSelectors = targetSelectors;
                TargetSelectorChooser targetSelectorChooser = new TargetSelectorChooser(_psdzContext.TargetSelectors);
                IPsdzTargetSelector psdzTargetSelectorNewest =
                    targetSelectorChooser.GetNewestTargetSelectorByMainSeries(mainSeries);
                if (psdzTargetSelectorNewest == null)
                {
                    sbResult.AppendLine("No target selector");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                _psdzContext.ProjectName = psdzTargetSelectorNewest.Project;
                _psdzContext.VehicleInfo = psdzTargetSelectorNewest.VehicleInfo;
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "Target selector: Project={0}, Vehicle={1}, Series={2}",
                    psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo,
                    psdzTargetSelectorNewest.Baureihenverbund));
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                string bauIStufe = _psdzContext.DetectVehicle.ILevelShip;
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel shipment: {0}", bauIStufe));

                string url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", ipAddress, diagPort);
                IPsdzConnection psdzConnection;
                if (icomConnection)
                {
                    psdzConnection = programmingService.Psdz.ConnectionManagerService.ConnectOverIcom(
                        psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, 1000, series,
                        bauIStufe, IcomConnectionType.Ip, false);
                }
                else
                {
                    psdzConnection = programmingService.Psdz.ConnectionManagerService.ConnectOverEthernet(
                        psdzTargetSelectorNewest.Project, psdzTargetSelectorNewest.VehicleInfo, url, series,
                        bauIStufe);
                }

                Vehicle vehicle = new Vehicle();
                vehicle.VCI.VCIType = icomConnection ?
                    BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ICOM : BMW.Rheingold.CoreFramework.Contracts.Vehicle.VCIDeviceType.ENET;
                vehicle.VCI.IPAddress = ipAddress;
                vehicle.VCI.Port = diagPort;
                vehicle.VCI.NetworkType = "LAN";
                vehicle.VCI.VIN = _psdzContext.DetectVehicle.Vin;
                _psdzContext.Vehicle = vehicle;

                programmingService.CreateEcuProgrammingInfos(_psdzContext.Vehicle);
                _psdzContext.Connection = psdzConnection;

                sbResult.AppendLine("Vehicle connected");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Connection: Id={0}, Port={1}",
                    psdzConnection.Id, psdzConnection.Port));

                UpdateStatus(sbResult.ToString());

                programmingService.AddListener(_psdzContext);
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

                if (_psdzContext != null)
                {
                    if (_psdzContext.Connection == null)
                    {
                        ClearProgrammingObjects();
                    }
                }
            }
        }

        private async Task<bool> DisconnectVehicleTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => DisconnectVehicle()).ConfigureAwait(false);
        }

        private bool DisconnectVehicle()
        {
            log.Info("DisconnectVehicle Start");
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine("Disconnecting vehicle ...");
                UpdateStatus(sbResult.ToString());

                if (programmingService == null)
                {
                    sbResult.AppendLine("No Host");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (_psdzContext?.Connection == null)
                {
                    sbResult.AppendLine("No connection");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                programmingService.RemoveListener();
                programmingService.Psdz.ConnectionManagerService.CloseConnection(_psdzContext.Connection);

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

                if (_psdzContext != null)
                {
                    if (_psdzContext.Connection == null)
                    {
                        ClearProgrammingObjects();
                    }
                }
            }
        }

        private async Task<bool> VehicleFunctionsTask(OperationType operationType)
        {
            return await Task.Run(() => VehicleFunctions(operationType)).ConfigureAwait(false);
        }

        private bool VehicleFunctions(OperationType operationType)
        {
            log.InfoFormat("VehicleFunctions Start - Type: {0}", operationType);
            StringBuilder sbResult = new StringBuilder();

            try
            {
                sbResult.AppendLine("Executing vehicle functions ...");
                UpdateStatus(sbResult.ToString());

                if (programmingService == null)
                {
                    sbResult.AppendLine("No Host");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                if (_psdzContext?.Connection == null)
                {
                    sbResult.AppendLine("No connection");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzVin psdzVin = programmingService.Psdz.VcmService.GetVinFromMaster(_psdzContext.Connection);
                if (string.IsNullOrEmpty(psdzVin?.Value))
                {
                    sbResult.AppendLine("Reading VIN failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Vin: {0}", psdzVin.Value));
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                if (operationType == OperationType.ExecuteTal)
                {
                    sbResult.AppendLine("Execute TAL");
                    UpdateStatus(sbResult.ToString());

                    if (_psdzContext.Tal == null)
                    {
                        sbResult.AppendLine("No TAL present");
                        UpdateStatus(sbResult.ToString());
                        return false;
                    }

                    DateTime calculationStartTime = DateTime.Now;
                    IEnumerable<IPsdzEcuIdentifier> psdzEcuIdentifiersPrg = programmingService.Psdz.ProgrammingService.CheckProgrammingCounter(_psdzContext.Connection, _psdzContext.Tal);
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ProgCounter: {0}", psdzEcuIdentifiersPrg.Count()));
                    foreach (IPsdzEcuIdentifier ecuIdentifier in psdzEcuIdentifiersPrg)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                            ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                    }
                    UpdateStatus(sbResult.ToString());
                    _cts?.Token.ThrowIfCancellationRequested();

                    PsdzSecureCodingConfigCto secureCodingConfig = SecureCodingConfigWrapper.GetSecureCodingConfig(programmingService);
                    IPsdzCheckNcdResultEto psdzCheckNcdResultEto = programmingService.Psdz.SecureCodingService.CheckNcdAvailabilityForGivenTal(_psdzContext.Tal, secureCodingConfig.NcdRootDirectory, psdzVin);
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ncd EachSigned: {0}", psdzCheckNcdResultEto.isEachNcdSigned));
                    foreach (IPsdzDetailedNcdInfoEto detailedNcdInfo in psdzCheckNcdResultEto.DetailedNcdStatus)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ncd: Btld={0}, Cafd={1}, Status={2}",
                            detailedNcdInfo.Btld.HexString, detailedNcdInfo.Cafd.HexString, detailedNcdInfo.NcdStatus));
                    }
                    UpdateStatus(sbResult.ToString());
                    _cts?.Token.ThrowIfCancellationRequested();

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
                    string secureCodingPath = SecureCodingConfigWrapper.GetSecureCodingPathWithVin(programmingService, psdzVin.Value);
                    string jsonRequestFilePath = Path.Combine(secureCodingPath, string.Format(CultureInfo.InvariantCulture, "SecureCodingNCDCalculationRequest_{0}_{1}_{2}.json",
                        psdzVin.Value, DealerId, calculationStartTime.ToString("HHmmss", CultureInfo.InvariantCulture)));
                    PsdzBackendNcdCalculationEtoEnum backendNcdCalculationEtoEnumOld = secureCodingConfig.BackendNcdCalculationEtoEnum;
                    try
                    {
                        secureCodingConfig.BackendNcdCalculationEtoEnum = PsdzBackendNcdCalculationEtoEnum.ALLOW;
                        IList<IPsdzSecurityBackendRequestFailureCto> psdzSecurityBackendRequestFailureList = programmingService.Psdz.SecureCodingService.RequestCalculationNcdAndSignatureOffline(requestNcdEtos, jsonRequestFilePath, secureCodingConfig, psdzVin, _psdzContext.FaTarget);
                        int failureCount = psdzSecurityBackendRequestFailureList.Count;
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ncd failures: {0}", failureCount));
                        foreach (IPsdzSecurityBackendRequestFailureCto psdzSecurityBackendRequestFailure in psdzSecurityBackendRequestFailureList)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Failure: Cause={0}, Retry={1}, Url={2}",
                                psdzSecurityBackendRequestFailure.Cause, psdzSecurityBackendRequestFailure.Retry, psdzSecurityBackendRequestFailure.Url));
                        }
                        UpdateStatus(sbResult.ToString());
                        _cts?.Token.ThrowIfCancellationRequested();

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
                        _cts?.Token.ThrowIfCancellationRequested();

                        IEnumerable<IPsdzSgbmId> sweList = programmingService.Psdz.LogicService.RequestSweList(_psdzContext.Tal, true);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Swe list: {0}", sweList.Count()));
                        foreach (IPsdzSgbmId psdzSgbmId in sweList)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString));
                        }
                        UpdateStatus(sbResult.ToString());
                        _cts?.Token.ThrowIfCancellationRequested();

                        IEnumerable<IPsdzSgbmId> sgbmIds = ProgrammingUtils.RemoveCafdsCalculatedOnSCB(cafdCalculatedInSCB, sweList);
                        IEnumerable<IPsdzSgbmId> softwareEntries = programmingService.Psdz.MacrosService.CheckSoftwareEntries(sgbmIds);
                        int softwareEntryCount = softwareEntries.Count();
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Sw entries: {0}", softwareEntryCount));
                        foreach (IPsdzSgbmId psdzSgbmId in softwareEntries)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Sgbm: {0}", psdzSgbmId.HexString));
                        }
                        UpdateStatus(sbResult.ToString());
                        _cts?.Token.ThrowIfCancellationRequested();

                        if (softwareEntryCount > 0)
                        {
                            sbResult.AppendLine("Software failures present");
                            UpdateStatus(sbResult.ToString());
                            return false;
                        }

                        sbResult.AppendLine("Executing Backup Tal ...");
                        UpdateStatus(sbResult.ToString());
                        TalExecutionSettings talExecutionSettings = ProgrammingUtils.GetTalExecutionSettings(programmingService);
                        IPsdzTal backupTalResult = programmingService.Psdz.IndividualDataRestoreService.ExecuteAsyncBackupTal(
                            _psdzContext.Connection, _psdzContext.IndividualDataBackupTal, null, _psdzContext.FaTarget, psdzVin, talExecutionSettings, _psdzContext.PathToBackupData);
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
                        _cts?.Token.ThrowIfCancellationRequested();

                        sbResult.AppendLine("Executing Tal ...");
                        UpdateStatus(sbResult.ToString());
                        IPsdzTal executeTalResult = programmingService.Psdz.TalExecutionService.ExecuteTal(_psdzContext.Connection, _psdzContext.Tal,
                            null, psdzVin, _psdzContext.FaTarget, talExecutionSettings, _psdzContext.PathToBackupData, _cts.Token);
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
                        _cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            programmingService.Psdz.ProgrammingService.TslUpdate(_psdzContext.Connection, true, _psdzContext.SvtActual, _psdzContext.Sollverbauung.Svt);
                            sbResult.AppendLine("Tsl updated");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Tsl update failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        _cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            programmingService.Psdz.VcmService.WriteIStufen(_psdzContext.Connection, _psdzContext.IstufeShipment, _psdzContext.IstufeLast, _psdzContext.IstufeCurrent);
                            sbResult.AppendLine("ILevel updated");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Write ILevel failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        _cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            programmingService.Psdz.VcmService.WriteIStufenToBackup(_psdzContext.Connection, _psdzContext.IstufeShipment, _psdzContext.IstufeLast, _psdzContext.IstufeCurrent);
                            sbResult.AppendLine("ILevel backup updated");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Write ILevel backup failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        _cts?.Token.ThrowIfCancellationRequested();

                        IPsdzResponse piaResponse = programmingService.Psdz.EcuService.UpdatePiaPortierungsmaster(_psdzContext.Connection, _psdzContext.SvtActual);
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "PIA master update Success={0}, Cause={1}",
                            piaResponse.IsSuccessful, piaResponse.Cause));
                        UpdateStatus(sbResult.ToString());
                        _cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            programmingService.Psdz.VcmService.WriteFa(_psdzContext.Connection, _psdzContext.FaTarget);
                            sbResult.AppendLine("FA written");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "FA write failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        _cts?.Token.ThrowIfCancellationRequested();

                        try
                        {
                            programmingService.Psdz.VcmService.WriteFaToBackup(_psdzContext.Connection, _psdzContext.FaTarget);
                            sbResult.AppendLine("FA backup written");
                            UpdateStatus(sbResult.ToString());
                        }
                        catch (Exception ex)
                        {
                            sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "FA backup write failure: {0}", ex.Message));
                            UpdateStatus(sbResult.ToString());
                        }
                        _cts?.Token.ThrowIfCancellationRequested();
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
                IPsdzTalFilter psdzTalFilter = programmingService.Psdz.ObjectBuilder.BuildTalFilter();
                // disable backup
                psdzTalFilter = programmingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.FscBackup }, TalFilterOptions.MustNot, psdzTalFilter);
                if (bModifyFa)
                {   // enable deploy
                    psdzTalFilter = programmingService.Psdz.ObjectBuilder.DefineFilterForAllEcus(new[] { TaCategories.CdDeploy }, TalFilterOptions.Must, psdzTalFilter);
                }
                _psdzContext.SetTalFilter(psdzTalFilter);

                IPsdzTalFilter psdzTalFilterEmpty = programmingService.Psdz.ObjectBuilder.BuildTalFilter();
                _psdzContext.SetTalFilterForIndividualDataTal(psdzTalFilterEmpty);

                if (_psdzContext.TalFilter != null)
                {
                    sbResult.AppendLine("TalFilter:");
                    sbResult.Append(_psdzContext.TalFilter.AsXml);
                }

                _psdzContext.CleanupBackupData();
                IPsdzIstufenTriple iStufenTriple = programmingService.Psdz.VcmService.GetIStufenTripleActual(_psdzContext.Connection);
                if (iStufenTriple == null)
                {
                    sbResult.AppendLine("Reading ILevel failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                _psdzContext.SetIstufen(iStufenTriple);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel: Current={0}, Last={1}, Shipment={2}",
                    iStufenTriple.Current, iStufenTriple.Last, iStufenTriple.Shipment));

                if (!_psdzContext.SetPathToBackupData(psdzVin.Value))
                {
                    sbResult.AppendLine("Create backup path failed");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }

                IPsdzStandardFa standardFa = programmingService.Psdz.VcmService.GetStandardFaActual(_psdzContext.Connection);
                IPsdzFa psdzFa = programmingService.Psdz.ObjectBuilder.BuildFa(standardFa, psdzVin.Value);
                _psdzContext.SetFaActual(psdzFa);
                sbResult.AppendLine("FA current:");
                sbResult.Append(psdzFa.AsXml);
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                if (!bModifyFa)
                {   // reset target fa
                    _psdzContext.SetFaTarget(psdzFa);
                }
                programmingService.PdszDatabase.ResetXepRules();

                IEnumerable<IPsdzIstufe> psdzIstufes = programmingService.Psdz.LogicService.GetPossibleIntegrationLevel(_psdzContext.FaTarget);
                _psdzContext.SetPossibleIstufenTarget(psdzIstufes);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevels: {0}", psdzIstufes.Count()));
                foreach (IPsdzIstufe iStufe in psdzIstufes.OrderBy(x => x))
                {
                    if (iStufe.IsValid)
                    {
                        sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " ILevel: {0}", iStufe.Value));
                    }
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                string latestIstufeTarget = _psdzContext.LatestPossibleIstufeTarget;
                if (string.IsNullOrEmpty(latestIstufeTarget))
                {
                    sbResult.AppendLine("No target ILevels");
                    UpdateStatus(sbResult.ToString());
                    return false;
                }
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel Latest: {0}", latestIstufeTarget));

                IPsdzIstufe psdzIstufeShip = programmingService.Psdz.ObjectBuilder.BuildIstufe(_psdzContext.IstufeShipment);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel Ship: {0}", psdzIstufeShip.Value));

                IPsdzIstufe psdzIstufeTarget = programmingService.Psdz.ObjectBuilder.BuildIstufe(bModifyFa ? _psdzContext.IstufeCurrent : latestIstufeTarget);
                _psdzContext.Vehicle.TargetILevel = psdzIstufeTarget.Value;
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "ILevel Target: {0}", psdzIstufeTarget.Value));
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IEnumerable<IPsdzEcuIdentifier> psdzEcuIdentifiers = programmingService.Psdz.MacrosService.GetInstalledEcuList(_psdzContext.FaActual, psdzIstufeShip);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "EcuIds: {0}", psdzEcuIdentifiers.Count()));
                foreach (IPsdzEcuIdentifier ecuIdentifier in psdzEcuIdentifiers)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " EcuId: BaseVar={0}, DiagAddr={1}, DiagOffset={2}",
                        ecuIdentifier.BaseVariant, ecuIdentifier.DiagAddrAsInt, ecuIdentifier.DiagnosisAddress.Offset));
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzStandardSvt psdzStandardSvt = programmingService.Psdz.EcuService.RequestSvt(_psdzContext.Connection, psdzEcuIdentifiers);
                IPsdzStandardSvt psdzStandardSvtNames = programmingService.Psdz.LogicService.FillBntnNamesForMainSeries(_psdzContext.Connection.TargetSelector.Baureihenverbund, psdzStandardSvt);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Svt Ecus: {0}", psdzStandardSvtNames.Ecus.Count()));
                foreach (IPsdzEcu ecu in psdzStandardSvtNames.Ecus)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Variant: BaseVar={0}, Var={1}, Name={2}",
                        ecu.BaseVariant, ecu.EcuVariant, ecu.BnTnName));
                }

                IPsdzSvt psdzSvt = programmingService.Psdz.ObjectBuilder.BuildSvt(psdzStandardSvtNames, psdzVin.Value);
                _psdzContext.SetSvtActual(psdzSvt);
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                programmingService.PdszDatabase.LinkSvtEcus(_psdzContext.DetectVehicle.EcuList, psdzSvt);
                programmingService.PdszDatabase.GetEcuVariants(_psdzContext.DetectVehicle.EcuList);
                if (!_psdzContext.UpdateVehicle(programmingService, psdzStandardSvtNames))
                {
                    sbResult.AppendLine("UpdateVehicle failed");
                    UpdateStatus(sbResult.ToString());
                    return true;
                }

                programmingService.PdszDatabase.ResetXepRules();
                programmingService.PdszDatabase.GetEcuVariants(_psdzContext.DetectVehicle.EcuList, _psdzContext.Vehicle);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecus: {0}", _psdzContext.DetectVehicle.EcuList.Count()));
                foreach (PdszDatabase.EcuInfo ecuInfo in _psdzContext.DetectVehicle.EcuList)
                {
                    sbResult.AppendLine(ecuInfo.ToString(ClientContext.Language));
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();
                if (operationType == OperationType.CreateOptions)
                {
                    programmingService.PdszDatabase.StoreTestModuleData();
                    programmingService.PdszDatabase.ReadSwiRegister(_psdzContext.Vehicle);
                    if (programmingService.PdszDatabase.SwiRegisterTree != null)
                    {
                        string treeText = programmingService.PdszDatabase.SwiRegisterTree.ToString(ClientContext.Language);
                        if (!string.IsNullOrEmpty(treeText))
                        {
                            log.Info(Environment.NewLine + "Swi tree:" + Environment.NewLine + treeText);
                        }

                        Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>> optionsDict = new Dictionary<PdszDatabase.SwiRegisterEnum, List<OptionsItem>>();
                        foreach (OptionType optionType in _optionTypes)
                        {
                            optionType.SwiRegister = programmingService.PdszDatabase.FindNodeForRegister(optionType.SwiRegisterEnum);
                            List<PdszDatabase.SwiAction> swiActions = programmingService.PdszDatabase.GetSwiActionsForRegister(optionType.SwiRegisterEnum, true);
                            if (swiActions != null)
                            {
                                sbResult.AppendLine();
                                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Swi actions: {0}", optionType.Name));
                                List<OptionsItem> optionsItems = new List<OptionsItem>();
                                foreach (PdszDatabase.SwiAction swiAction in swiActions)
                                {
                                    sbResult.AppendLine(swiAction.ToString(ClientContext.Language));
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

                IPsdzReadEcuUidResultCto psdzReadEcuUid = programmingService.Psdz.SecurityManagementService.readEcuUid(_psdzContext.Connection, psdzEcuIdentifiers, _psdzContext.SvtActual);

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
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzReadStatusResultCto psdzReadStatusResult = programmingService.Psdz.SecureFeatureActivationService.ReadStatus(PsdzStatusRequestFeatureTypeEtoEnum.ALL_FEATURES, _psdzContext.Connection, _psdzContext.SvtActual, psdzEcuIdentifiers, true, 3, 100);
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
                _cts?.Token.ThrowIfCancellationRequested();

                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Status features: {0}", psdzReadStatusResult.FeatureStatusSet.Count()));
                foreach (IPsdzFeatureLongStatusCto featureLongStatus in psdzReadStatusResult.FeatureStatusSet)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Feature: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, Status={3}, Token={4}",
                        featureLongStatus.EcuIdentifierCto.BaseVariant, featureLongStatus.EcuIdentifierCto.DiagAddrAsInt, featureLongStatus.EcuIdentifierCto.DiagnosisAddress.Offset,
                        featureLongStatus.FeatureStatusEto, featureLongStatus.TokenId));
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzTalFilter talFilterFlash = new PsdzTalFilter();
                IPsdzSollverbauung psdzSollverbauung = programmingService.Psdz.LogicService.GenerateSollverbauungGesamtFlash(_psdzContext.Connection, psdzIstufeTarget, psdzIstufeShip, _psdzContext.SvtActual, _psdzContext.FaTarget, talFilterFlash);
                _psdzContext.SetSollverbauung(psdzSollverbauung);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Target construction: Count={0}, Units={1}",
                    psdzSollverbauung.PsdzOrderList.BntnVariantInstances.Length, psdzSollverbauung.PsdzOrderList.NumberOfUnits));
                foreach (IPsdzEcuVariantInstance bntnVariant in psdzSollverbauung.PsdzOrderList.BntnVariantInstances)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Variant: BaseVar={0}, Var={1}, Name={2}",
                        bntnVariant.Ecu.BaseVariant, bntnVariant.Ecu.EcuVariant, bntnVariant.Ecu.BnTnName));
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IEnumerable<IPsdzEcuContextInfo> psdzEcuContextInfos = programmingService.Psdz.EcuService.RequestEcuContextInfos(_psdzContext.Connection, psdzEcuIdentifiers);
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Ecu contexts: {0}", psdzEcuContextInfos.Count()));
                foreach (IPsdzEcuContextInfo ecuContextInfo in psdzEcuContextInfos)
                {
                    sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Ecu context: BaseVar={0}, DiagAddr={1}, DiagOffset={2}, ManuDate={3}, PrgDate={4}, PrgCnt={5}, FlashCnt={6}, FlashRemain={7}",
                        ecuContextInfo.EcuId.BaseVariant, ecuContextInfo.EcuId.DiagAddrAsInt, ecuContextInfo.EcuId.DiagnosisAddress.Offset,
                        ecuContextInfo.ManufacturingDate, ecuContextInfo.LastProgrammingDate, ecuContextInfo.ProgramCounter, ecuContextInfo.PerformedFlashCycles, ecuContextInfo.RemainingFlashCycles));
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzSwtAction psdzSwtAction = programmingService.Psdz.ProgrammingService.RequestSwtAction(_psdzContext.Connection, true);
                _psdzContext.SwtAction = psdzSwtAction;
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
                                swtApplication.SwtType, swtApplication.FscState,  swtApplication.Fsc.Length));
                        }
                    }
                }
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzTal psdzTal = programmingService.Psdz.LogicService.GenerateTal(_psdzContext.Connection, _psdzContext.SvtActual, psdzSollverbauung, _psdzContext.SwtAction, _psdzContext.TalFilter, _psdzContext.FaActual.Vin);
                _psdzContext.Tal = psdzTal;
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
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzTal psdzBackupTal = programmingService.Psdz.IndividualDataRestoreService.GenerateBackupTal(_psdzContext.Connection, _psdzContext.PathToBackupData, _psdzContext.Tal, _psdzContext.TalFilter);
                _psdzContext.IndividualDataBackupTal = psdzBackupTal;
                sbResult.AppendLine("Backup Tal:");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", psdzBackupTal.AsXml.Length));
                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();

                IPsdzTal psdzRestorePrognosisTal = programmingService.Psdz.IndividualDataRestoreService.GenerateRestorePrognosisTal(_psdzContext.Connection, _psdzContext.PathToBackupData, _psdzContext.Tal, _psdzContext.IndividualDataBackupTal, _psdzContext.TalFilterForIndividualDataTal);
                _psdzContext.IndividualDataRestorePrognosisTal = psdzRestorePrognosisTal;
                sbResult.AppendLine("Restore prognosis Tal:");
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, " Size: {0}", psdzRestorePrognosisTal.AsXml.Length));

                UpdateStatus(sbResult.ToString());
                _cts?.Token.ThrowIfCancellationRequested();
                return true;
            }
            catch (Exception ex)
            {
                sbResult.AppendLine(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message));
                UpdateStatus(sbResult.ToString());
                if (operationType != OperationType.ExecuteTal)
                {
                    _psdzContext.Tal = null;
                }
                return false;
            }
            finally
            {
                log.InfoFormat("VehicleFunctions Finish - Type: {0}", operationType);
                log.Info(Environment.NewLine + sbResult);
            }
        }

        private bool InitProgrammingObjects(string istaFolder)
        {
            try
            {
                _psdzContext = new PsdzContext(istaFolder);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void ClearProgrammingObjects()
        {
            if (_psdzContext != null)
            {
                _psdzContext.Dispose();
                _psdzContext = null;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            if (TaskActive)
            {
                _cts?.Cancel();
            }
        }

        private void buttonIstaFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialogIsta.SelectedPath = textBoxIstaFolder.Text;
            DialogResult result = folderBrowserDialogIsta.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxIstaFolder.Text = folderBrowserDialogIsta.SelectedPath;
                UpdateDisplay();
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateDisplay();
            StoreSettings();
            timerUpdate.Enabled = false;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            PropertyInfo[] langueProperties = typeof(PdszDatabase.EcuTranslation).GetProperties();
            foreach (PropertyInfo propertyInfo in langueProperties)
            {
                string name = propertyInfo.Name;
                if (name.StartsWith("Text", StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxLanguage.Items.Add(name.Substring(4));
                }
            }

            comboBoxLanguage.SelectedIndex = 0;
            comboBoxLanguage.EndUpdate();

            LoadSettings();
            UpdateDisplay();
            UpdateStatus();
            timerUpdate.Enabled = true;
            labelProgressEvent.Text = string.Empty;
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void buttonStartHost_Click(object sender, EventArgs e)
        {
            StartProgrammingServiceTask(DealerId).ContinueWith(task =>
            {
                TaskActive = false;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void buttonStopHost_Click(object sender, EventArgs e)
        {
            StopProgrammingServiceTask().ContinueWith(task =>
            {
                TaskActive = false;
                if (e == null)
                {
                    BeginInvoke((Action)(() =>
                    {
                        Close();
                    }));
                }
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TaskActive)
            {
                e.Cancel = true;
                return;
            }

            if (programmingService != null && programmingService.IsPsdzPsdzServiceHostInitialized())
            {
                buttonStopHost_Click(sender, null);
                e.Cancel = true;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_psdzContext?.Connection != null)
            {
                return;
            }

            bool icomConnection = checkBoxIcom.Checked;
            _cts = new CancellationTokenSource();
            ConnectVehicleTask(textBoxIstaFolder.Text, ipAddressControlVehicleIp.Text, icomConnection).ContinueWith(task =>
            {
                TaskActive = false;
                _cts.Dispose();
                _cts = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (_psdzContext?.Connection == null)
            {
                return;
            }

            DisconnectVehicleTask().ContinueWith(task =>
            {
                TaskActive = false;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void buttonFunc_Click(object sender, EventArgs e)
        {
            if (_psdzContext?.Connection == null)
            {
                return;
            }

            OperationType operationType = OperationType.CreateOptions;
            if (sender == buttonCreateOptions)
            {
                operationType = OperationType.CreateOptions;
            }
            else if (sender == buttonModILevel)
            {
                operationType = OperationType.BuildTalILevel;
                UpdateTargetFa(true);
            }
            else if (sender == buttonModFa)
            {
                operationType = OperationType.BuildTalModFa;
                UpdateTargetFa();
            }
            else if (sender == buttonExecuteTal)
            {
                operationType = OperationType.ExecuteTal;
            }

            _cts = new CancellationTokenSource();
            VehicleFunctionsTask(operationType).ContinueWith(task =>
            {
                TaskActive = false;
                _cts.Dispose();
                _cts = null;
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void buttonVehicleSearch_Click(object sender, EventArgs e)
        {
            bool preferIcom = checkBoxIcom.Checked;

            SearchVehiclesTask().ContinueWith(task =>
            {
                TaskActive = false;
                BeginInvoke((Action)(() =>
                {
                    List<EdInterfaceEnet.EnetConnection> detectedVehicles = task.Result;
                    EdInterfaceEnet.EnetConnection connectionDirect = null;
                    EdInterfaceEnet.EnetConnection connectionIcom = null;
                    EdInterfaceEnet.EnetConnection connectionSelected = null;
                    if (detectedVehicles != null)
                    {
                        foreach (EdInterfaceEnet.EnetConnection enetConnection in detectedVehicles)
                        {
                            if (enetConnection.IpAddress.ToString().StartsWith("192.168.11."))
                            {   // ICOM vehicle IP
                                continue;
                            }

                            if (connectionSelected == null)
                            {
                                connectionSelected = enetConnection;
                            }

                            switch (enetConnection.ConnectionType)
                            {
                                case EdInterfaceEnet.EnetConnection.InterfaceType.Icom:
                                    if (connectionIcom == null)
                                    {
                                        connectionIcom = enetConnection;
                                    }
                                    break;

                                default:
                                    if (connectionDirect == null)
                                    {
                                        connectionDirect = enetConnection;
                                    }
                                    break;
                            }
                        }
                    }

                    if (preferIcom)
                    {
                        if (connectionIcom != null)
                        {
                            connectionSelected = connectionIcom;
                        }
                    }
                    else
                    {
                        if (connectionDirect != null)
                        {
                            connectionSelected = connectionDirect;
                        }
                    }

                    bool ipValid = false;
                    try
                    {
                        if (connectionSelected != null)
                        {
                            ipAddressControlVehicleIp.Text = connectionSelected.IpAddress.ToString();
                            checkBoxIcom.Checked = connectionSelected.ConnectionType == EdInterfaceEnet.EnetConnection.InterfaceType.Icom;
                            ipValid = true;
                        }
                    }
                    catch (Exception)
                    {
                        ipValid = false;
                    }

                    if (!ipValid)
                    {
                        ipAddressControlVehicleIp.Text = DefaultIp;
                        checkBoxIcom.Checked = false;
                    }
                }));

            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void checkedListBoxOptions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_ignoreCheck)
            {
                return;
            }

            if (e.Index >= 0 && e.Index < checkedListBoxOptions.Items.Count)
            {
                if (checkedListBoxOptions.Items[e.Index] is OptionsItem optionsItem)
                {
                    if (e.CurrentValue == CheckState.Indeterminate)
                    {
                        e.NewValue = e.CurrentValue;
                    }
                    else
                    {
                        if (_selectedOptions != null)
                        {
                            if (e.NewValue == CheckState.Checked)
                            {
                                _selectedOptions.Add(optionsItem);
                            }
                            else
                            {
                                _selectedOptions.Remove(optionsItem);
                            }
                        }
                    }
                }
            }

            BeginInvoke((Action)(() =>
            {
                UpdateTargetFa();
            }));
        }

        private void comboBoxOptionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChange)
            {
                return;
            }

            BeginInvoke((Action)(() =>
            {
                UpdateTargetFa();
            }));
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChange)
            {
                return;
            }

            ClientContext.Language = comboBoxLanguage.SelectedItem.ToString();

            BeginInvoke((Action)(() =>
            {
                UpdateCurrentOptions();
            }));
        }
    }
}
