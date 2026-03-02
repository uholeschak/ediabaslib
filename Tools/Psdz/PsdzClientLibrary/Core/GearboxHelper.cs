using System;

namespace PsdzClient.Core
{
    public static class GearboxHelper
    {
        public const string Automatic = "AUT";

        public static bool HasVehicleGearboxECU(string motor, string getriebe, Func<string, bool> hasSa)
        {
            if ("W10".Equals(motor, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if ("AUT".Equals(getriebe, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (hasSa("205") || hasSa("206") || hasSa("2TB") || hasSa("2TC") || hasSa("2MK"))
            {
                return true;
            }
            return false;
        }
    }
}