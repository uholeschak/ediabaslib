using System.ComponentModel;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    public interface IInteractionRequestModel<out TResponse> : IInteractionModel, INotifyPropertyChanged where TResponse : InteractionResponse
    {
        TResponse Response { get; }

        bool ShowPending { get; }

        void OnResponseRecived(InteractionResponse response);

        TResponse WaitOnResponse(bool resetFirst = false);
    }
}