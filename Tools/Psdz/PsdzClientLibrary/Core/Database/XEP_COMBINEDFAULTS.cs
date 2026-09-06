using System;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_COMBINEDFAULTS : INotifyPropertyChanged
    {
        private decimal idField;

        private string cODEField;

        private string fAULTCODETYPEField;

        private decimal? kMBEREICHField;

        private decimal? zEITBEREICHField;

        private string rULEField;

        private string zEITBEREICHEINHEITField;

        private decimal? wEIGHTINGField;

        private decimal? sICHERHEITSRELEVANTField;

        private DateTime? vALIDTOField;

        private DateTime? vALIDFROMField;

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

        public string FAULTCODETYPE
        {
            get
            {
                return fAULTCODETYPEField;
            }
            set
            {
                if (fAULTCODETYPEField != null)
                {
                    if (!fAULTCODETYPEField.Equals(value))
                    {
                        fAULTCODETYPEField = value;
                        OnPropertyChanged("FAULTCODETYPE");
                    }
                }
                else
                {
                    fAULTCODETYPEField = value;
                    OnPropertyChanged("FAULTCODETYPE");
                }
            }
        }

        public decimal? KMBEREICH
        {
            get
            {
                return kMBEREICHField;
            }
            set
            {
                if (kMBEREICHField.HasValue)
                {
                    if (!kMBEREICHField.Equals(value))
                    {
                        kMBEREICHField = value;
                        OnPropertyChanged("KMBEREICH");
                    }
                }
                else
                {
                    kMBEREICHField = value;
                    OnPropertyChanged("KMBEREICH");
                }
            }
        }

        public decimal? ZEITBEREICH
        {
            get
            {
                return zEITBEREICHField;
            }
            set
            {
                if (zEITBEREICHField.HasValue)
                {
                    if (!zEITBEREICHField.Equals(value))
                    {
                        zEITBEREICHField = value;
                        OnPropertyChanged("ZEITBEREICH");
                    }
                }
                else
                {
                    zEITBEREICHField = value;
                    OnPropertyChanged("ZEITBEREICH");
                }
            }
        }

        public string RULE
        {
            get
            {
                return rULEField;
            }
            set
            {
                if (rULEField != null)
                {
                    if (!rULEField.Equals(value))
                    {
                        rULEField = value;
                        OnPropertyChanged("RULE");
                    }
                }
                else
                {
                    rULEField = value;
                    OnPropertyChanged("RULE");
                }
            }
        }

        public string ZEITBEREICHEINHEIT
        {
            get
            {
                return zEITBEREICHEINHEITField;
            }
            set
            {
                if (zEITBEREICHEINHEITField != null)
                {
                    if (!zEITBEREICHEINHEITField.Equals(value))
                    {
                        zEITBEREICHEINHEITField = value;
                        OnPropertyChanged("ZEITBEREICHEINHEIT");
                    }
                }
                else
                {
                    zEITBEREICHEINHEITField = value;
                    OnPropertyChanged("ZEITBEREICHEINHEIT");
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
            if (!(obj is XEP_COMBINEDFAULTS xEP_COMBINEDFAULTS))
            {
                return false;
            }
            if (ID != xEP_COMBINEDFAULTS.ID)
            {
                return false;
            }
            if (string.CompareOrdinal(CODE, xEP_COMBINEDFAULTS.CODE) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(FAULTCODETYPE, xEP_COMBINEDFAULTS.FAULTCODETYPE) != 0)
            {
                return false;
            }
            if (!(KMBEREICH == xEP_COMBINEDFAULTS.KMBEREICH))
            {
                return false;
            }
            if (!(ZEITBEREICH == xEP_COMBINEDFAULTS.ZEITBEREICH))
            {
                return false;
            }
            if (string.CompareOrdinal(RULE, xEP_COMBINEDFAULTS.RULE) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(ZEITBEREICHEINHEIT, xEP_COMBINEDFAULTS.ZEITBEREICHEINHEIT) != 0)
            {
                return false;
            }
            if (!(WEIGHTING == xEP_COMBINEDFAULTS.WEIGHTING))
            {
                return false;
            }
            if (!(SICHERHEITSRELEVANT == xEP_COMBINEDFAULTS.SICHERHEITSRELEVANT))
            {
                return false;
            }
            if (VALIDFROM != xEP_COMBINEDFAULTS.VALIDFROM)
            {
                return false;
            }
            if (VALIDTO != xEP_COMBINEDFAULTS.VALIDTO)
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
