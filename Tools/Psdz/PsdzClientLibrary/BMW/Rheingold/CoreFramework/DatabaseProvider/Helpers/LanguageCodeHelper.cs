using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.Helpers
{
    public class LanguageCodeHelper
    {
        public static readonly List<string> LanguageCodes = new List<string>
    {
        "cs-CZ", "de-DE", "es-ES", "en-GB", "el-GR", "en-US", "fr-FR", "it-IT", "ja-JP", "ko-KR",
        "nl-NL", "pl-PL", "pt-PT", "ru-RU", "sv-SE", "th-TH", "tr-TR", "zh-CN", "zh-TW"
    };

        public static string GetCurrentLanguageTitleColumnName()
        {
            return "TITLE_" + GetCurrentLanguageCode();
        }

        public static string GetCurrentLanguageCode()
        {
            return GetShortLanguageCode(ConfigSettings.CurrentUICulture);
        }

        public static string GetShortLanguageCode(string code)
        {
            switch (code)
            {
                case "cs-CZ":
                    return "CSCZ";
                case "de-DE":
                    return "DEDE";
                case "es-ES":
                    return "ES";
                case "en-GB":
                    return "ENGB";
                case "el-GR":
                    return "EL";
                case "en-US":
                    return "ENUS";
                case "fr-FR":
                    return "FR";
                case "it-IT":
                    return "IT";
                case "ja-JP":
                    return "JA";
                case "ko-KR":
                    return "KO";
                case "nl-NL":
                    return "NL";
                case "pl-PL":
                    return "PLPL";
                case "pt-PT":
                    return "PT";
                case "ru-RU":
                    return "RU";
                case "sv-SE":
                    return "SV";
                case "th-TH":
                    return "TH";
                case "tr-TR":
                    return "TR";
                case "zh-CN":
                    return "ZHCN";
                case "zh-TW":
                    return "ZHTW";
                default:
                    Log.Warning("GetShortLanguageCode", "The language {0} not available - returning en-GB", code);
                    return "ENGB";
            }
        }

        public static string GetLongLanguageCode(string language)
        {
            switch (language)
            {
                case "CSCZ":
                    return "cs-CZ";
                case "DEDE":
                    return "de-DE";
                case "ES":
                    return "es-ES";
                case "ENGB":
                    return "en-GB";
                case "EL":
                    return "el-GR";
                case "ENUS":
                    return "en-US";
                case "FR":
                    return "fr-FR";
                case "IT":
                    return "it-IT";
                case "JA":
                    return "ja-JP";
                case "KO":
                    return "ko-KR";
                case "NL":
                    return "nl-NL";
                case "PLPL":
                    return "pl-PL";
                case "PT":
                    return "pt-PT";
                case "RU":
                    return "ru-RU";
                case "SV":
                    return "sv-SE";
                case "TH":
                    return "th-TH";
                case "TR":
                    return "tr-TR";
                case "ZHCN":
                    return "zh-CN";
                case "ZHTW":
                    return "zh-TW";
                default:
                    Log.Warning("GetCultureCodeByLanguage", "language {0} not available - returning en-GB", language);
                    return "en-GB";
            }
        }
    }
}
