using K4os.Compression.LZ4;
using System.IO;
using System;
using System.Buffers;

namespace ApkUncompress;

public class ApkUncompressCommon
{
    const uint CompressedDataMagic = 0x5A4C4158; // 'XALZ', little-endian

    readonly ArrayPool<byte> bytePool;

    public ApkUncompressCommon()
    {
        bytePool = ArrayPool<byte>.Shared;
    }

    public bool UncompressDLL(Stream inputStream, string filePath, string prefix, string? outputPath)
    {
        string outputFile = Path.Combine(prefix, filePath);
        if (!string.IsNullOrEmpty(outputPath))
        {
            outputFile = Path.Combine(outputPath, outputFile);
        }
        bool retVal = true;

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
                    retVal = false;
                }
                else
                {
                    string? outputDir = Path.GetDirectoryName(outputFile);
                    if (!String.IsNullOrEmpty(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    using (var fs = File.Open(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(assemblyBytes, 0, decoded);
                        fs.Flush();
                    }
                }

                bytePool.Return(sourceBytes);
                bytePool.Return(assemblyBytes);
            }
        }

        return retVal;
    }
}
