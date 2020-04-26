using System;
using System.Collections.Generic;
using System.IO;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class BtEscapeStreamReader : Stream
    {
        private Stream _inStream;
        private bool _escapeMode;
        private byte _escapeCode;
        private byte _escapeMask;
        private bool readEscape;
        private List<byte> _readDataList;

        public BtEscapeStreamReader(Stream inStream, bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            _inStream = inStream;
            SetEscapeMode(escapeMode, escapeCode, escapeMask);
            _readDataList = new List<byte>();
        }

        public void SetEscapeMode(bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            if (_escapeMode != escapeMode)
            {
                readEscape = false;
            }
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
                return true;
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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _readDataList.Count;
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
            _inStream?.Flush();
        }

        public override void Close()
        {
            if (_inStream != null)
            {
                _inStream.Close();
                _inStream = null;
            }
            _readDataList.Clear();
            readEscape = false;
        }

        public bool IsDataAvailable()
        {
            ReadInStream();
            return _readDataList.Count > 0;
        }

        public override int ReadByte()
        {
            ReadInStream();
            if (_readDataList.Count < 1)
            {
                return -1;
            }

            int data = _readDataList[0];
            _readDataList.RemoveAt(0);
            return data;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int length = 0;
            for (int i = 0; i < count; i++)
            {
                int data = ReadByte();
                if (data < 0)
                {
                    break;
                }

                buffer[length + offset] = (byte) data;
                length++;
            }

            return length;
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private void ReadInStream()
        {
            if (_inStream == null)
            {
                return;
            }

            while (_inStream.IsDataAvailable())
            {
                int data = _inStream.ReadByte();
                if (data >= 0)
                {
                    if (_escapeMode)
                    {
                        if (readEscape)
                        {
                            data ^= _escapeMask;
                            readEscape = false;
                        }
                        else
                        {
                            if (data == _escapeCode)
                            {
                                readEscape = true;
                            }
                        }
                    }
                    else
                    {
                        readEscape = false;
                    }

                    if (!readEscape)
                    {
                        _readDataList.Add((byte)data);
                    }
                }
            }
        }
    }
}
