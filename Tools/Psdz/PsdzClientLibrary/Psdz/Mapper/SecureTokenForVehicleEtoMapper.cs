namespace BMW.Rheingold.Psdz
{
    internal static class SecureTokenForVehicleEtoMapper
    {
        public static IPsdzSecureTokenForVehicleEto Map(SecureTokenForVehicleEtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzSecureTokenForVehicleEto
            {
                FeatureIdCto = FeatureIdCtoMapper.Map(model.FeatureId),
                TokenId = model.TokenId,
                SerializedSecureToken = model.SerializedSecureToken
            };
        }

        public static SecureTokenForVehicleEtoModel Map(IPsdzSecureTokenForVehicleEto psdzSecureTokenForVehicleEto)
        {
            if (psdzSecureTokenForVehicleEto == null)
            {
                return null;
            }
            return new SecureTokenForVehicleEtoModel
            {
                FeatureId = FeatureIdCtoMapper.Map(psdzSecureTokenForVehicleEto.FeatureIdCto),
                TokenId = psdzSecureTokenForVehicleEto.TokenId,
                SerializedSecureToken = psdzSecureTokenForVehicleEto.SerializedSecureToken
            };
        }
    }
}