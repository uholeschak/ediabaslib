using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IVehicleStateManager
    {
        IVehicleStateLocator GetState(IVehiclePartLocator part);
    }
}
