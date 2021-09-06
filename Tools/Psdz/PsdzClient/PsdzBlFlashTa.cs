using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzProtocol))]
    public class PsdzBlFlashTa : PsdzTa
    {
        [DataMember]
        public PsdzProtocol? ActualProtocol { get; set; }

        [DataMember]
        public PsdzProtocol? PreferredProtocol { get; set; }
    }
}
