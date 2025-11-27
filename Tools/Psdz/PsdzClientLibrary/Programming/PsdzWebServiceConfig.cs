using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using PsdzClient;

namespace BMW.Rheingold.Programming
{
    public class PsdzWebServiceConfig
    {
        private const string DEFAULT_DEALER_ID = "1234";

        public string PsdzDataPath { get; }

        public string EdiabasBinPath { get; }

        public string DealerId { get; }

        public string PsdzWebApiLogDir { get; }

        public string PsdzWebServiceLogFilePath { get; }

        public string PsdzLogFilePath { get; }

        public string PsdzWebServiceTomcatLogFilePath { get; }

        public string PsdzWebServiceSpringbootLogFilePath { get; }

        public string ProdiasDriverLogFilePath { get; set; }

        public string[] JvmOptions { get; }

        public string[] JarArguments { get; }

        [PreserveSource(Hint = "istaFolder added")]
        public PsdzWebServiceConfig(string istaFolder, string dealerId = null)
        {
            // [UH] [IGNORE] replaced logs dir
            string logsDir = Path.Combine(istaFolder, "logs");
            PsdzWebApiLogDir = Path.Combine(logsDir, "webclient");
            if (!Directory.Exists(PsdzWebApiLogDir))
            {
                Directory.CreateDirectory(PsdzWebApiLogDir);
            }
            PsdzWebServiceLogFilePath = Path.Combine(PsdzWebApiLogDir, "PsdzWebservice.log");
            PsdzLogFilePath = Path.Combine(PsdzWebApiLogDir, "psdz.log");
            PsdzWebServiceTomcatLogFilePath = Path.Combine(PsdzWebApiLogDir, "tomcat.log");
            PsdzWebServiceSpringbootLogFilePath = Path.Combine(PsdzWebApiLogDir, "springboot.log");
            ProdiasDriverLogFilePath = Path.Combine(PsdzWebApiLogDir, "prodias_driver.log");
            if (!File.Exists(PsdzWebServiceLogFilePath))
            {
                File.Create(PsdzWebServiceLogFilePath).Close();
            }
            if (!File.Exists(PsdzLogFilePath))
            {
                File.Create(PsdzLogFilePath).Close();
            }
            if (!File.Exists(PsdzWebServiceSpringbootLogFilePath))
            {
                File.Create(PsdzWebServiceSpringbootLogFilePath).Close();
            }
            if (!File.Exists(ProdiasDriverLogFilePath))
            {
                File.Create(ProdiasDriverLogFilePath).Close();
            }
            // [UH] [IGNORE] modified
            PsdzDataPath = Path.Combine(istaFolder, @"PSdZ\data_swi");
            EdiabasBinPath = Path.Combine(istaFolder, @"Ediabas\BIN");
            if (EdiabasBinPath == null)
            {
                EdiabasBinPath = "none";
                Log.Warning(Log.CurrentMethod(), "Ediabas Bin Path was null!");
            }
            DealerId = convertDealerIdToHex(dealerId);
            string text = "EC25F6127D1D02E37827F68A0DC41F3341A9F8C63C96EB970BCDCDEA70E619A8";
            JvmOptions = GetPsdzJvmOptions();
            JarArguments = new string[9] { PsdzDataPath, EdiabasBinPath, DealerId, PsdzWebServiceLogFilePath, PsdzLogFilePath, PsdzWebServiceTomcatLogFilePath, text, PsdzWebServiceSpringbootLogFilePath, ProdiasDriverLogFilePath };
            Log.Info(Log.CurrentMethod(), "Psdz Webservice log file:".PadRight(40) + "{0}", PsdzWebServiceLogFilePath);
            Log.Info(Log.CurrentMethod(), "Psdz log file:".PadRight(40) + "{0}", PsdzLogFilePath);
            Log.Info(Log.CurrentMethod(), "Psdz Webservice tomcat log file:".PadRight(40) + "{0}", PsdzWebServiceTomcatLogFilePath);
            Log.Info(Log.CurrentMethod(), "Psdz Webservice springboot log file:".PadRight(40) + "{0}", PsdzWebServiceSpringbootLogFilePath);
            Log.Info(Log.CurrentMethod(), "Path to psdzdata folder:".PadRight(40) + "{0}", PsdzDataPath);
            Log.Info(Log.CurrentMethod(), "Path to Ediabas bin folder:".PadRight(40) + "{0}", EdiabasBinPath);
            Log.Info(Log.CurrentMethod(), "Dealer ID:".PadRight(40) + "{0}", DealerId);
            Log.Info(Log.CurrentMethod(), "JVM Options: \n{0}", string.Join(" ", JvmOptions));
            Log.Info(Log.CurrentMethod(), "JAR Arguments: \n{0}", string.Join(" ", JarArguments));
        }

