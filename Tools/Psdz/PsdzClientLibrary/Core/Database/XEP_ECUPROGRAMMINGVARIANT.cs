using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUPROGRAMMINGVARIANT : INotifyPropertyChanged, IXepEcuProgrammingVariant
    {
        private decimal idField;
        private string nameField;
        private decimal? flashLimitField;
        private decimal ecuVariantIdField;
        public decimal Id
        {
            get
            {
                return idField;
            }

            set
            {
                if (!idField.Equals(value))
                {
                    idField = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get
            {
                return nameField;
            }

            set
            {
                if (nameField == null || !nameField.Equals(value))
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public decimal? FlashLimit
        {
            get
            {
                return flashLimitField;
            }

            set
            {
                if (!flashLimitField.HasValue || !flashLimitField.Equals(value))
                {
                    flashLimitField = value;
                    OnPropertyChanged("FlashLimit");
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
                if (!ecuVariantIdField.Equals(value))
                {
                    ecuVariantIdField = value;
                    OnPropertyChanged("EcuVariantId");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_ECUPROGRAMMINGVARIANT Clone()
        {
            return (XEP_ECUPROGRAMMINGVARIANT)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is XEP_ECUPROGRAMMINGVARIANT xEP_ECUPROGRAMMINGVARIANT))
            {
                return false;
            }

            if (Id != xEP_ECUPROGRAMMINGVARIANT.Id)
            {
                return false;
            }

            if (string.CompareOrdinal(Name, xEP_ECUPROGRAMMINGVARIANT.Name) != 0)
            {
                return false;
            }

            if (!(FlashLimit == xEP_ECUPROGRAMMINGVARIANT.FlashLimit))
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