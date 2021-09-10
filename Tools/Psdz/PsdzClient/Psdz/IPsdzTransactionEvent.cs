using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    
    public interface IPsdzTransactionEvent : ILocalizableMessage, IPsdzEvent
    {
        PsdzTransactionInfo TransactionInfo { get; }

        PsdzTaCategories TransactionType { get; }
    }
}
