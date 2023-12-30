using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using K4os.Compression.LZ4;
using System;
using System.Buffers;
using System.IO;
using Xamarin.Android.AssemblyStore;

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

        static bool UncompressDLL(string filePath, string prefix, string? outputPath)
        {
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                return apkUncompress.UncompressDLL(fs, Path.GetFileName(filePath), prefix, outputPath);
            }
        }

        static bool UncompressFromAPK_IndividualEntries(ZipFile apk, string filePath, string assembliesPath, string prefix, string? outputPath)
        {
            foreach (ZipEntry entry in apk)
            {
                if (!entry.IsFile)
                {
                    continue;
                }

                if (!entry.Name.StartsWith(assembliesPath, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!entry.Name.EndsWith(".dll", StringComparison.Ordinal))
                {
                    continue;
                }

                using (var stream = new MemoryStream())
                {
                    byte[] buffer = new byte[4096]; // 4K is optimum
                    using (Stream zipStream = apk.GetInputStream(entry))
                    {
                        StreamUtils.Copy(zipStream, stream, buffer);
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    string fileName = entry.Name.Substring(assembliesPath.Length);
                    apkUncompress.UncompressDLL(stream, fileName, prefix, outputPath);
                }
            }

            return true;
        }

        static bool UncompressFromAPK_AssemblyStores(string filePath, string prefix, string? outputPath)
        {
            var explorer = new AssemblyStoreExplorer(filePath, keepStoreInMemory: true);
            foreach (AssemblyStoreAssembly assembly in explorer.Assemblies)
            {
                string assemblyName = assembly.DllName;

                if (!String.IsNullOrEmpty(assembly.Store.Arch))
                {
                    assemblyName = $"{assembly.Store.Arch}/{assemblyName}";
                }

                using (var stream = new MemoryStream())
                {
                    assembly.ExtractImage(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    apkUncompress.UncompressDLL(stream, assemblyName, prefix, outputPath);
                }
            }

            return true;
        }

        static bool UncompressFromAPK(string filePath, string assembliesPath, string? outputPath)
        {
            string prefix = $"uncompressed-{Path.GetFileNameWithoutExtension(filePath)}";
            string blobName = $"{assembliesPath}assemblies.blob";

            try
            {
                bool blobFound = false;
                ZipFile? zf = null;
                try
                {
                    FileStream fs = File.OpenRead(filePath);
                    zf = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue; // Ignore directories
                        }
                        if (string.Compare(zipEntry.Name, blobName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            blobFound = true;
                            break;
                        }
                    }
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }

                if (!blobFound)
                {
                    return UncompressFromAPK_IndividualEntries(zf, filePath, assembliesPath, prefix, outputPath);
                }

                return UncompressFromAPK_AssemblyStores(filePath, prefix, outputPath);
            }
            catch (Exception)
            {
                return false;
            }
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
                if (String.Compare(".dll", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!UncompressDLL(file, "uncompressed-", outputPath))
                    {
                        Console.WriteLine("Uncompress failed: {0}", file);
                        haveErrors = true;
                    }

                    Console.WriteLine("Uncompressed: {0}", file);
                    continue;
                }

                if (String.Compare(".apk", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!UncompressFromAPK(file, "assemblies/", outputPath))
                    {
                        Console.WriteLine("Uncompress failed: {0}", file);
                        haveErrors = true;
                    }

                    Console.WriteLine("Uncompressed: {0}", file);
                    continue;
                }

                if (String.Compare(".aab", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!UncompressFromAPK(file, "base/root/assemblies/", outputPath))
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
