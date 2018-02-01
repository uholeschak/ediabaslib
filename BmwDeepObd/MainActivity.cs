//#define APP_USB_FILTER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using BmwDeepObd.FilePicker;
using EdiabasLib;
using Java.Interop;
using Mono.CSharp;
// ReSharper disable MergeCastWithTypeCheck

#if APP_USB_FILTER
[assembly: Android.App.UsesFeature("android.hardware.usb.host")]
#endif

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name", MainLauncher = false,
            LaunchMode = LaunchMode.SingleTask,
            UiOptions=UiOptions.SplitActionBarWhenNarrow,
            ConfigurationChanges = ConfigChanges.KeyboardHidden |
                ConfigChanges.Orientation |
                ConfigChanges.ScreenSize)]
    [Android.App.MetaData("android.support.UI_OPTIONS", Value = "splitActionBarWhenNarrow")]
#if APP_USB_FILTER
    [Android.App.IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Android.App.MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
#endif
    public class ActivityMain : AppCompatActivity, TabLayout.IOnTabSelectedListener
    {
        private enum ActivityRequest
        {
            RequestSelectDevice,
            RequestAdapterConfig,
            RequestSelectConfig,
            RequestXmlTool,
            RequestEdiabasTool,
#if !OBB_MODE
            RequestSelectEcuZip,
#endif
            RequestYandexKey,
            RequestGlobalSettings,
        }

        public enum LastAppState
        {
            Init,
            Compile,
            TabsCreated,
            Terminated,
        }

        private class DownloadInfo
        {
            public DownloadInfo(string fileName, string downloadDir, string targetDir, XElement infoXml = null)
            {
                FileName = fileName;
                DownloadDir = downloadDir;
                TargetDir = targetDir;
                InfoXml = infoXml;
            }

            public string FileName { get; }

            public string TargetDir { get; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public string DownloadDir { get; }

            public XElement InfoXml { get; }
        }

#if !OBB_MODE
        private class DownloadUrlInfo
        {
            public DownloadUrlInfo(string url, long fileSize, string name, string password)
            {
                Url = url;
                FileSize = fileSize;
                Name = name;
                Password = password;
            }

            public string Url { get; }

            public long FileSize { get; }

            public string Name { get; }

            public string Password { get; }
        }
#endif

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
                LastAppState = LastAppState.Init;
                AppDataPath = string.Empty;
                EcuPath = string.Empty;
                TraceActive = true;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                ConfigFileName = string.Empty;
                CheckCpuUsage = true;
                VerifyEcuFiles = true;
            }

            public LastAppState LastAppState { get; set; }
            public string AppDataPath { get; set; }
            public string EcuPath { get; set; }
            public bool UserEcuFiles { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool DataLogActive { get; set; }
            public bool DataLogAppend { get; set; }
            public string DeviceName { get; set; }
            public string DeviceAddress { get; set; }
            public string ConfigFileName { get; set; }
            public int LastVersionCode { get; set; }
            public bool CheckCpuUsage { get; set; }
            public bool VerifyEcuFiles { get; set; }
            public bool CommErrorsOccured { get; set; }
            public bool StorageAccessGranted { get; set; }
            public bool AutoStart { get; set; }
            public bool VagInfoShown { get; set; }
            public string DataLogDir { get; set; }
            public string TraceDir { get; set; }

            public ActivityCommon.InterfaceType SelectedInterface { get; set; }
        }

        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const string SharedAppName = ActivityCommon.AppNameSpace;
        private const string AppFolderName = ActivityCommon.AppNameSpace;
#if OBB_MODE
        private const string EcuDirNameBmw = "EcuBmw";
        private const string EcuDirNameVag = "EcuVag";
        private const string EcuDownloadUrl = @"http://www.holeschak.de/BmwDeepObd/Obb1.xml";
        private const long EcuExtractSize = 2500000000;         // extracted ecu files size
        private const string InfoXmlName = "ObbInfo.xml";
        private const string ContentFileName = "Content.xml";
#else
        private const string EcuDirNameBmw = "Ecu";
        private const string EcuDirNameVag = "EcuVag";
        private const string EcuDownloadUrlBmw = @"http://www.holeschak.de/BmwDeepObd/Ecu3.xml";
        private const string EcuDownloadUrlVag = @"http://www.holeschak.de/BmwDeepObd/EcuVag1.xml";
        private const string InfoXmlName = "Info.xml";
        private const long EcuZipSizeBmw = 130000000;           // BMW ecu zip file size
        private const long EcuExtractSizeBmw = 1200000000;      // BMW extracted ecu files size
        private const long EcuZipSizeVag = 53000000;            // VAG ecu zip file size
        private const long EcuExtractSizeVag = 910000000;       // VAG extracted ecu files size
#endif
        private const int CpuLoadCritical = 70;
        private const int RequestPermissionExternalStorage = 0;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage
        };

        public const string ExtraStopComm = "stop_communication";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
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
        private Handler _updateHandler;
        private TabLayout _tabLayout;
        private ViewPager _viewPager;
        private TabsFragmentPagerAdapter _fragmentPagerAdapter;
        private readonly ConnectButtonInfo _connectButtonInfo = new ConnectButtonInfo();
        private ImageView _imageBackground;
        private WebClient _webClient;
        private CustomProgressDialog _downloadProgress;
        private CustomProgressDialog _compileProgress;
        private bool _extractZipCanceled;
        private long _downloadFileSize;
        private string _obbFileName;
#if !OBB_MODE
        private List<DownloadUrlInfo> _downloadUrlInfoList;
#endif
        private AlertDialog _startAlertDialog;
        private AlertDialog _configSelectAlertDialog;
        private AlertDialog _downloadEcuAlertDialog;
        private bool _translateActive;
        private List<string> _translationList;
        private List<string> _translatedList;

        private string ManufacturerEcuDirName
        {
            get
            {
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Bmw:
#if OBB_MODE
                        return Path.Combine(ActivityCommon.EcuBaseDir, EcuDirNameBmw);
#else
                        return EcuDirNameBmw;
#endif
                }
#if OBB_MODE
                return Path.Combine(ActivityCommon.EcuBaseDir, EcuDirNameVag);
#else
                return EcuDirNameVag;
#endif
            }
        }

