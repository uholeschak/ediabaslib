using System.Runtime.Serialization;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    public class PsdzFeatureIdCto : IPsdzFeatureIdCto
    {
        [DataMember]
        public long Value { get; set; }
    }
}
