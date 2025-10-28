using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    internal sealed class SoftwareSigStateEnumMapper : ProgrammingEnumMapperBase<PsdzSoftwareSigState, SoftwareSigState>
    {
        protected override IDictionary<PsdzSoftwareSigState, SoftwareSigState> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
