using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzIstufenTriple : IPsdzIstufenTriple
    {
        [DataMember]
        public string Current { get; set; }

        [DataMember]
        public string Last { get; set; }

        [DataMember]
        public string Shipment { get; set; }
    }
}
