using System;
using System.IO;

namespace EdiabasLib
{
    class MemoryStreamReader : Stream
    {
        public MemoryStreamReader(string path)
        {
            this.filePos = 0;

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.None);
            this.fileLength = fileStream.Length;
            this.fileBuffer = new byte[this.fileLength];
            fileStream.Read(this.fileBuffer, 0, (int)this.fileLength);
            fileStream.Close();
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
            fileBuffer = null;
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
            Array.Copy(this.fileBuffer, this.filePos, buffer, offset, useCount);
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

        private byte[] fileBuffer = null;
        private long filePos = 0;
        private long fileLength = 0;
    }
}
