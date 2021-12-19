using System;
using System.Threading;
using System.Threading.Tasks;
using PsdzClient.Programing;

namespace WebPsdzClient.App_Data
{
    public class SessionContainer : IDisposable
    {
        public delegate void UpdateDisplayDelegate();
        public event UpdateDisplayDelegate UpdateDisplayEvent;
        public ProgrammingJobs ProgrammingJobs { get; private set; }
        public CancellationTokenSource Cts { get; set; }
        public bool TaskActive { get; set; }
        public string StatusText
        {
            get
            {
                lock (_lockObject)
                {
                    return _statusText;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _statusText = value;
                }
            }
        }

        private bool _disposed;
        private object _lockObject = new object();
        private string _statusText;

        public SessionContainer(string dealerId)
        {
            ProgrammingJobs = new ProgrammingJobs(dealerId);
            ProgrammingJobs.UpdateStatusEvent += UpdateStatus;
            StatusText = string.Empty;
        }

        public void UpdateStatus(string message = null)
        {
            StatusText = message ?? string.Empty;
            UpdateDisplayEvent?.Invoke();
        }

        public async Task<bool> StartProgrammingServiceTask(string istaFolder)
        {
            return await Task.Run(() => ProgrammingJobs.StartProgrammingService(Cts, istaFolder)).ConfigureAwait(false);
        }

        public async Task<bool> StopProgrammingServiceTask()
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            return await Task.Run(() => ProgrammingJobs.StopProgrammingService(Cts)).ConfigureAwait(false);
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

        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (ProgrammingJobs != null)
                {
                    ProgrammingJobs.Dispose();
                    ProgrammingJobs = null;
                }

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
