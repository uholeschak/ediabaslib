using SQLitePCL;
using System.Reflection;
using System;

namespace PsdzClientLibrary
{
    public class SqlLoader
    {
        public static void Init()
        {
            DoDynamic_cdecl("e_sqlite3mc", NativeLibrary.WHERE_RUNTIME_RID | NativeLibrary.WHERE_ADJACENT | NativeLibrary.WHERE_CODEBASE);
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
            return (IGetFunctionPointer)new MyGetFunctionPointer(NativeLibrary.Load(name, assembly, flags));
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
