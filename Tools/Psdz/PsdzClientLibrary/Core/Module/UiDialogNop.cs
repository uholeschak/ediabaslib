using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.FASTA.Model
{
    internal class UiDialogNop : IUiDialog, IProtocolTransaction
    {
        public string MethodName { get; set; }

        public decimal? ParameterValue { get; set; }

        public bool HasOrder { get; }

        public string Type { get; set; }

        public bool Display { get; set; }

        public object CyclicalJournalizingDifferentiator { get; set; }

        public ActionResult Result { get; set; }

        public IMessageText MessageText { get; set; }

        public IMessageText CreateAndAddMessageText(IList<LocalizedText> messageTextList, string messageID = null, decimal? parameterValue = null)
        {
            return new MessageTextNop();
        }

        public void AddAnswer(IList<LocalizedText> answerTextList, string key)
        {
            Log.Debug("UiDialogNop.AddAnswer()", "No operation executed.");
        }

        public void AddInfoList(string infoName, string infoValue)
        {
            Log.Debug("UiDialogNop.AddInfoList()", "No operation executed.");
        }

        public void Initialize(string methodName, string type)
        {
            Log.Debug("UiDialogNop.Initialize()", "No operation executed.");
        }

        public void SetTitle(IList<LocalizedText> messageTypeTitleList)
        {
            Log.Debug("UiDialogNop.SetTitle()", "No operation executed.");
        }
    }
}
