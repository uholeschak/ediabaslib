using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.Net.Http;
using Android.OS;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.WebKit;
using Android.Webkit;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content.PM;
using BmwDeepObd.Dialogs;
using EdiabasLib;
using Java.Interop;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/bmw_coding_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(BmwCodingActivity),
        LaunchMode = LaunchMode.SingleInstance,
        AlwaysRetainTaskState = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class BmwCodingActivity : BaseActivity
    {
        private class VehicleRequest
        {
            public enum VehicleRequestType
            {
                Connect,
                Disconnect,
                Transmit
            }

            public VehicleRequest(VehicleRequestType requestType, string sessionId, string id, string data = null)
            {
                RequestType = requestType;
                SessionId = sessionId;
                Id = id;
                Data = data;
            }

            public VehicleRequestType RequestType { get; }
            public string SessionId { get; }
            public string Id { get; }
            public string Data { get; }
        }

        public class InstanceData
        {
            public InstanceData()
            {
                CodingUrl = string.Empty;
                CodingUrlTest = string.Empty;
                DayString = string.Empty;
                ValidSerial = string.Empty;
                InitialUrl = string.Empty;
                Url = string.Empty;
                ServerConnected = false;
                ConnectTimeouts = 0;
                TraceDir = string.Empty;
                TraceActive = true;
                TraceAppend = false;
                CommErrorsOccurred = false;
                SslErrorShown = false;
            }

            public string CodingUrl { get; set; }
            public string CodingUrlTest { get; set; }
            public string DayString { get; set; }
            public string ValidSerial { get; set; }
            public string InitialUrl { get; set; }
            public string Url { get; set; }
            public bool ServerConnected { get; set; }
            public int ConnectTimeouts { get; set; }
            public string TraceDir { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool CommErrorsOccurred { get; set; }
            public bool SslErrorShown { get; set; }
        }

        public delegate void AcceptDelegate(bool accepted);
        public delegate void InfoCheckDelegate(bool success, bool cancelled, string codingUrl = null, string codingUrlTest = null, string message = null, string dayString = null, string validSerial = null);

        private const int FirstConnectTimeout = 20000;
        private const int ConnectionTimeout = 6000;

        private const string AuthUser = "DeepObd";
        private const string AuthPwd = "BmwCoding";

        // Intent extra
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";
        public const string ExtraElmWifiIp = "elmwifi_ip";
        public const string ExtraDeepObdWifiIp = "deepobdwifi_ip";

        private const string InfoCodingUrl = @"https://www.holeschak.de/BmwDeepObd/BmwCoding.php";
#if DEBUG
        private static readonly string Tag = typeof(BmwCodingActivity).FullName;
#endif

        private enum ActivityRequest
        {
            RequestOpenExternalFile,
        }

        private InstanceData _instanceData = new InstanceData();
        private ActivityCommon _activityCommon;
        private string _ecuDir;
        private string _appDataDir;
        private string _deviceAddress;
        private Handler _updateHandler;
        private Java.Lang.Runnable _startRunnable;
        private EdiabasNet _ediabas;
        private volatile bool _ediabasJobAbort;
        private Thread _ediabasThread;
        private AutoResetEvent _ediabasThreadWakeEvent;
        private object _ediabasThreadLock = new object();
        private object _ediabasLock = new object();
        private object _requestLock = new object();
        private object _timeLock = new object();
        private object _instanceLock = new object();
        private Queue<VehicleRequest> _requestQueue = new Queue<VehicleRequest>();
        private bool _activityActive;
        private HttpClient _infoHttpClient;
        private bool _urlLoaded;
        private AlertDialog _alertDialogInfo;
        private AlertDialog _alertDialogConnectError;
        private Timer _connectionCheckTimer;
        public long _connectionUpdateTime;

        private WebView _webViewCoding;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme();
            base.OnCreate(savedInstanceState);
            _allowTitleHiding = false;

            if (savedInstanceState != null)
            {
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SetContentView(Resource.Layout.bmw_coding);

            SetResult(Android.App.Result.Ok);

            _ediabasThreadWakeEvent = new AutoResetEvent(false);
            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                }
            }, BroadcastReceived);

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);

            _activityCommon.SetPreferredNetworkInterface();

            _updateHandler = new Handler(Looper.MainLooper);
            _startRunnable = new Java.Lang.Runnable(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                StartEdiabasThread();
            });

            _webViewCoding = FindViewById<WebView>(Resource.Id.webViewCoding);
            try
            {
                ProxyConfig proxyConfig = new ProxyConfig.Builder().Build();
                ProxyController.Instance.ClearProxyOverride(new ProxyExecutor(), new Java.Lang.Runnable(() => { }));
                ProxyController.Instance.SetProxyOverride(proxyConfig, new ProxyExecutor(), new Java.Lang.Runnable(() => { }));
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                WebSettings webSettings = _webViewCoding?.Settings;
                if (webSettings != null)
                {
                    webSettings.JavaScriptEnabled = true;
                    webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                    webSettings.DomStorageEnabled = true;
                    webSettings.BuiltInZoomControls = true;
                    webSettings.LoadWithOverviewMode = true;
                    webSettings.UseWideViewPort = true;
                    webSettings.CacheMode = CacheModes.NoCache;

                    string userAgent = webSettings.UserAgentString;
                    if (!string.IsNullOrEmpty(userAgent))
                    {
                        long packageVersion = _activityCommon.VersionCode;
                        string language = _activityCommon.GetCurrentLanguage();
                        string userAgentAppend = string.Format(CultureInfo.InvariantCulture, " DeepObd/{0}/{1}", packageVersion, language);
                        userAgent += userAgentAppend;

                        webSettings.UserAgentString = userAgent;
                    }
                }

                _webViewCoding.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
                _webViewCoding.ScrollbarFadingEnabled = false;
                _webViewCoding.AddJavascriptInterface(new WebViewJSInterface(this), "app");
                _webViewCoding.SetWebViewClient(new WebViewClientImpl(this));
                _webViewCoding.SetWebChromeClient(new WebChromeClientImpl(this));
            }
            catch (Exception)
            {
                // ignored
            }

            lock (_ediabasLock)
            {
                UpdateLogInfo();
            }

            UpdateConnectTime();
            UpdateOptionsMenu();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
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

                GetConnectionInfoRequest();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _activityActive = true;
            UpdateConnectTime();

            if (_connectionCheckTimer == null)
            {
                _connectionCheckTimer = new Timer(state =>
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        LoadWebServerUrl();

                        if (!_urlLoaded)
                        {
                            return;
                        }

                        bool serverConnected;
                        lock (_instanceLock)
                        {
                            serverConnected = _instanceData.ServerConnected;
                        }

                        long connectTime = GetConnectTime();
                        long timeout = serverConnected ? ConnectionTimeout : FirstConnectTimeout;

                        if (Stopwatch.GetTimestamp() - connectTime >= timeout * ActivityCommon.TickResolMs)
                        {
                            if (!serverConnected)
                            {
                                ConnectionFailMessage();
                            }
                            else
                            {
                                try
                                {
                                    string loadUrl;
                                    lock (_instanceLock)
                                    {
                                        loadUrl = _instanceData.Url;
                                        _instanceData.ConnectTimeouts++;
                                    }

                                    UpdateConnectTime();
                                    Toast.MakeText(this, GetString(Resource.String.bmw_coding_network_error), ToastLength.Short)?.Show();
                                    if (_activityCommon.IsNetworkPresent(out _))
                                    {
                                        _activityCommon.SetPreferredNetworkInterface();
                                        _webViewCoding.LoadUrl(loadUrl);
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }
                    });
                }, null, 500, 500);
            }

        }

        protected override void OnPause()
        {
            base.OnPause();
            _activityActive = false;

            DisposeTimer();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (_activityCommon != null && _activityCommon.MtcBtService)
            {
                _activityCommon.StopMtcService();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

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

            StopEdiabasThread();

            if (_infoHttpClient != null)
            {
                try
                {
                    _infoHttpClient.CancelPendingRequests();
                    _infoHttpClient.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
                _infoHttpClient = null;
            }

            DisposeTimer();
            if (_activityCommon != null)
            {
                _activityCommon.Dispose();
                _activityCommon = null;
            }

            if (_ediabasThreadWakeEvent != null)
            {
                _ediabasThreadWakeEvent.Dispose();
                _ediabasThreadWakeEvent = null;
            }
        }

        public override void OnBackPressedEvent()
        {
            ConnectionActiveWarn(accepted =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                if (accepted)
                {
                    OnBackPressedContinue();
                }
            });
        }

        private void OnBackPressedContinue()
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

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    ConnectionActiveWarn(accepted =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        if (accepted)
                        {
                            FinishContinue();
                        }
                    });
                    return true;

                case Resource.Id.menu_submenu_log:
                    if (IsEdiabasConnected())
                    {
                        return true;
                    }
                    SelectDataLogging();
                    return true;

                case Resource.Id.menu_send_trace:
                    if (IsEdiabasConnected())
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
                    if (IsEdiabasConnected())
                    {
                        return true;
                    }
                    OpenTraceFile();
                    return true;

                case Resource.Id.menu_submenu_help:
                    _activityCommon.ShowWifiConnectedWarning(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _activityCommon.OpenWebUrl("https://uholeschak.github.io/ediabaslib/docs/BMW_Coding.html");
                    });
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FinishContinue()
        {
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
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestOpenExternalFile:
                    UpdateOptionsMenu();
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.bmw_coding_menu, menu);
            return true;
        }

        public override void PrepareOptionsMenu(IMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            bool commActive = IsEdiabasConnected();
            bool interfaceAvailable = _activityCommon.IsInterfaceAvailable(null, true);

            IMenuItem logSubMenu = menu.FindItem(Resource.Id.menu_submenu_log);
            logSubMenu?.SetEnabled(interfaceAvailable && !commActive);

            IMenuItem traceSubmenu = menu.FindItem(Resource.Id.menu_trace_submenu);
            traceSubmenu?.SetEnabled(!commActive);

            bool tracePresent = ActivityCommon.IsTraceFilePresent(_instanceData.TraceDir);
            IMenuItem sendTraceMenu = menu.FindItem(Resource.Id.menu_send_trace);
            sendTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);

            IMenuItem openTraceMenu = menu.FindItem(Resource.Id.menu_open_trace);
            openTraceMenu?.SetEnabled(interfaceAvailable && !commActive && _instanceData.TraceActive && tracePresent);
        }

        private void DisposeTimer()
        {
            if (_connectionCheckTimer != null)
            {
                _connectionCheckTimer.Dispose();
                _connectionCheckTimer = null;
            }
        }

        private void ConnectionActiveWarn(AcceptDelegate handler)
        {
            if (!IsEdiabasConnected())
            {
                handler.Invoke(true);
                return;
            }

            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    _instanceData.CommErrorsOccurred = true;
                    handler(true);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    handler(false);
                })
                .SetMessage(Resource.String.bmw_coding_connection_active)
                .SetTitle(Resource.String.alert_title_warning)
                .Show();
        }

        private void ConnectionFailMessage()
        {
            if (_alertDialogConnectError != null)
            {
                return;
            }

            bool ignoreDismiss = false;
            _alertDialogConnectError = new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    if (_activityCommon.IsNetworkPresent(out _))
                    {
                        _activityCommon.SetPreferredNetworkInterface();
                        _webViewCoding.LoadUrl(_instanceData.Url);
                    }

                    ignoreDismiss = true;
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetMessage(Resource.String.bmw_coding_connect_url_failed)
                .SetTitle(Resource.String.alert_title_error)
                .Show();

            if (_alertDialogConnectError != null)
            {
                _alertDialogConnectError.DismissEvent += (sender, args) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _alertDialogConnectError = null;
                    UpdateConnectTime();
                    if (!ignoreDismiss)
                    {
                        Finish();
                    }
                };
            }
        }

        public bool GetConnectionInfoRequest()
        {
            if (_alertDialogInfo != null)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(_instanceData.CodingUrl))
            {
                return true;
            }

            try
            {
                bool ignoreDismiss = false;
                string infoMessage = GetString(Resource.String.bmw_coding_connect_request).Replace("\n", "<br>"); ;
                _alertDialogInfo = new AlertDialog.Builder(this)
                    .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        ignoreDismiss = true;
                        bool infoResult = GetConnectionInfo((success, cancelled, url, urlTest, message, dayString, validSerial) =>
                        {
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                if (cancelled)
                                {
                                    Finish();
                                }

                                if (success && !string.IsNullOrEmpty(url))
                                {
                                    lock (_instanceLock)
                                    {
                                        _instanceData.CodingUrl = url;
                                        _instanceData.CodingUrlTest = urlTest;
                                        _instanceData.DayString = dayString;
                                        _instanceData.ValidSerial = validSerial;
                                    }

                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        new AlertDialog.Builder(this)
                                            .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                                            {
                                            })
                                            .SetCancelable(true)
                                            .SetMessage(message)
                                            .SetTitle(Resource.String.alert_title_info)
                                            .Show();
                                    }
                                    return;
                                }

                                string errorMessage = message;
                                if (string.IsNullOrEmpty(errorMessage))
                                {
                                    errorMessage = GetString(Resource.String.bmw_coding_connect_failed);
                                }

                                AlertDialog alertDialogError = new AlertDialog.Builder(this)
                                    .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                                    {
                                    })
                                    .SetCancelable(true)
                                    .SetMessage(errorMessage)
                                    .SetTitle(Resource.String.alert_title_error)
                                    .Show();
                                if (alertDialogError != null)
                                {
                                    alertDialogError.DismissEvent += (sender, args) =>
                                    {
                                        if (_activityCommon == null)
                                        {
                                            return;
                                        }

                                        Finish();
                                    };

                                }
                            });
                        });

                        if (!infoResult)
                        {
                            Finish();
                        }
                    })
                    .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                    {
                    })
                    .SetMessage(ActivityCommon.FromHtml(infoMessage))
                    .SetTitle(Resource.String.alert_title_info)
                    .Show();
                if (_alertDialogInfo != null)
                {
                    _alertDialogInfo.DismissEvent += (sender, args) =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        _alertDialogInfo = null;
                        if (!ignoreDismiss)
                        {
                            Finish();
                        }
                    };

                    TextView messageView = _alertDialogInfo.FindViewById<TextView>(Android.Resource.Id.Message);
                    if (messageView != null)
                    {
                        messageView.MovementMethod = new LinkMovementMethod();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool GetConnectionInfo(InfoCheckDelegate handler)
        {
            if (handler == null)
            {
                return false;
            }

            if (_activityCommon.SelectedInterface == ActivityCommon.InterfaceType.Bluetooth)
            {
                if (_activityCommon.MtcBtService)
                {
                    string message = GetString(Resource.String.bmw_coding_mtc_reject);
                    handler.Invoke(false, false, null, null, message);
                    return true;
                }
            }

            if (_activityCommon.IsElmDevice(_deviceAddress))
            {
                string message = GetString(Resource.String.bmw_coding_elm_reject);
                handler.Invoke(false, false, null, null, message);
                return true;
            }

            if (_infoHttpClient == null)
            {
                _infoHttpClient = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = ActivityCommon.DefaultSslProtocols,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    Proxy = ActivityCommon.GetProxySettings()
                });
            }

            CustomProgressDialog progress = new CustomProgressDialog(this);
            progress.SetMessage(GetString(Resource.String.bmw_coding_connecting));
            progress.ButtonAbort.Enabled = false;
            progress.Show();
            _activityCommon.SetLock(ActivityCommon.LockTypeCommunication);
            _activityCommon.SetPreferredNetworkInterface();

            Thread sendThread = new Thread(() =>
            {
                try
                {
                    MultipartFormDataContent formInfo = new MultipartFormDataContent
                    {
                        { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", _activityCommon.VersionCode)), "app_ver" },
                        { new StringContent(ActivityCommon.AppId), "app_id" },
                        { new StringContent(_activityCommon.GetCurrentLanguage()), "lang" },
                        { new StringContent(string.Format(CultureInfo.InvariantCulture, "{0}", (long) Build.VERSION.SdkInt)), "android_ver" },
                        { new StringContent(_activityCommon.SelectedInterface.ToDescriptionString()), "interface_type" },
                        { new StringContent(ActivityCommon.LastAdapterSerial ?? string.Empty), "adapter_serial" },
                    };

                    System.Threading.Tasks.Task<HttpResponseMessage> taskDownload = _infoHttpClient.PostAsync(InfoCodingUrl, formInfo);

                    CustomProgressDialog progressLocal = progress;
                    RunOnUiThread(() =>
                    {
                        if (progressLocal != null)
                        {
                            progressLocal.AbortClick = sender =>
                            {
                                try
                                {
                                    _infoHttpClient.CancelPendingRequests();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            };
                            progressLocal.ButtonAbort.Enabled = true;
                        }
                    });

                    HttpResponseMessage responseUpload = taskDownload.Result;
                    responseUpload.EnsureSuccessStatusCode();
                    string responseInfoXml = responseUpload.Content.ReadAsStringAsync().Result;
                    bool success = GetCodingInfo(responseInfoXml, out string codingUrl, out string codingUrlTest, out string message, out string dayString, out string validSerial);
                    handler?.Invoke(success, false, codingUrl, codingUrlTest, message, dayString, validSerial);

                    if (progress != null)
                    {
                        progress.Dismiss();
                        progress = null;
                        _activityCommon.SetLock(ActivityCommon.LockType.None);
                    }
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        if (progress != null)
                        {
                            progress.Dismiss();
                            progress = null;
                            _activityCommon.SetLock(ActivityCommon.LockType.None);
                        }

                        bool cancelled = ex.InnerException is System.Threading.Tasks.TaskCanceledException;
                        handler?.Invoke(false, cancelled);
                    });
                }
            });

            sendThread.Start();
            return true;
        }

        private bool GetCodingInfo(string xmlResult, out string codingUrl, out string codingUrlTest, out string message, out string dayString, out string validSerial)
        {
            codingUrl = null;
            codingUrlTest = null;
            message = null;
            dayString = null;
            validSerial = null;

            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return false;
                }

                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return false;
                }

                bool success = false;
                XElement infoNode = xmlDoc.Root.Element("info");
                if (infoNode != null)
                {
                    XAttribute urlAttr = infoNode.Attribute("url");
                    if (urlAttr != null && !string.IsNullOrEmpty(urlAttr.Value))
                    {
                        codingUrl = urlAttr.Value;
                        success = true;
                    }

                    XAttribute urlTestAttr = infoNode.Attribute("url_test");
                    if (urlTestAttr != null && !string.IsNullOrEmpty(urlTestAttr.Value))
                    {
                        codingUrlTest = urlTestAttr.Value;
                    }

                    XAttribute messageAttr = infoNode.Attribute("message");
                    if (messageAttr != null && !string.IsNullOrEmpty(messageAttr.Value))
                    {
                        message = messageAttr.Value;
                        success = true;
                    }

                    XAttribute dayAttr = infoNode.Attribute("day");
                    if (dayAttr != null && !string.IsNullOrEmpty(dayAttr.Value))
                    {
                        dayString = dayAttr.Value;
                    }
                }

                XElement serialInfoNode = xmlDoc.Root?.Element("serial_info");
                if (serialInfoNode != null)
                {
                    string serial = string.Empty;
                    XAttribute serialAttr = serialInfoNode.Attribute("serial");
                    if (serialAttr != null)
                    {
                        serial = serialAttr.Value;
                    }

                    string oem = string.Empty;
                    XAttribute oemAttr = serialInfoNode.Attribute("oem");
                    if (oemAttr != null)
                    {
                        oem = oemAttr.Value;
                    }

                    bool disabled = false;
                    XAttribute disabledAttr = serialInfoNode.Attribute("disabled");
                    if (disabledAttr != null)
                    {
                        try
                        {
                            disabled = XmlConvert.ToBoolean(disabledAttr.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    if (!string.IsNullOrEmpty(serial) && !string.IsNullOrEmpty(oem) && !disabled)
                    {
                        if (string.Compare(ActivityCommon.LastAdapterSerial, serial, StringComparison.Ordinal) == 0)
                        {
                            validSerial = serial;
                        }
                    }
                }

                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SendVehicleResponseThread(string id, string response)
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                SendVehicleResponse(id, response);
            });
        }

        private void LoadWebServerUrl()
        {
            if (string.IsNullOrEmpty(_instanceData.CodingUrl))
            {
                return;
            }

            if (_urlLoaded)
            {
                return;
            }

            if (!_activityCommon.IsNetworkPresent(out string domains))
            {
                return;
            }

            try
            {
                string loadUrl;
                lock (_instanceLock)
                {
                    if (string.IsNullOrEmpty(_instanceData.Url))
                    {
                        string url;
                        if (!string.IsNullOrEmpty(domains) && domains.Contains("local.holeschak.de", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(_instanceData.CodingUrlTest))
                            {
                                url = _instanceData.CodingUrlTest;
                            }
                            else
                            {
                                //url = @"http://ulrich3.local.holeschak.de:3000";
                                url = @"https://ulrich3.local.holeschak.de:8443";
                                //url = @"http://coding-server.local.holeschak.de:8008";
                            }
                        }
                        else
                        {
                            url = _instanceData.CodingUrl;
                        }

                        _instanceData.InitialUrl = url;
                        _instanceData.Url = url;
                    }

                    loadUrl = _instanceData.Url;
                }

                _activityCommon.SetPreferredNetworkInterface();
                _webViewCoding.LoadUrl(loadUrl);
                _urlLoaded = true;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool SendVehicleResponse(string id, string response)
        {
            try
            {
                _activityCommon.SetPreferredNetworkInterface();
                string script = string.Format(CultureInfo.InvariantCulture, "sendVehicleResponse(`{0}`, `{1}`);", id, response);
                if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                {
                    _webViewCoding.LoadUrl("javascript:" + script);
                }
                else
                {
                    _webViewCoding.EvaluateJavascript(script, new VehicleSendCallback());
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReloadPage()
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                try
                {
                    _webViewCoding.Reload();
                }
                catch (Exception)
                {
                    // ignored
                }
            });
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
                        }
                    }
                    break;
            }
        }

        private string EnqueueVehicleRequest(VehicleRequest vehicleRequest)
        {
            UpdateConnectTime();

            lock (_requestLock)
            {
                if (_requestQueue.Count > 0)
                {
                    return "Request queue full";
                }

                _requestQueue.Enqueue(vehicleRequest);
                _ediabasThreadWakeEvent.Set();
            }

            if (_updateHandler != null)
            {
                if (!IsEdiabasThreadRunning())
                {
                    ActivityCommon.PostRunnable(_updateHandler, _startRunnable);
                }
            }

            return string.Empty;
        }

        private void EdiabasOpen()
        {
            lock (_ediabasLock)
            {
                if (_ediabas == null)
                {
                    _ediabas = new EdiabasNet
                    {
                        EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                        AbortJobFunc = AbortEdiabasJob
                    };
                    _ediabas.SetConfigProperty("EcuPath", _ecuDir);
                    UpdateLogInfo();
                }

                _ediabas.EdInterfaceClass.EnableTransmitCache = false;
                _activityCommon.SetEdiabasInterface(_ediabas, _deviceAddress, _appDataDir);
            }
        }

        private void UpdateLogInfo()
        {
            string logDir = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(_appDataDir))
                {
                    logDir = Path.Combine(_appDataDir, "LogBmwCoding");
                    Directory.CreateDirectory(logDir);
                }
            }
            catch (Exception)
            {
                logDir = string.Empty;
            }

            _instanceData.TraceDir = string.Empty;
            if (_instanceData.TraceActive)
            {
                _instanceData.TraceDir = logDir;
            }

            if (_ediabas != null)
            {
                ActivityCommon.SetEdiabasConfigProperties(_ediabas, _instanceData.TraceDir, string.Empty, _instanceData.TraceAppend);
            }
        }

        private bool StartEdiabasThread()
        {
            if (IsEdiabasThreadRunning())
            {
                return true;
            }

            EdiabasOpen();

            _ediabasJobAbort = false;
            _ediabasThreadWakeEvent.Reset();
            lock (_ediabasThreadLock)
            {
                _ediabasThread = new Thread(EdiabasThread);
                _ediabasThread.Start();
            }
            UpdateOptionsMenu();

            return true;
        }

        private bool StopEdiabasThread()
        {
            _ediabasJobAbort = true;
            _ediabasThreadWakeEvent.Set();
            if (IsEdiabasThreadRunning())
            {
                // ReSharper disable once InconsistentlySynchronizedField
                _ediabasThread?.Join();
                // clear thread pointer
                IsEdiabasThreadRunning();
            }

            lock (_ediabasLock)
            {
                if (_ediabas != null)
                {
                    _ediabas.Dispose();
                    _ediabas = null;
                }
            }
            UpdateOptionsMenu();

            return true;
        }

        private bool IsEdiabasThreadRunning()
        {
            lock (_ediabasThreadLock)
            {
                if (_ediabasThread == null)
                {
                    return false;
                }
                if (_ediabasThread.IsAlive)
                {
                    return true;
                }
                _ediabasThread = null;
            }

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

        public bool EdiabasConnect(string sessionId)
        {
            lock (_ediabasLock)
            {
                if (_ediabas == null)
                {
                    return false;
                }

                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas connect, SessionId={0}", sessionId);

                try
                {
                    if (_ediabas.EdInterfaceClass.InterfaceConnect())
                    {
                        if (_ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
                        {
                            edInterfaceObd.CommParameter =
                                new UInt32[] { 0x0000010F, 0x0001C200, 0x000004B0, 0x00000014, 0x0000000A, 0x00000002, 0x00001388 };
                            edInterfaceObd.CommAnswerLen =
                                new Int16[] { 0x0000, 0x0000 };
                        }

                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Ediabas connected");
                        return true;
                    }

                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Ediabas connect failed");
                    return false;
                }
                catch (Exception ex)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas connect Exception: {0}", EdiabasNet.GetExceptionText(ex));
                    return false;
                }
            }
        }

        public bool EdiabasDisconnect(string sessionId)
        {
            lock (_ediabasLock)
            {
                if (_ediabas == null)
                {
                    return false;
                }

                _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas disconnect, SessionId={0}", sessionId);

                try
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Ediabas disconnect");
                    return _ediabas.EdInterfaceClass.InterfaceDisconnect();
                }
                catch (Exception ex)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas disconnect Exception: {0}", EdiabasNet.GetExceptionText(ex));
                    return false;
                }
            }
        }

        public bool IsEdiabasConnected()
        {
            lock (_ediabasLock)
            {
                if (_ediabas == null)
                {
                    return false;
                }

                return _ediabas.EdInterfaceClass.Connected;
            }
        }

        public List<byte[]> EdiabasTransmit(string sessionId, byte[] requestData)
        {
            List<byte[]> responseList = new List<byte[]>();
            if (requestData == null || requestData.Length < 3)
            {
                return responseList;
            }

            lock (_ediabasLock)
            {
                byte[] sendData = requestData;
                bool funcAddress = (sendData[0] & 0xC0) == 0xC0;     // functional address

                _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Ediabas transmit, SessionId={0}, Func={1}", sessionId, funcAddress);

                for (; ; )
                {
                    bool dataReceived = false;

                    if (_ediabas == null)
                    {
                        break;
                    }

                    try
                    {
                        if (_ediabas.EdInterfaceClass.TransmitData(sendData, out byte[] receiveData))
                        {
                            if (receiveData.Length > 0)
                            {
                                byte[] responseData = new byte[receiveData.Length - 1];
                                Array.Copy(receiveData, responseData, responseData.Length);
                                responseList.Add(responseData);
                            }

                            dataReceived = true;
                        }
                        else
                        {
                            if (!funcAddress)
                            {
                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "*** No response");
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }

                    if (!funcAddress || !dataReceived)
                    {
                        break;
                    }

                    if (AbortEdiabasJob())
                    {
                        break;
                    }

                    sendData = Array.Empty<byte>();
                }
            }

            return responseList;
        }

        private void EdiabasThread()
        {
            for (;;)
            {
                _ediabasThreadWakeEvent.WaitOne(100);
                if (_ediabasJobAbort)
                {
                    break;
                }

                VehicleRequest vehicleRequest = null;
                lock (_requestLock)
                {
                    if (!_requestQueue.TryDequeue(out vehicleRequest))
                    {
                        vehicleRequest = null;
                    }
                }

                if (vehicleRequest != null)
                {
                    StringBuilder sbBody = new StringBuilder();
                    sbBody.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
                    sbBody.Append("<vehicle_info>\r\n");

                    bool valid = true;
                    switch (vehicleRequest.RequestType)
                    {
                        case VehicleRequest.VehicleRequestType.Connect:
                        {
                            EdiabasDisconnect(vehicleRequest.SessionId);
                            bool isConnected = EdiabasConnect(vehicleRequest.SessionId);
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                lock (_instanceLock)
                                {
                                    _instanceData.ConnectTimeouts = 0;
                                    if (!isConnected)
                                    {
                                        _instanceData.CommErrorsOccurred = true;
                                    }
                                }
                                _activityCommon.SetLock(ActivityCommon.LockType.ScreenDim);
                                UpdateOptionsMenu();
                            });
                            break;
                        }

                        case VehicleRequest.VehicleRequestType.Disconnect:
                            EdiabasDisconnect(vehicleRequest.SessionId);
                            RunOnUiThread(() =>
                            {
                                if (_activityCommon == null)
                                {
                                    return;
                                }

                                lock (_instanceLock)
                                {
                                    _instanceData.ConnectTimeouts = 0;
                                }

                                _activityCommon.SetLock(ActivityCommon.LockType.None);
                                UpdateOptionsMenu();
                            });
                            break;

                        case VehicleRequest.VehicleRequestType.Transmit:
                        {
                            if (string.IsNullOrEmpty(vehicleRequest.Data))
                            {
                                valid = false;
                                break;
                            }

                            string requestString = vehicleRequest.Data.Replace(" ", "");
                            byte[] requestData = EdiabasNet.HexToByteArray(requestString);
                            sbBody.Append($" <data request=\"{System.Web.HttpUtility.HtmlEncode(requestString)}\" />\r\n");
                            List<byte[]> responseList = EdiabasTransmit(vehicleRequest.SessionId, requestData);
                            foreach (byte[] responseData in responseList)
                            {
                                string responseReport = BitConverter.ToString(responseData).Replace("-", "");
                                sbBody.Append($" <data response=\"{System.Web.HttpUtility.HtmlEncode(responseReport)}\" />\r\n");
                            }
                            break;
                        }
                    }

                    string validReport = valid ? "1" : "0";
                    string idReport = vehicleRequest.Id ?? string.Empty;
                    sbBody.Append($" <request valid=\"{System.Web.HttpUtility.HtmlEncode(validReport)}\" id=\"{System.Web.HttpUtility.HtmlEncode(idReport)}\" />\r\n");

                    bool connected = IsEdiabasConnected();
                    string connectedState = connected ? "1" : "0";

                    sbBody.Append($" <status connected=\"{System.Web.HttpUtility.HtmlEncode(connectedState)}\"");

                    string connectTimeoutsState;
                    lock (_instanceLock)
                    {
                        connectTimeoutsState = string.Format(CultureInfo.InvariantCulture, "{0}", _instanceData.ConnectTimeouts);
                    }
                    sbBody.Append($" timeouts=\"{System.Web.HttpUtility.HtmlEncode(connectTimeoutsState)}\"");

                    if (vehicleRequest.RequestType == VehicleRequest.VehicleRequestType.Connect)
                    {
                        string appIdState = ActivityCommon.AppId ?? string.Empty;
                        string adapterSerialState = ActivityCommon.LastAdapterSerial ?? string.Empty;
                        string validSerial;
                        lock (_instanceLock)
                        {
                            validSerial = _instanceData.ValidSerial ?? string.Empty;
                        }

                        string serialValidState = "0";
                        if (!string.IsNullOrEmpty(validSerial) && string.Compare(validSerial, adapterSerialState, StringComparison.Ordinal) == 0)
                        {
                            serialValidState = "1";
                        }

                        sbBody.Append($" app_id=\"{System.Web.HttpUtility.HtmlEncode(appIdState)}\"");
                        sbBody.Append($" adapter_serial=\"{System.Web.HttpUtility.HtmlEncode(adapterSerialState)}\"");
                        sbBody.Append($" serial_valid=\"{System.Web.HttpUtility.HtmlEncode(serialValidState)}\"");
                    }
                    sbBody.Append(" />\r\n");
                    sbBody.Append("</vehicle_info>\r\n");

                    SendVehicleResponseThread(vehicleRequest.Id, sbBody.ToString());
                }
            }
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
            };
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleListItemMultipleChoice, logNames.ToArray());
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Multiple;
            listView.SetItemChecked(0, _instanceData.TraceActive);
            listView.SetItemChecked(1, _instanceData.TraceAppend);

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
                    }
                }

                lock (_ediabasLock)
                {
                    UpdateLogInfo();
                }

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

        private bool SendTraceFile(EventHandler<EventArgs> handler)
        {
            if (_instanceData.CommErrorsOccurred && _instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir))
            {
                if (!StopEdiabasThread())
                {
                    return false;
                }

                return _activityCommon.RequestSendTraceFile(_appDataDir, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        private bool SendTraceFileAlways(EventHandler<EventArgs> handler)
        {
            if (IsEdiabasConnected())
            {
                return false;
            }

            if (ActivityCommon.CollectDebugInfo ||
                (_instanceData.TraceActive && !string.IsNullOrEmpty(_instanceData.TraceDir)))
            {
                if (!StopEdiabasThread())
                {
                    return false;
                }

                return _activityCommon.SendTraceFile(_appDataDir, _instanceData.TraceDir, GetType(), handler);
            }
            return false;
        }

        private bool OpenTraceFile()
        {
            if (IsEdiabasConnected())
            {
                return false;
            }

            string baseDir = _instanceData.TraceDir;
            if (string.IsNullOrEmpty(baseDir))
            {
                return false;
            }

            if (!StopEdiabasThread())
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

        private void ReportError(string msg)
        {
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                _instanceData.CommErrorsOccurred = true;

                lock (_ediabasLock)
                {
                    _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ReportError: {0}", msg);
                }
            });
        }

        private void UpdateConnectTime(bool reset = false)
        {
            lock (_timeLock)
            {
                _connectionUpdateTime = reset ? DateTime.MinValue.Ticks : Stopwatch.GetTimestamp();
            }
        }

        private long GetConnectTime()
        {
            lock (_timeLock)
            {
                return _connectionUpdateTime;
            }
        }

        private class WebViewClientImpl : WebViewClientCompat
        {
            private BmwCodingActivity _activity;

            public WebViewClientImpl(BmwCodingActivity activity)
            {
                _activity = activity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                return false;
            }

            public override void OnPageFinished(WebView view, string url)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("OnPageFinished: Url={0}", url));
#endif
                _activity.UpdateConnectTime();
            }

            public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceErrorCompat error)
            {
                _activity.UpdateConnectTime(true);
            }

            public override void OnReceivedSslError(WebView view, SslErrorHandler handler, SslError error)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("OnReceivedSslError: Url={0}", error?.Url ?? string.Empty));
