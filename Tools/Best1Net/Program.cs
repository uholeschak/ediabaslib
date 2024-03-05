using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Best1Net
{
    internal class Program
    {
        public const string BestDllName = "Best32.dll";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ProgressDelegate(int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ErrorTextDelegate([MarshalAs(UnmanagedType.LPStr)] string text);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int ErrorValueDelegate(uint value, [MarshalAs(UnmanagedType.LPStr)] string text);

        [DllImport(BestDllName)]
        public static extern IntPtr __best1AsmVersion();

        [DllImport(BestDllName)]
        public static extern int __best1Init(IntPtr inputFile, IntPtr outputFile, int val1,
            IntPtr revUser, int generateMapfile, int val3, IntPtr password, IntPtr configFile, int val5);

        [DllImport(BestDllName)]
        public static extern ErrorValueDelegate __best1Config(ProgressDelegate progressCallback, ErrorTextDelegate errorTextCallback, ErrorValueDelegate errorValueCallback);

        [DllImport(BestDllName)]
        public static extern int __best1Options(int mapOptions);

        [DllImport(BestDllName)]
        public static extern int __best1Asm([MarshalAs(UnmanagedType.LPStr)] string mapFile, [MarshalAs(UnmanagedType.LPStr)] string infoFile);

        [DllImport(BestDllName)]
        public static extern int __best2Init();

        static int Main(string[] args)
        {
            IntPtr libHandle = IntPtr.Zero;
            try
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("Usage: Best1Net <inputFile>");
                    return 1;
                }

                string inputFile = args[0];
                if (!File.Exists(inputFile))
                {
                    Console.WriteLine("Input file not found");
                    return 1;
                }

                string fileExt = Path.GetExtension(inputFile);
                string outExt = ".prg";
                if (string.Compare(fileExt, ".b1g", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    outExt = ".grp";
                }

                string outputFile = Path.ChangeExtension(inputFile, outExt);

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

                IntPtr inputFilePtr = Marshal.StringToHGlobalAnsi(inputFile);
                IntPtr outputFilePtr = Marshal.StringToHGlobalAnsi(outputFile);
                IntPtr passwordPtr = Marshal.StringToHGlobalAnsi("");
                IntPtr configFilePtr = Marshal.StringToHGlobalAnsi("");

                try
                {
                    int initResult = __best1Init(inputFilePtr, outputFilePtr, 0, IntPtr.Zero, 0, 0, passwordPtr, configFilePtr, 0);
                    Console.WriteLine("Best1 init result: {0}", initResult);

                    if (initResult != 0)
                    {
                        Console.WriteLine("Best1 init failed");
                        return 1;
                    }

                    ErrorValueDelegate configResult = __best1Config(ProgressEvent, ErrorTextEvent, ErrorValueEvent);
                    int optionsResult = __best1Options(0);

                    int asmResult = __best1Asm(null, null);
                    Console.WriteLine("Best1 asm result: {0}", asmResult);

                    IntPtr bestVersionPtr = __best1AsmVersion();
                    if (IntPtr.Zero != bestVersionPtr)
                    {
                        byte[] versionArray = new byte[0x6C];
                        Marshal.Copy(bestVersionPtr, versionArray, 0, versionArray.Length);

                        Console.WriteLine("Best version: {0}", BitConverter.ToString(versionArray));
                    }
                }
                finally
                {
                    if (inputFilePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(inputFilePtr);
                    }

                    if (outputFilePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(outputFilePtr);
                    }

                    if (passwordPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(passwordPtr);
                    }

                    if (configFilePtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(configFilePtr);
                    }
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

        private static int ProgressEvent(int value)
        {
            Console.WriteLine("Progress: {0}", value);
            return 0;
        }

        private static int ErrorTextEvent(string text)
        {
            Console.WriteLine("Error: {0}", text);
            return 0;
        }

        private static int ErrorValueEvent(uint value, string text)
        {
            Console.WriteLine("Error value: {0}: {1}", value, text);
            return 0;
        }
    }
}
