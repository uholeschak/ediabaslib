using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public enum PsdzCertMemoryObjectType
    {
        Certificate,
        Binding,
        OtherBinding,
        OnlineCertificatesEcu,
        OnlineBindingsEcu,
        SecOcKeyList
    }

    public enum PsdzCertMemoryObjectSource
    {
        CBB,
        UNKNOWN,
        VEHICLE
    }

    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzCertMemoryObject
    {
        [DataMember]
        public IPsdzEcuIdentifier Ecu { get; set; }

        [DataMember]
        public string SerializedCertificate { get; set; }

        [DataMember]
        public PsdzCertMemoryObjectType CertMemoryObjectType { get; set; }

        [DataMember]
        public PsdzCertMemoryObjectSource CertMemoryObjectSource { get; set; }
    }
}
