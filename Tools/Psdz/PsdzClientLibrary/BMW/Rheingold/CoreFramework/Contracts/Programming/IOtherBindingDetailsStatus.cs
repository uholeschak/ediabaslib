namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IOtherBindingDetailsStatus
    {
        EcuCertCheckingStatus? OtherBindingStatus { get; }

        string RollenName { get; }

        string EcuName { get; }
    }
}