using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzEcuIdentifier))]
    [DataContract]
    public class PsdzEcuFailureResponse
    {
        [DataMember]
        public IPsdzEcuIdentifier Ecu { get; set; }

        [DataMember]
        public string Reason { get; set; }
    }
}
