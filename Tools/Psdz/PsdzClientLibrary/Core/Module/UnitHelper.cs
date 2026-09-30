using System;
using System.Globalization;
using System.Text.RegularExpressions;
using BMW.Rheingold.Measurement.Model;

namespace BMW.Rheingold.MeasurementCommunication
{
    public static class UnitHelper
    {
        private static int[] exponent = new int[5] { 0, 3, 6, 9, 12 };

        private static string[] magPrefix = new string[5]
        {
        string.Empty,
        "k",
        "M",
        "G",
        "T"
        };

        private static string[] minPrefix = new string[5]
        {
        string.Empty,
        "m",
        "µ",
        "n",
        "p"
        };

        public static double Factor(string unit)
        {
            string value = null;
            if (!string.IsNullOrEmpty(unit) && unit.Length > 1)
            {
                value = unit.Substring(0, 1);
            }
            if (!string.IsNullOrEmpty(value))
            {
                for (int num = minPrefix.Length - 1; num >= 1; num--)
                {
                    if (minPrefix[num].Equals(value))
                    {
                        return Math.Pow(10.0, -1 * exponent[num]);
                    }
                }
                for (int num2 = magPrefix.Length - 1; num2 >= 1; num2--)
                {
                    if (magPrefix[num2].Equals(value))
                    {
                        return Math.Pow(10.0, exponent[num2]);
                    }
                }
            }
            return 1.0;
        }

        public static double FromUnitPrefix(string prefixedNumber)
        {
            double num = double.NaN;
            int num2 = 0;
            Match match = Regex.Match(prefixedNumber, "[-+,.0-9]+");
            if (match.Success)
            {
                num = Convert.ToDouble(match.Value, CultureInfo.InvariantCulture);
            }
            match = Regex.Match(prefixedNumber.Replace("Ohm", string.Empty), "[kMGTmµnp]+");
            if (match.Success)
            {
                string text = match.Value.Substring(0, 1);
                if (text.Length != 0)
                {
                    num2 = 0;
                    string[] array = minPrefix;
                    foreach (string value in array)
                    {
                        if (text.Equals(value))
                        {
                            num *= Math.Pow(10.0, num2 * 3);
                            break;
                        }
                        num2--;
                    }
                    num2 = 0;
                    array = magPrefix;
                    foreach (string value2 in array)
                    {
                        if (text.Equals(value2))
                        {
                            num *= Math.Pow(10.0, num2 * 3);
                            break;
                        }
                        num2++;
                    }
                }
            }
            return num;
        }

        public static string Prefix(double value)
        {
            string[] array = Prefix(value, "G4");
            return array[0] + " " + array[1];
        }

        public static string RangeToFormat(string range)
        {
            string result = "N2";
            if (!string.IsNullOrEmpty(range))
            {
                Match match = new Regex("[.0-9]+").Match(range);
                if (match.Success && decimal.TryParse(match.Value, out var result2))
                {
                    result = Mapping.RangeToFormat(result2);
                }
            }
            return result;
        }

        public static string[] ValueUnit(double value, string range)
        {
            string numberFormat = RangeToFormat(range);
            return Prefix(value, numberFormat);
        }

        private static string[] Prefix(double value, string numberFormat)
        {
            int num = 0;
            string[] array = new string[2];
            if (value != 0.0)
            {
                num = (int)Math.Floor(Math.Log10(Math.Abs(value)) / 3.0);
            }
            value *= Math.Pow(10.0, -num * 3);
            if (num < 0)
            {
                if (-num >= Utility.minPrefix.Length)
                {
                    array[0] = "UDR ";
                    array[1] = "-";
                }
                else
                {
                    array[0] = value.ToString(numberFormat, CultureInfo.GetCultureInfo("en-GB"));
                    array[1] = Utility.minPrefix[-num];
                }
            }
            else if (num >= Utility.magPrefix.Length)
            {
                array[0] = "OVR ";
                array[1] = "-";
            }
            else
            {
                array[0] = value.ToString(numberFormat, CultureInfo.GetCultureInfo("en-GB"));
                array[1] = Utility.magPrefix[num];
            }
            return array;
        }
    }
}
