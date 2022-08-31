using System;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClientLibrary
{
    public static class VehicleCharacteristicsUtility
    {
        public static bool HasMrr30(this IVehicle vec) => vec.HasEcu("MRR_30");

        public static bool HasFrr30v(this IVehicle vec) => vec.HasEcu("FRR_30V");

        public static bool HasHuMgu(this IVehicle vec) => vec.HasEcu("HU_MGU");

        public static bool HasEnavevo(this IVehicle vec) => vec.HasEcu("ENAVEVO");

        public static bool HasNbtevo(this IVehicle vec) => vec.HasEcu("NBTEVO");

        public static bool HasEnavevoOrNbtevo(this IVehicle vec) => vec.HasNbtevo() || vec.HasEnavevo();

        public static bool HasAmph70(this IVehicle vec) => vec.HasEcu("AMPH70");

        public static bool HasAmpt70(this IVehicle vec) => vec.HasEcu("AMPT70");

        public static bool IsBev(this IVehicle vec) => vec.HasHybridMark("BEVE");

        public static bool IsPhev(this IVehicle vec) => vec.HasHybridMark("PHEV");

        public static bool IsHybr(this IVehicle vec) => vec.HasHybridMark("HYBR");

        public static bool IsErex(this IVehicle vec) => vec.HasHybridMark("EREX");

        private static bool HasEcu(this IVehicle vec, string sgbd) => vec.ECU != null && vec.ECU.Any<IEcu>((Func<IEcu, bool>)(ecu =>
        {
            string ecuSgbd = ecu.ECU_SGBD;
            return ecuSgbd != null && ecuSgbd.Equals(sgbd, StringComparison.InvariantCultureIgnoreCase);
        }));

        private static bool HasHybridMark(this IVehicle vec, string mark)
        {
            string hybridkennzeichen = vec.Hybridkennzeichen;
            return hybridkennzeichen != null && hybridkennzeichen.Equals(mark, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
