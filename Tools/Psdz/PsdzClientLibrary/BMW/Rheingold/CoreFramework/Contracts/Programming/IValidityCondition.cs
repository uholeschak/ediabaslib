namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
	public interface IValidityCondition
    {
        ConditionTypeEnum ConditionType { get; set; }

        string ValidityValue { get; set; }
    }
}
