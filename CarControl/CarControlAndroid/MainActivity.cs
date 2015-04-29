using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CarControl;
using EdiabasLib;
using Java.Interop;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CarControlAndroid
{
    [Android.App.Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat", MainLauncher = true,
               ConfigurationChanges=Android.Content.PM.ConfigChanges.KeyboardHidden | Android.Content.PM.ConfigChanges.Orientation)]
    public class ActivityMain : ActionBarActivity, ActionBar.ITabListener
    {
        enum activityRequest
        {
            REQUEST_SELECT_DEVICE,
            REQUEST_ENABLE_BT
        }

        public static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
        private string deviceName = string.Empty;
        private string deviceAddress = string.Empty;
        private bool loggingActive = false;
        private const string sharedAppName = "CarControl";
        private string externalPath;
        private string ecuPath;
        private JobReader jobReader;
        private Handler updateHandler;
        private BluetoothAdapter bluetoothAdapter;
        private CommThread commThread;
        private List<Fragment> fragmentList;
        private ToggleButton buttonConnect;
        private View barConnectView;
        private Fragment fragmentAxis;
        private Fragment fragmentMotor;
        private Fragment fragmentMotorUnevenRunning;
        private Fragment fragmentMotorRotIrregular;
        private Fragment fragmentMotorPm;
        private Fragment fragmentCccNav;
        private Fragment fragmentIhk;
        private Fragment fragmentErrors;
        private Fragment fragmentAdapterConfig;
        private Fragment fragmentTest;

        public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //Log.Debug(Tag, "The tab {0} was re-selected.", tab.Text);
        }

        public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //Log.Debug(Tag, "The tab {0} has been selected.", tab.Text);
            Fragment frag = fragmentList[tab.Position];
            ft.Replace(Resource.Id.tabFrameLayout, frag);
            UpdateSelectedDevice();
        }

        public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            // perform any extra work associated with saving fragment state here.
            //Log.Debug(Tag, "The tab {0} as been unselected.", tab.Text);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);

            SupportActionBar.NavigationMode = Android.Support.V7.App.ActionBar.NavigationModeTabs;
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SupportActionBar.SetDisplayUseLogoEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetIcon(Android.Resource.Color.Transparent);   // hide icon
            SetContentView (Resource.Layout.main);

            GetSettings();
            externalPath = Path.Combine (Path.Combine (Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "external_sd"), "CarControl");
            jobReader = new JobReader(Path.Combine (externalPath, "JobList.xml"));
            updateHandler = new Handler();

            barConnectView = LayoutInflater.Inflate(Resource.Layout.bar_connect, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(barConnectView, barLayoutParams);

            buttonConnect = barConnectView.FindViewById<ToggleButton> (Resource.Id.buttonConnect);
            fragmentList = new List<Fragment>();

            fragmentAxis = new TabContentFragment(this, Resource.Layout.tab_axis);
            fragmentList.Add(fragmentAxis);
            AddTabToActionBar(Resource.String.tab_axis);

            fragmentMotor = new TabContentFragment(this, Resource.Layout.tab_list);
            fragmentList.Add(fragmentMotor);
            AddTabToActionBar(Resource.String.tab_motor);

            fragmentMotorUnevenRunning = new TabContentFragment(this, Resource.Layout.tab_activate);
            fragmentList.Add(fragmentMotorUnevenRunning);
            AddTabToActionBar(Resource.String.tab_motor_uneven_running);

            fragmentMotorRotIrregular = new TabContentFragment(this, Resource.Layout.tab_activate);
            fragmentList.Add(fragmentMotorRotIrregular);
            AddTabToActionBar(Resource.String.tab_motor_rot_irregular);

            fragmentMotorPm = new TabContentFragment(this, Resource.Layout.tab_list);
            fragmentList.Add(fragmentMotorPm);
            AddTabToActionBar(Resource.String.tab_motor_pm);

            fragmentCccNav = new TabContentFragment(this, Resource.Layout.tab_list);
            fragmentList.Add(fragmentCccNav);
            AddTabToActionBar(Resource.String.tab_ccc_nav);

            fragmentIhk = new TabContentFragment(this, Resource.Layout.tab_list);
            fragmentList.Add(fragmentIhk);
            AddTabToActionBar(Resource.String.tab_ihk);

            fragmentErrors = new TabContentFragment(this, Resource.Layout.tab_list);
            fragmentList.Add(fragmentErrors);
            AddTabToActionBar(Resource.String.tab_errors);

            fragmentAdapterConfig = new TabContentFragment(this, Resource.Layout.tab_adapter);
            fragmentList.Add(fragmentAdapterConfig);
            AddTabToActionBar(Resource.String.tab_adapter_config);

            fragmentTest = new TabContentFragment(this, Resource.Layout.tab_text);
            fragmentList.Add(fragmentTest);
            AddTabToActionBar(Resource.String.tab_test);

            foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
            {
                int resourceId = Resource.Layout.tab_list;
                if (pageInfo.JobInfo.Activate) resourceId = Resource.Layout.tab_activate;

                Fragment fragmentPage = new TabContentFragment(this, resourceId, pageInfo);
                fragmentList.Add(fragmentPage);
                pageInfo.InfoObject = fragmentPage;
                AddTabToActionBar(GetPageString(pageInfo, pageInfo.Name));
            }

            // Get local Bluetooth adapter
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, Resource.String.bt_not_available, ToastLength.Long).Show ();
                Finish ();
                return;
            }

            ecuPath = Path.Combine (
                System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "Ecu");
            // copy asset files
            CopyAssets (ecuPath);
            // compile user code
            CompileCode();

            // Get our button from the layout resource,
            // and attach an event to it
            buttonConnect.Click += ButtonConnectClick;
            UpdateDisplay();
        }

        void AddTabToActionBar(int labelResourceId)
        {
            ActionBar.Tab tab = SupportActionBar.NewTab()
                .SetText(labelResourceId)
                .SetTabListener(this);
            SupportActionBar.AddTab(tab);
        }

        void AddTabToActionBar(string label)
        {
            ActionBar.Tab tab = SupportActionBar.NewTab()
                .SetText(label)
                .SetTabListener(this);
            SupportActionBar.AddTab(tab);
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

        protected override void OnActivityResult (int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch((activityRequest)requestCode)
            {
            case activityRequest.REQUEST_SELECT_DEVICE:
                // When DeviceListActivity returns with a device to connect
                if (resultCode == Android.App.Result.Ok)
                {
                    // Get the device MAC address
                    deviceName = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_NAME);
                    deviceAddress = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                    SupportInvalidateOptionsMenu();
                }
                break;

            case activityRequest.REQUEST_ENABLE_BT:
                // When the request to enable Bluetooth returns
                if (resultCode != Android.App.Result.Ok)
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
            IMenuItem scanMenu = menu.FindItem (Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(culture, "{0}: {1}", GetString(Resource.String.menu_device), deviceName));
                scanMenu.SetEnabled(!commActive);
            }
            IMenuItem logMenu = menu.FindItem (Resource.Id.menu_enable_log);
            if (logMenu != null)
            {
                logMenu.SetEnabled(!commActive);
                logMenu.SetChecked(loggingActive);
            }
            return base.OnPrepareOptionsMenu (menu);
        }

        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            switch (item.ItemId) 
            {
                case Resource.Id.menu_scan:
                    // Launch the DeviceListActivity to see devices and do scan
                    Intent serverIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(serverIntent, (int)activityRequest.REQUEST_SELECT_DEVICE);
                    return true;

                case Resource.Id.menu_enable_log:
                    loggingActive = !loggingActive;
                    SupportInvalidateOptionsMenu();
                    return true;

                case Resource.Id.menu_exit:
                    OnDestroy();
                    System.Environment.Exit(0);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
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

        [Export ("onAxisDownClick")]
        public void OnAxisDownClick (View v)
        {
            if (commThread == null)
            {
                return;
            }
            ToggleButton buttonDown = (ToggleButton)v;
            if (buttonDown.Checked)
            {
                commThread.AxisOpMode = CommThread.OperationMode.OpModeDown;
                View parentView = (View)v.Parent;
                ToggleButton buttonUp = parentView.FindViewById<ToggleButton> (Resource.Id.button_axis_up);
                if (buttonUp != null) buttonUp.Checked = false;
            }
            else
            {
                if (commThread != null) commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }
        }

        [Export ("onAxisUpClick")]
        public void OnAxisUpClick (View v)
        {
            if (commThread == null)
            {
                return;
            }
            ToggleButton buttonUp = (ToggleButton)v;
            if (buttonUp.Checked)
            {
                commThread.AxisOpMode = CommThread.OperationMode.OpModeUp;
                View parentView = (View)v.Parent;
                ToggleButton buttonDown = parentView.FindViewById<ToggleButton> (Resource.Id.button_axis_down);
                if (buttonDown != null) buttonDown.Checked = false;
            }
            else
            {
                commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
            }
        }

        [Export ("onActiveClick")]
        public void OnActiveClick (View v)
        {
            if (commThread == null)
            {
                return;
            }
            ToggleButton button = v.FindViewById<ToggleButton> (Resource.Id.button_active);
            commThread.CommActive = button.Checked;
        }

        [Export ("onConfig500Click")]
        public void OnConfig500Click (View v)
        {
            if (commThread == null)
            {
                return;
            }
            commThread.AdapterConfigValue = 0x01;
            UpdateDisplay();
        }

        [Export ("onConfig100Click")]
        public void OnConfig100Click (View v)
        {
            if (commThread == null)
            {
                return;
            }
            commThread.AdapterConfigValue = 0x09;
            UpdateDisplay();
        }

        [Export ("onConfigOffClick")]
        public void OnConfigOffClick (View v)
        {
            if (commThread == null)
            {
                return;
            }
            commThread.AdapterConfigValue = 0x00;
            UpdateDisplay();
        }

        private bool StartCommThread()
        {
            try
            {
                if (commThread == null)
                {
                    commThread = new CommThread(ecuPath);
                    commThread.DataUpdated += DataUpdated;
                    commThread.ThreadTerminated += ThreadTerminated;
                }
                string logFile = null;
                if (loggingActive)
                {
                    logFile = Path.Combine(externalPath, "ifh.trc");
                }
                JobReader.PageInfo selPageInfo;
                CommThread.SelectedDevice selDevice = GetSelectedDevice(out selPageInfo);
                commThread.StartThread("BLUETOOTH:" + deviceAddress, logFile, selDevice, selPageInfo, true);
            }
            catch (Exception)
            {
                return false;
            }
            SupportInvalidateOptionsMenu();
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
            SupportInvalidateOptionsMenu();
            return true;
        }

        private bool GetSettings()
        {
            try
            {
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                deviceName = prefs.GetString("DeviceName", "DIAG");
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
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(sharedAppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.PutString("DeviceName", deviceName);
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

        private CommThread.SelectedDevice GetSelectedDevice(out JobReader.PageInfo pageInfo)
        {
            CommThread.SelectedDevice selDevice = CommThread.SelectedDevice.DeviceAxis;
            JobReader.PageInfo selPageInfo = null;
            int index = SupportActionBar.SelectedTab.Position;
            switch (index)
            {
                case 0:
                    selDevice = CommThread.SelectedDevice.DeviceAxis;
                    break;

                case 1:
                    selDevice = CommThread.SelectedDevice.DeviceMotor;
                    break;

                case 2:
                    selDevice = CommThread.SelectedDevice.DeviceMotorUnevenRunning;
                    break;

                case 3:
                    selDevice = CommThread.SelectedDevice.DeviceMotorRotIrregular;
                    break;

                case 4:
                    selDevice = CommThread.SelectedDevice.DeviceMotorPM;
                    break;

                case 5:
                    selDevice = CommThread.SelectedDevice.DeviceCccNav;
                    break;

                case 6:
                    selDevice = CommThread.SelectedDevice.DeviceIhk;
                    break;

                case 7:
                    selDevice = CommThread.SelectedDevice.DeviceErrors;
                    break;

                case 8:
                    selDevice = CommThread.SelectedDevice.AdapterConfig;
                    break;

                case 9:
                    selDevice = CommThread.SelectedDevice.Test;
                    break;

                default:
                    if (index >= 10 && index < (10 + jobReader.PageList.Count))
                    {
                        selDevice = CommThread.SelectedDevice.Dynamic;
                        selPageInfo = jobReader.PageList[index - 10];
                    }
                    break;
            }
            pageInfo = selPageInfo;
            return selDevice;
        }

        private void UpdateSelectedDevice()
        {
            if ((commThread == null) || !commThread.ThreadRunning())
            {
                return;
            }

            JobReader.PageInfo newPageInfo;
            CommThread.SelectedDevice newDevice = GetSelectedDevice(out newPageInfo);
            bool newCommActive = true;
            switch (newDevice)
            {
                case CommThread.SelectedDevice.DeviceMotorUnevenRunning:
                case CommThread.SelectedDevice.DeviceMotorRotIrregular:
                    newCommActive = false;
                    break;
            }
            if (newPageInfo != null && newPageInfo.JobInfo.Activate)
            {
                newCommActive = false;
            }
            if ((commThread.Device != newDevice) || (commThread.JobPageInfo != newPageInfo))
            {
                commThread.CommActive = newCommActive;
                commThread.JobPageInfo = newPageInfo;
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
            bool adapterConfigValid = false;
            bool testValid = false;
            bool dynamicValid = false;
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
                            motorRotIrregularValid  = true;
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

                        case CommThread.SelectedDevice.AdapterConfig:
                            adapterConfigValid = true;
                            break;

                        case CommThread.SelectedDevice.Test:
                            testValid = true;
                            break;

                        case CommThread.SelectedDevice.Dynamic:
                            dynamicValid = true;
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

            if (fragmentAxis != null && fragmentAxis.View != null)
            {
                ListView listViewResult = fragmentAxis.View.FindViewById<ListView> (Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter (this, 2);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                ToggleButton buttonAxisUp = fragmentAxis.View.FindViewById<ToggleButton> (Resource.Id.button_axis_up);
                ToggleButton buttonAxisDown = fragmentAxis.View.FindViewById<ToggleButton> (Resource.Id.button_axis_down);

                if (axisDataValid)
                {
                    string dataText;
                    bool found;
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    Int64 axisMode = GetResultInt64 (resultDict, "MODE_CTRL_LESEN_WERT", out found);
                    dataText = string.Empty;
                    if (found)
                    {
                        if ((axisMode & CommThread.AxisModeConveyor) != 0x00)
                        {
                            dataText = GetString (Resource.String.axis_mode_conveyor);
                        }
                        else if ((axisMode & CommThread.AxisModeTransport) != 0x00)
                        {
                            dataText = GetString (Resource.String.axis_mode_transport);
                        }
                        else if ((axisMode & CommThread.AxisModeGarage) != 0x00)
                        {
                            dataText = GetString (Resource.String.axis_mode_garage);
                        }
                        else
                        {
                            dataText = GetString (Resource.String.axis_mode_normal);
                        }
                    }
                    resultListAdapter.Items.Add (new TableResultItem (GetString (Resource.String.label_axis_mode), dataText));

                    dataText = FormatResultInt64 (resultDict, "ORGFASTFILTER_RL", "{0,4}");
                    if (dataText.Length > 0)
                        dataText += " / ";
                    dataText += FormatResultInt64 (resultDict, "FASTFILTER_RL", "{0,4}");
                    resultListAdapter.Items.Add (new TableResultItem (GetString (Resource.String.label_axis_left), dataText));

                    dataText = FormatResultInt64 (resultDict, "ORGFASTFILTER_RR", "{0,4}");
                    if (dataText.Length > 0)
                        dataText += " / ";
                    dataText += FormatResultInt64 (resultDict, "FASTFILTER_RR", "{0,4}");
                    resultListAdapter.Items.Add (new TableResultItem (GetString (Resource.String.label_axis_right), dataText));

                    Int64 voltage = GetResultInt64 (resultDict, "ANALOG_U_KL30", out found);
                    if (found)
                    {
                        dataText = string.Format (culture, "{0,6:0.00}", (double)voltage / 1000);
                    }
                    else
                    {
                        dataText = string.Empty;
                    }
                    resultListAdapter.Items.Add (new TableResultItem (GetString (Resource.String.label_axis_bat_volt), dataText));

                    dataText = FormatResultInt64 (resultDict, "STATE_SPEED", "{0,4}");
                    resultListAdapter.Items.Add (new TableResultItem (GetString (Resource.String.label_axis_speed), dataText));

                    dataText = string.Empty;
                    for (int channel = 0; channel < 4; channel++)
                    {
                        dataText = FormatResultInt64 (resultDict, string.Format (culture, "STATUS_SIGNALE_NUMERISCH{0}_WERT", channel), "{0}") + dataText;
                    }
                    resultListAdapter.Items.Add (new TableResultItem (GetString (Resource.String.label_axis_valve_state), dataText));

                    Int64 speed = GetResultInt64 (resultDict, "STATE_SPEED", out found);
                    if (!found)
                    {
                        speed = 0;
                    }

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
                    switch (commThread.AxisOpMode)
                    {
                        case CommThread.OperationMode.OpModeStatus:
                            buttonAxisDown.Checked = false;
                            buttonAxisUp.Checked = false;
                            break;

                        case CommThread.OperationMode.OpModeUp:
                            buttonAxisDown.Checked = false;
                            buttonAxisUp.Checked = true;
                            break;

                        case CommThread.OperationMode.OpModeDown:
                            buttonAxisDown.Checked = true;
                            buttonAxisUp.Checked = false;
                            break;
                    }
                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();

                    buttonAxisDown.Checked = false;
                    buttonAxisDown.Enabled = false;
                    buttonAxisUp.Checked = false;
                    buttonAxisUp.Enabled = false;
                    if (commThread != null)
                    {
                        commThread.AxisOpMode = CommThread.OperationMode.OpModeStatus;
                    }
                }
            }

            if (fragmentMotor != null && fragmentMotor.View != null)
            {
                ListView listViewResult = fragmentMotor.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;

                if (motorDataValid)
                {
                    bool found;
                    string dataText;
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_bat_voltage), FormatResultDouble (resultDict, "STAT_UBATT_WERT", "{0,7:0.00}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_temp), FormatResultDouble (resultDict, "STAT_CTSCD_tClntLin_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_air_mass), FormatResultDouble (resultDict, "STAT_LUFTMASSE_WERT", "{0,7:0.00}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_intake_air_temp), FormatResultDouble (resultDict, "STAT_LADELUFTTEMPERATUR_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_ambient_temp), FormatResultDouble (resultDict, "STAT_UMGEBUNGSTEMPERATUR_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_boost_press_set), FormatResultDouble (resultDict, "STAT_LADEDRUCK_SOLL_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_boost_press_act), FormatResultDouble (resultDict, "STAT_LADEDRUCK_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_rail_press_set), FormatResultDouble (resultDict, "STAT_RAILDRUCK_SOLL_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_rail_press_act), FormatResultDouble (resultDict, "STAT_RAILDRUCK_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_air_mass_set), FormatResultDouble (resultDict, "STAT_LUFTMASSE_SOLL_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_air_mass_act), FormatResultDouble (resultDict, "STAT_LUFTMASSE_PRO_HUB_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_ambient_press), FormatResultDouble (resultDict, "STAT_UMGEBUNGSDRUCK_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_fuel_temp), FormatResultDouble (resultDict, "STAT_KRAFTSTOFFTEMPERATURK_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_temp_before_filter), FormatResultDouble (resultDict, "STAT_ABGASTEMPERATUR_VOR_PARTIKELFILTER_1_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_temp_before_cat), FormatResultDouble (resultDict, "STAT_ABGASTEMPERATUR_VOR_KATALYSATOR_WERT", "{0,6:0.0}")));

                    dataText = string.Format (culture, "{0,6:0.0}", GetResultDouble (resultDict, "STAT_STRECKE_SEIT_ERFOLGREICHER_REGENERATION_WERT", out found) / 1000.0);
                    if (!found)
                        dataText = string.Empty;
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_part_filt_dist_since_regen), dataText));

                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_exhaust_press), FormatResultDouble (resultDict, "STAT_DIFFERENZDRUCK_UEBER_PARTIKELFILTER_WERT", "{0,6:0.0}")));

                    dataText = ((GetResultDouble (resultDict, "STAT_OELDRUCKSCHALTER_EIN_WERT", out found) > 0.5) && found) ? "1" : "0";
                    if (!found)
                        dataText = string.Empty;
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_oil_press_switch), dataText));

                    dataText = ((GetResultDouble (resultDict, "STAT_REGENERATIONSANFORDERUNG_WERT", out found) < 0.5) && found) ? "1" : "0";
                    if (!found)
                        dataText = string.Empty;
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_part_filt_request), dataText));

                    dataText = ((GetResultDouble (resultDict, "STAT_EGT_st_WERT", out found) > 1.5) && found) ? "1" : "0";
                    if (!found)
                        dataText = string.Empty;
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_part_filt_status), dataText));

                    dataText = ((GetResultDouble (resultDict, "STAT_REGENERATION_BLOCKIERUNG_UND_FREIGABE_WERT", out found) < 0.5) && found) ? "1" : "0";
                    if (!found)
                        dataText = string.Empty;
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_part_filt_unblocked), dataText));

                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();
                }
            }

            if (fragmentMotorUnevenRunning != null && fragmentMotorUnevenRunning.View != null)
            {
                ListView listViewResult = fragmentMotorUnevenRunning.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                ToggleButton buttonActive = fragmentMotorUnevenRunning.View.FindViewById<ToggleButton> (Resource.Id.button_active);

                if (motorDataUnevenRunningValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_quant_corr_c1), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL1_WERT", "{0,5:0.00}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_quant_corr_c2), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL2_WERT", "{0,5:0.00}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_quant_corr_c3), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL3_WERT", "{0,5:0.00}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_quant_corr_c4), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_LLR_MENGE_ZYL4_WERT", "{0,5:0.00}")));

                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();
                }
                if (commThread != null && commThread.ThreadRunning ())
                {
                    buttonActive.Enabled = true;
                    buttonActive.Checked = commThread.CommActive;
                }
                else
                {
                    buttonActive.Enabled = false;
                    buttonActive.Checked = false;
                }
            }

            if (fragmentMotorRotIrregular != null && fragmentMotorRotIrregular.View != null)
            {
                ListView listViewResult = fragmentMotorRotIrregular.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                ToggleButton buttonActive = fragmentMotorRotIrregular.View.FindViewById<ToggleButton> (Resource.Id.button_active);

                if (motorRotIrregularValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_rpm_c1), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL1_WERT", "{0,7:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_rpm_c2), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL2_WERT", "{0,7:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_rpm_c3), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL3_WERT", "{0,7:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_rpm_c4), FormatResultDouble (resultDict, "STAT_LAUFUNRUHE_DREHZAHL_ZYL4_WERT", "{0,7:0.0}")));

                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();
                }
                if (commThread != null && commThread.ThreadRunning ())
                {
                    buttonActive.Enabled = true;
                    buttonActive.Checked = commThread.CommActive;
                }
                else
                {
                    buttonActive.Enabled = false;
                    buttonActive.Checked = false;
                }
            }

            if (fragmentMotorPm != null && fragmentMotorPm.View != null)
            {
                ListView listViewResult = fragmentMotorPm.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;

                if (motorPmValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_bat_cap), FormatResultDouble (resultDict, "STAT_BATTERIE_KAPAZITAET_WERT", "{0,3:0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_soh), FormatResultDouble (resultDict, "STAT_SOH_WERT", "{0,5:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_soc_fit), FormatResultDouble (resultDict, "STAT_SOC_FIT_WERT", "{0,5:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_season_temp), FormatResultDouble (resultDict, "STAT_TEMP_SAISON_WERT", "{0,5:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_cal_events), FormatResultDouble (resultDict, "STAT_KALIBRIER_EVENT_CNT_WERT", "{0,3:0}")));

                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_soc_q), FormatResultDouble (resultDict, "STAT_Q_SOC_AKTUELL_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_day1), FormatResultDouble (resultDict, "STAT_Q_SOC_VOR_1_TAG_WERT", "{0,6:0.0}")));

                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_start_cap), FormatResultDouble (resultDict, "STAT_STARTFAEHIGKEITSGRENZE_AKTUELL_WERT", "{0,5:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_day1), FormatResultDouble (resultDict, "STAT_STARTFAEHIGKEITSGRENZE_VOR_1_TAG_WERT", "{0,5:0.0}")));

                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_soc_percent), FormatResultDouble (resultDict, "STAT_LADUNGSZUSTAND_AKTUELL_WERT", "{0,5:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_motor_pm_day1), FormatResultDouble (resultDict, "STAT_LADUNGSZUSTAND_VOR_1_TAG_WERT", "{0,5:0.0}")));

                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();
                }
            }

            if (fragmentCccNav != null && fragmentCccNav.View != null)
            {
                ListView listViewResult = fragmentCccNav.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this, 1.8f);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;

                if (cccNavValid)
                {
                    bool found;
                    string dataText;
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_pos_lat), FormatResultString (resultDict, "STAT_GPS_POSITION_BREITE", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_pos_long), FormatResultString (resultDict, "STAT_GPS_POSITION_LAENGE", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_pos_height), FormatResultString (resultDict, "STAT_GPS_POSITION_HOEHE", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_gps_date_time), FormatResultString (resultDict, "STAT_TIME_DATE_VAL", "{0}").Replace (".*6*", ".201")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_pos_type), FormatResultString (resultDict, "STAT_GPS_TEXT", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_speed), FormatResultString (resultDict, "STAT_SPEED_VAL", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_res_horz), FormatResultString (resultDict, "STAT_HORIZONTALE_AUFLOES", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_res_vert), FormatResultString (resultDict, "STAT_VERTICALE_AUFLOES", "{0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_res_pos), FormatResultString (resultDict, "STAT_POSITION_AUFLOES", "{0}")));

                    dataText = ((GetResultInt64 (resultDict, "STAT_ALMANACH", out found) > 0.5) && found) ? "1" : "0";
                    if (!found)
                    {
                        dataText = string.Empty;
                    }
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_almanach), dataText));

                    dataText = ((GetResultInt64 (resultDict, "STAT_HIP_DRIVER", out found) < 0.5) && found) ? "1" : "0";
                    if (!found)
                    {
                        dataText = string.Empty;
                    }
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ccc_nav_hip_driver), dataText));

                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();
                }
            }

            if (fragmentIhk != null && fragmentIhk.View != null)
            {
                ListView listViewResult = fragmentIhk.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;

                if (ihkValid)
                {
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ihk_in_temp), FormatResultDouble (resultDict, "STAT_TINNEN_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ihk_in_temp_delay), FormatResultDouble (resultDict, "STAT_TINNEN_VERZOEGERT_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ihk_out_temp), FormatResultDouble (resultDict, "STAT_TAUSSEN_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ihk_setpoint), FormatResultDouble (resultDict, "STAT_SOLL_LI_KORRIGIERT_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ihk_heat_ex_temp), FormatResultDouble (resultDict, "STAT_WT_RE_WERT", "{0,6:0.0}")));
                    resultListAdapter.Items.Add (
                        new TableResultItem (GetString (Resource.String.label_ihk_heat_ex_setpoint), FormatResultDouble (resultDict, "STAT_WTSOLL_RE_WERT", "{0,6:0.0}")));

                    resultListAdapter.NotifyDataSetChanged ();
                }
                else
                {
                    resultListAdapter.Items.Clear ();
                    resultListAdapter.NotifyDataSetChanged ();
                }
            }

            if (fragmentErrors != null && fragmentErrors.View != null)
            {
                ListView listViewResult = fragmentErrors.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;

                if (errorsValid)
                {
                    List<CommThread.EdiabasErrorReport> errorReportList;
                    lock (CommThread.DataLock)
                    {
                        errorReportList = commThread.EdiabasErrorReportList;
                    }
                    resultListAdapter.Items.Clear();
                    if (errorReportList != null)
                    {
                        foreach (CommThread.EdiabasErrorReport errorReport in errorReportList)
                        {
                            string message;
                            int resId = Resources.GetIdentifier (errorReport.DeviceName, "string", PackageName);
                            if (resId != 0)
                            {
                                message = string.Format(culture, "{0}: ", GetString (resId));
                            }
                            else
                            {
                                message = string.Format(culture, "{0}: ", errorReport.DeviceName);
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
                                resultListAdapter.Items.Add(new TableResultItem(message, null));
                            }
                        }
                        if (resultListAdapter.Items.Count == 0)
                        {
                            resultListAdapter.Items.Add(new TableResultItem(GetString (Resource.String.error_no_error), null));
                        }
                    }

                    resultListAdapter.NotifyDataSetChanged();
                }
                else
                {
                    resultListAdapter.Items.Clear();
                    resultListAdapter.NotifyDataSetChanged();
                }
            }

            if (fragmentAdapterConfig != null && fragmentAdapterConfig.View != null)
            {
                ListView listViewResult = fragmentAdapterConfig.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                Button buttonCan500 = fragmentAdapterConfig.View.FindViewById<Button> (Resource.Id.button_adapter_config_can_500);
                Button buttonCan100 = fragmentAdapterConfig.View.FindViewById<Button> (Resource.Id.button_adapter_config_can_100);
                Button buttonCanOff = fragmentAdapterConfig.View.FindViewById<Button> (Resource.Id.button_adapter_config_can_off);

                if (adapterConfigValid)
                {
                    bool found;
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear();

                    Int64 resultValue = GetResultInt64(resultDict, "DONE", out found);
                    if (found)
                    {
                        if (resultValue > 0)
                        {
                            resultListAdapter.Items.Add(new TableResultItem(GetString (Resource.String.adapter_config_ok), null));
                        }
                        else
                        {
                            resultListAdapter.Items.Add(new TableResultItem(GetString (Resource.String.adapter_config_error), null));
                        }
                    }

                    resultListAdapter.NotifyDataSetChanged();
                }
                else
                {
                    resultListAdapter.Items.Clear();
                    resultListAdapter.NotifyDataSetChanged();
                }
                if (commThread != null && commThread.ThreadRunning ())
                {
                    buttonCan500.Enabled = true;
                    buttonCan100.Enabled = true;
                    buttonCanOff.Enabled = true;
                }
                else
                {
                    buttonCan500.Enabled = false;
                    buttonCan100.Enabled = false;
                    buttonCanOff.Enabled = false;
                }
            }

            if (fragmentTest != null && fragmentTest.View != null)
            {
                TextView textView = fragmentTest.View.FindViewById<TextView> (Resource.Id.textViewResult);

                if (testValid)
                {
                    lock (CommThread.DataLock)
                    {
                        textView.Text = commThread.TestResult;
                    }
                }
                else
                {
                    textView.Text = string.Empty;
                }
            }

            Fragment dynamicFragment = null;
            JobReader.PageInfo pageInfo = null;
            GetSelectedDevice(out pageInfo);
            if (pageInfo != null)
            {
                dynamicFragment = (Fragment)pageInfo.InfoObject;
            }

            if (dynamicFragment != null && dynamicFragment.View != null)
            {
                ListView listViewResult = dynamicFragment.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter (this, pageInfo.Weight);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                ToggleButton buttonActive = null;
                if (pageInfo.JobInfo.Activate)
                {
                    buttonActive = dynamicFragment.View.FindViewById<ToggleButton>(Resource.Id.button_active);
                }

                if (dynamicValid)
                {
                    //bool found;
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (CommThread.DataLock)
                    {
                        resultDict = commThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear();

                    foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                    {
                        string result = string.Empty;
                        if (displayInfo.Format == null)
                        {
                            if (resultDict != null)
                            {
                                try
                                {
                                    result = pageInfo.ClassObject.FormatResult(resultDict, displayInfo.Result);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        else
                        {
                            result = FormatResultEdiabas(resultDict, displayInfo.Result, displayInfo.Format);
                        }
                        resultListAdapter.Items.Add(new TableResultItem(GetPageString(pageInfo, displayInfo.Name), result));
                    }

                    resultListAdapter.NotifyDataSetChanged();
                }
                else
                {
                    resultListAdapter.Items.Clear();
                    resultListAdapter.NotifyDataSetChanged();
                }

                try
                {
                    Type pageType = pageInfo.ClassObject.GetType();
                    if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("UpdateLayout") != null)
                    {
                        pageInfo.ClassObject.UpdateLayout(pageInfo, dynamicValid, commThread != null);
                    }
                }
                catch (Exception)
                {
                }

                if (buttonActive != null)
                {
                    if (commThread != null && commThread.ThreadRunning())
                    {
                        buttonActive.Enabled = true;
                        buttonActive.Checked = commThread.CommActive;
                    }
                    else
                    {
                        buttonActive.Enabled = false;
                        buttonActive.Checked = false;
                    }
                }
            }
        }

        public static String FormatResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            double value = GetResultDouble(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            Int64 value = GetResultInt64(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            string value = GetResultString(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultEdiabas(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            string result = string.Empty;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                result = EdiabasNet.FormatResult(resultData, format);
                if (result == null) result = "?";
            }
            return result;
        }

        public static Int64 GetResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData is Int64)
                {
                    found = true;
                    return (Int64)resultData.opData;
                }
            }
            return 0;
        }

        public static Double GetResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData is Double)
                {
                    found = true;
                    return (Double)resultData.opData;
                }
            }
            return 0;
        }

        public static String GetResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.opData is String)
                {
                    found = true;
                    return (String)resultData.opData;
                }
            }
            return string.Empty;
        }

        public static String GetPageString(JobReader.PageInfo pageInfo, string name)
        {
            string lang = CultureInfo.CurrentUICulture.Name;
            string langIso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            JobReader.StringInfo stringInfoDefault = null;
            JobReader.StringInfo stringInfoSel = null;
            foreach (JobReader.StringInfo stringInfo in pageInfo.StringList)
            {
                if (string.IsNullOrEmpty(stringInfo.Lang))
                {
                    stringInfoDefault = stringInfo;
                }
                else if ((string.Compare(stringInfo.Lang, lang, StringComparison.OrdinalIgnoreCase) == 0) ||
                        (string.Compare(stringInfo.Lang, langIso, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    stringInfoSel = stringInfo;
                }
            }
            if (stringInfoSel == null) stringInfoSel = stringInfoDefault;
            string result = String.Empty;
            if (stringInfoSel != null)
            {
                if (!stringInfoSel.StringDict.TryGetValue(name, out result))
                {
                    result = String.Empty;
                }
            }
            return result;
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

        private void CompileCode()
        {
            if (jobReader.pageList.Count == 0)
            {
                return;
            }
            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.compile_start));
            progress.Show();

            new Thread(new ThreadStart(delegate
            {
                foreach (JobReader.PageInfo pageInfo in jobReader.pageList)
                {
                    if (pageInfo.JobInfo.ClassCode == null) continue;
                    StringWriter reportWriter = new StringWriter();
                    try
                    {
                        Evaluator evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter(reportWriter)));
                        evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
                        evaluator.ReferenceAssembly(typeof(EdiabasNet).Assembly);
                        evaluator.ReferenceAssembly(typeof(View).Assembly);
                        string classCode = @"
                            using Android.Views;
                            using Android.Widget;
                            using EdiabasLib;
                            using CarControl;
                            using CarControlAndroid;
                            using System;
                            using System.Collections.Generic;"
                            + pageInfo.JobInfo.ClassCode;
                        evaluator.Compile(classCode);
                        pageInfo.Eval = evaluator;
                        pageInfo.ClassObject = evaluator.Evaluate("new PageClass()");
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("ExecuteJob") == null)
                        {
                            throw new Exception("No ExecuteJob method");
                        }
                    }
                    catch (Exception ex)
                    {
                        string exText = reportWriter.ToString();
                        if (string.IsNullOrEmpty(exText))
                        {
                            exText = EdiabasNet.GetExceptionText(ex);
                        }
                        exText = pageInfo.Name + ":\r\n" + exText;
                        RunOnUiThread(() => ShowAlert(exText));
                    }
                }

                RunOnUiThread(() => progress.Hide());
            })).Start();
        }

        private void ShowAlert(string message)
        {
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetNeutralButton(Resource.String.compile_ok_btn, (s, e) => { });
            builder.Create().Show();
        }

        public class TabContentFragment : Fragment
        {
            private ActivityMain activity;
            private int resourceId;
            private JobReader.PageInfo pageInfo;

            public TabContentFragment(ActivityMain activity, int resourceId, JobReader.PageInfo pageInfo)
            {
                this.activity = activity;
                this.resourceId = resourceId;
                this.pageInfo = pageInfo;
            }

            public TabContentFragment(ActivityMain activity, int resourceId)
                : this(activity, resourceId, null)
            {
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                View view = inflater.Inflate(resourceId, null);
                if (pageInfo != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("CreateLayout") != null)
                        {
                            LinearLayout pageLayout = view.FindViewById<LinearLayout>(Resource.Id.listLayout);
                            pageInfo.ClassObject.CreateLayout(pageInfo, pageLayout);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                activity.updateHandler.Post(() =>
                {
                    activity.UpdateDisplay();
                });
                return view;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();

                if (pageInfo != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (string.IsNullOrEmpty(pageInfo.JobInfo.Name) && pageType.GetMethod("DestroyLayout") != null)
                        {
                            pageInfo.ClassObject.DestroyLayout(pageInfo);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
