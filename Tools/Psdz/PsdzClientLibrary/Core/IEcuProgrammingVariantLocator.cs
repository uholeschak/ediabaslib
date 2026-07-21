namespace PsdzClient.Core;

[AuthorAPI(SelectableTypeDeclaration = true)]
public interface IEcuProgrammingVariantLocator : ISPELocator
{
    string Name { get; }

    decimal? FlashLimit { get; }

    decimal EcuVariantId { get; }
}
