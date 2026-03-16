using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PsdzRpcClient;

public sealed class SingleThreadSynchronizationContext : SynchronizationContext
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object State)> _queue = new();
    private readonly int _mainThreadId = Environment.CurrentManagedThreadId;

    public override void Post(SendOrPostCallback d, object state)
    {
        _queue.Add((d, state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
        if (Environment.CurrentManagedThreadId == _mainThreadId)
        {
            d(state);
        }
        else
        {
            using ManualResetEventSlim mre = new(false);
            Post(_ =>
            {
                try
                {
                    d(state);
                }
                finally
                {
                    mre.Set();
                }
            }, null);
            mre.Wait();
        }
    }

    /// <summary>
    /// Verarbeitet die Warteschlange auf dem aufrufenden Thread.
    /// Blockiert, bis <see cref="Complete"/> aufgerufen wird oder <paramref name="ct"/> abgebrochen wird.
    /// </summary>
    public void RunOnCurrentThread(CancellationToken ct)
    {
        SetSynchronizationContext(this);
        try
        {
            foreach (var (callback, state) in _queue.GetConsumingEnumerable(ct))
            {
                callback(state);
            }
        }
        catch (OperationCanceledException)
        {
            // Erwartet bei Abbruch
        }
    }

    public void Complete() => _queue.CompleteAdding();
}
