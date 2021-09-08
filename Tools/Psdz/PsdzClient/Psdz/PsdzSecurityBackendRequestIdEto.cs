using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzSecurityBackendRequestIdEto : IPsdzSecurityBackendRequestIdEto
    {
        [DataMember]
        public int Value { get; set; }
    }
}
