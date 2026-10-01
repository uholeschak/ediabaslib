using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzKdsIdCto : IPsdzKdsIdCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string IdAsHex { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Id { get; set; }
    }
}
