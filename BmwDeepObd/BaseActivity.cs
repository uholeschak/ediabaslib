using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Activity;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;

namespace BmwDeepObd
{
    public class BaseActivity : AppCompatActivity
    {
        public class InstanceDataBase
        {
            public bool ActionBarVisibilitySet
            {
                get => _actionBarVisibilitySet;
                set => _actionBarVisibilitySet = value;
            }

            public bool ActionBarVisible
            {
                get => _actionBarVisible;
                set
                {
                    _actionBarVisible = value;
                    _actionBarVisibilitySet = true;
                }
            }

            public bool LowMemoryShown { get; set; }

            public bool LongClickShown { get; set; }

            private bool _actionBarVisibilitySet;
            private bool _actionBarVisible;
        }

#if DEBUG
        private static readonly string Tag = typeof(BaseActivity).FullName;
#endif
        public const ConfigChanges ActivityConfigChanges =
            ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenLayout |
            ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize;
        private const int AutoFullScreenTimeout = 3000;
        private const int LongPressTimeout = 2000;
        public const string InstanceDataKeyDefault = "InstanceData";
        public const string InstanceDataKeyBase = "InstanceDataBase";
        protected InstanceDataBase _instanceDataBase = new InstanceDataBase();
        protected bool _actvityDestroyed;
        private GestureDetectorCompat _gestureDetector;
        protected Configuration _currentConfiguration;
        private Android.App.ActivityManager _activityManager;
        protected View _decorView;
        protected bool _allowTitleHiding = true;
        protected bool _allowFullScreenMode = true;
        protected bool _touchShowTitle = false;
        protected bool _fullScreen;
        protected bool _hasFocus;
        protected bool _autoFullScreenStarted;
        protected long _autoFullScreenStartTime;
        protected Timer _autoFullScreenTimer;
        protected Timer _memoryCheckTimer;
        protected Handler _longPressHandler;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _instanceDataBase = GetInstanceState(savedInstanceState, _instanceDataBase, InstanceDataKeyBase) as InstanceDataBase;
            }

            ResetTitle();

            GestureListener gestureListener = new GestureListener(this);
            _gestureDetector = new GestureDetectorCompat(this, gestureListener);

            BackPressCallback backPressCallback = new BackPressCallback(this);
            OnBackPressedDispatcher.AddCallback(backPressCallback);

            _currentConfiguration = Resources?.Configuration;
            _activityManager = GetSystemService(Context.ActivityService) as Android.App.ActivityManager;

            _decorView = Window?.DecorView;
            if (_decorView != null)
            {
                _decorView.SystemUiVisibilityChange += (sender, args) =>
                {
                    _fullScreen = ((SystemUiFlags) args.Visibility & SystemUiFlags.Fullscreen) != 0;
                    _autoFullScreenStarted = false;
                };
            }

            _longPressHandler = new Handler(Looper.MainLooper);

