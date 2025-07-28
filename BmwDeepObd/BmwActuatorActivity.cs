using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BmwFileReader;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(
        Name = ActivityCommon.AppNameSpace + "." + nameof(BmwActuatorActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class BmwActuatorActivity : BaseActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public InstanceData()
            {
                SelectedFunctionText = string.Empty;
            }

            public int SelectedFunction { get; set; }
            public string SelectedFunctionText { get; set; }
            public bool Continuous { get; set; }
            public bool StopActuator { get; set; }
            public bool AutoClose { get; set; }
        }

        // Intent extra
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraEcuName = "ecu_name";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraSimulationDir = "simulation_dir";
        public const string ExtraTraceDir = "trace_dir";
        public const string ExtraTraceAppend = "trace_append";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraElmWifiIp = "elmwifi_ip";
        public const string ExtraDeepObdWifiIp = "deepobdwifi_ip";

        public static XmlToolActivity.EcuInfo IntentEcuInfo { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private InputMethodManager _imm;
        private View _contentView;
        private ScrollView _scrollViewBmwActuator;
        private LinearLayout _layoutBmwActuator;
        private LinearLayout _layoutBmwActuatorFunction;
        private TextView _textViewBmwActuatorFunction;
        private Spinner _spinnerBmwActuatorFunction;
        private StringObjAdapter _spinnerBmwActuatorFunctionAdapter;
        private LinearLayout _layoutBmwActuatorInfo;
        private LinearLayout _layoutBmwActuatorComments;
        private TextView _textViewBmwActuatorCommentsTitle;
        private TextView _textBmwActuatorComments;
        private LinearLayout _layoutBmwActuatorStatus;
        private TextView _textViewBmwActuatorStatusTitle;
        private TextView _textBmwActuatorStatus;
        private LinearLayout _layoutBmwActuatorOperation;
        private TextView _textViewBmwActuatorOperationTitle;
        private Button _buttonBmwActuatorExecuteSingle;
        private Button _buttonBmwActuatorExecuteContinuous;
        private Button _buttonBmwActuatorStop;
        private ActivityCommon _activityCommon;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private bool _activityActive;
        private bool _ediabasJobAbort;
        private string _appDataDir;
        private string _ecuDir;
        private string _simulationDir;
        private string _traceDir;
        private bool _traceAppend;
        private string _deviceAddress;
        private List<XmlToolEcuActivity.JobInfo> _jobActuatorList;
        private bool _checkActuatorJobGroups;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme();
            base.OnCreate(savedInstanceState);
            _allowFullScreenMode = false;
            if (savedInstanceState != null)
            {
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.bmw_actuator);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            if (IntentEcuInfo == null)
            {
                Finish();
                return;
            }

            _activityCommon = new ActivityCommon(this, () =>
            {

            }, BroadcastReceived);

            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _simulationDir = Intent.GetStringExtra(ExtraSimulationDir);
            _traceDir = Intent.GetStringExtra(ExtraTraceDir);
            _traceAppend = Intent.GetBooleanExtra(ExtraTraceAppend, true);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);

            _ecuInfo = IntentEcuInfo;
            UpdateInfoAdaptionList();

            SupportActionBar.Title = string.Format(GetString(Resource.String.bmw_actuator_title), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);

            _scrollViewBmwActuator = FindViewById<ScrollView>(Resource.Id.scrollViewBmwActuator);
            _scrollViewBmwActuator.SetOnTouchListener(this);

            _layoutBmwActuator = FindViewById<LinearLayout>(Resource.Id.layoutBmwActuator);
            _layoutBmwActuator.SetOnTouchListener(this);

            _layoutBmwActuatorFunction = FindViewById<LinearLayout>(Resource.Id.layoutBmwActuatorFunction);
            _layoutBmwActuatorFunction.SetOnTouchListener(this);

            _textViewBmwActuatorFunction = FindViewById<TextView>(Resource.Id.textViewBmwActuatorFunction);
            _textViewBmwActuatorFunction.SetOnTouchListener(this);

            _spinnerBmwActuatorFunction = FindViewById<Spinner>(Resource.Id.spinnerBmwActuatorFunction);
            _spinnerBmwActuatorFunction.SetOnTouchListener(this);
            _spinnerBmwActuatorFunctionAdapter = new StringObjAdapter(this);
            _spinnerBmwActuatorFunction.Adapter = _spinnerBmwActuatorFunctionAdapter;
            _spinnerBmwActuatorFunction.ItemSelected += ActuatorFunctionItemSelected;

            _layoutBmwActuatorInfo = FindViewById<LinearLayout>(Resource.Id.layoutBmwActuatorInfo);
            _layoutBmwActuatorInfo.SetOnTouchListener(this);

            _layoutBmwActuatorComments = FindViewById<LinearLayout>(Resource.Id.layoutBmwActuatorComments);
            _layoutBmwActuatorComments.SetOnTouchListener(this);

            _textViewBmwActuatorCommentsTitle = FindViewById<TextView>(Resource.Id.textViewBmwActuatorCommentsTitle);
            _textViewBmwActuatorCommentsTitle.SetOnTouchListener(this);

            _textBmwActuatorComments = FindViewById<TextView>(Resource.Id.textBmwActuatorComments);
            _textBmwActuatorComments.SetOnTouchListener(this);
            _textBmwActuatorComments.MovementMethod = new ScrollingMovementMethod();

            _layoutBmwActuatorStatus = FindViewById<LinearLayout>(Resource.Id.layoutBmwActuatorStatus);
            _layoutBmwActuatorStatus.SetOnTouchListener(this);

            _textViewBmwActuatorStatusTitle = FindViewById<TextView>(Resource.Id.textViewBmwActuatorStatusTitle);
            _textViewBmwActuatorStatusTitle.SetOnTouchListener(this);

            _textBmwActuatorStatus = FindViewById<TextView>(Resource.Id.textBmwActuatorStatus);
            _textBmwActuatorStatus.SetOnTouchListener(this);
            _textBmwActuatorStatus.MovementMethod = new ScrollingMovementMethod();

            _layoutBmwActuatorOperation = FindViewById<LinearLayout>(Resource.Id.layoutBmwActuatorOperation);
            _layoutBmwActuatorOperation.SetOnTouchListener(this);

            _textViewBmwActuatorOperationTitle = FindViewById<TextView>(Resource.Id.textViewBmwActuatorOperationTitle);
            _textViewBmwActuatorOperationTitle.SetOnTouchListener(this);

            _buttonBmwActuatorExecuteSingle = FindViewById<Button>(Resource.Id.buttonBmwActuatorExecuteSingle);
            _buttonBmwActuatorExecuteSingle.SetOnTouchListener(this);
            _buttonBmwActuatorExecuteSingle.Click += (sender, args) =>
            {
                ExecuteActuatorFunction(false);
            };

            _buttonBmwActuatorExecuteContinuous = FindViewById<Button>(Resource.Id.buttonBmwActuatorExecuteContinuous);
            _buttonBmwActuatorExecuteContinuous.SetOnTouchListener(this);
            _buttonBmwActuatorExecuteContinuous.Click += (sender, args) =>
            {
                ExecuteActuatorFunction(true);
            };

            _buttonBmwActuatorStop = FindViewById<Button>(Resource.Id.buttonBmwActuatorStop);
            _buttonBmwActuatorStop.SetOnTouchListener(this);
            _buttonBmwActuatorStop.Click += (sender, args) =>
            {
                _instanceData.StopActuator = true;
            };

            UpdateActuatorFunctionList();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_activityCommon != null)
            {
                if (_activityCommon.MtcBtService)
                {
                    _activityCommon.StartMtcService();
                }
                _activityCommon?.RequestUsbPermission(null);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _activityActive = true;
        }

        protected override void OnPause()
        {
            base.OnPause();
            _activityActive = false;
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (_activityCommon != null && _activityCommon.MtcBtService)
            {
                _activityCommon.StopMtcService();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _ediabasJobAbort = true;
            if (IsJobRunning())
            {
                _jobThread?.Join();
            }
            EdiabasClose();
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressedEvent()
        {
            if (IsJobRunning())
            {
                _instanceData.StopActuator = true;
                _instanceData.AutoClose = true;
                return;
            }
            StoreResults();
            base.OnBackPressedEvent();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsJobRunning())
                    {
                        _instanceData.StopActuator = true;
                        _instanceData.AutoClose = true;
                        return true;
                    }
                    StoreResults();
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool IsFinishAllowed()
        {
            if (IsJobRunning())
            {
                return false;
            }
            return true;
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

        private void EdiabasOpen()
        {
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet
                {
                    EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                    AbortJobFunc = AbortEdiabasJob
                };
                _ediabas.SetConfigProperty("EcuPath", _ecuDir);
                ActivityCommon.SetEdiabasConfigProperties(_ediabas, _traceDir, _simulationDir, _traceAppend);
            }

            _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress, _appDataDir);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool EdiabasClose()
        {
            if (IsJobRunning())
            {
                return false;
            }
            if (_ediabas != null)
            {
                _ediabas.Dispose();
                _ediabas = null;
            }
            return true;
        }

        private bool IsJobRunning()
        {
            if (_jobThread == null)
            {
                return false;
            }
            if (_jobThread.IsAlive)
            {
                return true;
            }
            _jobThread = null;
            return false;
        }

        private bool AbortEdiabasJob()
        {
            if (_ediabasJobAbort)
            {
                return true;
            }
            return false;
        }

        private void BroadcastReceived(Context context, Intent intent)
        {
            if (intent == null)
            {   // from usb check timer
                if (_activityActive)
                {
                    _activityCommon.RequestUsbPermission(null);
                }
                return;
            }
            string action = intent.Action;
            switch (action)
            {
                case UsbManager.ActionUsbDeviceAttached:
                    if (_activityActive)
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                        if (usbDevice != null)
                        {
                            _activityCommon.RequestUsbPermission(usbDevice);
                        }
                    }
                    break;

                case UsbManager.ActionUsbDeviceDetached:
                    if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Ftdi)
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                        if (usbDevice != null && EdFtdiInterface.IsValidUsbDevice(usbDevice))
                        {
                            EdiabasClose();
                        }
                    }
                    break;
            }
        }

        private void StoreResults()
        {
        }

        private void ActuatorFunctionItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            UpdateActuator();
        }

        private void UpdateInfoAdaptionList()
        {
        }

        private void CreateActuatorJobList()
        {
            if (_jobActuatorList != null)
            {
                return;
            }

            string language = _activityCommon.GetCurrentLanguage();
            bool multipleFuncStruct = false;
            _jobActuatorList = new List<XmlToolEcuActivity.JobInfo>();
            foreach (XmlToolEcuActivity.JobInfo jobInfo in _ecuInfo.JobList)
            {
                if (jobInfo.EcuFixedFuncStruct == null)
                {
                    continue;
                }

                switch (jobInfo.EcuFixedFuncStruct.GetNodeClassType())
                {
                    case EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator:
                    {
                        string displayText = jobInfo.EcuFixedFuncStruct.Title?.GetTitle(language);
                        if (!string.IsNullOrWhiteSpace(displayText))
                        {
                            if (!multipleFuncStruct && jobInfo.EcuFuncStruct != null)
                            {
                                foreach (XmlToolEcuActivity.JobInfo jobInfoCheck in _jobActuatorList)
                                {
                                    if (jobInfoCheck.EcuFuncStruct == jobInfo.EcuFuncStruct)
                                    {
                                        multipleFuncStruct = true;
                                    }
                                }
                            }

                            _jobActuatorList.Add(jobInfo);
                        }

                        break;
                    }
                }
            }

            _checkActuatorJobGroups = multipleFuncStruct;
            _jobActuatorList.Sort((info1, info2) =>
            {
                string title1;
                string title2;
                Int64 sort1;
                Int64 sort2;
                int result;

                if (_checkActuatorJobGroups)
                {
                    sort1 = Int64.MaxValue;
                    sort2 = Int64.MaxValue;
                    title1 = string.Empty;
                    title2 = string.Empty;

                    if (info1.EcuFuncStruct != null)
                    {
                        sort1 = info1.EcuFuncStruct.SortOrder.ConvertToInt(Int64.MaxValue);
                        title1 = info1.EcuFuncStruct.Title?.GetTitle(language) ?? string.Empty;
                    }

                    if (info2.EcuFuncStruct != null)
                    {
                        sort2 = info2.EcuFuncStruct.SortOrder.ConvertToInt(Int64.MaxValue);
                        title2 = info2.EcuFuncStruct.Title?.GetTitle(language) ?? string.Empty;
                    }

                    result = sort1.CompareTo(sort2);
                    if (result != 0)
                    {
                        return result;
                    }

                    result = string.Compare(title1, title2, StringComparison.InvariantCulture);
                    if (result != 0)
                    {
                        return result;
                    }
                }

                sort1 = Int64.MaxValue;
                sort2 = Int64.MaxValue;
                title1 = string.Empty;
                title2 = string.Empty;

                if (info1.EcuFixedFuncStruct != null)
                {
                    sort1 = info1.EcuFixedFuncStruct.SortOrder.ConvertToInt(Int64.MaxValue);
                    title1 = info1.EcuFixedFuncStruct.Title?.GetTitle(language) ?? string.Empty;
                }

                if (info2.EcuFixedFuncStruct != null)
                {
                    sort2 = info2.EcuFixedFuncStruct.SortOrder.ConvertToInt(Int64.MaxValue);
                    title2 = info2.EcuFixedFuncStruct.Title?.GetTitle(language) ?? string.Empty;
                }

                result = sort1.CompareTo(sort2);
                if (result != 0)
                {
                    return result;
                }

                return string.Compare(title1, title2, StringComparison.InvariantCulture);
            });
        }

        private void UpdateActuatorFunctionList()
        {
            int selectedFunction = _instanceData.SelectedFunction;
            int selection = 0;
            string language = _activityCommon.GetCurrentLanguage();

            CreateActuatorJobList();

            _spinnerBmwActuatorFunctionAdapter.Items.Clear();

            int index = 0;
            foreach (XmlToolEcuActivity.JobInfo jobInfo in _jobActuatorList)
            {
                bool validFunction = true;
                List<EcuFunctionStructs.EcuJob> ecuJobList = jobInfo.EcuFixedFuncStruct.EcuJobList;
                if (ecuJobList == null || ecuJobList.Count == 0)
                {
                    validFunction = false;
                }

                string titleTextFunc = jobInfo.EcuFixedFuncStruct.Title?.GetTitle(language) ?? string.Empty;
                string displayText1 = titleTextFunc.Trim();
                if (!validFunction)
                {
                    displayText1 = "(" + displayText1 + ")";
                }

                string displayText2 = string.Empty;
                if (_checkActuatorJobGroups)
                {
                    string titleTextGroup = jobInfo.EcuFuncStruct.Title?.GetTitle(language) ?? string.Empty;
                    displayText2 = titleTextGroup.Trim();
                }

                _spinnerBmwActuatorFunctionAdapter.Items.Add(new StringObjType(displayText1, displayText2, string.Empty, index));

                if (index == selectedFunction)
                {
                    selection = index;
                }

                index++;
            }

            _spinnerBmwActuatorFunctionAdapter.NotifyDataSetChanged();
            _spinnerBmwActuatorFunction.SetSelection(selection);

            UpdateActuator();
        }

        private void UpdateActuator()
        {
            int index = _spinnerBmwActuatorFunction.SelectedItemPosition;
            if (index >= 0 && index < _spinnerBmwActuatorFunctionAdapter.Items.Count)
            {
                StringObjType item = _spinnerBmwActuatorFunctionAdapter.Items[index];
                int function = (int)item.Data;
                if (function >= 0)
                {
                    _instanceData.SelectedFunction = function;
                    _instanceData.SelectedFunctionText = item.Text;
                }
            }

            UpdateActuatorInfo();
        }

        private XmlToolEcuActivity.JobInfo GetSelectedJob()
        {
            int selectedFunction = _instanceData.SelectedFunction;
            if (selectedFunction >= 0 && selectedFunction < _jobActuatorList.Count)
            {
                return _jobActuatorList[selectedFunction];
            }

            return null;
        }

        public static void AppendSbText(StringBuilder sb, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (sb.Length > 0)
                {
                    sb.Append("\r\n");
                }

                sb.Append(text);
            }
        }

        private void UpdateActuatorInfo()
        {
            UpdateActuatorStatus();

            string language = _activityCommon.GetCurrentLanguage();
            StringBuilder stringBuilderComments = new StringBuilder();
            XmlToolEcuActivity.JobInfo selectedJob = GetSelectedJob();

            string titleTextGroup = selectedJob?.EcuFuncStruct?.Title?.GetTitle(language);
            if (!string.IsNullOrWhiteSpace(titleTextGroup))
            {
                AppendSbText(stringBuilderComments, titleTextGroup.Trim());
            }

            string titleTextFunc = selectedJob?.EcuFixedFuncStruct?.Title?.GetTitle(language);
            if (!string.IsNullOrWhiteSpace(titleTextFunc))
            {
                AppendSbText(stringBuilderComments, titleTextFunc.Trim());
            }

            List<EcuFunctionStructs.EcuJob> ecuJobList = selectedJob?.EcuFixedFuncStruct?.EcuJobList;
            if (ecuJobList != null)
            {
                string preOpText = selectedJob.EcuFixedFuncStruct.PrepOp?.GetTitle(language);
                if (!string.IsNullOrWhiteSpace(preOpText))
                {
                    int presetCount = ecuJobList.Count(x => x.GetPhaseType() == EcuFunctionStructs.EcuJob.PhaseType.Preset);
                    if (presetCount > 0)
                    {
                        AppendSbText(stringBuilderComments, preOpText.Trim());
                    }
                    else
                    {
                        preOpText = string.Empty;
                    }
                }

                string procOpText = selectedJob.EcuFixedFuncStruct.ProcOp?.GetTitle(language);
                if (!string.IsNullOrWhiteSpace(procOpText) && procOpText != preOpText)
                {
                    int mainCount = ecuJobList.Count(x => x.GetPhaseType() == EcuFunctionStructs.EcuJob.PhaseType.Main);
                    if (mainCount > 0)
                    {
                        AppendSbText(stringBuilderComments, procOpText.Trim());
                    }
                    else
                    {
                        procOpText = string.Empty;
                    }
                }

                string postOpText = selectedJob.EcuFixedFuncStruct.PostOp?.GetTitle(language);
                if (!string.IsNullOrWhiteSpace(postOpText) && postOpText != preOpText && postOpText != procOpText)
                {
                    int resetCount = ecuJobList.Count(x => x.GetPhaseType() == EcuFunctionStructs.EcuJob.PhaseType.Reset);
                    if (resetCount > 0)
                    {
                        AppendSbText(stringBuilderComments, postOpText.Trim());
                    }
                }
            }

            string actuatorFunctionComment = stringBuilderComments.ToString();

            _layoutBmwActuatorComments.Visibility = !string.IsNullOrWhiteSpace(actuatorFunctionComment) ? ViewStates.Visible : ViewStates.Gone;
            _textBmwActuatorComments.Text = actuatorFunctionComment;
        }

        private void UpdateActuatorStatus(bool cyclicUpdate = false)
        {
            bool jobRunning = IsJobRunning();
            bool validFunction = _instanceData.SelectedFunction >= 0;

            if (!cyclicUpdate)
            {
                _textBmwActuatorStatus.Text = string.Empty;
            }

            if (!jobRunning)
            {
                XmlToolEcuActivity.JobInfo selectedJob = GetSelectedJob();
                List<EcuFunctionStructs.EcuJob> ecuJobList = selectedJob?.EcuFixedFuncStruct?.EcuJobList;
                if (ecuJobList == null || ecuJobList.Count == 0)
                {
                    validFunction = false;
                }
            }

            _spinnerBmwActuatorFunction.Enabled = !jobRunning;
            _buttonBmwActuatorExecuteSingle.Enabled = !jobRunning && validFunction;
            _buttonBmwActuatorExecuteContinuous.Enabled = !jobRunning && validFunction;
            _buttonBmwActuatorStop.Enabled = jobRunning;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private void ExecuteActuatorFunction(bool continuous)
        {
            if (IsJobRunning())
            {
                return;
            }

            XmlToolEcuActivity.JobInfo selectedJob = GetSelectedJob();
            if (selectedJob == null)
            {
                return;
            }
            object lockObject = new object();
            string language = _activityCommon.GetCurrentLanguage();

            bool activation = selectedJob.EcuFixedFuncStruct.Activation.ConvertToInt() > 0;
            Int64 activationDuration = selectedJob.EcuFixedFuncStruct.ActivationDurationMs.ConvertToInt();

            EdiabasOpen();

            _instanceData.Continuous = continuous;
            _instanceData.StopActuator = false;
            _instanceData.AutoClose = false;

            UpdateActuatorStatus();

            bool executeFailed = false;
            _jobThread = new Thread(() =>
            {
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, _ecuInfo.Sgbd);

                    // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionComponentTrigger.PhasePreset
                    EcuFunctionStructs.EcuJob.PhaseType phase = EcuFunctionStructs.EcuJob.PhaseType.Preset;
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        string preOpText = selectedJob.EcuFixedFuncStruct.PrepOp?.GetTitle(language);
                        if (!string.IsNullOrWhiteSpace(preOpText))
                        {
                            _textBmwActuatorStatus.Text = preOpText;
                        }

                        UpdateActuatorStatus(true);
                    });

                    // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionComponentTrigger.DoTriggerComponent
                    bool hasResetJobs = selectedJob.EcuFixedFuncStruct.EcuJobList.Any(ecuJob => ecuJob.GetPhaseType() == EcuFunctionStructs.EcuJob.PhaseType.Reset);
                    StringBuilder sbStatus = new StringBuilder();
                    bool statusUpdated = false;
                    long startTime = Stopwatch.GetTimestamp();
                    for (; ; )
                    {
                        bool stopActuator = _instanceData.StopActuator;

                        EcuFunctionStructs.EcuJob.PhaseType currentPhase = phase;
                        if (stopActuator && currentPhase == EcuFunctionStructs.EcuJob.PhaseType.Main)
                        {
                            break;
                        }

                        bool updateStatus = currentPhase == EcuFunctionStructs.EcuJob.PhaseType.Main;
                        if (updateStatus && !statusUpdated)
                        {
                            lock (lockObject)
                            {
                                sbStatus.Clear();
                                string procOpText = selectedJob.EcuFixedFuncStruct.ProcOp?.GetTitle(language);
                                if (!string.IsNullOrWhiteSpace(procOpText))
                                {
                                    AppendSbText(sbStatus, procOpText);
                                }
                            }

                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                lock (lockObject)
                                {
                                    _textBmwActuatorStatus.Text = sbStatus.ToString();
                                }
                                UpdateActuatorStatus(true);
                            });
                        }

                        List<EdiabasThread.EcuFunctionResult> resultList = EdiabasThread.ExecuteEcuJobs(_ediabas, selectedJob.EcuFixedFuncStruct, null, false, currentPhase);
                        if (resultList == null)
                        {
                            executeFailed = true;
                            break;
                        }

                        if (phase == EcuFunctionStructs.EcuJob.PhaseType.Preset)
                        {
                            phase = EcuFunctionStructs.EcuJob.PhaseType.Main;
                        }

                        if (updateStatus && !statusUpdated)
                        {
                            statusUpdated = true;
                            lock (lockObject)
                            {
                                foreach (EdiabasThread.EcuFunctionResult ecuFunctionResult in resultList)
                                {
                                    AppendSbText(sbStatus, ecuFunctionResult.ResultString);
                                }
                            }

                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                lock (lockObject)
                                {
                                    _textBmwActuatorStatus.Text = sbStatus.ToString();
                                }
                                UpdateActuatorStatus(true);
                            });
                        }

                        // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionComponentTrigger.PhaseMain
                        if (currentPhase == EcuFunctionStructs.EcuJob.PhaseType.Main)
                        {
                            if (!_instanceData.Continuous)
                            {
                                if (!hasResetJobs)
                                {
                                    break;
                                }

                                for (;;)
                                {
                                    if (_instanceData.StopActuator)
                                    {
                                        break;
                                    }

                                    if (activation)
                                    {
                                        if (Stopwatch.GetTimestamp() - startTime > activationDuration * ActivityCommon.TickResolMs)
                                        {
                                            break;
                                        }
                                    }

                                    Thread.Sleep(100);
                                }
                                break;
                            }
                        }
                    }

                    if (!executeFailed)
                    {
                        // from: RheingoldSessionController.dll BMW.Rheingold.RheingoldSessionController.EcuFunctions.EcuFunctionComponentTrigger.PhaseReset
                        EcuFunctionStructs.EcuJob.PhaseType currentPhase = EcuFunctionStructs.EcuJob.PhaseType.Reset;
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }

                            string postOpText = selectedJob.EcuFixedFuncStruct.PostOp?.GetTitle(language);
                            if (!string.IsNullOrWhiteSpace(postOpText))
                            {
                                _textBmwActuatorStatus.Text = postOpText;
                            }

                            UpdateActuatorStatus(true);
                        });

                        List<EdiabasThread.EcuFunctionResult> resultList = EdiabasThread.ExecuteEcuJobs(_ediabas, selectedJob.EcuFixedFuncStruct, null,false, currentPhase);
                        if (resultList == null)
                        {
                            executeFailed = true;
                        }
                    }
                }
                catch (Exception)
                {
                    executeFailed = true;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    if (executeFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.bmw_actuator_operation_failed), Resource.String.alert_title_error);
                    }
                    else
                    {
                        if (_instanceData.AutoClose)
                        {
                            Finish();
                            return;
                        }
                    }

                    if (IsJobRunning())
                    {
                        _jobThread?.Join();
                    }

                    _instanceData.StopActuator = false;

                    UpdateActuatorStatus(true);
                });
            });
            _jobThread.Start();

            UpdateActuatorStatus(true);
        }
    }
}
