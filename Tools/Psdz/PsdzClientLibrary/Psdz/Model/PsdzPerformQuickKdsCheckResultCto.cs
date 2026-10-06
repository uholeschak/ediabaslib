using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Kds;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Kds
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzKdsFailureResponseCto))]
    [KnownType(typeof(PsdzKdsIdCto))]
    [KnownType(typeof(PsdzKdsQuickCheckResultCto))]
    public class PsdzPerformQuickKdsCheckResultCto : IPsdzPerformQuickKdsCheckResultCto
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

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzKdsQuickCheckResultCto> KdsQuickCheckResult { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long ActionErrorCode { get; set; }
    }
}
