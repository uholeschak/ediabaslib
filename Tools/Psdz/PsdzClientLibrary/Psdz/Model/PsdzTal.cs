using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzTalLine))]
    [KnownType(typeof(PsdzTalExecutionState))]
    [KnownType(typeof(PsdzExecutionTime))]
    public class PsdzTal : PsdzTalElement, IPsdzTal, IPsdzTalElement
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzEcuIdentifier> AffectedEcus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string AsXml { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzEcuIdentifier> InstalledEcuListIst { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzEcuIdentifier> InstalledEcuListSoll { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzTalExecutionState TalExecutionState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember(IsRequired = true)]
        public IEnumerable<IPsdzTalLine> TalLines { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzExecutionTime PsdzExecutionTime { get; set; }
    }
}
