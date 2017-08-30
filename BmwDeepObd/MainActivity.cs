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
using System.Xml;
using System.Xml.Linq;
using Android.Content;
using Android.Content.PM;
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
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable UsePatternMatching

#if APP_USB_FILTER
[assembly: Android.App.UsesFeature("android.hardware.usb.host")]
#endif

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name", MainLauncher = true,
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
            RequestSelectEcuZip,
            RequestYandexKey,
            RequestGlobalSettings,
        }

        private enum LastAppState
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

            public string DownloadDir { get; }

            public XElement InfoXml { get; }
        }

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

        class ConnectButtonInfo
        {
            public ToggleButton Button { get; set; }
            public bool Enabled { get; set; }
            public bool Checked { get; set; }
        }

        private static readonly long TickResolMs = Stopwatch.Frequency / 1000;
        private const char DataLogSeparator = '\t';
        private const string SharedAppName = "de.holeschak.bmw_deep_obd";
        private const string AppFolderName = "de.holeschak.bmw_deep_obd";
        private const string EcuDirNameBmw = "Ecu";
        private const string EcuDirNameVag = "EcuVag";
        private const string EcuDownloadUrlBmw = @"http://www.holeschak.de/BmwDeepObd/Ecu3.xml";
        private const string EcuDownloadUrlVag = @"http://www.holeschak.de/BmwDeepObd/EcuVag1.xml";
        private const string InfoXmlName = "Info.xml";
        private const long EcuZipSizeBmw = 130000000;           // BMW ecu zip file size
        private const long EcuExtractSizeBmw = 1200000000;      // BMW extracted ecu files size
        private const long EcuZipSizeVag = 53000000;            // VAG ecu zip file size
        private const long EcuExtractSizeVag = 910000000;       // VAG extracted ecu files size
        private const int RequestPermissionExternalStorage = 0;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage
        };

        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        private LastAppState _lastAppState = LastAppState.Init;
        private bool _backPressed;
        private long _lastBackPressesTime;
        private string _deviceName = string.Empty;
        private string _deviceAddress = string.Empty;
        private string _configFileName = string.Empty;
        private int _currentVersionCode;
        private int _lastVersionCode;
        private string _appDataPath = String.Empty;
        private string _ecuPath = String.Empty;
        private bool _userEcuFiles;
        private bool _traceActive = true;
        private bool _traceAppend;
        private bool _dataLogActive;
        private bool _dataLogAppend;
        private bool _commErrorsOccured;
        private bool _activityActive;
        private bool _onResumeExecuted;
        private bool _storageAccessGranted;
        private bool _createTabsPending;
        private bool _compileCodePending;
        private bool _autoStart;
        private bool _vagInfoShown;
        private bool _btInitiallyEnabled;
        private ActivityCommon _activityCommon;
        private Handler _updateHandler;
        private StreamWriter _swDataLog;
        private string _dataLogDir;
        private string _traceDir;
        private TabLayout _tabLayout;
        private ViewPager _viewPager;
        private TabsFragmentPagerAdapter _fragmentPagerAdapter;
        private readonly ConnectButtonInfo _connectButtonInfo = new ConnectButtonInfo();
        private ImageView _imageBackground;
        private WebClient _webClient;
        private Android.App.ProgressDialog _downloadProgress;
        private long _downloadFileSize;
        private List<DownloadUrlInfo> _downloadUrlInfoList;
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
                        return EcuDirNameBmw;
                }
                return EcuDirNameVag;
            }
        }

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

        public void OnTabReselected(TabLayout.Tab tab)
        {
        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            UpdateSelectedPage();
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
            ClearPage(tab.Position);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.main);

            _fragmentPagerAdapter = new TabsFragmentPagerAdapter(SupportFragmentManager);
            _viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            _viewPager.Adapter = _fragmentPagerAdapter;
            _tabLayout = FindViewById<TabLayout>(Resource.Id.sliding_tabs);
            _tabLayout.SetupWithViewPager(_viewPager);
            _tabLayout.AddOnTabSelectedListener(this);
            _tabLayout.Visibility = ViewStates.Gone;

            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                }
            }, BroadcastReceived);
            _activityCommon.RegisterInternetCellular();
            _btInitiallyEnabled = _activityCommon.IsBluetoothEnabled();

            GetSettings();
            StoreLastAppState(LastAppState.Init);

            _updateHandler = new Handler();

            _imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += DownloadProgressChanged;
            _webClient.DownloadFileCompleted += DownloadCompleted;
        }

        void CreateActionBarTabs()
        {
            if (!_activityActive)
            {
                _createTabsPending = true;
                return;
            }
            _createTabsPending = false;
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
            if (_tabLayout.TabCount > 0)
            {
                _tabLayout.GetTabAt(0).Select();
            }
            UpdateDisplay();
            StoreLastAppState(LastAppState.TabsCreated);
        }

        protected override void OnStop()
        {
            base.OnStop();

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
            SupportInvalidateOptionsMenu();
            UpdateDisplay();
        }

        protected override void OnPause()
        {
            base.OnPause();

            _activityActive = false;
            if (!UseCommService())
            {
                StopEdiabasThread(false);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //if (!UseCommService())
            {
                StopEdiabasThread(true);
            }
            StoreSettings();
            if (_activityCommon != null)
            {
                if (!_btInitiallyEnabled && ActivityCommon.BtDisableHandling == ActivityCommon.BtDisableType.DisableIfByApp &&
                    ActivityCommon.IsBluetoothEnabledByApp() && !_activityCommon.IsBluetoothConnected() &&
                    !ActivityCommon.CommActive)
                {
                    _activityCommon.BluetoothDisable();
                }
                _activityCommon.UnRegisterInternetCellular();
                _activityCommon.Dispose();
                _activityCommon = null;
            }
            if (_updateHandler != null)
            {
                _updateHandler.Dispose();
                _updateHandler = null;
            }
            if (_webClient != null)
            {
                _webClient.Dispose();
                _webClient = null;
            }
            if (!ActivityCommon.CommActive)
            {
                MemoryStreamReader.CleanUp();
            }
            StoreLastAppState(LastAppState.Terminated);
        }

        public override void OnBackPressed()
        {
            if (!ActivityCommon.DoubleClickForAppExit)
            {
                _backPressed = false;
                base.OnBackPressed();
                return;
            }
            if (_backPressed)
            {
                _backPressed = false;
                if (Stopwatch.GetTimestamp() - _lastBackPressesTime < 2000 * TickResolMs)
                {
                    base.OnBackPressed();
                    return;
                }
            }
            if (!_backPressed)
            {
                _backPressed = true;
                _lastBackPressesTime = Stopwatch.GetTimestamp();
                Toast.MakeText(this, GetString(Resource.String.back_button_twice_for_exit), ToastLength.Short).Show();
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _deviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _deviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        bool callAdapterConfig = data.Extras.GetBoolean(DeviceListActivity.ExtraCallAdapterConfig, false);
                        SupportInvalidateOptionsMenu();
                        if (callAdapterConfig)
                        {
                            AdapterConfig();
                        }
                        else if (_autoStart)
                        {
                            ButtonConnectClick(_connectButtonInfo.Button, new EventArgs());
                        }
                    }
                    _autoStart = false;
                    break;

                case ActivityRequest.RequestAdapterConfig:
                    break;

                case ActivityRequest.RequestSelectConfig:
                    // When FilePickerActivity returns with a file
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        _configFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        ReadConfigFile();
                        SupportInvalidateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestXmlTool:
                    // When XML tool returns with a file
                    _activityCommon.SetPreferredNetworkInterface();
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        _configFileName = data.Extras.GetString(XmlToolActivity.ExtraFileName);
                        ReadConfigFile();
                        SupportInvalidateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestEdiabasTool:
                    _activityCommon.SetPreferredNetworkInterface();
                    break;

                case ActivityRequest.RequestSelectEcuZip:
                    if (data != null && resultCode == Android.App.Result.Ok)
                    {
                        string zipFile = data.Extras.GetString(XmlToolActivity.ExtraFileName);
                        string fileName = Path.GetFileName(zipFile) ?? string.Empty;
                        string ecuPath = Path.Combine(_appDataPath, ManufacturerEcuDirName);

                        XElement xmlInfo = new XElement("Info");
                        xmlInfo.Add(new XAttribute("Url", zipFile));
                        xmlInfo.Add(new XAttribute("Name", fileName));

                        ExtractZipFile(zipFile, ecuPath, xmlInfo);
                    }
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey);
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestGlobalSettings:
                    UpdateDirectories();
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                    CheckForEcuFiles();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
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
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_adapter), _deviceName));
                scanMenu.SetEnabled(interfaceAvailable && !commActive);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem adapterConfigMenu = menu.FindItem(Resource.Id.menu_adapter_config);
            if (adapterConfigMenu != null)
            {
                adapterConfigMenu.SetEnabled(interfaceAvailable && !commActive);
                adapterConfigMenu.SetVisible(_activityCommon.AllowAdapterConfig(_deviceAddress));
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
                if (!string.IsNullOrEmpty(_configFileName))
                {
                    fileName = Path.GetFileNameWithoutExtension(_configFileName);
                }
                selCfgMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_sel_cfg), fileName));
                selCfgMenu.SetEnabled(!commActive);
            }

            IMenuItem xmlToolMenu = menu.FindItem(Resource.Id.menu_xml_tool);
            xmlToolMenu?.SetEnabled(!commActive);

            IMenuItem ediabasToolMenu = menu.FindItem(Resource.Id.menu_ediabas_tool);
            ediabasToolMenu?.SetEnabled(!commActive);

            IMenuItem downloadEcu = menu.FindItem(Resource.Id.menu_download_ecu);
            downloadEcu?.SetEnabled(!commActive);

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            logSubMenu?.SetEnabled(interfaceAvailable && !commActive);

            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _traceActive && ActivityCommon.IsTraceFilePresent(_traceDir));

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
                    _autoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice, _appDataPath);
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
                    SendTraceFileAlways(null);
                    return true;

                case Resource.Id.menu_translation_enable:
                    if (!ActivityCommon.EnableTranslation && string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey))
                    {
                        EditYandexKey();
                        return true;
                    }
                    ActivityCommon.EnableTranslation = !ActivityCommon.EnableTranslation;
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                    return true;

                case Resource.Id.menu_translation_yandex_key:
                    EditYandexKey();
                    return true;

                case Resource.Id.menu_translation_clear_cache:
                    _activityCommon.ClearTranslationCache();
                    _translationList = null;
                    _translatedList = null;
                    SupportInvalidateOptionsMenu();
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
            _autoStart = false;
            if (string.IsNullOrEmpty(_deviceAddress))
            {
                if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, _appDataPath, (s, args) =>
                    {
                        _autoStart = true;
                    }))
                {
                    UpdateDisplay();
                    return;
                }
            }
            if (_activityCommon.ShowWifiWarning(retry => 
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

        private void HandleStartDialogs(bool firstStart)
        {
            if (!_activityCommon.RequestInterfaceEnable((sender, args) =>
            {
                SupportInvalidateOptionsMenu();
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
            if (_swDataLog == null)
            {
                if (ActivityCommon.LockTypeCommunication == ActivityCommon.LockType.None)
                {
                    useService = false;
                }
            }
            else
            {
                if (ActivityCommon.LockTypeLogging == ActivityCommon.LockType.None)
                {
                    useService = false;
                }
            }
            return useService;
        }

        private bool StartEdiabasThread()
        {
            _autoStart = false;
            _commErrorsOccured = false;
            try
            {
                if (ActivityCommon.EdiabasThread == null)
                {
                    ActivityCommon.EdiabasThread = new EdiabasThread(string.IsNullOrEmpty(ActivityCommon.JobReader.EcuPath) ? _ecuPath : ActivityCommon.JobReader.EcuPath, _activityCommon);
                    ActivityCommon.EdiabasThread.DataUpdated += DataUpdated;
                    ActivityCommon.EdiabasThread.ThreadTerminated += ThreadTerminated;
                }
                string logDir = string.Empty;
                if (!string.IsNullOrEmpty(ActivityCommon.JobReader.LogPath))
                {
                    logDir = Path.IsPathRooted(ActivityCommon.JobReader.LogPath) ? ActivityCommon.JobReader.LogPath : Path.Combine(_appDataPath, ActivityCommon.JobReader.LogPath);
                    try
                    {
                        Directory.CreateDirectory(logDir);
                    }
                    catch (Exception)
                    {
                        logDir = string.Empty;
                    }
                }
                _dataLogDir = logDir;

                _traceDir = null;
                if (_traceActive && !string.IsNullOrEmpty(_configFileName))
                {
                    _traceDir = logDir;
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
                            portName = "BLUETOOTH:" + _deviceAddress;
                            break;

                        case ActivityCommon.InterfaceType.Enet:
                            connectParameter = new EdInterfaceEnet.ConnectParameterType(this, _activityCommon.MaConnectivity);
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
                            connectParameter = new EdElmWifiInterface.ConnectParameterType(this, _activityCommon.MaConnectivity);
                            break;

                        case ActivityCommon.InterfaceType.Ftdi:
                            portName = "FTDI0";
                            connectParameter = new EdFtdiInterface.ConnectParameterType(this, _activityCommon.UsbManager);
                            break;
                    }
                    ActivityCommon.EdiabasThread.StartThread(portName, connectParameter, _traceDir, _traceAppend, pageInfo, true);
                    if (UseCommService())
                    {
                        _activityCommon.StartForegroundService();
                    }
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (_dataLogActive)
                    {
                        _activityCommon.SetLock(ActivityCommon.LockTypeLogging);
                    }
                    else
                    {
                        _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            SupportInvalidateOptionsMenu();
            return true;
        }

        private void StopEdiabasThread(bool wait)
        {
            if (ActivityCommon.EdiabasThread != null)
            {
                try
                {
                    ActivityCommon.EdiabasThread.StopThread(wait);
                    _activityCommon.StopForegroundService();
                    if (wait)
                    {
                        ActivityCommon.EdiabasThread.DataUpdated -= DataUpdated;
                        ActivityCommon.EdiabasThread.ThreadTerminated -= ThreadTerminated;
                        ActivityCommon.EdiabasThread.Dispose();
                        ActivityCommon.EdiabasThread = null;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
            _activityCommon?.SetLock(ActivityCommon.LockType.None);
            CloseDataLog();
            SupportInvalidateOptionsMenu();
        }

        private void CloseDataLog()
        {
            if (_swDataLog != null)
            {
                _swDataLog.Dispose();
                _swDataLog = null;
            }
        }

        private void GetSettings()
        {
            try
            {
                _currentVersionCode = PackageManager.GetPackageInfo(PackageName, 0).VersionCode;
                ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                string stateString = prefs.GetString("LastAppState", string.Empty);
                if (!System.Enum.TryParse(stateString, true, out _lastAppState))
                {
                    _lastAppState = LastAppState.Init;
                }
                _deviceName = prefs.GetString("DeviceName", string.Empty);
                _deviceAddress = prefs.GetString("DeviceAddress", string.Empty);
                _activityCommon.SelectedEnetIp = prefs.GetString("EnetIp", string.Empty);
                _configFileName = prefs.GetString("ConfigFile", string.Empty);
                _activityCommon.CustomStorageMedia = prefs.GetString("StorageMedia", string.Empty);
                _lastVersionCode = prefs.GetInt("VersionCode", -1);
                ActivityCommon.EnableTranslation = prefs.GetBoolean("EnableTranslation", false);
                ActivityCommon.YandexApiKey = prefs.GetString("YandexApiKey", string.Empty);
                ActivityCommon.AppId = prefs.GetString("AppId", string.Empty);
                ActivityCommon.SelectedManufacturer = (ActivityCommon.ManufacturerType) prefs.GetInt("Manufacturer", (int)ActivityCommon.ManufacturerType.Bmw);
                ActivityCommon.BtEnbaleHandling = (ActivityCommon.BtEnableType)prefs.GetInt("BtEnable", (int)ActivityCommon.BtEnableType.Ask);
                ActivityCommon.BtDisableHandling = (ActivityCommon.BtDisableType)prefs.GetInt("BtDisable", (int)ActivityCommon.BtDisableType.DisableIfByApp);
                ActivityCommon.LockTypeCommunication = (ActivityCommon.LockType)prefs.GetInt("LockComm", (int)ActivityCommon.LockType.ScreenDim);
                ActivityCommon.LockTypeLogging = (ActivityCommon.LockType)prefs.GetInt("LockLog", (int)ActivityCommon.LockType.Cpu);
                ActivityCommon.StoreDataLogSettings = prefs.GetBoolean("StoreDataLogSettings", ActivityCommon.StoreDataLogSettings);
                if (ActivityCommon.StoreDataLogSettings)
                {
                    _dataLogActive = prefs.GetBoolean("DataLogActive", _dataLogActive);
                    _dataLogAppend = prefs.GetBoolean("DataLogAppend", _dataLogAppend);
                }
                ActivityCommon.DoubleClickForAppExit = prefs.GetBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
                ActivityCommon.CollectDebugInfo = prefs.GetBoolean("CollectDebugInfo", ActivityCommon.CollectDebugInfo);
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
                prefsEdit.PutString("DeviceName", _deviceName);
                prefsEdit.PutString("DeviceAddress", _deviceAddress);
                prefsEdit.PutString("EnetIp", _activityCommon.SelectedEnetIp);
                prefsEdit.PutString("ConfigFile", _configFileName);
                prefsEdit.PutString("StorageMedia", _activityCommon.CustomStorageMedia ?? string.Empty);
                prefsEdit.PutInt("VersionCode", _currentVersionCode);
                prefsEdit.PutBoolean("EnableTranslation", ActivityCommon.EnableTranslation);
                prefsEdit.PutString("YandexApiKey", ActivityCommon.YandexApiKey ?? string.Empty);
                prefsEdit.PutString("AppId", ActivityCommon.AppId);
                prefsEdit.PutInt("Manufacturer", (int) ActivityCommon.SelectedManufacturer);
                prefsEdit.PutInt("BtEnable", (int)ActivityCommon.BtEnbaleHandling);
                prefsEdit.PutInt("BtDisable", (int) ActivityCommon.BtDisableHandling);
                prefsEdit.PutInt("LockComm", (int)ActivityCommon.LockTypeCommunication);
                prefsEdit.PutInt("LockLog", (int)ActivityCommon.LockTypeLogging);
                prefsEdit.PutBoolean("StoreDataLogSettings", ActivityCommon.StoreDataLogSettings);
                prefsEdit.PutBoolean("DataLogActive", _dataLogActive);
                prefsEdit.PutBoolean("DataLogAppend", _dataLogAppend);
                prefsEdit.PutBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
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
            _storageAccessGranted = true;
            ActivityCommon.SetStoragePath();
            UpdateDirectories();
            _activityCommon.RequestUsbPermission(null);
            ReadConfigFile();
            if (_startAlertDialog == null && _currentVersionCode != _lastVersionCode)
            {
                _startAlertDialog = new AlertDialog.Builder(this)
                    .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.version_change_info_message)
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
                _startAlertDialog.DismissEvent += (sender, args) =>
                {
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
            CloseDataLog();
            _appDataPath = string.Empty;
            _ecuPath = string.Empty;
            _userEcuFiles = false;
            if (string.IsNullOrEmpty(_activityCommon.CustomStorageMedia))
            {
                if (string.IsNullOrEmpty(_activityCommon.ExternalWritePath))
                {
                    if (string.IsNullOrEmpty(_activityCommon.ExternalPath))
                    {
                        Toast.MakeText(this, GetString(Resource.String.no_ext_storage), ToastLength.Long).Show();
                        Finish();
                    }
                    else
                    {
                        _appDataPath = Path.Combine(_activityCommon.ExternalPath, AppFolderName);
                        _ecuPath = Path.Combine(_appDataPath, ManufacturerEcuDirName);
                    }
                }
                else
                {
                    _appDataPath = _activityCommon.ExternalWritePath;
                    _ecuPath = Path.Combine(_appDataPath, ManufacturerEcuDirName);
                    if (!ValidEcuFiles(_ecuPath))
                    {
                        string userEcuPath = Path.Combine(_appDataPath, "../../../..", AppFolderName, ManufacturerEcuDirName);
                        if (ValidEcuFiles(userEcuPath))
                        {
                            _ecuPath = userEcuPath;
                            _userEcuFiles = true;
                        }
                    }
                }
            }
            else
            {
                _appDataPath = Path.Combine(_activityCommon.CustomStorageMedia, AppFolderName);
                _ecuPath = Path.Combine(_appDataPath, ManufacturerEcuDirName);
            }

            string backgroundImageFile = Path.Combine(_appDataPath, "Images", "Background.jpg");
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
                _activityCommon.RequestUsbPermission(null);
                return;
            }
            string action = intent.Action;
            switch (action)
            {
                case ForegroundService.NotificationBroadcastAction:
                {
                    string request = intent.GetStringExtra(ForegroundService.BroadcastMessageKey);
                    if (request.Equals(ForegroundService.BroadcastStopComm))
                    {
                        StopEdiabasThread(false);
                    }
                    break;
                }

                case UsbManager.ActionUsbDeviceAttached:
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
                        if (EdFtdiInterface.IsValidUsbDevice(usbDevice))
                        {
                            _activityCommon.RequestUsbPermission(usbDevice);
                            SupportInvalidateOptionsMenu();
                            UpdateDisplay();
                        }
                        break;
                    }
            }
        }

        private void DataUpdated(object sender, EventArgs e)
        {
            RunOnUiThread(UpdateDisplay);
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

            if (_commErrorsOccured && _traceActive && !string.IsNullOrEmpty(_traceDir))
            {
                _activityCommon.RequestSendTraceFile(_appDataPath, _traceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType());
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (ActivityCommon.CommActive)
            {
                return false;
            }
            if (_traceActive && !string.IsNullOrEmpty(_traceDir))
            {
                return _activityCommon.SendTraceFile(_appDataPath, _traceDir, PackageManager.GetPackageInfo(PackageName, 0), GetType(), handler);
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
                CloseDataLog();
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
            bool threadRunning = false;

            _connectButtonInfo.Enabled = true;
            if (ActivityCommon.CommActive)
            {
                if (ActivityCommon.EdiabasThread.ThreadStopping())
                {
                    _connectButtonInfo.Enabled = false;
                }
                else
                {
                    threadRunning = true;
                }
                if (ActivityCommon.EdiabasThread.CommActive)
                {
                    dynamicValid = true;
                }
                _connectButtonInfo.Checked = true;
            }
            else
            {
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
                ListView listViewResult = dynamicFragment.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this, pageInfo.Weight, pageInfo.TextResId, pageInfo.ErrorsInfo != null);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
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

                if (dynamicValid)
                {
                    if (_dataLogActive && threadRunning && _swDataLog == null &&
                        !string.IsNullOrEmpty(_dataLogDir) && !string.IsNullOrEmpty(pageInfo.LogFile))
                    {
                        try
                        {
                            FileMode fileMode;
                            string logFileName = pageInfo.LogFile;
                            logFileName = logFileName.Replace("{D}", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", Culture));

                            string fileName = Path.Combine(_dataLogDir, logFileName);
                            if (File.Exists(fileName))
                            {
                                fileMode = (_dataLogAppend || ActivityCommon.JobReader.AppendLog) ? FileMode.Append : FileMode.Create;
                            }
                            else
                            {
                                fileMode = FileMode.Create;
                            }
                            _swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
                            if (fileMode == FileMode.Create)
                            {
                                // add header
                                StringBuilder sbLog = new StringBuilder();
                                sbLog.Append(GetString(Resource.String.datalog_date));
                                foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                                {
                                    if (!string.IsNullOrEmpty(displayInfo.LogTag))
                                    {
                                        sbLog.Append(DataLogSeparator);
                                        sbLog.Append(displayInfo.LogTag.Replace(DataLogSeparator, ' '));
                                    }
                                }
                                try
                                {
                                    sbLog.Append("\r\n");
                                    _swDataLog.Write(sbLog.ToString());
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
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
                        _commErrorsOccured = true;
                    }

                    MethodInfo formatResult = null;
                    MethodInfo formatResultColor = null;
                    MethodInfo formatResultMulti = null;
                    MethodInfo formatErrorResult = null;
                    MethodInfo updateResult = null;
                    MethodInfo updateResultMulti = null;
                    if (pageInfo.ClassObject != null)
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        formatResult = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(Dictionary< string, EdiabasNet.ResultData >), typeof(string) });
                        formatResultColor = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType() });
                        formatResultMulti = pageType.GetMethod("FormatResult", new[] { typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(string), typeof(Android.Graphics.Color?).MakeByRefType() });
                        formatErrorResult = pageType.GetMethod("FormatErrorResult");
                        updateResult = pageType.GetMethod("UpdateResultList", new[] { typeof(JobReader.PageInfo), typeof(Dictionary<string, EdiabasNet.ResultData>), typeof(List<TableResultItem>) } );
                        updateResultMulti = pageType.GetMethod("UpdateResultList", new[] { typeof(JobReader.PageInfo), typeof(MultiMap<string, EdiabasNet.ResultData>), typeof(List<TableResultItem>) });
                    }
                    string currDateTime = string.Empty;
                    if (_dataLogActive)
                    {
                        currDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Culture);
                    }

                    List<TableResultItem> tempResultList = new List<TableResultItem>();
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
                                    _commErrorsOccured = true;
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
                                        EdiabasNet.ResultData resultData;
                                        if (errorReport.ErrorDict.TryGetValue("FNR_WERT", out resultData))
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
                                        List<string> textList = _activityCommon.ConvertVagDtcCode(_ecuPath, errorCode, errorTypeList, kwp1281, saeMode);

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
                                            _translateActive = false;
                                            _translatedList = transList;
                                            SupportInvalidateOptionsMenu();
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
                        StringBuilder sbLog = new StringBuilder();
                        sbLog.Append(currDateTime);
                        bool logDataPresent = false;
                        foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                        {
                            string result = string.Empty;
                            Android.Graphics.Color? textColor = null;
                            if (displayInfo.Format == null)
                            {
                                if (resultDict != null)
                                {
                                    try
                                    {
                                        if (formatResultMulti != null)
                                        {
                                            object[] args = { pageInfo, resultDict, displayInfo.Result, null };
                                            result = formatResultMulti.Invoke(pageInfo.ClassObject, args) as string;
                                            textColor = args[3] as Android.Graphics.Color?;
                                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict, displayInfo.Result, ref textColor);
                                        }
                                        else if (formatResultColor != null)
                                        {
                                            object[] args = { pageInfo, resultDict.ToDictionary(), displayInfo.Result, null };
                                            result = formatResultColor.Invoke(pageInfo.ClassObject, args) as string;
                                            textColor = args[3] as Android.Graphics.Color?;
                                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict.ToDictionary(), displayInfo.Result, ref textColor);
                                        }
                                        else if (formatResult != null)
                                        {
                                            object[] args = { pageInfo, resultDict.ToDictionary(), displayInfo.Result };
                                            result = formatResult.Invoke(pageInfo.ClassObject, args) as string;
                                            //result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict.ToDictionary(), displayInfo.Result);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                            }
                            else
                            {
                                result = FormatResultEdiabas(resultDict, displayInfo.Result, displayInfo.Format);
                            }
                            if (result != null)
                            {
                                tempResultList.Add(new TableResultItem(GetPageString(pageInfo, displayInfo.Name), result, null, false, false, textColor));
                                if (!string.IsNullOrEmpty(displayInfo.LogTag) && _dataLogActive && _swDataLog != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(result))
                                    {
                                        logDataPresent = true;
                                    }
                                    sbLog.Append(DataLogSeparator);
                                    sbLog.Append(result.Replace(DataLogSeparator, ' '));
                                }
                            }
                        }
                        if (logDataPresent && _dataLogActive && _swDataLog != null)
                        {
                            try
                            {
                                sbLog.Append("\r\n");
                                _swDataLog.Write(sbLog.ToString());
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    if (updateResultMulti != null)
                    {
                        object[] args = { pageInfo, resultDict, tempResultList };
                        updateResultMulti.Invoke(pageInfo.ClassObject, args);
                        //pageInfo.ClassObject.UpdateResultList(pageInfo, resultDict, tempResultList);
                    }
                    else if (updateResult != null)
                    {
                        object[] args = { pageInfo, resultDict?.ToDictionary(), tempResultList };
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
                else
                {
                    resultListAdapter.Items.Clear();
                    resultListAdapter.NotifyDataSetChanged();
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
            bool found;
            double value = GetResultDouble(resultDict, dataName, index, out found);
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
            bool found;
            Int64 value = GetResultInt64(resultDict, dataName, index, out found);
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
            bool found;
            string value = GetResultString(resultDict, dataName, index, out found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultEdiabas(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            StringBuilder sbResult = new StringBuilder();
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    string result;
                    if (resultData.OpData.GetType() == typeof(byte[]))
                    {
                        StringBuilder sb = new StringBuilder();
                        byte[] data = (byte[])resultData.OpData;
                        foreach (byte value in data)
                        {
                            sb.Append(string.Format(Culture, "{0:X02} ", value));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = EdiabasNet.FormatResult(resultData, format) ?? "?";
                    }
                    if (sbResult.Length > 0)
                    {
                        sbResult.Append("\r\n");
                    }
                    sbResult.Append(result);
                }
            }
            return sbResult.ToString();
        }

        public static Int64 GetResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            return GetResultInt64(new MultiMap<string, EdiabasNet.ResultData>(resultDict), dataName, 0, out found);
        }

        public static Int64 GetResultInt64(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, int index, out bool found)
        {
            found = false;
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
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
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
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
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
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
                _updateHandler.Post(CreateActionBarTabs);
                return;
            }
            ActivityCommon.JobReader.Clear();
            if (_lastAppState != LastAppState.Compile)
            {
                ActivityCommon.JobReader.ReadXml(_configFileName);
            }
            if (ActivityCommon.JobReader.PageList.Count > 0)
            {
                ActivityCommon.SelectedManufacturer = ActivityCommon.JobReader.Manufacturer;
                _activityCommon.SelectedInterface = ActivityCommon.JobReader.Interface;
            }
            else
            {
                _activityCommon.SelectedInterface = ActivityCommon.InterfaceType.None;
            }
            _activityCommon.ClearTranslationCache();
            _translationList = null;
            _translatedList = null;
            UpdateDirectories();
            RequestConfigSelect();
            CompileCode();
        }

        private void CompileCode()
        {
            if (!_activityActive)
            {
                _compileCodePending = true;
                return;
            }
            _compileCodePending = false;
            if (ActivityCommon.JobReader.PageList.Count == 0)
            {
                _updateHandler.Post(CreateActionBarTabs);
                return;
            }
            StoreLastAppState(LastAppState.Compile);
            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.compile_start));
            progress.Show();

            Thread compileThreadWrapper = new Thread(() =>
            {
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

                foreach (string compileResult in compileResultList)
                {
                    string result = compileResult;
                    RunOnUiThread(() => _activityCommon.ShowAlert(result, Resource.String.alert_title_error));
                }

                RunOnUiThread(() =>
                {
                    CreateActionBarTabs();
                    progress.Hide();
                    progress.Dispose();
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
                _downloadProgress = new Android.App.ProgressDialog(this);
                _downloadProgress.SetCancelable(false);
                _downloadProgress.SetButton((int)DialogButtonType.Negative, GetString(Resource.String.button_abort), (sender, args) =>
                    {
                        if (_webClient.IsBusy)
                        {
                            _webClient.CancelAsync();
                        }
                    });
                _downloadProgress.DismissEvent += (sender, args) =>
                {
                    _downloadProgress = null;
                    _activityCommon.SetLock(ActivityCommon.LockType.None);
                };
                _downloadUrlInfoList = null;
            }
            _downloadProgress.SetMessage(GetString(Resource.String.downloading_file));
            _downloadProgress.SetProgressStyle(Android.App.ProgressDialogStyle.Horizontal);
            _downloadProgress.Progress = 0;
            _downloadProgress.Max = 100;
            _downloadProgress.Show();
            _downloadProgress.GetButton((int)DialogButtonType.Negative).Enabled = false;    // early abort crashes!
            _downloadFileSize = fileSize;
            _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);
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
                        xmlInfo.Add(new XAttribute("Name", fileName));
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
                        if (_downloadProgress != null)
                        {
                            _downloadProgress.Hide();
                            _downloadProgress.Dispose();
                            _downloadProgress = null;
                            _activityCommon.SetLock(ActivityCommon.LockType.None);
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
                DownloadInfo downloadInfo = e.UserState as DownloadInfo;
                if (_downloadProgress != null)
                {
                    bool error = false;
                    _downloadProgress.GetButton((int)DialogButtonType.Negative).Enabled = false;
                    if (downloadInfo != null)
                    {
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
                                _downloadProgress.GetButton((int)DialogButtonType.Negative).Enabled = false;
                                ExtractZipFile(downloadInfo.FileName, downloadInfo.TargetDir, downloadInfo.InfoXml, true);
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
                    }
                    _downloadProgress.Hide();
                    _downloadProgress.Dispose();
                    _downloadProgress = null;
                    _activityCommon.SetLock(ActivityCommon.LockType.None);
                    _downloadUrlInfoList = null;
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

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_downloadProgress != null)
                {
                    if (e.TotalBytesToReceive < 0 && _downloadFileSize > 0)
                    {
                        _downloadProgress.Progress = (int)(e.BytesReceived * 100 / _downloadFileSize);
                    }
                    else
                    {
                        _downloadProgress.Progress = e.ProgressPercentage;
                    }
                    if (_webClient.IsBusy)
                    {
                        _downloadProgress.GetButton((int)DialogButtonType.Negative).Enabled = true;
                    }
                }
            });
        }

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
                        string url = urlAttr.Value;
                        string name = null;
                        string password = null;
                        long fileSize = -1;
                        XAttribute sizeAttr = fileNode.Attribute("file_size");
                        if (sizeAttr != null)
                        {
                            fileSize = XmlConvert.ToInt64(sizeAttr.Value);
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

        private void ExtractZipFile(string fileName, string targetDirectory, XElement infoXml, bool removeFile = false)
        {
            bool extractCanceled = false;
            if (_downloadProgress == null)
            {
                _downloadProgress = new Android.App.ProgressDialog(this);
                _downloadProgress.SetCancelable(false);
                _downloadProgress.DismissEvent += (sender, args) => { _downloadProgress = null; };
                _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);
            }
            _downloadProgress.SetMessage(GetString(Resource.String.extract_cleanup));
            _downloadProgress.SetProgressStyle(Android.App.ProgressDialogStyle.Horizontal);
            _downloadProgress.Progress = 0;
            _downloadProgress.Max = 100;
            _downloadProgress.SetButton((int)DialogButtonType.Negative, GetString(Resource.String.button_abort),
                (sender, args) => { extractCanceled = true; });
            _downloadProgress.Show();
            _downloadProgress.GetButton((int)DialogButtonType.Negative).Enabled = false;

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
                    RunOnUiThread(() =>
                    {
                        if (_downloadProgress != null)
                        {
                            _downloadProgress.SetMessage(GetString(Resource.String.extract_file));
                            _downloadProgress.GetButton((int) DialogButtonType.Negative).Enabled = true;
                        }
                    });

                    ActivityCommon.ExtractZipFile(fileName, targetDirectory,
                        percent =>
                        {
                            RunOnUiThread(() =>
                            {
                                if (_downloadProgress != null)
                                {
                                    _downloadProgress.Progress = percent;
                                }
                            });
                            return extractCanceled;
                        });
                    infoXml?.Save(Path.Combine(targetDirectory, InfoXmlName));
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
                    if (_downloadProgress != null)
                    {
                        _downloadProgress.Hide();
                        _downloadProgress.Dispose();
                        _downloadProgress = null;
                        _activityCommon.SetLock(ActivityCommon.LockType.None);
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

        private void DownloadEcuFiles(bool manualRequest = false)
        {
            string ecuPath = Path.Combine(_appDataPath, ManufacturerEcuDirName);
            try
            {
                ActivityCommon.FileSystemBlockInfo blockInfo = ActivityCommon.GetFileSystemBlockInfo(_appDataPath);
                long ecuDirSize = ActivityCommon.GetDirectorySize(ecuPath);
                double freeSpace = blockInfo.AvailableSizeBytes + ecuDirSize;
                long requiredSize = ManufacturerEcuExtractSize + ManufacturerEcuZipSize;
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
                        DownloadFile(ManufacturerEcuDownloadUrl, Path.Combine(_appDataPath, ActivityCommon.DownloadDir), ecuPath);
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
                DownloadFile(ManufacturerEcuDownloadUrl, Path.Combine(_appDataPath, ActivityCommon.DownloadDir), ecuPath);
            }
        }

        private void ManualEcuFilesInstall()
        {
            string downloadsDir = _appDataPath;
            Java.IO.File directoryDownloads = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
            if (!string.IsNullOrEmpty(directoryDownloads?.AbsolutePath))
            {
                downloadsDir = directoryDownloads.AbsolutePath;
            }

            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            serverIntent.PutExtra(FilePickerActivity.ExtraTitle, GetString(Resource.String.select_ecu_zip));
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, downloadsDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".zip");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectEcuZip);
        }

        private bool CheckForEcuFiles(bool checkPackage = false)
        {
            if (!_storageAccessGranted || (_downloadEcuAlertDialog != null))
            {
                return true;
            }

            if (!ValidEcuFiles(_ecuPath))
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

            if (checkPackage && !_userEcuFiles)
            {
                if (!ValidEcuPackage(_ecuPath))
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
                if (string.Compare(Path.GetFileNameWithoutExtension(nameAttr.Value), Path.GetFileNameWithoutExtension(ManufacturerEcuDownloadUrl), StringComparison.OrdinalIgnoreCase) != 0)
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
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && !_vagInfoShown)
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
                _vagInfoShown = true;
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
                _configFileName = null;
                CreateActionBarTabs();
                UpdateDirectories();
                if (CheckForEcuFiles())
                {
                    RequestConfigSelect();
                }
                SupportInvalidateOptionsMenu();
                UpdateDisplay();
            });
        }

        private void RequestConfigSelect()
        {
            if (_configSelectAlertDialog != null)
            {
                return;
            }
            if (_lastAppState == LastAppState.Compile)
            {
                _lastAppState = LastAppState.Init;
                _configFileName = string.Empty;
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
            listView.SetItemChecked(0, _traceActive);
            listView.SetItemChecked(1, _traceAppend);
            listView.SetItemChecked(2, _dataLogActive);
            listView.SetItemChecked(3, _dataLogAppend);

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
                            _traceActive = value;
                            break;

                        case 1:
                            _traceAppend = value;
                            break;

                        case 2:
                            _dataLogActive = value;
                            break;

                        case 3:
                            _dataLogAppend = value;
                            break;
                    }
                }
                if (!_dataLogActive)
                {
                    CloseDataLog();
                }
                SupportInvalidateOptionsMenu();
            });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) => { });
            builder.Show();
        }

        private void EditYandexKey()
        {
            Intent serverIntent = new Intent(this, typeof(YandexKeyActivity));
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestYandexKey);
        }

        private void EditGlobalSettings(string selection = null)
        {
            Intent serverIntent = new Intent(this, typeof(GlobalSettingsActivity));
            if (selection != null)
            {
                serverIntent.PutExtra(GlobalSettingsActivity.ExtraSelection, selection);
            }
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestGlobalSettings);
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
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeviceAddress, _deviceAddress);
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestAdapterConfig);
        }

        private void EnetIpConfig()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            _activityCommon.SelectEnetIp((sender, args) =>
            {
                SupportInvalidateOptionsMenu();
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
            string initDir = _appDataPath;
            try
            {
                if (!string.IsNullOrEmpty(_configFileName))
                {
                    initDir = Path.GetDirectoryName(_configFileName);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".cccfg");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectConfig);
        }

        private void StartXmlTool()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(XmlToolActivity));
            serverIntent.PutExtra(XmlToolActivity.ExtraInitDir, _ecuPath);
            serverIntent.PutExtra(XmlToolActivity.ExtraAppDataDir, _appDataPath);
            serverIntent.PutExtra(XmlToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(XmlToolActivity.ExtraDeviceName, _deviceName);
            serverIntent.PutExtra(XmlToolActivity.ExtraDeviceAddress, _deviceAddress);
            serverIntent.PutExtra(XmlToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            serverIntent.PutExtra(XmlToolActivity.ExtraFileName, _configFileName);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestXmlTool);
        }

        private void StartEdiabasTool()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
            EdiabasToolActivity.IntentTranslateActivty = _activityCommon;
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInitDir, _ecuPath);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraAppDataDir, _appDataPath);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceName, _deviceName);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceAddress, _deviceAddress);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEdiabasTool);
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
                ActivityMain activityMain = Activity as ActivityMain;
                if (activityMain != null)
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

                ActivityMain activityMain = Activity as ActivityMain;
                if (activityMain != null && _pageInfoIndex >= 0 && _pageInfoIndex < ActivityCommon.JobReader.PageList.Count)
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
