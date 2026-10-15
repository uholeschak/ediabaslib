using System;
using BMW.Rheingold.Programming.ProgrammingEngine.Events;

namespace BMW.Rheingold.Programming.ProgrammingEngine
{
    internal interface IProgrammingEventManager
    {
        event EventHandler<ProgrammingEventArgs> ProgrammingEventRaised;
    }
}
