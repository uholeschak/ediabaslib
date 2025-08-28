using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    [DataContract]
    public class PsdzTransactionProgressEvent : PsdzTransactionEvent, IPsdzTransactionProgressEvent, IPsdzTransactionEvent, IPsdzEvent, ILocalizableMessage
    {
        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public int TaProgress { get; set; }
    }
}
