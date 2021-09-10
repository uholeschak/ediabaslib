using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using BMW.Rheingold.Psdz;

namespace BMW.Rheingold.Psdz.Client
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

        private const string HostReadyEventName = "Global\\PsdzServiceHostReadyEvent";

        private const string HostFailedEventName = "Global\\PsdzServiceHostFailedEvent";

        private const string HostFailedEventMemErrorName = "Global\\PsdzServiceHostFailedMemError";

        private const string PsdzServiceHostProcessName = "PsdzServiceHost";

        private readonly string psdzHostDir;

        private readonly PsdzServiceArgs psdzServiceArgs;

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
            PsdzServiceArgs.Serialize(tempFileName, psdzServiceArgs);
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = Path.Combine(psdzHostDir, string.Format(CultureInfo.InvariantCulture, "{0}.exe", PsdzServiceHostProcessName));
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardInput = false;
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.RedirectStandardError = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Environment["PSDZSERVICEHOST_LOGDIR"] = psdzServiceHostLogDir;
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
