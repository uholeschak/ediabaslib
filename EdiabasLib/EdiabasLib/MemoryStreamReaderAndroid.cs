using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EdiabasLib
{
    class MemoryStreamReader : Stream
    {
        [DllImport("libc", SetLastError = true)]
        static extern int open(string path, int flags, int access);

        [DllImport("libc")]
        static extern int close(int fd);

        [DllImport("libc")]
        static extern IntPtr mmap(IntPtr addr, IntPtr len, int prot, int flags, int fd, int offset);

        [DllImport("libc")]
        static extern int munmap(IntPtr addr, IntPtr size);

        const int DEFFILEMODE = 0x666;
        const int O_RDONLY = 0x0;
        const int O_WRONLY = 0x1;
        const int O_RDWR = 0x2;

        const int PROT_READ = 0x1;
        const int PROT_WRITE = 0x2;
        const int PROT_EXEC = 0x4;

        const int MAP_PRIVATE = 0x2;
        const int MAP_SHARED = 0x1;

        public MemoryStreamReader(string path)
        {
            this.filePos = 0;
            this.fd = -1;
            this.mapAddr = (IntPtr)(-1);

            FileInfo fileInfo = new FileInfo(path);
            this.fileLength = fileInfo.Length;

            bool openSuccess = false;
            fd = open(path, O_RDONLY, DEFFILEMODE);
            if (fd != -1)
            {
                this.mapAddr = mmap(IntPtr.Zero, (IntPtr)this.fileLength, PROT_READ, MAP_PRIVATE, fd, 0);
                if (this.mapAddr != (IntPtr)(-1))
                {
                    openSuccess = true;
                }
            }
            if (!openSuccess)
            {
                Close();
                throw new FileNotFoundException();
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
                return this.fileLength;
            }
        }

        public override long Position
        {
            get
            {
                return filePos;
            }
            set
            {
                filePos = value;
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

        public override void Close()
        {
            if (this.mapAddr != (IntPtr)(-1))
            {
                munmap(this.mapAddr, (IntPtr)this.fileLength);
                this.mapAddr = (IntPtr)(-1);
            }
            if (this.fd != -1)
            {
                close(this.fd);
                this.fd = -1;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = this.filePos;
            long num2 = this.filePos + (long)count;
            if (num < 0L)
            {
                throw new ArgumentOutOfRangeException("Attempt to read before the start of the stream");
            }
            int useCount = count;
            if (num2 > this.fileLength)
            {
                useCount = (int)(this.fileLength - offset - this.filePos);
                if (useCount < 0)
                {
                    useCount = 0;
                }
            }
            Marshal.Copy(this.PosPtr, buffer, offset, useCount);
            this.filePos += (long)useCount;
            return count;
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
                    newPos = this.filePos + offset;
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
            this.filePos = newPos;
            return this.filePos;
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

        private IntPtr PosPtr
        {
            get
            {
                return IntPtr.Add(this.mapAddr, (int)this.filePos);
            }
        }

        private long filePos = 0;
        private long fileLength = 0;
        private int fd = -1;
        private IntPtr mapAddr = (IntPtr)(-1);
    }
}
