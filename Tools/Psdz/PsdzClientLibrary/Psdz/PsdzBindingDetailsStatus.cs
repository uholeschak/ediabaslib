using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
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
