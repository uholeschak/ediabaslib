using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;
using BMW.Authoring.Vehicle.Interface;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IVehicle : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IFaultList FaultList { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ICCMList CCMList { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcuList EcuList { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDetails Details { get; }

        IVerbraucherList VerbraucherList { get; }

        ICentralErrorMemory CentralErrorMemory { get; }
    }
}
