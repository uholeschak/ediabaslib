using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public static class GearboxUtility
    {
        private static DateTime legacyDetectionConditionDate = new DateTime(2020, 7, 1);
        private static Predicate<IReactorVehicle> useLegacyGearboxTypeDetection = delegate (IReactorVehicle v)
        {
            try
            {
                return new DateTime(int.Parse(v.Modelljahr), int.Parse(v.Modellmonat), 1) < legacyDetectionConditionDate;
            }
            catch (Exception)
            {
                return false;
            }
        };
        public const string Manual = "MECH";
        public const string Automatic = "AUT";
        public const string NoGearbox = "-";
        public const string UnknownGearbox = "X";
        public static void SetGearboxType(IReactorVehicle vehicle, string gearboxType, ILogger Log, Action<string, string, LayoutGroup> protocolServiceCode, [CallerMemberName] string caller = null)
        {
            if (useLegacyGearboxTypeDetection(vehicle))
            {
                Log.Info("GearboxUtility.SetGearboxType()", "Gearbox type set to " + gearboxType + ". Called from: " + caller);
                ProtocolGerboxServiceCode(vehicle, gearboxType, protocolServiceCode, "SetGearboxType");
                vehicle.Getriebe = gearboxType;
            }
        }

        public static void SetAutomaticGearboxByEgsEcu(IIdentVehicle vehicle, ILogger Log, ReactorEngine reactor, Action<string, string, LayoutGroup> protocolServiceCode)
        {
            if (useLegacyGearboxTypeDetection(vehicle))
            {
                if (vehicle.getECUbyECU_GRUPPE("G_EGS") != null && string.CompareOrdinal(((IReactorVehicle)vehicle).Getriebe, "AUT") != 0)
                {
                    Log.Info("GearboxUtility.SetAutomaticGearboxByEgsEcu()", "found EGS ecu in vehicle with recoginzed manual gearbox; will be overwritten");
                    ProtocolGerboxServiceCode(vehicle, "AUT", protocolServiceCode, "SetAutomaticGearboxByEgsEcu");
                    reactor.SetGetriebe("AUT", DataSource.Hardcoded);
                }
            }
        }

        public static void PerformGearboxAssignments(IIdentVehicle vehicle, ILogger Log, ReactorEngine reactor, Action<string, string, LayoutGroup> protocolServiceCode)
        {
            if (!useLegacyGearboxTypeDetection(vehicle))
            {
                return;
            }

            if (!string.IsNullOrEmpty(((IReactorVehicle)vehicle).Ereihe) && "E65 E66 E67 E68".Contains(((IReactorVehicle)vehicle).Ereihe))
            {
                ProtocolGerboxServiceCode(vehicle, "AUT", protocolServiceCode, "PerformGearboxAssignments");
                reactor.SetGetriebe("AUT", DataSource.Hardcoded);
            }
            else if (HasVehicleGearboxECU(vehicle) || vehicle.getECU(24L) != null)
            {
                IIdentEcu eCU = vehicle.getECU(24L);
                if ((eCU == null && (vehicle.VehicleIdentLevel == IdentificationLevel.VINVehicleReadout || vehicle.VehicleIdentLevel == IdentificationLevel.VINVehicleReadoutOnlineUpdated)) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "DKG", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "GSGE", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "SMG", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "SSG", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || (eCU != null && !string.IsNullOrEmpty(eCU.VARIANTE) && string.Compare(eCU.VARIANTE, 0, "GSD", 0, 3, StringComparison.OrdinalIgnoreCase) == 0) || vehicle.HasSA("2MK") || vehicle.HasSA("206") || vehicle.HasSA("2TC"))
                {
                    if ("AUT".Equals(((IReactorVehicle)vehicle).Getriebe, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Info("VehicleIdent.doVehicleIdent()", "found DKG ECU in vehicle with recognized automatic gearbox; will be overwritten");
                    }

                    ProtocolGerboxServiceCode(vehicle, "MECH", protocolServiceCode, "PerformGearboxAssignments");
                    reactor.SetGetriebe("MECH", DataSource.Hardcoded);
                }
                else
                {
                    if ("MECH".Equals(((IReactorVehicle)vehicle).Getriebe, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Info("VehicleIdent.doVehicleIdent()", "found EGS ECU in vehicle with recognized manual gearbox; will be overwritten");
                    }

                    ProtocolGerboxServiceCode(vehicle, "AUT", protocolServiceCode, "PerformGearboxAssignments");
                    reactor.SetGetriebe("AUT", DataSource.Hardcoded);
                }
            }
            else
            {
                if (((IReactorVehicle)vehicle).FA != null && ((IReactorVehicle)vehicle).FA.SA != null && ((IReactorVehicle)vehicle).FA.SA.Count > 0 && !((IReactorVehicle)vehicle).FA.SA.Contains("205"))
                {
                    Log.Info("VehicleIdent.doVehicleIdent()", "gearbox set to manual, because vehicle does not contain 205 sa");
                    ProtocolGerboxServiceCode(vehicle, "MECH", protocolServiceCode, "PerformGearboxAssignments");
                    reactor.SetGetriebe("MECH", DataSource.Hardcoded);
                }

                if (vehicle.ECU != null && vehicle.ECU.Count > 0 && vehicle.BordnetType != BordnetType.IBUS)
                {
                    Log.Info("VehicleIdent.doVehicleIdent()", "gearbox set to manual, because vehicle has no D_EGS or G_EGS");
                    ProtocolGerboxServiceCode(vehicle, "AUT", protocolServiceCode, "PerformGearboxAssignments");
                    reactor.SetGetriebe("AUT", DataSource.Hardcoded);
                }
            }
        }

        public static bool HasVehicleGearboxECU(IIdentVehicle vehicle)
        {
            return GearboxHelper.HasVehicleGearboxECU(((IReactorVehicle)vehicle).Motor, ((IReactorVehicle)vehicle).Getriebe, vehicle.HasSA);
        }

        private static void ProtocolGerboxServiceCode(IReactorVehicle vec, string hardcodedGearboxValue, Action<string, string, LayoutGroup> protocolServiceCode, [CallerMemberName] string caller = "")
        {
            if (protocolServiceCode != null)
            {
                string nVI10_GearboxHardcodedLogic_nu_LF = ServiceCodes.NVI10_GearboxHardcodedLogic_nu_LF;
                string arg = vec.Ereihe + ", " + hardcodedGearboxValue + ", " + vec.Getriebe + ", " + caller;
                protocolServiceCode(nVI10_GearboxHardcodedLogic_nu_LF, arg, LayoutGroup.D);
            }
        }
    }
}