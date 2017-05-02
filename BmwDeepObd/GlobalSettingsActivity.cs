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
                case ActivityCommon.BtEnbleType.Ask:
                    _radioButtonAskForBtEnable.Checked = true;
                    break;

                case ActivityCommon.BtEnbleType.Always:
                    _radioButtonAlwaysEnableBt.Checked = true;
                    break;

                default:
                    _radioButtonNoBtHandling.Checked = true;
                    break;
            }
            _checkBoxDisableBtAtExit.Checked = ActivityCommon.BtDisableHandling == ActivityCommon.BtDisableType.DisableIfByApp;
        }

        private void StoreSettings()
        {
            if (_radioButtonAskForBtEnable.Checked)
            {
                ActivityCommon.BtEnbaleHandling = ActivityCommon.BtEnbleType.Ask;
            }
            else if (_radioButtonAlwaysEnableBt.Checked)
            {
                ActivityCommon.BtEnbaleHandling = ActivityCommon.BtEnbleType.Always;
            }
            else
            {
                ActivityCommon.BtEnbaleHandling = ActivityCommon.BtEnbleType.Nothing;
            }

            ActivityCommon.BtDisableHandling = _checkBoxDisableBtAtExit.Checked ? ActivityCommon.BtDisableType.DisableIfByApp : ActivityCommon.BtDisableType.Nothing;
        }
    }
}
