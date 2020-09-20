using System;
using Android.Content;
using Android.OS;
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
        public const string ExtraServiceId = "service_id";
        public const string ExtraArguments = "arguments";

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

            _serviceId = Intent.GetIntExtra(ExtraServiceId, 0);
            _defaultArguments = Intent.GetStringExtra(ExtraArguments);

            _activityCommon = new ActivityCommon(this);

            _radioButtonArgTypeArg = FindViewById<RadioButton>(Resource.Id.radioButtonArgTypeArg);
            _radioButtonArgTypeId = FindViewById<RadioButton>(Resource.Id.radioButtonArgTypeId);

            _listViewArgs = FindViewById<ListView>(Resource.Id.argList);
            _argsListAdapter = new ResultListAdapter(this);
            _listViewArgs.Adapter = _argsListAdapter;
            _listViewArgs.SetOnTouchListener(this);

            UpdateDisplay();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            UpdateResult();
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (UpdateResult())
                    {
                        Finish();
                    }
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

            try
            {
                string argType = string.Empty;
                if (!string.IsNullOrEmpty(_defaultArguments))
                {
                    string[] argArray = _defaultArguments.Split(";");
                    if (argArray.Length > 0)
                    {
                        argType = argArray[0].Trim();
                    }
                }

                switch (argType.ToUpperInvariant())
                {
                    case EdiabasToolActivity.ArgTypeID:
                        _radioButtonArgTypeId.Checked = true;
                        break;

                    default:
                        _radioButtonArgTypeArg.Checked = true;
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool UpdateResult()
        {
            try
            {
                string argType = EdiabasToolActivity.ArgTypeArg;
                if (_radioButtonArgTypeId.Checked)
                {
                    argType = EdiabasToolActivity.ArgTypeID;
                }

                string arguments = argType;
                Intent intent = new Intent();
                intent.PutExtra(ExtraArguments, arguments);
                SetResult(Android.App.Result.Ok, intent);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
