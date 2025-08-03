using PsdzClient.Core;
using System;

namespace PsdzClient.Utility
{
    internal class VehicleHelper
    {
        internal static bool HasSA(IVehicleRuleEvaluation vehicle, string checkSA)
        {
            if (vehicle.FA == null)
            {
                return false;
            }
            IFARuleEvaluation iFARuleEvaluation = ((vehicle.TargetFA != null) ? vehicle.TargetFA : vehicle.FA);
            if (iFARuleEvaluation.SA != null)
            {
                foreach (string item in iFARuleEvaluation.SA)
                {
                    if (string.Compare(item, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                    if (item.Length == 4 && string.Compare(item.Substring(1), checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            if (iFARuleEvaluation.E_WORT != null)
            {
                foreach (string item2 in iFARuleEvaluation.E_WORT)
                {
                    if (string.Compare(item2, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            if (iFARuleEvaluation.HO_WORT != null)
            {
                foreach (string item3 in iFARuleEvaluation.HO_WORT)
                {
                    if (string.Compare(item3, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static IIdentEcu GetECUbyTITLE_ECUTREE(IVehicleRuleEvaluation vehicle, string grobName)
        {
            if (string.IsNullOrEmpty(grobName) || vehicle == null || vehicle.ECU == null)
            {
                return null;
            }
            foreach (IIdentEcu item in vehicle.ECU)
            {
                if (string.Compare(item.TITLE_ECUTREE, grobName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return item;
                }
            }
            return null;
        }

        internal static IIdentEcu GetECUbyECU_SGBD(IVehicleRuleEvaluation vehicle, string ECU_SGBD)
        {
            if (string.IsNullOrEmpty(ECU_SGBD) || vehicle == null || vehicle.ECU == null)
            {
                return null;
            }
            string[] array = ECU_SGBD.Split(new char[1] { '|' });
            string[] array2 = array;
            foreach (string b in array2)
            {
                foreach (IIdentEcu item in vehicle.ECU)
                {
                    if (string.Equals(item.ECU_SGBD, b, StringComparison.OrdinalIgnoreCase) || string.Equals(item.VARIANTE, b, StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        internal static IIdentEcu GetECUbyECU_GRUPPE(IVehicleRuleEvaluation vehicle, string ECU_GRUPPE)
        {
            if (string.IsNullOrEmpty(ECU_GRUPPE) || vehicle == null || vehicle.ECU == null)
            {
                return null;
            }
            foreach (IIdentEcu item in vehicle.ECU)
            {
                if (string.IsNullOrEmpty(item.ECU_GRUPPE))
                {
                    continue;
                }
                string[] array = ECU_GRUPPE.Split(new char[1] { '|' });
                string[] array2 = item.ECU_GRUPPE.Split(new char[1] { '|' });
                string[] array3 = array2;
                foreach (string a in array3)
                {
                    string[] array4 = array;
                    foreach (string b in array4)
                    {
                        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

#if false
        internal static bool GetISTACharacteristics(decimal id, out string value, long datavalueId, IDataProviderRuleEvaluation dataProvider, ILogger logger, IVehicleRuleEvaluation vehcile, ValidationRuleInternalResults internalResult)
        {
            IXepCharacteristicRoots characteristicRootsById = dataProvider.GetCharacteristicRootsById(id);
            if (characteristicRootsById != null)
            {
                VehicleCharacteristicVehicleHelper vehicleCharacteristicVehicleHelper = new VehicleCharacteristicVehicleHelper(dataProvider, vehcile);
                return vehicleCharacteristicVehicleHelper.GetISTACharacteristics(characteristicRootsById.Nodeclass, out value, id, vehcile, datavalueId, internalResult);
            }
            logger.Warning("Vehicle.getISTACharactersitics()", "No entry found in CharacteristicRoots for id: {0}!", id);
            value = "???";
            return false;
        }
#endif
    }
}