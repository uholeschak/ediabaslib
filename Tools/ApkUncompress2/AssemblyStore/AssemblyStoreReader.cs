using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Xamarin.Android.Tools;

namespace Xamarin.Android.AssemblyStore;

// [UH] IDisposable added
abstract class AssemblyStoreReader : IDisposable
{
    private bool _disposed; // [UH] added
    static readonly UTF8Encoding ReaderEncoding = new UTF8Encoding(false);

    protected Stream StoreStream { get; }

    public abstract string Description { get; }
    public abstract bool NeedsExtensionInName { get; }
    public string StorePath { get; }

    public AndroidTargetArch TargetArch { get; protected set; } = AndroidTargetArch.Arm;
    public uint AssemblyCount { get; protected set; }
    public uint IndexEntryCount { get; protected set; }
    public IList<AssemblyStoreItem>? Assemblies { get; protected set; }
    public bool Is64Bit { get; protected set; }

    protected AssemblyStoreReader(Stream store, string path)
    {
        StoreStream = store;
        StorePath = path;
    }

    public static AssemblyStoreReader? Create(Stream store, string path)
    {
        AssemblyStoreReader? reader = MakeReaderReady(new StoreReader_V1(store, path));
        if (reader != null)
        {
            return reader;
        }

        reader = MakeReaderReady(new StoreReader_V2(store, path));
        if (reader != null)
        {
            return reader;
        }

        return null;
    }

    static AssemblyStoreReader? MakeReaderReady(AssemblyStoreReader reader)
    {
        if (!reader.IsSupported())
        {
            return null;
        }

        reader.Prepare();
        return reader;
    }

    protected BinaryReader CreateReader() => new BinaryReader(StoreStream, ReaderEncoding, leaveOpen: true);

    protected abstract bool IsSupported();
    protected abstract void Prepare();
    protected abstract ulong GetStoreStartDataOffset();

    public Stream ReadEntryImageData(AssemblyStoreItem entry, bool uncompressIfNeeded = false)
    {
        ulong startOffset = GetStoreStartDataOffset();
        StoreStream.Seek((uint)startOffset + entry.DataOffset, SeekOrigin.Begin);
        var stream = new MemoryStream();

        if (uncompressIfNeeded)
        {
            throw new NotImplementedException();
        }

        const long BufferSize = 65535;
        byte[] buffer = Utils.BytePool.Rent((int)BufferSize);
        long remainingToRead = entry.DataSize;

        while (remainingToRead > 0)
        {
            int nread = StoreStream.Read(buffer, 0, (int)Math.Min(BufferSize, remainingToRead));
            stream.Write(buffer, 0, nread);
            remainingToRead -= (long)nread;
        }
        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    // [UH] Added
    public bool StoreEntryImageData(AssemblyStoreItem entry, string fileName)
    {
        try
        {
            ulong startOffset = GetStoreStartDataOffset();
            StoreStream.Seek((uint)startOffset + entry.DataOffset, SeekOrigin.Begin);
            using (FileStream stream = File.Create(fileName))
            {
                const long BufferSize = 65535;
                byte[] buffer = Utils.BytePool.Rent((int)BufferSize);
                long remainingToRead = entry.DataSize;

                while (remainingToRead > 0)
                {
                    int nread = StoreStream.Read(buffer, 0, (int)Math.Min(BufferSize, remainingToRead));
                    stream.Write(buffer, 0, nread);
                    remainingToRead -= (long)nread;
                }
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // [UH] Added
    public void Dispose()
    {
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    // [UH] Added
    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                StoreStream?.Dispose();
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }

}
