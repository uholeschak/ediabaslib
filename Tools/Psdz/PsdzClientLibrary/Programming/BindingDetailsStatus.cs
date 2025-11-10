using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class BindingDetailsStatus : IBindingDetailsStatus
    {
        public EcuCertCheckingStatus? BindingStatus { get; internal set; }

        public EcuCertCheckingStatus? CertificateStatus { get; internal set; }

        public string RollenName { get; internal set; }
    }
}
