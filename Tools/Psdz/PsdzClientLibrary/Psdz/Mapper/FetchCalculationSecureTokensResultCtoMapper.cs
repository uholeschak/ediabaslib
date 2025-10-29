using System.Linq;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class FetchCalculationSecureTokensResultCtoMapper
    {
        private static SecurityBackendRequestProgressStatusToMapper _securityBackendRequestProgressStatusToMapper = new SecurityBackendRequestProgressStatusToMapper();

        private static TokenOverallStatusEtoMapper _tokenOverallStatusEtoMapper = new TokenOverallStatusEtoMapper();

        internal static IPsdzFetchCalculationSecureTokensResultCto Map(FetchCalculationSecureTokensResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzFetchCalculationSecureTokensResultCto
            {
                DetailedStatus = model.TokenDetailedStatusEtos?.Select(DetailedStatusCtoMapper.Map).ToList(),
                DurationOfLastRequest = model.DurationOfLastRequest,
                Failures = model.Failures.Select(SecurityBackendRequestFailureMapper.Map).ToList(),
                FeatureSetReference = model.FeatureSetReference,
                JsonString = model.JsonString,
                ProgressStatus = _securityBackendRequestProgressStatusToMapper.GetValue(model.ProgressStatus),
                SecureTokens = model.SecureTokens?.Select(SecureTokenEtoMapper.Map).ToList(),
                TokenOverallStatusEto = _tokenOverallStatusEtoMapper.GetValue(model.TokenOverallStatusEto),
                TokenPackageReference = model.TokenPackageReference,
                SecureTokenForVehicle = SecureTokenForVehicleEtoMapper.Map(model.SecureTokenForVehicle)
            };
        }

        internal static FetchCalculationSecureTokensResultCtoModel Map(IPsdzFetchCalculationSecureTokensResultCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }
            return new FetchCalculationSecureTokensResultCtoModel
            {
                TokenDetailedStatusEtos = psdzObject.DetailedStatus?.Select(DetailedStatusCtoMapper.Map).ToList(),
                DurationOfLastRequest = psdzObject.DurationOfLastRequest,
                Failures = psdzObject.Failures?.Select(SecurityBackendRequestFailureMapper.Map).ToList(),
                FeatureSetReference = psdzObject.FeatureSetReference,
                JsonString = psdzObject.JsonString,
                ProgressStatus = _securityBackendRequestProgressStatusToMapper.GetValue(psdzObject.ProgressStatus),
                SecureTokens = psdzObject.SecureTokens?.Select(SecureTokenEtoMapper.Map).ToList(),
                TokenOverallStatusEto = _tokenOverallStatusEtoMapper.GetValue(psdzObject.TokenOverallStatusEto),
                TokenPackageReference = psdzObject.TokenPackageReference,
                SecureTokenForVehicle = SecureTokenForVehicleEtoMapper.Map(psdzObject.SecureTokenForVehicle)
            };
        }
    }
}