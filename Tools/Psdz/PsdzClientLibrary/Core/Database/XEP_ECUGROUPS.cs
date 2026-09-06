using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUGROUPS : INotifyPropertyChanged, IXepEcuGroups
    {
        private decimal idField;

        private decimal obdIdentificationField;

        private decimal faultMemoryDeleteIdentificatioField;

        private decimal faultMemoryDeleteWaitingTimeField;

        private string nameField;

        private decimal virtuellField;

        private decimal sicherheitsrelevantField;

        private DateTime validToField;

        private DateTime validFromField;

        private decimal diagnosticAddressField;

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

        public decimal ObdIdentification
        {
            get
            {
                return obdIdentificationField;
            }
            set
            {
                if (!obdIdentificationField.Equals(value))
                {
                    obdIdentificationField = value;
                    OnPropertyChanged("ObdIdentification");
                }
            }
        }

        public decimal FaultMemoryDeleteIdentificatio
        {
            get
            {
                return faultMemoryDeleteIdentificatioField;
            }
            set
            {
                if (!faultMemoryDeleteIdentificatioField.Equals(value))
                {
                    faultMemoryDeleteIdentificatioField = value;
                    OnPropertyChanged("FaultMemoryDeleteIdentificatio");
                }
            }
        }

        public decimal FaultMemoryDeleteWaitingTime
        {
            get
            {
                return faultMemoryDeleteWaitingTimeField;
            }
            set
            {
                if (!faultMemoryDeleteWaitingTimeField.Equals(value))
                {
                    faultMemoryDeleteWaitingTimeField = value;
                    OnPropertyChanged("FaultMemoryDeleteWaitingTime");
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

        public decimal Virtuell
        {
            get
            {
                return virtuellField;
            }
            set
            {
                if (!virtuellField.Equals(value))
                {
                    virtuellField = value;
                    OnPropertyChanged("Virtuell");
                }
            }
        }

        public decimal Sicherheitsrelevant
        {
            get
            {
                return sicherheitsrelevantField;
            }
            set
            {
                if (!sicherheitsrelevantField.Equals(value))
                {
                    sicherheitsrelevantField = value;
                    OnPropertyChanged("Sicherheitsrelevant");
                }
            }
        }

        public DateTime ValidTo
        {
            get
            {
                return validToField;
            }
            set
            {
                if (!validToField.Equals(value))
                {
                    validToField = value;
                    OnPropertyChanged("ValidTo");
                }
            }
        }

        public DateTime ValidFrom
        {
            get
            {
                return validFromField;
            }
            set
            {
                if (!validFromField.Equals(value))
                {
                    validFromField = value;
                    OnPropertyChanged("ValidFrom");
                }
            }
        }

        public decimal DiagnosticAddress
        {
            get
            {
                return diagnosticAddressField;
            }
            set
            {
                if (!diagnosticAddressField.Equals(value))
                {
                    diagnosticAddressField = value;
                    OnPropertyChanged("DiagnosticAddress");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual XEP_ECUGROUPS Clone()
        {
            return (XEP_ECUGROUPS)MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_ECUGROUPS xEP_ECUGROUPS))
            {
                return false;
            }
            if (Id != xEP_ECUGROUPS.Id)
            {
                return false;
            }
            if (ObdIdentification != xEP_ECUGROUPS.ObdIdentification)
            {
                return false;
            }
            if (FaultMemoryDeleteIdentificatio != xEP_ECUGROUPS.FaultMemoryDeleteIdentificatio)
            {
                return false;
            }
            if (FaultMemoryDeleteWaitingTime != xEP_ECUGROUPS.FaultMemoryDeleteWaitingTime)
            {
                return false;
            }
            if (string.CompareOrdinal(Name, xEP_ECUGROUPS.Name) != 0)
            {
                return false;
            }
            if (Virtuell != xEP_ECUGROUPS.Virtuell)
            {
                return false;
            }
            if (ValidTo != xEP_ECUGROUPS.ValidTo)
            {
                return false;
            }
            if (ValidFrom != xEP_ECUGROUPS.ValidFrom)
            {
                return false;
            }
            if (DiagnosticAddress != xEP_ECUGROUPS.DiagnosticAddress)
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
