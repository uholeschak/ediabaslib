using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
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
