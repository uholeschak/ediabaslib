using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.SecurityManagement;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzFeatureIdCto))]
    [KnownType(typeof(PsdzEcuUidCto))]
    [KnownType(typeof(PsdzValidityConditionCto))]
    [KnownType(typeof(PsdzFeatureSpecificFieldCto))]
    public class PsdzFeatureRequestCto : IPsdzFeatureRequestCto
    {
        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [DataMember]
        public PsdzSfaLinkTypeEtoEnum SfaLinkType { get; set; }

        [DataMember]
        public IPsdzEcuUidCto EcuUid { get; set; }

        [DataMember]
        public IList<IPsdzValidityConditionCto> ValidityConditions { get; set; }

        [DataMember]
        public IList<IPsdzFeatureSpecificFieldCto> FeatureSpecificFields { get; set; }

        [DataMember]
        public int EnableType { get; set; }
    }
}
