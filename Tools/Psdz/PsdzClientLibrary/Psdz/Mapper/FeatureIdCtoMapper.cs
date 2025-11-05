using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class FeatureIdCtoMapper
    {
        public static IPsdzFeatureIdCto Map(FeatureIdCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzFeatureIdCto
            {
                Value = model.FeatureId
            };
        }

        public static FeatureIdCtoModel Map(IPsdzFeatureIdCto model)
        {
            if (model == null)
            {
                return null;
            }

            return new FeatureIdCtoModel
            {
                FeatureId = model.Value
            };
        }
    }
}