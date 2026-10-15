using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    public class PsdzFeatureIdCto : IPsdzFeatureIdCto
    {
        [DataMember]
        public long Value { get; set; }
    }
}
