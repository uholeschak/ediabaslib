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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Android.App.Backup;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware.Usb;
using Android.OS;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Core.Content.PM;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Base62;
using BmwDeepObd.FilePicker;
using BmwFileReader;
using EdiabasLib;
using Google.Android.Material.Tabs;
using Java.Interop;
using Mono.CSharp;
// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

[assembly: Android.App.UsesFeature("android.hardware.bluetooth", Required = false)]
[assembly: Android.App.UsesFeature("android.hardware.bluetooth_le", Required = false)]

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name", MainLauncher = false,
            Name = ActivityCommon.AppNameSpace + "." + nameof(ActivityMain),
            LaunchMode = LaunchMode.SingleTask,
            UiOptions = UiOptions.SplitActionBarWhenNarrow,
            ConfigurationChanges = ActivityConfigChanges)]
    [Android.App.MetaData("android.support.UI_OPTIONS", Value = "splitActionBarWhenNarrow")]
#pragma warning disable CS0618 // TabLayout.IOnTabSelectedListener2 creates compiler errors
    public class ActivityMain : BaseActivity, TabLayout.IOnTabSelectedListener
#pragma warning restore CS0618
    {
        private delegate void ErrorMessageResultDelegate(ErrorMessageData errorMessageData);

        private enum ActivityRequest
        {
            RequestAppStorePermissions,
            RequestAppDetailBtSettings,
            RequestLocationSettings,
            RequestAppSettingsAccessFiles,
            RequestOverlayPermissions,
            RequestNotificationSettingsApp,
            RequestNotificationSettingsChannel,
            RequestSelectDevice,
            RequestAdapterConfig,
            RequestSelectConfig,
            RequestXmlTool,
            RequestEdiabasTool,
            RequestBmwCoding,
            RequestYandexKey,
            RequestGlobalSettings,
            RequestEditConfig,
            RequestEditXml,
        }

        public enum SettingsMode
        {
            All,
            Private,
            Public,
        }

        public enum LastAppState
        {
            [XmlEnum(Name = "Init")] Init,
            [XmlEnum(Name = "Compile")] Compile,
            [XmlEnum(Name = "TabsCreated")] TabsCreated,
            [XmlEnum(Name = "Stopped")] Stopped,
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

        private class ConnectButtonInfo
        {
            public ToggleButton Button { get; set; }
            public bool Enabled { get; set; }
            public bool Checked { get; set; }
        }

        private class ErrorMessageData
        {
            public ErrorMessageData(List<ErrorMessageEntry> errorList, List<EdiabasThread.EdiabasErrorReportReset> errorResetList, List<string> translationList, bool commError)
            {
                ErrorList = errorList;
                ErrorResetList = errorResetList;
                TranslationList = translationList;
                CommError = commError;
            }

            public List<ErrorMessageEntry> ErrorList { get; }
            public List<EdiabasThread.EdiabasErrorReportReset> ErrorResetList { get; }
            public List<string> TranslationList { get; }
            public bool CommError { get; }
        }

        private class ErrorMessageEntry
        {
            public ErrorMessageEntry(EdiabasThread.EdiabasErrorReport errorReport, string message)
            {
                ErrorReport = errorReport;
                Message = message;
            }

            public EdiabasThread.EdiabasErrorReport ErrorReport { get; }
            public string Message { get; }
        }

        public class InstanceData
        {
            public InstanceData()
            {
                LastLocale = string.Empty;
                LastAppState = LastAppState.Init;
                LastSettingsHash = string.Empty;
                AppDataPath = string.Empty;
                EcuPath = string.Empty;
                VagPath = string.Empty;
                TraceActive = true;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                ConfigFileName = string.Empty;
                CheckCpuUsage = true;
                VerifyEcuFiles = true;
                SelectedEnetIp = string.Empty;
            }

            public string LastLocale { get; set; }
            public ActivityCommon.ThemeType? LastThemeType { get; set; }
            public LastAppState LastAppState { get; set; }
            public string LastSettingsHash { get; set; }
            public bool GetSettingsCalled { get; set; }
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
            public long LastVersionCode { get; set; }
            public bool VersionInfoShown { get; set; }
            public bool StorageRequirementsAccepted { get; set; }
            public bool LocationProviderShown { get; set; }
            public bool BatteryWarningShown { get; set; }
            public bool ConfigMatchVehicleShown { get; set; }
            public bool DataLogTemporaryShown { get; set; }
            public bool CheckCpuUsage { get; set; }
            public bool VerifyEcuFiles { get; set; }
            public bool VerifyEcuMd5 { get; set; }
            public int CommErrorsCount { get; set; }
            public bool AutoStart { get; set; }
            public bool VagInfoShown { get; set; }
            public string DataLogDir { get; set; }
            public string TraceDir { get; set; }
            public string TraceBackupDir { get; set; }
            public bool UpdateAvailable { get; set; }
            public int UpdateVersionCode { get; set; }
            public string UpdateMessage { get; set; }
            public long UpdateCheckTime { get; set; }
            public int UpdateSkipVersion { get; set; }
            public long TransLoginTimeNext { get; set; }
            public string XmlEditorPackageName { get; set; }
            public string XmlEditorClassName { get; set; }

            public ActivityCommon.InterfaceType SelectedInterface { get; set; }
            public string SelectedEnetIp { get; set; }
        }

        [XmlInclude(typeof(ActivityCommon.SerialInfoEntry))]
        [XmlType("Settings")]
        public class StorageData
        {
            public StorageData()
            {
                LastAppState = LastAppState.Init;
                SelectedLocale = string.Empty;
                SelectedTheme = ActivityCommon.ThemeDefault;
                DeviceName = string.Empty;
                DeviceAddress = string.Empty;
                ConfigFileName = string.Empty;
                UpdateCheckTime = DateTime.MinValue.Ticks;
                UpdateSkipVersion = -1;
                TransLoginTimeNext = DateTime.MinValue.Ticks;
                LastVersionCode = -1;
                StorageRequirementsAccepted = false;
                XmlEditorPackageName = string.Empty;
                XmlEditorClassName = string.Empty;

                RecentConfigFiles = new List<string>();
                CustomStorageMedia = ActivityCommon.CustomStorageMedia;
                CopyToAppSrc = ActivityCommon.CopyToAppSrc;
                CopyToAppDst = ActivityCommon.CopyToAppDst;
                CopyFromAppSrc = ActivityCommon.CopyFromAppSrc;
                CopyFromAppDst = ActivityCommon.CopyFromAppDst;
                UsbFirmwareFileName = ActivityCommon.UsbFirmwareFileName;
                EnableTranslation = ActivityCommon.EnableTranslation;
                EnableTranslateLogin = ActivityCommon.EnableTranslateLogin;
                YandexApiKey = ActivityCommon.YandexApiKey;
                IbmTranslatorApiKey = ActivityCommon.IbmTranslatorApiKey;
                IbmTranslatorUrl = ActivityCommon.IbmTranslatorUrl;
                Translator = !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey) ? ActivityCommon.TranslatorType.YandexTranslate : ActivityCommon.SelectedTranslator;
                ShowBatteryVoltageWarning = ActivityCommon.ShowBatteryVoltageWarning;
                BatteryWarnings = ActivityCommon.BatteryWarnings;
                BatteryWarningVoltage = ActivityCommon.BatteryWarningVoltage;
                SerialInfo = new List<ActivityCommon.SerialInfoEntry>();
                AdapterBlacklist = ActivityCommon.AdapterBlacklist;
                LastAdapterSerial = ActivityCommon.LastAdapterSerial;
                EmailAddress = ActivityCommon.EmailAddress;
                TraceInfo = ActivityCommon.TraceInfo;
                AppId = ActivityCommon.AppId;
                AutoHideTitleBar = ActivityCommon.AutoHideTitleBar;
                SuppressTitleBar = ActivityCommon.SuppressTitleBar;
                FullScreenMode = ActivityCommon.FullScreenMode;
                SwapMultiWindowOrientation = ActivityCommon.SwapMultiWindowOrientation;
                SelectedInternetConnection = ActivityCommon.SelectedInternetConnection;
                SelectedManufacturer = ActivityCommon.SelectedManufacturer;
                BtEnbaleHandling = ActivityCommon.BtEnbaleHandling;
                BtDisableHandling = ActivityCommon.BtDisableHandling;
                LockTypeCommunication = ActivityCommon.LockTypeCommunication;
                LockTypeLogging = ActivityCommon.LockTypeLogging;
                StoreDataLogSettings = ActivityCommon.StoreDataLogSettings;
                AutoConnectHandling = ActivityCommon.AutoConnectHandling;
                UpdateCheckDelay = ActivityCommon.UpdateCheckDelay;
                DoubleClickForAppExit = ActivityCommon.DoubleClickForAppExit;
                SendDataBroadcast = ActivityCommon.SendDataBroadcast;
                CheckCpuUsage = ActivityCommon.CheckCpuUsage;
                CheckEcuFiles = ActivityCommon.CheckEcuFiles;
                OldVagMode = ActivityCommon.OldVagMode;
                UseBmwDatabase = ActivityCommon.UseBmwDatabase;
                ShowOnlyRelevantErrors = ActivityCommon.ShowOnlyRelevantErrors;
                ScanAllEcus = ActivityCommon.ScanAllEcus;
                CollectDebugInfo = ActivityCommon.CollectDebugInfo;

                InitData(ActivityCommon.ActivityMainSettings);
            }

            public StorageData(ActivityMain activityMain, bool storage = false) : this()
            {
                InitData(activityMain, storage);
            }

            public void InitData(ActivityMain activityMain, bool storage = false)
            {
                if (activityMain == null)
                {
                    return;
                }

                InstanceData instanceData = activityMain._instanceData;
                ActivityCommon activityCommon = activityMain.ActivityCommonMain;

                if (instanceData == null || activityCommon == null)
                {
                    return;
                }

                LastAppState = instanceData.LastAppState;
                SelectedLocale = ActivityCommon.SelectedLocale ?? string.Empty;
                SelectedTheme = ActivityCommon.SelectedTheme ?? ActivityCommon.ThemeDefault;
                SelectedEnetIp = activityCommon.SelectedEnetIp;
                DeviceName = instanceData.DeviceName;
                DeviceAddress = instanceData.DeviceAddress;
                ConfigFileName = instanceData.ConfigFileName;
                UpdateCheckTime = instanceData.UpdateCheckTime;
                UpdateSkipVersion = instanceData.UpdateSkipVersion;
                TransLoginTimeNext = DateTime.MinValue.Ticks;
                LastVersionCode = activityMain._currentVersionCode;
                StorageRequirementsAccepted = instanceData.StorageRequirementsAccepted;
                XmlEditorPackageName = instanceData.XmlEditorPackageName ?? string.Empty;
                XmlEditorClassName = instanceData.XmlEditorClassName ?? string.Empty;
                if (storage)
                {
                    RecentConfigFiles = ActivityCommon.GetRecentConfigList();
                    SerialInfo = ActivityCommon.GetSerialInfoList();
                }
            }

            public string CalcualeHash()
            {
                StringBuilder sb = new StringBuilder();
                PropertyInfo[] properties = GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    object value = property.GetValue(this);
                    if (value != null)
                    {
                        sb.Append(value);
                    }
                }

                using (SHA256Managed sha256 = new SHA256Managed())
                {
                    return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))).Replace("-", "");
                }
            }

            [XmlElement("LastAppState")] public LastAppState LastAppState { get; set; }
            [XmlElement("Locale")] public string SelectedLocale { get; set; }
            [XmlElement("Theme")] public ActivityCommon.ThemeType SelectedTheme { get; set; }
            [XmlElement("EnetIp")] public string SelectedEnetIp { get; set; }
            [XmlElement("DeviceName")] public string DeviceName { get; set; }
            [XmlElement("DeviceAddress")] public string DeviceAddress { get; set; }
            [XmlElement("ConfigFile")] public string ConfigFileName { get; set; }
            [XmlElement("UpdateCheckTime")] public long UpdateCheckTime { get; set; }
            [XmlElement("UpdateSkipVersion")] public int UpdateSkipVersion { get; set; }
            [XmlElement("TransLoginTimeNext")] public long TransLoginTimeNext { get; set; }
            [XmlElement("VersionCode")] public long LastVersionCode { get; set; }
            [XmlElement("StorageAccepted")] public bool StorageRequirementsAccepted { get; set; }
            [XmlElement("XmlEditorPackageName")] public string XmlEditorPackageName { get; set; }
            [XmlElement("XmlEditorClassName")] public string XmlEditorClassName { get; set; }
            [XmlElement("DataLogActive")] public bool DataLogActive { get; set; }
            [XmlElement("DataLogAppend")] public bool DataLogAppend { get; set; }

            [XmlElement("RecentConfigFiles")] public List<string> RecentConfigFiles { get; set; }
            [XmlElement("StorageMedia")] public string CustomStorageMedia { get; set; }
            [XmlElement("CopyToAppSrc")] public string CopyToAppSrc { get; set; }
            [XmlElement("CopyToAppDst")] public string CopyToAppDst { get; set; }
            [XmlElement("CopyFromAppSrc")] public string CopyFromAppSrc { get; set; }
            [XmlElement("CopyFromAppDst")] public string CopyFromAppDst { get; set; }
            [XmlElement("UsbFirmwareFile")] public string UsbFirmwareFileName { get; set; }
            [XmlElement("EnableTranslation")] public bool EnableTranslation { get; set; }
            [XmlElement("EnableTranslateLogin")] public bool EnableTranslateLogin { get; set; }
            [XmlElement("YandexApiKey")] public string YandexApiKey { get; set; }
            [XmlElement("IbmTranslatorApiKey")] public string IbmTranslatorApiKey { get; set; }
            [XmlElement("IbmTranslatorUrl")] public string IbmTranslatorUrl { get; set; }
            [XmlElement("Translator")] public ActivityCommon.TranslatorType Translator { get; set; }
            [XmlElement("ShowBatteryVoltageWarning")] public bool ShowBatteryVoltageWarning { get; set; }
            [XmlElement("BatteryWarnings")] public long BatteryWarnings { get; set; }
            [XmlElement("BatteryWarningVoltage")] public double BatteryWarningVoltage { get; set; }
            [XmlElement("SerialInfo")] public List<ActivityCommon.SerialInfoEntry> SerialInfo { get; set; }
            [XmlElement("AdapterBlacklist")] public string AdapterBlacklist { get; set; }
            [XmlElement("LastAdapterSerial")] public string LastAdapterSerial { get; set; }
            [XmlElement("EmailAddress")] public string EmailAddress { get; set; }
            [XmlElement("TraceInfo")] public string TraceInfo { get; set; }
            [XmlElement("AppId")] public string AppId { get; set; }
            [XmlElement("AutoHideTitleBar")] public bool AutoHideTitleBar { get; set; }
            [XmlElement("SuppressTitleBar")] public bool SuppressTitleBar { get; set; }
            [XmlElement("FullScreenMode")] public bool FullScreenMode { get; set; }
            [XmlElement("SwapMultiWindowOrientation")] public bool SwapMultiWindowOrientation { get; set; }
            [XmlElement("InternetConnection")] public ActivityCommon.InternetConnectionType SelectedInternetConnection { get; set; }
            [XmlElement("Manufacturer")] public ActivityCommon.ManufacturerType SelectedManufacturer { get; set; }
            [XmlElement("BtEnbale")] public ActivityCommon.BtEnableType BtEnbaleHandling { get; set; }
            [XmlElement("BtDisable")] public ActivityCommon.BtDisableType BtDisableHandling { get; set; }
            [XmlElement("LockComm")] public ActivityCommon.LockType LockTypeCommunication { get; set; }
            [XmlElement("LockLog")] public ActivityCommon.LockType LockTypeLogging { get; set; }
            [XmlElement("StoreDataLogSettings")] public bool StoreDataLogSettings { get; set; }
            [XmlElement("AutoConnect")] public ActivityCommon.AutoConnectType AutoConnectHandling { get; set; }
            [XmlElement("UpdateCheckDelay")] public long UpdateCheckDelay { get; set; }
            [XmlElement("DoubleClickForAppExit")] public bool DoubleClickForAppExit { get; set; }
            [XmlElement("SendDataBroadcast")] public bool SendDataBroadcast { get; set; }
            [XmlElement("CheckCpuUsage")] public bool CheckCpuUsage { get; set; }
            [XmlElement("CheckEcuFiles")] public bool CheckEcuFiles { get; set; }
            [XmlElement("OldVagMode")] public bool OldVagMode { get; set; }
            [XmlElement("UseBmwDatabase")] public bool UseBmwDatabase { get; set; }
            [XmlElement("ShowOnlyRelevantErrors")] public bool ShowOnlyRelevantErrors { get; set; }
            [XmlElement("ScanAllEcus")] public bool ScanAllEcus { get; set; }
            [XmlElement("CollectDebugInfo")] public bool CollectDebugInfo { get; set; }
        }

