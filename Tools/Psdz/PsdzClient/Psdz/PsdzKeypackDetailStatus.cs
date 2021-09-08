using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzKeypackDetailStatus
    {
        [DataMember]
        public PsdzEcuCertCheckingStatus? KeyPackStatus { get; set; }

        [DataMember]
        public string KeyId { get; set; }
    }
}
