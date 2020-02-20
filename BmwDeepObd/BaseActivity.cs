using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;

namespace BmwDeepObd
{
    public class BaseActivity : AppCompatActivity
    {
        private GestureDetectorCompat _gestureDetector;
        private bool _startCalled;
        protected bool _allowTitleHiding = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ResetTitle();

            GestureListener gestureListener = new GestureListener(this);
            _gestureDetector = new GestureDetectorCompat(this, gestureListener);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (!_startCalled)
            {
                _startCalled = true;
                if (ActivityCommon.SuppressTitleBar)
                {
                    if (SupportActionBar.CustomView == null && _allowTitleHiding)
                    {
                        SupportActionBar.Hide();
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
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            _gestureDetector.OnTouchEvent(ev);
            return base.DispatchTouchEvent(ev);
        }

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(SetLocale(@base, ActivityMain.GetLocaleSetting()));
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            SetLocale(this, ActivityMain.GetLocaleSetting());
        }

        public void ResetTitle()
        {
            try
            {
                int? label = PackageManager?.GetActivityInfo(ComponentName, PackageInfoFlags.MetaData)?.LabelRes;
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

        public static Context SetLocale(Context context, string language)
        {
            try
            {
                Java.Util.Locale locale = null;
                if (string.IsNullOrEmpty(language))
                {
                    Android.Support.V4.OS.LocaleListCompat localeList =
                        Android.Support.V4.OS.ConfigurationCompat.GetLocales(Resources.System.Configuration);
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
                    configuration.Locale = locale;
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

        public static object GetInstanceState(Bundle savedInstanceState, object lastInstanceData, string key = "InstanceData")
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

        public static bool StoreInstanceState(Bundle outState, object instanceData, string key = "InstanceData")
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
            private const int topBorder = 200;
            private const int flingMinDiff = 100;
            private const int flingMinVel = 100;
            private readonly BaseActivity _activity;
            private readonly View _contentView;

            public GestureListener(BaseActivity activity)
            {
                _activity = activity;
                _contentView = _activity?.FindViewById<View>(Android.Resource.Id.Content);
            }

            public override bool OnDown(MotionEvent e)
            {
                return true;
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                if (!ActivityCommon.AutoHideTitleBar && !ActivityCommon.SuppressTitleBar)
                {
                    return true;
                }

                if (_contentView != null && e1 != null && e2 != null)
                {
                    int top = _contentView.Top;
                    float y1 = e1.GetRawY(0) - top;
                    float y2 = e2.GetRawY(0) - top;
                    if (y1 < topBorder || y2 < topBorder)
                    {
                        float diffX = e2.GetRawX(0) - e1.GetRawX(0);
                        float diffY = e2.GetRawY(0) - e1.GetRawY(0);
                        if (Math.Abs(diffX) < Math.Abs(diffY))
                        {
                            if (Math.Abs(diffY) > flingMinDiff && Math.Abs(velocityY) > flingMinVel)
                            {
                                if (diffY > 0)
                                {
                                    _activity.SupportActionBar.Show();
                                }
                                else
                                {
                                    _activity.SupportActionBar.Hide();
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }
    }
}
