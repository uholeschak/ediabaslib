using System;

namespace PsdzRpcClient
{
    public class ProgressEventArgs : EventArgs
    {
        public int Percent { get; }
        public bool Marquee { get; }
        public string Message { get; }
        public ProgressEventArgs(int percent, bool marquee, string message)
        {
            Percent = percent;
            Marquee = marquee;
            Message = message;
        }
    }
}
