using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Programming;

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