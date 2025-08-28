using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzRequestNcdEto : IPsdzRequestNcdEto
    {
        [DataMember]
        public IPsdzSgbmId Btld { get; set; }

        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }
    }
}
