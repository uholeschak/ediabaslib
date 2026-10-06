
namespace BMW.Rheingold.CoreFramework.Contracts.Programming
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