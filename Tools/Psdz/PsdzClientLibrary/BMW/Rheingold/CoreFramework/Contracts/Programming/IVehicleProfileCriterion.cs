namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleProfileCriterion
    {
        string Name { get; }

        string NameEn { get; }

        int Value { get; }
    }
}
