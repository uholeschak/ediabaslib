using System;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClientLibrary
{
    public static class VehicleCharacteristicsUtility
    {
        public static bool HasMrr30(this IVehicle vec)
        {
            return vec.HasEcu("MRR_30");
        }

        public static bool HasFrr30v(this IVehicle vec)
        {
            return vec.HasEcu("FRR_30V");
        }

        public static bool HasHuMgu(this IVehicle vec)
        {
            return vec.HasEcu("HU_MGU");
        }

        public static bool HasEnavevo(this IVehicle vec)
        {
            return vec.HasEcu("ENAVEVO");
        }

        public static bool HasEnavevoOrNbtevo(this IVehicle vec)
        {
            return vec.HasEcu("NBTEVO") || vec.HasEnavevo();
        }

        public static bool IsBev(this IVehicle vec)
        {
            return vec.HasHybridMark("BEVE");
        }

        public static bool IsPhev(this IVehicle vec)
        {
            return vec.HasHybridMark("PHEV");
        }

        private static bool HasEcu(this IVehicle vec, string sgbd)
        {
            return vec.ECU != null && vec.ECU.Any(delegate (IEcu ecu)
            {
                string ecu_SGBD = ecu.ECU_SGBD;
                return ecu_SGBD != null && ecu_SGBD.Equals(sgbd, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private static bool HasHybridMark(this IVehicle vec, string mark)
        {
            string hybridkennzeichen = vec.Hybridkennzeichen;
            return hybridkennzeichen != null && hybridkennzeichen.Equals(mark, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}