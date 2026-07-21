namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface ICombinedFaultLocator : IFaultCodeLocator, ISPELocator
{
}
