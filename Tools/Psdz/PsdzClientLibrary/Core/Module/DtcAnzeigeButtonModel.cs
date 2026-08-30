using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class DtcAnzeigeButtonModel : INotifyPropertyChanged
    {
        [DataMember]
        private bool isMarked;
        [DataMember]
        private bool isSelected;
        [DataMember]
        private string fortAsHexString;
        [DataMember]
        private string faultLabel;
        [DataMember]
        private string buttonNo;
        [DataMember]
        private int index;
        [DataMember]
        private bool isEnabled;
        public bool IsMarked
        {
            get
            {
                return isMarked;
            }

            set
            {
                isMarked = value;
                OnPropertyChanged("IsMarked");
            }
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public string FortAsHexString
        {
            get
            {
                return fortAsHexString;
            }

            set
            {
                fortAsHexString = value;
                OnPropertyChanged("FortAsHexString");
            }
        }

        public string FaultLabel
        {
            get
            {
                return faultLabel;
            }

            set
            {
                faultLabel = value;
                OnPropertyChanged("FaultLabel");
            }
        }

        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
                OnPropertyChanged("Index");
            }
        }

        public string ButtonNo
        {
            get
            {
                return buttonNo;
            }

            set
            {
                buttonNo = value;
                OnPropertyChanged("ButtonNo");
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }

            set
            {
                isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public DtcAnzeigeButtonModel()
        {
        }

        public DtcAnzeigeButtonModel(bool isMarked, bool isSelected, string fortAsHexString, string faultLabel, string buttonNo, int index)
        {
            this.isSelected = isSelected;
            this.isMarked = isMarked;
            this.fortAsHexString = fortAsHexString;
            this.faultLabel = faultLabel;
            this.buttonNo = buttonNo;
            this.index = index;
            isEnabled = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}