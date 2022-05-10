using System;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType("SGBDBusLogisticsEntry")]
    public class SGBDBusLogisticsEntry : ISGBDBusLogisticsEntry
    {
        [XmlElement("Bus", Order = 1)]
        public BusType Bus { get; set; }

        [XmlArray("SubBusList", Order = 3)]
        [XmlArrayItem("BusEntry")]
        public BusType[] SubBusList { get; set; }

        [XmlElement("Variant", Order = 2)]
        public string Variant { get; set; }

        public SGBDBusLogisticsEntry()
        {
        }

        internal SGBDBusLogisticsEntry(string variant, BusType bus, BusType[] subBus)
        {
            Bus = bus;
            Variant = variant;
            SubBusList = subBus;
        }
    }
}
