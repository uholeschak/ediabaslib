using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdiabasLib
{
    public abstract class EdCommBase : IDisposable
    {
        private bool disposed = false;
        protected Ediabas ediabas;
        protected bool newCommPars = true;

        public abstract bool InterfaceConnect();
        public abstract bool InterfaceDisconnect();
        public abstract bool TransmitData(byte[] sendData, out byte[] receiveData);

        public void NewCommunicationPars()
        {
            newCommPars = true;
        }

        public abstract string InterfaceType
        {
            get;
        }

        public abstract UInt32 InterfaceVersion
        {
            get;
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

        protected EdCommBase(Ediabas ediabas)
        {
            this.ediabas = ediabas;
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
