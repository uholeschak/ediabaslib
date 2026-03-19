using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PsdzRpcClient;

public sealed class SingleThreadSynchronizationContext : SynchronizationContext
{
    private readonly ConcurrentQueue<(SendOrPostCallback Callback, object State)> _queue = new();

    public override void Post(SendOrPostCallback d, object state)
    {
        _queue.Enqueue((d, state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
        d(state);
    }

    /// <summary>
    /// Führt eine Aktion auf dem Main-Thread aus (Fire-and-Forget).
    /// </summary>
    public void BeginInvoke(Action action)
    {
        Post(_ => action(), null);
    }

    /// <summary>
    /// Führt eine async-Aktion auf dem Main-Thread aus (Fire-and-Forget).
    /// </summary>
    public void BeginInvoke(Func<Task> asyncAction)
    {
        Post(async _ => await asyncAction(), null);
    }

    /// <summary>
    /// Führt eine Funktion auf dem Main-Thread aus und gibt das Ergebnis zurück.
    /// Nicht-blockierend: Der aufrufende Thread kann await verwenden.
    /// </summary>
    public Task<T> InvokeAsync<T>(Func<T> func)
    {
        TaskCompletionSource<T> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Post(_ =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);
        return tcs.Task;
    }

    /// <summary>
    /// Verarbeitet alle ausstehenden Callbacks auf dem aufrufenden Thread.
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
