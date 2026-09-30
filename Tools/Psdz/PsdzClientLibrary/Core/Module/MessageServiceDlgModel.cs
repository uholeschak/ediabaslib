using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class MessageServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string text;

        [DataMember]
        private string value;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (!object.Equals(text, value))
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!object.Equals(this.value, value))
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }
    }
}
