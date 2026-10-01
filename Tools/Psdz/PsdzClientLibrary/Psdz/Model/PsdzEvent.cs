using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Localization;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzEvent : IPsdzEvent, ILocalizableMessage
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier EcuId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string EventId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Message { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int MessageId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long Timestamp { get; set; }
    }
}
