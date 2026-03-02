using PsdzClient.Core;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    public class InteractionMessageModel : InteractionRequestModel<InteractionButtonResponse>, IInteractionMessageModel, IInteractionModel, INotifyPropertyChanged
    {
        private string messageText;

        private string detailsText;

        private string buttonText;

        private bool isDetailButtonVisible;

        private bool isbtnRightVisible;

        [DataMember]
        public string MessageText
        {
            get
            {
                return messageText;
            }
            set
            {
                messageText = value;
                OnPropertyChanged("MessageText");
            }
        }

        [DataMember]
        public string DetailText
        {
            get
            {
                return detailsText;
            }
            set
            {
                detailsText = value;
                OnPropertyChanged("DetailText");
            }
        }

        [DataMember]
        public string ButtonText
        {
            get
            {
                return buttonText;
            }
            set
            {
                buttonText = value;
                OnPropertyChanged("ButtonText");
            }
        }

        [DataMember]
        public bool IsDetailButtonVisible
        {
            get
            {
                return isDetailButtonVisible;
            }
            set
            {
                isDetailButtonVisible = value;
                OnPropertyChanged("IsDetailButtonVisible");
            }
        }

        [DataMember]
        public bool IsbtnRightVisible
        {
            get
            {
                return isbtnRightVisible;
            }
            set
            {
                isbtnRightVisible = value;
                OnPropertyChanged("IsbtnRightVisible");
            }
        }

        public InteractionMessageModel()
            : this("", "", "")
        {
        }

        public InteractionMessageModel(string messageText)
            : this("", messageText, "")
        {
        }

        public InteractionMessageModel(string title, string messageText)
            : this(title, messageText, "")
        {
        }

        public InteractionMessageModel(string title, string messageText, string details)
        {
            base.Title = title;
            MessageText = messageText;
            DetailText = details;
            IsDetailButtonVisible = true;
            IsbtnRightVisible = true;
            ButtonText = FormatedData.Localize("#Button.OK");
        }

        public override void OnResponseReceived(InteractionButtonResponse response)
        {
            Log.Info("InteractionMessageModel.OnResponseRecived()", "InteractionButtonResponse was set to the model. Parameter: Button = '{0}' ", response?.Action);
            NotifyAboutResponseReceived();
            Dispose();
        }

        public override void LogMessage()
        {
            Log.Warning("InteractionMessageModel.LogMessage", "\n\t[TestAutomationInteraction]\n\tTitle: [" + base.Title + "]\n\tAnswers: [Commit]\n\tQuestion: [" + MessageText + "]");
        }

        public override void LogResponseMessage(object response)
        {
            string name = Enum.GetName(typeof(InteractionButton), ((InteractionButtonResponse)response).Action);
            Log.Info("InteractionMessageModel.LogResponseMessage", "\n\t[TestAutomationInteraction]\n\tTitle: [" + base.Title + "]\n\tAnswer: [" + name + "]\n\tQuestion: [" + MessageText + "]");
        }
    }
}