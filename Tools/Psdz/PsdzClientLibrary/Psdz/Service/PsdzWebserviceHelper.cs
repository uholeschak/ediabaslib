using System;
using System.Diagnostics;
using PsdzClient.Core;

namespace BMW.Rheingold.Psdz
{
    internal static class PsdzWebserviceHelper
    {
        internal static void AddServiceCodeToFastaProtocol(string serviceCode, string message)
        {
            if (ServiceLocator.Current.TryGetService<IFasta2Service>(out var service))
            {
                service.AddServiceCode(serviceCode, message, LayoutGroup.P, allowMultipleEntries: false, bufferIfSessionNotStarted: true);
            }
        }

        internal static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max)
            {
                return s;
            }
            return s.Substring(0, max) + "...";
        }

        internal static bool TryKillTree(Process proc, int timeoutMs = 10000)
        {
            if (proc == null)
            {
                return true;
            }
            try
            {
                proc.Refresh();
                if (proc.HasExited)
                {
                    return true;
                }
                using (Process process = Process.Start(new ProcessStartInfo
                       {
                           FileName = "taskkill",
                           Arguments = $"/PID {proc.Id} /T /F",
                           UseShellExecute = false,
                           CreateNoWindow = true,
                           RedirectStandardOutput = true,
                           RedirectStandardError = true
                       }))
                {
                    process?.WaitForExit(timeoutMs);
                }
                proc.Refresh();
                if (proc.HasExited)
                {
                    return true;
                }
                proc.Kill();
                return proc.WaitForExit(Math.Max(3000, timeoutMs / 3));
            }
            catch
            {
                try
                {
                    proc.Refresh();
                    return proc.HasExited;
                }
                catch
                {
                    return false;
                }
            }
            finally
            {
                try
                {
                    proc.Close();
                }
                catch
                {
                }
            }
        }
    }
}