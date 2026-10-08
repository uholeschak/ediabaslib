using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class BindingCalculationFailureMapper
    {
        public static PsdzBindingCalculationFailure Map(SecurityBackendRequestFailureCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzBindingCalculationFailure
            {
                Retry = model.Retry,
                Url = model.Url,
                Reason = model.Cause.Description
            };
        }
    }
}