using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleStateLocator : ISPELocator
    {
        string Title { get; }
    }

}
