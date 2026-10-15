using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IRuleCache
    {
        IDictionary<decimal, IRuleExpression> CacheXepRules { get; }

        IDictionary<decimal, IRuleExpression> PatchXepRules { get; }

        ISearchCacheContainer SearchCacheContainer { get; set; }
    }
}