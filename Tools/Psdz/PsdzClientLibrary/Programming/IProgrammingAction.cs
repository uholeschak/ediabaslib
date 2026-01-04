using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IProgrammingAction : INotifyPropertyChanged, IComparable<IProgrammingAction>, ITherapyPlanAction2, ITherapyPlanAction
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

        ICollection<IEscalationStep> EscalationSteps { get; }

        IProgrammingActionData DataContext { get; }

        IList<int> AffectedEcuDiagAddr { get; }

        ProgrammingActionType Type { get; }

        string GetShortType();

        bool Select(bool value);

        bool RequiresEscalation();
    }
}
