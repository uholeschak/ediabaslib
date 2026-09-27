using System;
using PsdzClient.Core;

#pragma warning disable CS0618
namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI]
    [Obsolete("This interface is deprecated and will be removed in future (earliest 4.63) versions. Please use the authoring namespace instead to access session context information.")]
    public interface IAppSessionContext
    {
        [AuthorAPIHidden]
        [Obsolete("This interface is deprecated and will be removed in future (earliest 4.63) versions. Please use the authoring namespace instead to access session context information.")]
        ILogic Logic { get; }

        [AuthorAPIHidden]
        [Obsolete("This interface is deprecated and will be removed in future (earliest 4.63) versions. Please use the authoring namespace instead to access session context information.")]
        IStateApplication AppState { get; }

        [Obsolete("This interface is deprecated and will be removed in future (earliest 4.63) versions. Please use: BMW.Authoring.Session.OperationalMode instead.")]
        OperationalMode OperationalMode { get; }

        [Obsolete("This interface is deprecated and will be removed in future (earliest 4.63) versions. Please use the authoring namespace instead to access session context information.")]
        bool IsOnlineMode { get; }

        [Obsolete("This interface is deprecated and will be removed in future (earliest 4.63) versions. Please use: BMW.Authoring.Session.VehicleState.IsVehicleConnectionOnline instead.")]
        bool IsVehicleConnectionOnline { get; }
    }
}