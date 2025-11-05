using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class ValidationRuleInternalResults : List<ValidationRuleInternalResult>
    {
        public IRuleExpression RuleExpression { get; set; }
    }
}