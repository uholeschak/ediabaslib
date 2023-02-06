using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using BmwFileReader;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/arg_assist_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(ArgAssistStatActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class ArgAssistStatActivity : ArgAssistBaseActivity
    {
        public class InstanceData
        {
            public string Arguments { get; set; }
            public bool ArgsAmountWarnShown { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _dynamicId;
        private bool _mwBlock;
        private bool _ignoreCheckChange;

        private LinearLayout _layoutArgType;
        private LinearLayout _layoutBlockNumber;
        private Spinner _spinnerBlockNumber;
        private StringObjAdapter _spinnerBlockNumberAdapter;
        private CheckBox _checkBoxDefineBlockNew;
        private ListView _listViewArgs;
        private EdiabasToolActivity.ResultSelectListAdapter _argsListAdapter;

        private int ArgAmountLimit => _mwBlock ? 10 : 5;

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
            SetContentView(Resource.Layout.arg_assist_status);

            InitBaseVariables();

            _dynamicId = _serviceId == (int)SgFunctions.UdsServiceId.DynamicallyDefineId;
            _mwBlock = _serviceId == (int)SgFunctions.UdsServiceId.MwBlock;
            if (!_activityRecreated && _instanceData != null)
            {
                _instanceData.Arguments = Intent.GetStringExtra(ExtraArguments);
            }

            _buttonApply.Click += (sender, args) =>
            {
                if (ArgsSelectCount() > 0 && UpdateResult())
                {
                    Finish();
                }
            };

            _buttonExecute.Click += (sender, args) =>
            {
                if (ArgsSelectCount() > 0 && UpdateResult(true))
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
            };

            _radioButtonArgTypeId.CheckedChange += (sender, args) =>
            {
                if (_ignoreCheckChange)
                {
                    return;
                }
                UpdateArgList();
            };

            _layoutArgType = FindViewById<LinearLayout>(Resource.Id.layoutArgType);
            _layoutArgType.SetOnTouchListener(this);
            _layoutArgType.Visibility = _mwBlock ? ViewStates.Gone : ViewStates.Visible;

            _layoutBlockNumber = FindViewById<LinearLayout>(Resource.Id.layoutBlockNumber);
            _layoutBlockNumber.SetOnTouchListener(this);
            _layoutBlockNumber.Visibility = _dynamicId ? ViewStates.Visible : ViewStates.Gone;

            _spinnerBlockNumber = FindViewById<Spinner>(Resource.Id.spinnerBlockNumber);
            _spinnerBlockNumberAdapter = new StringObjAdapter(this);
            _spinnerBlockNumber.Adapter = _spinnerBlockNumberAdapter;

            _checkBoxDefineBlockNew = FindViewById<CheckBox>(Resource.Id.checkBoxDefineBlockNew);
            _checkBoxDefineBlockNew.SetOnTouchListener(this);
            _checkBoxDefineBlockNew.Visibility = _mwBlock || _dynamicId ? ViewStates.Visible : ViewStates.Gone;

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
                string blockNumber = string.Empty;
                string defineBlockNew = string.Empty;
                string argType = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.Arguments))
                {
                    string[] argArray = _instanceData.Arguments.Split(";");
                    if (_mwBlock)
                    {
                        if (argArray.Length > 0)
                        {
                            defineBlockNew = argArray[0].Trim();
                            selectList = argArray.ToList();
                            selectList.RemoveAt(0);
                        }
                    }
                    else if (_dynamicId)
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
                    case ArgTypeID:
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
                }

                if (_mwBlock || _dynamicId)
                {
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
            finally
            {
                _ignoreCheckChange = false;
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
                    foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList.OrderBy(x => argTypeId ? x.Id : x.Arg))
                    {
                        bool addArg = false;
                        if (_mwBlock)
                        {
                            if (funcInfo.ServiceList == null)
                            {
                                addArg = true;
                            }
                        }
                        else
                        {
                            if (funcInfo.ServiceList != null && funcInfo.ServiceList.Contains(_serviceId))
                            {
                                addArg = true;
                            }
                        }

                        if (addArg)
                        {
                            string argType = argTypeId ? funcInfo.Id : funcInfo.Arg;
                            string argName = argTypeId ? funcInfo.Arg : funcInfo.Id;
                            string name = argType + " (" + argName + ")";
                            string info = funcInfo.InfoTrans ?? funcInfo.Info;

                            if (!string.IsNullOrEmpty(_argFilterText))
                            {
                                if (!IsSearchFilterMatching(name, _argFilterText))
                                {
                                    continue;   // filter is not matching
                                }
                            }

                            EdiabasToolActivity.ExtraInfo extraInfo = new EdiabasToolActivity.ExtraInfo(name, argType, new List<string> {info});
                            if (selectList != null)
                            {
                                if (selectList.Contains(argType))
                                {
                                    extraInfo.Selected = true;
                                }
                            }
                            _argsListAdapter.Items.Add(extraInfo);
                        }
                    }
                }

                UpdateArgFilter();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void UpdateArgFilter()
        {
            foreach (EdiabasToolActivity.ExtraInfo extraInfo in _argsListAdapter.Items)
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

            _argsListAdapter.NotifyDataSetChanged();
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
                if (ArgsSelectCount() > ArgAmountLimit)
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
                    if (extraInfo.ItemSelected)
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
                string argType = ArgTypeArg;
                if (_radioButtonArgTypeId.Checked)
                {
                    argType = ArgTypeID;
                }

                StringBuilder sb = new StringBuilder();
                if (_mwBlock)
                {
                    sb.Append(_checkBoxDefineBlockNew.Checked ? "YES" : "NO");
                }
                else
                {
                    if (_dynamicId)
                    {
                        int blockNumber = 0;
                        int position = _spinnerBlockNumber.SelectedItemPosition;
                        if (position >= 0 && position < _spinnerBlockNumberAdapter.Items.Count)
                        {
                            StringObjType item = _spinnerBlockNumberAdapter.Items[position];
                            blockNumber = (int)item.Data;
                        }

                        sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}", blockNumber));
                        sb.Append(";");

                        sb.Append(_checkBoxDefineBlockNew.Checked ? "YES" : "NO");
                        sb.Append(";");
                    }

                    sb.Append(argType);
                }

                foreach (EdiabasToolActivity.ExtraInfo extraInfo in _argsListAdapter.Items)
                {
                    if (extraInfo.ItemSelected)
                    {
                        sb.Append(";");
                        sb.Append(extraInfo.Type);
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
