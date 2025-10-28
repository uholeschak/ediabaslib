using BMW.Rheingold.Psdz.Model.Certificate;
using System.Linq;

namespace BMW.Rheingold.Psdz
{
    internal static class RequestEcuSecCheckingResultMapper
    {
        internal static PsdzRequestEcuSecCheckingResult Map(RequestEcuSecCheckingResultModel model)
        {
            if (model == null)
            {
                return null;
            }
            return new PsdzRequestEcuSecCheckingResult
            {
                EcuSecCheckingMaxWaitingTimes = model.EcuSecCheckingMaxWaitingTimes?.ToDictionary((EcuCheckingMaxWaitingTimeResultModel kvPair) => EcuIdentifierMapper.Map(kvPair.EcuIdentifierModel), (EcuCheckingMaxWaitingTimeResultModel kvPair) => kvPair.MaxWaitingTime),
                FailedEcus = model.FailedEcus?.Select(EcuFailureResponseCtoMapper.Map)
            };
        }
    }
}