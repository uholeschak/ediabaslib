using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PsdzClient.Core
{
    public interface IFARuleEvaluation
    {
        ObservableCollection<string> SA { get; set; }

        ObservableCollection<string> E_WORT { get; set; }

        ObservableCollection<string> HO_WORT { get; set; }
    }
}