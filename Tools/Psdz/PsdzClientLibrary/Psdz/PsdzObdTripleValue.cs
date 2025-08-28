using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Obd
{
    [DataContract]
    public class PsdzObdTripleValue : IPsdzObdTripleValue
    {
        [DataMember]
        public string CalId { get; set; }

        [DataMember]
        public string ObdId { get; set; }

        [DataMember]
        public string SubCVN { get; set; }
    }
}
