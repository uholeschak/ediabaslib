using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
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
