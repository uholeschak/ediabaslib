using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzTaCategories
    {
        BlFlash,
        CdDeploy,
        FscBackup,
        FscDeploy,
        FscDeployPrehwd,
        GatewayTableDeploy,
        HddUpdate,
        HwDeinstall,
        HwInstall,
        IbaDeploy,
        IdBackup,
        IdRestore,
        SwDeploy,
        SFADeploy,
        Unknown,
        EcuActivate,
        EcuPoll,
        EcuMirrorDeploy
    }

	public interface IPsdzTalLine : IPsdzTalElement
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        PsdzFscDeploy FscDeploy { get; }

        PsdzBlFlash BlFlash { get; }

        PsdzIbaDeploy IbaDeploy { get; }

        PsdzSwDeploy SwDeploy { get; }

        PsdzIdRestore IdRestore { get; }

        PsdzSFADeploy SFADeploy { get; }

        PsdzTaCategories TaCategories { get; }

        IPsdzTaCategory TaCategory { get; }
    }
}
