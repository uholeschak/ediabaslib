using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
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
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Base62;
using BmwDeepObd.Dialogs;
using BmwDeepObd.FilePicker;
using BmwFileReader;
using EdiabasLib;
using Google.Android.Material.Tabs;
using Java.Interop;
using Skydoves.BalloonLib;

// ReSharper disable MergeCastWithTypeCheck
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

[assembly: Android.App.UsesFeature("android.hardware.bluetooth", Required = false)]
[assembly: Android.App.UsesFeature("android.hardware.bluetooth_le", Required = false)]

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/app_name",
        MainLauncher = true,
        Exported = true,
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
            RequestOpenExternalFile,
            RequestServiceBusy,
            RequestYandexKey,
            RequestGlobalSettings,
            RequestGlobalSettingsCopy,
            RequestEditConfig,
            RequestEditXml,
        }

        private enum BalloonAlligment
        {
            Center,
            Top,
            Bottom,
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

        public class InstanceData : ActivityCommon.InstanceDataCommon
        {
            public enum CommRequest
            {
                None,
                Connect,
                Disconnect
            }

            public InstanceData()
            {
                CommOptionRequest = CommRequest.None;
            }

            public bool MtcBtDisconnectWarnShown { get; set; }
            public bool AutoConnectExecuted { get; set; }
            public CommRequest CommOptionRequest { get; set; }
        }

#if DEBUG
        private static readonly string Tag = typeof(ActivityMain).FullName;
#endif
        private const string EcuDownloadUrl = @"https://www.holeschak.de/BmwDeepObd/Obb.php";
        private const long EcuExtractSize = 3000000000;         // extracted ecu files size
        private const string EcuPackInfoXmlName = "EcuPackInfo.xml";
        private const string SampleInfoFileName = "SampleInfo.xml";
        private const string CaCertInfoFileName = "CaCertsInfo.xml";
        private const string ContentFileName = "Content.xml";
        private const string TranslationFileNameMain = "TranslationMain.xml";
        private const int MenuGroupRecentId = 1;
        private const int CpuLoadCritical = 70;
        private const int AutoHideTimeout = 3000;
        private readonly string[] _permissionsExternalStorage =
        {
            Android.Manifest.Permission.WriteExternalStorage,
        };

        [SupportedOSPlatform("android33.0")]
        private readonly string[] _permissionsPostNotifications =
        {
            Android.Manifest.Permission.PostNotifications
        };

        public const string ExtraShowTitle = "show_title";
        public const string ExtraNoAutoconnect = "no_autoconnect";
        public const string ExtraCommOption = "comm_option";
        public const string CommOptionConnect = "connect";
        public const string CommOptionDisconnect = "disconnect";
        public const string ExtraStoreOption = "store_option";
        public const string StoreOptionSettings = "store_settings";
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        public static bool StoreXmlEditor = Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1;
        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private bool _lastCompileCrash;
        private bool _backPressed;
        private long _lastBackPressedTime;
        private bool _activityActive;
        private bool _onResumeExecuted;
        private bool _storageAccessGranted;
        private bool _notificationRequested;
        private bool _notificationGranted;
        private bool _locationPermissionRequested;
        private bool _locationPermissionGranted;
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
        private Java.Lang.Runnable _createActionBarRunnable;
        private Java.Lang.Runnable _handleConnectOptionRunnable;
        private Java.Lang.Runnable _compileCodeRunnable;
        private Java.Lang.Runnable _updateDisplayRunnable;
        private Java.Lang.Runnable _updateDisplayForceRunnable;
        private SelectTabPageRunnable _selectTabPageRunnable;
        private CheckAdapter _checkAdapter;
        private TabLayout _tabLayout;
        private ViewPager2 _viewPager;
        private TabsFragmentStateAdapter _fragmentStateAdapter;
        protected View _contentView;
        private readonly ConnectButtonInfo _connectButtonInfo = new ConnectButtonInfo();
        private ImageView _imageBackground;
        private HttpClient _httpClient;
        private Thread _errorEvalThread;
        private CustomProgressDialog _downloadProgress;
        private CustomProgressDialog _compileProgress;
        private bool _extractZipCanceled;
        private string _assetEcuFileName;
        private long _assetEcuFileSize = -1;
        private AssetManager _assetManager;
        private AlertDialog _startAlertDialog;
        private AlertDialog _configSelectAlertDialog;
        private AlertDialog _downloadEcuAlertDialog;
        private AlertDialog _errorRestAlertDialog;
        private bool _translateActive;
        private List<string> _translationList;
        private List<string> _translatedList;

        public ActivityCommon ActivityCommonMain => _activityCommon;

        public InstanceData InstanceDataMain => _instanceData;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        protected override void OnCreate(Bundle savedInstanceState)
        {
            AddActivityToStack(this);
            ActivityCommon.GetThemeSettings();
            SetTheme();
            base.OnCreate(savedInstanceState);

            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.OMr1)
                {
                    SetShowWhenLocked(true);
                    SetTurnScreenOn(true);
                    Android.App.KeyguardManager keyguardManager = GetSystemService(Context.KeyguardService) as Android.App.KeyguardManager;
                    keyguardManager?.RequestDismissKeyguard(this, null);
                }
                else
                {
                    Window?.AddFlags(WindowManagerFlags.DismissKeyguard | WindowManagerFlags.ShowWhenLocked | WindowManagerFlags.TurnScreenOn);
                }
            }
            catch (Exception)
            {
                // ignored
            }

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
            _contentView = FindViewById<View>(Android.Resource.Id.Content);

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

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
                _activityCommon.SelectedElmWifiIp = _instanceData.SelectedElmWifiIp;
                _activityCommon.SelectedDeepObdWifiIp = _instanceData.SelectedDeepObdWifiIp;
                _activityCommon.MtcBtDisconnectWarnShown = _instanceData.MtcBtDisconnectWarnShown;
            }

            GetSettings();
            _lastCompileCrash = false;
            if (!_activityRecreated && _instanceData != null)
            {
                if (_instanceData.LastAppState == ActivityCommon.LastAppState.Compile)
                {
                    _lastCompileCrash = true;
                    _instanceData.LastAppState = ActivityCommon.LastAppState.Init;
                    _instanceData.ConfigFileName = string.Empty;
                    // store settings in OnResume to keep the correct seelcted lanuage
                }
            }

            _activityCommon.SetPreferredNetworkInterface();

            _updateHandler = new Handler(Looper.MainLooper);
            _createActionBarRunnable = new Java.Lang.Runnable(CreateActionBarTabs);
            _handleConnectOptionRunnable = new Java.Lang.Runnable(HandleConnectOption);
            _compileCodeRunnable = new Java.Lang.Runnable(CompileCode);
            _updateDisplayRunnable = new Java.Lang.Runnable(() =>
            {
                UpdateDisplay();
            });

            _updateDisplayForceRunnable = new Java.Lang.Runnable(() =>
            {
                UpdateDisplay(true);
            });

            _selectTabPageRunnable = new SelectTabPageRunnable(this);

            _checkAdapter = new CheckAdapter(_activityCommon);
            _imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);

            StoreLastAppState(ActivityCommon.LastAppState.Init);

            if (_httpClient == null)
            {
                _httpClient = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = ActivityCommon.DefaultSslProtocols,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    Proxy = ActivityCommon.GetProxySettings()
                })
                {
                    Timeout = TimeSpan.FromSeconds(ActivityCommon.HttpClientTimeout)
                };
            }

            HandleIntent(Intent);

            if (ActivityCommon.CommActive)
            {
                lock (EdiabasThread.DataLock)
                {
                    ConnectEdiabasEvents();
                }
            }
            else
            {
                if (!ForegroundService.IsCommThreadRunning())
                {
                    ActivityCommon.StopForegroundService(this);
                }

                if (!_activityRecreated)
                {
                    ActivityCommon.BtInitiallyEnabled = _activityCommon.IsBluetoothEnabled();
#if false
                    if (ActivityCommon.AutoConnectHandling == ActivityCommon.AutoConnectType.StartBoot)
                    {
                        try
                        {
                            Intent broadcastIntent = new Intent(ActionBroadcastReceiver.ActionStartService);
                            broadcastIntent.SetClass(this, typeof(ActionBroadcastReceiver));
                            SendBroadcast(broadcastIntent);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
#endif
                }
            }
        }

        private void PostSelectTabPage(int pageIndex)
        {
            _selectTabPageRunnable.SelectTabPageIndex = pageIndex;
            ActivityCommon.PostRunnable(_updateHandler, _selectTabPageRunnable);
        }

        private void PostCreateActionBarTabs()
        {
            if (_activityCommon == null)
            {
                return;
            }

            if (_updateHandler == null)
            {
                return;
            }

            ActivityCommon.PostRunnable(_updateHandler, _createActionBarRunnable);
        }

        private void CreateActionBarTabs()
        {
            if (!_activityActive)
            {
                _createTabsPending = true;
                return;
            }
            _createTabsPending = false;

            ConnectActionArgs.AutoConnectMode autoConnect = ConnectActionArgs.AutoConnectMode.None;
            // get last active tab
            JobReader.PageInfo currentPage = null;
            if (IsCommActive())
            {
                _ignoreTabsChange = true;
                currentPage = ActivityCommon.EdiabasThread?.JobPageInfo;
            }
            else
            {
                if (!_instanceData.AutoConnectExecuted)
                {
                    switch (ActivityCommon.AutoConnectHandling)
                    {
                        case ActivityCommon.AutoConnectType.Connect:
                        case ActivityCommon.AutoConnectType.ConnectClose:
                            autoConnect = ConnectActionArgs.AutoConnectMode.Auto;
                            break;
                    }
                }

                if (_tabsCreated)
                {
                    currentPage = GetSelectedPage();
                }
            }

            InstanceData.CommRequest commRequest = _instanceData.CommOptionRequest;
            if (commRequest != InstanceData.CommRequest.None)
            {
                _instanceData.CommOptionRequest = InstanceData.CommRequest.None;

                switch (commRequest)
                {
                    case InstanceData.CommRequest.Connect:
                        autoConnect = ConnectActionArgs.AutoConnectMode.Connect;
                        break;

                    case InstanceData.CommRequest.Disconnect:
                        autoConnect = ConnectActionArgs.AutoConnectMode.Disconnect;
                        break;
                }
            }

            JobReader jobReader = ActivityCommon.JobReader;
            int pageIndex = 0;
            if (autoConnect == ConnectActionArgs.AutoConnectMode.Auto)
            {
                pageIndex = _instanceData.LastSelectedJobIndex;
            }
            else
            {
                if (currentPage != null)
                {
                    int i = 0;
                    foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
                    {
                        if (pageInfo == currentPage)
                        {
                            pageIndex = i;
                            break;
                        }
                        i++;
                    }
                }
            }

            _fragmentStateAdapter.ClearPages();
            _tabLayout.Visibility = ViewStates.Gone;

            foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
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
            _tabLayout.Visibility = (jobReader.PageList.Count > 0) ? ViewStates.Visible : ViewStates.Gone;
            _fragmentStateAdapter.NotifyDataSetChanged();
            if (_tabLayout.TabCount > 0)
            {
                if (pageIndex >= _tabLayout.TabCount)
                {
                    pageIndex = 0;
                }

                PostSelectTabPage(pageIndex);
            }

            _ignoreTabsChange = false;
            _tabsCreated = true;
            UpdateDisplay();
            StoreLastAppState(ActivityCommon.LastAppState.TabsCreated);

            bool connectStarted = false;
            try
            {
                if (autoConnect != ConnectActionArgs.AutoConnectMode.None)
                {
                    if (jobReader.PageList.Count > 0 && _activityCommon.IsInterfaceAvailable(_instanceData.SimulationPath))
                    {
                        ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto, autoConnect));
                        connectStarted = true;

                        if (autoConnect == ConnectActionArgs.AutoConnectMode.Auto &&
                            UseCommService() && ActivityCommon.CommActive &&
                            ActivityCommon.AutoConnectHandling == ActivityCommon.AutoConnectType.ConnectClose)
                        {
                            _instanceData.AutoConnectExecuted = true;
                            Finish();
                        }
                    }
                }
                else
                {
                    StoreActiveJobIndex();
                }
            }
            finally
            {
                if (!connectStarted)
                {
                    SendCarSessionConnectBroadcast(false);
                }
            }

            _instanceData.AutoConnectExecuted = true;
        }

        private void HandleConnectOption()
        {
            if (!_activityActive)
            {
                return;
            }

            if (!_tabsCreated)
            {
                return;
            }

            InstanceData.CommRequest commRequest = _instanceData.CommOptionRequest;
            if (commRequest == InstanceData.CommRequest.None)
            {
                return;
            }

            _instanceData.CommOptionRequest = InstanceData.CommRequest.None;
            UpdateDisplay();

            bool connectStarted = false;
            try
            {
                if (!_connectButtonInfo.Enabled)
                {
                    return;
                }

                JobReader jobReader = ActivityCommon.JobReader;
                switch (commRequest)
                {
                    case InstanceData.CommRequest.Connect:
                        if (_connectButtonInfo.Checked)
                        {
                            break;
                        }

                        if (jobReader.PageList.Count > 0 && _activityCommon.IsInterfaceAvailable(_instanceData.SimulationPath))
                        {
                            ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto, ConnectActionArgs.AutoConnectMode.Connect));
                            connectStarted = true;
                        }
                        break;

                    case InstanceData.CommRequest.Disconnect:
                        if (!_connectButtonInfo.Checked)
                        {
                            break;
                        }

                        ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto, ConnectActionArgs.AutoConnectMode.Disconnect));
                        connectStarted = true;
                        break;
                }
            }
            finally
            {
                if (!connectStarted)
                {
                    SendCarSessionConnectBroadcast(false);
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreTranslation();
            _instanceData.SelectedInterface = _activityCommon.SelectedInterface;
            _instanceData.SelectedEnetIp = _activityCommon.SelectedEnetIp;
            _instanceData.SelectedElmWifiIp = _activityCommon.SelectedElmWifiIp;
            _instanceData.SelectedDeepObdWifiIp = _activityCommon.SelectedDeepObdWifiIp;
            _instanceData.MtcBtDisconnectWarnShown = _activityCommon.MtcBtDisconnectWarnShown;
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
#if DEBUG
            Log.Info(Tag, string.Format("OnStart: {0}", this));
#endif
            base.OnStart();

            ClearActivityStack();
            AddActivityToStack(this);

            _onResumeExecuted = false;
            _storageAccessGranted = false;
            _notificationRequested = false;
            _notificationGranted = false;
            _locationPermissionRequested = false;
            _locationPermissionGranted = false;
            _overlayPermissionRequested = false;
            _overlayPermissionGranted = false;
            _storageManagerPermissionRequested = false;
            _storageManagerPermissionGranted = false;
            _activityCommon?.StartMtcService();
        }

        protected override void OnStop()
        {
#if DEBUG
            Log.Info(Tag, string.Format("OnStop: {0}", this));
#endif
            base.OnStop();

            _activityCommon?.StopMtcService();
            StoreLastAppState(ActivityCommon.LastAppState.Stopped);
            SendCarSessionConnectBroadcast(false, true);
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
#if DEBUG
            Log.Info(Tag, string.Format("OnResume: {0}", this));
#endif
            base.OnResume();

            if (CheckForegroundService((int)ActivityRequest.RequestServiceBusy))
            {
                return;
            }

            bool firstStart = !_onResumeExecuted;
            if (!_onResumeExecuted)
            {
                _onResumeExecuted = true;
                RequestStoragePermissions();
                if (_lastCompileCrash)
                {
                    StoreSettings();
                }
            }

            if (_storageAccessGranted)
            {
                _activityCommon?.RequestUsbPermission(null);
            }

            _activityActive = true;

            UpdateLockState();
            if (_compileCodePending)
            {
                PostCompileCode();
            }

            if (_createTabsPending)
            {
                // this also handles connect option
                PostCreateActionBarTabs();
            }
            else
            {
                HandleConnectOption();
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
#if DEBUG
            Log.Info(Tag, string.Format("OnPause: {0}", this));
#endif
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

        public override void Finish()
        {
            base.Finish();

            _instanceData.AutoConnectExecuted = false;
            if (!ActivityCommon.CommActive && _activityCommon != null)
            {
                _activityCommon.BluetoothDisableAtExit();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!UseCommService())
            {
                StopEdiabasThread(true);
            }

            lock (EdiabasThread.DataLock)
            {
                DisconnectEdiabasEvents();
            }

            if (IsErrorEvalJobRunning())
            {
                _errorEvalThread?.Join();
            }

            if (_httpClient != null)
            {
                try
                {
                    _httpClient.CancelPendingRequests();
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

            _checkAdapter?.Dispose();
            _checkAdapter = null;

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
                ShowBallonMessage(GetString(Resource.String.back_button_twice_for_exit));
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            PostUpdateDisplay(true);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            HandleIntent(intent);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
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
                                ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
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
                            ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
                            break;
                        }
                    }

                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestNotificationSettingsApp:
                    if (_activityCommon.NotificationsEnabled() && _instanceData.AutoStart)
                    {
                        ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
                        break;
                    }

                    _instanceData.AutoStart = false;
                    break;

                case ActivityRequest.RequestNotificationSettingsChannel:
                    UpdateOptionsMenu();
                    if (_activityCommon.NotificationsEnabled(ActivityCommon.NotificationChannelCommunication) && _instanceData.AutoStart)
                    {
                        ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
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
                            ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
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

                        if (!string.IsNullOrEmpty(_instanceData.ConfigFileName) && _activityCommon.SelectedInterface != ActivityCommon.InterfaceType.Simulation)
                        {
                            if (!_instanceData.ConfigMatchVehicleShown)
                            {
                                _instanceData.ConfigMatchVehicleShown = true;
                                string balloonMessage = GetString(Resource.String.config_match_vehicle);
                                ShowBallonMessage(balloonMessage, 20000);
                            }
                        }
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
                            _instanceData.AdapterCheckOk = false;
                            _instanceData.DeviceName = data.Extras.GetString(XmlToolActivity.ExtraDeviceName);
                            _instanceData.DeviceAddress = data.Extras.GetString(XmlToolActivity.ExtraDeviceAddress);
                            _activityCommon.SelectedEnetIp = data.Extras.GetString(XmlToolActivity.ExtraEnetIp);
                            _activityCommon.SelectedElmWifiIp = data.GetStringExtra(XmlToolActivity.ExtraElmWifiIp);
                            _activityCommon.SelectedDeepObdWifiIp = data.GetStringExtra(XmlToolActivity.ExtraDeepObdWifiIp);
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

                case ActivityRequest.RequestOpenExternalFile:
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    break;

                case ActivityRequest.RequestServiceBusy:
                    if (resultCode == Android.App.Result.Canceled)
                    {
                        Finish();
                        return;
                    }

                    if (CheckForegroundService((int)ActivityRequest.RequestServiceBusy))
                    {
                        return;
                    }

                    if (ActivityCommon.CommActive)
                    {
                        lock (EdiabasThread.DataLock)
                        {
                            DisconnectEdiabasEvents();
                            ConnectEdiabasEvents();
                        }
                    }

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
                case ActivityRequest.RequestGlobalSettingsCopy:
                    if ((ActivityRequest)requestCode == ActivityRequest.RequestGlobalSettings)
                    {
                        if (data?.Extras != null && resultCode == Android.App.Result.Ok)
                        {
                            string exportFileName = data.Extras.GetString(GlobalSettingsActivity.ExtraExportFile);
                            string importFileName = data.Extras.GetString(GlobalSettingsActivity.ExtraImportFile);
                            ActivityCommon.SettingsMode settingsMode = (ActivityCommon.SettingsMode)data.Extras.GetInt(GlobalSettingsActivity.ExtraSettingsMode,
                                (int)ActivityCommon.SettingsMode.Private);
                            if (!string.IsNullOrEmpty(exportFileName))
                            {
                                if (!_activityCommon.StoreSettingsToFile(_instanceData, exportFileName, settingsMode, out string errorMessage))
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
                                if (!_activityCommon.GetSettingsFromFile(_instanceData, importFileName, ActivityCommon.SettingsMode.Private))
                                {
                                    string message = GetString(Resource.String.settings_import_no_file) + "\r\n" + exportFileName;
                                    _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                                }
                            }
                        }

                        _activityCommon.SetPreferredNetworkInterface();
                        if ((_instanceData.LastThemeType ?? ActivityCommon.ThemeDefault) != (ActivityCommon.SelectedTheme ?? ActivityCommon.ThemeDefault))
                        {
                            StoreSettings();
                            // update translations
                            _activityCommon.RegisterNotificationChannels();
                            SetTheme();
                            Recreate();
                            break;
                        }
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
            inflater.Inflate(Resource.Menu.main_option_menu, menu);
            _optionsMenu = menu;
            return base.OnCreateOptionsMenu(menu);
        }

        public override void PrepareOptionsMenu(IMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            bool commActive = IsCommActive();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable(_instanceData.SimulationPath, true);
            bool pageSgbd = !string.IsNullOrEmpty(GetSelectedPageSgbd());
            bool selectedPageFuncAvail = SelectedPageFunctionsAvailable();
            JobReader jobReader = ActivityCommon.JobReader;
            bool manualEdit = jobReader != null && jobReader.ManualEdit;
            JobReader.PageInfo currentPage = GetSelectedPage();
            bool enableSelectedPageEdit = !string.IsNullOrEmpty(currentPage?.XmlFileName);

            bool hasDisplayOrder = false;
            if (currentPage != null)
            {
                if (currentPage.ErrorsInfo == null)
                {
                    int orderCount = currentPage.DisplayList.Count(x => x.DisplayOrder != null);
                    hasDisplayOrder = orderCount > 1 && orderCount == currentPage.DisplayList.Count;
                }
            }

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
                bool menuVisible = _activityCommon.GetAdapterIpName(out string longName, out string _);

                enetIpMenu.SetTitle(longName);
                enetIpMenu.SetEnabled(interfaceAvailable && !commActive);
                enetIpMenu.SetVisible(menuVisible);
            }

            IMenuItem cfgSubmenu = menu.FindItem(Resource.Id.menu_cfg_submenu);
            if (cfgSubmenu != null)
            {
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
                cfgPageFuncMenu.SetVisible(pageSgbd && !string.IsNullOrEmpty(_instanceData.ConfigFileName));
            }

            IMenuItem cfgPageEdiabasMenu = menu.FindItem(Resource.Id.menu_cfg_page_ediabas);
            if (cfgPageEdiabasMenu != null)
            {
                cfgPageEdiabasMenu.SetEnabled(interfaceAvailable && !commActive);
                cfgPageEdiabasMenu.SetVisible(pageSgbd && !string.IsNullOrEmpty(_instanceData.ConfigFileName));
            }

            bool bmwVisible = ActivityCommon.SelectedManufacturer == ActivityCommon.ManufacturerType.Bmw && selectedPageFuncAvail && !string.IsNullOrEmpty(_instanceData.ConfigFileName);
            IMenuItem cfgPageBmwActuatorMenu = menu.FindItem(Resource.Id.menu_cfg_page_bmw_actuator);
            if (cfgPageBmwActuatorMenu != null)
            {
                cfgPageBmwActuatorMenu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageBmwActuatorMenu.SetVisible(bmwVisible);
            }

            IMenuItem cfgPageBmwServiceMenu = menu.FindItem(Resource.Id.menu_cfg_page_bmw_service);
            if (cfgPageBmwServiceMenu != null)
            {
                cfgPageBmwServiceMenu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageBmwServiceMenu.SetVisible(bmwVisible);
            }

            bool vagVisible = ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw && selectedPageFuncAvail && !string.IsNullOrEmpty(_instanceData.ConfigFileName);
            IMenuItem cfgPageVagCodingMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_coding);
            if (cfgPageVagCodingMenu != null)
            {
                cfgPageVagCodingMenu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageVagCodingMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagCoding2Menu = menu.FindItem(Resource.Id.menu_cfg_page_vag_coding2);
            if (cfgPageVagCoding2Menu != null)
            {
                cfgPageVagCoding2Menu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageVagCoding2Menu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagAdaptionMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_adaption);
            if (cfgPageVagAdaptionMenu != null)
            {
                cfgPageVagAdaptionMenu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageVagAdaptionMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagLoginMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_login);
            if (cfgPageVagLoginMenu != null)
            {
                cfgPageVagLoginMenu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageVagLoginMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgPageVagSecAccessMenu = menu.FindItem(Resource.Id.menu_cfg_page_vag_sec_access);
            if (cfgPageVagSecAccessMenu != null)
            {
                cfgPageVagSecAccessMenu.SetEnabled(!manualEdit && interfaceAvailable && !commActive);
                cfgPageVagSecAccessMenu.SetVisible(vagVisible);
            }

            IMenuItem cfgEditMenu = menu.FindItem(Resource.Id.menu_cfg_edit);
            cfgEditMenu?.SetEnabled(!commActive && !string.IsNullOrEmpty(_instanceData.ConfigFileName));

            IMenuItem cfgPagesEditMenu = menu.FindItem(Resource.Id.menu_cfg_pages_edit);
            cfgPagesEditMenu?.SetEnabled(!commActive && !string.IsNullOrEmpty(_instanceData.ConfigFileName) && !string.IsNullOrEmpty(jobReader.XmlFileNamePages));

            IMenuItem cfgPageMenu = menu.FindItem(Resource.Id.menu_cfg_page_menu);
            cfgPageMenu?.SetEnabled(!commActive && enableSelectedPageEdit);

            IMenuItem cfgPageEditMenu = menu.FindItem(Resource.Id.menu_cfg_page_edit);
            cfgPageEditMenu?.SetEnabled(!commActive && enableSelectedPageEdit);

            IMenuItem cfgPageEditFontsizeMenu = menu.FindItem(Resource.Id.menu_cfg_page_edit_fontsize);
            cfgPageEditFontsizeMenu?.SetEnabled(
                !commActive && enableSelectedPageEdit &&
                currentPage.DisplayFontSize != null && currentPage.DisplayMode == JobReader.PageInfo.DisplayModeType.List);

            IMenuItem cfgPageEditGaugesLandscapeMenu = menu.FindItem(Resource.Id.menu_cfg_page_edit_gauges_landscape);
            cfgPageEditGaugesLandscapeMenu?.SetEnabled(
                !commActive && enableSelectedPageEdit &&
                currentPage.GaugesLandscape != null && currentPage.DisplayMode == JobReader.PageInfo.DisplayModeType.Grid);

            IMenuItem cfgPageEditGaugesPortraitMenu = menu.FindItem(Resource.Id.menu_cfg_page_edit_gauges_portrait);
            cfgPageEditGaugesPortraitMenu?.SetEnabled(
                !commActive && enableSelectedPageEdit &&
                currentPage.GaugesPortrait != null && currentPage.DisplayMode == JobReader.PageInfo.DisplayModeType.Grid);

            IMenuItem cfgPageEditDisplayOrderMenu = menu.FindItem(Resource.Id.menu_cfg_page_edit_display_order);
            cfgPageEditDisplayOrderMenu?.SetEnabled(!commActive && enableSelectedPageEdit && hasDisplayOrder);

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
                logSubMenu.SetEnabled(interfaceAvailable && !commActive);
            }

            IMenuItem traceSubmenu = menu.FindItem(Resource.Id.menu_trace_submenu);
            traceSubmenu?.SetEnabled(!commActive);

            bool tracePresent = ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir);
            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);

            IMenuItem openTraceMenu = menu.FindItem(Resource.Id.menu_open_trace);
            openTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);

            bool backupTracePresent = ActivityCommon.IsTraceFilePresent(_instanceData.TraceBackupDir);
            IMenuItem sendLastTraceMenu = menu.FindItem(Resource.Id.menu_send_last_trace);
            if (sendLastTraceMenu != null)
            {
                sendLastTraceMenu.SetEnabled(interfaceAvailable && !commActive && backupTracePresent);
                sendLastTraceMenu.SetVisible(backupTracePresent);
            }

            IMenuItem openLastTraceMenu = menu.FindItem(Resource.Id.menu_open_last_trace);
            if (openLastTraceMenu != null)
            {
                openLastTraceMenu.SetEnabled(interfaceAvailable && !commActive && backupTracePresent);
                openLastTraceMenu.SetVisible(backupTracePresent);
            }

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
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (_activityCommon == null)
            {
                return base.OnOptionsItemSelected(item);
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

            JobReader jobReader = ActivityCommon.JobReader;
            JobReader.PageInfo currentPage = GetSelectedPage();
            switch (item.ItemId)
            {
                case Resource.Id.menu_manufacturer:
                    SelectManufacturer();
                    return true;

                case Resource.Id.menu_scan:
                    _instanceData.AutoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice, _instanceData.AppDataPath);
                    return true;

                case Resource.Id.menu_adapter_config:
                    AdapterConfig();
                    return true;

                case Resource.Id.menu_enet_ip:
                    AdapterIpConfig();
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

                case Resource.Id.menu_cfg_page_bmw_service:
                    StartXmlTool(XmlToolActivity.EcuFunctionCallType.BmwService);
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
                    StartEditXml(jobReader.XmlFileNamePages);
                    return true;

                case Resource.Id.menu_cfg_page_edit:
                    StartEditXml(currentPage?.XmlFileName);
                    return true;

                case Resource.Id.menu_cfg_page_edit_fontsize:
                    EditFontSize(currentPage);
                    return true;

                case Resource.Id.menu_cfg_page_edit_gauges_landscape:
                    EditGaugesCount(currentPage, true);
                    return true;

                case Resource.Id.menu_cfg_page_edit_gauges_portrait:
                    EditGaugesCount(currentPage, false);
                    return true;

                case Resource.Id.menu_cfg_page_edit_display_order:
                    EditDisplayOrder(currentPage);
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
                    string vehicleSeries = jobReader.VehicleSeries;

                    if (!string.IsNullOrEmpty(vehicleSeries) && vehicleSeries.Length > 0)
                    {
                        if (ActivityCommon.IsBmwCodingSeries(vehicleSeries))
                        {
                            allowBmwCoding = true;
                        }
                    }
                    else
                    {
                        string sgbdFunctional = jobReader.SgbdFunctional;
                        JobReader.PageInfo errorPage = jobReader.ErrorPage;

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

                case Resource.Id.menu_open_trace:
                    if (ActivityCommon.CommActive)
                    {
                        return true;
                    }
                    OpenTraceFile();
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

                case Resource.Id.menu_open_last_trace:
                    if (ActivityCommon.CommActive)
                    {
                        return true;
                    }
                    OpenTraceFile(true);
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
                    StoreTranslation();
                    _translationList = null;
                    _translatedList = null;
                    UpdateOptionsMenu();
                    UpdateDisplay();
                    return true;

                case Resource.Id.menu_global_settings:
                    StartGlobalSettings();
                    return true;

                case Resource.Id.menu_submenu_help:
                    _activityCommon.ShowWifiConnectedWarning(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _activityCommon.OpenWebUrl("https://github.com/uholeschak/ediabaslib/blob/master/docs/Deep_OBD_for_BMW_and_VAG.md");
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

        public override bool IsFinishAllowed()
        {
            if (_activityCommon == null)
            {
                return true;
            }

            if (IsErrorEvalJobRunning())
            {
                return false;
            }

            if (_checkAdapter.IsJobRunning())
            {
                return false;
            }

            if (_activityCommon.IsUdsReaderJobRunning())
            {
                return false;
            }

            if (_activityCommon.IsEcuFuncReaderJobRunning())
            {
                return false;
            }

            if (_activityCommon.TranslateActive)
            {
                return false;
            }

            if (_downloadProgress != null)
            {
                return false;
            }

            if (_compileProgress != null)
            {
                return false;
            }

            return true;
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

                    if (alertDialog != null)
                    {
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
                    }
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
                                    ConnectAction(sender, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
                                }
                            }))
                        {
                            break;
                        }
                    }

                    if (_instanceData.AutoStart)
                    {
                        ConnectAction(_connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Auto));
                        break;
                    }

                    _instanceData.AutoStart = false;
                    break;
            }
        }

        protected void ConnectAction(object sender, ConnectActionArgs connectActionArgs, int recursionLevel = 0)
        {
            ConnectActionArgs.AutoConnectMode autoConnect = ConnectActionArgs.AutoConnectMode.None;
            if (connectActionArgs != null)
            {
                autoConnect = connectActionArgs.AutoConnect;
            }

            bool connectStarted = false;
            try
            {
                _instanceData.AutoStart = false;
                if (!CheckForEcuFiles())
                {
                    UpdateOptionsMenu();
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
                        UpdateOptionsMenu();
                        UpdateDisplay();
                        return;
                    }
                }

                if (recursionLevel == 0)
                {
                    SendCarSessionConnectBroadcast(true);
                }

                if (!ActivityCommon.CommActive && autoConnect == ConnectActionArgs.AutoConnectMode.None)
                {
                    if (RequestWifiPermissions(true))
                    {
                        return;
                    }

                    if (_activityCommon.ShowConnectWarning(action =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        bool connectStartedLocal = false;
                        switch (action)
                        {
                            case ActivityCommon.SsidWarnAction.Continue:
                                ConnectAction(sender, connectActionArgs, recursionLevel++);
                                connectStartedLocal = true;
                                break;

                            case ActivityCommon.SsidWarnAction.EditIp:
                                AdapterIpConfig();
                                break;
                        }

                        if (!connectStartedLocal)
                        {
                            SendCarSessionConnectBroadcast(false);
                        }
                    }))
                    {
                        UpdateOptionsMenu();
                        UpdateDisplay();
                        connectStarted = true;
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
                        ConnectAction(sender, connectActionArgs, recursionLevel++);
                    }))
                    {
                        connectStarted = true;
                        return;
                    }

                    if (RequestNotificationPermissions((o, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        ConnectAction(sender, connectActionArgs, recursionLevel++);
                    }))
                    {
                        connectStarted = true;
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
                                ConnectAction(sender, connectActionArgs, recursionLevel++);
                            }
                            else
                            {
                                SendCarSessionConnectBroadcast(false);
                                VerifyEcuMd5();
                            }
                        }))
                    {
                        connectStarted = true;
                        return;
                    }
                }

                bool commActive = ActivityCommon.CommActive;
                switch (autoConnect)
                {
                    case ConnectActionArgs.AutoConnectMode.Connect:
                        if (commActive)
                        {
                            return;
                        }
                        break;

                    case ConnectActionArgs.AutoConnectMode.Disconnect:
                        if (!commActive)
                        {
                            return;
                        }
                        break;
                }

                if (commActive)
                {
                    StopEdiabasThread(false);
                    StoreActiveJobIndex(true);
                }
                else
                {
                    ActivityCommon.JobReader.UpdateCompatIdUsage();
                    _fragmentStateAdapter?.UpdatePageTitles();

                    if (ActivityCommon.JobReader.CompatIdsUsed)
                    {
                        if (!ActivityCommon.JobReader.CompatIdWarningShown)
                        {
                            ShowBallonMessage(GetString(Resource.String.compile_compat_id_warn));
                            ActivityCommon.JobReader.CompatIdWarningShown = true;
                        }
                    }

                    if (!_instanceData.AdapterCheckOk && _activityCommon.AdapterCheckRequired)
                    {
                        if (_checkAdapter.StartCheckAdapter(_instanceData.AppDataPath,
                                _activityCommon.SelectedInterface, _instanceData.DeviceAddress,
                                checkError =>
                                {
                                    RunOnUiThread(() =>
                                    {
                                        if (!checkError)
                                        {
                                            _instanceData.AdapterCheckOk = true;
                                            if (StartEdiabasThread())
                                            {
                                                UpdateSelectedPage();
                                            }
                                        }

                                        SendCarSessionConnectBroadcast(false);
                                        UpdateOptionsMenu();
                                        UpdateDisplay();
                                    });
                                }))
                        {
                            connectStarted = true;
                            return;
                        }
                    }

                    if (StartEdiabasThread())
                    {
                        UpdateSelectedPage();
                    }
                }

                UpdateOptionsMenu();
                UpdateDisplay();
            }
            finally
            {
                if (!connectStarted)
                {
                    SendCarSessionConnectBroadcast(false);
                }
            }
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
            if (button != null)
            {
                ActivityCommon.EdiabasThread.CommActive = button.Checked;
            }
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
#if DEBUG
                Log.Info(Tag, string.Format("OnErrorResetClick: Ecus={0}", string.Join(", ", errorResetList)));
