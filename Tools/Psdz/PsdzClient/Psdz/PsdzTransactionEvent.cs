using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzTransactionEvent : PsdzEvent, ILocalizableMessage, IPsdzEvent, IPsdzTransactionEvent
    {
        [DataMember]
        public PsdzTransactionInfo TransactionInfo { get; set; }

        [DataMember]
        public PsdzTaCategories TransactionType { get; set; }
    }
}
