using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class EscapeStreamWriter : Stream
    {
        private Stream _outStream;
        private Mutex _writeMutex;
        private bool _escapeMode;
        private byte _escapeCode;
        private byte _escapeMask;
        private List<byte> _writeDataList;

        public EscapeStreamWriter(Stream outStream, bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            _outStream = outStream;
            _writeMutex = new Mutex(false);
            SetEscapeMode(escapeMode, escapeCode, escapeMask);
            _writeDataList = new List<byte>();
        }

        public void SetEscapeMode(bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            _escapeMode = escapeMode;
            _escapeCode = escapeCode;
            _escapeMask = escapeMask;
        }

        public bool EscapeMode
        {
            get
            {
                return _escapeMode;
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return _writeDataList.Count;
            }
        }

        public override long Position
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
            if (!AcquireWriteMutex())
            {
                return;
            }

            try
            {
                _outStream?.Flush();
            }
            finally
            {
                ReleaseWriteMutex();
            }
        }

        public override void Close()
        {
            if (!AcquireWriteMutex(-1))
            {
                return;
            }

            try
            {
                if (_outStream != null)
                {
                    _outStream.Close();
                    _outStream = null;
                }
                _writeDataList.Clear();
            }
            finally
            {
                ReleaseWriteMutex();
            }

            try
            {
                if (_writeMutex != null)
                {
                    _writeMutex.Close();
                    _writeMutex = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
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

        public override void WriteByte(byte value)
        {
            if (!AcquireWriteMutex())
            {
                return;
            }

            try
            {
                if (_outStream == null)
                {
                    return;
                }

                _writeDataList.Add(value);
                WriteOutStream();
            }
            finally
            {
                ReleaseWriteMutex();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!AcquireWriteMutex())
            {
                return;
            }

            try
            {
                if (_outStream == null)
                {
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    _writeDataList.Add(buffer[i + offset]);
                }

                WriteOutStream();
            }
            finally
            {
                ReleaseWriteMutex();
            }
        }

        private bool AcquireWriteMutex(int timeout = 10000)
        {
            try
            {
                if (!_writeMutex.WaitOne(timeout))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReleaseWriteMutex()
        {
            try
            {
                _writeMutex.ReleaseMutex();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void WriteOutStream()
        {
            if (_outStream == null)
            {
                return;
            }

            if (!_escapeMode)
            {
                if (_writeDataList.Count > 0)
                {
                    _outStream.Write(_writeDataList.ToArray(), 0, _writeDataList.Count);
                    _writeDataList.Clear();
                }
            }
            else
            {
                List<byte> writeData = new List<byte>();
                while (_writeDataList.Count > 0)
                {
                    byte data = _writeDataList[0];
                    if (data == 0 || data == _escapeCode)
                    {
                        writeData.Add(_escapeCode);
                        writeData.Add((byte) (data ^ _escapeMask));
                    }
                    else
                    {
                        writeData.Add(data);
                    }
                    _writeDataList.RemoveAt(0);
                }

                if (writeData.Count > 0)
                {
                    _outStream.Write(writeData.ToArray(), 0, writeData.Count);
                }
            }
        }
    }
}
