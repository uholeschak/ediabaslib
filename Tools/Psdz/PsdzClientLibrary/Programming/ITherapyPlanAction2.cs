using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ITherapyPlanAction2 : ITherapyPlanAction, INotifyPropertyChanged
    {
        ProgrammingActionState StateProgramming { get; }

        IList<ISgbmIdChange> SgbmIds { get; }

        ITherapyPlanActionData ActionData { get; set; }
    }
}