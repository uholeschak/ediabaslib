namespace BMW.Rheingold.Psdz.Model.Swt
{
    public enum PsdzFscCertificateState
    {
        Accepted,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }

    public enum PsdzFscState
    {
        Accepted,
        Cancelled,
        Imported,
        Invalid,
        NotAvailable,
        Rejected
    }

    public enum PsdzSwtActionType
    {
        ActivateStore,
        ActivateUpdate,
        ActivateUpgrade,
        Deactivate,
        ReturnState,
        WriteVin
    }

    public enum PsdzSwtType
    {
        Full,
        Light,
        PreEnabFull,
        PreEnabLight,
        Short,
        Unknown
    }

    public interface IPsdzSwtApplication
    {
        byte[] Fsc { get; }

        byte[] FscCert { get; }

        bool IsBackupPossible { get; }

        int Position { get; }

        PsdzFscCertificateState FscCertState { get; }

        PsdzFscState FscState { get; }

        PsdzSoftwareSigState? SoftwareSigState { get; }

        PsdzSwtActionType? SwtActionType { get; }

        PsdzSwtType SwtType { get; }

        IPsdzSwtApplicationId SwtApplicationId { get; }
    }
}
