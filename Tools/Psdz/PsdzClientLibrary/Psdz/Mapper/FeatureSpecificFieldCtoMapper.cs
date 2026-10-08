using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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