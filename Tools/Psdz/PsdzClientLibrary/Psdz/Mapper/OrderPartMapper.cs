using BMW.Rheingold.Psdz.Model.Svb;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class OrderPartMapper
    {
        internal static IPsdzOrderPart Map(OrderPartModel model)
        {
            if (model == null)
            {
                return null;
            }

            PsdzOrderPart psdzOrderPart = LogisticPartMapper.Map<PsdzOrderPart>(model);
            IPsdzLogisticPart[] deliverables = model.Deliverables?.Select(LogisticPartMapper.Map<PsdzLogisticPart>).ToArray();
            psdzOrderPart.Deliverables = deliverables;
            deliverables = model.Pattern?.Select(LogisticPartMapper.Map<PsdzLogisticPart>).ToArray();
            psdzOrderPart.Pattern = deliverables;
            return psdzOrderPart;
        }

        internal static OrderPartModel Map(IPsdzOrderPart psdzOrderPart)
        {
            if (psdzOrderPart == null)
            {
                return null;
            }

            OrderPartModel orderPartModel = LogisticPartMapper.Map<OrderPartModel>(psdzOrderPart);
            orderPartModel.Deliverables = psdzOrderPart.Deliverables?.Select(LogisticPartMapper.Map<LogisticPartModel>).ToArray();
            orderPartModel.Pattern = psdzOrderPart.Pattern?.Select(LogisticPartMapper.Map<LogisticPartModel>).ToArray();
            return orderPartModel;
        }
    }
}