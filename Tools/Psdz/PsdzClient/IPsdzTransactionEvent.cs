using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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
