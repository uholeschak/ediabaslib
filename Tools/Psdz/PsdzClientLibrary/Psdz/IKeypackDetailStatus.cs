namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IKeypackDetailStatus
    {
        EcuCertCheckingStatus? KeyPackStatus { get; }

        string KeyId { get; }
    }
}