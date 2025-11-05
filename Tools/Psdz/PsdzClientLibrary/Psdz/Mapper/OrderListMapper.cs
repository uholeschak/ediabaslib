using BMW.Rheingold.Psdz.Model.Svb;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class OrderListMapper
    {
        internal static IPsdzOrderList Map(OrderListModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzOrderList
            {
                NumberOfUnits = model.NumberOfUnits,
                BntnVariantInstances = model.BntnVariantInstances?.Select(EcuVariantInstanceMapper.Map).ToArray()
            };
        }

        internal static OrderListModel Map(IPsdzOrderList psdzOrderList)
        {
            if (psdzOrderList == null)
            {
                return null;
            }

            return new OrderListModel
            {
                NumberOfUnits = psdzOrderList.NumberOfUnits,
                BntnVariantInstances = psdzOrderList.BntnVariantInstances?.Select(EcuVariantInstanceMapper.Map).ToArray()
            };
        }
    }
}