using System;
using System.Threading.Tasks;

namespace PsdzRpcClient;

public class ShowMessageEventArgs : EventArgs
{
    public string Message { get; }
    public bool OkBtn { get; }
    public bool Result { get; set; }

    /// <summary>
    /// Wird bei wait=true verwendet. Der Event-Handler ruft <see cref="SetResult"/> auf,
    /// wenn die Benutzereingabe vorliegt.
    /// </summary>
    public TaskCompletionSource<bool> Completion { get; }

    public ShowMessageEventArgs(string message, bool okBtn, TaskCompletionSource<bool> completion = null)
    {
        Message = message;
        OkBtn = okBtn;
        Result = true;
        Completion = completion;
    }

    /// <summary>
    /// Setzt das Ergebnis und signalisiert dem wartenden Task die Fertigstellung.
    /// </summary>
    public void SetResult(bool result)
    {
        Result = result;
        Completion?.TrySetResult(result);
    }
}