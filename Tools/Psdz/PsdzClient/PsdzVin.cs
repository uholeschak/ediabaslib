using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzVin : IPsdzVin
    {
        [DataMember]
        public string Value { get; set; }
    }
}
