using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace BmwFileReader
{
    public static class VehicleStructsBmw
    {
        public const string VehicleSeriesXmlFile = "VehicleSeries.xml";
        public const string RulesZipFile = "RulesInfo.zip";
        public const string RulesXmlFile = "RulesInfo.xml";
        public const string RulesCsFile = "RulesInfo.cs";

        [XmlType("VEI")]
        public class VehicleEcuInfo
        {
            public VehicleEcuInfo()
            {
            }

            public VehicleEcuInfo(int diagAddr, string name, string groupSgbd)
            {
                DiagAddr = diagAddr;
                Name = name;
                GroupSgbd = groupSgbd;
            }

            [XmlElement("DiagAddr")] public int DiagAddr { get; set; }
            [XmlElement("Name"), DefaultValue(null)] public string Name { get; set; }
            [XmlElement("GroupSgbd"), DefaultValue(null)] public string GroupSgbd { get; set; }
        }

        [XmlInclude(typeof(VehicleEcuInfo))]
        [XmlType("VSI")]
        public class VehicleSeriesInfo
        {
            public VehicleSeriesInfo()
            {
            }

            public VehicleSeriesInfo(string series, string brSgbd, string bnType, List<string> brandList = null, List<VehicleEcuInfo> ecuList = null, string date = null, string dateCompare = null)
            {
                Series = series;
                BrSgbd = brSgbd;
                BnType = bnType;
                BrandList = brandList;
                EcuList = ecuList;
                Date = date;
                DateCompare = dateCompare;
            }

            public void ResetDate()
            {
                Date = null;
                DateCompare = null;
            }

            [XmlElement("Series"), DefaultValue(null)] public string Series { get; set; }
            [XmlElement("BrSgbd"), DefaultValue(null)] public string BrSgbd { get; set; }
            [XmlElement("BnType"), DefaultValue(null)] public string BnType { get; set; }
            [XmlElement("BrandList"), DefaultValue(null)] public List<string> BrandList { get; set; }
            [XmlElement("EcuList"), DefaultValue(null)] public List<VehicleEcuInfo> EcuList { get; set; }
            [XmlElement("Date"), DefaultValue(null)] public string Date { get; set; }
            [XmlElement("DateCompare"), DefaultValue(null)] public string DateCompare { get; set; }
        }

        [XmlType("VersionInfo")]
        public class VersionInfo
        {
            public VersionInfo() : this(null, null)
            {
            }

            public VersionInfo(string version, DateTime? dateTime)
            {
                Version = version;
                if (dateTime != null)
                {
                    Date = dateTime.Value;
                }
            }

            public bool IsIdentical(string version, DateTime? dateTime)
            {
                if (Version == null || version == null ||
                    string.Compare(Version, version, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                if (dateTime == null || Date != dateTime)
                {
                    return false;
                }

                return true;
            }

            public bool IsMinDate(DateTime? dateTime)
            {
                if (dateTime == null || Date < dateTime)
                {
                    return false;
                }

                return true;
            }

            [XmlElement("Version"), DefaultValue(null)] public string Version { get; set; }
            [XmlElement("Date"), DefaultValue(null)] public DateTime Date { get; set; }
        }

        [XmlInclude(typeof(VersionInfo))]
        [XmlInclude(typeof(VehicleSeriesInfo))]
        [XmlType("VehicleSeriesInfoDataXml")]
        public class VehicleSeriesInfoData
        {
            public VehicleSeriesInfoData() : this(null, null, null)
            {
            }

            public VehicleSeriesInfoData(string timeStamp, VersionInfo versionInfo, SerializableDictionary<string, List<VehicleSeriesInfo>> vehicleSeriesDict)
            {
                TimeStamp = timeStamp;
                Version = versionInfo;
                VehicleSeriesDict = vehicleSeriesDict;
            }

            [XmlElement("TimeStamp"), DefaultValue(null)] public string TimeStamp { get; set; }
            [XmlElement("Version"), DefaultValue(null)] public VersionInfo Version { get; set; }
            [XmlElement("VehicleSeriesDict"), DefaultValue(null)] public SerializableDictionary<string, List<VehicleSeriesInfo>> VehicleSeriesDict { get; set; }
        }

        [XmlType("RI")]
        public class RuleInfo
        {
            public RuleInfo()
            {
            }

            public RuleInfo(string id, string ruleFormula)
            {
                Id = id;
                RuleFormula = ruleFormula;
            }

            [XmlElement("Id"), DefaultValue(null)] public string Id { get; set; }
            [XmlElement("RF"), DefaultValue(null)] public string RuleFormula { get; set; }
        }

        [XmlInclude(typeof(RuleInfo))]
        [XmlType("RulesInfoData")]
        public class RulesInfoData
        {
            public RulesInfoData() : this(null, null)
            {
            }

            public RulesInfoData(SerializableDictionary<string, RuleInfo> faultRuleDict, SerializableDictionary<string, RuleInfo> ecuFuncRuleDict)
            {
                FaultRuleDict = faultRuleDict;
                EcuFuncRuleDict = ecuFuncRuleDict;
            }

            [XmlElement("FaultRuleDict"), DefaultValue(null)] public SerializableDictionary<string, RuleInfo> FaultRuleDict { get; set; }
            [XmlElement("EcuFuncRuleDict"), DefaultValue(null)] public SerializableDictionary<string, RuleInfo> EcuFuncRuleDict { get; set; }
        }
    }
}
