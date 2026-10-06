using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzBindingDetailsStatus
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? BindingStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? CertificateStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string RollenName { get; set; }
    }
}
