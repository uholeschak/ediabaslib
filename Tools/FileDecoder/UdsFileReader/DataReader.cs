using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace UdsFileReader
{
    public class DataReader
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        public const string FileExtension = ".ldat";

        public static string GetMd5Hash(string text)
        {
            //Prüfen ob Daten übergeben wurden.
            if ((text == null) || (text.Length == 0))
            {
                return string.Empty;
            }

            //MD5 Hash aus dem String berechnen. Dazu muss der string in ein Byte[]
            //zerlegt werden. Danach muss das Resultat wieder zurück in ein string.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(text);
            byte[] result = md5.ComputeHash(textToHash);

            return BitConverter.ToString(result).Replace("-", "");
        }

        public static List<string[]> ReadFileLines(string fileName)
        {
            List<string[]> lineList = new List<string[]>();
            ZipFile zf = null;
            try
            {
                Stream zipStream = null;
                string fileNameBase = Path.GetFileName(fileName);
                FileStream fs = File.OpenRead(fileName);
                zf = new ZipFile(fs)
                {
                    Password = GetMd5Hash(Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant())
                };
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue; // Ignore directories
                    }
                    if (string.Compare(zipEntry.Name, fileNameBase, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        zipStream = zf.GetInputStream(zipEntry);
                        break;
                    }
                }

                if (zipStream == null)
                {
                    return null;
                }
                try
                {
                    using (StreamReader sr = new StreamReader(zipStream, Encoding))
                    {
                        for (; ; )
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }

                            int commentStart = line.IndexOf(';');
                            if (commentStart >= 0)
                            {
                                line = line.Substring(0, commentStart);
                            }

                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            string[] lineArray = line.Split(',');
                            if (lineArray.Length > 0)
                            {
                                lineList.Add(lineArray);
                            }
                        }
                    }
                }
                catch (NotImplementedException)
                {
                    // closing of encrypted stream throws execption
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return lineList;
        }
    }
}
