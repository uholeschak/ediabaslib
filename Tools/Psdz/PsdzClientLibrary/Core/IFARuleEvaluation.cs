using System.Collections.ObjectModel;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IFARuleEvaluation
    {
        ObservableCollection<string> SA { get; set; }

        ObservableCollection<string> E_WORT { get; set; }

        ObservableCollection<string> HO_WORT { get; set; }
    }
}