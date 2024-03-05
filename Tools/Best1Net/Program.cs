using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Best1Net
{
    internal class Program
    {
        public const string BestDllName = "Best32.dll";

        [DllImport(BestDllName)]
        public static extern IntPtr __best1AsmVersion();

        [DllImport(BestDllName)]
        public static extern void __best1Init([MarshalAs(UnmanagedType.LPStr)] string inputFile, [MarshalAs(UnmanagedType.LPStr)] string outputFile, int generateMapfile,
            [MarshalAs(UnmanagedType.LPStr)] string revUser, int val2, int val3, [MarshalAs(UnmanagedType.LPStr)] string password, int val4, int val5);

        [DllImport(BestDllName)]
        public static extern void __best1Asm([MarshalAs(UnmanagedType.LPStr)] string mapFile, [MarshalAs(UnmanagedType.LPStr)] string infoFile);

        [DllImport(BestDllName)]
        public static extern void __best2Init();

        static int Main(string[] args)
        {
            IntPtr libHandle = IntPtr.Zero;
            try
            {
                string ediabasPath = Environment.GetEnvironmentVariable("EDIABAS_PATH");
                if (string.IsNullOrEmpty(ediabasPath))
                {
                    Console.WriteLine("EDIABS path not found");
                    return 1;
                }

                string ediabasBinPath = Path.Combine(ediabasPath, "bin");
                if (!Directory.Exists(ediabasBinPath))
                {
                    Console.WriteLine("EDIABS bin path not found");
                    return 1;
                }

                string bestDllPath = Path.Combine(ediabasBinPath, BestDllName);
                if (!File.Exists(bestDllPath))
                {
                    Console.WriteLine("{0} not found", BestDllName);
                    return 1;
                }

                if (!NativeLibrary.TryLoad(bestDllPath, out libHandle))
                {
                    Console.WriteLine("{0} not loaded", BestDllName);
                    return 1;
                }

                IntPtr bestVersionPtr = __best1AsmVersion();
                if (IntPtr.Zero != bestVersionPtr)
                {
                    string bestVersion = Marshal.PtrToStringAnsi(bestVersionPtr);
                    Console.WriteLine("Best version: {0}", bestVersion);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                return 1;
            }
            finally
            {
                if (libHandle != IntPtr.Zero)
                {
                    NativeLibrary.Free(libHandle);
                }
            }

            return 0;
        }
    }
}
