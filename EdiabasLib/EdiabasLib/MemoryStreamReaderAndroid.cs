using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Android.OS;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class MemoryStreamReader : Stream
    {
        // ReSharper disable UnusedMember.Local
        const int Deffilemode = 0x666;
        const int ORdonly = 0x0;
        const int OWronly = 0x1;
        const int ORdwr = 0x2;

        const int ProtRead = 0x1;
        const int ProtWrite = 0x2;
        const int ProtExec = 0x4;

        const int MapPrivate = 0x2;
        const int MapShared = 0x1;
        // ReSharper restore UnusedMember.Local

        private bool _disposed;
        private long _filePos;
        private readonly long _fileLength;
        private Java.IO.FileDescriptor _fd;
        private long _mapAddr;
        private static readonly object DirDictLock = new object();
        private static string _dirDictName = string.Empty;
        private static Dictionary<string, string> _dirDict;
        private static DirectoryObserver _directoryObserver;

        public MemoryStreamReader(string path)
        {
            _filePos = 0;
            _fileLength = 0;
            _fd = null;
            _mapAddr = (IntPtr)(-1);

            if (!File.Exists(path))
            {   // get the case sensitive name from the directory
                string fileName = Path.GetFileName(path);
                string dirName = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(dirName))
                {
                    throw new FileNotFoundException();
                }
                lock (DirDictLock)
                {
                    if ((_dirDict == null) || (_directoryObserver == null) ||
                        (string.Compare(dirName, _dirDictName, StringComparison.Ordinal) != 0))
                    {
                        Dictionary<string, string> dirDict = GetDirDict(dirName);
                        // ReSharper disable once JoinNullCheckWithUsage
                        if (dirDict == null)
                        {
                            throw new FileNotFoundException();
                        }
                        _dirDictName = dirName;
                        _dirDict = dirDict;
                        RemoveDirectoryObserver();
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                        {
                            _directoryObserver = new DirectoryObserver(dirName);
                        }
                        else
                        {
                            _directoryObserver = new DirectoryObserver(new Java.IO.File(dirName));
                        }
                        _directoryObserver.StartWatching();
                    }

                    if (!_dirDict.TryGetValue(fileName.ToUpperInvariant(), out string realName))
                    {
                        throw new FileNotFoundException();
                    }

                    path = Path.Combine(dirName, realName);
                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException();
                    }
                }
            }

            FileInfo fileInfo = new FileInfo(path);
            _fileLength = fileInfo.Length;

            bool openSuccess = false;
            string failureReason = string.Empty;
            _fd = Android.Systems.Os.Open(path, ORdonly, Deffilemode);
            if (_fd != null)
            {
                _mapAddr = Android.Systems.Os.Mmap(0, _fileLength, ProtRead, MapPrivate, _fd, 0);
                if (_mapAddr != -1)
                {
                    openSuccess = true;
                }
                else
                {
                    failureReason = "Os.Mmap failed";
                }
            }
            else
            {
                failureReason = "Os.Open failed";
            }

            if (!openSuccess)
            {
                CloseHandles();
                throw new FileNotFoundException(failureReason);
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
                return _filePos;
            }
            set
            {
                _filePos = value;
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
        }

        // Stream Close() calls Dispose(true)
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    CloseHandles();
                }

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = _filePos;
            long num2 = _filePos + count;
            if (num < 0L)
            {
                throw new Exception("Attempt to read before the start of the stream");
            }
            int useCount = count;
            if (num2 > _fileLength)
            {
                useCount = (int)(_fileLength - offset - _filePos);
                if (useCount < 0)
                {
                    useCount = 0;
                }
            }
            Marshal.Copy(PosPtr, buffer, offset, useCount);
            _filePos += useCount;
            return useCount;
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
                    newPos = _filePos + offset;
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
            _filePos = newPos;
            return _filePos;
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

        public static void CleanUp()
        {
            RemoveDirectoryObserver();
        }

        private IntPtr PosPtr
        {
            get
            {
                return new IntPtr(_mapAddr + _filePos);
            }
        }

        private void CloseHandles()
        {
            if (_mapAddr != -1)
            {
                Android.Systems.Os.Munmap(_mapAddr, _fileLength);
                _mapAddr = -1;
            }

            if (_fd != null)
            {
                Android.Systems.Os.Close(_fd);
                _fd = null;
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

        private static void RemoveDirectoryObserver()
        {
            lock (DirDictLock)
            {
                if (_directoryObserver != null)
                {
                    try
                    {
                        _directoryObserver.StopWatching();
                        // don't dispose the observer, otherwise it will crash at exit
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    _directoryObserver = null;
                }
            }
        }

        private class DirectoryObserver : FileObserver
        {
            const FileObserverEvents Events =
                (FileObserverEvents.Modify | FileObserverEvents.CloseWrite | FileObserverEvents.MovedFrom | FileObserverEvents.MovedTo |
                FileObserverEvents.Create | FileObserverEvents.Delete | FileObserverEvents.DeleteSelf | FileObserverEvents.MoveSelf);

            public DirectoryObserver(String rootPath)
#pragma warning disable 618
#pragma warning disable CA1422
                : base(rootPath, Events)
#pragma warning restore CA1422
#pragma warning restore 618
            {
            }

            public DirectoryObserver(Java.IO.File rootPath)
                : base(rootPath, Events)
            {
            }

            public override void OnEvent(FileObserverEvents e, String path)
            {
                //Android.Util.Log.Info("File event", String.Format("{0}:{1}", path, e));
                if ((e & Events) != 0)
                {
                    lock (DirDictLock)
                    {
                        StopWatching();
                        _dirDict = null;
                    }
                }
            }
        }
    }
}
