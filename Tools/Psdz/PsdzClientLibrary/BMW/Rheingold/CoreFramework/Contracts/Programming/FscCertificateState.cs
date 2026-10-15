namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum FscCertificateState
    {
        Accepted,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }
}
