using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzReadCertMemoryObjectResult
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzCertMemoryObject[] MemoryObjects { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuFailureResponse[] FailedEcus { get; set; }
    }
}
