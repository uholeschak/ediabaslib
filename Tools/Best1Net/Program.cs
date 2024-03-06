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

        [DllImport(BestDllName, EntryPoint = "__best32Startup")]
        public static extern int __best32Startup32(int version, IntPtr infoText, int printInfo, IntPtr verBuffer, int verSize);

        [DllImport(BestDllName, EntryPoint = "__best32Shutdown")]
        public static extern int __best32Shutdown32();

        [DllImport(BestDllName, EntryPoint = "__best1AsmVersion")]
        public static extern IntPtr __best1AsmVersion32();

        [DllImport(BestDllName, EntryPoint = "__best1Init")]
        public static extern int __best1Init32(IntPtr inputFile, IntPtr outputFile, int val1,
            IntPtr revUser, int generateMapfile, int val3, IntPtr password, IntPtr configFile, int val5);

        [DllImport(BestDllName, EntryPoint = "__best1Config")]
        public static extern ErrorValueDelegate __best1Config32(ProgressDelegate progressCallback, ErrorTextDelegate errorTextCallback, ErrorValueDelegate errorValueCallback);

        [DllImport(BestDllName, EntryPoint = "__best1Options")]
        public static extern int __best1Options32(int mapOptions);

        [DllImport(BestDllName, EntryPoint = "__best1Asm")]
        public static extern int __best1Asm32([MarshalAs(UnmanagedType.LPStr)] string mapFile, [MarshalAs(UnmanagedType.LPStr)] string infoFile);

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
                    Console.WriteLine("EDIABAS path not found");
                    return 1;
                }

                string ediabasBinPath = Path.Combine(ediabasPath, "bin");
                if (!Directory.Exists(ediabasBinPath))
                {
                    Console.WriteLine("EDIABAS bin path not found");
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

                bool best32Started = false;
                int bestVerSize = 16;
                IntPtr bestVerPtr = Marshal.AllocHGlobal(bestVerSize);
                IntPtr inputFilePtr = Marshal.StringToHGlobalAnsi(inputFile);
                IntPtr outputFilePtr = Marshal.StringToHGlobalAnsi(outputFile);
                IntPtr passwordPtr = Marshal.StringToHGlobalAnsi("");
                IntPtr configFilePtr = Marshal.StringToHGlobalAnsi("");

                try
                {
                    int startResult = __best32Startup32(0x20000, IntPtr.Zero, 0, bestVerPtr, bestVerSize);
                    //Console.WriteLine("Best32 start result: {0}", startResult);
                    if (startResult != 1)
                    {
                        Console.WriteLine("Best32 startup failed");
                        return 1;
                    }

                    best32Started = true;
                    string bestVer = Marshal.PtrToStringAnsi(bestVerPtr);
                    Console.WriteLine("Best version: {0}", bestVer);

                    int initResult = __best1Init32(inputFilePtr, outputFilePtr, 0, IntPtr.Zero, 0, 0, passwordPtr, configFilePtr, 0);
                    //Console.WriteLine("Best1 init result: {0}", initResult);

                    if (initResult != 0)
                    {
                        Console.WriteLine("Best1 init failed");
                        return 1;
                    }

                    ErrorValueDelegate configResult = __best1Config32(ProgressEvent, ErrorTextEvent, ErrorValueEvent);
                    if (configResult == null)
                    {
                        Console.WriteLine("Best1 config failed");
                        return 1;
                    }

                    int optionsResult = __best1Options32(0);
                    // the option result is the specified value

                    int asmResult = __best1Asm32(null, null);
                    //Console.WriteLine("Best1 asm result: {0}", asmResult);
                    if (asmResult != 0)
                    {
                        Console.WriteLine("Best1 asm failed");
                        return 1;
                    }

                    IntPtr bestVersionPtr = __best1AsmVersion32();
                    if (IntPtr.Zero != bestVersionPtr)
                    {
                        Int32 asmVer = Marshal.ReadInt32(bestVersionPtr);
                        Console.WriteLine("Asm version: {0:X08}", asmVer);
                    }
                }
                finally
                {
                    if (best32Started)
                    {
                        __best32Shutdown32();
                    }

                    if (bestVerPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(bestVerPtr);
                    }

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
            if (value >= 0)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("Line: {0}", value);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Done");
            }
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
