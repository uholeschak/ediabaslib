using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Tal.TalStatus;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    [KnownType(typeof(PsdzFailureCause))]
    public class PsdzTalElement : IPsdzTalElement
    {
        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public PsdzTaExecutionState? ExecutionState { get; set; }

        [DataMember]
        public IEnumerable<IPsdzFailureCause> FailureCauses { get; set; }

        [DataMember]
        public bool HasFailureCauses { get; set; }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }
    }
}
