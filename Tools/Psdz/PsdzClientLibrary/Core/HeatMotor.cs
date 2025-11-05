using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class HeatMotor : INotifyPropertyChanged
    {
        private string driveId;
        private string heatMOTBezeichnungField;
        private string heatMOTBaureiheField;
        private string heatMOTPlatzhalter1Field;
        private string heatMOTPlatzhalter2Field;
        private string heatMOTFortlaufendeNumField;
        private string heatMOTLeistungsklasseField;
        private string heatMOTLebenszyklusField;
        private string heatMOTKraftstoffartField;
        public string DriveId
        {
            get
            {
                return driveId;
            }

            set
            {
                if (driveId != null)
                {
                    if (!driveId.Equals(value))
                    {
                        driveId = value;
                        OnPropertyChanged("DriveId");
                    }
                }
                else
                {
                    driveId = value;
                    OnPropertyChanged("DriveId");
                }
            }
        }

        public string HeatMOTBezeichnung
        {
            get
            {
                return heatMOTBezeichnungField;
            }

            set
            {
                if (heatMOTBezeichnungField != null)
                {
                    if (!heatMOTBezeichnungField.Equals(value))
                    {
                        heatMOTBezeichnungField = value;
                        OnPropertyChanged("HeatMOTBezeichnung");
                    }
                }
                else
                {
                    heatMOTBezeichnungField = value;
                    OnPropertyChanged("HeatMOTBezeichnung");
                }
            }
        }

        public string HeatMOTBaureihe
        {
            get
            {
                return heatMOTBaureiheField;
            }

            set
            {
                if (heatMOTBaureiheField != null)
                {
                    if (!heatMOTBaureiheField.Equals(value))
                    {
                        heatMOTBaureiheField = value;
                        OnPropertyChanged("HeatMOTBaureihe");
                    }
                }
                else
                {
                    heatMOTBaureiheField = value;
                    OnPropertyChanged("HeatMOTBaureihe");
                }
            }
        }

        public string HeatMOTPlatzhalter1
        {
            get
            {
                return heatMOTPlatzhalter1Field;
            }

            set
            {
                if (heatMOTPlatzhalter1Field != null)
                {
                    if (!heatMOTPlatzhalter1Field.Equals(value))
                    {
                        heatMOTPlatzhalter1Field = value;
                        OnPropertyChanged("HeatMOTPlatzhalter1");
                    }
                }
                else
                {
                    heatMOTPlatzhalter1Field = value;
                    OnPropertyChanged("HeatMOTPlatzhalter1");
                }
            }
        }

        public string HeatMOTPlatzhalter2
        {
            get
            {
                return heatMOTPlatzhalter2Field;
            }

            set
            {
                if (heatMOTPlatzhalter2Field != null)
                {
                    if (!heatMOTPlatzhalter2Field.Equals(value))
                    {
                        heatMOTPlatzhalter2Field = value;
                        OnPropertyChanged("HeatMOTPlatzhalter2");
                    }
                }
                else
                {
                    heatMOTPlatzhalter2Field = value;
                    OnPropertyChanged("HeatMOTPlatzhalter2");
                }
            }
        }

        public string HeatMOTFortlaufendeNum
        {
            get
            {
                return heatMOTFortlaufendeNumField;
            }

            set
            {
                if (heatMOTFortlaufendeNumField != null)
                {
                    if (!heatMOTFortlaufendeNumField.Equals(value))
                    {
                        heatMOTFortlaufendeNumField = value;
                        OnPropertyChanged("HeatMOTFortlaufendeNum");
                    }
                }
                else
                {
                    heatMOTFortlaufendeNumField = value;
                    OnPropertyChanged("HeatMOTFortlaufendeNum");
                }
            }
        }

        public string HeatMOTLeistungsklasse
        {
            get
            {
                return heatMOTLeistungsklasseField;
            }

            set
            {
                if (heatMOTLeistungsklasseField != null)
                {
                    if (!heatMOTLeistungsklasseField.Equals(value))
                    {
                        heatMOTLeistungsklasseField = value;
                        OnPropertyChanged("HeatMOTLeistungsklasse");
                    }
                }
                else
                {
                    heatMOTLeistungsklasseField = value;
                    OnPropertyChanged("HeatMOTLeistungsklasse");
                }
            }
        }

        public string HeatMOTLebenszyklus
        {
            get
            {
                return heatMOTLebenszyklusField;
            }

            set
            {
                if (heatMOTLebenszyklusField != null)
                {
                    if (!heatMOTLebenszyklusField.Equals(value))
                    {
                        heatMOTLebenszyklusField = value;
                        OnPropertyChanged("HeatMOTLebenszyklus");
                    }
                }
                else
                {
                    heatMOTLebenszyklusField = value;
                    OnPropertyChanged("HeatMOTLebenszyklus");
                }
            }
        }

        public string HeatMOTKraftstoffart
        {
            get
            {
                return heatMOTKraftstoffartField;
            }

            set
            {
                if (heatMOTKraftstoffartField != null)
                {
                    if (!heatMOTKraftstoffartField.Equals(value))
                    {
                        heatMOTKraftstoffartField = value;
                        OnPropertyChanged("HeatMOTKraftstoffart");
                    }
                }
                else
                {
                    heatMOTKraftstoffartField = value;
                    OnPropertyChanged("HeatMOTKraftstoffart");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}