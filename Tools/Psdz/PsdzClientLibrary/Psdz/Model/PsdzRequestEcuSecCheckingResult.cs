using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Certificate;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzRequestEcuSecCheckingResult
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<PsdzEcuFailureResponse> FailedEcus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, int> EcuSecCheckingMaxWaitingTimes { get; set; }
    }
}
