using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Tal.TalFilter
{
    [DataContract]
    public class PsdzTalFilter : IPsdzTalFilter
    {
        [DataMember]
        public string AsXml { get; set; }
    }
}
