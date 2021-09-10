using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Tal;

namespace PsdzClient.Programming
{
    class TaCategoriesEnumMapper : ProgrammingEnumMapperBase<PsdzTaCategories, TaCategories>
    {
        protected override IDictionary<PsdzTaCategories, TaCategories> CreateMap()
        {
            return new Dictionary<PsdzTaCategories, TaCategories>
            {
                {
                    PsdzTaCategories.BlFlash,
                    TaCategories.BlFlash
                },
                {
                    PsdzTaCategories.CdDeploy,
                    TaCategories.CdDeploy
                },
                {
                    PsdzTaCategories.FscBackup,
                    TaCategories.FscBackup
                },
                {
                    PsdzTaCategories.FscDeploy,
                    TaCategories.FscDeploy
                },
                {
                    PsdzTaCategories.FscDeployPrehwd,
                    TaCategories.FscDeployPrehwd
                },
                {
                    PsdzTaCategories.SFADeploy,
                    TaCategories.SFADeploy
                },
                {
                    PsdzTaCategories.GatewayTableDeploy,
                    TaCategories.GatewayTableDeploy
                },
                {
                    PsdzTaCategories.HddUpdate,
                    TaCategories.HddUpdate
                },
                {
                    PsdzTaCategories.HwDeinstall,
                    TaCategories.HwDeinstall
                },
                {
                    PsdzTaCategories.HwInstall,
                    TaCategories.HwInstall
                },
                {
                    PsdzTaCategories.IbaDeploy,
                    TaCategories.IbaDeploy
                },
                {
                    PsdzTaCategories.IdBackup,
                    TaCategories.IdBackup
                },
                {
                    PsdzTaCategories.IdRestore,
                    TaCategories.IdRestore
                },
                {
                    PsdzTaCategories.SwDeploy,
                    TaCategories.SwDeploy
                },
                {
                    PsdzTaCategories.Unknown,
                    TaCategories.Unknown
                },
                {
                    PsdzTaCategories.EcuActivate,
                    TaCategories.EcuActivate
                },
                {
                    PsdzTaCategories.EcuPoll,
                    TaCategories.EcuPoll
                },
                {
                    PsdzTaCategories.EcuMirrorDeploy,
                    TaCategories.EcuMirrorDeploy
                }
            };
        }
    }
}
