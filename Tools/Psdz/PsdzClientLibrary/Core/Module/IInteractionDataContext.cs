using BMW.Rheingold.CoreFramework.Interaction.Models;
using PsdzClient.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Interaction
{
    public interface IInteractionDataContext : INotifyPropertyChanged
    {
        IInteractionProgressModel BackgroundInteractionModel { get; }

        ObservableCollection<IInteractionModel> ModelCollection { get; }

        void AddBackgroundInteraction(IInteractionProgressModel model);

        bool IsBackgroundInteractionAvailable(IInteractionProgressModel model);

        void RemoveBackgroundInteraction(IInteractionProgressModel model);
    }
}
