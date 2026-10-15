using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class QuestionSelectServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private ObservableCollection<QuestionSelectButtonModel> btns;

        [DataMember]
        private string pageTitle;

        [DataMember]
        private string priorText;

        [DataMember]
        private string successorText;

        public ObservableCollection<QuestionSelectButtonModel> Buttons
        {
            get
            {
                return btns;
            }
            private set
            {
                btns = value;
                OnPropertyChanged("Buttons");
            }
        }

        public string PageTitle
        {
            get
            {
                return pageTitle;
            }
            set
            {
                if (!object.Equals(pageTitle, value))
                {
                    pageTitle = value;
                    OnPropertyChanged("PageTitle");
                }
            }
        }

        public string PriorText
        {
            get
            {
                return priorText;
            }
            set
            {
                if (!object.Equals(priorText, value))
                {
                    priorText = value;
                    OnPropertyChanged("PriorText");
                }
            }
        }

        public string SuccessorText
        {
            get
            {
                return successorText;
            }
            set
            {
                if (!object.Equals(successorText, value))
                {
                    successorText = value;
                    OnPropertyChanged("SuccessorText");
                }
            }
        }

        public QuestionSelectServiceDlgModel()
        {
            Buttons = new ObservableCollection<QuestionSelectButtonModel>();
        }
    }
}
