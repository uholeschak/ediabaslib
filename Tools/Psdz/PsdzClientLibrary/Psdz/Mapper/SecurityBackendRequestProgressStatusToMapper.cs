using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class SecurityBackendRequestProgressStatusToMapper : MapperBase<PsdzSecurityBackendRequestProgressStatusToEnum, SecurityBackendRequestProgressStatusTo>
    {
        protected override IDictionary<PsdzSecurityBackendRequestProgressStatusToEnum, SecurityBackendRequestProgressStatusTo> CreateMap()
        {
            return new Dictionary<PsdzSecurityBackendRequestProgressStatusToEnum, SecurityBackendRequestProgressStatusTo>
            {
                {
                    PsdzSecurityBackendRequestProgressStatusToEnum.ERROR,
                    SecurityBackendRequestProgressStatusTo.ERROR
                },
                {
                    PsdzSecurityBackendRequestProgressStatusToEnum.RUNNING,
                    SecurityBackendRequestProgressStatusTo.RUNNING
                },
                {
                    PsdzSecurityBackendRequestProgressStatusToEnum.SUCCESS,
                    SecurityBackendRequestProgressStatusTo.SUCCESS
                },
                {
                    PsdzSecurityBackendRequestProgressStatusToEnum.UNKNOWN_REQUEST_ID,
                    SecurityBackendRequestProgressStatusTo.UNKNOWN_REQUEST_ID
                }
            };
        }
    }
}