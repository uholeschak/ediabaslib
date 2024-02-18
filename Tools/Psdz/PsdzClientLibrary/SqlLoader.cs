using SQLitePCL;
using System.Reflection;
using System;
using System.IO;

namespace PsdzClientLibrary
{
    public class SqlLoader
    {
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
