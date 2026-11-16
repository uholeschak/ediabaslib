//#define IO_TEST
using Android.Content.Res;
using EdiabasLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BmwDeepObd
{
    public static class ZipUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(ZipUtils).FullName;
#endif
        private const int StreamBufferSize = 4096;

        public class ZipTreeNode
        {
            private readonly List<ZipTreeNode> _children = new List<ZipTreeNode>();
            private readonly Dictionary<string, ZipTreeNode> _childDict = new Dictionary<string, ZipTreeNode>(StringComparer.Ordinal);

            public ZipTreeNode(string name, bool isDirectory, ZipTreeNode parent)
            {
                Name = name ?? string.Empty;
                IsDirectory = isDirectory;
                Parent = parent;
                FullName = string.IsNullOrEmpty(parent?.FullName) ? Name : parent.FullName + "/" + Name;
            }

            public string Name { get; }

            // Path inside the archive with '/' as separator, empty for the root node
            public string FullName { get; }

            public bool IsDirectory { get; }

            public bool IsFile => !IsDirectory;

            public bool IsRoot => Parent == null;

            public ZipTreeNode Parent { get; }

            public IReadOnlyList<ZipTreeNode> Children => _children;

            // -1 if unknown
            public long Size { get; internal set; } = -1;

            // -1 if unknown
            public long CompressedSize { get; internal set; } = -1;

            // null for directories that have no own archive entry
            public DateTime? LastModified { get; internal set; }

            public ZipTreeNode GetChild(string name)
            {
                if (name == null)
                {
                    return null;
                }

                return _childDict.TryGetValue(name, out ZipTreeNode childNode) ? childNode : null;
            }

            internal ZipTreeNode AddChild(string name, bool isDirectory)
            {
                ZipTreeNode childNode = new ZipTreeNode(name, isDirectory, this);
                _children.Add(childNode);
                _childDict.Add(name, childNode);
                return childNode;
            }

            internal void SortChildren()
            {
                _children.Sort((x, y) =>
                {
                    if (x.IsDirectory != y.IsDirectory)
                    {
                        return x.IsDirectory ? -1 : 1;
                    }

                    int result = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                    return result != 0 ? result : string.CompareOrdinal(x.Name, y.Name);
                });

                foreach (ZipTreeNode childNode in _children)
                {
                    if (childNode.IsDirectory)
                    {
                        childNode.SortChildren();
                    }
                }
            }
        }

        public static bool CheckZipFile(string archiveFilename)
        {
            ZipFile zf = null;
            try
            {
                try
                {
                    FileStream fs = File.OpenRead(archiveFilename);
                    zf = new ZipFile(fs);
                    return zf.TestArchive(false);
                }
                catch (Exception)
                {
                    return false;
                }
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

        public static void ExtractZipFile(AssetManager assetManager, Assembly resourceAssembly, string archiveFilenameIn, string outFolder, string key,
            List<string> ignoreFolders, List<string> encodeExtensions, ActivityCommon.ProgressZipDelegate progressHandler)
        {
#if DEBUG
            string lastFileName = string.Empty;
            Android.Util.Log.Info(Tag, string.Format("ExtractZipFile Archive: '{0}', Folder: '{1}'", archiveFilenameIn, outFolder));
#endif
            Stream fs = null;
            ZipFile zf = null;
            string tempFile = Path.Combine(outFolder, "temp.zip");
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    using (Aes crypto = Aes.Create())
                    {
                        crypto.Mode = CipherMode.CBC;
                        crypto.Padding = PaddingMode.PKCS7;
                        crypto.KeySize = 256;

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                        using (MD5 md5 = MD5.Create())
                        {
                            crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }

                        bool extractAborted = false;
                        for (int retry = 0;; retry++)
                        {
                            try
                            {
                                Stream fsRead = null;
                                try
                                {
                                    if (assetManager != null)
                                    {
                                        string tempFileName = Path.GetTempFileName();
                                        fs = File.Create(tempFileName, StreamBufferSize, FileOptions.DeleteOnClose);
                                        string[] archiveFiles = archiveFilenameIn.Split(";");
                                        byte[] buffer = new byte[StreamBufferSize];

                                        foreach (string archiveFile in archiveFiles)
                                        {
                                            using (Stream inputStream = assetManager.Open(archiveFile))
                                            {
                                                if (inputStream == null)
                                                {
                                                    throw new IOException($"Opening asset stream {archiveFile} failed");
                                                }

                                                StreamUtils.Copy(inputStream, fs, buffer);
                                            }
                                        }

                                        fs.Seek(0, SeekOrigin.Begin);
                                        fsRead = fs;
                                    }
                                    else if (resourceAssembly != null)
                                    {
                                        fsRead = resourceAssembly.GetManifestResourceStream(archiveFilenameIn);
                                    }
                                    else
                                    {
                                        fsRead = File.OpenRead(archiveFilenameIn);
                                    }

                                    if (fsRead == null)
                                    {
                                        extractAborted = true;
                                        throw new IOException("Opening archive failed");
                                    }

                                    using (CryptoStream crStream = new CryptoStream(fsRead, crypto.CreateDecryptor(), CryptoStreamMode.Read))
                                    {
                                        byte[] buffer = new byte[StreamBufferSize];
                                        using (FileStream fsWrite = File.Create(tempFile))
                                        {
                                            bool aborted = false;
                                            StreamUtils.Copy(crStream, fsWrite, buffer, (sender, args) =>
                                            {
                                                if (progressHandler != null)
                                                {
                                                    if (progressHandler((int)args.PercentComplete, true))
                                                    {
                                                        args.ContinueRunning = false;
                                                        aborted = true;
                                                    }
                                                }
                                            }, TimeSpan.FromSeconds(1), null, null, fsRead.Length);
                                            if (aborted)
                                            {
                                                extractAborted = true;
                                                return;
                                            }
#if IO_TEST
                                            if (retry == 0)
                                            {
                                                throw new IOException("Exception test");
                                            }
#endif
                                        }
                                    }
                                }
                                finally
                                {
                                    fsRead?.Dispose();
                                }

                                break;
                            }
#pragma warning disable 168
                            catch (Exception ex)
#pragma warning restore 168
                            {
#if DEBUG
                                Android.Util.Log.Info(Tag, EdiabasNet.GetExceptionText(ex));
#endif
                                if (extractAborted || retry > ActivityCommon.FileIoRetries)
                                {
                                    throw;
                                }

                                Thread.Sleep(ActivityCommon.FileIoRetryDelay);
                            }
                        }
                    }
                    fs = File.OpenRead(tempFile);
                }
                else
                {
                    if (assetManager != null)
                    {
                        string tempFileName = Path.GetTempFileName();
                        fs = File.Create(tempFileName, StreamBufferSize, FileOptions.DeleteOnClose);
                        string[] archiveFiles = archiveFilenameIn.Split(";");
                        byte[] buffer = new byte[StreamBufferSize];

                        foreach (string archiveFile in archiveFiles)
                        {
                            using (Stream inputStream = assetManager.Open(archiveFile))
                            {
                                if (inputStream == null)
                                {
                                    throw new IOException($"Opening asset stream {archiveFile} failed");
                                }

                                StreamUtils.Copy(inputStream, fs, buffer);
                            }
                        }

                        fs.Seek(0, SeekOrigin.Begin);
                    }
                    else if (resourceAssembly != null)
                    {
                        fs = resourceAssembly.GetManifestResourceStream(archiveFilenameIn);
                    }
                    else
                    {
                        fs = File.OpenRead(archiveFilenameIn);
                    }
                }

                zf = new ZipFile(fs);

                // count files to extract, to display the correct progress
                int fileCount = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    string entryFileName = zipEntry.Name;
                    if (ignoreFolders != null)
                    {
                        bool ignoreFile = false;
                        foreach (string ignoreFolder in ignoreFolders)
                        {
                            if (entryFileName.StartsWith(ignoreFolder))
                            {
                                ignoreFile = true;
                                break;
                            }
                        }
                        if (ignoreFile)
                        {
                            continue;
                        }
                    }
                    fileCount++;
                }
                if (fileCount == 0)
                {
                    fileCount = 1;
                }
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("ExtractZipFile FileCount: {0}", fileCount));
#if IO_TEST
                int testCount = 0;
#endif
#endif
                long index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / fileCount)))
                        {
                            return;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    string entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    if (ignoreFolders != null)
                    {
                        bool ignoreFile = false;
                        foreach (string ignoreFolder in ignoreFolders)
                        {
                            if (entryFileName.StartsWith(ignoreFolder))
                            {
                                ignoreFile = true;
                                break;
                            }
                        }
                        if (ignoreFile)
                        {
                            continue;
                        }
                    }

                    for (int retry = 0; ; retry++)
                    {
                        bool noRetry = false;
                        try
                        {
                            // Manipulate the output filename here as desired.
                            string fullZipToPath = Path.Combine(outFolder, entryFileName);
                            string fileExt = Path.GetExtension(fullZipToPath);
                            if (encodeExtensions != null && encodeExtensions.Contains(fileExt, StringComparer.OrdinalIgnoreCase))
                            {
                                fullZipToPath = EdiabasNet.EncodeFilePath(fullZipToPath);
                            }
                            string directoryName = Path.GetDirectoryName(fullZipToPath);
                            if (!string.IsNullOrEmpty(directoryName))
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                Directory.CreateDirectory(directoryName);
                            }

                            byte[] buffer = new byte[StreamBufferSize];
                            noRetry = true;     // no retry for zip stream exception
                            using (Stream zipStream = zf.GetInputStream(zipEntry))
                            {
                                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                                // of the file, but does not waste memory.
                                // The "using" will close the stream even if an exception occurs.
                                noRetry = false;
#if DEBUG
                                lastFileName = fullZipToPath;
#if IO_TEST
                                testCount++;
                                if (testCount % 100 == 0)
                                {
                                    throw new IOException("Exception test");
                                }
#endif
#endif
                                using (FileStream streamWriter = File.Create(fullZipToPath))
                                {
                                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                                }
                            }
                            break;
                        }
#pragma warning disable 168
                        catch (Exception ex)
#pragma warning restore 168
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, EdiabasNet.GetExceptionText(ex));
#endif
                            if (noRetry || retry > ActivityCommon.FileIoRetries)
                            {
                                throw;
                            }

                            Thread.Sleep(ActivityCommon.FileIoRetryDelay);
                        }
                    }

                    index++;
                }
