using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionQuestionModel : InteractionRequestModel<InteractionButtonResponse>, IInteractionQuestionModel, IInteractionModel, INotifyPropertyChanged
    {
        private string questionText;

        private string questionTextHtml;

        [DataMember]
        public string QuestionText
        {
            get
            {
                return questionText;
            }
            set
            {
                questionText = value;
                OnPropertyChanged("QuestionText");
            }
        }

        [DataMember]
        public string QuestionTextHtml
        {
            get
            {
                return questionTextHtml;
            }
            set
            {
                questionTextHtml = value;
                OnPropertyChanged("QuestionTextHtml");
            }
        }

        [DataMember]
        public string CmdYesLabel { get; set; }

        [DataMember]
        public string CmdNoLabel { get; set; }

        public InteractionQuestionModel()
            : this("", "")
        {
        }

        public InteractionQuestionModel(string questionText)
            : this("", questionText)
        {
        }

        public InteractionQuestionModel(string title, string questionText)
        {
            CmdYesLabel = new FormatedData("#Yes").Localize();
            CmdNoLabel = new FormatedData("#No").Localize();
            base.Title = title;
            QuestionText = questionText;
        }

        public override void OnResponseReceived(InteractionButtonResponse response)
        {
            Log.Info("InteractionQuestionModel.OnResponseRecived()", "InteractionButtonResponse was set to the model. Parameter: Button:'{0}'.", response?.Action);
            NotifyAboutResponseReceived();
            Dispose();
        }
    }
}
