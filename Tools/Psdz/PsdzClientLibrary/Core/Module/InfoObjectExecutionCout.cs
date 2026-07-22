using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class InfoObjectExecutionCout : INotifyPropertyChanged
    {
        private int allField;

        private int canceledField;

        public int All
        {
            get
            {
                return allField;
            }
            set
            {
                if (!allField.Equals(value))
                {
                    allField = value;
                    OnPropertyChanged("All");
                }
            }
        }

        public int Canceled
        {
            get
            {
                return canceledField;
            }
            set
            {
                if (!canceledField.Equals(value))
                {
                    canceledField = value;
                    OnPropertyChanged("Canceled");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
