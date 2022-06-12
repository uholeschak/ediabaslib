using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public class ProgrammingCurrentEcuChangedEventArgs : ProgrammingEventArgs
    {
        public ProgrammingCurrentEcuChangedEventArgs(DateTime timestamp, IEcuProgrammingInfo ecuProgrammingInfo) : base(timestamp)
        {
            this.EcuProgrammingInfo = ecuProgrammingInfo;
        }

        public IEcuProgrammingInfo EcuProgrammingInfo { get; private set; }
    }
}
