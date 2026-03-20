using System;

namespace PsdzRpcClient;

public class ShowMessageEventArgs : EventArgs
{
    public string Message { get; }
    public bool OkBtn { get; }
    public bool Result { get; set; }

    public ShowMessageEventArgs(string message, bool okBtn)
    {
        Message = message;
        OkBtn = okBtn;
        Result = true;
    }
}