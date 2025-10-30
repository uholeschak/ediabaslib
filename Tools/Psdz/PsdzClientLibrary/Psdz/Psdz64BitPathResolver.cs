using PsdzClient.Core;
using System;
using System.IO;

namespace PsdzClient.Programming
{
    public static class Psdz64BitPathResolver
    {
        private static readonly Lazy<bool> force64Bit = new Lazy<bool>(delegate
        {
#if false
            using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
            {
                if (istaIcsServiceClient.IsAvailable() || IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))
                {
                    return istaIcsServiceClient.GetFeatureEnabledStatus("ForceUsingJava64Bit", istaIcsServiceClient.IsAvailable()).IsActive;
                }
            }
#endif
            return false;
        });

        public static bool Force64Bit => force64Bit.Value;

        // [UH] modified
        public static string GetPsdzPath(string istaFolder)
        {
            string psdzSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\binx64" : @"PSdZ\bin";
            string psdzBinaryPath = Path.Combine(istaFolder, psdzSubDir);
            return psdzBinaryPath;
        }

        // [UH] istaFolder, psdzWebService added
        public static string GetJrePath(string istaFolder, bool psdzWebService = false)
        {
            string text = (psdzWebService ? "WebService\\" : string.Empty);
            if (!psdzWebService)
            {
                string tlsPath = Path.Combine(istaFolder, "Tls13");
                if (Directory.Exists(tlsPath))
                {
                    text = "Tls13\\" + text;
                }
            }
            string defaultValue = (Environment.Is64BitOperatingSystem ? (text + "OpenJREx64") : (text + "OpenJREx86"));
            string configPath = ConfigSettings.getPathString(psdzWebService ? "BMW.Rheingold.Programming.PsdzJrePath.WebService" : "BMW.Rheingold.Programming.PsdzJrePath", string.Empty);
            if (!string.IsNullOrEmpty(configPath))
            {
                return Path.GetFullPath(configPath);
            }
            return Path.Combine(istaFolder, defaultValue);
        }
    }
}