using BMW.Rheingold.Psdz.Model.Localization;
using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzTransactionProgressEvent : PsdzTransactionEvent, IPsdzTransactionProgressEvent, IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Progress { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int TaProgress { get; set; }
    }
}
