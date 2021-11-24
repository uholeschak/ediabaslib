using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    class RootCertificateStateEnumMapper : ProgrammingEnumMapperBase<PsdzRootCertificateState, RootCertificateState>
    {
        protected override IDictionary<PsdzRootCertificateState, RootCertificateState> CreateMap()
        {
            return new Dictionary<PsdzRootCertificateState, RootCertificateState>
            {
                {
                    PsdzRootCertificateState.Accepted,
                    RootCertificateState.Accepted
                },
                {
                    PsdzRootCertificateState.Invalid,
                    RootCertificateState.Invalid
                },
                {
                    PsdzRootCertificateState.NotAvailable,
                    RootCertificateState.NotAvailable
                },
                {
                    PsdzRootCertificateState.Rejected,
                    RootCertificateState.Rejected
                }
            };
        }
    }
}
