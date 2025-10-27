using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Windows.Foundation;
using BmwFileReader;
using EdiabasLib;
using Microsoft.Win32;
using Peak.Can.Basic;
// ReSharper disable LocalizableElement

namespace CarSimulator
{
    public partial class MainForm : Form
    {
        public const string Api32DllName = @"api32.dll";
        public const string ResponseFileStd = "g31_coding.txt";
        public const string ResponseFileE61 = "e61.txt";
        public const string RegKeyFtdiBus = @"SYSTEM\CurrentControlSet\Enum\FTDIBUS";
        public const string RegValueFtdiLatencyTimer = @"LatencyTimer";
        public const int DefaultSslPort = 3496;
        private const string EcuDirName = "Ecu";
        private const string ResponseDirName = "Response";
        private volatile bool _launchingSettings;
        private string _appDir;
        private string _ediabasBinDirBmw;
        private string _ediabasEcuDirBmw;
        private string _ediabasBinDirIstad;
        private string _ediabasEcuDirIstad;
        private string _rootFolder;
        private string _ecuFolder;
        private string _responseDir;
        private bool _serverUseDoIP = false;
        private string _serverCertFile;
        private string _serverCertPwd;
        private int _serverSslPort = DefaultSslPort;
        private bool _serverUseBcSsl = true;
        private CommThread _commThread;
        private int _lastPortCount;
        private readonly CommThread.ConfigData _configData;
        private EdiabasNet _ediabas;
        private SgFunctions _sgFunctions;
        private DeviceTest _deviceTest;
        private bool _closeRequest;

        public string responseDir => _responseDir;
        public CommThread commThread => _commThread;
        public CommThread.ConfigData threadConfigData => _configData;
        public EdiabasNet ediabas => _ediabas;
        public SgFunctions sgFunctions => _sgFunctions;
        public DeviceTest deviceTest => _deviceTest;

        public MainForm()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            InitializeComponent();

            GetDirectories();
            _rootFolder = Properties.Settings.Default.RootFolder;
            _ecuFolder = Properties.Settings.Default.EcuFolder;
            _serverUseDoIP = Properties.Settings.Default.ServerDoIP;
            _serverCertFile = Properties.Settings.Default.ServerCertFile;
            _serverCertPwd = Properties.Settings.Default.ServerCertPwd;
            try
            {
                _serverSslPort = Properties.Settings.Default.ServerSslPort;
            }
            catch (Exception)
            {
                _serverSslPort = DefaultSslPort;
            }
            _serverUseBcSsl = Properties.Settings.Default.ServerUseBcSsl;

            _appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(_rootFolder) || !Directory.Exists(_rootFolder))
            {
                _rootFolder = GetParentSubDir(_appDir, ResponseDirName, 4);
            }

            _responseDir = _rootFolder;

            if (string.IsNullOrEmpty(_ecuFolder) || !Directory.Exists(_ecuFolder))
            {
                if (!string.IsNullOrEmpty(_ediabasEcuDirIstad))
                {
                    _ecuFolder = _ediabasEcuDirIstad;
                }
            }

            if (string.IsNullOrEmpty(_ecuFolder))
            {
                if (!string.IsNullOrEmpty(_ediabasEcuDirBmw))
                {
                    _ecuFolder = _ediabasEcuDirBmw;
                }
            }

