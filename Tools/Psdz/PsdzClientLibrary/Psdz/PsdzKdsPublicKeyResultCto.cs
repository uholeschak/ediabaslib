using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsPublicKeyResultCto : IPsdzKdsPublicKeyResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] PublicKey { get; set; }
    }
}
