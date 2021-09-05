using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace PsdzClient
{
    class PsdzConfig
    {
        public string PsdzServiceHostLogFilePath { get; }

        public string PsdzLogFilePath { get; }

        public string HostPath { get; }

        public string PsdzServiceHostLogDir { get; }

        public PsdzServiceArgs PsdzServiceArgs { get; }

        public PsdzConfig(string istaFolder, string dealerId)
        {
            string psdzHostSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\hostx64" : @"PSdZ\host";
            HostPath = Path.Combine(istaFolder, psdzHostSubDir);
            PsdzServiceHostLogDir = Path.Combine(istaFolder, @"logs\client");
            PsdzServiceHostLogFilePath = Path.Combine(PsdzServiceHostLogDir, @"PsdzServiceHost.log");
            PsdzLogFilePath = Path.Combine(PsdzServiceHostLogDir, @"psdz.log");
            if (!Directory.Exists(PsdzServiceHostLogDir))
            {
                Directory.CreateDirectory(PsdzServiceHostLogDir);
            }
            PsdzServiceArgs = CreateServiceArgs(istaFolder, PsdzLogFilePath, dealerId);
        }

        private static PsdzServiceArgs CreateServiceArgs(string istaFolder, string psdzLogFilePath, string dealerId)
        {
            string psdzSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\binx64" : @"PSdZ\bin";
            string psdzBinaryPath = Path.Combine(istaFolder, psdzSubDir);
            PsdzServiceArgs psdzServiceArgs = new PsdzServiceArgs();
            string jreSubDir = Environment.Is64BitOperatingSystem ? @"OpenJREx64" : @"OpenJREx86";
            psdzServiceArgs.JrePath = Path.Combine(istaFolder, jreSubDir);
            psdzServiceArgs.JvmOptions = GetPsdzJvmOptions(psdzBinaryPath, psdzLogFilePath);
            psdzServiceArgs.PsdzBinaryPath = psdzBinaryPath;
            psdzServiceArgs.PsdzDataPath = Path.Combine(istaFolder, @"PSdZ\data");
            psdzServiceArgs.EdiabasBinPath = Path.Combine(istaFolder, @"Ediabas\BIN");
            psdzServiceArgs.IsTestRun = false;
            psdzServiceArgs.IdleTimeout = 10000;
            psdzServiceArgs.ClientConfigArgs = new ClientConfigArgs();
            psdzServiceArgs.ClientConfigArgs.DealerID = dealerId;
            return psdzServiceArgs;
        }

        private static string[] GetPsdzJvmOptions(string psdzBinaryPath, string psdzLogFilePath)
        {
            int memory = 1024;
            if (Environment.Is64BitOperatingSystem)
            {
                memory *= 2;
            }

            string defaultValue = string.Join(" ", new List<string>
            {
                "-XX:ThreadStackSize=1024",
                string.Format(CultureInfo.InvariantCulture, "-Xmx{0}m", memory),
                "-XX:+UseG1GC",
                "-XX:MaxGCPauseMillis=50",
                "-Dcom.sun.management.jmxremote",
                "-Djava.endorsed.dirs=${PSDZ_BIN_PATH}\\endorsed",
                "-Djava.library.path='${PSDZ_BIN_PATH}\\prodias; ${PSDZ_BIN_PATH}\\psdz'"
            });

            string text = defaultValue.Replace("${PSDZ_BIN_PATH}", psdzBinaryPath);

            return new List<string>(Regex.Split(text, "\\s+(?=\\-)"))
            {
                "-DPsdzLogFilePath=" + psdzLogFilePath,
                "-Dlog4j.configuration=psdzservicehost.properties"
            }.ToArray();
        }
    }
}
