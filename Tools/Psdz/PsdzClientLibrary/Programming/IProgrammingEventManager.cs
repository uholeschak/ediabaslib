using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Programming.ProgrammingEngine.Events;

namespace BMW.Rheingold.Programming.ProgrammingEngine
{
    internal interface IProgrammingEventManager
    {
        event EventHandler<ProgrammingEventArgs> ProgrammingEventRaised;
    }
}
