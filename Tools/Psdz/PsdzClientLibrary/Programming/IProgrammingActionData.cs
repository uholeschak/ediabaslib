using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IProgrammingActionData : INotifyPropertyChanged
    {
        ProgrammingActionState StateProgramming { get; }

        IEcu ParentEcu { get; }

        ProgrammingActionType Type { get; }

        string Channel { get; }

        string Note { get; }

        bool IsEditable { get; }

        bool IsSelected { get; }

        bool IsFlashAction { get; }

        int Order { get; }
    }
}
