using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class SecureTokenForTalMapper
    {
        public static IPsdzSecureTokenForTal Map(SecureTokenForTalModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSecureTokenForTal
            {
                EcuIdentifier = EcuIdentifierMapper.Map(model.EcuIdentifier),
                FeatureId = model.FeatureId,
                SerializedSecureToken = model.SerializedSecureToken,
                TokenId = model.TokenId
            };
        }

        public static SecureTokenForTalModel Map(IPsdzSecureTokenForTal psdzSecureTokenForTal)
        {
            if (psdzSecureTokenForTal == null)
            {
                return null;
            }

            return new SecureTokenForTalModel
            {
                EcuIdentifier = EcuIdentifierMapper.Map(psdzSecureTokenForTal.EcuIdentifier),
                FeatureId = psdzSecureTokenForTal.FeatureId,
                SerializedSecureToken = psdzSecureTokenForTal.SerializedSecureToken,
                TokenId = psdzSecureTokenForTal.TokenId
            };
        }
    }
}