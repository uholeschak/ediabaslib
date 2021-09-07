using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public enum EcuCertCheckingStatus
    {
        CheckStillRunning,
        Empty,
        Incomplete,
        Malformed,
        Ok,
        Other,
        SecurityError,
        Unchecked,
        WrongVin17
    }
    
    public interface IEcuCertCheckingResponse
    {
        IEcuIdentifier Ecu { get; }

        EcuCertCheckingStatus? CertificateStatus { get; }

        EcuCertCheckingStatus? BindingsStatus { get; }

        EcuCertCheckingStatus? OtherBindingsStatus { get; }

        IBindingDetailsStatus[] BindingDetailStatus { get; }

        IOtherBindingDetailsStatus[] OtherBindingDetailStatus { get; }
    }
}
