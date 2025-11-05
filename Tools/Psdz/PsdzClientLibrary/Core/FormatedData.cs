using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using System;

namespace PsdzClient.Core
{
    [Serializable]
    public class FormatedData : BaseFormatedData
    {
        private static Translator translator;
        private bool useTranslatedValuesAsOnePlaceHolder;
        private string seperatorForValuesAsOnPlacholder;
        private object[] values;
        private string fmtStrIdModuleName = "ISTAGui";
        [XmlIgnore]
        public object[] Values
        {
            get
            {
                return values;
            }

            set
            {
                if (value != values)
                {
                    values = value;
                    OnPropertyChanged("Values");
                }
            }
        }

        [XmlIgnore]
        public bool TranslateValues
        {
            get
            {
                return base.translateValues;
            }

            set
            {
                base.translateValues = value;
            }
        }

        [XmlIgnore]
        public string ModuleName
        {
            get
            {
                return fmtStrIdModuleName;
            }

            set
            {
                fmtStrIdModuleName = value;
            }
        }

        public FormatedData()
        {
            values = new object[0];
        }

        public FormatedData(string fmtStrId, params object[] values) : this(fmtStrId, translateValues: false, values)
        {
        }

        public FormatedData(string fmtStrId, bool translateValues, params object[] values)
        {
            this.values = values ?? new object[0];
            base.translateValues = translateValues;
            base.fmtStrId = fmtStrId;
        }

        public FormatedData(string fmtStrId, bool translateValues, bool useTranslatedValuesAsOnePlaceHolder, string seperatorForValuesAsOnPlacholder, params object[] values) : this(fmtStrId, translateValues, values)
        {
            this.useTranslatedValuesAsOnePlaceHolder = useTranslatedValuesAsOnePlaceHolder;
            this.seperatorForValuesAsOnPlacholder = seperatorForValuesAsOnPlacholder;
        }

        public static string Localize(string fmtStrId)
        {
            return Localize(fmtStrId, "ISTAGui", false);
        }

        public static string Localize(string fmtStrId, string moduleName, bool translateValues, params object[] values)
        {
            try
            {
                return new FormatedData(fmtStrId, translateValues, values)
                {
                    ModuleName = moduleName
                }.Localize();
            }
            catch (Exception exception)
            {
                Log.WarningException("FormatedData.Localize(string fmtStrId, string moduleName, bool translateValues, params object[] values)", exception);
            }

            return null;
        }

        public string Localize()
        {
            return BuildLocalizedMessage(new CultureInfo(ConfigSettings.CurrentUICulture));
        }

        public string Localize(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("Parameter culture must not be null.");
            }

            return BuildLocalizedMessage(culture);
        }

        public IList<LocalizedText> Localize(IList<string> lang)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            list.AddRange(lang.Select((string x) => new LocalizedText(Localize(new CultureInfo(x)), x)));
            return list;
        }

        private string BuildLocalizedMessage(CultureInfo culture)
        {
            try
            {
                if (translator == null)
                {
                    translator = new Translator();
                    translator.ResourceName = "BMW.Rheingold.CoreFramework.Localization.Localization.xml";
                }

                return Translate(translator, this, culture);
            }
            catch (Exception ex)
            {
                string text = ((!string.IsNullOrEmpty(base.fmtStrId)) ? ("<" + base.fmtStrId + ">") : "No message defined");
                Log.Error("FormatedData.BuildLocalizedMessage()", "Failed to localize \"{0}\", returning \"{1}\". Reason: {2}", base.fmtStrId, text, ex);
                return text;
            }
        }

        private string Translate(Translator translator, FormatedData fmtStr, CultureInfo culture)
        {
            string name = translator.GetName(fmtStr.fmtStrId, ModuleName, culture.Name);
            List<string> list = fmtStr.values?.Select((object c) => (c != null) ? c.ToString() : "").ToList();
            object[] array;
            if (fmtStr.TranslateValues)
            {
                list.Clear();
                array = fmtStr.Values;
                foreach (object obj in array)
                {
                    list.Add(translator.GetName(obj.ToString(), ModuleName, culture.Name));
                }
            }

            if (useTranslatedValuesAsOnePlaceHolder)
            {
                return string.Format(name, string.Join(seperatorForValuesAsOnPlacholder, list.ToArray()));
            }

            array = list.ToArray();
            return string.Format(name, array);
        }
    }
}