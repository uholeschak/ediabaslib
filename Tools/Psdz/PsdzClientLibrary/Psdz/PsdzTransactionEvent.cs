using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz.Model.Events
{
    [DataContract]
    public class PsdzTransactionEvent : PsdzEvent, IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        [DataMember]
        public PsdzTransactionInfo TransactionInfo { get; set; }

        [DataMember]
        public PsdzTaCategories TransactionType { get; set; }
    }
}
