using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI]
    public interface IAppSessionContext
    {
        [AuthorAPIHidden]
        ILogic Logic { get; }

        [AuthorAPIHidden]
        IStateApplication AppState { get; }

        OperationalMode OperationalMode { get; }

        bool IsOnlineMode { get; }

        bool IsVehicleConnectionOnline { get; }
    }
}
