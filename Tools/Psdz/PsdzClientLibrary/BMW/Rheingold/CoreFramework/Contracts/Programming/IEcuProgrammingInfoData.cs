using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuProgrammingInfoData : IEcuProgrammingInfo, INotifyPropertyChanged
    {
        ObservableCollection<IProgrammingActionData> ProgrammingActionData { get; }

        string Category { get; }

        string EcuTitle { get; }

        string EcuDescription { get; }

        bool IsExchangeScheduledDisabled { get; }

        bool IsExchangeDoneDisabled { get; }
    }
}