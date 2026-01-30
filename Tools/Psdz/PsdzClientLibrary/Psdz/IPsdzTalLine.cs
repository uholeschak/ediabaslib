using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    public interface IPsdzTalLine : IPsdzTalElement
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        PsdzFscDeploy FscDeploy { get; }

        PsdzBlFlash BlFlash { get; }

        PsdzIbaDeploy IbaDeploy { get; }

        PsdzSwDeploy SwDeploy { get; }

        PsdzIdRestore IdRestore { get; }

        PsdzSFADeploy SFADeploy { get; }

        PsdzIdBackup IdBackup { get; }

        PsdzFscBackup FscBackup { get; }

        PsdzHddUpdate HddUpdate { get; }

        PsdzSmacTransferStart SmacTransferStart { get; }

        PsdzSmacTransferStatus SmacTransferStatus { get; }

        PsdzEcuMirrorDeploy EcuMirrorDeploy { get; }

        PsdzEcuActivate EcuActivate { get; }

        PsdzEcuPoll EcuPoll { get; }

        PsdzTaCategories TaCategories { get; }

        IPsdzTaCategory TaCategory { get; }
    }
}
