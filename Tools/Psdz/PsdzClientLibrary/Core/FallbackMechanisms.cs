using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PsdzClient.Core
{
    public class FallbackMechanisms
    {
        private readonly DataHolder dataHolder;
        public FallbackMechanisms(DataHolder dataHolder)
        {
            this.dataHolder = dataHolder;
        }

        public void ProductionDate(Action<DateTime> setFallback, string productionDatePropName, string modelljahrPropName, string modellmonatPropName, string modelltagPropName)
        {
            IList<PropertyData<DateTime>> propertyCollection = dataHolder.GetPropertyCollection<DateTime>(productionDatePropName);
            if (propertyCollection.Count != 0 && propertyCollection.First().Source != DataSource.Fallback && !propertyCollection.All((PropertyData<DateTime> d) => !d.IsValidValue))
            {
                return;
            }

            IList<PropertyData<string>> propertyCollection2 = dataHolder.GetPropertyCollection<string>(modelljahrPropName);
            IList<PropertyData<string>> propertyCollection3 = dataHolder.GetPropertyCollection<string>(modellmonatPropName);
            if (int.TryParse(propertyCollection2.FirstOrDefault()?.Value, out var result) && int.TryParse(propertyCollection3.FirstOrDefault()?.Value, out var result2))
            {
                int.TryParse(dataHolder.GetPropertyCollection<string>(modelltagPropName).FirstOrDefault()?.Value, out var result3);
                if (result3 == 0)
                {
                    result3 = 1;
                }

                setFallback(new DateTime(result, result2, result3));
            }
        }

        public void HandleVin17Fallbacks(Action<string> setBasicType, Action<string> setVinRangeType, string vin17PropName, string basicTypePropName, string vinRangeTypePropName)
        {
            PropertyData<string> propertyData = dataHolder.GetPropertyCollection<string>(vin17PropName).FirstOrDefault();
            if (propertyData == null || string.IsNullOrEmpty(propertyData.Value))
            {
                return;
            }

            string text = propertyData.Value.Substring(3, 4);
            if (!string.IsNullOrEmpty(text))
            {
                string text2 = text.Substring(0, 3);
                switch (text[3])
                {
                    case 'A':
                        text2 += "1";
                        break;
                    case 'B':
                        text2 += "2";
                        break;
                    case 'C':
                        text2 += "3";
                        break;
                    case 'D':
                        text2 += "4";
                        break;
                    case 'E':
                        text2 += "5";
                        break;
                    case 'F':
                        text2 += "6";
                        break;
                    case 'G':
                        text2 += "7";
                        break;
                    case 'H':
                        text2 += "8";
                        break;
                    case 'J':
                        text2 += "9";
                        break;
                    default:
                        text2 = text;
                        break;
                }

                IList<PropertyData<string>> propertyCollection = dataHolder.GetPropertyCollection<string>(basicTypePropName);
                IList<PropertyData<string>> propertyCollection2 = dataHolder.GetPropertyCollection<string>(vinRangeTypePropName);
                if (propertyCollection.Count == 0 || propertyCollection.First().Source == DataSource.Fallback || propertyCollection.All((PropertyData<string> t) => !t.IsValidValue))
                {
                    setBasicType(text2);
                }

                if (propertyCollection2.Count == 0 || propertyCollection2.First().Source == DataSource.Fallback || propertyCollection2.All((PropertyData<string> t) => !t.IsValidValue))
                {
                    setVinRangeType(text2);
                }
            }
        }

        public void HandleILevelWerkFallbacks(Action<string> setModelJahr, Action<string> setModelMonat, string iLevelWerkPropName, string modellmonatPropName, string modelljahrPropName)
        {
            PropertyData<string> propertyData = dataHolder.GetPropertyCollection<string>(iLevelWerkPropName).FirstOrDefault();
            if (propertyData == null || string.IsNullOrEmpty(propertyData.Value) || !Regex.IsMatch(propertyData.Value, "^\\w{4}[_\\-]\\d{2}[_\\-]\\d{2}[_\\-]\\d{3}$"))
            {
                return;
            }

            int num = propertyData.Value.IndexOf("-");
            if (num >= 0)
            {
                string text = propertyData.Value.Substring(num + 1, 5);
                string value = text.Substring(0, 2);
                string obj = text.Substring(3, 2);
                IList<PropertyData<string>> propertyCollection = dataHolder.GetPropertyCollection<string>(modellmonatPropName);
                IList<PropertyData<string>> propertyCollection2 = dataHolder.GetPropertyCollection<string>(modelljahrPropName);
                if (propertyCollection.Count == 0 || propertyCollection.First().Source == DataSource.Fallback || propertyCollection.All((PropertyData<string> m) => !m.IsValidValue))
                {
                    setModelMonat(obj);
                }

                if (propertyCollection2.Count == 0 || propertyCollection2.First().Source == DataSource.Fallback || propertyCollection2.All((PropertyData<string> j) => !j.IsValidValue))
                {
                    int num2 = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    setModelJahr(((num2 <= 50) ? (num2 + 2000) : (num2 + 1900)).ToString());
                }
            }
        }
    }
}