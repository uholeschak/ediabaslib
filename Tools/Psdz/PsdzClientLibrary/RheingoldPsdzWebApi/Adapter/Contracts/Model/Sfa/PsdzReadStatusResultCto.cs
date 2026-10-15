using System.Collections.Generic;
using System.Runtime.Serialization;

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