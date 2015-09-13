using System;
using System.Threading;

namespace EdiabasLib
{
    public abstract class EdInterfaceBase : IDisposable
    {
        private bool _disposed;
        protected EdiabasNet EdiabasProtected;
        protected object ConnectParameterProtected;
        protected static Mutex InterfaceMutex;
        protected bool MutexAquired;
        protected UInt32 CommRepeatsProtected;
        protected UInt32[] CommParameterProtected;
        protected Int16[] CommAnswerLenProtected = new Int16[2];

        public abstract bool IsValidInterfaceName(string name);

        public virtual bool InterfaceLock()
        {
            if (InterfaceMutex == null)
            {
                return false;
            }
            try
            {
                if (!InterfaceMutex.WaitOne(0, false))
                {
                    return false;
                }
                MutexAquired = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public virtual bool InterfaceUnlock()
        {
            if (InterfaceMutex == null)
            {
                return true;
            }
            if (MutexAquired)
            {
                MutexAquired = false;
                InterfaceMutex.ReleaseMutex();
            }
            return true;
        }

        public virtual bool InterfaceConnect()
        {
            CommRepeatsProtected = 0;
            CommParameterProtected = null;
            CommAnswerLenProtected[0] = 0;
            CommAnswerLenProtected[1] = 0;
            return true;
        }

        public virtual bool InterfaceDisconnect()
        {
            return true;
        }

        public abstract bool InterfaceReset();

        public abstract bool TransmitData(byte[] sendData, out byte[] receiveData);

        public abstract bool ReceiveFrequent(out byte[] receiveData);

        public abstract bool StopFrequent();

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
                }
            }
        }

        public abstract string InterfaceType { get; }

        public abstract UInt32 InterfaceVersion { get; }

        public abstract string InterfaceName { get; }

        public virtual string InterfaceVerName
        {
            get { return "IFH-STD Version 7.3.0"; }
        }

        public abstract byte[] KeyBytes { get; }

        public abstract byte[] State { get; }

        public abstract Int64 BatteryVoltage { get; }

        public abstract Int64 IgnitionVoltage { get; }

        public abstract bool Connected { get; }

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
