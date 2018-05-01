using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

                if (PartNumber.Length > 9)
                {
                    string part1 = PartNumber.Substring(0, 3);
                    string part2 = PartNumber.Substring(3, 3);
                    string part3 = PartNumber.Substring(6, 3);
                    _suffix = PartNumber.Substring(9);
                    _baseName = part1 + "-" + part2 + "-" + part3;
                    _fullName = _baseName + "-" + _suffix;
                }
            }

            public string GetFileName(string dir)
            {
                try
                {
                    if (string.IsNullOrEmpty(_fullName))
                    {
                        return null;
                    }

                    List<string> dirList = GetDirList(dir);
                    string fileName = ResolveFileName(dirList);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return null;
                    }

                    List<string[]> redirectList = GetRedirects(fileName);
                    if (redirectList == null)
                    {
                        return null;
                    }

                    foreach (string[] redirects in redirectList)
                    {
                        string targetFile = Path.ChangeExtension(redirects[0], FileExtension);
                        for (int i = 1; i < redirects.Length; i++)
                        {
                            string redirect = redirects[i].Trim();
                            if (redirect.Length > 9)
                            {
                                string regString = WildCardToRegular(redirect);
                                if (Regex.IsMatch(_fullName, regString, RegexOptions.IgnoreCase))
                                {
                                    if (!string.IsNullOrEmpty(targetFile))
                                    {
                                        foreach (string subDir in dirList)
                                        {
                                            fileName = Path.Combine(subDir, targetFile);
                                            if (File.Exists(fileName))
                                            {
                                                return fileName;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }

                return null;
            }

            private string ResolveFileName(List<string> dirList)
            {
                try
                {
                    foreach (string subDir in dirList)
                    {
                        string fileName = Path.Combine(subDir, _fullName + FileExtension);
                        if (File.Exists(fileName))
                        {
                            return fileName;
                        }
                        fileName = Path.Combine(subDir, _baseName + FileExtension);
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

            private List<string> GetDirList(string dir)
            {
                try
                {
                    List<string> dirList = new List<string>();
                    string[] dirs = Directory.GetDirectories(dir);
                    if (dirs.Length > 0)
                    {
                        dirList.AddRange(dirs);
                    }
                    dirList.Add(dir);

                    return dirList;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public List<string[]> GetRedirects(string fileName)
            {
                try
                {
                    List<string[]> redirectList = new List<string[]>();
                    List<string[]> textList = ReadFileLines(fileName);
                    foreach (string[] lineArray in textList)
                    {
                        if (lineArray.Length < 3)
                        {
                            continue;
                        }
                        if (string.Compare(lineArray[0], "REDIRECT", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            continue;
                        }

                        redirectList.Add(lineArray.Skip(1).ToArray());
                    }

                    return redirectList;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public DataReader DataReader { get; }
            public string PartNumber { get; }
            public int Address { get; }

            private string _fullName;
            private string _baseName;
            private string _suffix;
        }

        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".") + "$";
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
