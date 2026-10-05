using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

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
