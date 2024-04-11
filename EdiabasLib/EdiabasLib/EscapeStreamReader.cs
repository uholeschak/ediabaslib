using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class EscapeStreamReader : Stream
    {
        private bool _disposed;
        private Stream _inStream;
        private Mutex _readMutex;
        private AutoResetEvent _writeEvent;
        private bool _escapeMode;
        private byte _escapeCode;
        private byte _escapeMask;
        private bool _readEscape;
        private List<byte> _readDataList;

        public EscapeStreamReader(Stream inStream, bool escapeMode = false, byte escapeCode = EdCustomAdapterCommon.EscapeCodeDefault, byte escapeMask = EdCustomAdapterCommon.EscapeMaskDefault)
        {
            _inStream = inStream;
            _readMutex = new Mutex(false);
            _writeEvent = new AutoResetEvent(false);
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
                if (!AcquireReadMutex())
                {
                    return 0;
                }

                try
                {
                    ReadInStream();
                    return _readDataList.Count;
                }
                finally
                {
                    ReleaseReadMutex();
                }
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
                    if (!AcquireReadMutex(-1))
                    {
                        return;
                    }

                    try
                    {
                        if (_inStream != null)
                        {
                            _inStream.Dispose();
                            _inStream = null;
                        }
                        _readDataList.Clear();
                        _readEscape = false;
                    }
                    finally
                    {
                        ReleaseReadMutex();
                    }

                    try
                    {
                        if (_writeEvent != null)
                        {
                            _writeEvent.Dispose();
                            _writeEvent = null;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        if (_readMutex != null)
                        {
                            _readMutex.Dispose();
                            _readMutex = null;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    // Note disposing has been done.
                    _disposed = true;
                }
            }

            base.Dispose(disposing);
        }

        public bool IsDataAvailable(int timeout = 0, ManualResetEvent cancelEvent = null)
        {
            _writeEvent.Reset();
            if (Length > 0)
            {
                return true;
            }

            if (timeout > 0)
            {
                if (cancelEvent != null)
                {
                    WaitHandle[] waitHandles = { _writeEvent, cancelEvent };
                    if (WaitHandle.WaitAny(waitHandles, timeout) == 1)
                    {
                        return false;
                    }
                }
                else
                {
                    _writeEvent.WaitOne(timeout);
                }
                return Length > 0;
            }

            return false;
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

            while (_inStream.HasData())
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

                    if (_readDataList.Count > 0)
                    {
                        _writeEvent.Set();
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
