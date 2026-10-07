using RheingoldPsdzWebApi.Adapter.Contracts.Model.Events;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    public interface IPsdzTransactionProgressEvent : IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        int Progress { get; }

        int TaProgress { get; }
    }
}
