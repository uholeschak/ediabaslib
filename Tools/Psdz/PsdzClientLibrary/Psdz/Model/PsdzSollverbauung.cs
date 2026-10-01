using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSvt))]
    [KnownType(typeof(PsdzOrderList))]
    public class PsdzSollverbauung : IPsdzSollverbauung
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AsXml { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSvt Svt { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzOrderList PsdzOrderList { get; set; }
    }
}
