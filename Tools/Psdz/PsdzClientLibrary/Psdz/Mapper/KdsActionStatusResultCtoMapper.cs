using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class KdsActionStatusResultCtoMapper
    {
        private static KdsActionStatusEtoMapper _kdsActionStatusEtoMapper = new KdsActionStatusEtoMapper();
        internal static IPsdzKdsActionStatusResultCto Map(KdsActionStatusResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzKdsActionStatusResultCto
            {
                KdsActionStatus = _kdsActionStatusEtoMapper.GetValue(model.KdsActionStatus),
                KdsFailureResponse = KdsFailureResponseCtoMapper.Map(model.KdsFailureResponseCto),
                KdsId = KdsIdCtoMapper.Map(model.KdsId)
            };
        }

        internal static KdsActionStatusResultCtoModel Map(IPsdzKdsActionStatusResultCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new KdsActionStatusResultCtoModel
            {
                KdsActionStatus = _kdsActionStatusEtoMapper.GetValue(psdzObject.KdsActionStatus),
                KdsFailureResponseCto = KdsFailureResponseCtoMapper.Map(psdzObject.KdsFailureResponse),
                KdsId = KdsIdCtoMapper.Map(psdzObject.KdsId)
            };
        }
    }
}