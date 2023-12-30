using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using K4os.Compression.LZ4;
using System;
using System.Buffers;
using System.IO;
using Xamarin.Android.AssemblyStore;

namespace Xamarin.Android.Tools.DecompressAssemblies
{
    internal class App
    {
        const uint CompressedDataMagic = 0x5A4C4158; // 'XALZ', little-endian

        static readonly ArrayPool<byte> bytePool = ArrayPool<byte>.Shared;

        static int Usage()
        {
            Console.WriteLine("Usage: decompress-assemblies {file.{dll,apk,aab}} [{file.{dll,apk,aab} ...]");
            Console.WriteLine();
            Console.WriteLine("DLL files passed on command line are uncompressed to the current directory with the `uncompressed-` prefix added to their name.");
            Console.WriteLine("DLL files from AAB/APK archives are uncompressed to a subdirectory of the current directory named after the archive with extension removed");
            return 1;
        }

        static bool UncompressDLL(Stream inputStream, string fileName, string filePath, string prefix)
        {
            string outputFile = $"{prefix}{filePath}";
            bool retVal = true;

            Console.WriteLine($"Processing {fileName}");
            //
            // LZ4 compressed assembly header format:
            //   uint magic;                 // 0x5A4C4158; 'XALZ', little-endian
            //   uint descriptor_index;      // Index into an internal assembly descriptor table
            //   uint uncompressed_length;   // Size of assembly, uncompressed
            //
            using (var reader = new BinaryReader(inputStream))
            {
                uint magic = reader.ReadUInt32();
                if (magic == CompressedDataMagic)
                {
                    reader.ReadUInt32(); // descriptor index, ignore
                    uint decompressedLength = reader.ReadUInt32();

                    int inputLength = (int)(inputStream.Length - 12);
                    byte[] sourceBytes = bytePool.Rent(inputLength);
                    reader.Read(sourceBytes, 0, inputLength);

                    byte[] assemblyBytes = bytePool.Rent((int)decompressedLength);
                    int decoded = LZ4Codec.Decode(sourceBytes, 0, inputLength, assemblyBytes, 0, (int)decompressedLength);
                    if (decoded != (int)decompressedLength)
                    {
                        Console.Error.WriteLine($"  Failed to decompress LZ4 data of {fileName} (decoded: {decoded})");
                        retVal = false;
                    }
                    else
                    {
                        string outputDir = Path.GetDirectoryName(outputFile);
                        if (!String.IsNullOrEmpty(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                        }
                        using (var fs = File.Open(outputFile, FileMode.Create, FileAccess.Write))
                        {
                            fs.Write(assemblyBytes, 0, decoded);
                            fs.Flush();
                        }
                        Console.WriteLine($"  uncompressed to: {outputFile}");
                    }

                    bytePool.Return(sourceBytes);
                    bytePool.Return(assemblyBytes);
                }
                else
                {
                    Console.WriteLine($"  assembly is not compressed");
                }
            }

            return retVal;
        }

        static bool UncompressDLL(string filePath, string prefix)
        {
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                return UncompressDLL(fs, filePath, Path.GetFileName(filePath), prefix);
            }
        }

        static bool UncompressFromAPK_IndividualEntries(ZipFile apk, string filePath, string assembliesPath, string prefix)
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
                    UncompressDLL(stream, $"{filePath}!{entry.Name}", fileName, prefix);
                }
            }

            return true;
        }

        static bool UncompressFromAPK_AssemblyStores(string filePath, string prefix)
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
                    UncompressDLL(stream, $"{filePath}!{assemblyName}", assemblyName, prefix);
                }
            }

            return true;
        }

        static bool UncompressFromAPK(string filePath, string assembliesPath)
        {
            string prefix = $"uncompressed-{Path.GetFileNameWithoutExtension(filePath)}{Path.DirectorySeparatorChar}";
            string blobName = $"{assembliesPath}assemblies.blob";

            try
            {
                bool blobFound = false;
                ZipFile zf = null;
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
                    return UncompressFromAPK_IndividualEntries(zf, filePath, assembliesPath, prefix);
                }

                return UncompressFromAPK_AssemblyStores(filePath, prefix);
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
                if (String.Compare(".dll", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!UncompressDLL(file, "uncompressed-"))
                    {
                        haveErrors = true;
                    }
                    continue;
                }

                if (String.Compare(".apk", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!UncompressFromAPK(file, "assemblies/"))
                    {
                        haveErrors = true;
                    }
                    continue;
                }

                if (String.Compare(".aab", ext, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!UncompressFromAPK(file, "base/root/assemblies/"))
                    {
                        haveErrors = true;
                    }
                    continue;
                }
            }

            return haveErrors ? 1 : 0;
        }
    }
}
