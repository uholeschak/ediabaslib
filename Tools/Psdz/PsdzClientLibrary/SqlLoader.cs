using SQLitePCL;
using System.Reflection;
using System;
using System.IO;
using HarmonyLib;

namespace PsdzClientLibrary
{
    public class SqlLoader
    {
        public static bool PatchLoader(Harmony harmony)
        {
            MethodInfo methodCallSqliteInitPrefix = typeof(SqlLoader).GetMethod("CallSqliteInitPrefix", BindingFlags.NonPublic | BindingFlags.Static);
            if (methodCallSqliteInitPrefix == null)
            {
                return false;
            }

            MethodInfo methodCallGetTypePrefix = typeof(SqlLoader).GetMethod("CallGetTypePrefix", BindingFlags.NonPublic | BindingFlags.Static);
            if (methodCallGetTypePrefix == null)
            {
                return false;
            }

            MethodInfo methodInit = typeof(Batteries_V2).GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
            if (methodInit == null)
            {
                return false;
            }

            MethodInfo methodGetType = typeof(Type).GetMethod("GetType", BindingFlags.Public | BindingFlags.Static,
                null, new Type[] { typeof(string) }, null);
            if (methodGetType == null)
            {
                return false;
            }

            bool patchedSqliteInit = false;
            bool patchedGetType = false;
            foreach (MethodBase methodBase in harmony.GetPatchedMethods())
            {
                if (methodBase == methodInit)
                {
                    patchedSqliteInit = true;
                }

                if (methodBase == methodGetType)
                {
                    patchedGetType = true;
                }
            }

            // Another option is setting shadowCopyBinAssemblies to false in the web.config file, section system.web.
            // <hostingEnvironment shadowCopyBinAssemblies="false" />
            if (!patchedSqliteInit)
            {
                harmony.Patch(methodInit, new HarmonyMethod(methodCallSqliteInitPrefix));
            }

            // Trust assemblies in web.config file, section system.web.
            // Create publicKey with: sn -Tp <assembly>
            // <fullTrustAssemblies>
            //   <add assemblyName="Microsoft.Data.Sqlite" version="8.0.2.0" publicKey="xxxxx" />
            // </fullTrustAssemblies>
            if (!patchedGetType)
            {
                harmony.Patch(methodGetType, new HarmonyMethod(methodCallGetTypePrefix));
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
