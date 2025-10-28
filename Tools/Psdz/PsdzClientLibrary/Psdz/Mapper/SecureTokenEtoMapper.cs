using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class SecureTokenEtoMapper
    {
        internal static IPsdzSecureTokenEto Map(SecureTokenEtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzSecureTokenEto
            {
                EcuIdentifier = EcuIdentifierCtoMapper.Map(model.EcuIdentifier),
                FeatureIdCto = FeatureIdCtoMapper.Map(model.FeatureId),
                SerializedSecureToken = model.SerializedSecureToken,
                TokenId = model.TokenId
            };
        }

        internal static SecureTokenEtoModel Map(IPsdzSecureTokenEto psdzSecureTokenEto)
        {
            if (psdzSecureTokenEto == null)
            {
                return null;
            }
            return new SecureTokenEtoModel
            {
                EcuIdentifier = EcuIdentifierCtoMapper.Map(psdzSecureTokenEto.EcuIdentifier),
                FeatureId = FeatureIdCtoMapper.Map(psdzSecureTokenEto.FeatureIdCto),
                SerializedSecureToken = psdzSecureTokenEto.SerializedSecureToken,
                TokenId = psdzSecureTokenEto.TokenId
            };
        }
    }
}