using BMW.Rheingold.Psdz.Model.Svb;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class EcuVariantInstanceMapper
    {
        internal static IPsdzEcuVariantInstance Map(EcuVariantInstanceModel model)
        {
            if (model == null)
            {
                return null;
            }

            PsdzEcuVariantInstance psdzEcuVariantInstance = LogisticPartMapper.Map<PsdzEcuVariantInstance>(model);
            psdzEcuVariantInstance.CombinedWith = model.CombinedWith?.Select(Map).ToArray();
            psdzEcuVariantInstance.Ecu = EcuMapper.Map(model.Ecu);
            psdzEcuVariantInstance.OrderablePart = OrderPartMapper.Map(model.OrderablePart);
            return psdzEcuVariantInstance;
        }

        internal static EcuVariantInstanceModel Map(IPsdzEcuVariantInstance model)
        {
            if (model == null)
            {
                return null;
            }

            EcuVariantInstanceModel ecuVariantInstanceModel = LogisticPartMapper.Map<EcuVariantInstanceModel>(model);
            ecuVariantInstanceModel.CombinedWith = model.CombinedWith?.Select(Map).ToList();
            ecuVariantInstanceModel.Ecu = EcuMapper.Map(model.Ecu);
            ecuVariantInstanceModel.OrderablePart = OrderPartMapper.Map(model.OrderablePart);
            return ecuVariantInstanceModel;
        }
    }
}