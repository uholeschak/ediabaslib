using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO.Ports;
using System.IO;

namespace CarSimulator
{
    public partial class MainForm : Form
    {
        private const string _stdResponseFile = "Response.txt";
        private CommThread _commThread;
        private int _lastPortCount;
        private CommThread.ConfigData _configData;

        public MainForm()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            InitializeComponent();

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
            string[] ports = SerialPort.GetPortNames();
            if (ports == null)
            {
                ports = new string[0];
            }
            if (_lastPortCount == ports.Length) return;
            listPorts.BeginUpdate();
            listPorts.Items.Clear();
            int index = -1;
            foreach (string port in ports)
            {
                index = listPorts.Items.Add(port);
            }
            listPorts.SelectedIndex = index;
            listPorts.EndUpdate();

            buttonConnect.Enabled = listPorts.SelectedIndex >= 0;
            _lastPortCount = ports.Length;
        }

        private void UpdateResponseFiles()
        {
            string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] files = Directory.GetFiles(appDir, "*.txt");
            listBoxResponseFiles.BeginUpdate();
            listBoxResponseFiles.Items.Clear();
            string selectItem = null;
            foreach (string file in files)
            {
                string baseFileName = Path.GetFileName(file);
                if (string.Compare(baseFileName, _stdResponseFile, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    selectItem = baseFileName;
                }
                listBoxResponseFiles.Items.Add(baseFileName);
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
            List<CommThread.ResponseEntry> responseList = _configData.ResponseList;
            try
            {
                configList.Clear();
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
                            numberArray = line.Split(new char[] { ' ' });
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
                                    }
                                }
                            }
                            continue;
                        }

                        numberArray = line.Split(new char[] { ' ' });
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
                                }
                            }
                        }
                        if (listCompare.Count > 0 && listResponse.Count > 0)
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
                                    if (responseEntry.Response.Length < listResponse.Count)
                                    {
                                        responseList.Remove(responseEntry);
                                    }
                                    else
                                    {
                                        addEntry = false;
                                    }
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
                if (conceptType == CommThread.ConceptType.conceptIso9141)
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

                        int telOffset = 0;
                        while ((telOffset + 1) < responseEntry.Response.Length)
                        {
                            if ((responseEntry.Response.Length - telOffset) < 3)
                            {
                                break;
                            }
                            int telLength = responseEntry.Response[telOffset + 0];
                            if (telLength < 3)
                            {
                                if (!messageShown)
                                {
                                    messageShown = true;
                                    MessageBox.Show("Invalid response file response!");
                                }
                                break;
                            }
                            if (telOffset + telLength > responseEntry.Response.Length)
                            {
                                break;
                            }
                            byte[] responseTel = new byte[telLength];
                            Array.Copy(responseEntry.Response, telOffset, responseTel, 0, telLength);
                            responseEntry.ResponseList.Add(responseTel);
                            telOffset += telLength;
                        }
                        if (telOffset != responseEntry.Response.Length)
                        {
                            if (!messageShown)
                            {
                                messageShown = true;
                                MessageBox.Show("Invalid response file response!");
                            }
                        }
                    }
                }
                else if ((conceptType != CommThread.ConceptType.concept1) && (conceptType != CommThread.ConceptType.concept3))
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

                        int telOffset = 0;
                        while ((telOffset + 1) < responseEntry.Response.Length)
                        {
                            if ((responseEntry.Response.Length - telOffset) < 4)
                            {
                                break;
                            }
                            int telLength = responseEntry.Response[telOffset + 0] & 0x3F;
                            if (telLength == 0)
                            {   // with length byte
                                telLength = responseEntry.Response[telOffset + 3] + 5;
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
                            if (telOffset + telLength > responseEntry.Response.Length)
                            {
                                break;
                            }
                            byte[] responseTel = new byte[telLength];
                            Array.Copy(responseEntry.Response, telOffset, responseTel, 0, telLength);
                            responseEntry.ResponseList.Add(responseTel);
                            telOffset += telLength;
                        }
                        if (telOffset != responseEntry.Response.Length)
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
            if (_commThread.ThreadRunning())
            {
                buttonConnect.Text = "Disconnect";
            }
            else
            {
                buttonConnect.Text = "Connect";
            }
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

                CommThread.ConceptType conceptType = CommThread.ConceptType.conceptBwmFast;
                if (radioButtonKwp2000S.Checked) conceptType = CommThread.ConceptType.conceptKwp2000S;
                if (radioButtonDs2.Checked) conceptType = CommThread.ConceptType.conceptDs2;
                if (radioButtonConcept1.Checked) conceptType = CommThread.ConceptType.concept1;
                if (radioButtonIso9141.Checked) conceptType = CommThread.ConceptType.conceptIso9141;
                if (radioButtonConcept3.Checked) conceptType = CommThread.ConceptType.concept3;

                string appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string responseFile = (string)listBoxResponseFiles.SelectedItem;
                bool e61Internal = true;
                if (responseFile != null)
                {
                    if (string.Compare(responseFile, _stdResponseFile, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        e61Internal = false;
                    }

                    if (!ReadResponseFile(Path.Combine(appDir, responseFile), conceptType))
                    {
                        MessageBox.Show("Reading response file failed!");
                    }
                }

                _commThread.StartThread(selectedPort, conceptType, checkBoxAdsAdapter.Checked, e61Internal, _configData);
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
