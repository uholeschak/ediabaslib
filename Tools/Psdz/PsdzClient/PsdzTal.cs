using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzTalLine))]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzTalExecutionState))]
    public class PsdzTal : PsdzTalElement, IPsdzTalElement, IPsdzTal
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
    }
}
