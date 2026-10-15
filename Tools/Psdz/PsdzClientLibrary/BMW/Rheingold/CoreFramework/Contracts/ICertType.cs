namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ICertType
    {
        string Code { get; set; }

        string Serial { get; set; }

        string Value { get; set; }

        byte[] GetBinaryValue();
    }
}
