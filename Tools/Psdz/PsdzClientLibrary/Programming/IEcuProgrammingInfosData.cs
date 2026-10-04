using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

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
