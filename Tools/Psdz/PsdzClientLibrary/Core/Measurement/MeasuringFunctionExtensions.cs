using BMW.Rheingold.Measurement.Common;
using PsdzClient.Core;

namespace BMW.Rheingold.Measurement.Common
{
    public static class MeasuringFunctionExtensions
    {
        public static string Abbreviation(this MeasuringFunction function)
        {
            switch (function)
            {
                case MeasuringFunction.Current:
                    return "I";
                case MeasuringFunction.Voltage:
                case MeasuringFunction.Diode:
                    return "U";
                case MeasuringFunction.Resistance:
                    return "R";
                case MeasuringFunction.Pressure:
                    return "P";
                case MeasuringFunction.Temperature:
                    return "T";
                default:
                    Log.Error("MeasuringFunctionExtensions.Abbreviation()", "Function {0} has no abbreviation. Using \"?\" instead.", function);
                    return "?";
            }
        }

        public static string Unit(this MeasuringFunction function)
        {
            switch (function)
            {
                case MeasuringFunction.Current:
                    return "A";
                case MeasuringFunction.Voltage:
                case MeasuringFunction.Diode:
                    return "V";
                case MeasuringFunction.Resistance:
                    return new string(new char[1] { 'Ω' });
                case MeasuringFunction.Pressure:
                    return "bar";
                case MeasuringFunction.Temperature:
                    return new string(new char[2] { '°', 'C' });
                default:
                    Log.Error("MeasuringFunctionExtensions.Unit()", "Function {0} has no unit. Using \"?\" instead.", function);
                    return "?";
            }
        }
    }
}
