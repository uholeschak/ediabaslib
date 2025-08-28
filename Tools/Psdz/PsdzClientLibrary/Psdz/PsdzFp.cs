using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzFp : PsdzStandardFp, IPsdzFp, IPsdzStandardFp
    {
        [DataMember]
        public string Baureihenverbund { get; set; }

        [DataMember]
        public string Entwicklungsbaureihe { get; set; }
    }
}
