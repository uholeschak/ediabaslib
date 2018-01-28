using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BmwDeepObd.FilePicker;
using EdiabasLib;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
// ReSharper disable LoopCanBeConvertedToQuery

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/tool_title",
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                        Android.Content.PM.ConfigChanges.Orientation |
                        Android.Content.PM.ConfigChanges.ScreenSize)]
    public class EdiabasToolActivity : AppCompatActivity, View.IOnTouchListener
    {
        private enum ActivityRequest
        {
            RequestSelectSgbd,
            RequestSelectDevice,
            RequestCanAdapterConfig,
            RequestYandexKey,
        }

        private class ExtraInfo
        {
            public ExtraInfo(string name, string type, List<string> commentList)
            {
                Name = name;
                Type = type;
                CommentList = commentList;
                Selected = false;
            }

            public string Name { get; }

            public string Type { get; }

            public List<string> CommentList { get; }

            public List<string> CommentListTrans { get; set; }

            public bool Selected { get; set; }
        }

        private class JobInfo
        {
            public JobInfo(string name, string objectName)
            {
                Name = name;
                ObjectName = objectName;
                Comments = new List<string>();
                Arguments = new List<ExtraInfo>();
                Results = new List<ExtraInfo>();
            }

            public string Name { get; }

            public string ObjectName { get; }

            public List<string> Comments { get; }

            public List<string> CommentsTrans { get; set; }

            public List<ExtraInfo> Arguments { get; }

            public List<ExtraInfo> Results { get; }
        }

        public class InstanceData
        {
            public InstanceData()
            {
                SgbdFileName = string.Empty;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                TraceActive = true;
            }

            public bool ForceAppend { get; set; }
            public bool AutoStart { get; set; }
            public int AutoStartItemId { get; set; }
            public string SgbdFileName { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string DataLogDir { get; set; }
            public string TraceDir { get; set; }
            public bool Offline { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool DataLogActive { get; set; }
            public bool CommErrorsOccured { get; set; }
        }

        // Intent extra
        public const string ExtraInitDir = "init_dir";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraSgbdFile = "sgbd";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");

        public static ActivityCommon IntentTranslateActivty { get; set; }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _updateOptionsMenu;
        private InputMethodManager _imm;
        private View _contentView;
        private View _barView;
        private CheckBox _checkBoxContinuous;
        private ToggleButton _buttonConnect;
        private Spinner _spinnerJobs;
        private JobListAdapter _jobListAdapter;
        private CheckBox _checkBoxBinArgs;
        private EditText _editTextArgs;
        private Spinner _spinnerResults;
        private ResultSelectListAdapter _resultSelectListAdapter;
        private int _resultSelectLastItem;
        private ListView _listViewInfo;
        private ResultListAdapter _infoListAdapter;
        private string _initDirStart;
        private string _appDataDir;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private StreamWriter _swDataLog;
        private Thread _jobThread;
        private volatile bool _runContinuous;
        private volatile bool _ediabasJobAbort;
        private string _sgbdFileNameInitial = string.Empty;
        private bool _activityActive;
        private bool _translateEnabled;
        private bool _translateActive;
        private bool _jobListTranslated;
        private readonly List<JobInfo> _jobList = new List<JobInfo>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.ediabas_tool);

            _imm = (InputMethodManager)GetSystemService(InputMethodService);
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            _barView = LayoutInflater.Inflate(Resource.Layout.bar_ediabas_tool, null);
            ActionBar.LayoutParams barLayoutParams = new ActionBar.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            barLayoutParams.Gravity = barLayoutParams.Gravity &
                (int)(~(GravityFlags.HorizontalGravityMask | GravityFlags.VerticalGravityMask)) |
                (int)(GravityFlags.Left | GravityFlags.CenterVertical);
            SupportActionBar.SetCustomView(_barView, barLayoutParams);

            SetResult(Android.App.Result.Canceled);

            _checkBoxContinuous = _barView.FindViewById<CheckBox>(Resource.Id.checkBoxContinuous);
            _checkBoxContinuous.SetOnTouchListener(this);

            _buttonConnect = _barView.FindViewById<ToggleButton>(Resource.Id.buttonConnect);
            _buttonConnect.SetOnTouchListener(this);
            _buttonConnect.Click += (sender, args) =>
            {
                if (_buttonConnect.Checked)
                {
                    ExecuteSelectedJob(_checkBoxContinuous.Checked);
                }
                else
                {
                    _runContinuous = false;
                    UpdateDisplay();
                }
            };

            _spinnerJobs = FindViewById<Spinner>(Resource.Id.spinnerJobs);
            _jobListAdapter = new JobListAdapter(this);
            _spinnerJobs.Adapter = _jobListAdapter;
            _spinnerJobs.SetOnTouchListener(this);
            _spinnerJobs.ItemSelected += (sender, args) =>
                {
                    NewJobSelected();
                    DisplayJobComments();
                };

            _editTextArgs = FindViewById<EditText>(Resource.Id.editTextArgs);
            _editTextArgs.SetOnTouchListener(this);

            _checkBoxBinArgs = FindViewById<CheckBox>(Resource.Id.checkBoxBinArgs);
            _checkBoxBinArgs.SetOnTouchListener(this);

            _spinnerResults = FindViewById<Spinner>(Resource.Id.spinnerResults);
            _resultSelectListAdapter = new ResultSelectListAdapter(this);
            _spinnerResults.Adapter = _resultSelectListAdapter;
            _spinnerResults.SetOnTouchListener(this);
            _spinnerResults.ItemSelected += (sender, args) =>
                {
                    if (_resultSelectLastItem != _spinnerResults.SelectedItemPosition)
                    {
                        DisplayJobResult();
                    }
                    _resultSelectLastItem = -1;
                };
            _resultSelectLastItem = -1;

            _listViewInfo = FindViewById<ListView>(Resource.Id.infoList);
            _infoListAdapter = new ResultListAdapter(this);
            _listViewInfo.Adapter = _infoListAdapter;
            _listViewInfo.SetOnTouchListener(this);

            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                    UpdateDisplay();
                }
            }, BroadcastReceived, IntentTranslateActivty)
            {
                SelectedInterface = (ActivityCommon.InterfaceType)
                    Intent.GetIntExtra(ExtraInterface, (int) ActivityCommon.InterfaceType.None)
            };

            _initDirStart = Intent.GetStringExtra(ExtraInitDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _sgbdFileNameInitial = Intent.GetStringExtra(ExtraSgbdFile);
            if (!_activityRecreated)
            {
                _instanceData.DeviceName = Intent.GetStringExtra(ExtraDeviceName);
                _instanceData.DeviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            }
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);

            EdiabasClose(_instanceData.ForceAppend);
            UpdateDisplay();

            if (!_activityRecreated && !string.IsNullOrEmpty(_sgbdFileNameInitial))
            {
                _instanceData.SgbdFileName = _sgbdFileNameInitial;
            }
            ReadSgbd();
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
            if (!_activityCommon.RequestEnableTranslate((sender, args) =>
            {
                HandleStartDialogs();
            }))
            {
                HandleStartDialogs();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            _instanceData.ForceAppend = true;   // OnSaveInstanceState is called before OnStop
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

            _runContinuous = false;
            _ediabasJobAbort = true;
            if (IsJobRunning())
            {
                _jobThread.Join();
            }
            EdiabasClose(true);
            _activityCommon.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            if (!IsJobRunning())
            {
                if (!SendTraceFile((sender, args) =>
                {
                    base.OnBackPressed();
                }))
                {
                    base.OnBackPressed();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestSelectSgbd:
                    // When FilePickerActivity returns with a file
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        _instanceData.SgbdFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        UpdateOptionsMenu();
                        ReadSgbd();
                    }
                    break;

                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _instanceData.DeviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _instanceData.DeviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        bool callAdapterConfig = data.Extras.GetBoolean(DeviceListActivity.ExtraCallAdapterConfig, false);
                        EdiabasClose();
                        UpdateOptionsMenu();
                        if (callAdapterConfig)
                        {
                            AdapterConfig();
                        }
                        else if (_instanceData.AutoStart)
                        {
                            SelectSgbdFile(_instanceData.AutoStartItemId == Resource.Id.menu_tool_sel_sgbd_grp);
                        }
                    }
                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestCanAdapterConfig:
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey);
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.ediabas_tool_menu, menu);
            return true;
        }

        public override bool OnMenuOpened(int featureId, IMenu menu)
        {
            if (_updateOptionsMenu)
            {
                _updateOptionsMenu = false;
                OnPrepareOptionsMenu(menu);
            }
            return base.OnMenuOpened(featureId, menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            bool commActive = IsJobRunning();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable();
            bool fixedSgbd = !string.IsNullOrEmpty(_sgbdFileNameInitial);

            IMenuItem offlineMenu = menu.FindItem(Resource.Id.menu_tool_offline);
            if (offlineMenu != null)
            {
                offlineMenu.SetEnabled(!commActive && !fixedSgbd);
                offlineMenu.SetChecked(_instanceData.Offline);
            }

            IMenuItem selInterfaceMenu = menu.FindItem(Resource.Id.menu_tool_sel_interface);
            if (selInterfaceMenu != null)
            {
                selInterfaceMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_interface), _activityCommon.InterfaceName()));
                selInterfaceMenu.SetEnabled(!commActive && !_instanceData.Offline && !fixedSgbd);
            }

            IMenuItem selSgbdGrpMenu = menu.FindItem(Resource.Id.menu_tool_sel_sgbd_grp);
            if (selSgbdGrpMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.SgbdFileName))
                {
                    bool groupFile = string.Compare(Path.GetExtension(_instanceData.SgbdFileName), ".grp", StringComparison.OrdinalIgnoreCase) == 0;
                    if (groupFile)
                    {
                        fileName = Path.GetFileNameWithoutExtension(_instanceData.SgbdFileName);
                    }
                }
                selSgbdGrpMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_sgbd_grp), fileName));
                selSgbdGrpMenu.SetEnabled(!commActive && interfaceAvailable && !_instanceData.Offline && !fixedSgbd);
            }

            IMenuItem selSgbdPrgMenu = menu.FindItem(Resource.Id.menu_tool_sel_sgbd_prg);
            if (selSgbdPrgMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.SgbdFileName))
                {
                    if (!string.IsNullOrEmpty(_ediabas?.SgbdFileName))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        fileName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName);
                    }
                }
                selSgbdPrgMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_tool_sel_sgbd_prg), fileName));
                selSgbdPrgMenu.SetEnabled(!commActive && interfaceAvailable && !fixedSgbd);
            }

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_adapter), _instanceData.DeviceName));
                scanMenu.SetEnabled(!commActive && interfaceAvailable && !_instanceData.Offline && !fixedSgbd);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem adapterConfigMenu = menu.FindItem(Resource.Id.menu_adapter_config);
            if (adapterConfigMenu != null)
            {
                adapterConfigMenu.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline && !fixedSgbd);
                adapterConfigMenu.SetVisible(_activityCommon.AllowAdapterConfig(_instanceData.DeviceAddress));
            }

            IMenuItem enetIpMenu = menu.FindItem(Resource.Id.menu_enet_ip);
            if (enetIpMenu != null)
            {
                enetIpMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_enet_ip),
                    string.IsNullOrEmpty(_activityCommon.SelectedEnetIp) ? GetString(Resource.String.select_enet_ip_auto) : _activityCommon.SelectedEnetIp));
                enetIpMenu.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline && !fixedSgbd);
                enetIpMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet);
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            logSubMenu?.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline);

            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline && _instanceData.TraceActive && ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir));

            IMenuItem translationSubmenu = menu.FindItem(Resource.Id.menu_translation_submenu);
            if (translationSubmenu != null)
            {
                translationSubmenu.SetEnabled(true);
                translationSubmenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            IMenuItem translationEnableMenu = menu.FindItem(Resource.Id.menu_translation_enable);
            if (translationEnableMenu != null)
            {
                translationEnableMenu.SetEnabled(!commActive || !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey));
                translationEnableMenu.SetVisible(ActivityCommon.IsTranslationRequired());
                translationEnableMenu.SetChecked(ActivityCommon.EnableTranslation);
            }

            IMenuItem translationYandexKeyMenu = menu.FindItem(Resource.Id.menu_translation_yandex_key);
            if (translationYandexKeyMenu != null)
            {
                translationYandexKeyMenu.SetEnabled(!commActive);
                translationYandexKeyMenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            IMenuItem translationClearCacheMenu = menu.FindItem(Resource.Id.menu_translation_clear_cache);
            if (translationClearCacheMenu != null)
            {
                translationClearCacheMenu.SetEnabled(!_activityCommon.IsTranslationCacheEmpty());
                translationClearCacheMenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            return base.OnPrepareOptionsMenu(menu);
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
                    if (!SendTraceFile((sender, args) =>
                    {
                        Finish();
                    }))
                    {
                        Finish();
                    }
                    return true;

                case Resource.Id.menu_tool_offline:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    if (EdiabasClose())
                    {
                        _instanceData.Offline = !_instanceData.Offline;
                        UpdateOptionsMenu();
                        UpdateDisplay();
                    }
                    return true;

                case Resource.Id.menu_tool_sel_interface:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectInterface();
                    return true;

                case Resource.Id.menu_tool_sel_sgbd_grp:
                case Resource.Id.menu_tool_sel_sgbd_prg:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    _instanceData.AutoStart = false;
                    if (!_instanceData.Offline)
                    {
                        if (string.IsNullOrEmpty(_instanceData.DeviceAddress))
                        {
                            if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, _appDataDir, (sender, args) =>
                            {
                                _instanceData.AutoStart = true;
                                _instanceData.AutoStartItemId = item.ItemId;
                            }))
                            {
                                return true;
                            }
                        }
                        if (_activityCommon.ShowConnectWarning(retry =>
                        {
                            if (retry)
                            {
                                OnOptionsItemSelected(item);
                            }
                        }))
                        {
                            return true;
                        }
                    }
                    SelectSgbdFile(item.ItemId == Resource.Id.menu_tool_sel_sgbd_grp);
                    return true;

                case Resource.Id.menu_scan:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    _instanceData.AutoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice, _appDataDir);
                    return true;

                case Resource.Id.menu_adapter_config:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    AdapterConfig();
                    return true;

                case Resource.Id.menu_enet_ip:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    EnetIpConfig();
                    return true;

                case Resource.Id.menu_submenu_log:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SelectDataLogging();
                    return true;

                case Resource.Id.menu_send_trace:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    SendTraceFileAlways((sender, args) =>
                    {
                        UpdateOptionsMenu();
                    });
                    return true;

                case Resource.Id.menu_translation_enable:
                    if (!ActivityCommon.EnableTranslation && string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey))
                    {
                        EditYandexKey();
                        return true;
                    }
                    ActivityCommon.EnableTranslation = !ActivityCommon.EnableTranslation;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    UpdateTranslationText();
                    return true;

                case Resource.Id.menu_translation_yandex_key:
                    EditYandexKey();
                    return true;

                case Resource.Id.menu_translation_clear_cache:
                    _activityCommon.ClearTranslationCache();
                    ResetTranslations();
                    _jobListTranslated = false;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    UpdateTranslationText();
                    return true;

                case Resource.Id.menu_submenu_help:
                    _activityCommon.ShowWifiConnectedWarning(() =>
                    {
                        StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://github.com/uholeschak/ediabaslib/blob/master/docs/EdiabasTool.md")));
                    });
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (v == _spinnerJobs)
                    {
                        DisplayJobComments();
                    }
                    else if (v == _spinnerResults)
                    {
                        DisplayJobResult();
                    }
                    else if (v == _checkBoxBinArgs || v == _editTextArgs)
                    {
                        DisplayJobArguments();
                        break;
                    }
                    HideKeyboard();
                    break;
            }
            return false;
        }

        private void UpdateOptionsMenu()
        {
            _updateOptionsMenu = true;
        }

        private void HandleStartDialogs()
        {
            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.None)
            {
                SelectInterface();
            }
            SelectInterfaceEnable();
            UpdateOptionsMenu();
            UpdateDisplay();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool EdiabasClose(bool forceAppend = false)
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
            _instanceData.ForceAppend = forceAppend;
            _jobList.Clear();
            CloseDataLog();
            UpdateDisplay();
            UpdateOptionsMenu();
            return true;
        }

        private void CloseDataLog()
        {
            if (_swDataLog != null)
            {
                _swDataLog.Dispose();
                _swDataLog = null;
            }
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
            _runContinuous = false;
            return false;
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private bool SendTraceFile(EventHandler<EventArgs> handler)
        {
            if (_instanceData.CommErrorsOccured && _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.RequestSendTraceFile(_appDataDir, _instanceData.TraceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
            }
            return false;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.SendTraceFile(_appDataDir, _instanceData.TraceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
            }
            return false;
        }

        private void UpdateTranslationText()
        {
            _jobListAdapter.NotifyDataSetChanged();
            NewJobSelected();
            DisplayJobComments();
        }

        private void UpdateDisplay()
        {
            if (ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation && string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey))
            {
                EditYandexKey();
                return;
            }
            bool checkContinuousEnable = true;
            bool buttonConnectEnable = true;
            bool inputsEnabled = true;
            if ((_ediabas == null) || (_jobList.Count == 0))
            {
                _jobListAdapter.Items.Clear();
                _jobListAdapter.NotifyDataSetChanged();
                _resultSelectListAdapter.Items.Clear();
                _resultSelectListAdapter.NotifyDataSetChanged();
                _infoListAdapter.Items.Clear();
                _infoListAdapter.NotifyDataSetChanged();
                inputsEnabled = false;
                checkContinuousEnable = false;
                buttonConnectEnable = false;
            }
            else
            {
                if (!_activityCommon.IsInterfaceAvailable())
                {
                    buttonConnectEnable = false;
                }
                if (TranslateEcuText((sender, args) =>
                {
                    UpdateDisplay();
                    UpdateTranslationText();
                }))
                {
                    return;
                }
            }

            if (!ActivityCommon.EnableTranslation)
            {
                _jobListTranslated = false;
            }

            if (IsJobRunning() || _instanceData.Offline)
            {
                checkContinuousEnable = false;
                buttonConnectEnable = _runContinuous;
                if (!_instanceData.Offline)
                {
                    inputsEnabled = false;
                }
            }
            _checkBoxContinuous.Enabled = checkContinuousEnable;
            _buttonConnect.Enabled = buttonConnectEnable;
            if (!buttonConnectEnable)
            {
                _buttonConnect.Checked = false;
            }
            _spinnerJobs.Enabled = inputsEnabled;
            _editTextArgs.Enabled = inputsEnabled;
            _checkBoxBinArgs.Enabled = inputsEnabled;
            _spinnerResults.Enabled = inputsEnabled;

            HideKeyboard();
        }

        private void SelectSgbdFile(bool groupFile)
        {
            // Launch the FilePickerActivity to select a sgbd file
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = _initDirStart;
            try
            {
                if (!string.IsNullOrEmpty(_instanceData.SgbdFileName))
                {
                    initDir = Path.GetDirectoryName(_instanceData.SgbdFileName);
                }
            }
            catch (Exception)
            {
                initDir = _initDirStart;
            }
            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.tool_select_sgbd));
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, groupFile ? ".grp" : ".prg");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectSgbd);
        }

        private void SelectInterface()
        {
            if (IsJobRunning())
            {
                return;
            }
            _activityCommon.SelectInterface((sender, args) =>
            {
                EdiabasClose();
                _instanceData.SgbdFileName = string.Empty;
                UpdateOptionsMenu();
                SelectInterfaceEnable();
            });
        }

        private void SelectInterfaceEnable()
        {
            _activityCommon.RequestInterfaceEnable((sender, args) =>
            {
                UpdateOptionsMenu();
            });
        }

        private void SelectDataLogging()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.menu_submenu_log);
            ListView listView = new ListView(this);

            List<string> logNames = new List<string>
            {
                GetString(Resource.String.datalog_enable_trace),
                GetString(Resource.String.datalog_append_trace),
                GetString(Resource.String.datalog_enable_datalog),
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemMultipleChoice, logNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Multiple;
            listView.SetItemChecked(0, _instanceData.TraceActive);
            listView.SetItemChecked(1, _instanceData.TraceAppend);
            listView.SetItemChecked(2, _instanceData.DataLogActive);

            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                SparseBooleanArray sparseArray = listView.CheckedItemPositions;
                for (int i = 0; i < sparseArray.Size(); i++)
                {
                    bool value = sparseArray.ValueAt(i);
                    switch (sparseArray.KeyAt(i))
                    {
                        case 0:
                            _instanceData.TraceActive = value;
                            break;

                        case 1:
                            _instanceData.TraceAppend = value;
                            break;

                        case 2:
                            _instanceData.DataLogActive = value;
                            break;
                    }
                }
                if (!_instanceData.DataLogActive)
                {
                    CloseDataLog();
                }
                UpdateLogInfo();
                UpdateOptionsMenu();
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
            {
            });
            builder.Show();
        }

        private void AdapterConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }
            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet)
            {
                _activityCommon.EnetAdapterConfig();
                return;
            }
            Intent serverIntent = new Intent(this, typeof(CanAdapterActivity));
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestCanAdapterConfig);
        }

        private void EditYandexKey()
        {
            Intent serverIntent = new Intent(this, typeof(YandexKeyActivity));
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestYandexKey);
        }

        private void EnetIpConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }
            _activityCommon.SelectEnetIp((sender, args) =>
            {
                UpdateOptionsMenu();
            });
        }

        private JobInfo GetSelectedJob()
        {
            int pos = _spinnerJobs.SelectedItemPosition;
            if (pos < 0)
            {
                return null;
            }
            return _jobListAdapter.Items[pos];
        }

        private void NewJobSelected()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _resultSelectListAdapter.Items.Clear();
            string defaultArgs = string.Empty;
            if (jobInfo != null)
            {
                foreach (ExtraInfo result in jobInfo.Results.OrderBy(x => x.Name))
                {
                    _resultSelectListAdapter.Items.Add(result);
                }
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    if (string.Compare(jobInfo.Name, XmlToolActivity.JobReadMwBlock, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        defaultArgs = jobInfo.ObjectName.Contains("1281") ? "0;WertEinmalLesen" : "100;LESEN";
                    }
                }
            }
            _resultSelectListAdapter.NotifyDataSetChanged();
            _resultSelectLastItem = 0;
            _editTextArgs.Text = defaultArgs;
            _checkBoxBinArgs.Checked = false;
        }

        private void DisplayJobComments()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_job), null));
                StringBuilder stringBuilderComments = new StringBuilder();
                stringBuilderComments.Append(jobInfo.Name);
                stringBuilderComments.Append(":");
                List<string> commentList = jobInfo.CommentsTrans ?? jobInfo.Comments;
                foreach (string comment in commentList)
                {
                    stringBuilderComments.Append("\r\n");
                    stringBuilderComments.Append(comment);
                }
                _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        private void DisplayJobArguments()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_arguments), null));
                if (string.Compare(jobInfo.Name, "FS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_arguments_error_detail), null));
                }
                foreach (ExtraInfo info in jobInfo.Arguments)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                    _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        private void DisplayJobResult()
        {
            if (_jobList.Count == 0)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            _infoListAdapter.Items.Clear();
            if (jobInfo != null)
            {
                _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_result), null));
                if (_spinnerResults.SelectedItemPosition >= 0)
                {
                    ExtraInfo info = _resultSelectListAdapter.Items[_spinnerResults.SelectedItemPosition];
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        stringBuilderComments.Append("\r\n");
                        stringBuilderComments.Append(comment);
                    }
                    _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool TranslateEcuText(EventHandler<EventArgs> handler = null)
        {
            if (_translateEnabled && !_translateActive && ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation)
            {
                if (!_jobListTranslated)
                {
                    _jobListTranslated = true;
                    List<string> stringList = new List<string>();
                    foreach (JobInfo jobInfo in _jobList)
                    {
                        if (jobInfo.Comments != null && jobInfo.CommentsTrans == null)
                        {
                            foreach (string comment in jobInfo.Comments)
                            {
                                if (!string.IsNullOrEmpty(comment))
                                {
                                    stringList.Add(comment);
                                }
                            }
                        }
                        if (jobInfo.Arguments != null)
                        {
                            foreach (ExtraInfo extraInfo in jobInfo.Arguments)
                            {
                                if (extraInfo.CommentList != null && extraInfo.CommentListTrans == null)
                                {
                                    foreach (string comment in extraInfo.CommentList)
                                    {
                                        if (!string.IsNullOrEmpty(comment))
                                        {
                                            stringList.Add(comment);
                                        }
                                    }
                                }
                            }
                        }
                        if (jobInfo.Results != null)
                        {
                            foreach (ExtraInfo extraInfo in jobInfo.Results)
                            {
                                if (extraInfo.CommentList != null && extraInfo.CommentListTrans == null)
                                {
                                    foreach (string comment in extraInfo.CommentList)
                                    {
                                        if (!string.IsNullOrEmpty(comment))
                                        {
                                            stringList.Add(comment);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (stringList.Count == 0)
                    {
                        return false;
                    }
                    _translateActive = true;
                    if (_activityCommon.TranslateStrings(stringList, transList =>
                    {
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            _translateActive = false;
                            try
                            {
                                if (transList != null && transList.Count == stringList.Count)
                                {
                                    int transIndex = 0;
                                    foreach (JobInfo jobInfo in _jobList)
                                    {
                                        if (jobInfo.Comments != null && jobInfo.CommentsTrans == null)
                                        {
                                            jobInfo.CommentsTrans = new List<string>();
                                            foreach (string comment in jobInfo.Comments)
                                            {
                                                if (!string.IsNullOrEmpty(comment))
                                                {
                                                    if (transIndex < transList.Count)
                                                    {
                                                        jobInfo.CommentsTrans.Add(transList[transIndex++]);
                                                    }
                                                }
                                            }
                                        }
                                        if (jobInfo.Arguments != null)
                                        {
                                            foreach (ExtraInfo extraInfo in jobInfo.Arguments)
                                            {
                                                if (extraInfo.CommentList != null && extraInfo.CommentListTrans == null)
                                                {
                                                    extraInfo.CommentListTrans = new List<string>();
                                                    foreach (string comment in extraInfo.CommentList)
                                                    {
                                                        if (!string.IsNullOrEmpty(comment))
                                                        {
                                                            if (transIndex < transList.Count)
                                                            {
                                                                extraInfo.CommentListTrans.Add(transList[transIndex++]);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        if (jobInfo.Results != null)
                                        {
                                            foreach (ExtraInfo extraInfo in jobInfo.Results)
                                            {
                                                if (extraInfo.CommentList != null && extraInfo.CommentListTrans == null)
                                                {
                                                    extraInfo.CommentListTrans = new List<string>();
                                                    foreach (string comment in extraInfo.CommentList)
                                                    {
                                                        if (!string.IsNullOrEmpty(comment))
                                                        {
                                                            if (transIndex < transList.Count)
                                                            {
                                                                extraInfo.CommentListTrans.Add(transList[transIndex++]);
                                                            }
                                                        }
                                                    }
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
                            handler?.Invoke(this, new EventArgs());
                        });
                    }))
                    {
                        return true;
                    }
                    _translateActive = false;
                }
            }
            else
            {
                ResetTranslations();
            }
            return false;
        }

        private void ResetTranslations()
        {
            try
            {
                foreach (JobInfo jobInfo in _jobList)
                {
                    jobInfo.CommentsTrans = null;
                    if (jobInfo.Arguments != null)
                    {
                        foreach (ExtraInfo extraInfo in jobInfo.Arguments)
                        {
                            extraInfo.CommentListTrans = null;
                        }
                    }
                    if (jobInfo.Results != null)
                    {
                        foreach (ExtraInfo extraInfo in jobInfo.Results)
                        {
                            extraInfo.CommentListTrans = null;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateLogInfo()
        {
            if (_ediabas == null)
            {
                return;
            }

            string logDir = Path.Combine(_appDataDir, "LogEdiabasTool");
            try
            {
                Directory.CreateDirectory(logDir);
            }
            catch (Exception)
            {
                logDir = string.Empty;
            }
            _instanceData.DataLogDir = logDir;

            _instanceData.TraceDir = null;
            if (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.SgbdFileName))
            {
                _instanceData.TraceDir = logDir;
            }

            if (!string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                _ediabas.SetConfigProperty("TracePath", _instanceData.TraceDir);
                _ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                _ediabas.SetConfigProperty("AppendTrace", _instanceData.TraceAppend | _instanceData.ForceAppend ? "1" : "0");
                _ediabas.SetConfigProperty("CompressTrace", "1");
            }
            else
            {
                _ediabas.SetConfigProperty("IfhTrace", "0");
            }
        }

        private void ReadSgbd()
        {
            if (string.IsNullOrEmpty(_instanceData.SgbdFileName))
            {
                return;
            }
            _translateEnabled = false;
            CloseDataLog();
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet
                {
                    EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                    AbortJobFunc = AbortEdiabasJob
                };
                _ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(_instanceData.SgbdFileName));
            }
            _jobList.Clear();
            UpdateDisplay();

            _activityCommon.SetEdiabasInterface(_ediabas, _instanceData.DeviceAddress);

            UpdateLogInfo();

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.tool_read_sgbd));
            progress.ButtonAbort.Visibility = ViewStates.Gone;
            progress.Show();

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                List<string> messageList = new List<string>();
                try
                {
                    _ediabas.ResolveSgbdFile(_instanceData.SgbdFileName);

                    _ediabas.ArgString = "ALL";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.NoInitForVJobs = true;
                    _ediabas.ExecuteJob("_JOBS");

                    List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        string objectName = string.Empty;
                        int dictIndex = 0;
                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                        {
                            EdiabasNet.ResultData resultData;
                            if (dictIndex == 0)
                            {
                                if (resultDict.TryGetValue("OBJECT", out resultData))
                                {
                                    objectName = resultData.OpData as string ?? string.Empty;
                                }
                                dictIndex++;
                                continue;
                            }
                            if (resultDict.TryGetValue("JOBNAME", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    _jobList.Add(new JobInfo((string)resultData.OpData, objectName));
                                }
                            }
                            dictIndex++;
                        }
                    }

                    foreach (JobInfo job in _jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_JOBCOMMENTS");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            for (int i = 0; ; i++)
                            {
                                if (resultDict.TryGetValue("JOBCOMMENT" + i.ToString(Culture), out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        job.Comments.Add((string)resultData.OpData);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    foreach (JobInfo job in _jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_ARGUMENTS");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            int dictIndex = 0;
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                            {
                                if (dictIndex == 0)
                                {
                                    dictIndex++;
                                    continue;
                                }
                                string arg = string.Empty;
                                string argType = string.Empty;
                                List<string> argCommentList = new List<string>();
                                if (resultDict.TryGetValue("ARG", out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        arg = (string)resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("ARGTYPE", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        argType = (string)resultData.OpData;
                                    }
                                }
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("ARGCOMMENT" + i.ToString(Culture), out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            argCommentList.Add((string)resultData.OpData);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                job.Arguments.Add(new ExtraInfo(arg, argType, argCommentList));
                                dictIndex++;
                            }
                        }
                    }

                    foreach (JobInfo job in _jobList)
                    {
                        _ediabas.ArgString = job.Name;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.NoInitForVJobs = true;
                        _ediabas.ExecuteJob("_RESULTS");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            int dictIndex = 0;
                            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                            {
                                if (dictIndex == 0)
                                {
                                    dictIndex++;
                                    continue;
                                }
                                string result = string.Empty;
                                string resultType = string.Empty;
                                List<string> resultCommentList = new List<string>();
                                if (resultDict.TryGetValue("RESULT", out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        result = (string)resultData.OpData;
                                    }
                                }
                                if (resultDict.TryGetValue("RESULTTYPE", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        resultType = (string)resultData.OpData;
                                    }
                                }
                                for (int i = 0; ; i++)
                                {
                                    if (resultDict.TryGetValue("RESULTCOMMENT" + i.ToString(Culture), out resultData))
                                    {
                                        if (resultData.OpData is string)
                                        {
                                            resultCommentList.Add((string)resultData.OpData);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                job.Results.Add(new ExtraInfo(result, resultType, resultCommentList));
                                dictIndex++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string exceptionText = String.Empty;
                    if (!AbortEdiabasJob())
                    {
                        exceptionText = EdiabasNet.GetExceptionText(ex);
                    }
                    messageList.Add(exceptionText);
                    if (ActivityCommon.IsCommunicationError(exceptionText))
                    {
                        _instanceData.CommErrorsOccured = true;
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();
                    progress.Dispose();

                    if (IsJobRunning())
                    {
                        _jobThread.Join();
                    }
                    _infoListAdapter.Items.Clear();
                    foreach (string message in messageList)
                    {
                        _infoListAdapter.Items.Add(new TableResultItem(message, null));
                    }
                    _infoListAdapter.NotifyDataSetChanged();

                    _jobListAdapter.Items.Clear();
                    foreach (JobInfo job in _jobList.OrderBy(x => x.Name))
                    {
                        _jobListAdapter.Items.Add(job);
                    }
                    _jobListAdapter.NotifyDataSetChanged();

                    _jobListTranslated = false;
                    _translateEnabled = true;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                });
            });
            _jobThread.Start();
        }

        private void ExecuteSelectedJob(bool continuous)
        {
            _infoListAdapter.Items.Clear();
            _infoListAdapter.NotifyDataSetChanged();
            if (_ediabas == null)
            {
                return;
            }
            if (_instanceData.Offline)
            {
                return;
            }
            JobInfo jobInfo = GetSelectedJob();
            if (jobInfo == null)
            {
                return;
            }
            if (IsJobRunning())
            {
                return;
            }

            string jobName = jobInfo.Name;
            string jobArgs = _editTextArgs.Text;
            bool jobBinArgs = _checkBoxBinArgs.Checked;
            StringBuilder stringBuilderResults = new StringBuilder();
            foreach (ExtraInfo info in jobInfo.Results)
            {
                if (info.Selected)
                {
                    if (stringBuilderResults.Length > 0)
                    {
                        stringBuilderResults.Append(";");
                    }
                    stringBuilderResults.Append(info.Name);
                }
            }
            string jobResults = stringBuilderResults.ToString();
            _runContinuous = continuous;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (_instanceData.DataLogActive)
            {
                _activityCommon.SetLock(ActivityCommon.LockTypeLogging);
            }
            else
            {
                _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);
            }

            _jobThread = new Thread(() =>
            {
                for (; ; )
                {
                    List<string> messageList = new List<string>();
                    try
                    {
                        if (string.Compare(jobName, "FS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0 &&
                            string.IsNullOrEmpty(jobArgs))
                        {
                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob("FS_LESEN");

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;

                            bool jobOk = false;
                            if (resultSets != null && resultSets.Count > 1)
                            {
                                if (resultSets[resultSets.Count - 1].TryGetValue("JOB_STATUS", out EdiabasNet.ResultData resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        // read details
                                        string jobStatus = (string) resultData.OpData;
                                        if (String.Compare(jobStatus, "OKAY", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            jobOk = true;
                                        }
                                    }
                                }
                            }
                            // read error details
                            if (jobOk)
                            {
                                int dictIndex = 0;
                                foreach (Dictionary<string, EdiabasNet.ResultData> resultDictLocal in resultSets)
                                {
                                    if (dictIndex == 0)
                                    {
                                        dictIndex++;
                                        continue;
                                    }

                                    if (resultDictLocal.TryGetValue("F_ORT_NR", out EdiabasNet.ResultData resultData))
                                    {
                                        if (resultData.OpData is Int64)
                                        {
                                            // read details
                                            _ediabas.ArgString = string.Format("0x{0:X02}", (Int64) resultData.OpData);
                                            _ediabas.ArgBinaryStd = null;
                                            _ediabas.ResultsRequests = jobResults;

                                            _ediabas.ExecuteJob("FS_LESEN_DETAIL");

                                            List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail =
                                                new List<Dictionary<string, EdiabasNet.ResultData>>(_ediabas.ResultSets);
                                            PrintResults(messageList, resultSetsDetail);
                                        }
                                    }
                                    dictIndex++;
                                }
                                if (messageList.Count == 0)
                                {
                                    messageList.Add(GetString(Resource.String.tool_no_errors));
                                }
                            }
                            else
                            {
                                messageList.Add(GetString(Resource.String.tool_read_errors_failure));
                            }
                        }
                        else
                        {
                            if (jobBinArgs)
                            {
                                jobArgs = jobArgs.Trim();
                                string[] argArray = jobArgs.Split(' ', ';', ',');
                                if (argArray.Length > 1)
                                {
                                    try
                                    {
                                        List<byte> binList = new List<byte>();
                                        foreach (string arg in argArray)
                                        {
                                            if (!string.IsNullOrEmpty(arg))
                                            {
                                                binList.Add(Convert.ToByte(arg, 16));
                                            }
                                        }
                                        _ediabas.ArgBinary = binList.ToArray();
                                    }
                                    catch (Exception)
                                    {
                                        _ediabas.ArgBinary = new byte[0];
                                    }
                                }
                                else
                                {
                                    _ediabas.ArgBinary = EdiabasNet.HexToByteArray(jobArgs);
                                }
                            }
                            else
                            {
                                _ediabas.ArgString = jobArgs;
                            }
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = jobResults;
                            _ediabas.ExecuteJob(jobName);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                            PrintResults(messageList, resultSets);
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionText = String.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exceptionText = EdiabasNet.GetExceptionText(ex);
                        }
                        messageList.Add(exceptionText);
                        if (ActivityCommon.IsCommunicationError(exceptionText))
                        {
                            _instanceData.CommErrorsOccured = true;
                        }
                        _runContinuous = false;
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        _infoListAdapter.Items.Clear();
                        foreach (string message in messageList)
                        {
                            _infoListAdapter.Items.Add(new TableResultItem(message, null));
                        }
                        _infoListAdapter.NotifyDataSetChanged();
                        UpdateDisplay();
                    });

                    if (!_runContinuous)
                    {
                        break;
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (IsJobRunning())
                    {
                        _jobThread.Join();
                    }
                    _activityCommon.SetLock(ActivityCommon.LockType.None);
                    UpdateOptionsMenu();
                    UpdateDisplay();
                });
            });
            _jobThread.Start();
            UpdateOptionsMenu();
            UpdateDisplay();
        }

        private void PrintResults(List<string> messageList, List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            int dataSet = 0;
            if (resultSets != null)
            {
                if (_ediabas != null && _instanceData.DataLogActive && _swDataLog == null &&
                    !string.IsNullOrEmpty(_instanceData.DataLogDir) && !string.IsNullOrEmpty(_ediabas.SgbdFileName))
                {
                    try
                    {
                        string fileName = Path.Combine(_instanceData.DataLogDir, Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName)) + ".log";
                        FileMode fileMode = File.Exists(fileName) ? FileMode.Append : FileMode.Create;
                        _swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                _swDataLog?.Write("----------------------------------------\r\n");
                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(string.Format(Culture, "DATASET: {0}", dataSet));
                    foreach (string key in resultDict.Keys.OrderBy(x => x))
                    {
                        EdiabasNet.ResultData resultData = resultDict[key];
                        string resultText = string.Empty;
                        if (resultData.OpData is string)
                        {
                            resultText = (string)resultData.OpData;
                        }
                        else if (resultData.OpData is double)
                        {
                            resultText = string.Format(Culture, "R: {0}", (Double)resultData.OpData);
                        }
                        else if (resultData.OpData is long)
                        {
                            Int64 value = (Int64)resultData.OpData;
                            switch (resultData.ResType)
                            {
                                case EdiabasNet.ResultType.TypeB:  // 8 bit
                                    resultText = string.Format(Culture, "B: {0} 0x{1:X02}", value, (Byte)value);
                                    break;

                                case EdiabasNet.ResultType.TypeC:  // 8 bit char
                                    resultText = string.Format(Culture, "C: {0} 0x{1:X02}", value, (Byte)value);
                                    break;

                                case EdiabasNet.ResultType.TypeW:  // 16 bit
                                    resultText = string.Format(Culture, "W: {0} 0x{1:X04}", value, (UInt16)value);
                                    break;

                                case EdiabasNet.ResultType.TypeI:  // 16 bit signed
                                    resultText = string.Format(Culture, "I: {0} 0x{1:X04}", value, (UInt16)value);
                                    break;

                                case EdiabasNet.ResultType.TypeD:  // 32 bit
                                    resultText = string.Format(Culture, "D: {0} 0x{1:X08}", value, (UInt32)value);
                                    break;

                                case EdiabasNet.ResultType.TypeL:  // 32 bit signed
                                    resultText = string.Format(Culture, "L: {0} 0x{1:X08}", value, (UInt32)value);
                                    break;

                                default:
                                    resultText = "?";
                                    break;
                            }
                        }
                        else if (resultData.OpData.GetType() == typeof(byte[]))
                        {
                            StringBuilder sb = new StringBuilder();
                            byte[] data = (byte[])resultData.OpData;
                            foreach (byte value in data)
                            {
                                sb.Append(string.Format(Culture, "{0:X02} ", value));
                            }
                            resultText = sb.ToString();
                        }
                        stringBuilder.Append("\r\n");
                        stringBuilder.Append(resultData.Name + ": " + resultText);
                    }
                    messageList.Add(stringBuilder.ToString());
                    if (_swDataLog != null)
                    {
                        _swDataLog.Write(stringBuilder.ToString());
                        _swDataLog.Write("\r\n");
                        _swDataLog.Write("\r\n");
                    }
                    dataSet++;
                }
            }
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
                            UpdateOptionsMenu();
                            UpdateDisplay();
                        }
                    }
                    break;
            }
        }

        private class JobListAdapter : BaseAdapter<JobInfo>
        {
            private readonly List<JobInfo> _items;
            public List<JobInfo> Items => _items;
            private readonly Android.App.Activity _context;
            private readonly Android.Graphics.Color _backgroundColor;

            public JobListAdapter(Android.App.Activity context)
            {
                _context = context;
                _items = new List<JobInfo>();

                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new[] { Android.Resource.Attribute.ColorBackground });
                _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override JobInfo this[int position] => _items[position];

            public override int Count => _items.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.job_list, null);
                view.SetBackgroundColor(_backgroundColor);
                TextView textName = view.FindViewById<TextView>(Resource.Id.textJobName);
                TextView textDesc = view.FindViewById<TextView>(Resource.Id.textJobDesc);
                textName.Text = item.Name;
                List<string> commentList = item.CommentsTrans ?? item.Comments;
                if (commentList.Count > 0)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    foreach (string comment in commentList)
                    {
                        stringBuilderComments.Append(comment + " ");
                    }
                    textDesc.Text = stringBuilderComments.ToString();
                }
                else
                {
                    textDesc.Text = " ";
                }

                return view;
            }
        }

        private class ResultSelectListAdapter : BaseAdapter<ExtraInfo>
        {
            private readonly List<ExtraInfo> _items;
            public List<ExtraInfo> Items => _items;
            private readonly Android.App.Activity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public ResultSelectListAdapter(Android.App.Activity context)
            {
                _context = context;
                _items = new List<ExtraInfo>();
                TypedArray typedArray = context.Theme.ObtainStyledAttributes(
                    new[] { Android.Resource.Attribute.ColorBackground });
                _backgroundColor = typedArray.GetColor(0, 0xFFFFFF);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ExtraInfo this[int position] => _items[position];

            public override int Count => _items.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = _items[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ediabas_result_list, null);
                view.SetBackgroundColor(_backgroundColor);
                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxResultSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                _ignoreCheckEvent = false;

                checkBoxSelect.Tag = new TagInfo(item);
                checkBoxSelect.CheckedChange -= OnCheckChanged;
                checkBoxSelect.CheckedChange += OnCheckChanged;

                TextView textResultName = view.FindViewById<TextView>(Resource.Id.textResultName);
                TextView textResultDesc = view.FindViewById<TextView>(Resource.Id.textResultDesc);
                textResultName.Text = item.Name;
                List<string> commentList = item.CommentListTrans ?? item.CommentList;
                if (commentList.Count > 0)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    foreach (string comment in commentList)
                    {
                        stringBuilderComments.Append(comment + " ");
                    }
                    textResultDesc.Text = stringBuilderComments.ToString();
                }
                else
                {
                    textResultDesc.Text = " ";
                }

                return view;
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = (CheckBox)sender;
                    TagInfo tagInfo = (TagInfo)checkBox.Tag;
                    if (tagInfo.Info.Selected != args.IsChecked)
                    {
                        tagInfo.Info.Selected = args.IsChecked;
                        NotifyDataSetChanged();
                    }
                }
            }

            private class TagInfo : Java.Lang.Object
            {
                public TagInfo(ExtraInfo info)
                {
                    Info = info;
                }

                public ExtraInfo Info { get; }
            }
        }
    }
}
