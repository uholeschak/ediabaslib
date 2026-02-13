using System;

namespace PsdzRpcClient;

public class ShowMessageEventArgs : EventArgs
{
    public string Message { get; }
    public bool OkBtn { get; }
    public bool Wait { get; }
    public bool Result { get; set; }

    public ShowMessageEventArgs(string message, bool okBtn, bool wait)
    {
        Message = message;
        OkBtn = okBtn;
        Wait = wait;
        Result = true;
    }
}