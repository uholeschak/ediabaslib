using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.OS;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BmwDeepObd.Dialogs;
using EdiabasLib;
using PsdzRpcClient;
using PsdzRpcServer.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
                Url = string.Empty;
                IstaFolder = string.Empty;
                TraceDir = string.Empty;
                TraceActive = true;
                TraceAppend = false;
                CommErrorsOccurred = false;
            }

            public string CodingUrl { get; set; }
            public string CodingUrlTest { get; set; }
            public string DayString { get; set; }
            public string ValidSerial { get; set; }
            public string Vin { get; set; }
            public string Url { get; set; }
            public string IstaFolder { get; set; }
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
        private static readonly string Tag = typeof(BmwRpcCodingActivity).FullName;
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
        private EdiabasProxyClient _ediabasProxyClient;
        private Task<bool> _startTask;
        private CancellationTokenSource _startCts;
        private object _startLock = new object();
        private object _timeLock = new object();
        private object _instanceLock = new object();
        private object _statusLock = new object();
        private bool _activityActive;
        private HttpClient _infoHttpClient;
        private AlertDialog _alertDialogInfo;
        private AlertDialog _alertDialogConnectError;
        public long _connectionUpdateTime;
        private PsdzRpcStatusInfo _statusInfo;
        private PsdzRpcSwiRegisterEnum? _selectedSwiRegister;
        private string _statusMessage;
        private bool _rpcClientConnected;

        private Button _buttonCodingConnect;
        private Button _buttonCodingDisconnect;
        private Button _buttonCodingOptions;
        private Button _buttonCodingGenerateTal;
        private Button _buttonCodingExecuteTal;
        private Button _buttonCodingAbort;
        private ProgressBar _progressBar;
        private TextView _textCodingStatus;

        private bool _taskActive;
        public bool TaskActive
        {
            get
            {
                lock (_statusLock)
                {
                    return _taskActive;
                }
            }
            private set
            {
                lock (_statusLock)
                {
                    _taskActive = value;
                }
            }
        }

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

            _activityCommon = new ActivityCommon(this, () =>
            {
                if (_activityActive)
                {
                    UpdateOptionsMenu();
                }
            }, BroadcastReceived);

            _buttonCodingConnect = FindViewById<Button>(Resource.Id.buttonCodingConnect);
            _buttonCodingConnect.Click += async (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }

                try
                {
                    if (TaskActive)
                    {
                        return;
                    }

                    string istaFolder;
                    lock (_instanceLock)
                    {
                        istaFolder = _instanceData.IstaFolder;
                    }

                    bool result = await _psdzRpcClient.RpcService.ConnectVehicle(istaFolder, string.Empty, false).ConfigureAwait(false);
                    if (result)
                    {
                        await RpcClientTaskStarted().ConfigureAwait(false);
                    }
                    else
                    {
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ConnectVehicle failed");
                    }
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ConnectVehicle: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                }
            };

            _buttonCodingDisconnect = FindViewById<Button>(Resource.Id.buttonCodingDisconnect);
            _buttonCodingDisconnect.Click += async (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }

                try
                {
                    if (TaskActive)
                    {
                        return;
                    }

                    bool result = await _psdzRpcClient.RpcService.DisconnectVehicle().ConfigureAwait(false);
                    if (result)
                    {
                        await RpcClientTaskStarted().ConfigureAwait(false);
                    }
                    else
                    {
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "DisconnectVehicle failed");
                    }
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "DisconnectVehicle: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                }
            };

            _buttonCodingOptions = FindViewById<Button>(Resource.Id.buttonCodingOptions);
            _buttonCodingOptions.Click += async (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }

                try
                {
                    if (TaskActive)
                    {
                        return;
                    }

                    bool result = await _psdzRpcClient.RpcService.VehicleFunctions(PsdzOperationType.CreateOptions).ConfigureAwait(false);
                    if (result)
                    {
                        await RpcClientTaskStarted().ConfigureAwait(false);
                    }
                    else
                    {
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions CreateOptions failed");
                    }
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions CreateOptions: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                }
            };

            _buttonCodingGenerateTal = FindViewById<Button>(Resource.Id.buttonCodingGenerateTal);
            _buttonCodingGenerateTal.Click += async (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }

                try
                {
                    if (TaskActive)
                    {
                        return;
                    }

                    bool result = await _psdzRpcClient.RpcService.VehicleFunctions(PsdzOperationType.BuildTalModFa).ConfigureAwait(false);
                    if (result)
                    {
                        await RpcClientTaskStarted().ConfigureAwait(false);
                    }
                    else
                    {
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions BuildTal failed");
                    }
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions BuildTal: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                }
            };

            _buttonCodingExecuteTal = FindViewById<Button>(Resource.Id.buttonCodingExecuteTal);
            _buttonCodingExecuteTal.Click += async (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }

                try
                {
                    if (TaskActive)
                    {
                        return;
                    }

                    bool result = await _psdzRpcClient.RpcService.VehicleFunctions(PsdzOperationType.ExecuteTal).ConfigureAwait(false);
                    if (result)
                    {
                        await RpcClientTaskStarted().ConfigureAwait(false);
                    }
                    else
                    {
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions ExecuteTal failed");
                    }
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctions ExecuteTal: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                }
            };

            _buttonCodingAbort = FindViewById<Button>(Resource.Id.buttonCodingAbort);
            _buttonCodingAbort.Click += async (s, e) =>
            {
                if (!_activityActive)
                {
                    return;
                }

                try
                {
                    await _psdzRpcClient.RpcService.CancelOperation();
                }
                catch (Exception ex)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "CancelOperation: Exception={0}",
                        EdiabasNet.GetExceptionText(ex, false, false));
                }
            };

            _progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            _progressBar.Max = 100;
            _progressBar.Progress = 0;
            _progressBar.Indeterminate = false;

            _textCodingStatus = FindViewById<TextView>(Resource.Id.textCodingStatus);

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);
            _activityCommon.SelectedElmWifiIp = Intent.GetStringExtra(ExtraElmWifiIp);
            _activityCommon.SelectedDeepObdWifiIp = Intent.GetStringExtra(ExtraDeepObdWifiIp);

            _activityCommon.SetPreferredNetworkInterface();

            CreateRpcClient();
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

            StartRpcClient();
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

            Task.Run(DisposeRpcClient).GetAwaiter().GetResult();

            if (_activityCommon != null)
            {
                _activityCommon.Dispose();
                _activityCommon = null;
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
                        return;
                    }

                    StartRpcClient();
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
                                        return;
                                    }

                                    StartRpcClient();
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
                            return;
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

                    Task<HttpResponseMessage> taskDownload = _infoHttpClient.PostAsync(InfoCodingUrl, formInfo);

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

        private async Task RpcClientTaskStarted()
        {
            TaskActive = true;
            await RpcClientUpdateDisplay().ConfigureAwait(false);
        }

        private async Task RpcClientTaskCompleted()
        {
            TaskActive = false;
            await RpcClientUpdateDisplay().ConfigureAwait(false);
        }

        private async Task RpcClientUpdateDisplay()
        {
            await GetRemoteStatusAsync().ConfigureAwait(false);
            RunOnUiThread(() =>
            {
                if (_activityCommon == null)
                {
                    return;
                }

                UpdateDisplay();
            });
        }

        private async Task<bool> GetRemoteStatusAsync()
        {
            try
            {
                if (_psdzRpcClient.RpcService == null)
                {
                    return false;
                }

                PsdzRpcStatusInfo statusInfo = await _psdzRpcClient.RpcService.GetStatusInfo().ConfigureAwait(false);
                lock (_statusLock)
                {
                    _statusInfo = statusInfo;
                }

                return true;
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

            PsdzRpcStatusInfo statusInfo;
            string statusMessage;
            bool rpcClientConnected;

            lock (_statusLock)
            {
                statusInfo = _statusInfo;
                statusMessage = _statusMessage;
                rpcClientConnected = _rpcClientConnected;
            }

            if (statusInfo == null || !rpcClientConnected)
            {
                _buttonCodingConnect.Enabled = false;
                _buttonCodingDisconnect.Enabled = false;
                _buttonCodingOptions.Enabled = false;
                _buttonCodingGenerateTal.Enabled = false;
                _buttonCodingExecuteTal.Enabled = false;
                _buttonCodingAbort.Enabled = false;
                _progressBar.Visibility = ViewStates.Invisible;
                return;
            }

            bool active = TaskActive;
            bool modifyTal = !active && statusInfo.PsdzInitialized && statusInfo.VehicleConnected && statusInfo.HasOptionsDict;
            _buttonCodingConnect.Enabled = !active && !statusInfo.VehicleConnected;
            _buttonCodingDisconnect.Enabled = !active && statusInfo.PsdzInitialized && statusInfo.VehicleConnected;
            _buttonCodingOptions.Enabled = !active && statusInfo.PsdzInitialized && statusInfo.VehicleConnected && !statusInfo.HasOptionsDict;
            _buttonCodingGenerateTal.Enabled = modifyTal;
            _buttonCodingExecuteTal.Enabled = modifyTal && statusInfo.TalPresent;
            _buttonCodingAbort.Enabled = active && statusInfo.CancelPossible;
            _progressBar.Visibility = active && statusInfo.CancelPossible ? ViewStates.Visible : ViewStates.Invisible;

            _textCodingStatus.Text = statusMessage ?? string.Empty;

            UpdateConnectTime();
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
            return _ediabasProxyClient.IsEdiabasConnected();
        }

        private bool CreateRpcClient()
        {
            try
            {
                _psdzRpcClient = new PsdzRpcClient.PsdzRpcClient(null,
                    PsdzRpcServiceConstants.CaCertFile, PsdzRpcServiceConstants.ClientPfxFile, Assembly.GetExecutingAssembly());
                _psdzRpcClient.ClientConnected += async (sender, connected) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _ediabasProxyClient.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ClientConnected: Connected={0}",
                        connected);

                    lock (_statusLock)
                    {
                        _rpcClientConnected = connected;
                    }

                    if (connected)
                    {
                        lock (_statusLock)
                        {
                            _statusMessage = string.Empty;
                        }
                        await RpcClientUpdateDisplay().ConfigureAwait(false);
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        if (connected)
                        {
                            _activityCommon.SetLock(ActivityCommon.LockType.ScreenDim);
                        }
                        else
                        {
                            _activityCommon.SetLock(ActivityCommon.LockType.None);
                            ConnectionFailMessage();
                        }

                        _progressBar.Progress = 0;
                        _progressBar.Indeterminate = false;
                        UpdateDisplay();
                    });
                };

                _psdzRpcClient.CallbackHandler.StartProgrammingCompleted += async (s, success) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "StartProgrammingCompleted: Success={0}",
                        success);
                    await RpcClientTaskCompleted().ConfigureAwait(false);
                };

                _psdzRpcClient.CallbackHandler.StopProgrammingCompleted += async (s, success) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "StopProgrammingCompleted: Success={0}",
                        success);
                    await RpcClientTaskCompleted().ConfigureAwait(false);
                };

                _psdzRpcClient.CallbackHandler.ConnectVehicleCompleted += async (s, connectArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ConnectVehicleCompleted: Success={0}, Vin={1}",
                        connectArgs.Success, connectArgs.Vin);

                    if (connectArgs.Success)
                    {
                        lock (_instanceLock)
                        {
                            _instanceData.Vin = connectArgs.Vin;
                        }
                    }

                    await RpcClientTaskCompleted().ConfigureAwait(false);
                };

                _psdzRpcClient.CallbackHandler.DisconnectVehicleCompleted += async (s, success) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "DisconnectVehicleCompleted: Success={0}",
                        success);
                    lock (_instanceLock)
                    {
                        _instanceData.Vin = null;
                    }

                    await RpcClientTaskCompleted().ConfigureAwait(false);
                };

                _psdzRpcClient.CallbackHandler.VehicleFunctionsCompleted += async (s, vehicleArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "VehicleFunctionsCompleted: Success={0}, Type={1}",
                        vehicleArgs.Success, vehicleArgs.OperationType);

                    await RpcClientTaskCompleted().ConfigureAwait(false);
                };

                _psdzRpcClient.CallbackHandler.UpdateStatus += (s, message) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    lock (_statusLock)
                    {
                        _statusMessage = message;
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

                _psdzRpcClient.CallbackHandler.UpdateProgress += (s, progressArgs) =>
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

                        _progressBar.Indeterminate = progressArgs.Marquee;
                        _progressBar.Progress = progressArgs.Percent;
                        UpdateDisplay();
                    });
                };

                _psdzRpcClient.CallbackHandler.UpdateOptions += (sender, optionArgs) =>
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

                _psdzRpcClient.CallbackHandler.UpdateOptionSelections += (sender, swiRegisterEnum) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    lock (_statusLock)
                    {
                        _selectedSwiRegister = swiRegisterEnum;
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

                _psdzRpcClient.CallbackHandler.ShowMessage += (sender, msgArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        msgArgs.Result = false;
                        return;
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            return;
                        }

                        new AlertDialog.Builder(this)
                            .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                            {
                            })
                            .SetCancelable(true)
                            .SetMessage(msgArgs.Message)
                            .SetTitle(Resource.String.alert_title_info)
                            .Show();
                    });

                    msgArgs.Result = true;
                };

                _psdzRpcClient.CallbackHandler.ShowMessageWait += (sender, msgArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        msgArgs.SetResult(false);
                        return;
                    }

                    RunOnUiThread(() =>
                    {
                        if (_activityCommon == null)
                        {
                            msgArgs.SetResult(false);
                            return;
                        }

                        if (msgArgs.OkBtn)
                        {
                            AlertDialog alertDialogOk = new AlertDialog.Builder(this)
                                .SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                                {
                                })
                                .SetCancelable(true)
                                .SetMessage(msgArgs.Message)
                                .SetTitle(Resource.String.alert_title_info)
                                .Show();

                            if (alertDialogOk != null)
                            {
                                alertDialogOk.DismissEvent += (sender, args) =>
                                {
                                    msgArgs.SetResult(true);
                                };
                            }
                            return;
                        }

                        bool dialogResult = false;
                        AlertDialog alertDialogYesNo = new AlertDialog.Builder(this)
                            .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                            {
                                dialogResult = true;
                            })
                            .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                            {
                                dialogResult = false;
                            })
                            .SetCancelable(true)
                            .SetMessage(msgArgs.Message)
                            .SetTitle(Resource.String.alert_title_info)
                            .Show();

                        if (alertDialogYesNo != null)
                        {
                            alertDialogYesNo.DismissEvent += (sender, args) =>
                            {
                                msgArgs.SetResult(dialogResult);
                            };
                        }
                    });
                };

                _psdzRpcClient.CallbackHandler.TelSendQueueSize += (sender, queueArgs) =>
                {
                    queueArgs.Result = -1; // Simulate no queue
                };

                _psdzRpcClient.CallbackHandler.ServiceInitialized += async (sender, serviceArgs) =>
                {
                    if (_activityCommon == null)
                    {
                        return;
                    }

                    if (_psdzRpcClient.RpcService != null && !serviceArgs.LoggingInitialized)
                    {
                        string logFile = Path.Combine(serviceArgs.HostLogDir, "PsdzClient.log");

                        bool result = await _psdzRpcClient.RpcService.SetupLog4Net(logFile).ConfigureAwait(false);
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "Setup log4net result: {0}", result);

                        bool resetResult = await _psdzRpcClient.RpcService.ResetStarterGuard().ConfigureAwait(false);
                        _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ResetStarterGuard result: {0}", resetResult);
                    }
                };

                EdiabasNet ediabas = new EdiabasNet
                {
                    EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                };
                ediabas.SetConfigProperty("EcuPath", _ecuDir);
                ediabas.EdInterfaceClass.EnableTransmitCache = false;
                _activityCommon.SetEdiabasInterface(ediabas, _deviceAddress, _appDataDir);

                UpdateLogInfo();

                _ediabasProxyClient = new EdiabasProxyClient(ediabas);
                _ediabasProxyClient.VehicleResponseEvent += (vehicleResponse) =>
                {
                    return Task.Run(() => _psdzRpcClient.RpcService.SetVehicleResponse(vehicleResponse)).GetAwaiter().GetResult();
                };

                _ediabasProxyClient.MessageEvent += (messageType, message) =>
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "EdiabasProxyClient: Type={0}, Message={1}", messageType.ToString(), message);
                };

                _psdzRpcClient.CallbackHandler.VehicleConnect += (s, id) =>
                {
                    EdiabasProxyClient proxy = _ediabasProxyClient;
                    if (proxy == null || proxy.IsDisposed)
                    {
                        return;
                    }
                    proxy.VehicleConnect(id);
                };

                _psdzRpcClient.CallbackHandler.VehicleDisconnect += (s, id) =>
                {
                    EdiabasProxyClient proxy = _ediabasProxyClient;
                    if (proxy == null || proxy.IsDisposed)
                    {
                        return;
                    }
                    proxy.VehicleDisconnect(id);
                };

                _psdzRpcClient.CallbackHandler.VehicleSend += (s, sendArgs) =>
                {
                    EdiabasProxyClient proxy = _ediabasProxyClient;
                    if (proxy == null || proxy.IsDisposed)
                    {
                        return;
                    }
                    proxy.VehicleSend(sendArgs.Id, sendArgs.Data);
                };

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        async Task DisposeRpcClient()
        {
            try
            {
                Task<bool> startTask;
                lock (_startLock)
                {
                    _startCts?.Cancel();
                    startTask = _startTask;
                }

                if (startTask != null)
                {
                    try
                    {
                        await startTask.ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                if (_psdzRpcClient != null)
                {
                    _psdzRpcClient.Dispose();
                    _psdzRpcClient = null;
                }

                if (_ediabasProxyClient != null)
                {
                    await _ediabasProxyClient.StopEdiabasThread().ConfigureAwait(false);
                    await _ediabasProxyClient.DisposeAsync().ConfigureAwait(false);
                    _ediabasProxyClient = null;
                }

                lock (_statusLock)
                {
                    _rpcClientConnected = false;
                }

                lock (_startLock)
                {
                    if (_startCts != null)
                    {
                        _startCts.Dispose();
                        _startCts = null;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool StartRpcClient()
        {
            try
            {
                lock (_instanceLock)
                {
                    if (string.IsNullOrEmpty(_instanceData.CodingUrl))
                    {
                        return false;
                    }

                    lock (_statusLock)
                    {
                        if (_rpcClientConnected)
                        {
                            return true;
                        }
                    }
                }

                lock (_startLock)
                {
                    if (_startTask != null && !_startTask.IsCompleted)
                    {
                        return false;
                    }
                }

                TaskActive = false;
                lock (_startLock)
                {
                    _startCts = new CancellationTokenSource();
                    _startTask = RpcClientConnect();
                    _startTask.ContinueWith(t =>
                    {
                        if (!t.Result)
                        {
                            lock (_statusLock)
                            {
                                _rpcClientConnected = false;
                            }
                        }

                        lock (_startLock)
                        {
                            _startTask = null;
                            _startCts?.Dispose();
                            _startCts = null;
                        }
                    });
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> RpcClientConnect()
        {
            try
            {
                if (_ediabasProxyClient == null)
                {
                    return false;
                }

                if (!_activityCommon.IsNetworkPresent(out string domains))
                {
                    return false;
                }

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
                                url = @"http://vm-ista.local.holeschak.de:8000/";
                            }
                        }
                        else
                        {
                            url = _instanceData.CodingUrl;
                        }

                        url = @"http://vm-ista.local.holeschak.de:8000/";
                        _instanceData.Url = url;
                    }

                    loadUrl = _instanceData.Url;
                }

                if (string.IsNullOrEmpty(loadUrl))
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: loadUrl is empty");
                    return false;
                }

                if (!Uri.TryCreate(loadUrl, UriKind.Absolute, out Uri loadUri) || string.IsNullOrEmpty(loadUri.Host))
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Invalid loadUrl={0}", loadUrl);
                    return false;
                }

                string remoteHost = loadUri.Host;
                bool connected = await _psdzRpcClient.ConnectTcpAsync(remoteHost, PsdzRpcServiceConstants.DefaultTcpPort, null, _startCts.Token).ConfigureAwait(false);
                if (!connected)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: ConnectTcpAsync failed");
                    return false;
                }

                int localVersion = PsdzRpcServiceConstants.InterfaceVersion;
                int remoteVersion = await _psdzRpcClient.RpcService.GetInterfaceVersion().ConfigureAwait(false);
                if (remoteVersion < localVersion)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Interface version mismatch");
                    return false;
                }

                string istaFolder = await _psdzRpcClient.RpcService.GetIstaInstallLocation().ConfigureAwait(false);
                if (string.IsNullOrEmpty(istaFolder))
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Failed to get ISTA install location");
                    return false;
                }

                lock (_instanceLock)
                {
                    _instanceData.IstaFolder = istaFolder;
                }

                bool licenseResult = await _psdzRpcClient.RpcService.SetLicenseValid(true).ConfigureAwait(false);
                if (!licenseResult)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: SetLicenseValid failed");
                    return false;
                }

                string language = _activityCommon.GetCurrentLanguage();
                bool matched = await _psdzRpcClient.RpcService.SetLanguage(language).ConfigureAwait(false);
                if (matched)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: SetLanguage matched: {0}", language);
                }
                else
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: SetLanguage mismatch: {0}", language);
                }

                if (!_ediabasProxyClient.StartEdiabasThread())
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: StartEdiabasThread failed");
                    return false;
                }

                bool proxyResult = await _psdzRpcClient.RpcService.EnableVehicleProxy().ConfigureAwait(false);
                if (!proxyResult)
                {
                    _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: EnableVehicleProxy failed");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "RpcConnect: Exception={0}",
                    EdiabasNet.GetExceptionText(ex, false, false));
                return false;
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

            if (_ediabasProxyClient?.Ediabas != null)
            {
                ActivityCommon.SetEdiabasConfigProperties(_ediabasProxyClient.Ediabas, _instanceData.TraceDir, string.Empty, _instanceData.TraceAppend);
            }
        }

        private void SelectDataLogging()
        {
            if (IsEdiabasConnected())
            {
                return;
            }

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

                _ediabasProxyClient?.EdiabasLogFormat(EdiabasNet.EdLogLevel.Ifh, "ReportError: {0}", msg);
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
