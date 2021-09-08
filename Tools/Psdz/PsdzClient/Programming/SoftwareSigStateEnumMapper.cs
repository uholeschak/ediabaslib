using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Psdz;

namespace PsdzClient.Programming
{
    class SoftwareSigStateEnumMapper : ProgrammingEnumMapperBase<PsdzSoftwareSigState, SoftwareSigState>
    {
        protected override IDictionary<PsdzSoftwareSigState, SoftwareSigState> CreateMap()
        {
            return new Dictionary<PsdzSoftwareSigState, SoftwareSigState>
            {
                {
                    PsdzSoftwareSigState.Accepted,
                    SoftwareSigState.Accepted
                },
                {
                    PsdzSoftwareSigState.Imported,
                    SoftwareSigState.Imported
                },
                {
                    PsdzSoftwareSigState.Invalid,
                    SoftwareSigState.Invalid
                },
                {
                    PsdzSoftwareSigState.NotAvailable,
                    SoftwareSigState.NotAvailable
                },
                {
                    PsdzSoftwareSigState.Rejected,
                    SoftwareSigState.Rejected
                }
            };
        }
    }
}
