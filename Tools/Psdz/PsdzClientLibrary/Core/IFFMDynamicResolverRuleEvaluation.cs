using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public interface IFFMDynamicResolverRuleEvaluation
    {
        [PreserveSource(Hint = "iObj type modified")]
        bool? Resolve(decimal id, PsdzDatabase.SwiInfoObj iObj);
    }
}