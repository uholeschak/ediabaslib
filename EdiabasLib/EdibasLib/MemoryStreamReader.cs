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
            mmFile = MemoryMappedFile.CreateFromFile(path);
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
                return mmStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return mmStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return mmStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return mmStream.Length;
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
                return mmStream.WriteTimeout;
            }
            set
            {
                mmStream.WriteTimeout = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return mmStream.ReadTimeout;
            }
            set
            {
                mmStream.ReadTimeout = value;
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
            return mmStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return mmStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            mmStream.SetLength(value);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return mmStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return mmStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return mmStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            mmStream.EndWrite(asyncResult);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            mmStream.Write(buffer, offset, count);
        }

        private MemoryMappedFile mmFile = null;
        private MemoryMappedViewStream mmStream = null;
    }
}
