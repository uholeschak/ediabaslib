using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IUiDialog : IProtocolTransaction
    {
        string MethodName { get; set; }

        string Type { get; }

        bool Display { get; set; }

        bool HasOrder { get; }

        decimal? ParameterValue { get; set; }

        IMessageText MessageText { get; set; }

        void Initialize(string methodName, string type);

        IMessageText CreateAndAddMessageText(IList<LocalizedText> messageTextList, string messageID = null, decimal? parameterValue = null);

        void AddAnswer(IList<LocalizedText> answerTextList, string key);

        void SetTitle(IList<LocalizedText> messageTypeTitleList);
    }
}
