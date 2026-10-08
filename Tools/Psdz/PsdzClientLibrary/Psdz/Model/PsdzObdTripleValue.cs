using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Obd
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
