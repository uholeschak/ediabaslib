using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class KdsClientsForRefurbishResultCtoMapper
    {
        internal static IPsdzKdsClientsForRefurbishResultCto Map(KdsClientsForRefurbishResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzKdsClientsForRefurbishResultCto
            {
                KdsFailureResponse = KdsFailureResponseCtoMapper.Map(model.KdsFailureResponseCto),
                KdsIds = model.KdsIds?.Select(KdsIdCtoMapper.Map).ToList()
            };
        }

        internal static KdsClientsForRefurbishResultCtoModel Map(IPsdzKdsClientsForRefurbishResultCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new KdsClientsForRefurbishResultCtoModel
            {
                KdsFailureResponseCto = KdsFailureResponseCtoMapper.Map(psdzObject.KdsFailureResponse),
                KdsIds = psdzObject.KdsIds?.Select(KdsIdCtoMapper.Map).ToList()
            };
        }
    }
}