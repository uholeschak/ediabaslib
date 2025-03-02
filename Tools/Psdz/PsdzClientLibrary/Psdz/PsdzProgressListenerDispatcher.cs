using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Client
{
    internal class PsdzProgressListenerDispatcher : IPsdzProgressListener
    {
        private readonly IList<IPsdzProgressListener> psdzProgressListeners = new List<IPsdzProgressListener>();

        public void AddPsdzProgressListener(IPsdzProgressListener progressListener)
        {
            lock (psdzProgressListeners)
            {
                if (!psdzProgressListeners.Contains(progressListener))
                {
                    psdzProgressListeners.Add(progressListener);
                }
            }
        }

        public void BeginTask(string task)
        {
            lock (psdzProgressListeners)
            {
                foreach (IPsdzProgressListener psdzProgressListener in psdzProgressListeners)
                {
                    psdzProgressListener.BeginTask(task);
                }
            }
        }

        public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
        {
            lock (psdzProgressListeners)
            {
                if (psdzProgressListeners.Contains(progressListener))
                {
                    psdzProgressListeners.Remove(progressListener);
                }
            }
        }

        public void SetDuration(long milliseconds)
        {
            lock (psdzProgressListeners)
            {
                foreach (IPsdzProgressListener psdzProgressListener in psdzProgressListeners)
                {
                    psdzProgressListener.SetDuration(milliseconds);
                }
            }
        }

        public void SetElapsedTime(long milliseconds)
        {
            lock (psdzProgressListeners)
            {
                foreach (IPsdzProgressListener psdzProgressListener in psdzProgressListeners)
                {
                    psdzProgressListener.SetElapsedTime(milliseconds);
                }
            }
        }

        public void SetFinished()
        {
            lock (psdzProgressListeners)
            {
                foreach (IPsdzProgressListener psdzProgressListener in psdzProgressListeners)
                {
                    psdzProgressListener.SetFinished();
                }
            }
        }

        public void Clear()
        {
            lock (psdzProgressListeners)
            {
                psdzProgressListeners.Clear();
            }
        }
    }
}
