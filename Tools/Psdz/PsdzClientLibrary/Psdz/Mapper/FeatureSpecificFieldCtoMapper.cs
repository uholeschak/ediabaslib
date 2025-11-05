using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    internal static class FeatureSpecificFieldCtoMapper
    {
        internal static IPsdzFeatureSpecificFieldCto Map(FeatureSpecificFieldCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzFeatureSpecificFieldCto
            {
                FieldType = model.FieldType,
                FieldValue = model.FieldValue
            };
        }

        internal static FeatureSpecificFieldCtoModel Map(IPsdzFeatureSpecificFieldCto model)
        {
            if (model == null)
            {
                return null;
            }

            return new FeatureSpecificFieldCtoModel
            {
                FieldType = model.FieldType,
                FieldValue = model.FieldValue
            };
        }
    }
}