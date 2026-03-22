using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient
{
    public sealed class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object State)> _queue = new ();

        public override void Post(SendOrPostCallback d, object state)
        {
            _queue.Enqueue((d, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            // Auf dem aufrufenden Thread direkt ausführen
            d(state);
        }

        /// <summary>
        /// Führt eine Aktion auf dem Main-Thread aus (analog zu Control.BeginInvoke).
        /// </summary>
        public void BeginInvoke(Action action)
        {
            Post(_ => action(), null);
        }

        /// <summary>
        /// Führt eine async-Aktion auf dem Main-Thread aus.
        /// await-Continuations kehren ebenfalls auf den Main-Thread zurück.
        /// </summary>
        public void BeginInvoke(Func<Task> asyncAction)
        {
            Post(async _ => await asyncAction(), null);
        }

        /// <summary>
        /// Verarbeitet alle ausstehenden Callbacks auf dem aufrufenden Thread.
        /// Nicht-blockierend: kehrt sofort zurück wenn die Queue leer ist.
        /// </summary>
        public void ProcessPendingCallbacks()
        {
            SynchronizationContext previous = Current;
            SetSynchronizationContext(this);
            try
            {
                while (_queue.TryDequeue(out var item))
                {
                    item.Callback(item.State);
                }
            }
            finally
            {
                SetSynchronizationContext(previous);
            }
        }
    }
}
