using System.Collections.Generic;
using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class SecureTokenRequestCtoMapper
    {
        internal static IPsdzSecureTokenRequestCto Map(SecureTokenRequestCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSecureTokenRequestCto
            {
                VIN = VinMapper.Map(model.Vin),
                EcuFeatureRequests = model.EcuFeatureRequests?.ToDictionary((EcuFeatureRequests a) => EcuIdentifierCtoMapper.Map(a.Ecu), (EcuFeatureRequests a) => a.FeatureRequests?.Select(FeatureRequestCtoMapper.Map))
            };
        }

        internal static SecureTokenRequestCtoModel Map(IPsdzSecureTokenRequestCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new SecureTokenRequestCtoModel
            {
                Vin = VinMapper.Map(psdzObject.VIN),
                EcuFeatureRequests = psdzObject.EcuFeatureRequests?.Select((KeyValuePair<IPsdzEcuIdentifier, IEnumerable<IPsdzFeatureRequestCto>> kvPair) => new EcuFeatureRequests { Ecu = EcuIdentifierCtoMapper.Map(kvPair.Key), FeatureRequests = kvPair.Value?.Select(FeatureRequestCtoMapper.Map).ToList() }).ToList()
            };
        }
    }
}