using BMW.Rheingold.Psdz.Model.Localization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Events;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    public interface IPsdzTransactionProgressEvent : IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        int Progress { get; }

        int TaProgress { get; }
    }
}
