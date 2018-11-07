using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
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
    public class VagCodingActivity : AppCompatActivity, View.IOnTouchListener
    {
        public class InstanceData
        {
            public int SelectedSubsystem { get; set; }
            public byte[] CurrentCoding { get; set; }
            public XmlToolActivity.EcuInfo.CodingType? CurrentCodingType { get; set; }
            public UInt64? CurrentCodingMax { get; set; }
            public UInt64 CurrentWorkshopNumber { get; set; }
            public UInt64 CurrentImporterNumber { get; set; }
            public UInt64 CurrentEquipmentNumber { get; set; }
            public string CurrentDataFileName { get; set; }
        }

        // Intent extra
        public const string ExtraEcuName = "ecu_name";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraTraceDir = "trace_dir";
        public const string ExtraTraceAppend = "trace_append";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";

        public static XmlToolActivity.EcuInfo IntentEcuInfo { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private InputMethodManager _imm;
        private View _contentView;
        private LinearLayout _layoutVagCoding;
        private TextView _textViewVagCodingSubsystem;
        private Spinner _spinnerVagCodingSubsystem;
        private StringObjAdapter _spinnerVagCodingSubsystemAdapter;
        private LinearLayout _layoutVagCodingShort;
        private TextView _textViewVagCodingShortTitle;
        private EditText _editTextVagCodingShort;
        private LinearLayout _layoutVagCodingComments;
        private TextView _textViewVagCodingCommentsTitle;
        private TextView _textViewCodingComments;
        private TextView _textViewVagCodingRawTitle;
        private EditText _editTextVagCodingRaw;
        private LinearLayout _layoutVagCodingRepairShopCode;
        private TextView _textViewVagWorkshopNumberTitle;
        private EditText _editTextVagWorkshopNumber;
        private TextView _textViewVagImporterNumberTitle;
        private EditText _editTextVagImporterNumber;
        private TextView _textViewVagEquipmentNumberTitle;
        private EditText _editTextVagEquipmentNumber;
        private Button _buttonCodingWrite;
        private LinearLayout _layoutVagCodingAssitant;
        private ResultListAdapter _layoutVagCodingAssitantAdapter;
        private ListView _listViewVagCodingAssistant;
        private ActivityCommon _activityCommon;
        private XmlToolActivity.EcuInfo _ecuInfo;
        private EdiabasNet _ediabas;
        private Thread _jobThread;
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
            SupportActionBar.Title = string.Format(GetString(Resource.String.vag_coding_title), Intent.GetStringExtra(ExtraEcuName) ?? string.Empty);
            SetContentView(Resource.Layout.vag_coding);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            SetResult(Android.App.Result.Canceled);

            _activityCommon = new ActivityCommon(this, () =>
            {

            }, BroadcastReceived);

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _traceDir = Intent.GetStringExtra(ExtraTraceDir);
            _traceAppend = Intent.GetBooleanExtra(ExtraTraceAppend, true);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);

            _ecuInfo = IntentEcuInfo;

            _layoutVagCoding = FindViewById<LinearLayout>(Resource.Id.layoutVagCoding);
            _layoutVagCoding.SetOnTouchListener(this);

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

            _textViewCodingComments = FindViewById<TextView>(Resource.Id.textViewCodingComments);
            _textViewCodingComments.SetOnTouchListener(this);
            _textViewCodingComments.MovementMethod = new ScrollingMovementMethod();

            _textViewVagCodingRawTitle = FindViewById<TextView>(Resource.Id.textViewVagCodingRawTitle);
            _textViewVagCodingRawTitle.SetOnTouchListener(this);

            _editTextVagCodingRaw = FindViewById<EditText>(Resource.Id.editTextVagCodingRaw);
            _editTextVagCodingRaw.EditorAction += CodingEditorAction;

            _layoutVagCodingRepairShopCode = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingRepairShopCode);
            _layoutVagCodingRepairShopCode.SetOnTouchListener(this);

            _textViewVagWorkshopNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagWorkshopNumberTitle);
            _textViewVagWorkshopNumberTitle.SetOnTouchListener(this);

            _editTextVagWorkshopNumber = FindViewById<EditText>(Resource.Id.editTextVagWorkshopNumber);
            _editTextVagWorkshopNumber.EditorAction += CodingEditorAction;

            _textViewVagImporterNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagImporterNumberTitle);
            _textViewVagImporterNumberTitle.SetOnTouchListener(this);

            _editTextVagImporterNumber = FindViewById<EditText>(Resource.Id.editTextVagImporterNumber);
            _editTextVagImporterNumber.EditorAction += CodingEditorAction;

            _textViewVagEquipmentNumberTitle = FindViewById<TextView>(Resource.Id.textViewVagEquipmentNumberTitle);
            _textViewVagEquipmentNumberTitle.SetOnTouchListener(this);

            _editTextVagEquipmentNumber = FindViewById<EditText>(Resource.Id.editTextVagEquipmentNumber);
            _editTextVagEquipmentNumber.EditorAction += CodingEditorAction;

            _buttonCodingWrite = FindViewById<Button>(Resource.Id.buttonCodingWrite);
            _buttonCodingWrite.SetOnTouchListener(this);
            _buttonCodingWrite.Click += (sender, args) =>
            {
                ExecuteWriteCoding();
            };

            _layoutVagCodingAssitant = FindViewById<LinearLayout>(Resource.Id.layoutVagCodingAssitant);
            _layoutVagCodingAssitant.SetOnTouchListener(this);

            _listViewVagCodingAssistant = FindViewById<ListView>(Resource.Id.listViewVagCodingAssistant);
            _layoutVagCodingAssitantAdapter = new ResultListAdapter(this, -1, 0, true);
            _listViewVagCodingAssistant.Adapter = _layoutVagCodingAssitantAdapter;
            _listViewVagCodingAssistant.SetOnTouchListener(this);

            UpdateCodingSubsystemList();
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
                    ReadCodingEditors();
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

        private void SubSystemItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            HideKeyboard();
            UpdateCoding();
        }

        private void UpdateCodingSubsystemList()
        {
            int selection = _instanceData.SelectedSubsystem;

            _spinnerVagCodingSubsystemAdapter.Items.Clear();
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
            _spinnerVagCodingSubsystemAdapter.NotifyDataSetChanged();

            _spinnerVagCodingSubsystem.SetSelection(selection);
            UpdateCoding();
        }

        private void UpdateCoding()
        {
            _instanceData.CurrentCoding = null;
            _instanceData.CurrentDataFileName = null;
            if (_spinnerVagCodingSubsystem.SelectedItemPosition >= 0)
            {
                int subSystemIndex = (int)_spinnerVagCodingSubsystemAdapter.Items[_spinnerVagCodingSubsystem.SelectedItemPosition].Data;
                _instanceData.SelectedSubsystem = subSystemIndex;

                string dataFileName = null;
                byte[] coding = null;
                XmlToolActivity.EcuInfo.CodingType? codingType = null;
                UInt64? codingMax = null;
                UInt64 workshopNumber = 0;
                UInt64 importerNumber = 0;
                UInt64 equipmentNumber = 0;

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
                        if (codingLength > 0 && codingData.Length >= codingLength)
                        {
                            coding = new byte[codingLength];
                            Array.Copy(codingData, coding, coding.Length);
                        }
                    }

                    codingType = _ecuInfo.VagCodingType;
                    codingMax = _ecuInfo.VagCodingMax;
                    workshopNumber = _ecuInfo.VagWorkshopNumber ?? 0;
                    importerNumber = _ecuInfo.VagImporterNumber ?? 0;
                    equipmentNumber = _ecuInfo.VagEquipmentNumber ?? 0;
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
                                codingType = XmlToolActivity.EcuInfo.CodingType.LongUds;
                                dataFileName = subSystem.VagDataFileName;
                                break;
                            }
                        }
                    }
                }

                if (coding != null)
                {
                    _instanceData.CurrentCoding = new byte[coding.Length];
                    Array.Copy(coding, _instanceData.CurrentCoding, coding.Length);
                }

                _instanceData.CurrentCodingType = codingType;
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
                    break;
            }
        }

        private void ReadCodingEditors()
        {
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
                        Array.Copy(dataArray, _instanceData.CurrentCoding, dataArray.Length);
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
                                if (codingData.Length >= _instanceData.CurrentCoding.Length)
                                {
                                    Array.Copy(codingData, _instanceData.CurrentCoding, _instanceData.CurrentCoding.Length);
                                }
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
                        if (UInt64.TryParse(_editTextVagWorkshopNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 valueWorkshop))
                        {
                            if (valueWorkshop <= 99999)
                            {
                                _instanceData.CurrentWorkshopNumber = valueWorkshop;
                            }
                        }
                        if (UInt64.TryParse(_editTextVagImporterNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 valueImporter))
                        {
                            if (valueImporter <= 999)
                            {
                                _instanceData.CurrentImporterNumber = valueImporter;
                            }
                        }
                        if (UInt64.TryParse(_editTextVagEquipmentNumber.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt64 valueEquipment))
                        {
                            if (valueEquipment <= 999999)
                            {
                                _instanceData.CurrentEquipmentNumber = valueEquipment;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                }

                UpdateCodingInfo();
            }
        }

        private bool IsShortCoding()
        {
            if (_instanceData.CurrentCodingType != null)
            {
                switch (_instanceData.CurrentCodingType.Value)
                {
                    case XmlToolActivity.EcuInfo.CodingType.ShortV1:
                    case XmlToolActivity.EcuInfo.CodingType.ShortV2:
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
                        UInt64 value = BitConverter.ToUInt64(dataArray, 0);
                        codingTextShort = string.Format(CultureInfo.InvariantCulture, "{0}", value);
                        codingTextShortTitle = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_coding_short_title), 0, _instanceData.CurrentCodingMax);
                    }

                    if (_instanceData.SelectedSubsystem == 0)
                    {
                        codingTextWorkshop = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.CurrentWorkshopNumber);
                        codingTextImporter = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.CurrentImporterNumber);
                        codingTextEquipment = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.CurrentEquipmentNumber);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _editTextVagCodingRaw.Text = codingTextRaw;
            _editTextVagCodingShort.Text = codingTextShort;
            _textViewVagCodingShortTitle.Text = codingTextShortTitle;
            _editTextVagWorkshopNumber.Text = codingTextWorkshop;
            _editTextVagImporterNumber.Text = codingTextImporter;
            _editTextVagEquipmentNumber.Text = codingTextEquipment;
            _buttonCodingWrite.Enabled = !IsJobRunning();
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

        private void UpdateCodingInfo()
        {
            UpdateCodingText();

            bool shortCoding = IsShortCoding();
            StringBuilder sbCodingComment = new StringBuilder();
            _layoutVagCodingAssitantAdapter.Items.Clear();
            if (_instanceData.CurrentCoding != null && _instanceData.CurrentDataFileName != null)
            {
                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(_instanceData.CurrentDataFileName);
                // ReSharper disable once UseNullPropagation
                if (udsReader != null)
                {
                    if (shortCoding)
                    {
                        List<UdsFileReader.DataReader.DataInfo> dataInfoCodingList =
                            udsReader.DataReader.ExtractDataType(_instanceData.CurrentDataFileName, UdsFileReader.DataReader.DataType.Coding);
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
                                            _layoutVagCodingAssitantAdapter.Items.Add(new TableResultItem(sbComment.ToString(), null, null, false, false));
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
                                            _layoutVagCodingAssitantAdapter.Items.Add(resultItem);
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
                            _layoutVagCodingAssitantAdapter.Items.Add(new TableResultItem(sbComment.ToString(), null, null, false, false));
                            sbComment.Clear();
                        }
                    }
                }
            }

            _layoutVagCodingShort.Visibility = shortCoding ? ViewStates.Visible : ViewStates.Gone;
            _editTextVagCodingShort.Enabled = shortCoding;
            _editTextVagCodingRaw.Enabled = !shortCoding;

            _textViewCodingComments.Text = sbCodingComment.ToString();
            _layoutVagCodingComments.Visibility = sbCodingComment.Length > 0 ? ViewStates.Visible : ViewStates.Gone;

            _layoutVagCodingRepairShopCode.Visibility = _instanceData.SelectedSubsystem == 0 ? ViewStates.Visible : ViewStates.Gone;

            _layoutVagCodingAssitantAdapter.NotifyDataSetChanged();
            _layoutVagCodingAssitant.Visibility = _layoutVagCodingAssitantAdapter.Items.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private void ExecuteWriteCoding()
        {
            if (_instanceData.CurrentCoding == null || _instanceData.CurrentCodingType == null)
            {
                return;
            }
            if (IsJobRunning())
            {
                return;
            }

            EdiabasOpen();

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.xml_tool_execute_test_job));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            bool executeFailed = false;
            _jobThread = new Thread(() =>
            {
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, _ecuInfo.Sgbd);

                    string codingString = BitConverter.ToString(_instanceData.CurrentCoding).Replace("-", "");
                    string writeJobName = string.Empty;
                    string writeJobArgs = string.Empty;

                    bool shortCoding = false;
                    switch (_instanceData.CurrentCodingType.Value)
                    {
                        case XmlToolActivity.EcuInfo.CodingType.ShortV1:
                        case XmlToolActivity.EcuInfo.CodingType.ShortV2:
                            shortCoding = true;
                            break;
                    }

                    UInt64 codingValue = 0;
                    string repairShopCodeString = string.Empty;
                    if (shortCoding)
                    {
                        byte[] dataArray = new byte[8];
                        int length = dataArray.Length < _instanceData.CurrentCoding.Length ? dataArray.Length : _instanceData.CurrentCoding.Length;
                        Array.Copy(_instanceData.CurrentCoding, dataArray, length);
                        codingValue = BitConverter.ToUInt64(dataArray, 0);

                        repairShopCodeString = string.Format(CultureInfo.InvariantCulture, "{0:000000}{1:000}{2:00000}",
                            _instanceData.CurrentEquipmentNumber, _instanceData.CurrentImporterNumber, _instanceData.CurrentWorkshopNumber);
                    }

                    XmlToolActivity.EcuInfoSubSys subSystem = null;
                    switch (_instanceData.CurrentCodingType.Value)
                    {
                        case XmlToolActivity.EcuInfo.CodingType.ShortV1:
                            writeJobName = XmlToolActivity.JobWriteCodingV1;
                            writeJobArgs = repairShopCodeString + ";" + "3;" + string.Format(CultureInfo.InvariantCulture, "{0}", codingValue);
                            break;

                        case XmlToolActivity.EcuInfo.CodingType.ShortV2:
                            writeJobName = XmlToolActivity.JobWriteCodingV2;
                            writeJobArgs = repairShopCodeString + ";" + string.Format(CultureInfo.InvariantCulture, "{0}", codingValue);
                            break;

                        case XmlToolActivity.EcuInfo.CodingType.LongUds:
                            writeJobName = XmlToolActivity.JobWriteS2EUds;
                            if (_instanceData.SelectedSubsystem == 0)
                            {
                                writeJobArgs = "0x0600;" + codingString;
                            }
                            else
                            {
                                if (_ecuInfo.SubSystems == null || _instanceData.SelectedSubsystem >= _ecuInfo.SubSystems.Count)
                                {
                                    break;
                                }
                                subSystem = _ecuInfo.SubSystems[_instanceData.SelectedSubsystem];
                                writeJobArgs = string.Format(CultureInfo.InvariantCulture, "{0};{1}", 0x6000 + subSystem.SubSysAddr, codingString);
                            }
                            break;

                        case XmlToolActivity.EcuInfo.CodingType.ReadLong:
                            writeJobName = XmlToolActivity.JobWriteLongCoding;
                            writeJobArgs = codingString;
                            break;

                        case XmlToolActivity.EcuInfo.CodingType.CodingS22:
                            writeJobName = XmlToolActivity.JobWriteCoding;
                            writeJobArgs = codingString;
                            break;
                    }

                    if (string.IsNullOrEmpty(writeJobName))
                    {
                        throw new Exception("Not supported");
                    }
                    _ediabas.ArgString = writeJobArgs;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob(writeJobName);

                    bool resultOk = false;
                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[0];
                        if (resultDict.TryGetValue("JOBSTATUS", out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                string result = (string)resultData.OpData;
                                if (string.Compare(result, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    resultOk = true;
                                }
                            }
                        }
                    }

                    if (!resultOk)
                    {
                        executeFailed = true;
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
                        _activityCommon.ShowAlert(GetString(Resource.String.vag_coding_write_coding_failed), Resource.String.alert_title_error);
                        UpdateCoding();
                    }
                    else
                    {
                        AlertDialog alertDialog = new AlertDialog.Builder(this)
                            .SetMessage(Resource.String.vag_coding_write_coding_ok)
                            .SetTitle(Resource.String.alert_title_info)
                            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
                            .Show();
                        alertDialog.DismissEvent += (sender, args) =>
                        {
                            _ecuInfo.JobList = null;    // force update
                            SetResult(Android.App.Result.Ok);
                            Finish();
                        };
                    }
                });
            });
            _jobThread.Start();

            UpdateCodingText();
        }
    }
}
