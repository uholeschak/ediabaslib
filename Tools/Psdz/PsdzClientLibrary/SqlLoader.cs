using SQLitePCL;
using System.Reflection;
using System;
using System.IO;
using HarmonyLib;

namespace PsdzClientLibrary
{
    public static class SqlLoader
    {
        private static bool _isPatched;
        private static readonly string[] _testTypes =
        {
            "Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime",
            "Windows.Storage.ApplicationData, Microsoft.Windows.SDK.NET",
            "Windows.Storage.StorageFolder, Windows, ContentType=WindowsRuntime",
            "Windows.Storage.StorageFolder, Microsoft.Windows.SDK.NET"
        };

        public static bool PatchLoader(Harmony harmony)
        {
            if (_isPatched)
            {
                return true;
            }

            try
            {
                bool patchSqliteInitRequired = false;
                bool patchGetTypeRequired = false;

                try
                {
                    foreach (string testType in _testTypes)
                    {
                        Type dummy = Type.GetType(testType);
                    }
                }
                catch (Exception)
                {
                    patchGetTypeRequired = true;
                }

                string location = Path.GetDirectoryName(typeof(SqlLoader).Assembly.Location);
                if (!string.IsNullOrEmpty(location))
                {
                    string libPath = Path.Combine(location, "runtimes");
                    if (!Directory.Exists(libPath))
                    {
                        patchSqliteInitRequired = true;
                    }
                }

                // https://github.com/ericsink/SQLitePCL.raw/issues/405
                // Another option is setting shadowCopyBinAssemblies to false in the web.config file, section system.web.
                // <hostingEnvironment shadowCopyBinAssemblies="false" />
                if (patchSqliteInitRequired)
                {
                    MethodInfo methodCallSqliteInitPrefix = typeof(SqlLoader).GetMethod("CallSqliteInitPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodCallSqliteInitPrefix == null)
                    {
                        return false;
                    }

                    MethodInfo methodInit = typeof(Batteries_V2).GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
                    if (methodInit == null)
                    {
                        return false;
                    }

                    harmony.Patch(methodInit, new HarmonyMethod(methodCallSqliteInitPrefix));
                }

                // Fixed in the next update
                // https://github.com/dotnet/efcore/issues/32614
                if (patchGetTypeRequired)
                {
                    MethodInfo methodCallGetTypePrefix = typeof(SqlLoader).GetMethod("CallGetTypePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodCallGetTypePrefix == null)
                    {
                        return false;
                    }

                    MethodInfo methodGetType = typeof(Type).GetMethod("GetType", BindingFlags.Public | BindingFlags.Static,
                        null, new Type[] { typeof(string) }, null);
                    if (methodGetType == null)
                    {
                        return false;
                    }

                    harmony.Patch(methodGetType, new HarmonyMethod(methodCallGetTypePrefix));
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _isPatched = true;
            }

            return true;
        }

        public static void Init()
        {
            string codeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (!string.IsNullOrEmpty(codeBase))
            {
                UriBuilder uriBuilder = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(new Uri(uriBuilder.Path).LocalPath);
                string rid = (IntPtr.Size == 8) ? "win-x64" : "win-x86";
                string libPath = Path.Combine(path, "runtimes", rid, "native", "e_sqlite3mc");
                DoDynamic_cdecl(libPath, NativeLibrary.WHERE_PLAIN);
            }
        }

        public static void DoDynamic_cdecl(string name, int flags)
        {
            IGetFunctionPointer gf = MakeDynamic(name, flags);
            SQLite3Provider_dynamic_cdecl.Setup(name, gf);
            raw.SetProvider(new SQLite3Provider_dynamic_cdecl());
        }

        public static IGetFunctionPointer MakeDynamic(string name, int flags)
        {
            Assembly assembly = typeof(raw).Assembly;
            return new MyGetFunctionPointer(NativeLibrary.Load(name, assembly, flags));
        }

        private class MyGetFunctionPointer : IGetFunctionPointer
        {
            private readonly IntPtr _dll;

            public MyGetFunctionPointer(IntPtr dll) => this._dll = dll;

            public IntPtr GetFunctionPointer(string name)
            {
                IntPtr address;
                return NativeLibrary.TryGetExport(this._dll, name, out address) ? address : IntPtr.Zero;
            }
        }
        private static bool CallSqliteInitPrefix()
        {
            Init();
            return false;
        }

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
