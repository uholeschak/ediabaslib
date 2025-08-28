using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzVin : IPsdzVin
    {
        [DataMember]
        public string Value { get; set; }
    }
}
