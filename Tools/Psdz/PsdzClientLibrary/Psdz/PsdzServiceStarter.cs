using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using BMW.Rheingold.Psdz;
using PsdzClient;

namespace BMW.Rheingold.Psdz.Client
{
    public class PsdzServiceStarter
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

        // [UH] added
        public static bool IsThisServerInstanceRunning()
        {
            if (ClientContext.EnablePsdzMultiSession())
            {
                return IsServerInstanceRunning(Process.GetCurrentProcess().Id);
            }
            return IsServerInstanceRunning();
        }

        public static bool IsServerInstanceRunning(int istaProcessId = 0)
        {
            bool result;
            using (Mutex mutex = new Mutex(false, PsdzServiceStarterMutex))
            {
                try
                {
                    mutex.WaitOne();
                    result = IsPsdzServiceHostRunning(istaProcessId);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return result;
        }

        private static bool IsPsdzServiceHostRunning(int istaProcessId)
        {
            Process[] processesByName = Process.GetProcessesByName(PsdzServiceHostProcessName);
            if (istaProcessId == 0)
            {
                return processesByName.Any();
            }
            foreach (ManagementObject item in new ManagementObjectSearcher(string.Format("select CommandLine from Win32_Process where Name='{0}.exe'", PsdzServiceHostProcessName)).Get())
            {
                string[] array = item["CommandLine"].ToString().Split(' ');
                if (array.Length == 3)
                {
                    int num2 = int.Parse(array[2]);
                    if (istaProcessId == num2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public PsdzServiceStartResult StartIfNotRunning(int istaProcessId = 0)
        {
            PsdzServiceStartResult result;
            using (Mutex mutex = new Mutex(false, PsdzServiceStarterMutex))
            {
                try
                {
                    mutex.WaitOne();
                    if (PsdzServiceStarter.IsPsdzServiceHostRunning(istaProcessId))
                    {
                        result = PsdzServiceStartResult.PsdzStillRunning;
                    }
                    else
                    {
                        result = this.StartServerInstance(istaProcessId);
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return result;
        }

        private PsdzServiceStartResult StartServerInstance(int istaProcessId)
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
            if (istaProcessId == 0)
            {
                processStartInfo.Arguments = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", tempFileName);
            }
            else
            {
                processStartInfo.Arguments = string.Format(CultureInfo.InvariantCulture, "\"{0}\" {1}", tempFileName, istaProcessId);
            }
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
