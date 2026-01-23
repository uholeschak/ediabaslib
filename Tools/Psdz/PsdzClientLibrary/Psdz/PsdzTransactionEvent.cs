using BMW.Rheingold.Psdz.Model.Localization;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzTransactionEvent : PsdzEvent, IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzTransactionInfo TransactionInfo { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzTaCategories TransactionType { get; set; }
    }
}
