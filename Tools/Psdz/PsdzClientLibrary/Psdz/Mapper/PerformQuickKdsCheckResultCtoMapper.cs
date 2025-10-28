﻿using BMW.Rheingold.Psdz.Model.Kds;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class PerformQuickKdsCheckResultCtoMapper
    {
        private static KdsActionStatusEtoMapper _kdsActionStatusEtoMapper = new KdsActionStatusEtoMapper();

        internal static IPsdzPerformQuickKdsCheckResultCto Map(PerformQuickKdsCheckResultCtoModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzPerformQuickKdsCheckResultCto
            {
                KdsActionStatus = _kdsActionStatusEtoMapper.GetValue(model.KdsActionStatus),
                KdsId = KdsIdCtoMapper.Map(model.KdsId),
                KdsQuickCheckResult = model.KdsQuickCheckResults?.Select(KdsQuickCheckResultCtoMapper.Map).ToList(),
                KdsFailureResponse = KdsFailureResponseCtoMapper.Map(model.KdsFailureResponse)
            };
        }

        internal static PerformQuickKdsCheckResultCtoModel Map(IPsdzPerformQuickKdsCheckResultCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }
            return new PerformQuickKdsCheckResultCtoModel
            {
                KdsActionStatus = _kdsActionStatusEtoMapper.GetValue(psdzObject.KdsActionStatus),
                KdsId = KdsIdCtoMapper.Map(psdzObject.KdsId),
                KdsQuickCheckResults = psdzObject.KdsQuickCheckResult?.Select(KdsQuickCheckResultCtoMapper.Map).ToList(),
                KdsFailureResponse = KdsFailureResponseCtoMapper.Map(psdzObject.KdsFailureResponse)
            };
        }
    }
}