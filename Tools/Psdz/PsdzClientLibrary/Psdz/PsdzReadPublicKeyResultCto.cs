using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [KnownType(typeof(PsdzKdsPublicKeyResultCto))]
    public class PsdzReadPublicKeyResultCto : IPsdzReadPublicKeyResultCto
    {
        [DataMember]
        public IPsdzKdsFailureResponseCto FailureResponse { get; set; }

        [DataMember]
        public IList<IPsdzKdsPublicKeyResultCto> KdsPublicKeys { get; set; }
    }
}
