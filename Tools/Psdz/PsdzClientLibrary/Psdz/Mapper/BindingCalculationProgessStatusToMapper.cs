using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
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