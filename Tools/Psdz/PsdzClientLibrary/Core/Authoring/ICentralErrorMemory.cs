using BMW.Authoring;
using BMW.Authoring.API;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle.Interface
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface ICentralErrorMemory : IHideObjectMembers
    {
        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        CentralErrorMemoryStatus CentralErrorMemoryStatus { get; set; }

        List<IZfsResult> ZfsResult { get; set; }

        List<ICemResult> CemResult { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        CentralErrorMemoryStatus DoEcuReadCentralErrorMemoryForNewGenerationVehicles(IAuthoringModule istaModule, PsdzClient.Core.Vehicle vehicle);
    }
}
