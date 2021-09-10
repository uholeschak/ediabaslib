using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzFp : PsdzStandardFp, IPsdzFp, IPsdzStandardFp
    {
        [DataMember]
        public string Baureihenverbund { get; set; }

        [DataMember]
        public string Entwicklungsbaureihe { get; set; }
    }
}
