using System.Collections.Generic;

namespace PsdzClient.Core
{
    public interface IRuleCache
    {
        IDictionary<decimal, IRuleExpression> CacheXepRules { get; }

        IDictionary<decimal, IRuleExpression> PatchXepRules { get; }

        ISearchCacheContainer SearchCacheContainer { get; set; }
    }
}