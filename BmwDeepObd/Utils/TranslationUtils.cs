using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace BmwDeepObd
{
    public static class TranslationUtils
    {
        public class YandexCloudIamTokenRequest
        {
            public YandexCloudIamTokenRequest(string oAuthToken)
            {
                OAuthToken = oAuthToken;
            }

            [JsonPropertyName("yandexPassportOauthToken")]
            public string OAuthToken { get; }
        }

        public class YandexCloudListLanguagesRequest
        {
            public YandexCloudListLanguagesRequest(string folderId = null)
            {
                FolderId = folderId;
            }

            [JsonPropertyName("folderId")]
            public string FolderId { get; }
        }

        public class YandexCloudTranslateRequest
        {
            public YandexCloudTranslateRequest(string[] textArray, string source, string target, string folderId = null)
            {
                TextArray = textArray;
                Source = source;
                Target = target;
                Format = "PLAIN_TEXT";
                FolderId = folderId;
            }

            [JsonPropertyName("texts")]
            public string[] TextArray { get; }

            [JsonPropertyName("sourceLanguageCode")]
            public string Source { get; }

            [JsonPropertyName("targetLanguageCode")]
            public string Target { get; }

            [JsonPropertyName("format")]
            public string Format { get; }

            [JsonPropertyName("folderId")]
            public string FolderId { get; }
        }

        public class DeeplTranslateRequest
        {
            public DeeplTranslateRequest(string[] textArray, string source, string target)
            {
                TextArray = textArray;
                Source = source;
                Target = target;
            }

            [JsonPropertyName("text")]
            public string[] TextArray { get; }

            [JsonPropertyName("source_lang")]
            public string Source { get; }

            [JsonPropertyName("target_lang")]
            public string Target { get; }
        }

        public class IbmTranslateRequest
        {
            public IbmTranslateRequest(string[] textArray, string source, string target)
            {
                TextArray = textArray;
                Source = source;
                Target = target;
            }

            [JsonPropertyName("text")]
            public string[] TextArray { get; }

            [JsonPropertyName("source")]
            public string Source { get; }

            [JsonPropertyName("target")]
            public string Target { get; }
        }


        public static string GetYandexTranslationError(string xmlResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return message;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return message;
                }
                XAttribute attrCode = xmlDoc.Root.Attribute("code");
                if (attrCode != null)
                {
                    errorCode = XmlConvert.ToInt32(attrCode.Value);
                }
                XAttribute attrMessage = xmlDoc.Root.Attribute("message");
                if (attrMessage != null)
                {
                    message = attrMessage.Value;
                }
                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        public static string GetIbmTranslationError(string jsonResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return message;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                if (jsonDocument.RootElement.TryGetProperty("code", out JsonElement jsonErrorCode))
                {
                    errorCode = jsonErrorCode.GetInt32();
                }

                if (jsonDocument.RootElement.TryGetProperty("error", out JsonElement jsonErrorText))
                {
                    message = jsonErrorText.GetString();
                }

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        public static string GetDeeplTranslationError(string jsonResult)
        {
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return message;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                if (jsonDocument.RootElement.TryGetProperty("message", out JsonElement jsonErrorText))
                {
                    message = jsonErrorText.GetString();
                }

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        public static string GetYandexCloudTranslationError(string jsonResult, out int errorCode)
        {
            errorCode = -1;
            string message = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return message;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                if (jsonDocument.RootElement.TryGetProperty("code", out JsonElement jsonErrorCode))
                {
                    errorCode = jsonErrorCode.GetInt32();
                }

                if (jsonDocument.RootElement.TryGetProperty("message", out JsonElement jsonErrorText))
                {
                    message = jsonErrorText.GetString();
                }

                return message;
            }
            catch (Exception)
            {
                return message;
            }
        }

        public static string GetYandexCloudIamToken(string jsonResult, out string tokenExpires)
        {
            tokenExpires = null;
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                string iamTokenString = null;
                if (jsonDocument.RootElement.TryGetProperty("iamToken", out JsonElement iamToken))
                {
                    iamTokenString = iamToken.GetString();
                }

                if (!string.IsNullOrEmpty(iamTokenString))
                {
                    if (jsonDocument.RootElement.TryGetProperty("expiresAt", out JsonElement expiresAt))
                    {
                        tokenExpires = expiresAt.GetString();
                    }
                }

                return iamTokenString;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetYandexLanguages(string xmlResult)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                XElement dirsNode = xmlDoc.Root?.Element("dirs");
                if (dirsNode == null)
                {
                    return null;
                }
                List<string> transList = new List<string>();
                foreach (XElement textNode in dirsNode.Elements("string"))
                {
                    using (XmlReader reader = textNode.CreateReader())
                    {
                        reader.MoveToContent();
                        transList.Add(reader.ReadInnerXml());
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetIbmLanguages(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("languages", out JsonElement languages))
                {
                    return null;
                }

                foreach (JsonElement translation in languages.EnumerateArray())
                {
                    if (translation.TryGetProperty("language", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetDeeplLanguages(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                foreach (JsonElement translation in jsonDocument.RootElement.EnumerateArray())
                {
                    if (translation.TryGetProperty("language", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetYandexCloudLanguages(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("languages", out JsonElement languages))
                {
                    return null;
                }

                foreach (JsonElement language in languages.EnumerateArray())
                {
                    if (language.TryGetProperty("code", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetGoogleApisLanguages()
        {
            List<string> transList = new List<string>()
            {
                "af", "sq", "am", "ar", "hy", "az", "eu", "be", "bn", "bs", "bg", "my", "ca", "ca",
                "ceb", "zh-cn", "co", "cs", "da", "nl", "nl", "en", "eo", "et", "fi", "fr", "fy", "ka",
                "de", "gd", "gd", "ga", "gl", "el", "gu", "ht", "ht", "ha", "haw", "he", "hi", "hr",
                "hu", "ig", "is", "id", "it", "jw", "ja", "kn", "kk", "km", "ky", "ky", "ko", "ku",
                "lo", "la", "lv", "lt", "lb", "lb", "mk", "ml", "mi", "mr", "ms", "mg", "mt", "mn",
                "ne", "no", "ny", "ny", "ny", "or", "pa", "pa", "fa", "pl", "pt", "ps", "ps", "ro",
                "ro", "ro", "ru", "si", "si", "sk", "sl", "sm", "sn", "sd", "so", "st", "es", "es",
                "sr", "su", "sw", "sv", "ta", "te", "tg", "tl", "th", "tr", "ug", "ug", "uk", "ur",
                "uz", "vi", "cy", "xh", "yi", "yo", "zu", "zh-CN", "zh-TW"
            };

            return transList;
        }

        public static List<string> GetYandexTranslations(string xmlResult)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlResult))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(xmlResult);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                List<string> transList = new List<string>();
                foreach (XElement textNode in xmlDoc.Root.Elements("text"))
                {
                    using (XmlReader reader = textNode.CreateReader())
                    {
                        reader.MoveToContent();
                        transList.Add(reader.ReadInnerXml());
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetIbmTranslations(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("translations", out JsonElement translations))
                {
                    return null;
                }

                foreach (JsonElement translation in translations.EnumerateArray())
                {
                    if (translation.TryGetProperty("translation", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetDeeplTranslations(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("translations", out JsonElement translations))
                {
                    return null;
                }

                foreach (JsonElement translation in translations.EnumerateArray())
                {
                    if (translation.TryGetProperty("text", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetYandexCloudTranslations(string jsonResult)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (!jsonDocument.RootElement.TryGetProperty("translations", out JsonElement translations))
                {
                    return null;
                }

                foreach (JsonElement translation in translations.EnumerateArray())
                {
                    if (translation.TryGetProperty("text", out JsonElement transElem))
                    {
                        transList.Add(transElem.GetString());
                    }
                }

                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetGoogleApisTranslations(string jsonResult, List<string> transRequestList)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonResult))
                {
                    return null;
                }

                JsonDocument jsonDocument = JsonDocument.Parse(jsonResult);
                List<string> transList = new List<string>();
                if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array || jsonDocument.RootElement.GetArrayLength() < 1)
                {
                    return null;
                }

                if (transRequestList != null)
                {
                    JsonElement element0 = jsonDocument.RootElement[0];
                    int itemCount = element0.GetArrayLength();
                    if (element0.ValueKind != JsonValueKind.Array || itemCount < 1)
                    {
                        return null;
                    }

                    int requestIndex = 0;
                    string sourceParts = string.Empty;
                    string translationParts = string.Empty;

                    for (int i = 0; i < itemCount; i++)
                    {
                        JsonElement element1 = element0[i];
                        if (element1.ValueKind != JsonValueKind.Array || element1.GetArrayLength() < 2)
                        {
                            return null;
                        }

                        if (requestIndex >= transRequestList.Count)
                        {
                            return null;
                        }

                        string requestCleaned = transRequestList[requestIndex].TrimEnd('\n').Replace(" ", string.Empty);
                        string source = element1[1].GetString() ?? string.Empty;
                        string translation = element1[0].GetString() ?? string.Empty;

                        sourceParts += source;
                        translationParts += translation;

                        string sourceCleanded = sourceParts.TrimEnd('\n').Replace(" ", string.Empty);
                        if (sourceCleanded.Length < requestCleaned.Length)
                        {
                            continue;
                        }

                        if (sourceCleanded.Length == requestCleaned.Length)
                        {
                            if (string.Compare(sourceCleanded, requestCleaned, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                return null;
                            }
                        }
                        else
                        {
#if DEBUG
                            Android.Util.Log.Info(nameof(TranslationUtils), string.Format("GetGoogleApisTranslations Mismatch: Source='{0}', Request='{1}'", sourceCleanded, requestCleaned));
#endif
                        }

                        transList.Add(translationParts.TrimEnd('\n'));
                        requestIndex++;
                        sourceParts = string.Empty;
                        translationParts = string.Empty;
                    }

                    if (requestIndex != transRequestList.Count)
                    {
                        return null;
                    }
                }
                else
                {
                    int itemCount = jsonDocument.RootElement.GetArrayLength();
                    for (int i = 0; i < itemCount; i++)
                    {
                        JsonElement element = jsonDocument.RootElement[i];
                        if (element.ValueKind != JsonValueKind.String)
                        {
                            return null;
                        }

                        string translation = element.GetString() ?? string.Empty;
                        transList.Add(translation);
                    }
                }
                return transList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsTranslationAvailable()
        {
            switch (ActivityCommon.SelectedTranslator)
            {
                case ActivityCommon.TranslatorType.YandexTranslate:
                    return !string.IsNullOrWhiteSpace(ActivityCommon.YandexApiKey);

                case ActivityCommon.TranslatorType.IbmWatson:
                    return !string.IsNullOrWhiteSpace(ActivityCommon.IbmTranslatorApiKey) && !string.IsNullOrWhiteSpace(ActivityCommon.IbmTranslatorUrl);

                case ActivityCommon.TranslatorType.Deepl:
                    return !string.IsNullOrWhiteSpace(ActivityCommon.DeeplApiKey);

                case ActivityCommon.TranslatorType.YandexCloud:
                    return !string.IsNullOrWhiteSpace(ActivityCommon.YandexCloudApiKey);

                case ActivityCommon.TranslatorType.GoogleApis:
                    return !string.IsNullOrWhiteSpace(ActivityCommon.GoogleApisUrl);
            }

            return false;
        }

        public static bool TranslationRequiresApiKey()
        {
            switch (ActivityCommon.SelectedTranslator)
            {
                case ActivityCommon.TranslatorType.GoogleApis:
                    return false;
            }

            return true;
        }

        public static bool IsYandexCloudOauthToken(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            string keyTrim = apiKey.Trim();
            if (keyTrim.Length > 40 && keyTrim[2] == '_')
            {
                return true;
            }

            return false;
        }
    }
}
