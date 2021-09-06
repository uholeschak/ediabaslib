using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzOtherBindingDetailsStatus
    {
        [DataMember]
        public PsdzEcuCertCheckingStatus? OtherBindingStatus { get; set; }

        [DataMember]
        public string RollenName { get; set; }

        [DataMember]
        public string EcuName { get; set; }
    }
}
