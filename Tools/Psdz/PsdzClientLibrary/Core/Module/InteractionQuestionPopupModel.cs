using PsdzClient.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Interaction.Responses;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionQuestionPopupModel : InteractionRequestModel<InteractionQuestionPopupResponse>
    {
        [DataMember]
        private string question;

        [DataMember]
        private QuestionPopupDialogAnswer answerLeft;

        [DataMember]
        private QuestionPopupDialogAnswer answerMiddle;

        [DataMember]
        private QuestionPopupDialogAnswer answerRight;

        public string Question
        {
            get
            {
                return question;
            }
            set
            {
                question = value;
                OnPropertyChanged("Question");
            }
        }

        public QuestionPopupDialogAnswer AnswerLeft
        {
            get
            {
                return answerLeft;
            }
            set
            {
                if (answerLeft == null || answerLeft.Equals(value))
                {
                    answerLeft = value;
                    OnPropertyChanged("AnswerLeft");
                }
            }
        }

        public QuestionPopupDialogAnswer AnswerMiddle
        {
            get
            {
                return answerMiddle;
            }
            set
            {
                if (answerMiddle == null || answerMiddle.Equals(value))
                {
                    answerMiddle = value;
                    OnPropertyChanged("AnswerMiddle");
                }
            }
        }

        public QuestionPopupDialogAnswer AnswerRight
        {
            get
            {
                return answerRight;
            }
            set
            {
                if (answerRight == null || answerRight.Equals(value))
                {
                    answerRight = value;
                    OnPropertyChanged("AnswerRight");
                }
            }
        }

        public ISet<string> GetButtonIds()
        {
            ISet<string> set = new HashSet<string>();
            if (AnswerLeft != null && !string.IsNullOrEmpty(AnswerLeft.ButtonId))
            {
                set.Add(AnswerLeft.ButtonId);
            }
            if (AnswerMiddle != null && !string.IsNullOrEmpty(AnswerMiddle.ButtonId))
            {
                set.Add(AnswerMiddle.ButtonId);
            }
            if (AnswerRight != null && !string.IsNullOrEmpty(AnswerRight.ButtonId))
            {
                set.Add(AnswerRight.ButtonId);
            }
            return set;
        }

        public override void OnResponseReceived(InteractionQuestionPopupResponse response)
        {
            Log.Info("InteractionQuestionPopupModel.OnResponseRecived()", "InteractionQuestionPopupResponse was set to the model. Parameter: Selection:'{0}'  Answer:'{0}'.", response?.Selection, response?.Answer);
            NotifyAboutResponseReceived();
            Dispose();
        }

        public override void LogMessage()
        {
            string text = AnswerLeft?.Text;
            string text2 = AnswerMiddle?.Text;
            string text3 = AnswerRight?.Text;
            string text4 = string.Empty;
            if (text != null)
            {
                text4 = text4 + "[" + text + "]";
            }
            if (text2 != null)
            {
                text4 = text4 + "[" + text2 + "]";
            }
            if (text3 != null)
            {
                text4 = text4 + "[" + text3 + "]";
            }
            Log.Warning("InteractionQuestionPopupModel.LogMessage", "\n\t[TestAutomationInteraction]\n\tTitle: [" + base.Title + "]\n\tAnswers: " + text4 + "\n\tQuestion: [" + Question + "]");
        }

        public override void LogResponseMessage(object response)
        {
            string text = ((InteractionQuestionPopupResponse)response).Answer.Text;
            Log.Info("InteractionQuestionPopupModel.LogResponseMessage", "\n\t[TestAutomationInteraction]\n\tTitle: [" + base.Title + "]\n\tAnswer: [" + text + "]\n\tQuestion: [" + Question + "]");
        }
    }
}
