using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.ISPI.TRIC.ISTA.Contracts
{
    public interface IRuleCache
    {
        IDictionary<decimal, IRuleExpression> CacheXepRules { get; }

        IDictionary<decimal, IRuleExpression> PatchXepRules { get; }

        ISearchCacheContainer SearchCacheContainer { get; set; }
    }
}