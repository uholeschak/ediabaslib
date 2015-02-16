using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using EdiabasLib;
using CarControl;

namespace CarControlAndroid
{
    [Activity (Label = "@string/app_name", Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true,
               ConfigurationChanges=Android.Content.PM.ConfigChanges.KeyboardHidden | Android.Content.PM.ConfigChanges.Orientation)]
    public class ActivityMain : TabActivity
    {
        enum activityRequest
        {
            REQUEST_SELECT_DEVICE,
            REQUEST_ENABLE_BT
        }

        private string deviceAddress = string.Empty;
        private string sharedAppName = "CarControl";
        private string ecuPath;
        private BluetoothAdapter bluetoothAdapter = null;
        private CommThread commThread;
        private ToggleButton buttonConnect;
        private ToggleButton buttonAxisDown;
        private ToggleButton buttonAxisUp;
        private ToggleButton buttonUnevenRunningActive;
        private ToggleButton buttonRotIrregularActive;
        private TextView textViewResultAxis;
        private TextView textViewResultMotor;
        private TextView textViewResultMotorUnevenRunning;
        private TextView textViewResultMotorRotIrregular;
        private TextView textViewResultMotorPm;
        private TextView textViewResultCccNav;
        private TextView textViewResultIhk;
        private TextView textViewResultErrors;
        private TextView textViewResultTest;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // force linking of I18N.DLL
            var ignore = new I18N.West.CP437();

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.main);
            TabHost.TabWidget.SetDividerDrawable (Resource.Drawable.tab_divider);
            CreateTab("axis", GetString (Resource.String.tab_axis), Resource.Id.tabAxis);
            CreateTab("motor", GetString (Resource.String.tab_motor), Resource.Id.tabMotor);
            CreateTab("motor_uneven_running", GetString (Resource.String.tab_motor_uneven_running), Resource.Id.tabMotorUnevenRunning);
            CreateTab("motor_rot_irregular", GetString (Resource.String.tab_motor_rot_irregular), Resource.Id.tabMotorRotIrregular);
            CreateTab("motor_pm", GetString (Resource.String.tab_motor_pm), Resource.Id.tabMotorPm);
            CreateTab("ccc_nav", GetString (Resource.String.tab_ccc_nav), Resource.Id.tabCccNav);
            CreateTab("ihk", GetString (Resource.String.tab_ihk), Resource.Id.tabIhk);
            CreateTab("errors", GetString (Resource.String.tab_errors), Resource.Id.tabErrors);
            CreateTab("test", GetString (Resource.String.tab_test), Resource.Id.tabTest);

            buttonConnect = FindViewById<ToggleButton> (Resource.Id.buttonConnect);
            buttonAxisUp = FindViewById<ToggleButton> (Resource.Id.button_axis_up);
            buttonAxisDown = FindViewById<ToggleButton> (Resource.Id.button_axis_down);
            buttonUnevenRunningActive = FindViewById<ToggleButton> (Resource.Id.button_uneven_running_active);
            buttonRotIrregularActive = FindViewById<ToggleButton> (Resource.Id.button_rot_irregular_active);
            textViewResultAxis = FindViewById<TextView> (Resource.Id.textViewResultAxis);
            textViewResultMotor = FindViewById<TextView> (Resource.Id.textViewResultMotor);
            textViewResultMotorUnevenRunning = FindViewById<TextView> (Resource.Id.textViewResultMotorUnevenRunning);
            textViewResultMotorRotIrregular = FindViewById<TextView> (Resource.Id.textViewResultMotorRotIrregular);
            textViewResultMotorPm = FindViewById<TextView> (Resource.Id.textViewResultMotorPm);
            textViewResultCccNav = FindViewById<TextView> (Resource.Id.textViewResultCccNav);
            textViewResultIhk = FindViewById<TextView> (Resource.Id.textViewResultIhk);
            textViewResultErrors = FindViewById<TextView> (Resource.Id.textViewResultErrors);
            textViewResultTest = FindViewById<TextView> (Resource.Id.textViewResultTest);

            // Get local Bluetooth adapter
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (bluetoothAdapter == null)
            {
                Toast.MakeText (this, Resource.String.bt_not_available, ToastLength.Long).Show ();
                Finish ();
                return;
            }

