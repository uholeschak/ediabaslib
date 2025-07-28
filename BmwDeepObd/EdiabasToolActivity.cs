using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using BmwDeepObd.Dialogs;
using BmwDeepObd.FilePicker;
using BmwFileReader;
using EdiabasLib;

// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
// ReSharper disable LoopCanBeConvertedToQuery

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/tool_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(EdiabasToolActivity),
        ConfigurationChanges = ActivityConfigChanges)]
    public class EdiabasToolActivity : BaseActivity, View.IOnTouchListener
    {
        private enum ActivityRequest
        {
            RequestAppDetailBtSettings,
            RequestSelectSim,
            RequestSelectSgbd,
            RequestSelectDevice,
            RequestAdapterConfig,
            RequestOpenExternalFile,
            RequestYandexKey,
            RequestArgAssistStat,
            RequestArgAssistControl,
        }

        public class ExtraInfo
        {
            public ExtraInfo(string name, string type, List<string> commentList)
            {
                Name = name;
                Type = type;
                CommentList = commentList;
                Selected = false;
                ItemVisible = true;
                CheckVisible = true;
                GroupId = null;
                GroupSelected = false;
                GroupVisible = false;
                Tag = null;
            }

            public string Name { get; }

            public string Type { get; }

            public List<string> CommentList { get; }

            public List<string> CommentListTrans { get; set; }

            public bool Selected { get; set; }

            public bool ItemSelected => Selected && !GroupVisible;

            public bool ItemVisible { get; set; }

            public bool CheckVisible { get; set; }

            public int? GroupId { get; set; }

            public bool GroupSelected { get; set; }

            public bool GroupVisible { get; set; }

            public object Tag { get; set; }
        }

        private class JobInfo : ICloneable
        {
            public JobInfo(string name, string objectName)
            {
                Name = name;
                ObjectName = objectName;
                Comments = new List<string>();
                Arguments = new List<ExtraInfo>();
                Results = new List<ExtraInfo>();
                InitialIndex = null;
                InitialArgs = null;
                InitialResults = null;
            }

            public string Name { get; }

            public string ObjectName { get; }

            public List<string> Comments { get; }

            public List<string> CommentsTrans { get; set; }

            public List<ExtraInfo> Arguments { get; }

            public List<ExtraInfo> Results { get; }

            public int? InitialIndex { get; set; }

            public string InitialArgs { get; set; }

            public string InitialResults { get; set; }

            public JobInfo Clone()
            {
                JobInfo other = (JobInfo)MemberwiseClone();
                return other;
            }

            object ICloneable.Clone()
            {
                return Clone();
            }
        }

        public class InstanceData
        {
            public InstanceData()
            {
                SgbdFileName = string.Empty;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                TraceActive = true;
                SelectedJobName = string.Empty;
            }

            public bool ForceAppend { get; set; }
            public bool AutoStart { get; set; }
            public int AutoStartItemId { get; set; }
            public string SelectedJobName { get; set; }
            public string SgbdFileName { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string DataLogDir { get; set; }
            public string TraceDir { get; set; }
            public string SimulationDir { get; set; }
            public bool CheckMissingJobs { get; set; }
            public bool Offline { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool DataLogActive { get; set; }
            public int CommErrorsCount { get; set; }
        }

        // Intent extra
        public const string ExtraInitDir = "init_dir";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraConfigDir = "config_dir";
        public const string ExtraSimulationDir = "simulation_dir";
        public const string ExtraSgbdFile = "sgbd";
        public const string ExtraJobList = "job_list";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraElmWifiIp = "elmwifi_ip";
        public const string ExtraDeepObdWifiIp = "deepobdwifi_ip";
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        public const int UpdateDisplayDelay = 200;

        private const string TranslationFileName = "TranslationEdiabas.xml";

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private string _jobFilterText = string.Empty;
        private JobInfo _lastSelectedJob;
        private InputMethodManager _imm;
        private View _contentView;
        private View _barView;
        private AndroidX.AppCompat.Widget.SearchView _searchView;
        private CheckBox _checkBoxContinuous;
        private ToggleButton _buttonConnect;
        private Spinner _spinnerJobs;
        private JobListAdapter _jobListAdapter;
        private CheckBox _checkBoxBinArgs;
        private Button _buttonArgAssist;
        private EditText _editTextArgs;
        private Spinner _spinnerResults;
        private ResultSelectListAdapter _resultSelectListAdapter;
        private int _resultSelectLastItem;
        private ListView _listViewInfo;
        private ResultListAdapter _infoListAdapter;
        private string _initDirStart;
        private string _appDataDir;
        private string _configDir;
        private ActivityCommon _activityCommon;
        private EdiabasNet _ediabas;
        private StreamWriter _swDataLog;
        private Thread _jobThread;
        private bool _transmitCanceled;
        private volatile bool _runContinuous;
        private volatile bool _ediabasJobAbort;
        private string _sgbdFileNameInitial = string.Empty;
        private List<string[]> _jobListInitial;
        private bool _activityActive;
        private string _argAssistArgs;
        private bool _argAssistExecute;
        private bool _translateEnabled;
        private bool _translateActive;
        private bool _jobListTranslated;
        private readonly List<JobInfo> _jobList = new List<JobInfo>();
        private SgFunctions _sgFunctions;
        private readonly List<SgFunctions.SgFuncInfo> _sgFuncInfoList = new List<SgFunctions.SgFuncInfo>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme();
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
                JobInfo selectedJob = GetSelectedJob();
                bool update = _lastSelectedJob != null && selectedJob == _lastSelectedJob;

                NewJobSelected(update);
                if (!update)
                {
                    DisplayJobComments();
                }
            };

            _editTextArgs = FindViewById<EditText>(Resource.Id.editTextArgs);
            _editTextArgs.SetOnTouchListener(this);
            _editTextArgs.EditorAction += TextEditorAction;

            _checkBoxBinArgs = FindViewById<CheckBox>(Resource.Id.checkBoxBinArgs);
            _checkBoxBinArgs.SetOnTouchListener(this);

            _buttonArgAssist = FindViewById<Button>(Resource.Id.buttonArgAssist);
            _buttonArgAssist.SetOnTouchListener(this);
            _buttonArgAssist.Visibility =
                ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw ? ViewStates.Visible : ViewStates.Gone;
            _buttonArgAssist.Click += (sender, args) =>
            {
                StartArgAssist();
            };

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
            }, BroadcastReceived)
            {
                SelectedInterface = (ActivityCommon.InterfaceType)
                    Intent.GetIntExtra(ExtraInterface, (int) ActivityCommon.InterfaceType.None)
            };

            _initDirStart = Intent.GetStringExtra(ExtraInitDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _configDir = Intent.GetStringExtra(ExtraConfigDir);
            _sgbdFileNameInitial = Intent.GetStringExtra(ExtraSgbdFile);
            string[] jobListInitial = Intent.GetStringArrayExtra(ExtraJobList);
            if (jobListInitial != null && jobListInitial.Length > 0)
            {
                _jobListInitial = new List<string[]>();
                foreach (string jobEntry in jobListInitial)
                {
                    string[] jobItems = jobEntry.Split('#');
                    _jobListInitial.Add(jobItems);
                }
            }

            if (!_activityRecreated)
            {
                _instanceData.SimulationDir = Intent.GetStringExtra(ExtraSimulationDir);
                _instanceData.DeviceName = Intent.GetStringExtra(ExtraDeviceName);
                _instanceData.DeviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
                if (jobListInitial != null && jobListInitial.Length > 0)
                {
                    _instanceData.CheckMissingJobs = true;
                }
            }

            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);
            _activityCommon.SetPreferredNetworkInterface();

            ResetArgAssistResult();
            EdiabasClose(_instanceData.ForceAppend);
            ReadTranslation();
            UpdateDisplay();

            if (!_activityRecreated && !string.IsNullOrEmpty(_sgbdFileNameInitial))
            {
                _instanceData.SgbdFileName = _sgbdFileNameInitial;
            }

            string selectJobName = _activityRecreated ? _instanceData.SelectedJobName : null;
            ReadSgbd(selectJobName);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreTranslation();
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

            if (_activityCommon != null && _activityCommon.MtcBtService)
            {
                _activityCommon.StopMtcService();
            }
        }

        public override void Finish()
        {
            base.Finish();
            StoreTranslation();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _runContinuous = false;
            _ediabasJobAbort = true;
            if (IsJobRunning())
            {
                _jobThread?.Join();
            }
            EdiabasClose(true);

            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressedEvent()
        {
            if (!IsJobRunning())
            {
                if (!SendTraceFile((sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    base.OnBackPressedEvent();
                }))
                {
                    base.OnBackPressedEvent();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestAppDetailBtSettings:
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestSelectSim:
                    // When FilePickerActivity returns with a file
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        string simulationDir = string.Empty;
                        try
                        {
                            string fileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                            if (File.Exists(fileName))
                            {
                                simulationDir = Path.GetDirectoryName(fileName);
                            }
                            else if (Directory.Exists(fileName))
                            {
                                simulationDir = fileName;
                            }
                        }
                        catch (Exception)
                        {
                            simulationDir = string.Empty;
                        }

                        _instanceData.SimulationDir = simulationDir;
                        UpdateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestSelectSgbd:
                    // When FilePickerActivity returns with a file
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        _instanceData.SgbdFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        UpdateOptionsMenu();
                        ReadSgbd();
                    }
                    break;

                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
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

                case ActivityRequest.RequestAdapterConfig:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        bool invalidateAdapter = data.Extras.GetBoolean(CanAdapterActivity.ExtraInvalidateAdapter, false);
                        if (invalidateAdapter)
                        {
                            _instanceData.DeviceName = string.Empty;
                            _instanceData.DeviceAddress = string.Empty;
                            UpdateOptionsMenu();
                        }
                    }
                    break;

                case ActivityRequest.RequestOpenExternalFile:
                    UpdateOptionsMenu();
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = ActivityCommon.IsTranslationAvailable();
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestArgAssistStat:
                case ActivityRequest.RequestArgAssistControl:
                    ArgAssistBaseActivity.IntentSgFuncInfo = null;
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        _argAssistArgs = data.Extras.GetString(ArgAssistBaseActivity.ExtraArguments, "");
                        _argAssistExecute = data.Extras.GetBoolean(ArgAssistBaseActivity.ExtraExecute, false);
                        if (IsJobRunning())
                        {
                            break;
                        }

                        SetArgAssistResult();
                    }

                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (_activityCommon == null)
            {
                return;
            }
            switch (requestCode)
            {
                case ActivityCommon.RequestPermissionBluetooth:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        UpdateOptionsMenu();
                        break;
                    }

                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            ActivityCommon.OpenAppSettingDetails(this, (int)ActivityRequest.RequestAppDetailBtSettings);
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.access_permission_rejected)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.ediabas_tool_menu, menu);

            IMenuItem menuSearch = menu.FindItem(Resource.Id.action_search);
            if (menuSearch != null)
            {
                menuSearch.SetActionView(new AndroidX.AppCompat.Widget.SearchView(this));

                if (menuSearch.ActionView is AndroidX.AppCompat.Widget.SearchView searchViewV7)
                {
                    _searchView = searchViewV7;
                    searchViewV7.QueryTextChange += (sender, e) =>
                    {
                        e.Handled = OnQueryTextChange(e.NewText, false);
                    };

                    searchViewV7.QueryTextSubmit += (sender, e) =>
                    {
                        e.Handled = OnQueryTextChange(e.NewText, true);
                    };
                }
            }

            return true;
        }

        public override void PrepareOptionsMenu(IMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            bool commActive = IsJobRunning();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable(_instanceData.SimulationDir, true);
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

            IMenuItem selSgbdSimDirMenu = menu.FindItem(Resource.Id.menu_sel_sim_dir);
            if (selSgbdSimDirMenu != null)
            {
                bool validDir = ActivityCommon.IsValidSimDir(_instanceData.SimulationDir);
                selSgbdSimDirMenu.SetVisible(!commActive && _activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Simulation);
                selSgbdSimDirMenu.SetChecked(validDir);
            }

            IMenuItem selSgbdGrpMenu = menu.FindItem(Resource.Id.menu_tool_sel_sgbd_grp);
            if (selSgbdGrpMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.SgbdFileName))
                {
                    bool groupFile = string.Compare(Path.GetExtension(_instanceData.SgbdFileName), EdiabasNet.GroupFileExt, StringComparison.OrdinalIgnoreCase) == 0;
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
                bool menuVisible = _activityCommon.GetAdapterIpName(out string longName, out string _);

                enetIpMenu.SetTitle(longName);
                enetIpMenu.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline && !fixedSgbd);
                enetIpMenu.SetVisible(menuVisible);
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            logSubMenu?.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline);

            IMenuItem traceSubmenu = menu.FindItem(Resource.Id.menu_trace_submenu);
            traceSubmenu?.SetEnabled(!commActive);

            bool tracePresent = ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir);
            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && !_instanceData.Offline && _instanceData.TraceActive && tracePresent);

            IMenuItem openTraceMenu = menu.FindItem(Resource.Id.menu_open_trace);
            openTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);

            IMenuItem translationSubmenu = menu.FindItem(Resource.Id.menu_translation_submenu);
            if (translationSubmenu != null)
            {
                translationSubmenu.SetEnabled(true);
                translationSubmenu.SetVisible(_activityCommon.IsTranslationRequired());
            }

            IMenuItem translationEnableMenu = menu.FindItem(Resource.Id.menu_translation_enable);
            if (translationEnableMenu != null)
            {
                translationEnableMenu.SetEnabled(!commActive || ActivityCommon.IsTranslationAvailable());
                translationEnableMenu.SetVisible(_activityCommon.IsTranslationRequired());
                translationEnableMenu.SetChecked(ActivityCommon.EnableTranslation);
            }

            IMenuItem translationYandexKeyMenu = menu.FindItem(Resource.Id.menu_translation_yandex_key);
            if (translationYandexKeyMenu != null)
            {
                translationYandexKeyMenu.SetEnabled(!commActive);
                translationYandexKeyMenu.SetVisible(_activityCommon.IsTranslationRequired());
            }

            IMenuItem translationClearCacheMenu = menu.FindItem(Resource.Id.menu_translation_clear_cache);
            if (translationClearCacheMenu != null)
            {
                translationClearCacheMenu.SetEnabled(!_activityCommon.IsTranslationCacheEmpty());
                translationClearCacheMenu.SetVisible(_activityCommon.IsTranslationRequired());
            }
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
                        if (_activityCommon == null)
                        {
                            return;
                        }

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

                case Resource.Id.menu_sel_sim_dir:
                    if (IsJobRunning())
                    {
                        return true;
                    }

                    SelectSimDir();
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
                        if (_activityCommon.ShowConnectWarning(action =>
                        {
                            switch (action)
                            {
                                case ActivityCommon.SsidWarnAction.Continue:
                                    OnOptionsItemSelected(item);
                                    break;

                                case ActivityCommon.SsidWarnAction.EditIp:
                                    AdapterIpConfig();
                                    break;
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
                    AdapterIpConfig();
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
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        UpdateOptionsMenu();
                    });
                    return true;

                case Resource.Id.menu_open_trace:
                    if (IsJobRunning())
                    {
                        return true;
                    }
                    OpenTraceFile();
                    return true;

                case Resource.Id.menu_translation_enable:
                    if (!ActivityCommon.EnableTranslation && !ActivityCommon.IsTranslationAvailable())
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
                    StoreTranslation();
                    _jobListTranslated = false;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    UpdateTranslationText();
                    return true;

                case Resource.Id.menu_submenu_help:
                    _activityCommon.ShowWifiConnectedWarning(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _activityCommon.OpenWebUrl("https://uholeschak.github.io/ediabaslib/docs/EdiabasTool.html");
                    });
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool IsFinishAllowed()
        {
            if (_activityCommon == null)
            {
                return true;
            }

            if (IsJobRunning())
            {
                return false;
            }

            if (_activityCommon.TranslateActive)
            {
                return false;
            }

            return true;
        }

        public override void CloseSearchView()
        {
            if (_searchView != null && !_searchView.Iconified)
            {
                _searchView.Iconified = true;
                _searchView.Iconified = true;
            }

            _jobFilterText = string.Empty;
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

        private bool OnQueryTextChange(string text, bool submit)
        {
            if (string.Compare(_jobFilterText, text, StringComparison.Ordinal) != 0)
            {
                _jobFilterText = text;
                UpdateJobList(true);
            }

            if (submit)
            {
                HideKeyboard();
            }

            return true;
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

            if (_sgFunctions != null)
            {
                _sgFunctions.Dispose();
                _sgFunctions = null;
            }

            if (_ediabas != null)
            {
                _ediabas.Dispose();
                _ediabas = null;
            }

            _instanceData.ForceAppend = forceAppend;
            ClearLists();
            CloseDataLog();
            UpdateLogInfo();
            UpdateDisplay();
            UpdateOptionsMenu();
            return true;
        }

        private void ClearLists()
        {
            _jobList.Clear();
            _sgFuncInfoList.Clear();
            _sgFunctions?.ResetCache();
            _lastSelectedJob = null;
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

        private void TextEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            switch (e.ActionId)
            {
                case ImeAction.Go:
                case ImeAction.Send:
                case ImeAction.Next:
                case ImeAction.Done:
                case ImeAction.Previous:
                    HideKeyboard();
                    if (sender == _editTextArgs)
                    {
                        if (_buttonArgAssist.Enabled)
                        {
                            NewJobSelected(true);
                        }
                    }
                    break;
            }
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }

        private bool SendTraceFile(EventHandler<EventArgs> handler)
        {
            long responseCount = 0;
            if (_ediabas?.EdInterfaceClass != null)
            {
                responseCount = _ediabas.EdInterfaceClass.ResponseCount;
            }

            if (_instanceData.CommErrorsCount >= ActivityCommon.MinSendCommErrors && responseCount > 0 &&
                _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                return _activityCommon.RequestSendTraceFile(_appDataDir, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (ActivityCommon.CollectDebugInfo ||
                (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir)))
            {
                if (!EdiabasClose())
                {
                    return false;
                }
                _instanceData.SgbdFileName = string.Empty;
                return _activityCommon.SendTraceFile(_appDataDir, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        private bool OpenTraceFile()
        {
            string baseDir = _instanceData.TraceDir;
            if (string.IsNullOrEmpty(baseDir))
            {
                return false;
            }

            if (!EdiabasClose())
            {
                return false;
            }

            string traceFile = Path.Combine(baseDir, ActivityCommon.TraceFileName);
            string errorMessage = _activityCommon.OpenExternalFile(traceFile, (int)ActivityRequest.RequestOpenExternalFile);
            if (errorMessage != null)
            {
                if (string.IsNullOrEmpty(traceFile))
                {
                    return true;
                }

                string message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.open_trace_file_failed), traceFile);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message = errorMessage + "\r\n" + message;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return false;
            }
            return true;
        }

        private void UpdateTranslationText()
        {
            _jobListAdapter.NotifyDataSetChanged();
            NewJobSelected();
            DisplayJobComments();
        }

        private bool ReadTranslation()
        {
            if (_activityCommon == null)
            {
                return false;
            }

            try
            {
                if (ActivityCommon.IsTranslationAvailable() && _activityCommon.IsTranslationRequired() && !string.IsNullOrEmpty(_configDir))
                {
                    return _activityCommon.ReadTranslationCache(Path.Combine(_configDir, TranslationFileName));
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private bool StoreTranslation()
        {
            if (_activityCommon == null)
            {
                return false;
            }

            try
            {
                if (ActivityCommon.IsTranslationAvailable() && _activityCommon.IsTranslationRequired() && !string.IsNullOrEmpty(_configDir))
                {
                    return _activityCommon.StoreTranslationCache(Path.Combine(_configDir, TranslationFileName));
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private void UpdateDisplay()
        {
            if (_activityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation && !ActivityCommon.IsTranslationAvailable())
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
                if (!_activityCommon.IsInterfaceAvailable(_instanceData.SimulationDir))
                {
                    buttonConnectEnable = false;
                }
                if (TranslateEcuText((sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
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

            bool isRunning = IsJobRunning();
            if (isRunning || _instanceData.Offline)
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
            if (!isRunning)
            {
                _buttonConnect.Checked = false;
            }
            _spinnerJobs.Enabled = inputsEnabled;
            _editTextArgs.Enabled = inputsEnabled;
            _checkBoxBinArgs.Enabled = inputsEnabled;
            _spinnerResults.Enabled = inputsEnabled;
            if (!inputsEnabled)
            {
                _buttonArgAssist.Enabled = false;
            }
            else
            {
                JobInfo jobInfo = GetSelectedJob();
                int serviceId = GetArgAssistJobService(jobInfo);
                _buttonArgAssist.Enabled = GetArgAssistFuncCount(serviceId) > 0;
            }

            HideKeyboard();
        }

        private void SelectSimDir()
        {
            // Launch the FilePickerActivity to select a simulation dir
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = _appDataDir;
            try
            {
                if (!string.IsNullOrEmpty(_instanceData.SimulationDir) && Directory.Exists(_instanceData.SimulationDir))
                {
                    initDir = _instanceData.SimulationDir;
                }
            }
            catch (Exception)
            {
                initDir = _appDataDir;
            }

            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.menu_sel_sim_dir));
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".sim");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectSim);
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
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, groupFile ? EdiabasNet.GroupFileExt : EdiabasNet.PrgFileExt);
            serverIntent.PutExtra(FilePickerActivity.ExtraShowExtension, false);
            serverIntent.PutExtra(FilePickerActivity.ExtraDecodeFileName, true);
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
                if (_activityCommon == null)
                {
                    return;
                }
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
                if (_activityCommon == null)
                {
                    return;
                }
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
                if (_activityCommon == null)
                {
                    return;
                }

                SparseBooleanArray sparseArray = listView.CheckedItemPositions;
                if (sparseArray == null)
                {
                    return;
                }

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

            AlertDialog alertDialog = builder.Create();
            alertDialog.ShowEvent += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                if (ActivityCommon.IsDocumentTreeSupported())
                {
                    ActivityCommon.ShowAlertDialogBallon(this, alertDialog, Resource.String.menu_hint_copy_folder);
                }
            };
            alertDialog.Show();
        }

        private void AdapterConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }

            if (_activityCommon.ShowConnectWarning(action =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                switch (action)
                {
                    case ActivityCommon.SsidWarnAction.Continue:
                        AdapterConfig();
                        break;

                    case ActivityCommon.SsidWarnAction.EditIp:
                        AdapterIpConfig();
                        break;
                }
            }))
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
            serverIntent.PutExtra(CanAdapterActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            serverIntent.PutExtra(CanAdapterActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(CanAdapterActivity.ExtraAppDataDir, _appDataDir);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestAdapterConfig);
        }

        private bool EditYandexKey()
        {
            try
            {
                Intent serverIntent = new Intent(this, typeof(TranslateKeyActivity));
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestYandexKey);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void StartArgAssist()
        {
            JobInfo jobInfo = GetSelectedJob();
            int serviceId = GetArgAssistJobService(jobInfo);

            if (serviceId >= 0)
            {
                Intent serverIntent;
                ActivityRequest activityRequest;
                switch ((SgFunctions.UdsServiceId) serviceId)
                {
                    case SgFunctions.UdsServiceId.ReadDataById:
                    case SgFunctions.UdsServiceId.DynamicallyDefineId:
                    case SgFunctions.UdsServiceId.MwBlock:
                    {
                        serverIntent = new Intent(this, typeof(ArgAssistStatActivity));
                        activityRequest = ActivityRequest.RequestArgAssistStat;
                        break;
                    }

                    default:
                    {
                        serverIntent = new Intent(this, typeof(ArgAssistControlActivity));
                        activityRequest = ActivityRequest.RequestArgAssistControl;
                        break;
                    }
                }

                ResetArgAssistResult();
                ArgAssistBaseActivity.IntentSgFuncInfo = _sgFuncInfoList;
                string arguments = _editTextArgs.Enabled ? _editTextArgs.Text : string.Empty;
                serverIntent.PutExtra(ArgAssistBaseActivity.ExtraServiceId, serviceId);
                serverIntent.PutExtra(ArgAssistBaseActivity.ExtraOffline, _instanceData.Offline);
                serverIntent.PutExtra(ArgAssistBaseActivity.ExtraArguments, arguments);
                StartActivityForResult(serverIntent, (int)activityRequest);
            }
        }

        private void AdapterIpConfig()
        {
            if (!EdiabasClose())
            {
                return;
            }
            _activityCommon.SelectAdapterIp((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                UpdateOptionsMenu();
            });
        }

        private JobInfo GetSelectedJob()
        {
            int index = _spinnerJobs.SelectedItemPosition;
            if (index < 0 || index >= _jobListAdapter.Items.Count)
            {
                return null;
            }
            return _jobListAdapter.Items[index];
        }

        private void NewJobSelected(bool update = false)
        {
            if (_jobList.Count == 0)
            {
                return;
            }

            JobInfo jobInfo = GetSelectedJob();
            _instanceData.SelectedJobName = jobInfo?.Name ?? string.Empty;
            int serviceId = GetArgAssistJobService(jobInfo);

            _resultSelectListAdapter.Items.Clear();
            string defaultArgs = string.Empty;
            bool defaultBinArgs = false;
            if (jobInfo != null)
            {
                if (jobInfo.InitialArgs != null)
                {
                    defaultArgs = jobInfo.InitialArgs;
                    if (defaultArgs.StartsWith('|'))
                    {
                        defaultBinArgs = true;
                        defaultArgs = defaultArgs.Remove(0, 1);
                    }
                }
                else
                {
                    foreach (ExtraInfo result in jobInfo.Results.OrderBy(x => x.Name))
                    {
                        _resultSelectListAdapter.Items.Add(result);
                    }

                    AddArgAssistResults(serviceId);

                    if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                    {
                        if (string.Compare(jobInfo.Name, XmlToolActivity.JobReadMwBlock, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            defaultArgs = jobInfo.ObjectName.Contains("1281") ? "0;WertEinmalLesen" : "100;LESEN";
                        }
                    }
                }
            }

            _resultSelectListAdapter.NotifyDataSetChanged();
            _resultSelectLastItem = 0;
            _lastSelectedJob = jobInfo;
            if (!update)
            {
                _editTextArgs.Text = defaultArgs;
                _checkBoxBinArgs.Checked = defaultBinArgs;
                _buttonArgAssist.Enabled = GetArgAssistFuncCount(serviceId) > 0;
            }
        }

        private int GetArgAssistFuncCount(int serviceId)
        {
            int funcCount = 0;
            if (serviceId >= 0)
            {
                foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList)
                {
                    if (serviceId == (int) SgFunctions.UdsServiceId.MwBlock)
                    {
                        if (funcInfo.ServiceList == null)
                        {
                            funcCount++;
                        }
                    }
                    else
                    {
                        if (funcInfo.ServiceList != null && funcInfo.ServiceList.Contains(serviceId))
                        {
                            funcCount++;
                        }
                    }
                }
            }

            return funcCount;
        }

        private void AddArgAssistResults(int serviceId)
        {
            if (serviceId >= 0)
            {
                string[] argArray = _editTextArgs.Text.Split(";");
                bool mwBlock = false;
                string argType = string.Empty;
                List<string> argList = null;
                switch ((SgFunctions.UdsServiceId) serviceId)
                {
                    case SgFunctions.UdsServiceId.ReadDataById:
                        if (argArray.Length > 1)
                        {
                            argType = argArray[0].Trim();
                            argList = argArray.ToList();
                            argList.RemoveAt(0);
                        }
                        break;

                    case SgFunctions.UdsServiceId.DynamicallyDefineId:
                        if (argArray.Length > 3)
                        {
                            argType = argArray[2].Trim();
                            argList = argArray.ToList();
                            argList.RemoveRange(0, 3);
                        }
                        break;

                    case SgFunctions.UdsServiceId.MwBlock:
                        mwBlock = true;
                        if (argArray.Length > 0)
                        {
                            argList = argArray.ToList();
                            argList.RemoveAt(0);
                        }
                        break;

                    default:
                        if (argArray.Length > 1)
                        {
                            argType = argArray[0].Trim();
                            argList = new List<string> {argArray[1]};
                        }
                        break;
                }

                int groupId = 0;
                bool argTypeId = argType.ToUpperInvariant() == ArgAssistBaseActivity.ArgTypeID;
                if (argList != null)
                {
                    foreach (string arg in argList)
                    {
                        SgFunctions.SgFuncInfo argFuncInfo = null;
                        foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList)
                        {
                            if (mwBlock)
                            {
                                if (string.Compare(arg.Trim(), funcInfo.Arg, StringComparison.OrdinalIgnoreCase) == 0 && funcInfo.ServiceList == null)
                                {
                                    argFuncInfo = funcInfo;
                                }
                            }
                            else
                            {
                                if (string.Compare(arg.Trim(), argTypeId ? funcInfo.Id : funcInfo.Arg, StringComparison.OrdinalIgnoreCase) == 0 &&
                                    funcInfo.ServiceList != null && funcInfo.ServiceList.Contains(serviceId))
                                {
                                    argFuncInfo = funcInfo;
                                    break;
                                }
                            }
                        }

                        if (argFuncInfo != null)
                        {
                            if (argFuncInfo.ResInfoList != null)
                            {
                                List<SgFunctions.SgFuncNameInfo> resultList = new List<SgFunctions.SgFuncNameInfo>();
                                foreach (SgFunctions.SgFuncNameInfo funcNameInfo in argFuncInfo.ResInfoList)
                                {
                                    if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                                    {
                                        if (funcBitFieldInfo.TableDataType == SgFunctions.TableDataType.Bit &&
                                            funcBitFieldInfo.NameInfoList != null)
                                        {
                                            foreach (SgFunctions.SgFuncNameInfo nameInfo in funcBitFieldInfo.NameInfoList)
                                            {
                                                if (nameInfo is SgFunctions.SgFuncBitFieldInfo nameInfoBitField)
                                                {
                                                    if (!string.IsNullOrEmpty(nameInfoBitField.ResultName))
                                                    {
                                                        resultList.Add(nameInfoBitField);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(funcBitFieldInfo.ResultName))
                                            {
                                                resultList.Add(funcNameInfo);
                                            }
                                        }
                                    }
                                }

                                int groupSize = 0;
                                foreach (SgFunctions.SgFuncNameInfo funcNameInfo in resultList.OrderBy(x => (x as SgFunctions.SgFuncBitFieldInfo)?.ResultName ?? string.Empty))
                                {
                                    if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                                    {
                                        string info = funcBitFieldInfo.InfoTrans ?? funcBitFieldInfo.Info;
                                        ExtraInfo extraInfo = new ExtraInfo(funcBitFieldInfo.ResultName, string.Empty, new List<string> {info})
                                        {
                                            GroupId = groupId
                                        };

                                        if (groupSize == 0)
                                        {
                                            string infoGroup = argFuncInfo.InfoTrans ?? argFuncInfo.Info;
                                            ExtraInfo extraInfoGroup = new ExtraInfo(arg, string.Empty, new List<string> { infoGroup })
                                            {
                                                GroupVisible = true,
                                                GroupId = groupId
                                            };

                                            _resultSelectListAdapter.Items.Add(extraInfoGroup);
                                        }

                                        _resultSelectListAdapter.Items.Add(extraInfo);
                                        groupSize++;
                                    }
                                }

                                if (groupSize > 0)
                                {
                                    groupId++;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(argFuncInfo.Result))
                                {
                                    string info = argFuncInfo.InfoTrans ?? argFuncInfo.Info;
                                    ExtraInfo extraInfo = new ExtraInfo(argFuncInfo.Result, string.Empty, new List<string> { info });
                                    _resultSelectListAdapter.Items.Add(extraInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetArgAssistJobService(JobInfo jobInfo)
        {
            if (jobInfo == null)
            {
                return -1;
            }
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                return -1;
            }

            return SgFunctions.GetJobService(jobInfo.Name);
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
                    AppendSbText(stringBuilderComments, comment);
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
                else if (string.Compare(jobInfo.Name, "IS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _infoListAdapter.Items.Add(new TableResultItem(GetString(Resource.String.tool_job_arguments_info_detail), null));
                }
                foreach (ExtraInfo info in jobInfo.Arguments)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
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
                int resultIndex = _spinnerResults.SelectedItemPosition;
                if (resultIndex >= 0 && resultIndex < _resultSelectListAdapter.ItemsVisible.Count)
                {
                    ExtraInfo info = _resultSelectListAdapter.ItemsVisible[resultIndex];
                    StringBuilder stringBuilderComments = new StringBuilder();
                    stringBuilderComments.Append(info.Name + " (" + info.Type + "):");
                    foreach (string comment in info.CommentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
                    }
                    _infoListAdapter.Items.Add(new TableResultItem(stringBuilderComments.ToString(), null));
                }
            }
            _infoListAdapter.NotifyDataSetChanged();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool TranslateEcuText(EventHandler<EventArgs> handler = null)
        {
            if (_translateEnabled && !_translateActive && _activityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation)
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

                    foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList)
                    {
                        if (funcInfo.InfoTrans == null)
                        {
                            if (!string.IsNullOrEmpty(funcInfo.Info))
                            {
                                stringList.Add(funcInfo.Info);
                            }
                        }
                    }

                    if (_sgFunctions != null)
                    {
                        foreach (KeyValuePair<string, List<SgFunctions.SgFuncArgInfo>> argInfoPair in _sgFunctions.SgFuncArgInfoDict)
                        {
                            foreach (SgFunctions.SgFuncArgInfo funcArgInfo in argInfoPair.Value)
                            {
                                if (funcArgInfo.InfoTrans == null)
                                {
                                    if (!string.IsNullOrEmpty(funcArgInfo.Info))
                                    {
                                        stringList.Add(funcArgInfo.Info);
                                    }
                                }
                            }
                        }

                        foreach (KeyValuePair<string, List<SgFunctions.SgFuncNameInfo>> funcNameInfoPair in _sgFunctions.SgFuncNameInfoDict)
                        {
                            foreach (SgFunctions.SgFuncNameInfo funcNameInfo in funcNameInfoPair.Value)
                            {
                                if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                                {
                                    if (funcBitFieldInfo.InfoTrans == null)
                                    {
                                        if (!string.IsNullOrEmpty(funcBitFieldInfo.Info))
                                        {
                                            stringList.Add(funcBitFieldInfo.Info);
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

                                    foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList)
                                    {
                                        if (funcInfo.InfoTrans == null)
                                        {
                                            if (!string.IsNullOrEmpty(funcInfo.Info))
                                            {
                                                if (transIndex < transList.Count)
                                                {
                                                    funcInfo.InfoTrans = transList[transIndex++];
                                                }
                                            }
                                        }
                                    }

                                    if (_sgFunctions != null)
                                    {
                                        foreach (KeyValuePair<string, List<SgFunctions.SgFuncArgInfo>> argInfoPair in _sgFunctions.SgFuncArgInfoDict)
                                        {
                                            foreach (SgFunctions.SgFuncArgInfo funcArgInfo in argInfoPair.Value)
                                            {
                                                if (funcArgInfo.InfoTrans == null)
                                                {
                                                    if (!string.IsNullOrEmpty(funcArgInfo.Info))
                                                    {
                                                        if (transIndex < transList.Count)
                                                        {
                                                            funcArgInfo.InfoTrans = transList[transIndex++];
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        foreach (KeyValuePair<string, List<SgFunctions.SgFuncNameInfo>> funcNameInfoPair in _sgFunctions.SgFuncNameInfoDict)
                                        {
                                            foreach (SgFunctions.SgFuncNameInfo funcNameInfo in funcNameInfoPair.Value)
                                            {
                                                if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                                                {
                                                    if (funcBitFieldInfo.InfoTrans == null)
                                                    {
                                                        if (!string.IsNullOrEmpty(funcBitFieldInfo.Info))
                                                        {
                                                            if (transIndex < transList.Count)
                                                            {
                                                                funcBitFieldInfo.InfoTrans = transList[transIndex++];
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

                    foreach (SgFunctions.SgFuncInfo funcInfo in _sgFuncInfoList)
                    {
                        funcInfo.InfoTrans = null;
                    }

                    if (_sgFunctions != null)
                    {
                        foreach (KeyValuePair<string, List<SgFunctions.SgFuncArgInfo>> argInfoPair in _sgFunctions.SgFuncArgInfoDict)
                        {
                            foreach (SgFunctions.SgFuncArgInfo funcArgInfo in argInfoPair.Value)
                            {
                                funcArgInfo.InfoTrans = null;
                            }
                        }

                        foreach (KeyValuePair<string, List<SgFunctions.SgFuncNameInfo>> funcNameInfoPair in _sgFunctions.SgFuncNameInfoDict)
                        {
                            foreach (SgFunctions.SgFuncNameInfo funcNameInfo in funcNameInfoPair.Value)
                            {
                                if (funcNameInfo is SgFunctions.SgFuncBitFieldInfo funcBitFieldInfo)
                                {
                                    funcBitFieldInfo.InfoTrans = null;
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

        private void UpdateLogInfo()
        {
            string logDir = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(_appDataDir))
                {
                    logDir = Path.Combine(_appDataDir, "LogEdiabasTool");
                    Directory.CreateDirectory(logDir);
                }
            }
            catch (Exception)
            {
                logDir = string.Empty;
            }
            _instanceData.DataLogDir = logDir;

            _instanceData.TraceDir = null;
            if (_instanceData.TraceActive)
            {
                _instanceData.TraceDir = logDir;
            }

            if (_ediabas != null)
            {
                ActivityCommon.SetEdiabasConfigProperties(_ediabas, _instanceData.TraceDir, _instanceData.SimulationDir, _instanceData.TraceAppend || _instanceData.ForceAppend);
            }
        }

        private void EdiabasInit()
        {
            if (_ediabas == null)
            {
                _ediabas = new EdiabasNet
                {
                    EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                    AbortJobFunc = AbortEdiabasJob
                };
                _ediabas.SetConfigProperty("EcuPath", Path.GetDirectoryName(_instanceData.SgbdFileName));
                _sgFunctions = new SgFunctions(_ediabas);
            }

            _transmitCanceled = false;
            _ediabas.EdInterfaceClass.TransmitCancel(false);
        }

        private void ReadSgbd(string selectJobName = null)
        {
            if (string.IsNullOrEmpty(_instanceData.SgbdFileName))
            {
                return;
            }
            _translateEnabled = false;
            CloseDataLog();
            EdiabasInit();

            ClearLists();
            UpdateDisplay();

            _activityCommon.SetEdiabasInterface(_ediabas, _instanceData.DeviceAddress, _appDataDir);

            UpdateLogInfo();

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.tool_read_sgbd));
            progress.ButtonAbort.Visibility = ViewStates.Visible;
            progress.AbortClick = sender =>
            {
                _transmitCanceled = true;
                _ediabas.EdInterfaceClass.TransmitCancel(true);
            };
            progress.Show();

            _ediabasJobAbort = false;
            _jobThread = new Thread(() =>
            {
                List<string> messageList = new List<string>();
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, _instanceData.SgbdFileName);

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

                    List<SgFunctions.SgFuncInfo> sgFuncInfoList = _sgFunctions.ReadSgFuncTable();
                    if (sgFuncInfoList == null || sgFuncInfoList.Count == 0)
                    {
                        sgFuncInfoList = _sgFunctions.ReadMwTabTable();
                    }

                    if (sgFuncInfoList != null)
                    {
                        _sgFuncInfoList.AddRange(sgFuncInfoList);
                    }
                }
                catch (Exception ex)
                {
                    string exceptionText = string.Empty;
                    if (!AbortEdiabasJob())
                    {
                        exceptionText = EdiabasNet.GetExceptionText(ex, false, false);
                    }
                    messageList.Add(exceptionText);
                    if (!_transmitCanceled && ActivityCommon.IsCommunicationError(exceptionText))
                    {
                        _instanceData.CommErrorsCount++;
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    progress.Dismiss();

                    if (IsJobRunning())
                    {
                        _jobThread?.Join();
                    }

                    _jobListTranslated = false;
                    _translateEnabled = true;
                    UpdateJobList();

                    if (!string.IsNullOrEmpty(selectJobName))
                    {
                        for (int jobIndex = 0; jobIndex < _jobListAdapter.Items.Count; jobIndex++)
                        {
                            if (string.Compare(selectJobName, _jobListAdapter.Items[jobIndex].Name, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _spinnerJobs.SetSelection(jobIndex);
                                break;
                            }
                        }
                    }

                    UpdateOptionsMenu();
                    UpdateDisplay();

                    if (_jobList.Count == 0 && messageList.Count > 0)
                    {
                        _infoListAdapter.Items.Clear();
                        foreach (string message in messageList)
                        {
                            _infoListAdapter.Items.Add(new TableResultItem(message, null));
                        }
                        _infoListAdapter.NotifyDataSetChanged();
                    }

                    if (!string.IsNullOrEmpty(selectJobName))
                    {
                        SetArgAssistResult();
                    }
                });
            });
            _jobThread.Start();
        }

        private void UpdateJobList(bool keepSelection = false)
        {
            JobInfo jobInfoSelected = GetSelectedJob();
            HashSet<int> initialJobsIndexHash = new HashSet<int>();
            _jobListAdapter.Items.Clear();
            foreach (JobInfo job in _jobList.OrderBy(x => x.Name))
            {
                bool jobValid = true;
                if (!string.IsNullOrEmpty(_jobFilterText))
                {
                    jobValid = false;
                    if (IsSearchFilterMatching(job.Name, _jobFilterText))
                    {
                        jobValid = true;
                    }

                    foreach (ExtraInfo extraInfo in job.Arguments)
                    {
                        if (IsSearchFilterMatching(extraInfo.Name, _jobFilterText))
                        {
                            jobValid = true;
                            break;
                        }
                    }

                    foreach (ExtraInfo extraInfo in job.Results)
                    {
                        if (IsSearchFilterMatching(extraInfo.Name, _jobFilterText))
                        {
                            jobValid = true;
                            break;
                        }
                    }
                }


                if (_jobListInitial != null && _jobListInitial.Count > 0)
                {
                    int initialIndex = 0;
                    foreach (string[] jobItems in _jobListInitial)
                    {
                        if (jobItems.Length > 0 && string.Compare(job.Name, jobItems[0], StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            initialJobsIndexHash.Add(initialIndex);
                            if (jobValid)
                            {
                                JobInfo jobClone = job.Clone();
                                jobClone.InitialIndex = initialIndex;
                                jobClone.InitialArgs = jobItems.Length > 1 ? jobItems[1] : string.Empty;
                                jobClone.InitialResults = jobItems.Length > 2 ? jobItems[2] : string.Empty;
                                _jobListAdapter.Items.Add(jobClone);
                            }
                        }

                        initialIndex++;
                    }
                }
                else
                {
                    if (jobValid)
                    {
                        _jobListAdapter.Items.Add(job);
                    }
                }
            }

            _jobListAdapter.NotifyDataSetChanged();

            bool selectionChanged = true;
            if (keepSelection)
            {
                if (jobInfoSelected != null)
                {
                    int selectedIndex = _jobListAdapter.Items.IndexOf(jobInfoSelected);
                    if (selectedIndex >= 0)
                    {
                        _spinnerJobs.SetSelection(selectedIndex);
                    }

                    JobInfo jobInfoCurrent = GetSelectedJob();
                    if (jobInfoCurrent == jobInfoSelected)
                    {
                        selectionChanged = false;
                    }
                }
            }
            else
            {
                if (_jobListInitial != null && _jobListInitial.Count > 0)
                {
                    int initialIndex = -1;
                    int jobIndex = -1;
                    int index = 0;
                    foreach (JobInfo jobInfo in _jobListAdapter.Items)
                    {
                        if (jobInfo.InitialIndex.HasValue && (initialIndex < 0 || jobInfo.InitialIndex.Value < initialIndex))
                        {
                            initialIndex = jobInfo.InitialIndex.Value;
                            jobIndex = index;
                        }

                        index++;
                    }

                    if (jobIndex >= 0)
                    {
                        _spinnerJobs.SetSelection(jobIndex);
                    }

                    if (_instanceData.CheckMissingJobs)
                    {
                        List<string> missingJobsList = new List<string>();
                        int indexMissing = 0;
                        foreach (string[] jobItems in _jobListInitial)
                        {
                            if (jobItems.Length > 0 && !initialJobsIndexHash.Contains(indexMissing))
                            {
                                missingJobsList.Add(jobItems[0]);
                            }

                            indexMissing++;
                        }

                        if (missingJobsList.Count > 0 && _jobList.Count > 0)
                        {
                            StringBuilder sbJobs = new StringBuilder();
                            foreach (string jobName in missingJobsList)
                            {
                                if (sbJobs.Length > 0)
                                {
                                    sbJobs.Append(", ");
                                }

                                sbJobs.Append(jobName);
                            }
                            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Missing jobs names: {0}", sbJobs.ToString());

                            sbJobs.Insert(0, GetString(Resource.String.tool_jobs_not_found_list) + "\r\n");
                            _activityCommon.ShowAlert(sbJobs.ToString(), Resource.String.alert_title_warning);
                        }
                    }
                }
            }

            _instanceData.CheckMissingJobs = false;
            if (selectionChanged)
            {
                NewJobSelected();
                DisplayJobComments();
            }
        }

        private void ResetArgAssistResult()
        {
            _argAssistArgs = null;
            _argAssistExecute = false;
        }

        private bool SetArgAssistResult()
        {
            if (string.IsNullOrEmpty(_argAssistArgs))
            {
                return false;
            }

            _checkBoxBinArgs.Checked = false;
            _editTextArgs.Text = _argAssistArgs;
            NewJobSelected(true);

            if (!_instanceData.Offline && !string.IsNullOrEmpty(_editTextArgs.Text) && _argAssistExecute)
            {
                ExecuteSelectedJob(false);
            }

            ResetArgAssistResult();

            return true;
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
                if (!info.ItemSelected)
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
                long? lastUpdateTime = null;
                object messageListLock = new object();
                List<string> messageListNew = new List<string>();
                List<string> messageListCurrent = null;

                for (; ; )
                {
                    messageListNew.Clear();
                    try
                    {
                        bool fsDetail = string.Compare(jobName, "FS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0;
                        bool isDetail = string.Compare(jobName, "IS_LESEN_DETAIL", StringComparison.OrdinalIgnoreCase) == 0;
                        if ((fsDetail || isDetail) && string.IsNullOrEmpty(jobArgs))
                        {
                            string readJob = fsDetail ? "FS_LESEN" : "IS_LESEN";
                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(readJob);

                            List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;

                            bool jobOk = false;
                            if (resultSets != null && resultSets.Count > 1)
                            {
                                if (EdiabasThread.IsJobStatusOk(resultSets[^1]))
                                {
                                    jobOk = true;
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

                                            _ediabas.ExecuteJob(jobName);

                                            List<Dictionary<string, EdiabasNet.ResultData>> resultSetsDetail =
                                                new List<Dictionary<string, EdiabasNet.ResultData>>(_ediabas.ResultSets);
                                            PrintResults(messageListNew, resultSetsDetail);
                                        }
                                    }
                                    dictIndex++;
                                }
                                if (messageListNew.Count == 0)
                                {
                                    messageListNew.Add(GetString(Resource.String.tool_no_errors));
                                }
                            }
                            else
                            {
                                messageListNew.Add(GetString(Resource.String.tool_read_errors_failure));
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
                            PrintResults(messageListNew, resultSets);
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionText = String.Empty;
                        if (!AbortEdiabasJob())
                        {
                            exceptionText = EdiabasNet.GetExceptionText(ex, false, false);
                        }

                        messageListNew.Add(exceptionText);

                        if (ActivityCommon.IsCommunicationError(exceptionText))
                        {
                            _instanceData.CommErrorsCount++;
                        }
                        _runContinuous = false;
                    }

                    bool listChanged = false;
                    if (messageListCurrent == null || messageListCurrent.Count != messageListNew.Count)
                    {
                        listChanged = true;
                    }
                    else
                    {
                        if (!messageListNew.SequenceEqual(messageListCurrent))
                        {
                            listChanged = true;
                        }
                    }

                    if (listChanged)
                    {
                        List<string> messageListTemp;
                        lock (messageListLock)
                        {
                            messageListCurrent = new List<string>(messageListNew);
                            messageListTemp = messageListCurrent;
                        }

                        if (lastUpdateTime != null)
                        {
                            while (Stopwatch.GetTimestamp() - lastUpdateTime < UpdateDisplayDelay * ActivityCommon.TickResolMs)
                            {
                                Thread.Sleep(10);
                                if (!_runContinuous)
                                {
                                    break;
                                }
                            }
                        }

                        lastUpdateTime = Stopwatch.GetTimestamp();
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }

                            List<string> messageListLocal = null;
                            lock (messageListLock)
                            {
                                messageListLocal = new List<string>(messageListTemp);
                            }

                            _infoListAdapter.Items.Clear();
                            foreach (string message in messageListLocal)
                            {
                                _infoListAdapter.Items.Add(new TableResultItem(message, null));
                            }

                            _infoListAdapter.NotifyDataSetChanged();
                            UpdateDisplay();
                        });
                    }

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
                        _jobThread?.Join();
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

                                case EdiabasNet.ResultType.TypeQ:  // 64 bit
                                    resultText = string.Format(Culture, "D: {0} 0x{1:X016}", value, (UInt64)value);
                                    break;

                                case EdiabasNet.ResultType.TypeLL:  // 64 bit signed
                                    resultText = string.Format(Culture, "L: {0} 0x{1:X016}", value, (UInt64)value);
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
                            UpdateOptionsMenu();
                            UpdateDisplay();
                        }
                    }
                    break;

                case UsbManager.ActionUsbDeviceDetached:
                    if (_activityActive)
                    {
                        if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Ftdi)
                        {
                            UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                            if (usbDevice != null && EdFtdiInterface.IsValidUsbDevice(usbDevice))
                            {
                                EdiabasClose();
                                UpdateOptionsMenu();
                                UpdateDisplay();
                            }
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
                _backgroundColor = ActivityCommon.GetStyleColor(context, Android.Resource.Attribute.ColorBackground);
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

                string name = item.Name;
                if (!string.IsNullOrEmpty(item.InitialArgs))
                {
                    name += " " + item.InitialArgs;
                }
                textName.Text = name;

                List<string> commentList = item.CommentsTrans ?? item.Comments;
                if (commentList.Count > 0)
                {
                    StringBuilder stringBuilderComments = new StringBuilder();
                    foreach (string comment in commentList)
                    {
                        AppendSbText(stringBuilderComments, comment);
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

        public class ResultSelectListAdapter : BaseAdapter<ExtraInfo>
        {
            public delegate void CheckChangedEventHandler(ExtraInfo extraInfo);
            public event CheckChangedEventHandler CheckChanged;

            public delegate void GroupChangedEventHandler(ExtraInfo extraInfo);
            public event GroupChangedEventHandler GroupChanged;

            private readonly List<ExtraInfo> _items;
            private readonly List<ExtraInfo> _itemsVisible;
            public List<ExtraInfo> Items => _items;
            public List<ExtraInfo> ItemsVisible => _itemsVisible;
            private readonly Android.App.Activity _context;
            private readonly Android.Graphics.Color _backgroundColor;
            private bool _ignoreCheckEvent;

            public ResultSelectListAdapter(Android.App.Activity context)
            {
                _context = context;
                _items = new List<ExtraInfo>();
                _itemsVisible = new List<ExtraInfo>();
                _backgroundColor = ActivityCommon.GetStyleColor(context, Android.Resource.Attribute.ColorBackground);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override ExtraInfo this[int position] => _itemsVisible[position];

            public override int Count => _itemsVisible.Count;

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                ExtraInfo item = _itemsVisible[position];

                View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ediabas_result_list, null);
                view.SetBackgroundColor(_backgroundColor);

                CheckBox checkBoxGroupSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxGroupSelect);

                ViewStates viewStateGroup = ViewStates.Gone;
                if (item.GroupVisible)
                {
                    viewStateGroup = ViewStates.Visible;
                }
                else if (item.GroupId.HasValue)
                {
                    viewStateGroup = ViewStates.Invisible;
                }

                _ignoreCheckEvent = true;
                checkBoxGroupSelect.Visibility = viewStateGroup;
                checkBoxGroupSelect.Checked = item.GroupSelected;
                _ignoreCheckEvent = false;

                checkBoxGroupSelect.Tag = new TagInfo(item);
                checkBoxGroupSelect.CheckedChange -= OnGroupChanged;
                checkBoxGroupSelect.CheckedChange += OnGroupChanged;

                CheckBox checkBoxSelect = view.FindViewById<CheckBox>(Resource.Id.checkBoxResultSelect);
                _ignoreCheckEvent = true;
                checkBoxSelect.Checked = item.Selected;
                checkBoxSelect.Enabled = !item.GroupVisible || (item.GroupVisible && item.Selected);
                checkBoxSelect.Visibility = item.CheckVisible ? ViewStates.Visible : ViewStates.Gone;
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
                        AppendSbText(stringBuilderComments, comment);
                    }
                    textResultDesc.Text = stringBuilderComments.ToString();
                }
                else
                {
                    textResultDesc.Text = " ";
                }

                return view;
            }

            public override void NotifyDataSetChanged()
            {
                UpdateGroupList();
                base.NotifyDataSetChanged();
            }

            private void OnCheckChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = sender as CheckBox;
                    if (checkBox?.Tag is TagInfo tagInfo)
                    {
                        if (tagInfo.Info.Selected != args.IsChecked)
                        {
                            if (tagInfo.Info.GroupId.HasValue && tagInfo.Info.GroupVisible && !args.IsChecked)
                            {
                                DeselectGroup(tagInfo.Info.GroupId.Value);
                            }

                            tagInfo.Info.Selected = args.IsChecked;
                            CheckChanged?.Invoke(tagInfo.Info);
                            NotifyDataSetChanged();
                        }
                    }
                }
            }

            private void OnGroupChanged(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                if (!_ignoreCheckEvent)
                {
                    CheckBox checkBox = sender as CheckBox;
                    if (checkBox?.Tag is TagInfo tagInfo)
                    {
                        if (tagInfo.Info.GroupSelected != args.IsChecked)
                        {
                            tagInfo.Info.GroupSelected = args.IsChecked;
                            GroupChanged?.Invoke(tagInfo.Info);
                            NotifyDataSetChanged();
                        }
                    }
                }
            }

            private void UpdateGroupList()
            {
                HashSet<int> visibleGroups = new HashSet<int>();
                HashSet<int> checkedGroups = new HashSet<int>();
                foreach (ExtraInfo extraInfo in _items)
                {
                    if (extraInfo.GroupId.HasValue && extraInfo.GroupVisible && extraInfo.GroupSelected)
                    {
                        visibleGroups.Add(extraInfo.GroupId.Value);
                    }

                    if (extraInfo.GroupId.HasValue && extraInfo.ItemSelected)
                    {
                        checkedGroups.Add(extraInfo.GroupId.Value);
                    }
                }

                _itemsVisible.Clear();
                foreach (ExtraInfo extraInfo in _items)
                {
                    bool itemVisible = true;
                    if (extraInfo.GroupId.HasValue && !extraInfo.GroupVisible)
                    {
                        if (!visibleGroups.Contains(extraInfo.GroupId.Value))
                        {
                            itemVisible = false;
                        }
                    }

                    if (!extraInfo.ItemVisible && !extraInfo.GroupVisible)
                    {
                        itemVisible = false;
                    }

                    if (itemVisible)
                    {
                        _itemsVisible.Add(extraInfo);
                    }

                    if (extraInfo.GroupId.HasValue && extraInfo.GroupVisible)
                    {
                        extraInfo.Selected = checkedGroups.Contains(extraInfo.GroupId.Value);
                    }
                }
            }

            private void DeselectGroup(int groupId)
            {
                foreach (ExtraInfo extraInfo in _items)
                {
                    if (extraInfo.GroupId.HasValue && !extraInfo.GroupVisible)
                    {
                        if (extraInfo.GroupId.Value == groupId)
                        {
                            extraInfo.Selected = false;
                        }
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