#if !OBB_MODE
        private string ManufacturerEcuDownloadUrl
        {
            get
            {
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Bmw:
                        return EcuDownloadUrlBmw;
                }
                return EcuDownloadUrlVag;
            }
        }

        private long ManufacturerEcuZipSize
        {
            get
            {
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Bmw:
                        return EcuZipSizeBmw;
                }
                return EcuZipSizeVag;
            }
        }

        private long ManufacturerEcuExtractSize
        {
            get
            {
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Bmw:
                        return EcuExtractSizeBmw;
                }
                return EcuExtractSizeVag;
            }
        }
#endif

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
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
            ClearPage(tab.Position);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SetContentView(Resource.Layout.main);

            _fragmentPagerAdapter = new TabsFragmentPagerAdapter(SupportFragmentManager);
            _viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            _viewPager.Adapter = _fragmentPagerAdapter;
            _tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            _tabLayout.SetupWithViewPager(_viewPager);
            _tabLayout.AddOnTabSelectedListener(this);
            _tabLayout.Visibility = ViewStates.Gone;

            ActivityCommon.ActivityMainCurrent = this;
            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                    UpdateDisplay();
                }
            }, BroadcastReceived);
            _activityCommon.RegisterInternetCellular();
            if (_activityRecreated && _instanceData != null)
            {
                _activityCommon.SelectedInterface = _instanceData.SelectedInterface;
            }

            GetSettings();
            StoreLastAppState(LastAppState.Init);

            _updateHandler = new Handler();

            _imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += DownloadProgressChanged;
            _webClient.DownloadFileCompleted += DownloadCompleted;

            _stopCommRequest = Intent.GetBooleanExtra(ExtraStopComm, false);
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
            ActivityCommon.StoreInstanceState(outState, _instanceData);
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
            StoreSettings();
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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!UseCommService())
            {
                StopEdiabasThread(true);
            }
            DisconnectEdiabasEvents();
            if (_webClient != null)
            {
                if (_webClient.IsBusy)
                {
                    _webClient.CancelAsync();
                }
                _webClient.Dispose();
                _webClient = null;
            }
            _extractZipCanceled = true;
            StoreSettings();
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
            StoreLastAppState(LastAppState.Terminated);
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
                if (Stopwatch.GetTimestamp() - _lastBackPressedTime < 2000 * TickResolMs)
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

            UpdateDisplay();
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
                        _instanceData.ConfigFileName = data.Extras.GetString(XmlToolActivity.ExtraFileName);
                        ReadConfigFile();
                        UpdateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestEdiabasTool:
                    _activityCommon.SetPreferredNetworkInterface();
                    break;
#if !OBB_MODE
                case ActivityRequest.RequestSelectEcuZip:
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        string zipFile = data.Extras.GetString(XmlToolActivity.ExtraFileName);
                        string fileName = Path.GetFileName(zipFile) ?? string.Empty;
                        string ecuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);

                        XElement xmlInfo = new XElement("Info");
                        xmlInfo.Add(new XAttribute("Url", zipFile));
                        xmlInfo.Add(new XAttribute("Name", fileName));

                        ExtractZipFile(zipFile, ecuPath, xmlInfo, null);
                    }
                    break;
