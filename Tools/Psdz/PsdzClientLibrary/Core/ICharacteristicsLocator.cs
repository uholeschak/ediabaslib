namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface ICharacteristicsLocator : ISPELocator
{
    decimal ParentId { get; }

    string Title { get; }

    string Title_dede { get; }

    string Name { get; }
}
