using System;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V7.App;

namespace BmwDeepObd
{
    public class BaseActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ResetTitle();
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
    }
}
