using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    public interface IInteractionQuestionModel : IInteractionModel, INotifyPropertyChanged
    {
        string QuestionText { get; }

        string QuestionTextHtml { get; }

        string CmdYesLabel { get; set; }

        string CmdNoLabel { get; set; }
    }
}