#if DEBUG
        private static readonly string Tag = typeof(ActivityMain).FullName;
#endif
        private const string SharedAppName = ActivityCommon.AppNameSpace;
        private const string AppFolderName = ActivityCommon.AppNameSpace;
        private const string EcuDownloadUrl = @"https://www.holeschak.de/BmwDeepObd/Obb.php";
        private const long EcuExtractSize = 2600000000;         // extracted ecu files size
        private const string InfoXmlName = "ObbInfo.xml";
        private const string ContentFileName = "Content.xml";
        private const int MenuGroupRecentId = 1;
        private const int CpuLoadCritical = 70;
        private const int AutoHideTimeout = 3000;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage,
        };

        private readonly string[] _permissionsPostNotifications =
        {
            Android.Manifest.Permission.PostNotifications
        };

        public const string ExtraShowTitle = "show_title";
        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        public static bool StoreXmlEditor = Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1;
        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _updateOptionsMenu;
        private ActivityCommon.AutoConnectType _connectTypeRequest;
        private bool _backPressed;
        private long _lastBackPressedTime;
        private long _currentVersionCode;
        private bool _activityActive;
        private bool _onResumeExecuted;
        private bool _storageAccessGranted;
        private bool _notificationRequested;
        private bool _notificationGranted;
        private bool _locationPersissionRequested;
        private bool _locationPersissionGranted;
        private bool _overlayPermissionRequested;
        private bool _overlayPermissionGranted;
        private bool _storageManagerPermissionRequested;
        private bool _storageManagerPermissionGranted;
        private bool _createTabsPending;
        private bool _ignoreTabsChange;
        private bool _tabsCreated;
        private bool _compileCodePending;
        private long _maxDispUpdateTime;
        private ActivityCommon _activityCommon;
        public bool _autoHideStarted;
        public long _autoHideStartTime;
        private IMenu _optionsMenu;
        private Timer _autoHideTimer;
        private Handler _updateHandler;
        private BackupManager _backupManager;
        private TabLayout _tabLayout;
        private ViewPager2 _viewPager;
        private TabsFragmentStateAdapter _fragmentStateAdapter;
        private readonly ConnectButtonInfo _connectButtonInfo = new ConnectButtonInfo();
        private ImageView _imageBackground;
        private HttpClient _httpClient;
        private Thread _errorEvalThread;
        private CustomProgressDialog _downloadProgress;
        private CustomProgressDialog _compileProgress;
        private bool _extractZipCanceled;
        private string _assetFileName;
        private long _assetFileSize = -1;
        private AssetManager _assetManager;
        private AlertDialog _startAlertDialog;
        private AlertDialog _configSelectAlertDialog;
        private AlertDialog _downloadEcuAlertDialog;
        private AlertDialog _errorRestAlertDialog;
        private bool _translateActive;
        private List<string> _translationList;
        private List<string> _translatedList;

        public ActivityCommon ActivityCommonMain => _activityCommon;

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
            UpdateOptionsMenu();
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
            _touchShowTitle = true;
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SetContentView(Resource.Layout.main);

            _fragmentStateAdapter = new TabsFragmentStateAdapter(SupportFragmentManager, Lifecycle);
            _viewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
            _viewPager.Adapter = _fragmentStateAdapter;
            _tabLayout = FindViewById<TabLayout>(Resource.Id.tab_layout);
            TabLayoutMediator tabLayoutMediator = new TabLayoutMediator(_tabLayout, _viewPager, new TabConfigurationStrategy(this));
            tabLayoutMediator.Attach();

#pragma warning disable CS0618 // TabLayout.IOnTabSelectedListener2 creates compiler errors
            _tabLayout.AddOnTabSelectedListener(this);
