using System;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.VinValidator;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.VinValidator;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using BMW.ISPI.ISTA.Contracts.Interfaces.VinValidator;
using BMW.ISPI.TRIC.ISTA.Contracts.Implementations.Models;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.ISPI.TRIC.ISTA.Contracts.Models;
using PsdzClientLibrary.Core;

namespace BMW.ISPI.TRIC.ISTA.VinValidator
{
    public class Validator : IVinValidator
    {
        private enum Datasource
        {
            DB,
            SVMD,
            FBM
        }

        private readonly IVinValidatorSVMDAccess svmdAccess;

        private readonly IVinValidatorDataAccess dbAccess;

        private readonly IVinValidatorFDLAccess fdlAccess;

        private readonly ILogger log;

        public static List<TypeKeys> TypeKeys { get; set; } = new List<TypeKeys>();

        public List<TypeKeys> PossibleTypeKeys { get; set; } = new List<TypeKeys>();

        public Validator(IVinValidatorVehicle vehicle, IVinValidatorSVMDAccess svmdAccess, IVinValidatorDataAccess dbAccess, IVinValidatorFDLAccess fdlAccess, ILogger log)
        {
            this.svmdAccess = svmdAccess;
            this.dbAccess = dbAccess;
            this.fdlAccess = fdlAccess;
            this.log = log;
        }

        internal VINResolverResult ValidateVINAndSetTypeKeys(string vin, bool isVehicleConnected)
        {
            return ValidateVINAndSetTypeKeys(vin, isVehicleConnected, Language.de_DE);
        }

        public VINResolverResult ValidateVINAndSetTypeKeys(string vin, bool isVehicleConnected, Language language)
        {
            VINResolverResult vINResolverResult = ValidateOverSVMD(vin, isVehicleConnected);
            if (vINResolverResult.SVMDStatus == BackendStatus.Success)
            {
                return vINResolverResult;
            }
            vINResolverResult = ValidateOverFDLGate(vin, language);
            if (vINResolverResult.FDLGateStatus == BackendStatus.Success)
            {
                return vINResolverResult;
            }
            return ValidateOverDB(vin);
        }

        private VINResolverResult ValidateOverSVMD(string vin, bool isVehicleConnected)
        {
            bool flag = vin.Length > 7;
            string text = (flag ? vin.Substring(10, 7) : vin);
            BackendData<VinValidatorSVMDRequestVINResponse> possibleVIN17AndTypeKeys = svmdAccess.GetPossibleVIN17AndTypeKeys(text);
            VINResolverResult vINResolverResult = new VINResolverResult
            {
                ResultType = VINResolverResultType.NoHit,
                SVMDStatus = possibleVIN17AndTypeKeys.Status
            };
            switch (possibleVIN17AndTypeKeys.Status)
            {
                case BackendStatus.Success:
                    {
                        VinValidatorSVMDRequestVINResponse data = possibleVIN17AndTypeKeys.Data;
                        vINResolverResult.PossibleVINs = data.Keys;
                        vINResolverResult.ResultType = ((data.Count() <= 1) ? VINResolverResultType.SingleHit : VINResolverResultType.MultipleHit);
                        if (vINResolverResult.ResultType == VINResolverResultType.MultipleHit && !isVehicleConnected && !flag)
                        {
                            log.Info(log.CurrentMethod(), "Multiple hits in SVMD. Validation over DB to determine EBezeichnung.");
                            ValidateOverDB(text);
                            break;
                        }
                        string text2 = vINResolverResult.PossibleVINs.FirstOrDefault((string v) => v.Equals(vin, StringComparison.CurrentCultureIgnoreCase));
                        if (text2 != null)
                        {
                            vINResolverResult.VINResolvedSuccessfully = true;
                            VinValidationTypeKeyResult vinValidationTypeKeyResult = data[text2];
                            vinValidationTypeKeyResult.TypeKeyLead = GetTypeKeyLeadFromDB(vinValidationTypeKeyResult.TypeKey);
                            AssignTypeKeys(vinValidationTypeKeyResult, Datasource.SVMD);
                            log.Info(log.CurrentMethod(), "Vin properly validated over SVMD. Type keys assigned.");
                        }
                        else if (!flag && vINResolverResult.ResultType == VINResolverResultType.MultipleHit)
                        {
                            IEnumerable<string> allMatchingTypSchluessels = dbAccess.GetAllMatchingTypSchluessels(text);
                            Dictionary<string, string> eBezeichnungsByTypeSchlussels = GetEBezeichnungsByTypeSchlussels(allMatchingTypSchluessels);
                            log.Info(log.CurrentMethod(), "Multiple hits from the SVMD. User would be informed which vehicle he wants to use.");
                            log.Info(log.CurrentMethod(), "Multiple hits from the SVMD. Found EBezeichnungs: " + string.Join(",", eBezeichnungsByTypeSchlussels.Values));
                        }
                        break;
                    }
                case BackendStatus.SuccessNoData:
                    log.Info(log.CurrentMethod(), "No info found in SVMD. Session would be closed.");
                    break;
                case BackendStatus.Error:
                    log.Error(log.CurrentMethod(), "Error getting data from SVMDProcessor.");
                    break;
            }
            return vINResolverResult;
        }

