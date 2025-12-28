using Org.BouncyCastle.Tls.Crypto;
using PsdzClient.Core;
using System;
using System.IO;

namespace PsdzClient.Programming
{
    public static class Psdz64BitPathResolver
    {
        [PreserveSource(Hint = "Cleaned", OriginalHash = "7E1B3F17B11BE32638A1425A1E16CBCA")]
        private static readonly Lazy<bool> force64Bit = new Lazy<bool>(delegate
        {
            return false;
        });
        public static bool Force64Bit => force64Bit.Value;

        [PreserveSource(Hint = "istaFolder, psdzWebService added", OriginalHash = "C29E9557E1A3EFCA6DDD185E6485F37C")]
        public static string GetJrePath(string istaFolder, bool psdzWebService)
        {
            string text = string.Empty;
            if (psdzWebService)
            {
                text = "WebService\\";
                string javaPath = Path.Combine(istaFolder, "WebService", "OpenJREx64", "bin", "java.exe");
                if (!File.Exists(javaPath))
                { // [UH] [IGNORE] Fallback for older installations
                    text = "PSdZ\\WebService\\";
                }
            }
            else
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

        [PreserveSource(Hint = "Added")]
        public static string GetPsdzPath(string istaFolder)
        {
            string psdzSubDir = Environment.Is64BitOperatingSystem ? @"PSdZ\binx64" : @"PSdZ\bin";
            string psdzBinaryPath = Path.Combine(istaFolder, psdzSubDir);
            return psdzBinaryPath;
        }
    }
}