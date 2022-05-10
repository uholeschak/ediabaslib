using System;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType("BusNameEntry")]
    public class BusNameEntry : IBusNameEntry
    {
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

        [XmlIgnore]
        public BusType Bus { get; set; }

        [XmlText]
        public string Name { get; set; }

        public BusNameEntry()
        {
            Bus = BusType.UNKNOWN;
            Name = string.Empty;
        }

        internal BusNameEntry(BusType bus, string name)
        {
            Bus = bus;
            Name = name;
        }
    }
}
