using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Text;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using BmwDeepObd.Dialogs;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(
        Name = ActivityCommon.AppNameSpace + "." + nameof(VagCodingActivity),
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class VagCodingActivity : BaseActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public int SelectedSubsystem { get; set; }
            public byte[] CurrentCoding { get; set; }
            public XmlToolActivity.EcuInfo.CodingRequestType? CurrentCodingRequestType { get; set; }
            public UInt64? CurrentCodingMax { get; set; }
            public UInt64? CurrentWorkshopNumber { get; set; }
            public UInt64? CurrentImporterNumber { get; set; }
            public UInt64? CurrentEquipmentNumber { get; set; }
            public string CurrentDataFileName { get; set; }
        }

        // Intent extra
        public const string ExtraCodingMode = "coding_mode";
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

        public const UInt64 WorkshopNumberMax = 99999;
        public const UInt64 WorkshopNumberMask = 0x01FFFF;      // 17 bit
        public const UInt64 ImporterNumberMax = 999;
        public const UInt64 ImporterNumberMask = 0x0003FF;      // 10 bit;
        public const UInt64 EquipmentNumberMax = 999999;
        public const UInt64 EquipmentNumberMask = 0x1FFFFF;     // 21 bit

        public enum CodingMode
        {
            Coding,
            Coding2,
            Login,
            SecurityAccess
        }

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
        private ScrollView _scrollViewVagCoding;
        private LinearLayout _layoutVagCoding;
        private LinearLayout _layoutVagCodingSubsystem;
        private TextView _textViewVagCodingSubsystem;
        private Spinner _spinnerVagCodingSubsystem;
        private StringObjAdapter _spinnerVagCodingSubsystemAdapter;
        private LinearLayout _layoutVagCodingShort;
        private TextView _textViewVagCodingShortTitle;
        private EditText _editTextVagCodingShort;
        private LinearLayout _layoutVagCodingComments;
        private TextView _textViewVagCodingCommentsTitle;
        private TextView _textViewVagCodingComments;
        private LinearLayout _layoutVagCodingRaw;
        private TextView _textViewVagCodingRawTitle;
        private EditText _editTextVagCodingRaw;
        private LinearLayout _layoutVagCodingRepairShopCode;
        private LinearLayout _layoutVagCodingWorkshop;
        private TextView _textViewVagWorkshopNumberTitle;
        private EditText _editTextVagWorkshopNumber;
        private LinearLayout _layoutVagCodingImporterNumber;
        private TextView _textViewVagImporterNumberTitle;
        private EditText _editTextVagImporterNumber;
        private LinearLayout _layoutVagCodingEquipmentNumber;
        private TextView _textViewVagEquipmentNumberTitle;
        private EditText _editTextVagEquipmentNumber;
        private LinearLayout _layoutCodingButtons;
        private TextView _textViewVagCodingExecuteTitle;
        private Button _buttonCodingExecute;
        private CheckBox _checkBoxEcuReset;
        private LinearLayout _layoutVagCodingAssitant;
        private ResultListAdapter _layoutVagCodingAssitantAdapter;
        private ListView _listViewVagCodingAssistant;
        private int _listViewCodingAssistantWidthLast;
        private ActivityCommon _activityCommon;
        private Handler _updateHandler;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
        private bool _activityActive;
        private bool _ediabasJobAbort;
        private CodingMode _codingMode;
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
            SetContentView(Resource.Layout.vag_coding);

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
            _updateHandler = new Handler(Looper.MainLooper);

            _codingMode = (CodingMode) Intent.GetIntExtra(ExtraCodingMode, (int) CodingMode.Coding);
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

            int resId;
            switch (_codingMode)
            {
                case CodingMode.Login:
                    resId = Resource.String.vag_coding_title_login;
                    break;

                case CodingMode.SecurityAccess:
                    resId = Resource.String.vag_coding_title_sec_access;
                    break;

                default:
                    resId = Resource.String.vag_coding_title_coding;
                    break;
            }
            SupportActionBar.Title = string.Format(GetString(resId), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);

            _scrollViewVagCoding = FindViewById<ScrollView>(Resource.Id.scrollViewVagCoding);
            _scrollViewVagCoding.SetOnTouchListener(this);

            _layoutVagCoding = FindViewById<LinearLayout>(Resource.Id.layoutVagCoding);
            _layoutVagCoding.SetOnTouchListener(this);

            _layoutVagCodingSubsystem = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingSubsystem);
            _layoutVagCodingSubsystem.SetOnTouchListener(this);

            _textViewVagCodingSubsystem = FindViewById<TextView>(Resource.Id.textViewVagCodingSubsystem);
            _textViewVagCodingSubsystem.SetOnTouchListener(this);

            _spinnerVagCodingSubsystem = FindViewById<Spinner>(Resource.Id.spinnerVagCodingSubsystem);
            _spinnerVagCodingSubsystem.SetOnTouchListener(this);
            _spinnerVagCodingSubsystemAdapter = new StringObjAdapter(this);
            _spinnerVagCodingSubsystem.Adapter = _spinnerVagCodingSubsystemAdapter;
            _spinnerVagCodingSubsystem.ItemSelected += SubSystemItemSelected;

            _layoutVagCodingShort = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingShort);
            _layoutVagCodingShort.SetOnTouchListener(this);

            _textViewVagCodingShortTitle = FindViewById<TextView>(Resource.Id.textViewVagCodingShortTitle);
            _textViewVagCodingShortTitle.SetOnTouchListener(this);

            _editTextVagCodingShort = FindViewById<EditText>(Resource.Id.editTextVagCodingShort);
            _editTextVagCodingShort.EditorAction += CodingEditorAction;

            _layoutVagCodingComments = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingComments);
            _layoutVagCodingComments.SetOnTouchListener(this);

            _textViewVagCodingCommentsTitle = FindViewById<TextView>(Resource.Id.textViewVagCodingCommentsTitle);
            _textViewVagCodingCommentsTitle.SetOnTouchListener(this);

            _textViewVagCodingComments = FindViewById<TextView>(Resource.Id.textViewVagCodingComments);
            _textViewVagCodingComments.SetOnTouchListener(this);
            _textViewVagCodingComments.MovementMethod = new ScrollingMovementMethod();

            _layoutVagCodingRaw = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingRaw);
            _layoutVagCodingRaw.SetOnTouchListener(this);

            _textViewVagCodingRawTitle = FindViewById<TextView>(Resource.Id.textViewVagCodingRawTitle);
            _textViewVagCodingRawTitle.SetOnTouchListener(this);

            _editTextVagCodingRaw = FindViewById<EditText>(Resource.Id.editTextVagCodingRaw);
            _editTextVagCodingRaw.EditorAction += CodingEditorAction;

            _layoutVagCodingRepairShopCode = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingRepairShopCode);
            _layoutVagCodingRepairShopCode.SetOnTouchListener(this);

            _layoutVagCodingWorkshop = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingWorkshop);
            _layoutVagCodingWorkshop.SetOnTouchListener(this);

            _textViewVagWorkshopNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagWorkshopNumberTitle);
            _textViewVagWorkshopNumberTitle.SetOnTouchListener(this);

            _editTextVagWorkshopNumber = FindViewById<EditText>(Resource.Id.editTextVagWorkshopNumber);
            _editTextVagWorkshopNumber.EditorAction += CodingEditorAction;

            _layoutVagCodingImporterNumber = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingImporterNumber);
            _layoutVagCodingImporterNumber.SetOnTouchListener(this);

            _textViewVagImporterNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagImporterNumberTitle);
            _textViewVagImporterNumberTitle.SetOnTouchListener(this);

            _editTextVagImporterNumber = FindViewById<EditText>(Resource.Id.editTextVagImporterNumber);
            _editTextVagImporterNumber.EditorAction += CodingEditorAction;

            _layoutVagCodingEquipmentNumber = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingEquipmentNumber);
            _layoutVagCodingEquipmentNumber.SetOnTouchListener(this);

            _textViewVagEquipmentNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagEquipmentNumberTitle);
            _textViewVagEquipmentNumberTitle.SetOnTouchListener(this);

            _editTextVagEquipmentNumber = FindViewById<EditText>(Resource.Id.editTextVagEquipmentNumber);
            _editTextVagEquipmentNumber.EditorAction += CodingEditorAction;

            _layoutCodingButtons = FindViewById<LinearLayout>(Resource.Id.layoutCodingButtons);
            _layoutCodingButtons.SetOnTouchListener(this);

            _textViewVagCodingExecuteTitle = FindViewById<TextView>(Resource.Id.textViewVagCodingExecuteTitle);
            _textViewVagCodingExecuteTitle.SetOnTouchListener(this);

            _buttonCodingExecute = FindViewById<Button>(Resource.Id.buttonCodingExecute);
            _buttonCodingExecute.SetOnTouchListener(this);
            _buttonCodingExecute.Click += (sender, args) =>
            {
                ExecuteCodingRequest();
            };

            _checkBoxEcuReset = FindViewById<CheckBox>(Resource.Id.checkBoxEcuReset);
            _checkBoxEcuReset.SetOnTouchListener(this);

            _layoutVagCodingAssitant = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingAssitant);
            _layoutVagCodingAssitant.SetOnTouchListener(this);

            _listViewVagCodingAssistant = FindViewById<ListView>(Resource.Id.listViewVagCodingAssistant);
            _layoutVagCodingAssitantAdapter = new ResultListAdapter(this, -1, 0, true);
            _listViewVagCodingAssistant.Adapter = _layoutVagCodingAssitantAdapter;
            _listViewVagCodingAssistant.SetOnTouchListener(this);
            _listViewVagCodingAssistant.LayoutChange += (sender, args) =>
            {
                UpdateCodingInfo();
            };

            UpdateCodingSubsystemList();
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

            if (_updateHandler != null)
            {
                try
                {
                    _updateHandler.RemoveCallbacksAndMessages(null);
                }
                catch (Exception)
                {
                    // ignored
                }
                _updateHandler = null;
            }
        }

        public override void OnBackPressedEvent()
        {
            if (IsJobRunning())
            {
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
                    ReadCodingEditors();
                    HideKeyboard();
                    break;

                case MotionEventActions.Move:
                    if (v == _listViewVagCodingAssistant)
                    {
                        return true;
                    }
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

        private void SubSystemItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            UpdateCoding();
        }

        private void UpdateCodingSubsystemList()
        {
            int selection = _instanceData.SelectedSubsystem;

            _spinnerVagCodingSubsystemAdapter.Items.Clear();
            if (_codingMode == CodingMode.Coding)
            {
                if (_ecuInfo.VagCodingShort != null || _ecuInfo.VagCodingLong != null)
                {
                    int index = 0;
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format("{0}: ", index));
                    bool append = false;
                    if (!string.IsNullOrEmpty(_ecuInfo.VagPartNumber))
                    {
                        sb.Append(_ecuInfo.VagPartNumber);
                        append = true;
                    }
                    if (!string.IsNullOrEmpty(_ecuInfo.VagSysName))
                    {
                        if (append)
                        {
                            sb.Append(" / ");
                        }
                        sb.Append(_ecuInfo.VagSysName);
                    }
                    _spinnerVagCodingSubsystemAdapter.Items.Add(new StringObjType(sb.ToString(), index));
                }

                if (_ecuInfo.SubSystems != null)
                {
                    foreach (XmlToolActivity.EcuInfoSubSys subSystem in _ecuInfo.SubSystems)
                    {
                        if (subSystem.VagCodingLong != null)
                        {
                            int index = subSystem.SubSysIndex + 1;
                            StringBuilder sb = new StringBuilder();
                            sb.Append(string.Format("{0}: ", index));
                            bool append = false;
                            if (!string.IsNullOrEmpty(subSystem.VagPartNumber))
                            {
                                sb.Append(subSystem.VagPartNumber);
                                append = true;
                            }
                            if (!string.IsNullOrEmpty(subSystem.Name))
                            {
                                if (append)
                                {
                                    sb.Append(" / ");
                                }
                                sb.Append(subSystem.Name);
                            }
                            _spinnerVagCodingSubsystemAdapter.Items.Add(new StringObjType(sb.ToString(), index));
                        }
                    }
                }
            }
            else
            {
                _spinnerVagCodingSubsystemAdapter.Items.Add(new StringObjType("-", 0));
            }
            _spinnerVagCodingSubsystemAdapter.NotifyDataSetChanged();

            _spinnerVagCodingSubsystem.SetSelection(selection);
            UpdateCoding();
        }

        private void UpdateCoding()
        {
            _instanceData.CurrentCoding = null;
            _instanceData.CurrentDataFileName = null;
            int subSysIndex = _spinnerVagCodingSubsystem.SelectedItemPosition;
            if (subSysIndex >= 0 && subSysIndex < _spinnerVagCodingSubsystemAdapter.Items.Count)
            {
                int subSystemIndex = (int)_spinnerVagCodingSubsystemAdapter.Items[subSysIndex].Data;
                _instanceData.SelectedSubsystem = subSystemIndex;

                string dataFileName = null;
                byte[] coding = null;
                XmlToolActivity.EcuInfo.CodingRequestType? codingRequestType = null;
                UInt64? codingMax = null;
                UInt64? workshopNumber = null;
                UInt64? importerNumber = null;
                UInt64? equipmentNumber = null;

                if (_codingMode == CodingMode.Coding)
                {
                    if (subSystemIndex == 0)
                    {
                        if (_ecuInfo.VagCodingLong != null)
                        {
                            coding = _ecuInfo.VagCodingLong;
                        }
                        else if (_ecuInfo.VagCodingShort != null)
                        {
                            int codingLength = 0;
                            ulong maxValue = _ecuInfo.VagCodingMax ?? 0;
                            while (maxValue != 0)
                            {
                                codingLength++;
                                maxValue >>= 8;
                            }
                            byte[] codingData = BitConverter.GetBytes(_ecuInfo.VagCodingShort.Value);
                            if (!BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(codingData);
                            }
                            if (codingLength > 0 && codingData.Length >= codingLength)
                            {
                                coding = new byte[codingLength];
                                Array.Copy(codingData, coding, coding.Length);
                            }
                        }

                        codingRequestType = _ecuInfo.VagCodingRequestType;
                        codingMax = _ecuInfo.VagCodingMax;
                        workshopNumber = _ecuInfo.VagWorkshopNumber;
                        importerNumber = _ecuInfo.VagImporterNumber;
                        equipmentNumber = _ecuInfo.VagEquipmentNumber;
                        dataFileName = _ecuInfo.VagDataFileName;
                    }
                    else
                    {
                        if (_ecuInfo.SubSystems != null)
                        {
                            foreach (XmlToolActivity.EcuInfoSubSys subSystem in _ecuInfo.SubSystems)
                            {
                                if (subSystem.SubSysIndex + 1 == subSystemIndex)
                                {
                                    coding = subSystem.VagCodingLong;
                                    codingRequestType = XmlToolActivity.EcuInfo.CodingRequestType.LongUds;
                                    dataFileName = subSystem.VagDataFileName;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    switch (_codingMode)
                    {
                        case CodingMode.Coding2:
                        case CodingMode.Login:
                            coding = new byte[2];
                            codingRequestType = XmlToolActivity.EcuInfo.CodingRequestType.ShortV2;
                            codingMax = 0xFFFF;
                            workshopNumber = _ecuInfo.VagWorkshopNumber;
                            importerNumber = _ecuInfo.VagImporterNumber;
                            equipmentNumber = _ecuInfo.VagEquipmentNumber;
                            dataFileName = _ecuInfo.VagDataFileName;
                            break;

                        case CodingMode.SecurityAccess:
                            coding = new byte[4];
                            codingRequestType = XmlToolActivity.EcuInfo.CodingRequestType.ShortV2;
                            codingMax = 99999;  // original program allows 999999, but is rejected by EDIABAS
                            dataFileName = _ecuInfo.VagDataFileName;
                            break;
                    }
                }

                if (coding != null)
                {
                    _instanceData.CurrentCoding = new byte[coding.Length];
                    Array.Copy(coding, _instanceData.CurrentCoding, coding.Length);
                }

                _instanceData.CurrentCodingRequestType = codingRequestType;
                _instanceData.CurrentCodingMax = codingMax;
                _instanceData.CurrentWorkshopNumber = workshopNumber;
                _instanceData.CurrentImporterNumber = importerNumber;
                _instanceData.CurrentEquipmentNumber = equipmentNumber;
                _instanceData.CurrentDataFileName = dataFileName;
            }

            UpdateCodingInfo();
        }

        private void CodingEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            switch (e.ActionId)
            {
                case ImeAction.Go:
                case ImeAction.Send:
                case ImeAction.Next:
                case ImeAction.Done:
                case ImeAction.Previous:
                    ReadCodingEditors();
                    HideKeyboard();
                    break;
            }
        }

        private void ReadCodingEditors()
        {
            bool dataChanged = false;
            if (_instanceData.CurrentCoding != null)
            {
                if (_editTextVagCodingRaw.Enabled)
                {
                    string codingText = _editTextVagCodingRaw.Text.Trim();
                    string[] codingArray = codingText.Split(' ', ';', ',');
                    byte[] dataArray;
                    try
                    {
                        List<byte> binList = new List<byte>();
                        foreach (string arg in codingArray)
                        {
                            if (!string.IsNullOrEmpty(arg))
                            {
                                binList.Add(Convert.ToByte(arg, 16));
                            }
                        }

                        dataArray = binList.ToArray();
                    }
                    catch (Exception)
                    {
                        dataArray = null;
                    }

                    if (dataArray != null && dataArray.Length == _instanceData.CurrentCoding.Length)
                    {
                        if (!dataArray.SequenceEqual(_instanceData.CurrentCoding))
                        {
                            Array.Copy(dataArray, _instanceData.CurrentCoding, dataArray.Length);
                            dataChanged = true;
                        }
                    }
                }
                else if (_editTextVagCodingShort.Enabled)
                {
                    try
                    {
                        if (UInt64.TryParse(_editTextVagCodingShort.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 value))
                        {
                            if (_instanceData.CurrentCodingMax.HasValue && value <= _instanceData.CurrentCodingMax.Value)
                            {
                                byte[] codingData = BitConverter.GetBytes(value);
                                if (!BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(codingData);
                                }
                                if (codingData.Length >= _instanceData.CurrentCoding.Length)
                                {
                                    for (int i = 0; i < _instanceData.CurrentCoding.Length; i++)
                                    {
                                        if (codingData[i] != _instanceData.CurrentCoding[i])
                                        {
                                            dataChanged = true;
                                            break;
                                        }
                                    }
                                    Array.Copy(codingData, _instanceData.CurrentCoding, _instanceData.CurrentCoding.Length);
                                }
                            }
                            else
                            {
                                dataChanged = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (_layoutVagCodingRepairShopCode.Visibility == ViewStates.Visible)
                {
                    try
                    {
                        if (_instanceData.CurrentWorkshopNumber.HasValue && _editTextVagWorkshopNumber.Enabled)
                        {
                            if (UInt64.TryParse(_editTextVagWorkshopNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 valueWorkshop))
                            {
                                if (valueWorkshop <= WorkshopNumberMax)
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
                                if (valueImporter <= ImporterNumberMax)
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
                                if (valueEquipment <= EquipmentNumberMax)
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

                if (dataChanged)
                {
                    UpdateCodingInfo();
                }
            }
        }

        private bool IsShortCoding()
        {
            if (_instanceData.CurrentCodingRequestType != null)
            {
                switch (_instanceData.CurrentCodingRequestType.Value)
                {
                    case XmlToolActivity.EcuInfo.CodingRequestType.ShortV1:
                    case XmlToolActivity.EcuInfo.CodingRequestType.ShortV2:
                        return true;
                }
            }

            return false;
        }

        private void UpdateCodingText()
        {
            string codingTextRaw = string.Empty;
            string codingTextShort = string.Empty;
            string codingTextShortTitle = string.Empty;
            string codingTextWorkshop = string.Empty;
            string codingTextImporter = string.Empty;
            string codingTextEquipment = string.Empty;
            string workshopNumberTitle = string.Empty;
            string importerNumberTitle = string.Empty;
            string equipmentNumberTitle = string.Empty;
            bool jobRunning = IsJobRunning();

            if (_instanceData.CurrentCoding != null)
            {
                try
                {
                    codingTextRaw = BitConverter.ToString(_instanceData.CurrentCoding).Replace("-", " ");

                    if (IsShortCoding())
                    {
                        byte[] dataArray = new byte[8];
                        int length = dataArray.Length < _instanceData.CurrentCoding.Length ? dataArray.Length : _instanceData.CurrentCoding.Length;
                        Array.Copy(_instanceData.CurrentCoding, dataArray, length);
                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(dataArray);
                        }
                        UInt64 value = BitConverter.ToUInt64(dataArray, 0);
                        codingTextShort = string.Format(CultureInfo.InvariantCulture, "{0}", value);

                        int resId;
                        switch (_codingMode)
                        {
                            case CodingMode.Login:
                                resId = Resource.String.vag_coding_login_title;
                                break;

                            case CodingMode.SecurityAccess:
                                resId = Resource.String.vag_coding_sec_access_title;
                                break;

                            default:
                                resId = Resource.String.vag_coding_short_title;
                                break;
                        }
                        codingTextShortTitle = string.Format(CultureInfo.InvariantCulture, GetString(resId), 0, _instanceData.CurrentCodingMax);
                    }

                    if (_instanceData.SelectedSubsystem == 0)
                    {
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
                    }

                    workshopNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_coding_workshop_number_title), 0, WorkshopNumberMax);
                    importerNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_coding_importer_number_title), 0, ImporterNumberMax);
                    equipmentNumberTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_coding_equipment_number_title), 0, EquipmentNumberMax);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _editTextVagCodingRaw.Text = codingTextRaw;
            _editTextVagCodingShort.Text = codingTextShort;
            _textViewVagCodingShortTitle.Text = codingTextShortTitle;
            _textViewVagWorkshopNumberTitle.Text = workshopNumberTitle;
            _editTextVagWorkshopNumber.Text = codingTextWorkshop;
            _textViewVagImporterNumberTitle.Text = importerNumberTitle;
            _editTextVagImporterNumber.Text = codingTextImporter;
            _textViewVagEquipmentNumberTitle.Text = equipmentNumberTitle;
            _editTextVagEquipmentNumber.Text = codingTextEquipment;
            _checkBoxEcuReset.Enabled = !jobRunning;
            _buttonCodingExecute.Enabled = !jobRunning;
        }

        private void UpdateCodingSelected(UdsFileReader.DataReader.DataInfoLongCoding dataInfoLongCoding, bool selectState)
        {
            if (_instanceData.CurrentCoding == null)
            {
                return;
            }
            if (dataInfoLongCoding.Byte == null)
            {
                return;
            }
            if (dataInfoLongCoding.Byte.Value >= _instanceData.CurrentCoding.Length)
            {
                return;
            }

            byte dataByte = _instanceData.CurrentCoding[dataInfoLongCoding.Byte.Value];
            if (dataInfoLongCoding.Bit != null)
            {
                byte mask = (byte) (1 << dataInfoLongCoding.Bit);
                if (selectState)
                {
                    dataByte |= mask;
                }
                else
                {
                    dataByte &= (byte) (~mask);
                }
            }
            else if (dataInfoLongCoding.BitMin != null && dataInfoLongCoding.BitMax != null && dataInfoLongCoding.BitValue != null)
            {
                byte mask = 0x00;
                for (int i = dataInfoLongCoding.BitMin.Value; i <= dataInfoLongCoding.BitMax.Value; i++)
                {
                    mask |= (byte)(1 << i);
                }
                dataByte &= (byte)(~mask);
                if (selectState)
                {
                    dataByte |= (byte)dataInfoLongCoding.BitValue.Value;
                }
            }

            _instanceData.CurrentCoding[dataInfoLongCoding.Byte.Value] = dataByte;
            UpdateCodingInfo();
        }

        private void UpdateCodingInfo(bool forceUpdate = false)
        {
            UpdateCodingText();

            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            bool shortCoding = IsShortCoding();
            StringBuilder sbCodingComment = new StringBuilder();
            List<TableResultItem> codingAssitantItems = new List<TableResultItem>();
            if (_instanceData.CurrentCoding != null && _instanceData.CurrentDataFileName != null)
            {
                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_instanceData.CurrentDataFileName);
                // ReSharper disable once UseNullPropagation
                if (udsReader != null)
                {
                    if (shortCoding)
                    {
                        UdsFileReader.DataReader.DataType dataType;
                        switch (_codingMode)
                        {
                            case CodingMode.Coding2:
                            case CodingMode.Login:
                                dataType = UdsFileReader.DataReader.DataType.Login;
                                break;

                            case CodingMode.SecurityAccess:
                                dataType = UdsFileReader.DataReader.DataType.Settings;
                                break;

                            default:
                                dataType = UdsFileReader.DataReader.DataType.Coding;
                                break;
                        }
                        List <UdsFileReader.DataReader.DataInfo> dataInfoCodingList =
                            udsReader.DataReader.ExtractDataType(_instanceData.CurrentDataFileName, dataType);
                        if (dataInfoCodingList != null)
                        {
                            foreach (UdsFileReader.DataReader.DataInfo dataInfo in dataInfoCodingList)
                            {
                                if (dataInfo.TextArray.Length > 0)
                                {
                                    if (sbCodingComment.Length > 0)
                                    {
                                        sbCodingComment.Append("\r\n");
                                    }
                                    sbCodingComment.Append(dataInfo.TextArray[0]);
                                }
                            }
                        }
                    }

                    if (_codingMode == CodingMode.Coding)
                    {
                        List<UdsFileReader.DataReader.DataInfo> dataInfoLcList =
                            udsReader.DataReader.ExtractDataType(_instanceData.CurrentDataFileName, UdsFileReader.DataReader.DataType.LongCoding);
                        if (dataInfoLcList != null)
                        {
                            string lastCommentLine = null;
                            TableResultItem lastGroupResultItem = null;
                            long lastGroupId = -1;
                            StringBuilder sbComment = new StringBuilder();
                            foreach (UdsFileReader.DataReader.DataInfo dataInfo in dataInfoLcList)
                            {
                                if (dataInfo is UdsFileReader.DataReader.DataInfoLongCoding dataInfoLongCoding)
                                {
                                    if (!string.IsNullOrEmpty(dataInfoLongCoding.Text))
                                    {
                                        bool textLine = dataInfoLongCoding.LineNumber != null;
                                        if (textLine)
                                        {
                                            if (lastCommentLine == null || (lastCommentLine != dataInfoLongCoding.Text))
                                            {
                                                if (sbComment.Length > 0)
                                                {
                                                    sbComment.Append("\r\n");
                                                }
                                                sbComment.Append(dataInfoLongCoding.Text);
                                            }
                                            lastCommentLine = dataInfoLongCoding.Text;
                                        }
                                        else
                                        {
                                            lastCommentLine = null;
                                            if (sbComment.Length > 0)
                                            {
                                                codingAssitantItems.Add(new TableResultItem(sbComment.ToString(), null, null, false, false));
                                                sbComment.Clear();
                                            }

                                            if (dataInfoLongCoding.Byte != null)
                                            {
                                                bool selected = false;
                                                bool enabled = false;
                                                long groupId = -1;
                                                byte? dataByte = null;
                                                StringBuilder sb = new StringBuilder();
                                                if (dataInfoLongCoding.Byte.Value < _instanceData.CurrentCoding.Length)
                                                {
                                                    dataByte = _instanceData.CurrentCoding[dataInfoLongCoding.Byte.Value];
                                                }
                                                sb.Append(string.Format("{0}", dataInfoLongCoding.Byte.Value));
                                                if (dataInfoLongCoding.Bit != null)
                                                {
                                                    if (dataByte.HasValue)
                                                    {
                                                        selected = (dataByte.Value & (1 << dataInfoLongCoding.Bit)) != 0;
                                                        enabled = true;
                                                    }
                                                    sb.Append(string.Format("/{0}", dataInfoLongCoding.Bit.Value));
                                                }
                                                else if (dataInfoLongCoding.BitMin != null && dataInfoLongCoding.BitMax != null && dataInfoLongCoding.BitValue != null)
                                                {
                                                    if (dataByte.HasValue)
                                                    {
                                                        byte mask = 0x00;
                                                        for (int i = dataInfoLongCoding.BitMin.Value; i <= dataInfoLongCoding.BitMax.Value; i++)
                                                        {
                                                            mask |= (byte)(1 << i);
                                                        }
                                                        selected = (dataByte.Value & mask) == dataInfoLongCoding.BitValue;
                                                        enabled = !selected;
                                                        groupId = (dataInfoLongCoding.Byte.Value << 16) + (dataInfoLongCoding.BitMin.Value << 8) + dataInfoLongCoding.BitMax.Value;
                                                    }
                                                    sb.Append(string.Format("/{0}-{1}={2:X02}",
                                                        dataInfoLongCoding.BitMin.Value, dataInfoLongCoding.BitMax.Value, dataInfoLongCoding.BitValue.Value));
                                                }

                                                sb.Append(" ");
                                                sb.Append(dataInfoLongCoding.Text);
                                                TableResultItem resultItem = new TableResultItem(sb.ToString(), null, dataInfoLongCoding, true, selected)
                                                {
                                                    CheckEnable = enabled
                                                };
                                                resultItem.CheckChangeEvent += item =>
                                                {
                                                    UpdateCodingSelected(item.Tag as UdsFileReader.DataReader.DataInfoLongCoding, item.Selected);
                                                };
                                                codingAssitantItems.Add(resultItem);
                                                if (groupId >= 0)
                                                {
                                                    if (lastGroupResultItem != null && lastGroupId >= 0 && lastGroupId == groupId)
                                                    {
                                                        lastGroupResultItem.SeparatorVisible = false;
                                                    }
                                                    lastGroupResultItem = resultItem;
                                                    lastGroupId = groupId;
                                                }
                                                else
                                                {
                                                    lastGroupResultItem = null;
                                                    lastGroupId = -1;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (sbComment.Length > 0)
                            {
                                codingAssitantItems.Add(new TableResultItem(sbComment.ToString(), null, null, false, false));
                                sbComment.Clear();
                            }
                        }
                    }
                }
            }

            _layoutVagCodingSubsystem.Visibility = _codingMode == CodingMode.Coding ? ViewStates.Visible : ViewStates.Gone;

            _layoutVagCodingShort.Visibility = shortCoding ? ViewStates.Visible : ViewStates.Gone;
            _editTextVagCodingShort.Enabled = shortCoding;

            _layoutVagCodingRaw.Visibility = _codingMode == CodingMode.Coding ? ViewStates.Visible : ViewStates.Gone;
            _editTextVagCodingRaw.Enabled = !shortCoding;

            string codingComment = sbCodingComment.ToString();
            _layoutVagCodingComments.Visibility = !string.IsNullOrWhiteSpace(codingComment) ? ViewStates.Visible : ViewStates.Gone;
            _textViewVagCodingComments.Text = codingComment;

            _layoutVagCodingRepairShopCode.Visibility = (_codingMode == CodingMode.Coding && _instanceData.SelectedSubsystem == 0) ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagCodingWorkshop.Visibility = _instanceData.CurrentWorkshopNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagCodingImporterNumber.Visibility = _instanceData.CurrentImporterNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;
            _layoutVagCodingEquipmentNumber.Visibility = _instanceData.CurrentEquipmentNumber.HasValue ? ViewStates.Visible : ViewStates.Gone;

            _checkBoxEcuReset.Visibility = isUdsEcu ? ViewStates.Visible : ViewStates.Gone;

            bool assistantChange = false;
            if (codingAssitantItems.Count == _layoutVagCodingAssitantAdapter.Items.Count)
            {
                for (int i = 0; i < codingAssitantItems.Count; i++)
                {
                    TableResultItem itemNew = codingAssitantItems[i];
                    TableResultItem itemOld = _layoutVagCodingAssitantAdapter.Items[i];
                    if (string.CompareOrdinal(itemNew.Text1 ?? string.Empty, itemOld.Text1 ?? string.Empty) != 0)
                    {
                        assistantChange = true;
                        break;
                    }

                    if (itemNew.CheckEnable != itemOld.CheckEnable)
                    {
                        assistantChange = true;
                        break;
                    }

                    if (itemNew.CheckVisible != itemOld.CheckVisible)
                    {
                        assistantChange = true;
                        break;
                    }

                    if (itemNew.Selected != itemOld.Selected)
                    {
                        assistantChange = true;
                        break;
                    }
                }
            }
            else
            {
                assistantChange = true;
            }

            bool widthChange = false;
            if (_listViewCodingAssistantWidthLast != _listViewVagCodingAssistant.Width)
            {
                _listViewCodingAssistantWidthLast = _listViewVagCodingAssistant.Width;
                assistantChange = true;
                widthChange = true;
            }

            if (forceUpdate || assistantChange)
            {
                _layoutVagCodingAssitantAdapter.Items.Clear();
                _layoutVagCodingAssitantAdapter.Items.AddRange(codingAssitantItems);
                _layoutVagCodingAssitantAdapter.NotifyDataSetChanged();
                AndroidUtility.SetListViewHeightBasedOnChildren(_listViewVagCodingAssistant);
                if (widthChange)
                {
                    _updateHandler.Post(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        UpdateCodingInfo(true);
                    });
                }
            }
            _layoutVagCodingAssitant.Visibility = _layoutVagCodingAssitantAdapter.Items.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
            _editTextVagCodingShort.ClearFocus();
            _editTextVagCodingRaw.ClearFocus();
            _editTextVagWorkshopNumber.ClearFocus();
            _editTextVagImporterNumber.ClearFocus();
            _editTextVagEquipmentNumber.ClearFocus();
        }

        private JobStatus CheckCodingResult()
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
            byte[] workShopData = BitConverter.GetBytes(_instanceData.CurrentWorkshopNumber ?? 0 & WorkshopNumberMask);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(workShopData);
            }

            byte[] importerNumberData = BitConverter.GetBytes((_instanceData.CurrentImporterNumber ?? 0 & ImporterNumberMask) << 1);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(importerNumberData);
            }

            byte[] equipmentNumberData = BitConverter.GetBytes((_instanceData.CurrentEquipmentNumber ?? 0 & EquipmentNumberMask) << 3);
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

        private void ExecuteCodingRequest()
        {
            bool valuesValid = true;
            if (_instanceData.CurrentWorkshopNumber != null && _instanceData.CurrentImporterNumber != null && _instanceData.CurrentEquipmentNumber != null)
            {
                valuesValid = _instanceData.CurrentWorkshopNumber.Value > 0 && _instanceData.CurrentWorkshopNumber.Value <= WorkshopNumberMax &&
                              _instanceData.CurrentImporterNumber.Value > 0 && _instanceData.CurrentImporterNumber.Value <= ImporterNumberMax &&
                              _instanceData.CurrentEquipmentNumber.Value > 0 && _instanceData.CurrentEquipmentNumber.Value <= EquipmentNumberMax;
            }

            if (!valuesValid)
            {
                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        ExecuteWriteCoding();
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

            ExecuteWriteCoding();
        }

        private void ExecuteWriteCoding()
        {
            if (_instanceData.CurrentCoding == null || _instanceData.CurrentCodingRequestType == null)
            {
                return;
            }
            if (IsJobRunning())
            {
                return;
            }

            EdiabasOpen();

            bool isUdsEcu = XmlToolActivity.IsUdsEcu(_ecuInfo);
            bool ecuReset = false;
            if (_checkBoxEcuReset.Visibility == ViewStates.Visible)
            {
                ecuReset = _checkBoxEcuReset.Checked;
            }

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.vag_coding_processing));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            bool executeFailed = false;
            bool finishUpdate = false;
            JobStatus jobStatus = JobStatus.Unknown;
            _jobThread = new Thread(() =>
            {
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, _ecuInfo.Sgbd);

                    bool shortCoding = false;
                    switch (_instanceData.CurrentCodingRequestType.Value)
                    {
                        case XmlToolActivity.EcuInfo.CodingRequestType.ShortV1:
                        case XmlToolActivity.EcuInfo.CodingRequestType.ShortV2:
                            shortCoding = true;
                            break;
                    }

                    UInt64 codingValue = 0;
                    if (shortCoding)
                    {
                        byte[] dataArray = new byte[8];
                        int length = dataArray.Length < _instanceData.CurrentCoding.Length ? dataArray.Length : _instanceData.CurrentCoding.Length;
                        Array.Copy(_instanceData.CurrentCoding, dataArray, length);
                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(dataArray);
                        }
                        codingValue = BitConverter.ToUInt64(dataArray, 0);
                    }

                    if (_codingMode == CodingMode.SecurityAccess && isUdsEcu)
                    {
                        // send dummy request to open the connection first
                        _ediabas.ArgString = "0xF19E";  // ASAM data
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(XmlToolActivity.JobReadS22Uds);

                        int dataOffset = XmlToolActivity.VagUdsRawDataOffset;
                        byte[] seed = null;
                        byte[] seedRequest = { 0x27, 0x03 };
                        _ediabas.EdInterfaceClass.TransmitData(seedRequest, out byte[] seedResponse);
                        if (seedResponse == null || seedResponse.Length < dataOffset + 6 || seedResponse[dataOffset + 0] != 0x67)
                        {
                            executeFailed = true;
                        }
                        else
                        {
                            seed = new byte[4];
                            Array.Copy(seedResponse, dataOffset + 2, seed, 0, seed.Length);
                        }

                        if (!executeFailed && seed != null)
                        {
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(seed);
                            }
                            UInt32 seedValue = BitConverter.ToUInt32(seed, 0);
                            UInt32 keyValue = (UInt32) (seedValue + codingValue);
                            byte[] keyData = BitConverter.GetBytes(keyValue);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(keyData);
                            }

                            byte[] keyRequest = { 0x27, 0x04, 0x00, 0x00, 0x00, 0x00 };
                            Array.Copy(keyData, 0, keyRequest, 2, keyData.Length);

                            _ediabas.EdInterfaceClass.TransmitData(keyRequest, out byte[] keyResponse);
                            if (keyResponse == null || keyResponse.Length < dataOffset + 2 || keyResponse[dataOffset + 0] != 0x67)
                            {
                                executeFailed = true;
                            }
                        }
                    }
                    else
                    {
                        string codingString = BitConverter.ToString(_instanceData.CurrentCoding).Replace("-", "");
                        string repairShopCodeString = string.Format(CultureInfo.InvariantCulture, "{0:000000}{1:000}{2:00000}",
                            _instanceData.CurrentEquipmentNumber ?? 0, _instanceData.CurrentImporterNumber ?? 0, _instanceData.CurrentWorkshopNumber ?? 0);
                        string repairShopCodeDataString = BitConverter.ToString(GetRepairShopCodeData()).Replace("-", "");
                        string writeJobProgDate = string.Empty;
                        string writeJobProgDateArgs = string.Empty;
                        string writeJobRscName = string.Empty;
                        string writeJobRscArgs = string.Empty;
                        string writeJobName = string.Empty;
                        string writeJobArgs = string.Empty;

                        if (_codingMode != CodingMode.Coding)
                        {
                            switch (_codingMode)
                            {
                                case CodingMode.Coding2:
                                    writeJobName = XmlToolActivity.JobWriteEcuCoding2;
                                    writeJobArgs = repairShopCodeString + string.Format(CultureInfo.InvariantCulture, ";{0}", codingValue);
                                    break;

                                case CodingMode.Login:
                                    writeJobName = XmlToolActivity.JobWriteLogin;
                                    writeJobArgs = string.Format(CultureInfo.InvariantCulture, "{0:00000};{1}", _ecuInfo.VagWorkshopNumber ?? 0, codingValue);
                                    break;

                                case CodingMode.SecurityAccess:
                                    writeJobName = XmlToolActivity.JobWriteLogin;
                                    // a repairShopCodeString of 0 is rejected, but the value is not used, so we specify a dummy
                                    // the max allowed code is 99999 here
                                    writeJobArgs = string.Format(CultureInfo.InvariantCulture, "00000100100001;{0}", codingValue);
                                    break;
                            }
                        }
                        else
                        {
                            switch (_instanceData.CurrentCodingRequestType.Value)
                            {
                                case XmlToolActivity.EcuInfo.CodingRequestType.ShortV1:
                                case XmlToolActivity.EcuInfo.CodingRequestType.ShortV2:
                                    writeJobName = XmlToolActivity.JobWriteEcuCoding;
                                    writeJobArgs = repairShopCodeString + string.Format(CultureInfo.InvariantCulture, ";{0};{1}", codingValue, _ecuInfo.VagCodingTypeValue ?? 0x03);
                                    break;

                                case XmlToolActivity.EcuInfo.CodingRequestType.LongUds:
                                    writeJobName = XmlToolActivity.JobWriteS2EUds;
                                    if (_instanceData.SelectedSubsystem == 0)
                                    {
                                        writeJobArgs = "0x0600;" + codingString;

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
                                    else
                                    {
                                        if (_ecuInfo.SubSystems == null || _instanceData.SelectedSubsystem >= _ecuInfo.SubSystems.Count)
                                        {
                                            break;
                                        }
                                        XmlToolActivity.EcuInfoSubSys subSystem = _ecuInfo.SubSystems[_instanceData.SelectedSubsystem];
                                        writeJobArgs = string.Format(CultureInfo.InvariantCulture, "{0};{1}", 0x6000 + subSystem.SubSysAddr, codingString);
                                    }
                                    break;

                                case XmlToolActivity.EcuInfo.CodingRequestType.ReadLong:
                                    writeJobName = XmlToolActivity.JobWriteLongCoding;
                                    writeJobArgs = repairShopCodeString + ";" + codingString + string.Format(CultureInfo.InvariantCulture, ";{0}", _ecuInfo.VagCodingTypeValue ?? 0x10);
                                    break;

                                case XmlToolActivity.EcuInfo.CodingRequestType.CodingS22:
                                    writeJobName = XmlToolActivity.JobWriteCoding;
                                    writeJobArgs = repairShopCodeString + ";" + codingString + string.Format(CultureInfo.InvariantCulture, ";{0}", _ecuInfo.VagCodingTypeValue ?? 0x10);
                                    break;
                            }
                        }

                        if (!executeFailed && !string.IsNullOrEmpty(writeJobProgDate))
                        {
                            _ediabas.ArgString = writeJobProgDateArgs;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(writeJobProgDate);

                            jobStatus = CheckCodingResult();
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

                            jobStatus = CheckCodingResult();
                            if (jobStatus != JobStatus.Ok)
                            {
                                executeFailed = true;
                            }
                        }

                        if (!executeFailed && !string.IsNullOrEmpty(writeJobName))
                        {
                            _ediabas.ArgString = writeJobArgs;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(writeJobName);

                            jobStatus = CheckCodingResult();
                            if (jobStatus != JobStatus.Ok)
                            {
                                executeFailed = true;
                            }
                        }

                        if (!executeFailed && ecuReset && isUdsEcu)
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
                            switch (_codingMode)
                            {
                                case CodingMode.Coding2:
                                    resId = Resource.String.vag_coding_write_coding2_failed;
                                    break;

                                case CodingMode.Login:
                                    resId = Resource.String.vag_coding_login_job_failed;
                                    break;

                                case CodingMode.SecurityAccess:
                                    resId = Resource.String.vag_coding_sec_access_job_failed;
                                    break;

                                default:
                                    resId = Resource.String.vag_coding_write_coding_failed;
                                    break;
                            }
                        }
                        _activityCommon.ShowAlert(GetString(resId), Resource.String.alert_title_error);
                        _updateHandler?.Post(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            UpdateCodingInfo();
                        });
                    }
                    else
                    {
                        int resId;
                        switch (_codingMode)
                        {
                            case CodingMode.Login:
                                resId = Resource.String.vag_coding_login_job_ok;
                                break;

                            case CodingMode.SecurityAccess:
                                resId = Resource.String.vag_coding_sec_access_job_ok;
                                break;

                            default:
                                resId = Resource.String.vag_coding_write_coding_ok;
                                break;
                        }

                        AlertDialog alertDialog = new AlertDialog.Builder(this)
                            .SetMessage(resId)
                            .SetTitle(Resource.String.alert_title_info)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                            .Show();
                        if (finishUpdate || _codingMode == CodingMode.Coding)
                        {
                            if (alertDialog != null)
                            {
                                alertDialog.DismissEvent += (sender, args) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    _ecuInfo.JobListValid = false;    // force update
                                    SetResult(Android.App.Result.Ok);
                                    Finish();
                                };
                            }
                        }
                        else
                        {
                            if (IsJobRunning())
                            {
                                _jobThread?.Join();
                            }
                            UpdateCodingInfo();
                        }
                    }
                });
            });
            _jobThread.Start();

            UpdateCodingText();
        }
    }
}