            _lastPortCount = -1;
            _configData = new CommThread.ConfigData();
            UpdateDirectoryList(_rootFolder);
            UpdateResponseFiles(_responseDir);
            _commThread = new CommThread(this);
            _ediabas = new EdiabasNet();
            _sgFunctions = new SgFunctions(_ediabas);
            _deviceTest = new DeviceTest(this);
            UpdatePorts();
            timerUpdate.Enabled = true;
            UpdateDisplay();
            UpdateCommThreadConfig();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_commThread != null)
            {
                _commThread.Dispose();
                _commThread = null;
            }

            if (_deviceTest != null)
            {
                _deviceTest.Dispose();
                _deviceTest = null;
            }

            if (_sgFunctions != null)
            {
                _sgFunctions.Dispose();
                _sgFunctions = null;
            }

            if (_ediabas != null)
            {
                _ediabas.Dispose();
                _ediabas = null;
            }

            try
            {
                Properties.Settings.Default.RootFolder = _rootFolder;
                Properties.Settings.Default.EcuFolder = _ecuFolder;
                Properties.Settings.Default.ServerDoIP = _serverUseDoIP;
                Properties.Settings.Default.ServerCertFile = _serverCertFile;
                Properties.Settings.Default.ServerCertPwd = _serverCertPwd;
                Properties.Settings.Default.ServerSslPort = _serverSslPort;
                Properties.Settings.Default.ServerUseBcSsl = _serverUseBcSsl;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void GetDirectories()
        {
            string dirBmw = Environment.GetEnvironmentVariable("ediabas_config_dir");
            if (!IsValidEdiabasDir(dirBmw))
            {
                string path = Environment.GetEnvironmentVariable("EDIABAS_PATH");
                if (!string.IsNullOrEmpty(path))
                {
                    dirBmw = Path.Combine(path, @"bin");
                }
            }

            if (!IsValidEdiabasDir(dirBmw))
            {
                string path = LocateFileInPath(Api32DllName);
                if (!string.IsNullOrEmpty(path))
                {
                    dirBmw = path;
                }
            }

            if (IsValidEdiabasDir(dirBmw))
            {
                _ediabasBinDirBmw = dirBmw;
                _ediabasEcuDirBmw = GetParentSubDir(_ediabasBinDirBmw, EcuDirName, 1);
            }

            try
            {
                using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key = localMachine32.OpenSubKey(@"SOFTWARE\BMWGroup\ISPI\ISTA"))
                    {
                        string path = key?.GetValue("InstallLocation", null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dirIstad = Path.Combine(path, @"Ediabas", @"BIN");
                            if (IsValidEdiabasDir(dirIstad))
                            {
                                _ediabasBinDirIstad = dirIstad;
                                _ediabasEcuDirIstad = GetParentSubDir(_ediabasBinDirIstad, EcuDirName, 2);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string LocateFileInPath(string fileName)
        {
            string envPath = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(envPath))
            {
                return null;
            }

            string result = envPath
                .Split(';')
                .FirstOrDefault(s => File.Exists(Path.Combine(s, fileName)));

            return result;
        }

        public string GetParentSubDir(string startDir, string subDir, int maxLevel)
        {
            try
            {
                string parentDir = startDir;
                int level = 0;

                while (!string.IsNullOrEmpty(parentDir) && level < maxLevel)
                {
                    if (!Directory.Exists(parentDir))
                    {
                        return null;
                    }

                    DirectoryInfo directoryInfo = Directory.GetParent(parentDir);
                    if (directoryInfo == null)
                    {
                        return null;
                    }

                    parentDir = directoryInfo.FullName;
                    string ecuDir = Path.Combine(parentDir, subDir);

                    if (Directory.Exists(ecuDir))
                    {
                        return ecuDir;
                    }

                    level++;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool StartSettingsApp(string settingsType)
        {
            try
            {
                if (string.IsNullOrEmpty(settingsType))
                {
                    return false;
                }

                if (_launchingSettings)
                {
                    return false;
                }

                _launchingSettings = true;
                IAsyncOperation<bool> launchUri =
                    Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:" + settingsType));
                launchUri.AsTask().ContinueWith(task =>
                {
                    if (InvokeRequired)
                    {
                        _launchingSettings = false;
                        BeginInvoke((Action)UpdateDisplay);
                    }
                });
                return true;
            }
            catch (Exception)
            {
                _launchingSettings = false;
                return false;
            }
            finally
            {
                UpdateDisplay();
            }
        }

        public static bool IsValidEdiabasDir(string dirName)
        {
            try
            {
                if (string.IsNullOrEmpty(dirName))
                {
                    return false;
                }
                string dllFile = Path.Combine(dirName, Api32DllName);
                if (!File.Exists(dllFile))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static List<string> GetFtdiRegKeys(string comPort, string serialString)
        {
            if (string.IsNullOrEmpty(comPort))
            {
                return null;
            }

            List<string> regKeys = new List<string>();
            try
            {
                using (RegistryKey ftdiBusKey = Registry.LocalMachine.OpenSubKey(RegKeyFtdiBus, false))
                {
                    if (ftdiBusKey != null)
                    {
                        foreach (string subKeyName in ftdiBusKey.GetSubKeyNames())
                        {
                            if (!string.IsNullOrEmpty(serialString))
                            {
                                if (!subKeyName.Contains(serialString))
                                {
                                    continue;
                                }
                            }

                            string paramKeyName = subKeyName + @"\0000\Device Parameters";
                            using (RegistryKey paramKey = ftdiBusKey.OpenSubKey(paramKeyName))
                            {
                                if (paramKey != null)
                                {
                                    string portName = paramKey.GetValue("PortName") as string;
                                    if (string.Compare(portName, comPort, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        regKeys.Add(RegKeyFtdiBus + @"\" + paramKeyName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return regKeys;
        }

        public static List<int> GetFtdiLatencyTimer(string comPort, string serialString)
        {
            List<string> regKeys = GetFtdiRegKeys(comPort, serialString);
            if (regKeys == null)
            {
                return null;
            }

            List<int> latencyTimers = new List<int>();
            foreach (string regKey in regKeys)
            {
                try
                {
                    using (RegistryKey ftdiKey = Registry.LocalMachine.OpenSubKey(regKey, false))
                    {
                        if (ftdiKey != null)
                        {
                            object latencyTimer = ftdiKey.GetValue(RegValueFtdiLatencyTimer);
                            if (latencyTimer is int latencyValue)
                            {
                                latencyTimers.Add(latencyValue);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return latencyTimers;
        }

        private void UpdatePorts()
        {
            if (_commThread.ThreadRunning())
            {
                return;
            }
            // ReSharper disable once ConstantNullCoalescingCondition
            string[] ports = SerialPort.GetPortNames() ?? Array.Empty<string>();
            if (_lastPortCount == ports.Length)
            {
                return;
            }

            listPorts.BeginUpdate();
            listPorts.Items.Clear();
            int index = listPorts.Items.Add("ENET");
            int indexEnet = index;
            Regex rx = new Regex("(COM[0-9]+).*");
            foreach (string port in ports)
            {
                string portFixed = rx.Replace(port, "$1");
                index = listPorts.Items.Add(portFixed);
            }
            try
            {
                TPCANStatus stsResult = PCANBasic.GetValue(PCANBasic.PCAN_USBBUS1, TPCANParameter.PCAN_CHANNEL_CONDITION, out UInt32 _, sizeof(UInt32));
                //if ((stsResult == TPCANStatus.PCAN_ERROR_OK) && (iBuffer == PCANBasic.PCAN_CHANNEL_AVAILABLE))
                if (stsResult == TPCANStatus.PCAN_ERROR_OK)
                {
                    index = listPorts.Items.Add("CAN");
                }
            }
            catch (Exception)
            {
                // ignored
            }

            listPorts.SelectedIndex = indexEnet;
            listPorts.EndUpdate();

            buttonConnect.Enabled = listPorts.SelectedIndex >= 0;
            _lastPortCount = ports.Length;
        }

        private void UpdateDirectoryList(string path)
        {
            treeViewDirectories.Nodes.Clear();
            TreeNode node = new TreeNode
            {
                Text = path,
                Tag = path
            };
            treeViewDirectories.BeginUpdate();
            treeViewDirectories.Nodes.Add(node);
            FillDirectory(path, node, 0);
            treeViewDirectories.ExpandAll();
            treeViewDirectories.SelectedNode = node;
            treeViewDirectories.EndUpdate();
        }

        private void FillDirectory(string path, TreeNode parent, int level)
        {
            try
            {
                // limit levels
                level++;
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    return;
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    if (di.Attributes.HasFlag(FileAttributes.Directory) &&
                        string.Compare(di.Name, "runtimes", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }

                    TreeNode child = new TreeNode
                    {
                        Text = di.Name,
                    };
                    string fullPath = Path.Combine(path, di.Name);
                    child.Tag = fullPath;
                    parent.Nodes.Add(child);

                    FillDirectory(child.FullPath, child, level);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateResponseFiles(string path)
        {
            listBoxResponseFiles.BeginUpdate();

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.txt");
                listBoxResponseFiles.Items.Clear();
                string selectItem = null;
                foreach (string file in files)
                {
                    string baseFileName = Path.GetFileName(file);
                    if (!string.IsNullOrEmpty(baseFileName))
                    {
                        if (string.Compare(baseFileName, ResponseFileStd, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            selectItem = baseFileName;
                        }
                        listBoxResponseFiles.Items.Add(baseFileName);
                    }
                }
                if (selectItem != null)
                {
                    listBoxResponseFiles.SelectedItem = selectItem;
                }
            }

            listBoxResponseFiles.EndUpdate();
        }

        public bool ReadResponseFile(string fileName, CommThread.ConceptType conceptType)
        {
            if (!File.Exists(fileName)) return false;

            List<byte[]> configList = _configData.ConfigList;
            List<CommThread.ResponseEntry> responseOnlyList = _configData.ResponseOnlyList;
            List<CommThread.ResponseEntry> responseList = _configData.ResponseList;
            try
            {
                configList.Clear();
                responseOnlyList.Clear();
                responseList.Clear();
                using (StreamReader streamReader = new StreamReader(fileName))
                {
                    List<byte> configData = null;
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.StartsWith(";")) continue;
                        if (line.Length < 2) continue;

                        string[] numberArray;
                        if (line.ToUpper().StartsWith("CFG:"))
                        {
                            configData = new List<byte>();
                            line = line.Substring(4);
                            numberArray = line.Split(' ');
                            foreach (string number in numberArray)
                            {
                                if (string.IsNullOrEmpty(number))
                                {
                                    continue;
                                }
                                try
                                {
                                    int value = Convert.ToInt32(number, 16);
                                    configData.Add((byte)value);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                            configList.Add(configData.ToArray());
                            continue;
                        }

                        numberArray = line.Split(' ');
                        bool responseData = false;
                        List<byte> listCompare = new List<byte>();
                        List<byte> listResponse = new List<byte>();

                        foreach (string number in numberArray)
                        {
                            if (string.IsNullOrEmpty(number))
                            {
                                continue;
                            }
                            if (number == ":")
                            {
                                responseData = true;
                            }
                            else
                            {
                                try
                                {
                                    int value = Convert.ToInt32(number, 16);
                                    if (responseData)
                                    {
                                        listResponse.Add((byte)value);
                                    }
                                    else
                                    {
                                        listCompare.Add((byte)value);
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        if (listCompare.Count == 0)
                        {   // empty request
                            if (listResponse.Count > 0)
                            {
                                responseOnlyList.Add(new CommThread.ResponseEntry(null, listResponse.ToArray(), configData?.ToArray()));
                            }
                        }
                        else if (listCompare.Count > 0 && listResponse.Count > 0)
                        {
                            // find duplicates
                            bool addEntry = true;
                            foreach (CommThread.ResponseEntry responseEntry in responseList)
                            {
                                bool equal = true;
                                if (listCompare.Count != responseEntry.Request.Length) continue;
                                if (configData != null)
                                {
                                    byte[] configBytes = configData.ToArray();
                                    if ((responseEntry.Config == null) || (responseEntry.Config.Length != configBytes.Length)) continue;
                                    // ReSharper disable once LoopCanBeConvertedToQuery
                                    for (int i = 0; i < configBytes.Length; i++)
                                    {
                                        if (configBytes[i] != responseEntry.Config[i])
                                        {
                                            equal = false;
                                            break;
                                        }
                                    }
                                }
                                // ReSharper disable once LoopCanBeConvertedToQuery
                                for (int i = 0; i < listCompare.Count; i++)
                                {
                                    if (listCompare[i] != responseEntry.Request[i])
                                    {
                                        equal = false;
                                        break;
                                    }
                                }
                                if (equal)
                                {       // entry found
                                    responseEntry.ResponseList.Add(listResponse.ToArray());
                                    addEntry = false;
                                    break;
                                }
                            }

                            if (addEntry)
                            {
                                responseList.Add(new CommThread.ResponseEntry(listCompare.ToArray(), listResponse.ToArray(), configData?.ToArray()));
                            }
                        }
                    }
                }

                bool messageShown = false;
                // split multi telegram responses
                if (conceptType == CommThread.ConceptType.ConceptKwp1281)
                {
                    List<CommThread.ResponseEntry> combinedList = new List<CommThread.ResponseEntry>();
                    combinedList.AddRange(responseList);
                    combinedList.AddRange(responseOnlyList);
                    foreach (CommThread.ResponseEntry responseEntry in combinedList)
                    {
                        if (responseEntry.Request != null)
                        {
                            if (responseEntry.Request.Length < 3)
                            {
                                if (!messageShown)
                                {
                                    messageShown = true;
                                    MessageBox.Show("Invalid response file request length!");
                                }
                            }
                            else
                            {
                                int telLength = responseEntry.Request[0];
                                if (telLength != responseEntry.Request.Length)
                                {
                                    if (!messageShown)
                                    {
                                        messageShown = true;
                                        MessageBox.Show("Invalid response file request!");
                                    }
                                }
                            }
                        }

                        byte[] response = responseEntry.ResponseList[0];
                        int telOffset = 0;
                        while ((telOffset + 1) < response.Length)
                        {
                            if ((response.Length - telOffset) < 3)
                            {
                                break;
                            }
                            int telLength = response[telOffset + 0];
                            if (telLength < 3)
                            {
                                if (!messageShown)
                                {
                                    messageShown = true;
                                    MessageBox.Show("Invalid response file response!");
                                }
                                break;
                            }
                            if (telOffset + telLength > response.Length)
                            {
                                break;
                            }
                            byte[] responseTel = new byte[telLength];
                            Array.Copy(response, telOffset, responseTel, 0, telLength);
                            responseEntry.ResponseMultiList.Add(responseTel);
                            telOffset += telLength;
                        }
                        if (telOffset != response.Length)
                        {
                            if (!messageShown)
                            {
                                messageShown = true;
                                MessageBox.Show("Invalid response file response!");
                            }
                        }
                    }
                }
                else if ((conceptType != CommThread.ConceptType.Concept1) && (conceptType != CommThread.ConceptType.Concept3))
                {
                    foreach (CommThread.ResponseEntry responseEntry in responseList)
                    {
                        {
                            if (responseEntry.Request.Length < 4)
                            {
                                if (!messageShown)
                                {
                                    messageShown = true;
                                    MessageBox.Show("Invalid response file request length!");
                                }
                            }
                            else
                            {
                                int telLength = responseEntry.Request[0] & 0x3F;
                                if (telLength == 0)
                                {   // with length byte
                                    if (responseEntry.Request[3] == 0)
                                    {
                                        telLength = ((responseEntry.Request[4] << 8) | responseEntry.Request[5]) + 7;
                                    }
                                    else
                                    {
                                        telLength = responseEntry.Request[3] + 5;
                                    }
                                }
                                else
                                {
                                    telLength += 4;
                                }
                                if (telLength != responseEntry.Request.Length)
                                {
                                    if (!messageShown)
                                    {
                                        messageShown = true;
                                        MessageBox.Show("Invalid response file request!");
                                    }
                                }
                            }
                        }

                        byte[] response = responseEntry.ResponseList[0];
                        int telOffset = 0;
                        while ((telOffset + 1) < response.Length)
                        {
                            if ((response.Length - telOffset) < 4)
                            {
                                break;
                            }
                            int telLength = response[telOffset + 0] & 0x3F;
                            if (telLength == 0)
                            {   // with length byte
                                if (response[telOffset + 3] == 0)
                                {
                                    telLength = ((response[telOffset + 4] << 8) | response[telOffset + 5]) + 7;
                                }
                                else
                                {
                                    telLength = response[telOffset + 3] + 5;
                                }
                            }
                            else
                            {
                                telLength += 4;
                            }
                            if (telLength < 4)
                            {
                                if (!messageShown)
                                {
                                    messageShown = true;
                                    MessageBox.Show("Invalid response file response!");
                                }
                                break;
                            }
                            if (telOffset + telLength > response.Length)
                            {
                                break;
                            }
                            byte[] responseTel = new byte[telLength];
                            Array.Copy(response, telOffset, responseTel, 0, telLength);
                            responseEntry.ResponseMultiList.Add(responseTel);
                            telOffset += telLength;
                        }
                        if (telOffset != response.Length)
                        {
                            if (!messageShown)
                            {
                                messageShown = true;
                                MessageBox.Show("Invalid response file response!");
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void UpdateDisplay()
        {
            bool connected = _commThread != null && _commThread.ThreadRunning();
            bool testing = _deviceTest != null && _deviceTest.TestActive;
            bool testAborted = _deviceTest != null && _deviceTest.AbortTest;
            bool active = connected || testing || _launchingSettings;
            bool testFilePresent = !string.IsNullOrEmpty(responseDir) && File.Exists(Path.Combine(responseDir, ResponseFileE61));
            bool ecuFolderExits = !string.IsNullOrEmpty(_ecuFolder) && Directory.Exists(_ecuFolder);

            textBoxEcuFolder.Text = _ecuFolder ?? string.Empty;
            checkBoxEnetDoIp.Checked = _serverUseDoIP;
            textBoxServerCert.Text = _serverCertFile ?? string.Empty;
            textBoxCertPwd.Text = _serverCertPwd ?? string.Empty;
            textBoxCertPwd.Enabled = !active;
            textBoxSslPort.Text = _serverSslPort.ToString(CultureInfo.InvariantCulture);
            textBoxSslPort.Enabled = !active;
            checkBoxBcSsl.Checked = _serverUseBcSsl;
            checkBoxBcSsl.Enabled = !active;
            textBoxCertPwd.Enabled = !active;
            buttonConnect.Text = connected && !testing ? "Disconnect" : "Connect";
            buttonConnect.Enabled = !testing;
            buttonErrorDefault.Enabled = !testing;
            buttonDeviceTestBtEdr.Enabled = !active && testFilePresent && ecuFolderExits;
            buttonDeviceTestBtBle.Enabled = !active && testFilePresent && ecuFolderExits;
            buttonDeviceTestWifi.Enabled = !active && testFilePresent && ecuFolderExits;
            buttonAbortTest.Enabled = testing && !testAborted;
            buttonEcuFolder.Enabled = !active;
            buttonRootFolder.Enabled = !active;
            buttonServerCert.Enabled = !active;
            checkBoxMoving.Enabled = !testing;
            checkBoxVariableValues.Enabled = !testing;
            checkBoxIgnitionOk.Enabled = !testing;
            checkBoxAdsAdapter.Enabled = !active;
            checkBoxKLineResponder.Enabled = !active;
            checkBoxHighTestVoltage.Enabled = !active;
            checkBoxBtNameStd.Enabled = !active;
            checkBoxEnetDoIp.Enabled = !active;
            groupBoxConcepts.Enabled = !active;
        }

        private bool CheckPortLatencyTime(string comPort, string serialString = null)
        {
            List<int> regLatencyTimers = GetFtdiLatencyTimer(comPort, serialString);
            if (regLatencyTimers != null && regLatencyTimers.Count > 0)
            {
                int maxRegLatencyTimer = regLatencyTimers.Max();
                if (maxRegLatencyTimer > 1)
                {
                    MessageBox.Show(string.Format("Port latency time too large: {0}ms", maxRegLatencyTimer));
                    return false;
                }
            }
            return true;
        }

        private void UpdateCommThreadConfig()
        {
            _commThread.Moving = checkBoxMoving.Checked;
            _commThread.VariableValues = checkBoxVariableValues.Checked;
            _commThread.IgnitionOk = checkBoxIgnitionOk.Checked;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_commThread.ThreadRunning())
            {
                _commThread.StopThread();
            }
            else
            {
                if (listPorts.SelectedIndex < 0)
                {
                    return;
                }

                string selectedPort = listPorts.SelectedItem?.ToString();
                if (!CheckPortLatencyTime(selectedPort))
                {
                    return;
                }

                CommThread.ConceptType conceptType = CommThread.ConceptType.ConceptBwmFast;
                if (radioButtonKwp2000Bmw.Checked) conceptType = CommThread.ConceptType.ConceptKwp2000Bmw;
                if (radioButtonKwp2000S.Checked) conceptType = CommThread.ConceptType.ConceptKwp2000S;
                if (radioButtonDs2.Checked) conceptType = CommThread.ConceptType.ConceptDs2;
                if (radioButtonConcept1.Checked) conceptType = CommThread.ConceptType.Concept1;
                if (radioButtonKwp1281.Checked) conceptType = CommThread.ConceptType.ConceptKwp1281;
                if (radioButtonConcept3.Checked) conceptType = CommThread.ConceptType.Concept3;
                if (radioButtonKwp2000.Checked) conceptType = CommThread.ConceptType.ConceptKwp2000;
                if (radioButtonTp20.Checked) conceptType = CommThread.ConceptType.ConceptTp20;

                CommThread.EnetCommType enetCommType = CommThread.EnetCommType.None;
                if (checkBoxEnetDoIp.Checked)
                {
                    enetCommType |= CommThread.EnetCommType.DoIp;
                }
                else
                {
                    enetCommType |= CommThread.EnetCommType.Hsfz;
                }

                string responseFile = (string)listBoxResponseFiles.SelectedItem;
                if (responseFile == null)
                {
                    return;
                }

                CommThread.ResponseType responseType = CommThread.ResponseType.Standard;
                if (string.Compare(responseFile, "e61.txt", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    responseType = CommThread.ResponseType.E61;
                }
                else if (string.Compare(responseFile, "e90.txt", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    responseType = CommThread.ResponseType.E90;
                }
                else if (responseFile.StartsWith("g31", StringComparison.OrdinalIgnoreCase))
                {
                    responseType = CommThread.ResponseType.G31;
                }
                else if (string.Compare(responseFile, "smg2.txt", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    responseType = CommThread.ResponseType.SMG2;
                }

                if (!Directory.Exists(_ecuFolder))
                {
                    MessageBox.Show("Ecu folder not existing");
                }
                _ediabas.SetConfigProperty("EcuPath", _ecuFolder);

                if (!ReadResponseFile(Path.Combine(_responseDir, responseFile), conceptType))
                {
                    MessageBox.Show("Reading response file failed!");
                }

                UpdateCommThreadConfig();
                _commThread.ServerCertFile = _serverCertFile;
                _commThread.ServerCertPwd = _serverCertPwd;
                _commThread.ServerSslPort = _serverSslPort;
                _commThread.ServerUseBcSsl = _serverUseBcSsl;
                _commThread.StartThread(selectedPort, conceptType, checkBoxAdsAdapter.Checked, checkBoxKLineResponder.Checked, responseType, _configData, enetCommType);
            }

            UpdateDisplay();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            UpdatePorts();
            //UpdateDisplay();
            if (_closeRequest && !_deviceTest.TestActive)
            {
                _closeRequest = false;
                Close();
            }
        }

        private void checkBoxMoving_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCommThreadConfig();
        }

        private void checkBoxIgnitionOk_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCommThreadConfig();
        }

        private void checkBoxVariableValues_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCommThreadConfig();
        }

        private void buttonErrorReset_Click(object sender, EventArgs e)
        {
            _commThread.ErrorDefault = true;
            UpdateDisplay();
        }

        private void treeViewDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode node = e.Node;
                if (node?.Tag is string path)
                {
                    _responseDir = path;
                    UpdateResponseFiles(path);
                    UpdateDisplay();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void UpdateTestStatusText(string text = null, bool appendText = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateTestStatusText(text, appendText);
                }));
                return;
            }

            if (text == null)
            {
                UpdateDisplay();
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                if (appendText)
                {
                    string lastText = richTextBoxTestResults.Text;
                    if (!string.IsNullOrEmpty(lastText))
                    {
                        sb.Append(lastText);
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("\r\n");
                    }
                    sb.Append(text);
                }

                richTextBoxTestResults.Text = sb.ToString();
                richTextBoxTestResults.SelectionStart = richTextBoxTestResults.TextLength;
                richTextBoxTestResults.Update();
                richTextBoxTestResults.ScrollToCaret();
            }
        }

        public void RefreshDisplay()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    RefreshDisplay();
                }));
                return;
            }

            UpdateDisplay();
        }

        private void buttonRootFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = _rootFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                _rootFolder = folderBrowserDialog.SelectedPath;
                UpdateDirectoryList(_rootFolder);
                UpdateDisplay();
            }
        }

        private void buttonEcuFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = _ecuFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                _ecuFolder = folderBrowserDialog.SelectedPath;
                UpdateDisplay();
            }
        }

        private void buttonDeviceTest_Click(object sender, EventArgs e)
        {
            string selectedPort = listPorts.SelectedItem?.ToString();
            if (!CheckPortLatencyTime(selectedPort))
            {
                return;
            }

            string btDeviceName = checkBoxBtNameStd.Checked ? DeviceTest.DefaultBtNameStd : DeviceTest.DefaultBtName;
            _deviceTest.MaxErrorVoltage = checkBoxHighTestVoltage.Checked ? 147 : 0;
            bool isWifiTest = sender == buttonDeviceTestWifi;
            bool enableBle = sender == buttonDeviceTestBtBle;

            _deviceTest.ExecuteTest(isWifiTest, enableBle, selectedPort, btDeviceName);
        }

        private void buttonAbortTest_Click(object sender, EventArgs e)
        {
            _deviceTest.AbortTest = true;
            UpdateDisplay();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_deviceTest.TestActive)
            {
                _closeRequest = true;
                _deviceTest.AbortTest = true;
                UpdateDisplay();
                e.Cancel = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream(Properties.Resources.AppIcon))
            {
                Icon = new Icon(ms);
            }

            checkBoxBtNameStd.Checked = true;
            checkBoxIgnitionOk.Checked = true;
            checkBoxKLineResponder.Checked = true;
            UpdateDisplay();
        }

        private void buttonServerCert_Click(object sender, EventArgs e)
        {
            string initDir = _appDir;
            string certFile = textBoxServerCert.Text;
            string fileName = string.Empty;
            if (File.Exists(certFile))
            {
                fileName = Path.GetFileName(certFile);
                initDir = Path.GetDirectoryName(certFile);
            }

            openCertFileDialog.FileName = fileName;
            openCertFileDialog.InitialDirectory = initDir ?? string.Empty;
            DialogResult result = openCertFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                _serverCertFile = openCertFileDialog.FileName;
                UpdateDisplay();
            }
        }

        private void textBoxCertPwd_TextChanged(object sender, EventArgs e)
        {
            _serverCertPwd = textBoxCertPwd.Text;
        }

        private void textBoxSslPort_TextChanged(object sender, EventArgs e)
        {
            _serverSslPort = int.TryParse(textBoxSslPort.Text, out int value) ? value : DefaultSslPort;
        }

        private void checkBoxBcSsl_CheckedChanged(object sender, EventArgs e)
        {
            _serverUseBcSsl = checkBoxBcSsl.Checked;
        }

        private void checkBoxEnetDoIp_CheckedChanged(object sender, EventArgs e)
        {
            _serverUseDoIP = checkBoxEnetDoIp.Checked;
        }

        private void richTextBoxTestResults_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            string url = e.LinkText;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (url.Contains("privacy-location"))
            {
                StartSettingsApp("privacy-location");
            }
        }
    }
}
