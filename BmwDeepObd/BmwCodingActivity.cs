using System;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.WebKit;
using AndroidX.DocumentFile.Provider;
using BmwDeepObd.FilePicker;
using Android.Webkit;

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
        public const string ExtraAppDataDir = "app_data_dir";

        private enum ActivityRequest
        {
            RequestDevelopmentSettings,
        }

        private InstanceData _instanceData = new InstanceData();
        private string _appDataDir;
        private ActivityCommon _activityCommon;

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
            _appDataDir = Intent.GetStringExtra(ExtraAppDataDir);

            _activityCommon = new ActivityCommon(this);

            _webViewCoding = FindViewById<WebView>(Resource.Id.webViewCoding);

            try
            {
                WebSettings webSettings = _webViewCoding?.Settings;
                if (webSettings != null)
                {
                    webSettings.JavaScriptEnabled = true;
                    webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                }

                _webViewCoding.SetWebViewClient(new WebViewClientImpl(this));
                _webViewCoding.LoadUrl(@"https:://www.holeschak.de");
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
            _activityCommon?.Dispose();
            _activityCommon = null;
        }

        public override void OnBackPressed()
        {
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
        }
    }
}
