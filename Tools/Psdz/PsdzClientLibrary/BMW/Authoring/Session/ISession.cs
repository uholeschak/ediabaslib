using System;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Authoring.Session.Interface;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication;

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
        [Obsolete("Use DiagCodeList instead. To be removed with 4.64.")]
        IEnumerable<BMW.Rheingold.CoreFramework.Contracts.Vehicle.IDiagCode> DiagCodes { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDiagCodeList DiagCodeList { get; }

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