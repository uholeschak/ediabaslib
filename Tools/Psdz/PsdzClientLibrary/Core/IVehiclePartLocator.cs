namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IVehiclePartLocator : ISPELocator
{
    string Title { get; }
}
