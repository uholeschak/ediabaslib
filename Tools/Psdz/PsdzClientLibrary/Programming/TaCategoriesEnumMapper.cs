using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace BMW.Rheingold.Programming.Common
{
    internal sealed class TaCategoriesEnumMapper : ProgrammingEnumMapperBase<PsdzTaCategories, TaCategories>
    {
        protected override IDictionary<PsdzTaCategories, TaCategories> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
