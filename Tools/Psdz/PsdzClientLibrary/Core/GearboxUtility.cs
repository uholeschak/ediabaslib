using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    // ToDo: Check on update
    public static class GearboxUtility
    {
        private static DateTime legacyDetectionConditionDate = new DateTime(2020, 7, 1);

        private static Predicate<IVehicle> useLegacyGearboxTypeDetection = delegate (IVehicle v)
        {
            try
            {
                return new DateTime(int.Parse(v.Modelljahr), int.Parse(v.Modellmonat), 1) < legacyDetectionConditionDate;
            }
            catch (Exception ex)
            {
                Log.Error("GearboxUtility.useLegacyGearboxTypeDetection", "Error occured when checking the condition, possible problem with vehicle construction date - year: " + v.Modelljahr + ", month: " + v.Modellmonat, ex);
                return false;
            }
        };

        public const string Manual = "MECH";

        public const string Automatic = "AUT";

        public const string NoGearbox = "-";

        public const string UnknownGearbox = "X";

        public static void SetGearboxType(IVehicle vehicle, string gearboxType, [CallerMemberName] string caller = null)
        {
            if (useLegacyGearboxTypeDetection(vehicle))
            {
                Log.Info("GearboxUtility.SetGearboxType()", "Gearbox type set to " + gearboxType + ". Called from: " + caller);
                vehicle.Getriebe = gearboxType;
            }
        }

        public static void SetAutomaticGearboxByEgsEcu(Vehicle vehicle)
        {
            if (useLegacyGearboxTypeDetection(vehicle) && vehicle.getECUbyECU_GRUPPE("G_EGS") != null && string.CompareOrdinal(vehicle.Getriebe, "AUT") != 0)
            {
                Log.Info("GearboxUtility.SetAutomaticGearboxByEgsEcu()", "found EGS ecu in vehicle with recoginzed manual gearbox; will be overwritten");
                vehicle.Getriebe = "AUT";
            }
        }

        public static void PerformGearboxAssignments(Vehicle vehicle)
        {
            if (!useLegacyGearboxTypeDetection(vehicle))
            {
                return;
            }
            if (!string.IsNullOrEmpty(vehicle.Ereihe) && "E65 E66 E67 E68".Contains(vehicle.Ereihe))
            {
                vehicle.Getriebe = "AUT";
            }
            else if (HasVehicleGearboxECU(vehicle) || vehicle.getECU(24L) != null)
            {
                ECU eCU = vehicle.getECU(24L);
                if ((eCU == null && (vehicle.VehicleIdentLevel == IdentificationLevel.VINVehicleReadout || vehicle.VehicleIdentLevel == IdentificationLevel.VINVehicleReadoutOnlineUpdated)) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "DKG", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "GSGE", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "SMG", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "SSG", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "GSD", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || vehicle.hasSA("2MK") || vehicle.hasSA("206") || vehicle.hasSA("2TC"))
                {
                    if ("AUT".Equals(vehicle.Getriebe, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Info("VehicleIdent.doVehicleIdent()", "found DKG ECU in vehicle with recognized automatic gearbox; will be overwritten");
                    }
                    vehicle.Getriebe = "MECH";
                }
                else
                {
                    if ("MECH".Equals(vehicle.Getriebe, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Info("VehicleIdent.doVehicleIdent()", "found EGS ECU in vehicle with recognized manual gearbox; will be overwritten");
                    }
                    vehicle.Getriebe = "AUT";
                }
            }
            else
            {
                if (vehicle.FA != null && vehicle.FA.SA != null && vehicle.FA.SA.Count > 0 && !vehicle.FA.SA.Contains("205"))
                {
                    Log.Info("VehicleIdent.doVehicleIdent()", "gearbox set to manual, because vehicle does not contain 205 sa");
                    vehicle.Getriebe = "MECH";
                }
                if (vehicle.ECU != null && vehicle.ECU.Count > 0 && vehicle.BNType != BNType.IBUS)
                {
                    Log.Info("VehicleIdent.doVehicleIdent()", "gearbox set to manual, because vehicle has no D_EGS or G_EGS");
                    vehicle.Getriebe = "MECH";
                }
            }
        }

        public static bool HasVehicleGearboxECU(Vehicle vehicle)
        {
            if ("W10".Equals(vehicle.Motor, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if ("AUT".Equals(vehicle.Getriebe, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (vehicle.hasSA("205") || vehicle.hasSA("206") || vehicle.hasSA("2TB") || vehicle.hasSA("2TC") || vehicle.hasSA("2MK"))
            {
                return true;
            }
            return false;
        }
    }
}
