namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IBindingDetailsStatus
    {
        EcuCertCheckingStatus? BindingStatus { get; }

        EcuCertCheckingStatus? CertificateStatus { get; }

        string RollenName { get; }
    }
}