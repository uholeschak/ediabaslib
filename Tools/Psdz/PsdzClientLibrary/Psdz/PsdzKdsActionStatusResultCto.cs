using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    public class PsdzKdsActionStatusResultCto : IPsdzKdsActionStatusResultCto
    {
        [DataMember]
        public PsdzKdsActionStatusEto KdsActionStatus { get; set; }

        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }
    }
}
