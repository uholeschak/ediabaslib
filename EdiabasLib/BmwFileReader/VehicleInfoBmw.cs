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
#if Android
using ICSharpCode.SharpZipLib.Zip;
#endif

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace BmwFileReader
{
    public class VehicleInfoBmw
    {
        // ReSharper disable InconsistentNaming
        public enum BusType
        {
            ROOT,
            ETHERNET,
            MOST,
            KCAN,
            KCAN2,
            KCAN3,
            BCAN,
            BCAN2,
            BCAN3,
            FLEXRAY,
            FACAN,
            FASCAN,
            SCAN,
            NONE,
            SIBUS,
            KBUS,
            FCAN,
            ACAN,
            HCAN,
            LOCAN,
            ZGW,
            DWA,
            BYTEFLIGHT,
            INTERNAL,
            VIRTUAL,
            VIRTUALBUSCHECK,
            VIRTUALROOT,
            IBUS,
            LECAN,
            IKCAN,
            UNKNOWN
        }
        // ReSharper restore InconsistentNaming

        public interface IEcuLogisticsEntry
        {
            int DiagAddress { get; }

            string Name { get; }

            string GroupSgbd { get; }

            BusType Bus { get; }

            int Column { get; }

            int Row { get; }

            string ShortName { get; }

            long? SubDiagAddress { get; }

            BusType[] SubBusList { get; }
        }

        public class EcuLogisticsEntry : IEcuLogisticsEntry
        {
            public int DiagAddress { get; }
            public string Name { get; }
            public BusType Bus { get; }
            public BusType[] SubBusList { get; }
            public string GroupSgbd { get; }
            public int Column { get; }
            public int Row { get; }
            public string ShortName { get; }
            public long? SubDiagAddress { get; }

            public EcuLogisticsEntry()
            {
            }

            public EcuLogisticsEntry(int diagAddress, string name, BusType bus, string groupSgbd, int column, int row)
                : this(diagAddress, null, name, bus, null, groupSgbd, column, row, null)
            {
            }

            public EcuLogisticsEntry(int diagAddress, string name, BusType bus, BusType[] subBusList, string groupSgbd,
                int column, int row) : this(diagAddress, null, name, bus, subBusList, groupSgbd, column, row, null)
            {
            }

            public EcuLogisticsEntry(int diagAddress, int subDiagAddress, string name, BusType bus, string groupSgbd,
                int column, int row)
                : this(diagAddress, subDiagAddress, name, bus, null, groupSgbd, column, row, null)
            {
            }

            public EcuLogisticsEntry(int diagAddress, string name, BusType bus, string groupSgbd, int column, int row,
                string shortName) : this(diagAddress, null, name, bus, null, groupSgbd, column, row, shortName)
            {
            }

            public EcuLogisticsEntry(int diagAddress, long? subDiagAddress, string name, BusType bus,
                BusType[] subBusList, string groupSgbd, int column, int row, string shortName)
            {
                DiagAddress = diagAddress;
                Name = name;
                Bus = bus;
                SubBusList = subBusList;
                GroupSgbd = groupSgbd;
                Column = column;
                Row = row;
                ShortName = shortName;
                SubDiagAddress = subDiagAddress;
            }
        }

        public class EcuLogisticsData
        {
            public EcuLogisticsData(string xmlName)
            {
                XmlName = xmlName;
                Data = null;
            }

            public string XmlName { get; }
            public ReadOnlyCollection<IEcuLogisticsEntry> Data { get; set; }
        }

        private const string DatabaseFileName = @"Database.zip";

        public static EcuLogisticsData EcuLogisticsDataE36 = new EcuLogisticsData("E36EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE38 = new EcuLogisticsData("E38EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE39 = new EcuLogisticsData("E39EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE46 = new EcuLogisticsData("E46EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE52 = new EcuLogisticsData("E52EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE53 = new EcuLogisticsData("E53EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE83 = new EcuLogisticsData("E83EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataE85 = new EcuLogisticsData("E85EcuCharacteristics.xml");
        public static EcuLogisticsData EcuLogisticsDataR50 = new EcuLogisticsData("R50EcuCharacteristics.xml");

        public static ReadOnlyCollection<EcuLogisticsData> EcuLogisticsList = new ReadOnlyCollection<EcuLogisticsData>(new EcuLogisticsData[]
        {
            EcuLogisticsDataE36,
            EcuLogisticsDataE38,
            EcuLogisticsDataE39,
            EcuLogisticsDataE46,
            EcuLogisticsDataE52,
            EcuLogisticsDataE83,
            EcuLogisticsDataE85,
            EcuLogisticsDataR50,
        });

        public const string ResultUnknown = "UNBEK";

#if Android
        private static Dictionary<string, string> _typeKeyDict;
#endif
        private static bool EcuLogisticsCreated;

        private static VehicleStructsBmw.VehicleSeriesInfoData _vehicleSeriesInfoData;

        public static bool CreateEcuLogistics(string resourcePath)
        {
            if (!EcuLogisticsCreated)
            {
                bool failed = false;
                foreach (EcuLogisticsData ecuLogisticsData in EcuLogisticsList)
                {
                    if (ecuLogisticsData.Data == null)
                    {
                        string resourceName = resourcePath + ecuLogisticsData.XmlName;
                        ecuLogisticsData.Data = ReadEcuLogisticsXml(resourceName);
                    }

                    if (ecuLogisticsData.Data == null)
                    {
                        failed = true;
                    }
                }

                if (!failed)
                {
                    EcuLogisticsCreated = true;
                }
            }

            return EcuLogisticsCreated;
        }

        public static ReadOnlyCollection<IEcuLogisticsEntry> ReadEcuLogisticsXml(string resourceName)
        {
            try
            {
                List<IEcuLogisticsEntry> ecuLogisticsList = new List<IEcuLogisticsEntry>();
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    XDocument xmlDoc = XDocument.Load(stream);
                    if (xmlDoc.Root == null)
                    {
                        return null;
                    }
                    XNamespace ns = xmlDoc.Root.GetDefaultNamespace();
                    XElement logisticsList = xmlDoc.Root.Element(ns + "EcuLogisticsList");
                    if (logisticsList == null)
                    {
                        return null;
                    }

                    foreach (XElement ecuLogisticsNode in logisticsList.Elements(ns + "EcuLogisticsEntry"))
                    {
                        int diagAddress = 0;
                        string name = string.Empty;
                        BusType busType = BusType.ROOT;
                        string groupSgbd = string.Empty;
                        int column = 0;
                        int row = 0;

                        XAttribute diagAddrAttrib = ecuLogisticsNode.Attribute("DiagAddress");
                        if (diagAddrAttrib != null)
                        {
                            if (!Int32.TryParse(diagAddrAttrib.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out diagAddress))
                            {
                                diagAddress = 0;
                            }
                        }

                        XAttribute nameAttrib = ecuLogisticsNode.Attribute("Name");
                        if (nameAttrib != null)
                        {
                            name = nameAttrib.Value;
                        }

                        XElement busNode = ecuLogisticsNode.Element(ns + "Bus");
                        if (busNode != null)
                        {
                            if (!Enum.TryParse(busNode.Value, true, out busType))
                            {
                                busType = BusType.ROOT;
                            }
                        }

                        XElement groupSgbdNode = ecuLogisticsNode.Element(ns + "GroupSgbd");
                        if (groupSgbdNode != null)
                        {
                            groupSgbd = groupSgbdNode.Value;
                        }

                        XElement columnNode = ecuLogisticsNode.Element(ns + "Column");
                        if (columnNode != null)
                        {
                            if (!Int32.TryParse(columnNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out column))
                            {
                                column = 0;
                            }
                        }

                        XElement rowNode = ecuLogisticsNode.Element(ns + "Row");
                        if (rowNode != null)
                        {
                            if (!Int32.TryParse(rowNode.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out row))
                            {
                                row = 0;
                            }
                        }

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(groupSgbd))
                        {
                            ecuLogisticsList.Add(new EcuLogisticsEntry(diagAddress, name, busType, groupSgbd, column, row));
                        }
                    }
                }

                return new ReadOnlyCollection<IEcuLogisticsEntry>(ecuLogisticsList);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static VehicleStructsBmw.VehicleSeriesInfoData ReadVehicleSeriesInfo()
        {
            try
            {
                if (_vehicleSeriesInfoData != null)
                {
                    return _vehicleSeriesInfoData;
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();

                string resource = null;
                foreach (string resourceName in resourceNames)
                {
                    string[] resourceParts = resourceName.Split('.');
                    if (resourceParts.Length < 2)
                    {
                        continue;
                    }

                    string fileName = resourceParts[resourceParts.Length - 2] + "." + resourceParts[resourceParts.Length - 1];
                    if (string.Compare(fileName, VehicleStructsBmw.VehicleSeriesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        resource = resourceName;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(resource))
                {
                    return null;
                }

                using (Stream stream = assembly.GetManifestResourceStream(resource))
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
                return null;
            }
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
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, DatabaseFileName)))
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
#endif

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
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, DatabaseFileName)))
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

        public static IEcuLogisticsEntry GetEcuLogisticsByGroupName(ReadOnlyCollection<IEcuLogisticsEntry> ecuLogisticsList, string name)
        {
            string nameLower = name.ToLowerInvariant();
            foreach (IEcuLogisticsEntry entry in ecuLogisticsList)
            {
                if (entry.GroupSgbd.ToLowerInvariant().Contains(nameLower))
                {
                    return entry;
                }
            }
            return null;
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

        public static ReadOnlyCollection<IEcuLogisticsEntry> GetEcuLogisticsFromVehicleType(string resourcePath, string vehicleType, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ECU logistics from vehicle type: {0}", vehicleType ?? "No type");
            if (vehicleType == null)
            {
                return null;
            }

            if (!CreateEcuLogistics(typeof(BmwDeepObd.XmlToolActivity).Namespace + ".VehicleInfo."))
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Create ECU logistics failed");
            }

            // Mapping could be found in:
            // from: RheingoldDiagnostics.dll: BMW.Rheingold.Diagnostics.VehicleLogistics.GetCharacteristics(Vehicle vecInfo)
            ReadOnlyCollection<IEcuLogisticsEntry> ecuLogisticsEntries = null;
            switch (vehicleType.ToUpperInvariant())
            {
                case "E36":
                    ecuLogisticsEntries = EcuLogisticsDataE36.Data;
                    break;

                case "E38":
                    ecuLogisticsEntries = EcuLogisticsDataE38.Data;
                    break;

                case "E39":
                    ecuLogisticsEntries = EcuLogisticsDataE39.Data;
                    break;

                case "E46":
                    ecuLogisticsEntries = EcuLogisticsDataE46.Data;
                    break;

                case "E52":
                    ecuLogisticsEntries = EcuLogisticsDataE52.Data;
                    break;

                case "E53":
                    ecuLogisticsEntries = EcuLogisticsDataE53.Data;
                    break;

                case "E83":
                    ecuLogisticsEntries = EcuLogisticsDataE83.Data;
                    break;

                case "E85":
                case "E86":
                    ecuLogisticsEntries = EcuLogisticsDataE85.Data;
                    break;

                case "R50":
                case "R52":
                case "R53":
                    ecuLogisticsEntries = EcuLogisticsDataR50.Data;
                    break;
            }

            if (ecuLogisticsEntries == null)
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle type unknown");
            }

            return ecuLogisticsEntries;
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
                    return new VehicleStructsBmw.VehicleSeriesInfo(series, "F01", string.Empty);
            }

            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No vehicle series info found");
            return null;
        }
    }
}
