using System;

namespace BMW.Rheingold.Programming.ProgrammingEngine.Events
{
    internal abstract class ProgrammingEventArgs : EventArgs
    {
        public DateTime Timestamp { get; private set; }

        internal ProgrammingEventArgs(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}