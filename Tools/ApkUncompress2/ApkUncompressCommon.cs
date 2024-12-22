using System.Collections.Generic;
using System.IO;
using System;
using Xamarin.Android.AssemblyStore;

namespace ApkUncompress2;

public class ApkUncompressCommon
{
    public ApkUncompressCommon()
    {

    }

    public bool UncompressFromAPK(string filePath, string outputDir)
    {
        int extractedCount = 0;

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
                    foreach (AssemblyStoreItem storeItem in store.Assemblies)
                    {
                        Stream? stream = store.ReadImageData(storeItem);
                        if (stream == null)
                        {
                            continue;
                        }

                        string archName = store.TargetArch.HasValue ? store.TargetArch.Value.ToString().ToLowerInvariant() : "unknown";
                        string outFile = Path.Combine(outputDir, archName, storeItem.Name);
                        string? outDir = Path.GetDirectoryName(outFile);
                        if (string.IsNullOrEmpty(outDir))
                        {
                            continue;
                        }

                        Directory.CreateDirectory(outDir);
                        using (FileStream fileStream = File.Create(outFile))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.CopyTo(fileStream);
                        }
                        stream.Dispose();
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

        return extractedCount > 0;
    }
}
