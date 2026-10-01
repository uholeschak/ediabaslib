using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzResponse : IPsdzResponse
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Cause { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public object Result { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}
