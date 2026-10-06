using System.ComponentModel;
using BMW.Rheingold.CoreFramework.Interaction.Models;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
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