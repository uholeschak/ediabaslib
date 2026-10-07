using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface ISystemSfaProgrammingAction : IProgrammingAction, INotifyPropertyChanged, IComparable<IProgrammingAction>, ITherapyPlanAction2, ITherapyPlanAction
    {
    }
}