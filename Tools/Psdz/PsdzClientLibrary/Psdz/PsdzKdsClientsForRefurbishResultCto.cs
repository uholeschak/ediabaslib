using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsClientsForRefurbishResultCto : IPsdzKdsClientsForRefurbishResultCto
    {
        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [DataMember]
        public IList<IPsdzKdsIdCto> KdsIds { get; set; }
    }
}
