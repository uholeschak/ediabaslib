using System;
using System.Threading;

namespace EdiabasLib
{
    public abstract class EdInterfaceBase : IDisposable
    {
        private bool disposed = false;
        protected EdiabasNet ediabas;
        protected static Mutex interfaceMutex;
        protected bool mutexAquired = false;
        protected UInt32 commRepeats = 0;
        protected UInt32[] commParameter;
        protected Int16[] commAnswerLen = new Int16[2];

        public abstract bool IsValidInterfaceName(string name);

        public virtual bool InterfaceLock()
        {
            if (interfaceMutex == null)
            {
                return false;
            }
            try
            {
                if (!interfaceMutex.WaitOne(0, false))
                {
                    return false;
                }
                mutexAquired = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public virtual bool InterfaceUnlock()
        {
            if (interfaceMutex == null)
            {
                return true;
            }
            if (mutexAquired)
            {
                mutexAquired = false;
                interfaceMutex.ReleaseMutex();
            }
            return true;
        }

        public virtual bool InterfaceConnect()
        {
            commRepeats = 0;
            commParameter = null;
            commAnswerLen[0] = 0;
            commAnswerLen[1] = 0;
            return true;
        }

        public virtual bool InterfaceDisconnect()
        {
            return true;
        }

        public abstract bool TransmitData(byte[] sendData, out byte[] receiveData);

        public abstract bool ReceiveFrequent(out byte[] receiveData);

        public abstract bool StopFrequent();

        public virtual EdiabasNet Ediabas
        {
            get
            {
                return ediabas;
            }
            set
            {
                ediabas = value;
            }
        }

        public UInt32 CommRepeats
        {
            get
            {
                return commRepeats;
            }
            set
            {
                commRepeats = value;
            }
        }

        public virtual UInt32[] CommParameter
        {
            get;
            set;
        }

        public Int16[] CommAnswerLen
        {
            get
            {
                return commAnswerLen;
            }
            set
            {
                if (value != null && value.Length >= 2)
                {
                    commAnswerLen[0] = value[0];
                    commAnswerLen[1] = value[1];
                }
            }
        }

        public abstract string InterfaceType
        {
            get;
        }

        public abstract UInt32 InterfaceVersion
        {
            get;
        }

        public abstract string InterfaceName
        {
            get;
        }

        public virtual string InterfaceVerName
        {
            get
            {
                return "IFH-STD Version 7.3.0";
            }
        }

        public abstract byte[] KeyBytes
        {
            get;
        }

        public abstract byte[] State
        {
            get;
        }

        public abstract UInt32 BatteryVoltage
        {
            get;
        }

        public abstract UInt32 IgnitionVoltage
        {
            get;
        }

        public abstract bool Connected
        {
            get;
        }

        protected EdInterfaceBase()
        {
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
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

   }
}
