using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    public class PsdzFeatureIdCto : IPsdzFeatureIdCto
    {
        [DataMember]
        public long Value { get; set; }
    }
}