        public string GetJvmOptionsAsOneString()
        {
            return string.Join(" ", JvmOptions);
        }

        public string GetJarArgumentsAsOneString()
        {
            return "\"" + string.Join("\" \"", JarArguments) + "\"";
        }

        [PreserveSource(Hint = "Modified")]
        private string[] GetPsdzJvmOptions()
        {
            int num = 1280;
            if (Environment.Is64BitOperatingSystem) // [UH] [IGNORE] replaced
            {
                int totalPhysicalMemoryInGb = GetTotalPhysicalMemoryInGb();
                int configint = ConfigSettings.getConfigint("BMW.Rheingold.CoreFramework.ParallelOperationsLimit", -1);
                num = ((totalPhysicalMemoryInGb <= 8 || configint <= 3) ? ((int)((double)num * 2.5)) : (512 + 512 * configint));
            }
            string item = string.Format(CultureInfo.InvariantCulture, "-Xmx{0}m", num);
            List<string> values = new List<string>
            {
                "-XX:ThreadStackSize=1024",
                item,
                "-XX:MaxGCPauseMillis=50",
                "-Dcom.sun.management.jmxremote",
                "-Dlog4j.configurationFile=\"" + GetLog4JConfigFilePath(Path.GetFullPath(Path.Combine(PsdzDataPath, "..\\WebService"))) + "\""
            };
            string defaultValue = string.Join(" ", values);
            string[] source = Regex.Split(ConfigSettings.getConfigString("BMW.Rheingold.Programming.PsdzWebservice.JvmOptions", defaultValue), "\\s+(?=\\-)");
            string configString = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Truststore.Path", Path.GetFullPath(Path.Combine(PsdzDataPath, "..\\Security\\cacerts"))); //[UH] [IGNORE] path modified
            if (!File.Exists(configString))
            {
                Log.Error(Log.CurrentMethod(), "Truststore File '" + configString + "' does not exist. You can check BMW.Rheingold.Programming.Truststore.Path registry key.");
            }
            string configString2 = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Truststore.Type", "jks");
            source = source.Append("-Djavax.net.ssl.trustStore=\"" + configString + "\"").ToArray();
            source = source.Append("-Djavax.net.ssl.trustStoreType=" + configString2).ToArray();
            Log.Info(Log.CurrentMethod(), "JVM Options for Psdz Webservice: " + string.Join(" ", source));
            return source;
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
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
            return 0;
        }

        [PreserveSource(Hint = "webServiceDir added")]
        private static string GetLog4JConfigFilePath(string webServiceDir)
        {
            if (!Directory.Exists(webServiceDir))
            {
                Log.Error(Log.CurrentMethod(), "Directory " + webServiceDir + " does not exists.");
            }
            string path = (ShouldDebugSpringboot(webServiceDir) ? "psdz-log4j2-spring-debug-config.xml" : "psdz-log4j2-config.xml");
            string text = Path.Combine(webServiceDir, path);
            if (text == null || !File.Exists(text))
            {
                Log.Error(Log.CurrentMethod(), text + " does not exist");
            }
            Log.Info(Log.CurrentMethod(), "returning " + text);
            return text;
        }

        private static bool ShouldDebugSpringboot(string webServiceDir)
        {
            if (File.Exists(Path.Combine(webServiceDir, "psdz-log4j2-spring-debug-config.xml")))
            {
                return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Springboot.Logging.Level.Debug.Enabled", defaultValue: false);
            }
            return false;
        }

        private static string GetEdiabasBinPath()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("PATH");
            string[] array = (string.IsNullOrEmpty(environmentVariable) ? new string[0] : environmentVariable.Split(';'));
            foreach (string text in array)
            {
                if (File.Exists(Path.Combine(text, "api32.dll")))
                {
                    return text;
                }
            }
            return null;
        }

        private string convertDealerIdToHex(string dealerId)
        {
            string text;
            if (dealerId != null && ushort.TryParse(dealerId, out var result))
            {
                text = result.ToString("X");
                Log.Info(Log.CurrentMethod(), "Dealer ID " + text + " is used.");
            }
            else
            {
                text = "1234";
                Log.Info(Log.CurrentMethod(), "dealerId " + dealerId + " cannot be converted to a Hex value. The default value (1234) is used.");
            }
            return text;
        }
    }
}