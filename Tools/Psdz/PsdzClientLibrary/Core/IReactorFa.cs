using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;

namespace PsdzClient.Core
{
    public interface IReactorFa : INotifyPropertyChanged, IFARuleEvaluation
    {
        string C_DATE { get; set; }

        DateTime? C_DATETIME { get; set; }

        string TYPE { get; set; }

        string LACK { get; set; }

        string LACK_TEXT { get; set; }

        string POLSTER { get; set; }

        string POLSTER_TEXT { get; set; }

        ObservableCollection<LocalizedSAItem> SaLocalizedItems { get; set; }
    }
}