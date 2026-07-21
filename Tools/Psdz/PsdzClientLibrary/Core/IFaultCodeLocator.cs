namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IFaultCodeLocator : ISPELocator
{
    ITextContent TextContent { get; }

    string Code { get; }

    IDocumentLocator GetDocument();

    IDocumentLocator GetDocument(string docType);

    void Set();
}
