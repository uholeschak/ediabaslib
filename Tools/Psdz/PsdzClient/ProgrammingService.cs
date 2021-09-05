using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PsdzClient.PsdzServiceStarter;

namespace PsdzClient
{
    class ProgrammingService
    {
        private readonly PsdzConfig psdzConfig;

        private readonly PsdzServiceStarter pdszService;

        public ProgrammingService(string istaFolder, string dealerId)
        {
            psdzConfig = new PsdzConfig(istaFolder, dealerId);
            pdszService = new PsdzServiceStarter(psdzConfig.HostPath, psdzConfig.PsdzServiceHostLogDir, psdzConfig.PsdzServiceArgs);
        }

        private bool StartPsdzServiceHost()
        {
            PsdzServiceStarter.PsdzServiceStartResult psdzServiceStartResult = pdszService.StartIfNotRunning();
            switch (psdzServiceStartResult)
            {
                case PsdzServiceStartResult.PsdzStillRunning:
                case PsdzServiceStartResult.PsdzStartOk:
                    break;

                case PsdzServiceStartResult.PsdzStartFailedMemError:
                    return false;

                default:
                    return false;
            }

            return WaitForPsdzServiceHostInitialization();
        }

        // Token: 0x060002CE RID: 718 RVA: 0x000187A8 File Offset: 0x000169A8
        private bool WaitForPsdzServiceHostInitialization()
        {
            DateTime t = DateTime.Now.AddSeconds(40f);
            while (!IsServerInstanceRunning())
            {
                if (DateTime.Now > t)
                {
                    return false;
                }
                Thread.Sleep(500);
            }

            return true;
        }
    }
}
