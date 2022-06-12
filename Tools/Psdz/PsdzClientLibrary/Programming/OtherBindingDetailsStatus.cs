using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class OtherBindingDetailsStatus : IOtherBindingDetailsStatus
    {
        public EcuCertCheckingStatus? OtherBindingStatus { get; internal set; }

        public string RollenName { get; internal set; }

        public string EcuName { get; internal set; }
    }
}
