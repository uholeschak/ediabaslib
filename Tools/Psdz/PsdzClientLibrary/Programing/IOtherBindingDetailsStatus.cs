using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IOtherBindingDetailsStatus
    {
        EcuCertCheckingStatus? OtherBindingStatus { get; }

        string RollenName { get; }

        string EcuName { get; }
    }
}
