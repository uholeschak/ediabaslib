using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IProgrammingAction : INotifyPropertyChanged, IComparable<IProgrammingAction>
    {
        DateTime StartExecution { get; }

        DateTime EndExecution { get; }

        string Channel { get; }

        bool IsEditable { get; }

        bool IsFlashAction { get; }

        bool IsSelected { get; }

        string Note { get; }

        int Order { get; }

        string PartNumber { get; }

        //ICollection<IEscalationStep> EscalationSteps { get; }

        IProgrammingActionData DataContext { get; }

        string GetShortType();

        bool Select(bool value);

        ProgrammingActionType Type { get; }

        IList<ISgbmIdChange> SgbmIds { get; }

        bool RequiresEscalation();
    }
}
