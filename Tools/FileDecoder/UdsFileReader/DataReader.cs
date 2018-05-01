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

        public enum DataType
        {
            Measurement,
            Basic,
            Adaption,
            Settings,
            Coding,
            LongCoding,
        }

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
                    string suffix = PartNumber.Substring(9);
                    _baseName = part1 + "-" + part2 + "-" + part3;
                    _fullName = _baseName + "-" + suffix;
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
                    if (redirectList != null)
                    {
                        foreach (string[] redirects in redirectList)
                        {
                            string targetFile = Path.ChangeExtension(redirects[0], FileExtension);
                            if (string.IsNullOrEmpty(targetFile))
                            {
                                continue;
                            }

                            for (int i = 1; i < redirects.Length; i++)
                            {
                                string redirect = redirects[i].Trim();
                                bool matched = false;
                                if (redirect.Length > 12)
                                {   // min 1 char suffix
                                    string regString = WildCardToRegular(redirect);
                                    if (Regex.IsMatch(_fullName, regString, RegexOptions.IgnoreCase))
                                    {
                                        matched = true;
                                    }
                                }
                                else
                                {
                                    string fullRedirect = _baseName + redirect;
                                    if (string.Compare(_fullName, fullRedirect, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        matched = true;
                                    }
                                }

                                if (matched)
                                {
                                    foreach (string subDir in dirList)
                                    {
                                        string targetFileName = Path.Combine(subDir, targetFile);
                                        if (File.Exists(targetFileName))
                                        {
                                            return targetFileName;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return fileName;
                }
                catch (Exception)
                {
                    return null;
                }
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

            private readonly string _fullName;
            private readonly string _baseName;
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

        public class DataInfo
        {
            public DataInfo(DataType dataType, int? value1, int? value2, string[] textArray)
            {
                DataType = dataType;
                Value1 = value1;
                Value2 = value2;
                TextArray = textArray;
            }

            public DataType DataType { get; }
            public int? Value1 { get; }
            public int? Value2 { get; }
            public string[] TextArray { get; }
        }

        public List<DataInfo> ExtractDataType(string fileName, DataType dataType)
        {
            try
            {
                List<DataInfo> dataInfoList = new List<DataInfo>();
                List<string[]> lineList = ReadFileLines(fileName);
                if (lineList == null)
                {
                    return null;
                }

                char? prefix = null;
                int textOffset = 2;
                switch (dataType)
                {
                    case DataType.Adaption:
                        prefix = 'A';
                        break;

                    case DataType.Basic:
                        prefix = 'B';
                        break;

                    case DataType.Settings:
                        prefix = 'S';
                        textOffset = 1;
                        break;

                    case DataType.Coding:
                        prefix = 'C';
                        textOffset = 1;
                        break;

                    case DataType.LongCoding:
                        prefix = 'L';
                        textOffset = 0;
                        break;
                }

                foreach (string[] lineArray in lineList)
                {
                    if (lineArray.Length < 2)
                    {
                        continue;
                    }

                    string entry1 = lineArray[0].Trim();
                    if (entry1.Length < 1)
                    {
                        continue;
                    }

                    if (prefix != null)
                    {
                        if (entry1[0] != prefix)
                        {
                            continue;
                        }

                        entry1 = entry1.Substring(1);
                    }
                    else
                    {
                        if (!Char.IsNumber(entry1[0]))
                        {
                            continue;
                        }
                    }

                    int? value1 = null;
                    int? value2 = null;
                    if (textOffset >= 1)
                    {
                        if (Int32.TryParse(entry1, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out Int32 valueOut1))
                        {
                            value1 = valueOut1;
                        }
                    }
                    if (textOffset >= 2)
                    {
                        if (Int32.TryParse(lineArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 valueOut2))
                        {
                            value2 = valueOut2;
                        }
                    }
                    string[] textArray = lineArray.Skip(textOffset).ToArray();

                    dataInfoList.Add(new DataInfo(dataType, value1, value2, textArray));
                }

                return dataInfoList;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
