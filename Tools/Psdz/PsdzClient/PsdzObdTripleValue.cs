using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzObdTripleValue : IPsdzObdTripleValue
    {
        [DataMember]
        public string CalId { get; set; }

        [DataMember]
        public string ObdId { get; set; }

        [DataMember]
        public string SubCVN { get; set; }
    }
}
