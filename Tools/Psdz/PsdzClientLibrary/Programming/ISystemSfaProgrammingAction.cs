using PsdzClient.Programming;
using System;
using System.ComponentModel;

namespace PsdzClient.Programming
{
    public interface ISystemSfaProgrammingAction : IProgrammingAction, INotifyPropertyChanged, IComparable<IProgrammingAction>
    {
    }
}