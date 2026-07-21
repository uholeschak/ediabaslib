using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework
{
    public interface IStateApplication : INotifyPropertyChanged
    {
        IStateImib Imib { get; }
    }
}
