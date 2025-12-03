using System;
using System.IO;
using System.Buffers;

using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ICSharpCode.SharpZipLib.Zip;

namespace Xamarin.Android.AssemblyStore;

static class Utils
{
    static readonly string[] aabZipEntries = {
        "base/manifest/AndroidManifest.xml",
        "BundleConfig.pb",
    };

    static readonly string[] aabBaseZipEntries = {
        "manifest/AndroidManifest.xml",
    };

    static readonly string[] apkZipEntries = {
        "AndroidManifest.xml",
    };

    public const uint ZIP_MAGIC = 0x4034b50;
    public const uint ASSEMBLY_STORE_MAGIC = 0x41424158;
    public const uint ELF_MAGIC = 0x464c457f;

    public static readonly ArrayPool<byte> BytePool = ArrayPool<byte>.Shared;

    public static (ulong offset, ulong size, ELFPayloadError error) FindELFPayloadSectionOffsetAndSize(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        Class elfClass = ELFReader.CheckELFType(stream);
        if (elfClass == Class.NotELF)
        {
            return ReturnError(null, ELFPayloadError.NotELF);
        }

        if (!ELFReader.TryLoad(stream, shouldOwnStream: false, out IELF? elf))
        {
            return ReturnError(elf, ELFPayloadError.LoadFailed);
        }

        if (elf.Type != FileType.SharedObject)
        {
            return ReturnError(elf, ELFPayloadError.NotSharedLibrary);
        }

        if (elf.Endianess != ELFSharp.Endianess.LittleEndian)
        {
            return ReturnError(elf, ELFPayloadError.NotLittleEndian);
        }

        if (!elf.TryGetSection("payload", out ISection? payloadSection))
        {
            return ReturnError(elf, ELFPayloadError.NoPayloadSection);
        }

        bool is64 = elf.Machine switch
        {
            Machine.ARM => false,
            Machine.Intel386 => false,

            Machine.AArch64 => true,
            Machine.AMD64 => true,

            _ => throw new NotSupportedException($"Unsupported ELF architecture '{elf.Machine}'")
        };

        ulong offset;
        ulong size;

        if (is64)
        {
            (offset, size) = GetOffsetAndSize64((Section<ulong>)payloadSection);
        }
        else
        {
            (offset, size) = GetOffsetAndSize32((Section<uint>)payloadSection);
        }

        elf.Dispose();
        return (offset, size, ELFPayloadError.None);

        (ulong offset, ulong size) GetOffsetAndSize64(Section<ulong> payload)
        {
            return (payload.Offset, payload.Size);
        }

        (ulong offset, ulong size) GetOffsetAndSize32(Section<uint> payload)
        {
            return ((ulong)payload.Offset, (ulong)payload.Size);
        }

        (ulong offset, ulong size, ELFPayloadError error) ReturnError(IELF? elf, ELFPayloadError error)
        {
            elf?.Dispose();

            return (0, 0, error);
        }
    }

    public static (FileFormat format, FileInfo? info) DetectFileFormat (string path)
	{
		if (String.IsNullOrEmpty (path)) {
			return (FileFormat.Unknown, null);
		}

		var info = new FileInfo (path);
		if (!info.Exists) {
			return (FileFormat.Unknown, null);
		}

		using var reader = new BinaryReader (info.OpenRead ());

		// ATM, all formats we recognize have 4-byte magic at the start
		FileFormat format = reader.ReadUInt32 () switch {
			Utils.ZIP_MAGIC            => FileFormat.Zip,
			Utils.ELF_MAGIC            => FileFormat.ELF,
			Utils.ASSEMBLY_STORE_MAGIC => FileFormat.AssemblyStore,
			_                          => FileFormat.Unknown
		};

		if (format == FileFormat.Unknown || format != FileFormat.Zip) {
			return (format, info);
		}

		return (DetectAndroidArchive (info, format), info);
	}

	static FileFormat DetectAndroidArchive (FileInfo info, FileFormat defaultFormat)
	{
        ZipFile? zf = null;
        try
        {
            FileStream fs = File.OpenRead(info.FullName);
            zf = new ZipFile(fs);

            if (HasAllEntries(zf, aabZipEntries))
            {
                return FileFormat.Aab;
            }

            if (HasAllEntries(zf, apkZipEntries))
            {
                return FileFormat.Apk;
            }

            if (HasAllEntries(zf, aabBaseZipEntries))
            {
                return FileFormat.AabBase;
            }
        }
        catch (Exception)
        {
            return defaultFormat;
        }
        finally
        {
            if (zf != null)
            {
                zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                zf.Close(); // Ensure we release resources
            }
        }

        return defaultFormat;
	}

    static bool HasAllEntries (ZipFile zf, string[] entries)
    {
        foreach (string entry in entries)
        {
            bool found = false;
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile)
                {
                    continue; // Ignore directories
                }

                if (string.Compare(zipEntry.Name, entry, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
        }

        return true;
    }
}
