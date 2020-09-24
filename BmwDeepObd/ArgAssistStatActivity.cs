using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public class InstanceData
        {
            public string Arguments { get; set; }
        }

        // Intent extra
        public const string ExtraServiceId = "service_id";
        public const string ExtraOffline = "offline";
        public const string ExtraArguments = "arguments";
        public const string ExtraExecute = "execute";

        public static List<EdiabasToolActivity.SgFuncInfo> IntentSgFuncInfo { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private InputMethodManager _imm;
        private View _contentView;
        private View _barView;
        private ActivityCommon _activityCommon;

        private int _serviceId;
        private bool _offline;
        private Button _buttonApply;
        private Button _buttonExecute;
        private RadioButton _radioButtonArgTypeArg;
        private RadioButton _radioButtonArgTypeId;
        private ListView _listViewArgs;
        private EdiabasToolActivity.ResultSelectListAdapter _argsListAdapter;
        private List<EdiabasToolActivity.SgFuncInfo> _sgFuncInfoList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.arg_assist_status);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            _barView = LayoutInflater.Inflate(Resource.Layout.bar_arg_assist, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                                      (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                                      (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(_barView, barLayoutParams);

            SetResult(Android.App.Result.Canceled);

            if (IntentSgFuncInfo == null)
            {
                Finish();
                return;
            }

            _serviceId = Intent.GetIntExtra(ExtraServiceId, -1);
            _offline = Intent.GetBooleanExtra(ExtraOffline, false);
            if (!_activityRecreated && _instanceData != null)
            {
                _instanceData.Arguments = Intent.GetStringExtra(ExtraArguments);
            }

            _activityCommon = new ActivityCommon(this);

            _sgFuncInfoList = IntentSgFuncInfo;

            _buttonApply = _barView.FindViewById<Button>(Resource.Id.buttonApply);
            _buttonApply.SetOnTouchListener(this);
            _buttonApply.Click += (sender, args) =>
            {
                if (ArgsSelected() && UpdateResult())
                {
                    Finish();
                }
            };

            _buttonExecute = _barView.FindViewById<Button>(Resource.Id.buttonExecute);
            _buttonExecute.SetOnTouchListener(this);
            _buttonExecute.Enabled = !_offline;
            _buttonExecute.Click += (sender, args) =>
            {
                if (ArgsSelected() && UpdateResult(true))
                {
                    Finish();
                }
            };

            _radioButtonArgTypeArg = FindViewById<RadioButton>(Resource.Id.radioButtonArgTypeArg);
            _radioButtonArgTypeArg.CheckedChange += (sender, args) =>
            {
                UpdateArgList();
            };

            _radioButtonArgTypeId = FindViewById<RadioButton>(Resource.Id.radioButtonArgTypeId);
            _radioButtonArgTypeId.CheckedChange += (sender, args) =>
            {
                UpdateArgList();
            };

            _listViewArgs = FindViewById<ListView>(Resource.Id.argList);
            _argsListAdapter = new EdiabasToolActivity.ResultSelectListAdapter(this);
            _listViewArgs.Adapter = _argsListAdapter;
            _listViewArgs.SetOnTouchListener(this);

            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _instanceData.Arguments = GetArgString();
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            if (ArgsSelected())
            {
                UpdateResult();
            }
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (ArgsSelected() && UpdateResult())
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
                List<string> selectList = null;
                string argType = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.Arguments))
                {
                    string[] argArray = _instanceData.Arguments.Split(";");
                    if (argArray.Length > 0)
                    {
                        argType = argArray[0].Trim();
                    }

                    if (argArray.Length > 0)
                    {
                        selectList = argArray.ToList();
                        selectList.RemoveAt(0);
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

                UpdateArgList(selectList);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateArgList(List<string> selectList = null)
        {
            try
            {
                bool argTypeId = _radioButtonArgTypeId.Checked;

                _argsListAdapter.Items.Clear();
                if (_serviceId >= 0)
                {
                    foreach (EdiabasToolActivity.SgFuncInfo funcInfo in _sgFuncInfoList.OrderBy(x => argTypeId ? x.Id : x.Arg))
                    {
                        if (funcInfo.ServiceList.Contains(_serviceId))
                        {
                            string name = argTypeId ? funcInfo.Id : funcInfo.Arg;
                            EdiabasToolActivity.ExtraInfo extraInfo = new EdiabasToolActivity.ExtraInfo(name, string.Empty, new List<string> { funcInfo.Info });
                            if (selectList != null)
                            {
                                if (selectList.Contains(name))
                                {
                                    extraInfo.Selected = true;
                                }
                            }
                            _argsListAdapter.Items.Add(extraInfo);
                        }
                    }
                }

                _argsListAdapter.NotifyDataSetChanged();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool ArgsSelected()
        {
            try
            {
                foreach (EdiabasToolActivity.ExtraInfo extraInfo in _argsListAdapter.Items)
                {
                    if (extraInfo.Selected)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetArgString()
        {
            try
            {
                string argType = EdiabasToolActivity.ArgTypeArg;
                if (_radioButtonArgTypeId.Checked)
                {
                    argType = EdiabasToolActivity.ArgTypeID;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(argType);
                foreach (EdiabasToolActivity.ExtraInfo extraInfo in _argsListAdapter.Items)
                {
                    if (extraInfo.Selected)
                    {
                        sb.Append(";");
                        sb.Append(extraInfo.Name);
                    }
                }

                return sb.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private bool UpdateResult(bool execute = false)
        {
            try
            {
                Intent intent = new Intent();
                intent.PutExtra(ExtraArguments, GetArgString());
                intent.PutExtra(ExtraExecute, execute);
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
