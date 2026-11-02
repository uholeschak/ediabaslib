using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            foreach (Assembly assembly in loadedAssemblies)
            {
                // the location contain no path for embedded assemblies
#pragma warning disable IL3000
                string location = assembly.Location;
#pragma warning restore IL3000
                if (string.IsNullOrEmpty(location))
                {
                    continue;
                }

                if (!File.Exists(location))
                {
                    string fileName = Path.GetFileName(location);

                    const string dllExtension = ".dll";
                    if (!fileName.EndsWith(dllExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += dllExtension;
                    }

                    location = Path.Combine(assembliesDir, fileName);
                    if (!File.Exists(location))
                    {
                        foreach (string abi in abiDirs)
                        {
                            location = Path.Combine(assembliesDir, abi, fileName);
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences ABI={0}, Location={1}", abi, location));
#endif
                            if (File.Exists(location))
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File found: {0}", fileName));
#endif
                                break;
                            }
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences File not found: {0}", fileName));
#endif
                            location = null;
                        }
                    }
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
                    errorList.Add(fileName);
                }
            }

#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("GetLoadedMetadataReferences Error count: {0}", errorList.Count));
#endif
            return referencesList;
        }
    }
}
