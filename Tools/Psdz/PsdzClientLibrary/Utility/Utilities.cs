using PsdzClient.Core;
using System;
using System.Linq;

namespace PsdzClient.Utility
{
    public static class Utilities
    {
        public static bool HasHuMgu(this IEcuTreeVehicle vec)
        {
            return HasEcu(vec, "HU_MGU");
        }

        public static bool HasNbtevo(this IEcuTreeVehicle vec)
        {
            return HasEcu(vec, "NBTEVO");
        }

        public static bool HasAmpt70(this IEcuTreeVehicle vec)
        {
            return HasEcu(vec, "AMPT70");
        }

        public static bool HasAmph70(this IEcuTreeVehicle vec)
        {
            return HasEcu(vec, "AMPH70");
        }

        private static bool HasEcu(this IEcuTreeVehicle vec, string sgbd)
        {
            if (vec.ECU == null)
            {
                return false;
            }
            return vec.ECU.Any((IEcuTreeEcu ecu) => ecu.ECU_SGBD?.Equals(sgbd, StringComparison.InvariantCultureIgnoreCase) ?? false);
        }

        public static bool IsBev(this IEcuTreeVehicle vec)
        {
            return HasHybridMark(vec, "BEVE");
        }

        private static bool HasHybridMark(this IEcuTreeVehicle vec, string mark)
        {
            return vec.Hybridkennzeichen?.Equals(mark, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }
    }
}