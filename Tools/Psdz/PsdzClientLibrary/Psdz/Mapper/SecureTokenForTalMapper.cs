using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
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