#endif
                _activity.RunOnUiThread(() =>
                {
                    if (_activity._activityCommon == null)
                    {
                        return;
                    }

                    if (_activity._instanceData.SslErrorShown)
                    {
                        return;
                    }

                    _activity._instanceData.SslErrorShown = true;
                    _activity._activityCommon.ShowAlert(_activity.GetString(Resource.String.bmw_coding_ssl_fail), Resource.String.alert_title_info);
                });

                handler.Proceed();
            }

            public override void OnReceivedHttpAuthRequest(WebView view, HttpAuthHandler handler, string host, string realm)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("OnReceivedHttpAuthRequest: Host={0}, Realm={1}", host, realm));
#endif
                _activity.RunOnUiThread(() =>
                {
                    if (_activity._activityCommon == null)
                    {
                        return;
                    }

                    string password = AuthPwd;
                    if (!string.IsNullOrEmpty(_activity._instanceData.DayString))
                    {
                        string encodeString = AuthPwd + _activity._instanceData.DayString;
                        byte[] pwdArray = Encoding.ASCII.GetBytes(encodeString);
                        using (MD5 md5 = MD5.Create())
                        {
                            password = BitConverter.ToString(md5.ComputeHash(pwdArray)).Replace("-", "");
                        }
                    }
#if DEBUG
                    Android.Util.Log.Debug(Tag, string.Format("OnReceivedHttpAuthRequest: Name={0}, Pwd={1}", AuthUser, password));
#endif
                    handler.Proceed(AuthUser, password);
                });
            }

            public override void DoUpdateVisitedHistory(WebView view, string url, bool isReload)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("DoUpdateVisitedHistory: Url={0}, Reload={1}", url, isReload));
