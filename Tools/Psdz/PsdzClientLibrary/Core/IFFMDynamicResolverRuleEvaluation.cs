using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using PsdzClient;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IFFMDynamicResolverRuleEvaluation
    {
        bool? Resolve(decimal id, IXepInfoObjectRuleEvaluation iObj);
    }
}