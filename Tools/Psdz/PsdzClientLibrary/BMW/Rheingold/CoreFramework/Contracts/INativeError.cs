namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface INativeError
    {
        string Identifier { get; }

        string Message { get; }
    }
}
