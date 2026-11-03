using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UdsFileReader;

namespace BmwDeepObd
{
    public static class VagFileUtils
    {
        public static string GetVagDatUkdDir(string ecuPath, bool ignoreManufacturer = false)
        {
            string lang = LocaleUtils.GetCurrentLanguageStatic();
            string dirName = Path.Combine(ecuPath, "dat.ukd", lang);
            if (!Directory.Exists(dirName))
            {
                dirName = Path.Combine(ecuPath, "dat.ukd", ActivityCommon.DefaultLang);
            }

            if (!ignoreManufacturer)
            {
                string manufactName = string.Empty;
                switch (ActivityCommon.SelectedManufacturer)
                {
                    case ActivityCommon.ManufacturerType.Audi:
                        manufactName = "audi";
                        break;

                    case ActivityCommon.ManufacturerType.Seat:
                        manufactName = "seat";
                        break;

                    case ActivityCommon.ManufacturerType.Skoda:
                        manufactName = "skoda";
                        break;

                    case ActivityCommon.ManufacturerType.Vw:
                        manufactName = "vw";
                        break;
                }
                if (!string.IsNullOrEmpty(manufactName))
                {
                    dirName = Path.Combine(dirName, manufactName);
                }
            }
            return dirName;
        }

        public static List<ActivityCommon.VagEcuEntry> ReadVagEcuList(string ecuPath)
        {
            try
            {
                string datUkdPath = GetVagDatUkdDir(ecuPath);
                string xmlFile = Path.Combine(datUkdPath, "xml", "GWVL-ECU_config.xml");
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement ecuListNode = xmlDoc.Root?.Element("EcuList");
                if (ecuListNode == null)
                {
                    return null;
                }
                List<ActivityCommon.VagEcuEntry> ecuList = new List<ActivityCommon.VagEcuEntry>();
                foreach (XElement ecuNode in ecuListNode.Elements("Ecu"))
                {
                    XElement sysNode = ecuNode.Element("SysName");
                    if (sysNode == null)
                    {
                        continue;
                    }
                    if (sysNode.Attribute("Type") != null)
                    {
                        continue;
                    }
                    string sysName;
                    using (XmlReader reader = sysNode.CreateReader())
                    {
                        reader.MoveToContent();
                        sysName = reader.ReadInnerXml();
                    }
                    if (string.IsNullOrWhiteSpace(sysName))
                    {
                        continue;
                    }

                    XAttribute attrAddr = ecuNode.Attribute("AddrDez");
                    if (attrAddr == null)
                    {
                        continue;
                    }
                    int address;
                    try
                    {
                        address = XmlConvert.ToInt32(attrAddr.Value);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    ecuList.Add(new ActivityCommon.VagEcuEntry(sysName, address));
                }
                return ecuList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Convert russian mwtab xml files first with (linux):
        // mkdir ../mwru
        // find -name "*.xml" -exec sh -c "recode UTF8..ISO-8859-1 < '{}' | recode windows-1251..UTF8 > ../mwru/'{}'" \;
        public static List<ActivityCommon.MwTabEntry> ReadVagMwTab(string fileName, bool ignoreEmptyUnits = false)
        {
            try
            {
                Regex commentRegex = new Regex(@"(^I|\\(N[YZ ]|.))", RegexOptions.Multiline);
                XDocument xmlDoc = XDocument.Load(fileName);
                if (xmlDoc.Root == null)
                {
                    return null;
                }
                List<ActivityCommon.MwTabEntry> mwTabList = new List<ActivityCommon.MwTabEntry>();
                foreach (XElement measureNode in xmlDoc.Root.Elements("Messwert"))
                {
                    try
                    {
                        int blockNumber;
                        XElement agNode = measureNode.Element("AG");
                        if (agNode == null)
                        {
                            continue;
                        }
                        using (XmlReader reader = agNode.CreateReader())
                        {
                            reader.MoveToContent();
                            blockNumber = XmlConvert.ToInt32(reader.ReadInnerXml());
                        }

                        int? valueIndex = null;
                        XElement afNode = measureNode.Element("AF");
                        if (afNode != null)
                        {
                            using (XmlReader reader = afNode.CreateReader())
                            {
                                reader.MoveToContent();
                                try
                                {
                                    valueIndex = XmlConvert.ToInt32(reader.ReadInnerXml());
                                }
                                catch (Exception)
                                {
                                    valueIndex = null;
                                }
                            }
                        }

                        string description = string.Empty;
                        XElement nameNode = measureNode.Element("Name");
                        if (nameNode == null)
                        {
                            continue;
                        }
                        foreach (XElement textNode in nameNode.Elements("Text"))
                        {
                            XAttribute tiAttrib = textNode.Attribute("TI");
                            if (tiAttrib?.Value.Length > 0 && valueIndex.HasValue)
                            {
                                continue;
                            }
                            using (XmlReader reader = textNode.CreateReader())
                            {
                                reader.MoveToContent();
                                description = reader.ReadInnerXml();
                            }
                        }
                        if (string.IsNullOrEmpty(description))
                        {
                            continue;
                        }

                        XElement commentNode = measureNode.Element("Meldungstext");
                        string comment = string.Empty;
                        if (commentNode != null)
                        {
                            using (XmlReader reader = commentNode.CreateReader())
                            {
                                reader.MoveToContent();
                                comment = reader.ReadInnerXml();
                            }
                        }
                        comment = commentRegex.Replace(comment, string.Empty);

                        double? valueMin = null;
                        XElement minNode = measureNode.Element("analogerSWunten");
                        if (minNode != null)
                        {
                            using (XmlReader reader = minNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string value = reader.ReadInnerXml();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    try
                                    {
                                        valueMin = XmlConvert.ToDouble(value);
                                    }
                                    catch (Exception)
                                    {
                                        valueMin = null;
                                    }
                                }
                            }
                        }

                        double? valueMax = null;
                        XElement maxNode = measureNode.Element("analogerSWoben");
                        if (maxNode != null)
                        {
                            using (XmlReader reader = maxNode.CreateReader())
                            {
                                reader.MoveToContent();
                                string value = reader.ReadInnerXml();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    try
                                    {
                                        valueMax = XmlConvert.ToDouble(value);
                                    }
                                    catch (Exception)
                                    {
                                        valueMax = null;
                                    }
                                }
                            }
                        }

                        string valueUnit = string.Empty;
                        XElement sweNode = measureNode.Element("SWE");
                        if (sweNode != null)
                        {
                            using (XmlReader reader = sweNode.CreateReader())
                            {
                                reader.MoveToContent();
                                valueUnit = reader.ReadInnerXml();
                            }
                        }
                        if (ignoreEmptyUnits && string.IsNullOrWhiteSpace(valueUnit))
                        {
                            continue;
                        }
                        if (valueUnit.ToLowerInvariant().StartsWith("proz"))
                        {
                            valueUnit = "%";
                        }

                        string valueType = string.Empty;
                        XElement swfNode = measureNode.Element("SWF");
                        if (swfNode != null)
                        {
                            using (XmlReader reader = swfNode.CreateReader())
                            {
                                reader.MoveToContent();
                                valueType = reader.ReadInnerXml();
                            }
                        }

                        mwTabList.Add(new ActivityCommon.MwTabEntry(blockNumber, valueIndex, description, comment, valueUnit, valueType, valueMin, valueMax));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return mwTabList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetMatchingVagMwTabUdsFiles(string mwTabDir, long ecuAddr)
        {
            try
            {
                string fileMask = string.Format("UDS_{0:X02}_*.xml", ecuAddr);
                string[] files = Directory.GetFiles(mwTabDir, fileMask, SearchOption.TopDirectoryOnly);
                return files.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<string> GetMatchingVagMwTabFiles(string mwTabDir, string sgName)
        {
            try
            {
                List<string> fileList = new List<string>();
                string xmlEntry = string.Format("<SgVariante>{0}</SgVariante>", sgName);
                string[] files = Directory.GetFiles(mwTabDir, "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(file))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                string line = streamReader.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }
                                line = line.Trim();
                                if (string.Compare(line, xmlEntry, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    fileList.Add(file);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return fileList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<ActivityCommon.MwTabFileEntry> GetMatchingVagMwTabsUds(string mwTabDir, long ecuAddr)
        {
            List<ActivityCommon.MwTabFileEntry> mwTabFileList = new List<ActivityCommon.MwTabFileEntry>();
            List<string> fileList = GetMatchingVagMwTabUdsFiles(mwTabDir, ecuAddr);
            if (fileList == null)
            {
                return mwTabFileList;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string fileName in fileList)
            {
                List<ActivityCommon.MwTabEntry> mwTabList = ReadVagMwTab(fileName);
                if (mwTabList == null)
                {
                    continue;
                }
                List<ActivityCommon.MwTabEntry> mwTabListCleaned = mwTabList.Where(entry => !entry.ValueIndex.HasValue).ToList();
                mwTabFileList.Add(new ActivityCommon.MwTabFileEntry(fileName, mwTabListCleaned));
            }
            return mwTabFileList;
        }

        public static List<ActivityCommon.MwTabFileEntry> GetMatchingVagMwTabs(string mwTabDir, string sgName)
        {
            List<ActivityCommon.MwTabFileEntry> mwTabFileList = new List<ActivityCommon.MwTabFileEntry>();
            List<string> fileList = GetMatchingVagMwTabFiles(mwTabDir, sgName);
            if (fileList == null)
            {
                return mwTabFileList;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string fileName in fileList)
            {
                List<ActivityCommon.MwTabEntry> mwTabList = ReadVagMwTab(fileName, true);
                if (mwTabList == null)
                {
                    continue;
                }
                List<ActivityCommon.MwTabEntry> mwTabListCleaned = mwTabList.Where(entry => entry.ValueIndex.HasValue).ToList();
                mwTabFileList.Add(new ActivityCommon.MwTabFileEntry(fileName, mwTabListCleaned));
            }
            return mwTabFileList;
        }

        public static SortedSet<long> ExtractVagMwBlocks(List<ActivityCommon.MwTabFileEntry> mwTabFileList)
        {
            SortedSet<long> mwBlocks = new SortedSet<long>();

            foreach (ActivityCommon.MwTabFileEntry mwTabFileEntry in mwTabFileList)
            {
                foreach (ActivityCommon.MwTabEntry mwTabEntry in mwTabFileEntry.MwTabList)
                {
                    if (!mwBlocks.Contains(mwTabEntry.BlockNumber))
                    {
                        mwBlocks.Add(mwTabEntry.BlockNumber);
                    }
                }
            }

            return mwBlocks;
        }

        public static Dictionary<int, string> GetVagEcuNamesDict(string ecuPath)
        {
            try
            {
                string datUkdBasePath = GetVagDatUkdDir(ecuPath, true);
                string xmlFile = Path.Combine(datUkdBasePath, "xml", "ECU_Names.xml");
                XDocument xmlDoc = XDocument.Load(xmlFile);
                XElement ecuListNode = xmlDoc.Root?.Element("EcuList");
                if (ecuListNode == null)
                {
                    return null;
                }
                Dictionary<int, string> ecuNamesDict = new Dictionary<int, string>();
                foreach (XElement ecuNode in ecuListNode.Elements("Ecu"))
                {
                    XAttribute attrName = ecuNode.Attribute("Name");
                    string ecuName = attrName?.Value;
                    if (string.IsNullOrWhiteSpace(ecuName))
                    {
                        continue;
                    }
                    XAttribute attrAddr = ecuNode.Attribute("AddrHex");
                    if (attrAddr == null)
                    {
                        continue;
                    }
                    int address;
                    try
                    {
                        address = Convert.ToInt32(attrAddr.Value, 16);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    string existingName;
                    if (ecuNamesDict.TryGetValue(address, out existingName))
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (existingName.Length > ecuName.Length)
                        {   // use shortest name
                            ecuNamesDict[address] = ecuName;
                        }
                    }
                    else
                    {
                        ecuNamesDict.Add(address, ecuName);
                    }
                }
                return ecuNamesDict;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Android.Text.InputTypes ConvertVagUdsDataTypeToInputType(UInt32 dataTypeId)
        {
            Android.Text.InputTypes inputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagNoSuggestions;

            UdsReader.DataType dataType = (UdsReader.DataType)(dataTypeId & UdsReader.DataTypeMaskEnum);
            switch (dataType)
            {
                case UdsReader.DataType.FloatScaled:
                    inputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
                    break;

                case UdsReader.DataType.Integer1:
                case UdsReader.DataType.Integer2:
                    inputType = Android.Text.InputTypes.ClassNumber;
                    break;
            }

            if ((inputType & Android.Text.InputTypes.MaskClass) == Android.Text.InputTypes.ClassNumber)
            {
                if ((dataTypeId & UdsReader.DataTypeMaskSigned) != 0x00)
                {
                    inputType |= Android.Text.InputTypes.NumberFlagSigned;
                }
            }
            return inputType;
        }
    }
}
