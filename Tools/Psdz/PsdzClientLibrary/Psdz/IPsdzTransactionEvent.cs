using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz.Model.Events
{
    public enum PsdzTransactionInfo
    {
        Started,
        Repeating,
        ProgressInfo,
        Finished,
        FinishedWithError
    }

    public interface IPsdzTransactionEvent : IPsdzEvent, ILocalizableMessage
    {
        PsdzTransactionInfo TransactionInfo { get; }

        PsdzTaCategories TransactionType { get; }
    }
}
