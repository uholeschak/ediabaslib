using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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