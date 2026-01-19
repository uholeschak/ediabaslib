using HarmonyLib;
using log4net;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PsdzClient
{
    public static class SqlLoader
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int AddDllDirectory(string NewDirectory);

        private static readonly ILog log = LogManager.GetLogger(typeof(SqlLoader));

        private static readonly string[] _testTypes =
        {
            "Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime",
            "Windows.Storage.ApplicationData, Microsoft.Windows.SDK.NET",
            "Windows.Storage.StorageFolder, Windows, ContentType=WindowsRuntime",
            "Windows.Storage.StorageFolder, Microsoft.Windows.SDK.NET"
        };

        private static bool _isPatched;

        public static bool PatchLoader(Harmony harmony)
        {
            log.InfoFormat("PatchLoader: Is patched: {0}", _isPatched);
            if (_isPatched)
            {
                return true;
            }

            try
            {
                bool patchSqliteLoadRequired = false;
                bool patchGetTypeRequired = false;

                try
                {
                    foreach (string testType in _testTypes)
                    {
                        Type dummy = Type.GetType(testType);
                    }
                }
                catch (Exception ex)
                {
                    log.InfoFormat("PatchLoader: GetType Exception: {0}", ex.Message);
                    patchGetTypeRequired = true;
                }

                string location = Path.GetDirectoryName(typeof(SqliteConnection).Assembly.Location);
                if (!string.IsNullOrEmpty(location))
                {
                    string libPath = GetLibPath(location);
                    string libName = libPath + ".dll";
                    if (!File.Exists(libName))
                    {
                        patchSqliteLoadRequired = true;
                    }
                }

                // https://github.com/ericsink/SQLitePCL.raw/issues/405
                // Another option is setting shadowCopyBinAssemblies to false in the web.config file, section system.web.
                // <hostingEnvironment shadowCopyBinAssemblies="false" />
                if (patchSqliteLoadRequired)
                {
                    log.InfoFormat("PatchLoader: Patching Init");
#if NET
                    try
                    {
                        string assemblyDir = Path.GetDirectoryName(typeof(SqliteConnection).Assembly.Location);
                        if (!string.IsNullOrEmpty(assemblyDir))
                        {
                            string libPath = GetLibPath(assemblyDir);
                            AddDllDirectory(libPath);

                            NativeLibrary.SetDllImportResolver(typeof(SQLite3Provider_e_sqlite3mc).Assembly, (name, assembly, path) =>
                            {
                                IntPtr libHandle = IntPtr.Zero;
                                if (string.Compare(name, "e_sqlite3mc", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    libHandle = NativeLibrary.Load(name, assembly, DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.UserDirectories);
                                }
                                return libHandle;
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("PatchLoader: SetDllImportResolver Exception: {0}", e.Message);
                        return false;
                    }
#else
                    MethodInfo methodCallSqliteInitPrefix = typeof(SqlLoader).GetMethod("CallSqliteInitPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodCallSqliteInitPrefix == null)
                    {
                        log.ErrorFormat("PatchLoader: CallSqliteInitPrefix method missing");
                        return false;
                    }

                    MethodInfo methodInit = typeof(Batteries_V2).GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
                    if (methodInit == null)
                    {
                        log.ErrorFormat("PatchLoader: Init method missing");
                        return false;
                    }

                    harmony.Patch(methodInit, new HarmonyMethod(methodCallSqliteInitPrefix));
#endif
                }

                // Fixed in the next update
                // https://github.com/dotnet/efcore/issues/32614
                if (patchGetTypeRequired)
                {
                    log.InfoFormat("PatchLoader: Patching GetType");
                    MethodInfo methodCallGetTypePrefix = typeof(SqlLoader).GetMethod("CallGetTypePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodCallGetTypePrefix == null)
                    {
                        log.ErrorFormat("PatchLoader: CallGetTypePrefix method missing");
                        return false;
                    }

                    MethodInfo methodGetType = typeof(Type).GetMethod("GetType", BindingFlags.Public | BindingFlags.Static,
                        null, new Type[] { typeof(string) }, null);
                    if (methodGetType == null)
                    {
                        log.ErrorFormat("PatchLoader: GetType method missing");
                        return false;
                    }

                    harmony.Patch(methodGetType, new HarmonyMethod(methodCallGetTypePrefix));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("PatchLoader: Patch Exception: {0}", ex.Message);
                return false;
            }
            finally
            {
                _isPatched = true;
            }

            return true;
        }

        private static string GetLibPath(string path)
        {
            string ridBack = (IntPtr.Size == 8) ? "x64" : "x86";
            string rid = "win-" + ridBack;
            return Path.Combine(path, "runtimes", rid, "native", "e_sqlite3mc");
        }

#if !NET
        public static void Init()
        {
            string codeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (!string.IsNullOrEmpty(codeBase))
            {
                UriBuilder uriBuilder = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(new Uri(uriBuilder.Path).LocalPath);
                string libPath = GetLibPath(path);
                DoDynamic_cdecl(libPath, NativeLibrary.WHERE_PLAIN);
            }
        }

        public static void DoDynamic_cdecl(string name, int flags = 0)
        {
            IGetFunctionPointer gf = MakeDynamic(name, flags);
            SQLite3Provider_dynamic_cdecl.Setup(name, gf);
            raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
        }

        public static IGetFunctionPointer MakeDynamic(string name, int flags = 0)
        {
            Assembly assembly = typeof(raw).Assembly;
            return new MyGetFunctionPointer(NativeLibrary.Load(name, assembly, flags));
        }

        private class MyGetFunctionPointer : IGetFunctionPointer
        {
            private readonly IntPtr _dll;

            public MyGetFunctionPointer(IntPtr dll) => _dll = dll;

            public IntPtr GetFunctionPointer(string name)
            {
                IntPtr address;
                return NativeLibrary.TryGetExport(_dll, name, out address) ? address : IntPtr.Zero;
            }
        }

        private static bool CallSqliteInitPrefix()
        {
            Init();
            return false;
        }
#endif
        private static bool CallGetTypePrefix(ref object __result, string typeName)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                if (typeName.StartsWith("Windows.Storage.", StringComparison.OrdinalIgnoreCase))
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}
