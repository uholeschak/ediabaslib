using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzBindingCalculationRequestId
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int Value { get; set; }
    }
}
