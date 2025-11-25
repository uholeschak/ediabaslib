using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using BMW.Rheingold.Psdz;
using EdiabasLib;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    // ToDo: Check on update
    public class PsdzConfig
    {
        public string PsdzServiceHostLogFilePath { get; }

        public string PsdzLogFilePath { get; }

        public string ClientLogPath { get; private set; }

        public string HostPath { get; }

        public string PsdzServiceHostLogDir { get; }

        public PsdzServiceArgs PsdzServiceArgs { get; }

        [PreserveSource(Hint = "Modified")]
        public PsdzConfig(string istaFolder, string dealerId)
        {
            string psdzHostSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\hostx64" : @"PSdZ\host";
            HostPath = Path.Combine(istaFolder, psdzHostSubDir);
            string logsDir = Path.Combine(istaFolder, "logs");
            PsdzServiceHostLogDir = Path.Combine(logsDir, "client");
            ClientLogPath = PsdzServiceHostLogDir;
            PsdzServiceHostLogFilePath = Path.Combine(PsdzServiceHostLogDir, @"PsdzServiceHost.log");
            PsdzLogFilePath = Path.Combine(PsdzServiceHostLogDir, @"psdz.log");

            if (!EdiabasNet.IsDirectoryWritable(logsDir))
            {
                throw new UnauthorizedAccessException($"Directory is write protected: '{logsDir}'");
            }

            if (!Directory.Exists(PsdzServiceHostLogDir))
            {
                Directory.CreateDirectory(PsdzServiceHostLogDir);
            }

            if (!EdiabasNet.IsDirectoryWritable(PsdzServiceHostLogDir))
            {
                throw new UnauthorizedAccessException($"Directory is write protected: '{PsdzServiceHostLogDir}'");
            }

            PsdzServiceArgs = CreateServiceArgs(false, istaFolder, PsdzLogFilePath, dealerId);
            Log.Info("PsdzConfig.PsdzConfig()", "Hostpath:               {0}", HostPath);
            Log.Info("PsdzConfig.PsdzConfig()", "PSdZ Logging directory: {0}", PsdzServiceHostLogDir);
            Log.Info("PsdzConfig.PsdzConfig()", "PsdzServiceArgs: \n{0}", PsdzServiceArgs);
        }

        [PreserveSource(Hint = "Modified")]
        private static PsdzServiceArgs CreateServiceArgs(bool? isTestRun, string istaFolder, string psdzLogFilePath = null, string dealerId = null)
        {
            string psdzPath = Psdz64BitPathResolver.GetPsdzPath(istaFolder);
            PsdzServiceArgs psdzServiceArgs = new PsdzServiceArgs();
            psdzServiceArgs.JrePath = Psdz64BitPathResolver.GetJrePath(istaFolder);
            psdzServiceArgs.JvmOptions = GetPsdzJvmOptions(psdzPath, psdzLogFilePath);
            psdzServiceArgs.PsdzBinaryPath = psdzPath;
            psdzServiceArgs.PsdzDataPath = Path.Combine(istaFolder, @"PSdZ\data_swi");
            psdzServiceArgs.EdiabasBinPath = Path.Combine(istaFolder, @"Ediabas\BIN");
            psdzServiceArgs.IsTestRun = (isTestRun.HasValue ? isTestRun.Value : false);
            psdzServiceArgs.IdleTimeout = ConfigSettings.getConfigint("BMW.Rheingold.Programming.PsdzService.HostIdleTimeout", 10000);  // [UH] [IGNORE] timeout modified
            psdzServiceArgs.ClientConfigArgs = new ClientConfigArgs();
            psdzServiceArgs.ClientConfigArgs.DealerID = dealerId;
            return psdzServiceArgs;
        }

        [PreserveSource(Hint = "Modified")]
        private static string[] GetPsdzJvmOptions(string psdzBinaryPath, string psdzLogFilePath)
        {
            int num = 1024;
            if (Environment.Is64BitOperatingSystem) // [UH] [IGNORE] replaced
            {
                int totalPhysicalMemoryInGb = GetTotalPhysicalMemoryInGb();
                int configint = ConfigSettings.getConfigint("BMW.Rheingold.CoreFramework.ParallelOperationsLimit", -1);
                num = ((totalPhysicalMemoryInGb <= 8 || configint <= 3) ? ((int)((double)num * 2.5)) : (512 + 512 * configint));
            }

            string item = string.Format(CultureInfo.InvariantCulture, "-Xmx{0}m", num);
            List<string> values = new List<string> { "-XX:ThreadStackSize=1024", item, "-XX:+UseG1GC", "-XX:MaxGCPauseMillis=50", "-Dcom.sun.management.jmxremote", "-Djava.endorsed.dirs=${PSDZ_BIN_PATH}\\endorsed", "-Djava.library.path='${PSDZ_BIN_PATH}\\prodias; ${PSDZ_BIN_PATH}\\psdz'" };
            string defaultValue = string.Join(" ", values);
            string text = defaultValue.Replace("${PSDZ_BIN_PATH}", psdzBinaryPath);
            Log.Info("PsdzConfig.GetPsdzJvmOptions()", "{0}: {1}", "BMW.Rheingold.Programming.PsdzJvmOptions", text);
            List<string> list = new List<string>(Regex.Split(text, "\\s+(?=\\-)"));
            list.Add("-DPsdzLogFilePath=" + psdzLogFilePath);
            list.Add("-Dlog4j.configurationFile=psdz-log4j2-config.xml");
            string configString = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Truststore.Path", Path.GetFullPath(Path.Combine(psdzBinaryPath, "..\\Security\\cacerts")));
            string configString2 = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Truststore.Type", "jks");
            list.Add("-Djavax.net.ssl.trustStore=" + configString);
            list.Add("-Djavax.net.ssl.trustStoreType=" + configString2);
            Log.Info(Log.CurrentMethod(), "JVM Options for Psdz ServiceHost: " + string.Join(" ", list));
            return list.ToArray();
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
    }
}
