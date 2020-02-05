using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V7.App;

namespace BmwDeepObd
{
    public class BaseActivity : AppCompatActivity
    {
        private const string TestLanguage = null; // "ru";

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(SetLocale(@base, TestLanguage));
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            SetLocale(this, TestLanguage);
        }

        public static Context SetLocale(Context context, string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return context;
            }

            Java.Util.Locale locale = new Java.Util.Locale(language);
            Java.Util.Locale.Default = locale;

            Resources resources = context.Resources;
            Configuration configuration = resources.Configuration;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                configuration.SetLocale(locale);
            }
            else
            {
                configuration.Locale = locale;
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
    }
}
