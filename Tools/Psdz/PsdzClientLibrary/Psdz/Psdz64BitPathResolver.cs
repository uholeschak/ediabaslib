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

        [PreserveSource(Hint = "istaFolder, psdzWebService added", SignatureModified = true)]
        public static string GetJrePath(string istaFolder, bool psdzWebService)
        {
            //[-] new CommonServiceWrapper();
            //[-] string text = "PSdZ\\WebService\\";
            //[-] string text2 = ((!Force64Bit && (ConfigSettings.IsOssModeActive || IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA"))) ? "OpenJREx86" : "OpenJREx64");
            //[-] string defaultValue = "..\\..\\..\\" + text + text2;
            //[-] return Path.GetFullPath(ConfigSettings.getPathString("BMW.Rheingold.Programming.PsdzJrePath.WebService", defaultValue));
            //[+] string text = string.Empty;
            string text = string.Empty;
            //[+] if (psdzWebService)
            if (psdzWebService)
            //[+] {
            {
                //[+] text = "WebService\\";
                text = "WebService\\";
                //[+] string javaPath = Path.Combine(istaFolder, "WebService", "OpenJREx64", "bin", "java.exe");
                string javaPath = Path.Combine(istaFolder, "WebService", "OpenJREx64", "bin", "java.exe");
                //[+] if (!File.Exists(javaPath))
                if (!File.Exists(javaPath))
                //[+] {
                {
                    //[+] text = "PSdZ\\WebService\\";
                    text = "PSdZ\\WebService\\";
                    //[+] }
                }
            //[+] }
            }
            //[+] else
            else
            //[+] {
            {
                //[+] string tlsPath = Path.Combine(istaFolder, "Tls13");
                string tlsPath = Path.Combine(istaFolder, "Tls13");
                //[+] if (Directory.Exists(tlsPath))
                if (Directory.Exists(tlsPath))
                //[+] {
                {
                    //[+] text = "Tls13\\" + text;
                    text = "Tls13\\" + text;
                //[+] }
                }
            //[+] }
            }
            //[+] string defaultValue = (Environment.Is64BitOperatingSystem ? (text + "OpenJREx64") : (text + "OpenJREx86"));
            string defaultValue = (Environment.Is64BitOperatingSystem ? (text + "OpenJREx64") : (text + "OpenJREx86"));
            //[+] string configPath = ConfigSettings.getPathString(psdzWebService ? "BMW.Rheingold.Programming.PsdzJrePath.WebService" : "BMW.Rheingold.Programming.PsdzJrePath", string.Empty);
            string configPath = ConfigSettings.getPathString(psdzWebService ? "BMW.Rheingold.Programming.PsdzJrePath.WebService" : "BMW.Rheingold.Programming.PsdzJrePath", string.Empty);
            //[+] if (!string.IsNullOrEmpty(configPath))
            if (!string.IsNullOrEmpty(configPath))
            //[+] {
            {
                //[+] return Path.GetFullPath(configPath);
                return Path.GetFullPath(configPath);
            //[+] }
            }
            //[+] return Path.Combine(istaFolder, defaultValue);
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