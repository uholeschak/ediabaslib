using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace EdiabasLib
{
    class MemoryStreamReader : Stream
    {
        public MemoryStreamReader(string path)
        {
            if (!File.Exists(path))
            {   // get the case sensitive name from the directory
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        break;

                    default:
                        throw new FileNotFoundException();
                }
                string fileName = Path.GetFileName(path);
                string dirName = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(dirName))
                {
                    throw new FileNotFoundException();
                }
                lock (DirDictLock)
                {
                    bool newDict = false;
                    for (int i = 0; i < 2; i++)
                    {
                        if ((_dirDict == null) || (string.Compare(dirName, _dirDictName, StringComparison.Ordinal) != 0))
                        {
                            Dictionary<string, string> dirDict = GetDirDict(dirName);
                            if (dirDict == null)
                            {
                                throw new FileNotFoundException();
                            }
                            _dirDictName = dirName;
                            _dirDict = dirDict;
                            newDict = true;
                        }
                        string realName;
                        if (!_dirDict.TryGetValue(fileName.ToUpperInvariant(), out realName))
                        {
                            if (!newDict)
                            {
                                _dirDict = null;
                                continue;
                            }
                            throw new FileNotFoundException();
                        }
                        path = Path.Combine(dirName, realName);
                        if (!File.Exists(path))
                        {
                            if (!newDict)
                            {
                                _dirDict = null;
                                continue;
                            }
                            throw new FileNotFoundException();
                        }
                        break;
                    }
                }
            }

            FileInfo fileInfo = new FileInfo(path);
            _fileLength = fileInfo.Length;

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None);
            try
            {
                _mmFile = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.None, false);
                _mmStream = _mmFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            }
            catch (Exception)
            {
                CloseHandles();
                throw;
            }
        }

        public static MemoryStreamReader OpenRead(string path)
        {
            return new MemoryStreamReader(path);
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _fileLength;
            }
        }

        public override long Position
        {
            get
            {
                return _mmStream.Position;
            }
            set
            {
                _mmStream.Position = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int ReadTimeout
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            _mmStream.Flush();
        }

        public override void Close()
        {
            CloseHandles();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = _mmStream.Position;
            long num2 = _mmStream.Position + count;
            if (num < 0L)
            {
                throw new Exception("Attempt to read before the start of the stream");
            }
            int useCount = count;
            if (num2 > _fileLength)
            {
                useCount = (int)(_fileLength - offset - _mmStream.Position);
                if (useCount < 0)
                {
                    useCount = 0;
                }
            }
            return _mmStream.Read(buffer, offset, useCount);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos = 0;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPos = offset;
                    break;

                case SeekOrigin.Current:
                    newPos = _mmStream.Position + offset;
                    break;

                case SeekOrigin.End:
                    newPos = _fileLength + offset;
                    break;
            }
            if (newPos < 0)
            {
                throw new Exception("Attempt to seek before start of stream");
            }
            if (newPos >= _fileLength)
            {
                throw new Exception("Attempt to seek after end of stream");
            }
            _mmStream.Position = newPos;
            return _mmStream.Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private void CloseHandles()
        {
            if (_mmStream != null)
            {
                _mmStream.Dispose();
                _mmStream = null;
            }
            if (_mmFile != null)
            {
                _mmFile.Dispose();
                _mmFile = null;
            }
        }

        private Dictionary<string, string> GetDirDict(string dirName)
        {
            try
            {
                Dictionary<string, string> dirDict = new Dictionary<string, string>();
                DirectoryInfo dir = new DirectoryInfo(dirName);
                foreach (FileSystemInfo fsi in dir.GetFileSystemInfos())
                {
                    if ((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        continue;
                    }
                    string key = fsi.Name.ToUpperInvariant();
                    if (!dirDict.ContainsKey(key))
                    {
                        dirDict.Add(key, fsi.Name);
                    }
                }
                return dirDict;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private MemoryMappedFile _mmFile;
        private MemoryMappedViewStream _mmStream;
        private readonly long _fileLength;
        private static readonly object DirDictLock = new object();
        private static string _dirDictName = string.Empty;
        private static Dictionary<string, string> _dirDict;
    }
}
