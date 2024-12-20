using System;
using System.IO;

namespace ApkUncompress2
{
    internal class App
    {
        static int Usage()
        {
            Console.WriteLine("Usage: decompress-assemblies {file.{dll,apk,aab}} [{file.{dll,apk,aab} ...]");
            Console.WriteLine();
            Console.WriteLine("DLL files passed on command line are uncompressed to the file directory with the `uncompressed-` prefix added to their name.");
            Console.WriteLine("DLL files from AAB/APK archives are uncompressed to a subdirectory of the file directory named after the archive with extension removed");
            return 1;
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return Usage();
            }

            bool haveErrors = false;
            foreach (string file in args)
            {
                string ext = Path.GetExtension(file);
                string fullPath = Path.GetFullPath(file);
                if (string.IsNullOrEmpty(fullPath))
                {
                    continue;
                }

                string? outputPath = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(outputPath))
                {
                    continue;
                }
            }

            return haveErrors ? 1 : 0;
        }
    }
}
