namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IVirtualFaultCodeLocator : IFaultCodeLocator, ISPELocator
{
}
