using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class SALAPALocalizedEntry : INotifyPropertyChanged, ISalapaLocalizedEntry
    {
        private string fAHRZEUGARTField;
        private uint indexField;
        private string vERTRIEBSSCHLUESSELField;
        private string iSO_SPRACHEField;
        private string bENENNUNGField;
        public string FAHRZEUGART
        {
            get
            {
                return fAHRZEUGARTField;
            }

            set
            {
                if (fAHRZEUGARTField != null)
                {
                    if (!fAHRZEUGARTField.Equals(value))
                    {
                        fAHRZEUGARTField = value;
                        OnPropertyChanged("FAHRZEUGART");
                    }
                }
                else
                {
                    fAHRZEUGARTField = value;
                    OnPropertyChanged("FAHRZEUGART");
                }
            }
        }

        public uint Index
        {
            get
            {
                return indexField;
            }

            set
            {
                _ = indexField;
                if (!indexField.Equals(value))
                {
                    indexField = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        public string VERTRIEBSSCHLUESSEL
        {
            get
            {
                return vERTRIEBSSCHLUESSELField;
            }

            set
            {
                if (vERTRIEBSSCHLUESSELField != null)
                {
                    if (!vERTRIEBSSCHLUESSELField.Equals(value))
                    {
                        vERTRIEBSSCHLUESSELField = value;
                        OnPropertyChanged("VERTRIEBSSCHLUESSEL");
                    }
                }
                else
                {
                    vERTRIEBSSCHLUESSELField = value;
                    OnPropertyChanged("VERTRIEBSSCHLUESSEL");
                }
            }
        }

        public string ISO_SPRACHE
        {
            get
            {
                return iSO_SPRACHEField;
            }

            set
            {
                if (iSO_SPRACHEField != null)
                {
                    if (!iSO_SPRACHEField.Equals(value))
                    {
                        iSO_SPRACHEField = value;
                        OnPropertyChanged("ISO_SPRACHE");
                    }
                }
                else
                {
                    iSO_SPRACHEField = value;
                    OnPropertyChanged("ISO_SPRACHE");
                }
            }
        }

        public string BENENNUNG
        {
            get
            {
                return bENENNUNGField;
            }

            set
            {
                if (bENENNUNGField != null)
                {
                    if (!bENENNUNGField.Equals(value))
                    {
                        bENENNUNGField = value;
                        OnPropertyChanged("BENENNUNG");
                    }
                }
                else
                {
                    bENENNUNGField = value;
                    OnPropertyChanged("BENENNUNG");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SALAPALocalizedEntry()
        {
            fAHRZEUGARTField = "0";
            indexField = 0u;
            vERTRIEBSSCHLUESSELField = "0000";
            iSO_SPRACHEField = "0";
            bENENNUNGField = "0";
        }

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

            if (!(obj is SALAPALocalizedEntry))
            {
                return false;
            }

            SALAPALocalizedEntry sALAPALocalizedEntry = (SALAPALocalizedEntry)obj;
            if (Index != sALAPALocalizedEntry.Index)
            {
                return false;
            }

            if (string.Compare(BENENNUNG, sALAPALocalizedEntry.BENENNUNG, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            if (string.Compare(FAHRZEUGART, sALAPALocalizedEntry.FAHRZEUGART, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            if (string.Compare(ISO_SPRACHE, sALAPALocalizedEntry.ISO_SPRACHE, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            if (string.Compare(VERTRIEBSSCHLUESSEL, sALAPALocalizedEntry.VERTRIEBSSCHLUESSEL, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}