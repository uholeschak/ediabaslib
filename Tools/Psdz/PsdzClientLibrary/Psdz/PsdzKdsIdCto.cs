using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    public class PsdzKdsIdCto : IPsdzKdsIdCto
    {
        [DataMember]
        public string IdAsHex { get; set; }

        [DataMember]
        public int Id { get; set; }
    }
}
