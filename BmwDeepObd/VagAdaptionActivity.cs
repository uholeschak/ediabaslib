using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                               Android.Content.PM.ConfigChanges.Orientation |
                               Android.Content.PM.ConfigChanges.ScreenSize)]
    public class VagAdaptionActivity : AppCompatActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public int SelectedChannel { get; set; }
            public UInt64? CurrentWorkshopNumber { get; set; }
            public UInt64? CurrentImporterNumber { get; set; }
            public UInt64? CurrentEquipmentNumber { get; set; }
        }

        // Intent extra
        public const string ExtraEcuName = "ecu_name";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraTraceDir = "trace_dir";
        public const string ExtraTraceAppend = "trace_append";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";

        private const int MaxMeasValues = 4;

        private enum JobStatus
        {
            Unknown,
            Ok,
            IllegalArguments,
            AccessDenied,
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
        private TextView _textViewVagAdaptionOperationTitle;
        private Button _buttonAdaptionRead;
        private Button _buttonAdaptionTest;
        private Button _buttonAdaptionStore;
        private Button _buttonAdaptionStop;
        private ActivityCommon _activityCommon;
        private Handler _updateHandler;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private List<UdsFileReader.DataReader.DataInfo> _dataInfoAdaptionList;
        private bool _activityActive;
        private bool _ediabasJobAbort;
        private string _ecuDir;
        private string _traceDir;
        private bool _traceAppend;
        private string _deviceAddress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
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
            _updateHandler = new Handler();

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _traceDir = Intent.GetStringExtra(ExtraTraceDir);
            _traceAppend = Intent.GetBooleanExtra(ExtraTraceAppend, true);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);

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

            _textViewVagAdaptionOperationTitle = FindViewById<TextView>(Resource.Id.textViewVagAdaptionOperationTitle);
            _textViewVagAdaptionOperationTitle.SetOnTouchListener(this);

            _buttonAdaptionRead = FindViewById<Button>(Resource.Id.buttonAdaptionRead);
            _buttonAdaptionRead.SetOnTouchListener(this);
            _buttonAdaptionRead.Click += (sender, args) =>
            {
                ExecuteAdaption();
            };

            _buttonAdaptionTest = FindViewById<Button>(Resource.Id.buttonAdaptionTest);
            _buttonAdaptionTest.SetOnTouchListener(this);
            _buttonAdaptionTest.Click += (sender, args) =>
            {
                ExecuteAdaption();
            };

            _buttonAdaptionStore = FindViewById<Button>(Resource.Id.buttonAdaptionStore);
            _buttonAdaptionStore.SetOnTouchListener(this);
            _buttonAdaptionStore.Click += (sender, args) =>
            {
                ExecuteAdaption();
            };

            _buttonAdaptionStop = FindViewById<Button>(Resource.Id.buttonAdaptionStop);
            _buttonAdaptionStop.SetOnTouchListener(this);
            _buttonAdaptionStop.Click += (sender, args) =>
            {
                ExecuteAdaption();
            };

            UpdateAdaptionChannelList();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            ActivityCommon.StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (_activityCommon.MtcBtService)
            {
                _activityCommon.StartMtcService();
            }
            _activityCommon.RequestUsbPermission(null);
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
            if (_activityCommon.MtcBtService)
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
                _jobThread.Join();
            }
            EdiabasClose();
            _activityCommon.Dispose();
            _activityCommon = null;

            if (_updateHandler != null)
            {
                try
                {
                    _updateHandler.RemoveCallbacksAndMessages(null);
                    _updateHandler.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
                _updateHandler = null;
            }
        }

        public override void OnBackPressed()
        {
            if (IsJobRunning())
            {
                return;
            }
            StoreResults();
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    StoreResults();
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
                if (!string.IsNullOrEmpty(_traceDir))
                {
                    _ediabas.SetConfigProperty("TracePath", _traceDir);
                    _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                    _ediabas.SetConfigProperty("AppendTrace", _traceAppend ? "1" : "0");
                    _ediabas.SetConfigProperty("CompressTrace", "1");
                }
                else
                {
                    _ediabas.SetConfigProperty("IfhTrace", "0");
                }
            }

            _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress);
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
                        if (intent.GetParcelableExtra(UsbManager.ExtraDevice) is UsbDevice usbDevice)
                        {
                            _activityCommon.RequestUsbPermission(usbDevice);
                        }
                    }
                    break;

                case UsbManager.ActionUsbDeviceDetached:
                    if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Ftdi)
                    {
                        if (intent.GetParcelableExtra(UsbManager.ExtraDevice) is UsbDevice usbDevice &&
                            EdFtdiInterface.IsValidUsbDevice(usbDevice))
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
            if (!string.IsNullOrEmpty(_ecuInfo.VagDataFileName))
            {
                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_ecuInfo.VagDataFileName);
                // ReSharper disable once UseNullPropagation
                if (udsReader != null)
                {
                    dataInfoAdaptionList = udsReader.DataReader.ExtractDataType(_ecuInfo.VagDataFileName, UdsFileReader.DataReader.DataType.Adaption);
                }
            }

            _dataInfoAdaptionList = dataInfoAdaptionList;
        }

        private void UpdateAdaptionChannelList()
        {
            int selectedChannel = _instanceData.SelectedChannel;
            int selection = 0;

            _spinnerVagAdaptionChannelAdapter.Items.Clear();

            int index = 0;
            if (_dataInfoAdaptionList != null)
            {
                _spinnerVagAdaptionChannelAdapter.Items.Add(new StringObjType(GetString(Resource.String.vag_adaption_channel_select), -1));
                foreach (UdsFileReader.DataReader.DataInfo dataInfo in _dataInfoAdaptionList)
                {
                    if (dataInfo.Value1.HasValue && dataInfo.Value2.HasValue && dataInfo.Value2.Value == 0 && dataInfo.TextArray.Length > 0)
                    {
                        string text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", dataInfo.Value1.Value, dataInfo.TextArray[0]);
                        _spinnerVagAdaptionChannelAdapter.Items.Add(new StringObjType(text, dataInfo.Value1.Value));
                        if (selectedChannel == dataInfo.Value1.Value)
                        {
                            selection = index;
                        }
                        index++;
                    }
                }
            }

            _spinnerVagAdaptionChannelAdapter.NotifyDataSetChanged();
            _spinnerVagAdaptionChannel.SetSelection(selection);

            UpdateAdaption();
        }

        private void UpdateAdaption()
        {
            if (_spinnerVagAdaptionChannel.SelectedItemPosition >= 0)
            {
                int channel = (int)_spinnerVagAdaptionChannelAdapter.Items[_spinnerVagAdaptionChannel.SelectedItemPosition].Data;
                if (channel >= 0)
                {
                    _instanceData.SelectedChannel = channel;
                }
            }

            UpdateAdaptionInfo();
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

            if (dataChanged)
            {
                UpdateAdaptionInfo();
            }
        }

        private bool IsShortAdaption()
        {
            if (_ecuInfo.VagSupportedFuncHash.Contains((UInt64)XmlToolActivity.SupportedFuncType.Adaption))
            {
                return true;
            }

            return false;
        }

        private void UpdateAdaptionInfo()
        {
            UpdateAdaptionText();

            StringBuilder[] measTitles = new StringBuilder[MaxMeasValues];
            StringBuilder sbAdaptionComment = new StringBuilder();
            if (_dataInfoAdaptionList != null)
            {
                foreach (UdsFileReader.DataReader.DataInfo dataInfo in _dataInfoAdaptionList)
                {
                    if (dataInfo.Value1.HasValue && dataInfo.Value2.HasValue &&
                        dataInfo.Value1.Value == _instanceData.SelectedChannel && dataInfo.TextArray.Length > 0)
                    {
                        int index = dataInfo.Value2.Value;
                        if (index > 0 && index <= 4)
                        {
                            if (measTitles[index - 1] == null)
                            {
                                measTitles[index - 1] = new StringBuilder();
                            }

                            foreach (string text in dataInfo.TextArray)
                            {
                                if (measTitles[index - 1].Length > 0)
                                {
                                    measTitles[index - 1].Append(" ");
                                }
                                measTitles[index - 1].Append(text);
                            }
                        }
                        else
                        {
                            if (sbAdaptionComment.Length > 0)
                            {
                                sbAdaptionComment.Append("\r\n");
                            }
                            sbAdaptionComment.Append(dataInfo.TextArray[0]);
                        }
                    }
                }
            }

            int selection = 0;
            int itemIndex = 0;
            foreach (StringObjType stringObjType in _spinnerVagAdaptionChannelAdapter.Items)
            {
                if ((int) stringObjType.Data == _instanceData.SelectedChannel)
                {
                    selection = itemIndex;
                }
                itemIndex++;
            }
            _spinnerVagAdaptionChannel.SetSelection(selection);

            _layoutVagAdaptionComments.Visibility = sbAdaptionComment.Length > 0 ? ViewStates.Visible : ViewStates.Gone;
            _textVagViewAdaptionComments.Text = sbAdaptionComment.ToString();

            for (int i = 0; i < measTitles.Length; i++)
            {
                string title = measTitles[i] != null ? measTitles[i].ToString() : string.Empty;
                _textViewVagAdaptionMeasTitles[i].Text =
                    string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_meas_value_title), i + 1, title);
            }

            _layoutVagAdaptionRepairShopCode.Visibility = IsShortAdaption() ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagAdaptionWorkshop.Visibility = _instanceData.CurrentWorkshopNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagAdaptionImporterNumber.Visibility = _instanceData.CurrentImporterNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagAdaptionEquipmentNumber.Visibility = _instanceData.CurrentEquipmentNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
        }

        private void UpdateAdaptionText()
        {
            string adaptionChannelNumber = string.Empty;
            string codingTextWorkshop = string.Empty;
            string codingTextImporter = string.Empty;
            string codingTextEquipment = string.Empty;
            string workshopNumberTitle = string.Empty;
            string importerNumberTitle = string.Empty;
            string equipmentNumberTitle = string.Empty;

            try
            {
                adaptionChannelNumber = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.SelectedChannel);
                if (_instanceData.CurrentWorkshopNumber.HasValue)
                {
                    codingTextWorkshop = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.CurrentWorkshopNumber);
                }
                if (_instanceData.CurrentImporterNumber.HasValue)
                {
                    codingTextImporter = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.CurrentImporterNumber);
                }
                if (_instanceData.CurrentEquipmentNumber.HasValue)
                {
                    codingTextEquipment = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.CurrentEquipmentNumber);
                }

                workshopNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_workshop_number_title), 0, VagCodingActivity.WorkshopNumberMax);
                importerNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_importer_number_title), 0, VagCodingActivity.ImporterNumberMax);
                equipmentNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_adaption_equipment_number_title), 0, VagCodingActivity.EquipmentNumberMax);
            }
            catch (Exception)
            {
                // ignored
            }

            _editTextVagAdaptionChannelNumber.Text = adaptionChannelNumber;
            _textViewVagWorkshopNumberTitle.Text = workshopNumberTitle;
            _editTextVagWorkshopNumber.Text = codingTextWorkshop;
            _textViewVagImporterNumberTitle.Text = importerNumberTitle;
            _editTextVagImporterNumber.Text = codingTextImporter;
            _textViewVagEquipmentNumberTitle.Text = equipmentNumberTitle;
            _editTextVagEquipmentNumber.Text = codingTextEquipment;
            _buttonAdaptionRead.Enabled = !IsJobRunning();
            _buttonAdaptionTest.Enabled = !IsJobRunning();
            _buttonAdaptionStore.Enabled = !IsJobRunning();
            _buttonAdaptionStop.Enabled = IsJobRunning();
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
            _editTextVagAdaptionChannelNumber.ClearFocus();
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
            byte[] workShopData = BitConverter.GetBytes(_instanceData.CurrentWorkshopNumber ?? 0 & 0x01FFFF);    // 17 bit
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(workShopData);
            }

            byte[] importerNumberData = BitConverter.GetBytes((_instanceData.CurrentImporterNumber ?? 0 & 0x0003FF) << 1);  // 10 bit
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(importerNumberData);
            }

            byte[] equipmentNumberData = BitConverter.GetBytes((_instanceData.CurrentEquipmentNumber ?? 0 & 0x1FFFFF) << 3);  // 21 bit
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
            if (_instanceData.CurrentWorkshopNumber != null && _instanceData.CurrentImporterNumber != null && _instanceData.CurrentImporterNumber != null)
            {
                valuesValid = _instanceData.CurrentWorkshopNumber.Value != 0 &&
                              _instanceData.CurrentImporterNumber.Value != 0 &&
                              _instanceData.CurrentImporterNumber.Value != 0;
            }

            if (!valuesValid)
            {
                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        ExecuteAdaption();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.vag_coding_write_values_invalid)
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

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.vag_coding_processing));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            bool executeFailed = false;
            JobStatus jobStatus = JobStatus.Unknown;
            _jobThread = new Thread(() =>
            {
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, _ecuInfo.Sgbd);

                    {
                        string repairShopCodeString = string.Format(CultureInfo.InvariantCulture, "{0:000000}{1:000}{2:00000}",
                            _instanceData.CurrentEquipmentNumber ?? 0, _instanceData.CurrentImporterNumber ?? 0, _instanceData.CurrentWorkshopNumber ?? 0);
                        string repairShopCodeDataString = BitConverter.ToString(GetRepairShopCodeData()).Replace("-", "");
                        string writeJob = string.Empty;
                        string writeJobArgs = string.Empty;

                        if (!executeFailed && !string.IsNullOrEmpty(writeJob))
                        {
                            _ediabas.ArgString = writeJobArgs;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(writeJob);

                            jobStatus = CheckAdaptionResult();
                            if (jobStatus != JobStatus.Ok)
                            {
                                executeFailed = true;
                            }
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
                    progress.Dismiss();
                    progress.Dispose();

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
                        }

                        if (resId < 0)
                        {
                            resId = Resource.String.vag_coding_write_coding_failed;
                        }
                        _activityCommon.ShowAlert(GetString(resId), Resource.String.alert_title_error);
                    }
                });
            });
            _jobThread.Start();
        }
    }
}
