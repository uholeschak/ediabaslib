using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzObdTripleValue))]
    public class PsdzObdData : IPsdzObdData
    {
        [DataMember]
        public IEnumerable<IPsdzObdTripleValue> ObdTripleValues { get; set; }
    }
}
