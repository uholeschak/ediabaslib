using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using IO.Sule.Gaugelibrary;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/settings_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class GlobalSettingsActivity : AppCompatActivity
    {
        // Intent extra
        public const string ExtraSelection = "selection";
        public const string SelectionStorageLocation = "storage_location";

        private enum ActivityRequest
        {
            RequestDevelopmentSettings,
        }

        private string _selection;
        private ActivityCommon _activityCommon;
        private RadioButton _radioButtonAskForBtEnable;
        private RadioButton _radioButtonAlwaysEnableBt;
        private RadioButton _radioButtonNoBtHandling;
        private CheckBox _checkBoxDisableBtAtExit;
        private RadioButton _radioButtonCommLockNone;
        private RadioButton _radioButtonCommLockCpu;
        private RadioButton _radioButtonCommLockDim;
        private RadioButton _radioButtonCommLockBright;
        private RadioButton _radioButtonLogLockNone;
        private RadioButton _radioButtonLogLockCpu;
        private RadioButton _radioButtonLogLockDim;
        private RadioButton _radioButtonLogLockBright;
        private CheckBox _checkBoxStoreDataLogSettings;
        private RadioButton _radioButtonStartOffline;
        private RadioButton _radioButtonStartConnect;
        private RadioButton _radioButtonStartConnectClose;
        private CheckBox _checkBoxDoubleClickForAppExit;
        private CheckBox _checkBoxSendDataBroadcast;
        private TextView _textViewCaptionCpuUsage;
        private CheckBox _checkBoxCheckCpuUsage;
        private CheckBox _checkBoxCheckEcuFiles;
        private CheckBox _checkBoxOldVagMode;
        private Button _buttonStorageLocation;
        private CheckBox _checkBoxCollectDebugInfo;
        private CheckBox _checkBoxHciSnoopLog;
        private Button _buttonHciSnoopLog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.settings);

            SetResult(Android.App.Result.Canceled);
            _selection = Intent.GetStringExtra(ExtraSelection);

            _activityCommon = new ActivityCommon(this);

            GaugeView gaugeView = FindViewById<GaugeView>(Resource.Id.gauge_view1);
            gaugeView.SetTargetValue(50);
            gaugeView.Visibility = ViewStates.Gone;

            _radioButtonAskForBtEnable = FindViewById<RadioButton>(Resource.Id.radioButtonAskForBtEnable);
            _radioButtonAlwaysEnableBt = FindViewById<RadioButton>(Resource.Id.radioButtonAlwaysEnableBt);
            _radioButtonNoBtHandling = FindViewById<RadioButton>(Resource.Id.radioButtonNoBtHandling);

            _checkBoxDisableBtAtExit = FindViewById<CheckBox>(Resource.Id.checkBoxDisableBtAtExit);

            _radioButtonCommLockNone = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockNone);
            _radioButtonCommLockCpu = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockCpu);
            _radioButtonCommLockDim = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockDim);
            _radioButtonCommLockBright = FindViewById<RadioButton>(Resource.Id.radioButtonCommLockBright);

            _radioButtonLogLockNone = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockNone);
            _radioButtonLogLockCpu = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockCpu);
            _radioButtonLogLockDim = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockDim);
            _radioButtonLogLockBright = FindViewById<RadioButton>(Resource.Id.radioButtonLogLockBright);

            _checkBoxStoreDataLogSettings = FindViewById<CheckBox>(Resource.Id.checkBoxStoreDataLogSettings);

            _radioButtonStartOffline = FindViewById<RadioButton>(Resource.Id.radioButtonStartOffline);
            _radioButtonStartConnect = FindViewById<RadioButton>(Resource.Id.radioButtonStartConnect);
            _radioButtonStartConnectClose = FindViewById<RadioButton>(Resource.Id.radioButtonStartConnectClose);

            _checkBoxDoubleClickForAppExit = FindViewById<CheckBox>(Resource.Id.checkBoxDoubleClickForAppExit);
            _checkBoxSendDataBroadcast = FindViewById<CheckBox>(Resource.Id.checkBoxSendDataBroadcast);

            _textViewCaptionCpuUsage = FindViewById<TextView>(Resource.Id.textViewCaptionCpuUsage);
            _checkBoxCheckCpuUsage = FindViewById<CheckBox>(Resource.Id.checkBoxCheckCpuUsage);
            ViewStates viewStateCpuUsage = ActivityCommon.IsCpuStatisticsSupported() ? ViewStates.Visible : ViewStates.Gone;
            _textViewCaptionCpuUsage.Visibility = viewStateCpuUsage;
            _checkBoxCheckCpuUsage.Visibility = viewStateCpuUsage;

            _checkBoxCheckEcuFiles = FindViewById<CheckBox>(Resource.Id.checkBoxCheckEcuFiles);
            _checkBoxOldVagMode = FindViewById<CheckBox>(Resource.Id.checkBoxOldVagMode);

            _buttonStorageLocation = FindViewById<Button>(Resource.Id.buttonStorageLocation);
            _buttonStorageLocation.Click += (sender, args) =>
            {
                SelectMedia();
            };

            _checkBoxCollectDebugInfo = FindViewById<CheckBox>(Resource.Id.checkBoxCollectDebugInfo);

            ViewStates viewStateSnoopLog = _activityCommon.GetConfigHciSnoopLog(out bool _) ? ViewStates.Visible : ViewStates.Gone;
            _checkBoxHciSnoopLog = FindViewById<CheckBox>(Resource.Id.checkBoxHciSnoopLog);
            _checkBoxHciSnoopLog.Visibility = viewStateSnoopLog;
            _checkBoxHciSnoopLog.Enabled = false;

            _buttonHciSnoopLog = FindViewById<Button>(Resource.Id.buttonHciSnoopLog);
            _buttonHciSnoopLog.Visibility = viewStateSnoopLog;
            _buttonHciSnoopLog.Click += (sender, args) =>
            {
                ShowDevelopmentSettings();
            };

            ReadSettings();
            CheckSelection(_selection);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _activityCommon.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            StoreSettings();
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    StoreSettings();
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest) requestCode)
            {
                case ActivityRequest.RequestDevelopmentSettings:
                    UpdateDisplay();
                    break;
            }
        }

        private void ReadSettings()
        {
            switch (ActivityCommon.BtEnbaleHandling)
            {
                case ActivityCommon.BtEnableType.Ask:
                    _radioButtonAskForBtEnable.Checked = true;
                    break;

                case ActivityCommon.BtEnableType.Always:
                    _radioButtonAlwaysEnableBt.Checked = true;
                    break;

                default:
                    _radioButtonNoBtHandling.Checked = true;
                    break;
            }
            _checkBoxDisableBtAtExit.Checked = ActivityCommon.BtDisableHandling == ActivityCommon.BtDisableType.DisableIfByApp;

            switch (ActivityCommon.LockTypeCommunication)
            {
                case ActivityCommon.LockType.None:
                    _radioButtonCommLockNone.Checked = true;
                    break;

                case ActivityCommon.LockType.Cpu:
                    _radioButtonCommLockCpu.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenDim:
                    _radioButtonCommLockDim.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenBright:
                    _radioButtonCommLockBright.Checked = true;
                    break;
            }

            switch (ActivityCommon.LockTypeLogging)
            {
                case ActivityCommon.LockType.None:
                    _radioButtonLogLockNone.Checked = true;
                    break;

                case ActivityCommon.LockType.Cpu:
                    _radioButtonLogLockCpu.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenDim:
                    _radioButtonLogLockDim.Checked = true;
                    break;

                case ActivityCommon.LockType.ScreenBright:
                    _radioButtonLogLockBright.Checked = true;
                    break;
            }

            _checkBoxStoreDataLogSettings.Checked = ActivityCommon.StoreDataLogSettings;

            switch (ActivityCommon.AutoConnectHandling)
            {
                case ActivityCommon.AutoConnectType.Offline:
                    _radioButtonStartOffline.Checked = true;
                    break;

                case ActivityCommon.AutoConnectType.Connect:
                    _radioButtonStartConnect.Checked = true;
                    break;

                case ActivityCommon.AutoConnectType.ConnectClose:
                    _radioButtonStartConnectClose.Checked = true;
                    break;
            }

            _checkBoxDoubleClickForAppExit.Checked = ActivityCommon.DoubleClickForAppExit;
            _checkBoxSendDataBroadcast.Checked = ActivityCommon.SendDataBroadcast;
            _checkBoxCheckCpuUsage.Checked = ActivityCommon.CheckCpuUsage;
            _checkBoxCheckEcuFiles.Checked = ActivityCommon.CheckEcuFiles;
            _checkBoxOldVagMode.Checked = ActivityCommon.OldVagMode;
            _checkBoxCollectDebugInfo.Checked = ActivityCommon.CollectDebugInfo;
            UpdateDisplay();
        }

        private void StoreSettings()
        {
            ActivityCommon.BtEnableType enableType = ActivityCommon.BtEnbaleHandling;
            if (_radioButtonAskForBtEnable.Checked)
            {
                enableType = ActivityCommon.BtEnableType.Ask;
            }
            else if (_radioButtonAlwaysEnableBt.Checked)
            {
                enableType = ActivityCommon.BtEnableType.Always;
            }
            else if (_radioButtonNoBtHandling.Checked)
            {
                enableType = ActivityCommon.BtEnableType.Nothing;
            }
            ActivityCommon.BtEnbaleHandling = enableType;

            ActivityCommon.BtDisableHandling = _checkBoxDisableBtAtExit.Checked ? ActivityCommon.BtDisableType.DisableIfByApp : ActivityCommon.BtDisableType.Nothing;

            ActivityCommon.LockType lockType = ActivityCommon.LockTypeCommunication;
            if (_radioButtonCommLockNone.Checked)
            {
                lockType = ActivityCommon.LockType.None;
            }
            else if(_radioButtonCommLockCpu.Checked)
            {
                lockType = ActivityCommon.LockType.Cpu;
            }
            else if (_radioButtonCommLockDim.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenDim;
            }
            else if (_radioButtonCommLockBright.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenBright;
            }
            ActivityCommon.LockTypeCommunication = lockType;

            lockType = ActivityCommon.LockTypeLogging;
            if (_radioButtonLogLockNone.Checked)
            {
                lockType = ActivityCommon.LockType.None;
            }
            else if (_radioButtonLogLockCpu.Checked)
            {
                lockType = ActivityCommon.LockType.Cpu;
            }
            else if (_radioButtonLogLockDim.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenDim;
            }
            else if (_radioButtonLogLockBright.Checked)
            {
                lockType = ActivityCommon.LockType.ScreenBright;
            }
            ActivityCommon.LockTypeLogging = lockType;

            ActivityCommon.StoreDataLogSettings = _checkBoxStoreDataLogSettings.Checked;

            ActivityCommon.AutoConnectType autoConnectType = ActivityCommon.AutoConnectType.Offline;
            if (_radioButtonStartOffline.Checked)
            {
                autoConnectType = ActivityCommon.AutoConnectType.Offline;
            }
            else if (_radioButtonStartConnect.Checked)
            {
                autoConnectType = ActivityCommon.AutoConnectType.Connect;
            }
            else if (_radioButtonStartConnectClose.Checked)
            {
                autoConnectType = ActivityCommon.AutoConnectType.ConnectClose;
            }
            ActivityCommon.AutoConnectHandling = autoConnectType;

            ActivityCommon.DoubleClickForAppExit = _checkBoxDoubleClickForAppExit.Checked;
            ActivityCommon.SendDataBroadcast = _checkBoxSendDataBroadcast.Checked;
            ActivityCommon.CheckCpuUsage = _checkBoxCheckCpuUsage.Checked;
            ActivityCommon.CheckEcuFiles = _checkBoxCheckEcuFiles.Checked;
            ActivityCommon.OldVagMode = _checkBoxOldVagMode.Checked;
            ActivityCommon.CollectDebugInfo = _checkBoxCollectDebugInfo.Checked;
        }

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }

            string displayName = GetString(Resource.String.default_media);
            if (!string.IsNullOrEmpty(_activityCommon.CustomStorageMedia))
            {
                string shortName = ActivityCommon.GetTruncatedPathName(_activityCommon.CustomStorageMedia);
                if (!string.IsNullOrEmpty(shortName))
                {
                    displayName = shortName;
                }
            }
            _buttonStorageLocation.Text = displayName;

            bool snoopLogEnabled = false;
            if (_activityCommon.GetConfigHciSnoopLog(out bool enabledConfig))
            {
                snoopLogEnabled = enabledConfig;
            }
            if (ActivityCommon.ReadHciSnoopLogSettings(out bool enabledSettings, out string logFileName))
            {
                if (!enabledSettings)
                {
                    snoopLogEnabled = false;
                }
            }
            _checkBoxHciSnoopLog.Checked = snoopLogEnabled;
            _checkBoxHciSnoopLog.Text = string.Format(GetString(Resource.String.settings_hci_snoop_log), logFileName ?? "-");
        }

        private void SelectMedia()
        {
            // ReSharper disable once UseNullPropagation
            if (_activityCommon == null)
            {
                return;
            }
            _activityCommon.SelectMedia((s, a) =>
            {
                UpdateDisplay();
            });
        }

        private void CheckSelection(string selection)
        {
            if (selection == null)
            {
                return;
            }
            switch (selection)
            {
                case SelectionStorageLocation:
                    SelectMedia();
                    break;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ShowDevelopmentSettings()
        {
            try
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionApplicationDevelopmentSettings);
                StartActivityForResult(intent, (int)ActivityRequest.RequestDevelopmentSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
