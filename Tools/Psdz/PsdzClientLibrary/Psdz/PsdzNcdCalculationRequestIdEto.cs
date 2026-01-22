using PsdzClient;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzNcdCalculationRequestIdEto : IPsdzNcdCalculationRequestIdEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string RequestId { get; set; }
    }
}
