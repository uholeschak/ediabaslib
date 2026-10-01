using BMW.Rheingold.Psdz.Model.Localization;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal.TalStatus
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzTalElement))]
    [DataContract]
    public class PsdzFailureCause : IPsdzFailureCause, ILocalizableMessage
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Id { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string IdReference { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Message { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int MessageId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzTalElement TalElement { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long Timestamp { get; set; }
    }
}
