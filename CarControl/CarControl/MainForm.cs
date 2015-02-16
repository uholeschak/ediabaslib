using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Windows.Forms;
using EdiabasLib;
using Ftdi;
using NetworkFunctions;
using OpenNETCF.Net.NetworkInformation;
using PowerFunctions;

namespace CarControl
{
    public partial class MainForm : Form
    {
        private delegate void DataUpdatedDelegate();
        private DataUpdatedDelegate DataUpdatedInvoke;
        private delegate void ThreadTerminatedDelegate();
        private ThreadTerminatedDelegate ThreadTerminatedInvoke;

        private const string logFileTemp = "\\Temp\\ifh.trc";
        private string ecuPath;
        private CommThread _commThread;
        private int _lastPortCount;
        private int _lastUSBCount;
        private int _logStoreIndex;
        private bool _powerOff = false;

        public MainForm()
        {
            InitializeComponent();
            // change tab control border
            WinAPI.SetWindowLong(tabControlDevice.Handle, WinAPI.GWL_EXSTYLE, WinAPI.WS_EX_WINDOWEDGE);
            WindowState = FormWindowState.Maximized;

            _lastPortCount = -1;
            _lastUSBCount = -1;
            _logStoreIndex = -1;
            UpdatePorts();
            UpdateWlan();
            UpdateLog();
            checkBoxLogFile.Checked = false;
            ecuPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "Ecu");
            if (ecuPath.StartsWith("\\Windows\\"))
            {
                ecuPath = "\\UM\\Program Files\\CarControl\\Ecu";
            }
            DataUpdatedInvoke = new DataUpdatedDelegate(DataUpdatedMethode);
            ThreadTerminatedInvoke = new ThreadTerminatedDelegate(ThreadTerminatedMethode);
            timerUpdate.Enabled = true;
            tabControlDevice.SelectedIndex = 0;
            DataUpdatedMethode();
        }

        private void MainForm_Closed(object sender, EventArgs e)
        {
            StopCommThread(true);

            if (_powerOff)
            {
                Process process = new Process();
                process.StartInfo.FileName = "\\UM\\Program Files\\CoreCon\\clientshutdown.exe";
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();

                int result = 0;
                result |= WinAPI.RegFlushKey(new IntPtr(unchecked((int)WinAPI.HKEY_CLASSES_ROOT)));
                result |= WinAPI.RegFlushKey(new IntPtr(unchecked((int)WinAPI.HKEY_CURRENT_USER)));
                result |= WinAPI.RegFlushKey(new IntPtr(unchecked((int)WinAPI.HKEY_LOCAL_MACHINE)));
                result |= WinAPI.RegFlushKey(new IntPtr(unchecked((int)WinAPI.HKEY_USERS)));
                if (result == 0)
                {
                    Process.Start("\\UM\\Program Files\\mainshell\\Shutdown.exe", "");
                }
            }
        }

        private void UpdatePorts()
        {
            Ftd2xx.FT_STATUS ftStatus;
            IntPtr deviceCount = (IntPtr)0;
            IntPtr arg2 = (IntPtr)0;
            ftStatus = Ftd2xx.FT_ListDevices(ref deviceCount, ref arg2, Ftd2xx.FT_LIST_NUMBER_ONLY);
            if (ftStatus != Ftd2xx.FT_STATUS.FT_OK)
            {
                deviceCount = (IntPtr)0;
            }
            string[] ports = SerialPort.GetPortNames();
            if (ports == null)
            {
                ports = new string[0];
            }
            if ((_lastUSBCount == (int)deviceCount) && (_lastPortCount == ports.Length)) return;

            int index = -1;
            listPorts.BeginUpdate();
            listPorts.Items.Clear();
            for (int i = 0; i < (int)deviceCount; i++)
            {
                int pos = listPorts.Items.Add("FTDI"+i.ToString());
                if (index < 0) index = pos;
            }

            foreach (string port in ports)
            {
                switch (port)
                {
                    case "COM1":
                    case "COM2":
                    case "COM3":
                        break;

                    default:
                    {
                        int pos = listPorts.Items.Add(port);
                        if (index < 0) index = pos;
                        break;
                    }
                }
            }
            listPorts.SelectedIndex = index;
            listPorts.EndUpdate();

            _lastUSBCount = (int)deviceCount;
            _lastPortCount = ports.Length;

            buttonConnect.Enabled = listPorts.SelectedIndex >= 0;
        }

        private void UpdateWlan()
        {
            pushButtonWlan.ButtonState = GetWlanEnable();
        }

