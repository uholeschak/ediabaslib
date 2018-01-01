using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Peak.Can.Basic;
// ReSharper disable LocalizableElement

namespace CarSimulator
{
    public partial class MainForm : Form
    {
        private const string StdResponseFile = "e61.txt";
        private string _rootFolder;
        private string _responseDir;
        private readonly CommThread _commThread;
        private int _lastPortCount;
        private readonly CommThread.ConfigData _configData;

        public string responseDir => _responseDir;
        public CommThread commThread => _commThread;
        public CommThread.ConfigData threadConfigData => _configData;

        public MainForm()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            InitializeComponent();

            _rootFolder = Properties.Settings.Default.RootFolder;
            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(appDir))
            {
                if (string.IsNullOrEmpty(_rootFolder) || !Directory.Exists(_rootFolder))
                {
                    _rootFolder = appDir;
                }
            }
            _responseDir = _rootFolder;

            _lastPortCount = -1;
            _configData = new CommThread.ConfigData();
            UpdateDirectoryList(_rootFolder);
            UpdateResponseFiles(_responseDir);
            _commThread = new CommThread();
            UpdatePorts();
            timerUpdate.Enabled = true;
            UpdateDisplay();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _commThread.StopThread();
            Properties.Settings.Default.RootFolder = _rootFolder;
            Properties.Settings.Default.Save();
        }

        private void UpdatePorts()
        {
            if (_commThread.ThreadRunning())
            {
                return;
            }
            // ReSharper disable once ConstantNullCoalescingCondition
            string[] ports = SerialPort.GetPortNames() ?? new string[0];
            if (_lastPortCount == ports.Length) return;
            listPorts.BeginUpdate();
            listPorts.Items.Clear();
            int index = listPorts.Items.Add("ENET");
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
            listPorts.SelectedIndex = index;
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
            string[] files = Directory.GetFiles(path, "*.txt");
            listBoxResponseFiles.BeginUpdate();
            listBoxResponseFiles.Items.Clear();
            string selectItem = null;
            foreach (string file in files)
            {
                string baseFileName = Path.GetFileName(file);
                if (!string.IsNullOrEmpty(baseFileName))
                {
                    if (string.Compare(baseFileName, StdResponseFile, StringComparison.OrdinalIgnoreCase) == 0)
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
                                    configData.Add((byte) value);
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
            bool connected = _commThread.ThreadRunning();
            buttonConnect.Text = connected ? "Disconnect" : "Connect";
            buttonDeviceTestBt.Enabled = !connected;
            buttonDeviceTestWifi.Enabled = !connected;
            if (connected)
            {
                _commThread.Moving = checkBoxMoving.Checked;
                _commThread.VariableValues = checkBoxVariableValues.Checked;
                _commThread.IgnitionOk = checkBoxIgnitionOk.Checked;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_commThread.ThreadRunning())
            {
                _commThread.StopThread();
            }
            else
            {
                if (listPorts.SelectedIndex < 0) return;
                string selectedPort = listPorts.SelectedItem.ToString();

                CommThread.ConceptType conceptType = CommThread.ConceptType.ConceptBwmFast;
                if (radioButtonKwp2000Bmw.Checked) conceptType = CommThread.ConceptType.ConceptKwp2000Bmw;
                if (radioButtonKwp2000S.Checked) conceptType = CommThread.ConceptType.ConceptKwp2000S;
                if (radioButtonDs2.Checked) conceptType = CommThread.ConceptType.ConceptDs2;
                if (radioButtonConcept1.Checked) conceptType = CommThread.ConceptType.Concept1;
                if (radioButtonKwp1281.Checked) conceptType = CommThread.ConceptType.ConceptKwp1281;
                if (radioButtonConcept3.Checked) conceptType = CommThread.ConceptType.Concept3;
                if (radioButtonKwp2000.Checked) conceptType = CommThread.ConceptType.ConceptKwp2000;
                if (radioButtonTp20.Checked) conceptType = CommThread.ConceptType.ConceptTp20;

                string responseFile = (string)listBoxResponseFiles.SelectedItem;
                if (responseFile == null)
                {
                    return;
                }

                CommThread.ResponseType responseType = CommThread.ResponseType.Standard;
                if (string.Compare(responseFile, StdResponseFile, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    responseType = CommThread.ResponseType.E61;
                }
                if (string.Compare(responseFile, "e90.txt", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    responseType = CommThread.ResponseType.E90;
                }

                if (!ReadResponseFile(Path.Combine(_responseDir, responseFile), conceptType))
                {
                    MessageBox.Show("Reading response file failed!");
                }

                _commThread.StartThread(selectedPort, conceptType, checkBoxAdsAdapter.Checked, checkBoxKLineResponder.Checked, responseType, _configData);
            }

            UpdateDisplay();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            UpdatePorts();
            UpdateDisplay();
        }

        private void checkBoxMoving_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void checkBoxIgnitionOk_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void buttonErrorReset_Click(object sender, EventArgs e)
        {
            _commThread.ErrorDefault = true;
        }

        private void treeViewDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode node = e.Node;
                if (node.Tag is string path)
                {
                    _responseDir = path;
                    UpdateResponseFiles(path);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void UpdateTestStatusText(string text)
        {
            textBoxTestResults.Text = text;
            textBoxTestResults.SelectionStart = textBoxTestResults.TextLength;
            textBoxTestResults.Update();
            textBoxTestResults.ScrollToCaret();
        }

        private void buttonRootFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = _rootFolder;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                _rootFolder = folderBrowserDialog.SelectedPath;
                UpdateDirectoryList(_rootFolder);
            }
        }

        private void buttonDeviceTest_Click(object sender, EventArgs e)
        {
            DeviceTest deviceTest = new DeviceTest(this);
            string selectedPort = listPorts.SelectedItem.ToString();
            deviceTest.ExecuteTest(sender == buttonDeviceTestWifi, selectedPort);
            deviceTest.Dispose();
        }
    }
}
