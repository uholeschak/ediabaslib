using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace EdiabasLib
{
    class MemoryStreamReader : Stream
    {
        public MemoryStreamReader(string path)
        {
            if (!File.Exists(path))
            {   // try to use lower and upper case filenames
                string fileName = Path.GetFileName(path) ?? string.Empty;
                string dirName = Path.GetDirectoryName(path) ?? string.Empty;
                path = Path.Combine(dirName, fileName.ToLowerInvariant());
                if (!File.Exists(path))
                {
                    path = Path.Combine(dirName, fileName.ToUpperInvariant());
                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException();
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

        private MemoryMappedFile _mmFile;
        private MemoryMappedViewStream _mmStream;
        private readonly long _fileLength;
    }
}
