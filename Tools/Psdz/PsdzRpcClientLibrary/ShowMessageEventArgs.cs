using System;
using System.Threading;

namespace PsdzRpcClient
{
    public class ShowMessageEventArgs : EventArgs, IDisposable
    {
        public string Message { get; }
        public bool OkBtn { get; }
        public bool Result { get; set; }

        private ManualResetEventSlim _completionEvent;

        public ShowMessageEventArgs(string message, bool okBtn, bool waitForResult = false)
        {
            Message = message;
            OkBtn = okBtn;
            Result = true;
            _completionEvent = waitForResult ? new ManualResetEventSlim(false) : null;
        }

        /// <summary>
        /// Setzt das Ergebnis und signalisiert dem wartenden Thread die Fertigstellung.
        /// </summary>
        public void SetResult(bool result)
        {
            Result = result;
            _completionEvent?.Set();
        }

        /// <summary>
        /// Blockiert bis <see cref="SetResult"/> aufgerufen wird, oder Abbruch/Timeout.
        /// Gibt <c>false</c> zurück wenn abgebrochen oder Timeout.
        /// </summary>
        internal bool WaitForResult(CancellationToken ct = default, int timeoutMs = Timeout.Infinite)
        {
            if (_completionEvent != null)
            {
                try
                {
                    bool signaled = _completionEvent.Wait(timeoutMs, ct);
                    if (!signaled)
                    {
                        Result = false; // Timeout
                    }
                }
                catch (OperationCanceledException)
                {
                    Result = false;
                }
                finally
                {
                    _completionEvent.Dispose();
                    _completionEvent = null;
                }
            }
            return Result;
        }

        public void Dispose()
        {
            if (_completionEvent != null)
            {
                _completionEvent.Dispose();
                _completionEvent = null;
            }
        }
    }
}