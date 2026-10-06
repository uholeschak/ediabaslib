using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Tal;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Events;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    public interface IPsdzTransactionEvent : IPsdzEvent, ILocalizableMessage
    {
        PsdzTransactionInfo TransactionInfo { get; }

        PsdzTaCategories TransactionType { get; }
    }
}
