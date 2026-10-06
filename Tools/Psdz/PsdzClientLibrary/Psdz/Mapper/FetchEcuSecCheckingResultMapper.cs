using System.Linq;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class FetchEcuSecCheckingResultMapper
    {
        internal static PsdzFetchEcuCertCheckingResult Map(FetchEcuSecCheckingResultModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzFetchEcuCertCheckingResult
            {
                FailedEcus = model.FailedEcus?.Select(EcuFailureResponseCtoMapper.Map),
                Results = model.Results?.Select(EcuSecCheckingResponseEtoMapper.Map)
            };
        }
    }
}