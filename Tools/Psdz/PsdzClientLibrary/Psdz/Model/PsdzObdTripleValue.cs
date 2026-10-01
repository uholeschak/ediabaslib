using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Obd
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzObdTripleValue : IPsdzObdTripleValue
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string CalId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string ObdId { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string SubCVN { get; set; }
    }
}
