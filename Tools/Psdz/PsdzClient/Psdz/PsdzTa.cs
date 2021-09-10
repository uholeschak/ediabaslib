using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzTa : PsdzTalElement, IPsdzTa, IPsdzTalElement
    {
        [DataMember]
        public IPsdzSgbmId SgbmId { get; set; }
    }
}
