using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.Net.Http;
using Android.OS;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BmwDeepObd.Dialogs;
using EdiabasLib;
using Java.Security.Cert;
using PsdzRpcServer.Shared;
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

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/bmw_rpc_coding_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(BmwRpcCodingActivity),
        LaunchMode = LaunchMode.SingleInstance,
        AlwaysRetainTaskState = true,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class BmwRpcCodingActivity : BaseActivity
    {
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
                TraceDir = string.Empty;
                TraceActive = true;
                TraceAppend = false;
                CommErrorsOccurred = false;
            }

            public string CodingUrl { get; set; }
            public string CodingUrlTest { get; set; }
            public string DayString { get; set; }
            public string ValidSerial { get; set; }
            public string InitialUrl { get; set; }
            public string Url { get; set; }
            public bool ServerConnected { get; set; }
            public string TraceDir { get; set; }
            public bool TraceActive { get; set; }
            public bool TraceAppend { get; set; }
            public bool CommErrorsOccurred { get; set; }
        }

        public delegate void AcceptDelegate(bool accepted);
        public delegate void InfoCheckDelegate(bool success, bool cancelled, string codingUrl = null, string codingUrlTest = null, string message = null, string dayString = null, string validSerial = null);

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
        private PsdzRpcClient.PsdzRpcClient _psdzRpcClient;
        private EdiabasNet _ediabas;
        private volatile bool _ediabasJobAbort;
        private Thread _ediabasThread;
        private AutoResetEvent _ediabasThreadWakeEvent;
        private object _ediabasLock = new object();
        private object _timeLock = new object();
        private object _instanceLock = new object();
        private bool _activityActive;
        private HttpClient _infoHttpClient;
        private bool _urlLoaded;
        private AlertDialog _alertDialogInfo;
        private AlertDialog _alertDialogConnectError;
        public long _connectionUpdateTime;

        private Button _buttonCodingConnect;
        private Button _buttonCodingDisconnect;
        private Button _buttonCodingOptions;
        private Button _buttonCodingTal;
        private Button _buttonCodingExecute;
        private Button _buttonCodingAbort;

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
            SetContentView(Resource.Layout.bmw_rpc_coding);

            SetResult(Android.App.Result.Ok);

            _ediabasThreadWakeEvent = new AutoResetEvent(false);
            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                }
            }, BroadcastReceived);

            _buttonCodingConnect = FindViewById<Button>(Resource.Id.buttonCodingConnect);
            _buttonCodingConnect.Click += (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }
            };

            _buttonCodingDisconnect = FindViewById<Button>(Resource.Id.buttonCodingDisconnect);
            _buttonCodingDisconnect.Click += (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }
            };

            _buttonCodingOptions = FindViewById<Button>(Resource.Id.buttonCodingOptions);
            _buttonCodingOptions.Click += (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }
            };

            _buttonCodingTal = FindViewById<Button>(Resource.Id.buttonCodingTal);
            _buttonCodingTal.Click += (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }
            };

            _buttonCodingExecute = FindViewById<Button>(Resource.Id.buttonCodingExecute);
            _buttonCodingExecute.Click += (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }
            };

            _buttonCodingAbort = FindViewById<Button>(Resource.Id.buttonCodingAbort);
            _buttonCodingAbort.Click += (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }
            };

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);

            _activityCommon.SetPreferredNetworkInterface();

            _psdzRpcClient = new PsdzRpcClient.PsdzRpcClient(null, PsdzRpcServiceConstants.CaCertFile, PsdzRpcServiceConstants.ClientPfxFile);
            _psdzRpcClient.ClientConnected += (sender, connected) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }
                lock (_instanceLock)
                {
                    _instanceData.ServerConnected = connected;
                }

                if (!connected)
                {
                    ConnectionFailMessage();
                }

                UpdateConnectTime();
                UpdateDisplay();
            };

            _psdzRpcClient.CallbackHandler.StartProgrammingCompleted += (s, success) =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                RunOnUiThread(() =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    UpdateDisplay();
                });
            };

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
        }

        protected override void OnPause()
        {
            base.OnPause();
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

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

            if (_psdzRpcClient != null)
            {
                _psdzRpcClient.Dispose();
                _psdzRpcClient = null;
            }

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

                        _activityCommon.OpenWebUrl("https://github.com/uholeschak/ediabaslib/blob/master/docs/BMW_Coding.md");
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
                })
                {
                    Timeout = TimeSpan.FromSeconds(ActivityCommon.HttpClientTimeout)
                };
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

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }

            UpdateOptionsMenu();
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

    }
}
