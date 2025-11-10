using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
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