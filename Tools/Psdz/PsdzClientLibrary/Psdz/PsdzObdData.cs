using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Obd
{
    [DataContract]
    [KnownType(typeof(PsdzObdTripleValue))]
    public class PsdzObdData : IPsdzObdData
    {
        [DataMember]
        public IEnumerable<IPsdzObdTripleValue> ObdTripleValues { get; set; }
    }
}
