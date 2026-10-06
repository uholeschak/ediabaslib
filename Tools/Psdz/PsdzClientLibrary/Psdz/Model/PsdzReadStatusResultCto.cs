using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [KnownType(typeof(PsdzFeatureLongStatusCto))]
    public class PsdzReadStatusResultCto : IPsdzReadStatusResultCto
    {
        [DataMember]
        public IList<IPsdzEcuFailureResponseCto> Failures { get; set; }

        [DataMember]
        public IList<IPsdzFeatureLongStatusCto> FeatureStatusSet { get; set; }
    }
}