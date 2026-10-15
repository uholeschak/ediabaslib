
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    public interface IPsdzMcdDiagServiceEvent : IPsdzEvent, ILocalizableMessage
    {
        int ErrorId { get; }

        string ErrorName { get; }

        string JobName { get; }

        string LinkName { get; }

        string ServiceName { get; }

        string ResponseType { get; }

        bool IsTimingEvent { get; }
    }
}