        private VINResolverResult ValidateOverFDLGate(string vin, Language language)
        {
            VINResolverResult vINResolverResult = new VINResolverResult
            {
                ResultType = VINResolverResultType.NoHit
            };
            if (vin.Length != 17)
            {
                return vINResolverResult;
            }
            BackendData<VinValidationTypeKeyResult> typeKeys = fdlAccess.GetTypeKeys(vin, language);
            vINResolverResult.FDLGateStatus = typeKeys.Status;
            if (typeKeys.Status != BackendStatus.Success)
            {
                log.Info(log.CurrentMethod(), "Request to FDLGate failed. Validation impossible.");
                return vINResolverResult;
            }
            vINResolverResult.VINResolvedSuccessfully = true;
            vINResolverResult.ResultType = VINResolverResultType.SingleHit;
            vINResolverResult.PossibleVINs = new List<string> { vin };
            AssignTypeKeys(typeKeys.Data, Datasource.FBM);
            log.Info(log.CurrentMethod(), "Successfully validated over FDLGate. Type keys assigned.");
            return vINResolverResult;
        }

        private string GetTypeKeyLeadFromDB(string typeKey)
        {
            IXepCharacteristics xepCharacteristics = dbAccess.GetVehicleIdentByTypeKey(typeKey)?.FirstOrDefault((IXepCharacteristics c) => c.RootNodeClass == 99999999905m);
            if (xepCharacteristics != null)
            {
                return xepCharacteristics.Name;
            }
            log.Info(log.CurrentMethod(), "Unable to determine TypKeyLead from DB for vin {0}.", typeKey);
            return null;
        }

