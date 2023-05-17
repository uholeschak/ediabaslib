using System.ComponentModel;

namespace PsdzClient.Core
{
    public abstract class BaseFormatedData : INotifyPropertyChanged
    {
        private string fmtStrIdField;

        private bool translateValuesField;

        public string fmtStrId
        {
            get
            {
                return fmtStrIdField;
            }
            set
            {
                if (fmtStrIdField != null)
                {
                    if (!fmtStrIdField.Equals(value))
                    {
                        fmtStrIdField = value;
                        OnPropertyChanged("fmtStrId");
                    }
                }
                else
                {
                    fmtStrIdField = value;
                    OnPropertyChanged("fmtStrId");
                }
            }
        }

        [DefaultValue(false)]
        public bool translateValues
        {
            get
            {
                return translateValuesField;
            }
            set
            {
                if (!translateValuesField.Equals(value))
                {
                    translateValuesField = value;
                    OnPropertyChanged("translateValues");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseFormatedData()
        {
            translateValuesField = false;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
