using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using PsdzClient;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [PreserveSource(Hint = "Class cleaned", SuppressWarning = true)]
    public interface IFFMDynamicResolver : IFFMDynamicResolverRuleEvaluation
    {
    }
}
