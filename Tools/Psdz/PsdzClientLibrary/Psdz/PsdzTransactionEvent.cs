using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz.Model.Events
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
