using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
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
