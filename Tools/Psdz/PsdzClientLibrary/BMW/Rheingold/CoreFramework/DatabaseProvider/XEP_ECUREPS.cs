using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUREPS : INotifyPropertyChanged, IXepEcuReps
    {
        private decimal idField;
        private string steuergeraeteKuerzelField;
        public decimal Id
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
                    OnPropertyChanged("Id");
                }
            }
        }

        public string SteuergeraeteKuerzel
        {
            get
            {
                return steuergeraeteKuerzelField;
            }

            set
            {
                if (steuergeraeteKuerzelField != null)
                {
                    if (!steuergeraeteKuerzelField.Equals(value))
                    {
                        steuergeraeteKuerzelField = value;
                        OnPropertyChanged("SteuergeraeteKuerzel");
                    }
                }
                else
                {
                    steuergeraeteKuerzelField = value;
                    OnPropertyChanged("SteuergeraeteKuerzel");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_ECUREPS Clone()
        {
            return (XEP_ECUREPS)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is XEP_ECUREPS xEP_ECUREPS))
            {
                return false;
            }

            if (Id != xEP_ECUREPS.Id)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}