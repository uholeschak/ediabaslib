using BMW.Rheingold.Psdz.Model.Kds;

namespace BMW.Rheingold.Psdz
{
    internal static class KdsFailureResponseCtoMapper
    {
        internal static IPsdzKdsFailureResponseCto Map(KdsFailureResponseCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzKdsFailureResponseCto
            {
                Cause = LocalizableMessageToMapper.Map(model.Cause),
                KdsId = KdsIdCtoMapper.Map(model.KdsIdCto)
            };
        }

        internal static KdsFailureResponseCtoModel Map(IPsdzKdsFailureResponseCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new KdsFailureResponseCtoModel
            {
                Cause = LocalizableMessageToMapper.Map(psdzObject.Cause),
                KdsIdCto = KdsIdCtoMapper.Map(psdzObject.KdsId)
            };
        }
    }
}