using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal class FeatureConditionCtoMapper
    {
        private static ConditionTypeEtoMapper conditionTypeEtoMapper = new ConditionTypeEtoMapper();

        public static IPsdzFeatureConditionCto Map(FeatureConditionCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzFeatureConditionCto
            {
                ConditionType = conditionTypeEtoMapper.GetValue(model.ConditionType),
                CurrentValidityValue = model.CurrentValidityValue,
                Length = model.Length,
                ValidityValue = model.ValidityValue
            };
        }

        public static FeatureConditionCtoModel Map(IPsdzFeatureConditionCto psdzFeatureConditionCto)
        {
            if (psdzFeatureConditionCto == null)
            {
                return null;
            }
            return new FeatureConditionCtoModel
            {
                ConditionType = conditionTypeEtoMapper.GetValue(psdzFeatureConditionCto.ConditionType),
                CurrentValidityValue = psdzFeatureConditionCto.CurrentValidityValue,
                Length = psdzFeatureConditionCto.Length,
                ValidityValue = psdzFeatureConditionCto.ValidityValue
            };
        }
    }
}