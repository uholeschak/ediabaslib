using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public abstract class ProgrammingEventArgs : EventArgs
    {
        internal ProgrammingEventArgs(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }

        public DateTime Timestamp { get; private set; }
    }
}
