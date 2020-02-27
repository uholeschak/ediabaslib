//#define APP_USB_FILTER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using Base62;
using BmwDeepObd.FilePicker;
using BmwFileReader;
using EdiabasLib;
using Java.Interop;
using Mono.CSharp;
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

#if APP_USB_FILTER
[assembly: Android.App.UsesFeature("android.hardware.usb.host")]
#endif

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name", MainLauncher = false,
            LaunchMode = LaunchMode.SingleTop,
            UiOptions=UiOptions.SplitActionBarWhenNarrow,
            ConfigurationChanges = ConfigChanges.KeyboardHidden |
                ConfigChanges.Orientation |
                ConfigChanges.ScreenSize)]
    [Android.App.MetaData("android.support.UI_OPTIONS", Value = "splitActionBarWhenNarrow")]
#if APP_USB_FILTER
    [Android.App.IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Android.App.MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
#endif
    public class ActivityMain : BaseActivity, TabLayout.IOnTabSelectedListener
    {
        private enum ActivityRequest
        {
            RequestSelectDevice,
            RequestAdapterConfig,
            RequestSelectConfig,
            RequestXmlTool,
            RequestEdiabasTool,
            RequestYandexKey,
            RequestGlobalSettings,
            RequestEditConfig,
            RequestEditXml,
        }

        public enum LastAppState
        {
            Init,
            Compile,
            TabsCreated,
            Stopped,
        }

        private class DownloadInfo
        {
            public DownloadInfo(string downloadDir, string targetDir, XElement infoXml = null)
            {
                DownloadDir = downloadDir;
                TargetDir = targetDir;
                InfoXml = infoXml;
            }

            public string TargetDir { get; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public string DownloadDir { get; }

            public XElement InfoXml { get; }
        }

        class ConnectButtonInfo
        {
            public ToggleButton Button { get; set; }
            public bool Enabled { get; set; }
            public bool Checked { get; set; }
        }

        public class InstanceData
        {
            public InstanceData()
            {
                LastLocale = string.Empty;
                LastAppState = LastAppState.Init;
                AppDataPath = string.Empty;
                EcuPath = string.Empty;
                VagPath = string.Empty;
                TraceActive = true;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                ConfigFileName = string.Empty;
                CheckCpuUsage = true;
                VerifyEcuFiles = true;
            }

            public string LastLocale { get; set; }
            public ActivityCommon.ThemeType LastThemeType { get; set; }
            public LastAppState LastAppState { get; set; }
            public string AppDataPath { get; set; }
            public string EcuPath { get; set; }
            public string VagPath { get; set; }
            public string BmwPath { get; set; }
            public bool UserEcuFiles { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool DataLogActive { get; set; }
            public bool DataLogAppend { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string ConfigFileName { get; set; }
            public int LastVersionCode { get; set; }
            public bool StorageRequirementsAccepted { get; set; }
            public bool BatteryWarningShown { get; set; }
            public bool ConfigMatchVehicleShown { get; set; }
            public bool DataLogTemporaryShown { get; set; }
            public bool CheckCpuUsage { get; set; }
            public bool VerifyEcuFiles { get; set; }
            public bool CommErrorsOccured { get; set; }
            public bool StorageAccessGranted { get; set; }
            public bool AutoStart { get; set; }
            public bool VagInfoShown { get; set; }
            public string DataLogDir { get; set; }
            public string TraceDir { get; set; }
            public bool UpdateAvailable { get; set; }
            public int UpdateVersionCode { get; set; }
            public string UpdateMessage { get; set; }
            public long UpdateCheckTime { get; set; }
            public int UpdateSkipVersion { get; set; }
            public string XmlEditorPackageName { get; set; }
            public string XmlEditorClassName { get; set; }

            public ActivityCommon.InterfaceType SelectedInterface { get; set; }
        }

        private const string SharedAppName = ActivityCommon.AppNameSpace;
        private const string AppFolderName = ActivityCommon.AppNameSpace;
        private const string EcuDownloadUrl = @"https://www.holeschak.de/BmwDeepObd/Obb.php";
        private const long EcuExtractSize = 2500000000;         // extracted ecu files size
        private const string InfoXmlName = "ObbInfo.xml";
        private const string ContentFileName = "Content.xml";
        private const int CpuLoadCritical = 70;
        private const int AutoHideTimeout = 3000;
        private const int RequestPermissionExternalStorage = 0;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage
        };

        public const string ExtraStopComm = "stop_communication";
        public const string ExtraShowTitle = "show_title";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        public static bool StoreXmlEditor = Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1;
        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _updateOptionsMenu;
        private bool _stopCommRequest;
        private ActivityCommon.AutoConnectType _connectTypeRequest;
        private bool _backPressed;
        private long _lastBackPressedTime;
        private int _currentVersionCode;
        private bool _activityActive;
        private bool _onResumeExecuted;
        private bool _createTabsPending;
        private bool _ignoreTabsChange;
        private bool _compileCodePending;
        private ActivityCommon _activityCommon;
        public bool _autoHideStarted;
        public long _autoHideStartTime;
        private Timer _autoHideTimer;
        private Handler _updateHandler;
        private TabLayout _tabLayout;
        private ViewPager _viewPager;
        private TabsFragmentPagerAdapter _fragmentPagerAdapter;
        private readonly ConnectButtonInfo _connectButtonInfo = new ConnectButtonInfo();
        private ImageView _imageBackground;
        private HttpClient _httpClient;
        private CustomProgressDialog _downloadProgress;
        private CustomProgressDialog _compileProgress;
        private bool _extractZipCanceled;
        private string _obbFileName;
        private AlertDialog _startAlertDialog;
        private AlertDialog _configSelectAlertDialog;
        private AlertDialog _downloadEcuAlertDialog;
        private bool _translateActive;
        private List<string> _translationList;
        private List<string> _translatedList;

        public ActivityCommon ActivityCommon => _activityCommon;

        private string ManufacturerEcuDirName
        {
            get
            {
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Bmw:
                        return Path.Combine(ActivityCommon.EcuBaseDir, ActivityCommon.EcuDirNameBmw);
                }
                return Path.Combine(ActivityCommon.EcuBaseDir, ActivityCommon.EcuDirNameVag);
            }
        }

        public void OnTabReselected(TabLayout.Tab tab)
        {
        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            if (_ignoreTabsChange)
            {
                return;
            }
            UpdateSelectedPage();
            UpdateDisplay();
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
            ClearPage(tab.Position);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            GetThemeSettings();
            SetTheme(ActivityCommon.SelectedThemeId);
            base.OnCreate(savedInstanceState);
            _allowTitleHiding = false;
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SetContentView(Resource.Layout.main);

            _fragmentPagerAdapter = new TabsFragmentPagerAdapter(SupportFragmentManager);
            _viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            _viewPager.Adapter = _fragmentPagerAdapter;
            _tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            _tabLayout.SetupWithViewPager(_viewPager);
            _tabLayout.AddOnTabSelectedListener(this);
            _tabLayout.Visibility = ViewStates.Gone;

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            ActivityCommon.ActivityMainCurrent = this;
            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    UpdateCheck();
                }
            }, BroadcastReceived);
            if (_activityRecreated && _instanceData != null)
            {
                _activityCommon.SelectedInterface = _instanceData.SelectedInterface;
            }

            GetSettings();
            _activityCommon.UpdateRegisterInternetCellular();
            _activityCommon.SetPreferredNetworkInterface();

            StoreLastAppState(LastAppState.Init);

            _updateHandler = new Handler();

            _imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);

            if (_httpClient == null)
            {
                _httpClient = new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                });
            }

            _stopCommRequest = Intent.GetBooleanExtra(ExtraStopComm, false);
            bool showTitleRequest = Intent.GetBooleanExtra(ExtraShowTitle, false);
            if (showTitleRequest)
            {
                _instanceDataBase.ActionBarVisible = true;
            }

            _connectTypeRequest = ActivityCommon.AutoConnectHandling;
            if (ActivityCommon.CommActive)
            {
                ConnectEdiabasEvents();
            }
            else
            {
                if (!_activityRecreated)
                {
                    ActivityCommon.BtInitiallyEnabled = _activityCommon.IsBluetoothEnabled();
                }
            }
        }

        void CreateActionBarTabs()
        {
            if (!_activityActive)
            {
                _createTabsPending = true;
                return;
            }
            _createTabsPending = false;
            int pageIndex = 0;
            if (ActivityCommon.CommActive)
            {
                _ignoreTabsChange = true;
                // get last active tab
                int i = 0;
                foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                {
                    if (pageInfo == ActivityCommon.EdiabasThread.JobPageInfo)
                    {
                        pageIndex = i;
                        break;
                    }
                    i++;
                }
            }
            _fragmentPagerAdapter.ClearPages();
            _fragmentPagerAdapter.NotifyDataSetChanged();
            _tabLayout.Visibility = ViewStates.Gone;

            int index = 0;
            foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
            {
                int resourceId = Resource.Layout.tab_list;
                if (pageInfo.ErrorsInfo != null)
                {
                    resourceId = Resource.Layout.tab_errors;
                }
                else if (pageInfo.JobActivate)
                {
                    resourceId = Resource.Layout.tab_activate;
                }

                Fragment fragmentPage = TabContentFragment.NewInstance(resourceId, index);
                pageInfo.InfoObject = fragmentPage;
                _fragmentPagerAdapter.AddPage(fragmentPage, GetPageString(pageInfo, pageInfo.Name));
                index++;
            }
            _tabLayout.Visibility = (ActivityCommon.JobReader.PageList.Count > 0) ? ViewStates.Visible : ViewStates.Gone;
            _fragmentPagerAdapter.NotifyDataSetChanged();
            if (_tabLayout.TabCount > pageIndex)
            {
                _tabLayout.GetTabAt(pageIndex).Select();
            }
            _ignoreTabsChange = false;
            UpdateDisplay();
            StoreLastAppState(LastAppState.TabsCreated);
            if (_stopCommRequest)
            {
                _stopCommRequest = false;
                if (ActivityCommon.CommActive)
                {
                    StopEdiabasThread(false);
                }
            }
            else
            {
                switch (_connectTypeRequest)
                {
                    case ActivityCommon.AutoConnectType.Connect:
                    case ActivityCommon.AutoConnectType.ConnectClose:
                        if (ActivityCommon.JobReader.PageList.Count > 0 &&
                            !ActivityCommon.CommActive && _activityCommon.IsInterfaceAvailable())
                        {
                            ButtonConnectClick(_connectButtonInfo.Button, new EventArgs());
                            if (UseCommService() && ActivityCommon.SendDataBroadcast && ActivityCommon.CommActive &&
                                _connectTypeRequest == ActivityCommon.AutoConnectType.ConnectClose)
                            {
                                Finish();
                            }
                        }
                        break;
                }
                _connectTypeRequest = ActivityCommon.AutoConnectType.Offline;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _instanceData.SelectedInterface = _activityCommon.SelectedInterface;
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _activityCommon?.StartMtcService();
            if (_instanceData.StorageAccessGranted)
            {
                _activityCommon?.RequestUsbPermission(null);
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            _activityCommon?.StopMtcService();
            StoreLastAppState(LastAppState.Stopped);
        }

        protected override void OnResume()
        {
            base.OnResume();

            bool firstStart = !_onResumeExecuted;
            if (!_onResumeExecuted)
            {
                _onResumeExecuted = true;
                RequestStoragePermissions();
            }
            _activityActive = true;
            _activityCommon.MtcBtDisconnectWarnShown = false;
            UpdateLockState();
            if (_compileCodePending)
            {
                _updateHandler.Post(CompileCode);
            }
            if (_createTabsPending)
            {
                _updateHandler.Post(CreateActionBarTabs);
            }
            if (_startAlertDialog == null)
            {
                HandleStartDialogs(firstStart);
            }

            if (ActivityCommon.AutoHideTitleBar)
            {
                if (_autoHideTimer == null)
                {
                    _autoHideTimer = new Timer(state =>
                    {
                        RunOnUiThread(() =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }

                            if (_autoHideStarted)
                            {
                                if (Stopwatch.GetTimestamp() - _autoHideStartTime >= AutoHideTimeout * ActivityCommon.TickResolMs)
                                {
                                    _autoHideStarted = false;
                                    if (ActivityCommon.CommActive && SupportActionBar.IsShowing)
                                    {
                                        SupportActionBar.Hide();
                                    }
                                }
                            }
                            else
                            {
                                if (SupportActionBar.IsShowing && ActivityCommon.CommActive)
                                {
                                    _autoHideStartTime = Stopwatch.GetTimestamp();
                                    _autoHideStarted = true;
                                }
                            }
                        });
                    }, null, 500, 500);
                }
            }

            UpdateOptionsMenu();
            UpdateDisplay();
        }

        protected override void OnPause()
        {
            base.OnPause();

            _activityActive = false;
            UpdateLockState();
            if (!UseCommService())
            {
                StopEdiabasThread(false);
            }

            if (_autoHideTimer != null)
            {
                _autoHideTimer.Dispose();
                _autoHideTimer = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ActivityCommon.ActivityMainCurrent = null;
            if (!UseCommService())
            {
                StopEdiabasThread(true);
            }
            DisconnectEdiabasEvents();
            if (_httpClient != null)
            {
                try
                {
                    _httpClient.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
                _httpClient = null;
            }
            _extractZipCanceled = true;
            if (_activityCommon != null)
            {
                _activityCommon.UnRegisterInternetCellular();
                _activityCommon.Dispose();
                _activityCommon = null;
            }
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
            if (!ActivityCommon.DoubleClickForAppExit)
            {
                _backPressed = false;
                _instanceData.CheckCpuUsage = true;
                base.OnBackPressed();
                return;
            }
            if (_backPressed)
            {
                _backPressed = false;
                if (Stopwatch.GetTimestamp() - _lastBackPressedTime < 2000 * ActivityCommon.TickResolMs)
                {
                    _instanceData.CheckCpuUsage = true;
                    base.OnBackPressed();
                    return;
                }
            }
            if (!_backPressed)
            {
                _backPressed = true;
                _lastBackPressedTime = Stopwatch.GetTimestamp();
                Toast.MakeText(this, GetString(Resource.String.back_button_twice_for_exit), ToastLength.Short).Show();
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            _updateHandler?.Post(() => { UpdateDisplay(true); });
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            ActivityCommon.ActivityStartedFromMain = false;
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _instanceData.DeviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _instanceData.DeviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        bool callAdapterConfig = data.Extras.GetBoolean(DeviceListActivity.ExtraCallAdapterConfig, false);
                        UpdateOptionsMenu();
                        if (callAdapterConfig)
                        {
                            AdapterConfig();
                        }
                        else if (_instanceData.AutoStart)
                        {
                            ButtonConnectClick(_connectButtonInfo.Button, new EventArgs());
                        }
                    }
                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestAdapterConfig:
                    if (data != null && resultCode == Android.App.Result.Ok)
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

                case ActivityRequest.RequestSelectConfig:
                    // When FilePickerActivity returns with a file
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        _instanceData.ConfigFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        ReadConfigFile();
                        UpdateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestXmlTool:
                    // When XML tool returns with a file
                    _activityCommon.SetPreferredNetworkInterface();
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        ActivityCommon.InterfaceType interfaceType = (ActivityCommon.InterfaceType)data.Extras.GetInt(XmlToolActivity.ExtraInterface, (int)ActivityCommon.InterfaceType.None);
                        if (interfaceType != ActivityCommon.InterfaceType.None)
                        {
                            _activityCommon.SelectedInterface = interfaceType;
                            _instanceData.DeviceName = data.Extras.GetString(XmlToolActivity.ExtraDeviceName);
                            _instanceData.DeviceAddress = data.Extras.GetString(XmlToolActivity.ExtraDeviceAddress);
                            _activityCommon.SelectedEnetIp = data.Extras.GetString(XmlToolActivity.ExtraEnetIp);
                        }
                        _instanceData.ConfigFileName = data.Extras.GetString(XmlToolActivity.ExtraFileName);
                        ReadConfigFile();
                        UpdateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestEdiabasTool:
                    _activityCommon.SetPreferredNetworkInterface();
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey);
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestGlobalSettings:
                    _activityCommon.SetPreferredNetworkInterface();
                    if (_instanceData.LastThemeType != ActivityCommon.SelectedTheme ||
                        string.Compare(_instanceData.LastLocale ?? string.Empty, ActivityCommon.SelectedLocale ?? string.Empty, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        StoreSettings();
                        Finish();
                        StartActivity(Intent);
                        break;
                    }
                    UpdateCheck();
                    StoreSettings();
                    UpdateDirectories();
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    CheckForEcuFiles();
                    break;

                case ActivityRequest.RequestEditConfig:
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        string fileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            StartEditXml(fileName);
                        }
                    }
                    break;

                case ActivityRequest.RequestEditXml:
                    ReadConfigFile();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return base.OnCreateOptionsMenu(menu);
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
            bool commActive = ActivityCommon.CommActive;
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable();

            IMenuItem actionProviderConnect = menu.FindItem(Resource.Id.menu_action_provider_connect);
            if (actionProviderConnect != null)
            {
                Android.Support.V4.View.ActionProvider actionProvider = MenuItemCompat.GetActionProvider(actionProviderConnect);
                if (actionProvider == null)
                {
                    MenuItemCompat.SetActionProvider(actionProviderConnect, new ConnectActionProvider(this));
                }
            }

            IMenuItem manufacturerMenu = menu.FindItem(Resource.Id.menu_manufacturer);
            if (manufacturerMenu != null)
            {
                manufacturerMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_manufacturer), _activityCommon.ManufacturerName()));
                manufacturerMenu.SetEnabled(!commActive);
            }

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_adapter), _instanceData.DeviceName));
                scanMenu.SetEnabled(interfaceAvailable && !commActive);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem adapterConfigMenu = menu.FindItem(Resource.Id.menu_adapter_config);
            if (adapterConfigMenu != null)
            {
                adapterConfigMenu.SetEnabled(interfaceAvailable && !commActive);
                adapterConfigMenu.SetVisible(_activityCommon.AllowAdapterConfig(_instanceData.DeviceAddress));
            }

            IMenuItem enetIpMenu = menu.FindItem(Resource.Id.menu_enet_ip);
            if (enetIpMenu != null)
            {
                enetIpMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_enet_ip),
                    string.IsNullOrEmpty(_activityCommon.SelectedEnetIp) ? GetString(Resource.String.select_enet_ip_auto) : _activityCommon.SelectedEnetIp));
                enetIpMenu.SetEnabled(interfaceAvailable && !commActive);
                enetIpMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet);
            }

            IMenuItem cfgSubmenu = menu.FindItem(Resource.Id.menu_cfg_submenu);
            cfgSubmenu?.SetEnabled(!commActive);

            IMenuItem cfgSelMenu = menu.FindItem(Resource.Id.menu_cfg_sel);
            if (cfgSelMenu != null)
            {
                if (!string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    // ReSharper disable once ConstantNullCoalescingCondition
                    string fileName = Path.GetFileNameWithoutExtension(_instanceData.ConfigFileName) ?? string.Empty;
                    cfgSelMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_cfg_sel), fileName));
                }
                else
                {
                    cfgSelMenu.SetTitle(Resource.String.menu_cfg_sel);
                }
                cfgSelMenu.SetEnabled(!commActive);
            }

            IMenuItem cfgEditMenu = menu.FindItem(Resource.Id.menu_cfg_edit);
            cfgEditMenu?.SetEnabled(!commActive && !string.IsNullOrEmpty(_instanceData.ConfigFileName));

            IMenuItem cfgPagesEditMenu = menu.FindItem(Resource.Id.menu_cfg_pages_edit);
            cfgPagesEditMenu?.SetEnabled(!commActive && !string.IsNullOrEmpty(_instanceData.ConfigFileName) && !string.IsNullOrEmpty(ActivityCommon.JobReader.XmlFileNamePages));

            IMenuItem cfgPageEditMenu = menu.FindItem(Resource.Id.menu_cfg_page_edit);
            cfgPageEditMenu?.SetEnabled(!commActive && !string.IsNullOrEmpty(GetSelectedPage()?.XmlFileName));

            IMenuItem cfgSelectEditMenu = menu.FindItem(Resource.Id.menu_cfg_select_edit);
            cfgSelectEditMenu?.SetEnabled(!commActive);

            IMenuItem cfgEditResetMenu = menu.FindItem(Resource.Id.menu_cfg_edit_reset);
            if (cfgEditResetMenu != null)
            {
                cfgEditResetMenu.SetEnabled(!string.IsNullOrEmpty(_instanceData.XmlEditorPackageName) && !string.IsNullOrEmpty(_instanceData.XmlEditorClassName));
                cfgEditResetMenu.SetVisible(StoreXmlEditor);
            }

            IMenuItem cfgCloseMenu = menu.FindItem(Resource.Id.menu_cfg_close);
            cfgCloseMenu?.SetEnabled(!commActive && !string.IsNullOrEmpty(_instanceData.ConfigFileName));

            IMenuItem xmlToolMenu = menu.FindItem(Resource.Id.menu_xml_tool);
            xmlToolMenu?.SetEnabled(!commActive);

            IMenuItem ediabasToolMenu = menu.FindItem(Resource.Id.menu_ediabas_tool);
            ediabasToolMenu?.SetEnabled(!commActive);

            IMenuItem downloadEcu = menu.FindItem(Resource.Id.menu_download_ecu);
            if (downloadEcu != null)
            {
                downloadEcu.SetTitle(Resource.String.menu_extract_ecu);
                downloadEcu.SetEnabled(!commActive);
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            logSubMenu?.SetEnabled(interfaceAvailable && !commActive);

            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir));

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

            IMenuItem globalSettingsMenu = menu.FindItem(Resource.Id.menu_global_settings);
            globalSettingsMenu?.SetEnabled(!commActive);

            IMenuItem infoSubMenu = menu.FindItem(Resource.Id.menu_info);
            infoSubMenu?.SetEnabled(!commActive);

            IMenuItem exitSubMenu = menu.FindItem(Resource.Id.menu_exit);
            if (exitSubMenu != null)
            {
#if DEBUG
                exitSubMenu.SetVisible(true);
#else
                exitSubMenu.SetVisible(false);
#endif
                exitSubMenu.SetEnabled(!commActive);
            }

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_manufacturer:
                    SelectManufacturerInfo();
                    return true;

                case Resource.Id.menu_scan:
                    _instanceData.AutoStart = false;
                    if (_activityCommon.SelectBluetoothDevice((int) ActivityRequest.RequestSelectDevice, _instanceData.AppDataPath))
                    {
                        ActivityCommon.ActivityStartedFromMain = true;
                    }
                    return true;

                case Resource.Id.menu_adapter_config:
                    AdapterConfig();
                    return true;

                case Resource.Id.menu_enet_ip:
                    EnetIpConfig();
                    return true;

                case Resource.Id.menu_cfg_sel:
                    SelectConfigFile();
                    return true;

                case Resource.Id.menu_cfg_edit:
                    StartEditXml(_instanceData.ConfigFileName);
                    return true;

                case Resource.Id.menu_cfg_pages_edit:
                    StartEditXml(ActivityCommon.JobReader.XmlFileNamePages);
                    return true;

                case Resource.Id.menu_cfg_page_edit:
                    StartEditXml(GetSelectedPage()?.XmlFileName);
                    return true;

                case Resource.Id.menu_cfg_select_edit:
                    EditConfigFileIntent();
                    return true;

                case Resource.Id.menu_cfg_edit_reset:
                    _instanceData.XmlEditorPackageName = string.Empty;
                    _instanceData.XmlEditorClassName = string.Empty;
                    UpdateOptionsMenu();
                    return true;

                case Resource.Id.menu_cfg_close:
                    ClearConfiguration();
                    return true;

                case Resource.Id.menu_xml_tool:
                    StartXmlTool();
                    return true;

                case Resource.Id.menu_ediabas_tool:
                    StartEdiabasTool();
                    return true;

                case Resource.Id.menu_download_ecu:
                    DownloadEcuFiles(true);
                    return true;

                case Resource.Id.menu_submenu_log:
                    SelectDataLogging();
                    return true;

                case Resource.Id.menu_send_trace:
                    SendTraceFileAlways((sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
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
                    return true;

                case Resource.Id.menu_translation_yandex_key:
                    EditYandexKey();
                    return true;

                case Resource.Id.menu_translation_clear_cache:
                    _activityCommon.ClearTranslationCache();
                    _translationList = null;
                    _translatedList = null;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    return true;

                case Resource.Id.menu_global_settings:
                    EditGlobalSettings();
                    return true;

                case Resource.Id.menu_submenu_help:
                    _activityCommon.ShowWifiConnectedWarning(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://github.com/uholeschak/ediabaslib/blob/master/docs/Deep_OBD_for_BMW_and_VAG.md")));
                    });
                    return true;

                case Resource.Id.menu_info:
                {
                    string message = string.Format(GetString(Resource.String.app_info_message),
                        _activityCommon.GetPackageInfo()?.VersionName ?? string.Empty , ActivityCommon.AppId);
                    new AlertDialog.Builder(this)
                        .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                        .SetPositiveButton(Resource.String.button_copy, (sender, args) =>
                        {
                            _activityCommon.SetClipboardText(message);
                        })
                        .SetCancelable(true)
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_info)
                        .Show();
                    return true;
                }

                case Resource.Id.menu_exit:
                    OnDestroy();
                    System.Environment.Exit(0);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (_activityCommon == null)
            {
                return;
            }
            switch (requestCode)
            {
                case RequestPermissionExternalStorage:
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StoragePermissonGranted();
                        break;
                    }
                    Toast.MakeText(this, GetString(Resource.String.access_denied_ext_storage), ToastLength.Long).Show();
                    Finish();
                    break;
            }
        }

        protected void ButtonConnectClick(object sender, EventArgs e)
        {
            if (!CheckForEcuFiles())
            {
                UpdateDisplay();
                return;
            }
            _instanceData.AutoStart = false;
            if (string.IsNullOrEmpty(_instanceData.DeviceAddress))
            {
                if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, _instanceData.AppDataPath, (s, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        _instanceData.AutoStart = true;
                    }))
                {
                    UpdateDisplay();
                    return;
                }
                ActivityCommon.ActivityStartedFromMain = true;
            }
            if (!ActivityCommon.CommActive && _connectTypeRequest == ActivityCommon.AutoConnectType.Offline)
            {
                if (_activityCommon.ShowConnectWarning(retry =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (retry)
                    {
                        ButtonConnectClick(sender, e);
                    }
                }))
                {
                    UpdateDisplay();
                    return;
                }
            }

            if (!ActivityCommon.CommActive)
            {
                if (_activityCommon.InitReaderThread(_instanceData.BmwPath, _instanceData.VagPath, result =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (result)
                    {
                        ButtonConnectClick(sender, e);
                    }
                }))
                {
                    return;
                }
            }

            if (ActivityCommon.CommActive)
            {
                StopEdiabasThread(false);
            }
            else
            {
                if (StartEdiabasThread())
                {
                    UpdateSelectedPage();
                }
            }
            UpdateDisplay();
        }

        [Export("onActiveClick")]
        // ReSharper disable once UnusedMember.Global
        public void OnActiveClick(View v)
        {
            if (ActivityCommon.EdiabasThread == null)
            {
                return;
            }
            ToggleButton button = v.FindViewById<ToggleButton>(Resource.Id.button_active);
            ActivityCommon.EdiabasThread.CommActive = button.Checked;
        }

        [Export("onErrorResetClick")]
        // ReSharper disable once UnusedMember.Global
        public void OnErrorResetClick(View v)
        {
            if (!ActivityCommon.CommActive)
            {
                return;
            }
            View parent = v.Parent as View;
            parent = parent?.Parent as View;
            ListView listViewResult = parent?.FindViewById<ListView>(Resource.Id.resultList);
            ResultListAdapter resultListAdapter = (ResultListAdapter) listViewResult?.Adapter;
            if (resultListAdapter != null)
            {
                List<string> errorResetList =
                    (from resultItem in resultListAdapter.Items
                        let ecuName = resultItem.Tag as string
                        where ecuName != null && resultItem.CheckVisible && resultItem.Selected
                        select ecuName).ToList();
                lock (EdiabasThread.DataLock)
                {
                    ActivityCommon.EdiabasThread.ErrorResetList = errorResetList;
                }
            }
            UpdateDisplay();
        }

        [Export("onErrorResetAllClick")]
        // ReSharper disable once UnusedMember.Global
        public void OnErrorResetAllClick(View v)
        {
            if (!ActivityCommon.CommActive)
            {
                return;
            }
            string sgbdFunctional = ActivityCommon.EdiabasThread.JobPageInfo?.ErrorsInfo?.SgbdFunctional;
            if (!string.IsNullOrEmpty(sgbdFunctional))
            {
                View parent = v.Parent as View;
                parent = parent?.Parent as View;
                ListView listViewResult = parent?.FindViewById<ListView>(Resource.Id.resultList);
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult?.Adapter;
                if (resultListAdapter != null)
                {
                    bool changed = false;
                    foreach (TableResultItem resultItem in resultListAdapter.Items)
                    {
                        if (resultItem.CheckVisible)
                        {
                            if (!resultItem.Selected)
                            {
                                resultItem.Selected = true;
                                changed = true;
                            }
                        }
                    }

                    if (changed)
                    {
                        resultListAdapter.NotifyDataSetChanged();
                    }
                }
                lock (EdiabasThread.DataLock)
                {
                    ActivityCommon.EdiabasThread.ErrorResetSgbdFunc = sgbdFunctional;
                }
            }
            UpdateDisplay();
        }

        [Export("onErrorSelectClick")]
        // ReSharper disable once UnusedMember.Global
        public void OnErrorSelectClick(View v)
        {
            if (!ActivityCommon.CommActive)
            {
                return;
            }

            Button buttonErrorSelect = v.FindViewById<Button>(Resource.Id.button_error_select);
            bool select = buttonErrorSelect.Tag is Java.Lang.Boolean selectAll && selectAll.BooleanValue();

            View parent = v.Parent as View;
            parent = parent?.Parent as View;
            ListView listViewResult = parent?.FindViewById<ListView>(Resource.Id.resultList);
            ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult?.Adapter;
            if (resultListAdapter != null)
            {
                bool changed = false;
                foreach (TableResultItem resultItem in resultListAdapter.Items)
                {
                    if (resultItem.CheckVisible)
                    {
                        if (resultItem.Selected != select)
                        {
                            resultItem.Selected = select;
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    resultListAdapter.NotifyDataSetChanged();
                }
            }

            UpdateDisplay();
        }

        [Export("onCopyErrorsClick")]
        // ReSharper disable once UnusedMember.Global
        public void OnCopyErrorsClick(View v)
        {
            if (!ActivityCommon.CommActive)
            {
                return;
            }
            View parent = v.Parent as View;
            parent = parent?.Parent as View;
            ListView listViewResult = parent?.FindViewById<ListView>(Resource.Id.resultList);
            ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult?.Adapter;
            if (resultListAdapter != null)
            {
                StringBuilder sr = new StringBuilder();
                foreach (TableResultItem resultItem in resultListAdapter.Items)
                {
                    if (!string.IsNullOrEmpty(resultItem.Text1))
                    {
                        if (sr.Length > 0)
                        {
                            sr.Append("\r\n\r\n");
                        }
                        sr.Append(resultItem.Text1);
                    }
                }
                _activityCommon.SetClipboardText(sr.ToString());
            }
        }

        private void UpdateOptionsMenu()
        {
            _updateOptionsMenu = true;
        }

        private void HandleStartDialogs(bool firstStart)
        {
            if (firstStart)
            {
                UpdateCheck();
            }
            if (!_activityCommon.RequestInterfaceEnable((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                UpdateOptionsMenu();
                UpdateDisplay();
                if (firstStart)
                {
                    CheckForEcuFiles(true);
                }
            }))
            {
                if (firstStart)
                {
                    CheckForEcuFiles(true);
                }
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool UpdateCheck()
        {
            if (ActivityCommon.UpdateCheckDelay < 0)
            {
                _instanceData.UpdateCheckTime = DateTime.MinValue.Ticks;
                _instanceData.UpdateSkipVersion = -1;
                return false;
            }

            TimeSpan timeDiff = new TimeSpan(DateTime.Now.Ticks - _instanceData.UpdateCheckTime);
            if (timeDiff.Ticks < ActivityCommon.UpdateCheckDelay)
            {
                return false;
            }

            bool result = _activityCommon.UpdateCheck((success, updateAvailable, appVer, message) =>
            {
                if (success)
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _instanceData.UpdateCheckTime = DateTime.Now.Ticks;
                        StoreSettings();

                        if (appVer.HasValue)
                        {
                            _instanceData.UpdateAvailable = updateAvailable;
                            _instanceData.UpdateVersionCode = appVer.Value;
                            _instanceData.UpdateMessage = message;
                            DisplayUpdateInfo((sender, args) =>
                            {
                            });
                        }
                    });
                }
            }, _instanceData.UpdateSkipVersion);

            return result;
        }

        private bool UseCommService()
        {
            bool useService = true;
            if (_instanceData.DataLogActive)
            {
                if (ActivityCommon.LockTypeLogging == ActivityCommon.LockType.None)
                {
                    useService = false;
                }
            }
            else
            {
                if (ActivityCommon.LockTypeCommunication == ActivityCommon.LockType.None)
                {
                    useService = false;
                }
            }
            return useService;
        }

        private bool StartEdiabasThread()
        {
            _instanceData.AutoStart = false;
            _instanceData.CommErrorsOccured = false;
            try
            {
                if (ActivityCommon.EdiabasThread == null)
                {
                    ActivityCommon.EdiabasThread = new EdiabasThread(string.IsNullOrEmpty(ActivityCommon.JobReader.EcuPath) ? _instanceData.EcuPath : ActivityCommon.JobReader.EcuPath, _activityCommon, this);
                    ConnectEdiabasEvents();
                }
                string logDir = string.Empty;
                if (_instanceData.DataLogActive && !string.IsNullOrEmpty(ActivityCommon.JobReader.LogPath))
                {
                    logDir = Path.IsPathRooted(ActivityCommon.JobReader.LogPath) ? ActivityCommon.JobReader.LogPath : Path.Combine(_instanceData.AppDataPath, ActivityCommon.JobReader.LogPath);
                    try
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    catch (Exception)
                    {
                        logDir = string.Empty;
                    }
                }
                _instanceData.DataLogDir = logDir;

                _instanceData.TraceDir = null;
                if (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    _instanceData.TraceDir = Path.Combine(_instanceData.AppDataPath, "Log");
                }
                _translationList = null;
                _translatedList = null;

                JobReader.PageInfo pageInfo = GetSelectedPage();
                object connectParameter = null;
                if (pageInfo != null)
                {
                    string portName = string.Empty;
                    switch (_activityCommon.SelectedInterface)
                    {
                        case ActivityCommon.InterfaceType.Bluetooth:
                            portName = "BLUETOOTH:" + _instanceData.DeviceAddress;
                            connectParameter = new EdBluetoothInterface.ConnectParameterType(_activityCommon.NetworkData, _activityCommon.MtcBtService,
                                () => ActivityCommon.EdiabasThread.ActiveContext);
                            _activityCommon.ConnectMtcBtDevice(_instanceData.DeviceAddress);
                            break;

                        case ActivityCommon.InterfaceType.Enet:
                            connectParameter = new EdInterfaceEnet.ConnectParameterType(_activityCommon.NetworkData);
                            if (_activityCommon.Emulator)
                            {
                                // broadcast is not working with emulator
                                portName = ActivityCommon.EmulatorEnetIp;
                                break;
                            }
                            portName = string.IsNullOrEmpty(_activityCommon.SelectedEnetIp) ? "auto:all" : _activityCommon.SelectedEnetIp;
                            break;

                        case ActivityCommon.InterfaceType.ElmWifi:
                            portName = "ELM327WIFI";
                            connectParameter = new EdElmWifiInterface.ConnectParameterType(_activityCommon.NetworkData);
                            break;

                        case ActivityCommon.InterfaceType.DeepObdWifi:
                            portName = "DEEPOBDWIFI";
                            connectParameter = new EdCustomWiFiInterface.ConnectParameterType(_activityCommon.NetworkData, _activityCommon.MaWifi);
                            break;

                        case ActivityCommon.InterfaceType.Ftdi:
                            portName = "FTDI0";
                            connectParameter = new EdFtdiInterface.ConnectParameterType(_activityCommon.UsbManager);
                            break;
                    }
                    ActivityCommon.EdiabasThread.StartThread(portName, connectParameter, pageInfo, true,
                        _instanceData.VagPath, _instanceData.TraceDir, _instanceData.TraceAppend, _instanceData.DataLogDir, _instanceData.DataLogAppend);
                    if (UseCommService())
                    {
                        _activityCommon.StartForegroundService();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            UpdateLockState();
            UpdateOptionsMenu();
            return true;
        }

        private void StopEdiabasThread(bool wait)
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                try
                {
                    lock (ActivityCommon.GlobalLockObject)
                    {
                        if (ActivityCommon.EdiabasThread != null)
                        {
                            ActivityCommon.EdiabasThread.StopThread(wait);
                        }
                    }
                    if (wait)
                    {
                        _activityCommon?.StopForegroundService();
                        DisconnectEdiabasEvents();
                        lock (ActivityCommon.GlobalLockObject)
                        {
                            if (ActivityCommon.EdiabasThread != null)
                            {
                                ActivityCommon.EdiabasThread.Dispose();
                                ActivityCommon.EdiabasThread = null;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
            UpdateLockState();
            UpdateOptionsMenu();

            _autoHideStarted = false;
            SupportActionBar.Show();
        }

        private void ConnectEdiabasEvents()
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                ActivityCommon.EdiabasThread.DataUpdated += DataUpdated;
                ActivityCommon.EdiabasThread.PageChanged += PageChanged;
                ActivityCommon.EdiabasThread.ThreadTerminated += ThreadTerminated;
            }
        }

        private void DisconnectEdiabasEvents()
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                ActivityCommon.EdiabasThread.DataUpdated -= DataUpdated;
                ActivityCommon.EdiabasThread.PageChanged -= PageChanged;
                ActivityCommon.EdiabasThread.ThreadTerminated -= ThreadTerminated;
            }
        }

        private void UpdateLockState()
        {
            if (_activityCommon == null)
            {
                return;
            }
            if (!ActivityCommon.CommActive)
            {
                ActivityCommon.LockType lockType = ActivityCommon.LockType.None;
                if (_downloadProgress != null || _compileProgress != null)
                {
                    lockType = ActivityCommon.LockType.Cpu;
                }
                _activityCommon.SetLock(lockType);
            }
            else
            {
                ActivityCommon.LockType lockType = _instanceData.DataLogActive ? ActivityCommon.LockTypeLogging : ActivityCommon.LockTypeCommunication;
                if (!_activityActive)
                {
                    switch (lockType)
                    {
                        case ActivityCommon.LockType.ScreenDim:
                        case ActivityCommon.LockType.ScreenBright:
                            lockType = ActivityCommon.LockType.Cpu;
                            break;
                    }
                }
                if (_activityCommon.GetLock() != lockType)
                {   // unlock first
                    _activityCommon.SetLock(ActivityCommon.LockType.None);
                }
                _activityCommon.SetLock(lockType);
            }
        }

        public static string GetLocaleSetting(InstanceData instanceData = null)
        {
            try
            {
                if (ActivityCommon.SelectedLocale == null)
                {
                    ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                    ActivityCommon.SelectedLocale = prefs.GetString("Locale", string.Empty);
                }

                if (instanceData != null)
                {
                    instanceData.LastLocale = ActivityCommon.SelectedLocale;
                }
                return ActivityCommon.SelectedLocale;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void GetThemeSettings()
        {
            try
            {
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                ActivityCommon.SelectedTheme = (ActivityCommon.ThemeType)prefs.GetInt("Theme", (int)ActivityCommon.ThemeType.Dark);
                _instanceData.LastThemeType = ActivityCommon.SelectedTheme;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void GetSettings()
        {
            GetLocaleSetting(_instanceData);
            GetThemeSettings();

            try
            {
                _currentVersionCode = PackageManager.GetPackageInfo(PackageName, 0).VersionCode;
                _obbFileName = ExpansionDownloaderActivity.GetObbFilename(this);
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
#if false    // simulate settings reset
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.Clear();
                prefsEdit.Commit();
#endif
                _activityCommon.SelectedEnetIp = prefs.GetString("EnetIp", string.Empty);
                _activityCommon.CustomStorageMedia = prefs.GetString("StorageMedia", string.Empty);
                if (!ActivityCommon.StaticDataInitialized || !_activityRecreated)
                {
                    string stateString = prefs.GetString("LastAppState", string.Empty);
                    _instanceData.LastAppState = System.Enum.TryParse(stateString, true, out LastAppState lastAppState) ? lastAppState : LastAppState.Init;
                    _instanceData.DeviceName = prefs.GetString("DeviceName", string.Empty);
                    _instanceData.DeviceAddress = prefs.GetString("DeviceAddress", string.Empty);
                    _instanceData.ConfigFileName = prefs.GetString("ConfigFile", string.Empty);
                    _instanceData.UpdateCheckTime = prefs.GetLong("UpdateCheckTime", DateTime.MinValue.Ticks);
                    _instanceData.UpdateSkipVersion = prefs.GetInt("UpdateSkipVersion", -1);
                    _instanceData.LastVersionCode = prefs.GetInt("VersionCode", -1);
                    _instanceData.StorageRequirementsAccepted = prefs.GetBoolean("StorageAccepted", false);
                    _instanceData.XmlEditorPackageName = prefs.GetString("XmlEditorPackageName", string.Empty);
                    _instanceData.XmlEditorClassName = prefs.GetString("XmlEditorClassName", string.Empty);

                    ActivityCommon.BtNoEvents = prefs.GetBoolean("BtNoEvents", false);
                    ActivityCommon.EnableTranslation = prefs.GetBoolean("EnableTranslation", false);
                    ActivityCommon.YandexApiKey = prefs.GetString("YandexApiKey", string.Empty);
                    ActivityCommon.ShowBatteryVoltageWarning = prefs.GetBoolean("ShowBatteryWarning", true);
                    ActivityCommon.BatteryWarnings = prefs.GetLong("BatteryWarnings", 0);
                    ActivityCommon.BatteryWarningVoltage = prefs.GetFloat("BatteryWarningVoltage", 0);
                    ActivityCommon.LastAdapterSerial = prefs.GetString("LastAdapterSerial", string.Empty);
                    ActivityCommon.EmailAddress = prefs.GetString("EmailAddress", string.Empty);
                    ActivityCommon.TraceInfo = prefs.GetString("TraceInfo", string.Empty);
                    ActivityCommon.AppId = prefs.GetString("AppId", string.Empty);
                    ActivityCommon.AutoHideTitleBar = prefs.GetBoolean("AutoHideTitleBar", ActivityCommon.AutoHideTitleBar);
                    ActivityCommon.SuppressTitleBar = prefs.GetBoolean("SuppressTitleBar", ActivityCommon.SuppressTitleBar);
                    ActivityCommon.SelectedInternetConnection = (ActivityCommon.InternetConnectionType)prefs.GetInt("InternetConnection", (int)ActivityCommon.InternetConnectionType.Cellular);
                    ActivityCommon.SelectedManufacturer = (ActivityCommon.ManufacturerType)prefs.GetInt("Manufacturer", (int)ActivityCommon.ManufacturerType.Bmw);
                    ActivityCommon.BtEnbaleHandling = (ActivityCommon.BtEnableType)prefs.GetInt("BtEnable", (int)ActivityCommon.BtEnableType.Ask);
                    ActivityCommon.BtDisableHandling = (ActivityCommon.BtDisableType)prefs.GetInt("BtDisable", (int)ActivityCommon.BtDisableType.DisableIfByApp);
                    ActivityCommon.LockTypeCommunication = (ActivityCommon.LockType)prefs.GetInt("LockComm", (int)ActivityCommon.LockType.ScreenDim);
                    ActivityCommon.LockTypeLogging = (ActivityCommon.LockType)prefs.GetInt("LockLog", (int)ActivityCommon.LockType.Cpu);
                    ActivityCommon.StoreDataLogSettings = prefs.GetBoolean("StoreDataLogSettings", ActivityCommon.StoreDataLogSettings);
                    if (ActivityCommon.StoreDataLogSettings)
                    {
                        _instanceData.DataLogActive = prefs.GetBoolean("DataLogActive", _instanceData.DataLogActive);
                        _instanceData.DataLogAppend = prefs.GetBoolean("DataLogAppend", _instanceData.DataLogAppend);
                    }
                    ActivityCommon.AutoConnectHandling = (ActivityCommon.AutoConnectType)prefs.GetInt("AutoConnect", (int)ActivityCommon.AutoConnectType.Offline);
                    ActivityCommon.UpdateCheckDelay = prefs.GetLong("UpdateCheckDelay", ActivityCommon.UpdateCheckDelayDefault);
                    ActivityCommon.DoubleClickForAppExit = prefs.GetBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
                    ActivityCommon.SendDataBroadcast = prefs.GetBoolean("SendDataBroadcast", ActivityCommon.SendDataBroadcast);
                    ActivityCommon.CheckCpuUsage = prefs.GetBoolean("CheckCpuUsage", true);
                    ActivityCommon.CheckEcuFiles = prefs.GetBoolean("CheckEcuFiles", true);
                    ActivityCommon.OldVagMode = prefs.GetBoolean("OldVagMode", false);
                    ActivityCommon.UseBmwDatabase = prefs.GetBoolean("UseBmwDatabase", true);
                    ActivityCommon.ScanAllEcus = prefs.GetBoolean("ScanAllEcus", false);
                    ActivityCommon.CollectDebugInfo = prefs.GetBoolean("CollectDebugInfo", ActivityCommon.CollectDebugInfo);
                    ActivityCommon.StaticDataInitialized = true;

                    if (_instanceData.LastVersionCode != _currentVersionCode)
                    {
                        _instanceData.StorageRequirementsAccepted = false;
                        _instanceData.UpdateCheckTime = 0;
                        _instanceData.UpdateSkipVersion = -1;
                        ActivityCommon.BatteryWarnings = 0;
                        ActivityCommon.BatteryWarningVoltage = 0;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void StoreSettings()
        {
            try
            {
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.Clear();
                prefsEdit.PutString("LastAppState", _instanceData.LastAppState.ToString());
                prefsEdit.PutString("DeviceName", _instanceData.DeviceName);
                prefsEdit.PutString("DeviceAddress", _instanceData.DeviceAddress);
                prefsEdit.PutString("EnetIp", _activityCommon.SelectedEnetIp);
                prefsEdit.PutString("ConfigFile", _instanceData.ConfigFileName);
                prefsEdit.PutLong("UpdateCheckTime", _instanceData.UpdateCheckTime);
                prefsEdit.PutInt("UpdateSkipVersion", _instanceData.UpdateSkipVersion);
                prefsEdit.PutString("StorageMedia", _activityCommon.CustomStorageMedia ?? string.Empty);
                prefsEdit.PutInt("VersionCode", _currentVersionCode);
                prefsEdit.PutBoolean("StorageAccepted", _instanceData.StorageRequirementsAccepted);
                prefsEdit.PutString("XmlEditorPackageName", _instanceData.XmlEditorPackageName ?? string.Empty);
                prefsEdit.PutString("XmlEditorClassName", _instanceData.XmlEditorClassName ?? string.Empty);
                prefsEdit.PutBoolean("BtNoEvents", ActivityCommon.BtNoEvents);
                prefsEdit.PutBoolean("EnableTranslation", ActivityCommon.EnableTranslation);
                prefsEdit.PutString("YandexApiKey", ActivityCommon.YandexApiKey ?? string.Empty);
                prefsEdit.PutBoolean("ShowBatteryWarning", ActivityCommon.ShowBatteryVoltageWarning);
                prefsEdit.PutLong("BatteryWarnings", ActivityCommon.BatteryWarnings);
                prefsEdit.PutFloat("BatteryWarningVoltage", (float)ActivityCommon.BatteryWarningVoltage);
                prefsEdit.PutString("LastAdapterSerial", ActivityCommon.LastAdapterSerial ?? string.Empty);
                prefsEdit.PutString("EmailAddress", ActivityCommon.EmailAddress ?? string.Empty);
                prefsEdit.PutString("TraceInfo", ActivityCommon.TraceInfo ?? string.Empty);
                prefsEdit.PutString("AppId", ActivityCommon.AppId);
                prefsEdit.PutString("Locale", ActivityCommon.SelectedLocale ?? string.Empty);
                prefsEdit.PutInt("Theme", (int)ActivityCommon.SelectedTheme);
                prefsEdit.PutBoolean("AutoHideTitleBar", ActivityCommon.AutoHideTitleBar);
                prefsEdit.PutBoolean("SuppressTitleBar", ActivityCommon.SuppressTitleBar);
                prefsEdit.PutInt("InternetConnection", (int)ActivityCommon.SelectedInternetConnection);
                prefsEdit.PutInt("Manufacturer", (int) ActivityCommon.SelectedManufacturer);
                prefsEdit.PutInt("BtEnable", (int)ActivityCommon.BtEnbaleHandling);
                prefsEdit.PutInt("BtDisable", (int) ActivityCommon.BtDisableHandling);
                prefsEdit.PutInt("LockComm", (int)ActivityCommon.LockTypeCommunication);
                prefsEdit.PutInt("LockLog", (int)ActivityCommon.LockTypeLogging);
                prefsEdit.PutBoolean("StoreDataLogSettings", ActivityCommon.StoreDataLogSettings);
                prefsEdit.PutBoolean("DataLogActive", _instanceData.DataLogActive);
                prefsEdit.PutBoolean("DataLogAppend", _instanceData.DataLogAppend);
                prefsEdit.PutInt("AutoConnect", (int)ActivityCommon.AutoConnectHandling);
                prefsEdit.PutLong("UpdateCheckDelay", ActivityCommon.UpdateCheckDelay);
                prefsEdit.PutBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
                prefsEdit.PutBoolean("SendDataBroadcast", ActivityCommon.SendDataBroadcast);
                prefsEdit.PutBoolean("CheckCpuUsage", ActivityCommon.CheckCpuUsage);
                prefsEdit.PutBoolean("CheckEcuFiles", ActivityCommon.CheckEcuFiles);
                prefsEdit.PutBoolean("OldVagMode", ActivityCommon.OldVagMode);
                prefsEdit.PutBoolean("UseBmwDatabase", ActivityCommon.UseBmwDatabase);
                prefsEdit.PutBoolean("ScanAllEcus", ActivityCommon.ScanAllEcus);
                prefsEdit.PutBoolean("CollectDebugInfo", ActivityCommon.CollectDebugInfo);
                prefsEdit.Commit();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void StoreLastAppState(LastAppState lastAppState)
        {
            _instanceData.LastAppState = lastAppState;
            StoreSettings();
        }

        private void RequestStoragePermissions()
        {
            if (_permissionsExternalStorage.All(permisson => ContextCompat.CheckSelfPermission(this, permisson) == Permission.Granted))
            {
                StoragePermissonGranted();
                return;
            }
            ActivityCompat.RequestPermissions(this, _permissionsExternalStorage, RequestPermissionExternalStorage);
        }

        private void StoragePermissonGranted()
        {
            _instanceData.StorageAccessGranted = true;
            ActivityCommon.SetStoragePath();
            UpdateDirectories();
            _activityCommon.RequestUsbPermission(null);
            ReadConfigFile();
            if (_startAlertDialog == null && _currentVersionCode != _instanceData.LastVersionCode)
            {
                string message = (GetString(Resource.String.version_change_info_message) +
                                 GetString(Resource.String.version_last_changes)).Replace("\n", "<br>");
                _startAlertDialog = new AlertDialog.Builder(this)
                    .SetNeutralButton(Resource.String.button_donate, (sender, args) =>
                    {
                        StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VUFSVNBRQQBPJ")));
                    })
                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                    })
                    .SetCancelable(false)
                    .SetMessage(ActivityCommon.FromHtml(message))
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
                _startAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    _startAlertDialog = null;
                    HandleStartDialogs(true);
                };
                TextView messageView = _startAlertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                if (messageView != null)
                {
                    messageView.MovementMethod = new LinkMovementMethod();
                }
            }
        }

        private void UpdateDirectories()
        {
            _instanceData.AppDataPath = string.Empty;
            _instanceData.EcuPath = string.Empty;
            _instanceData.VagPath = string.Empty;
            _instanceData.UserEcuFiles = false;
            if (string.IsNullOrEmpty(_activityCommon.CustomStorageMedia))
            {
                if (string.IsNullOrEmpty(ActivityCommon.ExternalWritePath))
                {
                    if (string.IsNullOrEmpty(ActivityCommon.ExternalPath))
                    {
                        Toast.MakeText(this, GetString(Resource.String.no_ext_storage), ToastLength.Long).Show();
                        Finish();
                    }
                    else
                    {
                        _instanceData.AppDataPath = Path.Combine(ActivityCommon.ExternalPath, AppFolderName);
                    }
                }
                else
                {
                    _instanceData.AppDataPath = ActivityCommon.ExternalWritePath;
                }
            }
            else
            {
                _instanceData.AppDataPath = Path.Combine(_activityCommon.CustomStorageMedia, AppFolderName);
            }

            if (string.IsNullOrEmpty(_instanceData.EcuPath))
            {
                _instanceData.EcuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
            }
            if (string.IsNullOrEmpty(_instanceData.VagPath))
            {
                _instanceData.VagPath = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir, ActivityCommon.VagBaseDir);
            }
            if (string.IsNullOrEmpty(_instanceData.BmwPath))
            {
                _instanceData.BmwPath = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir, ActivityCommon.BmwBaseDir);
            }

            string backgroundImageFile = Path.Combine(_instanceData.AppDataPath, "Images", "Background.jpg");
            if (File.Exists(backgroundImageFile))
            {
                try
                {
                    _imageBackground.SetImageBitmap(Android.Graphics.BitmapFactory.DecodeFile(backgroundImageFile));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void BroadcastReceived(Context context, Intent intent)
        {
            if (_activityCommon == null)
            {
                return;
            }

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
                case ForegroundService.NotificationBroadcastAction:
                {
                    string request = intent.GetStringExtra(ForegroundService.BroadcastMessageKey);
                    if (request != null)
                    {
                        if (request.Equals(ForegroundService.BroadcastStopComm))
                        {
                            StopEdiabasThread(false);
                        }
                        else if (request.Equals(ForegroundService.BroadcastShowTitle))
                        {
                            SupportActionBar.Show();
                        }
                    }
                    break;
                }

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

                case UsbManager.ActionUsbDeviceDetached:
                    if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Ftdi)
                    {
                        if (intent.GetParcelableExtra(UsbManager.ExtraDevice) is UsbDevice usbDevice &&
                            EdFtdiInterface.IsValidUsbDevice(usbDevice))
                        {
                            StopEdiabasThread(false);
                        }
                    }
                    break;

                case ActivityCommon.ActionPackageName:
                    string packageName = intent.GetStringExtra(ActivityCommon.BroadcastXmlEditorPackageName);
                    string className = intent.GetStringExtra(ActivityCommon.BroadcastXmlEditorClassName);
                    if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(className))
                    {
                        _instanceData.XmlEditorPackageName = packageName;
                        _instanceData.XmlEditorClassName = className;
                        UpdateOptionsMenu();
                    }
                    break;
            }
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            RunOnUiThread(() => { UpdateDisplay(); });
        }

        private void PageChanged(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                try
                {
                    JobReader.PageInfo pageInfo = ActivityCommon.EdiabasThread?.JobPageInfo;
                    if (pageInfo != null)
                    {
                        int pageIndex = ActivityCommon.JobReader.PageList.IndexOf(pageInfo);
                        if (pageIndex >= 0 && pageIndex < _tabLayout.TabCount && _tabLayout.SelectedTabPosition != pageIndex)
                        {
                            _tabLayout.GetTabAt(pageIndex).Select();
                        }
                    }

                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        private void ThreadTerminated(object sender, EventArgs e)
        {
            RunOnUiThread(ThreadTerminatedMethode);
        }

        private void ThreadTerminatedMethode()
        {
            if (_activityCommon == null)
            {   // OnDestroy already executed
                return;
            }
            StopEdiabasThread(true);
            UpdateDisplay();

            _translationList = null;
            _translatedList = null;

            if (_instanceData.CommErrorsOccured && _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                _activityCommon.RequestSendTraceFile(_instanceData.AppDataPath, _instanceData.TraceDir, GetType());
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (ActivityCommon.CommActive)
            {
                return false;
            }
            if (ActivityCommon.CollectDebugInfo ||
                (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir)))
            {
                return _activityCommon.SendTraceFile(_instanceData.AppDataPath, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        private JobReader.PageInfo GetSelectedPage()
        {
            JobReader.PageInfo pageInfo = null;
            int index = _tabLayout.SelectedTabPosition;
            if (index >= 0 && index < (ActivityCommon.JobReader.PageList.Count))
            {
                pageInfo = ActivityCommon.JobReader.PageList[index];
            }
            return pageInfo;
        }

        private void UpdateSelectedPage()
        {
            if (!ActivityCommon.CommActive)
            {
                return;
            }

            JobReader.PageInfo newPageInfo = GetSelectedPage();
            if (newPageInfo == null)
            {
                return;
            }
            bool newCommActive = !newPageInfo.JobActivate;
            if (ActivityCommon.EdiabasThread.JobPageInfo != newPageInfo)
            {
                ActivityCommon.EdiabasThread.CommActive = newCommActive;
                ActivityCommon.EdiabasThread.JobPageInfo = newPageInfo;
            }
        }

        private void ClearPage(int index)
        {
            if (index >= 0 && index < (ActivityCommon.JobReader.PageList.Count))
            {
                JobReader.PageInfo pageInfo = ActivityCommon.JobReader.PageList[index];
                Fragment dynamicFragment = (Fragment)pageInfo.InfoObject;
                if (dynamicFragment?.View != null)
                {
                    ListView listViewResult = dynamicFragment.View.FindViewById<ListView>(Resource.Id.resultList);
                    ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                    if (resultListAdapter != null)
                    {
                        resultListAdapter.Items.Clear();
                        resultListAdapter.NotifyDataSetChanged();
                    }
                }
            }
        }

        private void UpdateDisplay(bool forceUpdate = false)
        {
            if (!_activityActive || (_activityCommon == null))
            {   // OnDestroy already executed
                return;
            }
            bool dynamicValid = false;
            string language = ActivityCommon.GetCurrentLanguage();

            _connectButtonInfo.Enabled = true;
            if (ActivityCommon.CommActive)
            {
                if (ActivityCommon.EdiabasThread.ThreadStopping())
                {
                    _connectButtonInfo.Enabled = false;
                }
                if (ActivityCommon.EdiabasThread.CommActive)
                {
                    dynamicValid = true;
                }
                _connectButtonInfo.Checked = true;
            }
            else
            {
                if (ActivityCommon.JobReader.PageList.Count == 0)
                {
                    _connectButtonInfo.Enabled = false;
                }
                if (!_activityCommon.IsInterfaceAvailable())
                {
                    _connectButtonInfo.Enabled = false;
                }
                _connectButtonInfo.Checked = false;
            }
            if (_connectButtonInfo.Button != null)
            {
                _connectButtonInfo.Button.Checked = _connectButtonInfo.Checked;
                _connectButtonInfo.Button.Enabled = _connectButtonInfo.Enabled;
            }
            _imageBackground.Visibility = dynamicValid ? ViewStates.Invisible : ViewStates.Visible;

            Fragment dynamicFragment = null;
            JobReader.PageInfo pageInfo = GetSelectedPage();
            if (pageInfo != null)
            {
                dynamicFragment = (Fragment)pageInfo.InfoObject;
            }

            if (!ActivityCommon.ShowBatteryVoltageWarning)
            {
                _instanceData.BatteryWarningShown = false;
            }
            if (dynamicValid && !_instanceData.BatteryWarningShown)
            {
                double? batteryVoltage;
                byte[] adapterSerial;
                lock (EdiabasThread.DataLock)
                {
                    batteryVoltage = ActivityCommon.EdiabasThread.BatteryVoltage;
                    adapterSerial = ActivityCommon.EdiabasThread.AdapterSerial;
                }

                if (_activityCommon.ShowBatteryWarning(batteryVoltage, adapterSerial))
                {
                    _instanceData.BatteryWarningShown = true;
                }
            }

            if (dynamicFragment?.View != null)
            {
                bool gridMode = pageInfo.DisplayMode == JobReader.PageInfo.DisplayModeType.Grid;
                ResultListAdapter resultListAdapter = null;
                ListView listViewResult = dynamicFragment.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult != null)
                {
                    if (listViewResult.Adapter == null)
                    {
                        listViewResult.Adapter = new ResultListAdapter(this, pageInfo.Weight, pageInfo.TextResId, pageInfo.ErrorsInfo != null);
                    }
                    resultListAdapter = listViewResult.Adapter as ResultListAdapter;
                }

                ResultGridAdapter resultGridAdapter = null;
                GridView gridViewResult = dynamicFragment.View.FindViewById<GridView>(Resource.Id.resultGrid);
                if (gridMode && gridViewResult != null)
                {
                    if (gridViewResult.Adapter == null)
                    {
                        gridViewResult.Adapter = new ResultGridAdapter(this);
                    }
                    resultGridAdapter = gridViewResult.Adapter as ResultGridAdapter;
                }

                if (listViewResult != null)
                {
                    listViewResult.Visibility = resultGridAdapter != null ? ViewStates.Gone : ViewStates.Visible;
                }
                if (gridViewResult != null)
                {
                    gridViewResult.Visibility = resultGridAdapter != null ? ViewStates.Visible : ViewStates.Gone;
                }

                ToggleButton buttonActive = null;
                if (pageInfo.JobActivate)
                {
                    buttonActive = dynamicFragment.View.FindViewById<ToggleButton>(Resource.Id.button_active);
                }
                Button buttonErrorReset = null;
                Button buttonErrorResetAll = null;
                Button buttonErrorSelect = null;
                Button buttonErrorCopy = null;
                if (pageInfo.ErrorsInfo != null)
                {
                    buttonErrorReset = dynamicFragment.View.FindViewById<Button>(Resource.Id.button_error_reset);
                    buttonErrorResetAll = dynamicFragment.View.FindViewById<Button>(Resource.Id.button_error_reset_all);
                    buttonErrorSelect = dynamicFragment.View.FindViewById<Button>(Resource.Id.button_error_select);
                    buttonErrorCopy = dynamicFragment.View.FindViewById<Button>(Resource.Id.button_copy);
                }

                int gaugeSize = 200;
                if (gridViewResult != null && gridViewResult.Visibility == ViewStates.Visible)
                {
                    int gaugeCount = pageInfo.GaugesPortrait;
                    switch (Resources.Configuration.Orientation)
                    {
                        case Android.Content.Res.Orientation.Landscape:
                            gaugeCount = pageInfo.GaugesLandscape;
                            break;
                    }
                    gaugeSize = (gridViewResult.Width / gaugeCount) - gridViewResult.HorizontalSpacing - 1;
                    if (gaugeSize < 10)
                    {
                        gaugeSize = 10;
                    }
                }

                if (dynamicValid && resultListAdapter != null)
                {
                    MultiMap<string, EdiabasNet.ResultData> resultDict = null;
                    string errorMessage = string.Empty;
                    lock (EdiabasThread.DataLock)
                    {
                        if (ActivityCommon.EdiabasThread.ResultPageInfo == pageInfo)
                        {
                            resultDict = ActivityCommon.EdiabasThread.EdiabasResultDict;
                            errorMessage = ActivityCommon.EdiabasThread.EdiabasErrorMessage;
                        }
                    }
                    if (ActivityCommon.IsCommunicationError(errorMessage))
                    {
                        _instanceData.CommErrorsOccured = true;
                    }

                    MethodInfo formatErrorResult = null;
                    MethodInfo updateResult = null;
                    MethodInfo updateResultMulti = null;
                    if (pageInfo.ClassObject != null)
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        formatErrorResult = pageType.GetMethod("FormatErrorResult");
                        updateResult = pageType.GetMethod("UpdateResultList", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(List<TableResultItem>) } );
                        updateResultMulti = pageType.GetMethod("UpdateResultList", new[] { typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(List<TableResultItem>) });
                    }

                    List<TableResultItem> tempResultList = new List<TableResultItem>();
                    List<GridResultItem> tempResultGrid = new List<GridResultItem>();
                    if (pageInfo.ErrorsInfo != null)
                    {   // read errors
                        List<string> stringList = new List<string>();
                        List<EdiabasThread.EdiabasErrorReport> errorReportList = null;
                        int updateProgress;
                        lock (EdiabasThread.DataLock)
                        {
                            if (ActivityCommon.EdiabasThread.ResultPageInfo == pageInfo)
                            {
                                errorReportList = ActivityCommon.EdiabasThread.EdiabasErrorReportList;
                            }
                            updateProgress = ActivityCommon.EdiabasThread.UpdateProgress;
                        }
                        if (errorReportList == null)
                        {
                            tempResultList.Add(new TableResultItem(string.Format(GetString(Resource.String.error_reading_errors), updateProgress), null));
                        }
                        else
                        {
                            string lastEcuName = null;
                            List<ActivityCommon.VagDtcEntry> dtcList = null;
                            int errorIndex = 0;
                            foreach (EdiabasThread.EdiabasErrorReport errorReport in errorReportList)
                            {
                                if (errorReport is EdiabasThread.EdiabasErrorReportReset errorReportReset)
                                {
                                    if (errorReportReset.ErrorResetOk)
                                    {
                                        bool changed = false;
                                        foreach (TableResultItem resultItem in resultListAdapter.Items)
                                        {
                                            if (string.IsNullOrEmpty(errorReport.EcuName) ||
                                                (resultItem.Tag is string ecuName && string.CompareOrdinal(ecuName, errorReport.EcuName) == 0))
                                            {
                                                if (resultItem.Selected)
                                                {
                                                    resultItem.Selected = false;
                                                    changed = true;
                                                }
                                            }
                                        }

                                        if (changed || forceUpdate)
                                        {
                                            resultListAdapter.NotifyDataSetChanged();
                                        }
                                    }
                                    continue;
                                }
                                if (ActivityCommon.IsCommunicationError(errorReport.ExecptionText))
                                {
                                    _instanceData.CommErrorsOccured = true;
                                }

                                bool shadow = errorReport is EdiabasThread.EdiabasErrorShadowReport;
                                string ecuTitle = GetPageString(pageInfo, errorReport.EcuName);
                                EcuFunctionStructs.EcuVariant ecuVariant = null;
                                if (ActivityCommon.EcuFunctionsActive && ActivityCommon.EcuFunctionReader != null)
                                {
                                    ecuVariant = ActivityCommon.EcuFunctionReader.GetEcuVariantCached(errorReport.SgbdResolved);
                                    if (ecuVariant != null)
                                    {
                                        string title = ecuVariant.Title?.GetTitle(language);
                                        if (!string.IsNullOrEmpty(title))
                                        {
                                            ecuTitle += " (" + title + ")";
                                        }
                                    }
                                }

                                StringBuilder srMessage = new StringBuilder();
                                srMessage.Append(ecuTitle);
                                srMessage.Append(": ");

                                if (errorReport.ErrorDict == null)
                                {
                                    srMessage.Append(GetString(Resource.String.error_no_response));
                                }
                                else
                                {
                                    if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                                    {
                                        Int64 errorCode = 0;
                                        if (errorReport.ErrorDict.TryGetValue("FNR_WERT", out EdiabasNet.ResultData resultData))
                                        {
                                            if (resultData.OpData is Int64)
                                            {
                                                errorCode = (Int64) resultData.OpData;
                                            }
                                        }

                                        byte[] ecuResponse1 = null;
                                        List<byte[]> ecuResponseList = new List<byte[]>();
                                        foreach (string key in errorReport.ErrorDict.Keys)
                                        {
                                            if (key.StartsWith("ECU_RESPONSE", StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (errorReport.ErrorDict.TryGetValue(key, out resultData))
                                                {
                                                    if (resultData.OpData.GetType() == typeof(byte[]))
                                                    {
                                                        if (string.Compare(key, "ECU_RESPONSE1", StringComparison.OrdinalIgnoreCase) == 0)
                                                        {
                                                            ecuResponse1 = (byte[]) resultData.OpData;
                                                        }
                                                        else
                                                        {
                                                            ecuResponseList.Add((byte[])resultData.OpData);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        List<long> errorTypeList = new List<long>();
                                        for (int i = 0; i < 1000; i++)
                                        {
                                            if (!errorReport.ErrorDict.TryGetValue(string.Format(Culture, "FART{0}_WERT", i + 1), out resultData))
                                            {
                                                break;
                                            }
                                            if (!(resultData.OpData is Int64))
                                            {
                                                break;
                                            }
                                            Int64 errorType = (Int64) resultData.OpData;
                                            errorTypeList.Add(errorType);
                                        }
                                        bool kwp1281 = false;
                                        bool uds = false;
                                        bool saeMode = false;
                                        bool saeDetail = false;
                                        if (errorReport.ErrorDict.TryGetValue("OBJECT", out resultData))
                                        {
                                            // ReSharper disable once UsePatternMatching
                                            string objectName = resultData.OpData as string;
                                            if (objectName != null)
                                            {
                                                if (objectName.Contains("1281"))
                                                {
                                                    kwp1281 = true;
                                                }
                                                else if (objectName.Contains("7000"))
                                                {
                                                    uds = true;
                                                }
                                            }
                                        }
                                        if (errorReport.ErrorDict.TryGetValue("SAE", out resultData))
                                        {
                                            if (resultData.OpData is Int64)
                                            {
                                                if ((Int64) resultData.OpData != 0)
                                                {
                                                    saeMode = true;
                                                    saeDetail = true;
                                                    errorCode <<= 8;
                                                }
                                            }
                                        }

                                        ActivityCommon.VagDtcEntry dtcEntry = null;
                                        if (kwp1281)
                                        {
                                            dtcList = null;
                                            byte dtcDetail = 0;
                                            if (errorTypeList.Count >= 2)
                                            {
                                                dtcDetail = (byte)((errorTypeList[0] & 0x7F) | (errorTypeList[1] << 7));
                                            }

                                            dtcEntry = new ActivityCommon.VagDtcEntry((uint) errorCode, dtcDetail, UdsFileReader.DataReader.ErrorType.Iso9141);
                                        }
                                        else if (uds)
                                        {
                                            dtcList = null;
                                            byte dtcDetail = 0;
                                            if (errorTypeList.Count >= 8)
                                            {
                                                for (int idx = 0; idx < 8; idx++)
                                                {
                                                    if (errorTypeList[idx] == 1)
                                                    {
                                                        dtcDetail |= (byte)(1 << idx);
                                                    }
                                                }
                                            }

                                            dtcEntry = new ActivityCommon.VagDtcEntry((uint) errorCode, dtcDetail, UdsFileReader.DataReader.ErrorType.Uds);
                                            saeMode = true;
                                            errorCode <<= 8;
                                        }
                                        else
                                        {
                                            if (ecuResponse1 != null)
                                            {
                                                errorIndex = 0;
                                                dtcList = ActivityCommon.ParseEcuDtcResponse(ecuResponse1, saeMode);
                                            }

                                            if (dtcList != null && errorIndex < dtcList.Count)
                                            {
                                                dtcEntry = dtcList[errorIndex];
                                            }
                                        }

                                        List<string> textList = null;
                                        if (ActivityCommon.VagUdsActive && dtcEntry != null)
                                        {
                                            if (dtcEntry.ErrorType != UdsFileReader.DataReader.ErrorType.Uds)
                                            {
                                                string dataFileName = null;
                                                if (!string.IsNullOrEmpty(errorReport.VagDataFileName))
                                                {
                                                    dataFileName = Path.Combine(_instanceData.VagPath, errorReport.VagDataFileName);
                                                }
                                                UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(dataFileName);
                                                if (udsReader != null)
                                                {
                                                    textList = udsReader.DataReader.ErrorCodeToString(
                                                        dtcEntry.DtcCode, dtcEntry.DtcDetail, dtcEntry.ErrorType, udsReader);
                                                    if (saeDetail && ecuResponseList.Count > 0)
                                                    {
                                                        byte[] detailData = ActivityCommon.ParseSaeDetailDtcResponse(ecuResponseList[0]);
                                                        if (detailData != null)
                                                        {
                                                            List<string> saeDetailList = udsReader.DataReader.SaeErrorDetailHeadToString(detailData, udsReader);
                                                            if (saeDetailList != null)
                                                            {
                                                                textList.AddRange(saeDetailList);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(errorReport.VagUdsFileName))
                                                {
                                                    string udsFileName = null;
                                                    if (!string.IsNullOrEmpty(errorReport.VagUdsFileName))
                                                    {
                                                        udsFileName = Path.Combine(_instanceData.VagPath, errorReport.VagUdsFileName);
                                                    }
                                                    UdsFileReader.UdsReader udsReader = ActivityCommon.GetUdsReader(udsFileName);
                                                    if (udsReader != null)
                                                    {
                                                        textList = udsReader.ErrorCodeToString(udsFileName, dtcEntry.DtcCode, dtcEntry.DtcDetail);
                                                        if (ecuResponseList.Count > 0)
                                                        {
                                                            byte[] response = ActivityCommon.ExtractUdsEcuResponses(ecuResponseList[0]);
                                                            if (response != null)
                                                            {
                                                                List<string> errorDetailList = udsReader.ErrorDetailBlockToString(udsFileName, response);
                                                                if (errorDetailList != null)
                                                                {
                                                                    textList.AddRange(errorDetailList);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            srMessage.Append("\r\n");
                                            srMessage.Append(GetString(Resource.String.error_code));
                                            srMessage.Append(": ");
                                            srMessage.Append(string.Format("0x{0:X04} 0x{1:X02} {2}", dtcEntry.DtcCode, dtcEntry.DtcDetail, dtcEntry.ErrorType.ToString()));
                                        }
                                        if (textList == null)
                                        {
                                            textList = _activityCommon.ConvertVagDtcCode(_instanceData.EcuPath, errorCode, errorTypeList, kwp1281, saeMode);
                                            srMessage.Append("\r\n");
                                            srMessage.Append(GetString(Resource.String.error_code));
                                            srMessage.Append(": ");
                                            srMessage.Append(string.Format("0x{0:X}", errorCode));
                                            foreach (long errorType in errorTypeList)
                                            {
                                                srMessage.Append(string.Format(";{0}", errorType));
                                            }

                                            if (saeMode)
                                            {
                                                srMessage.Append("\r\n");
                                                srMessage.Append(string.Format("{0}-{1:X02}", ActivityCommon.SaeCode16ToString(errorCode >> 8), errorCode & 0xFF));
                                            }
                                        }
                                        if (textList != null)
                                        {
                                            // ReSharper disable once LoopCanBeConvertedToQuery
                                            foreach (string text in textList)
                                            {
                                                srMessage.Append("\r\n");
                                                srMessage.Append(text);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // BMW
                                        string text1 = string.Empty;
                                        string text2 = string.Empty;
                                        Int64 errorCode = GetResultInt64(errorReport.ErrorDict, "F_ORT_NR", out bool errorCodeFound);
                                        if (!errorCodeFound)
                                        {
                                            errorCode = 0x0000;
                                        }

                                        List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelList = null;
                                        if (ecuVariant != null)
                                        {
#if false   // test code for result states
                                            List<EcuFunctionStructs.EcuEnvCondLabel> envCondLabelResultList = ActivityCommon.EcuFunctionReader.GetEnvCondLabelListWithResultStates(ecuVariant);
                                            if (envCondLabelResultList != null && envCondLabelResultList.Count > 0)
                                            {
                                                string detailTestText = EdiabasThread.ConvertEnvCondErrorDetail(this, errorReport, envCondLabelResultList);
                                                if (!string.IsNullOrEmpty(detailTestText))
                                                {
                                                    srMessage.Append("\r\n");
                                                    srMessage.Append(detailTestText);
                                                }
                                            }
#endif
                                            if (errorCode != 0x0000)
                                            {
                                                envCondLabelList = ActivityCommon.EcuFunctionReader.GetEnvCondLabelList(errorCode, ecuVariant);
                                                List<string> faultResultList = EdiabasThread.ConvertFaultCodeError(errorCode, errorReport, ecuVariant);
                                                if (faultResultList != null && faultResultList.Count == 2)
                                                {
                                                    text1 = faultResultList[0];
                                                    text2 = faultResultList[1];
                                                }
                                            }
                                        }

                                        if (string.IsNullOrEmpty(text1))
                                        {
                                            text1 = FormatResultString(errorReport.ErrorDict, "F_ORT_TEXT", "{0}");
                                            text2 = FormatResultString(errorReport.ErrorDict, "F_VORHANDEN_TEXT", "{0}");
                                            if (ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation)
                                            {
                                                int index = stringList.Count;
                                                stringList.Add(text1);
                                                stringList.Add(text2);
                                                if (_translationList != null && _translatedList != null &&
                                                    _translationList.Count == _translatedList.Count)
                                                {
                                                    if (index < _translatedList.Count)
                                                    {
                                                        if (string.Compare(text1, _translationList[index], StringComparison.Ordinal) == 0)
                                                        {
                                                            text1 = _translatedList[index];
                                                        }
                                                    }
                                                    index++;
                                                    if (index < _translatedList.Count)
                                                    {
                                                        if (string.Compare(text2, _translationList[index], StringComparison.Ordinal) == 0)
                                                        {
                                                            text2 = _translatedList[index];
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        string textErrorCode = FormatResultInt64(errorReport.ErrorDict, "F_ORT_NR", "{0:X04}");
                                        if (errorCode == 0x0000 || (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(textErrorCode)))
                                        {
                                            srMessage.Clear();
                                        }
                                        else
                                        {
                                            srMessage.Append("\r\n");
                                            if (!string.IsNullOrEmpty(textErrorCode))
                                            {
                                                srMessage.Append(GetString(Resource.String.error_code));
                                                if (shadow)
                                                {
                                                    srMessage.Append(" ");
                                                    srMessage.Append(GetString(Resource.String.error_shadow));
                                                }
                                                srMessage.Append(": ");
                                                srMessage.Append(textErrorCode);
                                                srMessage.Append("\r\n");
                                            }

                                            srMessage.Append(text1);
                                            if (!string.IsNullOrEmpty(text2))
                                            {
                                                srMessage.Append(", ");
                                                srMessage.Append(text2);
                                            }

                                            string detailText = EdiabasThread.ConvertEnvCondErrorDetail(this, errorReport, envCondLabelList);
                                            if (!string.IsNullOrEmpty(detailText))
                                            {
                                                srMessage.Append("\r\n");
                                                srMessage.Append(detailText);
                                            }
                                        }
                                    }
                                }
                                string message = srMessage.ToString();
                                if (formatErrorResult != null)
                                {
                                    try
                                    {
                                        object[] args = { pageInfo, errorReport, message };
                                        message = formatErrorResult.Invoke(pageInfo.ClassObject, args) as string;
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }

                                if (!string.IsNullOrEmpty(message))
                                {
                                    bool selected = (from resultItem in resultListAdapter.Items
                                                     let ecuName = resultItem.Tag as string
                                                     where ecuName != null && string.CompareOrdinal(ecuName, errorReport.EcuName) == 0
                                                     select resultItem.Selected).FirstOrDefault();
                                    bool newEcu = (lastEcuName == null) || (string.CompareOrdinal(lastEcuName, errorReport.EcuName) != 0);
                                    bool validResponse = errorReport.ErrorDict != null;
                                    TableResultItem newResultItem = new TableResultItem(message, null, errorReport.EcuName, newEcu && validResponse, selected);
                                    newResultItem.CheckChangeEvent += item =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        UpdateButtonErrorReset(buttonErrorReset, resultListAdapter.Items);
                                        UpdateButtonErrorResetAll(buttonErrorResetAll, resultListAdapter.Items, pageInfo);
                                        UpdateButtonErrorSelect(buttonErrorSelect, resultListAdapter.Items);
                                    };
                                    newResultItem.CheckEnable = !ActivityCommon.ErrorResetActive && !shadow;
                                    tempResultList.Add(newResultItem);
                                }
                                lastEcuName = errorReport.EcuName;
                                errorIndex++;
                            }
                            if (tempResultList.Count == 0)
                            {
                                tempResultList.Add(new TableResultItem(GetString(Resource.String.error_no_error), null));
                            }
                        }
                        UpdateButtonErrorReset(buttonErrorReset, tempResultList);
                        UpdateButtonErrorResetAll(buttonErrorResetAll, tempResultList, pageInfo);
                        UpdateButtonErrorSelect(buttonErrorSelect, tempResultList);
                        UpdateButtonErrorCopy(buttonErrorCopy, (errorReportList != null) ? tempResultList : null);

                        if (stringList.Count > 0)
                        {
                            if (!_translateActive)
                            {
                                // translation text present
                                bool translate = false;
                                if ((_translationList == null) || (_translationList.Count != stringList.Count))
                                {
                                    translate = true;
                                }
                                else
                                {
                                    // ReSharper disable once LoopCanBeConvertedToQuery
                                    for (int i = 0; i < stringList.Count; i++)
                                    {
                                        if (string.Compare(stringList[i], _translationList[i], StringComparison.Ordinal) != 0)
                                        {
                                            translate = true;
                                            break;
                                        }
                                    }
                                }
                                if (translate)
                                {
                                    _translationList = stringList;
                                    _translateActive = true;
                                    if (!_activityCommon.TranslateStrings(stringList, transList =>
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            if (_activityCommon == null)
                                            {
                                                return;
                                            }
                                            _translateActive = false;
                                            _translatedList = transList;
                                            UpdateOptionsMenu();
                                            _updateHandler?.Post(() => { UpdateDisplay(); });
                                        });
                                    }))
                                    {
                                        _translateActive = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _translationList = null;
                            _translatedList = null;
                        }
                    }
                    else
                    {
                        foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                        {
                            string result = ActivityCommon.FormatResult(ActivityCommon.EdiabasThread?.Ediabas, pageInfo, displayInfo, resultDict, out Android.Graphics.Color? textColor, out double? dataValue);
                            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                            {
                                if (ActivityCommon.VagUdsActive && !string.IsNullOrEmpty(pageInfo.JobsInfo.VagUdsFileName))
                                {
                                    string udsFileName = Path.Combine(_instanceData.VagPath, pageInfo.JobsInfo.VagUdsFileName);
                                    string resultUds = ActivityCommon.FormatResultVagUds(udsFileName, pageInfo, displayInfo, resultDict, out double? dataValueUds);
                                    if (!string.IsNullOrEmpty(resultUds))
                                    {
                                        result = resultUds;
                                    }
                                    if (dataValue == null)
                                    {
                                        dataValue = dataValueUds;
                                    }
                                }
                            }
                            else
                            {
                                string resultEcuFunc = ActivityCommon.FormatResultEcuFunction(pageInfo, displayInfo, resultDict, out double? dataValueEcuFunc);
                                if (!string.IsNullOrEmpty(resultEcuFunc))
                                {
                                    result = resultEcuFunc;
                                }
                                if (dataValue == null)
                                {
                                    dataValue = dataValueEcuFunc;
                                }
                            }

                            if (result != null)
                            {
                                if (resultGridAdapter != null)
                                {
                                    if (displayInfo.GridType != JobReader.DisplayInfo.GridModeType.Hidden)
                                    {
                                        double value;
                                        if (dataValue.HasValue)
                                        {
                                            value = dataValue.Value;
                                        }
                                        else
                                        {
                                            value = GetResultDouble(resultDict, displayInfo.Result, 0, out bool foundDouble);
                                            if (!foundDouble)
                                            {
                                                Int64 valueInt64 = GetResultInt64(resultDict, displayInfo.Result, 0, out bool foundInt64);
                                                if (foundInt64)
                                                {
                                                    value = valueInt64;
                                                }
                                            }
                                        }

                                        int resourceId = Resource.Layout.result_customgauge_square;
                                        switch (displayInfo.GridType)
                                        {
                                            case JobReader.DisplayInfo.GridModeType.Text:
                                                resourceId = Resource.Layout.result_customgauge_text;
                                                break;

                                            case JobReader.DisplayInfo.GridModeType.Simple_Gauge_Square:
                                                resourceId = Resource.Layout.result_customgauge_square;
                                                break;

                                            case JobReader.DisplayInfo.GridModeType.Simple_Gauge_Round:
                                                resourceId = Resource.Layout.result_customgauge_round;
                                                break;

                                            case JobReader.DisplayInfo.GridModeType.Simple_Gauge_Dot:
                                                resourceId = Resource.Layout.result_customgauge_dot;
                                                break;
                                        }
                                        tempResultGrid.Add(new GridResultItem(resourceId, GetPageString(pageInfo, displayInfo.Name), result, displayInfo.MinValue, displayInfo.MaxValue, value, gaugeSize, textColor));
                                    }
                                }
                                else
                                {
                                    tempResultList.Add(new TableResultItem(GetPageString(pageInfo, displayInfo.Name), result, null, false, false, textColor));
                                }
                            }
                        }
                    }

                    if (resultGridAdapter != null)
                    {
                        // check if list has changed
                        bool resultChanged = false;
                        if (tempResultGrid.Count != resultGridAdapter.Items.Count)
                        {
                            resultChanged = true;
                        }
                        else
                        {
                            for (int i = 0; i < tempResultGrid.Count; i++)
                            {
                                GridResultItem resultNew = tempResultGrid[i];
                                GridResultItem resultOld = resultGridAdapter.Items[i];
                                if (string.CompareOrdinal(resultNew.Name ?? string.Empty, resultOld.Name ?? string.Empty) != 0)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (string.CompareOrdinal(resultNew.ValueText ?? string.Empty, resultOld.ValueText ?? string.Empty) != 0)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (Math.Abs(resultNew.Value - resultOld.Value) > 0.000001)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (resultNew.GaugeSize != resultOld.GaugeSize)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (resultNew.GaugeColor != resultOld.GaugeColor)
                                {
                                    resultChanged = true;
                                    break;
                                }
                            }
                        }
                        if (resultChanged || forceUpdate)
                        {
                            resultGridAdapter.Items.Clear();
                            foreach (GridResultItem resultItem in tempResultGrid)
                            {
                                resultGridAdapter.Items.Add(resultItem);
                            }
                            resultGridAdapter.NotifyDataSetChanged();
                        }
                        gridViewResult.SetColumnWidth(gaugeSize);
                    }
                    else
                    {
                        if (updateResultMulti != null)
                        {
                            object[] args = {pageInfo, resultDict, tempResultList};
                            updateResultMulti.Invoke(pageInfo.ClassObject, args);
                            //pageInfo.ClassObject.UpdateResultList(pageInfo, resultDict, tempResultList);
                        }
                        else if (updateResult != null)
                        {
                            object[] args = {pageInfo, resultDict?.ToDictionary(), tempResultList};
                            updateResult.Invoke(pageInfo.ClassObject, args);
                            //pageInfo.ClassObject.UpdateResultList(pageInfo, resultDict?.ToDictionary(), tempResultList);
                        }

                        // check if list has changed
                        bool resultChanged = false;
                        if (tempResultList.Count != resultListAdapter.Items.Count)
                        {
                            resultChanged = true;
                        }
                        else
                        {
                            for (int i = 0; i < tempResultList.Count; i++)
                            {
                                TableResultItem resultNew = tempResultList[i];
                                TableResultItem resultOld = resultListAdapter.Items[i];
                                if (string.CompareOrdinal(resultNew.Text1 ?? string.Empty, resultOld.Text1 ?? string.Empty) != 0)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (string.CompareOrdinal(resultNew.Text2 ?? string.Empty, resultOld.Text2 ?? string.Empty) != 0)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (resultNew.TextColor != resultOld.TextColor)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (resultNew.CheckVisible != resultOld.CheckVisible)
                                {
                                    resultChanged = true;
                                    break;
                                }
                                if (resultNew.CheckEnable != resultOld.CheckEnable)
                                {
                                    resultChanged = true;
                                    break;
                                }
                            }
                        }
                        if (resultChanged || forceUpdate)
                        {
                            resultListAdapter.Items.Clear();
                            resultListAdapter.Items.AddRange(tempResultList);
                            resultListAdapter.NotifyDataSetChanged();
                        }
                    }
                }
                else
                {
                    resultListAdapter?.Items.Clear();
                    resultListAdapter?.NotifyDataSetChanged();
                    resultGridAdapter?.Items.Clear();
                    resultGridAdapter?.NotifyDataSetChanged();
                    UpdateButtonErrorReset(buttonErrorReset, null);
                    UpdateButtonErrorResetAll(buttonErrorResetAll, null, pageInfo);
                    UpdateButtonErrorSelect(buttonErrorSelect, null);
                    UpdateButtonErrorCopy(buttonErrorCopy, null);
                }

                if (pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        MethodInfo updateLayout = pageType.GetMethod("UpdateLayout");
                        if (updateLayout != null)
                        {
                            object[] args = { pageInfo, dynamicValid, ActivityCommon.EdiabasThread != null };
                            updateLayout.Invoke(pageInfo.ClassObject, args);
                            //pageInfo.ClassObject.UpdateLayout(pageInfo, dynamicValid, ActivityCommon.EdiabasThread != null);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (buttonActive != null)
                {
                    if (ActivityCommon.CommActive)
                    {
                        buttonActive.Enabled = true;
                        buttonActive.Checked = ActivityCommon.EdiabasThread.CommActive;
                    }
                    else
                    {
                        buttonActive.Enabled = false;
                        buttonActive.Checked = false;
                    }
                }
            }
        }

        private void UpdateButtonErrorReset(Button buttonErrorReset, List<TableResultItem> resultItems)
        {
            if (buttonErrorReset == null)
            {
                return;
            }
            bool selected = false;
            if (resultItems != null)
            {
                selected = resultItems.Any(resultItem => resultItem.CheckVisible && resultItem.Selected);
            }

            bool enabled = selected;
            if (enabled && ActivityCommon.ErrorResetActive)
            {
                enabled = false;
            }
            buttonErrorReset.Enabled = enabled;
        }

        private void UpdateButtonErrorResetAll(Button buttonErrorResetAll, List<TableResultItem> resultItems, JobReader.PageInfo pageInfo)
        {
            if (buttonErrorResetAll == null)
            {
                return;
            }

            string sgbdFunctional = pageInfo?.ErrorsInfo?.SgbdFunctional;
            bool visible = !string.IsNullOrEmpty(sgbdFunctional);

            bool checkVisible = false;
            if (resultItems != null)
            {
                checkVisible = resultItems.Any(resultItem => resultItem.CheckVisible);
            }

            bool enabled = checkVisible;
            if (enabled && ActivityCommon.ErrorResetActive)
            {
                enabled = false;
            }
            buttonErrorResetAll.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
            buttonErrorResetAll.Enabled = enabled;
        }

        private void UpdateButtonErrorSelect(Button buttonErrorSelect, List<TableResultItem> resultItems)
        {
            if (buttonErrorSelect == null)
            {
                return;
            }

            bool checkVisible = false;
            bool allSelected = false;
            if (resultItems != null)
            {
                checkVisible = resultItems.Any(resultItem => resultItem.CheckVisible);
                allSelected = resultItems.All(resultItem => !resultItem.CheckVisible || resultItem.Selected);
            }

            bool enabled = checkVisible;
            if (enabled && ActivityCommon.ErrorResetActive)
            {
                enabled = false;
            }

            bool selectAll = !enabled || !allSelected;
            int resId = selectAll ? Resource.String.button_error_select_all : Resource.String.button_error_deselect_all;
            buttonErrorSelect.Enabled = enabled;
            buttonErrorSelect.Text = GetString(resId);
            buttonErrorSelect.Tag = new Java.Lang.Boolean(selectAll);
        }

        private void UpdateButtonErrorCopy(Button buttonErrorCopy, List<TableResultItem> resultItems)
        {
            if (buttonErrorCopy == null)
            {
                return;
            }
            bool present = false;
            if (resultItems != null)
            {
                present = resultItems.Any(resultItem => !string.IsNullOrEmpty(resultItem.Text1));
            }
            buttonErrorCopy.Enabled = present;
        }

        // ReSharper disable once UnusedMember.Global
        public static String FormatResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            return FormatResultDouble(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, format);
        }

        public static String FormatResultDouble(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format, int index = 0)
        {
            double value = GetResultDouble(resultDict, dataName, index, out bool found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            return FormatResultInt64(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, format);
        }

        public static String FormatResultInt64(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format, int index = 0)
        {
            Int64 value = GetResultInt64(resultDict, dataName, index, out bool found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            return FormatResultString(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, format);
        }

        public static String FormatResultString(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format, int index = 0)
        {
            string value = GetResultString(resultDict, dataName, index, out bool found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        // ReSharper disable once UnusedMember.Global
        public static Int64 GetResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            return GetResultInt64(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, 0, out found);
        }

        public static Int64 GetResultInt64(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, int index, out bool found)
        {
            found = false;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out IList<EdiabasNet.ResultData> resultDataList))
            {
                if (index >= 0 && index < resultDataList.Count)
                {
                    EdiabasNet.ResultData resultData = resultDataList[index];
                    if (resultData.OpData is Int64)
                    {
                        found = true;
                        return (Int64) resultData.OpData;
                    }
                }
            }
            return 0;
        }

        // ReSharper disable once UnusedMember.Global
        public static Double GetResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            return GetResultDouble(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, 0, out found);
        }

        public static Double GetResultDouble(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, int index, out bool found)
        {
            found = false;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out IList<EdiabasNet.ResultData> resultDataList))
            {
                if (index >= 0 && index < resultDataList.Count)
                {
                    EdiabasNet.ResultData resultData = resultDataList[index];
                    if (resultData.OpData is Double)
                    {
                        found = true;
                        return (Double) resultData.OpData;
                    }
                }
            }
            return 0;
        }

        // ReSharper disable once UnusedMember.Global
        public static String GetResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            return GetResultString(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, 0, out found);
        }

        public static String GetResultString(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, int index, out bool found)
        {
            found = false;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out IList<EdiabasNet.ResultData> resultDataList))
            {
                if (index >= 0 && index < resultDataList.Count)
                {
                    EdiabasNet.ResultData resultData = resultDataList[index];
                    // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                    if (resultData.OpData is String)
                    {
                        found = true;
                        return (String)resultData.OpData;
                    }
                }
            }
            return string.Empty;
        }

        public static String GetPageString(JobReader.PageInfo pageInfo, string name)
        {
            string lang = Java.Util.Locale.Default.Language ?? string.Empty;
            string langIso = Java.Util.Locale.Default.ISO3Language ?? string.Empty;
            JobReader.StringInfo stringInfoDefault = null;
            JobReader.StringInfo stringInfoSel = null;
            foreach (JobReader.StringInfo stringInfo in pageInfo.StringList)
            {
                if (string.IsNullOrEmpty(stringInfo.Lang))
                {
                    stringInfoDefault = stringInfo;
                }
                else if ((string.Compare(stringInfo.Lang, lang, StringComparison.OrdinalIgnoreCase) == 0) ||
                        (string.Compare(stringInfo.Lang, langIso, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    stringInfoSel = stringInfo;
                }
            }
            if (stringInfoSel == null) stringInfoSel = stringInfoDefault;
            string result = String.Empty;
            if (stringInfoSel != null)
            {
                if (!stringInfoSel.StringDict.TryGetValue(name, out result))
                {
                    result = String.Empty;
                }
            }
            return result;
        }

        private static ActivityCommon GetActivityCommon(Context context)
        {
            ActivityCommon activityCommon = null;
            if (context is ActivityMain mainActivity)
            {
                activityCommon = mainActivity.ActivityCommon;
            }
            else if (context is ForegroundService foregroundService)
            {
                activityCommon = foregroundService.ActivityCommon;
            }

            return activityCommon;
        }

        // ReSharper disable once UnusedMember.Global
        public static bool ShowNotification(Context context, int id, int priority, string title, string message, bool update = false)
        {
            ActivityCommon activityCommon = GetActivityCommon(context);
            if (activityCommon != null)
            {
                return activityCommon.ShowNotification(id, priority, title, message, update);
            }

            return false;
        }

        // ReSharper disable once UnusedMember.Global
        public static bool HideNotification(Context context, int id)
        {
            ActivityCommon activityCommon = GetActivityCommon(context);
            if (activityCommon != null)
            {
                return activityCommon.HideNotification(id);
            }

            return false;
        }

        private void ReadConfigFile()
        {
            if (ActivityCommon.CommActive)
            {
                UpdateJobReaderSettings();
                _updateHandler.Post(CreateActionBarTabs);
                return;
            }

            bool failed = false;
            ActivityCommon.JobReader.Clear();
            if (_instanceData.LastAppState != LastAppState.Compile && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
            {
                if (!ActivityCommon.JobReader.ReadXml(_instanceData.ConfigFileName, out string errorMessage))
                {
                    failed = true;
                    string message = GetString(Resource.String.job_reader_read_xml_failed) + "\r\n";
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        message += errorMessage;
                    }
                    else
                    {
                        message += GetString(Resource.String.job_reader_file_name_invalid);
                    }
                    _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                }
            }

            UpdateJobReaderSettings();
            _activityCommon.ClearTranslationCache();
            _translationList = null;
            _translatedList = null;
            UpdateDirectories();
            if (!failed)
            {
                RequestConfigSelect();
            }
            CompileCode();
        }

        private void UpdateJobReaderSettings()
        {
            if (ActivityCommon.JobReader.PageList.Count > 0)
            {
                ActivityCommon.SelectedManufacturer = ActivityCommon.JobReader.Manufacturer;
                _activityCommon.SelectedInterface = ActivityCommon.JobReader.Interface;
            }
            else
            {
                _activityCommon.SelectedInterface = ActivityCommon.InterfaceType.None;
            }
        }

        private void CompileCode()
        {
            if (!_activityActive || _compileProgress != null)
            {
                _compileCodePending = true;
                return;
            }
            _compileCodePending = false;
            if (!ActivityCommon.IsCpuStatisticsSupported() || !ActivityCommon.CheckCpuUsage)
            {
                _instanceData.CheckCpuUsage = false;
            }
            if (!ActivityCommon.CheckEcuFiles)
            {
                _instanceData.VerifyEcuFiles = false;
            }
            if (ActivityCommon.JobReader.PageList.Count == 0 && !_instanceData.CheckCpuUsage)
            {
                _updateHandler.Post(CreateActionBarTabs);
                return;
            }
            StoreLastAppState(LastAppState.Compile);
            _compileProgress = new CustomProgressDialog(this);
            _compileProgress.SetMessage(GetString(_instanceData.CheckCpuUsage ? Resource.String.compile_cpu_usage : Resource.String.compile_start));
            _compileProgress.Indeterminate = false;
            _compileProgress.Progress = 0;
            _compileProgress.Max = 100;
            _compileProgress.ButtonAbort.Visibility = ViewStates.Gone;
            _compileProgress.Show();
            UpdateLockState();

            Thread compileThreadWrapper = new Thread(() =>
            {
                int cpuUsage = -1;
                long startTime = Stopwatch.GetTimestamp();
                if (_instanceData.CheckCpuUsage)
                {
                    // check CPU idle usage
                    _instanceData.CheckCpuUsage = false;
                    GC.Collect();
                    int count = 0;
                    int maxCount = 5;
                    for (int i = 0; i < maxCount; i++)
                    {
                        List<int> cpuUsageList = ActivityCommon.GetCpuUsageStatistic();
                        if (cpuUsageList == null || !_activityActive)
                        {
                            cpuUsage = -1;
                            break;
                        }
                        if (cpuUsageList.Count == 4)
                        {
                            count++;
                            int usage = cpuUsageList[0] + cpuUsageList[1];
                            cpuUsage = usage;
                            int localCount = count;
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                if (_compileProgress != null)
                                {
                                    string message = string.Format(GetString(Resource.String.compile_cpu_usage_value), usage);
                                    _compileProgress.SetMessage(message);
                                    _compileProgress.Progress = 100 * localCount / maxCount;
                                    startTime = Stopwatch.GetTimestamp();
                                }
                            });
                            if (usage < CpuLoadCritical && count >= 2)
                            {
                                break;
                            }
                        }
                    }
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (_compileProgress != null)
                        {
                            _compileProgress.Progress = 100;
                        }
                    });
                }

                string ecuBaseDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir);
                if (_instanceData.VerifyEcuFiles)
                {
                    _instanceData.VerifyEcuFiles = false;
                    if (ValidEcuPackage(ecuBaseDir))
                    {
                        int lastPercent = -1;
                        if (!ActivityCommon.VerifyContent(Path.Combine(ecuBaseDir, ContentFileName), false, percent =>
                        {
                            if (_activityCommon == null)
                            {
                                return true;
                            }
                            if (lastPercent != percent)
                            {
                                lastPercent = percent;
                                RunOnUiThread(() =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    if (_compileProgress != null)
                                    {
                                        _compileProgress.SetMessage(GetString(Resource.String.verify_files));
                                        _compileProgress.Indeterminate = false;
                                        _compileProgress.Progress = percent;
                                    }
                                });
                            }
                            return false;
                        }))
                        {
                            try
                            {
                                File.Delete(Path.Combine(ecuBaseDir, InfoXmlName));
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (_compileProgress != null)
                    {
                        _compileProgress.SetMessage(GetString(Resource.String.compile_start));
                        _compileProgress.Indeterminate = false;
                        _compileProgress.Progress = 0;
                    }
                });

                bool progressUpdated = false;
                List<string> compileResultList = new List<string>();
                List<Thread> threadList = new List<Thread>();
                foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                {
                    if (pageInfo.ClassCode == null) continue;
                    // limit number of active tasks
                    for (; ; )
                    {
                        int activeThreads = threadList.Count(thread => thread.IsAlive);
                        if (activeThreads < 3)
                        {
                            break;
                        }
                        Thread.Sleep(200);
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (_compileProgress != null)
                        {
                            if (cpuUsage >= 0 && Stopwatch.GetTimestamp() - startTime > 1000 * ActivityCommon.TickResolMs)
                            {
                                progressUpdated = true;
                                _compileProgress.SetMessage(GetString(Resource.String.compile_start));
                                _compileProgress.Indeterminate = true;
                            }
                        }
                    });

                    JobReader.PageInfo infoLocal = pageInfo;
                    Thread compileThread = new Thread(() =>
                    {
                        string result = string.Empty;
                        StringWriter reportWriter = new StringWriter();
                        try
                        {
                            Evaluator evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter(reportWriter)));
                            evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
                            evaluator.ReferenceAssembly(typeof(EdiabasNet).Assembly);
                            evaluator.ReferenceAssembly(typeof(View).Assembly);
                            string classCode = @"
                                using Android.Views;
                                using Android.Widget;
                                using Android.Content;
                                using EdiabasLib;
                                using BmwDeepObd;
                                using System;
                                using System.Collections.Generic;
                                using System.Diagnostics;
                                using System.Threading;"
                                + infoLocal.ClassCode;
                            evaluator.Compile(classCode);
                            object classObject = evaluator.Evaluate("new PageClass()");
                            if (((infoLocal.JobsInfo == null) || (infoLocal.JobsInfo.JobList.Count == 0)) &&
                                ((infoLocal.ErrorsInfo == null) || (infoLocal.ErrorsInfo.EcuList.Count == 0)))
                            {
                                if (classObject == null)
                                {
                                    throw new Exception("Compiling PageClass failed");
                                }
                                Type pageType = classObject.GetType();
                                if (pageType.GetMethod("ExecuteJob") == null)
                                {
                                    throw new Exception("No ExecuteJob method");
                                }
                            }
                            infoLocal.ClassObject = classObject;
                        }
                        catch (Exception ex)
                        {
                            infoLocal.ClassObject = null;
                            result = reportWriter.ToString();
                            if (string.IsNullOrEmpty(result))
                            {
                                result = EdiabasNet.GetExceptionText(ex);
                            }
                            result = GetPageString(infoLocal, infoLocal.Name) + ":\r\n" + result;
                        }
                        if (infoLocal.CodeShowWarnings && string.IsNullOrEmpty(result))
                        {
                            result = reportWriter.ToString();
                        }

                        if (!string.IsNullOrEmpty(result))
                        {
                            lock (compileResultList)
                            {
                                compileResultList.Add(result);
                            }
                        }
                    });
                    compileThread.Start();
                    threadList.Add(compileThread);
                }

                foreach (Thread compileThread in threadList)
                {
                    compileThread.Join();
                }

                if (cpuUsage >= 0 && !progressUpdated)
                {
                    Thread.Sleep(1000);
                }

                foreach (string compileResult in compileResultList)
                {
                    string result = compileResult;
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        _activityCommon.ShowAlert(result, Resource.String.alert_title_error);
                    });
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    CreateActionBarTabs();
                    _compileProgress.Dismiss();
                    _compileProgress.Dispose();
                    _compileProgress = null;
                    UpdateLockState();
                    if (cpuUsage >= CpuLoadCritical)
                    {
                        _activityCommon.ShowAlert(string.Format(GetString(Resource.String.compile_cpu_usage_high), cpuUsage), Resource.String.alert_title_warning);
                    }
                });
            });
            compileThreadWrapper.Start();
        }

        private void SelectMedia()
        {
            EditGlobalSettings(GlobalSettingsActivity.SelectionStorageLocation);
        }

        private void DownloadFile(string url, string downloadDir, string unzipTargetDir = null)
        {
            if (string.IsNullOrEmpty(_obbFileName))
            {
                _activityCommon.ShowAlert(GetString(Resource.String.exp_down_obb_missing), Resource.String.alert_title_error);
                return;
            }
            try
            {
                Directory.CreateDirectory(downloadDir);
            }
            catch (Exception)
            {
                _activityCommon.ShowAlert(GetString(Resource.String.write_files_failed), Resource.String.alert_title_error);
                return;
            }

            string certInfo = _activityCommon.GetCertificateInfo();
            ActivityCommon.ResetUdsReader();
            ActivityCommon.ResetEcuFunctionReader();

            if (_downloadProgress == null)
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                _downloadProgress = new CustomProgressDialog(this);
                _downloadProgress.AbortClick = sender => 
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (_httpClient != null)
                    {
                        try
                        {
                            _httpClient.CancelPendingRequests();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                };
            }
            _downloadProgress.SetMessage(GetString(Resource.String.downloading_file));
            _downloadProgress.Indeterminate = true;
            _downloadProgress.ButtonAbort.Enabled = false;
            _downloadProgress.Show();
            UpdateLockState();
            _activityCommon.SetPreferredNetworkInterface();

            Thread downloadThread = new Thread(() =>
            {
                DownloadInfo downloadInfo = null;
                try
                {
                    string fileName = Path.GetFileName(url) ?? string.Empty;
                    if (!string.IsNullOrEmpty(unzipTargetDir))
                    {
                        XElement xmlInfo = new XElement("Info");
                        xmlInfo.Add(new XAttribute("Url", url ?? string.Empty));
                        xmlInfo.Add(new XAttribute("Name", Path.GetFileName(_obbFileName) ?? string.Empty));
                        downloadInfo = new DownloadInfo(downloadDir, unzipTargetDir, xmlInfo);
                    }
                    // ReSharper disable once RedundantNameQualifier
                    string extension = Path.GetExtension(fileName);
                    bool isPhp = string.Compare(extension, ".php", StringComparison.OrdinalIgnoreCase) == 0;
                    System.Threading.Tasks.Task<HttpResponseMessage> taskDownload;
                    if (isPhp)
                    {
                        string obbName = string.Empty;
                        string installer = string.Empty;
                        try
                        {
                            obbName = Path.GetFileName(_obbFileName) ?? string.Empty;
                            installer = PackageManager.GetInstallerPackageName(PackageName);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        MultipartFormDataContent formDownload = new MultipartFormDataContent
                        {
                            { new StringContent(ActivityCommon.AppId), "appid" },
                            { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", _currentVersionCode)), "appver" },
                            { new StringContent(ActivityCommon.GetCurrentLanguage()), "lang" },
                            { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", Build.VERSION.Sdk)), "android_ver" },
                            { new StringContent(Build.Fingerprint), "fingerprint" },
                            { new StringContent(obbName), "obb_name" },
                            { new StringContent(installer ?? string.Empty), "installer" }
                        };

                        if (!string.IsNullOrEmpty(certInfo))
                        {
                            formDownload.Add(new StringContent(certInfo), "cert");
                        }

                        taskDownload = _httpClient.PostAsync(url, formDownload);
                    }
                    else
                    {
                        taskDownload = _httpClient.GetAsync(url);
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (_downloadProgress != null)
                        {
                            _downloadProgress.ButtonAbort.Enabled = true;
                        }
                    });

                    HttpResponseMessage responseDownload = taskDownload.Result;
                    bool success = responseDownload.IsSuccessStatusCode;
                    string responseDownloadXml = responseDownload.Content.ReadAsStringAsync().Result;
                    DownloadCompleted(success, responseDownloadXml, downloadInfo);
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (_downloadProgress != null)
                        {
                            _downloadProgress.Dismiss();
                            _downloadProgress.Dispose();
                            _downloadProgress = null;
                            UpdateLockState();
                        }

                        bool cancelled = ex.InnerException is System.Threading.Tasks.TaskCanceledException;
                        if (!cancelled)
                        {
                            ObbDownloadError(downloadInfo);
                        }
                    });
                }
            });
            downloadThread.Start();
        }

        private void DownloadCompleted(bool success, string responseXml, DownloadInfo downloadInfo)
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (_downloadProgress != null)
                {
                    bool error = false;
                    string errorMessage = null;
                    _downloadProgress.ButtonAbort.Enabled = false;
                    if (downloadInfo != null)
                    {
                        if (success)
                        {
                            string key = GetObbKey(responseXml, out errorMessage);
                            if (key != null)
                            {
#if DEBUG
                                bool yesSelected = false;
                                AlertDialog altertDialog = new AlertDialog.Builder(this)
                                    .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                                    {
                                        yesSelected = true;
                                        ExtractObbFile(downloadInfo, key);
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (s, a) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(string.Format("OBB key: {0}\nContinue?", key))
                                    .SetTitle(Resource.String.alert_title_info)
                                    .Show();
                                altertDialog.DismissEvent += (o, eventArgs) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    if (!yesSelected)
                                    {
                                        _downloadProgress.Dismiss();
                                        _downloadProgress.Dispose();
                                        _downloadProgress = null;
                                        UpdateLockState();
                                    }
                                };
#else
                                ExtractObbFile(downloadInfo, key);
#endif
                                return;
                            }

                            error = true;
                        }
                    }

                    _downloadProgress.Dismiss();
                    _downloadProgress.Dispose();
                    _downloadProgress = null;
                    UpdateLockState();

                    if (!success || error)
                    {
                        ObbDownloadError(downloadInfo, errorMessage);
                    }
                }
            });
        }

        private void ObbDownloadError(DownloadInfo downloadInfo, string errorMessage = null)
        {
            if (_activityCommon == null)
            {
                return;
            }

            string message = GetString(Resource.String.download_failed);
            if (downloadInfo == null)
            {
                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                message = errorMessage;
            }
            message += "\n" + GetString(Resource.String.obb_offline_extract);

            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                {
                    ObbExtractOffline(downloadInfo);
                })
                .SetNegativeButton(Resource.String.button_no, (s, a) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(message)
                .SetTitle(Resource.String.alert_title_error)
                .Show();
        }

        private void ObbExtractOffline(DownloadInfo downloadInfo)
        {
            if (_activityCommon == null)
            {
                return;
            }

            if (downloadInfo == null)
            {
                return;
            }

            string messageDetail = string.Format(GetString(Resource.String.obb_offline_extract_message),
                _activityCommon.GetPackageInfo()?.VersionName ?? string.Empty, ActivityCommon.AppId, ActivityCommon.ContactMail);
            // ReSharper disable once UseObjectOrCollectionInitializer
            TextInputDialog textInputDialog = new TextInputDialog(this);
            textInputDialog.Message = GetString(Resource.String.obb_offline_extract_title);
            textInputDialog.MessageDetail = messageDetail;
            textInputDialog.SetPositiveButton(Resource.String.button_ok, (s, arg) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                string key = DecryptObbOfflineKey(textInputDialog.Text.Trim());
                if (!string.IsNullOrEmpty(key))
                {
                    ExtractObbFile(downloadInfo, key);
                }
                else
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.obb_offline_key_invalid), Resource.String.alert_title_error);
                }
            });
            textInputDialog.SetNeutralButton(Resource.String.button_copy, (s, arg) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _activityCommon.SetClipboardText(messageDetail);
            });
            textInputDialog.SetNegativeButton(Resource.String.button_abort, (s, arg) =>
            {
            });
            textInputDialog.Show();
        }

        private string DecryptObbOfflineKey(string offlineKey)
        {
            try
            {
                if (string.IsNullOrEmpty(offlineKey))
                {
                    return null;
                }

                using (AesCryptoServiceProvider crypto = new AesCryptoServiceProvider())
                {
                    crypto.Mode = CipherMode.CBC;
                    crypto.Padding = PaddingMode.PKCS7;
                    crypto.KeySize = 256;

                    byte[] appIdBytes = Encoding.ASCII.GetBytes(ActivityCommon.AppId.ToLowerInvariant());
                    using (SHA256Managed sha256 = new SHA256Managed())
                    {
                        crypto.Key = sha256.ComputeHash(appIdBytes);
                    }
                    using (MD5 md5 = MD5.Create())
                    {
                        crypto.IV = md5.ComputeHash(appIdBytes);
                    }

                    using (MemoryStream msEncrypt = new MemoryStream(offlineKey.FromBase62()))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msEncrypt, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetObbKey(string xmlResult, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(_obbFileName))
                {
                    return null;
                }
                string baseName = Path.GetFileName(_obbFileName);
                if (string.IsNullOrEmpty(baseName))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return null;
                }

                foreach (XElement errorNode in xmlDoc.Root.Elements("error"))
                {
                    XAttribute messageAttr = errorNode.Attribute("message");
                    if (messageAttr != null && !string.IsNullOrEmpty(messageAttr.Value))
                    {
                        errorMessage = messageAttr.Value;
                        return null;
                    }
                }

                foreach (XElement fileNode in xmlDoc.Root.Elements("obb"))
                {
                    XAttribute urlAttr = fileNode.Attribute("name");
                    if (urlAttr != null && !string.IsNullOrEmpty(urlAttr.Value))
                    {
                        if (string.Compare(baseName, urlAttr.Value, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare("*", urlAttr.Value, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            XAttribute keyAttr = fileNode.Attribute("key");
                            if (keyAttr != null)
                            {
                                return keyAttr.Value;
                            }
                            return string.Empty;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ExtractObbFile(DownloadInfo downloadInfo, string key)
        {
            ExtractZipFile(_obbFileName, downloadInfo.TargetDir, downloadInfo.InfoXml, key, false,
                new List<string> { Path.Combine(_instanceData.AppDataPath, "EcuVag") });
        }

        private void ExtractZipFile(string fileName, string targetDirectory, XElement infoXml, string key, bool removeFile = false, List<string> removeDirs = null)
        {
            _extractZipCanceled = false;
            if (_downloadProgress == null)
            {
                _downloadProgress = new CustomProgressDialog(this);
                _downloadProgress.SetCancelable(false);
                UpdateLockState();
            }
            _downloadProgress.SetMessage(GetString(Resource.String.extract_cleanup));
            _downloadProgress.Indeterminate = false;
            _downloadProgress.Progress = 0;
            _downloadProgress.Max = 100;
            _downloadProgress.AbortClick = sender =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                _extractZipCanceled = true;
            };
            _downloadProgress.Show();
            _downloadProgress.ButtonAbort.Enabled = false;

            Thread extractThread = new Thread(() =>
            {
                bool extractFailed = false;
                bool ioError = false;
                bool aborted = false;
                string exceptionText = string.Empty;
                try
                {
                    for (int retry = 0;; retry++)
                    {
                        try
                        {
                            if (Directory.Exists(targetDirectory))
                            {
                                // delete xml files first to invalidate the ECUs
                                string[] xmlFiles = Directory.GetFiles(targetDirectory, "*.xml");
                                foreach (string xmlFile in xmlFiles)
                                {
                                    if (!string.IsNullOrEmpty(xmlFile))
                                    {
                                        File.Delete(xmlFile);
                                    }
                                }

                                string[] allFiles = Directory.GetFiles(targetDirectory, "*.*", SearchOption.AllDirectories);
                                int fileIndex = 0;
                                int lastPercent = -1;
                                foreach (string file in allFiles)
                                {
                                    if (!string.IsNullOrEmpty(file))
                                    {
                                        File.Delete(file);
                                    }

                                    if (_extractZipCanceled)
                                    {
                                        aborted = true;
                                        throw new Exception("Canceled");
                                    }

                                    int percent = fileIndex * 100 / allFiles.Length;
                                    if (lastPercent != percent)
                                    {
                                        lastPercent = percent;
                                        RunOnUiThread(() =>
                                        {
                                            if (_activityCommon == null)
                                            {
                                                return;
                                            }

                                            if (_downloadProgress != null)
                                            {
                                                _downloadProgress.SetMessage(GetString(Resource.String.extract_cleanup));
                                                _downloadProgress.Progress = percent;
                                                _downloadProgress.ButtonAbort.Enabled = true;
                                            }
                                        });
                                    }

                                    fileIndex++;
                                }

                                Directory.Delete(targetDirectory, true);
                            }
                            Directory.CreateDirectory(targetDirectory);
                            break;
                        }
                        catch (Exception)
                        {
                            if (aborted)
                            {
                                throw;
                            }

                            if (retry > ActivityCommon.FileIoRetries)
                            {
                                ioError = true;
                                throw;
                            }

                            Thread.Sleep(ActivityCommon.FileIoRetryDelay);
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (_downloadProgress != null)
                        {
                            _downloadProgress.Progress = 0;
                            _downloadProgress.ButtonAbort.Enabled = false;
                        }
                    });

                    if (removeDirs != null)
                    {
                        foreach (string removeDir in removeDirs)
                        {
                            try
                            {
                                if (Directory.Exists(removeDir))
                                {
                                    Directory.Delete(removeDir, true);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        if (_downloadProgress != null)
                        {
                            _downloadProgress.ButtonAbort.Enabled = true;
                        }
                    });

                    List<string> ignoreFolders = null;
                    if (!ActivityCommon.VagFilesRequired())
                    {
                        ignoreFolders = new List<string>
                        {
                            ActivityCommon.AppendDirectorySeparatorChar(ActivityCommon.EcuDirNameVag),
                            ActivityCommon.AppendDirectorySeparatorChar(ActivityCommon.VagBaseDir)
                        };
                    }
                    else
                    {
                        if (!ActivityCommon.VagUdsFilesRequired())
                        {
                            ignoreFolders = new List<string>
                            {
                                ActivityCommon.AppendDirectorySeparatorChar(ActivityCommon.VagBaseDir)
                            };
                        }
                    }
                    int lastZipPercent = -1;
                    ActivityCommon.ExtractZipFile(fileName, targetDirectory, key, ignoreFolders,
                        (percent, decrypt) =>
                        {
                            if (_activityCommon == null)
                            {
                                return true;
                            }
                            if (lastZipPercent != percent)
                            {
                                lastZipPercent = percent;
                                RunOnUiThread(() =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    if (_downloadProgress != null)
                                    {
                                        _downloadProgress.SetMessage(decrypt ? GetString(Resource.String.decrypt_file) : GetString(Resource.String.extract_file));
                                        _downloadProgress.Progress = percent;
                                    }
                                });
                            }
                            return _extractZipCanceled;
                        });

                    int lastVerifyPercent = -1;
                    if (!ActivityCommon.VerifyContent(Path.Combine(targetDirectory, ContentFileName), true, percent =>
                    {
                        if (_activityCommon == null)
                        {
                            return true;
                        }
                        if (lastVerifyPercent != percent)
                        {
                            lastVerifyPercent = percent;
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                if (_downloadProgress != null)
                                {
                                    _downloadProgress.SetMessage(GetString(Resource.String.verify_files));
                                    _downloadProgress.Progress = percent;
                                }
                            });
                        }
                        return _extractZipCanceled;
                    }))
                    {
                        extractFailed = true;
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!extractFailed)
                    {
                        infoXml?.Save(Path.Combine(targetDirectory, InfoXmlName));
                    }
                }
                catch (Exception ex)
                {
                    extractFailed = true;
                    exceptionText = EdiabasNet.GetExceptionText(ex);
                    if (ex is IOException)
                    {
                        ioError = true;
                    }
                }
                finally
                {
                    if (removeFile)
                    {
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    if (_downloadProgress != null)
                    {
                        _downloadProgress.Dismiss();
                        _downloadProgress.Dispose();
                        _downloadProgress = null;
                        UpdateLockState();
                    }

                    if (extractFailed)
                    {
                        if (!aborted)
                        {
                            string message = GetString(ioError ? Resource.String.extract_failed_io : Resource.String.extract_failed);
                            if (!string.IsNullOrEmpty(exceptionText))
                            {
                                message += "\r\n" + exceptionText;
                            }
                            _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                        }
                    }
                    else
                    {
                        RequestConfigSelect();
                    }
                });
            });
            extractThread.Start();
        }

        // ReSharper disable once UnusedParameter.Local
        private void DownloadEcuFiles(bool extraMessage = false)
        {
            string ecuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
            try
            {
                ActivityCommon.FileSystemBlockInfo blockInfo = ActivityCommon.GetFileSystemBlockInfo(_instanceData.AppDataPath);
                long ecuDirSize = ActivityCommon.GetDirectorySize(ecuPath);
                double freeSpace = blockInfo.AvailableSizeBytes + ecuDirSize;
                long requiredSize = EcuExtractSize;
                if (freeSpace < requiredSize)
                {
                    string message = string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_download_free_space), requiredSize);
                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            SelectMedia();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();
                    return;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (extraMessage)
            {
                string message = string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_extract_confirm), EcuExtractSize);

                new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        DownloadFile(EcuDownloadUrl, Path.Combine(_instanceData.AppDataPath, ActivityCommon.DownloadDir),
                            Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir));
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
            }
            else
            {
                DownloadFile(EcuDownloadUrl, Path.Combine(_instanceData.AppDataPath, ActivityCommon.DownloadDir),
                    Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir));
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private bool CheckForEcuFiles(bool checkPackage = false)
        {
            if (!_activityActive || !_instanceData.StorageAccessGranted || _downloadEcuAlertDialog != null)
            {
                return true;
            }

            if (!_instanceData.StorageRequirementsAccepted)
            {
                string message = string.Format(new FileSizeFormatProvider(), GetString(Resource.String.storage_requirements), EcuExtractSize);
                _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_accept, (sender, args) =>
                    {
                        _instanceData.StorageRequirementsAccepted = true;
                    })
                    .SetNegativeButton(Resource.String.button_decline, (sender, args) =>
                    {
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                _downloadEcuAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (!_instanceData.StorageRequirementsAccepted)
                    {
                        Finish();
                    }
                    _downloadEcuAlertDialog = null;
                    CheckForEcuFiles(checkPackage);
                };
                return false;
            }

            if (DisplayUpdateInfo((sender, args) =>
            {
                CheckForEcuFiles(checkPackage);
            }))
            {
                return false;
            }

            if (!ValidEcuFiles(_instanceData.EcuPath) || !ValidEcuPackage(Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir)))
            {
                string message = string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_extract), EcuExtractSize);

                _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        DownloadEcuFiles();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                _downloadEcuAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    _downloadEcuAlertDialog = null;
                };
                return false;
            }
            return true;
        }

        private bool DisplayUpdateInfo(EventHandler<EventArgs> handler)
        {
            if (!_activityActive || _downloadEcuAlertDialog != null)
            {
                return false;
            }

            if (ActivityCommon.CommActive)
            {
                return false;
            }

            string message = _instanceData.UpdateMessage;
            if (_instanceData.UpdateAvailable && !string.IsNullOrEmpty(message))
            {
                _instanceData.UpdateAvailable = false;
                if (_instanceData.UpdateSkipVersion >= 0 && _instanceData.UpdateVersionCode == _instanceData.UpdateSkipVersion)
                {
                    return false;
                }

                _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        if (ExpansionDownloaderActivity.IsFromGooglePlay(this))
                        {
                            try
                            {
                                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"market://details?id=" + PackageName)));
                                return;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        try
                        {
                            StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://play.google.com/store/apps/details?id=" + PackageName)));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetNeutralButton(Resource.String.button_skip, (sender, args) =>
                    {
                        _instanceData.UpdateSkipVersion = _instanceData.UpdateVersionCode;
                        StoreSettings();
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
                _downloadEcuAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    _downloadEcuAlertDialog = null;
                    handler?.Invoke(this, new EventArgs());
                };

                _instanceData.UpdateSkipVersion = -1;
                StoreSettings();
                return true;
            }
            return false;
        }

        private bool ValidEcuFiles(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return false;
                }
                return Directory.GetFiles(path, "*.prg", SearchOption.TopDirectoryOnly).Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidEcuPackage(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return false;
                }

                if (ActivityCommon.VagFilesRequired())
                {
                    if (!Directory.Exists(Path.Combine(path, ActivityCommon.EcuDirNameVag)))
                    {
                        return false;
                    }
                }
                if (ActivityCommon.VagUdsFilesRequired())
                {
                    if (!Directory.Exists(Path.Combine(path, ActivityCommon.VagBaseDir)))
                    {
                        return false;
                    }
                }

                string xmlInfoName = Path.Combine(path, InfoXmlName);
                if (!File.Exists(xmlInfoName))
                {
                    return false;
                }
                XDocument xmlInfo = XDocument.Load(xmlInfoName);
                XAttribute nameAttr = xmlInfo.Root?.Attribute("Name");
                if (nameAttr == null)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(_obbFileName) ||
                    string.Compare(Path.GetFileNameWithoutExtension(nameAttr.Value), Path.GetFileNameWithoutExtension(_obbFileName), StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SelectManufacturerInfo()
        {
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && !_instanceData.VagInfoShown)
            {
                AlertDialog alertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                        SelectManufacturer();
                    })
                    .SetNegativeButton(Resource.String.button_abort, (sender, args) => { })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.vag_mode_info)
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
                TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                if (messageView != null)
                {
                    messageView.MovementMethod = new LinkMovementMethod();
                }
                _instanceData.VagInfoShown = true;
                return;
            }
            SelectManufacturer();
        }

        private void SelectManufacturer()
        {
            _activityCommon.SelectManufacturer((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    _activityCommon.SelectedInterface = ActivityCommon.InterfaceType.Bluetooth;
                }
                ClearConfiguration();
                if (CheckForEcuFiles())
                {
                    RequestConfigSelect();
                }
            });
        }

        private void RequestConfigSelect()
        {
            if (_configSelectAlertDialog != null)
            {
                return;
            }
            if (_instanceData.LastAppState == LastAppState.Compile)
            {
                _instanceData.LastAppState = LastAppState.Init;
                _instanceData.ConfigFileName = string.Empty;
                _configSelectAlertDialog = new AlertDialog.Builder(this)
                    .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.compile_crash)
                    .SetTitle(Resource.String.alert_title_error)
                    .Show();
                _configSelectAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    _configSelectAlertDialog = null;
                };
                return;
            }
            if (ActivityCommon.JobReader.PageList.Count > 0)
            {
                return;
            }

            string message = GetString(Resource.String.config_select) + "\n" +
                             string.Format(GetString(Resource.String.manufacturer_select), _activityCommon.ManufacturerName());
            _configSelectAlertDialog = new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_select, (sender, args) =>
                {
                    SelectConfigFile();
                })
                .SetNegativeButton(Resource.String.button_generate, (sender, args) =>
                {
                    StartXmlTool();
                })
                .SetNeutralButton(Resource.String.button_abort, (sender, args) =>
                {
                    SelectManufacturerInfo();
                })
                .SetCancelable(true)
                .SetMessage(message)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
            _configSelectAlertDialog.DismissEvent += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                _configSelectAlertDialog = null;
            };
        }

        private void ClearConfiguration()
        {
            ActivityCommon.JobReader.Clear();
            _instanceData.ConfigFileName = string.Empty;
            CreateActionBarTabs();
            UpdateDirectories();
            UpdateOptionsMenu();
            UpdateDisplay();
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
                GetString(Resource.String.datalog_append_datalog),
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemMultipleChoice, logNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Multiple;
            listView.SetItemChecked(0, _instanceData.TraceActive);
            listView.SetItemChecked(1, _instanceData.TraceAppend);
            listView.SetItemChecked(2, _instanceData.DataLogActive);
            listView.SetItemChecked(3, _instanceData.DataLogAppend);

            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
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
                            if (value && !ActivityCommon.JobReader.LogTagsPresent)
                            {
                                _activityCommon.ShowAlert(GetString(Resource.String.datalog_no_tags), Resource.String.alert_title_warning);
                            }
                            else if (value && !ActivityCommon.StoreDataLogSettings && !_instanceData.DataLogTemporaryShown)
                            {
                                _instanceData.DataLogTemporaryShown = true;
                                _activityCommon.ShowAlert(GetString(Resource.String.datalog_temporary), Resource.String.alert_title_info);
                            }
                            _instanceData.DataLogActive = value;
                            break;

                        case 3:
                            if (value && !ActivityCommon.StoreDataLogSettings && !_instanceData.DataLogTemporaryShown)
                            {
                                _instanceData.DataLogTemporaryShown = true;
                                _activityCommon.ShowAlert(GetString(Resource.String.datalog_temporary), Resource.String.alert_title_info);
                            }
                            _instanceData.DataLogAppend = value;
                            break;
                    }
                }
                UpdateOptionsMenu();
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) => { });
            builder.Show();
        }

        private void EditYandexKey()
        {
            Intent serverIntent = new Intent(this, typeof(YandexKeyActivity));
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestYandexKey);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        private void EditGlobalSettings(string selection = null)
        {
            Intent serverIntent = new Intent(this, typeof(GlobalSettingsActivity));
            if (selection != null)
            {
                serverIntent.PutExtra(GlobalSettingsActivity.ExtraSelection, selection);
            }
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestGlobalSettings);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        private void AdapterConfig()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }

            if (_activityCommon.ShowConnectWarning(retry =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (retry)
                {
                    AdapterConfig();
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
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestAdapterConfig);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        private void EnetIpConfig()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            _activityCommon.SelectEnetIp((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                UpdateOptionsMenu();
            });
        }

        private void SelectConfigFile()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }

            if (_instanceData.ConfigMatchVehicleShown)
            {
                SelectConfigFileIntent();
                return;
            }

            _instanceData.ConfigMatchVehicleShown = true;
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_ok, (s, a) =>
                {
                    SelectConfigFileIntent();
                })
                .SetNegativeButton(Resource.String.button_abort, (s, a) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.config_match_vehicle)
                .SetTitle(Resource.String.alert_title_info)
                .Show();
        }

        private void SelectConfigFileIntent()
        {
            // Launch the FilePickerActivity to select a configuration
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = _instanceData.AppDataPath;
            try
            {
                if (!string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    initDir = Path.GetDirectoryName(_instanceData.ConfigFileName);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".cccfg");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectConfig);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        private void EditConfigFileIntent()
        {
            // Launch the FilePickerActivity to select a configuration
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = _instanceData.AppDataPath;
            try
            {
                if (!string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    initDir = Path.GetDirectoryName(_instanceData.ConfigFileName);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".cccfg;.ccpages;.ccpage");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEditConfig);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        private void StartXmlTool()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }

            if (_activityCommon.InitReaderThread(_instanceData.BmwPath, _instanceData.VagPath, result =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (result)
                {
                    StartXmlTool();
                }
            }))
            {
                return;
            }

            Intent serverIntent = new Intent(this, typeof(XmlToolActivity));
            serverIntent.PutExtra(XmlToolActivity.ExtraInitDir, _instanceData.EcuPath);
            serverIntent.PutExtra(XmlToolActivity.ExtraVagDir, _instanceData.VagPath);
            serverIntent.PutExtra(XmlToolActivity.ExtraBmwDir, _instanceData.BmwPath);
            serverIntent.PutExtra(XmlToolActivity.ExtraAppDataDir, _instanceData.AppDataPath);
            serverIntent.PutExtra(XmlToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(XmlToolActivity.ExtraDeviceName, _instanceData.DeviceName);
            serverIntent.PutExtra(XmlToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
            serverIntent.PutExtra(XmlToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            serverIntent.PutExtra(XmlToolActivity.ExtraFileName, _instanceData.ConfigFileName);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestXmlTool);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        private void StartEdiabasTool()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
            EdiabasToolActivity.IntentTranslateActivty = _activityCommon;
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInitDir, _instanceData.EcuPath);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraAppDataDir, _instanceData.AppDataPath);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceName, _instanceData.DeviceName);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEdiabasTool);
            ActivityCommon.ActivityStartedFromMain = true;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool StartEditXml(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                Intent viewIntent = new Intent(Intent.ActionView);
                Android.Net.Uri fileUri = Android.Net.Uri.FromFile(new Java.IO.File(fileName));
                string mimeType = Android.Webkit.MimeTypeMap.Singleton.GetMimeTypeFromExtension("xml");
                viewIntent.SetDataAndType(fileUri, mimeType);
                viewIntent.SetFlags(ActivityFlags.NewTask);

                IList<ResolveInfo> activities = _activityCommon.PackageManager?.QueryIntentActivities(viewIntent, PackageInfoFlags.MatchDefaultOnly);
                if (activities == null || activities.Count == 0)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.no_xml_editor_installed), Resource.String.alert_title_error);
                    return false;
                }

                if (StoreXmlEditor && !string.IsNullOrEmpty(_instanceData.XmlEditorPackageName) && !string.IsNullOrEmpty(_instanceData.XmlEditorClassName))
                {
                    try
                    {
                        viewIntent.SetComponent(new ComponentName(_instanceData.XmlEditorPackageName, _instanceData.XmlEditorClassName));
                        StartActivityForResult(viewIntent, (int)ActivityRequest.RequestEditXml);
                        ActivityCommon.ActivityStartedFromMain = true;
                        return true;
                    }
                    catch (Exception)
                    {
                        _instanceData.XmlEditorPackageName = string.Empty;
                        _instanceData.XmlEditorClassName = string.Empty;
                        UpdateOptionsMenu();
                    }
                }

                Intent chooseIntent;
                if (StoreXmlEditor)
                {
                    Intent receiver = new Intent(Android.App.Application.Context, typeof(ChooseReceiver));
                    Android.App.PendingIntent pendingIntent =
                        Android.App.PendingIntent.GetBroadcast(Android.App.Application.Context, 0, receiver, Android.App.PendingIntentFlags.UpdateCurrent);
                    chooseIntent = Intent.CreateChooser(viewIntent, GetString(Resource.String.choose_xml_editor), pendingIntent.IntentSender);
                }
                else
                {
                    chooseIntent = Intent.CreateChooser(viewIntent, GetString(Resource.String.choose_xml_editor));
                }
                StartActivityForResult(chooseIntent, (int)ActivityRequest.RequestEditXml);
                ActivityCommon.ActivityStartedFromMain = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public class TabsFragmentPagerAdapter : FragmentStatePagerAdapter
        {
            private class TabPageInfo
            {
                public TabPageInfo(Fragment fragment, string title)
                {
                    Fragment = fragment;
                    Title = title;
                }

                public Fragment Fragment { get; }
                public string Title { get; }
            }

            private readonly List<TabPageInfo> _pageList;

            public TabsFragmentPagerAdapter(FragmentManager fm) : base(fm)
            {
                _pageList = new List<TabPageInfo>();
            }

            public override int Count => _pageList.Count;

            public override void RestoreState(IParcelable state, Java.Lang.ClassLoader loader)
            {
            }

            public override int GetItemPosition(Java.Lang.Object @object)
            {
                return PositionNone;
            }

            public override Fragment GetItem(int position)
            {
                if (position >= _pageList.Count)
                {
                    return null;
                }
                return _pageList[position].Fragment;
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                if (position >= _pageList.Count)
                {
                    return null;
                }
                return new Java.Lang.String(_pageList[position].Title);
            }

            public void ClearPages()
            {
                _pageList.Clear();
            }

            public void AddPage(Fragment fragment, string title)
            {
                _pageList.Add(new TabPageInfo(fragment, title));
            }
        }

        public class TabContentFragment : Fragment
        {
            private int _resourceId;
            private int _pageInfoIndex;

            public TabContentFragment()
            {
                _resourceId = -1;
                _pageInfoIndex = -1;
            }

            public static TabContentFragment NewInstance(int resourceId, int pageInfoIndex)
            {
                TabContentFragment fragment = new TabContentFragment();
                Bundle bundle = new Bundle();
                bundle.PutInt("ResourceId", resourceId);
                bundle.PutInt("PageInfoIndex", pageInfoIndex);
                fragment.Arguments = bundle;
                return fragment;
            }

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                _resourceId = Arguments.GetInt("ResourceId", -1);
                _pageInfoIndex = Arguments.GetInt("PageInfoIndex", -1);
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                View view = inflater.Inflate(_resourceId, null);
                if (Activity is ActivityMain activityMain && activityMain == ActivityCommon.ActivityMainCurrent)
                {
                    if (_pageInfoIndex >= 0 && _pageInfoIndex < ActivityCommon.JobReader.PageList.Count)
                    {
                        JobReader.PageInfo pageInfo = ActivityCommon.JobReader.PageList[_pageInfoIndex];
                        if (pageInfo.ClassObject != null)
                        {
                            try
                            {
                                Type pageType = pageInfo.ClassObject.GetType();
                                MethodInfo createLayout = pageType.GetMethod("CreateLayout");
                                if (createLayout != null)
                                {
                                    LinearLayout pageLayout = view.FindViewById<LinearLayout>(Resource.Id.listLayout);
                                    object[] args = { activityMain, pageInfo, pageLayout };
                                    createLayout.Invoke(pageInfo.ClassObject, args);
                                    //pageInfo.ClassObject.CreateLayout(activityMain, pageInfo, pageLayout);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                    activityMain._updateHandler.Post(() =>
                    {
                        activityMain.UpdateDisplay();
                    });
                }

                return view;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();

                if (Activity is ActivityMain activityMain && activityMain == ActivityCommon.ActivityMainCurrent &&
                    _pageInfoIndex >= 0 && _pageInfoIndex < ActivityCommon.JobReader.PageList.Count)
                {
                    JobReader.PageInfo pageInfo = ActivityCommon.JobReader.PageList[_pageInfoIndex];
                    if (pageInfo.ClassObject != null)
                    {
                        try
                        {
                            Type pageType = pageInfo.ClassObject.GetType();
                            MethodInfo destroyLayout = pageType.GetMethod("DestroyLayout");
                            if (destroyLayout != null)
                            {
                                object[] args = { pageInfo };
                                destroyLayout.Invoke(pageInfo.ClassObject, args);
                                //pageInfo.ClassObject.DestroyLayout(pageInfo);
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

        public class ConnectActionProvider : Android.Support.V4.View.ActionProvider
        {
            public ConnectActionProvider(Context context)
                : base(context)
            {
            }

            public override View OnCreateActionView()
            {
                ActivityMain activityMain = (ActivityMain) Context;
                // Inflate the action view to be shown on the action bar.
                LayoutInflater layoutInflater = LayoutInflater.From(Context);
                View view = layoutInflater.Inflate(Resource.Layout.connect_action_provider, null);
                ToggleButton button = view.FindViewById<ToggleButton>(Resource.Id.buttonConnect);
                button.Click += (sender, args) =>
                {
                    activityMain.ButtonConnectClick(sender, args);
                };
                button.Checked = activityMain._connectButtonInfo.Checked;
                button.Enabled = activityMain._connectButtonInfo.Enabled;
                activityMain._connectButtonInfo.Button = button;

                return view;
            }

            public override bool OnPerformDefaultAction()
            {
                // This is called if the host menu item placed in the overflow menu of the
                // action bar is clicked and the host activity did not handle the click.
                ((ActivityMain)Context).ButtonConnectClick(((ActivityMain)Context)._connectButtonInfo.Button, new EventArgs());
                return true;
            }
        }

        [BroadcastReceiver(Exported = false)]
        public class ChooseReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    if (intent?.GetParcelableExtra(Intent.ExtraChosenComponent) is ComponentName clickedComponent)
                    {
                        string packageName = clickedComponent.PackageName;
                        string className = clickedComponent.ClassName;
                        if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(className))
                        {
                            Intent broadcastIntent = new Intent(ActivityCommon.ActionPackageName);
                            broadcastIntent.PutExtra(ActivityCommon.BroadcastXmlEditorPackageName, packageName);
                            broadcastIntent.PutExtra(ActivityCommon.BroadcastXmlEditorClassName, className);
                            LocalBroadcastManager.GetInstance(context).SendBroadcast(broadcastIntent);
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
