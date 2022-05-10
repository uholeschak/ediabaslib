using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType("XGBMBusLogisticsEntry")]
    public class XGBMBusLogisticsEntry : IXGBMBusLogisticsEntry
    {
        [XmlArray("SubBusList", Order = 1)]
        [XmlArrayItem("BusEntry")]
        public string[] XmlSubBUS
        {
            get
            {
                if (Bus == null)
                {
                    return null;
                }
                return new List<BusType>(Bus).ConvertAll((BusType x) => x.ToString()).ToArray();
            }
            set
            {
                List<BusType> list = new List<BusType>();
                if (value != null)
                {
                    foreach (string value2 in value)
                    {
                        list.Add(Enum.TryParse<BusType>(value2, ignoreCase: true, out var result) ? result : BusType.UNKNOWN);
                    }
                }
                Bus = list.ToArray();
            }
        }

        [XmlIgnore]
        public BusType[] Bus { get; private set; }

        [XmlAttribute("Prefix")]
        public string XgbmPrefix { get; set; }

        public XGBMBusLogisticsEntry()
        {
        }

        internal XGBMBusLogisticsEntry(string xgbmPrefix, BusType[] bus)
        {
            Bus = bus;
            XgbmPrefix = xgbmPrefix;
        }
    }
}
