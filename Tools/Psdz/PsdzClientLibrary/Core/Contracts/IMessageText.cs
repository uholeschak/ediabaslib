using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IMessageText
    {
        void AddText(IList<LocalizedText> messageTextList);

        void AddButton(string id, bool isPressed, IList<LocalizedText> buttonTextList);

        ISelectable CreateAndAddSelectable(string key);
    }
}
