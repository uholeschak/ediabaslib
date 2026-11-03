using System;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;

namespace BmwDeepObd
{
    public static class LocaleUtils
    {
        public static Java.Util.Locale GetJavaLocale(string locale)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Baklava)
            {
#pragma warning disable CA1416
                return Java.Util.Locale.Of(locale);
#pragma warning restore CA1416
            }
#pragma warning disable CA1422
            return new Java.Util.Locale(locale);
#pragma warning restore CA1422
        }

        public static string GetCurrentLanguageStatic(bool iso3 = false, Context context = null)
        {
            try
            {
                Java.Util.Locale locale = Java.Util.Locale.Default;
                string selectedLocale = GetLocaleSetting(context);
                if (!string.IsNullOrEmpty(selectedLocale))
                {
                    locale = GetJavaLocale(selectedLocale);
                }

                string language = locale?.Language;
                if (iso3)
                {
                    string iso3Language = locale?.ISO3Language;
                    if (!string.IsNullOrEmpty(iso3Language))
                    {
                        language = iso3Language;
                    }
                }

                return language ?? ActivityCommon.DefaultLang;
            }
            catch (Exception)
            {
                return ActivityCommon.DefaultLang;
            }
        }

        public static string GetLocaleSetting(Context context = null, bool updateRecent = false)
        {
            try
            {
                bool useRecent = false;
                string languageTags = null;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
                    if (context != null)
                    {
#pragma warning disable CA1416
                        if (context.ApplicationContext?.GetSystemService(Java.Lang.Class.FromType(typeof(Android.App.LocaleManager))) is Android.App.LocaleManager localeManager)
                        {
                            LocaleList appLocales = localeManager.ApplicationLocales;
                            languageTags = appLocales.ToLanguageTags();
                        }
#pragma warning restore CA1416
                    }
                }

                if (languageTags == null)
                {
                    AndroidX.Core.OS.LocaleListCompat appLocales = AppCompatDelegate.ApplicationLocales;
                    languageTags = appLocales.ToLanguageTags();
                    useRecent = true;
                }

                if (!string.IsNullOrEmpty(languageTags))
                {
                    string[] languages = languageTags.Split(',');
                    if (languages.Length > 0)
                    {
                        string language = languages[0];
                        if (language.Length > 2)
                        {
                            language = language.Substring(0, 2);
                        }

                        if (language.Length == 2)
                        {
                            if (updateRecent)
                            {
                                ActivityCommon.RecentLocale = language;
                            }
                            return language;
                        }
                    }
                }

                if (updateRecent)
                {
                    ActivityCommon.RecentLocale = string.Empty;
                    return string.Empty;
                }

                if (useRecent && ActivityCommon.RecentLocale != null)
                {
                    return ActivityCommon.RecentLocale;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static Context GetLocaleContext(Context context)
        {
            try
            {
                string selectedLocale = GetLocaleSetting(context);
                if (string.IsNullOrEmpty(selectedLocale))
                {
                    return context;
                }

                Java.Util.Locale locale = GetJavaLocale(selectedLocale);
                Resources resources = context.Resources;
                Configuration configuration = resources?.Configuration;
                if (configuration != null)
                {
                    configuration.SetLocale(locale);
                    return context.CreateConfigurationContext(configuration);
                }

                return context;
            }
            catch (Exception)
            {
                return context;
            }
        }
    }
}
