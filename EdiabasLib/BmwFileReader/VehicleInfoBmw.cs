using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public class ServiceTreeItem
        {
            public ServiceTreeItem(string id)
            {
                Id = id;
                ChildItems = new List<ServiceTreeItem>();
                ServiceDataItem = null;
                ServiceInfoList = null;
                ServiceInfoListAux = null;
                ServiceMenuInfoDict = null;
                MenuId = -1;
            }

            public string Id { get; set; }

            public List<ServiceTreeItem> ChildItems { get; set; }

            public VehicleStructsBmw.ServiceDataItem ServiceDataItem { get; set; }

            public List<VehicleStructsBmw.ServiceInfoData> ServiceInfoList { get; set; }

            public List<VehicleStructsBmw.ServiceInfoData> ServiceInfoListAux { get; set; }

            public Dictionary<int, List<VehicleStructsBmw.ServiceInfoData>> ServiceMenuInfoDict { get; set; }

            public int MenuId { get; set; }

            public int InfoDataCount
            {
                get
                {
                    int infoCount = 0;
                    if (ServiceInfoList != null)
                    {
                        infoCount = ServiceInfoList.Count;
                    }

                    foreach (ServiceTreeItem childItem in ChildItems)
                    {
                        infoCount += childItem.InfoDataCount;
                    }

                    return infoCount;
                }
            }
            public int ServiceCount
            {
                get
                {
                    int serviceCount = 0;
                    if (ServiceInfoList != null && ServiceInfoList.Count > 0)
                    {
                        serviceCount = 1;
                    }

                    foreach (ServiceTreeItem childItem in ChildItems)
                    {
                        serviceCount += childItem.ServiceCount;
                    }

                    return serviceCount;
                }
            }
        }

        public class EbcdicVIN7Comparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        throw new ArgumentNullException("y");
                    }
                    return 1;
                }
                else
                {
                    if (y == null)
                    {
                        throw new ArgumentNullException("x");
                    }
                    if (x.Length == 7)
                    {
                        if (y.Length == 7)
                        {
                            int num = 0;
                            for (int i = 0; i < 7; i++)
                            {
                                if (char.IsDigit(x[i]))
                                {
                                    if (!char.IsDigit(y[i]))
                                    {
                                        return 1;
                                    }
                                    num = x[i].CompareTo(y[i]);
                                    if (num != 0)
                                    {
                                        return num;
                                    }
                                }
                                else
                                {
                                    if (char.IsDigit(y[i]))
                                    {
                                        return -1;
                                    }
                                    num = x[i].CompareTo(y[i]);
                                    if (num != 0)
                                    {
                                        return num;
                                    }
                                }
                            }
                            return num;
                        }
                    }
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Vin must be of length 7: {0}, {1}", x, y));
                }
            }
        }

