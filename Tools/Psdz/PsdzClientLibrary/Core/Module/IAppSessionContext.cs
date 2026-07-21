using PsdzClient.Core;

#pragma warning disable CS0618
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
