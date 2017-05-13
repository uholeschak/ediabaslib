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
        // Intent extra
        public const string ExtraSelection = "selection";
        public const string SelectionStorageLocation = "storage_location";

        private string _selection;
        private ActivityCommon _activityCommon;
        private RadioButton _radioButtonAskForBtEnable;
        private RadioButton _radioButtonAlwaysEnableBt;
        private RadioButton _radioButtonNoBtHandling;
        private CheckBox _checkBoxDisableBtAtExit;
        private CheckBox _checkBoxStoreDataLogSettings;
        private Button _buttonStorageLocation;

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

            _radioButtonAskForBtEnable = FindViewById<RadioButton>(Resource.Id.radioButtonAskForBtEnable);
            _radioButtonAlwaysEnableBt = FindViewById<RadioButton>(Resource.Id.radioButtonAlwaysEnableBt);
            _radioButtonNoBtHandling = FindViewById<RadioButton>(Resource.Id.radioButtonNoBtHandling);
            _checkBoxDisableBtAtExit = FindViewById<CheckBox>(Resource.Id.checkBoxDisableBtAtExit);
            _checkBoxStoreDataLogSettings = FindViewById<CheckBox>(Resource.Id.checkBoxStoreDataLogSettings);
            _buttonStorageLocation = FindViewById<Button>(Resource.Id.buttonStorageLocation);
            _buttonStorageLocation.Click += (sender, args) =>
            {
                SelectMedia();
            };

            ReadSettings();
            CheckSelection(_selection);
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
            UpdateDisplay();
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

        private void UpdateDisplay()
        {
            const int maxLength = 40;
            string displayName = string.IsNullOrEmpty(_activityCommon.CustomStorageMedia) ? GetString(Resource.String.default_media) : _activityCommon.CustomStorageMedia;
            if (displayName.Length > maxLength)
            {
                displayName = "..." + displayName.Substring(displayName.Length - maxLength);
            }
            _buttonStorageLocation.Text = displayName;
        }

        private void SelectMedia()
        {
            _activityCommon.SelectMedia((s, a) =>
            {
                UpdateDisplay();
            });
        }

        private void CheckSelection(string selection)
        {
            switch (selection)
            {
                case SelectionStorageLocation:
                    SelectMedia();
                    break;
            }
        }

    }
}
