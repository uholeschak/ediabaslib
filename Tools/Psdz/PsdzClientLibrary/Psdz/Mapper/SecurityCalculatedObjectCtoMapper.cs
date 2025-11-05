using BMW.Rheingold.Psdz.Model.Certificate;
using System.Collections.Generic;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class SecurityCalculatedObjectCtoMapper
    {
        private static SecurityCalculationOverallStatusEtoMapper _securityCalculationOverallStatusEtoMapper = new SecurityCalculationOverallStatusEtoMapper();
        private static SecurityCalculationDetailedStatusEtoMapper _securityCalculationDetailedStatusEtoMapper = new SecurityCalculationDetailedStatusEtoMapper();
        internal static PsdzSecurityCalculatedObjectCto Map(SecurityCalculatedObjectCto model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSecurityCalculatedObjectCto
            {
                ServicePack = model.ServicePack,
                OverallStatus = _securityCalculationOverallStatusEtoMapper.GetValue(model.OverallStatus),
                MemoryObject = SecurityMemoryObjectEtoMapper.Map(model.MemoryObject),
                RoleStatus = model.RoleStatus?.ToDictionary((KeyValuePair<string, SecurityCalculationDetailedStatusEto> kvPair) => kvPair.Key, (KeyValuePair<string, SecurityCalculationDetailedStatusEto> kvPair) => _securityCalculationDetailedStatusEtoMapper.GetValue(kvPair.Value)),
                KeyIdStatus = model.KeyIdStatus?.ToDictionary((KeyValuePair<string, SecurityCalculationDetailedStatusEto> kvPair) => kvPair.Key, (KeyValuePair<string, SecurityCalculationDetailedStatusEto> kvPair) => _securityCalculationDetailedStatusEtoMapper.GetValue(kvPair.Value))
            };
        }

        internal static SecurityCalculatedObjectCto Map(PsdzSecurityCalculatedObjectCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new SecurityCalculatedObjectCto
            {
                ServicePack = psdzObject.ServicePack,
                OverallStatus = _securityCalculationOverallStatusEtoMapper.GetValue(psdzObject.OverallStatus),
                MemoryObject = SecurityMemoryObjectEtoMapper.Map(psdzObject.MemoryObject),
                RoleStatus = psdzObject.RoleStatus?.ToDictionary((KeyValuePair<string, PsdzCertCalculationDetailedStatus> kvPair) => kvPair.Key, (KeyValuePair<string, PsdzCertCalculationDetailedStatus> kvPair) => _securityCalculationDetailedStatusEtoMapper.GetValue(kvPair.Value)),
                KeyIdStatus = psdzObject.KeyIdStatus?.ToDictionary((KeyValuePair<string, PsdzCertCalculationDetailedStatus> kvPair) => kvPair.Key, (KeyValuePair<string, PsdzCertCalculationDetailedStatus> kvPair) => _securityCalculationDetailedStatusEtoMapper.GetValue(kvPair.Value))
            };
        }
    }
}