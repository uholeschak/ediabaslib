using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class QuestionServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private readonly ObservableCollection<bool> checkedInfo = new ObservableCollection<bool>(new bool[2]);

        [DataMember]
        private readonly ObservableCollection<string> textInfo = new ObservableCollection<string>(new string[2]);

        [DataMember]
        private string text;

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

        public ObservableCollection<bool> CheckedInfo => checkedInfo;

        public ObservableCollection<string> TextInfo => textInfo;
    }
}
