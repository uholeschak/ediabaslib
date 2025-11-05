using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuFeatureTokenRelationCtoMapper
    {
        private static FeatureGroupEtoMapper featureGroupEtoMapper = new FeatureGroupEtoMapper();
        public static IPsdzEcuFeatureTokenRelationCto Map(EcuFeatureTokenRelationCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzEcuFeatureTokenRelationCto
            {
                ECUIdentifier = EcuIdentifierCtoMapper.Map(model.EcuIdentifier),
                FeatureGroup = featureGroupEtoMapper.GetValue(model.FeatureGroup),
                FeatureId = FeatureIdCtoMapper.Map(model.FeatureId),
                TokenId = model.TokenId
            };
        }

        public static EcuFeatureTokenRelationCtoModel Map(IPsdzEcuFeatureTokenRelationCto psdzEcuFeatureTokenRelationCto)
        {
            if (psdzEcuFeatureTokenRelationCto == null)
            {
                return null;
            }

            return new EcuFeatureTokenRelationCtoModel
            {
                EcuIdentifier = EcuIdentifierCtoMapper.Map(psdzEcuFeatureTokenRelationCto.ECUIdentifier),
                FeatureGroup = featureGroupEtoMapper.GetValue(psdzEcuFeatureTokenRelationCto.FeatureGroup),
                FeatureId = FeatureIdCtoMapper.Map(psdzEcuFeatureTokenRelationCto.FeatureId),
                TokenId = psdzEcuFeatureTokenRelationCto.TokenId
            };
        }
    }
}