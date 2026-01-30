namespace BMW.Rheingold.Psdz.Model.Swt
{
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
