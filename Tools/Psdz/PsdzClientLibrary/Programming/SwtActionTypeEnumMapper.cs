using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Psdz.Model.Swt;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class SwtActionTypeEnumMapper : ProgrammingEnumMapperBase<PsdzSwtActionType, SwtActionType>
    {
        protected override IDictionary<PsdzSwtActionType, SwtActionType> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
