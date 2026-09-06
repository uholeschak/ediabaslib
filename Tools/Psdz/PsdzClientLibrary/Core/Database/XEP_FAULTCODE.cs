using System;
using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_FAULTCODE : INotifyPropertyChanged, IXepFaultCode
    {
        private decimal idField;

        private string cODEField;

        private string dATATYPEField;

        private decimal? wEIGHTINGField;

        private string sCHEINFEHLERField;

        private string aUSBLENDINDEXField;

        private decimal? rELEVANCEField;

        private decimal? sICHERHEITSRELEVANTField;

        private DateTime? vALIDTOField;

        private DateTime? vALIDFROMField;

        private string dIAGNOSEINDEXField;

        private decimal? eCUVARIANTIDField;

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

        public string DATATYPE
        {
            get
            {
                return dATATYPEField;
            }
            set
            {
                if (dATATYPEField != null)
                {
                    if (!dATATYPEField.Equals(value))
                    {
                        dATATYPEField = value;
                        OnPropertyChanged("DATATYPE");
                    }
                }
                else
                {
                    dATATYPEField = value;
                    OnPropertyChanged("DATATYPE");
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

        public string SCHEINFEHLER
        {
            get
            {
                return sCHEINFEHLERField;
            }
            set
            {
                if (sCHEINFEHLERField != null)
                {
                    if (!sCHEINFEHLERField.Equals(value))
                    {
                        sCHEINFEHLERField = value;
                        OnPropertyChanged("SCHEINFEHLER");
                    }
                }
                else
                {
                    sCHEINFEHLERField = value;
                    OnPropertyChanged("SCHEINFEHLER");
                }
            }
        }

        public string AUSBLENDINDEX
        {
            get
            {
                return aUSBLENDINDEXField;
            }
            set
            {
                if (aUSBLENDINDEXField != null)
                {
                    if (!aUSBLENDINDEXField.Equals(value))
                    {
                        aUSBLENDINDEXField = value;
                        OnPropertyChanged("AUSBLENDINDEX");
                    }
                }
                else
                {
                    aUSBLENDINDEXField = value;
                    OnPropertyChanged("AUSBLENDINDEX");
                }
            }
        }

        public decimal? RELEVANCE
        {
            get
            {
                return rELEVANCEField;
            }
            set
            {
                if (rELEVANCEField.HasValue)
                {
                    if (!rELEVANCEField.Equals(value))
                    {
                        rELEVANCEField = value;
                        OnPropertyChanged("RELEVANCE");
                    }
                }
                else
                {
                    rELEVANCEField = value;
                    OnPropertyChanged("RELEVANCE");
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

        public string DIAGNOSEINDEX
        {
            get
            {
                return dIAGNOSEINDEXField;
            }
            set
            {
                if (dIAGNOSEINDEXField != null)
                {
                    if (!dIAGNOSEINDEXField.Equals(value))
                    {
                        dIAGNOSEINDEXField = value;
                        OnPropertyChanged("DIAGNOSEINDEX");
                    }
                }
                else
                {
                    dIAGNOSEINDEXField = value;
                    OnPropertyChanged("DIAGNOSEINDEX");
                }
            }
        }

        public decimal? ECUVARIANTID
        {
            get
            {
                return eCUVARIANTIDField;
            }
            set
            {
                if (eCUVARIANTIDField.HasValue)
                {
                    if (!eCUVARIANTIDField.Equals(value))
                    {
                        eCUVARIANTIDField = value;
                        OnPropertyChanged("ECUVARIANTID");
                    }
                }
                else
                {
                    eCUVARIANTIDField = value;
                    OnPropertyChanged("ECUVARIANTID");
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
            if (!(obj is XEP_FAULTCODE xEP_FAULTCODE))
            {
                return false;
            }
            if (ID != xEP_FAULTCODE.ID)
            {
                return false;
            }
            if (string.CompareOrdinal(CODE, xEP_FAULTCODE.CODE) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(DATATYPE, xEP_FAULTCODE.DATATYPE) != 0)
            {
                return false;
            }
            if (!(WEIGHTING == xEP_FAULTCODE.WEIGHTING))
            {
                return false;
            }
            if (string.CompareOrdinal(SCHEINFEHLER, xEP_FAULTCODE.SCHEINFEHLER) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(AUSBLENDINDEX, xEP_FAULTCODE.AUSBLENDINDEX) != 0)
            {
                return false;
            }
            if (!(RELEVANCE == xEP_FAULTCODE.RELEVANCE))
            {
                return false;
            }
            if (!(SICHERHEITSRELEVANT == xEP_FAULTCODE.SICHERHEITSRELEVANT))
            {
                return false;
            }
            if (VALIDTO != xEP_FAULTCODE.VALIDTO)
            {
                return false;
            }
            if (VALIDFROM != xEP_FAULTCODE.VALIDFROM)
            {
                return false;
            }
            if (string.CompareOrdinal(DIAGNOSEINDEX, xEP_FAULTCODE.DIAGNOSEINDEX) != 0)
            {
                return false;
            }
            if (!(ECUVARIANTID == xEP_FAULTCODE.ECUVARIANTID))
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
