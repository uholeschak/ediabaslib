using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.ISPI.TRIC.ISTA.Contracts.Implementations;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IRuleExpression
    {
        [PreserveSource(Hint = "dataProvider removed")]
        bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult);
    }
}
