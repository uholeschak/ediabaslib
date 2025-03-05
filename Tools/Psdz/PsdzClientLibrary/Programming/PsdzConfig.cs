using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using BMW.Rheingold.Psdz;
using PsdzClientLibrary.Core;

namespace PsdzClient.Programming
{
    public class PsdzConfig
    {
        public string PsdzServiceHostLogFilePath { get; }

        public string PsdzLogFilePath { get; }

        public string ClientLogPath { get; private set; }

        public string HostPath { get; }

        public string PsdzServiceHostLogDir { get; }

        public PsdzServiceArgs PsdzServiceArgs { get; }

        public PsdzConfig(string istaFolder, string dealerId)
        {
            string psdzHostSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\hostx64" : @"PSdZ\host";
            HostPath = Path.Combine(istaFolder, psdzHostSubDir);
            PsdzServiceHostLogDir = Path.Combine(istaFolder, @"logs\client");
            ClientLogPath = PsdzServiceHostLogDir;
            PsdzServiceHostLogFilePath = Path.Combine(PsdzServiceHostLogDir, @"PsdzServiceHost.log");
            PsdzLogFilePath = Path.Combine(PsdzServiceHostLogDir, @"psdz.log");
            if (!Directory.Exists(PsdzServiceHostLogDir))
            {
                Directory.CreateDirectory(PsdzServiceHostLogDir);
            }
            PsdzServiceArgs = CreateServiceArgs(istaFolder, PsdzLogFilePath, dealerId);
            Log.Info("PsdzConfig.PsdzConfig()", "Hostpath:               {0}", HostPath);
            Log.Info("PsdzConfig.PsdzConfig()", "PSdZ Logging directory: {0}", PsdzServiceHostLogDir);
            Log.Info("PsdzConfig.PsdzConfig()", "PsdzServiceArgs: \n{0}", PsdzServiceArgs);
        }

        private static PsdzServiceArgs CreateServiceArgs(string istaFolder, string psdzLogFilePath, string dealerId)
        {
            string psdzSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\binx64" : @"PSdZ\bin";
            string psdzBinaryPath = Path.Combine(istaFolder, psdzSubDir);
            PsdzServiceArgs psdzServiceArgs = new PsdzServiceArgs();
            psdzServiceArgs.JrePath = GetJrePath(istaFolder);
            psdzServiceArgs.JvmOptions = GetPsdzJvmOptions(psdzBinaryPath, psdzLogFilePath);
            psdzServiceArgs.PsdzBinaryPath = psdzBinaryPath;
            psdzServiceArgs.PsdzDataPath = Path.Combine(istaFolder, @"PSdZ\data_swi");
            psdzServiceArgs.EdiabasBinPath = Path.Combine(istaFolder, @"Ediabas\BIN");
            psdzServiceArgs.IsTestRun = false;
            psdzServiceArgs.IdleTimeout = ConfigSettings.getConfigint("BMW.Rheingold.Programming.PsdzService.HostIdleTimeout", 10000);
            psdzServiceArgs.ClientConfigArgs = new ClientConfigArgs();
            psdzServiceArgs.ClientConfigArgs.DealerID = dealerId;
            return psdzServiceArgs;
        }

        private static string[] GetPsdzJvmOptions(string psdzBinaryPath, string psdzLogFilePath)
        {
            int num = 1024;
            if (Environment.Is64BitOperatingSystem)
            {
                int totalPhysicalMemoryInGb = GetTotalPhysicalMemoryInGb();
                int configint = ConfigSettings.getConfigint("BMW.Rheingold.CoreFramework.ParallelOperationsLimit", -1);
                num = ((totalPhysicalMemoryInGb <= 8 || configint <= 3) ? ((int)((double)num * 2.5)) : (512 + 512 * configint));
            }

            List<string> list = new List<string>();
            list.Add("-XX:ThreadStackSize=1024");
            list.Add(string.Format(CultureInfo.InvariantCulture, "-Xmx{0}m", num));
            list.Add("-XX:+UseG1GC");
            list.Add("-XX:MaxGCPauseMillis=50");
            list.Add("-Dcom.sun.management.jmxremote");
            list.Add("-Djava.endorsed.dirs=${PSDZ_BIN_PATH}\\endorsed");
            list.Add("-Djava.library.path='${PSDZ_BIN_PATH}\\prodias; ${PSDZ_BIN_PATH}\\psdz'");
            string defaultValue = string.Join(" ", list);
            string text = defaultValue.Replace("${PSDZ_BIN_PATH}", psdzBinaryPath);
            Log.Info("PsdzConfig.GetPsdzJvmOptions()", "{0}: {1}", "BMW.Rheingold.Programming.PsdzJvmOptions", text);
            return new List<string>(Regex.Split(text, "\\s+(?=\\-)"))
            {
                "-DPsdzLogFilePath=" + psdzLogFilePath,
                "-Dlog4j.configurationFile=psdz-log4j2-config.xml"
            }.ToArray();
        }

        private static int GetTotalPhysicalMemoryInGb()
        {
            try
            {
                using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = new ManagementObjectSearcher(new ObjectQuery("SELECT * FROM Win32_OperatingSystem")).Get().GetEnumerator())
                {
                    if (managementObjectEnumerator.MoveNext())
                    {
                        return int.Parse(((ManagementObject)managementObjectEnumerator.Current)["TotalVisibleMemorySize"].ToString()) / 1024 / 1024;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("PsdzConfig.GetTotalPhysicalMemoryInGb()", exception);
            }
            return 0;
        }

        public static string GetJrePath(string istaFolder)
        {
            bool configStringAsBoolean = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.PsdzWebservice.Enabled", defaultValue: false);
            string text = (configStringAsBoolean ? "WebService\\" : string.Empty);
            string defaultValue = (Environment.Is64BitOperatingSystem ? (text + "OpenJREx64") : (text + "OpenJREx86"));
            string configPath = ConfigSettings.getPathString(configStringAsBoolean ? "BMW.Rheingold.Programming.PsdzJrePath.WebService" : "BMW.Rheingold.Programming.PsdzJrePath", string.Empty);
            if (!string.IsNullOrEmpty(configPath))
            {
                return Path.GetFullPath(configPath);
            }
            return Path.Combine(istaFolder, defaultValue);
        }

    }
}
