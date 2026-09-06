using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_VIRTUALFAULTCODES : INotifyPropertyChanged
    {
        private decimal idField;

        private string cODEField;

        private decimal? eCUNOANSWERField;

        private DateTime? vALIDFROMField;

        private DateTime? vALIDTOField;

        private decimal? sICHERHEITSRELEVANTField;

        private decimal? wEIGHTINGField;

        private decimal? pARENTIDField;

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

        public string CODE
        {
            get
            {
                return cODEField;
            }
            set
            {
                if (cODEField != null)
                {
                    if (!cODEField.Equals(value))
                    {
                        cODEField = value;
                        OnPropertyChanged("CODE");
                    }
                }
                else
                {
                    cODEField = value;
                    OnPropertyChanged("CODE");
                }
            }
        }

        public decimal? ECUNOANSWER
        {
            get
            {
                return eCUNOANSWERField;
            }
            set
            {
                if (eCUNOANSWERField.HasValue)
                {
                    if (!eCUNOANSWERField.Equals(value))
                    {
                        eCUNOANSWERField = value;
                        OnPropertyChanged("ECUNOANSWER");
                    }
                }
                else
                {
                    eCUNOANSWERField = value;
                    OnPropertyChanged("ECUNOANSWER");
                }
            }
        }

        public DateTime? VALIDFROM
        {
            get
            {
                return vALIDFROMField;
            }
            set
            {
                if (vALIDFROMField.HasValue)
                {
                    if (!vALIDFROMField.Equals(value))
                    {
                        vALIDFROMField = value;
                        OnPropertyChanged("VALIDFROM");
                    }
                }
                else
                {
                    vALIDFROMField = value;
                    OnPropertyChanged("VALIDFROM");
                }
            }
        }

        public DateTime? VALIDTO
        {
            get
            {
                return vALIDTOField;
            }
            set
            {
                if (vALIDTOField.HasValue)
                {
                    if (!vALIDTOField.Equals(value))
                    {
                        vALIDTOField = value;
                        OnPropertyChanged("VALIDTO");
                    }
                }
                else
                {
                    vALIDTOField = value;
                    OnPropertyChanged("VALIDTO");
                }
            }
        }

        public decimal? SICHERHEITSRELEVANT
        {
            get
            {
                return sICHERHEITSRELEVANTField;
            }
            set
            {
                if (sICHERHEITSRELEVANTField.HasValue)
                {
                    if (!sICHERHEITSRELEVANTField.Equals(value))
                    {
                        sICHERHEITSRELEVANTField = value;
                        OnPropertyChanged("SICHERHEITSRELEVANT");
                    }
                }
                else
                {
                    sICHERHEITSRELEVANTField = value;
                    OnPropertyChanged("SICHERHEITSRELEVANT");
                }
            }
        }

        public decimal? WEIGHTING
        {
            get
            {
                return wEIGHTINGField;
            }
            set
            {
                if (wEIGHTINGField.HasValue)
                {
                    if (!wEIGHTINGField.Equals(value))
                    {
                        wEIGHTINGField = value;
                        OnPropertyChanged("WEIGHTING");
                    }
                }
                else
                {
                    wEIGHTINGField = value;
                    OnPropertyChanged("WEIGHTING");
                }
            }
        }

        public decimal? PARENTID
        {
            get
            {
                return pARENTIDField;
            }
            set
            {
                if (pARENTIDField.HasValue)
                {
                    if (!pARENTIDField.Equals(value))
                    {
                        pARENTIDField = value;
                        OnPropertyChanged("PARENTID");
                    }
                }
                else
                {
                    pARENTIDField = value;
                    OnPropertyChanged("PARENTID");
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
            if (!(obj is XEP_VIRTUALFAULTCODES xEP_VIRTUALFAULTCODES))
            {
                return false;
            }
            if (ID != xEP_VIRTUALFAULTCODES.ID)
            {
                return false;
            }
            if (string.CompareOrdinal(CODE, xEP_VIRTUALFAULTCODES.CODE) != 0)
            {
                return false;
            }
            if (!(ECUNOANSWER == xEP_VIRTUALFAULTCODES.ECUNOANSWER))
            {
                return false;
            }
            if (VALIDFROM != xEP_VIRTUALFAULTCODES.VALIDFROM)
            {
                return false;
            }
            if (VALIDTO != xEP_VIRTUALFAULTCODES.VALIDTO)
            {
                return false;
            }
            if (!(SICHERHEITSRELEVANT == xEP_VIRTUALFAULTCODES.SICHERHEITSRELEVANT))
            {
                return false;
            }
            if (!(WEIGHTING == xEP_VIRTUALFAULTCODES.WEIGHTING))
            {
                return false;
            }
            if (!(PARENTID == xEP_VIRTUALFAULTCODES.PARENTID))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
