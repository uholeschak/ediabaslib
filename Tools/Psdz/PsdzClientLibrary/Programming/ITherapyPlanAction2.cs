using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsdzClient.Programming
{
    public interface ITherapyPlanAction2 : ITherapyPlanAction, INotifyPropertyChanged
    {
        ProgrammingActionState StateProgramming { get; }

        IList<ISgbmIdChange> SgbmIds { get; }

        ITherapyPlanActionData ActionData { get; set; }
    }
}