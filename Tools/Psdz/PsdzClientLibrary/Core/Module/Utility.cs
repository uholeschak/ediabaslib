using System;
using System.Diagnostics;
using System.Globalization;

namespace BMW.Rheingold.MeasurementCommunication
{
    public static class Utility
    {
        public static readonly string[] magPrefix = new string[5]
        {
            string.Empty,
            "k",
            "M",
            "G",
            "T"
        };

        public static readonly string[] minPrefix = new string[5]
        {
            string.Empty,
            "m",
            "µ",
            "n",
            "p"
        };

        public static string FindCaller()
        {
            string[] array = FindCallingClassAndMethod(3);
            return "called by " + array[0] + "." + array[1];
        }

        public static string[] FindCallingClassAndMethod(int i)
        {
            StackFrame stackFrame = new StackFrame(i);
            Type declaringType = stackFrame.GetMethod().DeclaringType;
            return new string[2]
            {
                declaringType.Name.ToString(CultureInfo.InvariantCulture),
                stackFrame.GetMethod().Name
            };
        }
    }
}
