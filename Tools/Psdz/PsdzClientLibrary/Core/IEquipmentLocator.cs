namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IEquipmentLocator : ISPELocator
{
    string Title { get; }

    string Name { get; }
}
