using System;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlType("CombinedEcuHousingEntry")]
    public class CombinedEcuHousingEntry : ICombinedEcuHousingEntry
    {
        [XmlElement("Column", Order = 1)]
        public int Column { get; set; }

        [XmlElement("Row", Order = 2)]
        public int Row { get; set; }

        [XmlElement("EcuCount", Order = 3)]
        public int EcuCount { get; set; }

        [XmlElement("RequiredEcuAddresses", Order = 4)]
        public int[] RequiredEcuAddresses { get; set; }

        [XmlElement("RowSpan", Order = 5)]
        public int? RowSpan { get; set; }

        [XmlElement("ColumnSpan", Order = 6)]
        public int? ColumnSpan { get; set; }

        [XmlElement("ExtendedWidth", Order = 7)]
        public bool ExtendedWidth { get; set; }

        public CombinedEcuHousingEntry()
        {
        }

        public CombinedEcuHousingEntry(int column, int row, int ecuCount, int[] requiredEcuAddresses, int? rowSpan = null, int? columnSpan = null, bool extendedWidth = false)
        {
            Column = column;
            Row = row;
            EcuCount = ecuCount;
            RequiredEcuAddresses = requiredEcuAddresses ?? new int[0];
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
            ExtendedWidth = extendedWidth;
        }
    }
}
