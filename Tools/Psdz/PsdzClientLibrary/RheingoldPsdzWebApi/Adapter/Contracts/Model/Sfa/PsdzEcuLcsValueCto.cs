using PsdzClient;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzEcuLcsValueCto : IPsdzEcuLcsValueCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int LcsNumber { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int LcsValue { get; set; }
    }
}
