using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EdiabasLib
{
    class MemoryStreamReader : Stream
    {
        [DllImport("Coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
            );

        [DllImport("Coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            object lpFileMappingAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            string lpName);

        [DllImport("Coredll.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFileForMapping(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes, // set null
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("Coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Coredll.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap);
        [DllImport("Coredll.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        private const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const UInt32 SECTION_QUERY = 0x0001;
        private const UInt32 SECTION_MAP_WRITE = 0x0002;
        private const UInt32 SECTION_MAP_READ = 0x0004;
        private const UInt32 SECTION_MAP_EXECUTE = 0x0008;
        private const UInt32 SECTION_EXTEND_SIZE = 0x0010;
        private const UInt32 SECTION_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SECTION_QUERY |
            SECTION_MAP_WRITE |
            SECTION_MAP_READ |
            SECTION_MAP_EXECUTE |
            SECTION_EXTEND_SIZE);
        private const UInt32 FILE_MAP_ALL_ACCESS = SECTION_ALL_ACCESS;

        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private const int PAGE_READWRITE = 0x04;
        private const int PAGE_READONLY = 0x02;
        private const int FILE_MAP_READ = 0x0004;
        private const int FILE_MAP_WRITE = 0x0002;

        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint FILE_SHARE_DELETE = 4;
        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;

        public MemoryStreamReader(string path)
        {
            this.filePos = 0;

            FileInfo fileInfo = new FileInfo(path);
            this.fileLength = fileInfo.Length;

            fileHandle = MemoryStreamReader.CreateFile(path, GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            if (fileHandle == INVALID_HANDLE_VALUE)
            {
                throw new FileNotFoundException();
            }

            mapFileHandle = MemoryStreamReader.CreateFileMapping(fileHandle, null, PAGE_READONLY, 0, 0, null);
            if (mapFileHandle == IntPtr.Zero)
            {
                CloseHandle(fileHandle);
                throw new FileNotFoundException();
            }

            mapBuff = MemoryStreamReader.MapViewOfFile(mapFileHandle, FILE_MAP_READ, 0, 0, 0);
            if (mapBuff == IntPtr.Zero)
            {
                CloseHandle(mapFileHandle);
                CloseHandle(fileHandle);
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
            if (mapBuff != IntPtr.Zero)
            {
                MemoryStreamReader.UnmapViewOfFile(mapBuff);
                mapBuff = IntPtr.Zero;
            }
            if (mapFileHandle != IntPtr.Zero)
            {
                MemoryStreamReader.CloseHandle(mapFileHandle);
                mapFileHandle = IntPtr.Zero;
            }
            if (fileHandle != INVALID_HANDLE_VALUE)
            {
                MemoryStreamReader.CloseHandle(fileHandle);
                mapFileHandle = INVALID_HANDLE_VALUE;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = this.filePos + (long)offset;
            long num2 = this.filePos + (long)offset + (long)count;
            if (num < 0L)
            {
                throw new ArgumentOutOfRangeException("Attempt to read before the start of the stream");
            }
            int useCount = count;
            if (num2 > this.fileLength)
            {
                useCount = (int)(this.fileLength - offset - this.filePos);
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
                return new IntPtr((long)this.mapBuff.ToInt32() + this.filePos);
            }
        }

        private IntPtr fileHandle = INVALID_HANDLE_VALUE;
        private IntPtr mapFileHandle = INVALID_HANDLE_VALUE;
        private IntPtr mapBuff = IntPtr.Zero;
        private long filePos = 0;
        private long fileLength = 0;
    }
}
