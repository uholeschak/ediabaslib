using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IProgrammingStateData : IProgrammingState, INotifyPropertyChanged
    {
        string LocalizationId { get; set; }
    }
}
