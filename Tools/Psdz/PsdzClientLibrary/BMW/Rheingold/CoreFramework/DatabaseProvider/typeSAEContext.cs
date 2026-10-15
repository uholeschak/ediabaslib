using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class typeSAEContext : INotifyPropertyChanged
    {
        private string SAECodeField;
        private string SAETextField;
        private int? SAEStatusField = -1;
        public string SAECode
        {
            get
            {
                return SAECodeField;
            }

            set
            {
                if (SAECodeField != null)
                {
                    if (!SAECodeField.Equals(value))
                    {
                        SAECodeField = value;
                        OnPropertyChanged("SAECode");
                    }
                }
                else
                {
                    SAECodeField = value;
                    OnPropertyChanged("SAECode");
                }
            }
        }

        public string SAEText
        {
            get
            {
                return SAETextField;
            }

            set
            {
                if (SAETextField != null)
                {
                    if (!SAETextField.Equals(value))
                    {
                        SAETextField = value;
                        OnPropertyChanged("SAEText");
                    }
                }
                else
                {
                    SAETextField = value;
                    OnPropertyChanged("SAEText");
                }
            }
        }

        public int? SAEStatus
        {
            get
            {
                return SAEStatusField;
            }

            set
            {
                if (SAEStatusField.HasValue)
                {
                    if (!SAEStatusField.Equals(value))
                    {
                        SAEStatusField = value;
                        OnPropertyChanged("SAEStatus");
                    }
                }
                else
                {
                    SAEStatusField = value;
                    OnPropertyChanged("SAEStatus");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}