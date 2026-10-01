using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzVin : IPsdzVin
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Value { get; set; }
    }
}
