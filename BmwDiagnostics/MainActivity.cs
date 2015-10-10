//#define APP_USB_FILTER
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using BmwDiagnostics.FilePicker;
using EdiabasLib;
using Java.Interop;
using Mono.CSharp;
using Hoho.Android.UsbSerial.Driver;

#if APP_USB_FILTER
[assembly: Android.App.UsesFeature("android.hardware.usb.host")]
#endif

namespace BmwDiagnostics
{
    [Android.App.Activity(Label = "@string/app_name", MainLauncher = true,
            UiOptions=Android.Content.PM.UiOptions.SplitActionBarWhenNarrow,
            ConfigurationChanges = Android.Content.PM.ConfigChanges.KeyboardHidden |
                Android.Content.PM.ConfigChanges.Orientation |
                Android.Content.PM.ConfigChanges.ScreenSize)]
    [Android.App.MetaData("android.support.UI_OPTIONS", Value = "splitActionBarWhenNarrow")]
#if APP_USB_FILTER
    [Android.App.IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [Android.App.MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
#endif
    public class ActivityMain : AppCompatActivity, ActionBar.ITabListener
    {
        private enum ActivityRequest
        {
            RequestSelectDevice,
            RequestCanAdapterConfig,
            RequestSelectConfig,
            RequestXmlTool,
            RequestEdiabasTool,
        }

        private class UnzipInfo
        {
            public UnzipInfo(string fileName, string targetDir, XElement infoXml = null)
            {
                _fileName = fileName;
                _targetDir = targetDir;
                _infoXml = infoXml;
            }

            private readonly string _fileName;
            private readonly string _targetDir;
            private readonly XElement _infoXml;

            public string FileName
            {
                get { return _fileName; }
            }

            public string TargetDir
            {
                get { return _targetDir; }
            }

            public XElement InfoXml
            {
                get { return _infoXml; }
            }
        }

        class ConnectButtonInfo
        {
            public ToggleButton Button { get; set; }
            public bool Enabled { get; set; }
            public bool Checked { get; set; }
        }

        private const string SharedAppName = "de.holeschak.bmw_deep_obd";
        private const string AppFolderName = "de.holeschak.bmw_deep_obd";
        private const string EcuDirName = "Ecu";
        private const string EcuDownloadUrl = @"http://www.holeschak.de/BmwDeepObd/Ecu1.zip";
        private const string InfoXmlName = "Info.xml";

        public static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en");
        private string _deviceName = string.Empty;
        private string _deviceAddress = string.Empty;
        private string _configFileName = string.Empty;
        private int _currentVersionCode;
        private int _lastVersionCode;
        private string _appDataPath = String.Empty;
        private string _ecuPath = String.Empty;
        private bool _userEcuFiles;
        private bool _traceActive;
        private bool _traceAppend;
        private bool _dataLogActive;
        private bool _activityStarted;
        private bool _onStartExecuted;
        private bool _createTabsPending;
        private bool _autoStart;
        private ActivityCommon _activityCommon;
        private Timer _usbCheckTimer;
        private int _usbDeviceDetectCount;
        private JobReader _jobReader;
        private Handler _updateHandler;
        private EdiabasThread _ediabasThread;
        private StreamWriter _swDataLog;
        private string _dataLogDir;
        private List<Fragment> _fragmentList;
        private Fragment _lastFragment;
        private readonly ConnectButtonInfo _connectButtonInfo = new ConnectButtonInfo();
        private ImageView _imageBackground;
        private WebClient _webClient;
        private Android.App.ProgressDialog _downloadProgress;
        private AlertDialog _startAlertDialog;
        private AlertDialog _downloadEcuAlertDialog;

        public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //Log.Debug(Tag, "The tab {0} was re-selected.", tab.Text);
        }

        public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //Log.Debug(Tag, "The tab {0} has been selected.", tab.Text);
            Fragment frag = _fragmentList[tab.Position];
            ft.Replace(Resource.Id.tabFrameLayout, frag);
            _lastFragment = frag;
            UpdateSelectedPage();
        }

        public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            // perform any extra work associated with saving fragment state here.
            //Log.Debug(Tag, "The tab {0} as been unselected.", tab.Text);
            if (_lastFragment != null)
            {
                ft.Remove(_lastFragment);
                _lastFragment = null;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.NavigationMode = Android.Support.V7.App.ActionBar.NavigationModeStandard;
            SupportActionBar.SetHomeButtonEnabled(false);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayUseLogoEnabled(false);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetIcon(Resource.Drawable.icon);
            SetContentView(Resource.Layout.main);

            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityStarted)
                {
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                }
            }, BroadcastReceived);
            if (_activityCommon.UsbSupport)
            {   // usb handling
                RegisterReceiver(_activityCommon.BcReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
                RegisterReceiver(_activityCommon.BcReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
                if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                {   // attached event fails
                    _usbCheckTimer = new Timer(UsbCheckEvent, null, 1000, 1000);
                }
            }

            _appDataPath = string.Empty;
            _ecuPath = string.Empty;
            _userEcuFiles = false;
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
                    _ecuPath = Path.Combine(_appDataPath, EcuDirName);
                }
            }
            else
            {
                _appDataPath = _activityCommon.ExternalWritePath;
                _ecuPath = Path.Combine(_appDataPath, EcuDirName);
                if (!ValidEcuFiles(_ecuPath))
                {
                    string userEcuPath = Path.Combine(_appDataPath, "../../../..", AppFolderName, EcuDirName);
                    if (ValidEcuFiles(userEcuPath))
                    {
                        _ecuPath = userEcuPath;
                        _userEcuFiles = true;
                    }
                }
            }
            _updateHandler = new Handler();
            _jobReader = new JobReader();
            GetSettings();

            _imageBackground = FindViewById<ImageView>(Resource.Id.imageBackground);
            _fragmentList = new List<Fragment>();

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += DownloadProgressChanged;
            _webClient.DownloadFileCompleted += DownloadCompleted;
        }

        void AddTabToActionBar(string label)
        {
            ActionBar.Tab tab = SupportActionBar.NewTab()
                .SetText(label)
                .SetTabListener(this);
            SupportActionBar.AddTab(tab);
        }

        void CreateActionBarTabs()
        {
            if (!_activityStarted)
            {
                _createTabsPending = true;
                return;
            }
            _createTabsPending = false;
            SupportActionBar.RemoveAllTabs();
            _fragmentList.Clear();
            SupportActionBar.NavigationMode = Android.Support.V7.App.ActionBar.NavigationModeStandard;
            foreach (JobReader.PageInfo pageInfo in _jobReader.PageList)
            {
                int resourceId = Resource.Layout.tab_list;
                if (pageInfo.JobActivate) resourceId = Resource.Layout.tab_activate;

                Fragment fragmentPage = new TabContentFragment(this, resourceId, pageInfo);
                _fragmentList.Add(fragmentPage);
                pageInfo.InfoObject = fragmentPage;
                AddTabToActionBar(GetPageString(pageInfo, pageInfo.Name));
            }
            SupportActionBar.NavigationMode = (_jobReader.PageList.Count > 0) ? Android.Support.V7.App.ActionBar.NavigationModeTabs : Android.Support.V7.App.ActionBar.NavigationModeStandard;
            UpdateDisplay();
        }

        protected override void OnStart()
        {
            base.OnStart();

            bool firstStart = !_onStartExecuted;
            if (!_onStartExecuted)
            {
                _onStartExecuted = true;
                _activityCommon.RequestUsbPermission(null);
                ReadConfigFile();
                if (_startAlertDialog == null && _currentVersionCode != _lastVersionCode)
                {
                    _startAlertDialog = new AlertDialog.Builder(this)
                        .SetNeutralButton(Resource.String.button_ok, (sender, args) => { })
                        .SetCancelable(true)
                        .SetMessage(Resource.String.version_change_info_message)
                        .SetTitle(Resource.String.version_change_info_title)
                        .Show();
                    _startAlertDialog.DismissEvent += (sender, args) =>
                    {
                        _startAlertDialog = null;
                        HandleStartDialogs(firstStart);
                    };
                    TextView messageView = _startAlertDialog.FindViewById<TextView>(Android.Resource.Id.Message);
                    if (messageView != null)
                    {
                        messageView.MovementMethod = new LinkMovementMethod();
                    }
                }
            }
            _activityStarted = true;
            if (_createTabsPending)
            {
                CreateActionBarTabs();
            }
            if (_startAlertDialog == null)
            {
                HandleStartDialogs(firstStart);
            }
            UpdateDisplay();
        }

        protected override void OnStop()
        {
            base.OnStop();

            _activityStarted = false;
            if (_swDataLog == null)
            {
                StopEdiabasThread(false);
            }
            StoreSettings();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopEdiabasThread(true);
            StoreSettings();
            if (_usbCheckTimer != null)
            {
                _usbCheckTimer.Dispose();
                _usbCheckTimer = null;
            }
            if (_activityCommon != null)
            {
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
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestSelectDevice:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Android.App.Result.Ok)
                    {
                        // Get the device MAC address
                        _deviceName = data.Extras.GetString(DeviceListActivity.ExtraDeviceName);
                        _deviceAddress = data.Extras.GetString(DeviceListActivity.ExtraDeviceAddress);
                        SupportInvalidateOptionsMenu();
                        if (_autoStart)
                        {
                            ButtonConnectClick(_connectButtonInfo.Button, new EventArgs());
                        }
                    }
                    _autoStart = false;
                    break;

                case ActivityRequest.RequestCanAdapterConfig:
                    break;

                case ActivityRequest.RequestSelectConfig:
                    // When FilePickerActivity returns with a file
                    if (resultCode == Android.App.Result.Ok)
                    {
                        _configFileName = data.Extras.GetString(FilePickerActivity.ExtraFileName);
                        ReadConfigFile();
                        SupportInvalidateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestXmlTool:
                    // When XML tool returns with a file
                    if (resultCode == Android.App.Result.Ok)
                    {
                        _configFileName = data.Extras.GetString(XmlToolActivity.ExtraFileName);
                        ReadConfigFile();
                        SupportInvalidateOptionsMenu();
                    }
                    break;

                case ActivityRequest.RequestEdiabasTool:
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
            bool commActive = _ediabasThread != null && _ediabasThread.ThreadRunning();
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

            IMenuItem scanMenu = menu.FindItem(Resource.Id.menu_scan);
            if (scanMenu != null)
            {
                scanMenu.SetTitle(string.Format(Culture, "{0}: {1}", GetString(Resource.String.menu_device), _deviceName));
                scanMenu.SetEnabled(interfaceAvailable && !commActive);
                scanMenu.SetVisible(_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth);
            }

            IMenuItem canAdapterMenu = menu.FindItem(Resource.Id.menu_can_adapter_config);
            if (canAdapterMenu != null)
            {
                canAdapterMenu.SetEnabled(interfaceAvailable && !commActive);
                canAdapterMenu.SetVisible(_activityCommon.AllowCanAdapterConfig(_deviceAddress));
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
            if (xmlToolMenu != null)
            {
                xmlToolMenu.SetEnabled(!commActive);
            }

            IMenuItem ediabasToolMenu = menu.FindItem(Resource.Id.menu_ediabas_tool);
            if (ediabasToolMenu != null)
            {
                ediabasToolMenu.SetEnabled(!commActive);
            }

            IMenuItem downloadEcu = menu.FindItem(Resource.Id.menu_download_ecu);
            if (downloadEcu != null)
            {
                downloadEcu.SetEnabled(!commActive);
            }

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            if (logSubMenu != null)
            {
                logSubMenu.SetEnabled(interfaceAvailable && !commActive);
            }

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_scan:
                    _autoStart = false;
                    _activityCommon.SelectBluetoothDevice((int)ActivityRequest.RequestSelectDevice);
                    return true;

                case Resource.Id.menu_can_adapter_config:
                    CanAdapterConfig();
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
                    DownloadEcuFiles();
                    return true;

                case Resource.Id.menu_submenu_log:
                    SelectDataLogging();
                    return true;

                case Resource.Id.menu_submenu_help:
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://ediabaslib.codeplex.com/wikipage?title=Deep OBD for BMW")));
                    return true;

                case Resource.Id.menu_exit:
                    StopEdiabasThread(true);
                    StoreSettings();
                    System.Environment.Exit(0);
                    break;
            }
            return base.OnOptionsItemSelected(item);
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
                if (!_activityCommon.RequestBluetoothDeviceSelect((int)ActivityRequest.RequestSelectDevice, (s, args) =>
                    {
                        _autoStart = true;
                    }))
                {
                    return;
                }
            }

            if (_ediabasThread != null && _ediabasThread.ThreadRunning())
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
            if (_ediabasThread == null)
            {
                return;
            }
            ToggleButton button = v.FindViewById<ToggleButton>(Resource.Id.button_active);
            _ediabasThread.CommActive = button.Checked;
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

        private bool StartEdiabasThread()
        {
            _autoStart = false;
            try
            {
                if (_ediabasThread == null)
                {
                    _ediabasThread = new EdiabasThread(string.IsNullOrEmpty(_jobReader.EcuPath) ? _ecuPath : _jobReader.EcuPath, _activityCommon.SelectedInterface);
                    _ediabasThread.DataUpdated += DataUpdated;
                    _ediabasThread.ThreadTerminated += ThreadTerminated;
                }
                string logDir = string.Empty;
                if (!string.IsNullOrEmpty(_jobReader.LogPath))
                {
                    logDir = Path.IsPathRooted(_jobReader.LogPath) ? _jobReader.LogPath : Path.Combine(_appDataPath, _jobReader.LogPath);
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

                string traceDir = null;
                if (_traceActive && !string.IsNullOrEmpty(_configFileName))
                {
                    traceDir = logDir;
                }
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
                            if (_activityCommon.Emulator)
                            {   // broadcast is not working with emulator
                                portName = ActivityCommon.EmulatorEnetIp;
                            }
                            break;

                        case ActivityCommon.InterfaceType.Ftdi:
                            portName = "FTDI0";
                            connectParameter = new EdFtdiInterface.ConnectParameter(this, _activityCommon.UsbManager);
                            break;
                    }
                    _ediabasThread.StartThread(portName, connectParameter, traceDir, _traceAppend, pageInfo, true);
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
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
            if (_ediabasThread != null)
            {
                try
                {
                    _ediabasThread.StopThread(wait);
                    if (wait)
                    {
                        _ediabasThread.DataUpdated -= DataUpdated;
                        _ediabasThread.ThreadTerminated -= ThreadTerminated;
                        _ediabasThread.Dispose();
                        _ediabasThread = null;
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
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
                _deviceName = prefs.GetString("DeviceName", string.Empty);
                _deviceAddress = prefs.GetString("DeviceAddress", string.Empty);
                _configFileName = prefs.GetString("ConfigFile", string.Empty);
                _lastVersionCode = prefs.GetInt("VersionCode", -1);
            }
            catch
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
                prefsEdit.PutString("ConfigFile", _configFileName);
                prefsEdit.PutInt("VersionCode", _currentVersionCode);
                prefsEdit.Commit();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void BroadcastReceived(Context context, Intent intent)
        {
            string action = intent.Action;
            switch (action)
            {
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

                case UsbManager.ActionUsbDeviceDetached:
                    {
                        UsbDevice usbDevice = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
                        if (EdFtdiInterface.IsValidUsbDevice(usbDevice))
                        {
                            SupportInvalidateOptionsMenu();
                            UpdateDisplay();
                        }
                        break;
                    }
            }
        }

        private void UsbCheckEvent(Object state)
        {
            if (_usbCheckTimer == null)
            {
                return;
            }
            RunOnUiThread(() =>
            {
                List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_activityCommon.UsbManager);
                if (availableDrivers.Count > _usbDeviceDetectCount)
                {   // device attached
                    _activityCommon.RequestUsbPermission(null);
                    SupportInvalidateOptionsMenu();
                    UpdateDisplay();
                }
                _usbDeviceDetectCount = availableDrivers.Count;
            });
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
            StopEdiabasThread(true);
            UpdateDisplay();
        }

        private JobReader.PageInfo GetSelectedPage()
        {
            JobReader.PageInfo pageInfo = null;
            if (SupportActionBar.SelectedTab != null)
            {
                int index = SupportActionBar.SelectedTab.Position;
                if (index >= 0 && index < (_jobReader.PageList.Count))
                {
                    pageInfo = _jobReader.PageList[index];
                }
            }
            return pageInfo;
        }

        private void UpdateSelectedPage()
        {
            if ((_ediabasThread == null) || !_ediabasThread.ThreadRunning())
            {
                return;
            }

            JobReader.PageInfo newPageInfo = GetSelectedPage();
            if (newPageInfo == null)
            {
                return;
            }
            bool newCommActive = !newPageInfo.JobActivate;
            if (_ediabasThread.JobPageInfo != newPageInfo)
            {
                _ediabasThread.CommActive = newCommActive;
                _ediabasThread.JobPageInfo = newPageInfo;
                CloseDataLog();
            }
        }

        private void UpdateDisplay()
        {
            bool dynamicValid = false;
            bool threadRunning = false;

            _connectButtonInfo.Enabled = true;
            if (_ediabasThread != null && _ediabasThread.ThreadRunning())
            {
                if (_ediabasThread.ThreadStopping())
                {
                    _connectButtonInfo.Enabled = false;
                }
                else
                {
                    threadRunning = true;
                }
                if (_ediabasThread.CommActive)
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
            if (_connectButtonInfo.Button != null) _connectButtonInfo.Button.Enabled = _connectButtonInfo.Checked;
            if (_connectButtonInfo.Button != null) _connectButtonInfo.Button.Enabled = _connectButtonInfo.Enabled;
            _imageBackground.Visibility = dynamicValid ? ViewStates.Invisible : ViewStates.Visible;

            Fragment dynamicFragment = null;
            JobReader.PageInfo pageInfo = GetSelectedPage();
            if (pageInfo != null)
            {
                dynamicFragment = (Fragment)pageInfo.InfoObject;
            }

            if (dynamicFragment != null && dynamicFragment.View != null)
            {
                ListView listViewResult = dynamicFragment.View.FindViewById<ListView>(Resource.Id.resultList);
                if (listViewResult.Adapter == null)
                {
                    listViewResult.Adapter = new ResultListAdapter(this, pageInfo.Weight);
                }
                ResultListAdapter resultListAdapter = (ResultListAdapter)listViewResult.Adapter;
                ToggleButton buttonActive = null;
                if (pageInfo.JobActivate)
                {
                    buttonActive = dynamicFragment.View.FindViewById<ToggleButton>(Resource.Id.button_active);
                }

                if (dynamicValid)
                {
                    if (_dataLogActive && threadRunning && _swDataLog == null &&
                        !string.IsNullOrEmpty(_dataLogDir) && !string.IsNullOrEmpty(pageInfo.LogFile))
                    {
                        try
                        {
                            FileMode fileMode;
                            string fileName = Path.Combine(_dataLogDir, pageInfo.LogFile);
                            if (File.Exists(fileName))
                            {
                                fileMode = _jobReader.AppendLog ? FileMode.Append : FileMode.Create;
                            }
                            else
                            {
                                fileMode = FileMode.Create;
                            }
                            _swDataLog = new StreamWriter(new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    Dictionary<string, EdiabasNet.ResultData> resultDict;
                    lock (EdiabasThread.DataLock)
                    {
                        resultDict = _ediabasThread.EdiabasResultDict;
                    }
                    resultListAdapter.Items.Clear();

                    bool formatResult = false;
                    bool formatErrorResult = false;
                    bool updateResult = false;
                    if (pageInfo.ClassObject != null)
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        formatResult = pageType.GetMethod("FormatResult") != null;
                        formatErrorResult = pageType.GetMethod("FormatErrorResult") != null;
                        updateResult = pageType.GetMethod("UpdateResultList") != null;
                    }
                    string currDateTime = string.Empty;
                    if (_dataLogActive)
                    {
                        currDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", Culture);
                    }

                    if (pageInfo.ErrorsInfo != null)
                    {   // read errors
                        List<EdiabasThread.EdiabasErrorReport> errorReportList;
                        lock (EdiabasThread.DataLock)
                        {
                            errorReportList = _ediabasThread.EdiabasErrorReportList;
                        }
                        if (errorReportList != null)
                        {
                            foreach (EdiabasThread.EdiabasErrorReport errorReport in errorReportList)
                            {
                                string message = string.Format(Culture, "{0}: ",
                                    GetPageString(pageInfo, errorReport.EcuName));
                                if (errorReport.ErrorDict == null)
                                {
                                    message += GetString(Resource.String.error_no_response);
                                }
                                else
                                {
                                    message += "\r\n";
                                    message += FormatResultString(errorReport.ErrorDict, "F_ORT_TEXT", "{0}");
                                    message += ", ";
                                    message += FormatResultString(errorReport.ErrorDict, "F_VORHANDEN_TEXT", "{0}");
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
                                        message += "\r\n" + detailText;
                                    }
                                }
                                if (formatErrorResult)
                                {
                                    try
                                    {
                                        message = pageInfo.ClassObject.FormatErrorResult(pageInfo, errorReport, message);
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }

                                if (!string.IsNullOrEmpty(message))
                                {
                                    resultListAdapter.Items.Add(new TableResultItem(message, null));
                                }
                            }
                            if (resultListAdapter.Items.Count == 0)
                            {
                                resultListAdapter.Items.Add(
                                    new TableResultItem(GetString(Resource.String.error_no_error), null));
                            }
                        }
                    }
                    else
                    {
                        foreach (JobReader.DisplayInfo displayInfo in pageInfo.DisplayList)
                        {
                            string result = string.Empty;
                            if (displayInfo.Format == null)
                            {
                                if (resultDict != null)
                                {
                                    try
                                    {
                                        if (formatResult)
                                        {
                                            result = pageInfo.ClassObject.FormatResult(pageInfo, resultDict,
                                                displayInfo.Result);
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
                                resultListAdapter.Items.Add(new TableResultItem(GetPageString(pageInfo, displayInfo.Name), result));
                                if (!string.IsNullOrEmpty(displayInfo.LogTag) && _dataLogActive && _swDataLog != null)
                                {
                                    try
                                    {
                                        _swDataLog.Write("{0}\t{1}\t{2}\r\n", displayInfo.LogTag, currDateTime, result);
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                            }
                        }
                    }

                    if (updateResult)
                    {
                        pageInfo.ClassObject.UpdateResultList(pageInfo, resultDict, resultListAdapter);
                    }

                    resultListAdapter.NotifyDataSetChanged();
                }
                else
                {
                    resultListAdapter.Items.Clear();
                    resultListAdapter.NotifyDataSetChanged();
                }

                if (pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = pageInfo.ClassObject.GetType();
                        if (pageType.GetMethod("UpdateLayout") != null)
                        {
                            pageInfo.ClassObject.UpdateLayout(pageInfo, dynamicValid, _ediabasThread != null);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (buttonActive != null)
                {
                    if (_ediabasThread != null && _ediabasThread.ThreadRunning())
                    {
                        buttonActive.Enabled = true;
                        buttonActive.Checked = _ediabasThread.CommActive;
                    }
                    else
                    {
                        buttonActive.Enabled = false;
                        buttonActive.Checked = false;
                    }
                }
            }
        }

        public static String FormatResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            double value = GetResultDouble(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            Int64 value = GetResultInt64(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            bool found;
            string value = GetResultString(resultDict, dataName, out found);
            if (found)
            {
                return string.Format(Culture, format, value);
            }
            return string.Empty;
        }

        public static String FormatResultEdiabas(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            string result = string.Empty;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
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
            }
            return result;
        }

        public static Int64 GetResultInt64(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.OpData is Int64)
                {
                    found = true;
                    return (Int64)resultData.OpData;
                }
            }
            return 0;
        }

        public static Double GetResultDouble(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                if (resultData.OpData is Double)
                {
                    found = true;
                    return (Double)resultData.OpData;
                }
            }
            return 0;
        }

        public static String GetResultString(Dictionary<string, EdiabasNet.ResultData> resultDict, string dataName, out bool found)
        {
            found = false;
            EdiabasNet.ResultData resultData;
            if (resultDict != null && resultDict.TryGetValue(dataName, out resultData))
            {
                // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                if (resultData.OpData is String)
                {
                    found = true;
                    return (String)resultData.OpData;
                }
            }
            return string.Empty;
        }

        public static String GetPageString(JobReader.PageInfo pageInfo, string name)
        {
            string lang = CultureInfo.CurrentUICulture.Name;
            string langIso = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
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
            _jobReader.ReadXml(_configFileName);
            _activityCommon.SelectedInterface = (_jobReader.PageList.Count > 0) ? _jobReader.Interface : ActivityCommon.InterfaceType.None;
            RequestConfigSelect();
            CompileCode();
        }

        private void CompileCode()
        {
            if (_jobReader.PageList.Count == 0)
            {
                _updateHandler.Post(CreateActionBarTabs);
                return;
            }
            Android.App.ProgressDialog progress = new Android.App.ProgressDialog(this);
            progress.SetCancelable(false);
            progress.SetMessage(GetString(Resource.String.compile_start));
            progress.Show();

            Thread compileThreadWrapper = new Thread(() =>
            {
                List<string> compileResultList = new List<string>();
                List<Thread> threadList = new List<Thread>();
                foreach (JobReader.PageInfo pageInfo in _jobReader.PageList)
                {
                    if (pageInfo.ClassCode == null) continue;
                    // limit number of active tasks
                    for (; ; )
                    {
                        int activeThreads = threadList.Count(thread => thread.IsAlive);
                        if (activeThreads < 4)
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
                                using BmwDiagnostics;
                                using System;
                                using System.Collections.Generic;
                                using System.Diagnostics;
                                using System.Threading;"
                                + infoLocal.ClassCode;
                            evaluator.Compile(classCode);
                            infoLocal.ClassObject = evaluator.Evaluate("new PageClass()");
                            if (((infoLocal.JobsInfo == null) || (infoLocal.JobsInfo.JobList.Count == 0)) &&
                                ((infoLocal.ErrorsInfo == null) || (infoLocal.ErrorsInfo.EcuList.Count == 0)))
                            {
                                Type pageType = infoLocal.ClassObject.GetType();
                                if (pageType.GetMethod("ExecuteJob") == null)
                                {
                                    throw new Exception("No ExecuteJob method");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
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
                    RunOnUiThread(() => _activityCommon.ShowAlert(result));
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

        private void DownloadFile(string url, string fileName, string unzipTargetDir = null)
        {
            string dirName = Path.GetDirectoryName(fileName);
            if (dirName == null)
            {
                _activityCommon.ShowAlert(GetString(Resource.String.download_failed));
                return;
            }
            try
            {
                Directory.CreateDirectory(dirName);
            }
            catch (Exception)
            {
                _activityCommon.ShowAlert(GetString(Resource.String.download_failed));
                return;
            }

            _downloadProgress = new Android.App.ProgressDialog(this);
            _downloadProgress.DismissEvent += (sender, args) => { _downloadProgress = null; };
            _downloadProgress.SetCancelable(true);
            _downloadProgress.SetMessage(GetString(Resource.String.downloading_file));
            _downloadProgress.SetProgressStyle(Android.App.ProgressDialogStyle.Horizontal);
            _downloadProgress.Progress = 0;
            _downloadProgress.Max = 100;
            _downloadProgress.CancelEvent += DownloadProgressCancel;
            _downloadProgress.Show();

            Thread downloadThread = new Thread(() =>
            {
                try
                {
                    UnzipInfo unzipInfo = null;
                    if (!string.IsNullOrEmpty(unzipTargetDir))
                    {
                        XElement xmlInfo = new XElement("Info");
                        xmlInfo.Add(new XAttribute("Url", url));
                        xmlInfo.Add(new XAttribute("Name", Path.GetFileName(url)??string.Empty));
                        unzipInfo = new UnzipInfo(fileName, unzipTargetDir, xmlInfo);
                    }
                    // ReSharper disable once RedundantNameQualifier
                    _webClient.DownloadFileAsync(new System.Uri(url), fileName, unzipInfo);
                }
                catch (Exception)
                {
                    RunOnUiThread(() =>
                    {
                        _downloadProgress.Hide();
                        _downloadProgress.Dispose();
                        _downloadProgress = null;
                        _activityCommon.ShowAlert(GetString(Resource.String.download_failed));
                    });
                }
            });
            downloadThread.Start();
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_downloadProgress != null)
                {
                    _downloadProgress.SetCancelable(false);
                    if (e.Error == null)
                    {
                        UnzipInfo unzipInfo = e.UserState as UnzipInfo;
                        if (unzipInfo != null)
                        {
                            _downloadProgress.CancelEvent -= DownloadProgressCancel;
                            ExtractZipFile(unzipInfo.FileName, unzipInfo.TargetDir, unzipInfo.InfoXml, true);
                        }
                    }
                    else
                    {
                        _downloadProgress.Hide();
                        _downloadProgress.Dispose();
                        _downloadProgress = null;
                        if (!e.Cancelled && e.Error != null)
                        {
                            _activityCommon.ShowAlert(GetString(Resource.String.download_failed));
                        }
                    }
                }
            });
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (_downloadProgress != null)
                {
                    _downloadProgress.Progress = e.ProgressPercentage;
                }
            });
        }

        private void DownloadProgressCancel(object sender, EventArgs e)
        {
            if (_webClient.IsBusy)
            {
                _webClient.CancelAsync();
            }
        }

        private void ExtractZipFile(string fileName, string targetDirectory, XElement infoXml, bool removeFile = false)
        {
            try
            {
                if (Directory.Exists(targetDirectory))
                {
                    Directory.Delete(targetDirectory, true);
                }
                Directory.CreateDirectory(targetDirectory);
            }
            catch (Exception)
            {
                if (_downloadProgress != null)
                {
                    _downloadProgress.Hide();
                    _downloadProgress.Dispose();
                    _downloadProgress = null;
                }
                _activityCommon.ShowAlert(GetString(Resource.String.extract_failed));
                return;
            }

            bool extractCanceled = false;
            if (_downloadProgress == null)
            {
                _downloadProgress = new Android.App.ProgressDialog(this);
                _downloadProgress.DismissEvent += (sender, args) => { _downloadProgress = null; };
            }
            _downloadProgress.SetCancelable(true);
            _downloadProgress.SetMessage(GetString(Resource.String.extract_file));
            _downloadProgress.SetProgressStyle(Android.App.ProgressDialogStyle.Horizontal);
            _downloadProgress.Progress = 0;
            _downloadProgress.Max = 100;
            _downloadProgress.CancelEvent += (sender, args) => extractCanceled = true;
            _downloadProgress.Show();

            Thread extractThread = new Thread(() =>
            {
                bool extractFailed = false;
                try
                {
                    ActivityCommon.ExtractZipFile(fileName, targetDirectory,
                        percent =>
                        {
                            RunOnUiThread(() =>
                            {
                                _downloadProgress.Progress = percent;
                            });
                            return extractCanceled;
                        });
                    if (removeFile)
                    {
                        File.Delete(fileName);
                    }
                    if (infoXml != null)
                    {
                        infoXml.Save(Path.Combine(targetDirectory, InfoXmlName));
                    }
                }
                catch (Exception)
                {
                    extractFailed = true;
                }
                RunOnUiThread(() =>
                {
                    _downloadProgress.Hide();
                    _downloadProgress.Dispose();
                    _downloadProgress = null;
                    if (extractFailed)
                    {
                        _activityCommon.ShowAlert(GetString(Resource.String.extract_failed));
                    }
                });
            });
            extractThread.Start();
        }

        private void DownloadEcuFiles()
        {
            string ecuPath = Path.Combine(_appDataPath, EcuDirName);
            try
            {
                ActivityCommon.FileSystemBlockInfo blockInfo = ActivityCommon.GetFileSystemBlockInfo(_appDataPath);
                long ecuDirSize = ActivityCommon.GetDirectorySize(ecuPath);
                double freeSpace = blockInfo.AvailableSizeBytes + ecuDirSize;
                if (freeSpace < 1500000000)
                {
                    _activityCommon.ShowAlert(GetString(Resource.String.ecu_download_free_space));
                    return;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            DownloadFile(EcuDownloadUrl, Path.Combine(_appDataPath, "Download", "Ecu.zip"), ecuPath);
        }

        private bool CheckForEcuFiles(bool checkPackage = false)
        {
            if (_downloadEcuAlertDialog != null)
            {
                return true;
            }

            if (!ValidEcuFiles(_ecuPath))
            {
                string message = GetString(Resource.String.ecu_not_found) + "\n" + GetString(Resource.String.ecu_download);

                _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        DownloadEcuFiles();
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetMessage(message)
                    .SetTitle(Resource.String.ecu_download_title)
                    .Show();
                _downloadEcuAlertDialog.DismissEvent += (sender, args) => { _downloadEcuAlertDialog = null; };
                return false;
            }

            if (checkPackage && !_userEcuFiles)
            {
                if (!ValidEcuPackage(_ecuPath))
                {
                    string message = GetString(Resource.String.ecu_package) + "\n" + GetString(Resource.String.ecu_download);

                    _downloadEcuAlertDialog = new AlertDialog.Builder(this)
                        .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                        {
                            DownloadEcuFiles();
                        })
                        .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                        {
                        })
                        .SetMessage(message)
                        .SetTitle(Resource.String.ecu_download_title)
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
                if (xmlInfo.Root == null)
                {
                    return false;
                }
                XAttribute nameAttr = xmlInfo.Root.Attribute("Name");
                if (nameAttr == null)
                {
                    return false;
                }
                if (string.Compare(nameAttr.Value, Path.GetFileName(EcuDownloadUrl), StringComparison.OrdinalIgnoreCase) != 0)
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

        private void RequestConfigSelect()
        {
            if (_jobReader.PageList.Count > 0)
            {
                return;
            }
            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_select, (sender, args) =>
                {
                    SelectConfigFile();
                })
                .SetNegativeButton(Resource.String.button_generate, (sender, args) =>
                {
                    StartXmlTool();
                })
                .SetCancelable(true)
                .SetMessage(Resource.String.config_select)
                .SetTitle(Resource.String.config_select_title)
                .Show();
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
            listView.SetItemChecked(0, _traceActive);
            listView.SetItemChecked(1, _traceAppend);
            listView.SetItemChecked(2, _dataLogActive);

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

        private void CanAdapterConfig()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(CanAdapterActivity));
            serverIntent.PutExtra(CanAdapterActivity.ExtraDeviceAddress, _deviceAddress);
            serverIntent.PutExtra(CanAdapterActivity.ExtraInterfaceType, (int)_activityCommon.SelectedInterface);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestCanAdapterConfig);
        }

        private void SelectConfigFile()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            // Launch the FilePickerActivity to select a configuration
            Intent serverIntent = new Intent(this, typeof(FilePickerActivity));
            string initDir = _activityCommon.ExternalPath;
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
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestXmlTool);
        }

        private void StartEdiabasTool()
        {
            if (!CheckForEcuFiles())
            {
                return;
            }
            Intent serverIntent = new Intent(this, typeof(EdiabasToolActivity));
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInitDir, _ecuPath);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraAppDataDir, _appDataPath);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraInterface, (int)_activityCommon.SelectedInterface);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceName, _deviceName);
            serverIntent.PutExtra(EdiabasToolActivity.ExtraDeviceAddress, _deviceAddress);
            StartActivityForResult(serverIntent, (int)ActivityRequest.RequestEdiabasTool);
        }

        public class TabContentFragment : Fragment
        {
            private readonly ActivityMain _activity;
            private readonly int _resourceId;
            private readonly JobReader.PageInfo _pageInfo;

            public TabContentFragment(ActivityMain activity, int resourceId, JobReader.PageInfo pageInfo)
            {
                _activity = activity;
                _resourceId = resourceId;
                _pageInfo = pageInfo;
            }

            public TabContentFragment(ActivityMain activity, int resourceId)
                : this(activity, resourceId, null)
            {
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                View view = inflater.Inflate(_resourceId, null);
                if (_pageInfo != null && _pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = _pageInfo.ClassObject.GetType();
                        if (pageType.GetMethod("CreateLayout") != null)
                        {
                            LinearLayout pageLayout = view.FindViewById<LinearLayout>(Resource.Id.listLayout);
                            _pageInfo.ClassObject.CreateLayout(_activity, _pageInfo, pageLayout);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                _activity._updateHandler.Post(() =>
                {
                    _activity.UpdateDisplay();
                });
                return view;
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();

                if (_pageInfo != null && _pageInfo.ClassObject != null)
                {
                    try
                    {
                        Type pageType = _pageInfo.ClassObject.GetType();
                        if (pageType.GetMethod("DestroyLayout") != null)
                        {
                            _pageInfo.ClassObject.DestroyLayout(_pageInfo);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
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
