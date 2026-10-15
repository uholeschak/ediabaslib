using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
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
