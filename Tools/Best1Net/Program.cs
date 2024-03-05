using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Best1Net
{
    internal class Program
    {
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

                string bestDllPath = Path.Combine(ediabasBinPath, "Best32.dll");
                if (!File.Exists(bestDllPath))
                {
                    Console.WriteLine("Best32.dll not found");
                    return 1;
                }

                if (!NativeLibrary.TryLoad(bestDllPath, out libHandle))
                {
                    Console.WriteLine("Best32.dll not loaded");
                    return 1;
                }

                Console.WriteLine("Library loaded");
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
