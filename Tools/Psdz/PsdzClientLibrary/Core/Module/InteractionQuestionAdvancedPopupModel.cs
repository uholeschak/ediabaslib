using PsdzClient.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Interaction.Responses;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionQuestionAdvancedPopupModel : InteractionRequestModel<InteractionQuestionAdvancedPopupResponse>
    {
        [DataMember]
        private string heading;

        [DataMember]
        private string suffix;

        [DataMember]
        private List<QuestionPopupAdvancedDialogAnswer> selections;

        [DataMember]
        private QuestionPopupAdvancedDialogAnswer answerNext;

        [DataMember]
        private QuestionPopupAdvancedDialogAnswer answerCancel;

        [DataMember]
        private QuestionPopupAdvancedDialogAnswer answer;

        public string Heading
        {
            get
            {
                return heading;
            }
            set
            {
                heading = value;
                OnPropertyChanged("Heading");
            }
        }

        public string Suffix
        {
            get
            {
                return suffix;
            }
            set
            {
                suffix = value;
                OnPropertyChanged("Suffix");
            }
        }

        public List<QuestionPopupAdvancedDialogAnswer> Selections
        {
            get
            {
                return selections;
            }
            set
            {
                if (selections != value)
                {
                    selections = value;
                    OnPropertyChanged("Selections");
                }
            }
        }

        public QuestionPopupAdvancedDialogAnswer AnswerNext
        {
            get
            {
                return answerNext;
            }
            set
            {
                if (answerNext != value)
                {
                    answerNext = value;
                    OnPropertyChanged("answerNext");
                }
            }
        }

        public QuestionPopupAdvancedDialogAnswer AnswerCancel
        {
            get
            {
                return answerCancel;
            }
            set
            {
                if (answerCancel != value)
                {
                    answerCancel = value;
                    OnPropertyChanged("AnswerCancel");
                }
            }
        }

        public QuestionPopupAdvancedDialogAnswer Answer
        {
            get
            {
                return answer;
            }
            set
            {
                if (answer != value)
                {
                    answer = value;
                    OnPropertyChanged("Answer");
                }
            }
        }

        public override void OnResponseReceived(InteractionQuestionAdvancedPopupResponse response)
        {
            Log.Info("InteractionQuestionAdvancedPopupModel.OnResponseReceived", "InteractionQuestionPopupResponse was sent to the model. Parameter: Selection:'{0}'  Answer:'{0}'.", response?.Selection, response?.Answer);
            NotifyAboutResponseReceived();
            Dispose();
        }
    }
}
