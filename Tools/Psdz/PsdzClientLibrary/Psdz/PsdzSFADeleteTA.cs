using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSFADeleteTA : PsdzTa, IPsdzFsaTa, IPsdzTa, IPsdzTalElement
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long EstimatedExecutionTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long FeatureId { get; set; }
    }
}
