using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(
        Name = ActivityCommon.AppNameSpace + "." + nameof(VagAdaptionActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class VagAdaptionActivity : BaseActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public InstanceData()
            {
                SelectedChannelText = string.Empty;
                AdaptionValues = new string[MaxMeasValues];
                AdaptionUnits = new string[MaxMeasValues];
            }

            public int SelectedChannel { get; set; }
            public string SelectedChannelText { get; set; }
            public UInt64? AdaptionValueStart { get; set; }
            public UInt64? AdaptionValueNew { get; set; }
            public UInt64? AdaptionValueTest { get; set; }
            public bool UpdateAdaptionValueNew { get; set; }
            public byte[] AdaptionData { get; set; }
            public byte[] AdaptionDataNew { get; set; }
            public UInt64? CurrentWorkshopNumber { get; set; }
            public UInt64? CurrentImporterNumber { get; set; }
            public UInt64? CurrentEquipmentNumber { get; set; }
            public string[] AdaptionValues { get; set; }
            public string[] AdaptionUnits { get; set; }
            public bool TestAdaption { get; set; }
            public bool StoreAdaption { get; set; }
            public bool StopAdaption { get; set; }
            public bool AutoClose { get; set; }
            public bool EcuReset { get; set; }
            public bool AdaptionStored { get; set; }
        }

        // Intent extra
        public const string ExtraEcuName = "ecu_name";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraSimulationDir = "simulation_dir";
        public const string ExtraTraceDir = "trace_dir";
        public const string ExtraTraceAppend = "trace_append";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraElmWifiIp = "elmwifi_ip";
        public const string ExtraDeepObdWifiIp = "deepobdwifi_ip";

        private const int MaxMeasValues = 4;
        private const int ResetChannelNumber = 0;

        private enum JobStatus
        {
            Unknown,
            Ok,
            IllegalArguments,
            AccessDenied,
            ResetFailed,
        }

        public static XmlToolActivity.EcuInfo IntentEcuInfo { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private InputMethodManager _imm;
        private View _contentView;
        private ScrollView _scrollViewVagAdaption;
        private LinearLayout _layoutVagAdaption;
        private LinearLayout _layoutVagAdaptionChannel;
        private TextView _textViewVagAdaptionChannel;
        private Spinner _spinnerVagAdaptionChannel;
        private StringObjAdapter _spinnerVagAdaptionChannelAdapter;
        private LinearLayout _layoutVagAdaptionChannelNumber;
        private TextView _textViewVagAdaptionChannelNumber;
        private EditText _editTextVagAdaptionChannelNumber;
        private LinearLayout _layoutVagAdaptionInfo;
        private LinearLayout _layoutVagAdaptionComments;
        private TextView _textViewVagAdaptionCommentsTitle;
        private TextView _textVagViewAdaptionComments;
        private LinearLayout _layoutVagAdaptionMeasValues;
        private TextView[] _textViewVagAdaptionMeasTitles;
        private TextView[] _textViewAdaptionMeasValues;
        private LinearLayout _layoutVagAdaptionValues;
        private TextView _textViewVagAdaptionValueCurrentTitle;
        private TextView _textViewVagAdaptionValueCurrent;
        private TextView _textViewVagAdaptionValueNewTitle;
        private Spinner _spinnerVagAdaptionValueNew;
        private StringObjAdapter _spinnerVagAdaptionValueNewAdapter;
        private EditText _editTextVagAdaptionValueNew;
        private TextView _textViewVagAdaptionValueTestTitle;
        private TextView _textViewVagAdaptionValueTest;
        private LinearLayout _layoutVagAdaptionRepairShopCode;
        private LinearLayout _layoutVagAdaptionWorkshop;
        private TextView _textViewVagWorkshopNumberTitle;
        private EditText _editTextVagWorkshopNumber;
        private LinearLayout _layoutVagAdaptionImporterNumber;
        private TextView _textViewVagImporterNumberTitle;
        private EditText _editTextVagImporterNumber;
        private LinearLayout _layoutVagAdaptionEquipmentNumber;
        private TextView _textViewVagEquipmentNumberTitle;
        private EditText _editTextVagEquipmentNumber;
        private LinearLayout _layoutAdaptionOperation;
        private TextView _textViewVagAdaptionOperationTitle;
        private Button _buttonAdaptionRead;
        private Button _buttonAdaptionTest;
        private Button _buttonAdaptionStore;
        private Button _buttonAdaptionStop;
        private CheckBox _checkBoxEcuReset;
        private ActivityCommon _activityCommon;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private List<UdsFileReader.DataReader.DataInfo> _dataInfoAdaptionList;
        private List<UdsFileReader.UdsReader.ParseInfoAdp> _parseInfoAdaptionList;
        private bool _activityActive;
        private bool _ediabasJobAbort;
        private string _appDataDir;
        private string _ecuDir;
        private string _simulationDir;
        private string _traceDir;
        private bool _traceAppend;
        private string _deviceAddress;

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
            SetContentView(Resource.Layout.vag_adaption);

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

            SupportActionBar.Title = string.Format(GetString(Resource.String.vag_adaption_title_adaption), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);

            _scrollViewVagAdaption = FindViewById<ScrollView>(Resource.Id.scrollViewVagAdaption);
            _scrollViewVagAdaption.SetOnTouchListener(this);

            _layoutVagAdaption = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaption);
            _layoutVagAdaption.SetOnTouchListener(this);

            _layoutVagAdaptionChannel = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionChannel);
            _layoutVagAdaptionChannel.SetOnTouchListener(this);

            _textViewVagAdaptionChannel = FindViewById<TextView>(Resource.Id.textViewVagAdaptionChannel);
            _textViewVagAdaptionChannel.SetOnTouchListener(this);

            _spinnerVagAdaptionChannel = FindViewById<Spinner>(Resource.Id.spinnerVagAdaptionChannel);
            _spinnerVagAdaptionChannel.SetOnTouchListener(this);
            _spinnerVagAdaptionChannelAdapter = new StringObjAdapter(this);
            _spinnerVagAdaptionChannel.Adapter = _spinnerVagAdaptionChannelAdapter;
            _spinnerVagAdaptionChannel.ItemSelected += AdaptionChannelItemSelected;

            _layoutVagAdaptionChannelNumber = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionChannelNumber);
            _layoutVagAdaptionChannelNumber.SetOnTouchListener(this);

            _textViewVagAdaptionChannelNumber = FindViewById<TextView>(Resource.Id.textViewVagAdaptionChannelNumber);
            _textViewVagAdaptionChannelNumber.SetOnTouchListener(this);

            _editTextVagAdaptionChannelNumber = FindViewById<EditText>(Resource.Id.editTextVagAdaptionChannelNumber);
            _editTextVagAdaptionChannelNumber.EditorAction += AdaptionEditorAction;

            _layoutVagAdaptionInfo = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionInfo);
            _layoutVagAdaptionInfo.SetOnTouchListener(this);

            _layoutVagAdaptionComments = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionComments);
            _layoutVagAdaptionComments.SetOnTouchListener(this);

            _textViewVagAdaptionCommentsTitle = FindViewById<TextView>(Resource.Id.textViewVagAdaptionCommentsTitle);
            _textViewVagAdaptionCommentsTitle.SetOnTouchListener(this);

            _textVagViewAdaptionComments = FindViewById<TextView>(Resource.Id.textVagViewAdaptionComments);
            _textVagViewAdaptionComments.SetOnTouchListener(this);
            _textVagViewAdaptionComments.MovementMethod = new ScrollingMovementMethod();

            _layoutVagAdaptionMeasValues = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionMeasValues);
            _layoutVagAdaptionMeasValues.SetOnTouchListener(this);

            _textViewVagAdaptionMeasTitles = new TextView[MaxMeasValues];
            _textViewAdaptionMeasValues = new TextView[MaxMeasValues];

            _textViewVagAdaptionMeasTitles[0] = FindViewById<TextView>(Resource.Id.textViewVagAdaptionMeas1Title);
            _textViewVagAdaptionMeasTitles[0].SetOnTouchListener(this);

            _textViewAdaptionMeasValues[0] = FindViewById<TextView>(Resource.Id.textViewAdaptionMeas1Value);
            _textViewAdaptionMeasValues[0].SetOnTouchListener(this);

            _textViewVagAdaptionMeasTitles[1] = FindViewById<TextView>(Resource.Id.textViewVagAdaptionMeas2Title);
            _textViewVagAdaptionMeasTitles[1].SetOnTouchListener(this);

            _textViewAdaptionMeasValues[1] = FindViewById<TextView>(Resource.Id.textViewAdaptionMeas2Value);
            _textViewAdaptionMeasValues[1].SetOnTouchListener(this);

            _textViewVagAdaptionMeasTitles[2] = FindViewById<TextView>(Resource.Id.textViewVagAdaptionMeas3Title);
            _textViewVagAdaptionMeasTitles[2].SetOnTouchListener(this);

            _textViewAdaptionMeasValues[2] = FindViewById<TextView>(Resource.Id.textViewAdaptionMeas3Value);
            _textViewAdaptionMeasValues[2].SetOnTouchListener(this);

            _textViewVagAdaptionMeasTitles[3] = FindViewById<TextView>(Resource.Id.textViewVagAdaptionMeas4Title);
            _textViewVagAdaptionMeasTitles[3].SetOnTouchListener(this);

            _textViewAdaptionMeasValues[3] = FindViewById<TextView>(Resource.Id.textViewAdaptionMeas4Value);
            _textViewAdaptionMeasValues[3].SetOnTouchListener(this);

            _layoutVagAdaptionValues = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionValues);
            _layoutVagAdaptionValues.SetOnTouchListener(this);

            _textViewVagAdaptionValueCurrentTitle = FindViewById<TextView>(Resource.Id.textViewVagAdaptionValueCurrentTitle);
            _textViewVagAdaptionValueCurrentTitle.SetOnTouchListener(this);

            _textViewVagAdaptionValueCurrent = FindViewById<TextView>(Resource.Id.textViewVagAdaptionValueCurrent);
            _textViewVagAdaptionValueCurrent.SetOnTouchListener(this);

            _textViewVagAdaptionValueNewTitle = FindViewById<TextView>(Resource.Id.textViewVagAdaptionValueNewTitle);
            _textViewVagAdaptionValueNewTitle.SetOnTouchListener(this);

            _spinnerVagAdaptionValueNew = FindViewById<Spinner>(Resource.Id.spinnerVagAdaptionValueNew);
            _spinnerVagAdaptionValueNew.SetOnTouchListener(this);
            _spinnerVagAdaptionValueNewAdapter = new StringObjAdapter(this);
            _spinnerVagAdaptionValueNew.Adapter = _spinnerVagAdaptionValueNewAdapter;
            _spinnerVagAdaptionValueNew.ItemSelected += AdaptionValueNewItemSelected;

            _editTextVagAdaptionValueNew = FindViewById<EditText>(Resource.Id.editTextVagAdaptionValueNew);
            _editTextVagAdaptionValueNew.EditorAction += AdaptionEditorAction;

            _textViewVagAdaptionValueTestTitle = FindViewById<TextView>(Resource.Id.textViewVagAdaptionValueTestTitle);
            _textViewVagAdaptionValueTestTitle.SetOnTouchListener(this);

            _textViewVagAdaptionValueTest = FindViewById<TextView>(Resource.Id.textViewVagAdaptionValueTest);
            _textViewVagAdaptionValueTest.SetOnTouchListener(this);

            _layoutVagAdaptionRepairShopCode = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionRepairShopCode);
            _layoutVagAdaptionRepairShopCode.SetOnTouchListener(this);

            _layoutVagAdaptionWorkshop = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionWorkshop);
            _layoutVagAdaptionWorkshop.SetOnTouchListener(this);

            _textViewVagWorkshopNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagWorkshopNumberTitle);
            _textViewVagWorkshopNumberTitle.SetOnTouchListener(this);

            _editTextVagWorkshopNumber = FindViewById<EditText>(Resource.Id.editTextVagWorkshopNumber);
            _editTextVagWorkshopNumber.EditorAction += AdaptionEditorAction;

            _layoutVagAdaptionImporterNumber = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionImporterNumber);
            _layoutVagAdaptionImporterNumber.SetOnTouchListener(this);

            _textViewVagImporterNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagImporterNumberTitle);
            _textViewVagImporterNumberTitle.SetOnTouchListener(this);

            _editTextVagImporterNumber = FindViewById<EditText>(Resource.Id.editTextVagImporterNumber);
            _editTextVagImporterNumber.EditorAction += AdaptionEditorAction;

            _layoutVagAdaptionEquipmentNumber = FindViewById<LinearLayout>(Resource.Id.layoutVagAdaptionEquipmentNumber);
            _layoutVagAdaptionEquipmentNumber.SetOnTouchListener(this);

            _textViewVagEquipmentNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagEquipmentNumberTitle);
            _textViewVagEquipmentNumberTitle.SetOnTouchListener(this);

            _editTextVagEquipmentNumber = FindViewById<EditText>(Resource.Id.editTextVagEquipmentNumber);
            _editTextVagEquipmentNumber.EditorAction += AdaptionEditorAction;

            _layoutAdaptionOperation = FindViewById<LinearLayout>(Resource.Id.layoutAdaptionOperation);
            _layoutAdaptionOperation.SetOnTouchListener(this);

            _textViewVagAdaptionOperationTitle = FindViewById<TextView>(Resource.Id.textViewVagAdaptionOperationTitle);
            _textViewVagAdaptionOperationTitle.SetOnTouchListener(this);

            bool is1281Ecu = XmlToolActivity.Is1281Ecu(_ecuInfo);
            _buttonAdaptionRead = FindViewById<Button>(Resource.Id.buttonAdaptionRead);
            _buttonAdaptionRead.SetOnTouchListener(this);
            _buttonAdaptionRead.Click += (sender, args) =>
            {
                ExecuteAdaptionRequest();
            };

            _buttonAdaptionTest = FindViewById<Button>(Resource.Id.buttonAdaptionTest);
            _buttonAdaptionTest.SetOnTouchListener(this);
            _buttonAdaptionTest.Click += (sender, args) =>
            {
                _instanceData.TestAdaption = true;
            };

            _buttonAdaptionStore = FindViewById<Button>(Resource.Id.buttonAdaptionStore);
            _buttonAdaptionStore.SetOnTouchListener(this);
            _buttonAdaptionStore.Click += (sender, args) =>
            {
                _instanceData.StoreAdaption = true;
                if (is1281Ecu && !IsJobRunning())
                {
                    ExecuteAdaptionRequest();
                }
            };

            _buttonAdaptionStop = FindViewById<Button>(Resource.Id.buttonAdaptionStop);
            _buttonAdaptionStop.SetOnTouchListener(this);
            _buttonAdaptionStop.Click += (sender, args) =>
            {
                _instanceData.StopAdaption = true;
            };

            _checkBoxEcuReset = FindViewById<CheckBox>(Resource.Id.checkBoxEcuReset);
            _checkBoxEcuReset.SetOnTouchListener(this);
            _checkBoxEcuReset.Click += (sender, args) =>
            {
                ReadAdaptionEditors();
            };

            UpdateAdaptionChannelList();
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
                _instanceData.StopAdaption = true;
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
                        _instanceData.StopAdaption = true;
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
                    ReadAdaptionEditors();
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

        private bool IsValueName(UdsFileReader.UdsReader.ParseInfoAdp parseInfoAdp)
        {
            bool isValueName = false;
            if (parseInfoAdp != null)
            {
                UdsFileReader.UdsReader.DataType dataType =
                    (UdsFileReader.UdsReader.DataType)(parseInfoAdp.DataTypeEntry.DataTypeId & UdsFileReader.UdsReader.DataTypeMaskEnum);
                if (dataType == UdsFileReader.UdsReader.DataType.ValueName)
                {
                    isValueName = true;
                }
            }

            return isValueName;
        }

        private bool IsResetChannel()
        {
            return !XmlToolActivity.IsUdsEcu(_ecuInfo) && _instanceData.SelectedChannel == ResetChannelNumber;
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

        private void AdaptionChannelItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            UpdateAdaption();
        }

        private void UpdateInfoAdaptionList()
        {
            List<UdsFileReader.DataReader.DataInfo> dataInfoAdaptionList = null;
            List<UdsFileReader.UdsReader.ParseInfoAdp> parseInfoAdaptionList = null;
            if (XmlToolActivity.IsUdsEcu(_ecuInfo))
            {
                string udsFileName = _ecuInfo.VagUdsFileName;
                if (!string.IsNullOrEmpty(udsFileName))
                {
                    UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(udsFileName);
                    if (udsReader != null)
                    {
                        parseInfoAdaptionList =  udsReader.GetAdpParseInfoList(udsFileName);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_ecuInfo.VagDataFileName))
                {
                    UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_ecuInfo.VagDataFileName);
                    if (udsReader != null)
                    {
                        dataInfoAdaptionList = udsReader.DataReader.ExtractDataType(_ecuInfo.VagDataFileName, UdsFileReader.DataReader.DataType.Adaption);
                    }
                }
            }
            _parseInfoAdaptionList = parseInfoAdaptionList;
            _dataInfoAdaptionList = dataInfoAdaptionList;
        }

        private void UpdateAdaptionChannelList()
        {
            int selectedChannel = _instanceData.SelectedChannel;
            int selection = 0;

            _spinnerVagAdaptionChannelAdapter.Items.Clear();

            if (XmlToolActivity.IsUdsEcu(_ecuInfo))
            {
                int index = 0;
                if (_parseInfoAdaptionList != null)
                {
                    _spinnerVagAdaptionChannelAdapter.Items.Add(new StringObjType(GetString(Resource.String.vag_adaption_channel_select), -1));
                    foreach (UdsFileReader.UdsReader.ParseInfoAdp parseInfoAdp in _parseInfoAdaptionList)
                    {
                        StringBuilder sbDispText = new StringBuilder();
                        if (!string.IsNullOrEmpty(parseInfoAdp.DataIdString))
                        {
                            sbDispText.Append(parseInfoAdp.DataIdString);
                        }
                        if (!string.IsNullOrEmpty(parseInfoAdp.Name))
                        {
                            if (sbDispText.Length > 0)
                            {
                                sbDispText.Append(" ");
                            }
                            sbDispText.Append(parseInfoAdp.Name);
                            if (parseInfoAdp.SubItem.HasValue)
                            {
                                sbDispText.Append(string.Format(CultureInfo.InvariantCulture, " / {0}", parseInfoAdp.SubItem.Value));
                            }
                        }
                        string displayText = sbDispText.ToString();
                        _spinnerVagAdaptionChannelAdapter.Items.Add(new StringObjType(displayText, index));

                        if (index == selectedChannel)
                        {
                            selection = index;
                        }

                        index++;
                    }
                }
            }
            else
            {
                int index = 0;
                if (_dataInfoAdaptionList != null)
                {
                    bool resetChannelPresent = false;
                    _spinnerVagAdaptionChannelAdapter.Items.Add(new StringObjType(GetString(Resource.String.vag_adaption_channel_select), -1));
                    foreach (UdsFileReader.DataReader.DataInfo dataInfo in _dataInfoAdaptionList)
                    {
                        if (dataInfo.Value1.HasValue && dataInfo.Value2.HasValue && dataInfo.Value2.Value == 0 && dataInfo.TextArray.Length > 0)
                        {
                            string text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", dataInfo.Value1.Value, dataInfo.TextArray[0]);
                            _spinnerVagAdaptionChannelAdapter.Items.Add(new StringObjType(text, dataInfo.Value1.Value));

                            if (dataInfo.Value1.Value == ResetChannelNumber)
                            {
                                resetChannelPresent = true;
                            }
                            else
                            {
                                if (dataInfo.Value1.Value == selectedChannel)
                                {
                                    selection = index;
                                }
                            }

                            index++;
                        }
                    }

                    if (!resetChannelPresent)
                    {
                        string text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", ResetChannelNumber, GetString(Resource.String.vag_adaption_channel_reset));
                        _spinnerVagAdaptionChannelAdapter.Items.Insert(1, new StringObjType(text, ResetChannelNumber));
                    }
                }
            }

            _spinnerVagAdaptionChannelAdapter.NotifyDataSetChanged();
            _spinnerVagAdaptionChannel.SetSelection(selection);

            UpdateAdaption();
        }

        private void UpdateAdaption()
        {
            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            int channelIndex = _spinnerVagAdaptionChannel.SelectedItemPosition;
            if (channelIndex >= 0 && channelIndex < _spinnerVagAdaptionChannelAdapter.Items.Count)
            {
                StringObjType item = _spinnerVagAdaptionChannelAdapter.Items[channelIndex];
                int channel = (int)item.Data;
                if (channel >= 0 || isUdsEcu)
                {
                    _instanceData.SelectedChannel = channel;
                    _instanceData.SelectedChannelText = item.Text;
                }
            }

            _instanceData.CurrentWorkshopNumber = _ecuInfo.VagWorkshopNumber;
            _instanceData.CurrentImporterNumber = _ecuInfo.VagImporterNumber;
            _instanceData.CurrentEquipmentNumber = _ecuInfo.VagEquipmentNumber;

            UpdateAdaptionInfo();
        }

        private void AdaptionValueNewItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            ReadAdaptionEditors();
        }

        private void AdaptionEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            switch (e.ActionId)
            {
                case ImeAction.Go:
                case ImeAction.Send:
                case ImeAction.Next:
                case ImeAction.Done:
                case ImeAction.Previous:
                    ReadAdaptionEditors();
                    HideKeyboard();
                    break;
            }
        }

        private void ReadAdaptionEditors()
        {
            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            bool dataChanged = false;
            if (_editTextVagAdaptionChannelNumber.Enabled)
            {
                try
                {
                    if (UInt64.TryParse(_editTextVagAdaptionChannelNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                    {
                        if (value <= 0xFF)
                        {
                            if (_instanceData.SelectedChannel != (int) value)
                            {
                                _instanceData.SelectedChannel = (int)value;
                                dataChanged = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (isUdsEcu)
            {
                if ((_editTextVagAdaptionValueNew.Enabled || _spinnerVagAdaptionValueNew.Enabled) && _instanceData.AdaptionDataNew != null)
                {
                    int selectedChannel = _instanceData.SelectedChannel;
                    if (selectedChannel >= 0 && selectedChannel < _parseInfoAdaptionList.Count)
                    {
                        UdsFileReader.UdsReader.ParseInfoAdp parseInfoAdp = _parseInfoAdaptionList[selectedChannel];
                        if (parseInfoAdp != null)
                        {
                            if (_editTextVagAdaptionValueNew.Enabled)
                            {
                                string newValueString = _editTextVagAdaptionValueNew.Text;
                                string valueString = parseInfoAdp.DataTypeEntry.ToString(CultureInfo.InvariantCulture, _instanceData.AdaptionData, newValueString,
                                    out string _, out object _, out UInt32? _, out byte[] newData);
                                if (newData != null && valueString != null)
                                {
                                    _instanceData.AdaptionDataNew = newData;
                                    _editTextVagAdaptionValueNew.Text = valueString;
                                }
                                else
                                {
                                    // restore old value
                                    _instanceData.UpdateAdaptionValueNew = true;
                                    dataChanged = true;
                                }
                            }

                            if (_spinnerVagAdaptionValueNew.Enabled)
                            {
                                try
                                {
                                    int valueIndex = _spinnerVagAdaptionValueNew.SelectedItemPosition;
                                    if (valueIndex >= 0 && valueIndex < _spinnerVagAdaptionValueNewAdapter.Items.Count)
                                    {
                                        UInt64 selectedValue = (UInt64)_spinnerVagAdaptionValueNewAdapter.Items[valueIndex].Data;
                                        string valueString = parseInfoAdp.DataTypeEntry.ToString(CultureInfo.InvariantCulture, _instanceData.AdaptionData, selectedValue,
                                            out string _, out object dataValueObject, out UInt32? _, out byte[] newData);
                                        bool restoreValue = true;
                                        if (newData != null && valueString != null)
                                        {
                                            if (dataValueObject is UInt64 dataValueUint)
                                            {
                                                if (selectedValue == dataValueUint)
                                                {
                                                    _instanceData.AdaptionDataNew = newData;
                                                    restoreValue = false;
                                                }
                                            }
                                        }
                                        if (restoreValue)
                                        {
                                            // restore old value
                                            _instanceData.UpdateAdaptionValueNew = true;
                                            dataChanged = true;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_editTextVagAdaptionValueNew.Enabled && _instanceData.AdaptionValueNew.HasValue)
                {
                    try
                    {
                        if (UInt64.TryParse(_editTextVagAdaptionValueNew.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                        {
                            if (value <= 0xFFFF)
                            {
                                if (_instanceData.AdaptionValueNew.Value != value)
                                {
                                    _instanceData.AdaptionValueNew = value;
                                    if (_editTextVagAdaptionValueNew.Enabled)
                                    {
                                        _instanceData.UpdateAdaptionValueNew = true;
                                    }
                                    dataChanged = true;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            if (_layoutVagAdaptionRepairShopCode.Visibility == ViewStates.Visible)
            {
                try
                {
                    if (_instanceData.CurrentWorkshopNumber.HasValue && _editTextVagWorkshopNumber.Enabled)
                    {
                        if (UInt64.TryParse(_editTextVagWorkshopNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 valueWorkshop))
                        {
                            if (valueWorkshop <= VagCodingActivity.WorkshopNumberMax)
                            {
                                if (_instanceData.CurrentWorkshopNumber.Value != valueWorkshop)
                                {
                                    _instanceData.CurrentWorkshopNumber = valueWorkshop;
                                    dataChanged = true;
                                }
                            }
                            else
                            {
                                dataChanged = true;
                            }
                        }
                    }

                    if (_instanceData.CurrentImporterNumber.HasValue && _editTextVagImporterNumber.Enabled)
                    {
                        if (UInt64.TryParse(_editTextVagImporterNumber.Text, NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out UInt64 valueImporter))
                        {
                            if (valueImporter <= VagCodingActivity.ImporterNumberMax)
                            {
                                if (_instanceData.CurrentImporterNumber.Value != valueImporter)
                                {
                                    _instanceData.CurrentImporterNumber = valueImporter;
                                    dataChanged = true;
                                }
                            }
                            else
                            {
                                dataChanged = true;
                            }
                        }
                    }

                    if (_instanceData.CurrentEquipmentNumber.HasValue && _editTextVagEquipmentNumber.Enabled)
                    {
                        if (UInt64.TryParse(_editTextVagEquipmentNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 valueEquipment))
                        {
                            if (valueEquipment <= VagCodingActivity.EquipmentNumberMax)
                            {
                                if (_instanceData.CurrentEquipmentNumber.Value != valueEquipment)
                                {
                                    _instanceData.CurrentEquipmentNumber = valueEquipment;
                                    dataChanged = true;
                                }
                            }
                            else
                            {
                                dataChanged = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (_checkBoxEcuReset.Visibility == ViewStates.Visible)
            {
                _instanceData.EcuReset = _checkBoxEcuReset.Checked;
            }

            if (dataChanged)
            {
                UpdateAdaptionInfo();
            }
        }

        private void UpdateAdaptionInfo()
        {
            UpdateAdaptionText();

            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            if (isUdsEcu)
            {
                StringBuilder sbAdaptionComment = new StringBuilder();
                if (_parseInfoAdaptionList != null)
                {
                    int selectedChannel = _instanceData.SelectedChannel;
                    if (selectedChannel >= 0 && selectedChannel < _parseInfoAdaptionList.Count)
                    {
                        UdsFileReader.UdsReader.ParseInfoAdp parseInfoAdp = _parseInfoAdaptionList[selectedChannel];
                        if (parseInfoAdp != null)
                        {
                            if (!string.IsNullOrEmpty(_instanceData.SelectedChannelText))
                            {
                                sbAdaptionComment.Append(_instanceData.SelectedChannelText);
                                sbAdaptionComment.Append("\r\n");
                            }

                            sbAdaptionComment.Append(GetString(Resource.String.vag_adaption_service_id));
                            sbAdaptionComment.Append(string.Format(CultureInfo.InvariantCulture, " {0:00000}", parseInfoAdp.ServiceId));
                            if (parseInfoAdp.DataTypeEntry.NameDetail != null)
                            {
                                sbAdaptionComment.Append("\r\n");
                                sbAdaptionComment.Append(parseInfoAdp.DataTypeEntry.NameDetail);
                            }
                        }
                    }
                }

                string adaptionComment = sbAdaptionComment.ToString();
                _layoutVagAdaptionComments.Visibility = !string.IsNullOrWhiteSpace(adaptionComment) ? ViewStates.Visible : ViewStates.Gone;
                _textVagViewAdaptionComments.Text = adaptionComment;

                for (int i = 0; i < _textViewVagAdaptionMeasTitles.Length; i++)
                {
                    _textViewVagAdaptionMeasTitles[i].Text = string.Empty;
                    _textViewVagAdaptionMeasTitles[i].Visibility = ViewStates.Gone;
                    _textViewAdaptionMeasValues[i].Visibility = ViewStates.Gone;
                }
            }
            else
            {
                bool resetChannel = IsResetChannel();
                string[] measTitles = new string[MaxMeasValues];
                StringBuilder sbAdaptionComment = new StringBuilder();
                if (_dataInfoAdaptionList != null)
                {
                    foreach (UdsFileReader.DataReader.DataInfo dataInfo in _dataInfoAdaptionList)
                    {
                        if (dataInfo.Value1.HasValue && dataInfo.Value2.HasValue &&
                            dataInfo.Value1.Value == _instanceData.SelectedChannel && dataInfo.TextArray.Length > 0)
                        {
                            int index = dataInfo.Value2.Value;

                            StringBuilder sbLine = new StringBuilder();
                            foreach (string text in dataInfo.TextArray)
                            {
                                if (sbLine.Length > 0)
                                {
                                    sbLine.Append(" ");
                                }
                                sbLine.Append(text);
                            }

                            if (index > 0 && index <= 4)
                            {
                                measTitles[index - 1] = sbLine.ToString();
                            }
                            else if (index > 4)
                            {
                                if (sbAdaptionComment.Length > 0)
                                {
                                    sbAdaptionComment.Append("\r\n");
                                }
                                sbAdaptionComment.Append(sbLine);
                            }
                        }
                    }
                }

                string adaptionComment = sbAdaptionComment.ToString();
                if (resetChannel && string.IsNullOrWhiteSpace(adaptionComment))
                {
                    adaptionComment = GetString(Resource.String.vag_adaption_channel_reset_info);
                }

                int selection = 0;
                int itemIndex = 0;
                foreach (StringObjType stringObjType in _spinnerVagAdaptionChannelAdapter.Items)
                {
                    int channel = (int)stringObjType.Data;
                    if (channel == _instanceData.SelectedChannel && !resetChannel)
                    {
                        selection = itemIndex;
                    }
                    itemIndex++;
                }
                _spinnerVagAdaptionChannel.SetSelection(selection);

                _layoutVagAdaptionComments.Visibility = !string.IsNullOrWhiteSpace(adaptionComment) ? ViewStates.Visible : ViewStates.Gone;
                _textVagViewAdaptionComments.Text = adaptionComment;

                for (int i = 0; i < measTitles.Length; i++)
                {
                    _textViewVagAdaptionMeasTitles[i].Text =
                        string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_meas_value_title), i + 1, measTitles[i] ?? string.Empty);
                    _textViewVagAdaptionMeasTitles[i].Visibility = ViewStates.Visible;
                    _textViewAdaptionMeasValues[i].Visibility = ViewStates.Visible;
                }
            }

            _layoutVagAdaptionChannelNumber.Visibility = XmlToolActivity.IsUdsEcu(_ecuInfo) ? ViewStates.Gone : ViewStates.Visible;
            _layoutVagAdaptionRepairShopCode.Visibility = XmlToolActivity.Is1281Ecu(_ecuInfo) ? ViewStates.Gone : ViewStates.Visible;
            _layoutVagAdaptionWorkshop.Visibility = _instanceData.CurrentWorkshopNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagAdaptionImporterNumber.Visibility = _instanceData.CurrentImporterNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagAdaptionEquipmentNumber.Visibility = _instanceData.CurrentEquipmentNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _checkBoxEcuReset.Visibility = isUdsEcu ? ViewStates.Visible : ViewStates.Gone;
        }

        private void UpdateAdaptionText(bool cyclicUpdate = false)
        {
            string adaptionChannelNumber = string.Empty;
            string adaptionValueStartTitle = GetString(Resource.String.vag_adaption_value_current_title);
            string adaptionValueStart = string.Empty;
            string adaptionValueNew = string.Empty;
            string adaptionValueTest = string.Empty;
            string workshopNumberTitle = string.Empty;
            string importerNumberTitle = string.Empty;
            string equipmentNumberTitle = string.Empty;
            bool jobRunning = IsJobRunning();
            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            bool is1281Ecu = XmlToolActivity.Is1281Ecu(_ecuInfo);
            UdsFileReader.UdsReader.ParseInfoAdp parseInfoAdp = null;
            bool isValueName = false;
            bool resetChannel = IsResetChannel();
            bool validChannel = _instanceData.SelectedChannel >= 0;
            bool operationActive = _instanceData.TestAdaption || _instanceData.StoreAdaption || _instanceData.StoreAdaption;
            InputTypes inputType = InputTypes.ClassNumber;
            bool validData = false;

            string codingTextWorkshop = string.Empty;
            string codingTextImporter = string.Empty;
            string codingTextEquipment = string.Empty;
            bool editTextWorkshop = isUdsEcu;
            bool editTextImporter = isUdsEcu;
            bool editTextEquipment = isUdsEcu;

            try
            {
                adaptionChannelNumber = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.SelectedChannel);
                if (isUdsEcu)
                {
                    int selectedChannel = _instanceData.SelectedChannel;
                    if (selectedChannel >= 0 && selectedChannel < _parseInfoAdaptionList.Count)
                    {
                        parseInfoAdp = _parseInfoAdaptionList[selectedChannel];
                        if (parseInfoAdp != null)
                        {
                            isValueName = IsValueName(parseInfoAdp);
                        }
                    }
                }

                if (jobRunning)
                {
                    if (isUdsEcu)
                    {
                        if (parseInfoAdp != null && _instanceData.AdaptionData != null)
                        {
                            string valueString = parseInfoAdp.DataTypeEntry.ToString(CultureInfo.InvariantCulture, _instanceData.AdaptionData, null,
                                out string unitText, out object dataValueObject, out UInt32? usedBitLength, out byte[] _);

                            StringBuilder sb = new StringBuilder();
                            sb.Append(valueString);

                            if (!string.IsNullOrEmpty(unitText))
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append(" ");
                                }

                                sb.Append(unitText);
                            }

                            if (usedBitLength.HasValue)
                            {
                                if (usedBitLength.Value % 8 != 0)
                                {
                                    adaptionValueStartTitle += string.Format(CultureInfo.InvariantCulture, ", {0} Bit", usedBitLength.Value);
                                }
                                else
                                {
                                    adaptionValueStartTitle += string.Format(CultureInfo.InvariantCulture, ", {0} Byte", usedBitLength.Value / 8);
                                }
                            }

                            adaptionValueStart = sb.ToString();
                            adaptionValueNew = valueString;
                            inputType = ActivityCommon.ConvertVagUdsDataTypeToInputType(parseInfoAdp.DataTypeEntry.DataTypeId);
                            validData = valueString != null;
                            if (isValueName && _instanceData.UpdateAdaptionValueNew)
                            {
                                UInt64? currentValue = null;
                                if (dataValueObject is UInt64 dataValueUint)
                                {
                                    currentValue = dataValueUint;
                                }

                                int selection = 0;
                                byte[] testData = new byte[_instanceData.AdaptionData.Length];
                                Array.Copy(_instanceData.AdaptionData, testData, testData.Length);
                                _spinnerVagAdaptionValueNewAdapter.Items.Clear();
                                int index = 0;
                                for (UInt64 testValue = 0;; testValue++)
                                {
                                    string testValueString = parseInfoAdp.DataTypeEntry.ToString(CultureInfo.InvariantCulture, testData, testValue,
                                        out string _, out object testValueSet, out UInt32? _, out byte[] newData, true);
                                    bool setDataValid = false;
                                    if (testValueSet is UInt64 testValueSetUint)
                                    {
                                        if (newData != null && testValue == testValueSetUint)
                                        {
                                            setDataValid = true;
                                        }
                                    }

                                    if (!setDataValid)
                                    {
                                        break;
                                    }

                                    if (!string.IsNullOrEmpty(testValueString))
                                    {
                                        _spinnerVagAdaptionValueNewAdapter.Items.Add(new StringObjType(testValueString, testValue));
                                        if (currentValue.HasValue && currentValue == testValue)
                                        {
                                            selection = index;
                                        }
                                    }

                                    index++;
                                }

                                _spinnerVagAdaptionValueNewAdapter.NotifyDataSetChanged();
                                _spinnerVagAdaptionValueNew.SetSelection(selection);
                            }
                        }
                    }
                    else
                    {
                        if (_instanceData.AdaptionValueStart != null)
                        {
                            adaptionValueStart = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.AdaptionValueStart.Value);
                        }
                        if (_instanceData.AdaptionValueNew != null)
                        {
                            adaptionValueNew = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.AdaptionValueNew.Value);
                            validData = true;
                        }
                        if (_instanceData.AdaptionValueTest != null)
                        {
                            adaptionValueTest = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.AdaptionValueTest.Value);
                        }
                    }
                }
                else
                {
                    // job not running
                    _spinnerVagAdaptionValueNewAdapter.Items.Clear();
                    _spinnerVagAdaptionValueNewAdapter.NotifyDataSetChanged();
                }

                if (_instanceData.CurrentWorkshopNumber.HasValue)
                {
                    ulong currentWorkshopNumber = _instanceData.CurrentWorkshopNumber.Value & VagCodingActivity.WorkshopNumberMask;
                    if (_ecuInfo.VagWorkshopNumber.HasValue &&
                        (_ecuInfo.VagWorkshopNumber < 1 || _ecuInfo.VagWorkshopNumber > VagCodingActivity.WorkshopNumberMax))
                    {
                        editTextWorkshop = true;
                    }

                    codingTextWorkshop = string.Format(CultureInfo.InvariantCulture, "{0}", currentWorkshopNumber);
                }

                if (_instanceData.CurrentImporterNumber.HasValue)
                {
                    ulong currentImporterNumber = _instanceData.CurrentImporterNumber.Value & VagCodingActivity.ImporterNumberMask;
                    if (_ecuInfo.VagImporterNumber.HasValue && (_ecuInfo.VagImporterNumber < 1 || _ecuInfo.VagImporterNumber > VagCodingActivity.ImporterNumberMax))
                    {
                        editTextImporter = true;
                    }

                    codingTextImporter = string.Format(CultureInfo.InvariantCulture, "{0}", currentImporterNumber);
                }

                if (_instanceData.CurrentEquipmentNumber.HasValue)
                {
                    ulong currentEquipmentNumber = _instanceData.CurrentEquipmentNumber.Value & VagCodingActivity.EquipmentNumberMask;
                    if (_ecuInfo.VagEquipmentNumber.HasValue &&
                        (_ecuInfo.VagEquipmentNumber < 1 || _ecuInfo.VagEquipmentNumber > VagCodingActivity.EquipmentNumberMax))
                    {
                        editTextEquipment = true;
                    }

                    codingTextEquipment = string.Format(CultureInfo.InvariantCulture, "{0}", currentEquipmentNumber);
                }

                workshopNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_workshop_number_title), 0, VagCodingActivity.WorkshopNumberMax);
                importerNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_importer_number_title), 0, VagCodingActivity.ImporterNumberMax);
                equipmentNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_equipment_number_title), 0, VagCodingActivity.EquipmentNumberMax);
            }
            catch (Exception)
            {
                // ignored
            }

            _spinnerVagAdaptionChannel.Enabled = !jobRunning;
            _editTextVagAdaptionChannelNumber.Enabled = !jobRunning && !isUdsEcu;
            _editTextVagAdaptionChannelNumber.Text = adaptionChannelNumber;
            _textViewVagAdaptionValueCurrentTitle.Text = adaptionValueStartTitle;
            _textViewVagAdaptionValueCurrent.Text = adaptionValueStart;

            bool enableAdaptionNew = jobRunning && !resetChannel && validData;
            if (isValueName)
            {
                _spinnerVagAdaptionValueNew.Visibility = ViewStates.Visible;
                _editTextVagAdaptionValueNew.Visibility = ViewStates.Gone;
                _spinnerVagAdaptionValueNew.Enabled = enableAdaptionNew;
                _editTextVagAdaptionValueNew.Enabled = false;
            }
            else
            {
                _spinnerVagAdaptionValueNew.Visibility = ViewStates.Gone;
                _editTextVagAdaptionValueNew.Visibility = ViewStates.Visible;
                _spinnerVagAdaptionValueNew.Enabled = false;
                _editTextVagAdaptionValueNew.Enabled = enableAdaptionNew;
            }

            if (_editTextVagAdaptionValueNew.Enabled)
            {
                if (_instanceData.UpdateAdaptionValueNew)
                {
                    _instanceData.UpdateAdaptionValueNew = false;
                    _editTextVagAdaptionValueNew.Text = adaptionValueNew;
                    _editTextVagAdaptionValueNew.InputType = inputType;
                }
            }
            else
            {
                _editTextVagAdaptionValueNew.Text = adaptionValueNew;
            }

            _textViewVagAdaptionValueTestTitle.Visibility = isUdsEcu ? ViewStates.Gone : ViewStates.Visible;
            _textViewVagAdaptionValueTest.Visibility = _textViewVagAdaptionValueTestTitle.Visibility;
            _textViewVagAdaptionValueTest.Text = adaptionValueTest;

            for (int i = 0; i < _textViewAdaptionMeasValues.Length; i++)
            {
                string text = string.Empty;
                if (jobRunning)
                {
                    text = _instanceData.AdaptionValues[i] ?? string.Empty;
                    if (!string.IsNullOrEmpty(_instanceData.AdaptionUnits[i]))
                    {
                        text += " " + _instanceData.AdaptionUnits[i];
                    }
                }
                _textViewAdaptionMeasValues[i].Text = text;
            }

            if (!cyclicUpdate)
            {
                _textViewVagWorkshopNumberTitle.Text = workshopNumberTitle;
                _editTextVagWorkshopNumber.Text = codingTextWorkshop;

                _textViewVagImporterNumberTitle.Text = importerNumberTitle;
                _editTextVagImporterNumber.Text = codingTextImporter;

                _textViewVagEquipmentNumberTitle.Text = equipmentNumberTitle;
                _editTextVagEquipmentNumber.Text = codingTextEquipment;
            }

            _editTextVagWorkshopNumber.Enabled = editTextWorkshop && !cyclicUpdate;
            _editTextVagImporterNumber.Enabled = editTextImporter && !cyclicUpdate;
            _editTextVagEquipmentNumber.Enabled = editTextEquipment && !cyclicUpdate;

            if (isUdsEcu)
            {
                _buttonAdaptionRead.Enabled = !jobRunning && validChannel;
                _buttonAdaptionTest.Enabled = false;
                _buttonAdaptionStore.Enabled = validChannel && jobRunning && !operationActive && _instanceData.AdaptionDataNew != null;
            }
            else
            {
                if (is1281Ecu && resetChannel)
                {
                    _buttonAdaptionRead.Enabled = false;
                    _buttonAdaptionTest.Enabled = false;
                    _buttonAdaptionStore.Enabled = !operationActive;
                }
                else
                {
                    _buttonAdaptionRead.Enabled = !jobRunning;
                    _buttonAdaptionTest.Enabled = jobRunning && !operationActive && _instanceData.AdaptionValueStart != null && !resetChannel;
                    _buttonAdaptionStore.Enabled = jobRunning && !operationActive && _instanceData.AdaptionValueTest != null;
                }
            }
            _buttonAdaptionTest.Visibility = isUdsEcu ? ViewStates.Gone : ViewStates.Visible;
            _buttonAdaptionStop.Enabled = jobRunning && !operationActive;
            _checkBoxEcuReset.Enabled = _instanceData.AdaptionStored;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
            _editTextVagAdaptionChannelNumber.ClearFocus();
            _editTextVagAdaptionValueNew.ClearFocus();
            _editTextVagWorkshopNumber.ClearFocus();
            _editTextVagImporterNumber.ClearFocus();
            _editTextVagEquipmentNumber.ClearFocus();
        }

        private JobStatus CheckAdaptionResult()
        {
            JobStatus jobStatus = JobStatus.Unknown;
            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
            if (resultSets != null && resultSets.Count >= 1)
            {
                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[0];
                if (resultDict.TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                {
                    if (resultData.OpData is string)
                    {
                        string result = (string)resultData.OpData;
                        if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            jobStatus = JobStatus.Ok;
                        }
                        else if (result.Contains("ERROR_ARGUMENT", StringComparison.OrdinalIgnoreCase))
                        {
                            jobStatus = JobStatus.IllegalArguments;
                        }
                        else if (result.Contains("ERROR_NRC_SecurityAccessDenied", StringComparison.OrdinalIgnoreCase))
                        {
                            jobStatus = JobStatus.AccessDenied;
                        }
                    }
                }
            }

            return jobStatus;
        }

        private byte[] GetRepairShopCodeData()
        {
            byte[] repairShopCodeData = new byte[6];
            byte[] workShopData = BitConverter.GetBytes(_instanceData.CurrentWorkshopNumber ?? 0 & VagCodingActivity.WorkshopNumberMask);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(workShopData);
            }

            byte[] importerNumberData = BitConverter.GetBytes((_instanceData.CurrentImporterNumber ?? 0 & VagCodingActivity.ImporterNumberMask) << 1);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(importerNumberData);
            }

            byte[] equipmentNumberData = BitConverter.GetBytes((_instanceData.CurrentEquipmentNumber ?? 0 & VagCodingActivity.EquipmentNumberMask) << 3);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(equipmentNumberData);
            }

            for (int i = 0; i < 3; i++)
            {
                repairShopCodeData[i + 0] |= equipmentNumberData[i + 5];
            }
            for (int i = 0; i < 2; i++)
            {
                repairShopCodeData[i + 2] |= importerNumberData[i + 6];
            }
            for (int i = 0; i < 3; i++)
            {
                repairShopCodeData[i + 3] |= workShopData[i + 5];
            }

            return repairShopCodeData;
        }

        private void ExecuteAdaptionRequest()
        {
            bool valuesValid = true;
            if (_instanceData.CurrentWorkshopNumber != null && _instanceData.CurrentImporterNumber != null && _instanceData.CurrentEquipmentNumber != null)
            {
                valuesValid = _instanceData.CurrentWorkshopNumber.Value > 0 && _instanceData.CurrentWorkshopNumber.Value <= VagCodingActivity.WorkshopNumberMax &&
                              _instanceData.CurrentImporterNumber.Value > 0 && _instanceData.CurrentImporterNumber.Value <= VagCodingActivity.ImporterNumberMax &&
                              _instanceData.CurrentEquipmentNumber.Value > 0 && _instanceData.CurrentEquipmentNumber.Value <= VagCodingActivity.EquipmentNumberMax;
            }

            if (!valuesValid)
            {
                StringBuilder sbMessage = new StringBuilder();
                sbMessage.Append(GetString(Resource.String.vag_coding_write_values_invalid));
                bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
                if (!isUdsEcu)
                {
                    sbMessage.Append("\r\n");
                    sbMessage.Append(GetString(Resource.String.vag_adaption_change_coding_first));
                }

                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        ExecuteAdaption();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(sbMessage.ToString())
                    .SetTitle(Resource.String.alert_title_warning)
                    .Show();
                return;
            }

            ExecuteAdaption();
        }

        private void ExecuteAdaption()
        {
            if (IsJobRunning())
            {
                return;
            }

            EdiabasOpen();

            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            bool is1281Ecu = XmlToolActivity.Is1281Ecu(_ecuInfo);
            bool resetChannel = IsResetChannel();
            UInt64? startValue = null;
            if (resetChannel)
            {
                startValue = 0;
            }
            _instanceData.AdaptionValueStart = startValue;
            _instanceData.AdaptionValueNew = startValue;
            _instanceData.AdaptionValueTest = startValue;
            _instanceData.UpdateAdaptionValueNew = true;
            _instanceData.AdaptionData = null;
            _instanceData.AdaptionDataNew = null;
            for (int i = 0; i < _instanceData.AdaptionValues.Length; i++)
            {
                _instanceData.AdaptionValues[i] = null;
            }
            for (int i = 0; i < _instanceData.AdaptionUnits.Length; i++)
            {
                _instanceData.AdaptionUnits[i] = null;
            }
            _instanceData.TestAdaption = false;
            if (!(is1281Ecu && resetChannel))
            {
                _instanceData.StoreAdaption = false;
            }
            _instanceData.StopAdaption = false;
            _instanceData.AutoClose = false;
            _instanceData.EcuReset = false;
            _instanceData.AdaptionStored = false;
            _checkBoxEcuReset.Checked = false;

            bool executeFailed = false;
            bool finishUpdate = false;
            JobStatus jobStatus = JobStatus.Unknown;
            _jobThread = new Thread(() =>
            {
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, _ecuInfo.Sgbd);

                    string repairShopCodeString = string.Format(CultureInfo.InvariantCulture, "{0:000000}{1:000}{2:00000}",
                        _instanceData.CurrentEquipmentNumber ?? 0, _instanceData.CurrentImporterNumber ?? 0, _instanceData.CurrentWorkshopNumber ?? 0);
                    string repairShopCodeDataString = BitConverter.ToString(GetRepairShopCodeData()).Replace("-", "");
                    int adaptionChannel = _instanceData.SelectedChannel;
                    int serviceId = -1;
                    bool longAdaption = false;
                    string adaptionJob = string.Empty;
                    UInt64? adaptionValueType = null;
                    int adaptionValueLength = 2;

                    if (isUdsEcu)
                    {
                        adaptionJob = XmlToolActivity.JobReadS22Uds;
                        if (adaptionChannel >= 0 && adaptionChannel < _parseInfoAdaptionList.Count)
                        {
                            UdsFileReader.UdsReader.ParseInfoAdp parseInfoAdp = _parseInfoAdaptionList[adaptionChannel];
                            if (parseInfoAdp != null)
                            {
                                serviceId = (int)parseInfoAdp.ServiceId;
                            }
                        }

                        if (serviceId < 0)
                        {
                            executeFailed = true;
                        }
                    }
                    else if (is1281Ecu)
                    {
                        adaptionJob = @"Anpassung_lesen";
                    }
                    else if (_ecuInfo.VagSupportedFuncHash != null)
                    {
                        if (_ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.Adaption))
                        {
                            adaptionJob = @"Anpassung2";
                        }
                        else if (_ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.AdaptionLong))
                        {
                            longAdaption = true;
                            adaptionJob = @"LangeAnpassung";
                        }
                        else if (_ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.AdaptionLong2))
                        {
                            longAdaption = true;
                            adaptionJob = @"LangeAnpassung2";
                        }
                    }

                    if (string.IsNullOrEmpty(adaptionJob))
                    {
                        executeFailed = true;
                    }

                    if (!executeFailed && !string.IsNullOrEmpty(adaptionJob))
                    {
                        bool connected = false;
                        int errorCount = 0;

                        for (;;)
                        {
                            bool testAdaption = _instanceData.TestAdaption;
                            bool storeAdaption = _instanceData.StoreAdaption;
                            bool stopAdaption = _instanceData.StopAdaption;
                            UInt64? adaptionValueNew = _instanceData.AdaptionValueNew;
                            byte[] adaptionDataNew = _instanceData.AdaptionDataNew;
                            string adaptionJobArgs;
                            string writeJobProgDate = string.Empty;
                            string writeJobProgDateArgs = string.Empty;
                            string writeJobRscName = string.Empty;
                            string writeJobRscArgs = string.Empty;
                            bool read1281Adaption = false;

                            if (isUdsEcu)
                            {
                                adaptionJob = XmlToolActivity.JobReadS22Uds;
                                adaptionJobArgs = string.Format(CultureInfo.InvariantCulture, "{0}", serviceId);
                                if (storeAdaption && adaptionDataNew != null)
                                {
                                    string adationDataString = BitConverter.ToString(adaptionDataNew).Replace("-", "");
                                    adaptionJob = XmlToolActivity.JobWriteS2EUds;
                                    adaptionJobArgs = string.Format(CultureInfo.InvariantCulture, "{0};{1}", serviceId, adationDataString);

                                    byte[] progDate = _ecuInfo.VagProgDate;
                                    if (progDate == null)
                                    {
                                        progDate = new byte[3];
                                        DateTime dateTime = DateTime.Now;
                                        progDate[0] = (byte)ActivityCommon.DecToBcd(dateTime.Year % 100);
                                        progDate[1] = (byte)ActivityCommon.DecToBcd(dateTime.Month);
                                        progDate[2] = (byte)ActivityCommon.DecToBcd(dateTime.Day);
                                    }
                                    writeJobProgDate = XmlToolActivity.JobWriteS2EUds;
                                    writeJobProgDateArgs = "0xF199;" + BitConverter.ToString(progDate).Replace("-", "");

                                    writeJobRscName = XmlToolActivity.JobWriteS2EUds;
                                    writeJobRscArgs = "0xF198;" + repairShopCodeDataString;
                                }

                                if (connected)
                                {
                                    if (!(testAdaption || storeAdaption || stopAdaption))
                                    {
                                        Thread.Sleep(500);
                                        continue;
                                    }
                                }
                            }
                            else if (is1281Ecu)
                            {
                                adaptionJob = @"Anpassung_lesen";
                                adaptionJobArgs = string.Format(CultureInfo.InvariantCulture, "{0};WertEinmalLesen", adaptionChannel);
                                read1281Adaption = true;
                                if (adaptionValueNew != null)
                                {
                                    if (testAdaption)
                                    {
                                        read1281Adaption = false;
                                        adaptionJob = @"Anpassung_testen";
                                        adaptionJobArgs = string.Format(CultureInfo.InvariantCulture, "{0};{1};WertEinmalLesen", adaptionChannel, adaptionValueNew.Value);
                                    }
                                    else if (storeAdaption)
                                    {
                                        read1281Adaption = false;
                                        adaptionJob = @"Anpassung_speichern";
                                        adaptionJobArgs = string.Format(CultureInfo.InvariantCulture, "{0:00000};{1};{2}",
                                            _instanceData.CurrentWorkshopNumber ?? 0, adaptionChannel, adaptionValueNew.Value);
                                    }
                                }
                            }
                            else
                            {
                                string adaptionCommand = connected ? @"LESEN" : @"START";

                                if (stopAdaption)
                                {
                                    adaptionCommand = @"STOP";
                                }
                                else if ((testAdaption || storeAdaption) && adaptionValueNew != null)
                                {
                                    if (longAdaption)
                                    {
                                        StringBuilder sbAdaptionValue = new StringBuilder();
                                        for (int i = 0; i < adaptionValueLength; i++)
                                        {
                                            sbAdaptionValue.Insert(0, string.Format(CultureInfo.InvariantCulture, "{0:X02}", (adaptionValueNew.Value >> (i * 8)) & 0xFF));
                                        }
                                        adaptionCommand = string.Format(CultureInfo.InvariantCulture, "{0};{1};{2}",
                                            testAdaption ? @"AENDERN" : @"SPEICHERN", sbAdaptionValue.ToString(), adaptionValueType ?? 2);
                                    }
                                    else
                                    {
                                        adaptionCommand = string.Format(CultureInfo.InvariantCulture, "{0};{1}", testAdaption ? @"AENDERN" : @"SPEICHERN", adaptionValueNew.Value);
                                    }
                                }
                                adaptionJobArgs = repairShopCodeString + string.Format(CultureInfo.InvariantCulture, ";{0};{1}", adaptionChannel, adaptionCommand);
                            }

                            if (!executeFailed && !string.IsNullOrEmpty(writeJobProgDate))
                            {
                                _ediabas.ArgString = writeJobProgDateArgs;
                                _ediabas.ArgBinaryStd = null;
                                _ediabas.ResultsRequests = string.Empty;
                                _ediabas.ExecuteJob(writeJobProgDate);

                                jobStatus = CheckAdaptionResult();
                                if (jobStatus != JobStatus.Ok)
                                {
                                    executeFailed = true;
                                }
                            }

                            if (!executeFailed && !string.IsNullOrEmpty(writeJobRscName))
                            {
                                _ediabas.ArgString = writeJobRscArgs;
                                _ediabas.ArgBinaryStd = null;
                                _ediabas.ResultsRequests = string.Empty;
                                _ediabas.ExecuteJob(writeJobRscName);

                                jobStatus = CheckAdaptionResult();
                                if (jobStatus != JobStatus.Ok)
                                {
                                    executeFailed = true;
                                }
                            }

                            _ediabas.ArgString = adaptionJobArgs;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(adaptionJob);

                            jobStatus = CheckAdaptionResult();
                            if (jobStatus != JobStatus.Ok)
                            {
                                if (jobStatus == JobStatus.Unknown && read1281Adaption && errorCount < 3)
                                {
                                    errorCount++;
                                    _ediabas.CloseSgbd();
                                    RunOnUiThread(() =>
                                    {
                                        _instanceData.AdaptionValueStart = null;
                                        UpdateAdaptionText(true);
                                    });

                                    continue;
                                }

                                executeFailed = true;
                            }

                            if (!executeFailed)
                            {
                                errorCount = 0;
                            }

                            if (!executeFailed && stopAdaption && isUdsEcu && _instanceData.EcuReset)
                            {
                                int dataOffset = XmlToolActivity.VagUdsRawDataOffset;
                                byte[] resetRequest = { 0x11, 0x02 };
                                _ediabas.EdInterfaceClass.TransmitData(resetRequest, out byte[] resetResponse);
                                if (resetResponse == null || resetResponse.Length < dataOffset + 2 || resetResponse[dataOffset + 0] != 0x51)
                                {
                                    executeFailed = true;
                                    jobStatus = JobStatus.ResetFailed;
                                }
                                else
                                {
                                    finishUpdate = true;
                                }
                            }

                            if (executeFailed)
                            {
                                break;
                            }

                            if (stopAdaption)
                            {
                                break;
                            }

                            if (storeAdaption && is1281Ecu)
                            {
                                _instanceData.StoreAdaption = false;
                                break;
                            }

                            connected = true;

                            UInt64? adaptionValue = null;
                            byte[] adaptionData = null;
                            string[] adaptionValues = new string[MaxMeasValues];
                            string[] adaptionUnits = new string[MaxMeasValues];
                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            int dictIndex = 0;
                            if (resultSets != null)
                            {
                                if (resultSets.Count >= 2)
                                {
                                    // ReSharper disable once InlineOutVariableDeclaration
                                    EdiabasNet.ResultData resultData;
                                    Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];

                                    if (isUdsEcu)
                                    {
                                        if (resultDict.TryGetValue("ERGEBNIS1WERT", out resultData))
                                        {
                                            if (resultData.OpData is byte[] data)
                                            {
                                                adaptionData = data;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (resultDict.TryGetValue("ANPASSUNG_WERT", out resultData))
                                        {
                                            if (resultData.OpData is Int64 value)
                                            {
                                                adaptionValue = (UInt64)value;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ANPASSWERT", out resultData))
                                        {
                                            if (resultData.OpData is Int64 value)
                                            {
                                                adaptionValue = (UInt64)value;
                                            }
                                            else if (resultData.OpData is byte[] valueBin)
                                            {
                                                UInt64 tempValue = 0;
                                                foreach (byte data in valueBin)
                                                {
                                                    tempValue <<= 8;
                                                    tempValue |= data;
                                                }
                                                adaptionValue = tempValue;
                                                adaptionValueLength = valueBin.Length;
                                            }
                                        }
                                        if (resultDict.TryGetValue("ANPASSWERTTYP", out resultData))
                                        {
                                            if (resultData.OpData is Int64 value)
                                            {
                                                adaptionValueType = (UInt64)value;
                                            }
                                        }
                                    }
                                }

                                if (!isUdsEcu)
                                {
                                    int dictOffset = is1281Ecu ? 2 : 1;
                                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                                    {
                                        if (dictIndex < dictOffset)
                                        {
                                            dictIndex++;
                                            continue;
                                        }

                                        // ReSharper disable once InlineOutVariableDeclaration
                                        EdiabasNet.ResultData resultData;
                                        if (resultDictLocal.TryGetValue("MW_WERT", out resultData))
                                        {
                                            if (resultData.OpData is string valueText)
                                            {
                                                if (dictIndex - dictOffset < adaptionValues.Length)
                                                {
                                                    adaptionValues[dictIndex - dictOffset] = valueText;
                                                }
                                            }
                                            else if (resultData.OpData is Int64 valueInt)
                                            {
                                                if (dictIndex - dictOffset < adaptionValues.Length)
                                                {
                                                    adaptionValues[dictIndex - dictOffset] = string.Format(CultureInfo.InvariantCulture, "{0}", valueInt);
                                                }
                                            }
                                            else if (resultData.OpData is Double valueDouble)
                                            {
                                                if (dictIndex - dictOffset < adaptionValues.Length)
                                                {
                                                    adaptionValues[dictIndex - dictOffset] = string.Format(CultureInfo.InvariantCulture, "{0}", valueDouble);
                                                }
                                            }
                                        }
                                        if (resultDictLocal.TryGetValue("MWEINH_TEXT", out resultData))
                                        {
                                            if (resultData.OpData is string unitText)
                                            {
                                                if (dictIndex - dictOffset < adaptionUnits.Length)
                                                {
                                                    adaptionUnits[dictIndex - dictOffset] = unitText;
                                                }
                                            }
                                        }
                                        dictIndex++;
                                    }
                                }
                            }

                            if (testAdaption)
                            {
                                _instanceData.TestAdaption = false;
                                if (!resetChannel && adaptionValue != null)
                                {
                                    _instanceData.AdaptionValueTest = adaptionValue;
                                }
                            }

                            if (storeAdaption)
                            {
                                _instanceData.StoreAdaption = false;
                                if (isUdsEcu)
                                {
                                    connected = false;
                                    _instanceData.AdaptionDataNew = null;
                                    _instanceData.AdaptionStored = true;
                                }
                                else
                                {
                                    _instanceData.StopAdaption = true;
                                }
                            }

                            if (!resetChannel && _instanceData.AdaptionValueStart == null && adaptionValue != null)
                            {
                                _instanceData.AdaptionValueStart = adaptionValue;
                                _instanceData.AdaptionValueNew = adaptionValue;
                            }

                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                if (!resetChannel)
                                {
                                    for (int i = 0; i < adaptionValues.Length; i++)
                                    {
                                        _instanceData.AdaptionValues[i] = adaptionValues[i];
                                    }

                                    for (int i = 0; i < adaptionUnits.Length; i++)
                                    {
                                        _instanceData.AdaptionUnits[i] = adaptionUnits[i];
                                    }
                                }

                                if (adaptionData != null)
                                {
                                    _instanceData.AdaptionData = adaptionData;
                                    if (_instanceData.AdaptionDataNew == null)
                                    {
                                        _instanceData.AdaptionDataNew = adaptionData;
                                    }
                                }

                                UpdateAdaptionText(true);
                            });
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
                        int resId = -1;
                        switch (jobStatus)
                        {
                            case JobStatus.IllegalArguments:
                                resId = Resource.String.vag_coding_write_coding_illegal_arguments;
                                break;

                            case JobStatus.AccessDenied:
                                resId = Resource.String.vag_coding_write_coding_access_denied;
                                break;

                            case JobStatus.ResetFailed:
                                resId = Resource.String.vag_coding_write_coding_reset_failed;
                                break;
                        }

                        if (resId < 0)
                        {
                            resId = Resource.String.vag_coding_write_coding_failed;
                        }
                        _activityCommon.ShowAlert(GetString(resId), Resource.String.alert_title_error);
                    }
                    else
                    {
                        if (finishUpdate)
                        {
                            _ecuInfo.JobListValid = false;    // force update
                            SetResult(Android.App.Result.Ok);
                            Finish();
                            return;
                        }

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

                    _instanceData.TestAdaption = false;
                    _instanceData.StoreAdaption = false;
                    _instanceData.StopAdaption = false;
                    _instanceData.EcuReset = false;
                    _instanceData.AdaptionStored = false;
                    _checkBoxEcuReset.Checked = false;

                    UpdateAdaptionText(true);
                });
            });
            _jobThread.Start();

            UpdateAdaptionText();
        }
    }
}
