using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    public interface IInteractionProgressModel : IInteractionModel, INotifyPropertyChanged
    {
        double ProcessProgress { get; }

        string Description { get; }

        bool IsIndeterminate { get; }
    }
}
