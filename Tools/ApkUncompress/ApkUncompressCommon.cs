using K4os.Compression.LZ4;
using System.IO;
using System;
using System.Buffers;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Xamarin.Android.AssemblyStore;

namespace ApkUncompress;

public class ApkUncompressCommon
{
    public const string AssembliesPathApk = "assemblies/";
    public const string AssembliesPathAab = "base/root/assemblies/";

    private const uint CompressedDataMagic = 0x5A4C4158; // 'XALZ', little-endian
    private readonly ArrayPool<byte> bytePool;

    public ApkUncompressCommon()
    {
        bytePool = ArrayPool<byte>.Shared;
    }

    public bool UncompressDLL(Stream inputStream, string filePath, string prefix, string? outputPath)
    {
        string outputFile = filePath;
        if (!string.IsNullOrEmpty(prefix))
        {
            outputFile = Path.Combine(prefix, outputFile);
        }

        if (!string.IsNullOrEmpty(outputPath))
        {
            outputFile = Path.Combine(outputPath, outputFile);
        }
        bool retVal = true;

        string? outputDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        //
        // LZ4 compressed assembly header format:
        //   uint magic;                 // 0x5A4C4158; 'XALZ', little-endian
        //   uint descriptor_index;      // Index into an internal assembly descriptor table
        //   uint uncompressed_length;   // Size of assembly, uncompressed
        //
        using (BinaryReader reader = new BinaryReader(inputStream, new UTF8Encoding(), true))
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
                    retVal = false;
                }
                else
                {
                    using (var fs = File.Open(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(assemblyBytes, 0, decoded);
                        fs.Flush();
                    }
                }

                bytePool.Return(sourceBytes);
                bytePool.Return(assemblyBytes);

                return retVal;
            }
        }

        using (var fs = File.Open(outputFile, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[4096]; // 4K is optimum
            inputStream.Seek(0, SeekOrigin.Begin);
            StreamUtils.Copy(inputStream, fs, buffer);
        }

        return retVal;
    }

    public bool UncompressDLL(string filePath, string prefix, string? outputPath)
    {
        using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
            return UncompressDLL(fs, Path.GetFileName(filePath), prefix, outputPath);
        }
    }

    public bool UncompressFromAPK_IndividualEntries(ZipFile apk, string filePath, string assembliesPath, string prefix, string? outputPath)
    {
        bool result = true;
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
                if (!UncompressDLL(stream, fileName, prefix, outputPath))
                {
                    result = false;
                }
            }
        }

        return result;
    }

    public bool UncompressFromAPK_AssemblyStores(string filePath, string prefix, string? outputPath)
    {
        AssemblyStoreExplorer explorer = new AssemblyStoreExplorer(filePath, keepStoreInMemory: true);
        foreach (AssemblyStoreAssembly assembly in explorer.Assemblies)
        {
            string assemblyName = assembly.DllName;

            if (!String.IsNullOrEmpty(assembly.Store.Arch))
            {
                assemblyName = Path.Combine(assembly.Store.Arch, assemblyName);
            }

            using (var stream = new MemoryStream())
            {
                assembly.ExtractImage(stream);
                stream.Seek(0, SeekOrigin.Begin);
                UncompressDLL(stream, assemblyName, prefix, outputPath);
            }
        }

        return true;
    }

    public bool UncompressFromAPK(string filePath, string assembliesPath, string prefix, string? outputPath)
    {
        string blobName = $"{assembliesPath}assemblies.blob";

        try
        {
            ZipFile? zf = null;
            try
            {
                bool blobFound = false;
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

                if (!blobFound)
                {
                    return UncompressFromAPK_IndividualEntries(zf, filePath, assembliesPath, prefix, outputPath);
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

            return UncompressFromAPK_AssemblyStores(filePath, prefix, outputPath);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
