using Org.BouncyCastle.Tls.Crypto;
using PsdzClient.Core;
using System;
using System.IO;

namespace PsdzClient.Programming
{
    public static class Psdz64BitPathResolver
    {
        // Force64Bit removed

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
            string text = string.Empty;
            if (psdzWebService)
            {
                text = "WebService\\";
                string javaPath = Path.Combine(istaFolder, "WebService", "OpenJREx64", "bin", "java.exe");
                if (!File.Exists(javaPath))
                {
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
    }
}