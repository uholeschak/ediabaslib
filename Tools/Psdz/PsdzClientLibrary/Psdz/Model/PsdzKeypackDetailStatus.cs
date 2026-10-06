using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
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
