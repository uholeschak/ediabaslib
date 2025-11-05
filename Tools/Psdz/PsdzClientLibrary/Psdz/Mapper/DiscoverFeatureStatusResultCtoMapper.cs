using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class DiscoverFeatureStatusResultCtoMapper
    {
        public static IPsdzDiscoverFeatureStatusResultCto Map(DiscoverFeatureStatusResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzDiscoverFeatureStatusResultCto
            {
                ErrorMessage = model.ErrorMessage,
                FeatureStatus = model.FeatureStatusList?.Select(FeatureStatusToMapper.Map).ToList()
            };
        }
    }
}