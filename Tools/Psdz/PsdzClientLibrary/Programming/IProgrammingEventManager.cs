using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal interface IProgrammingEventManager
    {
        event EventHandler<ProgrammingEventArgs> ProgrammingEventRaised;
    }
}
