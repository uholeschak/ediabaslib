using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class ReadSecureEcuModeResultCtoMapper
    {
        private static SecureEcuModeEtoMapper _secureEcuModeEtoMapper = new SecureEcuModeEtoMapper();
        internal static IPsdzReadSecureEcuModeResultCto Map(ReadSecureEcuModeResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzReadSecureEcuModeResultCto
            {
                FailureResponse = model.Failures?.Select(EcuFailureResponseCtoMapper.MapCto),
                SecureEcuModes = model.SecureEcuModes?.ToDictionary((KeyValuePairEnumModel<EcuIdentifierModel, SecureEcuModeEto> kvPair) => EcuIdentifierMapper.Map(kvPair.Key), (KeyValuePairEnumModel<EcuIdentifierModel, SecureEcuModeEto> kvPair) => _secureEcuModeEtoMapper.GetValue(kvPair.Value))
            };
        }
    }
}