using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [DataContract]
    [KnownType(typeof(PsdzEcuLcsValueCto))]
    public class PsdzReadLcsResultCto : IPsdzReadLcsResultCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzEcuLcsValueCto> EcuLcsValues { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; set; }
    }
}
