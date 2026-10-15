using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Swt;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class SwtTypeEnumMapper : ProgrammingEnumMapperBase<PsdzSwtType, SwtType>
    {
        protected override IDictionary<PsdzSwtType, SwtType> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
