using BMW.Rheingold.Psdz.Model.Kds;

namespace BMW.Rheingold.Psdz
{
    internal static class KdsIdCtoMapper
    {
        internal static IPsdzKdsIdCto Map(KdsIdCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzKdsIdCto
            {
                Id = model.Id,
                IdAsHex = model.IdAsHex
            };
        }

        internal static KdsIdCtoModel Map(IPsdzKdsIdCto psdzKdsIdCto)
        {
            if (psdzKdsIdCto == null)
            {
                return null;
            }
            return new KdsIdCtoModel
            {
                Id = psdzKdsIdCto.Id,
                IdAsHex = psdzKdsIdCto.IdAsHex
            };
        }
    }
}