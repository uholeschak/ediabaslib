namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum FscState
    {
        Accepted,
        Cancelled,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }
}
