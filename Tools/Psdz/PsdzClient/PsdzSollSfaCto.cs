using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFeatureTokenRelationCto))]
    public class PsdzSollSfaCto : IPsdzSollSfaCto
    {
        [DataMember]
        public IEnumerable<IPsdzEcuFeatureTokenRelationCto> SollFeatures { get; set; }
    }
}
