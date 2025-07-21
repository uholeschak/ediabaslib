using BMW.Rheingold.Psdz.Client;
using EdiabasLib;
using log4net;
using PsdzClient.Programming;
using PsdzClient.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PsdzClient
{
    public partial class FormMain : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FormMain));

        private const string DealerId = "32395";
        private const string DefaultIp = @"127.0.0.1";

        private readonly ProgrammingJobs _programmingJobs;
        private readonly object _lockObject = new object();
        private bool _taskActive;
        private bool TaskActive
        {
            get
            {
                lock (_lockObject)
                {
                    return _taskActive;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _taskActive = value;
                }

                if (value)
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
        private CancellationTokenSource _cts;
        private string _lastTestFileName;
        private string _lastDecryptFileName;
        private bool _decryptEditMode;

        private readonly ProgrammingJobs.ExecutionMode _executionMode;

        public FormMain(string[] args = null)
        {
            InitializeComponent();

            _executionMode = ProgrammingJobs.ExecutionMode.Normal;
            if (args != null && args.Length > 0)
            {
                if (string.Compare(args[0], ProgrammingJobs.ArgumentGenerateModulesDirect, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _executionMode = ProgrammingJobs.ExecutionMode.GenerateModulesDirect;
                }
                else if (string.Compare(args[0], ProgrammingJobs.ArgumentGenerateServiceModules, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _executionMode = ProgrammingJobs.ExecutionMode.GenerateServiceModules;
                }
                else if (string.Compare(args[0], ProgrammingJobs.ArgumentGenerateTestModules, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _executionMode = ProgrammingJobs.ExecutionMode.GenerateTestModules;
                }
            }

            _programmingJobs = new ProgrammingJobs(DealerId, _executionMode);
            _programmingJobs.ParentWindowHandle = Handle;
            _programmingJobs.UpdateStatusEvent += UpdateStatus;
            _programmingJobs.ProgressEvent += UpdateProgress;
            _programmingJobs.UpdateOptionsEvent += UpdateOptions;
            _programmingJobs.UpdateOptionSelectionsEvent += UpdateOptionSelections;
            _programmingJobs.ShowMessageEvent += ShowMessageEvent;
            _programmingJobs.ServiceInitializedEvent += ServiceInitialized;
        }

        private void UpdateDisplay()
        {
            bool active = TaskActive;
            bool abortPossible = _cts != null;
            bool hostRunning = false;
            bool vehicleConnected = false;
            bool talPresent = false;
            bool editMode = _decryptEditMode;
            if (!active)
            {
                hostRunning = PsdzServiceStarter.IsThisServerInstanceRunning();
            }

            if (_programmingJobs.PsdzContext?.Connection != null)
            {
                vehicleConnected = true;
                talPresent = _programmingJobs.PsdzContext?.Tal != null;
            }

            Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
            bool ipEnabled = !active && !vehicleConnected;
            bool modifyTal = !active && hostRunning && vehicleConnected && optionsDict != null;

            textBoxIstaFolder.Enabled = !active && !hostRunning;
            comboBoxLanguage.Enabled = !active;
            ipAddressControlVehicleIp.Enabled = ipEnabled;
            checkBoxIcom.Enabled = ipEnabled;
            checkBoxGenServiceModules.Enabled = !active && !hostRunning;
            buttonVehicleSearch.Enabled = ipEnabled && !editMode;
            buttonInternalTest.Enabled = !active && !editMode;
            buttonDecryptFile.Enabled = !active;
            buttonStopHost.Enabled = !active && hostRunning && !editMode;
            buttonConnect.Enabled = !active && !vehicleConnected && !editMode;
            buttonDisconnect.Enabled = !active && hostRunning && vehicleConnected && !editMode;
            buttonCreateOptions.Enabled = !active && hostRunning && vehicleConnected && optionsDict == null && !editMode;
            buttonModILevel.Enabled = modifyTal && !editMode;
            buttonModFa.Enabled = modifyTal && !editMode;
            buttonExecuteTal.Enabled = modifyTal && talPresent && !editMode;
            buttonClose.Enabled = !active && !editMode;
            buttonAbort.Enabled = (active && abortPossible) || editMode;
            checkedListBoxOptions.Enabled = !active && hostRunning && vehicleConnected;

            if (!vehicleConnected)
            {
                UpdateOptions(null);
            }
            comboBoxOptionType.Enabled = optionsDict != null && optionsDict.Count > 0;
        }

        private bool LoadSettings()
        {
            try
            {
                _ignoreChange = true;
                string istaFolder = Properties.Settings.Default.IstaFolder;
                comboBoxLanguage.SelectedIndex = Properties.Settings.Default.LanguageIndex;
                ipAddressControlVehicleIp.Text = Properties.Settings.Default.VehicleIp;
                checkBoxIcom.Checked = Properties.Settings.Default.IcomConnection;
                checkBoxGenServiceModules.Checked = Properties.Settings.Default.GenServiceModules;
                _lastTestFileName = Properties.Settings.Default.TestFileName;
                _lastDecryptFileName = Properties.Settings.Default.DecryptFileName;

                if (string.IsNullOrWhiteSpace(ipAddressControlVehicleIp.Text.Trim('.')))
                {
                    ipAddressControlVehicleIp.Text = DefaultIp;
                    checkBoxIcom.Checked = false;
                }

                if (string.IsNullOrEmpty(istaFolder) || !Directory.Exists(istaFolder))
                {
                    istaFolder = ProgrammingJobs.GetIstaInstallLocation();
                }
                textBoxIstaFolder.Text = istaFolder;

                string language = comboBoxLanguage.SelectedItem.ToString();
                SetLanguage(language);
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
                Properties.Settings.Default.GenServiceModules = checkBoxGenServiceModules.Checked;
                Properties.Settings.Default.TestFileName = _lastTestFileName ?? string.Empty;
                Properties.Settings.Default.DecryptFileName = _lastDecryptFileName ?? string.Empty;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void SetLanguage(string language)
        {
            _programmingJobs.ClientContext.Language = language;
            if (!string.IsNullOrEmpty(language))
            {
                try
                {
                    CultureInfo culture = CultureInfo.CreateSpecificCulture(language.ToLowerInvariant());
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("InitializeCulture Exception: {0}", ex.Message);
                }
            }
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

        private void UpdateProgress(int percent, bool marquee, string message = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateProgress(percent, marquee, message);
                }));
                return;
            }

            if (marquee)
            {
                progressBarEvent.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBarEvent.Style = ProgressBarStyle.Blocks;
            }
            progressBarEvent.Value = percent;
            labelProgressEvent.Text = message ?? string.Empty;
        }

        private void ServiceInitialized(ProgrammingService programmingService)
        {
            string logFileName = "PsdzClient.log";
            if (_programmingJobs.IsModuleGenerationMode())
            {
                logFileName = "PsdzClientGenerate.log";
            }

            string logFile = Path.Combine(programmingService.GetPsdzServiceHostLogDir(), logFileName);
            ProgrammingJobs.SetupLog4Net(logFile);
            PsdzServiceStarter.ClearIstaPIDsFile();
        }

        private void UpdateCurrentOptions(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateCurrentOptions(swiRegisterEnum);
                }));
                return;
            }

            try
            {
                _ignoreChange = true;
                Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
                int selectedIndex = comboBoxOptionType.SelectedIndex;
                comboBoxOptionType.BeginUpdate();
                comboBoxOptionType.Items.Clear();
                if (optionsDict != null)
                {
                    foreach (ProgrammingJobs.OptionType optionTypeUpdate in _programmingJobs.OptionTypes)
                    {
                        int index = comboBoxOptionType.Items.Add(optionTypeUpdate);

                        if (swiRegisterEnum != null)
                        {
                            if (optionTypeUpdate.SwiRegisterEnum == swiRegisterEnum.Value)
                            {
                                selectedIndex = index;
                            }
                        }
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
                if (comboBoxOptionType.SelectedItem is ProgrammingJobs.OptionType optionType)
                {
                    SelectOptions(optionType.SwiRegisterEnum);
                }
                else
                {
                    SelectOptions(null);
                }
            }
            else
            {
                SelectOptions(null);
            }
        }

        private void UpdateOptions(Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateOptions(optionsDict);
                }));
                return;
            }

            _programmingJobs.SelectedOptions = new List<ProgrammingJobs.OptionsItem>();
            UpdateCurrentOptions();
        }

        private void UpdateOptionSelections(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateCurrentOptions(swiRegisterEnum);
                }));
                return;
            }

            UpdateCurrentOptions(swiRegisterEnum);
        }

        private bool ShowMessageEvent(CancellationTokenSource cts, string message, bool okBtn, bool wait)
        {
            MessageBoxButtons buttons = okBtn ? MessageBoxButtons.OK : MessageBoxButtons.YesNo;

            if (wait)
            {
                bool invokeResult = (bool) Invoke(new Func<bool>(() =>
                {
                    DialogResult dialogResult = MessageBox.Show(this, message, Text, buttons);
                    switch (dialogResult)
                    {
                        case DialogResult.OK:
                        case DialogResult.Yes:
                            return true;
                    }

                    return false;
                }));

                return invokeResult;
            }

            BeginInvoke((Action)(() =>
            {
                MessageBox.Show(this, message, Text, buttons);
            }));

            return true;
        }

        private void SelectOptions(PsdzDatabase.SwiRegisterEnum? swiRegisterEnum)
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
                if (_programmingJobs.ProgrammingService == null || _programmingJobs.PsdzContext?.Connection == null)
                {
                    checkedListBoxOptions.Items.Clear();
                    return;
                }

                bool replacement = false;
                if (swiRegisterEnum.HasValue)
                {
                    switch (PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnum.Value))
                    {
                        case PsdzDatabase.SwiRegisterGroup.HwDeinstall:
                        case PsdzDatabase.SwiRegisterGroup.HwInstall:
                            replacement = true;
                            break;
                    }
                }

                List<PsdzDatabase.SwiAction> selectedSwiActions = GetSelectedSwiActions();
                List<PsdzDatabase.SwiAction> linkedSwiActions = _programmingJobs.ProgrammingService.PsdzDatabase.ReadLinkedSwiActions(_programmingJobs.PsdzContext?.VecInfo, selectedSwiActions, null);
                ProgrammingJobs.OptionsItem topItemCurrent = null;
                int topIndexCurrent = checkedListBoxOptions.TopIndex;
                if (topIndexCurrent >= 0 && topIndexCurrent < checkedListBoxOptions.Items.Count)
                {
                    topItemCurrent = checkedListBoxOptions.Items[topIndexCurrent] as ProgrammingJobs.OptionsItem;
                }

                _ignoreCheck = true;
                Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
                checkedListBoxOptions.BeginUpdate();
                checkedListBoxOptions.Items.Clear();
                if (optionsDict != null && _programmingJobs.SelectedOptions != null && swiRegisterEnum.HasValue)
                {
                    if (optionsDict.TryGetValue(swiRegisterEnum.Value, out List<ProgrammingJobs.OptionsItem> optionsItems))
                    {
                        foreach (ProgrammingJobs.OptionsItem optionsItem in optionsItems.OrderBy(x => x.ToString()))
                        {
                            CheckState checkState = CheckState.Unchecked;
                            bool addItem = true;
                            int selectIndex = _programmingJobs.SelectedOptions.IndexOf(optionsItem);
                            if (selectIndex >= 0)
                            {
                                if (replacement)
                                {
                                    checkState = CheckState.Checked;
                                }
                                else
                                {
                                    if (selectIndex == _programmingJobs.SelectedOptions.Count - 1)
                                    {
                                        checkState = CheckState.Checked;
                                    }
                                    else
                                    {
                                        checkState = CheckState.Indeterminate;
                                    }
                                }
                            }
                            else
                            {
                                if (replacement)
                                {
                                    if (optionsItem.EcuInfo == null)
                                    {
                                        addItem = false;
                                    }
                                }
                                else
                                {
                                    if (optionsItem.SwiAction != null)
                                    {
                                        if (linkedSwiActions != null &&
                                            linkedSwiActions.Any(x => string.Compare(x.Id, optionsItem.SwiAction.Id, StringComparison.OrdinalIgnoreCase) == 0))
                                        {
                                            addItem = false;
                                        }
                                        else
                                        {
                                            if (!_programmingJobs.ProgrammingService.PsdzDatabase.EvaluateXepRulesById(optionsItem.SwiAction.Id, _programmingJobs.PsdzContext?.VecInfo, null))
                                            {
                                                addItem = false;
                                            }
                                        }
                                    }
                                }
                            }

                            if (!_programmingJobs.IsOptionsItemEnabled(optionsItem))
                            {
                                switch (checkState)
                                {
                                    case CheckState.Checked:
                                        checkState = CheckState.Indeterminate;
                                        break;

                                    case CheckState.Unchecked:
                                        addItem = false;
                                        break;
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

        private List<PsdzDatabase.SwiAction> GetSelectedSwiActions()
        {
            if (_programmingJobs.PsdzContext?.Connection == null || _programmingJobs.SelectedOptions == null)
            {
                return null;
            }

            List<PsdzDatabase.SwiAction> selectedSwiActions = new List<PsdzDatabase.SwiAction>();
            foreach (ProgrammingJobs.OptionsItem optionsItem in _programmingJobs.SelectedOptions)
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
            _programmingJobs.UpdateTargetFa(reset);
            UpdateCurrentOptions();
        }

        private string DecryptFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return null;
                }
                string text = Utility.Encryption.Decrypt(ReadAllText(fileName));
                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }
                return text;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string ReadAllText(string path)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] array = new byte[2048];
                    UTF8Encoding utf8Encoding = new UTF8Encoding(true);
                    int num;

                    while ((num = fileStream.Read(array, 0, array.Length)) > 0)
                    {
                        stringBuilder.Append(utf8Encoding.GetString(array, 0, num));
                    }
                }
                string text = stringBuilder.ToString();
                return text;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<bool> StopProgrammingServiceTask(string istaFolder, bool force)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => _programmingJobs.StopProgrammingService(_cts, istaFolder, force)).ConfigureAwait(false);
        }

        private async Task<List<EdInterfaceEnet.EnetConnection>> SearchVehiclesTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => SearchVehicles()).ConfigureAwait(false);
        }

        private List<EdInterfaceEnet.EnetConnection> SearchVehicles()
        {
            List<EdInterfaceEnet.EnetConnection> detectedVehicles;
            using (EdInterfaceEnet edInterface = new EdInterfaceEnet(false))
            {
                detectedVehicles = edInterface.DetectedVehicles(EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll,
                    new List<EdInterfaceEnet.CommunicationMode>()
                    {
                        EdInterfaceEnet.CommunicationMode.Hsfz
                    });
            }

            return detectedVehicles;
        }

        private async Task<string> InternalTestTask(string configurationContainerXml, Dictionary<string, string> runOverrideDict)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => InternalTest(configurationContainerXml, runOverrideDict)).ConfigureAwait(false);
        }

        private string InternalTest(string configurationContainerXml, Dictionary<string, string> runOverrideDict)
        {
            return _programmingJobs.ExecuteContainerXml(_cts, configurationContainerXml, runOverrideDict);
        }

        private async Task<bool> ConnectVehicleTask(string istaFolder, string remoteHost, bool useIcom)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => _programmingJobs.ConnectVehicle(_cts, istaFolder, remoteHost, useIcom)).ConfigureAwait(false);
        }

        private async Task<bool> DisconnectVehicleTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => _programmingJobs.DisconnectVehicle(_cts)).ConfigureAwait(false);
        }

        private async Task<bool> VehicleFunctionsTask(ProgrammingJobs.OperationType operationType)
        {
            return await Task.Run(() => _programmingJobs.VehicleFunctions(_cts, operationType)).ConfigureAwait(false);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            if (_decryptEditMode)
            {
                textBoxStatus.ReadOnly = true;
                _decryptEditMode = false;
            }
            if (TaskActive)
            {
                _cts?.Cancel();
            }

            UpdateDisplay();
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
            _ignoreChange = true;
            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            List<string> langList = PsdzDatabase.EcuTranslation.GetLanguages();
            foreach (string lang in langList)
            {
                comboBoxLanguage.Items.Add(lang);
            }

            comboBoxLanguage.SelectedIndex = 0;
            comboBoxLanguage.EndUpdate();
            _ignoreChange = false;

            LoadSettings();
            UpdateDisplay();
            UpdateStatus();
            timerUpdate.Enabled = true;
            labelProgressEvent.Text = string.Empty;

            if (_programmingJobs.IsModuleGenerationMode())
            {
                buttonConnect_Click(null, null);
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void buttonStopHost_Click(object sender, EventArgs e)
        {
            if (TaskActive)
            {
                return;
            }

            StopProgrammingServiceTask(textBoxIstaFolder.Text, true).ContinueWith(task =>
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

            if (_programmingJobs.ProgrammingService != null && _programmingJobs.ProgrammingService.IsPsdzPsdzServiceHostInitialized())
            {
                buttonStopHost_Click(sender, null);
                e.Cancel = true;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (TaskActive)
            {
                return;
            }

            if (_programmingJobs.PsdzContext?.Connection != null)
            {
                return;
            }

            StoreSettings();    // required for sub process
            bool useIcom = checkBoxIcom.Checked;
            _programmingJobs.GenServiceModules = checkBoxGenServiceModules.Checked;
            _cts = new CancellationTokenSource();
            ConnectVehicleTask(textBoxIstaFolder.Text, ipAddressControlVehicleIp.Text, useIcom).ContinueWith(task =>
            {
                TaskActive = false;
                _cts.Dispose();
                _cts = null;
                if (_programmingJobs.IsModuleGenerationMode())
                {
                    BeginInvoke((Action)(() =>
                    {
                        Environment.ExitCode = task.Result ? 0 : 1;
                        Close();
                    }));
                }
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (TaskActive)
            {
                return;
            }

            if (_programmingJobs.PsdzContext?.Connection == null)
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
            if (TaskActive)
            {
                return;
            }

            if (_programmingJobs.PsdzContext?.Connection == null)
            {
                return;
            }

            ProgrammingJobs.OperationType operationType = ProgrammingJobs.OperationType.CreateOptions;
            if (sender == buttonCreateOptions)
            {
                operationType = ProgrammingJobs.OperationType.CreateOptions;
            }
            else if (sender == buttonModILevel)
            {
                operationType = ProgrammingJobs.OperationType.BuildTalILevel;
                UpdateTargetFa(true);
            }
            else if (sender == buttonModFa)
            {
                operationType = ProgrammingJobs.OperationType.BuildTalModFa;
                UpdateTargetFa();
            }
            else if (sender == buttonExecuteTal)
            {
                operationType = ProgrammingJobs.OperationType.ExecuteTal;
            }

            _programmingJobs.LicenseValid = true;
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
            if (TaskActive)
            {
                return;
            }

            bool preferIcom = checkBoxIcom.Checked;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Resources.VehicleSearching);
            UpdateStatus(sb.ToString());

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
                            bool iCom = connectionSelected.ConnectionType == EdInterfaceEnet.EnetConnection.InterfaceType.Icom;
                            ipAddressControlVehicleIp.Text = connectionSelected.IpAddress.ToString();
                            checkBoxIcom.Checked = iCom;
                            sb.AppendLine(string.Format(Resources.VehicleIp, connectionSelected.IpAddress, iCom));
                            ipValid = true;
                        }
                    }
                    catch (Exception)
                    {
                        ipValid = false;
                    }

                    if (!ipValid)
                    {
                        sb.AppendLine(Resources.VehiceNotDetected);
                        ipAddressControlVehicleIp.Text = DefaultIp;
                        checkBoxIcom.Checked = false;
                    }

                    UpdateStatus(sb.ToString());
                }));
            });

            TaskActive = true;
            UpdateDisplay();
        }

        private void buttonInternalTest_Click(object sender, EventArgs e)
        {
            if (TaskActive)
            {
                return;
            }

            string initialDirectory = Application.StartupPath;
            string initialFileName = string.Empty;
            if (!string.IsNullOrEmpty(_lastTestFileName))
            {
                string lastDir = Path.GetDirectoryName(_lastTestFileName);
                if (Directory.Exists(lastDir))
                {
                    initialDirectory = lastDir;
                    if (File.Exists(_lastTestFileName))
                    {
                        initialFileName = Path.GetFileName(_lastTestFileName);
                    }
                }
            }

            openFileDialogTest.InitialDirectory = initialDirectory;
            openFileDialogTest.FileName = initialFileName;

            if (openFileDialogTest.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string configurationContainerXml;
            try
            {
                string fileName = openFileDialogTest.FileName;
                _lastTestFileName = fileName;
                configurationContainerXml = File.ReadAllText(fileName);
            }
            catch (Exception)
            {
                return;
            }

            Dictionary<string, string> runOverrideDict = new Dictionary<string, string>();
            if (_programmingJobs.PsdzContext?.Connection == null)
            {
                string convertResult = DetectVehicle.ConvertContainerXml(configurationContainerXml, runOverrideDict);
                if (string.IsNullOrEmpty(convertResult))
                {
                    UpdateStatus(Resources.ContainerError);
                    return;
                }

                UpdateStatus(convertResult);
                return;
            }

            runOverrideDict.Add("/Run/Group/G_MOTOR/VirtualVariantJob/ABGLEICH_CSF_PROG/Argument/ECUGroupOrVariant", "G_MOTOR");
            StringBuilder sb = new StringBuilder();
            UpdateStatus(sb.ToString());
            _cts = new CancellationTokenSource();
            InternalTestTask(configurationContainerXml, runOverrideDict).ContinueWith(task =>
            {
                TaskActive = false;
                _cts.Dispose();
                _cts = null;

                BeginInvoke((Action)(() =>
                {
                    if (!string.IsNullOrEmpty(task.Result))
                    {
                        sb.AppendLine(task.Result);
                    }
                    else
                    {
                        sb.AppendLine(Resources.NoResponse);
                    }

                    UpdateStatus(sb.ToString());
                }));
            });

            TaskActive = true;
            UpdateDisplay();
        }


        private void buttonDecryptFile_Click(object sender, EventArgs e)
        {
            if (TaskActive)
            {
                return;
            }

            string initialDirectory = Application.StartupPath;
            string initialFileName = string.Empty;
            if (!string.IsNullOrEmpty(_lastDecryptFileName))
            {
                string lastDir = Path.GetDirectoryName(_lastDecryptFileName);
                if (Directory.Exists(lastDir))
                {
                    initialDirectory = lastDir;
                    if (File.Exists(_lastDecryptFileName))
                    {
                        initialFileName = Path.GetFileName(_lastDecryptFileName);
                    }
                }
            }

            openFileDialogDecrypt.InitialDirectory = initialDirectory;
            openFileDialogDecrypt.FileName = initialFileName;

            if (openFileDialogDecrypt.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string fileName = openFileDialogDecrypt.FileName;
            _lastDecryptFileName = fileName;
            string text = DecryptFile(fileName);

            if (string.IsNullOrEmpty(text))
            {
                UpdateStatus(Resources.DecryptionFailed);
                return;
            }

            UpdateStatus(text);
            textBoxStatus.ReadOnly = false;
            _decryptEditMode = true;
            UpdateDisplay();
        }

        private void checkedListBoxOptions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_ignoreCheck)
            {
                return;
            }

            if (_programmingJobs?.SelectedOptions == null)
            {
                return;
            }

            Dictionary<PsdzDatabase.SwiRegisterEnum, List<ProgrammingJobs.OptionsItem>> optionsDict = _programmingJobs.OptionsDict;
            bool modified = false;
            if (e.Index >= 0 && e.Index < checkedListBoxOptions.Items.Count)
            {
                if (checkedListBoxOptions.Items[e.Index] is ProgrammingJobs.OptionsItem optionsItem)
                {
                    PsdzDatabase.SwiRegisterEnum swiRegisterEnum = optionsItem.SwiRegisterEnum;
                    if (_programmingJobs.SelectedOptions.Count > 0)
                    {
                        PsdzDatabase.SwiRegisterEnum swiRegisterEnumCurrent = _programmingJobs.SelectedOptions[0].SwiRegisterEnum;
                        if (PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnum) != PsdzDatabase.GetSwiRegisterGroup(swiRegisterEnumCurrent))
                        {
                            _programmingJobs.SelectedOptions.Clear();
                        }
                    }

                    if (!optionsDict.TryGetValue(swiRegisterEnum, out List<ProgrammingJobs.OptionsItem> optionsItems))
                    {
                        log.ErrorFormat("checkedListBoxOptions_ItemCheck No option items for: {0}", swiRegisterEnum);
                    }

                    if (e.CurrentValue == CheckState.Indeterminate)
                    {
                        e.NewValue = e.CurrentValue;
                    }
                    else
                    {
                        if (_programmingJobs.SelectedOptions != null)
                        {
                            List<ProgrammingJobs.OptionsItem> combinedOptionsItems = _programmingJobs.GetCombinedOptionsItems(optionsItem, optionsItems);
                            if (e.NewValue == CheckState.Checked)
                            {
                                if (!_programmingJobs.SelectedOptions.Contains(optionsItem))
                                {
                                    _programmingJobs.SelectedOptions.Add(optionsItem);
                                }

                                if (combinedOptionsItems != null)
                                {
                                    foreach (ProgrammingJobs.OptionsItem combinedItem in combinedOptionsItems)
                                    {
                                        if (!_programmingJobs.SelectedOptions.Contains(combinedItem))
                                        {
                                            _programmingJobs.SelectedOptions.Add(combinedItem);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _programmingJobs.SelectedOptions.Remove(optionsItem);

                                if (combinedOptionsItems != null)
                                {
                                    foreach (ProgrammingJobs.OptionsItem combinedItem in combinedOptionsItems)
                                    {
                                        _programmingJobs.SelectedOptions.Remove(combinedItem);
                                    }
                                }
                            }
                        }

                        modified = true;
                    }
                }
            }

            if (modified)
            {
                PsdzContext psdzContext = _programmingJobs.PsdzContext;
                if (psdzContext?.Connection != null)
                {
                    psdzContext.Tal = null;
                }

                BeginInvoke((Action)(() =>
                {
                    UpdateTargetFa();
                }));
            }
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

            string language = comboBoxLanguage.SelectedItem.ToString();
            SetLanguage(language);

            BeginInvoke((Action)(() =>
            {
                UpdateCurrentOptions();
            }));
        }
    }
}
