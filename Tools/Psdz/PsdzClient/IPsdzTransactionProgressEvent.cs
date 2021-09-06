using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzTransactionProgressEvent : ILocalizableMessage, IPsdzEvent, IPsdzTransactionEvent
    {
        int Progress { get; }

        int TaProgress { get; }
    }
}
