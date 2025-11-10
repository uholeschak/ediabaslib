using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum TaCategories
    {
        BlFlash,
        CdDeploy,
        FscBackup,
        FscDeploy,
        FscDeployPrehwd,
        SFADeploy,
        GatewayTableDeploy,
        HddUpdate,
        HwDeinstall,
        HwInstall,
        IbaDeploy,
        IdBackup,
        IdRestore,
        SwDeploy,
        Unknown,
        IdDelete,
        EcuActivate,
        EcuPoll,
        EcuMirrorDeploy,
        SmacTransferStart,
        SmacTransferStatus
    }
}