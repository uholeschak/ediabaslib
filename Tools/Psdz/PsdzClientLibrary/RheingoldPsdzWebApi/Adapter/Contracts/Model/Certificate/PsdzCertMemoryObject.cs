using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
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
