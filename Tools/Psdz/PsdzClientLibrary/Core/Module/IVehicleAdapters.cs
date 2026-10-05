using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace PBMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IVehicleAdapters
    {
        bool IsInstalled(IVehicleAdapterLocator adapter);
    }
}
