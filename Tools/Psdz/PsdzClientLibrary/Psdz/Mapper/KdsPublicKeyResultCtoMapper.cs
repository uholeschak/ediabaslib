using BMW.Rheingold.Psdz.Model.Kds;

namespace BMW.Rheingold.Psdz
{
    internal static class KdsPublicKeyResultCtoMapper
    {
        internal static IPsdzKdsPublicKeyResultCto Map(KdsPublicKeyResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzKdsPublicKeyResultCto
            {
                KdsId = KdsIdCtoMapper.Map(model.KdsIdCto),
                PublicKey = model.PublicKey
            };
        }

        internal static KdsPublicKeyResultCtoModel Map(IPsdzKdsPublicKeyResultCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new KdsPublicKeyResultCtoModel
            {
                KdsIdCto = KdsIdCtoMapper.Map(psdzObject.KdsId),
                PublicKey = psdzObject.PublicKey
            };
        }
    }
}