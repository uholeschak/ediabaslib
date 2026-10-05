using BMW.Rheingold.CoreFramework;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
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