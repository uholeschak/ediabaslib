using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Swt;

namespace PsdzClient.Programming
{
    internal sealed class SwtActionTypeEnumMapper : ProgrammingEnumMapperBase<PsdzSwtActionType, SwtActionType>
    {
        protected override IDictionary<PsdzSwtActionType, SwtActionType> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
