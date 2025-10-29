using BMW.Rheingold.Psdz.Model.Sfa;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class FeatureRequestCtoMapper
    {
        private static SfaLinkTypeEtoMapper _sfaLinkTypeEtoMapper = new SfaLinkTypeEtoMapper();

        internal static IPsdzFeatureRequestCto Map(FeatureRequestCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzFeatureRequestCto
            {
                EcuUid = EcuUidCtoMapper.Map(model.EcuUid),
                EnableType = model.EnableType,
                FeatureId = FeatureIdCtoMapper.Map(model.FeatureId),
                SfaLinkType = _sfaLinkTypeEtoMapper.GetValue(model.LinkType),
                FeatureSpecificFields = model.FeatureSpecificFields?.Select(FeatureSpecificFieldCtoMapper.Map).ToList(),
                ValidityConditions = model.ValidityConditions?.Select(ValidityConditionCtoMapper.Map).ToList()
            };
        }

        internal static FeatureRequestCtoModel Map(IPsdzFeatureRequestCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }
            return new FeatureRequestCtoModel
            {
                EcuUid = EcuUidCtoMapper.Map(psdzObject.EcuUid),
                EnableType = psdzObject.EnableType,
                FeatureId = FeatureIdCtoMapper.Map(psdzObject.FeatureId),
                LinkType = _sfaLinkTypeEtoMapper.GetValue(psdzObject.SfaLinkType),
                FeatureSpecificFields = psdzObject.FeatureSpecificFields?.Select(FeatureSpecificFieldCtoMapper.Map).ToList(),
                ValidityConditions = psdzObject.ValidityConditions?.Select(ValidityConditionCtoMapper.Map).ToList()
            };
        }
    }
}