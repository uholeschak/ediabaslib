using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzResponse : IPsdzResponse
    {
        [DataMember]
        public string Cause { get; set; }

        [DataMember]
        public object Result { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}
