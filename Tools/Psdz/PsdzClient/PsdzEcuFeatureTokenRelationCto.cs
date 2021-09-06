using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzFeatureIdCto))]
    [DataContract]
    public class PsdzEcuFeatureTokenRelationCto : IPsdzEcuFeatureTokenRelationCto
    {
        [DataMember]
        public IPsdzEcuIdentifier ECUIdentifier { get; set; }

        [DataMember]
        public PsdzFeatureGroupEtoEnum FeatureGroup { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureId { get; set; }

        [DataMember]
        public string TokenId { get; set; }
    }
}
