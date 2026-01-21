using System.Runtime.Serialization;
using PsdzClient;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzBindingCalculationFailure
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Url { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Retry { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Reason { get; set; }
    }
}
