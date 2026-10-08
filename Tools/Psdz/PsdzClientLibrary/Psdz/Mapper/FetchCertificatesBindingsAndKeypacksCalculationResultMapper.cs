using System.Linq;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal static class FetchCertificatesBindingsAndKeypacksCalculationResultMapper
    {
        private static BindingCalculationProgessStatusToMapper _bindingCalculationProgessStatusMapper = new BindingCalculationProgessStatusToMapper();
        internal static PsdzFetchBindingCalculationResult Map(FetchCertificatesBindingsAndKeypacksCalculationResultModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzFetchBindingCalculationResult
            {
                DurationOfLastRequest = model.DurationOfLastRequest,
                CalculatedBindings = model.CalculatedBindings?.Select(SecurityCalculatedObjectCtoMapper.Map).ToArray(),
                CalculatedCertificates = model.CalculatedCertificates?.Select(SecurityCalculatedObjectCtoMapper.Map).ToArray(),
                CalculatedKeypacks = model.CalculatedKeypacks?.Select(SecurityCalculatedObjectCtoMapper.Map).ToArray(),
                Failures = model.Failures?.Select(BindingCalculationFailureMapper.Map).ToArray(),
                ProgressStatus = _bindingCalculationProgessStatusMapper.GetValue(model.ProgressStatus)
            };
        }
    }
}