using BMW.Rheingold.Psdz.Model.Tal.TalStatus;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzFailureCause))]
    public class PsdzTalElement : IPsdzTalElement
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public DateTime EndTime { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzTaExecutionState? ExecutionState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzFailureCause> FailureCauses { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool HasFailureCauses { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public Guid Id { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public DateTime StartTime { get; set; }
    }
}
