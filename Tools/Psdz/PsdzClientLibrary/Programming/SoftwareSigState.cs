using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum SoftwareSigState
    {
        Accepted,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }
}