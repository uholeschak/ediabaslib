using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public enum PsdzRootCertificateState
    {
        Accepted,
        Invalid,
        NotAvailable,
        Rejected
    }

    public enum PsdzSoftwareSigState
    {
        Accepted,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }

    public interface IPsdzSwtEcu
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        PsdzRootCertificateState RootCertState { get; }

        PsdzSoftwareSigState SoftwareSigState { get; }

        IEnumerable<IPsdzSwtApplication> SwtApplications { get; }

        string Vin { get; }
    }
}
