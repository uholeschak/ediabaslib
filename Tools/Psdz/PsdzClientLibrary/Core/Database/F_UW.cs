using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class F_UW : IDtcUmwelt, INotifyPropertyChanged
    {
        private long? f_UW_NRField;
        private string f_UW_TEXTField;
        private object f_UW_WERTField;
        private string f_UW_EINHField;
        private byte[] f_UW_DATAField;
        private string f_UW_NAMEField;
        private object f_UW_RAWField;
        private UwType f_UW_TYPField;
        private bool ctordoneField;
        public long? F_UW_NR
        {
            get
            {
                return f_UW_NRField;
            }

            set
            {
                if (f_UW_NRField.HasValue)
                {
                    if (!f_UW_NRField.Equals(value))
                    {
                        f_UW_NRField = value;
                        OnPropertyChanged("F_UW_NR");
                    }
                }
                else
                {
                    f_UW_NRField = value;
                    OnPropertyChanged("F_UW_NR");
                }
            }
        }

        public string F_UW_NAME
        {
            get
            {
                return f_UW_NAMEField;
            }

            set
            {
                if (f_UW_NAMEField != null)
                {
                    if (!f_UW_NAMEField.Equals(value))
                    {
                        f_UW_NAMEField = value;
                        OnPropertyChanged("F_UW_NAME");
                    }
                }
                else
                {
                    f_UW_NAMEField = value;
                    OnPropertyChanged("F_UW_NAME");
                }
            }
        }

        public UwType F_UW_TYP
        {
            get
            {
                return f_UW_TYPField;
            }

            set
            {
                _ = f_UW_TYPField;
                if (!f_UW_TYPField.Equals(value))
                {
                    f_UW_TYPField = value;
                    OnPropertyChanged("F_UW_TYP");
                }
            }
        }

        public object F_UW_RAW
        {
            get
            {
                return f_UW_RAWField;
            }

            set
            {
                if (f_UW_RAWField != null)
                {
                    if (!f_UW_RAWField.Equals(value))
                    {
                        f_UW_RAWField = value;
                        OnPropertyChanged("F_UW_RAW");
                    }
                }
                else
                {
                    f_UW_RAWField = value;
                    OnPropertyChanged("F_UW_RAW");
                }
            }
        }

        public byte[] F_UW_DATA
        {
            get
            {
                return f_UW_DATAField;
            }

            set
            {
                if (f_UW_DATAField != null)
                {
                    if (!f_UW_DATAField.Equals(value))
                    {
                        f_UW_DATAField = value;
                        OnPropertyChanged("F_UW_DATA");
                    }
                }
                else
                {
                    f_UW_DATAField = value;
                    OnPropertyChanged("F_UW_DATA");
                }
            }
        }

        public string F_UW_TEXT
        {
            get
            {
                return f_UW_TEXTField;
            }

            set
            {
                if (f_UW_TEXTField != null)
                {
                    if (!f_UW_TEXTField.Equals(value))
                    {
                        f_UW_TEXTField = value;
                        OnPropertyChanged("F_UW_TEXT");
                    }
                }
                else
                {
                    f_UW_TEXTField = value;
                    OnPropertyChanged("F_UW_TEXT");
                }
            }
        }

        public object F_UW_WERT
        {
            get
            {
                return f_UW_WERTField;
            }

            set
            {
                if (f_UW_WERTField != null)
                {
                    if (!f_UW_WERTField.Equals(value))
                    {
                        f_UW_WERTField = value;
                        OnPropertyChanged("F_UW_WERT");
                    }
                }
                else
                {
                    f_UW_WERTField = value;
                    OnPropertyChanged("F_UW_WERT");
                }
            }
        }

        public string F_UW_EINH
        {
            get
            {
                return f_UW_EINHField;
            }

            set
            {
                if (f_UW_EINHField != null)
                {
                    if (!f_UW_EINHField.Equals(value))
                    {
                        f_UW_EINHField = value;
                        OnPropertyChanged("F_UW_EINH");
                    }
                }
                else
                {
                    f_UW_EINHField = value;
                    OnPropertyChanged("F_UW_EINH");
                }
            }
        }

        [DefaultValue(true)]
        public bool ctordone
        {
            get
            {
                return ctordoneField;
            }

            set
            {
                if (!ctordoneField.Equals(value))
                {
                    ctordoneField = value;
                    OnPropertyChanged("ctordone");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public F_UW()
        {
            ctordoneField = true;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}