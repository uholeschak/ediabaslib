using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType("EcuLogisticsEntry")]
    public class EcuLogisticsEntry : IEcuLogisticsEntry
    {
        [XmlElement("Bus", Order = 2)]
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

        [XmlArray("SubBusList", Order = 7)]
        [XmlArrayItem("BusEntry")]
        public string[] XmlSubBUS
        {
            get
            {
                if (SubBusList == null)
                {
                    return null;
                }

                return new List<BusType>(SubBusList).ConvertAll((BusType x) => x.ToString()).ToArray();
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

                SubBusList = list.ToArray();
            }
        }

        [XmlAttribute("DiagAddress")]
        public int DiagAddress { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("GroupSgbd", Order = 1)]
        public string GroupSgbd { get; set; }

        [XmlIgnore]
        public BusType Bus { get; private set; }

        [XmlElement("Column", Order = 3)]
        public int Column { get; set; }

        [XmlElement("Row", Order = 4)]
        public int Row { get; set; }

        [XmlElement("ShortName", Order = 5)]
        public string ShortName { get; set; }

        [XmlElement("SubDiagAddress", IsNullable = true, Order = 6)]
        public long? SubDiagAddress { get; set; }

        [XmlIgnore]
        public BusType[] SubBusList { get; private set; }

        public EcuLogisticsEntry()
        {
        }

        public EcuLogisticsEntry(int diagAddress, string name, BusType bus, string groupSgbd, int column, int row) : this(diagAddress, null, name, bus, null, groupSgbd, column, row, null)
        {
        }

        public EcuLogisticsEntry(int diagAddress, string name, BusType bus, BusType[] subBusList, string groupSgbd, int column, int row) : this(diagAddress, null, name, bus, subBusList, groupSgbd, column, row, null)
        {
        }

        public EcuLogisticsEntry(int diagAddress, int subDiagAddress, string name, BusType bus, string groupSgbd, int column, int row) : this(diagAddress, subDiagAddress, name, bus, null, groupSgbd, column, row, null)
        {
        }

        public EcuLogisticsEntry(int diagAddress, string name, BusType bus, string groupSgbd, int column, int row, string shortName) : this(diagAddress, null, name, bus, null, groupSgbd, column, row, shortName)
        {
        }

        public EcuLogisticsEntry(int diagAddress, long? subDiagAddress, string name, BusType bus, BusType[] subBusList, string groupSgbd, int column, int row, string shortName)
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
}