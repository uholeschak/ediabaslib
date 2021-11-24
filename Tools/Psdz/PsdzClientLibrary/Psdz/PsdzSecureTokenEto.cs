using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzFeatureIdCto))]
    [DataContract]
    public class PsdzSecureTokenEto : IPsdzSecureTokenEto
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureIdCto { get; set; }

        [DataMember]
        public string SerializedSecureToken { get; set; }

        [DataMember]
        public string TokenId { get; set; }
    }
}
