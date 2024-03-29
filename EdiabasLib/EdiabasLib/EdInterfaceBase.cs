using System;
using System.Collections.Generic;
using System.Threading;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public abstract class EdInterfaceBase : IDisposable
    {
        protected class TransmitStorage
        {
            public TransmitStorage(byte[] request, byte[] response, EdiabasNet.ErrorCodes errorCode)
            {
                Request = request;
                ResponseList = new List<byte[]>();
                if (response != null && response.Length > 0)
                {
                    byte[] buffer = new byte[response.Length];
                    Array.Copy(response, buffer, response.Length);
                    ResponseList.Add(buffer);
                }
                ErrorCode = errorCode;
            }

            public byte[] Request
            {
                set
                {
                    _request = value;
                    Key = BitConverter.ToString(value);
                }
                get { return _request; }
            }
            public string Key { get; private set; }
            private byte[] _request;
            public List<byte[]> ResponseList { get; set; }
            public EdiabasNet.ErrorCodes ErrorCode { get; set; }
        }

        // automatic baud rate detection
        // don't change this value, it's also used in the adapter.
        public static int BaudAuto = 2;

        private bool _disposed;
        private long _responseCounter;
        private object _responseCounterLock = new object();
        protected EdiabasNet EdiabasProtected;
        protected object ConnectParameterProtected;
        protected object MutexLock = new object();
        protected bool MutexAquired;
        protected UInt32 CommRepeatsProtected;
        protected UInt32[] CommParameterProtected;
        protected Int16[] CommAnswerLenProtected = new Int16[2];
        protected bool EnableTransCacheProtected;
        protected TransmitStorage ReadTransaction;
        protected TransmitStorage WriteTransaction;
        protected int ReadTransactionPos;
        protected Dictionary<string, TransmitStorage> TransmitCacheDict = new Dictionary<string, TransmitStorage>();

        protected virtual Mutex InterfaceMutex
        {
            get { return null; }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        protected virtual string InterfaceMutexName
        {
            get { return string.Empty; }
        }


        public abstract bool IsValidInterfaceName(string name);

        public virtual bool InterfaceLock()
        {
            lock (MutexLock)
            {
                if (InterfaceMutex == null)
                {
                    return false;
                }
                try
                {
                    if (!InterfaceMutex.WaitOne(0))
                    {
                        return false;
                    }
                    MutexAquired = true;
                }
                catch (AbandonedMutexException)
                {
                    MutexAquired = true;
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public virtual bool InterfaceUnlock()
        {
            lock (MutexLock)
            {
                if (InterfaceMutex == null)
                {
                    return true;
                }
                if (MutexAquired)
                {
                    try
                    {
                        InterfaceMutex.ReleaseMutex();
                    }
                    catch (Exception)
                    {
                        InterfaceMutex.Close();
                        InterfaceMutex.Dispose();
#if Android
                        InterfaceMutex = new Mutex(false);
#else
                        InterfaceMutex = new Mutex(false, InterfaceMutexName);
#endif
                    }

                    MutexAquired = false;
                }

                return true;
            }
        }

        public virtual bool InterfaceConnect()
        {
            CommRepeatsProtected = 0;
            CommParameterProtected = null;
            CommAnswerLenProtected[0] = 0;
            CommAnswerLenProtected[1] = 0;
            ResponseCount = 0;
            return true;
        }

        public virtual bool InterfaceDisconnect()
        {
            return true;
        }

        public abstract bool InterfaceReset();

        public abstract bool InterfaceBoot();

        public abstract bool TransmitData(byte[] sendData, out byte[] receiveData);

        public abstract bool TransmitFrequent(byte[] sendData);

        public abstract bool ReceiveFrequent(out byte[] receiveData);

        public abstract bool StopFrequent();

        public abstract bool RawData(byte[] sendData, out byte[] receiveData);

        public abstract bool TransmitCancel(bool cancel);

        public virtual EdiabasNet Ediabas
        {
            get { return EdiabasProtected; }
            set { EdiabasProtected = value; }
        }

        public virtual object ConnectParameter
        {
            get { return ConnectParameterProtected; }
            set { ConnectParameterProtected = value; }
        }

        public UInt32 CommRepeats
        {
            get { return CommRepeatsProtected; }
            set { CommRepeatsProtected = value; }
        }

        public virtual UInt32[] CommParameter { get; set; }

        public Int16[] CommAnswerLen
        {
            get { return CommAnswerLenProtected; }
            set
            {
                if (value != null && value.Length >= 2)
                {
                    CommAnswerLenProtected[0] = value[0];
                    CommAnswerLenProtected[1] = value[1];
                    EdiabasProtected.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Answer length: {0:X04} {1:X04}", CommAnswerLenProtected[0], CommAnswerLenProtected[1]);
                }
            }
        }

        public abstract bool BmwFastProtocol { get; }

        public abstract string InterfaceType { get; }

        public abstract UInt32 InterfaceVersion { get; }

        public abstract string InterfaceName { get; }

        public virtual string InterfaceVerName
        {
            get { return "IFH-STD Version 7.6.0"; }
        }

        public abstract byte[] KeyBytes { get; }

        public abstract byte[] State { get; }

        public abstract Int64 BatteryVoltage { get; }

        public abstract Int64 IgnitionVoltage { get; }

        public abstract int? AdapterVersion { get; }

        public abstract byte[] AdapterSerial { get; }

        public abstract double? AdapterVoltage { get; }

        public abstract Int64 GetPort(UInt32 index);

        public abstract bool Connected { get; }

        public virtual long ResponseCount
        {
            get
            {
                lock (_responseCounterLock)
                {
                    return _responseCounter;
                }
            }
            set
            {
                lock (_responseCounterLock)
                {
                    _responseCounter = value;
                }
            }
        }

        public virtual long IncResponseCount(long offset)
        {
            lock (_responseCounterLock)
            {
                _responseCounter += offset;
                return _responseCounter;
            }
        }

        public bool EnableTransmitCache
        {
            get
            {
                return EnableTransCacheProtected;
            }
            set
            {
                EnableTransCacheProtected = value;
                if (!EnableTransCacheProtected)
                {
                    WriteTransaction = null;
                    ReadTransaction = null;
                    ReadTransactionPos = 0;
                    TransmitCacheDict.Clear();
                }
            }
        }

        protected void CacheTransmission(byte[] request, byte[] response, EdiabasNet.ErrorCodes errorCode)
        {
            if (!EnableTransmitCache)
            {
                return;
            }
            if (errorCode == EdiabasNet.ErrorCodes.EDIABAS_IFH_0003)
            {
                return;
            }
            if (request.Length > 0)
            {
                StoreWriteTransaction();
                WriteTransaction = new TransmitStorage(request, response, errorCode);
            }
            else
            {
                if (WriteTransaction != null)
                {
                    if (response != null && response.Length > 0)
                    {
                        byte[] buffer = new byte[response.Length];
                        Array.Copy(response, buffer, response.Length);
                        WriteTransaction.ResponseList.Add(buffer);
                    }
                    if (errorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE && WriteTransaction.ErrorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                    {
                        WriteTransaction.ErrorCode = errorCode;
                    }
                }
            }
        }

        protected void StoreWriteTransaction()
        {
            if (WriteTransaction != null)
            {
                if (TransmitCacheDict.ContainsKey(WriteTransaction.Key))
                {
                    TransmitCacheDict.Remove(WriteTransaction.Key);
                }
                TransmitCacheDict.Add(WriteTransaction.Key, WriteTransaction);
                WriteTransaction = null;
            }
        }

        protected bool ReadCachedTransmission(byte[] request, out byte[] response, out EdiabasNet.ErrorCodes errorCode)
        {
            response = null;
            errorCode = EdiabasNet.ErrorCodes.EDIABAS_IFH_0009;
            if (!EnableTransmitCache)
            {
                return false;
            }
            if (request.Length > 0)
            {
                StoreWriteTransaction();
                ReadTransaction = null;
                ReadTransactionPos = 0;
                TransmitCacheDict.TryGetValue(BitConverter.ToString(request), out ReadTransaction);
            }
            if (ReadTransaction == null)
            {
                return false;
            }
            if (request.Length > 0)
            {
                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, request, 0, request.Length, "Send Cache");
            }
            if ((ReadTransaction.Request.Length > 0) && ((ReadTransaction.Request[0] & 0xC0) == 0xC0))
            {   // functional address
                if (ReadTransaction.ErrorCode == EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {   // transmission not completed
                    Ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Incomplete cached response");
                    return false;
                }
            }
            if (ReadTransaction.ResponseList.Count > ReadTransactionPos)
            {
                response = ReadTransaction.ResponseList[ReadTransactionPos++];
                errorCode = EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE;
                Ediabas.LogData(EdiabasNet.EdLogLevel.Ifh, response, 0, response.Length, "Resp Cache");
            }
            else
            {
                if (ReadTransaction.ErrorCode != EdiabasNet.ErrorCodes.EDIABAS_ERR_NONE)
                {
                    errorCode = ReadTransaction.ErrorCode;
                }
            }
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

   }
}
