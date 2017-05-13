using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/settings_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class GlobalSettingsActivity : AppCompatActivity
    {
        private ActivityCommon _activityCommon;
        private RadioButton _radioButtonAskForBtEnable;
        private RadioButton _radioButtonAlwaysEnableBt;
        private RadioButton _radioButtonNoBtHandling;
        private CheckBox _checkBoxDisableBtAtExit;
        private CheckBox _checkBoxStoreDataLogSettings;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.settings);

            SetResult(Android.App.Result.Canceled);

            _activityCommon = new ActivityCommon(this);

            _radioButtonAskForBtEnable = FindViewById<RadioButton>(Resource.Id.radioButtonAskForBtEnable);
            _radioButtonAlwaysEnableBt = FindViewById<RadioButton>(Resource.Id.radioButtonAlwaysEnableBt);
            _radioButtonNoBtHandling = FindViewById<RadioButton>(Resource.Id.radioButtonNoBtHandling);
            _checkBoxDisableBtAtExit = FindViewById<CheckBox>(Resource.Id.checkBoxDisableBtAtExit);
            _checkBoxStoreDataLogSettings = FindViewById<CheckBox>(Resource.Id.checkBoxStoreDataLogSettings);

            ReadSettings();
        }

        protected override void OnDestroy()
        {
            StoreSettings();

            base.OnDestroy();
            _activityCommon.Dispose();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
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
            _checkBoxStoreDataLogSettings.Checked = ActivityCommon.StoreDataLogSettings;
        }

        private void StoreSettings()
        {
            if (_radioButtonAskForBtEnable.Checked)
            {
                ActivityCommon.BtEnbaleHandling = ActivityCommon.BtEnableType.Ask;
            }
            else if (_radioButtonAlwaysEnableBt.Checked)
            {
                ActivityCommon.BtEnbaleHandling = ActivityCommon.BtEnableType.Always;
            }
            else
            {
                ActivityCommon.BtEnbaleHandling = ActivityCommon.BtEnableType.Nothing;
            }

            ActivityCommon.BtDisableHandling = _checkBoxDisableBtAtExit.Checked ? ActivityCommon.BtDisableType.DisableIfByApp : ActivityCommon.BtDisableType.Nothing;
            ActivityCommon.StoreDataLogSettings = _checkBoxStoreDataLogSettings.Checked;
        }
    }
}
