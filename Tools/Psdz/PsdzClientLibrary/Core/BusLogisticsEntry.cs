using System;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	[Serializable]
	[XmlType("BusLogisticsEntry")]
	public class BusLogisticsEntry : IBusLogisticsEntry
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
		public BusType Bus { get; private set; }

		[XmlElement("Column", Order = 1)]
		public int Column { get; set; }

		[XmlElement("MinRow", Order = 2)]
		public int MinRow { get; set; }

		[XmlElement("MaxRowLimit", Order = 3)]
		public int MaxRowLimit { get; set; }

		[XmlElement("XRelativeOffset", Order = 4)]
		public double XRelativeOffset { get; set; }

		[XmlElement("YRelativeOffset", Order = 5)]
		public double YRelativeOffset { get; set; }

		[XmlElement("PaintToRoot", Order = 6)]
		public bool PaintToRoot { get; set; }

		[XmlElement("ConnectOnlyToRightEcu", Order = 7)]
		public bool ConnectOnlyToRightEcu { get; set; }

		[XmlElement("DrawVerticalLines", Order = 8)]
		public bool DrawVerticalLines { get; set; }

		[XmlElement("DrawHorizontalLines", Order = 9)]
		public bool DrawHorizontalLines { get; set; }

		[XmlElement("BikeBus", Order = 10)]
		public bool BikeBus { get; set; }

		[XmlElement("RequiredEcuAddresses", Order = 11)]
		public int[] RequiredEcuAddresses { get; set; }

		public BusLogisticsEntry()
		{
		}

		internal BusLogisticsEntry(BusType bus, int column, int minRow, int maxRowLimit, double xRelativeOffset, double yRelativeOffset, bool paintToRoot, bool connectOnlyToRightEcu, bool drawVerticalLines, int[] requiredEcuAddresses)
			: this(bus, column, minRow, maxRowLimit, xRelativeOffset, paintToRoot, connectOnlyToRightEcu, drawVerticalLines, requiredEcuAddresses)
		{
			YRelativeOffset = yRelativeOffset;
		}

		internal BusLogisticsEntry(BusType bus, int column, int minRow, int maxRowLimit, double xRelativeOffset, bool paintToRoot, bool connectOnlyToRightEcu, bool drawVerticalLines, bool drawHorizontalLines, int[] requiredEcuAddresses)
			: this(bus, column, minRow, maxRowLimit, xRelativeOffset, paintToRoot, connectOnlyToRightEcu, drawVerticalLines, requiredEcuAddresses)
		{
			DrawHorizontalLines = drawHorizontalLines;
		}

		internal BusLogisticsEntry(BusType bus, int column, int minRow, int maxRowLimit, double xRelativeOffset, bool paintToRoot, bool connectOnlyToRightEcu, bool drawVerticalLines, int[] requiredEcuAddresses)
		{
			Bus = bus;
			Column = column;
			MinRow = minRow;
			MaxRowLimit = maxRowLimit;
			XRelativeOffset = xRelativeOffset;
			PaintToRoot = paintToRoot;
			ConnectOnlyToRightEcu = connectOnlyToRightEcu;
			DrawVerticalLines = drawVerticalLines;
			RequiredEcuAddresses = requiredEcuAddresses;
			BikeBus = false;
			DrawHorizontalLines = true;
			if (requiredEcuAddresses == null)
			{
				requiredEcuAddresses = new int[0];
			}
		}

		internal BusLogisticsEntry(BusType bus)
		{
			Bus = bus;
			BikeBus = true;
		}
	}
}
