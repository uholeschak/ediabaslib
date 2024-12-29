using System;
using System.IO;

namespace ApkUncompress
{
    internal class App
    {
        static ApkUncompressCommon apkUncompress = new ApkUncompressCommon();

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

                if (string.Compare(".dll", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string prefixDll = "uncompressed-";
                    string outputDirDll = Path.Combine(outputPath, prefixDll);
                    if (Directory.Exists(outputDirDll))
                    {
                        Directory.Delete(outputDirDll, true);
                    }

                    if (!apkUncompress.UncompressDLL(file, prefixDll, outputPath))
                    {
                        Console.WriteLine("Uncompress failed: {0}", file);
                        haveErrors = true;
                    }

                    Console.WriteLine("Uncompressed: {0}", file);
                    continue;
                }

                string? assembliesPath = null;
                if (string.Compare(".apk", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    assembliesPath = ApkUncompressCommon.AssembliesPathApk;
                }

                if (string.Compare(".aab", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    assembliesPath = ApkUncompressCommon.AssembliesPathAab;
                }

                if (!string.IsNullOrEmpty(assembliesPath))
                {
                    string prefix = $"uncompressed-{Path.GetFileNameWithoutExtension(file)}";
                    string outputDir = Path.Combine(outputPath, prefix);
                    if (Directory.Exists(outputDir))
                    {
                        Directory.Delete(outputDir, true);
                    }

                    if (!apkUncompress.UncompressFromAPK(file, assembliesPath, prefix, outputPath))
                    {
                        Console.WriteLine("Uncompress failed: {0}", file);
                        haveErrors = true;
                    }

                    Console.WriteLine("Uncompressed: {0}", file);
                    continue;
                }
            }

            return haveErrors ? 1 : 0;
        }
    }
}
