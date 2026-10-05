using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuProgrammingVariantLocator : ISPELocator
    {
        string Name { get; }

        decimal? FlashLimit { get; }

        decimal EcuVariantId { get; }
    }
}
