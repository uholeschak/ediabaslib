using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzCertMemoryObject
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier Ecu { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string SerializedCertificate { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzCertMemoryObjectType CertMemoryObjectType { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzCertMemoryObjectSource CertMemoryObjectSource { get; set; }
    }
}
