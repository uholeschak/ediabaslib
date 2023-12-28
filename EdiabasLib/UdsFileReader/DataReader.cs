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
        public static readonly Encoding EncodingLatin1 = Encoding.GetEncoding(1252);
        public static readonly Encoding EncodingCyrillic = Encoding.GetEncoding(1251);
        public const string FileExtension = ".ldat";
        public const string CodeFileExtension = ".cdat";
        public const string DataDir = "labels";

        public enum ErrorType
        {
            Iso9141,
            Kwp2000,
            Sae,
            Uds,
        }

        public Dictionary<UInt32, string> CodeMap { get; private set; }

        public enum DataType
        {
            Measurement,
            Basic,
            Adaption,
            Login,
            Settings,
            Coding,
            LongCoding,
        }

        public class FileNameResolver
        {
            public FileNameResolver(DataReader dataReader, string partNumber, string hwPartNumber, string partNumberSubSys, int address, int indexSubSys) :
                this(dataReader, partNumber, hwPartNumber, address)
            {
                PartNumberSubSys = partNumberSubSys;
                IndexSubSys = indexSubSys;
                _fullNameSubSys = ConvertPartNumber(PartNumberSubSys, out _baseNameSubSys);
            }

            public FileNameResolver(DataReader dataReader, string partNumber, string hwPartNumber, int address)
            {
                DataReader = dataReader;
                PartNumber = partNumber;
                HwPartNumber = hwPartNumber;
                Address = address;

                _fullName = ConvertPartNumber(PartNumber, out _baseName);
                _fullNameHw = ConvertPartNumber(hwPartNumber, out _baseNameHw);
            }

            private static string ConvertPartNumber(string partNumber, out string baseName)
            {
                string fullName = string.Empty;
                baseName = string.Empty;
                if (!string.IsNullOrEmpty(partNumber) && partNumber.Length >= 9)
                {
                    string part1 = partNumber.Substring(0, 3);
                    string part2 = partNumber.Substring(3, 3);
                    string part3 = partNumber.Substring(6, 3);
                    baseName = part1 + "-" + part2 + "-" + part3;
                    fullName = baseName;
                    if (partNumber.Length > 9)
                    {
                        string suffix = partNumber.Substring(9);
                        fullName = baseName + "-" + suffix;
                    }
                }

                return fullName;
            }

            public string GetFileName(string rootDir)
            {
                try
                {
                    if (string.IsNullOrEmpty(_fullName))
                    {
                        return null;
                    }

                    string dataDir = Path.Combine(rootDir, DataDir);
                    List<string> dirList = GetDirList(dataDir);
                    string fileName = ResolveFileName(dirList, out bool redirectFile);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return null;
                    }

                    string baseFileName = Path.GetFileNameWithoutExtension(fileName);
                    if (string.IsNullOrEmpty(baseFileName))
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
                            bool matched = false;

                            if (redirectFile)
                            {
                                string fullName = _fullName;
                                if (redirects.Length >= 3)
                                {
                                    string redirectSource = redirects[2].Trim();
                                    if (string.Compare(redirectSource, "HW", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        fullName = _fullNameHw;
                                    }
                                    else if (string.Compare(redirectSource, "SL", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        fullName = _fullNameSubSys;
                                    }
                                }

                                if (string.IsNullOrEmpty(fullName))
                                {
                                    continue;
                                }

                                string redirect = redirects[1].Trim();
                                string regString = WildCardToRegular(redirect);
                                if (Regex.IsMatch(fullName, regString, RegexOptions.IgnoreCase))
                                {
                                    matched = true;
                                }
                            }
                            else
                            {
                                for (int i = 1; i < redirects.Length; i++)
                                {
                                    string redirect = redirects[i].Trim();
                                    string fullRedirect = _baseName + redirect;
                                    if (string.Compare(_fullName, fullRedirect, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        matched = true;
                                    }
                                }
                            }
                            if (matched)
                            {
                                foreach (string subDir in dirList)
                                {
                                    string targetFileName = Path.Combine(subDir, targetFile.ToLowerInvariant());
                                    if (File.Exists(targetFileName))
                                    {
                                        return targetFileName;
                                    }
                                }
                            }
                        }
                    }

                    if (redirectFile)
                    {
                        return null;    // no redirect found
                    }

                    return fileName;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            private string ResolveFileName(List<string> dirList, out bool redirectFile)
            {
                redirectFile = false;
                try
                {
                    string partNumber;
                    if (!IndexSubSys.HasValue)
                    {
                        partNumber = PartNumber;
                        foreach (string subDir in dirList)
                        {
                            string fileName = Path.Combine(subDir, _fullName.ToLowerInvariant() + FileExtension);
                            if (File.Exists(fileName))
                            {
                                return fileName;
                            }

                            fileName = Path.Combine(subDir, _baseName.ToLowerInvariant() + FileExtension);
                            if (File.Exists(fileName))
                            {
                                return fileName;
                            }
                        }
                    }
                    else
                    {
                        partNumber = PartNumberSubSys;
                    }

                    if (!string.IsNullOrEmpty(partNumber) && partNumber.Length >= 2)
                    {
                        foreach (string subDir in dirList)
                        {
                            string part1 = partNumber.Substring(0, 2);
                            string part2 = string.Format(CultureInfo.InvariantCulture, "{0:X02}", Address);
                            string baseNameTest = part1 + "-" + part2;
                            if (IndexSubSys.HasValue)
                            {
                                string part3 = string.Format(CultureInfo.InvariantCulture, "{0:X02}", IndexSubSys.Value + 1);
                                baseNameTest += "-" + part3;
                            }

                            string fileName = Path.Combine(subDir, baseNameTest.ToLowerInvariant() + FileExtension);
                            if (File.Exists(fileName))
                            {
                                redirectFile = true;
                                return fileName;
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

            private List<string> GetDirList(string dir)
            {
                try
                {
                    List<string> dirList = new List<string>();
                    string[] dirs = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
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
            public string HwPartNumber { get; }
            public string PartNumberSubSys { get; }
            public int Address { get; }
            public int? IndexSubSys { get; }

            private readonly string _fullName;
            private readonly string _baseName;
            private readonly string _fullNameHw;
            private readonly string _baseNameHw;
            private readonly string _fullNameSubSys;
            private readonly string _baseNameSubSys;
        }

        public List<string> ErrorCodeToString(uint errorCode, uint errorDetail, ErrorType errorType, UdsReader udsReader = null)
        {
            List<string> resultList = new List<string>();
            string errorText = string.Empty;
            uint errorCodeMap = errorCode;
            int errorCodeKey = (int) (errorCode + 100000);
            bool useFullCode = errorCode >= 0x4000 && errorCode <= 0xBFFF;
            if (errorType != ErrorType.Sae)
            {
                useFullCode = false;
                if (errorCode < 0x4000 || errorCode > 0x7FFF)
                {
                    errorCodeKey = (int) errorCode;
                }
                else
                {
                    if (string.IsNullOrEmpty(PcodeToString(errorCode, out uint convertedValue)))
                    {
                        errorCodeKey = -1;
                    }
                    else
                    {
                        errorCodeMap = convertedValue;
                        errorCodeKey = (int) (convertedValue + 100000);
                    }
                }
            }
            bool fullCodeFound = false;
            if (useFullCode)
            {
                uint textKey = (errorCode << 8) | errorDetail;
                if (CodeMap.TryGetValue(textKey, out string longText))
                {
                    errorText = longText;
                    fullCodeFound = true;
                }
            }

            bool splitErrorText = false;
            if (!fullCodeFound)
            {
                if (errorCodeKey >= 0)
                {
                    if (CodeMap.TryGetValue((uint)errorCodeKey, out string shortText))
                    {
                        errorText = shortText;
                        if (errorCodeMap <= 0x3FFF)
                        {
                            splitErrorText = true;
                        }
                        if (errorCodeMap >= 0xC000 && errorCodeMap <= 0xFFFF)
                        {
                            splitErrorText = true;
                        }
                    }
                }
            }

            string errorDetailText1 = string.Empty;
            if (!string.IsNullOrEmpty(errorText))
            {
                int colonIndex = errorText.LastIndexOf(':');
                if (colonIndex >= 0)
                {
                    if (splitErrorText || fullCodeFound)
                    {
                        errorDetailText1 = errorText.Substring(colonIndex + 1).Trim();
                        errorText = errorText.Substring(0, colonIndex);
                    }
                }
            }

            string errorDetailText2 = string.Empty;
            string errorDetailText3 = string.Empty;
            uint detailCode = errorDetail;
            if (!useFullCode)
            {
                switch (errorType)
                {
                    case ErrorType.Iso9141:
                        if ((errorDetail & 0x80) != 0x00)
                        {
                            errorDetailText2 = (UdsReader.GetTextMapText(udsReader, 002693) ?? string.Empty);   // Sporadisch
                        }
                        detailCode &= 0x7F;
                        break;

                    default:
                        if ((errorDetail & 0x60) == 0x20)
                        {
                            errorDetailText2 = (UdsReader.GetTextMapText(udsReader, 002693) ?? string.Empty);   // Sporadisch
                        }
                        if ((errorDetail & 0x80) != 0x00)
                        {
                            errorDetailText3 = (UdsReader.GetTextMapText(udsReader, 066900) ?? string.Empty)
                                               + " " + (UdsReader.GetTextMapText(udsReader, 000085) ?? string.Empty);   // Warnleuchte EIN
                        }
                        detailCode &= 0x0F;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(errorText) && string.IsNullOrEmpty(errorDetailText1))
            {
                uint detailKey = (uint)(detailCode + (useFullCode ? 96000 : 98000));
                if (CodeMap.TryGetValue(detailKey, out string detail))
                {
                    if (!string.IsNullOrEmpty(detail))
                    {
                        errorDetailText1 = detail;
                    }
                }
            }

            if (string.IsNullOrEmpty(errorText))
            {
                errorText = (UdsReader.GetTextMapText(udsReader, 062047) ?? string.Empty); // Unbekannter Fehlercode
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:000000}", errorCode));
            if (!string.IsNullOrEmpty(errorText))
            {
                sb.Append(" - ");
                sb.Append(errorText);
            }
            resultList.Add(sb.ToString());

            switch (errorType)
            {
                case ErrorType.Iso9141:
                {
                    string detailCodeText = string.Format(CultureInfo.InvariantCulture, "{0:00}-{1}0", detailCode, (errorDetail & 0x80) != 0 ? 1 : 0);
                    if (errorCode > 0x3FFF && errorCode < 65000)
                    {
                        string pcodeText = PcodeToString(errorCode);
                        if (string.IsNullOrEmpty(pcodeText))
                        {
                            pcodeText = (UdsReader.GetTextMapText(udsReader, 099014) ?? string.Empty);  // Unbekannt
                        }
                        resultList.Add(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", pcodeText, detailCodeText));
                    }
                    else
                    {
                        resultList.Add(detailCodeText);
                    }
                    break;
                }

                case ErrorType.Kwp2000:
                    if (errorCode > 0x3FFF && errorCode < 65000)
                    {
                        string pcodeText = PcodeToString(errorCode);
                        if (string.IsNullOrEmpty(pcodeText))
                        {
                            pcodeText = (UdsReader.GetTextMapText(udsReader, 099014) ?? string.Empty);  // Unbekannt
                        }
                        resultList.Add(string.Format(CultureInfo.InvariantCulture, "{0} - {1:000}", pcodeText, detailCode));
                    }
                    else
                    {
                        resultList.Add(string.Format(CultureInfo.InvariantCulture, "{0:000}", detailCode));
                    }
                    break;

                default:
                    resultList.Add(string.Format(CultureInfo.InvariantCulture, "{0} - {1:000}", SaePcodeToString(errorCode), detailCode));
                    break;
            }

            if (!string.IsNullOrEmpty(errorDetailText1))
            {
                resultList.Add(errorDetailText1);
            }
            if (!string.IsNullOrEmpty(errorDetailText2))
            {
                resultList.Add(errorDetailText2);
            }
            if (!string.IsNullOrEmpty(errorDetailText3))
            {
                resultList.Add(errorDetailText3);
            }

            return resultList;
        }

        public List<string> SaeErrorDetailHeadToString(byte[] data, UdsReader udsReader = null)
        {
            List<string> resultList = new List<string>();
            if (data.Length < 15)
            {
                return null;
            }

            if (data[0] != 0x6C)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(UdsReader.GetTextMapText(udsReader, 003356) ?? string.Empty);  // Umgebungsbedingungen
            sb.Append(":");
            resultList.Add(sb.ToString());
            UInt32 value = data[2];
            if (value != 0xFF)
            {
                sb.Clear();
                sb.Append(UdsReader.GetTextMapText(udsReader, 018478) ?? string.Empty); // Fehlerstatus
                sb.Append(": ");
                sb.Append(Convert.ToString(value, 2).PadLeft(8, '0'));
                resultList.Add(sb.ToString());
            }

            value = data[3];
            if (value != 0xFF)
            {
                sb.Clear();
                sb.Append(UdsReader.GetTextMapText(udsReader, 016693) ?? string.Empty); // Fehlerpriorität
                sb.Append(": ");
                sb.Append($"{value & 0x0F:0}");
                resultList.Add(sb.ToString());
            }

            value = data[4];
            if (value != 0xFF)
            {
                sb.Clear();
                sb.Append(UdsReader.GetTextMapText(udsReader, 061517) ?? string.Empty); // Fehlerhäufigkeit
                sb.Append(": ");
                sb.Append($"{value:0}");
                resultList.Add(sb.ToString());
            }

            value = data[5];
            if (value != 0xFF)
            {
                sb.Clear();
                sb.Append(UdsReader.GetTextMapText(udsReader, 099026) ?? string.Empty); // Verlernzähler
                sb.Append(": ");
                sb.Append($"{value:0}");
                resultList.Add(sb.ToString());
            }

            value = (UInt32)((data[6] << 16) | (data[7] << 8) | data[8]);
            if (value != 0xFFFFF && value <= 0x3FFFFF)
            {
                sb.Clear();
                sb.Append(UdsReader.GetTextMapText(udsReader, 018858) ?? string.Empty); // Kilometerstand
                sb.Append(": ");
                sb.Append($"{value:0}");
                sb.Append(" ");
                sb.Append(UdsReader.GetUnitMapText(udsReader, 000108) ?? string.Empty); // km
                resultList.Add(sb.ToString());
            }

            value = data[9];
            if (value != 0xFF)
            {
                sb.Clear();
                sb.Append(UdsReader.GetTextMapText(udsReader, 039410) ?? string.Empty); // Zeitangabe
                sb.Append(": ");
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}", value & 0x0F));
                resultList.Add(sb.ToString());

                if (value < 2)
                {
                    if (value == 0)
                    {
                        // date time
                        UInt64 timeValue = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            timeValue <<= 8;
                            timeValue += data[10 + i];
                        }
                        if (timeValue != 0 && timeValue != 0x1FFFFFFFF)
                        {
                            UInt64 tempValue = timeValue;
                            UInt64 sec = tempValue & 0x3F;
                            tempValue >>= 6;
                            UInt64 min = tempValue & 0x3F;
                            tempValue >>= 6;
                            UInt64 hour = tempValue & 0x1F;
                            tempValue >>= 5;
                            UInt64 day = tempValue & 0x1F;
                            tempValue >>= 5;
                            UInt64 month = tempValue & 0x0F;
                            tempValue >>= 4;
                            UInt64 year = tempValue & 0x7F;

                            sb.Clear();
                            sb.Append(UdsReader.GetTextMapText(udsReader, 098044) ?? string.Empty); // Datum
                            sb.Append(": ");
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:00}.{1:00}.{2:00}", year + 2000, month, day));
                            resultList.Add(sb.ToString());

                            sb.Clear();
                            sb.Append(UdsReader.GetTextMapText(udsReader, 099068) ?? string.Empty); // Zeit
                            sb.Append(": ");
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hour, min, sec));
                            resultList.Add(sb.ToString());
                        }
                    }
                    else
                    {
                        // life span
                        UInt64 timeValue = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            timeValue <<= 8;
                            timeValue += data[11 + i];
                        }
                        if (timeValue != 0xFFFFFFFF)
                        {
                            sb.Clear();
                            // Zähler Fhzg.-Lebensdauer
                            sb.Append(UdsReader.GetTextMapText(udsReader, 098050) ?? string.Empty);
                            sb.Append(" ");
                            sb.Append(UdsReader.GetTextMapText(udsReader, 047622) ?? string.Empty);
                            sb.Append(": ");
                            sb.Append($"{timeValue:0}");
                            resultList.Add(sb.ToString());
                        }
                    }
                }
            }
            return resultList;
        }

        public static string PcodeToString(uint pcodeNum)
        {
            return PcodeToString(pcodeNum, out _);
        }

        public static string PcodeToString(uint pcodeNum, out uint convertedValue)
        {
            convertedValue = 0;
            int codeValue = (int) pcodeNum;
            if (codeValue < 0x4000)
            {
                return string.Empty;
            }
            if (codeValue > 65000)
            {
                return string.Empty;
            }

            int displayCode;
            string codeString;
            if (codeValue < 0x43E8)
            {
                displayCode = codeValue - 0x4000;
            }
            else if (codeValue < 0x3E7 + 0x4400)
            {
                displayCode = codeValue - 0x4018;
            }
            else if (codeValue < 0x3E7 + 0x4800)
            {
                displayCode = codeValue - 0x4030;
            }
            else if (codeValue < 0x3E7 + 0x4C00)
            {
                displayCode = codeValue - 0x4048;
            }
            else
            {
                displayCode = -1;
            }
            if (displayCode >= 0)
            {
                codeString = string.Format(CultureInfo.InvariantCulture, "{0:0000}", displayCode);
                if (!uint.TryParse(codeString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out convertedValue))
                {
                    convertedValue = 0;
                }
                return "P" + codeString;
            }

            if (codeValue < 0x7000)
            {
                return string.Empty;
            }
            if (codeValue > 0x3E7 + 0x7000)
            {
                if (codeValue > 0x3E7 + 0x7400)
                {
                    return string.Empty;
                }
                displayCode = codeValue - 0x7018;
            }
            else
            {
                displayCode = codeValue - 0x7000;
            }
            codeString = string.Format(CultureInfo.InvariantCulture, "{0:0000}", displayCode);
            if (!uint.TryParse(codeString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out convertedValue))
            {
                convertedValue = 0;
            }
            convertedValue |= 0xC000;
            return "U" + codeString;
        }

        public static string SaePcodeToString(uint pcodeNum)
        {
            char keyLetter = 'P';
            switch ((pcodeNum >> 14) & 0x03)
            {
                case 0x1:
                    keyLetter = 'C';
                    break;

                case 0x2:
                    keyLetter = 'B';
                    break;

                case 0x3:
                    keyLetter = 'U';
                    break;
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}{1:X04}", keyLetter, pcodeNum & 0x3FFF);
        }

        public static int GetModelYear(string vin)
        {
            try
            {
                if (string.IsNullOrEmpty(vin) || vin.Length < 10)
                {
                    return -1;
                }

                char yearCode = vin.ToUpperInvariant()[9];
                if (Int32.TryParse(yearCode.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Int32 value))
                {
                    if (value >= 1 && value <= 0xF)
                    {
                        return value + 2000;
                    }
                }
                if (yearCode >= 'G' && yearCode <= 'Z')
                {
                    if (yearCode > 'P')
                    {
                        if (yearCode >= 'R')
                        {
                            if (yearCode <= 'T')
                            {
                                return yearCode + 1942;
                            }
                            if (yearCode >= 'V')
                            {
                                return yearCode + 1941;
                            }
                        }
                    }
                    else
                    {
                        if (yearCode == 'P')
                        {
                            return yearCode + 1943;
                        }
                        if (yearCode >= 'G')
                        {
                            if (yearCode <= 'H')
                            {
                                return yearCode + 1945;
                            }
                            if (yearCode >= 'J' && yearCode <= 'N')
                            {
                                return yearCode + 1944;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return -1;
        }

        public static Encoding GetEncodingForFileName(string fileName)
        {
            string baseFileName = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
            if (baseFileName.EndsWith("-rus", true, CultureInfo.InvariantCulture))
            {
                return EncodingCyrillic;
            }

            DirectoryInfo dirInfoParent = Directory.GetParent(fileName);
            if (dirInfoParent != null)
            {
                if (string.Compare(dirInfoParent.Name, "rus", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return EncodingCyrillic;
                }
            }

            return EncodingLatin1;
        }

        private static string WildCardToRegular(string value)
        {
            string regEx = "^" + Regex.Escape(value).Replace("\\?", ".");
            string regExTrim = regEx.TrimEnd('.');     // ? at the end is optional
            if (regEx != regExTrim)
            {
                regExTrim = regExTrim.TrimEnd('-');    // remove also optional - at the end
                regEx = regExTrim + ".*";
            }
            regEx += "$";
            return regEx;
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
            using (MD5 md5 = MD5.Create())
            {
                byte[] textToHash = Encoding.Default.GetBytes(text);
                byte[] result = md5.ComputeHash(textToHash);

                return BitConverter.ToString(result).Replace("-", "");
            }
        }

        public static List<string[]> ReadFileLines(string fileName, bool codeFile = false)
        {
            List<string[]> lineList = new List<string[]>();
            ZipFile zf = null;
            try
            {
                Stream zipStream = null;
                Encoding encoding = GetEncodingForFileName(fileName);
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
                    using (StreamReader sr = new StreamReader(zipStream, encoding))
                    {
                        for (;;)
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }

                            if (codeFile)
                            {
                                string[] lineArray = line.Split(new [] {','}, 2);
                                if (lineArray.Length > 0)
                                {
                                    lineList.Add(lineArray);
                                }
                            }
                            else
                            {
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
                OrderIndex = 0;
                if (Value1.HasValue)
                {
                    OrderIndex |= (UInt64) Value1.Value << 32;
                }
                if (Value2.HasValue)
                {
                    OrderIndex |= (UInt32)Value2.Value;
                }
            }

            public DataType DataType { get; }
            public int? Value1 { get; }
            public int? Value2 { get; }
            public string[] TextArray { get; }
            public UInt64 OrderIndex { get; protected set; }
        }

        public class DataInfoLongCoding : DataInfo
        {
            public DataInfoLongCoding(DataType dataType, int? value1, int? value2, string[] textArray) :
                base(dataType, value1, value2, textArray)
            {
                if (textArray.Length >= 1)
                {
                    if (Int32.TryParse(textArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 value))
                    {
                        Byte = value;
                    }
                }

                int textIndex = 2;
                if (textArray.Length >= 2)
                {
                    string bitRange = textArray[1];
                    if (bitRange.EndsWith("="))
                    {
                        bitRange = bitRange.Substring(0, bitRange.Length - 1);
                        if (Int32.TryParse(bitRange, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 value))
                        {
                            LineNumber = value;
                        }
                    }
                    else
                    {
                        if (bitRange.Contains('~'))
                        {
                            string[] bitArray = textArray[1].Split('~');
                            if (bitArray.Length == 2)
                            {
                                if (Int32.TryParse(bitArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 valueMin))
                                {
                                    if (Int32.TryParse(bitArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 valueMax))
                                    {
                                        BitMin = valueMin;
                                        BitMax = valueMax;
                                    }
                                }
                            }
                            if (textArray.Length >= 3)
                            {
                                if (Int32.TryParse(textArray[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Int32 value))
                                {
                                    BitValue = value;
                                }
                            }
                            textIndex = 3;
                        }
                        else
                        {
                            if (Int32.TryParse(bitRange, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 value))
                            {
                                Bit = value;
                            }
                        }
                    }
                    if (textArray.Length >= textIndex + 1)
                    {
                        Text = textArray[textIndex];
                    }

                    OrderIndex = 0;
                    if (Byte.HasValue)
                    {
                        OrderIndex |= (UInt64)Byte.Value << 32;
                    }
                    if (LineNumber.HasValue)
                    {
                        OrderIndex |= (UInt32)LineNumber.Value;
                    }
                    else if (Bit.HasValue)
                    {
                        OrderIndex |= (UInt32)Bit.Value << 16;
                    }
                    else if (BitMin.HasValue)
                    {
                        OrderIndex |= (UInt32)BitMin.Value << 16;
                        if (BitValue.HasValue)
                        {
                            OrderIndex |= (UInt32)BitValue.Value;
                        }
                    }
                }
            }

            public int? Byte { get; }
            public int? Bit { get; }
            public int? BitMin { get; }
            public int? BitMax { get; }
            public int? BitValue { get; }
            public int? LineNumber { get; }
            public string Text { get; }
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

                string prefix = null;
                int numberCount = 2;
                int textOffset = 2;
                switch (dataType)
                {
                    case DataType.Adaption:
                        prefix = "A";
                        break;

                    case DataType.Basic:
                        prefix = "B";
                        break;

                    case DataType.Login:
                        prefix = "L";
                        textOffset = 1;
                        numberCount = 1;
                        break;

                    case DataType.Settings:
                        prefix = "S";
                        textOffset = 1;
                        numberCount = 1;
                        break;

                    case DataType.Coding:
                        prefix = "C";
                        textOffset = 1;
                        numberCount = 1;
                        break;

                    case DataType.LongCoding:
                        prefix = "LC";
                        textOffset = 1;
                        numberCount = 0;
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

                    bool longCoding = string.Compare(entry1, "LC", StringComparison.OrdinalIgnoreCase) == 0;
                    if (prefix != null)
                    {
                        if (longCoding)
                        {
                            if (string.Compare(entry1, prefix, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!entry1.StartsWith(prefix))
                            {
                                continue;
                            }
                            entry1 = entry1.Substring(prefix.Length);
                        }
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
                    if (numberCount >= 1)
                    {
                        if (Int32.TryParse(entry1, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out Int32 valueOut1))
                        {
                            value1 = valueOut1;
                        }
                    }
                    if (numberCount >= 2)
                    {
                        if (Int32.TryParse(lineArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 valueOut2))
                        {
                            value2 = valueOut2;
                        }
                    }
                    string[] textArray = lineArray.Skip(textOffset).ToArray();

                    switch (dataType)
                    {
                        case DataType.LongCoding:
                            dataInfoList.Add(new DataInfoLongCoding(dataType, value1, value2, textArray));
                            break;

                        default:
                            dataInfoList.Add(new DataInfo(dataType, value1, value2, textArray));
                            break;
                    }
                }

                List<DataInfo> dataInfoListOrder = dataInfoList.OrderBy(x => x.OrderIndex).ToList();
                return dataInfoListOrder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Init(string rootDir)
        {
            try
            {
                string[] files = Directory.GetFiles(rootDir, "code*" + CodeFileExtension, SearchOption.TopDirectoryOnly);
                if (files.Length != 1)
                {
                    return false;
                }
                List<string[]> lineList = ReadFileLines(files[0], true);
                if (lineList == null)
                {
                    return false;
                }
                CodeMap = new Dictionary<uint, string>();
                foreach (string[] lineArray in lineList)
                {
                    if (lineArray.Length != 2)
                    {
                        return false;
                    }
                    if (!UInt32.TryParse(lineArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 key))
                    {
                        return false;
                    }
                    CodeMap.Add(key, lineArray[1]);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
