using SQLitePCL;
using System.Reflection;
using System;
using System.IO;
using HarmonyLib;
using PsdzClient;

namespace PsdzClientLibrary
{
    public class SqlLoader
    {
        public static bool PatchLoader(Harmony harmony)
        {
            MethodInfo methodCallSqliteInitInitPrefix = typeof(PsdzDatabase).GetMethod("CallSqliteInitInitPrefix", BindingFlags.NonPublic | BindingFlags.Static);
            if (methodCallSqliteInitInitPrefix == null)
            {
                return false;
            }

            Type sqliteBatteriesType = typeof(Batteries_V2);
            MethodInfo methodInit = sqliteBatteriesType.GetMethod("Init", BindingFlags.Public | BindingFlags.Static);
            if (methodInit == null)
            {
                return false;
            }

            bool patchedGetDatabase = false;
            foreach (MethodBase methodBase in harmony.GetPatchedMethods())
            {
                if (methodBase == methodInit)
                {
                    patchedGetDatabase = true;
                }
            }

            if (!patchedGetDatabase)
            {
                harmony.Patch(methodInit, new HarmonyMethod(methodCallSqliteInitInitPrefix));
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
    }
}
