using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class QuestionPopupDialogAnswer : INotifyPropertyChanged
    {
        [DataMember]
        private string text;

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
                if (text == null || text.Equals(value))
                {
                    text = value;
                    OnPropertyChanged("Text");
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

        public QuestionPopupDialogAnswer(string text, int returnValue, string buttonId = null)
        {
            this.text = text;
            this.returnValue = returnValue;
            this.buttonId = buttonId;
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Parameter \"text\" must not be null.");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
