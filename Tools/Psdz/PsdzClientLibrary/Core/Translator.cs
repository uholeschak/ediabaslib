using PsdzClient.Core.Container;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System;
using System.Globalization;
using System.Linq;

namespace PsdzClient.Core
{
    public class Translator
    {
        private static HardenedStringObjectDictionary localizationCache = new HardenedStringObjectDictionary();
        private string fileName;
        private string resourceName;
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    if (!localizationCache.Keys.Contains(fileName))
                    {
                        localizationCache.Add(fileName, DeserializeLocalization(fileName));
                    }
                }
            }
        }

        public string ResourceName
        {
            get
            {
                return resourceName;
            }

            set
            {
                if (resourceName != value)
                {
                    resourceName = value;
                    if (!localizationCache.Keys.Contains(resourceName))
                    {
                        localizationCache.Add(resourceName, DeserializeLocalizationByResource(resourceName));
                    }
                }
            }
        }

        private Localization Localization
        {
            get
            {
                if (localizationCache.Keys.Contains(ResourceName))
                {
                    return localizationCache[ResourceName] as Localization;
                }

                return null;
            }
        }

        public Translator()
        {
        }

        public Translator(string fileName)
        {
            FileName = fileName;
        }

        public IList<string> GetIds(string module)
        {
            IList<string> list = new List<string>();
            try
            {
                Localization.TextRow[] textRows = Localization.Language.AsQueryable().First((Localization.LanguageRow item) => item.ModuleRow.name == module).GetTextRows();
                foreach (Localization.TextRow textRow in textRows)
                {
                    list.AddIfNotContains(textRow.id);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Translator.GetIds()", "Exception occurred for '{0}': {1}", module, ex.ToString());
            }

            return list;
        }

        public string GetId(string nameStr, string module)
        {
            if (string.IsNullOrEmpty(nameStr))
            {
                return null;
            }

            nameStr = nameStr.Replace("\n", "\\n");
            bool flag = false;
            string result = nameStr;
            string cultureStr = ConfigSettings.CurrentUICulture;
            while (!flag)
            {
                try
                {
                    result = Localization.Text.AsQueryable().First((Localization.TextRow item) => item.name == nameStr && item.LanguageRow.culture == cultureStr && item.LanguageRow.ModuleRow.name == module).id;
                    flag = true;
                }
                catch (Exception)
                {
                    try
                    {
                        Localization.LanguageRow languageRow = Localization.Language.AsQueryable().First((Localization.LanguageRow item) => item.culture == cultureStr && item.ModuleRow.name == module);
                        if (cultureStr == languageRow.defCulture)
                        {
                            flag = true;
                        }

                        cultureStr = languageRow.defCulture;
                    }
                    catch (Exception)
                    {
                        flag = true;
                    }
                }
            }

            return result;
        }

        public string GetName(string idStr, string module, string uiCulture)
        {
            if (string.IsNullOrEmpty(idStr))
            {
                return string.Empty;
            }

            try
            {
                Localization.TextRow textRow = Localization.Text.FirstOrDefault((Localization.TextRow item) => string.Equals(item.id, idStr) && item.LanguageRow != null && string.Equals(item.LanguageRow.culture, uiCulture) && item.LanguageRow.ModuleRow != null && string.Equals(item.LanguageRow.ModuleRow.name, module));
                if (textRow != null && !string.IsNullOrEmpty(textRow.name))
                {
                    return textRow.name.Replace("\\n", "\n");
                }

                Localization.LanguageRow language = Localization.Language.FirstOrDefault((Localization.LanguageRow item) => string.Equals(item.culture, uiCulture) && item.ModuleRow != null && string.Equals(item.ModuleRow.name, module));
                if (language != null)
                {
                    textRow = Localization.Text.FirstOrDefault((Localization.TextRow item) => string.Equals(item.id, idStr) && item.LanguageRow != null && string.Equals(item.LanguageRow.culture, language.defCulture) && item.LanguageRow.ModuleRow != null && string.Equals(item.LanguageRow.ModuleRow.name, module));
                    if (textRow != null && !string.IsNullOrEmpty(textRow.name))
                    {
                        return textRow.name.Replace("\\n", "\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Translator.GetName()", "Exception occurred for '{0}': {1}", idStr, ex.ToString());
            }

            return idStr.Replace("\\n", "\n");
        }

        public string getName(string idStr, string module)
        {
            return GetName(idStr, module, ConfigSettings.CurrentUICulture);
        }

        public string translate(FormatedData fmtStr, string module)
        {
            try
            {
                string name = getName(fmtStr.fmtStrId, module);
                if (fmtStr.TranslateValues)
                {
                    List<string> list = new List<string>();
                    object[] values = fmtStr.Values;
                    foreach (object obj in values)
                    {
                        list.Add(getName(obj.ToString(), module));
                    }

                    values = list.ToArray();
                    return string.Format(name, values);
                }

                return string.Format(name, fmtStr.Values);
            }
            catch (Exception exception)
            {
                Log.WarningException("Translator.translate()", exception);
            }

            return null;
        }

        private Localization DeserializeLocalization(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    Log.Warning("Translator.deserializeLocalization()", "fileName was null or empty");
                    return null;
                }

                if (CoreFramework.DebugLevel > 0)
                {
                    Log.Debug("Translator.deserializeLocalization()", "trying to load localization file: {0}", fileName);
                }

                if (File.Exists(fileName))
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        Localization result = new XmlSerializer(typeof(Localization)).Deserialize(fileStream) as Localization;
                        fileStream.Close();
                        return result;
                    }
                }

                Log.Warning("Translator.deserializeLocalization()", "localization file not found");
            }
            catch (Exception exception)
            {
                Log.WarningException("Translator.deserializeLocalization()", exception);
            }

            return null;
        }

        private Localization DeserializeLocalizationByResource(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                Log.Warning("Translator.deserializeLocalization()", "resourceName was null or empty");
                return null;
            }

            if (CoreFramework.DebugLevel > 0)
            {
                Log.Debug("Translator.deserializeLocalizationbyResource()", "trying to load localization resource by name: {0}", resourceName);
            }

            try
            {
                Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (manifestResourceStream != null)
                {
                    return new XmlSerializer(typeof(Localization)).Deserialize(manifestResourceStream) as Localization;
                }

                Log.Info("Translator.deserializeLocalizationbyResource()", "resource stream was null; check your uri: {0}", resourceName);
                string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                foreach (string text in manifestResourceNames)
                {
                    Log.Info("Translator.deserializeLocalizationbyResource()", "available resource: {0}", text);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Translator.deserializeLocalizationbyResource()", exception);
            }

            return null;
        }
    }
}