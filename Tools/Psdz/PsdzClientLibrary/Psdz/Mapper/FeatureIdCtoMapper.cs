using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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