using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.SecureCoding;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzScbResultStatusCto : IPsdzScbResultStatusCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AppErrorId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Code { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
