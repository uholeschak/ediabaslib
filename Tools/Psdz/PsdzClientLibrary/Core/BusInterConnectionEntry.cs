using System;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType("BusInterConnectionEntry")]
    public class BusInterConnectionEntry : IBusInterConnectionEntry
    {
        [XmlIgnore]
        public BusType Bus { get; private set; }

        [XmlAttribute("Bus")]
        public string XmlBus
        {
            get
            {
                return Bus.ToString();
            }
            set
            {
                if (value == null)
                {
                    Bus = BusType.UNKNOWN;
                }
                else
                {
                    Bus = (Enum.TryParse<BusType>(value, ignoreCase: true, out var result) ? result : BusType.UNKNOWN);
                }
            }
        }

        [XmlElement("XStart", Order = 1)]
        public double XStart { get; set; }

        [XmlElement("YStart", Order = 2)]
        public double YStart { get; set; }

        [XmlElement("XEnd", Order = 3)]
        public double XEnd { get; set; }

        [XmlElement("YEnd", Order = 4)]
        public double YEnd { get; set; }

        [XmlElement("RequiredEcuAddresses", Order = 5)]
        public int[] RequiredEcuAddresses { get; set; }

        public BusInterConnectionEntry()
        {
        }

        internal BusInterConnectionEntry(BusType bus, double xStart, double yStart, double xEnd, double yEnd, int[] requiredEcuAddresses)
        {
            Bus = bus;
            XStart = xStart;
            YStart = yStart;
            XEnd = xEnd;
            YEnd = yEnd;
            RequiredEcuAddresses = requiredEcuAddresses;
        }
    }
}
