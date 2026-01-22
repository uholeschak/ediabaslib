using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzEcuFailureResponse
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier Ecu { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Reason { get; set; }
    }
}
