using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class EscapeStreamReader : Stream
    {
        private Stream _inStream;
        private Mutex _readMutex;
        private bool _escapeMode;
        private byte _escapeCode;
        private byte _escapeMask;
        private bool _readEscape;
        private List<byte> _readDataList;

        public EscapeStreamReader(Stream inStream, bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            _inStream = inStream;
            _readMutex = new Mutex(false);
            SetEscapeMode(escapeMode, escapeCode, escapeMask);
            _readDataList = new List<byte>();
        }

        public void SetEscapeMode(bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            if (_escapeMode != escapeMode)
            {
                _readEscape = false;
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
            if (!AcquireReadMutex())
            {
                return;
            }

            try
            {
                _inStream?.Flush();
            }
            finally
            {
                ReleaseReadMutex();
            }
        }

        public override void Close()
        {
            if (!AcquireReadMutex(-1))
            {
                return;
            }

            try
            {
                if (_inStream != null)
                {
                    _inStream.Close();
                    _inStream = null;
                }
                _readDataList.Clear();
                _readEscape = false;
            }
            finally
            {
                ReleaseReadMutex();
            }
        }

        public bool IsDataAvailable()
        {
            if (!AcquireReadMutex())
            {
                return false;
            }

            try
            {
                ReadInStream();
                return _readDataList.Count > 0;
            }
            finally
            {
                ReleaseReadMutex();
            }
        }

        public override int ReadByte()
        {
            if (!AcquireReadMutex())
            {
                return -1;
            }

            try
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
            finally
            {
                ReleaseReadMutex();
            }
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

        private bool AcquireReadMutex(int timeout = 10000)
        {
            try
            {
                if (!_readMutex.WaitOne(timeout))
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

        private void ReleaseReadMutex()
        {
            try
            {
                _readMutex.ReleaseMutex();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ReadInStream()
        {
            if (_inStream == null)
            {
                return;
            }

#if Android
            while (_inStream.IsDataAvailable())
#else
            while (_inStream.Length > 0)
#endif
            {
                int data = _inStream.ReadByteAsync();
                //Android.Util.Log.Debug("InStream", "Main Data: {0}", data);
                if (data >= 0)
                {
                    if (_escapeMode)
                    {
                        if (_readEscape)
                        {
                            data ^= _escapeMask;
                            _readEscape = false;
                        }
                        else
                        {
                            if (data == _escapeCode)
                            {
                                _readEscape = true;
                            }
                        }
                    }
                    else
                    {
                        _readEscape = false;
                    }

                    if (!_readEscape)
                    {
                        _readDataList.Add((byte)data);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
