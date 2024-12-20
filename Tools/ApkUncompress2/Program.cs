using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Android.AssemblyStore;

namespace ApkUncompress2
{
    internal class App
    {
        static int Usage()
        {
            Console.WriteLine("Usage: decompress-assemblies {file.{apk,aab}} [{file.{apk,aab} ...]");
            Console.WriteLine();
            Console.WriteLine("DLL files passed on command line are uncompressed to the file directory with the `uncompressed-` prefix added to their name.");
            Console.WriteLine("DLL files from AAB/APK archives are uncompressed to a subdirectory of the file directory named after the archive with extension removed");
            return 1;
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                return Usage();
            }

            bool haveErrors = false;
            foreach (string inputFile in args)
            {
                (FileFormat format, FileInfo? info) = Utils.DetectFileFormat(inputFile);
                if (info == null)
                {
                    Console.WriteLine($"File '{inputFile}' does not exist.");
                    haveErrors = true;
                    continue;
                }

                (IList<AssemblyStoreExplorer>? explorers, string? errorMessage) = AssemblyStoreExplorer.Open(inputFile);
                if (explorers == null)
                {
                    Console.WriteLine(errorMessage ?? "Unknown error");
                    haveErrors = true;
                    continue;
                }
                
                string baseFileName = Path.GetFileNameWithoutExtension(inputFile);
                string? srcDir = Path.GetDirectoryName(inputFile);
                if (string.IsNullOrEmpty(srcDir))
                {
                    Console.WriteLine("Invalid directory");
                    haveErrors = true;
                    continue;
                }

                foreach (AssemblyStoreExplorer store in explorers)
                {
                    if (store.Assemblies != null)
                    {
                        foreach (AssemblyStoreItem storeItem in store.Assemblies)
                        {
                            Stream? stream = store.ReadImageData(storeItem);
                            if (stream == null)
                            {
                                Console.WriteLine($"Failed to read image data for {storeItem.Name}");
                                continue;
                            }

                            string archName = store.TargetArch.HasValue ? store.TargetArch.Value.ToString().ToLowerInvariant() : "unknown";
                            string outFile = Path.Combine(srcDir, baseFileName, archName, storeItem.Name);
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
                        }
                    }
                }

            }

            return haveErrors ? 1 : 0;
        }
    }
}
