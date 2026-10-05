using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz.Model.Swt;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class FscStateEnumMapper : ProgrammingEnumMapperBase<PsdzFscState, FscState>
    {
        protected override IDictionary<PsdzFscState, FscState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
