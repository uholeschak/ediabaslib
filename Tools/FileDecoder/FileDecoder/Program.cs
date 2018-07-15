//#define VERSION_17_1_3
//#define VERSION_17_8_0
//#define VERSION_17_8_1
#define VERSION_18_2_1

#if !VERSION_17_1_3
#define ZIPLIB_SUPPORT
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace FileDecoder
{
    static class Program
    {
        enum ResultCode
        {
            Ok,
            Error,
            Done,
        }

        // from string resource 383;
#if VERSION_18_2_1
        private const string TypeCodeString = "ccc49968f325f16b"; // DRV 18.2.1
        static readonly UInt64[] DecryptTypeCodeData = { 0x22, 0x46, 0x02, 0x1221 };
#endif
#if VERSION_17_8_0
        private const string TypeCodeString = "6da97491a5097b22"; // DRV 17.8.0
        static readonly UInt64[] DecryptTypeCodeData = { 0x18, 0x9B, 0x6B, 0x1180 };
#endif
#if VERSION_17_8_1
        private const string TypeCodeString = "9fa5bd574f2c23f5";   // RUS 17.8.1
        static readonly UInt64[] DecryptTypeCodeData = { 0x18, 0x9C, 0x6C, 0x1181 };
#endif
#if VERSION_17_1_3
        // 17.1.3
        private const string TypeCodeString = "f406626d5727b505";   // RUS
        static readonly UInt64[] DecryptTypeCodeData = { 0x11, 0x27, 0x05, 0x1113 };
#endif

        static readonly byte[] CryptTabIndex = { 0x23, 0x12, 0x21, 0x42, 0x51, 0x14, 0x47, 0x67 };
        static readonly byte[] CryptTab1 =
        {
            0x56, 0x15, 0x8F, 0xDB, 0xC5, 0x2A, 0x05, 0x7D, 0x19, 0xC8, 0xE5, 0x36, 0xA4, 0xB6, 0x91, 0x5E,
            0x47, 0x36, 0x36, 0x52, 0x1C, 0x73, 0x11, 0x00, 0x20, 0x9D, 0xC9, 0x59, 0x8A, 0x7A, 0x76, 0x63,
            0xE7, 0x7E, 0x74, 0xB2, 0x2B, 0xCE, 0xE4, 0x96, 0x92, 0xB1, 0xB7, 0x3C, 0xE8, 0x8D, 0x25, 0x2F,
            0x6B, 0x4B, 0x0C, 0xC0, 0x95, 0x05, 0x18, 0x2D, 0x08, 0x47, 0x1C, 0x5F, 0xE1, 0x90, 0x29, 0x3F,
            0x69, 0x9F, 0x98, 0x38, 0x80, 0x5A, 0x1B, 0x4D, 0x3A, 0x81, 0x39, 0xF7, 0xB4, 0x63, 0x6E, 0xD0,
            0x33, 0x4E, 0x4B, 0x76, 0x13, 0x93, 0x1F, 0x74, 0x45, 0x62, 0x6E, 0x38, 0x91, 0xBB, 0x2A, 0xC4,
            0xE5, 0x4E, 0xD1, 0xBA, 0x00, 0xAB, 0x3E, 0x8A, 0x06, 0x28, 0x93, 0xB2, 0xF8, 0x3D, 0xF2, 0x6C,
            0x36, 0x20, 0x72, 0xF7, 0x67, 0xAE, 0x8F, 0xD5, 0x19, 0xB8, 0xCB, 0xAA, 0x54, 0x3F, 0x70, 0x98,
            0xC4, 0xD6, 0x7C, 0x03, 0x23, 0x3E, 0x1A, 0xC1, 0x98, 0xBB, 0xBD, 0x6D, 0xBF, 0x6E, 0x70, 0xF5,
            0x4D, 0x57, 0x07, 0x3E, 0x6E, 0xC6, 0x42, 0xF6, 0x72, 0xFA, 0x1C, 0x97, 0x5B, 0x10, 0xF3, 0x51,
            0xD0, 0x3C, 0x5C, 0x36, 0x62, 0x8B, 0x76, 0xD7, 0x22, 0x52, 0x38, 0xBB, 0x0A, 0x47, 0x72, 0x35,
            0x36, 0xAE, 0x60, 0x35, 0x20, 0xEC, 0x94, 0x76, 0x5E, 0xD4, 0x28, 0xC5, 0x7B, 0x3A, 0x57, 0x02,
            0x1D, 0x80, 0xA3, 0x97, 0xBD, 0x93, 0x53, 0x58, 0xB5, 0x1D, 0x9C, 0xB5, 0x62, 0x88, 0x78, 0xB6,
            0x0F, 0xF2, 0x3B, 0xC3, 0x14, 0x0A, 0x1E, 0x84, 0x35, 0xFF, 0x54, 0x4A, 0x7B, 0x08, 0x81, 0x81,
            0x43, 0x6F, 0x70, 0x79, 0x72, 0x69, 0x67, 0x68, 0x74, 0x28, 0x63, 0x29, 0x20, 0x32, 0x30, 0x30,
            0x34, 0x2C, 0x20, 0x52, 0x6F, 0x73, 0x73, 0x2D, 0x54, 0x65, 0x63, 0x68, 0x20, 0x4C, 0x4C, 0x43 
        };
        static readonly byte[] CryptTab2 =
        {
            0x9D, 0xEF, 0x69, 0xD9, 0x63, 0xE2, 0xFF, 0x0E, 0x21, 0xD3, 0xEA, 0x50, 0x8F, 0x29, 0x47, 0x57,
            0x59, 0x70, 0xC3, 0x31, 0x22, 0x4F, 0xB9, 0xDA, 0xC9, 0xBA, 0x9B, 0x0C, 0xEB, 0x68, 0x93, 0x58,
            0xA9, 0x88, 0x6A, 0x76, 0xAB, 0xB0, 0xA8, 0xDA, 0x64, 0x16, 0xFD, 0x87, 0x98, 0x02, 0xDF, 0x28,
            0x1C, 0x83, 0x75, 0x5D, 0xDE, 0x62, 0xB4, 0x53, 0x25, 0x54, 0xC8, 0x97, 0x19, 0x73, 0xB5, 0x2C,
            0x85, 0xEF, 0x3E, 0xDB, 0xDC, 0x01, 0x06, 0xCA, 0x7A, 0x21, 0xF5, 0x50, 0x2D, 0x79, 0xDD, 0x08,
            0xF4, 0x99, 0x5D, 0x24, 0x06, 0x6C, 0x06, 0x04, 0x16, 0x6B, 0xBD, 0x08, 0xD6, 0x10, 0x60, 0xA2,
            0xBA, 0x8E, 0xAD, 0xAF, 0xFE, 0xBF, 0x5F, 0x07, 0xEA, 0x5E, 0x99, 0x52, 0x55, 0x76, 0x88, 0x1E,
            0x68, 0x1A, 0x45, 0x2F, 0xA4, 0x57, 0xF8, 0x16, 0x26, 0x67, 0x43, 0x06, 0x2A, 0x27, 0xDE, 0xE2,
            0xCF, 0xCC, 0x7E, 0x9A, 0x1A, 0xD1, 0xFC, 0xB8, 0x3C, 0x33, 0xB3, 0x36, 0x18, 0xE1, 0x2A, 0x93,
            0x00, 0x6E, 0xF3, 0x26, 0xBF, 0x0A, 0xD2, 0xB1, 0xDD, 0xB0, 0x22, 0x39, 0x1E, 0xA1, 0x75, 0x15,
            0x4C, 0x10, 0x7B, 0x46, 0x36, 0x1F, 0x25, 0x06, 0xF9, 0x0A, 0x09, 0xA3, 0x7E, 0xA2, 0x09, 0x8E,
            0x44, 0xFE, 0x2F, 0xAF, 0x60, 0x6D, 0xDC, 0xFC, 0xC2, 0xAE, 0x22, 0x4A, 0xB9, 0x64, 0x6E, 0x63,
            0xBA, 0xC4, 0x6A, 0x58, 0x5D, 0x91, 0x21, 0x19, 0xA8, 0x4A, 0x65, 0x42, 0x90, 0xA2, 0x6D, 0x38,
            0xBD, 0x30, 0xC3, 0x75, 0x8E, 0x68, 0x5E, 0x20, 0x5D, 0xCA, 0x0A, 0xE1, 0x04, 0x59, 0x11, 0xF3,
            0x9F, 0x4F, 0x14, 0x7B, 0x94, 0x0F, 0x3A, 0x18, 0xD1, 0x5B, 0x8D, 0xBB, 0x55, 0xC7, 0xA0, 0xB9,
            0xF2, 0x6D, 0x75, 0x1E, 0x51, 0xE3, 0x9F, 0x45, 0x36, 0x6A, 0xA4, 0xA5, 0x05, 0x68, 0xA5, 0xEE,
            0x86, 0x19, 0x41, 0x55, 0xE5, 0x81, 0xB6, 0x2B, 0xFD, 0xA5, 0x49, 0xB5, 0xD5, 0xFA, 0xE9, 0x38,
            0x6B, 0x1E, 0x0F, 0x53, 0xB1, 0xC6, 0xE8, 0x91, 0xD6, 0xF8, 0xB6, 0x3F, 0xC6, 0x7A, 0x74, 0x7B,
            0xF4, 0x8A, 0xB8, 0x8E, 0x57, 0xCF, 0xDE, 0x7C, 0xB2, 0x90, 0x63, 0xD9, 0x19, 0x24, 0x8F, 0xDD,
            0xB1, 0xA9, 0x57, 0xBB, 0xB7, 0xF9, 0x81, 0x2F, 0xC4, 0xDA, 0x09, 0x56, 0x4F, 0x75, 0xC4, 0xC3,
            0x73, 0x0A, 0x43, 0xCF, 0xF2, 0xE1, 0xFA, 0x30, 0x7B, 0x84, 0xA1, 0xCE, 0x28, 0x2B, 0xDB, 0xD2,
            0x4B, 0x78, 0x16, 0xFF, 0x6A, 0x64, 0xB2, 0x45, 0x88, 0x7A, 0x65, 0x93, 0xA6, 0x43, 0xDE, 0xEE,
            0x8A, 0x01, 0xA8, 0xC0, 0xBF, 0x9F, 0x52, 0x72, 0xDD, 0xE9, 0xCD, 0x3C, 0x0B, 0xF9, 0x15, 0x3C,
            0xC1, 0xF1, 0x13, 0xC6, 0xD3, 0xF0, 0xC3, 0xFC, 0xAB, 0x3E, 0x92, 0x9E, 0xD5, 0xCA, 0x0A, 0x23,
            0xC2, 0xD7, 0xB0, 0x08, 0xC5, 0xF2, 0x2D, 0x68, 0x62, 0x27, 0xAD, 0xCD, 0xC8, 0x74, 0x85, 0x46,
            0x9C, 0x7E, 0x18, 0xB9, 0xF8, 0x83, 0xFB, 0x7B, 0xB4, 0x90, 0x57, 0x1E, 0xE4, 0xF4, 0x8F, 0x8A,
            0xA1, 0xF4, 0x23, 0x4F, 0x0D, 0xC0, 0xD5, 0x3A, 0x91, 0xA6, 0x0A, 0x27, 0x69, 0x86, 0x72, 0x15,
            0x63, 0x86, 0xEB, 0x80, 0xE3, 0x06, 0xA3, 0xEB, 0x2B, 0xD7, 0x7D, 0xBC, 0xD9, 0xA7, 0xB7, 0x4C,
            0xB1, 0xC0, 0xC9, 0x3F, 0x9D, 0xF3, 0x90, 0x11, 0xF2, 0xCE, 0xAB, 0xF2, 0xF5, 0x16, 0x26, 0xD4,
            0x9E, 0x71, 0x55, 0xC2, 0x9C, 0x62, 0x03, 0x73, 0x98, 0x7A, 0xCD, 0x1F, 0xBE, 0xCD, 0xC8, 0x91,
            0x7A, 0xA4, 0x6A, 0x7E, 0x7F, 0x71, 0xA7, 0x15, 0x0E, 0x08, 0x5A, 0xD7, 0x75, 0x0B, 0xE7, 0xA9,
            0xD6, 0xA6, 0x1E, 0x27, 0x29, 0x7D, 0x63, 0x3C, 0x84, 0xE3, 0x0C, 0xEF, 0x9A, 0x4D, 0x0B, 0x80,
            0x83, 0x06, 0xCD, 0xB4, 0xBB, 0x24, 0x62, 0x6D, 0x6C, 0xBA, 0xDD, 0x7D, 0xF0, 0x4F, 0xFE, 0xBC,
            0x91, 0x8F, 0x0E, 0x59, 0x94, 0x41, 0x0B, 0x6D, 0x77, 0x79, 0x05, 0xD6, 0x76, 0x0F, 0xC8, 0x42,
            0x54, 0x4F, 0xBB, 0x8A, 0x57, 0xF2, 0x08, 0x42, 0x95, 0x4E, 0xFD, 0x8E, 0x6E, 0xC9, 0xB3, 0x36,
            0x5A, 0x93, 0xED, 0xFD, 0xE5, 0x95, 0x42, 0x2F, 0xF7, 0xA4, 0x7F, 0x7A, 0x59, 0xFB, 0x47, 0xFE,
            0x75, 0xE8, 0xFC, 0xA7, 0x5D, 0xC5, 0xE3, 0xBB, 0x0F, 0x2A, 0x82, 0xAF, 0xF8, 0x61, 0x4D, 0x3F,
            0xB6, 0x1A, 0x82, 0xBE, 0x22, 0x60, 0x52, 0xAA, 0x8E, 0xCC, 0x41, 0x83, 0x4B, 0xF9, 0xCF, 0xDD,
            0x6F, 0x37, 0x58, 0xB5, 0xD4, 0x84, 0x39, 0x01, 0x64, 0xB8, 0x34, 0x8A, 0x94, 0xFF, 0x16, 0xFE,
            0x2F, 0x8C, 0x96, 0x41, 0x55, 0x8C, 0x81, 0x05, 0xC3, 0x59, 0x14, 0x9A, 0x55, 0xF1, 0xA9, 0x07,
            0xC8, 0xA6, 0x97, 0x59, 0xC5, 0x17, 0x53, 0x3B, 0x1B, 0x5E, 0xDB, 0xC7, 0x4D, 0x8B, 0x54, 0x9C,
            0x4C, 0x52, 0xF1, 0x31, 0x85, 0x00, 0x18, 0x69, 0x1E, 0xB3, 0xC0, 0x67, 0x7D, 0xCB, 0x1E, 0xA3,
            0x0B, 0x9C, 0x80, 0x3D, 0x37, 0x65, 0x79, 0x92, 0xBD, 0x86, 0x3E, 0x0E, 0x28, 0xED, 0x50, 0x41,
            0x95, 0xD2, 0x5B, 0x34, 0xBB, 0xA4, 0x5E, 0xFD, 0x28, 0x43, 0x0D, 0x91, 0xCD, 0x6F, 0x74, 0xDA,
            0xBD, 0x81, 0xDC, 0x09, 0x32, 0x58, 0xF2, 0x2E, 0xD1, 0x97, 0x26, 0x05, 0x2F, 0x0E, 0x52, 0x13,
            0x93, 0x75, 0x9C, 0xF2, 0xFE, 0x60, 0x9D, 0xEA, 0x68, 0x6F, 0xC3, 0xC1, 0x4D, 0xC5, 0xF3, 0xD3,
            0x68, 0xBC, 0x73, 0x65, 0xBF, 0xD7, 0x08, 0x36, 0xDF, 0xF9, 0x5B, 0x57, 0x69, 0xD4, 0xA1, 0x3D,
            0xCD, 0xA3, 0x7B, 0x15, 0x56, 0x1C, 0x1B, 0x57, 0x67, 0xA0, 0xA9, 0x9E, 0x04, 0xB6, 0xE5, 0xB7
        };

        private static int _typeCode;
        private static string _typeCodeString;
#if ZIPLIB_SUPPORT
        private static UInt32 _holdrand;
#endif

        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            if (args.Length < 1)
            {
                Console.WriteLine("No input file specified");
                return 1;
            }

            string fileSpec = args[0];
            string dir = Path.GetDirectoryName(fileSpec);
            string searchPattern = Path.GetFileName(fileSpec);
            if (dir == null || searchPattern == null)
            {
                Console.WriteLine("Invalid file name");
                return 1;
            }

            string zipDir = dir;
            string typeCodeString = "USA";
            if (args.Length >= 2)
            {
                if (args[1].Length == 3)
                {
                    typeCodeString = args[1];
                }
                else
                {
                    zipDir = args[1];
                }
            }

            if (typeCodeString.Length != 3)
            {
                Console.WriteLine("Type code string length must be 3");
                return 1;
            }

            try
            {
                _typeCode = DecryptTypeCodeString(TypeCodeString);
                if (_typeCode < 0)
                {
                    Console.WriteLine("Decryption of type code failed");
                    return 1;
                }

                _typeCodeString = TypeCodeToString(_typeCode);
                Console.WriteLine("Type code: {0:X04} {1}", _typeCode, _typeCodeString);

                string[] files = Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string relPath = MakeRelativePath(dir, Path.GetDirectoryName(file));
                    if (relPath == null)
                    {
                        Console.WriteLine("*** No relative path for: {0}", file);
                        continue;
                    }
                    string zipOutDir = Path.Combine(zipDir, relPath);
                    string baseFileName = Path.GetFileName(file);
                    string ext = Path.GetExtension(file);
                    if (string.Compare(ext, @".rod", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Console.WriteLine("Decrypting: {0}", file);
                        string outFile = Path.ChangeExtension(file, @".rodtxt");
                        if (!DecryptSegmentFile(file, outFile))
                        {
                            Console.WriteLine("*** Decryption failed: {0}", file);
                        }
                        else
                        {
                            string zipFileName = Path.Combine(zipOutDir, Path.ChangeExtension(baseFileName, "uds") ?? string.Empty);
                            if (!CreateZip(new List<string>() {outFile}, "uds", zipFileName))
                            {
                                Console.WriteLine("*** Compression failed: {0}", file);
                            }
                        }
                    }
                    else if (string.Compare(ext, @".clb", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Console.WriteLine("Decrypting: {0}", file);
                        string outFile = Path.ChangeExtension(file, @".lbl");
                        if (!DecryptClbFile(file, outFile, typeCodeString))
                        {
                            Console.WriteLine("*** Decryption failed: {0}", file);
                        }
                        else
                        {
                            string zipFileName = Path.Combine(zipOutDir, Path.ChangeExtension(baseFileName, "ldat") ?? string.Empty);
                            if (!CreateZip(new List<string>() { outFile }, "ldat", zipFileName))
                            {
                                Console.WriteLine("*** Compression failed: {0}", file);
                            }
                        }
                    }
                    else if (string.Compare(ext, @".dat", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Console.WriteLine("Decrypting: {0}", file);
                        string outFile = Path.ChangeExtension(file, @".dattxt");
                        if (!DecryptDatFile(file, outFile))
                        {
                            Console.WriteLine("*** Decryption failed: {0}", file);
                        }
                        else
                        {
                            string zipFileName = Path.Combine(zipOutDir, Path.ChangeExtension(baseFileName, "cdat") ?? string.Empty);
                            if (!CreateZip(new List<string>() { outFile }, "cdat", zipFileName))
                            {
                                Console.WriteLine("*** Compression failed: {0}", file);
                            }
                        }
                    }
                    if (string.Compare(ext, @".lbl", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Console.WriteLine("Compressing: {0}", file);
                        string inFile = Path.ChangeExtension(file, @".lbl");
                        string zipFileName = Path.Combine(zipOutDir, Path.ChangeExtension(baseFileName, "ldat") ?? string.Empty);
                        if (!CreateZip(new List<string>() { inFile }, "ldat", zipFileName))
                        {
                            Console.WriteLine("*** Compression failed: {0}", file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return 0;
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(toPath))
            {
                return fromPath;
            }
            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {   // path can't be made relative.
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        private static bool CreateZip(List<string> inputFiles, string inputExt, string archiveFilenameOut)
        {
            try
            {
                if (string.IsNullOrEmpty(archiveFilenameOut))
                {
                    return false;
                }

                string dirName = Path.GetDirectoryName(archiveFilenameOut);
                if (string.IsNullOrEmpty(dirName))
                {
                    return false;
                }
                Directory.CreateDirectory(dirName);

                if (File.Exists(archiveFilenameOut))
                {
                    File.Delete(archiveFilenameOut);
                }
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(9);
                zipStream.Password = GetMd5Hash(Path.GetFileNameWithoutExtension(archiveFilenameOut).ToUpperInvariant());

                try
                {
                    foreach (string filename in inputFiles)
                    {

                        FileInfo fi = new FileInfo(filename);
                        string entryName = Path.GetFileName(filename);
                        entryName = Path.ChangeExtension(entryName, inputExt);

                        ZipEntry newEntry = new ZipEntry(entryName)
                        {
                            DateTime = fi.LastWriteTime,
                            Size = fi.Length,
                            //AESKeySize = 256
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[4096];
                        using (FileStream streamReader = File.OpenRead(filename))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer);
                        }
                        zipStream.CloseEntry();
                    }
                }
                finally
                {
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string GetMd5Hash(string text)
        {
            //Prüfen ob Daten übergeben wurden.
            if ((text == null) || (text.Length == 0))
            {
                return string.Empty;
            }

            //MD5 Hash aus dem String berechnen. Dazu muss der string in ein Byte[]
            //zerlegt werden. Danach muss das Resultat wieder zurück in ein string.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(text);
            byte[] result = md5.ComputeHash(textToHash);

            return BitConverter.ToString(result).Replace("-", "");
        }

        static bool DecryptClbFile(string inFile, string outFile, string typeCodeString)
        {
            bool extendedCode = false;
            DirectoryInfo dirInfo = Directory.GetParent(inFile);
            if (dirInfo != null)
            {
                string parentDir = dirInfo.Name;
                if (string.Compare(parentDir, _typeCodeString, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    extendedCode = true;
                    typeCodeString = _typeCodeString;
                }
            }

            byte typeCode = (byte)(typeCodeString[0] + typeCodeString[1] + typeCodeString[2]);
            if (extendedCode)
            {
                int dotIdx = inFile.LastIndexOf('.');
                if (dotIdx < 2)
                {
                    return false;
                }

                int codeOffset = _typeCode * inFile[dotIdx] * inFile[dotIdx - 1] * inFile[dotIdx - 2];
                typeCode += (byte) codeOffset;
            }

            try
            {
                using (FileStream fsRead = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream fsWrite = new FileStream(outFile, FileMode.Create))
                    {
                        for (int line = 0; ; line++)
                        {
                            byte[] data = DecryptClbLine(fsRead, line, typeCode);
                            if (data == null)
                            {
                                return false;
                            }
                            if (data.Length == 0)
                            {   // end of file
                                break;
                            }
                            if (line == 0)
                            {
                                if (!IsValidText(data))
                                {
#if false
                                    Encoding enc1 = Encoding.GetEncoding(1251);
                                    string asc1 = enc1.GetString(data);
                                    Console.WriteLine("Invalid: {0}", asc1);
#endif
#if true
                                    Console.WriteLine("Invalid decrypted text");
                                    return false;
#else
                                    Console.WriteLine("Type code invalid, trying all values");
                                    bool found = false;
                                    for (int code = 0; code < 0x100; code++)
                                    {
                                        fsRead.Seek(0, SeekOrigin.Begin);
                                        bool bValid = true;
                                        for (int j = 0; j < 4; j++)
                                        {
                                            data = DecryptLine(fsRead, j, (byte)code);
                                            if (data == null)
                                            {
                                                bValid = false;
                                                break;
                                            }
                                            if (data.Length == 0)
                                            {   // end of file
                                                break;
                                            }
                                            if (!IsValidText(data))
                                            {
#if false
                                                Encoding enc2 = Encoding.GetEncoding(1251);
                                                string asc2 = enc2.GetString(data);
                                                Console.WriteLine("Invalid: {0}", asc2);
#endif
                                                bValid = false;
                                                break;
                                            }
                                        }

                                        if (bValid)
                                        {
                                            found = true;
                                            typeCode = (byte)code;
                                            Console.WriteLine("Code found: {0:X02}", typeCode);
                                            fsRead.Seek(0, SeekOrigin.Begin);
                                            data = DecryptLine(fsRead, line, typeCode);
                                            break;
                                        }
                                    }

                                    if (!found)
                                    {
                                        Console.WriteLine("Type code not found");
                                        return false;
                                    }
#endif
                                }
                            }

                            foreach (byte value in data)
                            {
                                if (value == 0)
                                {
                                    break;
                                }
                                fsWrite.WriteByte(value);
                            }
                            fsWrite.WriteByte((byte)'\r');
                            fsWrite.WriteByte((byte)'\n');
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static bool IsValidText(byte[] data)
        {
            if (data.Length < 6)
            {
                return false;
            }

            if (data[0] == '#' || data[0] == ':')
            {
                return true;
            }

            for (int i = 0; i < 3; i++)
            {
                byte value = data[i];
                if (i == 0)
                {
                    if (!((value >= 0x30 && value <= 0x39) || (value >= 0x41 && value <= 0x5A) || (value == 0x3A)))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!((value >= 0x30 && value <= 0x39) || (value >= 0x41 && value <= 0x5A) || (value >= 0x61 && value <= 0x7A) ||
                          (value == 0x20) || (value == 0x28) || (value == 0x2C) || (value == 0x3A) || (value == 0x0D) || (value == 0x00)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static bool DecryptDatFile(string inFile, string outFile)
        {
            try
            {
                string baseName = Path.GetFileNameWithoutExtension(inFile);
                if (baseName == null)
                {
                    return false;
                }
                string typeName = "-" + _typeCodeString.ToUpperInvariant();
                int versionCode = 0;
                if (baseName.EndsWith(typeName))
                {
                    versionCode = _typeCode;
                }
                using (FileStream fsRead = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream fsWrite = new FileStream(outFile, FileMode.Create))
                    {
                        for (; ; )
                        {
                            byte[] data = DecryptStdLine(fsRead, versionCode, false);
                            if (data == null)
                            {
                                return false;
                            }
                            if (data.Length == 0)
                            {   // end of file
                                break;
                            }

                            foreach (byte value in data)
                            {
                                fsWrite.WriteByte(value);
                            }
                            fsWrite.WriteByte((byte)'\r');
                            fsWrite.WriteByte((byte)'\n');
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static byte[] DecryptClbLine(FileStream fs, int line, byte typeCode)
        {
            try
            {
                int lenH = fs.ReadByte();
                int lenL = fs.ReadByte();
                if (lenH < 0 || lenL < 0)
                {
                    return new byte[0];
                }

                int dataLen = (lenH << 8) + lenL;
                int remainder = 0;
                if ((dataLen % 8) != 0)
                {
                    remainder = 8 - dataLen % 8;
                }

                int blockSize = dataLen + remainder;
                byte[] data = new byte[blockSize + 8];
                int count = fs.Read(data, 0, blockSize + 2);
                if (count < blockSize + 2)
                {
                    return null;
                }
                UInt32[] buffer = new uint[(blockSize / sizeof(UInt32)) + 2];
                for (int i = 0; i < data.Length; i += sizeof(UInt32))
                {
                    buffer[i >> 2] = BitConverter.ToUInt32(data, i);
                }

                Int32 code1 = line;
                Int32 code2 = typeCode;
                Int32 mask1 = (code1 + 2) * (code2 + 1) + (code1 + 3) * (code1 + 1) * (code2 + 2);
                Int32 mask2 = (code1 + 2) * (code2 + 1) * (code2 + 3);
                Int32 tempVal1 = code2 + 1;
                Int32 tempVal2 = code2 % (code1 + 1);
                if (tempVal2 == 0)
                {
                    tempVal2 = code1 % tempVal1;
                }
                mask2 += tempVal2;
                if (mask1 < 0xFFFF)
                {
                    mask1 = (mask1 << 16) + (code1 + 2) * (code2 + 1) * (code2 + 3) * (code1 + 4);
                }
                if (mask2 < 0xFFFF)
                {
                    mask2 = (mask2 << 16) + (code1 + 1) * (code1 + 2) * (code1 + 3);
                }

                UInt32[] mask = new UInt32[2];
                mask[0] = (UInt32)mask1;
                mask[1] = (UInt32)mask2;
                if (!DecryptBlock(mask, buffer, 2))
                {
                    return null;
                }

                byte[] result = new byte[blockSize];
                for (int i = 0; i < blockSize; i += sizeof(UInt32))
                {
                    byte[] conf = BitConverter.GetBytes(buffer[i >> 2]);
                    Array.Copy(conf, 0, result, i, conf.Length);
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        static byte[] DecryptStdLine(FileStream fs, int versionCode, bool segmentFile)
        {
            try
            {
                bool numberValid = false;
                StringBuilder sbNumber = new StringBuilder();
                for (int i = 0; i < 16; i++)
                {
                    int value = fs.ReadByte();
                    if (value < 0)
                    {
                        return new byte[0];
                    }
                    if (value == '\r' || value == '\n')
                    {
                        continue;
                    }

                    if (segmentFile)
                    {
                        if (value == '[')
                        {
                            return new byte[0]; // end of segment
                        }
                    }
                    if (value == ' ')
                    {
                        numberValid = true;
                        break;
                    }

                    sbNumber.Append((char)value);
                }
                if (!numberValid)
                {
                    return null;
                }

                if (sbNumber.Length != 6 && sbNumber.Length != 8)
                {
                    return null;
                }

                if (!UInt32.TryParse(sbNumber.ToString(), out UInt32 _))
                {
                    return null;
                }

                int dataLenIn = fs.ReadByte();
                if (dataLenIn < 0)
                {
                    return null;
                }

                int dataLenOut = fs.ReadByte();
                if (dataLenOut < 0)
                {
                    return null;
                }

                if (dataLenOut > dataLenIn)
                {
                    return null;
                }

                if ((dataLenIn % 8) != 0)
                {
                    return null;
                }

                byte[] prefix = Encoding.ASCII.GetBytes(sbNumber + ",");
                if (dataLenIn <= 0)
                {
                    return prefix;
                }

                byte[] data = new byte[dataLenIn + 8];
                int count = fs.Read(data, 0, dataLenIn);
                if (count < dataLenIn)
                {
                    return null;
                }
                UInt32[] buffer = new uint[data.Length / sizeof(UInt32)];
                for (int i = 0; i < data.Length; i += sizeof(UInt32))
                {
                    buffer[i >> 2] = BitConverter.ToUInt32(data, i);
                }

                if (sbNumber.Length == 6)
                {
                    sbNumber.Append(sbNumber[4]);
                    sbNumber.Append(sbNumber[3]);
                }
                byte[] maskBuffer = Encoding.ASCII.GetBytes(sbNumber.ToString());

                Int32 cryptCode = sbNumber[5];
                Int32 cryptOffet = cryptCode * 2;
                for (int i = 0; i < 8; i++)
                {
#if ZIPLIB_SUPPORT
                    maskBuffer[i] += (byte) (versionCode + CryptTab2[(byte) cryptOffet]);
#else
                    maskBuffer[i] += CryptTab2[(byte)cryptOffet];
                    maskBuffer[i] += (byte) versionCode;
#endif
                    cryptOffet += cryptCode;
                }

                for (int i = 0; i < maskBuffer.Length; i++)
                {
                    maskBuffer[i] *= CryptTab1[CryptTab2[CryptTabIndex[i]]];
                }

                UInt32[] mask = new UInt32[2];
                mask[0] = BitConverter.ToUInt32(maskBuffer, 0);
                mask[1] = BitConverter.ToUInt32(maskBuffer, 4);
                if (!DecryptBlock(mask, buffer, 0))
                {
                    return null;
                }

                byte[] result = new byte[dataLenIn];
                for (int i = 0; i < dataLenIn; i += sizeof(UInt32))
                {
                    byte[] conf = BitConverter.GetBytes(buffer[i >> 2]);
                    Array.Copy(conf, 0, result, i, conf.Length);
                }
                Array.Resize(ref result, dataLenOut);
                return prefix.Concat(result).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        static bool DecryptSegmentFile(string inFile, string outFile)
        {
            try
            {
                using (FileStream fsRead = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream fsWrite = new FileStream(outFile, FileMode.Create))
                    {
                        for (;;)
                        {
                            ResultCode resultCode = DecryptSegment(fsRead, fsWrite, inFile);
                            if (resultCode == ResultCode.Error)
                            {
                                return false;
                            }
                            if (resultCode == ResultCode.Done)
                            {
                                break;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#if ZIPLIB_SUPPORT
        static ResultCode DecryptSegment(FileStream fsRead, FileStream fsWrite, string fileName)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool startFound = false;
                for (;;)
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Done;
                    }
                    if (value == '[')
                    {
                        startFound = true;
                    }
                    if (!startFound)
                    {
                        continue;
                    }
                    if (value == 0x0A && sb.Length > 0)
                    {
                        break;
                    }
                    if (value != 0x0A && value != 0x0D)
                    {
                        sb.Append((char)value);
                    }
                }
                string segmentNameLine = sb.ToString();
                if (segmentNameLine.Length < 5 || segmentNameLine[0] != '[' || segmentNameLine[segmentNameLine.Length -1] != ']')
                {
                    return ResultCode.Error;
                }
                string segmentName = segmentNameLine.Substring(1, segmentNameLine.Length - 2);

                int frameLen = 0;
                for (int i = 0; i < 3; i++)
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Error;
                    }

                    frameLen <<= 8;
                    frameLen |= value;
                }

                int contentLen = 0;
                for (int i = 0; i < 3; i++)
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Error;
                    }

                    contentLen <<= 8;
                    contentLen |= value;
                }

                bool compressed = (frameLen & 0x800000) == 0;
                int dataLength = frameLen & 0x7FFFFF;
                if ((dataLength & 0x07) != 0)
                {
                    return ResultCode.Error;
                }
                int readLength = dataLength + 8;
                byte[] data = new byte[readLength];

                int count = fsRead.Read(data, 0, data.Length);
                if (count < data.Length)
                {
                    return ResultCode.Error;
                }

                UInt32[] buffer = new uint[(data.Length / sizeof(UInt32))];
                for (int i = 0; i < data.Length; i += sizeof(UInt32))
                {
                    buffer[i >> 2] = BitConverter.ToUInt32(data, i);
                }

                byte[] segmentMask = CalcSegmentMask(fileName, segmentName);
                UInt32[] mask = new UInt32[2];
                for (int i = 0; i < segmentMask.Length; i += sizeof(UInt32))
                {
                    mask[i >> 2] = BitConverter.ToUInt32(segmentMask, i);
                }

                if (!DecryptBlock(mask, buffer, 0))
                {
                    return ResultCode.Error;
                }

                byte[] result = new byte[dataLength];
                for (int i = 0; i < dataLength; i += sizeof(UInt32))
                {
                    byte[] conf = BitConverter.GetBytes(buffer[i >> 2]);
                    Array.Copy(conf, 0, result, i, conf.Length);
                }

                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] segmentData = ascii.GetBytes(segmentName);
                fsWrite.WriteByte((byte)'[');
                fsWrite.Write(segmentData, 0, segmentData.Length);
                fsWrite.WriteByte((byte)']');
                fsWrite.WriteByte((byte)'\r');
                fsWrite.WriteByte((byte)'\n');

                if (!compressed)
                {
                    int writeLen = contentLen < result.Length ? contentLen : result.Length;
#if true
                    using (MemoryStream inStream = new MemoryStream(result, 0, writeLen))
                    {
                        if (!DecryptTextStream(inStream, fsWrite))
                        {
                            return ResultCode.Error;
                        }
                    }
#else
                    fsWrite.Write(result, 0, writeLen);
#endif
                }
                else
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        DecompressData(result, memStream, contentLen);
                        memStream.Position = 0;
#if true
                        if (!DecryptTextStream(memStream, fsWrite))
                        {
                            return ResultCode.Error;
                        }
#else
                        memStream.CopyTo(fsWrite);
#endif
                    }
                }

                long oldPos = fsWrite.Position;
                if (oldPos >= 2)
                {
                    fsWrite.Position = oldPos - 2;
                    if (fsWrite.ReadByte() == '\r' && fsWrite.ReadByte() == '\n')
                    {
                        fsWrite.Position = oldPos;
                    }
                    else
                    {
                        fsWrite.Position = oldPos;
                        fsWrite.WriteByte((byte)'\r');
                        fsWrite.WriteByte((byte)'\n');
                    }
                }
                fsWrite.WriteByte((byte)'[');
                fsWrite.WriteByte((byte)'/');
                fsWrite.Write(segmentData, 0, segmentData.Length);
                fsWrite.WriteByte((byte)']');
                fsWrite.WriteByte((byte)'\r');
                fsWrite.WriteByte((byte)'\n');

                return ResultCode.Ok;
            }
            catch (Exception)
            {
                return ResultCode.Error;
            }
        }

        static byte[] CalcSegmentMask(string fileName, string segmentName)
        {
            if (segmentName.Length < 3)
            {
                return null;
            }

            byte[] mask = new byte[8];

            for (int i = 0; i < 3; i++)
            {
                mask[i] = (byte)segmentName[i];
            }

            string baseName = Path.GetFileNameWithoutExtension(fileName);
            if (baseName == null)
            {
                return null;
            }
            baseName = baseName.ToUpperInvariant();
            long multVal = (byte)baseName[0];
            foreach (char character in baseName)
            {
                multVal *= (byte)character;
            }

            for (int i = 0; i < 5; i++)
            {
                mask[i + 3] = (byte)(multVal >> (i *8));
            }

            int offset = 0;
            string typeName = "-" + _typeCodeString.ToUpperInvariant();
            if ((baseName.StartsWith("TTTEXT") || baseName.StartsWith("UNIT")) && baseName.EndsWith(typeName))
            {
                offset = _typeCode;
            }

            int factor = (byte)segmentName[1];
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] += CryptTab2[(byte) (factor * (i + 2))];
                mask[i] += (byte)offset;
            }

            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] *= CryptTab1[CryptTab2[CryptTabIndex[i]]];
            }

            return mask;
        }
#else
        static ResultCode DecryptSegment(FileStream fsRead, FileStream fsWrite, string fileName)
        {
            try
            {
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                if (baseName == null)
                {
                    return ResultCode.Error;
                }
                string typeName = "-" + _typeCodeString.ToUpperInvariant();
                int versionCode = 0;
                if (baseName.EndsWith(typeName))
                {
                    versionCode = _typeCode;
                }

                StringBuilder sb = new StringBuilder();
                bool startFound = false;
                for (; ; )
                {
                    int value = fsRead.ReadByte();
                    if (value < 0)
                    {
                        return ResultCode.Done;
                    }
                    if (value == '[')
                    {
                        startFound = true;
                    }
                    if (sb.Length == 0 && value >= '0' && value <= '9')
                    {
                        // no segment name, direct start with number
                        fsRead.Position--;
                        sb.Append("[]");
                        break;
                    }
                    if (!startFound)
                    {
                        continue;
                    }
                    if (value == 0x0A && sb.Length > 0)
                    {
                        break;
                    }
                    if (value != 0x0A && value != 0x0D)
                    {
                        sb.Append((char)value);
                    }
                }
                string segmentNameLine = sb.ToString();
                if (segmentNameLine.Length < 2 || segmentNameLine[0] != '[' || segmentNameLine[segmentNameLine.Length - 1] != ']')
                {
                    return ResultCode.Error;
                }
                string segmentName = segmentNameLine.Substring(1, segmentNameLine.Length - 2);

                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] segmentData = ascii.GetBytes(segmentName);
                fsWrite.WriteByte((byte)'[');
                fsWrite.Write(segmentData, 0, segmentData.Length);
                fsWrite.WriteByte((byte)']');
                fsWrite.WriteByte((byte)'\r');
                fsWrite.WriteByte((byte)'\n');

                for (; ; )
                {
                    byte[] data = DecryptStdLine(fsRead, versionCode, true);
                    if (data == null)
                    {
                        return ResultCode.Error;
                    }
                    if (data.Length == 0)
                    {   // end of segment
                        break;
                    }

                    foreach (byte value in data)
                    {
                        fsWrite.WriteByte(value);
                    }
                    fsWrite.WriteByte((byte)'\r');
                    fsWrite.WriteByte((byte)'\n');
                }

                fsWrite.WriteByte((byte)'[');
                fsWrite.WriteByte((byte)'/');
                fsWrite.Write(segmentData, 0, segmentData.Length);
                fsWrite.WriteByte((byte)']');
                fsWrite.WriteByte((byte)'\r');
                fsWrite.WriteByte((byte)'\n');

                return ResultCode.Ok;
            }
            catch (Exception)
            {
                return ResultCode.Error;
            }
        }
#endif

        static bool DecryptBlock(UInt32[] mask, UInt32[] buffer, int codeTable)
        {
            UInt32 code1;
            UInt32 code2;
            UInt32 code3;
            UInt32 code4;

            switch (codeTable)
            {
                case 1:
                    code1 = 0x87A32FEB;
                    code2 = 0x77539F1B;
                    code3 = 0x67030F4B;
                    code4 = 0x57B37F7B;
                    break;

                case 2:
                    code1 = 0xFA7E14D0;
                    code2 = 0x249B910E;
                    code3 = 0x2FDD6FFC;
                    code4 = 0x15834A78;
                    break;

                default:
                    code1 = 0x29B76A4;
                    code2 = 0xCB6DB50A;
                    code3 = 0x71395D29;
                    code4 = 0x0DBC09C2;
                    break;
            }

            UInt32 mask1 = mask[0];
            UInt32 mask2 = mask[1];

            int bufferSize = buffer.Length * sizeof(UInt32);
            int dataSize = bufferSize;
            dataSize &= ~0x07;
            if (dataSize <= 8)
            {
                return false;
            }
            int blockCount = ((dataSize - 9) >> 3) + 1;
            int dataPos = 0;
            for (int i = 0; i < blockCount; i++)
            {
                UInt32 data1 = buffer[dataPos + 0];
                UInt32 data2 = buffer[dataPos + 1];
                UInt32 data1Old = data1;
                UInt32 data2Old = data2;
                UInt32 codeDyn = 0xC6EF3720;

                for (int j = 0; j < 32; j++)
                {
                    data2 -= (data1 + codeDyn) ^ (code3 + 16 * data1) ^ (code4 + (data1 >> 5));
                    data1 -= (data2 + codeDyn) ^ (code1 + 16 * data2) ^ (code2 + (data2 >> 5));
                    codeDyn += 0x61C88647;
                }
                buffer[dataPos + 0] = data1 ^ mask1;
                buffer[dataPos + 1] = data2 ^ mask2;
                mask1 = data1Old;
                mask2 = data2Old;

                dataPos += 2;
            }

            return true;
        }

#if ZIPLIB_SUPPORT
        static void DecompressData(byte[] inData, Stream fsout, int bytes)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStreamMod outZStream = new ZOutputStreamMod(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream, inData.Length);
                outZStream.finish();
                outMemoryStream.Position = 0;
                CopyStream(outMemoryStream, fsout, bytes);
            }
        }

        static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[0x8000];
            int len;
            while (bytes > 0 && (len = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, len);
                bytes -= len;
            }
        }

        static bool DecryptTextStream(Stream inStream, Stream outStream)
        {
            try
            {
                for (;;)
                {
                    List<byte> lineList = new List<byte>();
                    int lastValue = 0;
                    for (; ; )
                    {
                        int value = inStream.ReadByte();
                        if (value < 0)
                        {
                            break;
                        }
                        lineList.Add((byte)value);
                        if (lastValue == 0x0D && value == 0x0A)
                        {   // end of line
                            break;
                        }
                        lastValue = value;
                    }

                    if (lineList.Count == 0)
                    {
                        return true;
                    }

                    byte[] result = DecryptTextLine(lineList.ToArray());
                    if (result == null)
                    {
                        return false;
                    }
                    outStream.Write(result, 0, result.Length);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        static byte[] DecryptTextLine(byte[] line)
        {
            StringBuilder sbNumber = new StringBuilder();
            int colonIdx = 0;
            foreach (byte value in line)
            {
                if (value == ',')
                {
                    break;
                }
                sbNumber.Append((char)value);
                colonIdx++;
            }
            if (colonIdx < 1 || colonIdx > 8 || line.Length < colonIdx + 1)
            {
                return null;
            }

            if (!UInt32.TryParse(sbNumber.ToString(), out UInt32 code))
            {
                return null;
            }
            byte[] text = new byte[line.Length - colonIdx - 1];
            Array.Copy(line, colonIdx + 1, text, 0, text.Length);
            byte[] result = new byte[text.Length];

            Dictionary<byte, byte> mapDict = CreateCharMap(code);
            int index = 0;
            foreach (byte orgChar in text)
            {
                if (mapDict.TryGetValue(orgChar, out byte mappedChar))
                {
                    result[index] = mappedChar;
                }
                else
                {
                    result[index] = orgChar;
                }

                index++;
            }

            byte[] combinedResult = new byte[colonIdx + 1 + result.Length];
            Array.Copy(line, 0, combinedResult, 0, colonIdx + 1);
            Array.Copy(result, 0, combinedResult, colonIdx + 1, result.Length);
            return combinedResult;
        }

        static Dictionary<byte, byte> CreateCharMap(UInt32 code)
        {
            byte[] charList = new byte[26];
            for (int i = 0; i < charList.Length; i++)
            {
                charList[i] = (byte) ('a' + i);
            }

            byte[] numList = new byte[14];
            for (int i = 0; i < 10; i++)
            {
                numList[i] = (byte)('0' + i);
            }
            numList[10] = (byte)',';
            numList[11] = (byte)'.';
            numList[12] = (byte)'-';
            numList[13] = (byte)'_';

            byte[] charListMod = (byte[])charList.Clone();
            byte[] numListMod = (byte[])numList.Clone();
            ReorderLookupTables(charListMod, numListMod, code);

            Dictionary<byte, byte> mapDict = new Dictionary<byte, byte>();
            for (int i = 0; i < charList.Length; i++)
            {
                mapDict.Add(charListMod[i], charList[i]);   // lower chars
                mapDict.Add((byte)(charListMod[i] - 32), (byte)(charList[i] - 32)); // upper chars
            }

            for (int i = 0; i < numList.Length; i++)
            {
                mapDict.Add(numListMod[i], numList[i]);
            }

            return mapDict;
        }

        static void ReorderLookupTables(byte[] charList, byte[] numList, UInt32 code)
        {
            Srand(code);

            int charLen = charList.Length;
            for (int i = 0; i < charLen; i++)
            {
                long newPos = Rand() % charLen;
                byte newVal = charList[newPos];
                charList[newPos] = charList[i];
                charList[i] = newVal;
            }

            int numLen = numList.Length;
            for (int i = 0; i < numLen; i++)
            {
                long newPos = Rand() % numLen;
                byte newVal = numList[newPos];
                numList[newPos] = numList[i];
                numList[i] = newVal;
            }
        }

        // C-style rand function
        static void Srand(UInt32 seed)
        {
            _holdrand = seed;
        }

        static UInt32 Rand()
        {
            _holdrand = _holdrand * 214013u + 2531011u;

            return (_holdrand >> 16) & 0x7FFF;
        }
#endif

        static int DecryptTypeCodeString(string typeCode)
        {
            if (!UInt64.TryParse(typeCode, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out UInt64 value))
            {
                return -1;
            }

            byte[] data = BitConverter.GetBytes(value);

            int pos = (int) DecryptTypeCodeData[0];
            for (int i = 0; i < 8; i++)
            {
                byte tempCode = (byte) (data[i] ^ CryptTab2[i]);
                data[i] ^= CryptTab2[pos];
                pos = tempCode + (int) DecryptTypeCodeData[1];
            }

            UInt64 convVal = BitConverter.ToUInt64(data, 0);
            convVal -= DecryptTypeCodeData[2];
            UInt64 code = convVal / DecryptTypeCodeData[3];
            if (code > 0xFFF)
            {
                return -1;
            }

            return (int) code;
        }

        static string TypeCodeToString(int typeCode)
        {
            byte typeLow = (byte)typeCode;
            if (typeLow <= 0x48)
            {
                if (typeLow <= 0x30)
                {
                    switch (typeLow)
                    {
                        case 0x30:
                            return "NEZ";
                        case 0x19:
                            return "PRI";
                        case 0x1C:
                            return "PCI";
                        case 0x1F:
                            return "DRV";
                        case 0x20:
                            return "EST";
                        case 0x22:
                            return "PTT";
                        case 0x24:
                            return "ITT";
                    }

                    return "INVALID";
                }

                switch (typeLow)
                {
                    case 0x34:
                        return "AER";
                    case 0x38:
                        return "HZH";
                    case 0x3C:
                        return "BLD";
                    case 0x3E:
                        return "VLF";
                    case 0x40:
                        return "SEF";
                    default:
                        return "INVALID";
                }
            }

            if (typeLow <= 0xA0)
            {
                switch (typeLow)
                {
                    case 0xA0:
                        return "HGJ";
                    case 0x50:
                        return "BPA";
                    case 0x51:
                        return "ZGC";
                    case 0x70:
                        return "SVO";
                    case 0x80:
                        return "FRM";
                    case 0x91:
                        return "PTS";
                }
                return "INVALID";
            }

            switch (typeLow)
            {
                case 0xA1:
                    return "ROJ";
                case 0xA8:
                    return "AKP";
                case 0xB0:
                    return "ZHS";
                case 0xC0:
                    return "ARB";
                case 0xD0:
                    return "RUS";
            }
            return "INVALID";
        }
    }
}
