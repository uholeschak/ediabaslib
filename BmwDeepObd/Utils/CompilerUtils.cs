using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace BmwDeepObd
{
    public static class CompilerUtils
    {
        private static readonly string Tag = typeof(CompilerUtils).FullName;

        public static List<Microsoft.CodeAnalysis.MetadataReference> GetLoadedMetadataReferences(string packageAssembiesDir, out List<string> errorList)
        {
            string assembliesDir = packageAssembiesDir;
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Microsoft.CodeAnalysis.MetadataReference> referencesList = new List<Microsoft.CodeAnalysis.MetadataReference>();
            errorList = new List<string>();

            List<string> abiDirs = SystemUtils.GetCurrentAbiDirs();
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences ABI dirs={0}", string.Join(", ", abiDirs)));
#endif
            // directories of assemblies loaded from files (e.g. the fast deployment override dir)
            List<string> loadedAssemblyDirs = new List<string>();
            foreach (Assembly assembly in loadedAssemblies)
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

#pragma warning disable IL3000
                string location = assembly.Location;
#pragma warning restore IL3000
                if (string.IsNullOrEmpty(location) || !Path.IsPathRooted(location) || !File.Exists(location))
                {
                    continue;
                }

                string dirName = Path.GetDirectoryName(location);
                if (!string.IsNullOrEmpty(dirName) && !loadedAssemblyDirs.Contains(dirName))
                {
                    loadedAssemblyDirs.Add(dirName);
                }
            }

            List<string> searchDirs = new List<string> { assembliesDir };
            foreach (string abi in abiDirs)
            {
                searchDirs.Add(Path.Combine(assembliesDir, abi));
            }
            searchDirs.AddRange(loadedAssemblyDirs);

            foreach (Assembly assembly in loadedAssemblies)
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                // the location contain no path for embedded assemblies (Mono)
                // or is empty for assemblies loaded from memory (CoreCLR assembly store, System.Private.CoreLib)
#pragma warning disable IL3000
                string location = assembly.Location;
#pragma warning restore IL3000
                if (string.IsNullOrEmpty(location))
                {
                    // assemblies loaded from byte arrays (e.g. compiled user code) are in a separate load context
                    if (AssemblyLoadContext.GetLoadContext(assembly) != AssemblyLoadContext.Default)
                    {
                        continue;
                    }

                    string assemblyName = assembly.GetName().Name;
                    if (string.IsNullOrEmpty(assemblyName))
                    {
                        continue;
                    }

                    location = assemblyName;
                }

                if (!File.Exists(location))
                {
                    string fileName = Path.GetFileName(location);

                    const string dllExtension = ".dll";
                    if (!fileName.EndsWith(dllExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += dllExtension;
                    }

                    location = null;
                    foreach (string searchDir in searchDirs)
                    {
                        string searchLocation = Path.Combine(searchDir, fileName);
                        if (File.Exists(searchLocation))
                        {
                            location = searchLocation;
                            break;
                        }
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File={0}, Location={1}", fileName, location ?? "not found"));
#endif
                }

                if (location != null)
                {
                    try
                    {
                        using (File.Open(location, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                        }
                    }
#pragma warning disable CS0168 // Variable is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File read Exception: {0}", ex.Message));
#endif
                        location = null;
                    }
                }

                if (location != null)
                {
                    referencesList.Add(Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(location));
                }
                else
                {
                    // the location contain no path for embedded assemblies
#pragma warning disable IL3000
                    string fileName = Path.GetFileName(assembly.Location);
#pragma warning restore IL3000
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = assembly.GetName().Name;
                    }
                    errorList.Add(fileName);
                }
            }

#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences Reference count: {0}, Error count: {1}", referencesList.Count, errorList.Count));
#endif
            return referencesList;
        }
    }
}
