using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using EdiabasLib;
using ICSharpCode.SharpZipLib.Zip;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace BmwFileReader
{
    public class VehicleInfoBmw
    {
        public enum FailureSource
        {
            None,
            Resource,
            File
        }

        public const string ResultUnknown = "UNBEK";

#if Android
        private static Dictionary<string, string> _typeKeyDict;
#endif
        private static VehicleStructsBmw.VehicleSeriesInfoData _vehicleSeriesInfoData;
        private static VehicleStructsBmw.RulesInfoData _rulesInfoData;

        public static FailureSource ResourceFailure { get; private set; }

        public static void ClearResourceFailure()
        {
            ResourceFailure = FailureSource.None;
        }

        public static string FindResourceName(string resourceFileName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                foreach (string resourceName in resourceNames)
                {
                    string[] resourceParts = resourceName.Split('.');
                    if (resourceParts.Length < 2)
                    {
                        continue;
                    }

                    string fileName = resourceParts[resourceParts.Length - 2] + "." + resourceParts[resourceParts.Length - 1];
                    if (string.Compare(fileName, resourceFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return resourceName;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public static VehicleStructsBmw.VehicleSeriesInfoData ReadVehicleSeriesInfo()
        {
            try
            {
                if (_vehicleSeriesInfoData != null)
                {
                    return _vehicleSeriesInfoData;
                }

                string resourceName = FindResourceName(VehicleStructsBmw.VehicleSeriesXmlFile);
                if (string.IsNullOrEmpty(resourceName))
                {
                    ResourceFailure = FailureSource.Resource;
                    return null;
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.VehicleSeriesInfoData));
                        _vehicleSeriesInfoData = serializer.Deserialize(stream) as VehicleStructsBmw.VehicleSeriesInfoData;
                    }
                }

                return _vehicleSeriesInfoData;
            }
            catch (Exception)
            {
                ResourceFailure = FailureSource.Resource;
                return null;
            }
        }

        public static VehicleStructsBmw.RulesInfoData ReadRulesInfo(string databaseDir)
        {
            if (_rulesInfoData != null)
            {
                return _rulesInfoData;
            }

            _rulesInfoData = ReadRulesInfoFromResource();
            if (_rulesInfoData != null)
            {
                return _rulesInfoData;
            }

            _rulesInfoData = ReadRulesInfoFromFile(databaseDir);
            if (_rulesInfoData != null)
            {
                return _rulesInfoData;
            }

            return null;
        }

        public static VehicleStructsBmw.RulesInfoData ReadRulesInfoFromResource()
        {
            try
            {
                if (_rulesInfoData != null)
                {
                    return _rulesInfoData;
                }

                string resourceName = FindResourceName(VehicleStructsBmw.RulesXmlFile);
                if (string.IsNullOrEmpty(resourceName))
                {
                    return null;
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.RulesInfoData));
                        _rulesInfoData = serializer.Deserialize(stream) as VehicleStructsBmw.RulesInfoData;
                    }
                }

                return _rulesInfoData;
            }
            catch (Exception)
            {
                ResourceFailure = FailureSource.Resource;
                return null;
            }
        }

        public static VehicleStructsBmw.RulesInfoData ReadRulesInfoFromFile(string databaseDir)
        {
            if (_rulesInfoData != null)
            {
                return _rulesInfoData;
            }

            try
            {
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, VehicleStructsBmw.RulesZipFile)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, VehicleStructsBmw.RulesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (StreamReader sr = new StreamReader(zipStream))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.RulesInfoData));
                                    _rulesInfoData = serializer.Deserialize(sr) as VehicleStructsBmw.RulesInfoData;
                                }

                                break;
                            }
                        }
                    }

                    return _rulesInfoData;
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
                ResourceFailure = FailureSource.File;
                return null;
            }
        }

        public static int GetModelYearFromVin(string vin)
        {
            try
            {
                if (string.IsNullOrEmpty(vin) || vin.Length < 10)
                {
                    return -1;
                }

                char yearCode = vin.ToUpperInvariant()[9];
                if (yearCode == '0')
                {
                    return -1;
                }
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

#if Android
        public static Dictionary<string, string> GetTypeKeyDict(EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Extract type key dict");

            try
            {
                Dictionary<string, string> typeKeyDict = new Dictionary<string, string>();
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, EcuFunctionReader.EcuFuncFileName)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, "typekeys.txt", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (StreamReader sr = new StreamReader(zipStream))
                                {
                                    while (sr.Peek() >= 0)
                                    {
                                        string line = sr.ReadLine();
                                        if (line == null)
                                        {
                                            break;
                                        }
                                        string[] lineArray = line.Split(',');
                                        if (lineArray.Length == 2)
                                        {
                                            if (!typeKeyDict.ContainsKey(lineArray[0]))
                                            {
                                                typeKeyDict.Add(lineArray[0], lineArray[1]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Extract type key dict done");
                        return typeKeyDict;
                    }
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
            catch (Exception ex)
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Extract type key dict exception: {0}", EdiabasNet.GetExceptionText(ex));
                return null;
            }
        }

        public static string GetTypeKeyFromVin(string vin, EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type key from VIN: {0}", vin ?? "No VIN");
            if (vin == null)
            {
                return null;
            }
            string serialNumber;
            if (vin.Length == 7)
            {
                serialNumber = vin;
            }
            else if (vin.Length == 17)
            {
                serialNumber = vin.Substring(10, 7);
            }
            else
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "VIN length invalid");
                return null;
            }

            try
            {
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, EcuFunctionReader.EcuFuncFileName)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, "vinranges.txt", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (StreamReader sr = new StreamReader(zipStream))
                                {
                                    while (sr.Peek() >= 0)
                                    {
                                        string line = sr.ReadLine();
                                        if (line == null)
                                        {
                                            break;
                                        }
                                        string[] lineArray = line.Split(',');
                                        if (lineArray.Length == 3 &&
                                            lineArray[0].Length == 7 && lineArray[1].Length == 7)
                                        {
                                            if (string.Compare(serialNumber, lineArray[0], StringComparison.OrdinalIgnoreCase) >= 0 &&
                                                string.Compare(serialNumber, lineArray[1], StringComparison.OrdinalIgnoreCase) <= 0)
                                            {
                                                return lineArray[2];
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Type key not found in vin ranges");
                        return null;
                    }
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
            catch (Exception ex)
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type key from VIN exception: {0}", EdiabasNet.GetExceptionText(ex));
                return null;
            }
        }

        public static string GetVehicleTypeFromVin(string vin, EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle type from VIN: {0}", vin ?? "No VIN");
            string typeKey = GetTypeKeyFromVin(vin, ediabas, databaseDir);
            if (typeKey == null)
            {
                return null;
            }
            if (_typeKeyDict == null)
            {
                _typeKeyDict = GetTypeKeyDict(ediabas, databaseDir);
            }
            if (_typeKeyDict == null)
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No type key dict present");
                return null;
            }
            if (!_typeKeyDict.TryGetValue(typeKey.ToUpperInvariant(), out string vehicleType))
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle type not found");
                return null;
            }
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle type: {0}", vehicleType);
            return vehicleType;
        }
#endif

        // from: RheingoldCoreFramework.dll BMW.Rheingold.CoreFramework.DatabaseProvider.FA.ExtractEreihe
        public static string GetVehicleTypeFromBrName(string brName, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle type from BR name: {0}", brName ?? "No name");
            if (brName == null)
            {
                return null;
            }
            if (string.Compare(brName, ResultUnknown, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return null;
            }
            if (brName.Length != 4)
            {
                return null;
            }
            if (brName.EndsWith("_", StringComparison.Ordinal))
            {
                string vehicleType = brName.TrimEnd('_');
                if (Regex.Match(vehicleType, "[ERKHM]\\d\\d").Success)
                {
                    return vehicleType;
                }
            }
            if (brName.StartsWith("RR", StringComparison.OrdinalIgnoreCase))
            {
                string vehicleType = brName.TrimEnd('_');
                if (Regex.Match(vehicleType, "^RR\\d$").Success)
                {
                    return vehicleType;
                }
                if (Regex.Match(vehicleType, "^RR0\\d$").Success)
                {
                    return "RR" + brName.Substring(3, 1);
                }
                if (Regex.Match(vehicleType, "^RR1\\d$").Success)
                {
                    return vehicleType;
                }
            }
            return brName.Substring(0, 1) + brName.Substring(2, 2);
        }

        public static DateTime? ConvertConstructionDate(string cDateStr)
        {
            if (string.IsNullOrWhiteSpace(cDateStr))
            {
                return null;
            }

            if (DateTime.TryParseExact(cDateStr.Trim(), "MMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public static string GetVehicleSeriesInfoTimeStamp()
        {
            VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = ReadVehicleSeriesInfo();
            if (vehicleSeriesInfoData == null)
            {
                return string.Empty;
            }

            return vehicleSeriesInfoData.TimeStamp ?? string.Empty;
        }

        public static VehicleStructsBmw.VersionInfo GetVehicleSeriesInfoVersion()
        {
            VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = ReadVehicleSeriesInfo();
            if (vehicleSeriesInfoData == null)
            {
                return null;
            }

            return vehicleSeriesInfoData.Version;
        }

        public static VehicleStructsBmw.VehicleSeriesInfo GetVehicleSeriesInfo(string series, DateTime? cDate, EdiabasNet ediabas)
        {
            string cDateStr = "No date";
            long dateValue = -1;
            if (cDate.HasValue)
            {
                cDateStr = cDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                dateValue = cDate.Value.Year * 100 + cDate.Value.Month;
            }

            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info from vehicle series: {0}, CDate: {1}", series ?? "No series", cDateStr);
            if (series == null)
            {
                return null;
            }

            VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = ReadVehicleSeriesInfo();
            if (vehicleSeriesInfoData == null)
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No vehicle series info");
                return null;
            }

            string key = series.Trim().ToUpperInvariant();
            if (!vehicleSeriesInfoData.VehicleSeriesDict.TryGetValue(key, out List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList))
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle series not found");
            }

            if (vehicleSeriesInfoList != null)
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info count: {0}", vehicleSeriesInfoList.Count);
                if (vehicleSeriesInfoList.Count == 1)
                {
                    return vehicleSeriesInfoList[0];
                }

                if (dateValue >= 0)
                {
                    ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Checking date");

                    foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo in vehicleSeriesInfoList)
                    {
                        if (!string.IsNullOrEmpty(vehicleSeriesInfo.Date) && !string.IsNullOrEmpty(vehicleSeriesInfo.DateCompare))
                        {
                            VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfoMatch = null;
                            if (long.TryParse(vehicleSeriesInfo.Date, NumberStyles.Integer, CultureInfo.InvariantCulture, out long dateCompare))
                            {
                                string dateCompre = vehicleSeriesInfo.DateCompare.ToUpperInvariant();
                                if (dateCompre.Contains("<"))
                                {
                                    if (dateCompre.Contains("="))
                                    {
                                        if (dateCompare <= dateValue)
                                        {
                                            vehicleSeriesInfoMatch = vehicleSeriesInfo;
                                        }
                                    }
                                    else
                                    {
                                        if (dateCompare < dateValue)
                                        {
                                            vehicleSeriesInfoMatch = vehicleSeriesInfo;
                                        }
                                    }
                                }
                                else if (dateCompre.Contains(">"))
                                {
                                    if (dateCompre.Contains("="))
                                    {
                                        if (dateCompare >= dateValue)
                                        {
                                            vehicleSeriesInfoMatch = vehicleSeriesInfo;
                                        }
                                    }
                                    else
                                    {
                                        if (dateCompare > dateValue)
                                        {
                                            vehicleSeriesInfoMatch = vehicleSeriesInfo;
                                        }
                                    }
                                }
                            }

                            if (vehicleSeriesInfoMatch != null)
                            {
                                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Matched date expression: {0} {1}", vehicleSeriesInfoMatch.DateCompare, vehicleSeriesInfoMatch.Date);
                                return vehicleSeriesInfoMatch;
                            }
                        }
                    }
                }
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No date matched");
            }

            switch (key[0])
            {
                case 'F':
                case 'G':
                case 'I':
                case 'J':
                case 'U':
                    ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Using fallback from first letter");
                    return new VehicleStructsBmw.VehicleSeriesInfo(series, "F01", "BN2020");
            }

            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No vehicle series info found");
            return null;
        }

        public static VehicleStructsBmw.VehicleEcuInfo GetEcuInfoByGroupName(VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo, string name)
        {
            string nameLower = name.ToLowerInvariant();
            foreach (VehicleStructsBmw.VehicleEcuInfo ecuInfo in vehicleSeriesInfo.EcuList)
            {
                if (ecuInfo.GroupSgbd.ToLowerInvariant().Contains(nameLower))
                {
                    return ecuInfo;
                }
            }
            return null;
        }
    }
}
