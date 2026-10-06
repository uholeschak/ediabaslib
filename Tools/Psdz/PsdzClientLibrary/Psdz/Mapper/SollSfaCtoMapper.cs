using BMW.Rheingold.Psdz.Model.Sfa;
using System.Linq;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class SollSfaCtoMapper
    {
        internal static IPsdzSollSfaCto Map(SollSfaCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzSollSfaCto
            {
                SollFeatures = model.SollFeatures?.Select(EcuFeatureTokenRelationCtoMapper.Map)
            };
        }

        internal static SollSfaCtoModel Map(IPsdzSollSfaCto psdzSollSfaCto)
        {
            if (psdzSollSfaCto == null)
            {
                return null;
            }

            return new SollSfaCtoModel
            {
                SollFeatures = psdzSollSfaCto.SollFeatures?.Select(EcuFeatureTokenRelationCtoMapper.Map).ToList()
            };
        }
    }
}