#endif
                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey);
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestGlobalSettings:
                    UpdateDirectories();
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    CheckForEcuFiles();
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

            IMenuItem selCfgMenu = menu.FindItem(Resource.Id.menu_sel_cfg);
            if (selCfgMenu != null)
            {
                string fileName = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    fileName = Path.GetFileNameWithoutExtension(_instanceData.ConfigFileName);
                }
                selCfgMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_sel_cfg), fileName));
                selCfgMenu.SetEnabled(!commActive);
            }

            IMenuItem xmlToolMenu = menu.FindItem(Resource.Id.menu_xml_tool);
            xmlToolMenu?.SetEnabled(!commActive);

            IMenuItem ediabasToolMenu = menu.FindItem(Resource.Id.menu_ediabas_tool);
            ediabasToolMenu?.SetEnabled(!commActive);

            IMenuItem downloadEcu = menu.FindItem(Resource.Id.menu_download_ecu);
            if (downloadEcu != null)
            {
#if OBB_MODE
                downloadEcu.SetTitle(Resource.String.menu_extract_ecu);
#else
                downloadEcu.SetTitle(Resource.String.menu_download_ecu);
#endif
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

                case Resource.Id.menu_sel_cfg:
                    SelectConfigFile();
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
                        StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://github.com/uholeschak/ediabaslib/blob/master/docs/Deep_OBD_for_BMW_and_VAG.md")));
                    });
                    return true;

                case Resource.Id.menu_info:
                {
                    string message = string.Format(GetString(Resource.String.app_info_message),
                        PackageManager.GetPackageInfo(PackageName, 0).VersionName, ActivityCommon.AppId);
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
        }

        [Export("onCopyErrorsClick")]
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
            if (!_activityCommon.RequestInterfaceEnable((sender, args) =>
            {
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
                            connectParameter = new EdBluetoothInterface.ConnectParameterType(_activityCommon.MaConnectivity, _activityCommon.MtcBtService,
                                () => ActivityCommon.EdiabasThread.ActiveContext);
                            _activityCommon.ConnectMtcBtDevice(_instanceData.DeviceAddress);
                            break;

                        case ActivityCommon.InterfaceType.Enet:
                            connectParameter = new EdInterfaceEnet.ConnectParameterType(_activityCommon.MaConnectivity);
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
                            connectParameter = new EdElmWifiInterface.ConnectParameterType(_activityCommon.MaConnectivity);
                            break;

                        case ActivityCommon.InterfaceType.DeepObdWifi:
                            portName = "DEEPOBDWIFI";
                            connectParameter = new EdCustomWiFiInterface.ConnectParameterType(_activityCommon.MaConnectivity);
                            break;

                        case ActivityCommon.InterfaceType.Ftdi:
                            portName = "FTDI0";
                            connectParameter = new EdFtdiInterface.ConnectParameterType(_activityCommon.UsbManager);
                            break;
                    }
                    ActivityCommon.EdiabasThread.StartThread(portName, connectParameter, pageInfo, true, _instanceData.TraceDir, _instanceData.TraceAppend, _instanceData.DataLogDir, _instanceData.DataLogAppend);
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

        private void GetSettings()
        {
            try
            {
                _currentVersionCode = PackageManager.GetPackageInfo(PackageName, 0).VersionCode;
                _obbFileName = ExpansionDownloaderActivity.GetObbFilename(this);
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                _activityCommon.SelectedEnetIp = prefs.GetString("EnetIp", string.Empty);
                _activityCommon.CustomStorageMedia = prefs.GetString("StorageMedia", string.Empty);
                if (!_activityRecreated)
                {
                    string stateString = prefs.GetString("LastAppState", string.Empty);
                    _instanceData.LastAppState = System.Enum.TryParse(stateString, true, out LastAppState lastAppState) ? lastAppState : LastAppState.Init;
                    _instanceData.DeviceName = prefs.GetString("DeviceName", string.Empty);
                    _instanceData.DeviceAddress = prefs.GetString("DeviceAddress", string.Empty);
                    _instanceData.ConfigFileName = prefs.GetString("ConfigFile", string.Empty);
                    _instanceData.LastVersionCode = prefs.GetInt("VersionCode", -1);

                    ActivityCommon.BtNoEvents = prefs.GetBoolean("BtNoEvents", false);
                    ActivityCommon.EnableTranslation = prefs.GetBoolean("EnableTranslation", false);
                    ActivityCommon.YandexApiKey = prefs.GetString("YandexApiKey", string.Empty);
                    ActivityCommon.AppId = prefs.GetString("AppId", string.Empty);
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
                    ActivityCommon.DoubleClickForAppExit = prefs.GetBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
                    ActivityCommon.SendDataBroadcast = prefs.GetBoolean("SendDataBroadcast", ActivityCommon.SendDataBroadcast);
                    ActivityCommon.CheckCpuUsage = prefs.GetBoolean("CheckCpuUsage", true);
                    ActivityCommon.CheckEcuFiles = prefs.GetBoolean("CheckEcuFiles", true);
                    ActivityCommon.CollectDebugInfo = prefs.GetBoolean("CollectDebugInfo", ActivityCommon.CollectDebugInfo);
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
                prefsEdit.PutString("DeviceName", _instanceData.DeviceName);
                prefsEdit.PutString("DeviceAddress", _instanceData.DeviceAddress);
                prefsEdit.PutString("EnetIp", _activityCommon.SelectedEnetIp);
                prefsEdit.PutString("ConfigFile", _instanceData.ConfigFileName);
                prefsEdit.PutString("StorageMedia", _activityCommon.CustomStorageMedia ?? string.Empty);
                prefsEdit.PutInt("VersionCode", _currentVersionCode);
                prefsEdit.PutBoolean("BtNoEvents", ActivityCommon.BtNoEvents);
                prefsEdit.PutBoolean("EnableTranslation", ActivityCommon.EnableTranslation);
                prefsEdit.PutString("YandexApiKey", ActivityCommon.YandexApiKey ?? string.Empty);
                prefsEdit.PutString("AppId", ActivityCommon.AppId);
                prefsEdit.PutInt("Manufacturer", (int) ActivityCommon.SelectedManufacturer);
                prefsEdit.PutInt("BtEnable", (int)ActivityCommon.BtEnbaleHandling);
                prefsEdit.PutInt("BtDisable", (int) ActivityCommon.BtDisableHandling);
                prefsEdit.PutInt("LockComm", (int)ActivityCommon.LockTypeCommunication);
                prefsEdit.PutInt("LockLog", (int)ActivityCommon.LockTypeLogging);
                prefsEdit.PutBoolean("StoreDataLogSettings", ActivityCommon.StoreDataLogSettings);
                prefsEdit.PutBoolean("DataLogActive", _instanceData.DataLogActive);
                prefsEdit.PutBoolean("DataLogAppend", _instanceData.DataLogAppend);
                prefsEdit.PutInt("AutoConnect", (int)ActivityCommon.AutoConnectHandling);
                prefsEdit.PutBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
                prefsEdit.PutBoolean("SendDataBroadcast", ActivityCommon.SendDataBroadcast);
                prefsEdit.PutBoolean("CheckCpuUsage", ActivityCommon.CheckCpuUsage);
                prefsEdit.PutBoolean("CheckEcuFiles", ActivityCommon.CheckEcuFiles);
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
            try
            {
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.PutString("LastAppState", lastAppState.ToString());
                prefsEdit.Commit();
            }
            catch (Exception)
            {
                // ignored
            }
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
                        _instanceData.EcuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
                    }
                }
                else
                {
                    _instanceData.AppDataPath = ActivityCommon.ExternalWritePath;
                    _instanceData.EcuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
#if !OBB_MODE
                    if (!ValidEcuFiles(_instanceData.EcuPath))
                    {
                        string userEcuPath = Path.Combine(_instanceData.AppDataPath, "../../../..", AppFolderName, ManufacturerEcuDirName);
                        if (ValidEcuFiles(userEcuPath))
                        {
                            _instanceData.EcuPath = userEcuPath;
                            _instanceData.UserEcuFiles = true;
                        }
                    }
#endif
                }
            }
            else
            {
                _instanceData.AppDataPath = Path.Combine(_activityCommon.CustomStorageMedia, AppFolderName);
                _instanceData.EcuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
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
                    if (request != null && request.Equals(ForegroundService.BroadcastStopComm))
                    {
                        StopEdiabasThread(false);
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
            }
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            RunOnUiThread(UpdateDisplay);
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
                _activityCommon.RequestSendTraceFile(_instanceData.AppDataPath, _instanceData.TraceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType());
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (ActivityCommon.CommActive)
            {
                return false;
            }
            if (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                return _activityCommon.SendTraceFile(_instanceData.AppDataPath, _instanceData.TraceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
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

        private void UpdateDisplay()
        {
            if (!_activityActive || (_activityCommon == null))
            {   // OnDestroy already executed
                return;
            }
            bool dynamicValid = false;

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
                Button buttonErrorCopy = null;
                if (pageInfo.ErrorsInfo != null)
                {
                    buttonErrorReset = dynamicFragment.View.FindViewById<Button>(Resource.Id.button_error_reset);
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
                            foreach (EdiabasThread.EdiabasErrorReport errorReport in errorReportList)
                            {
                                if (ActivityCommon.IsCommunicationError(errorReport.ExecptionText))
                                {
                                    _instanceData.CommErrorsOccured = true;
                                }
                                StringBuilder srMessage = new StringBuilder();
                                srMessage.Append(string.Format(Culture, "{0}: ", GetPageString(pageInfo, errorReport.EcuName)));
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
                                        bool saeMode = false;
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
                                                    saeMode = true;
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
                                                    errorCode <<= 8;
                                                }
                                            }
                                        }
                                        List<string> textList = _activityCommon.ConvertVagDtcCode(_instanceData.EcuPath, errorCode, errorTypeList, kwp1281, saeMode);

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
                                        string text1 = FormatResultString(errorReport.ErrorDict, "F_ORT_TEXT", "{0}");
                                        string text2 = FormatResultString(errorReport.ErrorDict, "F_VORHANDEN_TEXT", "{0}");
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
                                        srMessage.Append("\r\n");
                                        srMessage.Append(text1);
                                        srMessage.Append(", ");
                                        srMessage.Append(text2);

                                        if (errorReport.ErrorDetailSet != null)
                                        {
                                            string detailText = string.Empty;
                                            foreach (Dictionary<string, EdiabasNet.ResultData> errorDetail in errorReport.ErrorDetailSet)
                                            {
                                                string kmText = FormatResultInt64(errorDetail, "F_UW_KM", "{0}");
                                                if (kmText.Length > 0)
                                                {
                                                    if (detailText.Length > 0)
                                                    {
                                                        detailText += ", ";
                                                    }
                                                    detailText += kmText + " km";
                                                }
                                            }
                                            if (detailText.Length > 0)
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
                                        //message = pageInfo.ClassObject.FormatErrorResult(pageInfo, errorReport, message);
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
                                        UpdateButtonErrorReset(buttonErrorReset, resultListAdapter.Items);
                                    };
                                    tempResultList.Add(newResultItem);
                                }
                                lastEcuName = errorReport.EcuName;
                            }
                            if (tempResultList.Count == 0)
                            {
                                tempResultList.Add(new TableResultItem(GetString(Resource.String.error_no_error), null));
                            }
                        }
                        UpdateButtonErrorReset(buttonErrorReset, tempResultList);
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
                                            _updateHandler?.Post(UpdateDisplay);
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
                            string result = ActivityCommon.FormatResult(pageInfo, displayInfo, resultDict, out Android.Graphics.Color? textColor);
                            if (result != null)
                            {
                                if (resultGridAdapter != null)
                                {
                                    if (displayInfo.GridType != JobReader.DisplayInfo.GridModeType.Hidden)
                                    {
                                        double value = GetResultDouble(resultDict, displayInfo.Result, 0, out bool foundDouble);
                                        if (!foundDouble)
                                        {
                                            Int64 valueInt64 = GetResultInt64(resultDict, displayInfo.Result, 0, out bool foundInt64);
                                            if (foundInt64)
                                            {
                                                value = valueInt64;
                                            }
                                        }

                                        int resourceId = Resource.Layout.result_customgauge_square;
                                        switch (displayInfo.GridType)
                                        {
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
                                        tempResultGrid.Add(new GridResultItem(resourceId, GetPageString(pageInfo, displayInfo.Name), result, displayInfo.MinValue, displayInfo.MaxValue, value, gaugeSize));
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
                            }
                        }
                        if (resultChanged)
                        {
                            resultGridAdapter.Items.Clear();
                            foreach (GridResultItem resultItem in tempResultGrid)
                            {
                                resultGridAdapter.Items.Add(resultItem);
                            }
                            resultGridAdapter.NotifyDataSetChanged();
                            gridViewResult.SetColumnWidth(gaugeSize);
                        }
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
                            }
                        }
                        if (resultChanged)
                        {
                            resultListAdapter.Items.Clear();
                            foreach (TableResultItem resultItem in tempResultList)
                            {
                                resultListAdapter.Items.Add(resultItem);
                            }
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
            buttonErrorReset.Enabled = selected;
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

        private void ReadConfigFile()
        {
            if (ActivityCommon.CommActive)
            {
                UpdateJobReaderSettings();
                _updateHandler.Post(CreateActionBarTabs);
                return;
            }
            ActivityCommon.JobReader.Clear();
            if (_instanceData.LastAppState != LastAppState.Compile)
            {
                ActivityCommon.JobReader.ReadXml(_instanceData.ConfigFileName);
            }
            UpdateJobReaderSettings();
            _activityCommon.ClearTranslationCache();
            _translationList = null;
            _translatedList = null;
            UpdateDirectories();
            RequestConfigSelect();
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
#if OBB_MODE
                if (_instanceData.VerifyEcuFiles)
                {
                    _instanceData.VerifyEcuFiles = false;
                    string ecuBaseDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir);
                    if (ValidEcuPackage(ecuBaseDir))
                    {
                        if (!ActivityCommon.VerifyContent(Path.Combine(ecuBaseDir, ContentFileName), false, percent =>
                        {
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                if (_compileProgress != null)
                                {
                                    _compileProgress.SetMessage(GetString(Resource.String.verify_files));
                                    _compileProgress.Progress = percent;
                                }
                            });
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
#endif
                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (_compileProgress != null)
                    {
                        _compileProgress.SetMessage(GetString(Resource.String.compile_start));
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
                            if (cpuUsage >= 0 && Stopwatch.GetTimestamp() - startTime > 1000 * TickResolMs)
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

        private void DownloadFile(string url, string downloadDir, string unzipTargetDir = null, long fileSize = -1)
        {
            if (string.IsNullOrEmpty(_obbFileName))
            {
                _activityCommon.ShowAlert(GetString(Resource.String.download_failed), Resource.String.alert_title_error);
                return;
            }
            try
            {
                Directory.CreateDirectory(downloadDir);
            }
            catch (Exception)
            {
                _activityCommon.ShowAlert(GetString(Resource.String.download_failed), Resource.String.alert_title_error);
                return;
            }

            if (_downloadProgress == null)
            {
                _downloadProgress = new CustomProgressDialog(this);
                _downloadProgress.AbortClick = sender => 
                {
                    if (_webClient.IsBusy)
                    {
                        _webClient.CancelAsync();
                    }
                    _downloadProgress?.Dismiss();
                    _downloadProgress = null;
                    UpdateLockState();
                };
#if !OBB_MODE
                _downloadUrlInfoList = null;
#endif
            }
            _downloadProgress.SetMessage(GetString(Resource.String.downloading_file));
            _downloadProgress.Indeterminate = false;
            _downloadProgress.Progress = 0;
            _downloadProgress.Max = 100;
            _downloadProgress.Show();
            _downloadProgress.ButtonAbort.Enabled = false;    // early abort crashes!
            _downloadFileSize = fileSize;
            UpdateLockState();
            _activityCommon.SetPreferredNetworkInterface();

            Thread downloadThread = new Thread(() =>
            {
                try
                {
                    string fileName = Path.GetFileName(url) ?? string.Empty;
                    string fileNameFull = Path.Combine(downloadDir, fileName);
                    DownloadInfo downloadInfo = null;
                    if (!string.IsNullOrEmpty(unzipTargetDir))
                    {
                        XElement xmlInfo = new XElement("Info");
                        xmlInfo.Add(new XAttribute("Url", url));
#if OBB_MODE
                        xmlInfo.Add(new XAttribute("Name", _obbFileName));
#else
                        xmlInfo.Add(new XAttribute("Name", fileName));
#endif
                        downloadInfo = new DownloadInfo(fileNameFull, downloadDir, unzipTargetDir, xmlInfo);
                    }
                    // ReSharper disable once RedundantNameQualifier
                    string extension = Path.GetExtension(fileName);
                    if (string.Compare(extension, ".xml", StringComparison.OrdinalIgnoreCase) == 0)
                    {   // XML URL file
                        _webClient.Credentials = null;
                    }
                    _webClient.DownloadFileAsync(new Uri(url), fileNameFull, downloadInfo);
                }
                catch (Exception)
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
                        _activityCommon.ShowAlert(GetString(Resource.String.download_failed), Resource.String.alert_title_error);
                    });
                }
            });
            downloadThread.Start();
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                DownloadInfo downloadInfo = e.UserState as DownloadInfo;
                if (_downloadProgress != null)
                {
                    bool error = false;
                    _downloadProgress.ButtonAbort.Enabled = false;
                    if (downloadInfo != null)
                    {
#if OBB_MODE
                        if (e.Error == null)
                        {
                            string key = GetObbKey(downloadInfo.FileName);
                            try
                            {
                                if (File.Exists(downloadInfo.FileName))
                                {
                                    File.Delete(downloadInfo.FileName);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            if (key != null)
                            {
                                ExtractZipFile(_obbFileName, downloadInfo.TargetDir, downloadInfo.InfoXml, key, false,
                                    new List<string> { Path.Combine(_instanceData.AppDataPath, "EcuVag") });
                                return;
                            }
                            error = true;
                        }
#else
                        if (e.Error == null)
                        {
                            string extension = Path.GetExtension(downloadInfo.FileName) ?? string.Empty;
                            if (string.Compare(extension, ".xml", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                // XML URL file
                                _downloadUrlInfoList = GetDownloadUrls(downloadInfo.FileName);
                                if ((_downloadUrlInfoList == null) || (_downloadUrlInfoList.Count < 1))
                                {
                                    error = true;
                                }
                                try
                                {
                                    if (File.Exists(downloadInfo.FileName))
                                    {
                                        File.Delete(downloadInfo.FileName);
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                                if (!error)
                                {
                                    StartDownload(downloadInfo);
                                    return;
                                }
                            }
                            else if (string.Compare(extension, ".zip", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                // zip file
                                if (!ActivityCommon.CheckZipFile(downloadInfo.FileName))
                                {   // zip file corrupted, try next server
                                    if (!e.Cancelled && (_downloadUrlInfoList != null) && (_downloadUrlInfoList.Count >= 1))
                                    {
                                        StartDownload(downloadInfo);
                                        return;
                                    }
                                }
                                _downloadProgress.ButtonAbort.Enabled = false;
                                ExtractZipFile(downloadInfo.FileName, downloadInfo.TargetDir, downloadInfo.InfoXml, null, true);
                                return;
                            }
                        }
                        else
                        {
                            if (!e.Cancelled && (_downloadUrlInfoList != null) && (_downloadUrlInfoList.Count >= 1))
                            {
                                StartDownload(downloadInfo);
                                return;
                            }
                        }
#endif
                    }
                    _downloadProgress.Dismiss();
                    _downloadProgress.Dispose();
                    _downloadProgress = null;
                    UpdateLockState();
#if !OBB_MODE
                    _downloadUrlInfoList = null;
#endif
                    if ((!e.Cancelled && e.Error != null) || error)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.download_failed), Resource.String.alert_title_error);
                    }
                }
                if (downloadInfo != null)
                {
                    try
                    {
                        if (File.Exists(downloadInfo.FileName))
                        {
                            File.Delete(downloadInfo.FileName);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            });
        }

#if !OBB_MODE
        private void StartDownload(DownloadInfo downloadInfo)
        {
            DownloadUrlInfo urlInfo = _downloadUrlInfoList[0];
            _downloadUrlInfoList.RemoveAt(0);
            _webClient.UseDefaultCredentials = false;
            if (string.IsNullOrEmpty(urlInfo.Name) || string.IsNullOrEmpty(urlInfo.Password))
            {
                _webClient.Credentials = null;
            }
            else
            {
                _webClient.Credentials = new NetworkCredential(urlInfo.Name, urlInfo.Password);
            }
            DownloadFile(urlInfo.Url, downloadInfo.DownloadDir, downloadInfo.TargetDir, urlInfo.FileSize);
        }
#endif

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                if (_downloadProgress != null && e != null)
                {
                    if (e.TotalBytesToReceive < 0 && _downloadFileSize > 0)
                    {
                        _downloadProgress.Progress = (int)(e.BytesReceived * 100 / _downloadFileSize);
                    }
                    else
                    {
                        _downloadProgress.Progress = e.ProgressPercentage;
                    }
                    if (_webClient != null && _webClient.IsBusy)
                    {
                        _downloadProgress.ButtonAbort.Enabled = true;
                    }
                }
            });
        }

#if OBB_MODE
        private string GetObbKey(string xmlFile)
        {
            try
            {
                if (!File.Exists(xmlFile))
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
                XDocument xmlDoc = XDocument.Load(xmlFile);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                foreach (XElement fileNode in xmlDoc.Root.Elements("obb"))
                {
                    XAttribute urlAttr = fileNode.Attribute("name");
                    if (!string.IsNullOrEmpty(urlAttr?.Value))
                    {
                        if (string.Compare(baseName, urlAttr?.Value, StringComparison.OrdinalIgnoreCase) == 0)
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
#else
        private List<DownloadUrlInfo> GetDownloadUrls(string xmlFile)
        {
            List<DownloadUrlInfo> urlInfo = new List<DownloadUrlInfo>();
            try
            {
                if (!File.Exists(xmlFile))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Load(xmlFile);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                foreach (XElement fileNode in xmlDoc.Root.Elements("file_v2"))
                {
                    XAttribute urlAttr = fileNode.Attribute("url");
                    if (!string.IsNullOrEmpty(urlAttr?.Value))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        string url = urlAttr.Value;
                        string name = null;
                        string password = null;
                        long fileSize = -1;
                        XAttribute sizeAttr = fileNode.Attribute("file_size");
                        if (sizeAttr != null)
                        {
                            fileSize = System.Xml.XmlConvert.ToInt64(sizeAttr.Value);
                        }
                        XAttribute usernameAttr = fileNode.Attribute("username");
                        if (usernameAttr != null)
                        {
                            name = usernameAttr.Value;
                        }
                        XAttribute passwordAttr = fileNode.Attribute("password");
                        if (passwordAttr != null)
                        {
                            password = passwordAttr.Value;
                        }
                        urlInfo.Add(new DownloadUrlInfo(url, fileSize, name, password));
                    }
                }
                return urlInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif

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
                _extractZipCanceled = true;
            };
            _downloadProgress.Show();
            _downloadProgress.ButtonAbort.Enabled = false;

            Thread extractThread = new Thread(() =>
            {
                bool extractFailed = false;
                bool ioError = false;
                try
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
                            Directory.Delete(targetDirectory, true);
                        }
                        Directory.CreateDirectory(targetDirectory);
                    }
                    catch (Exception)
                    {
                        ioError = true;
                        throw;
                    }

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

                    ActivityCommon.ExtractZipFile(fileName, targetDirectory, key,
                        (percent, decrypt) =>
                        {
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
                            return _extractZipCanceled;
                        });
#if OBB_MODE
                    if (!ActivityCommon.VerifyContent(Path.Combine(targetDirectory, ContentFileName), true, percent =>
                    {
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
                        return _extractZipCanceled;
                    }))
                    {
                        extractFailed = true;
                    }
#endif
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!extractFailed)
                    {
                        infoXml?.Save(Path.Combine(targetDirectory, InfoXmlName));
                    }
                }
                catch (Exception ex)
                {
                    extractFailed = true;
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
                        _activityCommon.ShowAlert(GetString(ioError ? Resource.String.extract_failed_io : Resource.String.extract_failed), Resource.String.alert_title_error);
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
        private void DownloadEcuFiles(bool manualRequest = false)
        {
            string ecuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
            try
            {
                ActivityCommon.FileSystemBlockInfo blockInfo = ActivityCommon.GetFileSystemBlockInfo(_instanceData.AppDataPath);
                long ecuDirSize = ActivityCommon.GetDirectorySize(ecuPath);
                double freeSpace = blockInfo.AvailableSizeBytes + ecuDirSize;
#if OBB_MODE
                long requiredSize = EcuExtractSize;
#else
                long requiredSize = ManufacturerEcuExtractSize + ManufacturerEcuZipSize;
#endif
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
#if OBB_MODE
            DownloadFile(EcuDownloadUrl, Path.Combine(_instanceData.AppDataPath, ActivityCommon.DownloadDir),
                Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir));
#else
            if (manualRequest)
            {
                string message = string.Format(GetString(Resource.String.download_manual), ManufacturerEcuDownloadUrl.Replace(".xml", ".zip"));

                AlertDialog alertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        ManualEcuFilesInstall();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                        DownloadFile(ManufacturerEcuDownloadUrl, Path.Combine(_instanceData.AppDataPath, ActivityCommon.DownloadDir), ecuPath);
                    })
                    .SetMessage(ActivityCommon.FromHtml(message))
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                TextView messageView = alertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                if (messageView != null)
                {
                    messageView.MovementMethod = new LinkMovementMethod();
                }
            }
            else
            {
                DownloadFile(ManufacturerEcuDownloadUrl, Path.Combine(_instanceData.AppDataPath, ActivityCommon.DownloadDir), ecuPath);
            }
#endif
        }

#if !OBB_MODE
        private void ManualEcuFilesInstall()
        {
            string downloadsDir = _instanceData.AppDataPath;
            Java.IO.File directoryDownloads = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
            if (!string.IsNullOrEmpty(directoryDownloads?.AbsolutePath))
            {
                // ReSharper disable once PossibleNullReferenceException
                downloadsDir = directoryDownloads.AbsolutePath;
            }

            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.select_ecu_zip));
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, downloadsDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".zip");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectEcuZip);
            ActivityCommon.ActivityStartedFromMain = true;
        }
#endif

        // ReSharper disable once UnusedParameter.Local
        private bool CheckForEcuFiles(bool checkPackage = false)
        {
            if (!_activityActive || !_instanceData.StorageAccessGranted || (_downloadEcuAlertDialog != null))
            {
                return true;
            }
#if OBB_MODE
            if (!ValidEcuFiles(_instanceData.EcuPath) || !ValidEcuPackage(Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir)))
            {
                string message = string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_extract), EcuExtractSize);

                _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        DownloadEcuFiles(true);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                _downloadEcuAlertDialog.DismissEvent += (sender, args) => { _downloadEcuAlertDialog = null; };
                return false;
            }
