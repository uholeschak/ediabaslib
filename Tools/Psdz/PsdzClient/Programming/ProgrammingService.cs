using System;
using System.Threading;
using PsdzClient.Psdz;

namespace PsdzClient.Programming
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
                case PsdzServiceStarter.PsdzServiceStartResult.PsdzStillRunning:
                case PsdzServiceStarter.PsdzServiceStartResult.PsdzStartOk:
                    break;

                case PsdzServiceStarter.PsdzServiceStartResult.PsdzStartFailedMemError:
                    return false;

                default:
                    return false;
            }

            return WaitForPsdzServiceHostInitialization();
        }

        private bool WaitForPsdzServiceHostInitialization()
        {
            DateTime endTime = DateTime.Now.AddSeconds(40f);
            while (!PsdzServiceStarter.IsServerInstanceRunning())
            {
                if (DateTime.Now > endTime)
                {
                    return false;
                }
                Thread.Sleep(500);
            }

            return true;
        }
    }
}
