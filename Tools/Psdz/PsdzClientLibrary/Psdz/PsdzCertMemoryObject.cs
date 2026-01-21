using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Certificate
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
