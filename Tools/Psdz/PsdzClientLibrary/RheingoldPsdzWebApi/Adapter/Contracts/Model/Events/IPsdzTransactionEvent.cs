using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    public interface IPsdzTransactionEvent : IPsdzEvent, ILocalizableMessage
    {
        PsdzTransactionInfo TransactionInfo { get; }

        PsdzTaCategories TransactionType { get; }
    }
}
