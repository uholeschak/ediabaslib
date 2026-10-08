using System;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.Programming.ProgrammingEngine.Events
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