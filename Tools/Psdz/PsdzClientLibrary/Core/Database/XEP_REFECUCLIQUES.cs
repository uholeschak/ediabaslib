using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_REFECUCLIQUES : INotifyPropertyChanged
    {
        private decimal idField;

        private decimal eCUCLIQUEIDField;

        public decimal ID
        {
            get
            {
                return idField;
            }
            set
            {
                _ = idField;
                if (!idField.Equals(value))
                {
                    idField = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public decimal ECUCLIQUEID
        {
            get
            {
                return eCUCLIQUEIDField;
            }
            set
            {
                _ = eCUCLIQUEIDField;
                if (!eCUCLIQUEIDField.Equals(value))
                {
                    eCUCLIQUEIDField = value;
                    OnPropertyChanged("ECUCLIQUEID");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_REFECUCLIQUES xEP_REFECUCLIQUES))
            {
                return false;
            }
            if (ID != xEP_REFECUCLIQUES.ID)
            {
                return false;
            }
            if (ECUCLIQUEID != xEP_REFECUCLIQUES.ECUCLIQUEID)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (ID + ECUCLIQUEID).GetHashCode();
        }
    }
}
