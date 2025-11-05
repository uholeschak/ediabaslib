using BMW.Rheingold.Psdz.Model.Kds;
using System.Linq;

namespace BMW.Rheingold.Psdz
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