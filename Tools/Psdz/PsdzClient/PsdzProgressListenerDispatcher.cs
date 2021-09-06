using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	class PsdzProgressListenerDispatcher : IPsdzProgressListener
	{
		public void AddPsdzProgressListener(IPsdzProgressListener progressListener)
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				if (!this.psdzProgressListeners.Contains(progressListener))
				{
					this.psdzProgressListeners.Add(progressListener);
				}
			}
		}

		public void BeginTask(string task)
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				foreach (IPsdzProgressListener psdzProgressListener in this.psdzProgressListeners)
				{
					psdzProgressListener.BeginTask(task);
				}
			}
		}

		public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				if (this.psdzProgressListeners.Contains(progressListener))
				{
					this.psdzProgressListeners.Remove(progressListener);
				}
			}
		}

		public void SetDuration(long milliseconds)
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				foreach (IPsdzProgressListener psdzProgressListener in this.psdzProgressListeners)
				{
					psdzProgressListener.SetDuration(milliseconds);
				}
			}
		}

		public void SetElapsedTime(long milliseconds)
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				foreach (IPsdzProgressListener psdzProgressListener in this.psdzProgressListeners)
				{
					psdzProgressListener.SetElapsedTime(milliseconds);
				}
			}
		}

		public void SetFinished()
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				foreach (IPsdzProgressListener psdzProgressListener in this.psdzProgressListeners)
				{
					psdzProgressListener.SetFinished();
				}
			}
		}

		public void Clear()
		{
			IList<IPsdzProgressListener> obj = this.psdzProgressListeners;
			lock (obj)
			{
				this.psdzProgressListeners.Clear();
			}
		}

		private readonly IList<IPsdzProgressListener> psdzProgressListeners = new List<IPsdzProgressListener>();
	}
}
