using BMW.Rheingold.Psdz.Model.Kds;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class ReadPublicKeyResultCtoMapper
    {
        internal static IPsdzReadPublicKeyResultCto Map(ReadPublicKeyResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzReadPublicKeyResultCto
            {
                KdsPublicKeys = model.PublicKeys?.Select(KdsPublicKeyResultCtoMapper.Map).ToList(),
                FailureResponse = KdsFailureResponseCtoMapper.Map(model.KdsFailureResponseCto)
            };
        }

        internal static ReadPublicKeyResultCtoModel Map(IPsdzReadPublicKeyResultCto psdzReadPublicKeyResultCto)
        {
            if (psdzReadPublicKeyResultCto == null)
            {
                return null;
            }
            return new ReadPublicKeyResultCtoModel
            {
                PublicKeys = psdzReadPublicKeyResultCto.KdsPublicKeys?.Select(KdsPublicKeyResultCtoMapper.Map).ToList(),
                KdsFailureResponseCto = KdsFailureResponseCtoMapper.Map(psdzReadPublicKeyResultCto.FailureResponse)
            };
        }
    }
}