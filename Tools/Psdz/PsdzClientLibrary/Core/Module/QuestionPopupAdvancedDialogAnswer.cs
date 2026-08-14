using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class QuestionPopupAdvancedDialogAnswer : INotifyPropertyChanged
    {
        [DataMember]
        private string text;
        [DataMember]
        private string content;
        [DataMember]
        private int returnValue;
        [DataMember]
        private string buttonId;
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public string Content
        {
            get
            {
                return content;
            }

            set
            {
                if (content != value)
                {
                    content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public int ReturnValue
        {
            get
            {
                return returnValue;
            }

            set
            {
                if (returnValue != value)
                {
                    returnValue = value;
                    OnPropertyChanged("ReturnValue");
                }
            }
        }

        public string ButtonId
        {
            get
            {
                return buttonId;
            }

            set
            {
                if (buttonId != value)
                {
                    buttonId = value;
                    OnPropertyChanged("ButtonId");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public QuestionPopupAdvancedDialogAnswer(string text, string content, int returnValue, string buttonId = null)
        {
            this.text = text;
            this.content = content;
            this.returnValue = returnValue;
            this.buttonId = buttonId;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}