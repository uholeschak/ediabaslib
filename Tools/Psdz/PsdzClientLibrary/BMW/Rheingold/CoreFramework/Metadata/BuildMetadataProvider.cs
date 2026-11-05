using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BMW.Rheingold.CoreFramework.Metadata
{
    public static class BuildMetadataProvider
    {
        private const string DisplayNameKey = "ProgrammingApiDisplayName";
        private const string DefaultDisplayName = "Open ProgrammingAPI";
        private const string ProgrammingApiExeName = "ProgrammingAPI.exe";
        public static string DisplayName
        {
            get
            {
                if (!ConfigSettings.IsProgrammingApiModeActive)
                {
                    return "ISTA";
                }

                Assembly programmingApiAssembly = GetProgrammingApiAssembly();
                return (((object)programmingApiAssembly == null) ? null : programmingApiAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault((AssemblyMetadataAttribute a) => a.Key == "ProgrammingApiDisplayName")?.Value) ?? "Open ProgrammingAPI";
            }
        }

        private static Assembly GetProgrammingApiAssembly()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly?.ManifestModule.Name == "ProgrammingAPI.exe")
            {
                return entryAssembly;
            }

            string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProgrammingAPI.exe");
            if (!File.Exists(text))
            {
                return entryAssembly;
            }

            return Assembly.LoadFrom(text);
        }
    }
}