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

        public EcuFunctionReader(string rootDir)
        {
            _rootDir = rootDir;
            _ecuVariantDict = new Dictionary<string, EcuFunctionStructs.EcuVariant>();
        }

        public EcuFunctionStructs.EcuVariant GetEcuVariantCached(string ecuName)
        {
            try
            {
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

        public EcuFunctionStructs.EcuVariant GetEcuVariant(string ecuName)
        {
            try
            {
                EcuFunctionStructs.EcuVariant ecuVariant = null;
                ZipFile zf = null;
                try
                {
                    string ecuFileName = ecuName.ToLowerInvariant() + ".xml";
                    using (FileStream fs = File.OpenRead(Path.Combine(_rootDir, EcuFuncFileName)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, ecuFileName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (TextReader reader = new StreamReader(zipStream))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(EcuFunctionStructs.EcuVariant));
                                    ecuVariant = serializer.Deserialize(reader) as EcuFunctionStructs.EcuVariant;
                                }
                                break;
                            }
                        }
                    }

                    return ecuVariant;
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
