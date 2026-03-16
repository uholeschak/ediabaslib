using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PsdzRpcClient;

public class SingleThreadSynchronizationContext : SynchronizationContext
{
    private readonly BlockingCollection<(SendOrPostCallback Callback, object State)> _queue = new();

    public override void Post(SendOrPostCallback d, object state)
    {
        _queue.Add((d, state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
        // Für Einfachheit als Post behandeln; bei Bedarf synchron warten
        Post(d, state);
    }

    /// <summary>
    /// Verarbeitet die Warteschlange auf dem aufrufenden Thread.
    /// Blockiert, bis <paramref name="ct"/> abgebrochen wird.
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
