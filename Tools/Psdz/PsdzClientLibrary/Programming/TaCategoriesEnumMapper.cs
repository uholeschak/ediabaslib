using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Psdz.Model.Tal;

namespace PsdzClient.Programming
{
    internal sealed class TaCategoriesEnumMapper : ProgrammingEnumMapperBase<PsdzTaCategories, TaCategories>
    {
        protected override IDictionary<PsdzTaCategories, TaCategories> CreateMap()
        {
            return CreateMapBase();
        }
    }
}
