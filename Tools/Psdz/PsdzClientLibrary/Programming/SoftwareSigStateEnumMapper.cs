using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class SoftwareSigStateEnumMapper : ProgrammingEnumMapperBase<PsdzSoftwareSigState, SoftwareSigState>
    {
        protected override IDictionary<PsdzSoftwareSigState, SoftwareSigState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
