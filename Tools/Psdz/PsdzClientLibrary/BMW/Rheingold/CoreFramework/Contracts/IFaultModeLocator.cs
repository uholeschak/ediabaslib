namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFaultModeLocator : ISPELocator
    {
        string Code { get; }

        string Title { get; }

        ITextContent TextContent { get; }
    }
}
