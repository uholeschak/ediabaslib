using BMW.Rheingold.CoreFramework.Interaction.Models;
using PsdzClient.Core;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Interaction;

namespace BMW.Rheingold.CoreFramework
{
    public interface IInteractionController : IInteractionButtonNotificationService
    {
        IInteractionDataContext InteractionDataContext { get; }

        void ChangeMode(InteractionProgressModel model, TaskMode mode);

        void DeregisterInteraction(InteractionModel model);

        void DeregisterInteractionBackground(IInteractionProgressModel model);

        void RegisterInteraction(InteractionModel model);

        Task<TResponse> RegisterInteractionAsync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse;

        TResponse RegisterInteractionSync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse;

        void RegisterInteractionBackground(IInteractionProgressModel model);
    }
}
