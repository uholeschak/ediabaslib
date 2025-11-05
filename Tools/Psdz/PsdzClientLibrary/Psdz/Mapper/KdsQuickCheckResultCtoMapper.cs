using BMW.Rheingold.Psdz.Model.Kds;

namespace BMW.Rheingold.Psdz
{
    internal static class KdsQuickCheckResultCtoMapper
    {
        private static KdsQuickCheckResultEtoMapper _kdsQuickCheckResultEtoMapper = new KdsQuickCheckResultEtoMapper();
        internal static IPsdzKdsQuickCheckResultCto Map(KdsQuickCheckResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzKdsQuickCheckResultCto
            {
                KdsId = KdsIdCtoMapper.Map(model.KdsId),
                QuickCheckResult = _kdsQuickCheckResultEtoMapper.GetValue(model.QuickCheckResult)
            };
        }

        internal static KdsQuickCheckResultCtoModel Map(IPsdzKdsQuickCheckResultCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new KdsQuickCheckResultCtoModel
            {
                KdsId = KdsIdCtoMapper.Map(psdzObject.KdsId),
                QuickCheckResult = _kdsQuickCheckResultEtoMapper.GetValue(psdzObject.QuickCheckResult)
            };
        }
    }
}