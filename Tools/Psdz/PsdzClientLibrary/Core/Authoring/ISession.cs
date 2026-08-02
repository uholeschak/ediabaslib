using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Session
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface ISession : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string GuiLanguage { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDealerData DealerData { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Pannenfall { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        IVehicleState VehicleState { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ISessionEquipment SessionEquipment { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool IsVerificationMode { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        bool IsDebugMode { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        BMW.Authoring.Session.Enums.OperationalMode OperationalMode { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEnumerable<IDiagCode> DiagCodes { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITestPlan TestPlan { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IEcuKom EcuKomDCan { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool StartEcuKomDCan();

        [EditorBrowsable(EditorBrowsableState.Always)]
        void StopEcuKomDCan();
    }
}
