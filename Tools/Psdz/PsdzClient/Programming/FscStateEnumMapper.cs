using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Psdz;

namespace PsdzClient.Programming
{
    class FscStateEnumMapper : ProgrammingEnumMapperBase<PsdzFscState, FscState>
    {
        protected override IDictionary<PsdzFscState, FscState> CreateMap()
        {
            return new Dictionary<PsdzFscState, FscState>
            {
                {
                    PsdzFscState.Accepted,
                    FscState.Accepted
                },
                {
                    PsdzFscState.Cancelled,
                    FscState.Cancelled
                },
                {
                    PsdzFscState.Imported,
                    FscState.Imported
                },
                {
                    PsdzFscState.Invalid,
                    FscState.Invalid
                },
                {
                    PsdzFscState.NotAvailable,
                    FscState.NotAvailable
                },
                {
                    PsdzFscState.Rejected,
                    FscState.Rejected
                }
            };
        }
    }
}