        private bool GetWlanEnable()
        {
            bool wlanEnabled = false;
            try
            {
                INetworkInterface[] rgni = WirelessNetworkInterface.GetAllNetworkInterfaces();
                if (rgni != null)
                {
                    foreach (WirelessNetworkInterface netInf in rgni)
                    {
                        wlanEnabled = netInf.OperationalStatus == OperationalStatus.Up;
                        break;
                    }
                }
            }
            catch
            {
            }
            return wlanEnabled;
        }

        private void SetWlanEnable(bool enable)
        {
            try
            {
                string ifaceName = String.Empty;
                INetworkInterface[] rgni = WirelessNetworkInterface.GetAllNetworkInterfaces();
                if (rgni != null)
                {
                    foreach (WirelessNetworkInterface netInf in rgni)
                    {
                        ifaceName = netInf.Name;
                        break;
                    }
                }
                if (ifaceName.Length == 0)
                {
                    List<string> disNetInf = NetFunc.GetDisabledNetworkInterfaces();
                    if (disNetInf != null)
                    {
                        foreach (string disInfName in disNetInf)
                        {
                            ifaceName = disInfName;
                            break;
                        }
                    }
                }
                if (ifaceName.Length == 0)
                {
                    ifaceName = "MT5921SD1";
                }

                if (enable)
                {
                    NetFunc.NDISPWR.SetPowerState(ifaceName, NetFunc.DevicePowerState.Unspecified);
                    NetFunc.SetDevicePower(ifaceName, NetFunc.DevicePowerState.Unspecified);
                    NetFunc.NDIS.BindInterface(ifaceName);
                }
                else
                {
                    NetFunc.NDIS.UnbindInterface(ifaceName);
                    NetFunc.NDISPWR.SetPowerState(ifaceName, NetFunc.DevicePowerState.D4);
                    NetFunc.SetDevicePower(ifaceName, NetFunc.DevicePowerState.D4);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateBattery()
        {
            string batteryText = Resources.strings.ResourceManager.GetString("batteryStatus");
            int batteryLife;
            int batteryCurrent;
            bool acLine;
            if (PowerFunc.GetBatteryInfo(out batteryLife, out batteryCurrent, out acLine))
            {
                if (batteryLife >= 0 && !acLine)
                {
                    labelBatteryLife.Text = string.Format("{0}: {1,3}% {2,5}mA", batteryText, batteryLife, batteryCurrent);
                }
                else
                {
                    labelBatteryLife.Text = string.Format("{0}: {1,5}mA", batteryText, batteryCurrent);
                }
            }
            else
            {
                labelBatteryLife.Text = string.Format("{0}: -", batteryText);
            }
        }

        private bool StartCommThread(string selectedPort, string logFile)
        {
            try
            {
                if (_commThread == null)
                {
                    _commThread = new CommThread(ecuPath);
                    _commThread.DataUpdated += new CommThread.DataUpdatedEventHandler(DataUpdated);
                    _commThread.ThreadTerminated += new CommThread.ThreadTerminatedEventHandler(ThreadTerminated);
                }
                _commThread.StartThread(selectedPort, logFile, CommThread.SelectedDevice.DeviceAxis, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StopCommThread(bool wait)
        {
            if (_commThread != null)
            {
                try
                {
                    _commThread.StopThread(wait);
                    if (wait)
                    {
                        _commThread.DataUpdated -= DataUpdated;
                        _commThread.ThreadTerminated -= ThreadTerminated;
                        _commThread.Dispose();
                        _commThread = null;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            BeginInvoke(DataUpdatedInvoke);
        }

        private void ThreadTerminated(object sender, EventArgs e)
        {
            BeginInvoke(ThreadTerminatedInvoke);
        }

        private void ThreadTerminatedMethode()
        {
            StopCommThread(true);
            UpdateDisplay();
        }

        private void DataUpdatedMethode()
        {
            try
            {
                bool axisDataValid = false;
                bool motorDataValid = false;
                bool motorDataUnevenRunningValid = false;
                bool motorRotIrregularValid = false;
                bool motorPmValid = false;
                bool cccNavValid = false;
                bool ihkValid = false;
                bool errorsValid = false;
                bool testValid = false;
                bool buttonConnectEnable = true;

                if (_commThread != null && _commThread.ThreadRunning())
                {
                    if (_commThread.ThreadStopping())
                    {
                        buttonConnectEnable = false;
                    }
                    switch (_commThread.Device)
                    {
                        case CommThread.SelectedDevice.DeviceAxis:
                            axisDataValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceMotor:
                            motorDataValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceMotorUnevenRunning:
                            motorDataUnevenRunningValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceMotorRotIrregular:
                            motorRotIrregularValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceMotorPM:
                            motorPmValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceCccNav:
                            cccNavValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceIhk:
                            ihkValid = true;
                            break;

                        case CommThread.SelectedDevice.DeviceErrors:
                            errorsValid = true;
                            break;

                        case CommThread.SelectedDevice.Test:
                            testValid = true;
                            break;
                    }

                    buttonConnect.Text = Resources.strings.ResourceManager.GetString("buttonModeDisconnect");
                    checkBoxConnected.Checked = _commThread.Connected;
                    textBoxErrorCount.Text = _commThread.ErrorCounter.ToString();
                }
                else
                {
                    buttonConnect.Text = Resources.strings.ResourceManager.GetString("buttonModeConnect");
                    checkBoxConnected.Checked = false;
                    textBoxErrorCount.Text = "";
                }
                buttonConnect.Enabled = buttonConnectEnable;

                if (axisDataValid)
                {
                    string tempText;
                    bool found;
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    Int64 axisMode = GetResultInt64(resultDict, "MODE_CTRL_LESEN_WERT", out found);
                    tempText = string.Empty;
                    if (found)
                    {
                        if ((axisMode & CommThread.AxisModeConveyor) != 0x00)
                        {
                            tempText = Resources.strings.ResourceManager.GetString("axisModeConveyor");
                        }
                        else if ((axisMode & CommThread.AxisModeTransport) != 0x00)
                        {
                            tempText = Resources.strings.ResourceManager.GetString("axisModeTransport");
                        }
                        else if ((axisMode & CommThread.AxisModeGarage) != 0x00)
                        {
                            tempText = Resources.strings.ResourceManager.GetString("axisModeGarage");
                        }
                        else
                        {
                            tempText = Resources.strings.ResourceManager.GetString("axisModeNormal");
                        }
                    }
                    textBoxAxisMode.Text = tempText;

                    tempText = FormatResultInt64(resultDict, "ORGFASTFILTER_RL", "{0,4}");
                    if (tempText.Length > 0) tempText += " / ";
                    tempText += FormatResultInt64(resultDict, "FASTFILTER_RL", "{0,4}");
                    textBoxAxisLeft.Text = tempText;

                    tempText = FormatResultInt64(resultDict, "ORGFASTFILTER_RR", "{0,4}");
                    if (tempText.Length > 0) tempText += " / ";
                    tempText += FormatResultInt64(resultDict, "FASTFILTER_RR", "{0,4}");
                    textBoxAxisRigth.Text = tempText;

                    Int64 voltage = GetResultInt64(resultDict, "ANALOG_U_KL30", out found);
                    if (found)
                    {
                        textBoxAxisBatteryVoltage.Text = string.Format("{0,6:0.00}", (double)voltage / 1000);
                    }
                    else
                    {
                        textBoxAxisBatteryVoltage.Text = string.Empty;
                    }

                    textBoxSpeed.Text = FormatResultInt64(resultDict, "STATE_SPEED", "{0,4}");

                    string outputStates = string.Empty;
                    for (int channel = 0; channel < 4; channel++)
                    {
                        outputStates = FormatResultInt64(resultDict, string.Format("STATUS_SIGNALE_NUMERISCH{0}_WERT", channel), "{0}") + outputStates;
                    }
                    textBoxValveState.Text = outputStates;

                    Int64 speed = GetResultInt64(resultDict, "STATE_SPEED", out found);
                    if (!found) speed = 0;

                    if (speed < 5)
                    {
                        pushButtonDown.Enabled = true;
                    }
                    else
                    {
                        if (_commThread.AxisOpMode == CommThread.OperationMode.OpModeDown)
                        {
                            _commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
                        }
                        pushButtonDown.ButtonState = false;
                        pushButtonDown.Enabled = false;
                    }
                    pushButtonUp.Enabled = true;
                }
                else
                {
                    textBoxAxisMode.Text = string.Empty;
                    textBoxAxisLeft.Text = string.Empty;
                    textBoxAxisRigth.Text = string.Empty;
                    textBoxValveState.Text = string.Empty;
                    textBoxAxisBatteryVoltage.Text = string.Empty;
                    textBoxSpeed.Text = string.Empty;
                    pushButtonDown.ButtonState = false;
                    pushButtonDown.Enabled = false;
                    pushButtonUp.ButtonState = false;
                    pushButtonUp.Enabled = false;
                    if (_commThread != null) _commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
                }

                if (motorDataValid)
                {
                    bool found;
                    string dataText;
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    textBoxMotorBatteryVoltage.Text = FormatResultDouble(resultDict, "STAT_UBATT_WERT", "{0,7:0.00}");
                    textBoxMotorTemp.Text = FormatResultDouble(resultDict, "STAT_CTSCD_tClntLin_WERT", "{0,6:0.0}");
                    textBoxMotorAirMass.Text = FormatResultDouble(resultDict, "STAT_LUFTMASSE_WERT", "{0,7:0.00}");
                    textBoxMotorIntakeAirTemp.Text = FormatResultDouble(resultDict, "STAT_LADELUFTTEMPERATUR_WERT", "{0,6:0.0}");
                    textBoxMotorAmbientTemp.Text = FormatResultDouble(resultDict, "STAT_UMGEBUNGSTEMPERATUR_WERT", "{0,6:0.0}");
                    textBoxMotorBoostPressSet.Text = FormatResultDouble(resultDict, "STAT_LADEDRUCK_SOLL_WERT", "{0,6:0.0}");
                    textBoxMotorBoostPressAct.Text = FormatResultDouble(resultDict, "STAT_LADEDRUCK_WERT", "{0,6:0.0}");
                    textBoxMotorRailPressSet.Text = FormatResultDouble(resultDict, "STAT_RAILDRUCK_SOLL_WERT", "{0,6:0.0}");
                    textBoxMotorRailPressAct.Text = FormatResultDouble(resultDict, "STAT_RAILDRUCK_WERT", "{0,6:0.0}");
                    textBoxMotorAirMassSet.Text = FormatResultDouble(resultDict, "STAT_LUFTMASSE_SOLL_WERT", "{0,6:0.0}");
                    textBoxMotorAirMassAct.Text = FormatResultDouble(resultDict, "STAT_LUFTMASSE_PRO_HUB_WERT", "{0,6:0.0}");
                    textBoxMotorAmbientPress.Text = FormatResultDouble(resultDict, "STAT_UMGEBUNGSDRUCK_WERT", "{0,6:0.0}");
                    textBoxMotorFuelTemp.Text = FormatResultDouble(resultDict, "STAT_KRAFTSTOFFTEMPERATURK_WERT", "{0,6:0.0}");
                    textBoxMotorTempBeforeFilter.Text = FormatResultDouble(resultDict, "STAT_ABGASTEMPERATUR_VOR_PARTIKELFILTER_1_WERT", "{0,6:0.0}");
                    textBoxMotorTempBeforeCat.Text = FormatResultDouble(resultDict, "STAT_ABGASTEMPERATUR_VOR_KATALYSATOR_WERT", "{0,6:0.0}");

                    dataText = string.Format("{0,6:0.0}", GetResultDouble(resultDict, "STAT_STRECKE_SEIT_ERFOLGREICHER_REGENERATION_WERT", out found) / 1000.0);
                    if (!found) dataText = string.Empty;
                    textBoxMotorPartFilterDistSinceRegen.Text = dataText;

                    textBoxMotorExhaustBackPressure.Text = FormatResultDouble(resultDict, "STAT_DIFFERENZDRUCK_UEBER_PARTIKELFILTER_WERT", "{0,6:0.0}");
                    checkBoxOilPressSwt.Checked = (GetResultDouble(resultDict, "STAT_OELDRUCKSCHALTER_EIN_WERT", out found) > 0.5) && found;
                    checkBoxMotorPartFilterRequest.Checked = (GetResultDouble(resultDict, "STAT_REGENERATIONSANFORDERUNG_WERT", out found) < 0.5) && found;
                    checkBoxMotorPartFilterStatus.Checked = (GetResultDouble(resultDict, "STAT_EGT_st_WERT", out found) > 1.5) && found;
                    checkBoxMotorPartFilterUnblock.Checked = (GetResultDouble(resultDict, "STAT_REGENERATION_BLOCKIERUNG_UND_FREIGABE_WERT", out found) < 0.5) && found;
                }
                else
                {
                    textBoxMotorBatteryVoltage.Text = string.Empty;
                    textBoxMotorTemp.Text = string.Empty;
                    textBoxMotorAirMass.Text = string.Empty;
                    textBoxMotorIntakeAirTemp.Text = string.Empty;
                    textBoxMotorAmbientTemp.Text = string.Empty;
                    textBoxMotorBoostPressSet.Text = string.Empty;
                    textBoxMotorBoostPressAct.Text = string.Empty;
                    textBoxMotorRailPressSet.Text = string.Empty;
                    textBoxMotorRailPressAct.Text = string.Empty;
                    textBoxMotorAirMassSet.Text = string.Empty;
                    textBoxMotorAirMassAct.Text = string.Empty;
                    textBoxMotorAmbientPress.Text = string.Empty;
                    textBoxMotorFuelTemp.Text = string.Empty;
                    textBoxMotorTempBeforeFilter.Text = string.Empty;
                    textBoxMotorTempBeforeCat.Text = string.Empty;
                    textBoxMotorPartFilterDistSinceRegen.Text = string.Empty;
                    textBoxMotorExhaustBackPressure.Text = string.Empty;
                    checkBoxOilPressSwt.Checked = false;
                    checkBoxMotorPartFilterRequest.Checked = false;
                    checkBoxMotorPartFilterStatus.Checked = false;
                    checkBoxMotorPartFilterUnblock.Checked = false;
                }

                if (motorDataUnevenRunningValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    textBoxMotorQuantCorrCylinder1.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL1_WERT", "{0,5:0.00}");
                    textBoxMotorQuantCorrCylinder2.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL2_WERT", "{0,5:0.00}");
                    textBoxMotorQuantCorrCylinder3.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL3_WERT", "{0,5:0.00}");
                    textBoxMotorQuantCorrCylinder4.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL4_WERT", "{0,5:0.00}");
                }
                else
                {
                    textBoxMotorQuantCorrCylinder1.Text = string.Empty;
                    textBoxMotorQuantCorrCylinder2.Text = string.Empty;
                    textBoxMotorQuantCorrCylinder3.Text = string.Empty;
                    textBoxMotorQuantCorrCylinder4.Text = string.Empty;
                }

                if (motorRotIrregularValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    textBoxMotorRpmCylinder1.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL1_WERT", "{0,7:0.0}");
                    textBoxMotorRpmCylinder2.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL2_WERT", "{0,7:0.0}");
                    textBoxMotorRpmCylinder3.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL3_WERT", "{0,7:0.0}");
                    textBoxMotorRpmCylinder4.Text = FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL4_WERT", "{0,7:0.0}");
                }
                else
                {
                    textBoxMotorRpmCylinder1.Text = string.Empty;
                    textBoxMotorRpmCylinder2.Text = string.Empty;
                    textBoxMotorRpmCylinder3.Text = string.Empty;
                    textBoxMotorRpmCylinder4.Text = string.Empty;
                }

                if (motorPmValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    textBoxPmBatCap.Text = FormatResultDouble(resultDict, "STAT_BATTERIE_KAPAZITAET_WERT", "{0,3:0}");
                    textBoxPmSoh.Text = FormatResultDouble(resultDict, "STAT_SOH_WERT", "{0,5:0.0}");
                    textBoxPmSocFit.Text = FormatResultDouble(resultDict, "STAT_SOC_FIT_WERT", "{0,5:0.0}");
                    textBoxPmSeasonTemp.Text = FormatResultDouble(resultDict, "STAT_TEMP_SAISON_WERT", "{0,5:0.0}");
                    textBoxPmCalEvents.Text = FormatResultDouble(resultDict, "STAT_KALIBRIER_EVENT_CNT_WERT", "{0,3:0}");

                    textBoxPmSocQ.Text = FormatResultDouble(resultDict, "STAT_Q_SOC_AKTUELL_WERT", "{0,6:0.0}");
                    textBoxPmSocQD1.Text = FormatResultDouble(resultDict, "STAT_Q_SOC_VOR_1_TAG_WERT", "{0,6:0.0}");

                    textBoxPmStartCap.Text = FormatResultDouble(resultDict, "STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT", "{0,5:0.0}");
                    textBoxPmStartCapD1.Text = FormatResultDouble(resultDict, "STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT", "{0,5:0.0}");

                    textBoxPmSocPercent.Text = FormatResultDouble(resultDict, "STAT_LADUNGSZUSTAND_AKTUELL_WERT", "{0,5:0.0}");
                    textBoxPmSocPercentD1.Text = FormatResultDouble(resultDict, "STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT", "{0,5:0.0}");
                }
                else
                {
                    textBoxPmBatCap.Text = string.Empty;
                    textBoxPmSoh.Text = string.Empty;
                    textBoxPmSocFit.Text = string.Empty;
                    textBoxPmSeasonTemp.Text = string.Empty;
                    textBoxPmCalEvents.Text = string.Empty;
                    textBoxPmSocQ.Text = string.Empty;
                    textBoxPmSocQD1.Text = string.Empty;
                    textBoxPmStartCap.Text = string.Empty; ;
                    textBoxPmStartCapD1.Text = string.Empty;
                    textBoxPmSocPercent.Text = string.Empty;
                    textBoxPmSocPercentD1.Text = string.Empty;
                }

                if (cccNavValid)
                {
                    bool found;
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    textBoxCccNavPosLat.Text = FormatResultString(resultDict, "STAT_GPS_POSITION_BREITE", "{0}");
                    textBoxCccNavPosLong.Text = FormatResultString(resultDict, "STAT_GPS_POSITION_LAENGE", "{0}");
                    textBoxCccNavPosHeight.Text = FormatResultString(resultDict, "STAT_GPS_POSITION_HOEHE", "{0}");
                    textBoxCccNavGpsDateTime.Text = FormatResultString(resultDict, "STAT_TIME_DATE_VAL", "{0}").Replace(".*6*", ".201");    // fix for invalid response
                    textBoxCccNavPosType.Text = FormatResultString(resultDict, "STAT_GPS_TEXT", "{0}");
                    textBoxCccNavSpeed.Text = FormatResultString(resultDict, "STAT_SPEED_VAL", "{0}");
                    textBoxCccNavResHorz.Text = FormatResultString(resultDict, "STAT_HORIZONTALE_AUFLOES", "{0}");
                    textBoxCccNavResVert.Text = FormatResultString(resultDict, "STAT_VERTICALE_AUFLOES", "{0}");
                    textBoxCccNavResPos.Text = FormatResultString(resultDict, "STAT_POSITION_AUFLOES", "{0}");
                    checkBoxCccNavAlmanach.Checked = (GetResultInt64(resultDict, "STAT_ALMANACH", out found) > 0.5) && found;
                    checkBoxHipDriver.Checked = (GetResultInt64(resultDict, "STAT_HIP_DRIVER", out found) < 0.5) && found;
                }
                else
                {
                    textBoxCccNavPosLat.Text = string.Empty;
                    textBoxCccNavPosLong.Text = string.Empty;
                    textBoxCccNavPosHeight.Text = string.Empty;
                    textBoxCccNavPosType.Text = string.Empty;
                    textBoxCccNavSpeed.Text = string.Empty;
                    textBoxCccNavGpsDateTime.Text = string.Empty;
                    textBoxCccNavResHorz.Text = string.Empty;
                    textBoxCccNavResVert.Text = string.Empty;
                    textBoxCccNavResPos.Text = string.Empty;
                    checkBoxCccNavAlmanach.Checked = false;
                    checkBoxHipDriver.Checked = false;
                }

                if (ihkValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                    lock (CommThread.DataLock)
                    {
                        resultDict = _commThread.EdiabasResultDict;
                    }
                    textBoxIhkInTemp.Text = FormatResultDouble(resultDict, "STAT_TINNEN_WERT", "{0,6:0.0}");
                    textBoxIhkInTempDelay.Text = FormatResultDouble(resultDict, "STAT_TINNEN_VERZOEGERT_WERT", "{0,6:0.0}");
                    textBoxIhkOutTemp.Text = FormatResultDouble(resultDict, "STAT_TAUSSEN_WERT", "{0,6:0.0}");
                    textBoxIhkSetpoint.Text = FormatResultDouble(resultDict, "STAT_SOLL_LI_KORRIGIERT_WERT", "{0,6:0.0}");
                    textBoxIhkHeatExTemp.Text = FormatResultDouble(resultDict, "STAT_WT_RE_WERT", "{0,6:0.0}");
                    textBoxIhkHeatExSetpoint.Text = FormatResultDouble(resultDict, "STAT_WTSOLL_RE_WERT", "{0,6:0.0}");
                }
                else
                {
                    textBoxIhkInTemp.Text = string.Empty;
                    textBoxIhkInTempDelay .Text = string.Empty;
                    textBoxIhkOutTemp.Text = string.Empty;
                    textBoxIhkSetpoint.Text = string.Empty;
                    textBoxIhkHeatExTemp.Text = string.Empty;
                    textBoxIhkHeatExSetpoint.Text = string.Empty;
                }

                if (errorsValid)
                {
                    List<CommThread.EdiabasErrorReport> errorReportList = null;
                    lock (CommThread.DataLock)
                    {
                        errorReportList = _commThread.EdiabasErrorReportList;
                    }

                    string message1 = string.Empty;
                    string message2 = string.Empty;
                    bool columnRight = false;

                    if (errorReportList != null)
                    {
                        foreach (CommThread.EdiabasErrorReport errorReport in errorReportList)
                        {
                            string message = string.Format("{0}: ",
                                Resources.strings.ResourceManager.GetString(errorReport.DeviceName));
                            if (errorReport.ErrorDict == null)
                            {
                                message += Resources.strings.ResourceManager.GetString("errorNoResponse");
                            }
                            else
                            {
                                message += "\r\n";
                                message += FormatResultString(errorReport.ErrorDict, "F_ORT_TEXT", "{0}");
                                message += ", ";
                                message += FormatResultString(errorReport.ErrorDict, "F_VORHANDEN_TEXT", "{0}");
                                string detailText = string.Empty;
                                foreach (Dictionary<string, EdiabasNet.ResultData> errorDetail in errorReport.ErrorDetailSet)
                                {
                                    string kmText = FormatResultInt64(errorDetail, "F_UW_KM", "{0}");
                                    if (kmText.Length > 0)
                                    {
                                        if (detailText.Length > 0)
                                        {
                                            detailText += ", ";
                                        }
                                        detailText += kmText + "km";
                                    }
                                }
                                if (detailText.Length > 0)
                                {
                                    message += "\r\n" + detailText;
                                }
                            }

                            if (message.Length > 0)
                            {
                                message += "\r\n";

                                if (!columnRight)
                                {
                                    message1 += message;
                                }
                                else
                                {
                                    message2 += message;
                                }
                                columnRight = !columnRight;
                            }
                        }
                        if ((message1.Length == 0) && (message2.Length == 0))
                        {
                            message1 = Resources.strings.ResourceManager.GetString("errorNoError");
                        }
                    }
                    textBoxErrors1.Text = message1;
                    textBoxErrors2.Text = message2;
                }
                else
                {
                    textBoxErrors1.Text = string.Empty;
                    textBoxErrors2.Text = string.Empty;
                }

                if (testValid)
                {
                    lock (CommThread.DataLock)
                    {
                        textBoxTest.Text = _commThread.TestResult;
                    }
                }
                else
                {
                    textBoxTest.Text = string.Empty;
                }
            }
            catch (Exception)
            {
            }
        }

        private String FormatResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            double value = GetResultDouble(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(format, value);
            }
            return string.Empty;
        }

        private String FormatResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            Int64 value = GetResultInt64(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(format, value);
            }
            return string.Empty;
        }

        private String FormatResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            string value = GetResultString(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(format, value);
            }
            return string.Empty;
        }

        private Int64 GetResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData.GetType() == typeof(Int64))
                {
                    found = true;
                    return (Int64)resultData.opData;
                }
            }
            return 0;
        }

        private Double GetResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData.GetType() == typeof(Double))
                {
                    found = true;
                    return (Double)resultData.opData;
                }
            }
            return 0;
        }

