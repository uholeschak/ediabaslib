using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzFp : PsdzStandardFp, IPsdzFp, IPsdzStandardFp
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Baureihenverbund { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Entwicklungsbaureihe { get; set; }
    }
}
