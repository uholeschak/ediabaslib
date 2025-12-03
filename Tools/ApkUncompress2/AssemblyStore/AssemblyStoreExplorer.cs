using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Xamarin.Android.Tools;

namespace Xamarin.Android.AssemblyStore;

// [UH] IDisposable added
class AssemblyStoreExplorer : IDisposable
{
    private bool _disposed; // [UH] added

    readonly AssemblyStoreReader reader;

    public string StorePath { get; }
    public AndroidTargetArch? TargetArch { get; }
    public uint AssemblyCount { get; }
    public uint IndexEntryCount { get; }
    public IList<AssemblyStoreItem>? Assemblies { get; }
    public IDictionary<string, AssemblyStoreItem>? AssembliesByName { get; }
    public bool Is64Bit { get; }

    protected AssemblyStoreExplorer(Stream storeStream, string path)
    {
        StorePath = path;
        var storeReader = AssemblyStoreReader.Create(storeStream, path);
        if (storeReader == null)
        {
            storeStream.Dispose();
            throw new NotSupportedException($"Format of assembly store '{path}' is unsupported");
        }

        reader = storeReader;
        TargetArch = reader.TargetArch;
        AssemblyCount = reader.AssemblyCount;
        IndexEntryCount = reader.IndexEntryCount;
        Assemblies = reader.Assemblies;
        Is64Bit = reader.Is64Bit;

        var dict = new Dictionary<string, AssemblyStoreItem>(StringComparer.Ordinal);
        foreach (AssemblyStoreItem item in Assemblies ?? [])
        {
            dict.Add(item.Name, item);
        }
        AssembliesByName = dict.AsReadOnly();
    }

    protected AssemblyStoreExplorer(FileInfo storeInfo)
        : this(storeInfo.OpenRead(), storeInfo.FullName)
    { }

    public static (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) Open(string inputFile)
    {
        (FileFormat format, FileInfo? info) = Utils.DetectFileFormat(inputFile);
        if (info == null)
        {
            return (null, $"File '{inputFile}' does not exist.");
        }

        switch (format)
        {
            case FileFormat.Unknown:
                return (null, $"File '{inputFile}' has an unknown format.");

            case FileFormat.Zip:
                return (null, $"File '{inputFile}' is a ZIP archive, but not an Android one.");

            case FileFormat.AssemblyStore:
            case FileFormat.ELF:
                return (new List<AssemblyStoreExplorer> { new AssemblyStoreExplorer(info) }, null);

            case FileFormat.Aab:
                return OpenAab(info);

            case FileFormat.AabBase:
                return OpenAabBase(info);

            case FileFormat.Apk:
                return OpenApk(info);

            default:
                return (null, $"File '{inputFile}' has an unsupported format '{format}'");
        }
    }

    static (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) OpenAab(FileInfo fi)
    {
        return OpenCommon(
            fi,
            new List<IList<string>> {
                StoreReader_V2.AabPaths,
                StoreReader_V1.AabPaths,
            }
        );
    }

    static (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) OpenAabBase(FileInfo fi)
    {
        return OpenCommon(
            fi,
            new List<IList<string>> {
                StoreReader_V2.AabBasePaths,
                StoreReader_V1.AabBasePaths,
            }
        );
    }

    static (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) OpenApk(FileInfo fi)
    {
        return OpenCommon(
            fi,
            new List<IList<string>> {
                StoreReader_V2.ApkPaths,
                StoreReader_V1.ApkPaths,
            }
        );
    }

    // [UH] modified
    static (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) OpenCommon (FileInfo fi, List<IList<string>> pathLists)
    {
        ZipFile? zf = null;
        try
        {
            FileStream fs = File.OpenRead(fi.FullName);
            zf = new ZipFile(fs);

            IList<AssemblyStoreExplorer>? explorers;
            string? errorMessage;
            bool pathsFound;

            foreach (IList<string> paths in pathLists)
            {
                (explorers, errorMessage, pathsFound) = TryLoad(fi, zf, paths);
                if (pathsFound)
                {
                    return (explorers, errorMessage);
                }
            }

            return (null, "Unable to find any blob entries");
        }
        catch (Exception ex)
        {
            return (null, $"Exception {ex.Message}");
        }
        finally
        {
            if (zf != null)
            {
                zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                zf.Close(); // Ensure we release resources
            }
        }
    }

    // [UH] modified
    static (IList<AssemblyStoreExplorer>? explorers, string? errorMessage, bool pathsFound) TryLoad (FileInfo fi, ZipFile zf, IList<string> paths)
    {
        var ret = new List<AssemblyStoreExplorer> ();

        foreach (string path in paths)
        {
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile)
                {
                    continue; // Ignore directories
                }

                if (string.Compare(zipEntry.Name, path, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    try
                    {
                        const int bufferSize = 4096;
                        string tempFileName = Path.GetTempFileName();
                        FileStream fileStream = File.Create(tempFileName, bufferSize, FileOptions.DeleteOnClose);

                        byte[] buffer = new byte[bufferSize];
                        using (Stream zipStream = zf.GetInputStream(zipEntry))
                        {
                            StreamUtils.Copy(zipStream, fileStream, buffer);
                        }

                        fileStream.Seek(0, SeekOrigin.Begin);
                        ret.Add(new AssemblyStoreExplorer(fileStream, $"{fi.FullName}!{path}"));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    break;
                }
            }
        }

        if (ret.Count == 0) {
            return (null, null, false);
        }

        return (ret, null, true);
    }

    public Stream? ReadImageData(AssemblyStoreItem item, bool uncompressIfNeeded = false)
    {
        return reader.ReadEntryImageData(item, uncompressIfNeeded);
    }

    // [UH] added
    public bool StoreImageData(AssemblyStoreItem item, string fileName)
    {
        return reader.StoreEntryImageData(item, fileName);
    }

    string EnsureCorrectAssemblyName(string assemblyName)
    {
        assemblyName = Path.GetFileName(assemblyName);
        if (reader.NeedsExtensionInName)
        {
            if (!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return $"{assemblyName}.dll";
            }
        }
        else
        {
            if (assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileNameWithoutExtension(assemblyName);
            }
        }

        return assemblyName;
    }

    public IList<AssemblyStoreItem>? Find(string assemblyName, AndroidTargetArch? targetArch = null)
    {
        if (Assemblies == null)
        {
            return null;
        }

        assemblyName = EnsureCorrectAssemblyName(assemblyName);
        var items = new List<AssemblyStoreItem>();
        foreach (AssemblyStoreItem item in Assemblies)
        {
            if (String.CompareOrdinal(assemblyName, item.Name) != 0)
            {
                continue;
            }

            if (targetArch != null && item.TargetArch != targetArch)
            {
                continue;
            }

            items.Add(item);
        }

        if (items.Count == 0)
        {
            return null;
        }

        return items;
    }

    public bool Contains(string assemblyName, AndroidTargetArch? targetArch = null)
    {
        IList<AssemblyStoreItem>? items = Find(assemblyName, targetArch);
        if (items == null || items.Count == 0)
        {
            return false;
        }

        return true;
    }

    // [UH] added
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

    // [UH] added
    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                reader?.Dispose();
            }

            // Note disposing has been done.
            _disposed = true;
        }
    }

}
