using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using BmwFileReader;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/arg_assist_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(ArgAssistControlActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class ArgAssistControlActivity : ArgAssistBaseActivity
    {
        public class ParameterData
        {
            public ParameterData(SgFunctions.SgFuncArgInfo SgFuncArgInfo, TextView textViewCaption, TextView textViewDesc, Drawable defaultBackground, List<object> itemList)
            {
                FuncArgInfo = SgFuncArgInfo;
                TextViewCaption = textViewCaption;
                TextViewDesc = textViewDesc;
                DefaultBackground = defaultBackground;
                ItemList = itemList;
            }

            public SgFunctions.SgFuncArgInfo FuncArgInfo { get; }

            public TextView TextViewCaption { get; }

            public TextView TextViewDesc { get; }

            public Drawable DefaultBackground { get; }

            public List<object> ItemList { get; }
        }

        public class InstanceData
        {
            public string Arguments { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private readonly List<ParameterData> _parameterList = new List<ParameterData>();
        private bool _activityRecreated;
        private bool _controlRoutine;
        private bool _controlIo;
        private bool _ignoreCheckChange;
        private EdiabasToolActivity.ExtraInfo _argumentSelectLastItem;

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
            _allowFullScreenMode = false;

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

            _controlRoutine = _serviceId == (int)SgFunctions.UdsServiceId.RoutineControl;
            _controlIo = _serviceId == (int)SgFunctions.UdsServiceId.IoControlById;

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
                EdiabasToolActivity.ExtraInfo item = GetSelectedArgsItem();
                if (_argumentSelectLastItem != item)
                {
                    UpdateArgParams();
                    UpdateButtonState();
                }
            };
            _argumentSelectLastItem = null;

            _textViewControlParam = FindViewById<TextView>(Resource.Id.textViewControlParam);
            _textViewControlParam.Visibility = _controlRoutine || _controlIo ? ViewStates.Visible : ViewStates.Gone;

            _radioGroupControlRoutine = FindViewById<RadioGroup>(Resource.Id.radioGroupControlRoutine);
            _radioGroupControlRoutine.Visibility = _controlRoutine ? ViewStates.Visible : ViewStates.Gone;
            _radioGroupControlRoutine.SetOnTouchListener(this);
            _radioButtonStr = FindViewById<RadioButton>(Resource.Id.radioButtonStr);
            _radioButtonStr.SetOnTouchListener(this);
            _radioButtonStr.CheckedChange += RadioButtonControlCheckedChange;
            _radioButtonStpr = FindViewById<RadioButton>(Resource.Id.radioButtonStpr);
            _radioButtonStpr.SetOnTouchListener(this);
            _radioButtonStpr.CheckedChange += RadioButtonControlCheckedChange;
            _radioButtonRrr = FindViewById<RadioButton>(Resource.Id.radioButtonRrr);
            _radioButtonRrr.SetOnTouchListener(this);
            _radioButtonRrr.CheckedChange += RadioButtonControlCheckedChange;

            _radioGroupControlIo = FindViewById<RadioGroup>(Resource.Id.radioGroupControlIo);
            _radioGroupControlIo.Visibility = _controlIo ? ViewStates.Visible : ViewStates.Gone;
            _radioGroupControlIo.SetOnTouchListener(this);
            _radioButtonRctEcu = FindViewById<RadioButton>(Resource.Id.radioButtonRctEcu);
            _radioButtonRctEcu.SetOnTouchListener(this);
            _radioButtonRctEcu.CheckedChange += RadioButtonControlCheckedChange;
            _radioButtonRtd = FindViewById<RadioButton>(Resource.Id.radioButtonRtd);
            _radioButtonRtd.SetOnTouchListener(this);
            _radioButtonRtd.CheckedChange += RadioButtonControlCheckedChange;
            _radioButtonFcs = FindViewById<RadioButton>(Resource.Id.radioButtonFcs);
            _radioButtonFcs.SetOnTouchListener(this);
            _radioButtonFcs.CheckedChange += RadioButtonControlCheckedChange;
            _radioButtonSta = FindViewById<RadioButton>(Resource.Id.radioButtonSta);
            _radioButtonSta.SetOnTouchListener(this);
            _radioButtonSta.CheckedChange += RadioButtonControlCheckedChange;

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
                if (_ignoreCheckChange)
                {
                    return;
                }
                UpdateArgList();
                UpdateArgParams();
            };

            _radioButtonArgTypeId.CheckedChange += (sender, args) =>
            {
                if (_ignoreCheckChange)
                {
                    return;
                }
                UpdateArgList();
                UpdateArgParams();
            };

            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _instanceData.Arguments = GetArgString();
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        public override void OnBackPressedEvent()
        {
            if (!StoreChangesRequest(accepted =>
            {
                if (accepted)
                {
                    UpdateResult();
                }

                base.OnBackPressedEvent();
            }))
            {
                base.OnBackPressedEvent();
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

        public override void UpdateArgFilter()
        {
            UpdateArgFilter(false);
        }

        public void UpdateArgFilter(bool forceUpdate)
        {
            EdiabasToolActivity.ExtraInfo selectedItem = GetSelectedArgsItem();
            foreach (EdiabasToolActivity.ExtraInfo extraInfo in _spinnerArgumentAdapter.Items)
            {
                bool itemVisible = true;
                if (!string.IsNullOrEmpty(_argFilterText))
                {
                    if (!IsSearchFilterMatching(extraInfo.Name, _argFilterText))
                    {
                        itemVisible = false;
                    }
                }

                extraInfo.ItemVisible = itemVisible;
            }

            _spinnerArgumentAdapter.NotifyDataSetChanged();
            if (selectedItem != null)
            {
                int selectedIndex = _spinnerArgumentAdapter.ItemsVisible.IndexOf(selectedItem);
                if (selectedIndex >= 0)
                {
                    _spinnerArgument.SetSelection(selectedIndex);
                }
            }

            _argumentSelectLastItem = GetSelectedArgsItem();
            bool selectionChanged = selectedItem != null && selectedItem != _argumentSelectLastItem;

            if (forceUpdate || selectionChanged)
            {
                UpdateArgParams();
                UpdateButtonState();
            }
        }

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }

            try
            {
                _ignoreCheckChange = true;

                List<string> selectList = null;
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

                        if (argArray.Length > 3)
                        {
                            selectList = argArray.ToList();
                            selectList.RemoveRange(0, 3);
                        }
                    }
                    else
                    {
                        if (argArray.Length > 2)
                        {
                            selectList = argArray.ToList();
                            selectList.RemoveRange(0, 2);
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
                UpdateArgParams(selectList);
                UpdateArgParamsVisible();
                UpdateButtonState();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _ignoreCheckChange = false;
            }
        }

        private void RadioButtonControlCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (_ignoreCheckChange)
            {
                return;
            }
            UpdateArgParamsVisible();
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
                    foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList.OrderBy(x => argTypeId ? x.Id : x.Arg))
                    {
                        if (funcInfo.ServiceList != null && funcInfo.ServiceList.Contains(_serviceId))
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

                bool forceUpdate = selectArg == null;
                UpdateArgFilter(forceUpdate);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateArgParamsVisible()
        {
            bool visible = true;
            if (_controlRoutine)
            {
                visible = _radioButtonStr.Checked;
            }
            else if (_controlIo)
            {
                visible = _radioButtonSta.Checked;
            }

            _layoutArgParams.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
        }

        private EdiabasToolActivity.ExtraInfo GetSelectedArgsItem()
        {
            int position = _spinnerArgument.SelectedItemPosition;
            if (position >= 0 && position < _spinnerArgumentAdapter.ItemsVisible.Count)
            {
                return _spinnerArgumentAdapter.ItemsVisible[position];
            }

            return null;
        }

        private void UpdateArgParams(List<string> selectParams = null)
        {
            try
            {
                _layoutArgParams.RemoveAllViews();
                _parameterList.Clear();
                Android.Content.Res.ColorStateList captionTextColors = _textViewArgTypeTitle.TextColors;
                Drawable captionTextBackground = _textViewArgTypeTitle.Background;
                EdiabasToolActivity.ExtraInfo item = GetSelectedArgsItem();

                if (item != null)
                {
                    if (item.Tag is SgFunctions.SgFuncInfo funcInfo)
                    {
                        if (funcInfo.ArgInfoList != null)
                        {
                            foreach (SgFunctions.SgFuncArgInfo funcArgInfo in funcInfo.ArgInfoList)
                            {
                                string selectParam = string.Empty;
                                if (selectParams != null && selectParams.Count > _parameterList.Count)
                                {
                                    selectParam = selectParams[_parameterList.Count];
                                }

                                LinearLayout argLayout = new LinearLayout(this);
                                argLayout.Orientation = Orientation.Vertical;

                                LinearLayout.LayoutParams wrapLayoutParams = new LinearLayout.LayoutParams(
                                    ViewGroup.LayoutParams.MatchParent,
                                    ViewGroup.LayoutParams.WrapContent);

                                TextView textViewCaption = new TextView(this);
                                textViewCaption.SetOnTouchListener(this);
                                textViewCaption.SetTextColor(captionTextColors);
                                textViewCaption.Background = captionTextBackground;

                                StringBuilder sbCaption = new StringBuilder();
                                sbCaption.Append(GetString(Resource.String.arg_assist_control_parameter));
                                sbCaption.Append(": ");
                                sbCaption.Append(funcArgInfo.Arg);
                                textViewCaption.Text = sbCaption.ToString();
                                argLayout.AddView(textViewCaption, wrapLayoutParams);

                                TextView textViewDesc = null;
                                StringBuilder sbDesc = new StringBuilder();
                                string info = funcArgInfo.InfoTrans ?? funcArgInfo.Info;
                                if (!string.IsNullOrEmpty(info))
                                {
                                    sbDesc.Append(info);
                                }

                                if (!string.IsNullOrEmpty(funcArgInfo.DataType))
                                {
                                    if (sbDesc.Length > 0)
                                    {
                                        sbDesc.Append("\r\n");
                                    }
                                    sbDesc.Append("Data type: ");
                                    sbDesc.Append(funcArgInfo.DataType);
                                }

                                if (!string.IsNullOrEmpty(funcArgInfo.Unit))
                                {
                                    if (sbDesc.Length > 0)
                                    {
                                        sbDesc.Append("\r\n");
                                    }
                                    sbDesc.Append("Unit: ");
                                    sbDesc.Append(funcArgInfo.Unit);
                                }


                                if (funcArgInfo.TableDataType == SgFunctions.TableDataType.Float)
                                {
                                    string minText = funcArgInfo.MinText;
                                    if (string.IsNullOrEmpty(minText))
                                    {
                                        if (funcArgInfo.Min.HasValue)
                                        {
                                            minText = string.Format(CultureInfo.InvariantCulture, "{0:0.0}", funcArgInfo.Min.Value);
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(minText))
                                    {
                                        if (sbDesc.Length > 0)
                                        {
                                            sbDesc.Append("\r\n");
                                        }
                                        sbDesc.Append("Min: ");
                                        sbDesc.Append(minText);
                                    }

                                    string maxText = funcArgInfo.MaxText;
                                    if (string.IsNullOrEmpty(maxText))
                                    {
                                        if (funcArgInfo.Max.HasValue)
                                        {
                                            maxText = string.Format(CultureInfo.InvariantCulture, "{0:0.0}", funcArgInfo.Max.Value);
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(maxText))
                                    {
                                        if (sbDesc.Length > 0)
                                        {
                                            sbDesc.Append("\r\n");
                                        }
                                        sbDesc.Append("Max: ");
                                        sbDesc.Append(maxText);
                                    }
                                }

                                if (sbDesc.Length > 0)
                                {
                                    textViewDesc = new TextView(this);
                                    textViewDesc.SetOnTouchListener(this);
                                    textViewDesc.Text = sbDesc.ToString();
                                    argLayout.AddView(textViewDesc, wrapLayoutParams);
                                }

                                Drawable defaultBackground = null;
                                List<object> itemList = new List<object>();
                                if (funcArgInfo.NameInfoList != null && funcArgInfo.NameInfoList.Count > 0)
                                {
                                    if (funcArgInfo.NameInfoList[0] is SgFunctions.SgFuncValNameInfo)
                                    {
                                        Spinner spinner = new Spinner(this);
                                        spinner.SetOnTouchListener(this);
                                        defaultBackground = spinner.Background;
                                        StringObjAdapter spinnerAdapter = new StringObjAdapter(this);
                                        spinnerAdapter.Items.Add(new StringObjType("--", null, Android.Graphics.Color.Red));
                                        int selection = 0;
                                        int index = 1;
                                        foreach (SgFunctions.SgFuncNameInfo funcNameInfo in funcArgInfo.NameInfoList)
                                        {
                                            if (funcNameInfo is SgFunctions.SgFuncValNameInfo valNameInfo)
                                            {
                                                spinner.Adapter = spinnerAdapter;
                                                StringBuilder sbName = new StringBuilder();
                                                sbName.Append(valNameInfo.Value);
                                                sbName.Append(": ");
                                                sbName.Append(valNameInfo.Text);
                                                spinnerAdapter.Items.Add(new StringObjType(sbName.ToString(), valNameInfo));
                                                if (string.Compare(valNameInfo.Text, selectParam, StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    selection = index;
                                                }
                                            }

                                            index++;
                                        }

                                        spinnerAdapter.NotifyDataSetChanged();
                                        spinner.SetSelection(selection);
                                        spinner.ItemSelected += (sender, args) =>
                                        {
                                            ValidateParams();
                                        };
                                        argLayout.AddView(spinner, wrapLayoutParams);
                                        itemList.Add(spinner);
                                    }
                                }
                                else
                                {
                                    EditText editText = new EditText(this);
                                    defaultBackground = editText.Background;
                                    editText.SetSingleLine();
                                    editText.ImeOptions = ImeAction.Done;
                                    editText.Text = selectParam;

                                    editText.TextChanged += (sender, args) =>
                                    {
                                        ValidateParams();
                                    };

                                    editText.EditorAction += (sender, args) =>
                                    {
                                        switch (args.ActionId)
                                        {
                                            case ImeAction.Go:
                                            case ImeAction.Send:
                                            case ImeAction.Next:
                                            case ImeAction.Done:
                                            case ImeAction.Previous:
                                                ValidateParams();
                                                HideKeyboard();
                                                break;
                                        }
                                    };

                                    argLayout.AddView(editText, wrapLayoutParams);
                                    itemList.Add(editText);
                                }

                                _layoutArgParams.AddView(argLayout, wrapLayoutParams);

                                _parameterList.Add(new ParameterData(funcArgInfo, textViewCaption, textViewDesc, defaultBackground, itemList));
                            }
                        }
                    }
                }

                ValidateParams();
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

        private void ValidateParams()
        {
            try
            {
                if (_layoutArgParams.Visibility == ViewStates.Visible)
                {
                    foreach (ParameterData parameterData in _parameterList)
                    {
                        foreach (object itemObject in parameterData.ItemList)
                        {
                            if (itemObject is EditText editText)
                            {
                                SgFunctions.SgFuncArgInfo funcArgInfo = parameterData.FuncArgInfo;
                                string paramText = editText.Text ?? string.Empty;
                                bool paramValid = true;

                                if (funcArgInfo != null)
                                {
                                    switch (funcArgInfo.TableDataType)
                                    {
                                        case SgFunctions.TableDataType.Float:
                                        {
                                            double floatValue = EdiabasNet.StringToFloat(paramText, out bool valid);
                                            if (!valid)
                                            {
                                                paramValid = false;
                                                break;
                                            }

                                            if (funcArgInfo.Min.HasValue && floatValue < funcArgInfo.Min.Value)
                                            {
                                                paramValid = false;
                                                break;
                                            }

                                            if (funcArgInfo.Max.HasValue && floatValue > funcArgInfo.Max.Value)
                                            {
                                                paramValid = false;
                                            }
                                            break;
                                        }

                                        case SgFunctions.TableDataType.String:
                                        {
                                            int length = paramText.Length;
                                            if (length == 0)
                                            {
                                                paramValid = false;
                                                break;
                                            }

                                            if (funcArgInfo.Length.HasValue)
                                            {
                                                if (length != funcArgInfo.Length.Value)
                                                {
                                                    paramValid = false;
                                                }
                                            }
                                            else
                                            {
                                                if (length > 255)
                                                {
                                                    paramValid = false;
                                                }
                                            }
                                            break;
                                        }

                                        case SgFunctions.TableDataType.Binary:
                                        {
                                            byte[] data = EdiabasNet.HexToByteArray(paramText);
                                            if (data == null || data.Length == 0)
                                            {
                                                paramValid = false;
                                                break;
                                            }

                                            if (funcArgInfo.Length.HasValue && data.Length != funcArgInfo.Length.Value)
                                            {
                                                paramValid = false;
                                            }
                                            break;
                                        }
                                    }
                                }

                                if (paramValid)
                                {
                                    editText.Background = parameterData.DefaultBackground;
                                }
                                else
                                {
                                    editText.SetBackgroundColor(Android.Graphics.Color.Red);
                                }
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

        private bool ArgsValid()
        {
            try
            {
                EdiabasToolActivity.ExtraInfo item = GetSelectedArgsItem();
                return item != null;
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

                EdiabasToolActivity.ExtraInfo item = GetSelectedArgsItem();
                if (item != null)
                {
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

                if (_layoutArgParams.Visibility == ViewStates.Visible)
                {
                    foreach (ParameterData parameterData in _parameterList)
                    {
                        string parameter = string.Empty;
                        foreach (object itemObject in parameterData.ItemList)
                        {
                            parameter = string.Empty;
                            if (itemObject is EditText editText)
                            {
                                parameter = editText.Text;
                            }
                            else if (itemObject is Spinner spinner)
                            {
                                if (spinner.Adapter is StringObjAdapter spinnerAdapter)
                                {
                                    int spinnerPos = spinner.SelectedItemPosition;
                                    if (spinnerPos >= 0 && spinnerPos < spinnerAdapter.Items.Count)
                                    {
                                        StringObjType itemSpinner = spinnerAdapter.Items[spinnerPos];
                                        if (itemSpinner.Data is SgFunctions.SgFuncValNameInfo valNameInfo)
                                        {
                                            parameter = valNameInfo.Text;
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(parameter))
                            {
                                break;
                            }
                        }

                        sb.Append(";");
                        sb.Append(parameter);
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
