using System.ComponentModel;

namespace PsdzClient.Core
{
    public interface IInteractionRequestModel<out TResponse> : IInteractionModel, INotifyPropertyChanged where TResponse : InteractionResponse
    {
        TResponse Response { get; }

        bool ShowPending { get; }

        void OnResponseRecived(InteractionResponse response);

        TResponse WaitOnResponse(bool resetFirst = false);
    }
}