using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public enum RootCertificateState
    {
        Accepted,
        Invalid,
        NotAvailable,
        Rejected
    }

    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public enum SoftwareSigState
    {
        Accepted,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }

    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwtEcu
    {
        IEcuIdentifier EcuIdentifier { get; }

        RootCertificateState RootCertificateState { get; }

        SoftwareSigState SoftwareSigState { get; }

        IEnumerable<ISwtApplication> SwtApplications { get; }
    }
}
