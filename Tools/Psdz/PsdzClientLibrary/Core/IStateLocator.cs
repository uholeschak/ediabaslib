namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IStateLocator : ISPELocator
    {
        decimal? ParentId { get; }

        string Statevalue { get; set; }

        ITextContent TextContent { get; set; }
    }
}
