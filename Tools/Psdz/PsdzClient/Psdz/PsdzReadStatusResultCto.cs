using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [KnownType(typeof(PsdzFeatureLongStatusCto))]
    [DataContract]
    public class PsdzReadStatusResultCto : IPsdzReadStatusResultCto
    {
        [DataMember]
        public IList<IPsdzEcuFailureResponseCto> Failures { get; set; }

        [DataMember]
        public IList<IPsdzFeatureLongStatusCto> FeatureStatusSet { get; set; }
    }
}
