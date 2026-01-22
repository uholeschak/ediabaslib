using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzOtherBindingDetailsStatus
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? OtherBindingStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string RollenName { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string EcuName { get; set; }
    }
}
