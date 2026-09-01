using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Interaction.Models;
using BMW.Rheingold.Psdz;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework
{
    public class InteractionService : IInteractionService
    {
        private readonly IInteractionController controller;

        public InteractionService(IInteractionController controller)
        {
            this.controller = controller;
        }

        public virtual void Deregister(InteractionModel model)
        {
            controller.DeregisterInteraction(model);
        }

        public void DeregisterBackground(IInteractionProgressModel model)
        {
            controller.DeregisterInteractionBackground(model);
        }

        public virtual void Register(InteractionModel model)
        {
            controller.RegisterInteraction(model);
        }

        public Task<TResponse> RegisterAsync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse
        {
            return controller.RegisterInteractionAsync(model);
        }

        public TResponse RegisterSync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse
        {
            return controller.RegisterInteractionSync(model);
        }

        public void RegisterBackground(IInteractionProgressModel model)
        {
            controller.RegisterInteractionBackground(model);
        }

        public InteractionMessageModel RegisterMessage(IList<LocalizedText> titleList, IList<LocalizedText> msgList, string details = "", int dialogSize = 0)
        {
            return RegisterMessage(titleList[0].TextItem, msgList[0].TextItem, details, dialogSize);
        }

        public InteractionMessageModel RegisterMessage(string title, string message, string details = "", int dialogSize = 0)
        {
            InteractionMessageModel interactionMessageModel = new InteractionMessageModel(title, message, details);
            if (dialogSize != 0)
            {
                interactionMessageModel.DialogSize = dialogSize;
            }
            controller.RegisterInteractionSync(interactionMessageModel);
            return interactionMessageModel;
        }

        public Task<InteractionButtonResponse> RegisterMessageAsync(string title, string message, string details = "", int dialogSize = 0)
        {
            InteractionMessageModel interactionMessageModel = new InteractionMessageModel(title, message, details);
            if (dialogSize != 0)
            {
                interactionMessageModel.DialogSize = dialogSize;
            }
            return controller.RegisterInteractionAsync(interactionMessageModel);
        }

        public InteractionQuestionModel RegisterQuestion(IList<LocalizedText> titleList, IList<LocalizedText> msgList)
        {
            return RegisterQuestion(titleList[0].TextItem, msgList[0].TextItem);
        }

        public InteractionQuestionModel RegisterQuestion(string title, string message)
        {
            InteractionQuestionModel interactionQuestionModel = new InteractionQuestionModel(title, message);
            controller.RegisterInteractionSync(interactionQuestionModel);
            return interactionQuestionModel;
        }
    }
}