#endif
                _activity.RunOnUiThread(() =>
                {
                    if (_activity._activityCommon == null)
                    {
                        return;
                    }

                    _activity.UpdateConnectTime();
                    if (!string.IsNullOrEmpty(url))
                    {
                        lock (_activity._instanceLock)
                        {
                            _activity._instanceData.Url = url;
                            string compareUrl = url.TrimEnd(' ', '/');
                            if (string.Compare(compareUrl, _activity._instanceData.InitialUrl, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                _activity._instanceData.ServerConnected = true;
                            }
                        }
                    }
                });

                base.DoUpdateVisitedHistory(view, url, isReload);
            }
        }

        private class WebChromeClientImpl : WebChromeClient
        {
            private BmwCodingActivity _activity;

            public WebChromeClientImpl(BmwCodingActivity activity)
            {
                _activity = activity;
            }

            public override bool OnConsoleMessage(ConsoleMessage consoleMessage)
            {
#if DEBUG
                string message = string.Format(CultureInfo.InvariantCulture, "Message: {0}, Line: {1}", consoleMessage.Message(), consoleMessage.LineNumber());
                Android.Util.Log.Debug(Tag, message);
#endif
                return true;
            }
        }

        private class VehicleSendCallback : Java.Lang.Object, IValueCallback
        {
            public void OnReceiveValue(Java.Lang.Object value)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleSendCallback: {0}", value));
#endif
            }
        }

        private class WebViewJSInterface : Java.Lang.Object
        {
            BmwCodingActivity _activity;

            public WebViewJSInterface(BmwCodingActivity activity)
            {
                _activity = activity;
            }

            [JavascriptInterface]
            [Export]
            public string VehicleConnect(string sessionId, string id)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleConnect: SessionId={0}, Id={1}", sessionId, id));
#endif
                return _activity.EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Connect, sessionId, id));
            }

            [JavascriptInterface]
            [Export]
            public string VehicleDisconnect(string sessionId, string id)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleDisconnect: SessionId={0}, Id={1}", sessionId, id));
