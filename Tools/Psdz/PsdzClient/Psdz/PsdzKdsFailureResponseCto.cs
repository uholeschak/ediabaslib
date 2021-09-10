using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.LocalizableMessageTo;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    [KnownType(typeof(PsdzLocalizableMessageTo))]
    [DataContract]
    [KnownType(typeof(PsdzKdsIdCto))]
    public class PsdzKdsFailureResponseCto : IPsdzKdsFailureResponseCto
    {
        [DataMember]
        public ILocalizableMessageTo Cause { get; set; }

        [DataMember]
        public IPsdzKdsIdCto KdsId { get; set; }
    }
}
