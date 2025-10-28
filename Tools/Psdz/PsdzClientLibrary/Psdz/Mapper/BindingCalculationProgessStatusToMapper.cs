using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Certificate;

namespace BMW.Rheingold.Psdz
{
    internal class BindingCalculationProgessStatusToMapper : MapperBase<PsdzBindingCalculationProgessStatus, SecurityBackendRequestProgressStatusTo>
    {
        protected override IDictionary<PsdzBindingCalculationProgessStatus, SecurityBackendRequestProgressStatusTo> CreateMap()
        {
            return new Dictionary<PsdzBindingCalculationProgessStatus, SecurityBackendRequestProgressStatusTo>
            {
                {
                    PsdzBindingCalculationProgessStatus.Error,
                    SecurityBackendRequestProgressStatusTo.ERROR
                },
                {
                    PsdzBindingCalculationProgessStatus.Running,
                    SecurityBackendRequestProgressStatusTo.RUNNING
                },
                {
                    PsdzBindingCalculationProgessStatus.Success,
                    SecurityBackendRequestProgressStatusTo.SUCCESS
                },
                {
                    PsdzBindingCalculationProgessStatus.UnknownRequestId,
                    SecurityBackendRequestProgressStatusTo.UNKNOWN_REQUEST_ID
                }
            };
        }
    }
}