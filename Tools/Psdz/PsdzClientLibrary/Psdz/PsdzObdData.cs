using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Obd
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzObdTripleValue))]
    public class PsdzObdData : IPsdzObdData
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzObdTripleValue> ObdTripleValues { get; set; }
    }
}
