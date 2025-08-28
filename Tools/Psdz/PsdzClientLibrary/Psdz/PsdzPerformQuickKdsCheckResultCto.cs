using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [DataContract]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [KnownType(typeof(PsdzKdsIdCto))]
    [KnownType(typeof(PsdzKdsQuickCheckResultCto))]
    public class PsdzPerformQuickKdsCheckResultCto : IPsdzPerformQuickKdsCheckResultCto
    {
        [DataMember]
        public PsdzKdsActionStatusEto KdsActionStatus { get; set; }

        [DataMember]
        public IPsdzKdsFailureResponseCto KdsFailureResponse { get; set; }

        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }

        [DataMember]
        public IList<IPsdzKdsQuickCheckResultCto> KdsQuickCheckResult { get; set; }

        [DataMember]
        public long ActionErrorCode { get; set; }
    }
}
