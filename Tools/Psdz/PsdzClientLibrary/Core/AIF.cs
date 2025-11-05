using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public class AIF : IAif, INotifyPropertyChanged
    {
        private long? aIF_LAENGEField;
        private int? aIF_ADRESSE_HIGHField;
        private int? aIF_ADRESSE_LOWField;
        private string aIF_FG_NRField;
        private string aIF_FG_NR_LANGField;
        private string aIF_DATUMField;
        private string aIF_ZB_NRField;
        private string aIF_SW_NRField;
        private string aIF_BEHOERDEN_NRField;
        private string aIF_HAENDLER_NRField;
        private string aIF_SERIEN_NRField;
        private long? aIF_KMField;
        private string aIF_PROG_NRField;
        private int? aIF_ANZ_FREIField;
        private int? aIF_ANZAHL_PROGField;
        private int? aIF_ANZ_DATENField;
        private int? aIF_GROESSEField;
        public long? AIF_LAENGE
        {
            get
            {
                return aIF_LAENGEField;
            }

            set
            {
                if (aIF_LAENGEField.HasValue)
                {
                    if (!aIF_LAENGEField.Equals(value))
                    {
                        aIF_LAENGEField = value;
                        OnPropertyChanged("AIF_LAENGE");
                    }
                }
                else
                {
                    aIF_LAENGEField = value;
                    OnPropertyChanged("AIF_LAENGE");
                }
            }
        }

        public int? AIF_ADRESSE_HIGH
        {
            get
            {
                return aIF_ADRESSE_HIGHField;
            }

            set
            {
                if (aIF_ADRESSE_HIGHField.HasValue)
                {
                    if (!aIF_ADRESSE_HIGHField.Equals(value))
                    {
                        aIF_ADRESSE_HIGHField = value;
                        OnPropertyChanged("AIF_ADRESSE_HIGH");
                    }
                }
                else
                {
                    aIF_ADRESSE_HIGHField = value;
                    OnPropertyChanged("AIF_ADRESSE_HIGH");
                }
            }
        }

        public int? AIF_ADRESSE_LOW
        {
            get
            {
                return aIF_ADRESSE_LOWField;
            }

            set
            {
                if (aIF_ADRESSE_LOWField.HasValue)
                {
                    if (!aIF_ADRESSE_LOWField.Equals(value))
                    {
                        aIF_ADRESSE_LOWField = value;
                        OnPropertyChanged("AIF_ADRESSE_LOW");
                    }
                }
                else
                {
                    aIF_ADRESSE_LOWField = value;
                    OnPropertyChanged("AIF_ADRESSE_LOW");
                }
            }
        }

        public string AIF_FG_NR
        {
            get
            {
                return aIF_FG_NRField;
            }

            set
            {
                if (aIF_FG_NRField != null)
                {
                    if (!aIF_FG_NRField.Equals(value))
                    {
                        aIF_FG_NRField = value;
                        OnPropertyChanged("AIF_FG_NR");
                    }
                }
                else
                {
                    aIF_FG_NRField = value;
                    OnPropertyChanged("AIF_FG_NR");
                }
            }
        }

        public string AIF_FG_NR_LANG
        {
            get
            {
                return aIF_FG_NR_LANGField;
            }

            set
            {
                if (aIF_FG_NR_LANGField != null)
                {
                    if (!aIF_FG_NR_LANGField.Equals(value))
                    {
                        aIF_FG_NR_LANGField = value;
                        OnPropertyChanged("AIF_FG_NR_LANG");
                    }
                }
                else
                {
                    aIF_FG_NR_LANGField = value;
                    OnPropertyChanged("AIF_FG_NR_LANG");
                }
            }
        }

        public string AIF_DATUM
        {
            get
            {
                return aIF_DATUMField;
            }

            set
            {
                if (aIF_DATUMField != null)
                {
                    if (!aIF_DATUMField.Equals(value))
                    {
                        aIF_DATUMField = value;
                        OnPropertyChanged("AIF_DATUM");
                    }
                }
                else
                {
                    aIF_DATUMField = value;
                    OnPropertyChanged("AIF_DATUM");
                }
            }
        }

        public string AIF_ZB_NR
        {
            get
            {
                return aIF_ZB_NRField;
            }

            set
            {
                if (aIF_ZB_NRField != null)
                {
                    if (!aIF_ZB_NRField.Equals(value))
                    {
                        aIF_ZB_NRField = value;
                        OnPropertyChanged("AIF_ZB_NR");
                    }
                }
                else
                {
                    aIF_ZB_NRField = value;
                    OnPropertyChanged("AIF_ZB_NR");
                }
            }
        }

        public string AIF_SW_NR
        {
            get
            {
                return aIF_SW_NRField;
            }

            set
            {
                if (aIF_SW_NRField != null)
                {
                    if (!aIF_SW_NRField.Equals(value))
                    {
                        aIF_SW_NRField = value;
                        OnPropertyChanged("AIF_SW_NR");
                    }
                }
                else
                {
                    aIF_SW_NRField = value;
                    OnPropertyChanged("AIF_SW_NR");
                }
            }
        }

        public string AIF_BEHOERDEN_NR
        {
            get
            {
                return aIF_BEHOERDEN_NRField;
            }

            set
            {
                if (aIF_BEHOERDEN_NRField != null)
                {
                    if (!aIF_BEHOERDEN_NRField.Equals(value))
                    {
                        aIF_BEHOERDEN_NRField = value;
                        OnPropertyChanged("AIF_BEHOERDEN_NR");
                    }
                }
                else
                {
                    aIF_BEHOERDEN_NRField = value;
                    OnPropertyChanged("AIF_BEHOERDEN_NR");
                }
            }
        }

        public string AIF_HAENDLER_NR
        {
            get
            {
                return aIF_HAENDLER_NRField;
            }

            set
            {
                if (aIF_HAENDLER_NRField != null)
                {
                    if (!aIF_HAENDLER_NRField.Equals(value))
                    {
                        aIF_HAENDLER_NRField = value;
                        OnPropertyChanged("AIF_HAENDLER_NR");
                    }
                }
                else
                {
                    aIF_HAENDLER_NRField = value;
                    OnPropertyChanged("AIF_HAENDLER_NR");
                }
            }
        }

        public string AIF_SERIEN_NR
        {
            get
            {
                return aIF_SERIEN_NRField;
            }

            set
            {
                if (aIF_SERIEN_NRField != null)
                {
                    if (!aIF_SERIEN_NRField.Equals(value))
                    {
                        aIF_SERIEN_NRField = value;
                        OnPropertyChanged("AIF_SERIEN_NR");
                    }
                }
                else
                {
                    aIF_SERIEN_NRField = value;
                    OnPropertyChanged("AIF_SERIEN_NR");
                }
            }
        }

        public long? AIF_KM
        {
            get
            {
                return aIF_KMField;
            }

            set
            {
                if (aIF_KMField.HasValue)
                {
                    if (!aIF_KMField.Equals(value))
                    {
                        aIF_KMField = value;
                        OnPropertyChanged("AIF_KM");
                    }
                }
                else
                {
                    aIF_KMField = value;
                    OnPropertyChanged("AIF_KM");
                }
            }
        }

        public string AIF_PROG_NR
        {
            get
            {
                return aIF_PROG_NRField;
            }

            set
            {
                if (aIF_PROG_NRField != null)
                {
                    if (!aIF_PROG_NRField.Equals(value))
                    {
                        aIF_PROG_NRField = value;
                        OnPropertyChanged("AIF_PROG_NR");
                    }
                }
                else
                {
                    aIF_PROG_NRField = value;
                    OnPropertyChanged("AIF_PROG_NR");
                }
            }
        }

        public int? AIF_ANZ_FREI
        {
            get
            {
                return aIF_ANZ_FREIField;
            }

            set
            {
                if (aIF_ANZ_FREIField.HasValue)
                {
                    if (!aIF_ANZ_FREIField.Equals(value))
                    {
                        aIF_ANZ_FREIField = value;
                        OnPropertyChanged("AIF_ANZ_FREI");
                    }
                }
                else
                {
                    aIF_ANZ_FREIField = value;
                    OnPropertyChanged("AIF_ANZ_FREI");
                }
            }
        }

        public int? AIF_ANZAHL_PROG
        {
            get
            {
                return aIF_ANZAHL_PROGField;
            }

            set
            {
                if (aIF_ANZAHL_PROGField.HasValue)
                {
                    if (!aIF_ANZAHL_PROGField.Equals(value))
                    {
                        aIF_ANZAHL_PROGField = value;
                        OnPropertyChanged("AIF_ANZAHL_PROG");
                    }
                }
                else
                {
                    aIF_ANZAHL_PROGField = value;
                    OnPropertyChanged("AIF_ANZAHL_PROG");
                }
            }
        }

        public int? AIF_ANZ_DATEN
        {
            get
            {
                return aIF_ANZ_DATENField;
            }

            set
            {
                if (aIF_ANZ_DATENField.HasValue)
                {
                    if (!aIF_ANZ_DATENField.Equals(value))
                    {
                        aIF_ANZ_DATENField = value;
                        OnPropertyChanged("AIF_ANZ_DATEN");
                    }
                }
                else
                {
                    aIF_ANZ_DATENField = value;
                    OnPropertyChanged("AIF_ANZ_DATEN");
                }
            }
        }

        public int? AIF_GROESSE
        {
            get
            {
                return aIF_GROESSEField;
            }

            set
            {
                if (aIF_GROESSEField.HasValue)
                {
                    if (!aIF_GROESSEField.Equals(value))
                    {
                        aIF_GROESSEField = value;
                        OnPropertyChanged("AIF_GROESSE");
                    }
                }
                else
                {
                    aIF_GROESSEField = value;
                    OnPropertyChanged("AIF_GROESSE");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public AIF()
        {
            aIF_ANZAHL_PROGField = 0;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}