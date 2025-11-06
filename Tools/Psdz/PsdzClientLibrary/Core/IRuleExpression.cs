using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public interface IRuleExpression
    {
        // [UH] arguments modified
        bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult);
    }
}
