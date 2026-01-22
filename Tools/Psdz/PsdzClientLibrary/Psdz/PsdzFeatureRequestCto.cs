using BMW.Rheingold.Psdz.Model.SecurityManagement;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzFeatureIdCto))]
    [KnownType(typeof(PsdzEcuUidCto))]
    [KnownType(typeof(PsdzValidityConditionCto))]
    [KnownType(typeof(PsdzFeatureSpecificFieldCto))]
    public class PsdzFeatureRequestCto : IPsdzFeatureRequestCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSfaLinkTypeEtoEnum SfaLinkType { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuUidCto EcuUid { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzValidityConditionCto> ValidityConditions { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzFeatureSpecificFieldCto> FeatureSpecificFields { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int EnableType { get; set; }
    }
}
