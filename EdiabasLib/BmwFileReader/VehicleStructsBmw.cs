using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace BmwFileReader
{
    public static class VehicleStructsBmw
    {
        public const string VehicleSeriesXmlFile = "VehicleSeries.xml";
        public const string FaultRulesZipFile = "FaultRules.zip";
        public const string FaultRulesXmlFile = "FaultRules.xml";

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

        [XmlInclude(typeof(VehicleSeriesInfo))]
        [XmlType("VehicleSeriesInfoDataXml")]
        public class VehicleSeriesInfoData
        {
            public VehicleSeriesInfoData() : this(null, null)
            {
            }

            public VehicleSeriesInfoData(string timeStamp, SerializableDictionary<string, List<VehicleSeriesInfo>> vehicleSeriesDict)
            {
                TimeStamp = timeStamp;
                VehicleSeriesDict = vehicleSeriesDict;
            }

            [XmlElement("TimeStamp"), DefaultValue(null)] public string TimeStamp { get; set; }
            [XmlElement("VehicleSeriesDict"), DefaultValue(null)] public SerializableDictionary<string, List<VehicleSeriesInfo>> VehicleSeriesDict { get; set; }
        }

        [XmlType("FRI")]
        public class FaultRuleInfo
        {
            public FaultRuleInfo()
            {
            }

            public FaultRuleInfo(string id, string ruleFormula)
            {
                Id = id;
                RuleFormula = ruleFormula;
            }

            [XmlElement("Id"), DefaultValue(null)] public string Id { get; set; }
            [XmlElement("RF"), DefaultValue(null)] public string RuleFormula { get; set; }
        }

        [XmlInclude(typeof(FaultRuleInfo))]
        [XmlType("FaultRulesInfoData")]
        public class FaultRulesInfoData
        {
            public FaultRulesInfoData() : this(null)
            {
            }

            public FaultRulesInfoData(SerializableDictionary<string, FaultRuleInfo> faultRuleDict)
            {
                FaultRuleDict = faultRuleDict;
            }

            [XmlElement("FaultRuleDict"), DefaultValue(null)] public SerializableDictionary<string, FaultRuleInfo> FaultRuleDict { get; set; }
        }
    }
}
