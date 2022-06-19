using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using EdiabasLib;

namespace BmwFileReader
{
    public static class VehicleStructsBmw
    {
        public const string VehicleSeriesXmlFile = "VehicleSeries.xml";

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

            public VehicleSeriesInfo(string series, string brSgbd, string bnType, string brand, List<VehicleEcuInfo> ecuList = null, string date = null, string dateCompare = null)
            {
                Series = series;
                BrSgbd = brSgbd;
                BnType = bnType;
                Brand = brand;
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
            [XmlElement("Brand"), DefaultValue(null)] public string Brand { get; set; }
            [XmlElement("EcuList"), DefaultValue(null)] public List<VehicleEcuInfo> EcuList { get; set; }
            [XmlElement("Date"), DefaultValue(null)] public string Date { get; set; }
            [XmlElement("DateCompare"), DefaultValue(null)] public string DateCompare { get; set; }
        }

        [XmlInclude(typeof(VehicleSeriesInfo))]
        [XmlType("VehicleSeriesInfoDataXml")]
        public class VehicleSeriesInfoData
        {
            public VehicleSeriesInfoData() : this(null)
            {
            }

            public VehicleSeriesInfoData(SerializableDictionary<string, List<VehicleSeriesInfo>> vehicleSeriesDict)
            {
                VehicleSeriesDict = vehicleSeriesDict;
            }

            [XmlElement("VehicleSeriesDict"), DefaultValue(null)] public SerializableDictionary<string, List<VehicleSeriesInfo>> VehicleSeriesDict { get; set; }
        }
    }
}
