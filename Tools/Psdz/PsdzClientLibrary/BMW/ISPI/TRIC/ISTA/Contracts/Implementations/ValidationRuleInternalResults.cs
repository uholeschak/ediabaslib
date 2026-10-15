using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Implementations
{
    public class ValidationRuleInternalResults : List<ValidationRuleInternalResult>
    {
        public IRuleExpression RuleExpression { get; set; }
    }
}