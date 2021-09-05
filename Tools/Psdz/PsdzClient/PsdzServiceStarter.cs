using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzClient
{
    class PsdzServiceStarter
    {
        public enum PsdzServiceStartResult
        {
            PsdzStillRunning,
            PsdzStartFailed,
            PsdzStartFailedMemError,
            PsdzStartOk
        }

        private const string PsdzServiceStarterMutex = "Global\\PsdzServiceStarterMutex";

        // Token: 0x0400018E RID: 398
        private const string HostReadyEventName = "Global\\PsdzServiceHostReadyEvent";

        // Token: 0x0400018F RID: 399
        private const string HostFailedEventName = "Global\\PsdzServiceHostFailedEvent";

        // Token: 0x04000190 RID: 400
        private const string HostFailedEventMemErrorName = "Global\\PsdzServiceHostFailedMemError";

        // Token: 0x04000191 RID: 401
        private const string PsdzServiceHostProcessName = "PsdzServiceHost";

        // Token: 0x04000192 RID: 402
        private readonly string psdzHostDir;

        // Token: 0x04000193 RID: 403
        private readonly PsdzServiceArgs psdzServiceArgs;

        // Token: 0x04000194 RID: 404
        private readonly string psdzServiceHostLogDir;

        public PsdzServiceStarter(string psdzHostDir, string psdzServiceHostLogDir, PsdzServiceArgs psdzServiceArgs)
        {
            if (psdzHostDir == null)
            {
                throw new ArgumentNullException("psdzHostDir");
            }
            if (psdzServiceHostLogDir == null)
            {
                throw new ArgumentNullException("psdzServiceHostLogDir");
            }
            if (psdzServiceArgs == null)
            {
                throw new ArgumentNullException("psdzServiceArgs");
            }
            this.psdzHostDir = psdzHostDir;
            this.psdzServiceHostLogDir = psdzServiceHostLogDir;
            this.psdzServiceArgs = psdzServiceArgs;
        }

        public static bool IsServerInstanceRunning()
        {
            bool result;
            using (Mutex mutex = new Mutex(false, PsdzServiceStarterMutex))
            {
                try
                {
                    mutex.WaitOne();
                    result = IsPsdzServiceHostRunning();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return result;
        }

        private static bool IsPsdzServiceHostRunning()
        {
            return Process.GetProcessesByName(PsdzServiceHostProcessName).Any<Process>();
        }

        public PsdzServiceStartResult StartIfNotRunning()
        {
            PsdzServiceStartResult result;
            using (Mutex mutex = new Mutex(false, PsdzServiceStarterMutex))
            {
                try
                {
                    mutex.WaitOne();
                    if (PsdzServiceStarter.IsPsdzServiceHostRunning())
                    {
                        result = PsdzServiceStartResult.PsdzStillRunning;
                    }
                    else
                    {
                        result = this.StartServerInstance();
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return result;
        }

        private PsdzServiceStartResult StartServerInstance()
        {
            string tempFileName = Path.GetTempFileName();
            PsdzServiceArgs.Serialize(tempFileName, this.psdzServiceArgs);
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = Path.Combine(this.psdzHostDir, string.Format(CultureInfo.InvariantCulture, "{0}.exe", PsdzServiceHostProcessName));
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardInput = false;
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.RedirectStandardError = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Environment["PSDZSERVICEHOST_LOGDIR"] = this.psdzServiceHostLogDir;
            processStartInfo.Arguments = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", tempFileName);
            EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, HostReadyEventName);
            EventWaitHandle eventWaitHandle2 = new EventWaitHandle(false, EventResetMode.AutoReset, HostFailedEventName);
            EventWaitHandle eventWaitHandle3 = new EventWaitHandle(false, EventResetMode.AutoReset, HostFailedEventMemErrorName);
            Process.Start(processStartInfo);
            int waitIndex = WaitHandle.WaitAny(new WaitHandle[]
            {
                eventWaitHandle,
                eventWaitHandle2,
                eventWaitHandle3
            }, new TimeSpan(0, 0, 5, 0));

            File.Delete(tempFileName);

            switch (waitIndex)
            {
                case 0:
                    return PsdzServiceStartResult.PsdzStartOk;
                case 1:
                    return PsdzServiceStartResult.PsdzStartFailed;
                case 2:
                    return PsdzServiceStartResult.PsdzStartFailedMemError;
                default:
                    return PsdzServiceStartResult.PsdzStartFailed;
            }
        }

	}
}
