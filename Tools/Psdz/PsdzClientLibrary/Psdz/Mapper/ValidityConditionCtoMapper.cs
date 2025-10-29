using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class ValidityConditionCtoMapper
    {
        private static ConditionTypeEtoMapper _conditionTypeEtoMapper = new ConditionTypeEtoMapper();

        public static IPsdzValidityConditionCto Map(ValidityConditionCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzValidityConditionCto
            {
                ValidityValue = model.ValidityValue,
                ConditionType = _conditionTypeEtoMapper.GetValue(model.ConditionType)
            };
        }

        public static ValidityConditionCtoModel Map(IPsdzValidityConditionCto psdzValidityConditionCto)
        {
            if (psdzValidityConditionCto == null)
            {
                return null;
            }
            return new ValidityConditionCtoModel
            {
                ValidityValue = psdzValidityConditionCto.ValidityValue,
                ConditionType = _conditionTypeEtoMapper.GetValue(psdzValidityConditionCto.ConditionType)
            };
        }
    }
}