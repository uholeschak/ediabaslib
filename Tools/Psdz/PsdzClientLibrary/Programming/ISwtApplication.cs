namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwtApplication : ISwtApplicationReport
    {
        byte[] Fsc { get; }

        byte[] FscCertificate { get; }

        FscCertificateState FscCertificateState { get; }

        bool IsBackupPossible { get; }

        SwtActionType? SwtActionType { get; }

        SwtType SwtType { get; }

        int Position { get; }
    }
}
