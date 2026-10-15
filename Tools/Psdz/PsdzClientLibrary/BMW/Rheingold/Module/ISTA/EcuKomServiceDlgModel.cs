using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class EcuKomServiceDlgModel : QuestionServiceDlgModel
    {
        [DataMember]
        private string txtParamFlow;

        [DataMember]
        private string wertFeldFlow;

        [DataMember]
        private string wertFeldFlow1;

        [DataMember]
        private string iOFrageTextFlow;

        [DataMember]
        private bool isButtonBarVisible;

        [DataMember]
        private string customButton0Content;

        [DataMember]
        private bool isCustomButton0Enabled;

        [DataMember]
        private bool isCustomButton0Visible;

        public string TxtParamFlow
        {
            get
            {
                return txtParamFlow;
            }
            set
            {
                txtParamFlow = value;
                OnPropertyChanged("TxtParamFlow");
            }
        }

        public string WertFeldFlow
        {
            get
            {
                return wertFeldFlow;
            }
            set
            {
                wertFeldFlow = value;
                OnPropertyChanged("WertFeldFlow");
            }
        }

        public string WertFeldFlow1
        {
            get
            {
                return wertFeldFlow1;
            }
            set
            {
                wertFeldFlow1 = value;
                OnPropertyChanged("WertFeldFlow1");
            }
        }

        public string IOFrageTextFlow
        {
            get
            {
                return iOFrageTextFlow;
            }
            set
            {
                iOFrageTextFlow = value;
                OnPropertyChanged("IOFrageTextFlow");
            }
        }

        public bool IsButtonBarVisible
        {
            get
            {
                return isButtonBarVisible;
            }
            set
            {
                isButtonBarVisible = value;
                OnPropertyChanged("IsButtonBarVisible");
            }
        }

        public string CustomButton0Content
        {
            get
            {
                return customButton0Content;
            }
            set
            {
                customButton0Content = value;
                OnPropertyChanged("CustomButton0Content");
            }
        }

        public bool IsCustomButton0Enabled
        {
            get
            {
                return isCustomButton0Enabled;
            }
            set
            {
                isCustomButton0Enabled = value;
                OnPropertyChanged("IsCustomButton0Enabled");
            }
        }

        public bool IsCustomButton0Visible
        {
            get
            {
                return isCustomButton0Visible;
            }
            set
            {
                isCustomButton0Visible = value;
                OnPropertyChanged("IsCustomButton0Visible");
            }
        }
    }
}
