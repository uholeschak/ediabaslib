using System;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/arg_assist_stat_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class ArgAssistStatActivity : BaseActivity, View.IOnTouchListener
    {
        // Intent extra
        public const string ServiceId = "service_id";
        public const string Arguments = "arguments";

        private InputMethodManager _imm;
        private View _contentView;
        private ActivityCommon _activityCommon;

        private int _serviceId;
        private string _defaultArguments;
        private RadioButton _radioButtonArgTypeArg;
        private RadioButton _radioButtonArgTypeId;
        private ListView _listViewArgs;
        private ResultListAdapter _argsListAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.arg_assist_status);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            _serviceId = Intent.GetIntExtra(ServiceId, 0);
            _defaultArguments = Intent.GetStringExtra(Arguments);

            _activityCommon = new ActivityCommon(this);

            _radioButtonArgTypeArg = FindViewById<RadioButton>(Resource.Id.radioButtonArgTypeArg);
            _radioButtonArgTypeId = FindViewById<RadioButton>(Resource.Id.radioButtonArgTypeId);

            _listViewArgs = FindViewById<ListView>(Resource.Id.argList);
            _argsListAdapter = new ResultListAdapter(this);
            _listViewArgs.Adapter = _argsListAdapter;
            _listViewArgs.SetOnTouchListener(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
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

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    HideKeyboard();
                    break;
            }
            return false;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }
        }
    }
}