#if DEBUG
                Android.Util.Log.Info(Tag, "ExtractZipFile done");
#endif
            }
#if DEBUG
            catch (Exception ex)
            {
                string exceptionText = EdiabasNet.GetExceptionText(ex);
                Android.Util.Log.Info(Tag, string.Format("ExtractZipFile File: '{0}', Exception: '{1}', ", lastFileName, exceptionText));
                throw;
            }
#endif
            finally
            {
                // ReSharper disable once ConstantConditionalAccessQualifier
                zf?.Close(); // Ensure we release resources
                // ReSharper disable once ConstantConditionalAccessQualifier
                fs?.Close();
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public static bool CreateZipFile(string[] inputFiles, string archiveFilenameOut, ActivityCommon.ProgressZipDelegate progressHandler)
        {
            try
            {
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    long index = 0;
                    foreach (string filename in inputFiles)
                    {
                        if (progressHandler != null)
                        {
                            if (progressHandler((int)(100 * index / inputFiles.Length)))
                            {
                                return false;
                            }
                        }

                        FileInfo fi = new FileInfo(filename);
                        string entryName = Path.GetFileName(filename);

                        ZipEntry newEntry = new ZipEntry(entryName)
                        {
                            DateTime = fi.LastWriteTime,
                            Size = fi.Length
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[StreamBufferSize];
                        using (FileStream streamReader = File.OpenRead(filename))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer);
                        }
                        zipStream.CloseEntry();
                        index++;
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

        public static bool ExtractZipFile(string archiveFilenameIn, string archiveName, Stream outStream)
        {
            try
            {
                ZipFile zf = null;
                try
                {
                    FileStream fs = File.OpenRead(archiveFilenameIn);
                    zf = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue; // Ignore directories
                        }
                        if (string.Compare(zipEntry.Name, archiveName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            byte[] buffer = new byte[StreamBufferSize];
                            using (Stream zipStream = zf.GetInputStream(zipEntry))
                            {
                                StreamUtils.Copy(zipStream, outStream, buffer);
                            }
                            break;
                        }
                    }
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CreateZipFile(Stream inStream, string archiveName, string archiveFilenameOut)
        {
            try
            {
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    ZipEntry newEntry = new ZipEntry(archiveName)
                    {
                        Size = inStream.Length
                    };
                    zipStream.PutNextEntry(newEntry);
                    byte[] buffer = new byte[StreamBufferSize];
                    StreamUtils.Copy(inStream, zipStream, buffer);
                    zipStream.CloseEntry();
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

        public static Dictionary<string, int> ExtractKeyWords(string archiveFilename, string wordRegEx, int maxWords, string lineRegEx, ActivityCommon.ProgressZipDelegate progressHandler)
        {
            Dictionary<string, int> wordDict = new Dictionary<string, int>();
            ZipFile zf = null;
            try
            {
                Regex regExWord = new Regex(wordRegEx);
                Regex regExLine = string.IsNullOrWhiteSpace(lineRegEx) ? null: new Regex(lineRegEx);
                long index = 0;
                FileStream fs = File.OpenRead(archiveFilename);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / zf.Count)))
                        {
                            return null;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    using (Stream zipStream = zf.GetInputStream(zipEntry))
                    {
                        using (StreamReader sr = new StreamReader(zipStream))
                        {
                            bool exit = false;
                            for (; ; )
                            {
                                string line = sr.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }

                                if (regExLine != null)
                                {
                                    MatchCollection matchesLine = regExLine.Matches(line);
                                    if ((matchesLine.Count == 1) && (matchesLine[0].Groups.Count > 1))
                                    {
                                        for (int match = 1; match < matchesLine[0].Groups.Count; match++)
                                        {
                                            if (matchesLine[0].Groups[match].Success)
                                            {
                                                string key = matchesLine[0].Groups[match].Value;
                                                if (!string.IsNullOrWhiteSpace(key))
                                                {
                                                    if (wordDict.ContainsKey(key))
                                                    {
                                                        wordDict[key] += 1;
                                                    }
                                                    else
                                                    {
                                                        wordDict[key] = 1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                string[] words = line.Split(' ', '\t', '\n', '\r');
                                foreach (string word in words)
                                {
                                    if (string.IsNullOrEmpty(word))
                                    {
                                        continue;
                                    }
                                    if (regExWord.IsMatch(word))
                                    {
                                        if (wordDict.ContainsKey(word))
                                        {
                                            wordDict[word] += 1;
                                        }
                                        else
                                        {
                                            if (maxWords > 0 && wordDict.Count > maxWords)
                                            {
                                                exit = true;
                                            }
                                            wordDict[word] = 1;
                                        }
                                    }
                                }
                                if (exit)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    index++;
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
            return wordDict;
        }

        // Returns the root directory node or null on error. The stream is not closed.
        public static ZipTreeNode ReadZipTree(Stream zipStream)
        {
            if (zipStream == null)
            {
                return null;
            }

            try
            {
                ZipTreeNode rootNode = new ZipTreeNode(string.Empty, true, null);
                if (zipStream.CanSeek)
                {
                    // Reads only the central directory
                    ZipFile zf = null;
                    try
                    {
                        zf = new ZipFile(zipStream);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            AddZipTreeEntry(rootNode, zipEntry);
                        }
                    }
                    finally
                    {
                        if (zf != null)
                        {
                            zf.IsStreamOwner = false;
                            zf.Close();
                        }
                    }
                }
                else
                {
                    ZipInputStream zipInputStream = new ZipInputStream(zipStream);
                    try
                    {
                        for (;;)
                        {
                            ZipEntry zipEntry = zipInputStream.GetNextEntry();
                            if (zipEntry == null)
                            {
                                break;
                            }

                            AddZipTreeEntry(rootNode, zipEntry);
                        }
                    }
                    finally
                    {
                        zipInputStream.IsStreamOwner = false;
                        zipInputStream.Close();
                    }
                }

                rootNode.SortChildren();
                return rootNode;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void AddZipTreeEntry(ZipTreeNode rootNode, ZipEntry zipEntry)
        {
            if (!zipEntry.IsFile && !zipEntry.IsDirectory)
            {
                return;
            }

            string[] nameParts = zipEntry.Name.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Where(part => part != ".").ToArray();
            if (nameParts.Length == 0)
            {
                return;
            }

            ZipTreeNode node = rootNode;
            for (int i = 0; i < nameParts.Length; i++)
            {
                // Parent directories may be missing as separate entries
                bool isDirectory = i < nameParts.Length - 1 || zipEntry.IsDirectory;
                ZipTreeNode childNode = node.GetChild(nameParts[i]);
                if (childNode == null)
                {
                    childNode = node.AddChild(nameParts[i], isDirectory);
                }
                else if (childNode.IsDirectory != isDirectory)
                {
                    return; // File and directory with the same name
                }

                node = childNode;
            }

            if (node.IsFile)
            {
                node.Size = zipEntry.Size;
                node.CompressedSize = zipEntry.CompressedSize;
            }

            node.LastModified = zipEntry.DateTime;
        }
    }
}
