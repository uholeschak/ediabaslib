using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Programming.API.Interface
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IProgrammingService : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IIstufenTriple RequestILevel();

        [EditorBrowsable(EditorBrowsableState.Always)]
        IIstufenTriple RequestILevelFromBackup();

        [EditorBrowsable(EditorBrowsableState.Always)]
        IPlannedSwiAction CreatePlannedSwiAction(string swiActionName, bool isDisabled);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool PlanVehicleModifications(IList<IPlannedSwiAction> plannedSwiActions);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IReadOnlyList<ISgbmId> GetSgbmIds(string baseVariant, int ecuAddress);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IReadOnlyList<ISgbmIdChange> GetDifferentSgbmIds(string ecu);
    }
}
