using System.Linq;
using BMW.Rheingold.Psdz.Model.Certificate;

namespace BMW.Rheingold.Psdz
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