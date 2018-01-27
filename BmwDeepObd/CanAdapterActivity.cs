using System;
using System.Linq;
using System.Threading;
using System.Text;
using Android.Bluetooth;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using EdiabasLib;
using System.IO;
using System.Net.Sockets;
using Android.Content;
using Android.Hardware.Usb;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/can_adapter_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                    Android.Content.PM.ConfigChanges.Orientation |
                    Android.Content.PM.ConfigChanges.ScreenSize)]
    public class CanAdapterActivity : AppCompatActivity, View.IOnTouchListener
    {
        // Intent extra
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraInterfaceType = "interface_type";

        private enum AdapterMode
        {
            CanOff = 0x00,
            Can500 = 0x01,
            Can100 = 0x09,
            CanAuto = 0xFF,
        }

        public class InstanceData
        {
            public bool FwUpdateShown { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private InputMethodManager _imm;
        private View _contentView;
        private View _barView;
        private Button _buttonRead;
        private Button _buttonWrite;
        private LinearLayout _layoutCanAdapter;
        private Spinner _spinnerCanAdapterMode;
        private StringObjAdapter _spinnerCanAdapterModeAdapter;
        private TextView _textViewCanAdapterSepTimeTitle;
        private Spinner _spinnerCanAdapterSepTime;
        private StringAdapter _spinnerCanAdapterSepTimeAdapter;
        private TextView _textViewCanAdapterBlockSizeTitle;
        private Spinner _spinnerCanAdapterBlockSize;
        private StringAdapter _spinnerCanAdapterBlockSizeAdapter;
        private TextView _textViewBtPinTitle;
        private EditText _editTextBtPin;
        private TextView _textViewBtNameTitle;
        private EditText _editTextBtName;
        private TextView _textViewCanAdapterIgnitionStateTitle;
        private TextView _textViewIgnitionState;
        private TextView _textViewBatteryVoltageTitle;
        private TextView _textViewBatteryVoltage;
        private TextView _textViewFwVersionTitle;
        private TextView _textViewFwVersion;
        private TextView _textViewSerNumTitle;
        private TextView _textViewSerNum;
        private Button _buttonFwUpdate;
        private CheckBox _checkBoxExpert;
        private bool _activityActive;
        private string _deviceAddress = string.Empty;
        private int _blockSize = -1;
        private int _separationTime = -1;
        private int _canMode = -1;
        private int _ignitionState = -1;
        private int _batteryVoltage = -1;
        private int _adapterType = -1;
        private int _fwVersion = -1;
        private byte[] _serNum;
        private byte[] _btPin;
        private byte[] _btName;
        private int _clampStatus = -1;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private Thread _adapterThread;

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
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.can_adapter_config);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            _barView = LayoutInflater.Inflate(Resource.Layout.bar_can_adapter, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(_barView, barLayoutParams);

            SetResult(Android.App.Result.Canceled);

            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            ActivityCommon.InterfaceType interfaceType = (ActivityCommon.InterfaceType) Intent.GetIntExtra(ExtraInterfaceType,
                (int) ActivityCommon.InterfaceType.Bluetooth);
            ViewStates visibility = IsCustomAdapter(interfaceType) ? ViewStates.Visible : ViewStates.Gone;

            _buttonRead = _barView.FindViewById<Button>(Resource.Id.buttonAdapterRead);
            _buttonRead.SetOnTouchListener(this);
            _buttonRead.Click += (sender, args) =>
            {
                PerformRead();
            };
            _buttonRead.Visibility = visibility;

            _buttonWrite = _barView.FindViewById<Button>(Resource.Id.buttonAdapterWrite);
            _buttonWrite.SetOnTouchListener(this);
            _buttonWrite.Click += (sender, args) =>
            {
                PerformWrite();
            };

            _layoutCanAdapter = FindViewById<LinearLayout>(Resource.Id.layoutCanAdapter);
            _layoutCanAdapter.SetOnTouchListener(this);

            _spinnerCanAdapterMode = FindViewById<Spinner>(Resource.Id.spinnerCanAdapterMode);
            _spinnerCanAdapterMode.SetOnTouchListener(this);
            _spinnerCanAdapterModeAdapter = new StringObjAdapter(this);
            _spinnerCanAdapterMode.Adapter = _spinnerCanAdapterModeAdapter;
            _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_500), AdapterMode.Can500));
            _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_100), AdapterMode.Can100));
            _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_off), AdapterMode.CanOff));
            _spinnerCanAdapterModeAdapter.NotifyDataSetChanged();

            _textViewCanAdapterSepTimeTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterSepTimeTitle);
            _textViewCanAdapterSepTimeTitle.Visibility = visibility;

            _spinnerCanAdapterSepTime = FindViewById<Spinner>(Resource.Id.spinnerCanAdapterSepTime);
            _spinnerCanAdapterSepTime.SetOnTouchListener(this);
            _spinnerCanAdapterSepTimeAdapter = new StringAdapter(this);
            _spinnerCanAdapterSepTime.Adapter = _spinnerCanAdapterSepTimeAdapter;
            _spinnerCanAdapterSepTimeAdapter.Items.Add(GetString(Resource.String.can_adapter_text_off));
            for (int i = 1; i <= 2; i++)
            {
                _spinnerCanAdapterSepTimeAdapter.Items.Add(i.ToString());
            }
            _spinnerCanAdapterSepTimeAdapter.NotifyDataSetChanged();
            _spinnerCanAdapterSepTime.Visibility = visibility;

            _textViewCanAdapterBlockSizeTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterBlockSizeTitle);
            _textViewCanAdapterBlockSizeTitle.Visibility = visibility;

            _spinnerCanAdapterBlockSize = FindViewById<Spinner>(Resource.Id.spinnerCanAdapterBlockSize);
            _spinnerCanAdapterBlockSize.SetOnTouchListener(this);
            _spinnerCanAdapterBlockSizeAdapter = new StringAdapter(this);
            _spinnerCanAdapterBlockSize.Adapter = _spinnerCanAdapterBlockSizeAdapter;
            _spinnerCanAdapterBlockSizeAdapter.Items.Add(GetString(Resource.String.can_adapter_text_off));
            for (int i = 0; i <= 15; i++)
            {
                _spinnerCanAdapterBlockSizeAdapter.Items.Add(i.ToString());
            }
            _spinnerCanAdapterBlockSizeAdapter.NotifyDataSetChanged();
            _spinnerCanAdapterBlockSize.Visibility = visibility;

            _textViewBtPinTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterBtPinTitle);
            _textViewBtPinTitle.Visibility = visibility;

            _editTextBtPin = FindViewById<EditText>(Resource.Id.editTextBtPin);
            _editTextBtPin.Visibility = visibility;

            _textViewBtNameTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterBtNameTitle);
            _textViewBtNameTitle.Visibility = visibility;

            _editTextBtName = FindViewById<EditText>(Resource.Id.editTextBtName);
            _editTextBtName.Visibility = visibility;

            _textViewCanAdapterIgnitionStateTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterIgnitionStateTitle);
            _textViewCanAdapterIgnitionStateTitle.Visibility = visibility;

            _textViewIgnitionState = FindViewById<TextView>(Resource.Id.textViewCanAdapterIgnitionState);
            _textViewIgnitionState.Visibility = visibility;

            _textViewBatteryVoltageTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterBatVoltageTitle);
            _textViewBatteryVoltageTitle.Visibility = visibility;

            _textViewBatteryVoltage = FindViewById<TextView>(Resource.Id.textViewCanAdapterBatVoltage);
            _textViewBatteryVoltage.Visibility = visibility;

            _textViewFwVersionTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterFwVersionTitle);
            _textViewFwVersionTitle.Visibility = visibility;

            _textViewFwVersion = FindViewById<TextView>(Resource.Id.textViewCanAdapterFwVersion);
            _textViewFwVersion.Visibility = visibility;

            _textViewSerNumTitle = FindViewById<TextView>(Resource.Id.textViewCanAdapterSerNumTitle);
            _textViewSerNum = FindViewById<TextView>(Resource.Id.textViewCanAdapterSerNum);
