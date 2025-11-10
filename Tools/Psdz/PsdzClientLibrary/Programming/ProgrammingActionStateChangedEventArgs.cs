using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Programming
{
    internal class ProgrammingActionStateChangedEventArgs : ProgrammingEventArgs
    {
        public IEcu Ecu { get; private set; }
        public ProgrammingActionState ProgrammingActionState { get; private set; }
        public ProgrammingActionType ProgrammingActionType { get; private set; }

        public ProgrammingActionStateChangedEventArgs(DateTime timestamp, IEcu ecu, ProgrammingActionType programmingActionType, ProgrammingActionState programmingActionState) : base(timestamp)
        {
            Ecu = ecu;
            ProgrammingActionType = programmingActionType;
            ProgrammingActionState = programmingActionState;
        }
    }
}