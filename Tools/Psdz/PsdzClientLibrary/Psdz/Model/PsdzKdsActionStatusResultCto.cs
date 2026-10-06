using PsdzClient;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    public class PsdzKdsActionStatusResultCto : IPsdzKdsActionStatusResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzKdsActionStatusEto KdsActionStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }
    }
}
