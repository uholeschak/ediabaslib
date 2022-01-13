using System;
using System.Globalization;
using System.IO;
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

            _webViewCoding = FindViewById<WebView>(Resource.Id.webViewCoding);

            try
            {
                WebSettings webSettings = _webViewCoding?.Settings;
                if (webSettings != null)
                {
                    webSettings.JavaScriptEnabled = true;
                    webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                    webSettings.DomStorageEnabled = true;
                    //webSettings.UserAgentString = string.Format(CultureInfo.InvariantCulture, "DeepObd:{0}", listenPort);
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

            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
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

        private EdiabasNet EdiabasSetup()
        {
            EdiabasNet ediabas = new EdiabasNet
            {
                EdInterfaceClass = _activityCommon.GetEdiabasInterfaceClass(),
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

        public class WebViewClientImpl : WebViewClientCompat
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

        public class WebChromeClientImpl : WebChromeClient
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

        class WebViewJSInterface : Java.Lang.Object
        {
            Context _context;

            public WebViewJSInterface(Context context)
            {
                _context = context;
            }

            [JavascriptInterface]
            [Export]
            public void DebugMessage(string msg)
            {
#if DEBUG
                Android.Util.Log.Debug(Tag, "Message: " + msg);
#endif
            }
        }
    }
}