#endif
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

            string sgbdFunctional = ActivityCommon.EdiabasThread?.JobPageInfo?.ErrorsInfo?.SgbdFunctional;
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

        private void HandleIntent(Intent intent)
        {
            if (intent != null)
            {
                if ((intent.Flags & ActivityFlags.LaunchedFromHistory) != 0)
                {   // old flags reused
                    return;
                }

                bool showTitleRequest = intent.GetBooleanExtra(ExtraShowTitle, false);
                if (showTitleRequest)
                {
                    ShowActionBar();
                }

                bool noAutoConnect = intent.GetBooleanExtra(ExtraNoAutoconnect, false);
                if (noAutoConnect)
                {
                    _instanceData.AutoConnectExecuted = false;
                }

                string storeOption = intent.GetStringExtra(ExtraStoreOption);
                if (!string.IsNullOrEmpty(storeOption))
                {
                    if (string.Compare(storeOption, StoreOptionSettings, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (_instanceData.GetSettingsCalled)
                        {
                            StoreSettings();
                        }
                    }
                }

                string commOption = intent.GetStringExtra(ExtraCommOption);
                if (!string.IsNullOrEmpty(commOption))
                {
                    InstanceData.CommRequest commRequest = InstanceData.CommRequest.None;
                    if (string.Compare(commOption, CommOptionConnect, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        commRequest = InstanceData.CommRequest.Connect;
                    }
                    else if (string.Compare(commOption, CommOptionDisconnect, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        commRequest = InstanceData.CommRequest.Disconnect;
                    }

                    _instanceData.CommOptionRequest = commRequest;
                    if (commRequest != InstanceData.CommRequest.None)
                    {
                        SendCarSessionConnectBroadcast(true);
                        if (_activityActive)
                        {
                            ActivityCommon.PostRunnable(_updateHandler, _handleConnectOptionRunnable);
                        }
                    }
                }
            }
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
            if (!_activityActive)
            {
                return false;
            }

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
            if (ActivityCommon.AutoConnectHandling != ActivityCommon.AutoConnectType.StartBoot)
            {
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

            }
            return useService;
        }

        private void SendCarSessionConnectBroadcast(bool started, bool activityStopped = false)
        {
            Intent broadcastIntent = new Intent(CarService.CarSession.CarSessionBroadcastAction);
            broadcastIntent.PutExtra(CarService.CarSession.ExtraConnectStarted, started);
            broadcastIntent.PutExtra(CarService.CarSession.ExtraActivityStopped, activityStopped);
            InternalBroadcastManager.InternalBroadcastManager.GetInstance(this).SendBroadcast(broadcastIntent);
        }

        private bool StartEdiabasThread()
        {
            try
            {
                JobReader.PageInfo pageInfo = GetSelectedPage();
                if (pageInfo == null)
                {
                    return false;
                }

                _instanceData.AutoStart = false;
                _instanceData.CommErrorsCount = 0;
                _translationList = null;
                _translatedList = null;
                _maxDispUpdateTime = 0;

                ActivityCommon.JobReader.UpdateCompatIdUsage();
                if (!_activityCommon.StartEdiabasThread(_instanceData, pageInfo, EdiabasEventHandler))
                {
                    return false;
                }

                if (UseCommService())
                {
                    ActivityCommon.StartForegroundService(this);
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
            _activityCommon.StopEdiabasThread(wait, EdiabasEventHandler);

            UpdateLockState();
            UpdateOptionsMenu();

            _autoHideStarted = false;
            SupportActionBar.Show();
        }

        private void ConnectEdiabasEvents()
        {
            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
            if (ediabasThread != null)
            {
                ediabasThread.DataUpdated += DataUpdated;
                ediabasThread.PageChanged += PageChanged;
                ediabasThread.ThreadTerminated += ThreadTerminated;
            }
        }

        private void DisconnectEdiabasEvents()
        {
            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
            if (ediabasThread != null)
            {
                ediabasThread.DataUpdated -= DataUpdated;
                ediabasThread.PageChanged -= PageChanged;
                ediabasThread.ThreadTerminated -= ThreadTerminated;
            }
        }

        private void EdiabasEventHandler(bool connect)
        {
            // the GloblalLockObject is already locked
            if (connect)
            {
                ConnectEdiabasEvents();
            }
            else
            {
                DisconnectEdiabasEvents();
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

        private void GetSettings()
        {
            string assetEcuFileName = ActivityCommon.GetAssetEcuFilename();
            if (!string.IsNullOrEmpty(assetEcuFileName))
            {
                try
                {
                    AssetManager assetManager = ActivityCommon.GetPackageContext()?.Assets;
                    if (assetManager != null)
                    {
                        AssetFileDescriptor assetFile = assetManager.OpenFd(assetEcuFileName);
                        _assetManager = assetManager;
                        _assetEcuFileName = assetEcuFileName;
                        _assetEcuFileSize = assetFile.Length;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (_assetManager == null)
            {
                _assetEcuFileName = string.Empty;
                _assetEcuFileSize = -1;
            }

            ActivityCommon.AssetEcuFileName = _assetEcuFileName;
            ActivityCommon.AssetEcuFileSize = _assetEcuFileSize;

            if (_activityCommon.GetSettings(_instanceData, ActivityCommon.SettingsMode.All, !_activityRecreated))
            {
                return;
            }
        }

        private void StoreSettings()
        {
            if (!_activityCommon.StoreSettings(_instanceData, ActivityCommon.SettingsMode.All, out string errorMessage))
            {
                string message = GetString(Resource.String.store_settings_failed);
                if (errorMessage != null)
                {
                    message += "\r\n" + errorMessage;
                }

                Toast.MakeText(this, message, ToastLength.Long)?.Show();
            }
        }

        private void StoreLastAppState(ActivityCommon.LastAppState lastAppState)
        {
            _instanceData.LastAppState = lastAppState;
            StoreSettings();
        }

        private void StoreActiveJobIndex(bool disable = false)
        {
            JobReader jobReader = ActivityCommon.JobReader;
            int selectedJobIndex = -1;
            switch (ActivityCommon.AutoConnectHandling)
            {
                case ActivityCommon.AutoConnectType.Connect:
                case ActivityCommon.AutoConnectType.ConnectClose:
                    if (jobReader.PageList.Count > 0)
                    {
                        selectedJobIndex = _tabLayout.SelectedTabPosition;
                    }
                    break;

                case ActivityCommon.AutoConnectType.StartBoot:
                    if (!disable && ActivityCommon.CommActive)
                    {
                        JobReader.PageInfo pageInfo = ActivityCommon.EdiabasThread?.JobPageInfo;
                        if (pageInfo != null)
                        {
                            selectedJobIndex = jobReader.PageList.IndexOf(pageInfo);
                        }
                    }
                    break;
            }

            if (_instanceData.LastSelectedJobIndex != selectedJobIndex)
            {
                _instanceData.LastSelectedJobIndex = selectedJobIndex;
                StoreSettings();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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
                if (altertDialog != null)
                {
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
                }
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
                if (altertDialog != null)
                {
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
                }
                return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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

            if (_startAlertDialog == null && !_instanceData.VersionInfoShown && _activityCommon.VersionCode != _instanceData.LastVersionCode)
            {
                _instanceData.VersionInfoShown = true;

                StringBuilder sbMessage = new StringBuilder();
                sbMessage.AppendLine(GetString(Resource.String.version_change_info_message));
                sbMessage.AppendLine();
                sbMessage.AppendLine(GetString(Resource.String.version_last_changes_header));
#if !ANDROID_AUTO
                sbMessage.AppendLine(GetString(Resource.String.car_service_test_info));
#endif
                sbMessage.AppendLine(GetString(Resource.String.version_last_changes));

                string message = sbMessage.ToString().Replace("\n", "<br>");
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
                if (_startAlertDialog != null)
                {
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
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        private void RequestNotificationsPermissions()
        {
            if (_activityDestroyed)
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

        public bool RequestWifiPermissions(bool autoStart = false)
        {
            try
            {
                if (_activityCommon == null)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    return false;
                }

                if (_locationPermissionGranted || _locationPermissionRequested)
                {
                    return false;
                }

                if (_activityCommon.SelectedInterface != ActivityCommon.InterfaceType.Enet)
                {
                    return false;
                }

                if (!_activityCommon.RequestWifiPermissions(granted =>
                    {
                        if (granted)
                        {
                            LocationPermissionsGranted();
                        }
                        else
                        {
                            _locationPermissionRequested = true;
                            _instanceData.AutoStart = autoStart;
                        }
                    }))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        private bool LocationPermissionsGranted(EventHandler<EventArgs> handler = null)
        {
            _locationPermissionGranted = true;

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
                                if (altertDialog != null)
                                {
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

                                }
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
            if (_activityCommon == null)
            {
                return;
            }

            if (!_activityCommon.UpdateDirectories(_instanceData))
            {
                Toast.MakeText(this, GetString(Resource.String.no_ext_storage), ToastLength.Long)?.Show();
                Finish();
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
                            break;
                        }

                        if (request.Equals(ForegroundService.BroadcastFinishActivity))
                        {
                            if (!ActivityCommon.CommActive && !_activityActive)
                            {
                                bool isEmpty = IsActivityListEmpty(new List<Type> { typeof(ActivityMain) });
                                if (isEmpty)
                                {
                                    Finish();
                                }
                                break;
                            }
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

                case ActivityCommon.PackageNameAction:
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
                    StoreActiveJobIndex();

                    JobReader jobReader = ActivityCommon.JobReader;
                    JobReader.PageInfo pageInfo = ActivityCommon.EdiabasThread?.JobPageInfo;
                    if (pageInfo != null)
                    {
                        int pageIndex = jobReader.PageList.IndexOf(pageInfo);
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

            long responseCount = 0;
            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
            if (ediabasThread != null)
            {
                responseCount = ediabasThread.GetResponseCount();
            }

            StopEdiabasThread(true);
            UpdateDisplay();

            _translationList = null;
            _translatedList = null;

            StoreTranslation();
            UpdateCheck();
            if (_instanceData.CommErrorsCount >= ActivityCommon.MinSendCommErrors && responseCount > 0 &&
                _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth)
                {
                    if (_activityCommon.MtcBtService && !_activityCommon.MtcBtConnected)
                    {
                        _activityCommon.MtcBtDisconnectWarnShown = false;
                    }
                }

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

        private bool OpenTraceFile(bool backupTrace = false)
        {
            string baseDir = backupTrace ? _instanceData.TraceBackupDir : _instanceData.TraceDir;
            if (string.IsNullOrEmpty(baseDir))
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
            JobReader jobReader = ActivityCommon.JobReader;
            JobReader.PageInfo pageInfo = null;
            int index = _tabLayout.SelectedTabPosition;
            if (index >= 0 && index < (jobReader.PageList.Count))
            {
                pageInfo = jobReader.PageList[index];
            }
            return pageInfo;
        }

        private void UpdateSelectedPage()
        {
            try
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

                ActivityCommon.EdiabasThread.JobPageInfo = newPageInfo;
            }
            finally
            {
                StoreActiveJobIndex();
            }
        }

        private void ClearPage(int index)
        {
            JobReader jobReader = ActivityCommon.JobReader;
            if (index >= 0 && index < (jobReader.PageList.Count))
            {
                JobReader.PageInfo pageInfo = jobReader.PageList[index];
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

        private string GetSelectedPageSgbd()
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

            string sgbd = pageInfo.JobsInfo.Sgbd;
            if (string.IsNullOrEmpty(sgbd))
            {
                foreach (JobReader.JobInfo jobInfo in pageInfo.JobsInfo.JobList)
                {
                    sgbd = jobInfo.Sgbd;
                    if (!string.IsNullOrEmpty(sgbd))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sgbd))
            {
                return null;
            }

            return sgbd;
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

        private void PostUpdateDisplay(bool force = false)
        {
            if (_activityCommon == null)
            {
                return;
            }

            if (_updateHandler == null)
            {
                return;
            }

            if (force)
            {
                ActivityCommon.PostRunnable(_updateHandler, _updateDisplayForceRunnable);
                return;
            }

            ActivityCommon.PostRunnable(_updateHandler, _updateDisplayRunnable);
        }

        private void UpdateDisplay(bool forceUpdate = false)
        {
            if (!_activityActive || (_activityCommon == null))
            {   // OnDestroy already executed
                return;
            }

            JobReader jobReader = ActivityCommon.JobReader;
            long startTime = Stopwatch.GetTimestamp();
            bool dynamicValid = false;

            _connectButtonInfo.Enabled = true;
            if (ActivityCommon.CommActive)
            {
                lock (EdiabasThread.DataLock)
                {
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    if (ediabasThread != null)
                    {
                        if (ediabasThread.ThreadStopping())
                        {
                            _connectButtonInfo.Enabled = false;
                        }

                        if (ediabasThread.CommActive)
                        {
                            dynamicValid = true;
                        }
                    }
                }

                _connectButtonInfo.Checked = true;
            }
            else
            {
                // ReSharper disable once ReplaceWithSingleAssignment.True
                bool buttonEnabled = true;

                if (jobReader.PageList.Count == 0)
                {
                    buttonEnabled = false;
                }

                if (!_activityCommon.IsInterfaceAvailable(_instanceData.SimulationPath))
                {
                    buttonEnabled = false;
                }

                if (IsErrorEvalJobRunning())
                {
                    buttonEnabled = false;
                }

                if (_downloadProgress != null || _downloadEcuAlertDialog != null)
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
                double? batteryVoltage = null;
                byte[] adapterSerial = null;
                lock (EdiabasThread.DataLock)
                {
                    EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                    if (ediabasThread != null)
                    {
                        batteryVoltage = ediabasThread.BatteryVoltage;
                        adapterSerial = ediabasThread.AdapterSerial;
                    }
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
#pragma warning disable CA1416 // Plattformkompatibilit�t �berpr�fen
                        if (ActivityCommon.SwapMultiWindowOrientation && IsInMultiWindowMode)
                        {
                            portrait = !portrait;
                        }
#pragma warning restore CA1416 // Plattformkompatibilit�t �berpr�fen
                    }

                    int gaugeCount = portrait ? pageInfo.GaugesPortraitValue : pageInfo.GaugesLandscapeValue;
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
                        EdiabasThread.UpdateState updateState = EdiabasThread.UpdateState.Init;
                        int updateProgress = 0;

                        lock (EdiabasThread.DataLock)
                        {
                            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                            if (ediabasThread != null)
                            {
                                if (ediabasThread.ResultPageInfo == pageInfo)
                                {
                                    errorReportList = ediabasThread.EdiabasErrorReportList;
                                }

                                updateState = ediabasThread.UpdateProgressState;
                                updateProgress = ediabasThread.UpdateProgress;
                            }
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
                                        PostUpdateDisplay();
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

                    if (ActivityCommon.IsCommunicationError(errorReport.ExceptionText))
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
                        if (_errorRestAlertDialog != null)
                        {
                            _errorRestAlertDialog.DismissEvent += (sender, args) =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }
                                _errorRestAlertDialog = null;
                            };
                        }
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
                                PostUpdateDisplay();
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

            EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
            if (ediabasThread == null)
            {
                return string.Empty;
            }

            return ediabasThread.GenerateErrorMessage(this, _activityCommon, pageInfo, errorReport, errorIndex, formatErrorResult, ref translationList,
                (Tuple<string, string> text, ref List<string> list) =>
                {
                    string text1 = text.Item1;
                    string text2 = text.Item2;

                    if (_activityCommon.IsTranslationRequired() && ActivityCommon.EnableTranslation)
                    {
                        int index = list.Count;
                        list.Add(text1);
                        list.Add(text2);
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

                    return new Tuple<string, string>(text1, text2);
                }, ref dtcList);
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

        public static string GetResultString(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, int index, out bool found)
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

        public static string GetPageString(JobReader.PageInfo pageInfo, string name)
        {
            string lang = ActivityCommon.GetCurrentLanguageStatic();
            string langIso = ActivityCommon.GetCurrentLanguageStatic(true);
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

            string result = string.Empty;
            if (stringInfoSel != null)
            {
                if (!stringInfoSel.StringDict.TryGetValue(name, out result))
                {
                    result = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(result) && stringInfoDefault != null)
            {
                if (!stringInfoDefault.StringDict.TryGetValue(name, out result))
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
                PostCreateActionBarTabs();
                return;
            }

            bool failed = false;
            JobReader jobReader = ActivityCommon.JobReader;
            string lastFileName = jobReader.XmlFileName ?? string.Empty;

            jobReader = new JobReader(true);
            try
            {
                if (_instanceData.LastAppState != ActivityCommon.LastAppState.Compile && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    if (!jobReader.ReadXml(_instanceData.ConfigFileName, out string errorMessage))
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

                string newFileName = jobReader.XmlFileName ?? string.Empty;
                if (string.Compare(lastFileName, newFileName, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    _tabsCreated = false;
                }
            }
            finally
            {
                ActivityCommon.JobReader = jobReader;
            }

            UpdateJobReaderSettings();
            _translationList = null;
            _translatedList = null;

            ReadTranslation();
            UpdateDirectories();
            PostUpdateDisplay();

            if (!failed)
            {
                RequestConfigSelect();
            }

            PostCompileCode();
        }

        private bool ReadTranslation()
        {
            if (_activityCommon == null)
            {
                return false;
            }

            try
            {
                if (ActivityCommon.IsTranslationAvailable() && _activityCommon.IsTranslationRequired() && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    string xmlFileDir = Path.GetDirectoryName(_instanceData.ConfigFileName);
                    if (!string.IsNullOrEmpty(xmlFileDir))
                    {
                        return _activityCommon.ReadTranslationCache(Path.Combine(xmlFileDir, TranslationFileNameMain));
                    }
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
                if (ActivityCommon.IsTranslationAvailable() && _activityCommon.IsTranslationRequired() && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    string xmlFileDir = Path.GetDirectoryName(_instanceData.ConfigFileName);
                    if (!string.IsNullOrEmpty(xmlFileDir))
                    {
                        return _activityCommon.StoreTranslationCache(Path.Combine(xmlFileDir, TranslationFileNameMain));
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        private void UpdateJobReaderSettings()
        {
            _instanceData.AdapterCheckOk = false;
            JobReader jobReader = ActivityCommon.JobReader;
            if (jobReader.PageList.Count > 0)
            {
                ActivityCommon.SelectedManufacturer = jobReader.Manufacturer;
                _activityCommon.SelectedInterface = jobReader.Interface;
            }
            else
            {
                _activityCommon.SelectedInterface = ActivityCommon.InterfaceType.None;
            }

            StoreSettings();
        }

        private void VerifyEcuMd5()
        {
            _instanceData.VerifyEcuMd5 = true;
            PostCompileCode();
        }

        private void PostCompileCode()
        {
            if (_activityCommon == null)
            {
                return;
            }

            if (_updateHandler == null)
            {
                return;
            }

            if (ActivityCommon.PostRunnable(_updateHandler, _compileCodeRunnable))
            {
#if DEBUG
                Log.Info(Tag, string.Format("PostCompileCode rejected: {0}", this));
#endif
            }
            else
            {
#if DEBUG
                Log.Info(Tag, string.Format("PostCompileCode accepted: {0}", this));
#endif
            }
        }

        private void CompileCode()
        {
            if (!_activityActive || _compileProgress != null)
            {
#if DEBUG
                Log.Info(Tag, string.Format("CompileCode rejected: {0}", this));
#endif
                _compileCodePending = true;
                return;
            }

#if DEBUG
            Log.Info(Tag, string.Format("CompileCode accepted: {0}", this));
#endif
            JobReader jobReader = ActivityCommon.JobReader;
            _compileCodePending = false;

            if (!ActivityCommon.IsCpuStatisticsSupported() || !ActivityCommon.CheckCpuUsage)
            {
                _instanceData.CheckCpuUsage = false;
            }

            if (!ActivityCommon.CheckEcuFiles)
            {
                _instanceData.VerifyEcuFiles = false;
            }

            if (jobReader.PageList.Count == 0 && !_instanceData.CheckCpuUsage && !_instanceData.ExtractSampleFiles &&
                !_instanceData.VerifyEcuFiles && !_instanceData.VerifyEcuMd5)
            {
                PostCreateActionBarTabs();
                return;
            }

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
                string exceptionMessage = string.Empty;
                try
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

                    if (_instanceData.ExtractSampleFiles)
                    {
                        if (ExtractSampleFiles())
                        {
                            _instanceData.ExtractSampleFiles = false;
                        }
                    }

                    if (_instanceData.ExtractCaCertFiles)
                    {
                        if (ExtractCaCertFiles())
                        {
                            _instanceData.ExtractCaCertFiles = false;
                        }
                    }

                    if (_instanceData.VerifyEcuFiles || _instanceData.VerifyEcuMd5)
                    {
                        bool checkMd5 = _instanceData.VerifyEcuMd5;
                        _instanceData.VerifyEcuFiles = false;
                        _instanceData.VerifyEcuMd5 = false;
                        string ecuBaseDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.EcuBaseDir);
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
                                    File.Delete(Path.Combine(ecuBaseDir, EcuPackInfoXmlName));
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

                        StoreLastAppState(ActivityCommon.LastAppState.Compile);
                    });

                    List<Microsoft.CodeAnalysis.MetadataReference> referencesList = null;
                    List<string> errorList = null;

                    for (int extractTry = 0; extractTry < 2; extractTry++)
                    {
                        bool forceUpdate = extractTry > 0;
                        if (jobReader.PageList.Any(pageInfo => pageInfo.ClassCode != null))
                        {
                            if (!_activityCommon.ExtraktPackageAssemblies(_instanceData.PackageAssembliesDir,
                                    percent =>
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            if (_activityCommon == null)
                                            {
                                                return;
                                            }

                                            if (_compileProgress != null)
                                            {
                                                _compileProgress.Indeterminate = false;
                                                _compileProgress.Progress = percent;
                                            }
                                        });

                                        return true;
                                    }, forceUpdate))
                            {
#if DEBUG
                                Log.Info(Tag, "CompileCode ExtraktPackageAssemblies failed: Force={0}", forceUpdate);
#endif
                            }
                        }

                        referencesList = ActivityCommon.GetLoadedMetadataReferences(_instanceData.PackageAssembliesDir, out errorList);
                        if (errorList.Count == 0)
                        {
                            break;
                        }

                        if (errorList.Count > 0)
                        {
#if DEBUG
                            Log.Info(Tag, string.Format("CompileCode GetLoadedMetadataReferences failed: Errors={0}, Force={1}", errorList.Count, forceUpdate));
#endif
                        }
                    }

                    if (jobReader.PageList.Any(pageInfo => pageInfo.ClassCode != null))
                    {
                        if (!_activityCommon.ExtraktPackageAssemblies(_instanceData.PackageAssembliesDir,
                                percent =>
                                {
                                    RunOnUiThread(() =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }

                                        if (_compileProgress != null)
                                        {
                                            _compileProgress.Indeterminate = false;
                                            _compileProgress.Progress = percent;
                                        }
                                    });

                                    return true;
                                }))
                        {
#if DEBUG
                            Log.Info(Tag, "CompileCode ExtraktPackageAssemblies failed");
#endif
                        }

                        bool progressUpdated = false;
                        List<string> compileResultList = new List<string>();
                        List<Thread> threadList = new List<Thread>();
                        foreach (JobReader.PageInfo pageInfo in jobReader.PageList)
                        {
                            if (pageInfo.ClassCode == null)
                            {
                                continue;
                            }

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
                                string result = _activityCommon.CompileCode(infoLocal, referencesList);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    result = GetPageString(infoLocal, infoLocal.Name) + ":\r\n" + result;
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

                        if (compileResultList.Count > 0)
                        {
                            if (errorList != null && errorList.Count > 0)
                            {
                                StringBuilder sbMessage = new StringBuilder();
                                sbMessage.AppendLine(GetString(Resource.String.compile_missing_assemblies));

                                foreach (string error in errorList)
                                {
                                    sbMessage.AppendLine(error);
                                }

                                RunOnUiThread(() =>
                                {
                                    if (_activityCommon == null)
                                    {
                                        return;
                                    }
                                    _activityCommon.ShowAlert(sbMessage.ToString(), Resource.String.alert_title_error);
                                });
                            }
                            else
                            {
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
                            }
                        }
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        StoreLastAppState(ActivityCommon.LastAppState.Compiled);
                        PostCreateActionBarTabs();

                        _compileProgress.Dismiss();
                        _compileProgress = null;
                        UpdateLockState();

                        if (cpuUsage >= CpuLoadCritical)
                        {
                            string balloonMessage = string.Format(GetString(Resource.String.compile_cpu_usage_high), cpuUsage);
                            ShowBallonMessage(balloonMessage);
                        }
                    });
                }
                catch (Exception e)
                {
                    exceptionMessage = EdiabasNet.GetExceptionText(e, false, false);
                }

                if (!string.IsNullOrEmpty(exceptionMessage))
                {
                    RunOnUiThread(() =>
                    {
                        _activityCommon.ShowAlert(exceptionMessage, Resource.String.alert_title_error);
                    });
                }
            });
            compileThreadWrapper.Start();
        }

        private void SelectMedia()
        {
            StartGlobalSettings(GlobalSettingsActivity.SelectionStorageLocation);
        }

        private void DownloadFile(string url, string downloadDir, string unzipTargetDir = null)
        {
            if (string.IsNullOrEmpty(_assetEcuFileName))
            {
                ShowAssetMissingRestart();
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
            UpdateDisplay();
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
                        xmlInfo.Add(new XAttribute("Name", Path.GetFileName(_assetEcuFileName) ?? string.Empty));
                        xmlInfo.Add(new XAttribute("DataId", EdiabasNet.EncodeFileNameKey ?? string.Empty));
                        if (_assetEcuFileSize > 0)
                        {
                            xmlInfo.Add(new XAttribute("Size", XmlConvert.ToString(_assetEcuFileSize)));
                        }
                        downloadInfo = new DownloadInfo(downloadDir, unzipTargetDir, xmlInfo);
                    }
                    // ReSharper disable once RedundantNameQualifier
                    string extension = Path.GetExtension(fileName);
                    bool isPhp = string.Compare(extension, ".php", StringComparison.OrdinalIgnoreCase) == 0;
                    System.Threading.Tasks.Task<HttpResponseMessage> taskDownload;
                    if (isPhp)
                    {
                        string installer = _activityCommon.GetInstallerPackageName();
                        string obbName = string.Empty;
                        try
                        {
                            obbName = Path.GetFileName(_assetEcuFileName) ?? string.Empty;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        MultipartFormDataContent formDownload = new MultipartFormDataContent
                        {
                            { new StringContent(ActivityCommon.AppId), "appid" },
                            { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", _activityCommon.VersionCode)), "appver" },
                            { new StringContent(_activityCommon.GetCurrentLanguage()), "lang" },
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
                            _downloadProgress = null;
                            UpdateLockState();
                            UpdateDisplay();
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
                                if (altertDialog != null)
                                {
                                    altertDialog.DismissEvent += (o, eventArgs) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }
                                        if (!yesSelected)
                                        {
                                            _downloadProgress.Dismiss();
                                            _downloadProgress = null;
                                            UpdateLockState();
                                            UpdateDisplay();
                                        }
                                    };
                                }
#else
                                ExtractObbFile(downloadInfo, key);
#endif
                                return;
                            }

                            error = true;
                        }
                    }

                    _downloadProgress.Dismiss();
                    _downloadProgress = null;
                    UpdateLockState();
                    UpdateDisplay();

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
            if (ActivityCommon.HasValidProxyHost() != null)
            {
                message += "\n" + GetString(Resource.String.download_proxy_config);
            }

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

                using (Aes crypto = Aes.Create())
                {
                    crypto.Mode = CipherMode.CBC;
                    crypto.Padding = PaddingMode.PKCS7;
                    crypto.KeySize = 256;

                    byte[] appIdBytes = Encoding.ASCII.GetBytes(ActivityCommon.AppId.ToLowerInvariant());
                    using (SHA256 sha256 = SHA256.Create())
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
                if (string.IsNullOrEmpty(_assetEcuFileName))
                {
                    return null;
                }
                string baseName = Path.GetFileName(_assetEcuFileName);
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
            ExtractZipFile(_assetManager, _assetEcuFileName, downloadInfo.TargetDir, downloadInfo.InfoXml, key, false,
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

                    List<string> encodeExtensions = null;
                    if (!ActivityCommon.DisableFileNameEncoding)
                    {
                        encodeExtensions = new List<string>() { EdiabasNet.PrgFileExt, EdiabasNet.GroupFileExt };
                    }

                    int lastZipPercent = -1;
                    ActivityCommon.ExtractZipFile(assetManager, null, fileName, targetDirectory, key, ignoreFolders, encodeExtensions,
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

                    ExtractSampleFiles(true);
                    ExtractCaCertFiles(true);

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (!extractFailed)
                    {
                        infoXml?.Save(Path.Combine(targetDirectory, EcuPackInfoXmlName));
                    }
                }
                catch (Exception ex)
                {
                    extractFailed = true;
                    exceptionText = EdiabasNet.GetExceptionText(ex, false, false);
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
                        _downloadProgress = null;
                        UpdateLockState();
                        UpdateDisplay();
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
            if (_downloadProgress != null)
            {
                return;
            }

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
            if (!_activityActive || !_storageAccessGranted || _downloadEcuAlertDialog != null || ActivityCommon.CommActive)
            {
                return true;
            }

            if (_downloadProgress != null)
            {
                return false;
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

                if (_downloadEcuAlertDialog != null)
                {
                    _downloadEcuAlertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _downloadEcuAlertDialog = null;
                        UpdateDisplay();

                        if (!_instanceData.StorageRequirementsAccepted)
                        {
                            Finish();
                            return;
                        }

                        CheckForEcuFiles(checkPackage);
                    };
                }

                UpdateDisplay();
                return false;
            }

            if (string.IsNullOrEmpty(_assetEcuFileName))
            {
                ShowAssetMissingRestart();
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
                if (_downloadEcuAlertDialog != null)
                {
                    _downloadEcuAlertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        _downloadEcuAlertDialog = null;
                        UpdateDisplay();
                    };
                }

                UpdateDisplay();
                return false;
            }

            VehicleInfoBmw.FailureSource failureSource = VehicleInfoBmw.ResourceFailure;
            VehicleInfoBmw.ClearResourceFailure();
            if (failureSource == VehicleInfoBmw.FailureSource.File)
            {
                VerifyEcuMd5();
            }

            return true;
        }

        private void ShowAssetMissingRestart()
        {
            AlertDialog alertDialog = new AlertDialog.Builder(this)
                .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                .SetCancelable(true)
                .SetMessage(Resource.String.asset_missing_restart)
                .SetTitle(Resource.String.alert_title_error)
                .Show();

            if (alertDialog != null)
            {
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
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _activityCommon.OpenPlayStoreForPackage(PackageName);
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
                if (_downloadEcuAlertDialog != null)
                {
                    _downloadEcuAlertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _downloadEcuAlertDialog = null;
                        UpdateDisplay();

                        handler?.Invoke(this, EventArgs.Empty);
                    };
                }

                _instanceData.UpdateSkipVersion = -1;
                StoreSettings();
                UpdateDisplay();
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

                IEnumerable<string> files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                if (!files.Any(s => s.EndsWith(EdiabasNet.PrgFileExt, StringComparison.OrdinalIgnoreCase) || s.EndsWith(EdiabasNet.EncodedFileExt, StringComparison.OrdinalIgnoreCase)))
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

                string xmlInfoName = Path.Combine(path, EcuPackInfoXmlName);
                if (!File.Exists(xmlInfoName))
                {
                    return false;
                }

                XDocument xmlInfo = XDocument.Load(xmlInfoName);
                if (xmlInfo.Root == null)
                {
                    return false;
                }

                string rootName = xmlInfo.Root.Name.LocalName;
                if (string.IsNullOrEmpty(rootName) ||
                    string.Compare(rootName, "Info", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                XAttribute nameAttr = xmlInfo.Root.Attribute("Name");
                if (nameAttr == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(_assetEcuFileName) ||
                    string.Compare(Path.GetFileNameWithoutExtension(nameAttr.Value), Path.GetFileNameWithoutExtension(_assetEcuFileName), StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                XAttribute dataIdAttr = xmlInfo.Root.Attribute("DataId");
                string dataId = dataIdAttr?.Value ?? string.Empty;
                string encodeKey = EdiabasNet.EncodeFileNameKey ?? string.Empty;
                if (string.Compare(dataId, encodeKey, StringComparison.Ordinal) != 0)
                {
                    return false;
                }

                XAttribute sizeAttr = xmlInfo.Root.Attribute("Size");
                if (sizeAttr != null)
                {
                    try
                    {
                        Int64 fileSize = XmlConvert.ToInt64(sizeAttr.Value);
                        if (fileSize != _assetEcuFileSize)
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

        private bool ExtractSampleFiles(bool force = false)
        {
            try
            {
                if (_activityCommon == null)
                {
                    return false;
                }

                AssetManager assets = ActivityCommon.GetPackageContext()?.Assets;
                if (assets == null)
                {
                    return false;
                }

                string resourceName = null;
                string[] assetFiles = assets.List(string.Empty);
                if (assetFiles != null)
                {
                    Regex regex = new Regex(@"^Sample.*\.zip$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (string fileName in assetFiles)
                    {
                        if (regex.IsMatch(fileName))
                        {
                            resourceName = fileName;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(resourceName))
                {
                    return false;
                }

                string sampleDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.ConfigBaseSubDir, ActivityCommon.ConfigSampleSubDir);
                string sampleInfoFile = Path.Combine(sampleDir, SampleInfoFileName);
                if (!force && File.Exists(sampleInfoFile))
                {
                    bool validInfoData = true;

                    XDocument xmlInfoRead = XDocument.Load(sampleInfoFile);
                    XAttribute nameAttr = xmlInfoRead.Root?.Attribute("Name");
                    if (nameAttr == null)
                    {
                        validInfoData = false;
                    }
                    else
                    {
                        if (string.Compare(nameAttr.Value, resourceName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            validInfoData = false;
                        }
                    }

                    XAttribute verAttr = xmlInfoRead.Root?.Attribute("AppVer");
                    if (verAttr == null)
                    {
                        validInfoData = false;
                    }
                    else
                    {
                        try
                        {
                            Int64 verValue = XmlConvert.ToInt64(verAttr.Value);
                            if (verValue != _activityCommon.VersionCode)
                            {
                                validInfoData = false;
                            }
                        }
                        catch (Exception)
                        {
                            validInfoData = false;
                        }
                    }

                    if (validInfoData)
                    {
                        return true;
                    }
                }

                if (Directory.Exists(sampleDir))
                {
                    try
                    {
                        Directory.Delete(sampleDir, true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                ActivityCommon.ExtractZipFile(assets, null, resourceName, sampleDir, null, null, null, null);

                XElement xmlInfo = new XElement("Info");
                xmlInfo.Add(new XAttribute("Name", resourceName));
                xmlInfo.Add(new XAttribute("AppVer", _activityCommon.VersionCode));
                xmlInfo.Save(sampleInfoFile);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ExtractCaCertFiles(bool force = false)
        {
            try
            {
                if (_activityCommon == null)
                {
                    return false;
                }

                AssetManager assets = ActivityCommon.GetPackageContext()?.Assets;
                if (assets == null)
                {
                    return false;
                }

                string resourceName = null;
                string[] assetFiles = assets.List(string.Empty);
                if (assetFiles != null)
                {
                    Regex regex = new Regex(@"^CaCerts.*\.zip$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (string fileName in assetFiles)
                    {
                        if (regex.IsMatch(fileName))
                        {
                            resourceName = fileName;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(resourceName))
                {
                    return false;
                }

                string securityDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.SecuritySubDir);
                string caCertsDir = Path.Combine(securityDir, ActivityCommon.CaCertsSubDir);
                string certsDir = Path.Combine(securityDir, ActivityCommon.CertsSubDir);
                string caCertInfoFile = Path.Combine(caCertsDir, CaCertInfoFileName);
                if (!force && File.Exists(caCertInfoFile))
                {
                    bool validInfoData = true;

                    XDocument xmlInfoRead = XDocument.Load(caCertInfoFile);
                    XAttribute nameAttr = xmlInfoRead.Root?.Attribute("Name");
                    if (nameAttr == null)
                    {
                        validInfoData = false;
                    }
                    else
                    {
                        if (string.Compare(nameAttr.Value, resourceName, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            validInfoData = false;
                        }
                    }

                    XAttribute verAttr = xmlInfoRead.Root?.Attribute("AppVer");
                    if (verAttr == null)
                    {
                        validInfoData = false;
                    }
                    else
                    {
                        try
                        {
                            Int64 verValue = XmlConvert.ToInt64(verAttr.Value);
                            if (verValue != _activityCommon.VersionCode)
                            {
                                validInfoData = false;
                            }
                        }
                        catch (Exception)
                        {
                            validInfoData = false;
                        }
                    }

                    if (validInfoData)
                    {
                        return true;
                    }
                }

                if (Directory.Exists(securityDir))
                {
                    try
                    {
                        Directory.Delete(securityDir, true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                try
                {
                    Directory.CreateDirectory(certsDir);
                }
                catch (Exception)
                {
                    // ignored
                }

                ActivityCommon.ExtractZipFile(assets, null, resourceName, caCertsDir, null, null, null, null);

                XElement xmlInfo = new XElement("Info");
                xmlInfo.Add(new XAttribute("Name", resourceName));
                xmlInfo.Add(new XAttribute("AppVer", _activityCommon.VersionCode));
                xmlInfo.Save(caCertInfoFile);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SelectManufacturer()
        {
            if (_activityCommon == null)
            {
                return;
            }

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

                ClearConfiguration();   // settings are stored here
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
            if (_lastCompileCrash)
            {
                _lastCompileCrash = false;
                _configSelectAlertDialog = new AlertDialog.Builder(this)
                    .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                    .SetCancelable(true)
                    .SetMessage(Resource.String.compile_crash)
                    .SetTitle(Resource.String.alert_title_error)
                    .Show();
                if (_configSelectAlertDialog != null)
                {
                    _configSelectAlertDialog.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }
                        _configSelectAlertDialog = null;
                    };
                }
                return;
            }

            JobReader jobReader = ActivityCommon.JobReader;
            if (jobReader.PageList.Count > 0)
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
                .SetNeutralButton(Resource.String.button_manufacturer, (sender, args) =>
                {
                    SelectManufacturer();
                })
                .SetCancelable(true)
                .SetMessage(message)
                .SetTitle(Resource.String.alert_title_question)
                .Show();
            if (_configSelectAlertDialog != null)
            {
                _configSelectAlertDialog.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }
                    _configSelectAlertDialog = null;
                };
            }
        }

        private void ClearConfiguration()
        {
            StoreTranslation();
            ActivityCommon.JobReader = new JobReader(true);
            _instanceData.ConfigFileName = string.Empty;
            StoreActiveJobIndex();
            StoreSettings();

            PostCreateActionBarTabs();

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
                if (sparseArray == null)
                {
                    return;
                }

                JobReader jobReader = ActivityCommon.JobReader;
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
                            if (value && !jobReader.LogTagsPresent)
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

        private void OpenDonateLink()
        {
            _activityCommon.OpenWebUrl("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VUFSVNBRQQBPJ");
        }

        private void ShowBallonMessage(string message, int dismissDuration = ActivityCommon.BalloonDismissDuration, BalloonAlligment alignment = BalloonAlligment.Center)
        {
            View rootView = _contentView?.RootView;
            if (rootView != null)
            {
                Balloon.Builder balloonBuilder = ActivityCommon.GetBalloonBuilder(this);
                balloonBuilder.SetText(message);
                balloonBuilder.SetAutoDismissDuration(dismissDuration);
                balloonBuilder.SetDismissWhenClicked(true);
                switch (alignment)
                {
                    case BalloonAlligment.Top:
                        balloonBuilder.SetArrowOrientation(ArrowOrientation.Top);
                        break;

                    case BalloonAlligment.Bottom:
                        balloonBuilder.SetArrowOrientation(ArrowOrientation.Bottom);
                        break;
                }

                Balloon balloon = balloonBuilder.Build();
                switch (alignment)
                {
                    case BalloonAlligment.Top:
                        balloon.ShowAlignTop(rootView);
                        break;

                    case BalloonAlligment.Bottom:
                        balloon.ShowAlignBottom(rootView);
                        break;

                    default:
                        balloon.ShowAtCenter(rootView);
                        break;
                }
            }
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

        private bool StartGlobalSettings(string selection = null, string copyFileName = null)
        {
            try
            {
                ActivityRequest requestCode = ActivityRequest.RequestGlobalSettings;
                Intent serverIntent = new Intent(this, typeof(GlobalSettingsActivity));
                serverIntent.PutExtra(GlobalSettingsActivity.ExtraAppDataDir, _instanceData.AppDataPath);
                if (!string.IsNullOrEmpty(selection))
                {
                    switch (selection)
                    {
                        case GlobalSettingsActivity.SelectionCopyFromApp:
                            requestCode = ActivityRequest.RequestGlobalSettingsCopy;
                            break;
                    }

                    serverIntent.PutExtra(GlobalSettingsActivity.ExtraSelection, selection);
                }

                if (!string.IsNullOrEmpty(copyFileName))
                {
                    serverIntent.PutExtra(GlobalSettingsActivity.ExtraCopyFileName, copyFileName);
                    serverIntent.PutExtra(GlobalSettingsActivity.ExtraDeleteFile, true);
                }

                StartActivityForResult(serverIntent, (int) requestCode);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void AdapterConfig()
        {
            if (!CheckForEcuFiles())
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
            serverIntent.PutExtra(CanAdapterActivity.ExtraAppDataDir, _instanceData.AppDataPath);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestAdapterConfig);
        }

        private void AdapterIpConfig()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }

            if (RequestWifiPermissions())
            {
                return;
            }

            _activityCommon.SelectAdapterIp((sender, args) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                StoreSettings();
                UpdateOptionsMenu();
                UpdateDisplay();
            });
        }

        private void SelectConfigFile()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }

            SelectConfigFileIntent();
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
                else
                {
                    string sampleDir = Path.Combine(_instanceData.AppDataPath, ActivityCommon.ConfigBaseSubDir, ActivityCommon.ConfigSampleSubDir);
                    if (Directory.Exists(sampleDir))
                    {
                        initDir = sampleDir;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            serverIntent.PutExtra(FilePickerActivity.ExtraInitDir, initDir);
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".cccfg");
            serverIntent.PutExtra(FilePickerActivity.ExtraInfoText, GetString(Resource.String.menu_hint_copy_folder));
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestSelectConfig);
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
            serverIntent.PutExtra(FilePickerActivity.ExtraFileExtensions, ".cccfg;.ccpages;.ccpage;.xml");
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEditConfig);
        }

        private void StartXmlTool(XmlToolActivity.EcuFunctionCallType ecuFuncCall = XmlToolActivity.EcuFunctionCallType.None)
        {
            try
            {
                if (!CheckForEcuFiles())
                {
                    return;
                }

                JobReader jobReader = ActivityCommon.JobReader;
                if (jobReader == null)
                {
                    return;
                }

                if (jobReader.ManualEdit && !string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            ClearConfiguration();
                            StartXmlTool(ecuFuncCall);
                        })
                        .SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                        {
                        })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.config_manual_edit)
                        .SetTitle(Resource.String.alert_title_info)
                        .Show();
                    return;
                }

                string pageFileName = null;
                bool ecuAutoRead = false;
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

                    if (ecuFuncCall == XmlToolActivity.EcuFunctionCallType.BmwService)
                    {
                        pageFileName = null;
                        ecuAutoRead = true;
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
                        VerifyEcuMd5();
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
                serverIntent.PutExtra(XmlToolActivity.ExtraSimulationDir, _instanceData.SimulationPath);
                if (!string.IsNullOrEmpty(pageFileName))
                {
                    serverIntent.PutExtra(XmlToolActivity.ExtraPageFileName, pageFileName);
                }
                serverIntent.PutExtra(XmlToolActivity.ExtraEcuFuncCall, (int)ecuFuncCall);
                serverIntent.PutExtra(XmlToolActivity.ExtraEcuAutoRead, ecuAutoRead);
                serverIntent.PutExtra(XmlToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(XmlToolActivity.ExtraDeviceName, _instanceData.DeviceName);
                serverIntent.PutExtra(XmlToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
                serverIntent.PutExtra(XmlToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(XmlToolActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(XmlToolActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                serverIntent.PutExtra(XmlToolActivity.ExtraFileName, _instanceData.ConfigFileName);
                serverIntent.PutExtra(XmlToolActivity.ExtraMotorbikes, jobReader.IsMotorbike);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestXmlTool);
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
                    string sgbd = GetSelectedPageSgbd();
                    if (string.IsNullOrEmpty(sgbd))
                    {
                        return;
                    }

                    sgdbFile = Path.Combine(_instanceData.EcuPath, sgbd);
                }

                Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
                serverIntent.PutExtra(EdiabasToolActivity.ExtraInitDir, _instanceData.EcuPath);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraAppDataDir, _instanceData.AppDataPath);
                if (!string.IsNullOrEmpty(_instanceData.SimulationPath))
                {
                    serverIntent.PutExtra(EdiabasToolActivity.ExtraSimulationDir, _instanceData.SimulationPath);
                }

                if (!string.IsNullOrEmpty(_instanceData.ConfigFileName))
                {
                    try
                    {
                        string xmlFileDir = Path.GetDirectoryName(_instanceData.ConfigFileName);
                        if (!string.IsNullOrEmpty(xmlFileDir))
                        {
                            serverIntent.PutExtra(EdiabasToolActivity.ExtraConfigDir, xmlFileDir);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (!string.IsNullOrEmpty(sgdbFile))
                {
                    serverIntent.PutExtra(EdiabasToolActivity.ExtraSgbdFile, sgdbFile);
                }

                serverIntent.PutExtra(EdiabasToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceName, _instanceData.DeviceName);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceAddress, _instanceData.DeviceAddress);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraEnetIp, _activityCommon.SelectedEnetIp);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(EdiabasToolActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEdiabasTool);
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
                serverIntent.PutExtra(BmwCodingActivity.ExtraElmWifiIp, _activityCommon.SelectedElmWifiIp);
                serverIntent.PutExtra(BmwCodingActivity.ExtraDeepObdWifiIp, _activityCommon.SelectedDeepObdWifiIp);
                StartActivityForResult(serverIntent, (int)ActivityRequest.RequestBmwCoding);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
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

                IList<ResolveInfo> activities = _activityCommon.QueryIntentActivities(viewIntent, PackageInfoFlags.MatchDefaultOnly);
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

                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = EdiabasNet.GetExceptionText(ex, false, false);
#if DEBUG
                Log.Info(Tag, string.Format("StartEditXml Exception: {0}", errorMessage));
#endif
                string message = GetString(Resource.String.file_access_denied);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message += "\r\n" + errorMessage;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return false;
            }
        }

        private bool EditFontSize(JobReader.PageInfo currentPage)
        {
            try
            {
                if (currentPage == null)
                {
                    return false;
                }

                string fileName = currentPage.XmlFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                string fileText = File.ReadAllText(fileName);
                if (string.IsNullOrWhiteSpace(fileText))
                {
                    return false;
                }

                string startNodeName = "page";
                Regex regexfontSize = new Regex($"(<\\s*{startNodeName}\\s+.*\\W{JobReader.PageFontSize}\\s*=\\s*)\"(\\w+)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection matches = regexfontSize.Matches(fileText);
                int validMatchCount = GetValidEditRegExMatches(matches, startNodeName);
                if (validMatchCount == 0)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.file_editing_failed), Resource.String.alert_title_error);
                    return false;
                }

                int currentFontIndex = 0;
                if (currentPage.DisplayFontSize != null)
                {
                    switch (currentPage.DisplayFontSize.Value)
                    {
                        case XmlToolActivity.DisplayFontSize.Small:
                            currentFontIndex = 0;
                            break;

                        case XmlToolActivity.DisplayFontSize.Medium:
                            currentFontIndex = 1;
                            break;

                        case XmlToolActivity.DisplayFontSize.Large:
                            currentFontIndex = 2;
                            break;
                    }
                }

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.menu_cfg_page_edit_fontsize);
                ListView listView = new ListView(this);

                List<string> sizeNames = new List<string>
                {
                    GetString(Resource.String.xml_tool_ecu_font_size_small),
                    GetString(Resource.String.xml_tool_ecu_font_size_medium),
                    GetString(Resource.String.xml_tool_ecu_font_size_large),
                };
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                    Android.Resource.Layout.SimpleListItemSingleChoice, sizeNames.ToArray());
                listView.Adapter = adapter;
                listView.ChoiceMode = ChoiceMode.Single;
                listView.SetItemChecked(0, currentFontIndex == 0);
                listView.SetItemChecked(1, currentFontIndex == 1);
                listView.SetItemChecked(2, currentFontIndex == 2);

                builder.SetView(listView);
                builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    string selectedFont = XmlToolActivity.DisplayFontSize.Small.ToString().ToLowerInvariant();
                    switch (listView.CheckedItemPosition)
                    {
                        case 1:
                            selectedFont = XmlToolActivity.DisplayFontSize.Medium.ToString().ToLowerInvariant();
                            break;

                        case 2:
                            selectedFont = XmlToolActivity.DisplayFontSize.Large.ToString().ToLowerInvariant();
                            break;
                    }

                    string fileTextMod = regexfontSize.Replace(fileText, match =>
                    {
                        if (IsValidEditRegExMatch(match, startNodeName))
                        {
                            return string.Format(CultureInfo.InvariantCulture, "{0}\"{1}\"", match.Groups[1].Value, selectedFont);
                        }

                        return match.ToString();
                    });

                    if (fileTextMod != fileText)
                    {
                        WriteFileText(fileName, fileTextMod);
                    }
                });
                builder.SetNegativeButton(Resource.String.button_abort, (sender, args) => { });
                builder.Show();
            }
            catch (Exception ex)
            {
                string errorMessage = EdiabasNet.GetExceptionText(ex, false, false);
#if DEBUG
                Log.Info(Tag, string.Format("EditFontSize Exception: {0}", errorMessage));
#endif
                string message = GetString(Resource.String.file_access_denied);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message += "\r\n" + errorMessage;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return false;
            }

            return true;
        }

        private bool EditGaugesCount(JobReader.PageInfo currentPage, bool landscape)
        {
            try
            {
                if (currentPage == null)
                {
                    return false;
                }

                string fileName = currentPage.XmlFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                string fileText = File.ReadAllText(fileName);
                if (string.IsNullOrWhiteSpace(fileText))
                {
                    return false;
                }

                string startNodeName = "page";
                string keyWord = landscape ? JobReader.PageGaugesLandscape : JobReader.PageGaugesPortrait;
                Regex regexGauges = new Regex($"(<\\s*{startNodeName}\\s+.*\\W{keyWord}\\s*=\\s*)\"(\\w+)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection matches = regexGauges.Matches(fileText);
                int validMatchCount = GetValidEditRegExMatches(matches, startNodeName);
                if (validMatchCount == 0)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.file_editing_failed), Resource.String.alert_title_error);
                    return false;
                }

                int currentGauges = landscape ? currentPage.GaugesLandscapeValue : currentPage.GaugesPortraitValue;
                int titleId = landscape ? Resource.String.menu_cfg_page_edit_gauges_landscape : Resource.String.menu_cfg_page_edit_gauges_portrait;
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle(titleId);
                ListView listView = new ListView(this);

                const int minGauges = 1;
                const int maxGauges = 20;
                int selectedPosition = 0;
                List<string> gaugeNames = new List<string>();
                for (int gauges = minGauges; gauges <= maxGauges; gauges++)
                {
                    gaugeNames.Add(string.Format(CultureInfo.InvariantCulture, "{0}", gauges));
                    if (gauges == currentGauges)
                    {
                        selectedPosition = gaugeNames.Count - 1;
                    }
                }

                if (selectedPosition >= gaugeNames.Count)
                {
                    selectedPosition = gaugeNames.Count - 1;
                }

                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                    Android.Resource.Layout.SimpleListItemSingleChoice, gaugeNames.ToArray());
                listView.Adapter = adapter;
                listView.ChoiceMode = ChoiceMode.Single;
                listView.SetItemChecked(selectedPosition, true);

                builder.SetView(listView);
                builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    int pos = listView.CheckedItemPosition;
                    if (pos < 0)
                    {
                        return;
                    }

                    int selectedGauges = pos + minGauges;
                    string fileTextMod = regexGauges.Replace(fileText, match =>
                    {
                        if (IsValidEditRegExMatch(match, startNodeName))
                        {
                            return string.Format(CultureInfo.InvariantCulture, "{0}\"{1}\"", match.Groups[1].Value, selectedGauges);
                        }

                        return match.ToString();
                    });

                    if (fileTextMod != fileText)
                    {
                        WriteFileText(fileName, fileTextMod);
                    }
                });
                builder.SetNegativeButton(Resource.String.button_abort, (sender, args) => { });
                builder.Show();
            }
            catch (Exception ex)
            {
                string errorMessage = EdiabasNet.GetExceptionText(ex, false, false);
#if DEBUG
                Log.Info(Tag, string.Format("EditGaugesCount Exception: {0}", errorMessage));
#endif
                string message = GetString(Resource.String.file_access_denied);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message += "\r\n" + errorMessage;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return false;
            }

            return true;
        }

        private bool EditDisplayOrder(JobReader.PageInfo currentPage)
        {
            try
            {
                if (currentPage == null)
                {
                    return false;
                }

                string fileName = currentPage.XmlFileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                string fileText = File.ReadAllText(fileName);
                if (string.IsNullOrWhiteSpace(fileText))
                {
                    return false;
                }

                if (currentPage.DisplayList.Count < 1)
                {
                    return false;
                }

                string startNodeName = "display";
                Regex regexDisplayOrder = new Regex($"(<\\s*{startNodeName}\\s+.*\\W{JobReader.DisplayNodeOrder}\\s*=\\s*)\"(\\d+)\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection matches = regexDisplayOrder.Matches(fileText);
                int validMatchCount = GetValidEditRegExMatches(matches, startNodeName);
                if (validMatchCount != currentPage.DisplayList.Count)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.file_editing_failed), Resource.String.alert_title_error);
                    return false;
                }

                List<TextListReorderDialog.StringObjInfo> itemList = new List<TextListReorderDialog.StringObjInfo>();
                foreach (JobReader.DisplayInfo info in currentPage.DisplayList)
                {
                    string description = string.Format(GetString(Resource.String.display_line_original_position), info.OriginalPosition + 1);
                    itemList.Add(new TextListReorderDialog.StringObjInfo(GetPageString(currentPage, info.Name), description, info, info.OriginalPosition));
                }

                TextListReorderDialog dialog = new TextListReorderDialog(this, itemList);
                dialog.SetTitle(Resource.String.menu_cfg_page_edit_display_order);
                dialog.Message = string.Empty;
                dialog.MessageDetail = string.Empty;

                AlertDialog alertDialog = dialog.Create();

                dialog.ButtonExtra.SetText(Resource.String.button_reset);
                dialog.ButtonExtra.Visibility = ViewStates.Visible;
                dialog.ButtonExtra.Click += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    List<TextListReorderDialog.StringObjInfo> itemListSort = itemList.OrderBy(x => x.ItemId).ToList();
                    dialog.UpdateItemList(itemListSort);
                };

                dialog.ButtonAbort.Click += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    alertDialog.Cancel();
                };

                dialog.ButtonOk.Click += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    DialogFinished(dialog, regexDisplayOrder, fileName, fileText, startNodeName);
                    alertDialog.Cancel();
                };

                alertDialog.Show();
            }
            catch (Exception ex)
            {
                string errorMessage = EdiabasNet.GetExceptionText(ex, false, false);
#if DEBUG
                Log.Info(Tag, string.Format("EditDisplayOrder Exception: {0}", errorMessage));
#endif
                string message = GetString(Resource.String.file_access_denied);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message += "\r\n" + errorMessage;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);
                return false;
            }

            return true;

            void DialogFinished(TextListReorderDialog dialog, Regex regexDisplayOrder, string fileName, string fileText, string startNodeName, bool reset = false)
            {
                if (_activityCommon == null)
                {
                    return;
                }

                List<TextListReorderDialog.StringObjInfo> itemListMod = dialog.ItemList;
                int itemCount = itemListMod.Count;
                int replaceIndex = 0;
                string fileTextMod = regexDisplayOrder.Replace(fileText, match =>
                {
                    if (IsValidEditRegExMatch(match, startNodeName))
                    {
                        int orderIndex = -1;
                        if (reset)
                        {
                            orderIndex = 0;
                        }
                        else
                        {
                            int index = 0;
                            foreach (TextListReorderDialog.StringObjInfo info in itemListMod)
                            {
                                if (info.Data is JobReader.DisplayInfo displayInfo)
                                {
                                    if (displayInfo.OriginalPosition == replaceIndex)
                                    {
                                        orderIndex = index;
                                        break;
                                    }
                                }

                                index++;
                            }
                        }

                        if (orderIndex >= 0)
                        {
                            replaceIndex++;
                            return string.Format(CultureInfo.InvariantCulture, "{0}\"{1}\"", match.Groups[1].Value, orderIndex);
                        }
                    }

                    return match.ToString();
                });

                if (replaceIndex != itemCount)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.file_editing_failed), Resource.String.alert_title_error);
                    return;
                }

                if (fileTextMod != fileText)
                {
                    WriteFileText(fileName, fileTextMod);
                }
            }
        }

        public bool WriteFileText(string fileName, string fileText)
        {
            try
            {
                File.WriteAllText(fileName, fileText);
                ReadConfigFile();
            }
            catch (Exception ex)
            {
                string errorMessage = EdiabasNet.GetExceptionText(ex, false, false);
#if DEBUG
                Log.Info(Tag, string.Format("WriteFileText Exception: {0}", errorMessage));
#endif
                string message = GetString(Resource.String.file_access_denied);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    message += "\r\n" + errorMessage;
                }

                _activityCommon.ShowAlert(message, Resource.String.alert_title_error);

                return false;
            }

            return true;
        }

        public bool IsValidEditRegExMatch(Match match, string startNodeName)
        {
            if (match.Groups.Count != 3)
            {
                return false;
            }

            if (!match.Groups[0].Success)
            {
                return false;
            }

            string matchText = match.Groups[0].Value;
            if (matchText.Contains("/>", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string matchNodeEnd = "</" + (startNodeName ?? string.Empty);
            if (matchText.Contains(matchNodeEnd, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public int GetValidEditRegExMatches(MatchCollection matches, string startNodeName)
        {
            int matchCount = 0;
            foreach (Match regexMatch in matches)
            {
                if (IsValidEditRegExMatch(regexMatch, startNodeName))
                {
                    matchCount++;
                }
            }

            return matchCount;
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
                public string Title { get; set; }
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

            public void UpdatePageTitles()
            {
                bool changed = false;
                foreach (TabPageInfo tabPageInfo in _pageList)
                {
                    JobReader.PageInfo pageInfo = tabPageInfo.PageInfo;
                    if (pageInfo != null)
                    {
                        string title = GetPageString(pageInfo, pageInfo.Name);
                        if (tabPageInfo.Title != title)
                        {
                            tabPageInfo.Title = title;
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    NotifyDataSetChanged();
                }
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
                if (Activity is ActivityMain activityMain)
                {
                    JobReader jobReader = ActivityCommon.JobReader;
                    if (_pageInfoIndex >= 0 && _pageInfoIndex < jobReader.PageList.Count)
                    {
                        JobReader.PageInfo pageInfo = jobReader.PageList[_pageInfoIndex];
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
                    activityMain.PostUpdateDisplay();
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
                JobReader jobReader = ActivityCommon.JobReader;
                if (_pageInfoIndex >= 0 && _pageInfoIndex < jobReader.PageList.Count)
                {
                    JobReader.PageInfo pageInfo = jobReader.PageList[_pageInfoIndex];
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

        protected class ConnectActionArgs
        {
            public enum AutoConnectMode
            {
                None,
                Auto,
                Connect,
                Disconnect,
            }

            public enum ConnectSource
            {
                None,
                Manual,
                Auto,
            }

            public ConnectSource Source { get; set; }

            public AutoConnectMode AutoConnect { get; set; }

            public ConnectActionArgs(ConnectSource source, AutoConnectMode autoConnect = AutoConnectMode.None)
            {
                Source = source;
                AutoConnect = autoConnect;
            }
        }

        public class SelectTabPageRunnable : Java.Lang.Object, Java.Lang.IRunnable
        {
            private ActivityMain _activityMain;
            public int SelectTabPageIndex { get; set; }

            public SelectTabPageRunnable(ActivityMain activityMain)
            {
                _activityMain = activityMain;
                SelectTabPageIndex = -1;
            }

            public void Run()
            {
                try
                {
                    if (!_activityMain._activityActive)
                    {
                        return;
                    }

                    TabLayout tabLayout = _activityMain._tabLayout;
                    if (tabLayout == null)
                    {
                        return;
                    }

                    int selectTabPageIndex = SelectTabPageIndex;
                    if (selectTabPageIndex < 0 || selectTabPageIndex >= tabLayout.TabCount)
                    {
                        return;
                    }

                    tabLayout.GetTabAt(selectTabPageIndex)?.Select();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private class StateExecutor : Java.Lang.Object, Java.Util.Concurrent.IExecutor
        {
            public void Execute(Java.Lang.IRunnable command)
            {
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
                JobReader jobReader = ActivityCommon.JobReader;
                if (index >= 0 && index < (jobReader.PageList.Count))
                {
                    JobReader.PageInfo pageInfo = jobReader.PageList[index];
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
                    activityMain.ConnectAction(sender, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Manual));
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
                ((ActivityMain)Context).ConnectAction(((ActivityMain)Context)._connectButtonInfo.Button, new ConnectActionArgs(ConnectActionArgs.ConnectSource.Manual));
                return true;
            }
        }

        [BroadcastReceiver(Exported = false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public class ChooseReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
                    {
                        ComponentName clickedComponent = intent?.GetParcelableExtraType<ComponentName>(Intent.ExtraChosenComponent);
                        if (clickedComponent != null)
                        {
                            string packageName = clickedComponent.PackageName;
                            string className = clickedComponent.ClassName;
                            if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(className))
                            {
                                Intent broadcastIntent = new Intent(ActivityCommon.PackageNameAction);
                                broadcastIntent.PutExtra(ActivityCommon.BroadcastXmlEditorPackageName, packageName);
                                broadcastIntent.PutExtra(ActivityCommon.BroadcastXmlEditorClassName, className);
                                InternalBroadcastManager.InternalBroadcastManager.GetInstance(context).SendBroadcast(broadcastIntent);
                            }
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
