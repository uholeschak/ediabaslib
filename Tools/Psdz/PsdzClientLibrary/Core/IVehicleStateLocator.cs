namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IVehicleStateLocator : ISPELocator
{
    string Title { get; }
}
