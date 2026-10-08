namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwtApplicationId
    {
        int AppNo { get; }

        int UpgradeIdx { get; }
    }
}
