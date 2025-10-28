using BMW.Rheingold.Psdz.Model.Sfa;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class FeatureLongStatusCtoMapper
    {
        private static FeatureStatusEtoEnumMapper featureStatusEtoEnumMapper = new FeatureStatusEtoEnumMapper();

        private static ValidationStatusEtoMapper validationStatusEtoMapper = new ValidationStatusEtoMapper();

        public static IPsdzFeatureLongStatusCto Map(FeatureLongStatusCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzFeatureLongStatusCto
            {
                EcuIdentifierCto = EcuIdentifierCtoMapper.Map(model.EcuIdentifier),
                FeatureConditions = model.FeatureConditions?.Select(FeatureConditionCtoMapper.Map).ToList(),
                FeatureId = FeatureIdCtoMapper.Map(model.FeatureId),
                FeatureStatusEto = featureStatusEtoEnumMapper.GetValue(model.FeatureStatusEto),
                MileageOfActivation = model.MileageOfActivation,
                TokenId = model.TokenId,
                ValidationStatusEto = validationStatusEtoMapper.GetValue(model.ValidationStatusEto)
            };
        }

        public static FeatureLongStatusCtoModel Map(IPsdzFeatureLongStatusCto psdzFeatureLongStatusCto)
        {
            if (psdzFeatureLongStatusCto == null)
            {
                return null;
            }
            return new FeatureLongStatusCtoModel
            {
                EcuIdentifier = EcuIdentifierCtoMapper.Map(psdzFeatureLongStatusCto.EcuIdentifierCto),
                FeatureConditions = psdzFeatureLongStatusCto.FeatureConditions?.Select(FeatureConditionCtoMapper.Map).ToList(),
                FeatureId = FeatureIdCtoMapper.Map(psdzFeatureLongStatusCto.FeatureId),
                FeatureStatusEto = featureStatusEtoEnumMapper.GetValue(psdzFeatureLongStatusCto.FeatureStatusEto),
                MileageOfActivation = psdzFeatureLongStatusCto.MileageOfActivation,
                TokenId = psdzFeatureLongStatusCto.TokenId,
                ValidationStatusEto = validationStatusEtoMapper.GetValue(psdzFeatureLongStatusCto.ValidationStatusEto)
            };
        }
    }
}