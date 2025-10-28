using BMW.Rheingold.Psdz.Model.Certificate;

namespace BMW.Rheingold.Psdz
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