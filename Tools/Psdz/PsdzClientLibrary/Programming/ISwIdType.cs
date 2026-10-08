namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwIdType
    {
        string ApplicationNo { get; set; }

        string UpgradeIndex { get; set; }
    }
}
