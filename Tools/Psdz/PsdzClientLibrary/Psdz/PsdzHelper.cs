using System;
using System.Globalization;

namespace BMW.Rheingold.Psdz
{
    internal static class PsdzHelper
    {
        public static void CheckString(string paramName, string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Param '{0}' must not be null or empty!", paramName));
            }
        }
    }
}