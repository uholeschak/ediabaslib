using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Implementations
{
    public class ValidationRuleInternalResults : List<ValidationRuleInternalResult>
    {
        public IRuleExpression RuleExpression { get; set; }
    }
}