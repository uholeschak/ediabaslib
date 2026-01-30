using PsdzClient.Core;

namespace PsdzClient.Programming
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

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum SwtActionType
    {
        ActivateStore,
        ActivateUpdate,
        ActivateUpgrade,
        Deactivate,
        ReturnState,
        WriteVin
    }

    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum SwtType
    {
        Full,
        Light,
        PreEnabFull,
        PreEnabLight,
        Short,
        Unknown
    }

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
