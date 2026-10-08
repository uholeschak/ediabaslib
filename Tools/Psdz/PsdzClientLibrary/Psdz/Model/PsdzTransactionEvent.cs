using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
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
