using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PsdzClient;
using PsdzClientLibrary;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public static class DatabaseUtil
    {
        public static string CreateInClause(IEnumerable<string> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append("'").Append(value.Trim()).Append("'");
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<long> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (long value in values)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<int> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (int value2 in values)
            {
                long value = value2;
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<decimal> values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (decimal value in values)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(value);
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<XEP_DIAGNOSISOBJECTSEX> listDiagObjs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (XEP_DIAGNOSISOBJECTSEX listDiagObj in listDiagObjs)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(listDiagObj.Id);
            }

            return stringBuilder.ToString();
        }

        public static IDictionary<decimal, decimal?> CreateDictionaryForRuleEvaluation(IEnumerable<XEP_DIAGNOSISOBJECTSEX> listDiagObjs)
        {
            Dictionary<decimal, decimal?> dictionary = new Dictionary<decimal, decimal?>();
            foreach (XEP_DIAGNOSISOBJECTSEX listDiagObj in listDiagObjs)
            {
                dictionary[listDiagObj.Id] = listDiagObj.ControlId;
            }

            return dictionary;
        }

        public static string CreateInClause(IEnumerable<DiagnosticObject> listDiagObjs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DiagnosticObject listDiagObj in listDiagObjs)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(listDiagObj.Id);
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<XEP_PERCEIVEDSYMPTOMSEX> listSymptoms)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (XEP_PERCEIVEDSYMPTOMSEX listSymptom in listSymptoms)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(listSymptom.Id);
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<RG_ECUFAULTS> listFaults)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (RG_ECUFAULTS listFault in listFaults)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(listFault.EcuFault_id);
            }

            return stringBuilder.ToString();
        }

        public static string CreateInClause(IEnumerable<IXepInfoObject> setInfoObjs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (IXepInfoObject setInfoObj in setInfoObjs)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(setInfoObj.Id);
            }

            return stringBuilder.ToString();
        }

        public static IDictionary<decimal, decimal?> CreateDictionaryForRuleEvaluation(IEnumerable<IXepInfoObject> setInfoObjs)
        {
            Dictionary<decimal, decimal?> dictionary = new Dictionary<decimal, decimal?>();
            foreach (IXepInfoObject setInfoObj in setInfoObjs)
            {
                dictionary[setInfoObj.Id] = setInfoObj.ControlId;
            }

            return dictionary;
        }

        public static string CreateInClause(IEnumerable<InfoObject> listDocuments)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (InfoObject listDocument in listDocuments)
            {
                if (!string.IsNullOrEmpty(stringBuilder.ToString()))
                {
                    stringBuilder.Append(",");
                }

                stringBuilder.Append(listDocument.Id);
            }

            return stringBuilder.ToString();
        }

        public static string Id2String(IEnumerable<IXepInfoObject> value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (IXepInfoObject item in value)
            {
                stringBuilder.Append(item.Id).Append(",");
            }

            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;
            }

            return stringBuilder.ToString();
        }

        public static string Long2String(IEnumerable<long> value)
        {
            return string.Join(",", value);
        }

        public static string Decimal2String(IEnumerable<decimal> value)
        {
            return string.Join(",", value);
        }

        public static string GetMajorVersionNumberFromVersionString(string versionString)
        {
            string text = versionString;
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }

            text = text.Trim();
            int num = text.LastIndexOf(".", StringComparison.Ordinal);
            if (num > 1)
            {
                text = text.Substring(0, num);
            }

            return text;
        }

        public static string GetPatchVersionNumberFromVersionString(string versionString)
        {
            string text = versionString;
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;
            }

            text = text.Trim();
            int num = text.LastIndexOf(".", StringComparison.Ordinal);
            if (num > 1)
            {
                text = text.Substring(num + 1);
            }

            return text;
        }

        public static void ExtractInfoTypeAndAbbreviation(string mixedInfoType, XEP_WSCONFIGFILTERS filters)
        {
            int num = mixedInfoType.LastIndexOf("(", StringComparison.Ordinal);
            if (num == -1)
            {
                return;
            }

            int num2 = mixedInfoType.LastIndexOf(")", StringComparison.Ordinal);
            if (num2 != -1)
            {
                int num3 = mixedInfoType.LastIndexOf(" ", StringComparison.Ordinal);
                if (num3 != -1 && num2 - num != 1)
                {
                    filters.Abbreviation = mixedInfoType.Substring(num + 1, num2 - num - 1);
                    filters.InfoType = mixedInfoType.Substring(0, num3);
                }
            }
        }

        public static ICollection<XEP_WSCONFIGFILTERS> CopyAndDeleteAbbreviation(ICollection<XEP_WSCONFIGFILTERS> listFilters)
        {
            HashSet<XEP_WSCONFIGFILTERS> hashSet = new HashSet<XEP_WSCONFIGFILTERS>();
            foreach (XEP_WSCONFIGFILTERS listFilter in listFilters)
            {
                hashSet.Add(CopyAndDeleteAbbreviation(listFilter));
            }

            return hashSet;
        }

        public static XEP_WSCONFIGFILTERS CopyAndDeleteAbbreviation(XEP_WSCONFIGFILTERS wsConfigFilter)
        {
            XEP_WSCONFIGFILTERS xEP_WSCONFIGFILTERS = wsConfigFilter.Clone();
            xEP_WSCONFIGFILTERS.Title_dede = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_dede);
            xEP_WSCONFIGFILTERS.Title_el = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_el);
            xEP_WSCONFIGFILTERS.Title_engb = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_engb);
            xEP_WSCONFIGFILTERS.Title_enus = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_enus);
            xEP_WSCONFIGFILTERS.Title_es = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_es);
            xEP_WSCONFIGFILTERS.Title_fr = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_fr);
            xEP_WSCONFIGFILTERS.Title_id = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_id);
            xEP_WSCONFIGFILTERS.Title_it = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_it);
            xEP_WSCONFIGFILTERS.Title_ja = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_ja);
            xEP_WSCONFIGFILTERS.Title_ko = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_ko);
            xEP_WSCONFIGFILTERS.Title_nl = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_nl);
            xEP_WSCONFIGFILTERS.Title_pt = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_pt);
            xEP_WSCONFIGFILTERS.Title_ru = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_ru);
            xEP_WSCONFIGFILTERS.Title_sv = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_sv);
            xEP_WSCONFIGFILTERS.Title_th = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_th);
            xEP_WSCONFIGFILTERS.Title_tr = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_tr);
            xEP_WSCONFIGFILTERS.Title_zhcn = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_zhcn);
            xEP_WSCONFIGFILTERS.Title_zhtw = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_zhtw);
            xEP_WSCONFIGFILTERS.Title_cscz = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_cscz);
            xEP_WSCONFIGFILTERS.Title_plpl = ExtractInfoType(xEP_WSCONFIGFILTERS.Title_plpl);
            return xEP_WSCONFIGFILTERS;
        }

        public static long ConvertIntoLong(string value)
        {
            long result = 0L;
            try
            {
                result = Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                Log.Warning("DatabaseUtil.ConvertIntoLong()", "Trying to convert '{0}' failed with exception: {1}", value, ex);
            }

            return result;
        }

        public static decimal ConvertIntoDecimal(string value)
        {
            decimal result = 0m;
            try
            {
                result = Convert.ToDecimal(value);
                return result;
            }
            catch (Exception ex)
            {
                Log.Warning("DatabaseUtil.ConvertIntoDecimal()", "Trying to convert '{0}' failed with exception: {1}", value, ex);
            }

            return result;
        }

        public static long ConvertIntoLong(decimal value)
        {
            long result = 0L;
            try
            {
                result = Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                Log.Warning("DatabaseUtil.ConvertIntoDecimal()", "Trying to convert '{0}' failed with exception: {1}", value, ex);
            }

            return result;
        }

        public static string RemoveIllegalCharacters(string inputText)
        {
            string input = inputText.Trim();
            try
            {
                input = Regex.Replace(input, "[';\"\\\\(\\\\)\\\\]", string.Empty, RegexOptions.None);
                return input.Trim();
            }
            catch (Exception exception)
            {
                Log.ErrorException("DatabaseUtil.RemoveIllegalCharacters()", exception);
                return string.Empty;
            }
        }

        public static ICollection<T> IntersectAll<T>(IEnumerable<IEnumerable<T>> lists)
        {
            HashSet<T> hashSet = null;
            foreach (IEnumerable<T> list in lists)
            {
                if (hashSet == null)
                {
                    hashSet = new HashSet<T>(list);
                }
                else
                {
                    hashSet.IntersectWith(list);
                }
            }

            if (hashSet == null)
            {
                hashSet = new HashSet<T>();
            }

            return hashSet;
        }

        public static void LogDiagObjectEvaluation(XEP_DIAGNOSISOBJECTSEX diagObject, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, int level)
        {
            //[-] bool flag = DatabaseProviderFactory.Instance.IsDiagObjectValid(diagObject.Id, vehicle, ffmDynamicResolver);
            //[+] bool flag = ClientContext.GetDatabase(vehicle)?.IsDiagObjectValid(diagObject.Id.ToString(CultureInfo.InvariantCulture), vehicle, ffmDynamicResolver) ?? false;
            bool flag = ClientContext.GetDatabase(vehicle)?.IsDiagObjectValid(diagObject.Id.ToString(CultureInfo.InvariantCulture), vehicle, ffmDynamicResolver) ?? false;
            Log.Info("IsDiagObjectValid()", "Checking LEVEL {4}: DiagObject: Name: {0} ID: {1} ControlID: {2} Result: {3}", diagObject.Name, diagObject.Id, diagObject.ControlId, flag, level);
            //[-] ICollection<XEP_DIAGNOSISOBJECTSEX> parentDiagObjects = DatabaseProviderFactory.Instance.GetParentDiagObjects(diagObject, vehicle, ffmDynamicResolver, getHidden: true);
            ICollection<XEP_DIAGNOSISOBJECTSEX> parentDiagObjects = XepConverter.Convert(ClientContext.GetDatabase(vehicle)?.GetParentDiagObjects(diagObject, vehicle, ffmDynamicResolver, getHidden: true));
            level++;
            foreach (XEP_DIAGNOSISOBJECTSEX item in parentDiagObjects)
            {
                LogDiagObjectEvaluation(item, vehicle, ffmDynamicResolver, level);
            }
        }

        public static Stream Compress(Stream dataStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                dataStream.Position = 0L;
                using (GZipStream destination = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                {
                    dataStream.CopyTo(destination);
                    dataStream.Flush();
                }

                return memoryStream;
            }
            catch (IOException ex)
            {
                Log.Error("DatabaseUtil.Compress()", "An IOException occurred while trying to set the pointer to the beginning of the data stream! Returning null! {0}", ex);
                return null;
            }
            catch (ArgumentNullException ex2)
            {
                Log.Error("DatabaseUtil.Compress()", "The given stream to compress was null!", ex2);
                return null;
            }
            catch (NotSupportedException ex3)
            {
                Log.Error("DatabaseUtil.Compress()", "The given stream which should be compressed, does not support reading! Returning null! {0}", ex3);
                return null;
            }
            catch (Exception exception)
            {
                Log.ErrorException("DatabaseUtil.Compress()", exception);
                return null;
            }
        }

        public static byte[] Compress(byte[] dataToCompress)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
                    {
                        gZipStream.Write(dataToCompress, 0, dataToCompress.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("DatabaseUtil.Compress()", exception);
                return null;
            }
        }

        public static Stream Decompress(Stream dataStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                dataStream.Position = 0L;
                using (GZipStream gZipStream = new GZipStream(dataStream, CompressionMode.Decompress, leaveOpen: true))
                {
                    gZipStream.CopyTo(memoryStream);
                    gZipStream.Flush();
                }

                return memoryStream;
            }
            catch (IOException ex)
            {
                Log.Error("DatabaseUtil.Decompress()", "An IOException occurred while trying to set the pointer to the beginning of the data stream! Returning null! {0}", ex);
                return null;
            }
            catch (ArgumentNullException ex2)
            {
                Log.Error("DatabaseUtil.Decompress()", "The given stream to compress was null!", ex2);
                return null;
            }
            catch (NotSupportedException ex3)
            {
                Log.Error("DatabaseUtil.Decompress()", "The given stream which should be compressed, does not support reading! Returning null! {0}", ex3);
                return null;
            }
            catch (Exception exception)
            {
                Log.ErrorException("DatabaseUtil.Decompress()", exception);
                return null;
            }
        }

        public static byte[] Decompress(byte[] compressedData)
        {
            try
            {
                byte[] result;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (MemoryStream stream = new MemoryStream(compressedData))
                    {
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            gZipStream.CopyTo(memoryStream);
                        }
                    }

                    result = memoryStream.ToArray();
                }

                return result;
            }
            catch (Exception exception)
            {
                Log.ErrorException("DatabaseUtil.Decompress()", exception);
                return null;
            }
        }

        public static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(plainText));
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] bytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.Unicode.GetString(bytes);
        }

        private static string ExtractInfoType(string infoTypeWithAbbreviation)
        {
            if (string.IsNullOrEmpty(infoTypeWithAbbreviation))
            {
                return infoTypeWithAbbreviation;
            }

            int num = infoTypeWithAbbreviation.LastIndexOf("(", StringComparison.Ordinal);
            if (num == -1)
            {
                return infoTypeWithAbbreviation;
            }

            int num2 = infoTypeWithAbbreviation.LastIndexOf(")", StringComparison.Ordinal);
            if (num2 == -1)
            {
                return infoTypeWithAbbreviation;
            }

            int num3 = infoTypeWithAbbreviation.LastIndexOf(" ", StringComparison.Ordinal);
            if (num3 == -1)
            {
                num3 = num;
            }

            if (num2 - num == 1)
            {
                return infoTypeWithAbbreviation;
            }

            return infoTypeWithAbbreviation.Substring(0, num3);
        }

        public static T CopyObjectData<T>(object source)
        {
            if (source == null)
            {
                return default(T);
            }

            T val = (T)Activator.CreateInstance(typeof(T));
            Type type = source.GetType();
            Type type2 = val.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    PropertyInfo property = type2.GetProperty(propertyInfo.Name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                    if (property != null)
                    {
                        property.SetValue(val, propertyInfo.GetValue(source));
                        continue;
                    }

                    Log.Error(Log.CurrentMethod(), "No suitable property found for Property Name {0} while converting from object type {1} to {2}!", propertyInfo.Name, type.FullName, type2.FullName);
                }
                catch (ArgumentException ex)
                {
                    Log.Error(Log.CurrentMethod(), "Argument Exception while copying object! Target Type: {0} Source Type: {1}. {2}. {3}", type2.FullName, type.FullName, ex.Message, ex);
                }
                catch (Exception ex2)
                {
                    Log.Error(Log.CurrentMethod(), "An Exception occured while trying to copying an object! Target Type: {0} Source Type: {1}. {2}", type2.FullName, type.FullName, ex2);
                }
            }

            return val;
        }

        public static ICollection<T> CopyObjectData<T>(IEnumerable<object> source)
        {
            ICollection<T> collection = new List<T>();
            foreach (object item in source)
            {
                T val = CopyObjectData<T>(item);
                if (val != null)
                {
                    collection.Add(val);
                }
            }

            return collection;
        }

        public static bool IsDocumentTypeHidden(string docType)
        {
            switch (docType)
            {
                case null:
                    return true;
                case "":
                    return true;
                case "ANL":
                    return true;
                case "BNT":
                    return true;
                case "FKB":
                    return true;
                case "GPI":
                    return true;
                case "NEU":
                    return true;
                case "STG":
                    return true;
                case "VUL":
                    return true;
                default:
                    return false;
            }
        }

        public static string GetAttributeLanguageExtension(string lang)
        {
            switch (lang)
            {
                case "de-DE":
                    return "DEDE";
                case "en-GB":
                    return "ENGB";
                case "en-US":
                    return "ENUS";
                case "fr-FR":
                    return "FR";
                case "es-ES":
                    return "ES";
                case "th-TH":
                    return "TH";
                case "tr-TR":
                    return "TR";
                case "el-GR":
                    return "EL";
                case "ja-JP":
                    return "JA";
                case "ru-RU":
                    return "RU";
                case "it-IT":
                    return "IT";
                case "nl-NL":
                    return "NL";
                case "cs-CZ":
                    return "CSCZ";
                case "pl-PL":
                    return "PLPL";
                case "pt-PT":
                    return "PT";
                case "sv-SE":
                    return "SV";
                case "zh-CN":
                    return "ZHCN";
                case "zh-TW":
                    return "ZHTW";
                case "ko-KR":
                    return "KO";
                default:
                {
                    string defaultAttributeLanguageExtension = GetDefaultAttributeLanguageExtension();
                    Log.Error("DatabaseProviderSQLite.GetAttributeLanguageExtension", "Language \"{0}\" not supported, returning {1}.", lang, defaultAttributeLanguageExtension);
                    return defaultAttributeLanguageExtension;
                }
            }
        }

        public static string GetDefaultAttributeLanguageExtension()
        {
            return GetAttributeLanguageExtension("en-GB");
        }

        public static string GetLanguageFromAttributeExtension(string attributeLanguageExtension)
        {
            string empty = string.Empty;
            if (attributeLanguageExtension == null)
            {
                goto IL_02fc;
            }

            int length = attributeLanguageExtension.Length;
            if (length != 2)
            {
                if (length != 4)
                {
                    goto IL_02fc;
                }

                char c = attributeLanguageExtension[3];
                if ((uint)c <= 76u)
                {
                    if (c != 'B')
                    {
                        if (c != 'E')
                        {
                            if (c != 'L' || !(attributeLanguageExtension == "PLPL"))
                            {
                                goto IL_02fc;
                            }

                            empty = "pl-PL";
                        }
                        else
                        {
                            if (!(attributeLanguageExtension == "DEDE"))
                            {
                                goto IL_02fc;
                            }

                            empty = "de-DE";
                        }
                    }
                    else
                    {
                        if (!(attributeLanguageExtension == "ENGB"))
                        {
                            goto IL_02fc;
                        }

                        empty = "en-GB";
                    }
                }
                else if ((uint)c <= 83u)
                {
                    if (c != 'N')
                    {
                        if (c != 'S' || !(attributeLanguageExtension == "ENUS"))
                        {
                            goto IL_02fc;
                        }

                        empty = "en-US";
                    }
                    else
                    {
                        if (!(attributeLanguageExtension == "ZHCN"))
                        {
                            goto IL_02fc;
                        }

                        empty = "zh-CN";
                    }
                }
                else if (c != 'W')
                {
                    if (c != 'Z' || !(attributeLanguageExtension == "CSCZ"))
                    {
                        goto IL_02fc;
                    }

                    empty = "cs-CZ";
                }
                else
                {
                    if (!(attributeLanguageExtension == "ZHTW"))
                    {
                        goto IL_02fc;
                    }

                    empty = "zh-TW";
                }
            }
            else
            {
                switch (attributeLanguageExtension[0])
                {
                    case 'F':
                        break;
                    case 'E':
                        goto IL_0175;
                    case 'T':
                        goto IL_019a;
                    case 'J':
                        goto IL_01bf;
                    case 'R':
                        goto IL_01d4;
                    case 'I':
                        goto IL_01e9;
                    case 'N':
                        goto IL_01fe;
                    case 'P':
                        goto IL_0213;
                    case 'S':
                        goto IL_0228;
                    case 'K':
                        goto IL_023d;
                    default:
                        goto IL_02fc;
                }

                if (!(attributeLanguageExtension == "FR"))
                {
                    goto IL_02fc;
                }

                empty = "fr-FR";
            }

            goto IL_031b;
            IL_0228:
                if (!(attributeLanguageExtension == "SV"))
                {
                    goto IL_02fc;
                }

            empty = "sv-SE";
            goto IL_031b;
            IL_023d:
                if (!(attributeLanguageExtension == "KO"))
                {
                    goto IL_02fc;
                }

            empty = "ko-KR";
            goto IL_031b;
            IL_0175:
                if (!(attributeLanguageExtension == "ES"))
                {
                    if (!(attributeLanguageExtension == "EL"))
                    {
                        goto IL_02fc;
                    }

                    empty = "el-GR";
                }
                else
                {
                    empty = "es-ES";
                }

            goto IL_031b;
            IL_02fc:
                empty = "en-GB";
            Log.Error("DatabaseProviderOracle.doLanguageSpecificInitialization()", "unknown database language {0}", attributeLanguageExtension);
            goto IL_031b;
            IL_019a:
                if (!(attributeLanguageExtension == "TH"))
                {
                    if (!(attributeLanguageExtension == "TR"))
                    {
                        goto IL_02fc;
                    }

                    empty = "tr-TR";
                }
                else
                {
                    empty = "th-TH";
                }

            goto IL_031b;
            IL_031b:
                return empty;
            IL_0213:
                if (!(attributeLanguageExtension == "PT"))
                {
                    goto IL_02fc;
                }

            empty = "pt-PT";
            goto IL_031b;
            IL_01fe:
                if (!(attributeLanguageExtension == "NL"))
                {
                    goto IL_02fc;
                }

            empty = "nl-NL";
            goto IL_031b;
            IL_01e9:
                if (!(attributeLanguageExtension == "IT"))
                {
                    goto IL_02fc;
                }

            empty = "it-IT";
            goto IL_031b;
            IL_01d4:
                if (!(attributeLanguageExtension == "RU"))
                {
                    goto IL_02fc;
                }

            empty = "ru-RU";
            goto IL_031b;
            IL_01bf:
                if (!(attributeLanguageExtension == "JA"))
                {
                    goto IL_02fc;
                }

            empty = "ja-JP";
            goto IL_031b;
        }
    }
}