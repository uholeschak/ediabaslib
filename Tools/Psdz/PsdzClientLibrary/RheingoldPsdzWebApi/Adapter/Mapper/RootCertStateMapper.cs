using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;

namespace RheingoldPsdzWebApi.Adapter.Mapper
{
    internal class RootCertStateMapper : MapperBase<PsdzRootCertificateState, RootCertStatusModel>
    {
        protected override IDictionary<PsdzRootCertificateState, RootCertStatusModel> CreateMap()
        {
            return new Dictionary<PsdzRootCertificateState, RootCertStatusModel>
            {
                {
                    PsdzRootCertificateState.Accepted,
                    RootCertStatusModel.Accepted
                },
                {
                    PsdzRootCertificateState.Invalid,
                    RootCertStatusModel.Invalid
                },
                {
                    PsdzRootCertificateState.NotAvailable,
                    RootCertStatusModel.NotAvailable
                },
                {
                    PsdzRootCertificateState.Rejected,
                    RootCertStatusModel.Rejected
                }
            };
        }
    }
}