using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.WebKit;
using Android.Webkit;
using EdiabasLib;
using Java.Interop;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/bmw_coding_title",
        Name = ActivityCommon.AppNameSpace + "." + nameof(BmwCodingActivity),
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

            public VehicleRequest(VehicleRequestType requestType, string id, string data = null)
            {
                RequestType = requestType;
                Id = id;
                Data = data;
            }

            public VehicleRequestType RequestType { get; }
            public string Id { get; }
            public string Data { get; }
        }

        public class InstanceData
        {
            public string Url { get; set; }
        }

        // Intent extra
        public const string ExtraEcuDir = "ecu_dir";
        public const string ExtraAppDataDir = "app_data_dir";
        public const string ExtraInterface = "interface";
        public const string ExtraDeviceName = "device_name";
        public const string ExtraDeviceAddress = "device_address";
        public const string ExtraEnetIp = "enet_ip";

#if DEBUG
        private static readonly string Tag = typeof(BmwCodingActivity).FullName;
#endif

        private enum ActivityRequest
        {
            RequestDevelopmentSettings,
        }

        private InstanceData _instanceData = new InstanceData();
        private ActivityCommon _activityCommon;
        private EdWebServer _edWebServer;
        private string _ecuDir;
        private string _appDataDir;
        private string _deviceName;
        private string _deviceAddress;
        private EdiabasNet _ediabas;
        private volatile bool _ediabasJobAbort;
        private Thread _ediabasThread;
        private AutoResetEvent _ediabasThreadWakeEvent = new AutoResetEvent(false);
        private object _ediabasLock = new object();
        private object _requestLock = new object();
        private Queue<VehicleRequest> _requestQueue = new Queue<VehicleRequest>();

        private WebView _webViewCoding;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(ActivityCommon.SelectedThemeId);
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

            SetResult(Android.App.Result.Canceled);

            _activityCommon = new ActivityCommon(this);

            _activityCommon.UpdateRegisterInternetCellular();
            _activityCommon.SetPreferredNetworkInterface();

            _ecuDir = Intent.GetStringExtra(ExtraEcuDir);
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);
            _activityCommon.SelectedInterface = (ActivityCommon.InterfaceType)
                Intent.GetIntExtra(ExtraInterface, (int)ActivityCommon.InterfaceType.None);
            _deviceName = Intent.GetStringExtra(ExtraDeviceName);
            _deviceAddress = Intent.GetStringExtra(ExtraDeviceAddress);
            _activityCommon.SelectedEnetIp = Intent.GetStringExtra(ExtraEnetIp);

            int listenPort = StartWebServer();
            StartEdiabasThread();

            _webViewCoding = FindViewById<WebView>(Resource.Id.webViewCoding);

            try
            {
                WebSettings webSettings = _webViewCoding?.Settings;
                if (webSettings != null)
                {
                    webSettings.JavaScriptEnabled = true;
                    webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                    webSettings.DomStorageEnabled = true;
                    string userAgent = webSettings.UserAgentString;
                    if (!string.IsNullOrEmpty(userAgent))
                    {
                        userAgent += " DeepObd";
                        webSettings.UserAgentString = userAgent;
                    }
                }

                _webViewCoding.AddJavascriptInterface(new WebViewJSInterface(this), "app");
                _webViewCoding.SetWebViewClient(new WebViewClientImpl(this));
                _webViewCoding.SetWebChromeClient(new WebChromeClientImpl(this));
                //_webViewCoding.LoadUrl(@"https://www.holeschak.de");
                _webViewCoding.LoadUrl(@"http://ulrich3.local.holeschak.de:3000");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopWebServer();
            StopEdiabasThread();

            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            if (IsEdiabasConnected())
            {
                return;
            }

            if (_edWebServer != null && _edWebServer.IsEdiabasConnected())
            {
                return;
            }

            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (IsEdiabasConnected())
                    {
                        return true;
                    }

                    if (_edWebServer != null && _edWebServer.IsEdiabasConnected())
                    {
                        return true;
                    }

                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            switch ((ActivityRequest)requestCode)
            {
                case ActivityRequest.RequestDevelopmentSettings:
                    break;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool ShowDevelopmentSettings()
        {
            try
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionApplicationDevelopmentSettings);
                StartActivityForResult(intent, (int)ActivityRequest.RequestDevelopmentSettings);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SendVehicleResponseThread(string response)
        {
            RunOnUiThread(() =>
            {
                SendVehicleResponse(response);
            });
        }

        private bool SendVehicleResponse(string response)
        {
            try
            {
                string script = string.Format(CultureInfo.InvariantCulture, "sendVehicleResponse('{0}');", response);
                _webViewCoding.EvaluateJavascript(script, new VehicleSendCallback());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string EnqueueVehicleRequest(VehicleRequest vehicleRequest)
        {
            lock (_requestLock)
            {
                if (_requestQueue.Count > 0)
                {
                    return "Request queue full";
                }

                _requestQueue.Enqueue(vehicleRequest);
                _ediabasThreadWakeEvent.Set();
            }

            return string.Empty;
        }

        private int StartWebServer(int listenPort = 8080)
        {
            try
            {
                EdiabasNet ediabas = EdiabasSetup();
                _edWebServer = new EdWebServer(ediabas, message =>
                {
                    _edWebServer.Ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, message);
                });
                int usedPort = _edWebServer.StartTcpListener("http://127.0.0.1:" + listenPort.ToString(CultureInfo.InvariantCulture));
                return usedPort;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private bool StopWebServer()
        {
            try
            {
                if (_edWebServer != null)
                {
                    _edWebServer.Dispose();
                    _edWebServer = null;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private EdiabasNet EdiabasSetup()
        {
            EdiabasNet ediabas = new EdiabasNet
            {
                EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
                AbortJobFunc = AbortEdiabasJob
            };
            ediabas.SetConfigProperty("EcuPath", _ecuDir);
            string traceDir = Path.Combine(_appDataDir, "LogBmwCoding");
            if (!string.IsNullOrEmpty(traceDir))
            {
                ediabas.SetConfigProperty("TracePath", traceDir);
                ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                ediabas.SetConfigProperty("CompressTrace", "1");
            }
            else
            {
                ediabas.SetConfigProperty("IfhTrace", "0");
            }

            _activityCommon.SetEdiabasInterface(ediabas, _deviceAddress);

            return ediabas;
        }

        private bool StartEdiabasThread()
        {
            if (IsEdiabasThreadRunning())
            {
                return true;
            }

            if (_ediabas == null)
            {
                _ediabas = EdiabasSetup();
            }

            _ediabasJobAbort = false;
            _ediabasThreadWakeEvent.Reset();
            _ediabasThread = new Thread(EdiabasThread);
            _ediabasThread.Start();

            return true;
        }

        private bool StopEdiabasThread()
        {
            _ediabasJobAbort = true;
            _ediabasThreadWakeEvent.Set();
            if (IsEdiabasThreadRunning())
            {
                _ediabasThread.Join();
            }

            return true;
        }

        private bool IsEdiabasThreadRunning()
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

        public bool EdiabasConnect()
        {
            lock (_ediabasLock)
            {
                try
                {
                    if (_ediabas.EdInterfaceClass.InterfaceConnect())
                    {
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

        public bool EdiabasDisconnect()
        {
            lock (_ediabasLock)
            {
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

        public List<byte[]> EdiabasTransmit(byte[] requestData)
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

                for (; ; )
                {
                    bool dataReceived = false;

                    try
                    {
                        if (_ediabas.EdInterfaceClass.TransmitData(sendData, out byte[] receiveData))
                        {
                            responseList.Add(receiveData);
                            dataReceived = true;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
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
                            EdiabasDisconnect();
                            EdiabasConnect();
                            break;

                        case VehicleRequest.VehicleRequestType.Disconnect:
                            EdiabasDisconnect();
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
                            List<byte[]> responseList = EdiabasTransmit(requestData);
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
                    sbBody.Append($" <status connected=\"{System.Web.HttpUtility.HtmlEncode(connectedState)}\" />\r\n");
                    sbBody.Append("</vehicle_info>\r\n");

                    SendVehicleResponseThread(sbBody.ToString());
                }
            }
        }

        private class WebViewClientImpl : WebViewClientCompat
        {
            private Android.App.Activity _activity;

            public WebViewClientImpl(Android.App.Activity activity)
            {
                _activity = activity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                return false;
            }

            public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceErrorCompat error)
            {
                Toast.MakeText(_activity, _activity.GetString(Resource.String.bmw_coding_network_error), ToastLength.Long)?.Show();
            }
        }

        private class WebChromeClientImpl : WebChromeClient
        {
            private Android.App.Activity _activity;

            public WebChromeClientImpl(Android.App.Activity activity)
            {
                _activity = activity;
            }

            public override bool OnConsoleMessage(ConsoleMessage consoleMessage)
            {
#if DEBUG
                string message = string.Format(CultureInfo.InvariantCulture, "Message: {0}, Line: {1}, Source: {2}", consoleMessage.Message(), consoleMessage.LineNumber(), consoleMessage.SourceId());
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
            public string VehicleConnect(string id)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleConnect: Id={0}", id));
#endif
                return _activity.EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Connect, id));
            }

            [JavascriptInterface]
            [Export]
            public string VehicleDisconnect(string id)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleDisconnect: Id={0}", id));
#endif
                return _activity.EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Disconnect, id));
            }

            [JavascriptInterface]
            [Export]
            public string VehicleSend(string id, string data)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, string.Format("VehicleSend: Id={0}, Data={1}", id, data));
#endif
                return _activity.EnqueueVehicleRequest(new VehicleRequest(VehicleRequest.VehicleRequestType.Transmit, id, data));
            }
        }
    }
}