            GetSettings ();

            ecuPath = Path.Combine (
                System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "Ecu");
            // copy asset files
            CopyAssets (ecuPath);

            // Get our button from the layout resource,
            // and attach an event to it
            TabHost.TabChanged += HandleTabChanged;
            buttonConnect.Click += ButtonConnectClick;
            buttonAxisDown.Click += ButtonAxisDownClick;
            buttonAxisUp.Click += ButtonAxisUpClick;
            buttonUnevenRunningActive.Click += ButtonUnevenRunningClick;
            buttonRotIrregularActive.Click += ButtonRotIrregularClick;

            UpdateDisplay ();
        }

        private void CreateTab(string tag, string label, int viewId)
        {
            TabHost.TabSpec spec = TabHost.NewTabSpec(tag);
            spec.SetIndicator(CreateTabView(label));
            spec.SetContent(viewId);
            TabHost.AddTab(spec);
        }

        private View CreateTabView(string label) 
        {
            var tabView = LayoutInflater.Inflate (Resource.Layout.tabs_bg, null);
            var textView = tabView.FindViewById<TextView> (Resource.Id.tabsText);
            textView.Text = label;
            return tabView;
        }

        protected override void OnStart ()
        {
            base.OnStart ();

            // If BT is not on, request that it be enabled.
            // setupChat() will then be called during onActivityResult
            if (!bluetoothAdapter.IsEnabled)
            {
                Intent enableIntent = new Intent (BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult (enableIntent, (int)activityRequest.REQUEST_ENABLE_BT);
            }
        }

        protected override void OnStop ()
        {
            base.OnStop ();

            StopCommThread (false);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy ();

            StopCommThread (true);

            StoreSettings ();
        }

        protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
        {
            switch((activityRequest)requestCode)
            {
            case activityRequest.REQUEST_SELECT_DEVICE:
                // When DeviceListActivity returns with a device to connect
                if (resultCode == Result.Ok)
                {
                    // Get the device MAC address
                    deviceAddress = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                }
                break;

            case activityRequest.REQUEST_ENABLE_BT:
                // When the request to enable Bluetooth returns
                if (resultCode != Result.Ok)
                {
                    // User did not enable Bluetooth or an error occured
                    Toast.MakeText(this, Resource.String.bt_not_enabled_leaving, ToastLength.Short).Show();
                    Finish();
                }
                break;
            }
        }

        public override bool OnCreateOptionsMenu (IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu (IMenu menu)
        {
            bool commActive = commThread != null && commThread.ThreadRunning ();
            IMenuItem scanMenu = menu.FindItem (Resource.Id.scan);
            if (scanMenu != null)
            {
                scanMenu.SetEnabled (!commActive);
            }
            return base.OnPrepareOptionsMenu (menu);
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            switch (item.ItemId) 
            {
                case Resource.Id.scan:
                // Launch the DeviceListActivity to see devices and do scan
                Intent serverIntent = new Intent(this, typeof(DeviceListActivity));
                StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_SELECT_DEVICE);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected void HandleTabChanged (object sender, TabHost.TabChangeEventArgs e)
        {
            UpdateSelectedDevice ();
            UpdateDisplay ();
        }

        protected void ButtonConnectClick (object sender, EventArgs e)
        {
            if (commThread != null && commThread.ThreadRunning())
            {
                StopCommThread (false);
            }
            else
            {
                StartCommThread ();
                UpdateSelectedDevice();
            }
            UpdateDisplay();
        }

        protected void ButtonAxisDownClick (object sender, EventArgs e)
        {
            if (commThread == null)
            {
                return;
            }
            if (buttonAxisDown.Checked)
            {
                commThread.AxisOpMode = CommThread.OperationMode.OpModeDown;
                buttonAxisUp.Checked = false;
            }
            else
            {
                if (commThread != null) commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }
        }

        protected void ButtonAxisUpClick (object sender, EventArgs e)
        {
            if (commThread == null)
            {
                return;
            }
            if (buttonAxisUp.Checked)
            {
                commThread.AxisOpMode = CommThread.OperationMode.OpModeUp;
                buttonAxisDown.Checked = false;
            }
            else
            {
                commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }
        }

        protected void ButtonUnevenRunningClick (object sender, EventArgs e)
        {
            if (commThread == null)
            {
                return;
            }
            commThread.CommActive = buttonUnevenRunningActive.Checked;
        }

        protected void ButtonRotIrregularClick (object sender, EventArgs e)
        {
            if (commThread == null)
            {
                return;
            }
            commThread.CommActive = buttonRotIrregularActive.Checked;
        }

        private bool StartCommThread()
        {
            try
            {
                if (commThread == null)
                {
                    commThread = new CommThread(ecuPath);
                    commThread.DataUpdated += new CommThread.DataUpdatedEventHandler(DataUpdated);
                    commThread.ThreadTerminated += new CommThread.ThreadTerminatedEventHandler(ThreadTerminated);
                }
                commThread.StartThread("BLUETOOTH:" + deviceAddress, null, CommThread.SelectedDevice.DeviceAxis, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StopCommThread(bool wait)
        {
            if (commThread != null)
            {
                try
                {
                    commThread.StopThread(wait);
                    if (wait)
                    {
                        commThread.DataUpdated -= DataUpdated;
                        commThread.ThreadTerminated -= ThreadTerminated;
                        commThread.Dispose();
                        commThread = null;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private bool GetSettings()
        {
            try
            {
                ISharedPreferences prefs = Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                deviceAddress = prefs.GetString("DeviceAddress", "98:D3:31:40:13:56");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool StoreSettings()
        {
            try
            {
                ISharedPreferences prefs = Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.PutString("DeviceAddress", deviceAddress);
                prefsEdit.Commit();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            RunOnUiThread(DataUpdatedMethode);
        }

        private void ThreadTerminated(object sender, EventArgs e)
        {
            RunOnUiThread(ThreadTerminatedMethode);
        }

        private void ThreadTerminatedMethode()
        {
            StopCommThread(true);
            UpdateDisplay();
        }

        private void UpdateSelectedDevice()
        {
            if ((commThread == null) || !commThread.ThreadRunning())
            {
                return;
            }

            CommThread.SelectedDevice newDevice = CommThread.SelectedDevice.DeviceAxis;
            bool newCommActive = true;
            switch (TabHost.CurrentTab)
            {
                case 0:
                default:
                    newDevice = CommThread.SelectedDevice.DeviceAxis;
                    break;

                case 1:
                    newDevice = CommThread.SelectedDevice.DeviceMotor;
                    break;

                case 2:
                    newDevice = CommThread.SelectedDevice.DeviceMotorUnevenRunning;
                    newCommActive = false;
                    break;

                case 3:
                    newDevice = CommThread.SelectedDevice.DeviceMotorRotIrregular;
                    newCommActive = false;
                    break;

                case 4:
                    newDevice = CommThread.SelectedDevice.DeviceMotorPM;
                    break;

                case 5:
                    newDevice = CommThread.SelectedDevice.DeviceCccNav;
                    break;

                case 6:
                    newDevice = CommThread.SelectedDevice.DeviceIhk;
                    break;

                case 7:
                    newDevice = CommThread.SelectedDevice.DeviceErrors;
                    break;

                case 8:
                    newDevice = CommThread.SelectedDevice.Test;
                    break;
            }
            if (commThread.Device != newDevice)
            {
                commThread.CommActive = newCommActive;
                commThread.Device = newDevice;
            }
        }

        private void UpdateDisplay()
        {
            DataUpdatedMethode();
        }

        private void DataUpdatedMethode()
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

            if (commThread != null && commThread.ThreadRunning ())
            {
                if (commThread.ThreadStopping ())
                {
                    buttonConnectEnable = false;
                }
                if (commThread.CommActive)
                {
                    switch (commThread.Device)
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
                }
                buttonConnect.Checked = true;
            }
            else
            {
                buttonConnect.Checked = false;
            }
            buttonConnect.Enabled = buttonConnectEnable;

            if (axisDataValid)
            {
                string outputText = string.Empty;
                string tempText;
                bool found;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                Int64 axisMode = GetResultInt64(resultDict, "MODE_CTRL_LESEN_WERT", out found);
                tempText = string.Empty;
                if (found)
                {
                    if ((axisMode & CommThread.AxisModeConveyor) != 0x00)
                    {
                        tempText = GetString (Resource.String.axis_mode_conveyor);
                    }
                    else if ((axisMode & CommThread.AxisModeTransport) != 0x00)
                    {
                        tempText = GetString (Resource.String.axis_mode_transport);
                    }
                    else if ((axisMode & CommThread.AxisModeGarage) != 0x00)
                    {
                        tempText = GetString (Resource.String.axis_mode_garage);
                    }
                    else
                    {
                        tempText = GetString (Resource.String.axis_mode_normal);
                    }
                }
                outputText += GetString (Resource.String.label_axis_mode) + " " + tempText + "\r\n";

                tempText = FormatResultInt64(resultDict, "ORGFASTFILTER_RL", "{0,4}");
                if (tempText.Length > 0) tempText += " / ";
                tempText += FormatResultInt64(resultDict, "FASTFILTER_RL", "{0,4}");
                outputText += GetString (Resource.String.label_axis_left) + " " + tempText + "\r\n";

                tempText = FormatResultInt64(resultDict, "ORGFASTFILTER_RR", "{0,4}");
                if (tempText.Length > 0) tempText += " / ";
                tempText += FormatResultInt64(resultDict, "FASTFILTER_RR", "{0,4}");
                outputText += GetString (Resource.String.label_axis_right) + " " + tempText + "\r\n";

                Int64 voltage = GetResultInt64(resultDict, "ANALOG_U_KL30", out found);
                if (found)
                {
                    tempText = string.Format("{0,6:0.00}", (double)voltage / 1000);
                }
                else
                {
                    tempText = string.Empty;
                }
                outputText += GetString (Resource.String.label_axis_bat_volt) + " " + tempText + "\r\n";

                tempText = FormatResultInt64(resultDict, "STATE_SPEED", "{0,4}");
                outputText += GetString (Resource.String.label_axis_speed) + " " + tempText + "\r\n";

                tempText = string.Empty;
                for (int channel = 0; channel < 4; channel++)
                {
                    tempText = FormatResultInt64(resultDict, string.Format("STATUS_SIGNALE_NUMERISCH{0}_WERT", channel), "{0}") + tempText;
                }
                outputText += GetString (Resource.String.label_axis_valve_state) + " " + tempText + "\r\n";

                Int64 speed = GetResultInt64(resultDict, "STATE_SPEED", out found);
                if (!found) speed = 0;

                if (speed < 5)
                {
                    buttonAxisDown.Enabled = true;
                }
                else
                {
                    if (commThread.AxisOpMode == CommThread.OperationMode.OpModeDown)
                    {
                        commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
                    }
                    buttonAxisDown.Checked = false;
                    buttonAxisDown.Enabled = false;
                }
                buttonAxisUp.Enabled = true;
                textViewResultAxis.Text = outputText;
            }
            else
            {
                textViewResultAxis.Text = string.Empty;
                buttonAxisDown.Checked = false;
                buttonAxisDown.Enabled = false;
                buttonAxisUp.Checked = false;
                buttonAxisUp.Enabled = false;
                if (commThread != null) commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }

            if (motorDataValid)
            {
                string outputText = string.Empty;
                bool found;
                string dataText;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                outputText += GetString (Resource.String.label_motor_bat_voltage) + " " +
                    FormatResultDouble(resultDict, "STAT_UBATT_WERT", "{0,7:0.00}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_CTSCD_tClntLin_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_air_mass) + " " +
                    FormatResultDouble(resultDict, "STAT_LUFTMASSE_WERT", "{0,7:0.00}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_intake_air_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_LADELUFTTEMPERATUR_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_ambient_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_UMGEBUNGSTEMPERATUR_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_boost_press_set) + " " +
                    FormatResultDouble(resultDict, "STAT_LADEDRUCK_SOLL_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_boost_press_act) + " " +
                    FormatResultDouble(resultDict, "STAT_LADEDRUCK_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_rail_press_set) + " " +
                    FormatResultDouble(resultDict, "STAT_RAILDRUCK_SOLL_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_rail_press_act) + " " +
                    FormatResultDouble(resultDict, "STAT_RAILDRUCK_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_air_mass_set) + " " +
                    FormatResultDouble(resultDict, "STAT_LUFTMASSE_SOLL_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_air_mass_act) + " " +
                    FormatResultDouble(resultDict, "STAT_LUFTMASSE_PRO_HUB_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_ambient_press) + " " +
                    FormatResultDouble(resultDict, "STAT_UMGEBUNGSDRUCK_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_fuel_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_KRAFTSTOFFTEMPERATURK_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_temp_before_filter) + " " +
                    FormatResultDouble(resultDict, "STAT_ABGASTEMPERATUR_VOR_PARTIKELFILTER_1_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_temp_before_cat) + " " +
                    FormatResultDouble(resultDict, "STAT_ABGASTEMPERATUR_VOR_KATALYSATOR_WERT", "{0,6:0.0}") + "\r\n";

                dataText = string.Format("{0,6:0.0}", GetResultDouble(resultDict, "STAT_STRECKE_SEIT_ERFOLGREICHER_REGENERATION_WERT", out found) / 1000.0);
                if (!found) dataText = string.Empty;
                outputText += GetString (Resource.String.label_motor_part_filt_dist_since_regen) + " " + dataText + "\r\n";

                outputText += GetString (Resource.String.label_motor_exhaust_press) + " " +
                    FormatResultDouble(resultDict, "STAT_DIFFERENZDRUCK_UEBER_PARTIKELFILTER_WERT", "{0,6:0.0}") + "\r\n";

                dataText = ((GetResultDouble(resultDict, "STAT_OELDRUCKSCHALTER_EIN_WERT", out found) > 0.5) && found) ? "1" : "0";
                outputText += GetString (Resource.String.label_motor_oil_press_switch) + " " + dataText + "\r\n";

                dataText = ((GetResultDouble(resultDict, "STAT_REGENERATIONSANFORDERUNG_WERT", out found) < 0.5) && found) ? "1" : "0";
                outputText += GetString (Resource.String.label_motor_part_filt_request) + " " + dataText + "\r\n";

                dataText = ((GetResultDouble(resultDict, "STAT_EGT_st_WERT", out found) > 1.5) && found) ? "1" : "0";
                outputText += GetString (Resource.String.label_motor_part_filt_status) + " " + dataText + "\r\n";

                dataText = ((GetResultDouble(resultDict, "STAT_REGENERATION_BLOCKIERUNG_UND_FREIGABE_WERT", out found) < 0.5) && found) ? "1" : "0";
                outputText += GetString (Resource.String.label_motor_part_filt_unblocked) + " " + dataText + "\r\n";

                textViewResultMotor.Text = outputText;
            }
            else
            {
                textViewResultMotor.Text = string.Empty;
            }

            if (motorDataUnevenRunningValid)
            {
                string outputText = string.Empty;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                outputText += GetString (Resource.String.label_motor_quant_corr_c1) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL1_WERT", "{0,5:0.00}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_quant_corr_c2) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL2_WERT", "{0,5:0.00}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_quant_corr_c3) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL3_WERT", "{0,5:0.00}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_quant_corr_c4) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL4_WERT", "{0,5:0.00}") + "\r\n";

                textViewResultMotorUnevenRunning.Text = outputText;
            }
            else
            {
                textViewResultMotorUnevenRunning.Text = string.Empty;
            }
            if (commThread != null && commThread.ThreadRunning ())
            {
                buttonUnevenRunningActive.Enabled = true;
                buttonUnevenRunningActive.Checked = commThread.CommActive;
            }
            else
            {
                buttonUnevenRunningActive.Enabled = false;
                buttonUnevenRunningActive.Checked = false;
            }

            if (motorRotIrregularValid)
            {
                string outputText = string.Empty;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                outputText += GetString (Resource.String.label_motor_rpm_c1) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL1_WERT", "{0,7:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_rpm_c2) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL2_WERT", "{0,7:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_rpm_c3) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL3_WERT", "{0,7:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_rpm_c4) + " " +
                    FormatResultDouble(resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL4_WERT", "{0,7:0.0}") + "\r\n";

                textViewResultMotorRotIrregular.Text = outputText;
            }
            else
            {
                textViewResultMotorRotIrregular.Text = string.Empty;
            }
            if (commThread != null && commThread.ThreadRunning ())
            {
                buttonRotIrregularActive.Enabled = true;
                buttonRotIrregularActive.Checked = commThread.CommActive;
            }
            else
            {
                buttonRotIrregularActive.Enabled = false;
                buttonRotIrregularActive.Checked = false;
            }

            if (motorPmValid)
            {
                string outputText = string.Empty;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                outputText += GetString (Resource.String.label_motor_pm_bat_cap) + " " +
                    FormatResultDouble(resultDict, "STAT_BATTERIE_KAPAZITAET_WERT", "{0,3:0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_pm_soh) + " " +
                    FormatResultDouble(resultDict, "STAT_SOH_WERT", "{0,5:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_pm_soc_fit) + " " +
                    FormatResultDouble(resultDict, "STAT_SOC_FIT_WERT", "{0,5:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_pm_season_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_TEMP_SAISON_WERT", "{0,5:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_motor_pm_cal_events) + " " +
                    FormatResultDouble(resultDict, "STAT_KALIBRIER_EVENT_CNT_WERT", "{0,3:0}") + "\r\n";

                outputText += GetString (Resource.String.label_motor_pm_soc_q) + " " +
                    FormatResultDouble (resultDict, "STAT_Q_SOC_AKTUELL_WERT", "{0,6:0.0}");
                outputText += " " + GetString (Resource.String.label_motor_pm_day1) + " " +
                    FormatResultDouble(resultDict, "STAT_Q_SOC_VOR_1_TAG_WERT", "{0,6:0.0}") + "\r\n";

                outputText += GetString (Resource.String.label_motor_pm_start_cap) + " " +
                    FormatResultDouble(resultDict, "STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT", "{0,5:0.0}");
                outputText += " " + GetString (Resource.String.label_motor_pm_day1) + " " +
                    FormatResultDouble(resultDict, "STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT", "{0,5:0.0}") + "\r\n";

                outputText += GetString (Resource.String.label_motor_pm_soc_percent) + " " +
                    FormatResultDouble(resultDict, "STAT_LADUNGSZUSTAND_AKTUELL_WERT", "{0,5:0.0}");
                outputText += " " + GetString (Resource.String.label_motor_pm_day1) + " " +
                    FormatResultDouble(resultDict, "STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT", "{0,5:0.0}") + "\r\n";

                textViewResultMotorPm.Text = outputText;
            }
            else
            {
                textViewResultMotorPm.Text = string.Empty;
            }

            if (cccNavValid)
            {
                string outputText = string.Empty;
                bool found;
                string dataText;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                outputText += GetString (Resource.String.label_ccc_nav_pos_lat) + " " +
                    FormatResultString(resultDict, "STAT_GPS_POSITION_BREITE", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_pos_long) + " " +
                    FormatResultString(resultDict, "STAT_GPS_POSITION_LAENGE", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_pos_height) + " " +
                    FormatResultString(resultDict, "STAT_GPS_POSITION_HOEHE", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_gps_date_time) + " " +
                    FormatResultString(resultDict, "STAT_TIME_DATE_VAL", "{0}").Replace(".*6*", ".201") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_pos_type) + " " +
                    FormatResultString(resultDict, "STAT_GPS_TEXT", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_speed) + " " +
                    FormatResultString(resultDict, "STAT_SPEED_VAL", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_res_horz) + " " +
                    FormatResultString(resultDict, "STAT_HORIZONTALE_AUFLOES", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_res_vert) + " " +
                    FormatResultString(resultDict, "STAT_VERTICALE_AUFLOES", "{0}") + "\r\n";
                outputText += GetString (Resource.String.label_ccc_nav_res_pos) + " " +
                    FormatResultString(resultDict, "STAT_POSITION_AUFLOES", "{0}") + "\r\n";

                dataText = ((GetResultInt64(resultDict, "STAT_ALMANACH", out found) > 0.5) && found) ? "1" : "0";
                outputText += GetString (Resource.String.label_ccc_nav_almanach) + " " + dataText + "\r\n";

                dataText = ((GetResultInt64(resultDict, "STAT_HIP_DRIVER", out found) < 0.5) && found) ? "1" : "0";
                outputText += GetString (Resource.String.label_ccc_nav_hip_driver) + " " + dataText + "\r\n";

                textViewResultCccNav.Text = outputText;
            }
            else
            {
                textViewResultCccNav.Text = string.Empty;
            }

            if (ihkValid)
            {
                string outputText = string.Empty;
                Dictionary<string, EdiabasNet.ResultData> resultDict = null;
                lock (CommThread.DataLock)
                {
                    resultDict = commThread.EdiabasResultDict;
                }
                outputText += GetString (Resource.String.label_ihk_in_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_TINNEN_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_ihk_in_temp_delay) + " " +
                    FormatResultDouble(resultDict, "STAT_TINNEN_VERZOEGERT_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_ihk_out_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_TAUSSEN_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_ihk_setpoint) + " " +
                    FormatResultDouble(resultDict, "STAT_SOLL_LI_KORRIGIERT_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_ihk_heat_ex_temp) + " " +
                    FormatResultDouble(resultDict, "STAT_WT_RE_WERT", "{0,6:0.0}") + "\r\n";
                outputText += GetString (Resource.String.label_ihk_heat_ex_setpoint) + " " +
                    FormatResultDouble(resultDict, "STAT_WTSOLL_RE_WERT", "{0,6:0.0}") + "\r\n";

                textViewResultIhk.Text = outputText;
            }
            else
            {
                textViewResultIhk.Text = string.Empty;
            }

            if (errorsValid)
            {
                List<CommThread.EdiabasErrorReport> errorReportList = null;
                lock (CommThread.DataLock)
                {
                    errorReportList = commThread.EdiabasErrorReportList;
                }

                string outputText = string.Empty;

                if (errorReportList != null)
                {
                    foreach (CommThread.EdiabasErrorReport errorReport in errorReportList)
                    {
                        string message;
                        int resId = Resources.GetIdentifier (errorReport.DeviceName, "string", PackageName);
                        if (resId != 0)
                        {
                            message = string.Format ("{0}: ", GetString (resId));
                        }
                        else
                        {
                            message = string.Format ("{0}: ", errorReport.DeviceName);
                        }
                        if (errorReport.ErrorDict == null)
                        {
                            message += GetString (Resource.String.error_no_response);
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

                            outputText += message;
                        }
                    }
                    if (outputText.Length == 0)
                    {
                        outputText = GetString (Resource.String.error_no_error);
                    }
                }
                textViewResultErrors.Text = outputText;
            }
            else
            {
                textViewResultErrors.Text = string.Empty;
            }

            if (testValid)
            {
                lock (CommThread.DataLock)
                {
                    textViewResultTest.Text = commThread.TestResult;
                }
            }
            else
            {
                textViewResultTest.Text = string.Empty;
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

        private bool CopyAssets(string ecuPath)
        {
            try
            {
                if (!Directory.Exists(ecuPath))
                {
                    Directory.CreateDirectory (ecuPath);
                }

                string assetDir = "Ecu";
                string[] assetList = Assets.List (assetDir);
                foreach(string assetName in assetList)
                {
                    using (Stream asset = Assets.Open (Path.Combine(assetDir, assetName)))
                    {
                        string fileDest = Path.Combine(ecuPath, assetName);
                        bool copyFile = false;
                        if (!File.Exists(fileDest))
                        {
                            copyFile = true;
                        }
                        else
                        {
                            using (var fileComp = new FileStream(fileDest, FileMode.Open))
                            {
                                copyFile = !StreamEquals(asset, fileComp);
                            }
                        }
                        if (copyFile)
                        {
                            using (Stream dest = File.Create (fileDest))
                            {
                                asset.CopyTo (dest);
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        private static bool StreamEquals(Stream stream1, Stream stream2)
        {
            const int bufferSize = 2048;
            byte[] buffer1 = new byte[bufferSize]; //buffer size
            byte[] buffer2 = new byte[bufferSize];
            while (true) {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                    return false;

                if (count1 == 0)
                    return true;

                for (int i = 0; i < buffer1.Length; i++)
                {
                    if (buffer1 [i] != buffer2 [i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
