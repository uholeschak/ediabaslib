using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzIstufenTriple : IPsdzIstufenTriple
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Current { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Last { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Shipment { get; set; }
    }
}
