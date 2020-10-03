using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/arg_assist_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class ArgAssistControlActivity : ArgAssistBaseActivity
    {
        public class InstanceData
        {
            public string Arguments { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _controlRoutine;
        private bool _controlIo;

        private ScrollView _scrollViewArgAssist;
        private LinearLayout _layoutArgAssist;
        private Spinner _spinnerArgument;
        private EdiabasToolActivity.ResultSelectListAdapter _spinnerArgumentAdapter;
        private TextView _textViewControlParam;
        private RadioGroup _radioGroupControlRoutine;
        private RadioButton _radioButtonStr;
        private RadioButton _radioButtonStpr;
        private RadioButton _radioButtonRrr;
        private RadioGroup _radioGroupControlIo;
        private RadioButton _radioButtonRctEcu;
        private RadioButton _radioButtonRtd;
        private RadioButton _radioButtonFcs;
        private RadioButton _radioButtonSta;
        private LinearLayout _layoutArgParams;

        protected override void OnCreate(Bundle savedInstanceState)
        {
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
            SetContentView(Resource.Layout.arg_assist_control);

            InitBaseVariables();

            _controlRoutine = _serviceId == (int)EdiabasToolActivity.UdsServiceId.RoutineControl;
            _controlIo = _serviceId == (int)EdiabasToolActivity.UdsServiceId.IoControlById;

            _scrollViewArgAssist = FindViewById<ScrollView>(Resource.Id.scrollViewArgAssist);
            _scrollViewArgAssist.SetOnTouchListener(this);

            _layoutArgAssist = FindViewById<LinearLayout>(Resource.Id.layoutArgAssist);
            _layoutArgAssist.SetOnTouchListener(this);

            _spinnerArgument = FindViewById<Spinner>(Resource.Id.spinnerArgument);
            _spinnerArgument.SetOnTouchListener(this);
            _spinnerArgumentAdapter = new EdiabasToolActivity.ResultSelectListAdapter(this);
            _spinnerArgument.Adapter = _spinnerArgumentAdapter;
            _spinnerArgument.ItemSelected += (sender, args) =>
            {
                UpdateArgParams();
            };

            _textViewControlParam = FindViewById<TextView>(Resource.Id.textViewControlParam);
            _textViewControlParam.Visibility = _controlRoutine || _controlIo ? ViewStates.Visible : ViewStates.Gone;

            _radioGroupControlRoutine = FindViewById<RadioGroup>(Resource.Id.radioGroupControlRoutine);
            _radioGroupControlRoutine.Visibility = _controlRoutine ? ViewStates.Visible : ViewStates.Gone;
            _radioGroupControlRoutine.SetOnTouchListener(this);
            _radioButtonStr = FindViewById<RadioButton>(Resource.Id.radioButtonStr);
            _radioButtonStr.SetOnTouchListener(this);
            _radioButtonStpr = FindViewById<RadioButton>(Resource.Id.radioButtonStpr);
            _radioButtonStpr.SetOnTouchListener(this);
            _radioButtonRrr = FindViewById<RadioButton>(Resource.Id.radioButtonRrr);
            _radioButtonRrr.SetOnTouchListener(this);

            _radioGroupControlIo = FindViewById<RadioGroup>(Resource.Id.radioGroupControlIo);
            _radioGroupControlIo.Visibility = _controlIo ? ViewStates.Visible : ViewStates.Gone;
            _radioGroupControlIo.SetOnTouchListener(this);
            _radioButtonRctEcu = FindViewById<RadioButton>(Resource.Id.radioButtonRctEcu);
            _radioButtonRctEcu.SetOnTouchListener(this);
            _radioButtonRtd = FindViewById<RadioButton>(Resource.Id.radioButtonRtd);
            _radioButtonRtd.SetOnTouchListener(this);
            _radioButtonFcs = FindViewById<RadioButton>(Resource.Id.radioButtonFcs);
            _radioButtonFcs.SetOnTouchListener(this);
            _radioButtonSta = FindViewById<RadioButton>(Resource.Id.radioButtonSta);
            _radioButtonSta.SetOnTouchListener(this);

            _layoutArgParams = FindViewById<LinearLayout>(Resource.Id.layoutArgParams);
            _layoutArgParams.SetOnTouchListener(this);

            if (!_activityRecreated && _instanceData != null)
            {
                _instanceData.Arguments = Intent.GetStringExtra(ExtraArguments);
            }

            _buttonApply.Click += (sender, args) =>
            {
                if (ArgsValid() && UpdateResult())
                {
                    Finish();
                }
            };

            _buttonExecute.Click += (sender, args) =>
            {
                if (ArgsValid() && UpdateResult(true))
                {
                    Finish();
                }
            };

            _radioButtonArgTypeArg.CheckedChange += (sender, args) =>
            {
                UpdateArgList();
            };

            _radioButtonArgTypeId.CheckedChange += (sender, args) =>
            {
                UpdateArgList();
            };

            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _instanceData.Arguments = GetArgString();
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
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

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }

            try
            {
                string selectArg = null;
                string argType = string.Empty;
                string controlParam = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.Arguments))
                {
                    string[] argArray = _instanceData.Arguments.Split(";");
                    if (argArray.Length > 0)
                    {
                        argType = argArray[0].Trim();
                    }

                    if (argArray.Length > 1)
                    {
                        selectArg = argArray[1].Trim();
                    }

                    if (_controlRoutine || _controlIo)
                    {
                        if (argArray.Length > 2)
                        {
                            controlParam = argArray[2].Trim();
                        }
                    }
                }

                switch (argType.ToUpperInvariant())
                {
                    case ArgTypeID:
                        _radioButtonArgTypeId.Checked = true;
                        break;

                    default:
                        _radioButtonArgTypeArg.Checked = true;
                        break;
                }

                if (_controlRoutine)
                {
                    switch (controlParam.ToUpperInvariant())
                    {
                        case "STPR":
                            _radioButtonStpr.Checked = true;
                            break;

                        case "RRR":
                            _radioButtonRrr.Checked = true;
                            break;

                        default:
                            _radioButtonStr.Checked = true;
                            break;
                    }
                }

                if (_controlIo)
                {
                    switch (controlParam.ToUpperInvariant())
                    {
                        case "RTD":
                            _radioButtonRtd.Checked = true;
                            break;

                        case "FCS":
                            _radioButtonFcs.Checked = true;
                            break;

                        case "STA":
                            _radioButtonSta.Checked = true;
                            break;

                        default:
                            _radioButtonRctEcu.Checked = true;
                            break;
                    }
                }

                UpdateArgList(selectArg);
                UpdateArgParams();
                UpdateButtonState();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateArgList(string selectArg = null)
        {
            try
            {
                bool argTypeId = _radioButtonArgTypeId.Checked;

                int selection = 0;
                int index = 0;
                _spinnerArgumentAdapter.Items.Clear();
                if (_serviceId >= 0)
                {
                    foreach (EdiabasToolActivity.SgFuncInfo funcInfo in _sgFuncInfoList.OrderBy(x => argTypeId ? x.Id : x.Arg))
                    {
                        if (funcInfo.ServiceList.Contains(_serviceId))
                        {
                            string name = argTypeId ? funcInfo.Id : funcInfo.Arg;
                            string info = funcInfo.InfoTrans ?? funcInfo.Info;
                            EdiabasToolActivity.ExtraInfo extraInfo = new EdiabasToolActivity.ExtraInfo(name, string.Empty, new List<string> { info })
                            {
                                CheckVisible = false,
                                Tag = funcInfo
                            };
                            _spinnerArgumentAdapter.Items.Add(extraInfo);

                            if (selectArg != null)
                            {
                                if (string.Compare(name, selectArg, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    selection = index;
                                }
                            }

                            index++;
                        }
                    }
                }

                _spinnerArgumentAdapter.NotifyDataSetChanged();
                _spinnerArgument.SetSelection(selection);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateArgParams()
        {
            try
            {
                _layoutArgParams.RemoveAllViews();
                Android.Content.Res.ColorStateList captionTextColors = _textViewArgTypeTitle.TextColors;
                Drawable captionTextBackground = _textViewArgTypeTitle.Background;
                int position = _spinnerArgument.SelectedItemPosition;
                if (position >= 0 && position < _spinnerArgumentAdapter.Items.Count)
                {
                    EdiabasToolActivity.ExtraInfo item = _spinnerArgumentAdapter.Items[position];
                    if (item.Tag is EdiabasToolActivity.SgFuncInfo funcInfo)
                    {
                        if (funcInfo.ArgInfoList != null)
                        {
                            foreach (EdiabasToolActivity.SgFuncArgInfo funcArgInfo in funcInfo.ArgInfoList)
                            {
                                LinearLayout argLayout = new LinearLayout(this);
                                argLayout.Orientation = Orientation.Vertical;

                                LinearLayout.LayoutParams wrapLayoutParams = new LinearLayout.LayoutParams(
                                    ViewGroup.LayoutParams.MatchParent,
                                    ViewGroup.LayoutParams.WrapContent);

                                TextView textViewCaption = new TextView(this);
                                textViewCaption.SetTextColor(captionTextColors);
                                textViewCaption.Background = captionTextBackground;

                                StringBuilder sbCaption = new StringBuilder();
                                sbCaption.Append(GetString(Resource.String.arg_assist_control_parameter));
                                sbCaption.Append(": ");
                                sbCaption.Append(funcArgInfo.Arg);
                                textViewCaption.Text = sbCaption.ToString();
                                argLayout.AddView(textViewCaption, wrapLayoutParams);

                                TextView textViewDesc = new TextView(this);

                                StringBuilder sbDesc = new StringBuilder();
                                if (!string.IsNullOrEmpty(funcArgInfo.Info))
                                {
                                    sbDesc.Append(funcArgInfo.Info);
                                }
                                if (!string.IsNullOrEmpty(funcArgInfo.Unit))
                                {
                                    if (sbDesc.Length > 0)
                                    {
                                        sbDesc.Append("\r\n");
                                    }
                                    sbDesc.Append("[");
                                    sbDesc.Append(funcArgInfo.Unit);
                                    sbDesc.Append("]");
                                }

                                if (sbDesc.Length > 0)
                                {
                                    textViewDesc.Text = sbDesc.ToString();
                                    argLayout.AddView(textViewDesc, wrapLayoutParams);
                                }

                                if (funcArgInfo.NameInfoList != null && funcArgInfo.NameInfoList.Count > 0)
                                {
                                    if (funcArgInfo.NameInfoList[0] is EdiabasToolActivity.SgFuncValNameInfo)
                                    {
                                        Spinner spinner = new Spinner(this);
                                        StringObjAdapter spinnerAdapter = new StringObjAdapter(this);
                                        foreach (EdiabasToolActivity.SgFuncNameInfo funcNameInfo in funcArgInfo.NameInfoList)
                                        {
                                            if (funcNameInfo is EdiabasToolActivity.SgFuncValNameInfo valNameInfo)
                                            {
                                                spinner.Adapter = spinnerAdapter;
                                                spinnerAdapter.Items.Add(new StringObjType(valNameInfo.Text, valNameInfo));
                                            }
                                        }
                                        spinnerAdapter.NotifyDataSetChanged();
                                        argLayout.AddView(spinner, wrapLayoutParams);
                                    }
                                }
                                else
                                {
                                    EditText editText = new EditText(this);
                                    argLayout.AddView(editText, wrapLayoutParams);
                                }

                                _layoutArgParams.AddView(argLayout, wrapLayoutParams);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool StoreChangesRequest(AcceptDelegate handler)
        {
            if (!ArgsValid())
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
            bool enable = ArgsValid();
            _buttonApply.Enabled = enable;
            _buttonExecute.Enabled = enable && !_offline;
        }

        private bool ArgsValid()
        {
            try
            {
                int position = _spinnerArgument.SelectedItemPosition;
                if (position >= 0 && position < _spinnerArgumentAdapter.Items.Count)
                {
                    return true;
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
                string argType = ArgTypeArg;
                if (_radioButtonArgTypeId.Checked)
                {
                    argType = ArgTypeID;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(argType);

                int position = _spinnerArgument.SelectedItemPosition;
                if (position >= 0 && position < _spinnerArgumentAdapter.Items.Count)
                {
                    EdiabasToolActivity.ExtraInfo item = _spinnerArgumentAdapter.Items[position];
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        sb.Append(";");
                        sb.Append(item.Name);
                    }
                }

                string controlParameter = string.Empty;
                if (_controlRoutine)
                {
                    if (_radioButtonStpr.Checked)
                    {
                        controlParameter = "STPR";
                    }
                    else if (_radioButtonRrr.Checked)
                    {
                        controlParameter = "RRR";
                    }
                    else
                    {
                        controlParameter = "STR";
                    }
                }
                else if (_controlIo)
                {
                    if (_radioButtonRtd.Checked)
                    {
                        controlParameter = "RTD";
                    }
                    else if (_radioButtonFcs.Checked)
                    {
                        controlParameter = "FCS";
                    }
                    else if (_radioButtonSta.Checked)
                    {
                        controlParameter = "STA";
                    }
                    else
                    {
                        controlParameter = "RCTECU";
                    }
                }

                if (!string.IsNullOrEmpty(controlParameter))
                {
                    sb.Append(";");
                    sb.Append(controlParameter);
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
