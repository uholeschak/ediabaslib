using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.FASTA.Model
{
    public class MessageTextNop : IMessageText
    {
        public void AddText(IList<LocalizedText> messageTextList)
        {
            Log.Debug("MessageTextNop.AddText()", "Not operation executed.");
        }

        public ISelectable CreateAndAddSelectable(string key)
        {
            Log.Debug("MessageTextNop.CreateAndAddSelectable()", "Not operation executed.");
            return new SelectableNop();
        }

        public void AddButton(string id, bool isPressed, IList<LocalizedText> buttonTextList)
        {
            Log.Debug("MessageTextNop.AddButton()", "Not operation executed.");
        }
    }
}
