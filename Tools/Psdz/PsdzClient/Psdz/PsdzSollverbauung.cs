using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [DataContract]
    [KnownType(typeof(PsdzSvt))]
    [KnownType(typeof(PsdzOrderList))]
    public class PsdzSollverbauung : IPsdzSollverbauung
    {
        [DataMember]
        public string AsXml { get; set; }

        [DataMember]
        public IPsdzSvt Svt { get; set; }

        [DataMember]
        public IPsdzOrderList PsdzOrderList { get; set; }
    }
}