#if Android
        public class TypeKeyInfo
        {
            public TypeKeyInfo(List<string> itemNames, Dictionary<string, List<string>> typeKeyDict)
            {
                ItemNames = itemNames;
                TypeKeyDict = typeKeyDict;
            }

            public List<string> ItemNames { get; set; }
            public Dictionary<string, List<string>> TypeKeyDict { get; set; }

            public int GetItemIndex(string itemName)
            {
                if (string.IsNullOrEmpty(itemName))
                {
                    return -1;
                }

                int index = 0;
                foreach (string currentName in ItemNames)
                {
                    if (string.Compare(currentName, itemName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return index;
                    }

                    index++;
                }

                return -1;
            }

            public string GetItem(string typeKey, string itemName)
            {
                int index = GetItemIndex(itemName);
                if (index < 0)
                {
                    return null;
                }

                if (!TypeKeyDict.TryGetValue(typeKey.ToUpperInvariant(), out List<string> typeKeyList))
                {
                    return null;
                }

                if (index >= typeKeyList.Count)
                {
                    return null;
                }

                return typeKeyList[index];
            }

            public Dictionary<string, string> GetTypeKeyProperties(string typeKey)
            {
                if (!TypeKeyDict.TryGetValue(typeKey.ToUpperInvariant(), out List<string> typeKeyList))
                {
                    return null;
                }

                Dictionary<string, string> propertyDict = new Dictionary<string, string>();
                int index = 0;
                foreach (string itemName in ItemNames)
                {
                    string itemValue = string.Empty;
                    if (index < typeKeyList.Count)
                    {
                        itemValue = typeKeyList[index] ?? string.Empty;
                    }

                    propertyDict.TryAdd(itemName, itemValue);
                    index++;
                }

                return propertyDict;
            }
        }

        public class VinRangeInfo
        {
            public VinRangeInfo(string rangeFrom, string rangeTo, string vin17_4_7, string typeKey, string prodYear, string prodMonth, string releaseState, string gearBox)
            {
                RangeFrom = rangeFrom;
                RangeTo = rangeTo;
                Vin17_4_7 = vin17_4_7;
                TypeKey = typeKey;
                ProdYear = prodYear;
                ProdMonth = prodMonth;
                ReleaseState = releaseState;
                GearBox = gearBox;
            }

            public string RangeFrom { get; }
            public string RangeTo { get; }
            public string Vin17_4_7 { get; }
            public string TypeKey { get; }
            public string ProdYear { get; }
            public string ProdMonth { get; }
            public string ReleaseState { get; }
            public string GearBox { get; }
        }

        private static TypeKeyInfo _typeKeyInfo;
#endif

        public const string ResultUnknown = "UNBEK";
        public const string VehicleSeriesName = "E-Bezeichnung";
        public const string ProductTypeName = "Produktart";
        public const string BrandName = "Marke";
        public const string TransmisionName = "Getriebe";
        public const string MotorName = "Motor";

        private static VehicleStructsBmw.VehicleSeriesInfoData _vehicleSeriesInfoData;
        private static VehicleStructsBmw.RulesInfoData _rulesInfoData;
        private static VehicleStructsBmw.ServiceData _serviceData;

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

                string resourceName = FindResourceName(VehicleStructsBmw.VehicleSeriesZipFile);
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
                        ZipFile zf = null;
                        try
                        {
                            zf = new ZipFile(stream);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, VehicleStructsBmw.VehicleSeriesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    using (Stream zipStream = zf.GetInputStream(zipEntry))
                                    {
                                        using (TextReader reader = new StreamReader(zipStream))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.VehicleSeriesInfoData));
                                            _vehicleSeriesInfoData = serializer.Deserialize(reader) as VehicleStructsBmw.VehicleSeriesInfoData;
                                        }
                                    }
                                }
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
                                using (Stream zipStream = zf.GetInputStream(zipEntry))
                                {
                                    using (StreamReader sr = new StreamReader(zipStream))
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.RulesInfoData));
                                        _rulesInfoData = serializer.Deserialize(sr) as VehicleStructsBmw.RulesInfoData;
                                    }
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

        public static VehicleStructsBmw.ServiceData ReadServiceData(string databaseDir)
        {
            if (_serviceData != null)
            {
                return _serviceData;
            }

            try
            {
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, VehicleStructsBmw.ServiceDataZipFile)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, VehicleStructsBmw.ServiceDataXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                using (Stream zipStream = zf.GetInputStream(zipEntry))
                                {
                                    using (StreamReader sr = new StreamReader(zipStream))
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.ServiceData));
                                        _serviceData = serializer.Deserialize(sr) as VehicleStructsBmw.ServiceData;
                                    }
                                }

                                break;
                            }
                        }
                    }

                    return _serviceData;
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
        public static TypeKeyInfo GetTypeKeyInfo(EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Extract type key info");

            try
            {
                List<string> itemNames = new List<string>();
                Dictionary<string, List<string>> typeKeyDict = new Dictionary<string, List<string>>();
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

                            if (string.Compare(zipEntry.Name, "typekeyinfo.txt", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                using (Stream zipStream = zf.GetInputStream(zipEntry))
                                {
                                    using (StreamReader sr = new StreamReader(zipStream, Encoding.UTF8, true, 0x1000))
                                    {
                                        while (!sr.EndOfStream)
                                        {
                                            string line = sr.ReadLine();
                                            if (line == null)
                                            {
                                                break;
                                            }

                                            bool isHeader = line.StartsWith("#");
                                            if (isHeader)
                                            {
                                                line = line.TrimStart('#');
                                            }

                                            string[] lineArray = line.Split('|');
                                            if (isHeader)
                                            {
                                                itemNames = lineArray.ToList();
                                                continue;
                                            }

                                            if (lineArray.Length > 1)
                                            {
                                                string key = lineArray[0].Trim();

                                                if (!string.IsNullOrEmpty(key))
                                                {
                                                    List<string> lineList = lineArray.ToList();
                                                    lineList.RemoveAt(0);
                                                    typeKeyDict.TryAdd(key, lineList);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Extract type key info done");
                        return new TypeKeyInfo(itemNames, typeKeyDict);
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
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Extract type key info exception: {0}", EdiabasNet.GetExceptionText(ex));
                return null;
            }
        }

        public static VinRangeInfo GetRangeInfoFromVin(string vin, EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type key from VIN: {0}", vin ?? "No VIN");
            if (vin == null)
            {
                return null;
            }

            string vin7;
            string vinType;

            if (vin.Length == 7)
            {
                vin7 = vin;
                vinType = null;
            }
            else if (vin.Length == 17)
            {
                vin7 = vin.Substring(10, 7);
                vinType = vin.Substring(3, 4);
            }
            else
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "VIN length invalid");
                return null;
            }

            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Matching Vin7: {0}, VinType: {1}", vin7, vinType);
            string bandType = vin7.Substring(0, 1).ToLowerInvariant();
            string fileName = string.Format(CultureInfo.InvariantCulture, "vinranges_{0}.txt", bandType);

            try
            {
                ZipFile zf = null;
                try
                {
                    IComparer<string> comparer = new EbcdicVIN7Comparer();
                    List<VinRangeInfo> vinRangeList = new List<VinRangeInfo>();

                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, EcuFunctionReader.EcuFuncFileName)))
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
                                using (Stream zipStream = zf.GetInputStream(zipEntry))
                                {
                                    using (StreamReader sr = new StreamReader(zipStream, Encoding.UTF8, true, 0x1000))
                                    {
                                        while (!sr.EndOfStream)
                                        {
                                            string line = sr.ReadLine();
                                            if (line == null)
                                            {
                                                break;
                                            }

                                            bool isComment = line.StartsWith("#");
                                            if (isComment)
                                            {
                                                continue;
                                            }

                                            string[] lineArray = line.Split(',');
                                            if (lineArray.Length >= 4 &&
                                                lineArray[0].Length == 7 && lineArray[1].Length == 7)
                                            {
                                                string rangeFrom = lineArray[0];
                                                string rangeTo = lineArray[1];
                                                if (string.Compare(vin7, rangeFrom, StringComparison.OrdinalIgnoreCase) >= 0 &&
                                                    string.Compare(vin7, rangeTo, StringComparison.OrdinalIgnoreCase) <= 0)
                                                {
                                                    if (!string.IsNullOrEmpty(vinType) &&
                                                        string.Compare(vinType, lineArray[2], StringComparison.OrdinalIgnoreCase) != 0)
                                                    {
                                                        continue;
                                                    }

                                                    bool rangeValid;
                                                    try
                                                    {
                                                        rangeValid = comparer.Compare(rangeFrom, vin7) <= 0 && comparer.Compare(rangeTo, vin7) >= 0;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        rangeValid = false;
                                                        ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VIN range exception: {0}", EdiabasNet.GetExceptionText(ex));
                                                    }

                                                    if (!rangeValid)
                                                    {
                                                        continue;
                                                    }

                                                    string vin17_4_7 = lineArray[2];
                                                    string typeKey = lineArray[3];
                                                    ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VIN matched: '{0}'-'{1}', TypeKey='{2}'",
                                                        lineArray[0], lineArray[1], typeKey);

                                                    string prodYear = null;
                                                    string prodMonth = null;
                                                    if (lineArray.Length >= 6)
                                                    {
                                                        prodYear = lineArray[4];
                                                        prodMonth = lineArray[5];
                                                    }

                                                    string releaseState = null;
                                                    if (lineArray.Length >= 7)
                                                    {
                                                        releaseState = lineArray[6];
                                                    }

                                                    string gearBox = null;
                                                    if (lineArray.Length >= 8)
                                                    {
                                                        gearBox = lineArray[7];
                                                    }

                                                    vinRangeList.Add(new VinRangeInfo(rangeFrom, rangeTo, vin17_4_7, typeKey, prodYear, prodMonth, releaseState, gearBox));

                                                    if (string.Compare(rangeFrom, vin7, StringComparison.OrdinalIgnoreCase) == 0 &&
                                                        string.Compare(rangeTo, vin7, StringComparison.OrdinalIgnoreCase) == 0)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }

                        ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "VIN ranges count: {0}", vinRangeList.Count);
                        if (vinRangeList.Count < 1)
                        {
                            return null;
                        }

                        if (vinRangeList.Count > 1)
                        {
                            if (!string.IsNullOrEmpty(vinType))
                            {
                                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Too many VIN range matches");
                                return null;
                            }

                            VinRangeInfo vinRangeInfo = vinRangeList[0];
                            // remove vin17_4_7 and gear box
                            return new VinRangeInfo(vinRangeInfo.RangeFrom, vinRangeInfo.RangeTo, string.Empty, vinRangeInfo.TypeKey, vinRangeInfo.ProdYear,
                                vinRangeInfo.ProdMonth, vinRangeInfo.ReleaseState, string.Empty);
                        }

                        return vinRangeList[0];
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

        public static Dictionary<string, string> GetVehiclePropertiesFromVin(string vin, EdiabasNet ediabas, string databaseDir, out VinRangeInfo vinRangeInfo)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle properties from VIN: {0}", vin ?? "No VIN");
            vinRangeInfo = GetRangeInfoFromVin(vin, ediabas, databaseDir);
            if (vinRangeInfo == null)
            {
                return null;
            }

            if (_typeKeyInfo == null)
            {
                _typeKeyInfo = GetTypeKeyInfo(ediabas, databaseDir);
            }

            if (_typeKeyInfo == null)
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No type key info present");
                return null;
            }

            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type key: {0}", vinRangeInfo.TypeKey);
            Dictionary<string, string> propertyDict = _typeKeyInfo.GetTypeKeyProperties(vinRangeInfo.TypeKey);
            if (propertyDict == null)
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No type key properties present");
                return null;
            }

            foreach (KeyValuePair<string, string> keyValuePair in propertyDict)
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Property: {0}, Value: {1}", keyValuePair.Key, keyValuePair.Value);
            }

            return propertyDict;
        }
#endif

        // ToDo: Check on update ExtractEreihe
        public static string GetVehicleSeriesFromBrName(string brName, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetVehicleSeriesFromBrName: {0}", brName ?? "No name");
            if (brName == null)
            {
                return null;
            }
            if (string.Compare(brName, ResultUnknown, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return null;
            }
#if Android
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
#else
            PsdzClient.Core.FA fa = new PsdzClient.Core.FA
            {
                BR = brName
            };

            string vehicleSeries = fa.ExtractEreihe();
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetVehicleSeriesFromBrName: {0}", vehicleSeries ?? string.Empty);
            return vehicleSeries;
#endif
        }

        // ToDo: Check on update ExtractType
        public static string GetVehicleTypeFromStdFa(string standardFa, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetVehicleTypeFromStdFa: {0}", standardFa ?? "No FA");
#if Android
            if (!string.IsNullOrEmpty(standardFa))
            {
                Match match = Regex.Match(standardFa, "\\*(?<TYPE>\\w{4})");
                if (match.Success)
                {
                    return match.Groups["TYPE"]?.Value;
                }
            }

            return null;
#else
            PsdzClient.Core.FA fa = new PsdzClient.Core.FA
            {
                STANDARD_FA = standardFa
            };

            string vehicleType = fa.ExtractType();
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "GetVehicleTypeFromStdFa: {0}", vehicleType ?? string.Empty);
            return vehicleType;
#endif
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

        public static VehicleStructsBmw.VehicleSeriesInfo GetVehicleSeriesInfo(DetectVehicleBmwBase detectVehicleBmw)
        {
            string series = detectVehicleBmw?.Series;
            string constructYear = detectVehicleBmw?.ConstructYear;
            string constructMonth = detectVehicleBmw?.ConstructMonth;
            string iLevelShip = detectVehicleBmw?.ILevelShip;
            EdiabasNet ediabas = detectVehicleBmw?.Ediabas;

            string modelSeries = null;
            if (!string.IsNullOrEmpty(iLevelShip) && iLevelShip.Length >= 4)
            {
                string[] iLevelParts = iLevelShip.Split('-');
                if (iLevelParts.Length > 1 && iLevelParts[0].Length >= 3)
                {
                    modelSeries = iLevelParts[0];
                }
            }

            long dateValue = -1;
            if (!string.IsNullOrEmpty(constructYear) && !string.IsNullOrEmpty(constructMonth))
            {
                if (Int32.TryParse(constructYear, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 year) &&
                    Int32.TryParse(constructMonth, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 month))
                {
                    dateValue = year * 100 + month;
                }
            }

            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info from vehicle series: {0}, modelSeries: {1}, date: {2}",
                series ?? string.Empty, modelSeries ?? string.Empty, dateValue);
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

                List<string> modelSeriesList = new List<string>();
                List<string> brSgbdList = new List<string>();
                List<string> sgbdAddList = new List<string>();
                List<string> bnTypeList = new List<string>();
                List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoMatches = new List<VehicleStructsBmw.VehicleSeriesInfo>();
                foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo in vehicleSeriesInfoList)
                {
                    if (!string.IsNullOrEmpty(vehicleSeriesInfo.ModelSeries))
                    {
                        if (!modelSeriesList.Contains(vehicleSeriesInfo.ModelSeries, StringComparer.OrdinalIgnoreCase))
                        {
                            modelSeriesList.Add(vehicleSeriesInfo.ModelSeries);
                        }
                    }

                    if (!string.IsNullOrEmpty(vehicleSeriesInfo.BrSgbd))
                    {
                        if (!brSgbdList.Contains(vehicleSeriesInfo.BrSgbd, StringComparer.OrdinalIgnoreCase))
                        {
                            brSgbdList.Add(vehicleSeriesInfo.BrSgbd);
                        }
                    }

                    if (!string.IsNullOrEmpty(vehicleSeriesInfo.SgbdAdd))
                    {
                        if (!sgbdAddList.Contains(vehicleSeriesInfo.SgbdAdd, StringComparer.OrdinalIgnoreCase))
                        {
                            sgbdAddList.Add(vehicleSeriesInfo.SgbdAdd);
                        }
                    }

                    if (!string.IsNullOrEmpty(vehicleSeriesInfo.BnType))
                    {
                        if (!bnTypeList.Contains(vehicleSeriesInfo.BnType, StringComparer.OrdinalIgnoreCase))
                        {
                            bnTypeList.Add(vehicleSeriesInfo.BnType);
                        }
                    }

                    bool matched = false;
                    if (!string.IsNullOrEmpty(modelSeries) && !string.IsNullOrEmpty(vehicleSeriesInfo.ModelSeries))
                    {
                        ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Checking model series");
                        if (string.Compare(vehicleSeriesInfo.ModelSeries, modelSeries, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Matched model series: {0}", modelSeries);
                            matched = true;
                        }
                    }

                    if (matched)
                    {
                        vehicleSeriesInfoMatches.Add(vehicleSeriesInfo);
                    }
                }

                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info matches: {0}", vehicleSeriesInfoMatches.Count);
                if (vehicleSeriesInfoMatches.Count == 1)
                {
                    return vehicleSeriesInfoMatches[0];
                }

                if (dateValue >= 0)
                {
                    for (int i = 0; i < vehicleSeriesInfoMatches.Count; i++)
                    {
                        VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = vehicleSeriesInfoMatches[i];
                        if (!string.IsNullOrEmpty(vehicleSeriesInfo.Date) && !string.IsNullOrEmpty(vehicleSeriesInfo.DateCompare))
                        {
                            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Checking date");

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
                            }
                            else
                            {
                                vehicleSeriesInfoMatches.Remove(vehicleSeriesInfo);
                                i--;
                            }
                        }
                    }

                    ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info matches date: {0}", vehicleSeriesInfoMatches.Count);
                    if (vehicleSeriesInfoMatches.Count == 1)
                    {
                        return vehicleSeriesInfoMatches[0];
                    }
                }

                if (detectVehicleBmw != null)
                {
                    for (int i = 0; i < vehicleSeriesInfoMatches.Count; i++)
                    {
                        VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = vehicleSeriesInfoMatches[i];
                        if (vehicleSeriesInfo.RuleEcus != null && vehicleSeriesInfo.RuleEcus.Count > 0)
                        {
                            VehicleStructsBmw.VehicleEcuInfo ecuInfoMatch = null;
                            foreach (VehicleStructsBmw.VehicleEcuInfo ecuInfo in vehicleSeriesInfo.RuleEcus)
                            {
                                if (!string.IsNullOrEmpty(ecuInfo.GroupSgbd) && !string.IsNullOrEmpty(ecuInfo.Sgbd))
                                {
                                    ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Checking ecu name Group: {0}, Name: {1}", ecuInfo.GroupSgbd, ecuInfo.Name);
                                    string ecuName = detectVehicleBmw.GetEcuNameByIdentCached(ecuInfo.GroupSgbd);
                                    if (!string.IsNullOrEmpty(ecuName))
                                    {
                                        ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Resolved ecu name: {0}", ecuName);
                                        if (string.Compare(ecuName, ecuInfo.Sgbd, StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            ecuInfoMatch = ecuInfo;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (ecuInfoMatch != null)
                            {
                                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Matched ecu name Group: {0}, Name: {1}", ecuInfoMatch.GroupSgbd, ecuInfoMatch.Name);
                            }
                            else
                            {
                                vehicleSeriesInfoMatches.Remove(vehicleSeriesInfo);
                                i--;
                            }
                        }
                    }

                    ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info matches ECU: {0}", vehicleSeriesInfoMatches.Count);
                    if (vehicleSeriesInfoMatches.Count == 1)
                    {
                        return vehicleSeriesInfoMatches[0];
                    }
                }

                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle info counts: ModelSerie: {0}, BrSgbd: {1}, SgbdAdd: {2}, BnType: {3}",
                    modelSeriesList.Count, brSgbdList.Count, sgbdAddList.Count, bnTypeList.Count);
                string modelSeriesInfo = null;
                string brSgbdInfo = null;
                string sgbdAddInfo = null;
                string bnTypeInfo = null;

                if (modelSeriesList.Count == 1)
                {
                    modelSeriesInfo = modelSeriesList[0];
                }

                if (brSgbdList.Count == 1)
                {
                    brSgbdInfo = brSgbdList[0];
                }

                if (sgbdAddList.Count == 1)
                {
                    sgbdAddInfo = sgbdAddList[0];
                }

                if (bnTypeList.Count == 1)
                {
                    bnTypeInfo = bnTypeList[0];
                }

                if (!string.IsNullOrEmpty(modelSeriesInfo))
                {
                    ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using detected ModelSeries: {0}, BrSgbd: {1}, SgbdAdd: {2}, BnType: {3}",
                        modelSeriesInfo ?? string.Empty, brSgbdInfo ?? string.Empty, sgbdAddInfo ?? string.Empty, bnTypeInfo ?? string.Empty);
                    return new VehicleStructsBmw.VehicleSeriesInfo(series, modelSeriesInfo, brSgbdInfo, sgbdAddInfo, bnTypeInfo);
                }
            }

            switch (key[0])
            {
                case 'F':
                case 'G':
                case 'I':
                case 'U':
                {
                    string brSgbd = "F01";
                    ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Using fallback BrSgbd: {0}", brSgbd);
                    return new VehicleStructsBmw.VehicleSeriesInfo(series, null, brSgbd, null, "BN2020");
                }
            }

            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No vehicle series info found");
            return null;
        }

        public static VehicleStructsBmw.VehicleEcuInfo GetEcuInfoByGroupName(VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo, string name)
        {
            if (vehicleSeriesInfo?.EcuList == null)
            {
                return null;
            }

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

        public static string RemoveNonAsciiChars(string text)
        {
            try
            {
                return new ASCIIEncoding().GetString(Encoding.ASCII.GetBytes(text.ToCharArray()));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

#if Android
        public static List<VehicleStructsBmw.ServiceDataItem> GetServiceDataItems(string databaseDir, RuleEvalBmw ruleEvalBmw = null)
        {
            VehicleStructsBmw.ServiceData serviceData = ReadServiceData(databaseDir);
            if (serviceData == null)
            {
                return null;
            }

            if (ruleEvalBmw == null)
            {
                return serviceData.ServiceDataList;
            }

            List<VehicleStructsBmw.ServiceDataItem> serviceDataItems = new List<VehicleStructsBmw.ServiceDataItem>();
            foreach (VehicleStructsBmw.ServiceDataItem serviceDataItem in serviceData.ServiceDataList)
            {
                bool valid = ruleEvalBmw.EvaluateRule(serviceDataItem.InfoObjId, RuleEvalBmw.RuleType.DiagObj);
                if (valid)
                {
                    foreach (string diagObjId in serviceDataItem.DiagObjIds)
                    {
                        if (!ruleEvalBmw.EvaluateRule(diagObjId, RuleEvalBmw.RuleType.DiagObj))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                    {
                        serviceDataItems.Add(serviceDataItem);
                    }
                }
            }

            return serviceDataItems;
        }

        public static bool IsValidServiceInfoData(VehicleStructsBmw.ServiceInfoData serviceInfoData, List<string> validSgbds, out bool auxItem)
        {
            auxItem = false;
            if (serviceInfoData == null)
            {
                return false;
            }

            string jobBare = serviceInfoData.EdiabasJobBare;
            if (string.IsNullOrEmpty(jobBare))
            {
                return false;
            }

            string[] jobBareItems = jobBare.Split('#');
            if (jobBareItems.Length < 2)
            {
                return false;
            }

            string jobSgbd = jobBareItems[0].Trim();
            if (string.IsNullOrEmpty(jobSgbd))
            {
                return false;
            }

            if (validSgbds != null && validSgbds.Count > 0)
            {
                bool valid = false;
                foreach (string sgbd in validSgbds)
                {
                    if (string.Compare(jobSgbd, sgbd.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        valid = true;
                        break;
                    }
                }

                if (!valid)
                {
                    return false;
                }
            }
#if false
            if (serviceInfoData.TextHashes == null || serviceInfoData.TextHashes.Count < 1)
            {
                return false;
            }
#endif
            if (jobBareItems[1].StartsWith("IDENT", StringComparison.OrdinalIgnoreCase))
            {
                auxItem = true;
                return false;
            }

            if (jobBareItems[1].StartsWith("STATUS", StringComparison.OrdinalIgnoreCase))
            {
                auxItem = true;
            }

            if (jobBareItems[1].Contains("LESEN", StringComparison.OrdinalIgnoreCase))
            {
                auxItem = true;
            }

            return true;
        }

        public static ServiceTreeItem GetServiceItemTree(List<VehicleStructsBmw.ServiceDataItem> serviceDataItems, List<string> validSgbds = null)
        {
            if (serviceDataItems == null)
            {
                return null;
            }

            ServiceTreeItem serviceTreeItemRoot = new ServiceTreeItem(null);
            foreach (VehicleStructsBmw.ServiceDataItem serviceDataItem in serviceDataItems)
            {
                List<VehicleStructsBmw.ServiceInfoData> serviceInfoList = new List<VehicleStructsBmw.ServiceInfoData>();
                List<VehicleStructsBmw.ServiceInfoData> serviceInfoListAux = new List<VehicleStructsBmw.ServiceInfoData>();
                foreach (VehicleStructsBmw.ServiceInfoData serviceInfoData in serviceDataItem.InfoDataList)
                {
                    if (!IsValidServiceInfoData(serviceInfoData, validSgbds, out bool auxItem))
                    {
                        continue;
                    }

                    if (auxItem)
                    {
                        serviceInfoListAux.Add(serviceInfoData);
                    }
                    else
                    {
                        serviceInfoList.Add(serviceInfoData);
                    }
                }

                if (serviceInfoList.Count == 0)
                {
                    continue;
                }

                ServiceTreeItem serviceTreeItemCurrent = serviceTreeItemRoot;
                foreach (string diagObjId in serviceDataItem.DiagObjIds)
                {
                    if (string.IsNullOrEmpty(diagObjId))
                    {
                        continue;
                    }

                    ServiceTreeItem childItemDiagMatch = null;
                    foreach (ServiceTreeItem childItem in serviceTreeItemCurrent.ChildItems)
                    {
                        if (childItem.Id == diagObjId)
                        {
                            childItemDiagMatch = childItem;
                            break;
                        }
                    }

                    if (childItemDiagMatch == null)
                    {
                        childItemDiagMatch = new ServiceTreeItem(diagObjId);
                        serviceTreeItemCurrent.ChildItems.Add(childItemDiagMatch);
                    }

                    serviceTreeItemCurrent = childItemDiagMatch;
                }

                ServiceTreeItem childItemInfoMatch = null;
                foreach (ServiceTreeItem childItem in serviceTreeItemCurrent.ChildItems)
                {
                    if (childItem.Id == serviceDataItem.InfoObjId)
                    {
                        childItemInfoMatch = childItem;
                        break;
                    }
                }

                if (childItemInfoMatch == null)
                {
                    childItemInfoMatch = new ServiceTreeItem(serviceDataItem.InfoObjId);
                    serviceTreeItemCurrent.ChildItems.Add(childItemInfoMatch);
                }

                childItemInfoMatch.ServiceDataItem = serviceDataItem;
                childItemInfoMatch.ServiceInfoList = serviceInfoList;
                childItemInfoMatch.ServiceInfoListAux = serviceInfoListAux;
            }

            return serviceTreeItemRoot;
        }

        public static VehicleStructsBmw.ServiceTextData GetServiceTextDataForHash(string hashCode)
        {
            if (_serviceData == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(hashCode))
            {
                return null;
            }

            if (!_serviceData.TextDict.TryGetValue(hashCode, out VehicleStructsBmw.ServiceTextData textData))
            {
                return null;
            }

            return textData;
        }
#endif
        }
}
