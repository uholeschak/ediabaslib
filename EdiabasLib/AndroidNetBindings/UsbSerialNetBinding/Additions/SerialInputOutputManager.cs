//
// Copyright 2014 LusoVU. All rights reserved.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301,
// USA.
// 
// Project home page: https://bitbucket.com/lusovu/xamarinusbserial
// 

using System;
using System.Threading;
using Android.Util;
using Hoho.Android.UsbSerial.Driver;

// ReSharper disable once CheckNamespace
namespace Hoho.Android.UsbSerial.Util
{
    public class SerialInputOutputManager : IDisposable
    {
        private static readonly string Tag = typeof (SerialInputOutputManager).Name;
        private const int DefaultReadTimeout = 1000;
        private const int DefaultBuffersize = 0x1000;

        private readonly IUsbSerialPort _port;
        private byte[] _buffer;
        private int _readTimeout;
        private Thread _commThread;
        private bool _terminateThread;

        public SerialInputOutputManager(IUsbSerialPort port)
        {
            _port = port;
        }

        public event EventHandler<SerialDataReceivedArgs> DataReceived;

        public event EventHandler<UnhandledExceptionEventArgs> ErrorReceived;

        public void Start(int bufferSize = DefaultBuffersize, int readTimeout = DefaultReadTimeout)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (IsStarted)
            {
                return;
            }
            _buffer = new byte[bufferSize];
            _readTimeout = readTimeout;

            _terminateThread = false;
            _commThread = new Thread(CommThreadFunc)
            {
                Priority = ThreadPriority.Highest
            };
            _commThread.Start();
        }

        public void Stop()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_commThread != null)
            {
                _terminateThread = true;
                _commThread.Join();
                _commThread = null;
                _buffer = null;
            }
        }

        public bool IsStarted => _commThread != null;

        private void CommThreadFunc()
        {
            while (!_terminateThread)
            {
                try
                {
                    Step();
                }
                catch (Exception ex)
                {
                    Log.Warn(Tag, "USB task exception: " + (ex.Message ?? string.Empty), ex);
                    EventHandler<UnhandledExceptionEventArgs> handler = Volatile.Read(ref ErrorReceived);
                    handler?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
                }
            }
        }

        private void Step()
        {
            // handle incoming data.
            var len = _port.Read(_buffer, _readTimeout);
            if (len > 0 && DataReceived != null)
            {
                Log.Debug(Tag, "Read data len=" + len);

                var data = new byte[len];
                Array.Copy(_buffer, data, len);
                EventHandler<SerialDataReceivedArgs> handler = Volatile.Read(ref DataReceived);
                handler?.Invoke(this, new SerialDataReceivedArgs(data));
            }
        }

        #region Dispose pattern implementation

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }

        ~SerialInputOutputManager()
        {
            Dispose(false);
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #endregion
    }
}
