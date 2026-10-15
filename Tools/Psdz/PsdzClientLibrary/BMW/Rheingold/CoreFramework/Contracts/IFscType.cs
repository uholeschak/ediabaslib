namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFscType
    {
        string Code { get; set; }

        string Id { get; set; }

        string Value { get; set; }

        byte[] GetBinaryValue();
    }
}
