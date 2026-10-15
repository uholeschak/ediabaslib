using System;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.ProgrammingEngine.Events
{
    internal class ProgrammingCurrentEcuChangedEventArgs : ProgrammingEventArgs
    {
        public IEcuProgrammingInfo EcuProgrammingInfo { get; private set; }

        public ProgrammingCurrentEcuChangedEventArgs(DateTime timestamp, IEcuProgrammingInfo ecuProgrammingInfo) : base(timestamp)
        {
            EcuProgrammingInfo = ecuProgrammingInfo;
        }
    }
}