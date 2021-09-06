using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzEcuContextInfo : IPsdzEcuContextInfo
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuId { get; set; }

        [DataMember]
        public DateTime? LastProgrammingDate { get; set; }

        [DataMember]
        public DateTime ManufacturingDate { get; set; }

        [DataMember]
        public int PerformedFlashCycles { get; set; }

        [DataMember]
        public int ProgramCounter { get; set; }

        [DataMember]
        public int RemainingFlashCycles { get; set; }
    }
}
