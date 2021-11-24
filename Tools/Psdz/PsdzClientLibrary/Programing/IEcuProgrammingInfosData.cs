using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IEcuProgrammingInfosData : INotifyPropertyChanged
    {
        ObservableCollection<IEcuProgrammingInfoData> List { get; }

        ObservableCollection<IEcuProgrammingInfoData> ECUsWithIndividualData { get; }

        ObservableCollection<IProgrammingActionData> SelectedActionData { get; }

        bool SelectionEstablished { get; }
    }
}