#endif
                return _activity.EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Disconnect, sessionId, id));
            }

            [JavascriptInterface]
            [Export]
            public string VehicleSend(string sessionId, string id, string data)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleSend: SessionId={0}, Id={1}, Data={2}", sessionId, id, data));
#endif
                return _activity.EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Transmit, sessionId, id, data));
            }

            [JavascriptInterface]
            [Export]
            public void ReportError(string sessionId, string msg)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("ReportError: SessionId={0}, Msg={1}", sessionId, msg));
#endif
                _activity.ReportError(msg);
            }

            [JavascriptInterface]
            [Export]
            public void UpdateStatus(bool status)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("UpdateStatus: Status={0}", status));
#endif
                _activity.UpdateConnectTime();
            }

            [JavascriptInterface]
            [Export]
            public void ReloadPage()
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, "ReloadPage");
#endif
                _activity.ReloadPage();
            }

            [JavascriptInterface]
            [Export]
            public int GetConnectTimeouts()
            {
                int connectTimeouts;
                lock (_activity._instanceLock)
                {
                    connectTimeouts = _activity._instanceData.ConnectTimeouts;
                }

#if DEBUG
                Android.Util.Log.Debug(Tag, "GetConnectTimeouts: Timeouts={0}", connectTimeouts);
#endif
                return connectTimeouts;
            }
        }

        private class ProxyExecutor : Java.Lang.Object, Java.Util.Concurrent.IExecutor
        {
            public void Execute(Java.Lang.IRunnable command)
            {
            }
        }

    }
}
