using PsdzClientLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Dummy")]
    public interface IInteractionService
    {
        void Deregister(InteractionModel model);

        //void DeregisterBackground(IInteractionProgressModel model);

        void Register(InteractionModel model);

        //Task<TResponse> RegisterAsync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse;

        //void RegisterBackground(IInteractionProgressModel model);

        //InteractionMessageModel RegisterMessage(string title, string message, string details = "", int dialogSize = 0);

        //InteractionMessageModel RegisterMessage(IList<LocalizedText> titleList, IList<LocalizedText> msgList, string details = "", int dialogSize = 0);

        //Task<InteractionButtonResponse> RegisterMessageAsync(string title, string message, string details = "", int dialogSize = 0);

        //InteractionQuestionModel RegisterQuestion(string title, string message);

        //InteractionQuestionModel RegisterQuestion(IList<LocalizedText> titleList, IList<LocalizedText> msgList);
    }
}
