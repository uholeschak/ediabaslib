using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IVirtualFaultCodeLocator : IFaultCodeLocator, ISPELocator
{
}
