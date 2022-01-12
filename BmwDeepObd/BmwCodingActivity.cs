using System;
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

        private enum ActivityRequest
        {
            RequestDevelopmentSettings,
        }

        private InstanceData _instanceData = new InstanceData();
        private ActivityCommon _activityCommon;
        private EdWebServer _edWebServer;
        private Thread _jobThread;
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

            _webViewCoding = FindViewById<WebView>(Resource.Id.webViewCoding);

            try
            {
                WebSettings webSettings = _webViewCoding?.Settings;
                if (webSettings != null)
                {
                    webSettings.JavaScriptEnabled = true;
                    webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                }

                _webViewCoding.AddJavascriptInterface(new WebViewJSInterface(this), "deepObd");
                _webViewCoding.SetWebViewClient(new WebViewClientImpl(this));
                _webViewCoding.LoadUrl(@"https:://www.holeschak.de");
            }
            catch (Exception)
            {
                // ignored
            }

            StartWebServer();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (IsJobRunning())
            {
                _jobThread.Join();
            }

            StopWebServer();

            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
            if (IsJobRunning())
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

        private bool IsJobRunning()
        {
            if (_jobThread == null)
            {
                return false;
            }
            if (_jobThread.IsAlive)
            {
                return true;
            }
            _jobThread = null;
            return false;
        }

        private bool StartWebServer()
        {
            try
            {
                EdiabasNet ediabas = EdiabasSetup();
                _edWebServer = new EdWebServer(ediabas, null);
                _edWebServer.StartTcpListener("http://127.0.0.1:8080");
                return true;
            }
            catch (Exception)
            {
                return false;
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

        class WebViewJSInterface : Java.Lang.Object
        {
#if DEBUG
            private static readonly string Tag = typeof(WebViewJSInterface).FullName;
#endif
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
