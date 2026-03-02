using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Rheingold.Psdz
{
    public interface IInteractionMessageModel : IInteractionModel, INotifyPropertyChanged
    {
        string MessageText { get; }

        string DetailText { get; }

        string ButtonText { get; }

        bool IsDetailButtonVisible { get; }

        bool IsbtnRightVisible { get; }
    }
}