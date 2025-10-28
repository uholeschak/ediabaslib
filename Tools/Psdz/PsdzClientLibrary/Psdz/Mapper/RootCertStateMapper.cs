using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Psdz
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