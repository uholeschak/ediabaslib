using System.Collections.Generic;
using System.IO;
using System;
using Xamarin.Android.AssemblyStore;

namespace ApkUncompress2;

public class ApkUncompressCommon
{
    public delegate bool ProgressDelegate(int percent);

    public ApkUncompressCommon()
    {
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
