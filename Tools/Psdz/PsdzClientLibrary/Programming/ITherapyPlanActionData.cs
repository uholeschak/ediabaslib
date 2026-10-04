using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ITherapyPlanActionData : ITherapyPlanAction2, ITherapyPlanAction, INotifyPropertyChanged
    {
        void SetState(typeDiagObjectState value);

        void SetStateProgramming(ProgrammingActionState value);

        void SetSgbmIds(IList<ISgbmIdChange> value);
    }
}