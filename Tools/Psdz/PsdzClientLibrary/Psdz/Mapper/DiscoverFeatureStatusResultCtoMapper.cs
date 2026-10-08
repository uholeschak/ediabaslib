using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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