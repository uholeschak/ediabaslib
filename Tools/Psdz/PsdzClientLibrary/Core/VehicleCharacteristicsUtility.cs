using System;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    // ToDo: Check on update
    public static class VehicleCharacteristicsUtility
    {
        public static bool HasMrr30(this IVehicle vec)
        {
            return vec.HasEcu("MRR_30");
        }

        public static bool HasHuMgu(this IVehicle vec)
        {
            return vec.HasEcu("HU_MGU");
        }

        public static bool HasEnavevo(this IVehicle vec)
        {
            return vec.HasEcu("ENAVEVO");
        }

        public static bool HasNbtevo(this IVehicle vec)
        {
            return vec.HasEcu("NBTEVO");
        }

        public static bool HasAmpt70(this IVehicle vec)
        {
            return vec.HasEcu("AMPT70");
        }

        public static bool HasAmph70(this IVehicle vec)
        {
            return vec.HasEcu("AMPH70");
        }

        public static bool IsBev(this IVehicle vec)
        {
            return vec.HasHybridMark("BEVE");
        }

        public static bool IsPhev(this IVehicle vec)
        {
            return vec.HasHybridMark("PHEV");
        }

        public static bool IsHybr(this IVehicle vec)
        {
            return vec.HasHybridMark("HYBR");
        }

        public static bool IsNohy(this IVehicle vec)
        {
            return vec.HasHybridMark("NOHY");
        }

        public static bool IsErex(this IVehicle vec)
        {
            return vec.HasHybridMark("EREX");
        }

        private static bool HasEcu(this IVehicle vec, string sgbd)
        {
            if (vec.ECU == null)
            {
                return false;
            }
            return vec.ECU.Any((ecu) => ecu.ECU_SGBD?.Equals(sgbd, StringComparison.InvariantCultureIgnoreCase) ?? false);
        }

        private static bool HasHybridMark(this IVehicle vec, string mark)
        {
            return vec.Hybridkennzeichen?.Equals(mark, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }
    }
}