#else
            if (!ValidEcuFiles(_instanceData.EcuPath))
            {
                string message = GetString(Resource.String.ecu_not_found) + "\n" +
                    string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_download), ManufacturerEcuZipSize) + "\n" +
                    string.Format(GetString(Resource.String.manufacturer_select), _activityCommon.ManufacturerName());

                _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        DownloadEcuFiles(true);
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetNeutralButton(Resource.String.button_abort, (sender, args) =>
                    {
                        SelectManufacturerInfo();
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.alert_title_question)
                    .Show();
                _downloadEcuAlertDialog.DismissEvent += (sender, args) => { _downloadEcuAlertDialog = null; };
                return false;
            }

            if (checkPackage && !_instanceData.UserEcuFiles)
            {
                if (!ValidEcuPackage(_instanceData.EcuPath))
                {
                    string message = GetString(Resource.String.ecu_package) + "\n" +
                        string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_download), ManufacturerEcuZipSize);

                    _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            DownloadEcuFiles(true);
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_question)
                        .Show();
                    _downloadEcuAlertDialog.DismissEvent += (sender, args) => { _downloadEcuAlertDialog = null; };
                }
            }
#endif
            return true;
        }

        private bool ValidEcuFiles(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return false;
                }
                return Directory.EnumerateFiles(path, "*.prg").Any();
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
#if OBB_MODE
                if (string.IsNullOrEmpty(_obbFileName) ||
                    string.Compare(Path.GetFileNameWithoutExtension(nameAttr.Value), Path.GetFileNameWithoutExtension(_obbFileName), StringComparison.OrdinalIgnoreCase) != 0)
#else
                if (string.Compare(Path.GetFileNameWithoutExtension(nameAttr.Value), Path.GetFileNameWithoutExtension(ManufacturerEcuDownloadUrl), StringComparison.OrdinalIgnoreCase) != 0)
#endif
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
                if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
                {
                    _activityCommon.SelectedInterface = ActivityCommon.InterfaceType.Bluetooth;
                }
                ActivityCommon.JobReader.Clear();
                _instanceData.ConfigFileName = string.Empty;
                CreateActionBarTabs();
                UpdateDirectories();
                if (CheckForEcuFiles())
                {
                    RequestConfigSelect();
                }
                UpdateOptionsMenu();
                UpdateDisplay();
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
                _configSelectAlertDialog = null;
            };
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

                        case 3:
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
                UpdateOptionsMenu();
            });
        }

        private void SelectConfigFile()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
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

        private void StartXmlTool()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(XmlToolActivity));
            serverIntent.PutExtra(XmlToolActivity.ExtraInitDir, _instanceData.EcuPath);
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
    }
}
