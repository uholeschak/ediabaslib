using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_REFECUVARIANTS : INotifyPropertyChanged
    {
        private decimal idField;
        private decimal ecuVariantIdField;
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

        public decimal EcuVariantId
        {
            get
            {
                return ecuVariantIdField;
            }

            set
            {
                _ = ecuVariantIdField;
                if (!ecuVariantIdField.Equals(value))
                {
                    ecuVariantIdField = value;
                    OnPropertyChanged("EcuVariantId");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is XEP_REFECUVARIANTS xEP_REFECUVARIANTS))
            {
                return false;
            }

            if (Id != xEP_REFECUVARIANTS.Id)
            {
                return false;
            }

            if (EcuVariantId != xEP_REFECUVARIANTS.EcuVariantId)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (Id + EcuVariantId).GetHashCode();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_REFECUVARIANTS Clone()
        {
            return (XEP_REFECUVARIANTS)MemberwiseClone();
        }
    }
}