#pragma warning restore CS0618
            _tabLayout.TabMode = TabLayout.ModeScrollable;
            _tabLayout.TabGravity = TabLayout.GravityStart;
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
                _activityCommon.SelectedEnetIp = _instanceData.SelectedEnetIp;
            }

            GetSettings();
            _activityCommon.SetPreferredNetworkInterface();

            _updateHandler = new Handler(Looper.MainLooper);
            _backupManager = new BackupManager(this);
            _imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);

            StoreLastAppState(LastAppState.Init);

            if (_httpClient == null)
            {
                _httpClient = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = ActivityCommon.DefaultSslProtocols,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                });
            }

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

            // get last active tab
            JobReader.PageInfo currentPage = null;
            if (IsCommActive())
            {
                _ignoreTabsChange = true;
                currentPage = ActivityCommon.EdiabasThread?.JobPageInfo;
            }
            else
            {
                if (_tabsCreated)
                {
                    currentPage = GetSelectedPage();
                }
            }

            int pageIndex = 0;
            if (currentPage != null)
            {
                int i = 0;
                foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                {
                    if (pageInfo == currentPage)
                    {
                        pageIndex = i;
                        break;
                    }
                    i++;
                }
            }

            _fragmentStateAdapter.ClearPages();
            _tabLayout.Visibility = ViewStates.Gone;

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

                _fragmentStateAdapter.AddPage(pageInfo, resourceId, GetPageString(pageInfo, pageInfo.Name));
            }
            _tabLayout.Visibility = (ActivityCommon.JobReader.PageList.Count > 0) ? ViewStates.Visible : ViewStates.Gone;
            _fragmentStateAdapter.NotifyDataSetChanged();
            if (_tabLayout.TabCount > 0)
            {
                if (pageIndex >= _tabLayout.TabCount)
                {
                    pageIndex = 0;
                }

                _updateHandler?.Post(() =>
                {
                    if (!_activityActive)
                    {
                        return;
                    }

                    _tabLayout.GetTabAt(pageIndex)?.Select();
                });
            }

            _ignoreTabsChange = false;
            _tabsCreated = true;
            UpdateDisplay();
            StoreLastAppState(LastAppState.TabsCreated);

            switch (_connectTypeRequest)
            {
                case ActivityCommon.AutoConnectType.Connect:
                case ActivityCommon.AutoConnectType.ConnectClose:
                    if (ActivityCommon.JobReader.PageList.Count > 0 &&
                        !ActivityCommon.CommActive && _activityCommon.IsInterfaceAvailable())
                    {
                        ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
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

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _instanceData.SelectedInterface = _activityCommon.SelectedInterface;
            _instanceData.SelectedEnetIp = _activityCommon.SelectedEnetIp;
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();

            _onResumeExecuted = false;
            _storageAccessGranted = false;
            _notificationRequested = false;
            _notificationGranted = false;
            _locationPersissionRequested = false;
            _locationPersissionGranted = false;
            _overlayPermissionRequested = false;
            _overlayPermissionGranted = false;
            _storageManagerPermissionRequested = false;
            _storageManagerPermissionGranted = false;
            _activityCommon?.StartMtcService();
        }

        protected override void OnStop()
        {
            base.OnStop();

            _activityCommon?.StopMtcService();
            StoreLastAppState(LastAppState.Stopped);
#if false
            try
            {
                Java.IO.File cacheDir = Android.App.Application.Context.CacheDir;
                if (cacheDir != null)
                {
                    Directory.Delete(cacheDir.AbsolutePath, true);
                }
            }
            catch (Exception)
            {
                // ignored
            }
#endif
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

            if (_storageAccessGranted)
            {
                _activityCommon?.RequestUsbPermission(null);
            }

            _activityActive = true;
            if (_activityCommon != null)
            {
                _activityCommon.MtcBtDisconnectWarnShown = false;
                _activityCommon.NotificationManagerCompat?.Cancel(CustomDownloadNotification.NotificationId);
            }

            UpdateLockState();
            if (_compileCodePending)
            {
                _updateHandler?.Post(CompileCode);
            }
            if (_createTabsPending)
            {
                _updateHandler?.Post(CreateActionBarTabs);
            }
            if (_startAlertDialog == null)
            {
                HandleStartDialogs(firstStart);
            }

            if (!IsCommActive())
            {
                UpdateCheck();
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

                            if (SupportActionBar == null)
                            {
                                return;
                            }

                            bool commActive = IsCommActive();
                            if (_autoHideStarted)
                            {
                                if (Stopwatch.GetTimestamp() - _autoHideStartTime >= AutoHideTimeout * ActivityCommon.TickResolMs)
                                {
                                    _autoHideStarted = false;
                                    if (commActive && SupportActionBar.IsShowing)
                                    {
                                        SupportActionBar.Hide();
                                    }
                                }
                            }
                            else
                            {
                                if (SupportActionBar.IsShowing && commActive)
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

            if (IsErrorEvalJobRunning())
            {
                _errorEvalThread.Join();
            }

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

            if (_tabLayout != null)
            {
                try
                {
#pragma warning disable CS0618 // TabLayout.IOnTabSelectedListener2 creates compiler errors
                    _tabLayout.RemoveOnTabSelectedListener(this);
#pragma warning restore CS0618
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _activityCommon?.Dispose();
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

        public override void OnBackPressedEvent()
        {
            if (!ActivityCommon.DoubleClickForAppExit)
            {
                _backPressed = false;
                _instanceData.CheckCpuUsage = true;
                base.OnBackPressedEvent();
                return;
            }
            if (_backPressed)
            {
                _backPressed = false;
                if (Stopwatch.GetTimestamp() - _lastBackPressedTime < 2000 * ActivityCommon.TickResolMs)
                {
                    _instanceData.CheckCpuUsage = true;
                    base.OnBackPressedEvent();
                    return;
                }
            }
            if (!_backPressed)
            {
                _backPressed = true;
                _lastBackPressedTime = Stopwatch.GetTimestamp();
                Toast.MakeText(this, GetString(Resource.String.back_button_twice_for_exit), ToastLength.Short)?.Show();
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            _updateHandler?.Post(() => { UpdateDisplay(true); });
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent != null)
            {
                bool showTitleRequest = intent.GetBooleanExtra(ExtraShowTitle, false);
                if (showTitleRequest)
                {
                    SupportActionBar.Show();
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            ActivityCommon.ActivityStartedFromMain = false;
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestAppStorePermissions:
                    RequestStoragePermissions(true);
                    break;

                case ActivityRequest.RequestAppDetailBtSettings:
                    UpdateOptionsMenu();
                    break;

                case ActivityRequest.RequestLocationSettings:
                    UpdateOptionsMenu();
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                    {
                        if (_activityCommon.LocationManager != null && _activityCommon.LocationManager.IsLocationEnabled)
                        {
                            if (_instanceData.AutoStart)
                            {
                                ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
                                break;
                            }
                        }
                    }

                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestAppSettingsAccessFiles:
                    StoragePermissionGranted();
                    UpdateOptionsMenu();
                    break;

                case ActivityRequest.RequestOverlayPermissions:
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                    {
                        _overlayPermissionGranted = Android.Provider.Settings.CanDrawOverlays(this);
                        if (_overlayPermissionGranted && _instanceData.AutoStart)
                        {
                            ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
                            break;
                        }
                    }

                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestNotificationSettingsApp:
                    if (_activityCommon.NotificationsEnabled() && _instanceData.AutoStart)
                    {
                        ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
                        break;
                    }

                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestNotificationSettingsChannel:
                    UpdateOptionsMenu();
                    if (_activityCommon.NotificationsEnabled(ActivityCommon.NotificationChannelCommunication) && _instanceData.AutoStart)
                    {
                        ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
                        break;
                    }

                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _instanceData.DeviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _instanceData.DeviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        bool callAdapterConfig = data.Extras.GetBoolean(DeviceListActivity.ExtraCallAdapterConfig, false);
                        StoreSettings();
                        UpdateOptionsMenu();
                        if (callAdapterConfig)
                        {
                            AdapterConfig();
                        }
                        else if (_instanceData.AutoStart)
                        {
                            ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
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
                            StoreSettings();
                            UpdateOptionsMenu();
                        }
                    }
                    break;

                case ActivityRequest.RequestSelectConfig:
                    // When FilePickerActivity returns with a file
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        _instanceData.ConfigFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        ActivityCommon.RecentConfigListAdd(_instanceData.ConfigFileName);
                        StoreSettings();
                        ReadConfigFile();
                        UpdateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestXmlTool:
                    // When XML tool returns with a file
                    _activityCommon.SetPreferredNetworkInterface();
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
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
                        ActivityCommon.RecentConfigListAdd(_instanceData.ConfigFileName);
                        StoreSettings();
                        ReadConfigFile();
                        UpdateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestEdiabasTool:
                    _activityCommon.SetPreferredNetworkInterface();
                    UpdateOptionsMenu();
                    break;

                case ActivityRequest.RequestBmwCoding:
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestYandexKey:
                    ActivityCommon.EnableTranslation = ActivityCommon.IsTranslationAvailable();
                    StoreSettings();
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestGlobalSettings:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                    {
                        string exportFileName = data.Extras.GetString(GlobalSettingsActivity.ExtraExportFile);
                        string importFileName = data.Extras.GetString(GlobalSettingsActivity.ExtraImportFile);
                        SettingsMode settingsMode = (SettingsMode) data.Extras.GetInt(GlobalSettingsActivity.ExtraSettingsMode, (int) SettingsMode.Private);
                        if (!string.IsNullOrEmpty(exportFileName))
                        {
                            if (!StoreSettings(exportFileName, settingsMode, out string errorMessage))
                            {
                                string message = GetString(Resource.String.store_settings_failed);
                                if (errorMessage != null)
                                {
                                    message += "\r\n" + errorMessage;
                                }

                                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                            }
                            else
                            {
                                string message = GetString(Resource.String.store_settings_filename) + "\r\n" + exportFileName;
                                _activityCommon.ShowAlert(message, Resource.String.alert_title_info);
                            }
                        }
                        else if (!string.IsNullOrEmpty(importFileName))
                        {
                            GetSettings(importFileName, SettingsMode.Private);
                        }
                    }

                    _activityCommon.SetPreferredNetworkInterface();
                    if ((_instanceData.LastThemeType ?? ActivityCommon.ThemeDefault) != (ActivityCommon.SelectedTheme ?? ActivityCommon.ThemeDefault) ||
                        string.Compare(_instanceData.LastLocale ?? string.Empty, ActivityCommon.SelectedLocale ?? string.Empty, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        StoreSettings();
                        // update translations
                        _activityCommon.RegisterNotificationChannels();
                        Recreate();
                        break;
                    }

                    StoreSettings();
                    UpdateCheck();
                    UpdateDirectories();
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    CheckForEcuFiles();
                    break;

                case ActivityRequest.RequestEditConfig:
                    if (data?.Extras != null && resultCode == Android.App.Result.Ok)
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
            _optionsMenu = menu;
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
            bool commActive = IsCommActive();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable();
            bool pageSgdb = !string.IsNullOrEmpty(GetSelectedPageSgdb());
            bool selectedPageFuncAvail = SelectedPageFunctionsAvailable();

            IMenuItem actionProviderConnect = menu.FindItem(Resource.Id.menu_action_provider_connect);
            if (actionProviderConnect != null)
            {
                AndroidX.Core.View.ActionProvider actionProvider = MenuItemCompat.GetActionProvider(actionProviderConnect);
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
            if (cfgSubmenu != null)
            {
                _activityCommon.SetMenuDocumentTreeTooltip(cfgSubmenu);
                cfgSubmenu.SetEnabled(!commActive);
            }

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

            List<string> recentConfigList = ActivityCommon.GetRecentConfigList();
            IMenuItem recentCfgMenu = menu.FindItem(Resource.Id.menu_recent_cfg_submenu);
            if (recentCfgMenu != null && recentConfigList != null)
            {
                recentCfgMenu.SetVisible(recentConfigList.Count > 0);
                ISubMenu recentCfgSubMenu = recentCfgMenu.SubMenu;
                if (recentCfgSubMenu != null)
                {
                    recentCfgSubMenu.RemoveGroup(MenuGroupRecentId);
                    int index = 0;
                    foreach (string fileName in recentConfigList)
                    {
                        string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                        if (!string.IsNullOrEmpty(baseFileName))
                        {
                            IMenuItem newMenu = recentCfgSubMenu.Add(MenuGroupRecentId, index, 1, baseFileName);
                            newMenu?.SetEnabled(!commActive);
                        }
                        index++;
                    }
                }
            }

            IMenuItem cfgPageFuncMenu = menu.FindItem(Resource.Id.menu_cfg_page_functions);
            if (cfgPageFuncMenu != null)
            {
                cfgPageFuncMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageFuncMenu.SetVisible(pageSgdb && !string.IsNullOrEmpty(_instanceData.ConfigFileName));
            }

            IMenuItem cfgPageEdiabasMenu = menu.FindItem(Resource.Id.menu_cfg_page_ediabas);
            if (cfgPageEdiabasMenu != null)
            {
                cfgPageEdiabasMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageEdiabasMenu.SetVisible(pageSgdb && !string.IsNullOrEmpty(_instanceData.ConfigFileName));
            }

            bool bmwVisible = ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && selectedPageFuncAvail && !string.IsNullOrEmpty(_instanceData.ConfigFileName);
            IMenuItem cfgPageBmwActuatorMenu = menu.FindItem(Resource.Id.menu_cfg_page_bmw_actuator);
            if (cfgPageBmwActuatorMenu != null)
            {
                cfgPageBmwActuatorMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageBmwActuatorMenu.SetVisible(bmwVisible);
            }

            bool vagVisible = ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && selectedPageFuncAvail && !string.IsNullOrEmpty(_instanceData.ConfigFileName);
            IMenuItem cfgPageVagCodingMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_coding);
            if (cfgPageVagCodingMenu != null)
            {
                cfgPageVagCodingMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageVagCodingMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagCoding2Menu = menu.FindItem(Resource.Id.menu_cfg_page_vag_coding2);
            if (cfgPageVagCoding2Menu != null)
            {
                cfgPageVagCoding2Menu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageVagCoding2Menu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagAdaptionMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_adaption);
            if (cfgPageVagAdaptionMenu != null)
            {
                cfgPageVagAdaptionMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageVagAdaptionMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagLoginMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_login);
            if (cfgPageVagLoginMenu != null)
            {
                cfgPageVagLoginMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageVagLoginMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagSecAccessMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_sec_access);
            if (cfgPageVagSecAccessMenu != null)
            {
                cfgPageVagSecAccessMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageVagSecAccessMenu.SetVisible(vagVisible);
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

            IMenuItem cfgPageBmwCodingMenu = menu.FindItem(Resource.Id.menu_bmw_coding);
            if (cfgPageBmwCodingMenu != null)
            {
                bool bmwCodingEnabled =
                    _activityCommon.IsBmwCodingInterface(_instanceData.DeviceAddress) && !string.IsNullOrEmpty(_instanceData.ConfigFileName);
                bool networkPresent = _activityCommon.IsNetworkPresent(out _);

                cfgPageBmwCodingMenu.SetEnabled(bmwCodingEnabled && interfaceAvailable && !commActive && networkPresent);
                cfgPageBmwCodingMenu.SetVisible(ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw);
            }

            IMenuItem downloadEcu = menu.FindItem(Resource.Id.menu_download_ecu);
            if (downloadEcu != null)
            {
                downloadEcu.SetTitle(Resource.String.menu_extract_ecu);
                downloadEcu.SetEnabled(!commActive);
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            if (logSubMenu != null)
            {
                _activityCommon.SetMenuDocumentTreeTooltip(logSubMenu);
                logSubMenu.SetEnabled(interfaceAvailable && !commActive);
            }

            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir));

            IMenuItem sendLastTraceMenu = menu.FindItem(Resource.Id.menu_send_last_trace);
            if (sendLastTraceMenu != null)
            {
                bool backupTrace = ActivityCommon.IsTraceFilePresent(_instanceData.TraceBackupDir);
                sendLastTraceMenu.SetEnabled(interfaceAvailable && !commActive && backupTrace);
                sendLastTraceMenu.SetVisible(backupTrace);
            }

            IMenuItem translationSubmenu = menu.FindItem(Resource.Id.menu_translation_submenu);
            if (translationSubmenu != null)
            {
                translationSubmenu.SetEnabled(true);
                translationSubmenu.SetVisible(ActivityCommon.IsTranslationRequired());
            }

            IMenuItem translationEnableMenu = menu.FindItem(Resource.Id.menu_translation_enable);
            if (translationEnableMenu != null)
            {
                translationEnableMenu.SetEnabled(!commActive || ActivityCommon.IsTranslationAvailable());
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
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string tooltipText = item.TooltipText;
                if (!string.IsNullOrWhiteSpace(tooltipText))
                {
                    Toast.MakeText(this, tooltipText, ToastLength.Short)?.Show();
                }
            }

            if (item.GroupId == MenuGroupRecentId)
            {
                bool found = false;
                IMenuItem recentCfgMenu = _optionsMenu?.FindItem(Resource.Id.menu_recent_cfg_submenu);
                ISubMenu recentCfgSubMenu = recentCfgMenu?.SubMenu;
                if (recentCfgSubMenu != null)
                {
                    if (recentCfgSubMenu.FindItem(item.ItemId) == item)
                    {
                        found = true;
                    }
                }

                if (found)
                {
                    int index = item.ItemId;
                    List<string> recentConfigList = ActivityCommon.GetRecentConfigList();
                    if (recentConfigList != null && index >= 0 && index < recentConfigList.Count)
                    {
                        _instanceData.ConfigFileName = recentConfigList[index];
                        ActivityCommon.RecentConfigListAdd(_instanceData.ConfigFileName);
                        StoreSettings();
                        ReadConfigFile();
                        UpdateOptionsMenu();
                    }
                    return true;
                }
            }

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

                case Resource.Id.menu_recent_cfg_clear:
                    ActivityCommon.RecentConfigListClear();
                    StoreSettings();
                    UpdateOptionsMenu();
                    return true;

                case Resource.Id.menu_cfg_page_ediabas:
                    StartEdiabasTool(true);
                    return true;

                case Resource.Id.menu_cfg_page_bmw_actuator:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.BmwActuator);
                    return true;

                case Resource.Id.menu_cfg_page_vag_coding:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.VagCoding);
                    return true;

                case Resource.Id.menu_cfg_page_vag_coding2:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.VagCoding2);
                    return true;

                case Resource.Id.menu_cfg_page_vag_adaption:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.VagAdaption);
                    return true;

                case Resource.Id.menu_cfg_page_vag_login:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.VagLogin);
                    return true;

                case Resource.Id.menu_cfg_page_vag_sec_access:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.VagSecAccess);
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
                    StoreSettings();
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

                case Resource.Id.menu_bmw_coding:
                {
                    bool allowBmwCoding = false;
                    string vehicleSeries = ActivityCommon.JobReader.VehicleSeries;

                    if (!string.IsNullOrEmpty(vehicleSeries) && vehicleSeries.Length > 0)
                    {
                        if (ActivityCommon.IsBmwCodingSeries(vehicleSeries))
                        {
                            allowBmwCoding = true;
                        }
                    }
                    else
                    {
                        string sgbdFunctional = ActivityCommon.JobReader.SgbdFunctional;
                        JobReader.PageInfo errorPage = ActivityCommon.JobReader.ErrorPage;

                        if (errorPage == null || !string.IsNullOrEmpty(sgbdFunctional))
                        {
                            allowBmwCoding = true;
                        }
                    }

                    if (!allowBmwCoding)
                    {
                        string message = string.Empty;
                        if (!string.IsNullOrEmpty(vehicleSeries))
                        {
                            message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.bmw_coding_series), vehicleSeries.ToUpperInvariant()) + "\n";
                        }

                        message += GetString(Resource.String.bmw_coding_requirement);
                        _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                        return true;
                    }

                    StartBmwCoding();
                    return true;
                }

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

                case Resource.Id.menu_send_last_trace:
                    SendBackupTraceFileAlways((sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        UpdateOptionsMenu();
                    });
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

                        try
                        {
                            StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://github.com/uholeschak/ediabaslib/blob/master/docs/Deep_OBD_for_BMW_and_VAG.md")));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                    return true;

                case Resource.Id.menu_info:
                {
                    string message = string.Format(GetString(Resource.String.app_info_message),
                        _activityCommon.GetPackageInfo()?.VersionName ?? string.Empty , ActivityCommon.AppId);
                    new AlertDialog.Builder(this)
                        .SetNeutralButton(Resource.String.button_donate, (sender, args) =>
                        {
                            OpenDonateLink();
                        })
                        .SetNegativeButton(Resource.String.button_copy, (sender, args) =>
                        {
                            _activityCommon.SetClipboardText(message);
                        })
                        .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(message)
                        .SetTitle(Resource.String.alert_title_info)
                        .Show();
                    return true;
                }

                case Resource.Id.menu_exit:
                    StoreSettings();
                    OnDestroy();
                    Java.Lang.Runtime.GetRuntime()?.Exit(0);
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
                case ActivityCommon.RequestPermissionExternalStorage:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        StoragePermissionGranted();
                        break;
                    }

                    if (!ActivityCommon.IsExtrenalStorageAccessRequired())
                    {
                        StoragePermissionGranted();
                        break;
                    }

                    bool finish = true;
                    AlertDialog alertDialog = new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            if (ActivityCommon.OpenAppSettingDetails(this, (int)ActivityRequest.RequestAppStorePermissions))
                            {
                                finish = false;
                            }
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.access_denied_ext_storage)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();

                    alertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        if (finish)
                        {
                            Finish();
                        }
                    };
                    break;

                case ActivityCommon.RequestPermissionNotifications:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        NotificationsPermissionGranted();
                        break;
                    }

                    NotificationsPermissionGranted(false);
                    break;

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

                case ActivityCommon.RequestPermissionLocation:
                    if (grantResults.Length > 0 && grantResults.All(permission => permission == Permission.Granted))
                    {
                        UpdateOptionsMenu();
                        if (LocationPermissionsGranted((sender, args) =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                if (_instanceData.AutoStart)
                                {
                                    ButtonConnectClick(sender, args);
                                }
                            }))
                        {
                            break;
                        }
                    }

                    if (_instanceData.AutoStart)
                    {
                        ButtonConnectClick(_connectButtonInfo.Button, EventArgs.Empty);
                        break;
                    }

                    _instanceData.AutoStart = false;
                    break;
            }
        }

        protected void ButtonConnectClick(object sender, EventArgs e)
        {
            _instanceData.AutoStart = false;
            if (!CheckForEcuFiles())
            {
                UpdateDisplay();
                return;
            }

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
                if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Enet)
                {
                    if (RequestLocationPermissions())
                    {
                        return;
                    }
                }

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

            if (UseCommService())
            {
                if (RequestOverlayPermissions((o, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        ButtonConnectClick(sender, e);
                    }))
                {
                    return;
                }

                if (RequestNotificationPermissions((o, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        ButtonConnectClick(sender, e);
                    }))
                {
                    return;
                }
            }

            if (!IsCommActive())
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
                    else
                    {
                        _instanceData.VerifyEcuMd5 = true;
                        _updateHandler?.Post(CompileCode);
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
            if (_activityCommon == null)
            {
                return;
            }

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
            if (_activityCommon == null)
            {
                return;
            }

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
            if (_activityCommon == null)
            {
                return;
            }

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
            if (_activityCommon == null)
            {
                return;
            }

            if (!ActivityCommon.CommActive)
            {
                return;
            }

            Button buttonErrorSelect = v.FindViewById<Button>(Resource.Id.button_error_select);
            bool select = buttonErrorSelect?.Tag is Java.Lang.Boolean selectAll && selectAll.BooleanValue();

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
            if (_activityCommon == null)
            {
                return;
            }

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
            TranslateLogin();

            long updateCheckDelay = ActivityCommon.UpdateCheckDelay;
            bool serialCheck = ActivityCommon.IsSerialNumberCheckRequired();
            if (serialCheck)
            {
                updateCheckDelay = 0;
            }

            if (updateCheckDelay < 0)
            {
                _instanceData.UpdateCheckTime = DateTime.MinValue.Ticks;
                _instanceData.UpdateSkipVersion = -1;
                return false;
            }

            TimeSpan timeDiff = new TimeSpan(DateTime.Now.Ticks - _instanceData.UpdateCheckTime);
            if (timeDiff.Ticks < updateCheckDelay)
            {
                return false;
            }

            bool hideMessage = (ActivityCommon.UpdateCheckDelay < 0) || (serialCheck && timeDiff.Ticks < ActivityCommon.UpdateCheckDelay);
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
                            if (!hideMessage)
                            {
                                DisplayUpdateInfo((sender, args) =>
                                {
                                });
                            }
                        }
                    });
                }
            }, _instanceData.UpdateSkipVersion);

            return result;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool TranslateLogin()
        {
            if (!ActivityCommon.EnableTranslateLogin)
            {
                _instanceData.TransLoginTimeNext = DateTime.MinValue.Ticks;
                return false;
            }

            if (DateTime.Now.Ticks < _instanceData.TransLoginTimeNext)
            {
                return false;
            }

            bool result = _activityCommon.TranslateLogin(success =>
            {
                _instanceData.TransLoginTimeNext = DateTime.Now.Ticks + (success ? TimeSpan.TicksPerDay : TimeSpan.TicksPerHour);
                StoreSettings();
            });

            return result;
        }

        private bool IsErrorEvalJobRunning()
        {
            if (_errorEvalThread == null)
            {
                return false;
            }
            if (_errorEvalThread.IsAlive)
            {
                return true;
            }
            _errorEvalThread = null;
            return false;
        }

        private bool IsCommActive()
        {
            if (ActivityCommon.CommActive)
            {
                return true;
            }

            if (IsErrorEvalJobRunning())
            {
                return true;
            }

            return false;
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
            _instanceData.CommErrorsCount = 0;
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

                _instanceData.TraceBackupDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.TraceBackupDir);
                _translationList = null;
                _translatedList = null;
                _maxDispUpdateTime = 0;

                JobReader.PageInfo pageInfo = GetSelectedPage();
                object connectParameter = null;
                if (pageInfo != null)
                {
                    string portName = string.Empty;
                    switch (_activityCommon.SelectedInterface)
                    {
                        case ActivityCommon.InterfaceType.Bluetooth:
                            portName = "BLUETOOTH:" + _instanceData.DeviceAddress;
                            connectParameter = new EdBluetoothInterface.ConnectParameterType(_activityCommon.NetworkData, _activityCommon.MtcBtService, _activityCommon.MtcBtEscapeMode,
                                () => ActivityCommon.EdiabasThread.ActiveContext);
                            _activityCommon.ConnectMtcBtDevice(_instanceData.DeviceAddress);
                            break;

                        case ActivityCommon.InterfaceType.Enet:
                            connectParameter = new EdInterfaceEnet.ConnectParameterType(_activityCommon.NetworkData);
                            if (_activityCommon.Emulator && !string.IsNullOrEmpty(ActivityCommon.EmulatorEnetIp))
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
                        _instanceData.VagPath, _instanceData.BmwPath, _instanceData.TraceDir, _instanceData.TraceAppend, _instanceData.DataLogDir, _instanceData.DataLogAppend);
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
                    lockType = ActivityCommon.LockType.ScreenDim;
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
            if (instanceData == null)
            {
                if (ActivityCommon.SelectedLocale == null)
                {
                    string settingsFile = ActivityCommon.GetSettingsFileName();
                    if (!string.IsNullOrEmpty(settingsFile) && File.Exists(settingsFile))
                    {
                        GetLocaleThemeSettings(settingsFile, true, false);
                    }
                }

                if (ActivityCommon.SelectedLocale != null)
                {
                    return ActivityCommon.SelectedLocale;
                }
            }

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

        private void GetThemeSettings(InstanceData instanceData = null)
        {
            if (instanceData == null)
            {
                string settingsFile = ActivityCommon.GetSettingsFileName();
                if (!string.IsNullOrEmpty(settingsFile) && File.Exists(settingsFile))
                {
                    if (ActivityCommon.SelectedTheme == null)
                    {
                        GetLocaleThemeSettings(settingsFile, false, true);
                    }

                    return;
                }
            }

            try
            {
                if (ActivityCommon.SelectedTheme == null)
                {
                    ISharedPreferences prefs = Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
                    ActivityCommon.SelectedTheme = (ActivityCommon.ThemeType)prefs.GetInt("Theme", (int)ActivityCommon.ThemeDefault);
                }

                if (instanceData != null)
                {
                    instanceData.LastThemeType = ActivityCommon.SelectedTheme;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void GetSettings()
        {
            PackageInfo packageInfo = PackageManager?.GetPackageInfo(PackageName ?? string.Empty, 0);
            _currentVersionCode = packageInfo != null ? PackageInfoCompat.GetLongVersionCode(packageInfo) : 0;
            string assetFileName = ExpansionDownloaderActivity.GetAssetFilename();
            if (!string.IsNullOrEmpty(assetFileName))
            {
                try
                {
                    AssetManager assetManager = ActivityCommon.GetPackageContext()?.Assets;
                    if (assetManager != null)
                    {
                        AssetFileDescriptor assetFile = assetManager.OpenFd(assetFileName);
                        _assetManager = assetManager;
                        _assetFileName = assetFileName;
                        _assetFileSize = assetFile.Length;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (_assetManager == null)
            {
                _assetFileName = ExpansionDownloaderActivity.GetObbFilename(this);
                _assetFileSize = -1;
            }

            ActivityCommon.AssetFileName = _assetFileName;
            ActivityCommon.AssetFileSize = _assetFileSize;

            string settingsFile = ActivityCommon.GetSettingsFileName();
            if (!string.IsNullOrEmpty(settingsFile) && File.Exists(settingsFile))
            {
                if (GetSettings(settingsFile, SettingsMode.All))
                {
                    return;
                }
            }

            GetPrefSettings();
        }

        private void GetPrefSettings()
        {
            GetLocaleSetting(_instanceData);
            GetThemeSettings(_instanceData);

            try
            {
                ISharedPreferences prefs =
                    Android.App.Application.Context.GetSharedPreferences(SharedAppName, FileCreationMode.Private);
#if false // simulate settings reset
                ISharedPreferencesEditor prefsEdit = prefs.Edit();
                prefsEdit.Clear();
                prefsEdit.Commit();
#endif
                if (!ActivityCommon.StaticDataInitialized || !_activityRecreated)
                {
                    string stateString = prefs.GetString("LastAppState", string.Empty);
                    _instanceData.LastAppState = System.Enum.TryParse(stateString, true, out LastAppState lastAppState)
                        ? lastAppState
                        : LastAppState.Init;
                    _activityCommon.SelectedEnetIp = prefs.GetString("EnetIp", string.Empty);
                    _instanceData.DeviceName = prefs.GetString("DeviceName", string.Empty);
                    _instanceData.DeviceAddress = prefs.GetString("DeviceAddress", string.Empty);
                    _instanceData.ConfigFileName = prefs.GetString("ConfigFile", string.Empty);
                    _instanceData.UpdateCheckTime = prefs.GetLong("UpdateCheckTime", DateTime.MinValue.Ticks);
                    _instanceData.UpdateSkipVersion = prefs.GetInt("UpdateSkipVersion", -1);
                    _instanceData.TransLoginTimeNext = prefs.GetLong("TransLoginTimeNext", DateTime.MinValue.Ticks);
                    _instanceData.LastVersionCode = prefs.GetLong("VersionCodeLong", -1);
                    _instanceData.StorageRequirementsAccepted = prefs.GetBoolean("StorageAccepted", false);
                    _instanceData.XmlEditorPackageName = prefs.GetString("XmlEditorPackageName", string.Empty);
                    _instanceData.XmlEditorClassName = prefs.GetString("XmlEditorClassName", string.Empty);

                    _activityCommon.SetDefaultSettings();
                    ActivityCommon.CustomStorageMedia =
                        prefs.GetString("StorageMedia", ActivityCommon.CustomStorageMedia);
                    ActivityCommon.EnableTranslation =
                        prefs.GetBoolean("EnableTranslation", ActivityCommon.EnableTranslation);
                    ActivityCommon.EnableTranslateLogin =
                        prefs.GetBoolean("EnableTranslateLogin", ActivityCommon.EnableTranslateLogin);
                    ActivityCommon.YandexApiKey = prefs.GetString("YandexApiKey", ActivityCommon.YandexApiKey);
                    ActivityCommon.IbmTranslatorApiKey =
                        prefs.GetString("IbmTranslatorApiKey", ActivityCommon.IbmTranslatorApiKey);
                    ActivityCommon.IbmTranslatorUrl =
                        prefs.GetString("IbmTranslatorUrl", ActivityCommon.IbmTranslatorUrl);
                    ActivityCommon.TranslatorType defaultTranslator =
                        !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey)
                            ? ActivityCommon.TranslatorType.YandexTranslate
                            : ActivityCommon.SelectedTranslator;
                    _activityCommon.Translator =
                        (ActivityCommon.TranslatorType) prefs.GetLong("Translator", (int) defaultTranslator);
                    ActivityCommon.ShowBatteryVoltageWarning = prefs.GetBoolean("ShowBatteryWarning",
                        ActivityCommon.ShowBatteryVoltageWarning);
                    ActivityCommon.BatteryWarnings = prefs.GetLong("BatteryWarnings", ActivityCommon.BatteryWarnings);
                    ActivityCommon.BatteryWarningVoltage = prefs.GetFloat("BatteryWarningVoltage",
                        (float) ActivityCommon.BatteryWarningVoltage);
                    ActivityCommon.AdapterBlacklist =
                        prefs.GetString("AdapterBlacklist", ActivityCommon.AdapterBlacklist);
                    ActivityCommon.LastAdapterSerial =
                        prefs.GetString("LastAdapterSerial", ActivityCommon.LastAdapterSerial);
                    ActivityCommon.EmailAddress = prefs.GetString("EmailAddress", ActivityCommon.EmailAddress);
                    ActivityCommon.TraceInfo = prefs.GetString("TraceInfo", ActivityCommon.TraceInfo);
                    ActivityCommon.AppId = prefs.GetString("AppId", ActivityCommon.AppId);
                    ActivityCommon.AutoHideTitleBar =
                        prefs.GetBoolean("AutoHideTitleBar", ActivityCommon.AutoHideTitleBar);
                    ActivityCommon.SuppressTitleBar =
                        prefs.GetBoolean("SuppressTitleBar", ActivityCommon.SuppressTitleBar);
                    ActivityCommon.FullScreenMode = prefs.GetBoolean("FullScreenMode", ActivityCommon.FullScreenMode);
                    ActivityCommon.SwapMultiWindowOrientation = prefs.GetBoolean("SwapMultiWindowOrientation",
                        ActivityCommon.SwapMultiWindowOrientation);
                    ActivityCommon.SelectedInternetConnection =
                        (ActivityCommon.InternetConnectionType) prefs.GetInt("InternetConnection",
                            (int) ActivityCommon.SelectedInternetConnection);
                    ActivityCommon.SelectedManufacturer =
                        (ActivityCommon.ManufacturerType) prefs.GetInt("Manufacturer",
                            (int) ActivityCommon.SelectedManufacturer);
                    ActivityCommon.BtEnbaleHandling =
                        (ActivityCommon.BtEnableType) prefs.GetInt("BtEnable", (int) ActivityCommon.BtEnbaleHandling);
                    ActivityCommon.BtDisableHandling =
                        (ActivityCommon.BtDisableType) prefs.GetInt("BtDisable",
                            (int) ActivityCommon.BtDisableHandling);
                    ActivityCommon.LockTypeCommunication =
                        (ActivityCommon.LockType) prefs.GetInt("LockComm", (int) ActivityCommon.LockTypeCommunication);
                    ActivityCommon.LockTypeLogging =
                        (ActivityCommon.LockType) prefs.GetInt("LockLog", (int) ActivityCommon.LockTypeLogging);
                    ActivityCommon.StoreDataLogSettings =
                        prefs.GetBoolean("StoreDataLogSettings", ActivityCommon.StoreDataLogSettings);
                    if (ActivityCommon.StoreDataLogSettings)
                    {
                        _instanceData.DataLogActive = prefs.GetBoolean("DataLogActive", _instanceData.DataLogActive);
                        _instanceData.DataLogAppend = prefs.GetBoolean("DataLogAppend", _instanceData.DataLogAppend);
                    }

                    ActivityCommon.AutoConnectHandling =
                        (ActivityCommon.AutoConnectType) prefs.GetInt("AutoConnect",
                            (int) ActivityCommon.AutoConnectHandling);
                    ActivityCommon.UpdateCheckDelay =
                        prefs.GetLong("UpdateCheckDelay", ActivityCommon.UpdateCheckDelay);
                    ActivityCommon.DoubleClickForAppExit =
                        prefs.GetBoolean("DoubleClickForExit", ActivityCommon.DoubleClickForAppExit);
                    ActivityCommon.SendDataBroadcast =
                        prefs.GetBoolean("SendDataBroadcast", ActivityCommon.SendDataBroadcast);
                    ActivityCommon.CheckCpuUsage = prefs.GetBoolean("CheckCpuUsage", ActivityCommon.CheckCpuUsage);
                    ActivityCommon.CheckEcuFiles = prefs.GetBoolean("CheckEcuFiles", ActivityCommon.CheckEcuFiles);
                    ActivityCommon.OldVagMode = prefs.GetBoolean("OldVagMode", ActivityCommon.OldVagMode);
                    ActivityCommon.UseBmwDatabase = prefs.GetBoolean("UseBmwDatabase", ActivityCommon.UseBmwDatabase);
                    ActivityCommon.ScanAllEcus = prefs.GetBoolean("ScanAllEcus", ActivityCommon.ScanAllEcus);
                    ActivityCommon.CollectDebugInfo =
                        prefs.GetBoolean("CollectDebugInfo", ActivityCommon.CollectDebugInfo);

                    CheckSettingsVersionChange();
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                ActivityCommon.StaticDataInitialized = true;
                _instanceData.LastSettingsHash = string.Empty;
                _instanceData.GetSettingsCalled = true;
            }
        }

        private void CheckSettingsVersionChange()
        {
            if (_instanceData.LastVersionCode != _currentVersionCode)
            {
                _instanceData.StorageRequirementsAccepted = false;
                _instanceData.UpdateCheckTime = DateTime.MinValue.Ticks;
                _instanceData.UpdateSkipVersion = -1;
                ActivityCommon.BatteryWarnings = 0;
                ActivityCommon.BatteryWarningVoltage = 0;
            }
        }

        private void StoreSettings()
        {
            if (!StoreSettings(ActivityCommon.GetSettingsFileName(), SettingsMode.All, out string errorMessage))
            {
                string message = GetString(Resource.String.store_settings_failed);
                if (errorMessage != null)
                {
                    message += "\r\n" + errorMessage;
                }

                Toast.MakeText(this, message, ToastLength.Long)?.Show();
            }
        }

#if false
        private void StorePrefsSettings()
        {
            try
            {
                if (!ActivityCommon.StaticDataInitialized || !_instanceData.GetSettingsCalled)
                {
                    return;
                }

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
                prefsEdit.PutLong("TransLoginTimeNext", _instanceData.TransLoginTimeNext);
                prefsEdit.PutString("StorageMedia", _activityCommon.CustomStorageMedia ?? string.Empty);
                prefsEdit.PutLong("VersionCodeLong", _currentVersionCode);
                prefsEdit.PutBoolean("StorageAccepted", _instanceData.StorageRequirementsAccepted);
                prefsEdit.PutString("XmlEditorPackageName", _instanceData.XmlEditorPackageName ?? string.Empty);
                prefsEdit.PutString("XmlEditorClassName", _instanceData.XmlEditorClassName ?? string.Empty);
                prefsEdit.PutBoolean("EnableTranslation", ActivityCommon.EnableTranslation);
                prefsEdit.PutBoolean("EnableTranslateLogin", ActivityCommon.EnableTranslateLogin);
                prefsEdit.PutString("YandexApiKey", ActivityCommon.YandexApiKey ?? string.Empty);
                prefsEdit.PutString("IbmTranslatorApiKey", ActivityCommon.IbmTranslatorApiKey ?? string.Empty);
                prefsEdit.PutString("IbmTranslatorUrl", ActivityCommon.IbmTranslatorUrl ?? string.Empty);
                prefsEdit.PutLong("Translator", (int)ActivityCommon.SelectedTranslator);
                prefsEdit.PutBoolean("ShowBatteryWarning", ActivityCommon.ShowBatteryVoltageWarning);
                prefsEdit.PutLong("BatteryWarnings", ActivityCommon.BatteryWarnings);
                prefsEdit.PutFloat("BatteryWarningVoltage", (float)ActivityCommon.BatteryWarningVoltage);
                prefsEdit.PutString("AdapterBlacklist", ActivityCommon.AdapterBlacklist ?? string.Empty);
                prefsEdit.PutString("LastAdapterSerial", ActivityCommon.LastAdapterSerial ?? string.Empty);
                prefsEdit.PutString("EmailAddress", ActivityCommon.EmailAddress ?? string.Empty);
                prefsEdit.PutString("TraceInfo", ActivityCommon.TraceInfo ?? string.Empty);
                prefsEdit.PutString("AppId", ActivityCommon.AppId);
                prefsEdit.PutString("Locale", ActivityCommon.SelectedLocale ?? string.Empty);
                prefsEdit.PutInt("Theme", (int)ActivityCommon.SelectedTheme);
                prefsEdit.PutBoolean("AutoHideTitleBar", ActivityCommon.AutoHideTitleBar);
                prefsEdit.PutBoolean("SuppressTitleBar", ActivityCommon.SuppressTitleBar);
                prefsEdit.PutBoolean("FullScreenMode", ActivityCommon.FullScreenMode);
                prefsEdit.PutBoolean("SwapMultiWindowOrientation", ActivityCommon.SwapMultiWindowOrientation);
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
#endif

        private void StoreLastAppState(LastAppState lastAppState)
        {
            _instanceData.LastAppState = lastAppState;
            StoreSettings();
        }

        public static XmlAttributeOverrides GetStoreXmlAttributeOverrides(SettingsMode settingsMode)
        {
            if (settingsMode == SettingsMode.All)
            {
                return null;
            }

            StorageData storageData = new StorageData();
            Type storageType = storageData.GetType();
            XmlAttributes ignoreXmlAttributes = new XmlAttributes
            {
                XmlIgnore = true
            };

            XmlAttributeOverrides storageClassAttributes = new XmlAttributeOverrides();
            storageClassAttributes.Add(storageType, nameof(storageData.LastAppState), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.UpdateCheckTime), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.UpdateSkipVersion), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.TransLoginTimeNext), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.LastVersionCode), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.StorageRequirementsAccepted), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.BatteryWarnings), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.BatteryWarningVoltage), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.SerialInfo), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.AdapterBlacklist), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.LastAdapterSerial), ignoreXmlAttributes);
            storageClassAttributes.Add(storageType, nameof(storageData.AppId), ignoreXmlAttributes);
            if (settingsMode == SettingsMode.Public)
            {
                storageClassAttributes.Add(storageType, nameof(storageData.SelectedEnetIp), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.DeviceName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.DeviceAddress), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.ConfigFileName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.XmlEditorPackageName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.XmlEditorClassName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.RecentConfigFiles), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.CustomStorageMedia), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.UsbFirmwareFileName), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.YandexApiKey), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.IbmTranslatorApiKey), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.IbmTranslatorUrl), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.EmailAddress), ignoreXmlAttributes);
                storageClassAttributes.Add(storageType, nameof(storageData.TraceInfo), ignoreXmlAttributes);
            }

            return storageClassAttributes;
        }

        public static StorageData GetStorageData(string fileName, ActivityMain activityMain, SettingsMode settingsMode = SettingsMode.All)
        {
            StorageData storageData = null;
            try
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        lock (ActivityCommon.GlobalSettingLockObject)
                        {
                            ActivityCommon.ActivityMainSettings = activityMain;
                            try
                            {
                                XmlAttributeOverrides storageClassAttributes = GetStoreXmlAttributeOverrides(settingsMode);
                                XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageData), storageClassAttributes);
                                using (StreamReader sr = new StreamReader(fileName))
                                {
                                    storageData = xmlSerializer.Deserialize(sr) as StorageData;
                                }
                            }
                            finally
                            {
                                ActivityCommon.ActivityMainSettings = null;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        storageData = null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            storageData ??= new StorageData();

            return storageData;
        }

        public static bool GetLocaleThemeSettings(string fileName, bool updateLocale, bool updateTheme)
        {
            StorageData storageData = GetStorageData(fileName, ActivityCommon.ActivityMainCurrent);

            if (updateLocale)
            {
                ActivityCommon.SelectedLocale = storageData.SelectedLocale;
            }

            if (updateTheme)
            {
                ActivityCommon.SelectedTheme = storageData.SelectedTheme;
            }

            return true;
        }

        public bool GetSettings(string fileName, SettingsMode settingsMode)
        {
            if (_instanceData == null || _activityCommon == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            bool import = settingsMode != SettingsMode.All;
            string hash = string.Empty;
            try
            {
                bool init = false;
                if (!ActivityCommon.StaticDataInitialized || !_activityRecreated)
                {
                    init = true;
                    _activityCommon.SetDefaultSettings();
                }

                StorageData storageData = GetStorageData(fileName, this, settingsMode);
                hash = storageData.CalcualeHash();

                if (init || import)
                {
                    _instanceData.LastAppState = storageData.LastAppState;
                    _activityCommon.SelectedEnetIp = storageData.SelectedEnetIp;
                    _instanceData.DeviceName = storageData.DeviceName;
                    _instanceData.DeviceAddress = storageData.DeviceAddress;
                    _instanceData.ConfigFileName = storageData.ConfigFileName;
                    _instanceData.UpdateCheckTime = storageData.UpdateCheckTime;
                    _instanceData.UpdateSkipVersion = storageData.UpdateSkipVersion;
                    _instanceData.TransLoginTimeNext = storageData.TransLoginTimeNext;
                    _instanceData.LastVersionCode = storageData.LastVersionCode;
                    _instanceData.StorageRequirementsAccepted = storageData.StorageRequirementsAccepted;
                    _instanceData.XmlEditorPackageName = storageData.XmlEditorPackageName;
                    _instanceData.XmlEditorClassName = storageData.XmlEditorClassName;

                    ActivityCommon.SelectedLocale = storageData.SelectedLocale;
                    ActivityCommon.SelectedTheme = storageData.SelectedTheme;

                    ActivityCommon.SetRecentConfigList(storageData.RecentConfigFiles);
                    ActivityCommon.CustomStorageMedia = storageData.CustomStorageMedia;
                    ActivityCommon.CopyToAppSrc = storageData.CopyToAppSrc;
                    ActivityCommon.CopyToAppDst = storageData.CopyToAppDst;
                    ActivityCommon.CopyFromAppSrc = storageData.CopyFromAppSrc;
                    ActivityCommon.CopyFromAppDst = storageData.CopyFromAppDst;
                    ActivityCommon.UsbFirmwareFileName = storageData.UsbFirmwareFileName;
                    ActivityCommon.EnableTranslation = storageData.EnableTranslation;
                    ActivityCommon.EnableTranslateLogin = storageData.EnableTranslateLogin;
                    ActivityCommon.YandexApiKey = storageData.YandexApiKey;
                    ActivityCommon.IbmTranslatorApiKey = storageData.IbmTranslatorApiKey;
                    ActivityCommon.IbmTranslatorUrl = storageData.IbmTranslatorUrl;
                    _activityCommon.Translator = storageData.Translator;
                    ActivityCommon.ShowBatteryVoltageWarning = storageData.ShowBatteryVoltageWarning;
                    ActivityCommon.BatteryWarnings = storageData.BatteryWarnings;
                    ActivityCommon.BatteryWarningVoltage = storageData.BatteryWarningVoltage;
                    ActivityCommon.SetSerialInfoList(storageData.SerialInfo);
                    ActivityCommon.AdapterBlacklist = storageData.AdapterBlacklist;
                    ActivityCommon.LastAdapterSerial = storageData.LastAdapterSerial;
                    ActivityCommon.EmailAddress = storageData.EmailAddress;
                    ActivityCommon.TraceInfo = storageData.TraceInfo;
                    ActivityCommon.AppId = storageData.AppId;
                    ActivityCommon.AutoHideTitleBar = storageData.AutoHideTitleBar;
                    ActivityCommon.SuppressTitleBar = storageData.SuppressTitleBar;
                    ActivityCommon.FullScreenMode = storageData.FullScreenMode;
                    ActivityCommon.SwapMultiWindowOrientation = storageData.SwapMultiWindowOrientation;
                    ActivityCommon.SelectedInternetConnection = storageData.SelectedInternetConnection;
                    ActivityCommon.SelectedManufacturer = storageData.SelectedManufacturer;
                    ActivityCommon.BtEnbaleHandling = storageData.BtEnbaleHandling;
                    ActivityCommon.BtDisableHandling = storageData.BtDisableHandling;
                    ActivityCommon.LockTypeCommunication = storageData.LockTypeCommunication;
                    ActivityCommon.LockTypeLogging = storageData.LockTypeLogging;
                    ActivityCommon.StoreDataLogSettings = storageData.StoreDataLogSettings;
                    if (ActivityCommon.StoreDataLogSettings)
                    {
                        _instanceData.DataLogActive = storageData.DataLogActive;
                        _instanceData.DataLogAppend = storageData.DataLogAppend;
                    }
                    ActivityCommon.AutoConnectHandling = storageData.AutoConnectHandling;
                    ActivityCommon.UpdateCheckDelay = storageData.UpdateCheckDelay;
                    ActivityCommon.DoubleClickForAppExit = storageData.DoubleClickForAppExit;
                    ActivityCommon.SendDataBroadcast = storageData.SendDataBroadcast;
                    ActivityCommon.CheckCpuUsage = storageData.CheckCpuUsage;
                    ActivityCommon.CheckEcuFiles = storageData.CheckEcuFiles;
                    ActivityCommon.OldVagMode = storageData.OldVagMode;
                    ActivityCommon.UseBmwDatabase = storageData.UseBmwDatabase;
                    ActivityCommon.ShowOnlyRelevantErrors = storageData.ShowOnlyRelevantErrors;
                    ActivityCommon.ScanAllEcus = storageData.ScanAllEcus;
                    ActivityCommon.CollectDebugInfo = storageData.CollectDebugInfo;

                    CheckSettingsVersionChange();
                }

                if (!import)
                {
                    _instanceData.LastLocale = ActivityCommon.SelectedLocale;
                    _instanceData.LastThemeType = ActivityCommon.SelectedTheme;
                }

                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                ActivityCommon.StaticDataInitialized = true;
                if (!import)
                {
                    _instanceData.LastSettingsHash = hash;
                }

                _instanceData.GetSettingsCalled = true;
            }
            return false;
        }

        public bool StoreSettings(string fileName, SettingsMode settingsMode, out string errorMessage)
        {
            errorMessage = null;
            if (_instanceData == null || _activityCommon == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            bool export = settingsMode != SettingsMode.All;
            try
            {
                if (!ActivityCommon.StaticDataInitialized || !_instanceData.GetSettingsCalled)
                {
                    return false;
                }

                lock (ActivityCommon.GlobalSettingLockObject)
                {
                    StorageData storageData = new StorageData(this, true);
                    string hash = storageData.CalcualeHash();

                    if (!export && string.Compare(hash, _instanceData.LastSettingsHash, StringComparison.Ordinal) == 0)
                    {
                        return true;
                    }

                    XmlAttributeOverrides storageClassAttributes = GetStoreXmlAttributeOverrides(settingsMode);
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageData), storageClassAttributes);
                    Java.IO.File tempFile = Java.IO.File.CreateTempFile("Settings", ".xml", Android.App.Application.Context.CacheDir);
                    if (tempFile == null)
                    {
                        return false;
                    }

                    tempFile.DeleteOnExit();
                    string tempFileName = tempFile.AbsolutePath;
                    using (StreamWriter sw = new StreamWriter(tempFileName))
                    {
                        xmlSerializer.Serialize(sw, storageData);
                    }

                    File.Copy(tempFileName, fileName, true);
                    tempFile.Delete();

                    if (!export)
                    {
                        _instanceData.LastSettingsHash = hash;
                    }
                }

                _backupManager?.DataChanged();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = EdiabasNet.GetExceptionText(ex);
            }
            return false;
        }

        private bool RequestOverlayPermissions(EventHandler<EventArgs> handler)
        {
            if (_overlayPermissionRequested || _overlayPermissionGranted)
            {
                return false;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.S)
            {
                return false;
            }

            if (Android.Provider.Settings.CanDrawOverlays(Android.App.Application.Context))
            {
                _overlayPermissionGranted = true;
            }

            if (!_overlayPermissionGranted && !_overlayPermissionRequested)
            {
                _overlayPermissionRequested = true;
                bool yesSelected = false;
                AlertDialog altertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                    {
                        try
                        {
                            Intent intent = new Intent(Android.Provider.Settings.ActionManageOverlayPermission,
                                Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName));
                            StartActivityForResult(intent, (int)ActivityRequest.RequestOverlayPermissions);
                            if (handler != null)
                            {
                                _instanceData.AutoStart = true;
                            }
                            yesSelected = true;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    })
                    .SetNegativeButton(Resource.String.button_no, (s, a) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.overlay_permission_denied)
                    .SetTitle(Resource.String.alert_title_warning)
                    .Show();
                altertDialog.DismissEvent += (o, eventArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (!yesSelected)
                    {
                        handler?.Invoke(o, eventArgs);
                    }
                };
                return true;
            }

            return false;
        }

        private bool RequestNotificationPermissions(EventHandler<EventArgs> handler)
        {
            bool notificationsEnabled = _activityCommon.NotificationsEnabled(ActivityCommon.NotificationChannelCommunication);
            if (_notificationRequested || notificationsEnabled)
            {
                return false;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return false;
            }

            if (!_notificationRequested)
            {
                _notificationRequested = true;
                bool yesSelected = false;
                AlertDialog altertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                    {
                        if (_activityCommon.ShowNotificationSettings((int)ActivityRequest.RequestNotificationSettingsApp,
                                (int)ActivityRequest.RequestNotificationSettingsChannel, ActivityCommon.NotificationChannelCommunication))
                        {
                            if (handler != null)
                            {
                                _instanceData.AutoStart = true;
                            }
                            yesSelected = true;
                        }
                    })
                    .SetNegativeButton(Resource.String.button_no, (s, a) =>
                    {
                    })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.notification_permission_denied)
                    .SetTitle(Resource.String.alert_title_warning)
                    .Show();
                altertDialog.DismissEvent += (o, eventArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (!yesSelected)
                    {
                        handler?.Invoke(o, eventArgs);
                    }
                };
                return true;
            }

            return false;
        }

        private bool RequestStorageManagerPermissions()
        {
            if (_storageManagerPermissionRequested || _storageManagerPermissionGranted)
            {
                return false;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                string[] permissions = _activityCommon.RetrievePermissions();
                if (permissions != null)
                {
                    if (!permissions.Contains("android.permission.MANAGE_EXTERNAL_STORAGE", StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                if (Android.OS.Environment.IsExternalStorageManager)
                {
                    _storageManagerPermissionGranted = true;
                }

                if (!_storageManagerPermissionGranted && !_storageManagerPermissionRequested)
                {
                    _storageManagerPermissionRequested = true;
                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (s, a) =>
                        {
                            ActivityCommon.OpenAppSettingAccessFiles(this, (int)ActivityRequest.RequestAppSettingsAccessFiles);
                        })
                        .SetNegativeButton(Resource.String.button_no, (s, a) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.access_manage_files)
                        .SetTitle(Resource.String.alert_title_warning)
                        .Show();
                    return true;
                }
            }

            return false;
        }

        private void RequestStoragePermissions(bool finish = false)
        {
            if (_activityCommon == null)
            {
                return;
            }

            if (ActivityCommon.IsNotificationsAccessRequired())
            {
                RequestNotificationsPermissions();
                return;
            }

            if (_permissionsExternalStorage.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
            {
                StoragePermissionGranted();
                RequestStorageManagerPermissions();
                return;
            }

            if (!ActivityCommon.IsExtrenalStorageAccessRequired())
            {
                StoragePermissionGranted();
                return;
            }

            if (finish)
            {
                Finish();
                return;
            }

            ActivityCompat.RequestPermissions(this, _permissionsExternalStorage, ActivityCommon.RequestPermissionExternalStorage);
        }

        private void StoragePermissionGranted()
        {
            _storageAccessGranted = true;
            ActivityCommon.SetStoragePath();
            ActivityCommon.RecentConfigListCleanup();
            UpdateDirectories();
            _activityCommon.RequestUsbPermission(null);
            ReadConfigFile();
            if (_startAlertDialog == null && !_instanceData.VersionInfoShown && _currentVersionCode != _instanceData.LastVersionCode)
            {
                _instanceData.VersionInfoShown = true;
                string message = (GetString(Resource.String.version_change_info_message) +
                                 GetString(Resource.String.version_last_changes)).Replace("\n", "<br>");
                _startAlertDialog = new AlertDialog.Builder(this)
                    .SetNeutralButton(Resource.String.button_donate, (sender, args) =>
                    {
                        OpenDonateLink();
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

        private void RequestNotificationsPermissions()
        {
            if (_actvityDestroyed)
            {
                return;
            }

            if (_permissionsPostNotifications.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
            {
                NotificationsPermissionGranted();
                return;
            }

            ActivityCompat.RequestPermissions(this, _permissionsPostNotifications, ActivityCommon.RequestPermissionNotifications);
        }

        private void NotificationsPermissionGranted(bool granted = true)
        {
            _notificationGranted = granted;
            StoragePermissionGranted();
        }

        public bool RequestLocationPermissions()
        {
            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    return false;
                }

                if (_locationPersissionGranted || _locationPersissionRequested)
                {
                    return false;
                }

                string[] requestPermissions = Build.VERSION.SdkInt < BuildVersionCodes.S ? ActivityCommon.PermissionsFineLocation : ActivityCommon.PermissionsCombinedLocation;
                if (requestPermissions.All(permission => ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted))
                {
                    LocationPermissionsGranted();
                    return false;
                }

                _locationPersissionRequested = true;
                ActivityCompat.RequestPermissions(this, requestPermissions, ActivityCommon.RequestPermissionLocation);
                _instanceData.AutoStart = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool LocationPermissionsGranted(EventHandler<EventArgs> handler = null)
        {
            _locationPersissionGranted = true;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                if (_activityCommon.LocationManager != null)
                {
                    try
                    {
                        if (!_activityCommon.LocationManager.IsLocationEnabled)
                        {
                            if (!_instanceData.LocationProviderShown)
                            {
                                _instanceData.LocationProviderShown = true;
                                bool yesSelected = false;
                                AlertDialog altertDialog = new AlertDialog.Builder(this)
                                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                                    {
                                        if (ActivityCommon.OpenLocationSettings(this, (int)ActivityRequest.RequestLocationSettings))
                                        {
                                            if (handler != null)
                                            {
                                                _instanceData.AutoStart = true;
                                            }
                                            yesSelected = true;
                                        }
                                    })
                                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(Resource.String.location_provider_disabled_wifi)
                                    .SetTitle(Resource.String.alert_title_warning)
                                    .Show();
                                altertDialog.DismissEvent += (o, eventArgs) =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    if (!yesSelected)
                                    {
                                        handler?.Invoke(o, eventArgs);
                                    }
                                };
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return false;
        }

        private void UpdateDirectories()
        {
            _instanceData.AppDataPath = string.Empty;
            _instanceData.EcuPath = string.Empty;
            _instanceData.VagPath = string.Empty;
            _instanceData.BmwPath = string.Empty;
            _instanceData.UserEcuFiles = false;
            if (string.IsNullOrEmpty(ActivityCommon.CustomStorageMedia))
            {
                if (string.IsNullOrEmpty(ActivityCommon.ExternalWritePath))
                {
                    if (string.IsNullOrEmpty(ActivityCommon.ExternalPath))
                    {
                        Toast.MakeText(this, GetString(Resource.String.no_ext_storage), ToastLength.Long)?.Show();
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
                _instanceData.AppDataPath = Path.Combine(ActivityCommon.CustomStorageMedia, AppFolderName);
            }

            _instanceData.EcuPath = Path.Combine(_instanceData.AppDataPath, ManufacturerEcuDirName);
            _instanceData.VagPath = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir, ActivityCommon.VagBaseDir);
            _instanceData.BmwPath = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir, ActivityCommon.BmwBaseDir);

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
                    }
                    break;
                }

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
                    if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Ftdi)
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtraType<UsbDevice>(UsbManager.ExtraDevice);
                        if (usbDevice != null && EdFtdiInterface.IsValidUsbDevice(usbDevice))
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
                        StoreSettings();
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
                            _tabLayout.GetTabAt(pageIndex)?.Select();
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

            long responseCount = ActivityCommon.EdiabasThread.GetResponseCount();
            StopEdiabasThread(true);
            UpdateDisplay();

            _translationList = null;
            _translatedList = null;

            UpdateCheck();
            if (_instanceData.CommErrorsCount >= ActivityCommon.MinSendCommErrors && responseCount > 0 &&
                _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
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

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendBackupTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (ActivityCommon.CommActive)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_instanceData.TraceBackupDir))
            {
                return _activityCommon.SendTraceFile(_instanceData.AppDataPath, _instanceData.TraceBackupDir, GetType(), handler);
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
                    ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult?.Adapter;
                    if (resultListAdapter != null)
                    {
                        resultListAdapter.Items.Clear();
                        resultListAdapter.NotifyDataSetChanged();
                    }
                }
            }
        }

        private string GetSelectedPageSgdb()
        {
            JobReader.PageInfo pageInfo = GetSelectedPage();
            if (pageInfo?.JobsInfo == null)
            {
                return null;
            }

            if (pageInfo.ErrorsInfo != null)
            {
                return null;
            }

            string sgdb = pageInfo.JobsInfo.Sgbd;
            if (string.IsNullOrEmpty(sgdb))
            {
                foreach (JobReader.JobInfo jobInfo in pageInfo.JobsInfo.JobList)
                {
                    sgdb = jobInfo.Sgbd;
                    if (!string.IsNullOrEmpty(sgdb))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sgdb))
            {
                return null;
            }

            return sgdb;
        }

        private bool SelectedPageFunctionsAvailable()
        {
            if (ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw)
            {
                if (!ActivityCommon.UseBmwDatabase)
                {
                    return false;
                }
            }
            else
            {
                if (ActivityCommon.OldVagMode)
                {
                    return false;
                }
            }

            JobReader.PageInfo pageInfo = GetSelectedPage();
            if (pageInfo?.JobsInfo == null)
            {
                return false;
            }

            if (pageInfo.ErrorsInfo != null)
            {
                return false;
            }

            return true;
        }

        private void UpdateDisplay(bool forceUpdate = false)
        {
            if (!_activityActive || (_activityCommon == null))
            {   // OnDestroy already executed
                return;
            }

            long startTime = Stopwatch.GetTimestamp();
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
                // ReSharper disable once ReplaceWithSingleAssignment.True
                bool buttonEnabled = true;

                if (ActivityCommon.JobReader.PageList.Count == 0)
                {
                    buttonEnabled = false;
                }

                if (!_activityCommon.IsInterfaceAvailable())
                {
                    buttonEnabled = false;
                }

                if (IsErrorEvalJobRunning())
                {
                    buttonEnabled = false;
                }

                _connectButtonInfo.Enabled = buttonEnabled;
                _connectButtonInfo.Checked = false;
            }
            if (_connectButtonInfo.Button != null)
            {
                _connectButtonInfo.Button.Checked = _connectButtonInfo.Checked;
                _connectButtonInfo.Button.Enabled = _connectButtonInfo.Enabled;
            }

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
                    StoreSettings();    // store warning voltage
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
                LinearLayout listLayout = dynamicFragment.View.FindViewById<LinearLayout>(Resource.Id.listLayout);
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

                if (listLayout != null)
                {
                    try
                    {
                        LinearLayout.LayoutParams layoutParams = (LinearLayout.LayoutParams)listLayout.LayoutParameters;
                        layoutParams.Height = resultGridAdapter != null ? ViewGroup.LayoutParams.WrapContent : ViewGroup.LayoutParams.MatchParent;
                        listLayout.LayoutParameters = layoutParams;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
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
                    bool portrait = true;
                    switch (_currentConfiguration.Orientation)
                    {
                        case Android.Content.Res.Orientation.Landscape:
                            portrait = false;
                            break;
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        if (ActivityCommon.SwapMultiWindowOrientation && IsInMultiWindowMode)
                        {
                            portrait = !portrait;
                        }
                    }

                    int gaugeCount = portrait ? pageInfo.GaugesPortrait : pageInfo.GaugesLandscape;
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
                        _instanceData.CommErrorsCount++;
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
                        List<EdiabasThread.EdiabasErrorReport> errorReportList = null;
                        EdiabasThread.UpdateState updateState;
                        int updateProgress;
                        lock (EdiabasThread.DataLock)
                        {
                            if (ActivityCommon.EdiabasThread.ResultPageInfo == pageInfo)
                            {
                                errorReportList = ActivityCommon.EdiabasThread.EdiabasErrorReportList;
                            }

                            updateState = ActivityCommon.EdiabasThread.UpdateProgressState;
                            updateProgress = ActivityCommon.EdiabasThread.UpdateProgress;
                        }

                        if (errorReportList == null)
                        {
                            string state = string.Empty;
                            switch (updateState)
                            {
                                case EdiabasThread.UpdateState.Init:
                                    state = GetString(Resource.String.error_reading_state_init);
                                    break;

                                case EdiabasThread.UpdateState.Error:
                                    state = GetString(Resource.String.error_reading_state_error);
                                    break;

                                case EdiabasThread.UpdateState.DetectVehicle:
                                    state = string.Format(GetString(Resource.String.error_reading_state_detect), updateProgress);
                                    break;

                                case EdiabasThread.UpdateState.ReadErrors:
                                    state = string.Format(GetString(Resource.String.error_reading_state_read), updateProgress);
                                    break;
                            }

                            StringBuilder sbInfo = new StringBuilder();
                            sbInfo.Append(string.Format(GetString(Resource.String.error_reading_errors), state));
                            bool relevantOnly = ActivityCommon.EcuFunctionsActive && ActivityCommon.ShowOnlyRelevantErrors;
                            if (relevantOnly)
                            {
                                sbInfo.Append("\r\n");
                                sbInfo.Append(GetString(Resource.String.error_reading_relevant_only));
                            }

                            tempResultList.Add(new TableResultItem(sbInfo.ToString(), null));
                        }
                        else
                        {
                            EvaluateErrorMessages(pageInfo, errorReportList, formatErrorResult, errorMessageData =>
                            {
                                RunOnUiThread(() =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }

                                    if (errorMessageData == null)
                                    {
                                        return;
                                    }

                                    bool commActive = ActivityCommon.CommActive && ActivityCommon.EdiabasThread.CommActive;
                                    if (!commActive)
                                    {
                                        _updateHandler?.Post(() => { UpdateDisplay(); });
                                        return;
                                    }

                                    JobReader.PageInfo pageInfoCurrent = GetSelectedPage();
                                    if (pageInfoCurrent != pageInfo)
                                    {   // page changed
                                        return;
                                    }

                                    if (errorMessageData.CommError)
                                    {
                                        _instanceData.CommErrorsCount++;
                                    }

                                    ProcessTranslation(errorMessageData.TranslationList);
                                    ProcessErrorReset(errorMessageData, resultListAdapter);

                                    string lastEcuName = null;
                                    foreach (ErrorMessageEntry errorMessageEntry in errorMessageData.ErrorList)
                                    {
                                        EdiabasThread.EdiabasErrorReport errorReport = errorMessageEntry.ErrorReport;
                                        string message = errorMessageEntry.Message;

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

                                            bool shadow = errorReport is EdiabasThread.EdiabasErrorShadowReport;
                                            newResultItem.CheckEnable = !ActivityCommon.ErrorResetActive && !shadow;
                                            tempResultList.Add(newResultItem);
                                        }

                                        lastEcuName = errorReport.EcuName;
                                    }

                                    if (tempResultList.Count == 0)
                                    {
                                        tempResultList.Add(new TableResultItem(GetString(Resource.String.error_no_error), null));
                                    }

                                    UpdateButtonErrorReset(buttonErrorReset, tempResultList);
                                    UpdateButtonErrorResetAll(buttonErrorResetAll, tempResultList, pageInfo);
                                    UpdateButtonErrorSelect(buttonErrorSelect, tempResultList);
                                    UpdateButtonErrorCopy(buttonErrorCopy, (errorReportList != null) ? tempResultList : null);

                                    UpdateResultListAdapter(resultListAdapter, tempResultList, forceUpdate);
                                });
                            });

                            return;
                        }
                    }
                    else
                    {   // no error page
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
                        UpdateResultGridAdapter(resultGridAdapter, tempResultGrid, forceUpdate);
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

                        UpdateResultListAdapter(resultListAdapter, tempResultList, forceUpdate);
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

                    _imageBackground.Visibility = ViewStates.Visible;
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

            long diffTime = Stopwatch.GetTimestamp() - startTime;
            if (diffTime > _maxDispUpdateTime)
            {
                _maxDispUpdateTime = diffTime;
#if DEBUG
                if (_maxDispUpdateTime / ActivityCommon.TickResolMs > 0)
                {
                    Log.Info(Tag, string.Format("UpdateDisplay: Update time all: {0}ms", _maxDispUpdateTime / ActivityCommon.TickResolMs));
                }
#endif
            }
        }

        private bool EvaluateErrorMessages(JobReader.PageInfo pageInfo, List<EdiabasThread.EdiabasErrorReport> errorReportList, MethodInfo formatErrorResult, ErrorMessageResultDelegate resultHandler)
        {
            if (IsErrorEvalJobRunning())
            {
                return false;
            }

            _errorEvalThread = new Thread(() =>
            {
                List<string> translationList = new List<string>();
                List<ErrorMessageEntry> errorList = new List<ErrorMessageEntry>();
                List<EdiabasThread.EdiabasErrorReportReset> errorResetList = new List<EdiabasThread.EdiabasErrorReportReset>();
                List<ActivityCommon.VagDtcEntry> dtcList = null;
                bool commError = false;
                int errorIndex = 0;
                foreach (EdiabasThread.EdiabasErrorReport errorReport in errorReportList)
                {
                    if (errorReport is EdiabasThread.EdiabasErrorReportReset errorReportReset)
                    {
                        switch (errorReportReset.ResetState)
                        {
                            case EdiabasThread.EdiabasErrorReportReset.ErrorRestState.Ok:
                            case EdiabasThread.EdiabasErrorReportReset.ErrorRestState.Condition:
                                errorResetList.Add(errorReportReset);
                                break;
                        }

                        continue;
                    }

                    if (ActivityCommon.IsCommunicationError(errorReport.ExecptionText))
                    {
                        commError = true;
                    }

                    string message = GenerateErrorMessage(pageInfo, errorReport, errorIndex, formatErrorResult, ref translationList, ref dtcList);
                    errorList.Add(new ErrorMessageEntry(errorReport, message));
                    errorIndex++;
                }

                if (resultHandler != null)
                {
                    ErrorMessageData errorMessageData = new ErrorMessageData(errorList, errorResetList, translationList, commError);
                    resultHandler.Invoke(errorMessageData);
                }
            });

            _errorEvalThread.Start();

            return true;
        }

        private void ProcessErrorReset(ErrorMessageData errorMessageData, ResultListAdapter resultListAdapter)
        {
            if (errorMessageData == null)
            {
                return;
            }

            foreach (EdiabasThread.EdiabasErrorReportReset errorReportReset in errorMessageData.ErrorResetList)
            {
                switch (errorReportReset.ResetState)
                {
                    case EdiabasThread.EdiabasErrorReportReset.ErrorRestState.Ok:
                    {
                        bool changed = false;
                        foreach (TableResultItem resultItem in resultListAdapter.Items)
                        {
                            if (string.IsNullOrEmpty(errorReportReset.EcuName) ||
                                (resultItem.Tag is string ecuName && string.CompareOrdinal(ecuName, errorReportReset.EcuName) == 0))
                            {
                                if (resultItem.Selected)
                                {
                                    errorReportReset.Reset();
                                    resultItem.Selected = false;
                                    changed = true;
                                }
                            }
                        }

                        if (changed)
                        {
                            resultListAdapter.NotifyDataSetChanged();
                        }

                        break;
                    }

                    case EdiabasThread.EdiabasErrorReportReset.ErrorRestState.Condition:
                    {
                        if (_errorRestAlertDialog != null)
                        {
                            break;
                        }

                        errorReportReset.Reset();
                        _errorRestAlertDialog = new AlertDialog.Builder(this)
                            .SetNeutralButton(Resource.String.button_ok, (sender, args) =>
                            {
                            })
                            .SetMessage(Resource.String.error_reset_condition)
                            .SetTitle(Resource.String.alert_title_warning)
                            .Show();
                        _errorRestAlertDialog.DismissEvent += (sender, args) =>
                        {
                            if (_activityCommon == null)
                            {
                                return;
                            }
                            _errorRestAlertDialog = null;
                        };
                        break;
                    }
                }
            }
        }

        private void ProcessTranslation(List<string> translationList)
        {
            if (translationList?.Count > 0)
            {
                if (!_translateActive)
                {
                    // translation text present
                    bool translate = false;
                    if (_translationList == null || _translationList.Count != translationList.Count)
                    {
                        translate = true;
                    }
                    else
                    {
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        for (int i = 0; i < translationList.Count; i++)
                        {
                            if (string.Compare(translationList[i], _translationList[i], StringComparison.Ordinal) != 0)
                            {
                                translate = true;
                                break;
                            }
                        }
                    }

                    if (translate)
                    {
                        _translationList = translationList;
                        _translateActive = true;
                        if (!_activityCommon.TranslateStrings(translationList, transList =>
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

        private void UpdateResultListAdapter(ResultListAdapter resultListAdapter, List<TableResultItem> tempResultList, bool forceUpdate)
        {
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

            _imageBackground.Visibility = ViewStates.Gone;
        }

        private void UpdateResultGridAdapter(ResultGridAdapter resultGridAdapter, List<GridResultItem> tempResultGrid, bool forceUpdate)
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

            _imageBackground.Visibility = ViewStates.Gone;
        }

        private string GenerateErrorMessage(JobReader.PageInfo pageInfo, EdiabasThread.EdiabasErrorReport errorReport, int errorIndex, MethodInfo formatErrorResult, ref List<string> translationList, ref List<ActivityCommon.VagDtcEntry> dtcList)
        {
            StringBuilder srMessage = new StringBuilder();
            string language = ActivityCommon.GetCurrentLanguage();
            bool shadow = errorReport is EdiabasThread.EdiabasErrorShadowReport;
            string ecuTitle = GetPageString(pageInfo, errorReport.EcuName);
            EcuFunctionStructs.EcuVariant ecuVariant = errorReport.EcuVariant;
            if (ecuVariant != null)
            {
                string title = ecuVariant.Title?.GetTitle(language);
                if (!string.IsNullOrEmpty(title))
                {
                    ecuTitle += " (" + title + ")";
                }
            }

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
                            errorCode = (Int64)resultData.OpData;
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
                                        ecuResponse1 = (byte[])resultData.OpData;
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
                        Int64 errorType = (Int64)resultData.OpData;
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
                            if (XmlToolActivity.Is1281EcuName(objectName))
                            {
                                kwp1281 = true;
                            }
                            else if (XmlToolActivity.IsUdsEcuName(objectName))
                            {
                                uds = true;
                            }
                        }
                    }
                    if (errorReport.ErrorDict.TryGetValue("SAE", out resultData))
                    {
                        if (resultData.OpData is Int64)
                        {
                            if ((Int64)resultData.OpData != 0)
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

                        dtcEntry = new ActivityCommon.VagDtcEntry((uint)errorCode, dtcDetail, UdsFileReader.DataReader.ErrorType.Iso9141);
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

                        dtcEntry = new ActivityCommon.VagDtcEntry((uint)errorCode, dtcDetail, UdsFileReader.DataReader.ErrorType.Uds);
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
                        srMessage.Append(GetString(Resource.String.error_error_code));
                        srMessage.Append(": ");
                        srMessage.Append(string.Format("0x{0:X04} 0x{1:X02} {2}", dtcEntry.DtcCode, dtcEntry.DtcDetail, dtcEntry.ErrorType.ToString()));
                    }
                    if (textList == null)
                    {
                        textList = _activityCommon.ConvertVagDtcCode(_instanceData.EcuPath, errorCode, errorTypeList, kwp1281, saeMode);
                        srMessage.Append("\r\n");
                        srMessage.Append(GetString(Resource.String.error_error_code));
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

                    bool showUnknown = !(ActivityCommon.EcuFunctionsActive && ActivityCommon.ShowOnlyRelevantErrors);
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
                        if (errorReport.IsValid)
                        {
                            if (errorCode != 0x0000)
                            {
                                envCondLabelList = ActivityCommon.EcuFunctionReader.GetEnvCondLabelList(errorCode, errorReport.ReadIs, ecuVariant);
                                List<string> faultResultList = EdiabasThread.ConvertFaultCodeError(errorCode, errorReport.ReadIs, errorReport, ecuVariant);

                                if (faultResultList != null && faultResultList.Count == 2)
                                {
                                    text1 = faultResultList[0];
                                    text2 = faultResultList[1];
                                }
                            }
                        }
                        else
                        {
                            if (!showUnknown)
                            {
                                errorCode = 0x0000;
                            }
                        }
                    }

                    if (showUnknown && errorCode != 0x0000 && string.IsNullOrEmpty(text1))
                    {
                        text1 = FormatResultString(errorReport.ErrorDict, "F_ORT_TEXT", "{0}");
                        text2 = FormatResultString(errorReport.ErrorDict, "F_VORHANDEN_TEXT", "{0}");
                        if (ActivityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation)
                        {
                            int index = translationList.Count;
                            translationList.Add(text1);
                            translationList.Add(text2);
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
                            if (errorReport.ReadIs)
                            {
                                srMessage.Append(GetString(Resource.String.error_info_code));
                            }
                            else
                            {
                                srMessage.Append(GetString(Resource.String.error_error_code));
                                if (shadow)
                                {
                                    srMessage.Append(" ");
                                    srMessage.Append(GetString(Resource.String.error_shadow));
                                }
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

            return message;
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
            buttonErrorSelect.Tag = Java.Lang.Boolean.ValueOf(selectAll);
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

            string result = string.Empty;
            if (stringInfoSel != null)
            {
                if (!stringInfoSel.StringDict.TryGetValue(name, out result))
                {
                    result = string.Empty;
                }
            }

            if (pageInfo.CompatIdsUsed)
            {
                result += " *";
            }
            return result;
        }

        private static ActivityCommon GetActivityCommon(Context context)
        {
            ActivityCommon activityCommon = null;
            if (context is ActivityMain mainActivity)
            {
                activityCommon = mainActivity.ActivityCommonMain;
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
            if (IsCommActive())
            {
                UpdateJobReaderSettings();
                _updateHandler?.Post(CreateActionBarTabs);
                return;
            }

            bool failed = false;
            string lastFileName = ActivityCommon.JobReader.XmlFileName ?? string.Empty;
            ActivityCommon.JobReader.Clear();
            if (_instanceData.LastAppState != LastAppState.Compile && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
            {
                if (!ActivityCommon.JobReader.ReadXml(_instanceData.ConfigFileName, out string errorMessage))
                {
                    ActivityCommon.RecentConfigListRemove(_instanceData.ConfigFileName);
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
                    _tabsCreated = false;
                }
            }

            string newFileName = ActivityCommon.JobReader.XmlFileName ?? string.Empty;
            if (string.Compare(lastFileName, newFileName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                _tabsCreated = false;
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

            StoreSettings();
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
                _updateHandler?.Post(CreateActionBarTabs);
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
                if (_instanceData.VerifyEcuFiles || _instanceData.VerifyEcuMd5)
                {
                    bool checkMd5 = _instanceData.VerifyEcuMd5;
                    _instanceData.VerifyEcuFiles = false;
                    _instanceData.VerifyEcuMd5 = false;
                    if (ValidEcuPackage(ecuBaseDir))
                    {
                        int lastPercent = -1;
                        if (!ActivityCommon.VerifyContent(Path.Combine(ecuBaseDir, ContentFileName), checkMd5, percent =>
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
                    _updateHandler?.Post(CreateActionBarTabs);
                    _compileProgress.Dismiss();
                    _compileProgress.Dispose();
                    _compileProgress = null;
                    UpdateLockState();
                    if (cpuUsage >= CpuLoadCritical)
                    {
                        _activityCommon.ShowAlert(string.Format(GetString(Resource.String.compile_cpu_usage_high), cpuUsage), Resource.String.alert_title_warning);
                    }
                    else
                    {
                        if (ActivityCommon.JobReader.CompatIdsUsed)
                        {
                            _activityCommon.ShowAlert(GetString(Resource.String.compile_compat_id_warn), Resource.String.alert_title_warning);
                        }
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
            if (string.IsNullOrEmpty(_assetFileName))
            {
                ShowObbMissingRestart();
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
                        xmlInfo.Add(new XAttribute("Name", Path.GetFileName(_assetFileName) ?? string.Empty));
                        if (_assetFileSize > 0)
                        {
                            xmlInfo.Add(new XAttribute("Size", XmlConvert.ToString(_assetFileSize)));
                        }
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
                            obbName = Path.GetFileName(_assetFileName) ?? string.Empty;
                            installer = PackageManager?.GetInstallerPackageName(PackageName ?? string.Empty);
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
                            { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", (long) Build.VERSION.SdkInt)), "android_ver" },
                            { new StringContent(Build.Fingerprint), "fingerprint" },
                            { new StringContent(obbName), "obb_name" },
                            { new StringContent(installer ?? string.Empty), "installer" },
                            { new StringContent(_activityCommon.SelectedInterface.ToDescriptionString() ?? string.Empty), "interface_type" },
                            { new StringContent(ActivityCommon.LastAdapterSerial ?? string.Empty), "adapter_serial" }
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
                            string key = GetObbKey(responseXml, out errorMessage, out string adapterBlacklist);
                            if (key != null)
                            {
                                ActivityCommon.UpdateSerialInfo(responseXml);
                                ActivityCommon.AdapterBlacklist = adapterBlacklist ?? string.Empty;
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

        private string GetObbKey(string xmlResult, out string errorMessage, out string adapterBlacklist)
        {
            errorMessage = null;
            adapterBlacklist = null;
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(_assetFileName))
                {
                    return null;
                }
                string baseName = Path.GetFileName(_assetFileName);
                if (string.IsNullOrEmpty(baseName))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return null;
                }

                foreach (XElement blacklistNode in xmlDoc.Root.Elements("blacklists"))
                {
                    XAttribute adaptersAttr = blacklistNode.Attribute("adapters");
                    if (adaptersAttr != null && !string.IsNullOrEmpty(adaptersAttr.Value))
                    {
                        adapterBlacklist = adaptersAttr.Value;
                    }
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
            ExtractZipFile(_assetManager, _assetFileName, downloadInfo.TargetDir, downloadInfo.InfoXml, key, false,
                new List<string> { Path.Combine(_instanceData.AppDataPath, "EcuVag") });
        }

        private void ExtractZipFile(AssetManager assetManager, string fileName, string targetDirectory, XElement infoXml, string key, bool removeFile = false, List<string> removeDirs = null)
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
                    ActivityCommon.ExtractZipFile(assetManager, fileName, targetDirectory, key, ignoreFolders,
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
                        if (!aborted && !_extractZipCanceled)
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
            string ecuPath = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir);
            try
            {
                ActivityCommon.FileSystemBlockInfo blockInfo = ActivityCommon.GetFileSystemBlockInfo(_instanceData.AppDataPath);
                long ecuDirSize = ActivityCommon.GetDirectorySize(ecuPath);
                double freeSpace = blockInfo.AvailableSizeBytes + ecuDirSize;
                long requiredSize = EcuExtractSize;
                if (freeSpace < requiredSize)
                {
                    string message = string.Format(new FileSizeFormatProvider(), GetString(Resource.String.ecu_download_free_space), requiredSize, freeSpace);
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
            if (!_activityActive || !_storageAccessGranted || _downloadEcuAlertDialog != null)
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

            if (string.IsNullOrEmpty(_assetFileName))
            {
                ShowObbMissingRestart();
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

            if (VehicleInfoBmw.ResourceFailure == VehicleInfoBmw.FailureSource.File)
            {
                _instanceData.VerifyEcuMd5 = true;
                _updateHandler?.Post(CompileCode);
            }

            return true;
        }

        private void ShowObbMissingRestart()
        {
            if (_assetManager != null)
            {
                return;
            }

            AlertDialog alertDialog = new AlertDialog.Builder(this)
                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                .SetCancelable(true)
                .SetMessage(Resource.String.obb_missing_restart)
                .SetTitle(Resource.String.alert_title_error)
                .Show();

            alertDialog.DismissEvent += (sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                StoreSettings();
                _activityCommon.RestartAppSoft();
            };
        }

        private bool DisplayUpdateInfo(EventHandler<EventArgs> handler)
        {
            if (!_activityActive || _downloadEcuAlertDialog != null)
            {
                return false;
            }

            if (IsCommActive())
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
                    handler?.Invoke(this, EventArgs.Empty);
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
                if (string.IsNullOrEmpty(_assetFileName) ||
                    string.Compare(Path.GetFileNameWithoutExtension(nameAttr.Value), Path.GetFileNameWithoutExtension(_assetFileName), StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                XAttribute sizeAttr = xmlInfo.Root?.Attribute("Size");
                if (sizeAttr != null)
                {
                    try
                    {
                        Int64 fileSize = XmlConvert.ToInt64(sizeAttr.Value);
                        if (fileSize != _assetFileSize)
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
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
                string message = string.Format(CultureInfo.InvariantCulture, GetString(Resource.String.vag_mode_info), ActivityCommon.VagEndDate).Replace("\n", "<br>");
                AlertDialog alertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                    {
                        SelectManufacturer();
                    })
                    .SetNegativeButton(Resource.String.button_abort, (sender, args) => { })
                    .SetCancelable(true)
                    .SetMessage(ActivityCommon.FromHtml(message))
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

                ClearConfiguration();   // settings are store here
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
                StoreSettings();
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
            StoreSettings();
            _updateHandler?.Post(CreateActionBarTabs);
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

        private void OpenDonateLink()
        {
            try
            {
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(@"https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VUFSVNBRQQBPJ")));
            }
            catch (Exception)
            {
                // ignored
            }
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
            serverIntent.PutExtra(GlobalSettingsActivity.ExtraAppDataDir, _instanceData.AppDataPath);
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
            serverIntent.PutExtra(CanAdapterActivity.ExtraAppDataDir, _instanceData.AppDataPath);
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
                StoreSettings();
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

        private void StartXmlTool(XmlToolActivity.EcuFunctionCallType ecuFuncCall = XmlToolActivity.EcuFunctionCallType.None)
        {
            try
            {
                if (!CheckForEcuFiles())
                {
                    return;
                }

                string pageFileName = null;
                if (ecuFuncCall != XmlToolActivity.EcuFunctionCallType.None)
                {
                    JobReader.PageInfo pageInfo = GetSelectedPage();
                    if (pageInfo == null)
                    {
                        return;
                    }

                    pageFileName = pageInfo.XmlFileName;
                    if (string.IsNullOrEmpty(pageFileName))
                    {
                        return;
                    }

                    if (ecuFuncCall == XmlToolActivity.EcuFunctionCallType.BmwActuator && !SelectedPageFunctionsAvailable())
                    {
                        return;
                    }
                }

                if (_activityCommon.InitReaderThread(_instanceData.BmwPath, _instanceData.VagPath, result =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    if (result)
                    {
                        StartXmlTool(ecuFuncCall);
                    }
                    else
                    {
                        _instanceData.VerifyEcuMd5 = true;
                        _updateHandler?.Post(CompileCode);
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
                if (!string.IsNullOrEmpty(pageFileName))
                {
                    serverIntent.PutExtra(XmlToolActivity.ExtraPageFileName, pageFileName);
                    serverIntent.PutExtra(XmlToolActivity.ExtraEcuFuncCall, (int)ecuFuncCall);
                }
                serverIntent.PutExtra(XmlToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(XmlToolActivity.ExtraDeviceName, _instanceData.DeviceName);
                serverIntent.PutExtra(XmlToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
                serverIntent.PutExtra(XmlToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(XmlToolActivity.ExtraFileName, _instanceData.ConfigFileName);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestXmlTool);
                ActivityCommon.ActivityStartedFromMain = true;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void StartEdiabasTool(bool currentPage = false)
        {
            try
            {
                if (!CheckForEcuFiles())
                {
                    return;
                }

                string sgdbFile = null;
                if (currentPage)
                {
                    string sgdb = GetSelectedPageSgdb();
                    if (string.IsNullOrEmpty(sgdb))
                    {
                        return;
                    }

                    sgdbFile = Path.Combine(_instanceData.EcuPath, sgdb);
                }

                Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
                EdiabasToolActivity.IntentTranslateActivty = _activityCommon;
                serverIntent.PutExtra(EdiabasToolActivity.ExtraInitDir, _instanceData.EcuPath);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraAppDataDir, _instanceData.AppDataPath);
                if (!string.IsNullOrEmpty(sgdbFile))
                {
                    serverIntent.PutExtra(EdiabasToolActivity.ExtraSgbdFile, sgdbFile);
                }
                serverIntent.PutExtra(EdiabasToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceName, _instanceData.DeviceName);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEdiabasTool);
                ActivityCommon.ActivityStartedFromMain = true;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void StartBmwCoding()
        {
            try
            {
                if (!CheckForEcuFiles())
                {
                    return;
                }

                Intent serverIntent = new Intent(this, typeof(BmwCodingActivity));
                serverIntent.PutExtra(BmwCodingActivity.ExtraAppDataDir, _instanceData.AppDataPath);
                serverIntent.PutExtra(BmwCodingActivity.ExtraEcuDir, _instanceData.EcuPath);
                serverIntent.PutExtra(BmwCodingActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(BmwCodingActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
                serverIntent.PutExtra(BmwCodingActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestBmwCoding);
            }
            catch (Exception)
            {
                // ignored
            }
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
                Android.Net.Uri fileUri = FileProvider.GetUriForFile(Android.App.Application.Context, PackageName + ".fileprovider", new Java.IO.File(fileName));
                string mimeType = Android.Webkit.MimeTypeMap.Singleton?.GetMimeTypeFromExtension("xml");
                viewIntent.SetDataAndType(fileUri, mimeType);
                viewIntent.SetFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission | ActivityFlags.NewTask);

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
                    Android.App.PendingIntentFlags intentFlags = Android.App.PendingIntentFlags.UpdateCurrent;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                    {
                        intentFlags |= Android.App.PendingIntentFlags.Mutable;
                    }
                    Android.App.PendingIntent pendingIntent =
                        Android.App.PendingIntent.GetBroadcast(Android.App.Application.Context, 0, receiver, intentFlags);
                    chooseIntent = Intent.CreateChooser(viewIntent, GetString(Resource.String.choose_xml_editor), pendingIntent?.IntentSender);
                }
                else
                {
                    chooseIntent = Intent.CreateChooser(viewIntent, GetString(Resource.String.choose_xml_editor));
                }
                StartActivityForResult(chooseIntent, (int)ActivityRequest.RequestEditXml);
                ActivityCommon.ActivityStartedFromMain = true;

                return true;
            }
#pragma warning disable CS0168
            catch (Exception ex)
#pragma warning restore CS0168
            {
#if DEBUG
                Log.Info(Tag, string.Format("StartEditXml Exception: {0}", EdiabasNet.GetExceptionText(ex)));
#endif
                _activityCommon.ShowAlert(GetString(Resource.String.xml_access_denied), Resource.String.alert_title_error);
                return false;
            }
        }

        public class TabsFragmentStateAdapter : FragmentStateAdapter
        {
            private class TabPageInfo
            {
                public TabPageInfo(JobReader.PageInfo pageInfo, long itemId, int resourceId, string title)
                {
                    ItemId = itemId;
                    PageInfo = pageInfo;
                    ResourceId = resourceId;
                    Title = title;
                }

                public JobReader.PageInfo PageInfo { get; }
                public long ItemId { get; }
                public int ResourceId { get; }
                public string Title { get; }
            }

            private static long IdOffset;
            private readonly FragmentManager _fragmentManager;
            private readonly List<TabPageInfo> _pageList;

            public TabsFragmentStateAdapter(FragmentManager fm, Lifecycle lifecycle) : base(fm, lifecycle)
            {
                _fragmentManager = fm;
                _pageList = new List<TabPageInfo>();
            }

            public override int ItemCount => _pageList.Count;

            public override Fragment CreateFragment(int position)
            {
                if (position < 0 || position >= _pageList.Count)
                {
                    return null;
                }

                TabPageInfo tabPageInfo = _pageList[position];
                Fragment fragmentPage = TabContentFragment.NewInstance(tabPageInfo.ResourceId, position);
                tabPageInfo.PageInfo.InfoObject = fragmentPage;
                return fragmentPage;
            }

            public override long GetItemId(int position)
            {
                if (position < 0 || position >= _pageList.Count)
                {
                    return RecyclerView.NoId;
                }

                TabPageInfo tabPageInfo = _pageList[position];
                return tabPageInfo.ItemId;
            }

            public override bool ContainsItem(long itemId)
            {
                foreach (TabPageInfo tabPageInfo in _pageList)
                {
                    if (tabPageInfo.ItemId == itemId)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void ClearPages()
            {
                int size = _pageList.Count;
                _pageList.Clear();
                NotifyItemRangeRemoved(0, size);
                NotifyItemRangeChanged(0, size);
                if (_fragmentManager != null && _fragmentManager.BackStackEntryCount > 0)
                {
                    _fragmentManager.PopBackStack(null, AndroidX.Fragment.App.FragmentManager.PopBackStackInclusive);
                }
            }

            public void AddPage(JobReader.PageInfo pageInfo, int resourceId, string title)
            {
                int position = _pageList.Count;
                _pageList.Add(new TabPageInfo(pageInfo, IdOffset, resourceId, title));
                IdOffset++;
                if (IdOffset < 0)
                {
                    IdOffset = 0;
                }

                NotifyItemInserted(position);
            }
        }

        public class TabContentFragment : Fragment
        {
            private int _resourceId;
            private int _pageInfoIndex;
            private View _view;

            public TabContentFragment()
            {
                _resourceId = -1;
                _pageInfoIndex = -1;
                _view = null;
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
                if (Arguments != null)
                {
                    _resourceId = Arguments.GetInt("ResourceId", -1);
                    _pageInfoIndex = Arguments.GetInt("PageInfoIndex", -1);
                }
                else
                {
                    _resourceId = -1;
                    _pageInfoIndex = -1;
                }
                _view = null;
#if DEBUG
                Log.Info(Tag, string.Format("TabContentFragment OnCreate: Resource={0}, PageIdx={1}", _resourceId, _pageInfoIndex));
#endif
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
#if DEBUG
                Log.Info(Tag, string.Format("TabContentFragment OnCreateView: {0}", _pageInfoIndex));
#endif
                View view = inflater.Inflate(_resourceId, null);
                if (Activity is ActivityMain activityMain && activityMain == ActivityCommon.ActivityMainCurrent)
                {
                    if (_pageInfoIndex >= 0 && _pageInfoIndex < ActivityCommon.JobReader.PageList.Count)
                    {
                        JobReader.PageInfo pageInfo = ActivityCommon.JobReader.PageList[_pageInfoIndex];
                        if (pageInfo.ClassObject != null && view != null)
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
                    activityMain._updateHandler?.Post(() =>
                    {
                        activityMain.UpdateDisplay();
                    });
                }

                _view = view;
                return view;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();
#if DEBUG
                Log.Info(Tag, string.Format("TabContentFragment OnDestroyView: {0}", _pageInfoIndex));
#endif
                if (Activity is ActivityMain activityMain && activityMain == ActivityCommon.ActivityMainCurrent &&
                    _pageInfoIndex >= 0 && _pageInfoIndex < ActivityCommon.JobReader.PageList.Count)
                {
                    JobReader.PageInfo pageInfo = ActivityCommon.JobReader.PageList[_pageInfoIndex];
                    if (pageInfo.ClassObject != null)
                    {
                        try
                        {
                            if (_view != null)
                            {
                                LinearLayout pageLayout = _view.FindViewById<LinearLayout>(Resource.Id.listLayout);
                                if (pageLayout != null)
                                {
                                    pageLayout.RemoveAllViews();
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                _view = null;
            }
        }

        public class TabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
        {
            private ActivityMain _activityMain;

            public TabConfigurationStrategy(ActivityMain activityMain)
            {
                _activityMain = activityMain;
            }

            public void OnConfigureTab(TabLayout.Tab tab, int index)
            {
                if (index >= 0 && index < (ActivityCommon.JobReader.PageList.Count))
                {
                    JobReader.PageInfo pageInfo = ActivityCommon.JobReader.PageList[index];
                    tab.SetText(GetPageString(pageInfo, pageInfo.Name));
                }
            }
        }

        public class ConnectActionProvider : AndroidX.Core.View.ActionProvider
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
                ((ActivityMain)Context).ButtonConnectClick(((ActivityMain)Context)._connectButtonInfo.Button, EventArgs.Empty);
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
                    ComponentName clickedComponent = intent?.GetParcelableExtraType<ComponentName>(Intent.ExtraChosenComponent);
                    if (clickedComponent != null)
                    {
                        string packageName = clickedComponent.PackageName;
                        string className = clickedComponent.ClassName;
                        if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(className))
                        {
                            Intent broadcastIntent = new Intent(ActivityCommon.ActionPackageName);
                            broadcastIntent.SetPackage(Android.App.Application.Context.PackageName);    // Replacement for LocalBroadcastManager
                            broadcastIntent.PutExtra(ActivityCommon.BroadcastXmlEditorPackageName, packageName);
                            broadcastIntent.PutExtra(ActivityCommon.BroadcastXmlEditorClassName, className);
                            Android.App.Application.Context.SendBroadcast(broadcastIntent);
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
