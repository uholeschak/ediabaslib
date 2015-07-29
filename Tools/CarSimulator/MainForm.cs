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
        private readonly string _responseDir;
        private readonly CommThread _commThread;
        private int _lastPortCount;
        private readonly CommThread.ConfigData _configData;

        public MainForm()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            InitializeComponent();

            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(appDir))
            {
                _responseDir = Path.Combine(appDir, "Response");
            }

            _lastPortCount = -1;
            _configData = new CommThread.ConfigData();
            UpdateResponseFiles();
            UpdatePorts();
            _commThread = new CommThread();
            timerUpdate.Enabled = true;
            UpdateDisplay();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _commThread.StopThread();
        }

        private void UpdatePorts()
        {
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
                UInt32 iBuffer;
                TPCANStatus stsResult = PCANBasic.GetValue(PCANBasic.PCAN_USBBUS1, TPCANParameter.PCAN_CHANNEL_CONDITION, out iBuffer, sizeof(UInt32));
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

        private void UpdateResponseFiles()
        {
            string[] files = Directory.GetFiles(_responseDir, "*.txt");
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

        private bool ReadResponseFile(string fileName, CommThread.ConceptType conceptType)
        {
            if (!File.Exists(fileName)) return false;

            List<byte> configList = _configData.ConfigList;
            List<byte[]> responseOnlyList = _configData.ResponseOnlyList;
            List<CommThread.ResponseEntry> responseList = _configData.ResponseList;
            try
            {
                configList.Clear();
                responseOnlyList.Clear();
                responseList.Clear();
                using (StreamReader streamReader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.StartsWith(";")) continue;
                        if (line.Length < 2) continue;

                        string[] numberArray;
                        if (line.ToUpper().StartsWith("CFG:"))
                        {
                            configList.Clear();
                            line = line.Substring(4);
                            numberArray = line.Split(' ');
                            foreach (string number in numberArray)
                            {
                                if (number.Length > 1)
                                {
                                    try
                                    {
                                        int value = Convert.ToInt32(number, 16);
                                        configList.Add((byte)value);
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                }
                            }
                            continue;
                        }

                        numberArray = line.Split(' ');
                        bool responseData = false;
                        List<byte> listCompare = new List<byte>();
                        List<byte> listResponse = new List<byte>();

                        foreach (string number in numberArray)
                        {
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
                                responseOnlyList.Add(listResponse.ToArray());
                            }
                        }
                        else if (listCompare.Count > 0 && listResponse.Count > 0)
                        {
                            // find duplicates
                            bool addEntry = true;
                            foreach (CommThread.ResponseEntry responseEntry in responseList)
                            {
                                if (listCompare.Count != responseEntry.Request.Length) continue;
                                bool equal = true;
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
                                responseList.Add(new CommThread.ResponseEntry(listCompare.ToArray(), listResponse.ToArray()));
                            }
                        }
                    }
                }

                bool messageShown = false;
                // split multi telegram responses
                if (conceptType == CommThread.ConceptType.ConceptIso9141)
                {
                    foreach (CommThread.ResponseEntry responseEntry in responseList)
                    {
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
                                    telLength = responseEntry.Request[3] + 5;
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
                                telLength = response[telOffset + 3] + 5;
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
            buttonConnect.Text = _commThread.ThreadRunning() ? "Disconnect" : "Connect";
            if (_commThread.ThreadRunning())
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
                if (radioButtonIso9141.Checked) conceptType = CommThread.ConceptType.ConceptIso9141;
                if (radioButtonConcept3.Checked) conceptType = CommThread.ConceptType.Concept3;

                string responseFile = (string)listBoxResponseFiles.SelectedItem;
                CommThread.ResponseType responseType = CommThread.ResponseType.Standard;
                if (responseFile != null)
                {
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
                }

                _commThread.StartThread(selectedPort, conceptType, checkBoxAdsAdapter.Checked, responseType, _configData);
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
    }
}
