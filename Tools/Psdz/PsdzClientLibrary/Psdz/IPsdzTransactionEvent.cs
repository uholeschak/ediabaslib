using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz.Model.Events
{
    public interface IPsdzTransactionEvent : IPsdzEvent, ILocalizableMessage
    {
        PsdzTransactionInfo TransactionInfo { get; }

        PsdzTaCategories TransactionType { get; }
    }
}
