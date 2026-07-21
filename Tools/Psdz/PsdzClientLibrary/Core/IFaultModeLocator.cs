namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IFaultModeLocator : ISPELocator
{
    string Code { get; }

    string Title { get; }

    ITextContent TextContent { get; }
}
