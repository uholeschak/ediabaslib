using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Tal;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Localization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal.TalStatus
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
