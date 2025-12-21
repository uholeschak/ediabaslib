using PsdzClient;

namespace PsdzClient.Core
{
    public interface IFFMDynamicResolverRuleEvaluation
    {
        [PreserveSource(Hint = "iObj type modified", SignatureModified = true)]
        bool? Resolve(decimal id, PsdzDatabase.SwiInfoObj iObj);
    }
}