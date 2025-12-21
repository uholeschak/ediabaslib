using PsdzClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public interface IInteractionService
    {
        void Deregister(InteractionModel model);

        [PreserveSource(Hint = "IInteractionProgressModel", Placeholder = true)]
        void DeregisterBackground();

        void Register(InteractionModel model);

        Task<TResponse> RegisterAsync<TResponse>(InteractionRequestModel<TResponse> model) where TResponse : InteractionResponse;

        [PreserveSource(Hint = "IInteractionProgressModel", Placeholder = true)]
        void RegisterBackground();

        [PreserveSource(Hint = "InteractionMessageModel", Placeholder = true)]
        PlaceholderType RegisterMessage(string title, string message, string details = "", int dialogSize = 0);

        [PreserveSource(Hint = "InteractionMessageModel", Placeholder = true)]
        PlaceholderType RegisterMessage(IList<LocalizedText> titleList, IList<LocalizedText> msgList, string details = "", int dialogSize = 0);

        Task<InteractionButtonResponse> RegisterMessageAsync(string title, string message, string details = "", int dialogSize = 0);

        [PreserveSource(Hint = "InteractionMessageModel", Placeholder = true)]
        PlaceholderType RegisterQuestion(string title, string message);

        [PreserveSource(Hint = "InteractionQuestionModel", Placeholder = true)]
        PlaceholderType RegisterQuestion(IList<LocalizedText> titleList, IList<LocalizedText> msgList);
    }
}
