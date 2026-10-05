using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEquipmentLocator : ISPELocator
    {
        string Title { get; }

        string Name { get; }
    }
}
