using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CreateObb
{
    static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No input directory specified");
                return 1;
            }
            if (args.Length < 2)
            {
                Console.WriteLine("No output file name specified");
                return 1;
            }

            string inDir = args[0];
            string outFile = args[1];
            if (string.IsNullOrEmpty(inDir))
            {
                Console.WriteLine("Input directory empty");
                return 1;
            }

            if (!Directory.Exists(inDir))
            {
                Console.WriteLine("Input directory not existing");
                return 1;
            }

            if (string.IsNullOrEmpty(outFile))
            {
                Console.WriteLine("Output file empty");
                return 1;
            }

            string key = string.Empty;
            if (args.Length >= 3)
            {
                key = args[2];
            }

            if (!File.Exists(outFile))
            {
                if (!CreateContentFile(inDir, Path.Combine(inDir, "Content.xml")))
                {
                    Console.WriteLine("Creating content file failed");
                    return 1;
                }

                if (!CreateZipFile(inDir, outFile, key))
                {
                    Console.WriteLine("Creating Zip file failed");
                    return 1;
                }

                Console.WriteLine("Creating Zip file done");
            }
            else
            {
                Console.WriteLine($"Output file {outFile} already existing");
            }

            int parts = SplitZipFile(outFile, Path.GetDirectoryName(outFile), 1024 * 1024 * 100);
            if (parts < 0)
            {
                Console.WriteLine("Splitting Zip file failed");
                return 1;
            }

            Console.WriteLine("Split zip file in {0} parts", parts);
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

        private static int SplitZipFile(string inFile, string outDir, int maxSize)
        {
            try
            {
                string baseFileName = Path.GetFileNameWithoutExtension(inFile);
                int fileIndex = 0;
                using (Stream inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    while (inStream.Position < inStream.Length)
                    {
                        string outFile = Path.Combine(outDir, $"{baseFileName}_part{fileIndex:D2}.bin");
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

                        fileIndex++;
                    }
                }

                return fileIndex;
            }
            catch (Exception)
            {
                return -1;
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
