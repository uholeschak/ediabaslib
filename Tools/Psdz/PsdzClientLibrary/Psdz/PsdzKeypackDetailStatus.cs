using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzKeypackDetailStatus
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? KeyPackStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string KeyId { get; set; }
    }
}