        private VINResolverResult ValidateOverDB(string vin)
        {
            string vin2 = ((vin.Length > 7) ? vin.Substring(10, 7) : vin);
            VINResolverResult vINResolverResult = new VINResolverResult();
            IEnumerable<string> allMatchingTypSchluessels = dbAccess.GetAllMatchingTypSchluessels(vin2);
            if (allMatchingTypSchluessels.Any())
            {
                if (allMatchingTypSchluessels.Count() == 1)
                {
                    IList<IXepCharacteristics> vehicleIdentByTypeKey = dbAccess.GetVehicleIdentByTypeKey(allMatchingTypSchluessels.First());
                    if (vehicleIdentByTypeKey != null)
                    {
                        VinValidationTypeKeyResult typeKeys = ReadTypeKeysFromCharacteristics(vehicleIdentByTypeKey);
                        AssignTypeKeys(typeKeys, Datasource.DB);
                        vINResolverResult.VINResolvedSuccessfully = true;
                        log.Info(log.CurrentMethod(), "Successfully validated over database. Type keys assigned.");
                    }
                }
                else
                {
                    Dictionary<string, string> eBezeichnungsByTypeSchlussels = GetEBezeichnungsByTypeSchlussels(allMatchingTypSchluessels);
                    log.Info(log.CurrentMethod(), "Multiple hits from the database. User would be informed which vehicle he wants to use.");
                    log.Info(log.CurrentMethod(), "Multiple hits from the database. Found EBezeichnungs: " + string.Join(",", eBezeichnungsByTypeSchlussels.Values));
                    foreach (string item in allMatchingTypSchluessels)
                    {
                        IList<IXepCharacteristics> vehicleIdentByTypeKey2 = dbAccess.GetVehicleIdentByTypeKey(item);
                        if (vehicleIdentByTypeKey2 != null)
                        {
                            VinValidationTypeKeyResult typeKeys2 = ReadTypeKeysFromCharacteristics(vehicleIdentByTypeKey2);
                            AssignTypeKeys(typeKeys2, Datasource.DB);
                            vINResolverResult.VINResolvedSuccessfully = true;
                        }
                    }
                }
            }
            else
            {
                log.Info(log.CurrentMethod(), "No info found in the database. Session would be closed.");
            }
            return vINResolverResult;
        }

        private VinValidationTypeKeyResult ReadTypeKeysFromCharacteristics(IEnumerable<IXepCharacteristics> characteristics)
        {
            VinValidationTypeKeyResult vinValidationTypeKeyResult = new VinValidationTypeKeyResult();
            foreach (IXepCharacteristics characteristic in characteristics)
            {
                if (characteristic.RootNodeClass == 40139650m)
                {
                    vinValidationTypeKeyResult.TypeKey = characteristic.Name;
                }
                else if (characteristic.RootNodeClass == 40139652m)
                {
                    vinValidationTypeKeyResult.TypeKeyBasics = characteristic.Name;
                }
                else if (characteristic.RootNodeClass == 99999999905m)
                {
                    vinValidationTypeKeyResult.TypeKeyLead = characteristic.Name;
                }
            }
            return vinValidationTypeKeyResult;
        }

        private Dictionary<string, string> GetEBezeichnungsByTypeSchlussels(IEnumerable<string> typeSchlussels)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string typeSchlussel in typeSchlussels)
            {
                IXepCharacteristics xepCharacteristics = dbAccess.GetVehicleIdentByTypeKey(typeSchlussel).FirstOrDefault((IXepCharacteristics c) => c.RootNodeClass == 40140802m);
                if (xepCharacteristics != null)
                {
                    dictionary.Add(typeSchlussel, xepCharacteristics.Name);
                }
            }
            return dictionary;
        }

        private void AssignTypeKeys(VinValidationTypeKeyResult typeKeys, Datasource source)
        {
            PossibleTypeKeys.Add(new TypeKeys
            {
                TypeKey = typeKeys.TypeKey,
                TypeKeyBasic = typeKeys.TypeKeyBasics,
                TypeKeyLead = typeKeys.TypeKeyLead
            });
            TypeKeys.Add(new TypeKeys
            {
                TypeKey = typeKeys.TypeKey,
                TypeKeyBasic = typeKeys.TypeKeyBasics,
                TypeKeyLead = typeKeys.TypeKeyLead
            });
            log.Info(log.CurrentMethod(), string.Format("Determined values from {0}: TypeKey: {1} TypeKeyBasic: {2} TypeKeyLead: {3}", source, (typeKeys.TypeKey != null) ? typeKeys.TypeKey : "-", (typeKeys.TypeKeyBasics != null) ? typeKeys.TypeKeyBasics : "-", (typeKeys.TypeKeyLead != null) ? typeKeys.TypeKeyLead : "-"));
        }
    }
}
