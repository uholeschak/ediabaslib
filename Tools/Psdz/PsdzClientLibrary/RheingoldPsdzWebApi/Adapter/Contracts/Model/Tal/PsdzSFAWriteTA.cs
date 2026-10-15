using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzSecureTokenForTal))]
    [DataContract]
    public class PsdzSFAWriteTA : PsdzTa, IPsdzFsaTa, IPsdzTa, IPsdzTalElement
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long EstimatedExecutionTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long FeatureId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSecureTokenForTal SecureToken { get; set; }
    }
}
