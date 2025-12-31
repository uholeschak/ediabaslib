using CommandLine;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace CreateObb
{
    static class Program
    {
        public class Options
        {
            public Options()
            {
                InputDir = string.Empty;
                OutputFile = string.Empty;
                Key = string.Empty;
                Force = false;
            }

            [Option('i', "inputdir", Required = true, HelpText = "Input directory.")]
            public string InputDir { get; set; }

            [Option('o', "outputfile", Required = false, HelpText = "Output file")]
            public string OutputFile { get; set; }

            [Option('k', "key", Required = false, HelpText = "Key")]
            public string Key { get; set; }

            [Option('f', "force", Required = false, HelpText = "Force creation of output file")]
            public bool Force { get; set; }
        }

        static int Main(string[] args)
        {
            try
            {
                string inputDir = null;
                string outputFile = null;
                string key = null;
                bool force = false;
                bool hasErrors = false;

                Parser parser = new Parser(with =>
                {
                    //ignore case for enum values
                    with.CaseInsensitiveEnumValues = true;
                    with.EnableDashDash = true;
                    with.HelpWriter = Console.Out;
                });

                parser.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        inputDir = o.InputDir;
                        outputFile = o.OutputFile;
                        key = o.Key;
                        force = o.Force;
                    })
                    .WithNotParsed(errs =>
                    {
                        string errors = string.Join("\n", errs);
                        Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));
                        hasErrors = true;
                    });

                if (hasErrors)
                {
                    return 1;
                }

                if (string.IsNullOrEmpty(inputDir))
                {
                    Console.WriteLine("Input directory missing");
                    return 1;
                }

                if (!Directory.Exists(inputDir))
                {
                    Console.WriteLine("Input directory {0} not existing", inputDir);
                    return 1;
                }

                if (string.IsNullOrEmpty(outputFile))
                {
                    Console.WriteLine("Output file empty");
                    return 1;
                }

                if (force || !File.Exists(outputFile))
                {
                    if (!CreateContentFile(inputDir, Path.Combine(inputDir, "Content.xml")))
                    {
                        Console.WriteLine("Creating content file failed");
                        return 1;
                    }

                    if (!CreateZipFile(inputDir, outputFile, key))
                    {
                        Console.WriteLine("Creating Zip file failed");
                        return 1;
                    }

                    Console.WriteLine("Creating Zip file done");
                }
                else
                {
                    Console.WriteLine($"Output file {outputFile} already existing");
                }

                List<string> parts = SplitZipFile(outputFile, Path.GetDirectoryName(outputFile), 1024 * 1024 * 100);
                if (parts == null || parts.Count == 0)
                {
                    Console.WriteLine("Splitting Zip file failed");
                    return 1;
                }

                Console.WriteLine("Split files created: {0}", string.Join(", ", parts));
            }
            catch (Exception e)
            {
                Console.WriteLine("*** Exception: {0}", e.Message);
                return 1;
            }

            return 0;
        }

        private static bool IsValidFile(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);

                if (string.Compare(fileName, "enc_cne_1.prg", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }

                if (string.Compare(fileName, "sig_gis_1.prg", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return true;
        }

        private static bool CreateContentFile(string inDir, string outFile)
        {
            try
            {
                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }
                Uri uriIn = new Uri(inDir + Path.DirectorySeparatorChar);
                XElement xmlContentNodes = new XElement("content");
                string[] files = Directory.GetFiles(inDir, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (!IsValidFile(file))
                    {
                        Console.WriteLine("Deleting invalid file: {0}", file);
                        File.Delete(file);
                        continue;
                    }

                    XElement xmlFileNode = new XElement("file");

                    Uri uriFile = new Uri(file);
                    Uri uriRel = uriIn.MakeRelativeUri(uriFile);
                    string relPath = Uri.UnescapeDataString(uriRel.OriginalString);

                    xmlFileNode.Add(new XAttribute("name", relPath));

                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        xmlFileNode.Add(new XAttribute("size", fileInfo.Length));

                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(file))
                            {
                                byte[] md5Data = md5.ComputeHash(stream);
                                StringBuilder sb = new StringBuilder();
                                foreach (byte value in md5Data)
                                {
                                    sb.Append($"{value:X02}");
                                }
                                xmlFileNode.Add(new XAttribute("md5", sb.ToString()));
                            }
                        }
                    }
                    xmlContentNodes.Add(xmlFileNode);
                }
                XDocument document = new XDocument(xmlContentNodes);
                document.Save(outFile);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static bool CreateZipFile(string inDir, string outFile, string key)
        {
            try
            {
                Aes crypto = null;
                FileStream fsOut = null;
                ZipOutputStream zipStream = null;
                try
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        crypto = Aes.Create();
                        crypto.Mode = CipherMode.CBC;
                        crypto.Padding = PaddingMode.PKCS7;
                        crypto.KeySize = 256;

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                        using (var md5 = MD5.Create())
                        {
                            crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                    }

                    fsOut = File.Create(outFile);
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (crypto != null)
                    {
                        CryptoStream crStream = new CryptoStream(fsOut,
                            crypto.CreateEncryptor(), CryptoStreamMode.Write);
                        zipStream = new ZipOutputStream(crStream);
                    }
                    else
                    {
                        zipStream = new ZipOutputStream(fsOut);
                    }


                    zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                    // This setting will strip the leading part of the folder path in the entries, to
                    // make the entries relative to the starting folder.
                    // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                    int folderOffset = inDir.Length + (inDir.EndsWith("\\") ? 0 : 1);

                    CompressFolder(inDir, zipStream, folderOffset);
                }
                finally
                {
                    if (zipStream != null)
                    {
                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                        zipStream.Close();
                    }
                    fsOut?.Close();
                    crypto?.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static List<string> SplitZipFile(string inFile, string outDir, int maxSize)
        {
            try
            {
                List<string> splitFiles = new List<string>();
                string baseFileName = Path.GetFileNameWithoutExtension(inFile);

                // Delete existing files matching the pattern
                string[] existingFiles = Directory.GetFiles(outDir, $"{baseFileName}_*.bin");
                foreach (string existingFile in existingFiles)
                {
                    File.Delete(existingFile);
                }

                int fileIndex = 0;
                using (Stream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    while (inStream.Position < inStream.Length)
                    {
                        string outFileName = $"{baseFileName}_{fileIndex:D2}.bin";
                        string outFile = Path.Combine(outDir, outFileName);
                        using (Stream outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                        {
                            int bytesWritten = 0;
                            while (bytesWritten < maxSize && inStream.Position < inStream.Length)
                            {
                                int bytesToRead = Math.Min(buffer.Length, maxSize - bytesWritten);
                                int bytesRead = inStream.Read(buffer, 0, bytesToRead);
                                outStream.Write(buffer, 0, bytesRead);
                                bytesWritten += bytesRead;
                            }
                        }

                        splitFiles.Add(outFileName);
                        fileIndex++;
                    }
                }

                return splitFiles;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
    }
}
