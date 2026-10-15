using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IVehicleStateManager
    {
        IVehicleStateLocator GetState(IVehiclePartLocator part);
    }
}
