using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsPublicKeyResultCto : IPsdzKdsPublicKeyResultCto
    {
        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }

        [DataMember]
        public byte[] PublicKey { get; set; }
    }
}
