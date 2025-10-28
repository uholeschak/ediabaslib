using BMW.Rheingold.Psdz.Model.Svb;

namespace BMW.Rheingold.Psdz
{
    internal static class SollverbauungMapper
    {
        internal static IPsdzSollverbauung Map(SollverbauungModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzSollverbauung
            {
                AsXml = model.AsXml,
                Svt = SvtMapper.Map(model.Svt),
                PsdzOrderList = OrderListMapper.Map(model.OrderList)
            };
        }

        internal static SollverbauungModel Map(IPsdzSollverbauung psdzSollverbauung)
        {
            if (psdzSollverbauung == null)
            {
                return null;
            }
            return new SollverbauungModel
            {
                AsXml = psdzSollverbauung.AsXml,
                Svt = SvtMapper.Map(psdzSollverbauung.Svt),
                OrderList = OrderListMapper.Map(psdzSollverbauung.PsdzOrderList)
            };
        }
    }
}