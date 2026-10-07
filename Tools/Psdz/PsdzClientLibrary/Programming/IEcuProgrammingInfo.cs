using System;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuProgrammingInfo : INotifyPropertyChanged
    {
        IEcu Ecu { get; }

        IEnumerable<IProgrammingAction> ProgrammingActions { get; }

        double ProgressValue { get; }

        ProgrammingActionState? State { get; }

        bool IsCodingDisabled { get; set; }

        bool IsProgrammingDisabled { get; set; }

        bool IsProgrammingSelectionDisabled { get; set; }

        bool IsCodingSelectionDisabled { get; set; }

        bool IsCodingScheduled { get; set; }

        bool IsProgrammingScheduled { get; set; }

        IStandardSvk SvkCurrent { get; }

        IStandardSvk SvkTarget { get; }

        bool IsExchangeScheduled { get; set; }

        bool IsExchangeDone { get; set; }

        string EcuIdentifier { get; }

        int FlashOrder { get; }

        IProgrammingAction GetProgrammingAction(ProgrammingActionType type);

        IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter);
    }
}
