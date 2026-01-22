using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa
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
