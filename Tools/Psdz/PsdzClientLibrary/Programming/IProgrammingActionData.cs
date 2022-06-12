using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Programming
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
