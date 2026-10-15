using System;

namespace BMW.Rheingold.Module.ISTA
{
    internal class BarProtocolData
    {
        public int Index { get; set; }

        public double BarValue { get; set; }

        public DateTime TimeOfRegistration { get; set; }

        public BarProtocolData(int index, double barValue)
        {
            Index = index;
            BarValue = barValue;
            TimeOfRegistration = DateTime.Now;
        }
    }
}
