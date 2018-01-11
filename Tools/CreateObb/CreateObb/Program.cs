using System;
using System.IO;
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

            if (!CreateZipFile(inDir, outFile))
            {
                Console.WriteLine("Creating Zip file failed");
                return 1;
            }

            return 0;
        }

        private static bool CreateZipFile(string inDir, string outFile)
        {
            try
            {
                FileStream fsOut = File.Create(outFile);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);

                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                try
                {
                    // This setting will strip the leading part of the folder path in the entries, to
                    // make the entries relative to the starting folder.
                    // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                    int folderOffset = inDir.Length + (inDir.EndsWith("\\") ? 0 : 1);

                    CompressFolder(inDir, zipStream, folderOffset);
                }
                finally
                {
                    zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
