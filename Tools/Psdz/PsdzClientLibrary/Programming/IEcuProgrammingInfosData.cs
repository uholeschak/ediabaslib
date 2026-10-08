using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IEcuProgrammingInfosData : INotifyPropertyChanged
    {
        ObservableCollection<IEcuProgrammingInfoData> List { get; }

        ObservableCollection<IEcuProgrammingInfoData> ECUsWithIndividualData { get; }

        ObservableCollection<IProgrammingActionData> SelectedActionData { get; }

        bool SelectionEstablished { get; }
    }
}