#if DEBUG
            _textViewSerNumTitle.Visibility = visibility;
            _textViewSerNum.Visibility = visibility;
#else
            _textViewSerNumTitle.Visibility = ViewStates.Gone;
            _textViewSerNum.Visibility = ViewStates.Gone;
#endif
            _buttonFwUpdate = FindViewById<Button>(Resource.Id.buttonCanAdapterFwUpdate);
            _buttonFwUpdate.Visibility = visibility;
            _buttonFwUpdate.Click += (sender, args) =>
            {
                PerformUpdateMessage();
            };
            _checkBoxExpert = FindViewById<CheckBox>(Resource.Id.checkBoxCanAdapterExpert);
            _checkBoxExpert.Visibility = visibility;
            _checkBoxExpert.Click += (sender, args) =>
            {
                UpdateDisplay();
            };

            _activityCommon = new ActivityCommon(this, () =>
            {
            }, BroadcastReceived)
            {
                SelectedInterface = interfaceType
            };

            UpdateDisplay();
            PerformRead();
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

            if (IsJobRunning())
            {
                _adapterThread.Join();
            }
            EdiabasClose();
            _activityCommon.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            if (!IsJobRunning())
            {
                base.OnBackPressed();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsJobRunning())
                    {
                        return true;
                    }
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
                    HideKeyboard();
                    break;
            }
            return false;
        }

        private void EdiabasInit()
        {
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet
                {
                    EdInterfaceClass = new EdInterfaceObd()
                };
                _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress);
            }
        }

        private bool InterfacePrepare()
        {
            if (!_ediabas.EdInterfaceClass.Connected)
            {
                if (!_ediabas.EdInterfaceClass.InterfaceConnect())
                {
                    return false;
                }
                _ediabas.EdInterfaceClass.CommParameter =
                    new UInt32[] {0x0000010F, 0x0001C200, 0x000004B0, 0x00000014, 0x0000000A, 0x00000002, 0x00001388};
                _ediabas.EdInterfaceClass.CommAnswerLen =
                    new Int16[] {0x0000, 0x0000};
            }
            return true;
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
            UpdateDisplay();
            return true;
        }

        private bool IsJobRunning()
        {
            if (_adapterThread == null)
            {
                return false;
            }
            if (_adapterThread.IsAlive)
            {
                return true;
            }
            _adapterThread = null;
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
                            UpdateDisplay();
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

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private void UpdateDisplay()
        {
            bool requestFwUpdate = false;
            bool bEnabled = !IsJobRunning();
            bool fwUpdateEnabled = bEnabled;
            bool expertMode = _checkBoxExpert.Checked;
            bool mtcService = _activityCommon.MtcBtService;
            _buttonRead.Enabled = bEnabled;
            _buttonWrite.Enabled = bEnabled;
            _editTextBtPin.Enabled = bEnabled && !mtcService && _btPin != null && _btPin.Length >= 4;
            int maxPinLength = (_btPin != null && _btPin.Length > 0) ? _btPin.Length : 4;
            _editTextBtPin.SetFilters(new Android.Text.IInputFilter[] { new Android.Text.InputFilterLengthFilter(maxPinLength) });
            if (!_editTextBtPin.Enabled && !mtcService)
            {
                _editTextBtPin.Text = string.Empty;
            }
            _editTextBtName.Enabled = bEnabled && !mtcService && _btName != null && _btName.Length > 0;
            int maxTextLength = (_btName != null && _btName.Length > 0) ? _btName.Length : 16;
            _editTextBtName.SetFilters(new Android.Text.IInputFilter[] { new Android.Text.InputFilterLengthFilter(maxTextLength) });
            if (!_editTextBtName.Enabled && !mtcService)
            {
                _editTextBtName.Text = string.Empty;
            }

            _textViewSerNum.Enabled = bEnabled;

            if (bEnabled)
            {
                if ((_separationTime < 0) || (_separationTime >= _spinnerCanAdapterSepTimeAdapter.Items.Count))
                {
                    _spinnerCanAdapterSepTime.SetSelection(0);
                }
                else
                {
                    _spinnerCanAdapterSepTime.SetSelection(_separationTime);
                    if (_separationTime != 0)
                    {
                        expertMode = true;
                    }
                }

                if ((_blockSize < 0) || (_blockSize >= _spinnerCanAdapterBlockSizeAdapter.Items.Count))
                {
                    _spinnerCanAdapterBlockSize.SetSelection(0);
                }
                else
                {
                    _spinnerCanAdapterBlockSize.SetSelection(_blockSize);
                    if (_blockSize != 0)
                    {
                        expertMode = true;
                    }
                }

                // moved down because of expert mode setting
                if (IsCustomAdapter(_activityCommon.SelectedInterface))
                {
                    if (_canMode == (int)AdapterMode.Can100)
                    {
                        expertMode = true;
                    }
                    _spinnerCanAdapterModeAdapter.Items.Clear();
                    if (_adapterType >= 0x0002 && _fwVersion >= 0x0008)
                    {
                        _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_auto), AdapterMode.CanAuto));
                    }
                    _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_500), AdapterMode.Can500));
                    if (expertMode)
                    {
                        _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_100), AdapterMode.Can100));
                    }
                    _spinnerCanAdapterModeAdapter.Items.Add(new StringObjType(GetString(Resource.String.button_can_adapter_can_off), AdapterMode.CanOff));
                    _spinnerCanAdapterModeAdapter.NotifyDataSetChanged();

                    int indexMode = 0;
                    for (int i = 0; i < _spinnerCanAdapterModeAdapter.Count; i++)
                    {
                        if ((int) _spinnerCanAdapterModeAdapter.Items[i].Data == _canMode)
                        {
                            indexMode = i;
                        }
                    }
                    _spinnerCanAdapterMode.SetSelection(indexMode);
                }

                if ((_editTextBtPin.Enabled || mtcService) && _btPin != null)
                {
                    string btPin = PinDataToString(_btPin);
                    _editTextBtPin.Text = btPin.Length >= 4 ? btPin : "1234";
                }

                if ((_editTextBtName.Enabled || mtcService) && _btName != null)
                {
                    try
                    {
                        int length = _btName.TakeWhile(value => value != 0x00).Count();
                        _editTextBtName.Text = Encoding.UTF8.GetString(_btName, 0, length);
                    }
                    catch (Exception)
                    {
                        _editTextBtName.Text = string.Empty;
                    }
                }

                string ignitionText = string.Empty;
                if (_ignitionState >= 0 || _clampStatus >= 0)
                {
                    if (_clampStatus >= 0)
                    {
                        if ((_clampStatus & 0x300) != 0x300)
                        {
                            // CAN enabled and status present
                            ignitionText = GetString(Resource.String.can_adapter_ignition_no_status);
                        }
                        else
                        {
                            ignitionText = ((_clampStatus & 0x00C) == 0x004) ?
                                GetString(Resource.String.can_adapter_ignition_on) : GetString(Resource.String.can_adapter_ignition_off);
                        }
                    }
                    else
                    {
                        ignitionText = (_ignitionState & 0x01) != 0x00 ? GetString(Resource.String.can_adapter_ignition_on) : GetString(Resource.String.can_adapter_ignition_off);
                        if ((_ignitionState & 0x80) != 0)
                        {
                            ignitionText = "(" + ignitionText + ")";
                        }
                    }
                }
                _textViewIgnitionState.Text = ignitionText;

                string voltageText = string.Empty;
                if (_adapterType > 1 && _batteryVoltage >= 0)
                {
                    voltageText = string.Format(ActivityMain.Culture, "{0,4:0.0}V", (double)_batteryVoltage / 10);
                }
                _textViewBatteryVoltage.Text = voltageText;

                string versionText = string.Empty;
                if (_adapterType >= 0 && _fwVersion >= 0)
                {
                    versionText = string.Format(ActivityMain.Culture, "{0}.{1} / ", (_fwVersion >> 8) & 0xFF, _fwVersion & 0xFF);
                    int fwUpdateVersion = PicBootloader.GetFirmwareVersion((uint)_adapterType);
                    if (fwUpdateVersion >= 0)
                    {
                        if (!_instanceData.FwUpdateShown && _fwVersion < fwUpdateVersion)
                        {
                            requestFwUpdate = true;
                        }
                        versionText += string.Format(ActivityMain.Culture, "{0}.{1}", (fwUpdateVersion >> 8) & 0xFF, fwUpdateVersion & 0xFF);
                    }
                    else
                    {
                        versionText += "--";
                    }
                    fwUpdateEnabled = fwUpdateVersion >= 0 && ((_fwVersion != fwUpdateVersion) || ActivityCommon.CollectDebugInfo);
                }
                _textViewFwVersion.Text = versionText;

                if (_textViewSerNum.Enabled)
                {
                    _textViewSerNum.Text = (_serNum == null) ? string.Empty : BitConverter.ToString(_serNum).Replace("-", "");
                }
            }
            _buttonFwUpdate.Enabled = fwUpdateEnabled;
            _spinnerCanAdapterMode.Enabled = bEnabled;
            _spinnerCanAdapterSepTime.Enabled = bEnabled && expertMode;
            _spinnerCanAdapterBlockSize.Enabled = bEnabled && expertMode;
            _checkBoxExpert.Enabled = bEnabled;
            _checkBoxExpert.Checked = expertMode;
            if (requestFwUpdate)
            {
                _instanceData.FwUpdateShown = true;
                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        PerformUpdateMessage();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.can_adapter_fw_update_present)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }

            HideKeyboard();
        }

        private void PerformRead()
        {
            if (!IsCustomAdapter(_activityCommon.SelectedInterface))
            {
                UpdateDisplay();
                return;
            }
            EdiabasInit();
            _adapterThread = new Thread(() =>
            {
                bool commFailed;
                try
                {
                    commFailed = !InterfacePrepare();
                    // block size
                    if (!commFailed)
                    {
                        _blockSize = AdapterCommand(0x80);
                        if (_blockSize < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // separation time
                    if (!commFailed)
                    {
                        _separationTime = AdapterCommand(0x81);
                        if (_separationTime < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // CAN mode
                    if (!commFailed)
                    {
                        _canMode = AdapterCommand(0x82);
                    }
                    // ignition state
                    if (!commFailed)
                    {
                        _ignitionState = AdapterCommand(0xFE, 0xFE);
                        if (_ignitionState < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // battery voltage
                    if (!commFailed)
                    {
                        _batteryVoltage = AdapterCommand(0xFC, 0xFC);
                        if (_batteryVoltage < 0)
                        {
                            commFailed = true;
                        }
                    }
                    // firmware version
                    if (!commFailed)
                    {
                        byte[] result = AdapterCommandCustom(0xFD, new byte[] {0xFD});
                        if ((result == null) || (result.Length < 4))
                        {
                            commFailed = true;
                        }
                        else
                        {
                            _adapterType = result[1] + (result[0] << 8);
                            _fwVersion = result[3] + (result[2] << 8);
                        }
                    }
                    // id
                    if (!commFailed)
                    {
                        byte[] result = AdapterCommandCustom(0xFB, new byte[] { 0xFB });
                        if (result == null)
                        {
                            commFailed = true;
                        }
                        else
                        {
                            _serNum = result;
                        }
                    }
                    // bluetooth pin
                    _btPin = null;
                    if (!commFailed && _adapterType >= 0x0003 && _fwVersion >= 0x0002)
                    {
                        byte[] result = AdapterCommandCustom(0x84, new byte[] { 0x84 });
                        if (result == null)
                        {
                            commFailed = true;
                        }
                        else
                        {
                            _btPin = result;
                        }
                    }
                    // bluetooth name
                    _btName = null;
                    if (!commFailed && _adapterType >= 0x0003 && _fwVersion >= 0x0005)
                    {
                        byte[] result = AdapterCommandCustom(0x85, new byte[] { 0x85 });
                        if (result == null)
                        {
                            commFailed = true;
                        }
                        else
                        {
                            _btName = result;
                        }
                    }
                    // clamp status
                    _clampStatus = -1;
                    if (!commFailed && _adapterType >= 0x0002 && _fwVersion >= 0x000A)
                    {
                        byte[] result = AdapterCommandCustom(0xFA, new byte[] { 0xFA });
                        if ((result == null) || (result.Length < 2))
                        {
                            commFailed = true;
                        }
                        else
                        {
                            _clampStatus = result[1] + (result[0] << 8);
                        }
                    }
                }
                catch (Exception)
                {
                    commFailed = true;
                }
                if (commFailed)
                {
                    _blockSize = -1;
                    _separationTime = -1;
                    _canMode = -1;
                    _ignitionState = -1;
                    _batteryVoltage = -1;
                    _adapterType = -1;
                    _fwVersion = -1;
                    _serNum = null;
                    _btPin = null;
                    _btName = null;
                    _clampStatus = -1;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (IsJobRunning())
                    {
                        _adapterThread.Join();
                    }
                    UpdateDisplay();
                    if (commFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_comm_error), Resource.String.alert_title_error);
                        EdiabasClose();
                    }
                });
            });
            _adapterThread.Start();
            UpdateDisplay();
        }

        private void PerformWrite()
        {
            EdiabasInit();

            int blockSize = _spinnerCanAdapterBlockSize.SelectedItemPosition;
            if (blockSize < 0) blockSize = 0;

            int separationTime = _spinnerCanAdapterSepTime.SelectedItemPosition;
            if (separationTime < 0) separationTime = 0;

            int canMode = 0x01;
            if (_spinnerCanAdapterMode.SelectedItemPosition >= 0)
            {
                canMode = (int)_spinnerCanAdapterModeAdapter.Items[_spinnerCanAdapterMode.SelectedItemPosition].Data;
            }

            byte[] btPinData = null;
            if (_editTextBtPin.Enabled && _btPin != null && _btPin.Length >= 4)
            {
                bool pinChanged = false;
                try
                {
                    btPinData = Encoding.UTF8.GetBytes(_editTextBtPin.Text);
                }
                catch (Exception)
                {
                    // ignored
                }
                if ((btPinData != null) && (btPinData.Length >= 4) && (btPinData.Length <= _btPin.Length))
                {
                    int length = _btPin.TakeWhile(value => value != 0x00).Count();
                    if (length != btPinData.Length)
                    {
                        pinChanged = true;
                    }
                    else
                    {
                        for (int i = 0; i < btPinData.Length; i++)
                        {
                            if (_btPin[i] != btPinData[i])
                            {
                                pinChanged = true;
                            }
                        }
                    }
                    if (!pinChanged)
                    {
                        btPinData = null;
                    }
                }
                else
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_pin_length), Resource.String.alert_title_error);
                    return;
                }
            }

            byte[] btNameData = null;
            if (_editTextBtName.Enabled && _btName != null && _btName.Length > 0)
            {
                try
                {
                    btNameData = Encoding.UTF8.GetBytes(_editTextBtName.Text);
                }
                catch (Exception)
                {
                    // ignored
                }
                bool nameChanged = false;
                if ((btNameData != null) && (btNameData.Length >= 1) && (btNameData.Length <= _btName.Length))
                {
                    int length = _btName.TakeWhile(value => value != 0x00).Count();
                    if (length != btNameData.Length)
                    {
                        nameChanged = true;
                    }
                    else
                    {
                        for (int i = 0; i < btNameData.Length; i++)
                        {
                            if (_btName[i] != btNameData[i])
                            {
                                nameChanged = true;
                            }
                        }
                    }
                    if (!nameChanged)
                    {
                        btNameData = null;
                    }
                }
                else
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_name_length), Resource.String.alert_title_error);
                    return;
                }
            }

            _adapterThread = new Thread(() =>
            {
                bool commFailed;
                byte[] btPinResponse = null;
                byte[] btNameResponse = null;
                try
                {
                    commFailed = !InterfacePrepare();
                    if (IsCustomAdapter(_activityCommon.SelectedInterface))
                    {
                        // block size
                        if (!commFailed)
                        {
                            if (AdapterCommand(0x00, (byte)blockSize) < 0)
                            {
                                commFailed = true;
                            }
                        }
                        // separation time
                        if (!commFailed)
                        {
                            if (AdapterCommand(0x01, (byte)separationTime) < 0)
                            {
                                commFailed = true;
                            }
                        }
                        // CAN mode
                        if (!commFailed)
                        {
                            if (AdapterCommand(0x02, (byte)canMode) < 0)
                            {
                                commFailed = true;
                            }
                        }
                        // Bluetooth pin
                        if (!commFailed && btPinData != null)
                        {
                            btPinResponse = AdapterCommandCustom(0x04, btPinData);
                            if ((btPinResponse == null) || (btPinResponse.Length < 4))
                            {
                                commFailed = true;
                            }
                        }
                        // Bluetooth name
                        if (!commFailed && btNameData != null)
                        {
                            btNameResponse = AdapterCommandCustom(0x05, btNameData);
                            if ((btNameResponse == null) || (btNameResponse.Length < 1))
                            {
                                commFailed = true;
                            }
                        }
                    }
                    else
                    {
                        // CAN mode
                        if (!commFailed)
                        {
                            if (!AdapterCommandStd((byte)canMode))
                            {
                                commFailed = true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    commFailed = true;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (IsJobRunning())
                    {
                        _adapterThread.Join();
                    }
                    if (commFailed)
                    {
                        _activityCommon.ShowAlert(IsCustomAdapter(_activityCommon.SelectedInterface)
                            ? GetString(Resource.String.can_adapter_comm_error)
                            : GetString(Resource.String.can_adapter_comm_error_std), Resource.String.alert_title_error);
                        EdiabasClose();
                    }
                    else
                    {
                        if (btPinResponse != null)
                        {
                            string message = string.Format(GetString(Resource.String.can_adapter_new_pin), PinDataToString(btPinResponse));
                            _activityCommon.ShowAlert(message, Resource.String.alert_title_info);
                        }
                        else
                        {
                            if (btNameResponse != null)
                            {
                                _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_new_name), Resource.String.alert_title_info);
                            }
                        }
                        PerformRead();
                    }
                });
            });
            _adapterThread.Start();
            UpdateDisplay();
        }

        private void PerformUpdateMessage()
        {
            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth)
            {
                if (!ActivityCommon.IsBtReliable() || _activityCommon.MtcBtService)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.can_adapter_bt_not_reliable), Resource.String.alert_title_error);
                    return;
                }
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    PerformUpdate();
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.can_adapter_fw_update_info)
                .SetTitle(Resource.String.alert_title_warning)
                .Show();
        }

        private void PerformUpdate()
        {
            EdiabasInit();
            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.can_adapter_fw_update_active));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            _adapterThread = new Thread(() =>
            {
                bool updateOk = false;
                bool connectOk = false;
                try
                {
                    connectOk = InterfacePrepare();
                    Stream inStream = null;
                    Stream outStream = null;
                    if (connectOk)
                    {
                        switch (_activityCommon.SelectedInterface)
                        {
                            case ActivityCommon.InterfaceType.Bluetooth:
                            {
                                BluetoothSocket bluetoothSocket = EdBluetoothInterface.BluetoothSocket;
                                if (bluetoothSocket == null)
                                {
                                    connectOk = false;
                                    break;
                                }
                                inStream = bluetoothSocket.InputStream;
                                outStream = bluetoothSocket.OutputStream;
                                break;
                            }

                            case ActivityCommon.InterfaceType.DeepObdWifi:
                            {
                                NetworkStream networkStream = EdCustomWiFiInterface.NetworkStream;
                                if (networkStream == null)
                                {
                                    connectOk = false;
                                    break;
                                }
                                inStream = networkStream;
                                outStream = networkStream;
                                break;
                            }
                        }
                    }
                    if (inStream == null || outStream == null)
                    {
                        connectOk = false;
                    }

                    if (connectOk)
                    {
                        updateOk = PicBootloader.FwUpdate(inStream, outStream);
                    }
                }
                catch (Exception)
                {
                    updateOk = false;
                }
                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (IsJobRunning())
                    {
                        _adapterThread.Join();
                    }
                    progress.Dismiss();
                    progress.Dispose();
                    string message;
                    if (updateOk)
                    {
                        message = GetString(Resource.String.can_adapter_fw_update_ok);
                    }
                    else
                    {
                        message = connectOk
                            ? GetString(Resource.String.can_adapter_fw_update_failed)
                            : GetString(Resource.String.can_adapter_fw_update_conn_failed);
                    }
                    _activityCommon.ShowAlert(message, updateOk ? Resource.String.alert_title_info : Resource.String.alert_title_error);
                    UpdateDisplay();
                    if (updateOk)
                    {
                        PerformRead();
                    }
                });
            });
            _adapterThread.Start();
            UpdateDisplay();
        }

        private static bool IsCustomAdapter(ActivityCommon.InterfaceType interfaceType)
        {
            switch (interfaceType)
            {
                case ActivityCommon.InterfaceType.Bluetooth:
                case ActivityCommon.InterfaceType.DeepObdWifi:
                    return true;
            }

            return false;
        }

        private string PinDataToString(byte[] pinData)
        {
            string btPin = string.Empty;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (byte value in pinData)
            {
                char digit = (char)value;
                if (digit >= '0' && digit <= '9')
                {
                    btPin += digit;
                }
            }
            return btPin;
        }

        private int AdapterCommand(byte command, byte data = 0x00)
        {
            if (!_ediabas.EdInterfaceClass.TransmitData(new byte[] { 0x82, 0xF1, 0xF1, command, data }, out byte[] response))
            {
                return -1;
            }
            if ((response.Length != 6) || (response[3] != command))
            {
                return -1;
            }
            return response[4];
        }

        private byte[] AdapterCommandCustom(byte command, byte[] data)
        {
            byte[] request = new byte[4 + data.Length];
            request[0] = (byte) (0x81 + data.Length);
            request[1] = 0xF1;
            request[2] = 0xF1;
            request[3] = command;
            Array.Copy(data, 0, request, 4, data.Length);

            if (!_ediabas.EdInterfaceClass.TransmitData(request, out byte[] response))
            {
                return null;
            }
            if ((response.Length < 5) || (response[3] != command))
            {
                return null;
            }
            byte[] result = new byte[response.Length - 5];
            Array.Copy(response, 4, result, 0, result.Length);
            return result;
        }

        private bool AdapterCommandStd(byte command)
        {
            if (!_ediabas.EdInterfaceClass.TransmitData(new byte[] { 0x81, 0x00, 0x00, command }, out byte[] response))
            {
                return false;
            }
            if ((response.Length != 5) || (response[3] != (byte)(~command)))
            {
                return false;
            }
            return true;
        }
    }
}
