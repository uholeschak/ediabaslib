using PsdzClient;
using BMW.ISPI.TRIC.ISTA.Contracts.Implementations;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IRuleExpression
    {
        [PreserveSource(Hint = "dataProvider removed")]
        bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult);
    }
}
