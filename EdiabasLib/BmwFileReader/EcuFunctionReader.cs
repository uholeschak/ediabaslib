using System;
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

        public EcuFunctionReader(string rootDir)
        {
            _rootDir = rootDir;
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
