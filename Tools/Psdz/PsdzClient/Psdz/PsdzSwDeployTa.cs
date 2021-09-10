using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public enum PsdzProtocol
    {
        KWP2000,
        UDS,
        HTTP,
        MIRROR
    }

    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzSwDeployTa : PsdzTa
    {
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
