using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using EdiabasLib;

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
            public bool ArgsAmountWarnShown { get; set; }
        }

        public delegate void AcceptDelegate(bool accepted);

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
        private bool _dynamicId;
        private Button _buttonApply;
        private Button _buttonExecute;
        private RadioButton _radioButtonArgTypeArg;
        private RadioButton _radioButtonArgTypeId;
        private LinearLayout _layoutBlockNumber;
        private Spinner _spinnerBlockNumber;
        private StringObjAdapter _spinnerBlockNumberAdapter;
        private CheckBox _checkBoxDefineBlockNew;
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
            _dynamicId = _serviceId == (int) EdiabasToolActivity.UdsServiceId.DynamicallyDefineId;
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
                if (ArgsSelectCount() > 0 && UpdateResult())
                {
                    Finish();
                }
            };

            _buttonExecute = _barView.FindViewById<Button>(Resource.Id.buttonExecute);
            _buttonExecute.SetOnTouchListener(this);
            _buttonExecute.Click += (sender, args) =>
            {
                if (ArgsSelectCount() > 0 && UpdateResult(true))
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

            _layoutBlockNumber = FindViewById<LinearLayout>(Resource.Id.layoutBlockNumber);
            _layoutBlockNumber.SetOnTouchListener(this);
            _layoutBlockNumber.Visibility = _dynamicId ? ViewStates.Visible : ViewStates.Gone;

            _spinnerBlockNumber = FindViewById<Spinner>(Resource.Id.spinnerBlockNumber);
            _spinnerBlockNumberAdapter = new StringObjAdapter(this);
            _spinnerBlockNumber.Adapter = _spinnerBlockNumberAdapter;

            _checkBoxDefineBlockNew = FindViewById<CheckBox>(Resource.Id.checkBoxDefineBlockNew);
            _checkBoxDefineBlockNew.SetOnTouchListener(this);

            _listViewArgs = FindViewById<ListView>(Resource.Id.argList);
            _argsListAdapter = new EdiabasToolActivity.ResultSelectListAdapter(this);
            _argsListAdapter.CheckChanged += extraInfo =>
            {
                UpdateButtonState();
                CheckArgsAmount();
            };

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
            if (!StoreChangesRequest(accepted =>
            {
                if (accepted)
                {
                    UpdateResult();
                }

                base.OnBackPressed();
            }))
            {
                base.OnBackPressed();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (!StoreChangesRequest(accepted =>
                    {
                        if (accepted)
                        {
                            UpdateResult();
                        }

                        Finish();
                    }))
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
                string blockNumber = string.Empty;
                string defineBlockNew = string.Empty;
                string argType = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.Arguments))
                {
                    string[] argArray = _instanceData.Arguments.Split(";");
                    if (_dynamicId)
                    {
                        if (argArray.Length > 2)
                        {
                            blockNumber = argArray[0].Trim();
                            defineBlockNew = argArray[1].Trim();
                            argType = argArray[2].Trim();
                            selectList = argArray.ToList();
                            selectList.RemoveRange(0, 3);
                        }
                    }
                    else
                    {
                        if (argArray.Length > 0)
                        {
                            argType = argArray[0].Trim();
                            selectList = argArray.ToList();
                            selectList.RemoveAt(0);
                        }
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

                if (_dynamicId)
                {
                    Int64 numberValue = EdiabasNet.StringToValue(blockNumber, out bool valid);
                    if (!valid)
                    {
                        numberValue = 0;
                    }

                    int selection = 0;
                    int index = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        _spinnerBlockNumberAdapter.Items.Add(new StringObjType(string.Format(CultureInfo.InvariantCulture, "{0}", i), i));
                        if (i == numberValue)
                        {
                            selection = index;
                        }

                        index++;
                    }
                    _spinnerBlockNumberAdapter.NotifyDataSetChanged();
                    _spinnerBlockNumber.SetSelection(selection);

                    bool newBlock = string.IsNullOrEmpty(defineBlockNew) ||
                        string.Compare(defineBlockNew, "JA", StringComparison.OrdinalIgnoreCase) == 0 ||
                        string.Compare(defineBlockNew, "YES", StringComparison.OrdinalIgnoreCase) == 0;
                    _checkBoxDefineBlockNew.Checked = newBlock;
                }

                UpdateArgList(selectList);
                UpdateButtonState();
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
                            string info = funcInfo.InfoTrans ?? funcInfo.Info;
                            EdiabasToolActivity.ExtraInfo extraInfo = new EdiabasToolActivity.ExtraInfo(name, string.Empty, new List<string> { info });
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

        private bool StoreChangesRequest(AcceptDelegate handler)
        {
            if (ArgsSelectCount() <= 0)
            {
                return false;
            }

            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    handler(true);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    handler(false);
                })
                .SetMessage(Resource.String.arg_assist_apply_args)
                .SetTitle(Resource.String.alert_title_question)
                .Show();

            return true;
        }

        private void UpdateButtonState()
        {
            bool enable = ArgsSelectCount() > 0;
            _buttonApply.Enabled = enable;
            _buttonExecute.Enabled = enable && !_offline;
        }

        private void CheckArgsAmount()
        {
            if (!_instanceData.ArgsAmountWarnShown)
            {
                if (ArgsSelectCount() > 10)
                {
                    _instanceData.ArgsAmountWarnShown = true;
                    _activityCommon.ShowAlert(GetString(Resource.String.arg_assist_amount_limit), Resource.String.alert_title_warning);
                }
            }
        }

        private int ArgsSelectCount()
        {
            try
            {
                int count = 0;
                foreach (EdiabasToolActivity.ExtraInfo extraInfo in _argsListAdapter.Items)
                {
                    if (extraInfo.Selected)
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception)
            {
                return 0;
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
                if (_dynamicId)
                {
                    int blockNumber = 0;
                    int position = _spinnerBlockNumber.SelectedItemPosition;
                    if (position >= 0 && position < _spinnerBlockNumberAdapter.Items.Count)
                    {
                        StringObjType item = _spinnerBlockNumberAdapter.Items[position];
                        blockNumber = (int) item.Data;
                    }

                    sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}", blockNumber));
                    sb.Append(";");

                    sb.Append(_checkBoxDefineBlockNew.Checked ? "YES" : "NO");
                    sb.Append(";");
                }

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
