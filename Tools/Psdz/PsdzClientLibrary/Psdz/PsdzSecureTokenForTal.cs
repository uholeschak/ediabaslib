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
    [DataContract]
    public class PsdzSecureTokenForTal : IPsdzSecureTokenForTal
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [DataMember]
        public long FeatureId { get; set; }

        [DataMember]
        public string SerializedSecureToken { get; set; }

        [DataMember]
        public string TokenId { get; set; }
    }
}
