using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework
{
    public interface IStateImib : INotifyPropertyChanged
    {
        bool IsConnected { get; }

        bool IsShown { get; }
    }
}
