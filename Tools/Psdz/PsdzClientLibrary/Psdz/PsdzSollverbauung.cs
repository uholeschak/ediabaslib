using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [DataContract]
    [KnownType(typeof(PsdzSvt))]
    [KnownType(typeof(PsdzOrderList))]
    public class PsdzSollverbauung : IPsdzSollverbauung
    {
        [DataMember]
        public string AsXml { get; set; }

        [DataMember]
        public IPsdzSvt Svt { get; set; }

        [DataMember]
        public IPsdzOrderList PsdzOrderList { get; set; }
    }
}
