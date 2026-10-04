using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.Programming.ProgrammingEngine.Events;

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