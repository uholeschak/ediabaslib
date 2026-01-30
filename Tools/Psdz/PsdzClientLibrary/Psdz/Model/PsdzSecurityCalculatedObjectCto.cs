using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSecurityCalculatedObjectCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzCertMemoryObject MemoryObject { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSecurityCalculationOverallStatus OverallStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<string, PsdzCertCalculationDetailedStatus> RoleStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<string, PsdzCertCalculationDetailedStatus> KeyIdStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ServicePack { get; set; }
    }
}
