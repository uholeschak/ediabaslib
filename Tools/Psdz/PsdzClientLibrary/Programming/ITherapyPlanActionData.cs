using PsdzClient.Programming;
using System.Collections.Generic;
using System.ComponentModel;

namespace PsdzClient.Programming
{
    public interface ITherapyPlanActionData : INotifyPropertyChanged
    {
        void SetState(typeDiagObjectState value);

        void SetStateProgramming(ProgrammingActionState value);

        void SetSgbmIds(IList<ISgbmIdChange> value);
    }
}