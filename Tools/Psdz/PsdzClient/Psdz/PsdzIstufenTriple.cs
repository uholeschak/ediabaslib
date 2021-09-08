using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzIstufenTriple : IPsdzIstufenTriple
    {
        [DataMember]
        public string Current { get; set; }

        [DataMember]
        public string Last { get; set; }

        [DataMember]
        public string Shipment { get; set; }
    }
}
