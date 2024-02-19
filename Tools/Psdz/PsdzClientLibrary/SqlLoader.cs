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
            MethodInfo methodCallSqliteInitInitPrefix = typeof(SqlLoader).GetMethod("CallSqliteInitInitPrefix", BindingFlags.NonPublic | BindingFlags.Static);
            if (methodCallSqliteInitInitPrefix == null)
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

            bool patchedGetDatabase = false;
            bool patchedGetType = false;
            foreach (MethodBase methodBase in harmony.GetPatchedMethods())
            {
                if (methodBase == methodInit)
                {
                    patchedGetDatabase = true;
                }

                if (methodBase == methodGetType)
                {
                    patchedGetType = true;
                }
            }

            if (!patchedGetDatabase)
            {
                harmony.Patch(methodInit, new HarmonyMethod(methodCallSqliteInitInitPrefix));
            }

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
        private static bool CallSqliteInitInitPrefix()
        {
            Init();
            return false;
        }

        private static bool CallGetTypePrefix(ref object __result, string typeName)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                if (typeName.StartsWith("Windows.Storage"))
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}