            if (_instanceDataBase != null)
            {
                if (_instanceDataBase.ActionBarVisibilitySet)
                {
                    if (_instanceDataBase.ActionBarVisible)
                    {
                        SupportActionBar.Show();
                    }
                    else
                    {
                        SupportActionBar.Hide();
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _actvityDestroyed = true;

            if (_memoryCheckTimer != null)
            {
                _memoryCheckTimer.Dispose();
                _memoryCheckTimer = null;
            }

            if (_autoFullScreenTimer != null)
            {
                _autoFullScreenTimer.Dispose();
                _autoFullScreenTimer = null;
            }

            if (_longPressHandler != null)
            {
                try
                {
                    _longPressHandler.RemoveCallbacksAndMessages(null);
                    _longPressHandler.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
                _longPressHandler = null;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceDataBase, InstanceDataKeyBase);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (!_instanceDataBase.ActionBarVisibilitySet)
            {
                _instanceDataBase.ActionBarVisible = true;
                if (ActivityCommon.SuppressTitleBar)
                {
                    if (SupportActionBar.CustomView == null && _allowTitleHiding)
                    {
                        SupportActionBar.Hide();
                        _instanceDataBase.ActionBarVisible = false;
                    }
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!ActivityCommon.AutoHideTitleBar && !ActivityCommon.SuppressTitleBar)
            {
                SupportActionBar.Show();
                _instanceDataBase.ActionBarVisible = true;
            }

            if (_memoryCheckTimer == null)
            {
                _memoryCheckTimer = new Timer(state =>
                {
                    if (_actvityDestroyed)
                    {
                        return;
                    }

                    RunOnUiThread(() =>
                    {
                        if (_actvityDestroyed)
                        {
                            return;
                        }

                        Android.App.ActivityManager.MemoryInfo memoryInfo = GetMemoryInfo();
                        if (memoryInfo != null)
                        {
                            if (memoryInfo.LowMemory && !_instanceDataBase.LowMemoryShown)
                            {
                                _instanceDataBase.LowMemoryShown = true;
                                Toast.MakeText(this, GetString(Resource.String.low_memory_detected), ToastLength.Short)?.Show();
                            }
                        }
                    });
                }, null, 2 * 1000, 10 * 1000);
            }

            if (ActivityCommon.FullScreenMode && _allowFullScreenMode)
            {
                if (_autoFullScreenTimer == null)
                {
                    _autoFullScreenTimer = new Timer(state =>
                    {
                        if (_actvityDestroyed)
                        {
                            return;
                        }

                        RunOnUiThread(() =>
                        {
                            if (_actvityDestroyed)
                            {
                                return;
                            }

                            if (_hasFocus)
                            {
                                if (_autoFullScreenStarted)
                                {
                                    if (Stopwatch.GetTimestamp() - _autoFullScreenStartTime >= AutoFullScreenTimeout * ActivityCommon.TickResolMs)
                                    {
                                        _autoFullScreenStarted = false;
                                        if (ActivityCommon.FullScreenMode && _allowFullScreenMode)
                                        {
                                            EnableFullScreenMode(true);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!_fullScreen)
                                    {
                                        _autoFullScreenStartTime = Stopwatch.GetTimestamp();
                                        _autoFullScreenStarted = true;
                                    }
                                }
                            }
                            else
                            {
                                _autoFullScreenStarted = false;
                            }
                        });
                    }, null, 500, 500);
                }
            }
            else
            {
                EnableFullScreenMode(false);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (_memoryCheckTimer != null)
            {
                _memoryCheckTimer.Dispose();
                _memoryCheckTimer = null;
            }

            if (_autoFullScreenTimer != null)
            {
                _autoFullScreenTimer.Dispose();
                _autoFullScreenTimer = null;
            }

            if (_longPressHandler != null)
            {
                _longPressHandler.RemoveCallbacksAndMessages(null);
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            _hasFocus = hasFocus;
            _autoFullScreenStarted = false;
            if (hasFocus)
            {
                if (ActivityCommon.FullScreenMode && _allowFullScreenMode)
                {
                    EnableFullScreenMode(true);
                }
                else
                {
                    EnableFullScreenMode(false);
                }
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            _gestureDetector.OnTouchEvent(ev);
#if DEBUG
            Android.Util.Log.Debug(Tag, string.Format("DispatchTouchEvent: {0}", ev.Action));
#endif
            switch (ev.Action)
            {
                case MotionEventActions.Up:
                    _longPressHandler.RemoveCallbacksAndMessages(null);
                    break;

                case MotionEventActions.Down:
                    _longPressHandler.RemoveCallbacksAndMessages(null);

                    if (ActivityCommon.AutoHideTitleBar || ActivityCommon.SuppressTitleBar)
                    {
                        _longPressHandler.PostDelayed(() =>
                        {
                            if (_actvityDestroyed)
                            {
                                return;
                            }

                            if (_touchShowTitle && !SupportActionBar.IsShowing)
                            {
                                SupportActionBar.Show();
                                _instanceDataBase.ActionBarVisible = true;
                                if (!_instanceDataBase.LongClickShown)
                                {
                                    _instanceDataBase.LongClickShown = true;
                                    Toast.MakeText(this, GetString(Resource.String.long_click_show_title), ToastLength.Short)?.Show();
                                }
                            }
                        }, LongPressTimeout);
                    }
                    break;
            }

            return base.DispatchTouchEvent(ev);
        }

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(SetLocale(@base, ActivityMain.GetLocaleSetting()));
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            _currentConfiguration = newConfig;
            SetLocale(this, ActivityMain.GetLocaleSetting());
        }

        public Android.App.ActivityManager.MemoryInfo GetMemoryInfo()
        {
            try
            {
                if (_activityManager != null)
                {
                    Android.App.ActivityManager.MemoryInfo memoryInfo = new Android.App.ActivityManager.MemoryInfo();
                    _activityManager.GetMemoryInfo(memoryInfo);
                    return memoryInfo;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public void ResetTitle()
        {
            try
            {
                int? label = ActivityCommon.GetActivityInfo(PackageManager, ComponentName, PackageInfoFlags.MetaData)?.LabelRes;
                if (label.HasValue && label != 0)
                {
                    SetTitle(label.Value);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public virtual void OnBackPressedEvent()
        {
            Finish();
        }

        public void EnableFullScreenMode(bool enable)
        {
            if (Build.VERSION.SdkInt <= BuildVersionCodes.Q)
            {
                if (_decorView != null)
                {
                    if (enable)
                    {
#pragma warning disable 618
                        _decorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Immersive | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen);
#pragma warning restore 618
                    }
                    else
                    {
#pragma warning disable 618
                        _decorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation);
#pragma warning restore 618
                    }
                }
            }
            else
            {
                if (Window != null && Window.InsetsController != null)
                {
                    if (enable)
                    {
                        if (Build.VERSION.SdkInt > BuildVersionCodes.Q)
                        {
                            Window.InsetsController.SystemBarsBehavior = (int) WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                            Window.InsetsController.SetSystemBarsAppearance((int) (WindowInsetsControllerAppearance.LightNavigationBars | WindowInsetsControllerAppearance.LightStatusBars),
                                (int)(WindowInsetsControllerAppearance.LightNavigationBars | WindowInsetsControllerAppearance.LightStatusBars));
                        }

                        Window.SetDecorFitsSystemWindows(false);
                        Window.InsetsController.Hide(WindowInsets.Type.StatusBars());
                        Window.InsetsController.Hide(WindowInsets.Type.CaptionBar());
                        Window.InsetsController.Hide(WindowInsets.Type.SystemBars());
                    }
                    else
                    {
                        if (Build.VERSION.SdkInt > BuildVersionCodes.Q)
                        {
                            Window.InsetsController.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowBarsByTouch;
                            Window.InsetsController.SetSystemBarsAppearance(0,
                                (int)(WindowInsetsControllerAppearance.LightNavigationBars | WindowInsetsControllerAppearance.LightStatusBars));
                        }

                        Window.SetDecorFitsSystemWindows(true);
                        Window.InsetsController.Show(WindowInsets.Type.StatusBars());
                        Window.InsetsController.Show(WindowInsets.Type.CaptionBar());
                        Window.InsetsController.Show(WindowInsets.Type.SystemBars());
                    }
                }
            }
        }

        public static Context SetLocale(Context context, string language)
        {
            try
            {
                Java.Util.Locale locale = null;
                if (string.IsNullOrEmpty(language))
                {
                    AndroidX.Core.OS.LocaleListCompat localeList =
                        AndroidX.Core.OS.ConfigurationCompat.GetLocales(Resources.System.Configuration);
                    if (localeList != null && localeList.Size() > 0)
                    {
                        locale = localeList.Get(0);
                    }
                }

                if (locale == null)
                {
                    locale = new Java.Util.Locale(!string.IsNullOrEmpty(language) ? language : "en");
                }

                Java.Util.Locale.Default = locale;

                Resources resources = context.Resources;
                Configuration configuration = resources.Configuration;
                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
                {
#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    configuration.Locale = locale;
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                }
                else
                {
                    configuration.SetLocale(locale);
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
                {
#pragma warning disable 618
                    resources.UpdateConfiguration(configuration, resources.DisplayMetrics);
#pragma warning restore 618
                    return context;
                }

                return context.CreateConfigurationContext(configuration);
            }
            catch (Exception)
            {
                return context;
            }
        }

        public static object GetInstanceState(Bundle savedInstanceState, object lastInstanceData, string key = InstanceDataKeyDefault)
        {
            if (savedInstanceState != null)
            {
                try
                {
                    string xml = savedInstanceState.GetString(key, string.Empty);
                    if (!string.IsNullOrEmpty(xml))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(lastInstanceData.GetType());
                        using (StringReader sr = new StringReader(xml))
                        {
                            object instanceData = xmlSerializer.Deserialize(sr);
                            if (instanceData.GetType() == lastInstanceData.GetType())
                            {
                                return instanceData;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return lastInstanceData;
        }

        public static bool StoreInstanceState(Bundle outState, object instanceData, string key = InstanceDataKeyDefault)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(instanceData.GetType());
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sw))
                    {
                        xmlSerializer.Serialize(writer, instanceData);
                        string xml = sw.ToString();
                        outState.PutString(key, xml);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        private class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            private const int flingMinDiff = 100;
            private const int flingMinVel = 100;
            private readonly BaseActivity _activity;
            private readonly View _contentView;
            private readonly int _topBorder;

            public GestureListener(BaseActivity activity)
            {
                _activity = activity;
                _contentView = _activity?.FindViewById<View>(Android.Resource.Id.Content);
                _topBorder = 200;
                if (activity != null)
                {
                    float yDpi = activity.Resources.DisplayMetrics.Ydpi;
                    _topBorder = (int)yDpi / 2;
                }
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                if (_activity._actvityDestroyed)
                {
                    return true;
                }

                if (!ActivityCommon.AutoHideTitleBar && !ActivityCommon.SuppressTitleBar)
                {
                    return true;
                }

                if (_contentView != null && e1 != null && e2 != null)
                {
                    int[] location = new int[2];
                    _contentView.GetLocationOnScreen(location);
                    int top = location[1];
                    float y1 = e1.RawY - top;
                    float y2 = e2.RawY - top;

                    if (y1 < _topBorder || y2 < _topBorder)
                    {
                        float diffX = e2.RawX - e1.RawX;
                        float diffY = e2.RawY - e1.RawY;
                        if (Math.Abs(diffX) < Math.Abs(diffY))
                        {
                            if (Math.Abs(diffY) > flingMinDiff && Math.Abs(velocityY) > flingMinVel)
                            {
                                if (diffY > 0)
                                {
                                    _activity.SupportActionBar.Show();
                                    _activity._instanceDataBase.ActionBarVisible = true;
                                }
                                else
                                {
                                    _activity.SupportActionBar.Hide();
                                    _activity._instanceDataBase.ActionBarVisible = false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }

        public class BackPressCallback : OnBackPressedCallback
        {
            private readonly BaseActivity _baseActivity;

            public BackPressCallback(BaseActivity baseActivity) : base(true)
            {
                _baseActivity = baseActivity;
            }

            public override void HandleOnBackPressed()
            {
                _baseActivity.OnBackPressedEvent();
            }
        }
    }
}
