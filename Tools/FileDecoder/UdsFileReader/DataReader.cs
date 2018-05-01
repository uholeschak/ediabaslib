using System;
using System.Collections.Generic;
using System.Globalization;
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

        public class FileNameResolver
        {
            public FileNameResolver(DataReader dataReader, string partNumber, int address)
            {
                DataReader = dataReader;
                PartNumber = partNumber;
                Address = address;
            }

            public string GetFileName(string dir)
            {
                try
                {
                    if (string.IsNullOrEmpty(PartNumber) || PartNumber.Length < 9)
                    {
                        return null;
                    }

                    List<string> dirList = new List<string>();
                    string[] dirs = Directory.GetDirectories(dir);
                    if (dirs.Length > 0)
                    {
                        dirList.AddRange(dirs);
                    }
                    dirList.Add(dir);

                    foreach (string subDir in dirList)
                    {
                        string fileName;
                        string part1 = PartNumber.Substring(0, 3);
                        string part2 = PartNumber.Substring(3, 3);
                        string part3 = PartNumber.Substring(6, 3);
                        string suffix = string.Empty;
                        if (PartNumber.Length > 9)
                        {
                            suffix = PartNumber.Substring(9);
                        }
                        string baseName = part1 + "-" + part2 + "-" + part3;

                        if (!string.IsNullOrEmpty(suffix))
                        {
                            fileName = Path.Combine(subDir, baseName + "-" + suffix + FileExtension);
                            if (File.Exists(fileName))
                            {
                                return fileName;
                            }
                        }
                        fileName = Path.Combine(subDir, baseName + FileExtension);
                        if (File.Exists(fileName))
                        {
                            return fileName;
                        }
                    }

                    foreach (string subDir in dirList)
                    {
                        string part1 = PartNumber.Substring(0, 2);
                        string part2 = string.Format(CultureInfo.InvariantCulture, "{0:00}", Address);
                        string baseName = part1 + "-" + part2;

                        string fileName = Path.Combine(subDir, baseName + FileExtension);
                        if (File.Exists(fileName))
                        {
                            return fileName;
                        }
                    }

                }
                catch (Exception)
                {
                    return null;
                }
                return null;
            }

            public DataReader DataReader { get; }
            public string PartNumber { get; }
            public int Address { get; }
        }

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
