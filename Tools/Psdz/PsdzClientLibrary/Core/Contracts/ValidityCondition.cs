using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public class ValidityCondition : IValidityCondition
    {
        public ConditionTypeEnum ConditionType { get; set; }

        public string ValidityValue { get; set; }

        public ValidityCondition()
        {
        }

        public ValidityCondition(ConditionTypeEnum condition, string validityValue)
        {
            ConditionType = condition;
            ValidityValue = validityValue;
        }
    }
}
