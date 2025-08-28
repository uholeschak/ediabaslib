using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzTalLine))]
    [KnownType(typeof(PsdzTalExecutionState))]
    [KnownType(typeof(PsdzExecutionTime))]
    public class PsdzTal : PsdzTalElement, IPsdzTal, IPsdzTalElement
    {
        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzEcuIdentifier> AffectedEcus { get; set; }

        [DataMember]
        public string AsXml { get; set; }

        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzEcuIdentifier> InstalledEcuListIst { get; set; }

        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzEcuIdentifier> InstalledEcuListSoll { get; set; }

        [DataMember]
        public PsdzTalExecutionState TalExecutionState { get; set; }

        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzTalLine> TalLines { get; set; }

        [DataMember]
        public IPsdzExecutionTime PsdzExecutionTime { get; set; }
    }
}
