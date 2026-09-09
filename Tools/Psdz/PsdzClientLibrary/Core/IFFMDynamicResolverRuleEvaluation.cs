using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using PsdzClient;

namespace PsdzClient.Core
{
    public interface IFFMDynamicResolverRuleEvaluation
    {
        bool? Resolve(decimal id, IXepInfoObjectRuleEvaluation iObj);
    }
}