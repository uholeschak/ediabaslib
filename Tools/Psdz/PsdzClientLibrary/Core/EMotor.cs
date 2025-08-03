using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class EMotor : INotifyPropertyChanged
    {
        private string eMOTBaureiheField;

        private string eMOTArbeitsverfahrenField;

        private string eMOTDrehmomentField;

        private string eMOTLeistungsklasseField;

        private string eMOTUeberarbeitungField;

        private string eMOTBezeichnungField;

        private string eMOTKraftstoffartField;

        private string eMOTEinbaulageField;

        public string EMOTBaureihe
        {
            get
            {
                return eMOTBaureiheField;
            }
            set
            {
                if (eMOTBaureiheField != value)
                {
                    eMOTBaureiheField = value;
                    OnPropertyChanged("EMOTBaureihe");
                }
            }
        }

        public string EMOTArbeitsverfahren
        {
            get
            {
                return eMOTArbeitsverfahrenField;
            }
            set
            {
                if (eMOTArbeitsverfahrenField != value)
                {
                    eMOTArbeitsverfahrenField = value;
                    OnPropertyChanged("EMOTArbeitsverfahren");
                }
            }
        }

        public string EMOTDrehmoment
        {
            get
            {
                return eMOTDrehmomentField;
            }
            set
            {
                if (eMOTDrehmomentField != value)
                {
                    eMOTDrehmomentField = value;
                    OnPropertyChanged("EMOTDrehmoment");
                }
            }
        }

        public string EMOTLeistungsklasse
        {
            get
            {
                return eMOTLeistungsklasseField;
            }
            set
            {
                if (eMOTLeistungsklasseField != value)
                {
                    eMOTLeistungsklasseField = value;
                    OnPropertyChanged("EMOTLeistungsklasse");
                }
            }
        }

        public string EMOTUeberarbeitung
        {
            get
            {
                return eMOTUeberarbeitungField;
            }
            set
            {
                if (eMOTUeberarbeitungField != value)
                {
                    eMOTUeberarbeitungField = value;
                    OnPropertyChanged("EMOTUeberarbeitung");
                }
            }
        }

        public string EMOTBezeichnung
        {
            get
            {
                return eMOTBezeichnungField;
            }
            set
            {
                if (eMOTBezeichnungField != value)
                {
                    eMOTBezeichnungField = value;
                    OnPropertyChanged("EMOTBezeichnung");
                }
            }
        }

        public string EMOTKraftstoffart
        {
            get
            {
                return eMOTKraftstoffartField;
            }
            set
            {
                if (eMOTKraftstoffartField != value)
                {
                    eMOTKraftstoffartField = value;
                    OnPropertyChanged("EMOTKraftstoffart");
                }
            }
        }

        public string EMOTEinbaulage
        {
            get
            {
                return eMOTEinbaulageField;
            }
            set
            {
                if (eMOTEinbaulageField != value)
                {
                    eMOTEinbaulageField = value;
                    OnPropertyChanged("EMOTEinbaulage");
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
