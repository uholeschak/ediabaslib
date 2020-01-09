using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;
// ReSharper disable ConvertToUsingDeclaration

namespace BmwFileReader
{
    public class EcuFunctionReader
    {
        public const string EcuFuncFileName = "EcuFunctions.zip";
        private readonly string _rootDir;
        private readonly Dictionary<string, EcuFunctionStructs.EcuVariant> _ecuVariantDict;
        public EcuFunctionStructs.EcuFaultData _ecuFaultData;
        private string _ecuFaultDataLanguage;

        public EcuFunctionReader(string rootDir)
        {
            _rootDir = rootDir;
            _ecuVariantDict = new Dictionary<string, EcuFunctionStructs.EcuVariant>();
        }

        public List<EcuFunctionStructs.EcuFixedFuncStruct> GetFixedFuncStructList(EcuFunctionStructs.EcuVariant ecuVariant)
        {
            List<EcuFunctionStructs.EcuFixedFuncStruct> fixedFuncStructList = new List<EcuFunctionStructs.EcuFixedFuncStruct>();

            if (ecuVariant.RefEcuVariantList != null)
            {
                foreach (EcuFunctionStructs.RefEcuVariant refEcuVariant in ecuVariant.RefEcuVariantList)
                {
                    if (refEcuVariant.FixedFuncStructList != null)
                    {
                        fixedFuncStructList.AddRange(refEcuVariant.FixedFuncStructList);
                    }
                }
            }

            if (ecuVariant.EcuFuncStructList != null)
            {
                foreach (EcuFunctionStructs.EcuFuncStruct ecuFuncStruct in ecuVariant.EcuFuncStructList)
                {
                    if (ecuFuncStruct.FixedFuncStructList != null)
                    {
                        fixedFuncStructList.AddRange(ecuFuncStruct.FixedFuncStructList);
                    }
                }
            }

            return fixedFuncStructList;
        }

        public EcuFunctionStructs.EcuVariant GetEcuVariantCached(string ecuName, string language)
        {
            try
            {
                if (GetEcuFaultDataCached(language) == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(ecuName))
                {
                    return null;
                }

                string key = ecuName.ToLowerInvariant();
                if (_ecuVariantDict.TryGetValue(key, out EcuFunctionStructs.EcuVariant ecuVariant))
                {
                    return ecuVariant;
                }

                ecuVariant = GetEcuVariant(ecuName);
                _ecuVariantDict[key] = ecuVariant;

                return ecuVariant;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public EcuFunctionStructs.EcuFaultData GetEcuFaultDataCached(string language)
        {
            if (_ecuFaultData == null || _ecuFaultDataLanguage == null ||
                string.Compare(_ecuFaultDataLanguage, language, StringComparison.OrdinalIgnoreCase) != 0)
            {
                _ecuFaultDataLanguage = null;
                _ecuFaultData = GetEcuFaultData(language);
                if (_ecuFaultData != null)
                {
                    _ecuFaultDataLanguage = language;
                }
            }

            return _ecuFaultData;
        }

        public EcuFunctionStructs.EcuVariant GetEcuVariant(string ecuName)
        {
            EcuFunctionStructs.EcuVariant ecuVariant = GetEcuDataObject(ecuName, typeof(EcuFunctionStructs.EcuVariant)) as EcuFunctionStructs.EcuVariant;
            return ecuVariant;
        }

        public EcuFunctionStructs.EcuFaultData GetEcuFaultData(string language)
        {
            string fileName = "faultdata_" + language.ToLowerInvariant();
            EcuFunctionStructs.EcuFaultData ecuFaultData = GetEcuDataObject(fileName, typeof(EcuFunctionStructs.EcuFaultData)) as EcuFunctionStructs.EcuFaultData;
            if (ecuFaultData == null)
            {
                fileName = "faultdata_en";
                ecuFaultData = GetEcuDataObject(fileName, typeof(EcuFunctionStructs.EcuFaultData)) as EcuFunctionStructs.EcuFaultData;
            }
            return ecuFaultData;
        }

        public object GetEcuDataObject(string name, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                object ecuObject = null;
                ZipFile zf = null;
                try
                {
                    string fileName = name.ToLowerInvariant() + ".xml";
                    using (FileStream fs = File.OpenRead(Path.Combine(_rootDir, EcuFuncFileName)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, fileName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (TextReader reader = new StreamReader(zipStream))
                                {
                                    XmlSerializer serializer = new XmlSerializer(type);
                                    ecuObject = serializer.Deserialize(reader);
                                }
                                break;
                            }
                        }
                    }

                    return ecuObject;
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
