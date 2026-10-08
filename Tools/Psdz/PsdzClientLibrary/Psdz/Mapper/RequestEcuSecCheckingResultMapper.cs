using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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