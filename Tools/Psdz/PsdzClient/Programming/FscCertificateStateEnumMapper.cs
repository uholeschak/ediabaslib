using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    class FscCertificateStateEnumMapper : ProgrammingEnumMapperBase<PsdzFscCertificateState, FscCertificateState>
    {
        protected override IDictionary<PsdzFscCertificateState, FscCertificateState> CreateMap()
        {
            return new Dictionary<PsdzFscCertificateState, FscCertificateState>
            {
                {
                    PsdzFscCertificateState.Accepted,
                    FscCertificateState.Accepted
                },
                {
                    PsdzFscCertificateState.Imported,
                    FscCertificateState.Imported
                },
                {
                    PsdzFscCertificateState.Invalid,
                    FscCertificateState.Invalid
                },
                {
                    PsdzFscCertificateState.NotAvailable,
                    FscCertificateState.NotAvailable
                },
                {
                    PsdzFscCertificateState.Rejected,
                    FscCertificateState.Rejected
                }
            };
        }
    }
}
