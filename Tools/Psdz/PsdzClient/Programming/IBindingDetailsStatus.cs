using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IBindingDetailsStatus
    {
        EcuCertCheckingStatus? BindingStatus { get; }

        EcuCertCheckingStatus? CertificateStatus { get; }

        string RollenName { get; }
    }
}
