using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace EdiabasLib
{
    class MemoryStreamReader : Stream
    {
        public MemoryStreamReader(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            this.fileLength = fileInfo.Length;

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None);
#if MMAP_PAGESIZE_BUG
            mmFile = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            System.Reflection.Assembly memMapAssem = typeof(MemoryMappedFile).Assembly;
            Type type = memMapAssem.GetType("System.IO.MemoryMappedFiles.MemoryMapImpl");
            if (type != null)
            {
                System.Reflection.FieldInfo info = type.GetField("pagesize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (info != null)
                {
                    info.SetValue(null, 4096);
                }
            }
#else
            mmFile = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read, null, HandleInheritability.None, false);
#endif
            mmStream = mmFile.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
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
                return this.fileLength;
            }
        }

        public override long Position
        {
            get
            {
                return mmStream.Position;
            }
            set
            {
                mmStream.Position = value;
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
            mmStream.Flush();
        }

        public override void Close()
        {
            if (mmStream != null)
            {
                mmStream.Dispose();
                mmStream = null;
            }
            if (mmFile != null)
            {
                mmFile.Dispose();
                mmFile = null;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = mmStream.Position;
            long num2 = mmStream.Position + (long)count;
            if (num < 0L)
            {
                throw new ArgumentOutOfRangeException("Attempt to read before the start of the stream");
            }
            int useCount = count;
            if (num2 > this.fileLength)
            {
                useCount = (int)(this.fileLength - offset - mmStream.Position);
                if (useCount < 0)
                {
                    useCount = 0;
                }
            }
            return mmStream.Read(buffer, offset, useCount);
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
                    newPos = mmStream.Position + offset;
                    break;

                case SeekOrigin.End:
                    newPos = fileLength + offset;
                    break;
            }
            if (newPos < 0)
            {
                throw new ArgumentOutOfRangeException("Attempt to seek before start of stream");
            }
            if (newPos >= fileLength)
            {
                throw new ArgumentOutOfRangeException("Attempt to seek after end of stream");
            }
            mmStream.Position = newPos;
            return mmStream.Position;
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

        private MemoryMappedFile mmFile = null;
        private MemoryMappedViewStream mmStream = null;
        private long fileLength = 0;
    }
}
