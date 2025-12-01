using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum RootCertificateState
    {
        Accepted,
        Invalid,
        NotAvailable,
        Rejected
    }
}