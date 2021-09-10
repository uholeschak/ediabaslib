using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [KnownType(typeof(PsdzSecureTokenForTal))]
    [DataContract]
    public class PsdzSFAWriteTA : PsdzTa, IPsdzFsaTa, IPsdzTa, IPsdzTalElement
    {
        [DataMember]
        public long EstimatedExecutionTime { get; set; }

        [DataMember]
        public long FeatureId { get; set; }

        [DataMember]
        public IPsdzSecureTokenForTal SecureToken { get; set; }
    }
}
