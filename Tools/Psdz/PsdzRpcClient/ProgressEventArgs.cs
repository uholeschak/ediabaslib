using System;

namespace PsdzRpcClient;

public class ProgressEventArgs : EventArgs
{
    public int Percent { get; }
    public string Message { get; }
    public ProgressEventArgs(int percent, string message)
    {
        Percent = percent;
        Message = message;
    }
}