        private String GetResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData.GetType() == typeof(String))
                {
                    found = true;
                    return (String)resultData.opData;
                }
            }
            return string.Empty;
        }

        private string GetErrorDescription(int deviceIndex, int errorCode, int errorType)
        {
            string result = string.Empty;
            string errorClass = string.Empty;
            string errorPresent = string.Empty;
            string errorWarning = string.Empty;

            switch (errorType & 0x0F)
            {
                case 0x00:
                    errorClass = Resources.strings.ResourceManager.GetString("errorClassNoSymptom");
                    break;

                case 0x01:
                    errorClass = Resources.strings.ResourceManager.GetString("errorClassOverThreshold");
                    break;

                case 0x02:
                    errorClass = Resources.strings.ResourceManager.GetString("errorClassBelowThreshold");
                    break;

                case 0x04:
                    errorClass = Resources.strings.ResourceManager.GetString("errorClassNoValue");
                    break;

                case 0x08:
                    errorClass = Resources.strings.ResourceManager.GetString("errorClassInvalidValue");
                    break;

                default:
                    errorClass = Resources.strings.ResourceManager.GetString("errorClassUnknown");
                    break;
            }

            switch (errorType & 0x60)
            {
                case 0x00:
                    errorPresent = Resources.strings.ResourceManager.GetString("errorPresentNever");
                    break;

                case 0x20:
                    errorPresent = Resources.strings.ResourceManager.GetString("errorPresentNotNow");
                    break;

                default:
                    errorPresent = Resources.strings.ResourceManager.GetString("errorPresentNow");
                    break;
            }
            if ((errorType & 0x80) != 0x00)
            {
                errorWarning = Resources.strings.ResourceManager.GetString("errorWarningActive");
            }
            result = "\r\n- " + errorClass + ", " + errorPresent;
            if (errorWarning.Length > 0) result += ", " + errorWarning;

            return result;
        }

        private void UpdateDisplay()
        {
            DataUpdatedMethode();
        }

        private void UpdateSelectedDevice()
        {
            if ((_commThread == null) || !_commThread.ThreadRunning())
            {
                return;
            }

            switch (tabControlDevice.SelectedIndex)
            {
                case 0:
                default:
                    _commThread.Device = CommThread.SelectedDevice.DeviceAxis;
                    break;

                case 1:
                    _commThread.Device = CommThread.SelectedDevice.DeviceMotor;
                    break;

                case 2:
                    _commThread.Device = CommThread.SelectedDevice.DeviceMotorUnevenRunning;
                    break;

                case 3:
                    _commThread.Device = CommThread.SelectedDevice.DeviceMotorRotIrregular;
                    break;

                case 4:
                    _commThread.Device = CommThread.SelectedDevice.DeviceMotorPM;
                    break;

                case 5:
                    _commThread.Device = CommThread.SelectedDevice.DeviceCccNav;
                    break;

                case 6:
                    _commThread.Device = CommThread.SelectedDevice.DeviceIhk;
                    break;

                case 7:
                    _commThread.Device = CommThread.SelectedDevice.DeviceErrors;
                    break;

                case 8:
                    _commThread.Device = CommThread.SelectedDevice.Test;
                    break;
            }
        }

        private void UpdateLog()
        {
            bool logExists = File.Exists(logFileTemp);
            bool threadRunning = _commThread != null && _commThread.ThreadRunning();
            buttonStoreLog.Enabled = logExists && !threadRunning && checkBoxLogFile.Checked;
            buttonStoreLog.Text = Resources.strings.ResourceManager.GetString("storeLog")
                + string.Format(": {0}", _logStoreIndex);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (_commThread != null && _commThread.ThreadRunning())
            {
                StopCommThread(false);
            }
            else
            {
                if (listPorts.SelectedIndex < 0) return;
                string selectedPort = listPorts.SelectedItem.ToString();

                string logFile = null;
                if (checkBoxLogFile.Checked)
                {
                    logFile = logFileTemp;
                }
                StartCommThread(selectedPort, logFile);
                UpdateSelectedDevice();
            }
            UpdateDisplay();
            UpdateLog();
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if ((_commThread == null) || !_commThread.ThreadRunning())
            {
                UpdatePorts();
            }
            UpdateWlan();
            UpdateBattery();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pushButtonDown_Click(object sender, EventArgs e)
        {
            if (_commThread == null)
            {
                return;
            }
            if (pushButtonDown.ButtonState)
            {
                if (_commThread != null) _commThread.AxisOpMode = CommThread.OperationMode.OpModeDown;
                pushButtonUp.ButtonState = false;
            }
            else
            {
                _commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }
        }

        private void pushButtonUp_Click(object sender, EventArgs e)
        {
            if (_commThread == null)
            {
                return;
            }
            if (pushButtonUp.ButtonState)
            {
                _commThread.AxisOpMode = CommThread.OperationMode.OpModeUp;
                pushButtonDown.ButtonState = false;
            }
            else
            {
                _commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }
        }

        private void buttonStoreLog_Click(object sender, EventArgs e)
        {
            if (!File.Exists(logFileTemp)) return;

            string storePath = "\\NORFlash";
            if (!Directory.Exists(storePath))
            {
                storePath = "\\Journe Touch\\Download";
                if (!Directory.Exists(storePath))
                {
                    return;
                }
            }
            for (int i = 0; i < 1000; i++)
            {
                string logFileStore = string.Format(storePath + "\\CarControl{0}.txt", i);
                if (File.Exists(logFileStore)) continue;
                try
                {
                    File.Copy(logFileTemp, logFileStore, true);
                    File.Delete(logFileTemp);
                    _logStoreIndex = i;
                }
                catch (Exception)
                {
                }
                break;
            }
            UpdateLog();
        }

        private void tabControlDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedDevice();
            UpdateDisplay();
        }

        private void buttonPowerOff_Click(object sender, EventArgs e)
        {
            _powerOff = true;
            Close();
        }

        private void pushButtonWlan_Click(object sender, EventArgs e)
        {
            SetWlanEnable(!GetWlanEnable());
            UpdateWlan();
        }
    }
}
