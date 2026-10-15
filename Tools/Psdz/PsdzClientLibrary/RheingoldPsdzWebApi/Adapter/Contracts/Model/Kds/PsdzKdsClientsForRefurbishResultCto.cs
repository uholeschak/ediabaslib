using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsClientsForRefurbishResultCto : IPsdzKdsClientsForRefurbishResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzKdsIdCto> KdsIds { get; set; }
    }
}
