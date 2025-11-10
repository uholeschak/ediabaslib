using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
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