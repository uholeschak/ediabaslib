using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzFeatureConditionCto))]
    [KnownType(typeof(PsdzFeatureIdCto))]
    public class PsdzFeatureLongStatusCto : IPsdzFeatureLongStatusCto
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifierCto { get; set; }

        [DataMember]
        public IList<IPsdzFeatureConditionCto> FeatureConditions { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [DataMember]
        public PsdzFeatureStatusEtoEnum FeatureStatusEto { get; set; }

        [DataMember]
        public int MileageOfActivation { get; set; }

        [DataMember]
        public DateTime TimeOfActivation { get; set; }

        [DataMember]
        public string TokenId { get; set; }

        [DataMember]
        public PsdzValidationStatusEtoEnum ValidationStatusEto { get; set; }
    }
}
