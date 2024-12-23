using System.Collections.Generic;
using System.IO;
using System;
using Xamarin.Android.AssemblyStore;
using ICSharpCode.SharpZipLib.Core;
using K4os.Compression.LZ4;
using System.Text;
using System.Buffers;

namespace ApkUncompress2;

public class ApkUncompressCommon
{
    public delegate bool ProgressDelegate(int percent);

    private const int BufferSize = 4096;
    private const uint CompressedDataMagic = 0x5A4C4158; // 'XALZ', little-endian
    private readonly ArrayPool<byte> bytePool;

    public ApkUncompressCommon()
    {
        bytePool = ArrayPool<byte>.Shared;
    }

    public bool UncompressDLL(string fileName)
    {
        bool retVal = true;

        try
        {
            string tempFileName = fileName + ".tmp";
            bool uncompressed = false;
            using (FileStream inputStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                //
                // LZ4 compressed assembly header format:
                //   uint magic;                 // 0x5A4C4158; 'XALZ', little-endian
                //   uint descriptor_index;      // Index into an internal assembly descriptor table
                //   uint uncompressed_length;   // Size of assembly, uncompressed
                //
                using (BinaryReader reader = new BinaryReader(inputStream, new UTF8Encoding(false), true))
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
                            using (var fs = File.Open(tempFileName, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(assemblyBytes, 0, decoded);
                                fs.Flush();
                            }
                            uncompressed = true;
                        }

                        bytePool.Return(sourceBytes);
                        bytePool.Return(assemblyBytes);
                    }
                }
            }

            if (uncompressed)
            {
                File.Move(tempFileName, fileName, true);
            }
        }
        catch (Exception)
        {
            retVal = false;
        }

        return retVal;
    }

    public bool UncompressFromAPK(string filePath, string outputDir, ProgressDelegate? progressDelegate = null)
    {
        int extractedCount = 0;

        try
        {
            (FileFormat format, FileInfo? info) = Utils.DetectFileFormat(filePath);
            if (info == null)
            {
                return false;
            }

            (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) = AssemblyStoreExplorer.Open(filePath);
            if (explorers == null)
            {
                return false;
            }

            try
            {
                foreach (AssemblyStoreExplorer store in explorers)
                {
                    if (store.Assemblies != null)
                    {
                        int itemCount = store.Assemblies.Count;
                        int itemIndex = 0;

                        foreach (AssemblyStoreItem storeItem in store.Assemblies)
                        {
                            if (progressDelegate != null)
                            {
                                if (itemCount > 0)
                                {
                                    if (!progressDelegate((itemIndex * 100) / itemCount))
                                    {
                                        return false;
                                    }
                                }
                            }

                            itemIndex++;

                            string archName = store.TargetArch.HasValue ? store.TargetArch.Value.ToString().ToLowerInvariant() : "unknown";
                            string outFile = Path.Combine(outputDir, archName, storeItem.Name);

                            string? outDir = Path.GetDirectoryName(outFile);
                            if (string.IsNullOrEmpty(outDir))
                            {
                                continue;
                            }

                            Directory.CreateDirectory(outDir);
                            if (!store.StoreImageData(storeItem, outFile))
                            {
                                continue;
                            }

                            if (!UncompressDLL(outFile))
                            {
                                continue;
                            }

                            extractedCount++;
                        }
                    }
                }
            }
            finally
            {
                foreach (AssemblyStoreExplorer store in explorers)
                {
                    store.Dispose();
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return extractedCount > 0;
    }
}
