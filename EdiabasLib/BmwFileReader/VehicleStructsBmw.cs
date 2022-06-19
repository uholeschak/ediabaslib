using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using EdiabasLib;

namespace BmwFileReader
{
    public static class VehicleStructsBmw
    {
        public const string VehicleSeriesXmlFile = "VehicleSeries.xml";

        [XmlType("VehicleSeriesInfo")]
        public class VehicleSeriesInfo
        {
            public VehicleSeriesInfo()
            {
            }

            public VehicleSeriesInfo(string series, string brSgbd, string bnType, string brand, string date = null, string dateCompare = null)
            {
                Series = series;
                BrSgbd = brSgbd;
                BnType = bnType;
                Brand = brand;
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
