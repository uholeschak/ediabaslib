using PsdzClient;
using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzEcuContextInfo : IPsdzEcuContextInfo
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier EcuId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public DateTime? LastProgrammingDate { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public DateTime ManufacturingDate { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int PerformedFlashCycles { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int ProgramCounter { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int RemainingFlashCycles { get; set; }
    }
}
